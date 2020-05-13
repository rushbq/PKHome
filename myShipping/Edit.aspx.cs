using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myShipping_Edit : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //** 檢查必要參數 **
                if (string.IsNullOrEmpty(Req_CompID))
                {
                    Response.Redirect("{0}Error/參數錯誤".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #region --權限--
                //[權限判斷] Start
                /* 
                 * 使用公司別代號，判斷對應的MENU ID
                 */
                bool isPass = false;
                string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

                switch (getCorpUid)
                {
                    case "2":
                        //深圳寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3701");
                        break;

                    default:
                        //SH
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3702");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //取得公司別
                string _corpName = fn_Param.GetCorpName(getCorpUid);
                lt_CorpName.Text = _corpName;
                Page.Title += "-" + _corpName;

                //[權限判斷] End
                #endregion

                //*** 設定母版的Menu ***
                Literal menu = (Literal)Page.Master.FindControl("lt_headerMenu");
                menu.Text = fn_Menu.GetTopMenu_ShipFreight(Req_Lang, Req_RootID, Req_CompID, Req_Tab);


                //[參數判斷] - ERP編號
                if (string.IsNullOrWhiteSpace(Req_ErpNo))
                {
                    CustomExtension.AlertMsg("查無資料,請重新查詢", Page_SearchUrl);
                    return;
                }

                //[參數判斷] - 資料編號
                if (Req_DataID.Equals("new"))
                {
                    //自動新增
                    Add_Data();
                }
                else
                {
                    //載入資料
                    LookupData();
                }

            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示:基本資料 --

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipFreightList(Req_CompID, search, null, out ErrMsg).Take(1).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            //this.ph_ErrMessage.Visible = true;
            //this.ph_Data.Visible = false;
            //this.lt_ShowMsg.Text = "無法取得資料";
            CustomExtension.AlertMsg("無法取得資料", Page_SearchUrl);
            return;
        }


        //填入資料
        this.hf_DataID.Value = query.Data_ID.ToString();
        this.hf_CustID.Value = query.CustID;
        this.hf_ErpFid.Value = query.Erp_SO_FID;
        this.hf_ErpSid.Value = query.Erp_SO_SID;
        this.hf_ShipFrom.Value = query.StockType;
        this.lt_ErpNo.Text = "{0}-{1}".FormatThis(query.Erp_SO_FID, query.Erp_SO_SID);
        this.lt_Cust.Text = "{0}&nbsp;{1}".FormatThis(query.CustID, query.CustName);
        this.lt_StockType.Text = "{0}".FormatThis(query.StockName);
        this.tb_ShipDate.Text = query.ShipDate;

        string _shipCompName = query.StockType.Equals("SH") ? "德邦快递" : "圆通快递"; //default:上海=1:德邦快递,深圳=16:圆通快递 (Table:Logistics)
        this.tb_ShipComp.Text = string.IsNullOrWhiteSpace(query.ShipCompName) ? _shipCompName : query.ShipCompName; 

        int _shipComp = query.StockType.Equals("SH") ? 1 : 16; //default:上海=1:德邦快递,深圳=16:圆通快递 (Table:Logistics)
        this.val_ShipComp.Text = string.IsNullOrWhiteSpace(query.ShipComp.ToString()) ? _shipComp.ToString() : query.ShipComp.ToString();
        this.ddl_ShipWay.SelectedValue = query.ShipWay;
        this.tb_ShipWho.Text = query.ShipWho;
        this.tb_Remark.Text = query.Remark;

        //判斷此筆資料是否被已關聯,若已關聯則不可編輯
        if (query.IsReled >= 1)
        {
            this.lt_CombineID.Text = query.Remark;
            this.ph_LockModal.Visible = true;
            this.ph_LockScript.Visible = true;
            return;
        }

        ////維護資訊
        //this.lt_Creater.Text = query.Create_Name;
        //this.lt_CreateTime.Text = query.Create_Time;
        //this.lt_Updater.Text = query.Update_Name;
        //this.lt_UpdateTime.Text = query.Update_Time;


        //-- 載入其他資料 --
        LookupData_Detail();
        LookupData_Rel();

    }

    #endregion

    #region -- 資料編輯:基本資料 --

    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data()
    {
        #region --取得ERP資料--

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        search.Add("ErpNo", Req_ErpNo);

        var erpData = _data.GetShipFreightList(Req_CompID, search, null, out ErrMsg).Take(1).FirstOrDefault();

        #endregion

        //判斷是否重複新增
        if (!string.IsNullOrEmpty(erpData.Data_ID.ToString()))
        {
            Response.Redirect("{0}/Edit/{1}?erpNo={2}".FormatThis(FuncPath(), erpData.Data_ID, Req_ErpNo));
            return;
        }

        //----- 設定:資料欄位 -----
        //產生Guid
        string guid = CustomExtension.GetGuid();

        var data = new ShipFreightItem
        {
            Data_ID = new Guid(guid),
            CompID = Req_CompID,
            CustID = erpData.CustID,
            StockType = erpData.StockType,
            Erp_SO_FID = erpData.Erp_SO_FID,
            Erp_SO_SID = erpData.Erp_SO_SID,
            ShipDate = erpData.Erp_SO_Date.ToDateString_ERP("/"),   //ERP單據日
            //ShipComp = erpData.StockType.Equals("SH") ? 2 : 16, //上海=2優速快遞,深圳=16圆通快递 (Table:Logistics)
            ShipWay = "C",  //C=其他
            ShipWho = erpData.ShipWho,  //ERP單據聯絡人(TG066 / TF015)
            ShipCnt = 1,
            Remark = "",
            Create_Who = fn_Param.CurrentUser
        };

        //----- 方法:新增資料 -----
        if (!_data.CreateShipFreight(data, out ErrMsg))
        {
            //Response.Write(ErrMsg);
            //this.ph_ErrMessage.Visible = true;
            //this.lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("新增失敗", Page_SearchUrl);
            return;
        }
        else
        {
            //更新本頁Url
            string thisUrl = "{0}/Edit/{1}?erpNo={2}".FormatThis(FuncPath(), guid, Req_ErpNo);

            //導向本頁
            Response.Redirect(thisUrl);
        }
    }


    /// <summary>
    /// 資料修改
    /// </summary>
    private void Edit_Data(string type)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 設定:資料欄位 -----
        var data = new ShipFreightItem
        {
            Data_ID = new Guid(Req_DataID),
            CompID = Req_CompID,
            ShipDate = this.tb_ShipDate.Text.ToDateString("yyyy/MM/dd"),
            ShipComp = Convert.ToInt16(this.val_ShipComp.Text),
            ShipWay = this.ddl_ShipWay.SelectedValue,
            ShipWho = this.tb_ShipWho.Text,
            ShipCnt = Convert.ToInt16(this.tb_ShipCnt.Text),
            Remark = this.tb_Remark.Text.Left(200),
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateShipFreight(data, out ErrMsg))
        {
            CustomExtension.AlertMsg("更新失敗", thisPage);
            return;
        }
        else
        {
            if (type.Equals("1"))
            {
                //導向本頁
                Response.Redirect(thisPage);
            }
            else
            {
                //導向列表頁
                Response.Redirect(Page_SearchUrl);
            }
        }

    }


    //SAVE-基本資料
    protected void btn_SaveStay_Click(object sender, EventArgs e)
    {
        doSave("1");
    }
    protected void btn_Save_Click(object sender, EventArgs e)
    {
        doSave("2");
    }

    private void doSave(string type)
    {
        string errTxt = "";

        if (string.IsNullOrWhiteSpace(this.tb_ShipWho.Text))
        {
            errTxt += "收貨人空白\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        /* 執行新增/更新 */
        if (string.IsNullOrEmpty(this.hf_DataID.Value))
        {
            Add_Data();
        }
        else
        {
            Edit_Data(type);
        }
    }

    #endregion


    #region -- 資料顯示:物流單號 --

    private void LookupData_Detail()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipFreightDetail(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lvDetailList.DataSource = query;
        this.lvDetailList.DataBind();
    }

    protected void lvDetailList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string dataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

            switch (e.CommandName.ToUpper())
            {
                case "DOCLOSE":
                    //----- 宣告:資料參數 -----
                    Menu3000Repository _data = new Menu3000Repository();

                    //----- 方法:刪除資料 -----
                    if (false == _data.DeleteShipFreightDetail(Req_DataID, dataID, out ErrMsg))
                    {
                        _data = null;

                        CustomExtension.AlertMsg("刪除失敗", "");

                        return;
                    }
                    else
                    {
                        //導向本頁
                        Response.Redirect(thisPage + "#shipNoList");
                    }

                    break;
            }
        }
    }


    #endregion


    #region -- 資料編輯:物流單號 --

    //SAVE-物流單號
    protected void btn_SaveDetail_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        if (string.IsNullOrWhiteSpace(this.tb_ShipNo.Text))
        {
            errTxt += "物流單號空白\\n";
        }
        if (string.IsNullOrWhiteSpace(this.tb_ShipCnt.Text))
        {
            errTxt += "件數空白\\n";
        }
        if (!this.tb_Pay1.Text.IsNumeric() || !this.tb_Pay2.Text.IsNumeric() || !this.tb_Pay3.Text.IsNumeric())
        {
            errTxt += "運費欄位請填寫半形數字\\n";
        }
        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 設定:資料欄位 -----
        var data = new ShipFreightDetail
        {
            Parent_ID = new Guid(Req_DataID),
            ShipNo = this.tb_ShipNo.Text,
            ShipCnt = Convert.ToInt32(this.tb_ShipCnt.Text),
            Pay1 = Convert.ToDouble(this.tb_Pay1.Text),
            Pay2 = Convert.ToDouble(this.tb_Pay2.Text),
            Pay3 = Convert.ToDouble(this.tb_Pay3.Text)
        };

        //----- 方法:建立資料 -----
        if (!_data.CreateShipFreightDetail(data, out ErrMsg))
        {
            //this.ph_ErrMessage.Visible = true;
            //this.lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("建立失敗", thisPage);
            return;
        }
        else
        {
            //導向本頁
            Response.Redirect(thisPage + "#shipNoList");
        }
    }

    #endregion


    #region -- 資料顯示:合併單號 --

    private void LookupData_Rel()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipFreightRel(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lvRelList.DataSource = query;
        this.lvRelList.DataBind();
    }

    protected void lvRelList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string dataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;
            string relID = ((HiddenField)e.Item.FindControl("hf_RelID")).Value;

            switch (e.CommandName.ToUpper())
            {
                case "DOCLOSE":
                    //----- 宣告:資料參數 -----
                    Menu3000Repository _data = new Menu3000Repository();

                    //----- 方法:刪除資料 -----
                    if (false == _data.DeleteShipFreightRel(relID, dataID, out ErrMsg))
                    {
                        _data = null;

                        CustomExtension.AlertMsg("刪除失敗", "");

                        return;
                    }
                    else
                    {
                        //導向本頁
                        Response.Redirect(thisPage + "#relList");
                    }

                    break;
            }
        }
    }

    #endregion


    #region -- 資料編輯:合併單號 --

    //SAVE-關聯ERP單號
    protected void btn_SaveRel_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        if (string.IsNullOrWhiteSpace(this.val_ErpNo.Text))
        {
            errTxt += "單據未正確選擇\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        //關聯的單號
        string[] erpNo = this.val_ErpNo.Text.Split('-');
        //產生Guid
        string guid = CustomExtension.GetGuid();

        //----- 設定:資料欄位 -----
        var data = new ShipFreightRel
        {
            Parent_ID = new Guid(Req_DataID),
            Rel_ID = new Guid(guid),
            Erp_SO_FID = erpNo[0],
            Erp_SO_SID = erpNo[1]
        };

        //----- 設定:基本資料欄位(關聯時自動INSERT) -----

        var baseData = new ShipFreightItem
        {
            Data_ID = new Guid(guid),
            CustID = hf_CustID.Value,
            CompID = Req_CompID,
            Erp_SO_FID = erpNo[0],
            Erp_SO_SID = erpNo[1],
            ShipDate = this.tb_ShipDate.Text.ToDateString("yyyy/MM/dd"),
            ShipComp = Convert.ToInt16(this.val_ShipComp.Text),
            ShipWay = this.ddl_ShipWay.SelectedValue,
            ShipWho = this.tb_ShipWho.Text,
            StockType = hf_ShipFrom.Value,
            Remark = "{0}-{1}".FormatThis(this.hf_ErpFid.Value, this.hf_ErpSid.Value), //填入目前ERP單號
            Create_Who = fn_Param.CurrentUser
        };

        //----- 方法:建立資料 -----
        if (!_data.CreateShipFreightRel(data, baseData, out ErrMsg))
        {
            //this.ph_ErrMessage.Visible = true;
            //this.lt_ShowMsg.Text = ErrMsg;
            //Response.Write(ErrMsg);
            CustomExtension.AlertMsg("合併失敗", thisPage);
            return;
        }
        else
        {
            //導向本頁
            Response.Redirect(thisPage + "#relList");
        }
    }

    #endregion


    #region -- 網址參數 --

    /// <summary>
    /// 取得網址參數 - 語系
    /// </summary>
    public string Req_Lang
    {
        get
        {
            string myLang = Page.RouteData.Values["lang"] == null ? "auto" : Page.RouteData.Values["lang"].ToString();

            //若為auto, 就去抓cookie
            return myLang.Equals("auto") ? fn_Language.Get_Lang(Request.Cookies["PKHome_Lang"].Value) : myLang;
        }
        set
        {
            this._Req_Lang = value;
        }
    }
    private string _Req_Lang;


    /// <summary>
    /// 取得網址參數 - RootID
    /// </summary>
    private string _Req_RootID;
    public string Req_RootID
    {
        get
        {
            String DataID = Page.RouteData.Values["rootID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
        }
        set
        {
            this._Req_RootID = value;
        }
    }

    /// <summary>
    /// 取得網址參數 - Company ID(TW/SH/SZ)
    /// </summary>
    private string _Req_CompID;
    public string Req_CompID
    {
        get
        {
            String DataID = Page.RouteData.Values["CompID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "SZ" : DataID;
        }
        set
        {
            this._Req_CompID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/ShipFreight/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
    }

    #endregion


    #region -- 傳遞參數 --

    /// <summary>
    /// 取得傳遞參數 - 資料編號
    /// </summary>
    private string _Req_DataID;
    public string Req_DataID
    {
        get
        {
            String DataID = Page.RouteData.Values["id"].ToString();

            return DataID;
        }
        set
        {
            this._Req_DataID = value;
        }
    }


    public string Req_ErpNo
    {
        get
        {
            string data = Request.QueryString["erpNo"] == null ? "" : Request.QueryString["erpNo"].ToString();
            return data;
        }
        set
        {
            this._Req_ErpNo = value;
        }
    }
    private string _Req_ErpNo;


    public string Req_Tab
    {
        get
        {
            string data = Request.QueryString["tab"] == null ? "1" : Request.QueryString["tab"].ToString();
            return data;
        }
        set
        {
            this._Req_Tab = value;
        }
    }
    private string _Req_Tab;


    /// <summary>
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/Edit/{1}?erpNo={2}".FormatThis(FuncPath(), Req_DataID, Req_ErpNo);
        }
        set
        {
            this._thisPage = value;
        }
    }


    /// <summary>
    /// 設定參數 - 列表頁Url
    /// </summary>
    private string _Page_SearchUrl;
    public string Page_SearchUrl
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("HomeList_Shipping");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }

    #endregion

}
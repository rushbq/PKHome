using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;
using ToyAdditionalData.Controllers;
using ToyAdditionalData.Models;

public partial class myToyAdditional_Edit : SecurityCheck
{
    public string ErrMsg;
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //權限判斷
                if (false == fn_CheckAuth.Check(fn_Param.CurrentUser, "3711"))
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[參數判斷] - 判斷是否有資料編號
                if (!string.IsNullOrEmpty(Req_DataID))
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


    #region -- 資料取得 --

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        ToyAdditionalRepository _data = new ToyAdditionalRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();


        //----- 原始資料:條件篩選 -----
        search.Add((int)mySearch.DataID, Req_DataID);


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetDataList(search).Take(1).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            this.ph_ErrMessage.Visible = true;
            this.ph_Data.Visible = false;
            this.lt_ShowMsg.Text = "無法取得資料";
            return;
        }

        //Get Data
        string finish = string.IsNullOrEmpty(query.Ship_Time) ? "N" : "Y";

        //填入資料
        this.hf_DataID.Value = query.Data_ID.ToString();
        this.lt_DataID.Text = query.SeqNo.ToString();
        this.ddl_CustType.SelectedValue = query.CustType.ToString();
        this.tb_CustName.Text = query.CustName;
        this.tb_CustTel.Text = query.CustTel;
        this.tb_CustAddr.Text = query.CustAddr;
        this.AC_ModelNo.Text = query.ModelNo;
        this.Rel_ModelNo_Val.Text = query.ModelNo;
        this.lb_ModelName.Text = query.ModelName;
        this.tb_Qty.Text = query.Qty.ToString();
        this.tb_Remark1.Text = query.Remark1;
        this.tb_Remark2.Text = query.Remark2;
        this.tb_Remark3.Text = query.Remark3;
        this.tb_ShipDate.Text = query.ShipDate.ToDateString("yyyy/MM/dd");
        this.tb_ShipNo.Text = query.ShipNo;
        this.tb_Freight.Text = query.Freight.ToString();

        //維護資訊
        this.lt_Creater.Text = query.Create_Name;
        this.lt_CreateTime.Text = query.Create_Time;
        this.lt_Updater.Text = query.Update_Name;
        this.lt_UpdateTime.Text = query.Update_Time;
        this.lt_Shipper.Text = query.Ship_Name;
        this.lt_ShipTime.Text = query.Ship_Time;

        //基本資料填過才能填出貨資料
        this.ph_ShipInfo.Visible = true;
        this.ph_ShipNav.Visible = true;


        //判斷是否已結案
        if (finish.Equals("Y"))
        {
            this.ph_SaveBase.Visible = false;
            this.ph_SaveShip.Visible = false;
        }
    }

    #endregion


    #region -- 資料編輯 --

    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data()
    {
        //----- 宣告:資料參數 -----
        ToyAdditionalRepository _data = new ToyAdditionalRepository();

        //----- 設定:資料欄位 -----
        //產生Guid
        string guid = CustomExtension.GetGuid();

        var data = new Items
        {
            Data_ID = new Guid(guid),
            CompID = Req_CompID,
            CustType = Convert.ToInt16(this.ddl_CustType.SelectedValue),
            CustName = this.tb_CustName.Text,
            CustTel = this.tb_CustTel.Text,
            CustAddr = this.tb_CustAddr.Text,
            ModelNo = this.Rel_ModelNo_Val.Text,
            Qty = Convert.ToInt16(this.tb_Qty.Text),
            Remark1 = this.tb_Remark1.Text,
            Remark2 = this.tb_Remark2.Text,
            Remark3 = this.tb_Remark3.Text,

            Create_Who = fn_Param.CurrentUser
        };


        //----- 方法:新增資料 -----
        if (!_data.Create(data, out ErrMsg))
        {
            this.ph_ErrMessage.Visible = true;
            this.lt_ShowMsg.Text = ErrMsg;
            return;
        }
        else
        {
            //更新本頁Url
            string thisUrl = "{0}/Edit/{1}".FormatThis(FuncPath(), guid);

            //導向本頁
            Response.Redirect(thisUrl);
        }
    }


    /// <summary>
    /// 資料修改
    /// </summary>
    private void Edit_Data()
    {
        //----- 宣告:資料參數 -----
        ToyAdditionalRepository _data = new ToyAdditionalRepository();

        //----- 設定:資料欄位 -----
        var data = new Items
        {
            Data_ID = new Guid(Req_DataID),
            CustType = Convert.ToInt16(this.ddl_CustType.SelectedValue),
            CustName = this.tb_CustName.Text,
            CustTel = this.tb_CustTel.Text,
            CustAddr = this.tb_CustAddr.Text,
            ModelNo = this.Rel_ModelNo_Val.Text,
            Qty = Convert.ToInt16(this.tb_Qty.Text),
            Remark1 = this.tb_Remark1.Text,
            Remark2 = this.tb_Remark2.Text,
            Remark3 = this.tb_Remark3.Text,

            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.Update(data, out ErrMsg))
        {
            this.ph_ErrMessage.Visible = true;
            this.lt_ShowMsg.Text = ErrMsg;
            return;
        }
        else
        {
            //導向本頁
            Response.Redirect(thisPage);
        }

    }


    /// <summary>
    /// 出貨
    /// </summary>
    private void Ship_Data()
    {
        //----- 宣告:資料參數 -----
        ToyAdditionalRepository _data = new ToyAdditionalRepository();
        string freight = this.tb_Freight.Text;

        //----- 設定:資料欄位 -----
        var data = new Items
        {
            Data_ID = new Guid(Req_DataID),
            ShipDate = this.tb_ShipDate.Text.ToDateString("yyyy/MM/dd"),
            ShipNo = this.tb_ShipNo.Text,
            Freight = string.IsNullOrEmpty(freight) ? 0 : Convert.ToDouble(freight),

            Ship_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.Update_Ship(data, out ErrMsg))
        {
            this.ph_ErrMessage.Visible = true;
            this.lt_ShowMsg.Text = ErrMsg;
            return;
        }
        else
        {
            //導向本頁
            Response.Redirect(Page_SearchUrl);
        }

    }


    protected void btn_Save_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        if (string.IsNullOrEmpty(this.Rel_ModelNo_Val.Text))
        {
            errTxt += "品號空白\\n";
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
            Edit_Data();
        }
    }

    protected void btn_Ship_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        if (string.IsNullOrEmpty(this.tb_ShipDate.Text))
        {
            errTxt += "寄出日未填寫\\n";
        }
        if (string.IsNullOrEmpty(this.tb_ShipNo.Text))
        {
            errTxt += "貨運及單號未填寫\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        //do Update
        Ship_Data();
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

            return DataID.ToLower().Equals("unknown") ? "TW" : DataID;
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
        return "{0}{1}/{2}/ToyAdditional/{3}".FormatThis(
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

            return DataID.ToLower().Equals("new") ? "" : DataID;
        }
        set
        {
            this._Req_DataID = value;
        }
    }


    /// <summary>
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/Edit/{1}".FormatThis(FuncPath(), Req_DataID);
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
            string tempUrl = CustomExtension.getCookie("HomeList_ToyAdditional");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }

    #endregion

}
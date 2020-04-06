using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myRebate_Edit : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;
                string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

                switch (getCorpUid)
                {
                    case "3":
                        //上海寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3732");
                        break;

                    case "2":
                        //深圳寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3731");
                        break;

                    default:
                        //TW
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3733");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[權限判斷] End
                #endregion

                //Create Menu
                CreateMenu_Year(this.ddl_Year);

                //[參數判斷] - 資料編號
                if (!string.IsNullOrWhiteSpace(Req_DataID))
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
        var query = _data.GetCustRebateBase(Req_CompID, search, out ErrMsg).Take(1).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料\\n請檢查ERP客戶資料", Page_SearchUrl);
            return;
        }


        //填入資料
        this.hf_DataID.Value = query.Data_ID.ToString();
        this.tb_Cust.Text = query.CustID;
        this.val_Cust.Text = query.CustID;
        this.lt_CustName.Text = query.CustName;
        this.ddl_Year.SelectedValue = query.DataYear;
        this.ddl_Formula.SelectedValue = query.Formula;
        this.tb_RespMoney.Text = query.Cnt_e.ToString();
        this.tb_RespPercent.Text = query.Cnt_f.ToString();
        this.tb_FightMoney.Text = query.Cnt_g.ToString();
        this.tb_FightPercent.Text = query.Cnt_h.ToString();
        this.tb_Remark.Text = query.Remark;


        ////維護資訊
        //this.lt_Creater.Text = query.Create_Name;
        //this.lt_CreateTime.Text = query.Create_Time;
        //this.lt_Updater.Text = query.Update_Name;
        //this.lt_UpdateTime.Text = query.Update_Time;
    }

    #endregion


    #region -- 資料編輯:基本資料 --

    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data(string type)
    {
        try
        {
            //----- 宣告:資料參數 -----
            Menu3000Repository _data = new Menu3000Repository();

            //----- 設定:資料欄位 -----
            //產生Guid
            string guid = CustomExtension.GetGuid();
            string _cnt_e = this.tb_RespMoney.Text;
            string _cnt_f = this.tb_RespPercent.Text;
            string _cnt_g = this.tb_FightMoney.Text;
            string _cnt_h = this.tb_FightPercent.Text;

            var data = new CustRebateItem
            {
                Data_ID = new Guid(guid),
                CompID = Req_CompID,
                DataYear = this.ddl_Year.SelectedValue,
                CustID = this.val_Cust.Text,
                Formula = this.ddl_Formula.SelectedValue,
                Cnt_e = string.IsNullOrWhiteSpace(_cnt_e) ? 0 : Convert.ToDouble(_cnt_e),
                Cnt_f = string.IsNullOrWhiteSpace(_cnt_f) ? 0 : Convert.ToDouble(_cnt_f),
                Cnt_g = string.IsNullOrWhiteSpace(_cnt_g) ? 0 : Convert.ToDouble(_cnt_g),
                Cnt_h = string.IsNullOrWhiteSpace(_cnt_h) ? 0 : Convert.ToDouble(_cnt_h),
                Remark = this.tb_Remark.Text.Left(200),
                Create_Who = fn_Param.CurrentUser
            };

            //----- 方法:新增資料 -----
            if (!_data.CreateCustRebate(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", Page_SearchUrl);
                return;
            }
            else
            {
                //更新本頁Url
                string thisUrl = "{0}/Edit/{1}".FormatThis(FuncPath(), guid);

                if (type.Equals("1"))
                {
                    //導向本頁
                    Response.Redirect(thisUrl);
                }
                else
                {
                    //導向列表頁
                    Response.Redirect(Page_SearchUrl);
                }
            }
        }
        catch (Exception)
        {

            throw;
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
        string _cnt_e = this.tb_RespMoney.Text;
        string _cnt_f = this.tb_RespPercent.Text;
        string _cnt_g = this.tb_FightMoney.Text;
        string _cnt_h = this.tb_FightPercent.Text;

        var data = new CustRebateItem
        {
            Data_ID = new Guid(Req_DataID),
            CompID = Req_CompID,
            DataYear = this.ddl_Year.SelectedValue,
            CustID = this.val_Cust.Text,
            Formula = this.ddl_Formula.SelectedValue,
            Cnt_e = string.IsNullOrWhiteSpace(_cnt_e) ? 0 : Convert.ToDouble(_cnt_e),
            Cnt_f = string.IsNullOrWhiteSpace(_cnt_f) ? 0 : Convert.ToDouble(_cnt_f),
            Cnt_g = string.IsNullOrWhiteSpace(_cnt_g) ? 0 : Convert.ToDouble(_cnt_g),
            Cnt_h = string.IsNullOrWhiteSpace(_cnt_h) ? 0 : Convert.ToDouble(_cnt_h),
            Remark = this.tb_Remark.Text.Left(200),
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateCustRebate(data, out ErrMsg))
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

        if (string.IsNullOrWhiteSpace(this.val_Cust.Text))
        {
            errTxt += "客戶空白\\n";
        }

        if (string.IsNullOrWhiteSpace(this.tb_RespMoney.Text) || string.IsNullOrWhiteSpace(this.tb_RespPercent.Text))
        {
            errTxt += "業績目標空白\\n";
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
            Add_Data(type);
        }
        else
        {
            Edit_Data(type);
        }
    }

    #endregion


    #region -- 附加功能 --
    protected void CreateMenu_Year(DropDownList item)
    {
        int currYear = DateTime.Now.Year;
        int prevYear = currYear - 1;
        int nextYear = currYear + 1;

        item.Items.Clear();
        for (int itemY = prevYear; itemY <= nextYear; itemY++)
        {
            item.Items.Add(new ListItem(itemY.ToString(), itemY.ToString()));
        }

        item.SelectedValue = currYear.ToString();
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
        return "{0}{1}/{2}/CustRebate/{3}".FormatThis(
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
            string tempUrl = CustomExtension.getCookie("HomeList_Rebate");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }

    #endregion

}
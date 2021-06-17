using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Menu2000Data.Controllers;
using PKLib_Method.Methods;

public partial class myMarketingHelp_View : SecurityCheck
{
    public string ErrMsg;
    public bool masterAuth = false; //主管權限(可在權限設定裡勾選)
    public bool replyAuth = false; //回覆權限(可在權限設定裡勾選)

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            #region --權限--
            //[權限判斷] Start
            /* 
             * 使用公司別代號，判斷對應的MENU ID
             */
            bool isPass = false;
            string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

            switch (getCorpUid)
            {
                //case "3":
                //    //上海寶工
                //    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "xxxx");
                //    break;

                case "2":
                    //深圳寶工
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2452");
                    masterAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2458");
                    replyAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2457");

                    break;

                default:
                    //TW
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2451");
                    masterAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2456");
                    replyAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2455");

                    break;
            }

            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }

            //[權限判斷] End
            #endregion

            if (!IsPostBack)
            {
                /* 多語系設定 */
                Page.Title = GetLocalResourceObject("pageTitle").ToString();

                //取得公司別
                lt_CorpName.Text = fn_Param.GetCorpName(getCorpUid);

                //載入資料
                LookupData();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示:基本資料 --

    /// <summary>
    /// 取得基本資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpList(Req_CompID, search, null, out ErrMsg).Take(1).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }

        #region >> 欄位填寫 <<

        //填入資料
        lt_TraceID.Text = query.TraceID;
        lt_ReqStatus.Text = query.StName;
        lt_CreateDate.Text = query.Create_Time.ToDateString("yyyy/MM/dd");
        lt_ReqSubject.Text = query.Req_Subject;
        lt_ReqContent.Text = query.Req_Content.Replace("\r", "<br/>");
        lt_EmgStatus.Text = query.EmgName;
        lt_WishDate.Text = query.Wish_Date;
        lt_EstDate.Text = query.Est_Date;
        lt_ReqClass.Text = query.TypeName;
        lt_ReqRes.Text = query.ResName;
        lt_ReqQty.Text = query.Req_Qty.ToString();

        string finishHours = query.Finish_Hours.ToString();
        lt_FinishHours.Text = string.IsNullOrWhiteSpace(finishHours) ? "案件處理中" : finishHours;
        string finishDate = query.Finish_Date.ToString();
        lt_FinishDate.Text = string.IsNullOrWhiteSpace(finishDate) ? "案件處理中" : finishDate;

        //-需求者
        lt_ReqWho.Text = query.Req_Name;

        ph_doJob.Visible = !query.StDisp.Equals("D") && !query.StDisp.Equals("E");

        #endregion


        #region >> 其他功能 <<

        //-- 載入其他資料 --
        LookupData_Files();
        LookupData_Inform();
        LookupData_Assigned();
        LookupData_Reply();

        #endregion


        ////維護資訊
        //this.lt_Creater.Text = query.Create_Name;
        //this.lt_CreateTime.Text = query.Create_Time;
        //this.lt_Updater.Text = query.Update_Name;
        //this.lt_UpdateTime.Text = query.Update_Time;

    }

    #endregion

    #region -- 資料顯示:檔案附件 --

    /// <summary>
    /// 顯示檔案附件
    /// </summary>
    private void LookupData_Files()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpFileList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Attachment.DataSource = query;
        this.lv_Attachment.DataBind();

        //Release
        query = null;
    }

    #endregion

    #region -- 資料顯示:轉寄人員 --

    /// <summary>
    /// 顯示轉寄人員
    /// </summary>
    private void LookupData_Inform()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpCCList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Inform.DataSource = query;
        this.lv_Inform.DataBind();

        //Release
        query = null;
    }

    #endregion

    #region -- 資料顯示:處理人員 --

    /// <summary>
    /// 顯示處理人員
    /// </summary>
    private void LookupData_Assigned()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpAssignList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Assigned.DataSource = query;
        this.lv_Assigned.DataBind();

        //Release
        query = null;
    }

    #endregion

    #region -- 資料顯示:進度說明 --

    /// <summary>
    /// 顯示進度說明
    /// </summary>
    private void LookupData_Reply()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpReplyList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Reply.DataSource = query;
        this.lv_Reply.DataBind();

        //Release
        query = null;
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
        return "{0}{1}/{2}/MarketingHelp/{3}".FormatThis(
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
            string tempUrl = CustomExtension.getCookie("HomeList_MKHelp");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }



    /// <summary>
    /// 上傳目錄
    /// </summary>
    private string _UploadFolder;
    public string UploadFolder
    {
        get
        {
            return "{0}MarketingHelp/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
        }
        set
        {
            this._UploadFolder = value;
        }

    }
    #endregion

}
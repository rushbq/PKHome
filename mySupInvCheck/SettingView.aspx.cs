using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

public partial class mySupInvCheck_SettingView : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //[權限判斷] Start
            #region --權限--

            bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4861");

            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }

            #endregion
            //[權限判斷] End


            if (!IsPostBack)
            {
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
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            search.Add("DataID", Req_DataID);

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetOneSupInvSend(search, out ErrMsg).FirstOrDefault();

            //----- 資料整理:繫結 ----- 
            if (query == null)
            {
                CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
                return;
            }


            //--- 填入基本資料 ---
            lt_SeqNo.Text = query.SeqNo.ToString();
            lt_Subject.Text = query.Subject;
            lt_IsOnTask.Text = Get_StatusName(query.IsOnTask);
            lt_TaskTime.Text = query.TaskTime;

            //-- 載入其他資料 --
            LookupData_CheckList();


            //維護資訊
            info_Creater.Text = query.Create_Name;
            info_CreateTime.Text = query.Create_Time;
            info_Updater.Text = query.Update_Name;
            info_UpdateTime.Text = query.Update_Time;
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //Release
            _data = null;
        }

    }

    private string Get_StatusName(string val)
    {
        if (val.Equals("Y"))
        {
            return "已加入排程";
        }
        else
        {
            return "資料設定中";
        }
    }
    #endregion


    #region -- 資料顯示:已勾選的供應商清單 --

    /// <summary>
    /// 顯示供應商清單, 未加入
    /// </summary>
    private void LookupData_CheckList()
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        int DataCnt = 0;

        //----- 原始資料:條件篩選 -----
        search.Add("ParentID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetSupInvReplyList(search, 0, 99999, out DataCnt, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        lv_CheckedSup.DataSource = query;
        lv_CheckedSup.DataBind();

        //Release
        _data = null;
        query = null;
    }


    #endregion


    #region -- 按鈕事件 --

    protected void btn_Export_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        DataTable DT = new DataTable();
        int DataCnt = 0;

        //----- 原始資料:條件篩選 -----
        search.Add("ParentID", Req_DataID);


        //----- 方法:取得資料(輸出順序以此為主) -----
        var query = _data.GetSupInvReplyList(search, 0, 99999, out DataCnt, out ErrMsg)
            .Select(fld => new
            {
                SupID = fld.SupID,
                SupName = fld.SupName,
                PurWhoName = fld.PurWhoName,
                SupMails = fld.SupMails,
                StockShow = fld.StockShow,
                IsSend = fld.IsSend,
                SendTime = fld.SendTime,
                IsWrite = fld.IsWrite
            });


        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);

        if (myDT.Rows.Count > 0)
        {
            //重新命名欄位標頭
            myDT.Columns["SupID"].ColumnName = "供應商代號";
            myDT.Columns["SupName"].ColumnName = "供應商名稱";
            myDT.Columns["PurWhoName"].ColumnName = "採購人員";
            myDT.Columns["SupMails"].ColumnName = "供應商Email";
            myDT.Columns["StockShow"].ColumnName = "寶工庫存顯示";
            myDT.Columns["IsSend"].ColumnName = "Mail發送";
            myDT.Columns["SendTime"].ColumnName = "發送時間";
            myDT.Columns["IsWrite"].ColumnName = "表單填寫 ";
        }

        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "DataOutput-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
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
            _Req_Lang = value;
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
            _Req_RootID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/SupInvCheck".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
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
            _Req_DataID = value;
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
            string tempUrl = CustomExtension.getCookie("SupInvCheckA");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() + "/Set" : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion

}
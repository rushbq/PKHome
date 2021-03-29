using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using twMGMTData.Controllers;
using PKLib_Method.Methods;


public partial class MGHelp_Search : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);


    //回覆權限
    public bool _ReplyAuth = false;
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //[權限判斷]
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2412");
                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }
                //取得回覆權限
                _ReplyAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2413");

                //Get Class(A:處理狀態, B:需求類別, C:處理記錄類別)
                Get_ClassList("A", filter_ReqStatus, "所有資料", Req_Status); //處理狀態
                Get_ClassList("B", filter_ReqClass, "所有資料", Req_Class); //需求類別

                //Get Data
                LookupDataList(Req_PageIdx);

            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示 --

    /// <summary>
    /// 取得資料
    /// </summary>
    /// <param name="pageIndex"></param>
    private void LookupDataList(int pageIndex)
    {
        //----- 宣告:網址參數 -----
        int RecordsPerPage = 20;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        int DataCnt = 0;
        ArrayList PageParam = new ArrayList();  //分類暫存條件參數

        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            #region >> 條件篩選 <<
            //Params
            string _DateType = Req_DateType;
            string _sDate = Req_sDate;
            string _eDate = Req_eDate;
            string _Cls1 = Req_Class; //需求類別
            string _Cls2 = Req_Status; //處理狀態
            string _Keyword = Req_Keyword;
            string _Dept = Req_Dept;
            string _Who = Req_Who;
            string _FinishWho = Req_FinishWho;


            //[查詢條件] - 日期選項(放在日期參數前)
            if (!string.IsNullOrWhiteSpace(_DateType))
            {
                search.Add("DateType", _DateType);
                PageParam.Add("DateType=" + Server.UrlEncode(_DateType));
                filter_dateType.Text = _DateType;
            }
            //[查詢條件] - 日期(Start-End)
            if (!string.IsNullOrWhiteSpace(_sDate))
            {
                search.Add("sDate", _sDate);
                PageParam.Add("sDate=" + Server.UrlEncode(_sDate));
                filter_sDate.Text = _sDate;
            }
            if (!string.IsNullOrWhiteSpace(_eDate))
            {
                search.Add("eDate", _eDate);
                PageParam.Add("eDate=" + Server.UrlEncode(_eDate));
                filter_eDate.Text = _eDate;
            }

            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(_Keyword))
            {
                search.Add("Keyword", _Keyword);
                PageParam.Add("k=" + Server.UrlEncode(_Keyword));
                filter_Keyword.Text = _Keyword;
            }

            //[查詢條件] - 需求類別
            if (!string.IsNullOrWhiteSpace(_Cls1))
            {
                search.Add("ReqClass", _Cls1);
                PageParam.Add("Cls1=" + Server.UrlEncode(_Cls1));
                filter_ReqClass.SelectedValue = _Cls1;
            }
            //[查詢條件] - 處理狀態
            if (!string.IsNullOrWhiteSpace(_Cls2))
            {
                search.Add("HelpStatus", _Cls2);
                PageParam.Add("Cls2=" + Server.UrlEncode(_Cls2));
                filter_ReqStatus.SelectedValue = _Cls2;
            }

            //[查詢條件] - 部門
            if (!string.IsNullOrWhiteSpace(_Dept))
            {
                search.Add("Dept", _Dept);
                PageParam.Add("dept=" + Server.UrlEncode(_Dept));
                filter_Dept.Text = _Dept;
                val_Dept.Text = _Dept;
            }
            //[查詢條件] - 需求者
            if (!string.IsNullOrWhiteSpace(_Who))
            {
                search.Add("ReqWho", _Who);
                PageParam.Add("who=" + Server.UrlEncode(_Who));
                filter_Emp.Text = _Who;
                val_Emp.Text = _Who;
            }
            //[查詢條件] - 結案人
            if (!string.IsNullOrWhiteSpace(_FinishWho))
            {
                search.Add("FinishWho", _FinishWho);
                PageParam.Add("fwho=" + Server.UrlEncode(_FinishWho));
                filter_FinishWho.Text = _FinishWho;
                val_FinishWho.Text = _FinishWho;
            }

            //[查詢條件] - 未結案
            if (Req_unClose.Equals("Y"))
            {
                search.Add("unClose", Req_unClose);
                PageParam.Add("uc=" + Server.UrlEncode(Req_unClose));
            }
            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.Get_MGhelpList(search, StartRow, RecordsPerPage, true
                , out DataCnt, out ErrMsg);

            //----- 資料整理:取得總筆數 -----
            TotalRow = DataCnt;

            //----- 資料整理:頁數判斷 -----
            if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
            {
                StartRow = 0;
                pageIndex = 1;
            }

            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
            lvDataList.DataBind();

            //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
            if (query.Count() == 0)
            {
                ph_EmptyData.Visible = true;
                ph_Data.Visible = false;

                //Clear
                CustomExtension.setCookie("ITHelp", "", -1);
            }
            else
            {
                ph_EmptyData.Visible = false;
                ph_Data.Visible = true;

                //分頁設定
                string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                    , thisPage, PageParam, false, true);

                Literal lt_Pager = (Literal)this.lvDataList.FindControl("lt_Pager");
                lt_Pager.Text = getPager;

                //重新整理頁面Url
                string reSetPage = "{0}?page={1}{2}".FormatThis(
                    thisPage
                    , pageIndex
                    , (PageParam.Count == 0 ? "" : "&") + string.Join("&", PageParam.ToArray()));

                //暫存頁面Url, 給其他頁使用
                CustomExtension.setCookie("ITHelp", Server.UrlEncode(reSetPage), 1);

            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _data = null;
        }

    }

    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;
        string Get_TraceID = ((HiddenField)e.Item.FindControl("hf_TraceID")).Value;

        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();
        try
        {
            //----- 方法:刪除資料 -----
            if (false == _data.Delete_MGHelp(Get_DataID))
            {
                CustomExtension.AlertMsg("刪除失敗", ErrMsg);
                return;
            }
            else
            {
                //刪除檔案
                string ftpFolder = "{0}MG_Help/{1}".FormatThis(
                    System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]
                    , Get_TraceID);
                _ftp.FTP_DelFolder(ftpFolder);


                //導向本頁(帶參數)
                Response.Redirect(filterUrl());
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            _data = null;
        }

    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                //Declare
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;
                MGMTRepository _data = new MGMTRepository();

                //取得資料:onTop
                string _onTop = DataBinder.Eval(dataItem.DataItem, "onTop").ToString();
                Literal lt_onTop = (Literal)e.Item.FindControl("lt_onTop");
                lt_onTop.Text = _onTop.Equals("Y") ? "<i class=\"thumbtack icon grey-text text-darken-1\"></i>" : "";

                //取得資料:Help_Status, Help_StatusName
                string _helpStatus = DataBinder.Eval(dataItem.DataItem, "Help_Status").ToString();
                string _helpStatusName = DataBinder.Eval(dataItem.DataItem, "Help_StatusName").ToString();

                //Label文字設定
                Literal lt_Status = (Literal)e.Item.FindControl("lt_Status");
                lt_Status.Text = _data.GetMgHelp_StatusLabel(_helpStatus, _helpStatusName);

                #region >> 權限判斷 <<

                PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                PlaceHolder ph_Del = (PlaceHolder)e.Item.FindControl("ph_Del");

                //回覆權限
                if (_ReplyAuth)
                {
                    //可刪可改
                    ph_Edit.Visible = true;
                    ph_Del.Visible = true;
                }
                else
                {
                    //一般權限(待處理Help_Status=110)可改
                    ph_Edit.Visible = _helpStatus.Equals("110");
                    ph_Del.Visible = false;
                }
                #endregion

                _data = null;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 查詢
    /// </summary>
    protected void btn_Search_Click(object sender, EventArgs e)
    {
        //執行查詢
        Response.Redirect(filterUrl(), false);
    }



    /// <summary>
    /// [按鈕] - 匯出
    /// </summary>
    protected void btn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<
        //Params
        string _DateType = Req_DateType;
        string _sDate = Req_sDate;
        string _eDate = Req_eDate;
        string _Cls1 = Req_Class; //需求類別
        string _Cls2 = Req_Status; //處理狀態
        string _Keyword = Req_Keyword;
        string _Dept = Req_Dept;
        string _Who = Req_Who;
        string _FinishWho = Req_FinishWho;

        //[查詢條件] - 日期選項(放在日期參數前)
        if (!string.IsNullOrWhiteSpace(_DateType))
        {
            search.Add("DateType", _DateType);
        }

        //[查詢條件] - 登記日期(Start-End)
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            search.Add("sDate", _sDate);
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            search.Add("eDate", _eDate);
        }

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            search.Add("Keyword", _Keyword);
        }

        //[查詢條件] - 需求類別
        if (!string.IsNullOrWhiteSpace(_Cls1))
        {
            search.Add("ReqClass", _Cls1);
        }
        //[查詢條件] - 處理狀態
        if (!string.IsNullOrWhiteSpace(_Cls2))
        {
            search.Add("HelpStatus", _Cls2);
        }

        //[查詢條件] - 部門
        if (!string.IsNullOrWhiteSpace(_Dept))
        {
            search.Add("Dept", _Dept);
        }
        //[查詢條件] - 需求者
        if (!string.IsNullOrWhiteSpace(_Who))
        {
            search.Add("ReqWho", _Who);
        }
        //[查詢條件] - 結案人
        if (!string.IsNullOrWhiteSpace(_FinishWho))
        {
            search.Add("FinishWho", _FinishWho);
        }

        //[查詢條件] - 未結案
        if (Req_unClose.Equals("Y"))
        {
            search.Add("unClose", Req_unClose);
        }

        #endregion


        //----- 原始資料:取得所有資料 -----
        int DataCnt = 0;
        var _rowData = _data.Get_MGhelpList(search, -1, -1, false
            , out DataCnt, out ErrMsg);

        if (_rowData.Count() == 0)
        {
            CustomExtension.AlertMsg("目前條件查不到資料.", "");
            return;
        }

        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(_rowData);

        #region ** 填入指定欄位 **

        Dictionary<string, string> _col = new Dictionary<string, string>();
        _col.Add("CreateDay", "登記日");
        _col.Add("Req_ClassName", "需求類別");
        _col.Add("Help_Subject", "主旨");
        _col.Add("Help_Content", "說明");
        _col.Add("Req_WhoName", "需求者");
        _col.Add("Finish_Hours", "工時");
        _col.Add("RateScore", "滿意度(1-5)");
        _col.Add("Wish_Time", "預計完成");
        _col.Add("Finish_Time", "結案日");
        _col.Add("Finish_WhoName", "結案人");
        _col.Add("TraceID", "追蹤編號");


        //將指定的欄位,轉成陣列
        string[] selectedColumns = _col.Keys.ToArray();

        //資料複製到新的Table(內容為指定的欄位資料)
        DataTable newDT = new DataView(myDT).ToTable(true, selectedColumns);


        #endregion


        #region ** 重新命名欄位,顯示為中文 **

        foreach (var item in _col)
        {
            string _id = item.Key;
            string _name = item.Value;

            newDT.Columns[_id].ColumnName = _name;

        }
        #endregion

        //匯出Excel
        CustomExtension.ExportExcel(
            newDT
            , "ExcelData-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }
    #endregion


    #region -- 附加功能 --


    /// <summary>
    /// 取得類別
    /// </summary>
    /// <param name="clsType">A:處理狀態, B:需求類別, C:處理記錄類別</param>
    /// <param name="ddl"></param>
    /// <param name="rootName"></param>
    /// <param name="inputValue"></param>
    private void Get_ClassList(string clsType, DropDownList ddl, string rootName, string inputValue)
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        //----- 原始資料:取得所有資料 -----
        DataTable query = _data.GetClass_MGhelp(clsType, out ErrMsg);

        //----- 資料整理 -----
        ddl.Items.Clear();

        if (!string.IsNullOrEmpty(rootName))
        {
            ddl.Items.Add(new ListItem(rootName, ""));
        }

        for (int row = 0; row < query.Rows.Count; row++)
        {
            ddl.Items.Add(new ListItem(
                query.Rows[row]["Label"].ToString()
                , query.Rows[row]["ID"].ToString()
                ));
        }

        //被選擇值
        if (!string.IsNullOrWhiteSpace(inputValue))
        {
            ddl.SelectedIndex = ddl.Items.IndexOf(ddl.Items.FindByValue(inputValue));
        }

        query = null;
    }


    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _DateType = filter_dateType.SelectedValue;
        string _sDate = this.filter_sDate.Text;
        string _eDate = this.filter_eDate.Text;
        string _Keyword = this.filter_Keyword.Text;
        string _Cls1 = this.filter_ReqClass.SelectedValue;
        string _Cls2 = this.filter_ReqStatus.SelectedValue;
        string _dept = this.val_Dept.Text;
        string _who = this.val_Emp.Text;
        string _fwho = this.val_FinishWho.Text;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page
        url.Append("{0}?page=1&uc={1}".FormatThis(thisPage, Req_unClose));

        //[查詢條件] - 日期選項(放在日期參數前)
        if (!string.IsNullOrWhiteSpace(_DateType))
        {
            url.Append("&DateType=" + Server.UrlEncode(_DateType));
        }

        //[查詢條件] - Date between
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
        }

        //[查詢條件] - 需求類別
        if (!string.IsNullOrWhiteSpace(_Cls1))
        {
            url.Append("&Cls1=" + Server.UrlEncode(_Cls1));
        }
        //[查詢條件] - 處理狀態
        if (!string.IsNullOrWhiteSpace(_Cls2))
        {
            url.Append("&Cls2=" + Server.UrlEncode(_Cls2));
        }

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            url.Append("&k=" + Server.UrlEncode(_Keyword));
        }
        //[查詢條件] - 需求部門
        if (!string.IsNullOrWhiteSpace(_dept))
        {
            url.Append("&dept=" + Server.UrlEncode(_dept));
        }
        //[查詢條件] - 需求者
        if (!string.IsNullOrWhiteSpace(_who))
        {
            url.Append("&who=" + Server.UrlEncode(_who));
        }
        //[查詢條件] - 結案人
        if (!string.IsNullOrWhiteSpace(_fwho))
        {
            url.Append("&fwho=" + Server.UrlEncode(_fwho));
        }

        return url.ToString();
    }

    #endregion


    #region -- 網址參數 --


    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}twManagement/".FormatThis(fn_Param.WebUrl);
    }

    #endregion


    #region -- 傳遞參數 --
    /// <summary>
    /// 取得傳遞參數 - PageIdx(目前索引頁)
    /// </summary>
    public int Req_PageIdx
    {
        get
        {
            int data = Request.QueryString["Page"] == null ? 1 : Convert.ToInt32(Request.QueryString["Page"]);
            return data;
        }
        set
        {
            this._Req_PageIdx = value;
        }
    }
    private int _Req_PageIdx;

    /// <summary>
    /// 取得傳遞參數 - Keyword
    /// </summary>
    public string Req_Keyword
    {
        get
        {
            String _data = Request.QueryString["k"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Keyword = value;
        }
    }
    private string _Req_Keyword;

    /// <summary>
    /// 取得傳遞參數 - sDate (預設180 days內)
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.AddDays(-365).ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_sDate = value;
        }
    }
    private string _Req_sDate;

    /// <summary>
    /// 取得傳遞參數 - eDate
    /// </summary>
    public string Req_eDate
    {
        get
        {
            String _data = Request.QueryString["eDate"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_eDate = value;
        }
    }
    private string _Req_eDate;

    /// <summary>
    /// 取得傳遞參數 - 需求類別
    /// </summary>
    public string Req_Class
    {
        get
        {
            String _data = Request.QueryString["Cls1"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "4", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Class = value;
        }
    }
    private string _Req_Class;

    /// <summary>
    /// 取得傳遞參數 - 處理狀態
    /// </summary>
    public string Req_Status
    {
        get
        {
            String _data = Request.QueryString["Cls2"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "4", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Status = value;
        }
    }
    private string _Req_Status;

    /// <summary>
    /// 取得傳遞參數 - 部門
    /// </summary>
    public string Req_Dept
    {
        get
        {
            String _data = Request.QueryString["dept"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "5", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Dept = value;
        }
    }
    private string _Req_Dept;


    /// <summary>
    /// 取得傳遞參數 - 需求者
    /// </summary>
    public string Req_Who
    {
        get
        {
            String _data = Request.QueryString["who"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "5", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Who = value;
        }
    }
    private string _Req_Who;


    /// <summary>
    /// 取得傳遞參數 - 結案人
    /// </summary>
    public string Req_FinishWho
    {
        get
        {
            String _data = Request.QueryString["fwho"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "5", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_FinishWho = value;
        }
    }
    private string _Req_FinishWho;


    /// <summary>
    /// 取得傳遞參數 - unClose
    /// </summary>
    public string Req_unClose
    {
        get
        {
            String _data = Request.QueryString["uc"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "N";
        }
        set
        {
            this._Req_unClose = value;
        }
    }
    private string _Req_unClose;

    /// <summary>
    /// 取得傳遞參數 - DateType
    /// </summary>
    private string _Req_DateType;
    public string Req_DateType
    {
        get
        {
            String data = Request.QueryString["DateType"];
            return string.IsNullOrEmpty(data) ? "A" : data.ToString();
        }
        set
        {
            this._Req_DateType = value;
        }
    }


    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}MGHelp_Search.aspx".FormatThis(FuncPath());
        }
        set
        {
            this._thisPage = value;
        }
    }
    private string _thisPage;

    #endregion
}
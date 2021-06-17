using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Menu2000Data.Controllers;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class myMarketingHelp_Search : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;
    public bool masterAuth = false; //主管權限(可在權限設定裡勾選)
    public bool replyAuth = false; //回覆權限(可在權限設定裡勾選)

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
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
                    //    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4152");
                    //    break;

                    case "2":
                        //SZ
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

                //取得公司別
                this.lt_CorpName.Text = fn_Param.GetCorpName(getCorpUid);

                //[權限判斷] End
                #endregion

                //[產生選單]
                Get_ClassList(Req_CompID, "需求類別", filter_ReqClass, GetLocalResourceObject("txt_所有資料").ToString());
                Get_ClassList(Req_CompID, "需求資源", filter_ReqRes, GetLocalResourceObject("txt_所有資料").ToString());
                Get_ClassList(Req_CompID, "緊急度", filter_EmgStatus, GetLocalResourceObject("txt_所有資料").ToString());
                Get_ClassList(Req_CompID, "處理狀態", filter_ReqStatus, GetLocalResourceObject("txt_所有資料").ToString());

                //[產生選單] 處理者(行企)
                Get_Processer(ddl_ProcWho, Req_Proc);

                /* 多語系設定 */
                Page.Title = GetLocalResourceObject("pageTitle").ToString();
                filter_Keyword.Attributes.Add("placeholder", GetLocalResourceObject("sh_追蹤編號").ToString());
                filter_sDate.Attributes.Add("placeholder", GetLocalResourceObject("sh_開始日").ToString());
                filter_eDate.Attributes.Add("placeholder", GetLocalResourceObject("sh_結束日").ToString());
                filter_Dept.Attributes.Add("placeholder", GetLocalResourceObject("sh_tip2").ToString());
                filter_Emp.Attributes.Add("placeholder", GetLocalResourceObject("sh_tip2").ToString());


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
    private void LookupDataList(int pageIndex)
    {
        //----- 宣告:網址參數 -----
        int RecordsPerPage = 20;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        Dictionary<string, string> sort = new Dictionary<string, string>();

        #region >> 條件篩選 <<
        string _DateType = Req_DateType;

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);
            PageParam.Add("k=" + Server.UrlEncode(Req_Keyword));
            filter_Keyword.Text = Req_Keyword;
        }

        //[查詢條件] - 日期選項(放在日期參數前)
        if (!string.IsNullOrWhiteSpace(_DateType))
        {
            search.Add("DateType", _DateType);
            PageParam.Add("DateType=" + Server.UrlEncode(_DateType));
            filter_dateType.Text = _DateType;
        }
        //[查詢條件] - sDate
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate);
            PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
            filter_sDate.Text = Req_sDate;
        }
        //[查詢條件] - eDate
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate);
            PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
            filter_eDate.Text = Req_eDate;
        }

        //[查詢條件] - reqStat
        if (!string.IsNullOrWhiteSpace(Req_St))
        {
            search.Add("reqStat", Req_St);
            PageParam.Add("st=" + Server.UrlEncode(Req_St));
            filter_ReqStatus.SelectedValue = Req_St;
        }
        //[查詢條件] - reqClass
        if (!string.IsNullOrWhiteSpace(Req_Cls))
        {
            search.Add("reqClass", Req_Cls);
            PageParam.Add("cls=" + Server.UrlEncode(Req_Cls));
            filter_ReqClass.SelectedValue = Req_Cls;
        }
        //[查詢條件] - reqRes
        if (!string.IsNullOrWhiteSpace(Req_Res))
        {
            search.Add("reqRes", Req_Res);
            PageParam.Add("res=" + Server.UrlEncode(Req_Res));
            filter_ReqRes.SelectedValue = Req_Res;
        }
        //[查詢條件] - emgStat
        if (!string.IsNullOrWhiteSpace(Req_Emg))
        {
            search.Add("emgStat", Req_Emg);
            PageParam.Add("emg=" + Server.UrlEncode(Req_Emg));
            filter_EmgStatus.SelectedValue = Req_Emg;
        }

        //[查詢條件] - reqDept
        if (!string.IsNullOrWhiteSpace(Req_Dept))
        {
            search.Add("reqDept", Req_Dept);
            PageParam.Add("dept=" + Server.UrlEncode(Req_Dept));
            filter_Dept.Text = Req_Dept;
            val_Dept.Text = Req_Dept;
        }
        //[查詢條件] - reqWho
        if (!string.IsNullOrWhiteSpace(Req_Who))
        {
            search.Add("reqWho", Req_Who);
            PageParam.Add("who=" + Server.UrlEncode(Req_Who));
            filter_Emp.Text = Req_Who;
            val_Emp.Text = Req_Who;
        }

        //[查詢條件] - 處理人員
        if (!string.IsNullOrWhiteSpace(Req_Proc))
        {
            search.Add("Proc", Req_Proc);
            PageParam.Add("Proc=" + Server.UrlEncode(Req_Proc));
            val_Proc.Text = Req_Proc;
        }

        //[排序條件]
        if (!string.IsNullOrWhiteSpace(Req_Sf))
        {
            sort.Add("Field", Req_Sf);
            PageParam.Add("sf=" + Server.UrlEncode(Req_Sf));
            sort_SortField.SelectedIndex = sort_SortField.Items.IndexOf(sort_SortField.Items.FindByValue(Req_Sf));
        }
        if (!string.IsNullOrWhiteSpace(Req_Sw))
        {
            sort.Add("Way", Req_Sw);
            PageParam.Add("sw=" + Server.UrlEncode(Req_Sw));
            sort_SortWay.SelectedIndex = sort_SortWay.Items.IndexOf(sort_SortWay.Items.FindByValue(Req_Sw));
        }
        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpList(Req_CompID, search, sort, out ErrMsg);

        //----- 資料整理:取得總筆數 -----
        TotalRow = query.Count();

        //----- 資料整理:頁數判斷 -----
        if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
        {
            StartRow = 0;
            pageIndex = 1;
        }

        //----- 資料整理:選取每頁顯示筆數 -----
        var data = query.Skip(StartRow).Take(RecordsPerPage);

        //----- 資料整理:繫結 ----- 
        this.lvDataList.DataSource = data;
        this.lvDataList.DataBind();

        //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
        if (query.Count() == 0)
        {
            this.ph_EmptyData.Visible = true;
            this.ph_Data.Visible = false;

            //Clear
            CustomExtension.setCookie("HomeList_MKHelp", "", -1);
        }
        else
        {
            this.ph_EmptyData.Visible = false;
            this.ph_Data.Visible = true;

            //分頁設定
            string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                , thisPage, PageParam, false, true);

            Literal lt_Pager = (Literal)lvDataList.FindControl("lt_Pager");
            lt_Pager.Text = getPager;

            //重新整理頁面Url
            string reSetPage = "{0}?Page={1}{2}".FormatThis(
                thisPage
                , pageIndex
                , "&" + string.Join("&", PageParam.ToArray()));

            //暫存頁面Url, 給其他頁使用
            CustomExtension.setCookie("HomeList_MKHelp", Server.UrlEncode(reSetPage), 1);

            /* 多語系設定 -Table header */
            ((Literal)lvDataList.FindControl("lt_header1")).Text = GetLocalResourceObject("header1").ToString();
            ((Literal)lvDataList.FindControl("lt_header2")).Text = GetLocalResourceObject("header2").ToString();
            ((Literal)lvDataList.FindControl("lt_header3")).Text = GetLocalResourceObject("header3").ToString();
            ((Literal)lvDataList.FindControl("lt_header4")).Text = GetLocalResourceObject("header4").ToString();
            ((Literal)lvDataList.FindControl("lt_header5")).Text = GetLocalResourceObject("header5").ToString();
            ((Literal)lvDataList.FindControl("lt_header6")).Text = GetLocalResourceObject("header6").ToString();
            ((Literal)lvDataList.FindControl("lt_header7")).Text = GetLocalResourceObject("header7").ToString();
            ((Literal)lvDataList.FindControl("lt_header8")).Text = GetLocalResourceObject("header8").ToString();
            ((Literal)lvDataList.FindControl("lt_header10")).Text = GetLocalResourceObject("header10").ToString();
        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 方法:刪除資料 -----
        if (false == _data.Delete_MKHelpData(Get_DataID))
        {
            CustomExtension.AlertMsg("刪除失敗", "");
            return;
        }
        else
        {
            //刪除檔案
            string ftpFolder = UploadFolder + Get_DataID;
            _ftp.FTP_DelFolder(ftpFolder);

            //導向本頁
            Response.Redirect(thisPage);
        }
    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取得資料:處理人員數
                Int16 _procCnt = Convert.ToInt16(DataBinder.Eval(dataItem.DataItem, "ProcCnt"));
                PlaceHolder ph_ProcWho = (PlaceHolder)e.Item.FindControl("ph_ProcWho");
                ph_ProcWho.Visible = !_procCnt.Equals(0);

                //取得資料:是否逾時
                string _timeOut = DataBinder.Eval(dataItem.DataItem, "IsTimeOut").ToString();
                //取得控制項
                Literal lt_IsTimeout = (Literal)e.Item.FindControl("lt_IsTimeout");
                lt_IsTimeout.Text = _timeOut.Equals("Y") ? "<i class=\"attention icon\" title=\"已逾期\"></i>" : "";
                HtmlControl trItem = (HtmlControl)e.Item.FindControl("trItem");
                trItem.Attributes.Add("class", _timeOut.Equals("Y") ? "negative" : "");


                //取得資料:是否結案
                string _status = DataBinder.Eval(dataItem.DataItem, "StDisp").ToString();
                //取得控制項
                PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                PlaceHolder ph_Del = (PlaceHolder)e.Item.FindControl("ph_Del");

                //判斷權限
                if (!replyAuth && !masterAuth)
                {
                    //一般使用者
                    ph_Edit.Visible = _status.Equals("A");
                    ph_Del.Visible = _status.Equals("A");
                }
                else
                {
                    switch (_status)
                    {
                        case "D":
                        case "E":
                            //已結案
                            ph_Edit.Visible = false;
                            ph_Del.Visible = false;

                            break;


                        case "C":
                            //處理中
                            ph_Edit.Visible = true;
                            ph_Del.Visible = masterAuth;

                            break;

                        default:
                            //未處理/派案中
                            ph_Edit.Visible = true;
                            ph_Del.Visible = true;

                            break;
                    }
                }

            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// 回傳狀態描述
    /// </summary>
    /// <param name="StDisp"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    public string Get_Status(string StDisp, string label)
    {
        string css = "";

        switch (StDisp)
        {
            case "A":
                //未處理
                css = "ui orange label";
                break;

            case "B":
                //派案中
                css = "ui yellow label";
                break;

            case "C":
                //處理中
                css = "ui blue label";
                break;

            case "D":
                //已結案
                css = "ui green basic label";
                break;
        }

        return "<div class=\"{0}\">{1}</div>".FormatThis(css, label);
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
    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        DataTable DT = new DataTable();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<
        string _DateType = Req_DateType;

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);
        }

        //[查詢條件] - 日期選項(放在日期參數前)
        if (!string.IsNullOrWhiteSpace(_DateType))
        {
            search.Add("DateType", _DateType);
        }

        //[查詢條件] - sDate
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate);
        }
        //[查詢條件] - eDate
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate);
        }

        //[查詢條件] - reqStat
        if (!string.IsNullOrWhiteSpace(Req_St))
        {
            search.Add("reqStat", Req_St);
        }
        //[查詢條件] - reqClass
        if (!string.IsNullOrWhiteSpace(Req_Cls))
        {
            search.Add("reqClass", Req_Cls);
        }
        //[查詢條件] - reqRes
        if (!string.IsNullOrWhiteSpace(Req_Res))
        {
            search.Add("reqRes", Req_Res);
        }
        //[查詢條件] - emgStat
        if (!string.IsNullOrWhiteSpace(Req_Emg))
        {
            search.Add("emgStat", Req_Emg);
        }

        //[查詢條件] - reqDept
        if (!string.IsNullOrWhiteSpace(Req_Dept))
        {
            search.Add("reqDept", Req_Dept);
        }
        //[查詢條件] - reqWho
        if (!string.IsNullOrWhiteSpace(Req_Who))
        {
            search.Add("reqWho", Req_Who);
        }

        //[查詢條件] - 處理人員
        if (!string.IsNullOrWhiteSpace(Req_Proc))
        {
            search.Add("Proc", Req_Proc);
        }

        #endregion

        //----- 方法:取得資料(輸出順序以此為主) -----
        var query = _data.GetMKHelpList(Req_CompID, search, null, out ErrMsg)
            .Select(fld => new
            {
                TraceID = fld.TraceID,
                Req_Subject = fld.Req_Subject,
                StName = fld.StName,
                Req_Qty = fld.Req_Qty,
                EmgName = fld.EmgName,
                TypeName = fld.TypeName,
                ResName = fld.ResName,
                Create_Time = fld.Create_Time.ToDateString("yyyy/MM/dd"),
                Req_Name = fld.Req_Name,
                Finish_Date = fld.Finish_Date
            });

        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);

        if (myDT.Rows.Count > 0)
        {
            //重新命名欄位標頭
            myDT.Columns["TraceID"].ColumnName = GetLocalResourceObject("header1").ToString();
            myDT.Columns["Req_Subject"].ColumnName = GetLocalResourceObject("header2").ToString();
            myDT.Columns["StName"].ColumnName = GetLocalResourceObject("header3").ToString();
            myDT.Columns["Req_Qty"].ColumnName = GetLocalResourceObject("header10").ToString();
            myDT.Columns["EmgName"].ColumnName = GetLocalResourceObject("header4").ToString();
            myDT.Columns["TypeName"].ColumnName = GetLocalResourceObject("header5").ToString() + "1";
            myDT.Columns["ResName"].ColumnName = GetLocalResourceObject("header5").ToString() + "2";
            myDT.Columns["Create_Time"].ColumnName = GetLocalResourceObject("header6").ToString();
            myDT.Columns["Req_Name"].ColumnName = GetLocalResourceObject("header7").ToString();
            myDT.Columns["Finish_Date"].ColumnName = GetLocalResourceObject("header9").ToString();
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


    #region -- 附加功能 --
    /// <summary>
    /// 取得類別資料 
    /// </summary>
    /// <param name="compID">compID</param>
    /// <param name="typeName">類別Type</param>
    /// <param name="ddl">下拉選單object</param>
    /// <param name="rootName">第一選項顯示名稱</param>
    private void Get_ClassList(string compID, string typeName, DropDownList ddl, string rootName)
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpClass(compID, typeName, out ErrMsg);


        //----- 資料整理 -----
        ddl.Items.Clear();

        if (!string.IsNullOrEmpty(rootName))
        {
            ddl.Items.Add(new ListItem(rootName, ""));
        }

        foreach (var item in query)
        {
            ddl.Items.Add(new ListItem(item.Label, item.ID.ToString()));
        }

        query = null;
    }


    /// <summary>
    /// 取得處理人員(指定部門)
    /// </summary>
    /// <param name="ddl"></param>
    /// <param name="reqValue">傳入已勾選(以逗號分隔)</param>
    private void Get_Processer(ListBox ddl, string reqValue)
    {
        //----- Clear -----
        ddl.Items.Clear();

        //----- 宣告 -----
        UsersRepository _user = new UsersRepository();
        Dictionary<int, string> _dept = new Dictionary<int, string>();

        //----- 取得資料 -----
        if (Req_CompID.Equals("TW"))
        {
            //條件:台灣行企
            _dept.Add(1, "180");
        }
        else
        {
            //條件:深圳行企
            _dept.Add(1, "314");
        }

        //呼叫並回傳資料
        var getUsers = _user.GetUsers(null, _dept);
        //選單設定root
        ddl.Items.Add(new ListItem("未指定", ""));
        //選單設定選項
        foreach (var item in getUsers)
        {
            ddl.Items.Add(new ListItem("{0} ({1})".FormatThis(item.ProfName, item.NickName), item.ProfID));
        }

        //判斷已選擇的值, 並設為selected
        if (!string.IsNullOrWhiteSpace(reqValue))
        {
            //將來源字串轉為陣列,以逗號為分隔
            string[] strAry = Regex.Split(reqValue, @"\,{1}");
            //使用LINQ整理資料
            var query = from el in strAry
                        select new
                        {
                            selectedVal = el.ToString()
                        };
            //使用迴圈方式,將選項設為selected
            for (int row = 0; row < ddl.Items.Count; row++)
            {
                foreach (var item in query)
                {
                    if (ddl.Items[row].Value.Equals(item.selectedVal))
                    {
                        ddl.Items[row].Selected = true;
                    }
                }
            }

        }

        getUsers = null;
    }

    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _Keyword = this.filter_Keyword.Text;
        string _DateType = filter_dateType.SelectedValue;
        string _sDate = this.filter_sDate.Text;
        string _eDate = this.filter_eDate.Text;
        string _st = this.filter_ReqStatus.SelectedValue;
        string _cls = this.filter_ReqClass.SelectedValue;
        string _res = this.filter_ReqRes.SelectedValue;
        string _emg = this.filter_EmgStatus.SelectedValue;
        string _dept = this.val_Dept.Text;
        string _who = this.val_Emp.Text;
        string _ProcWho = val_Proc.Text;
        string _SortField = this.sort_SortField.SelectedValue;
        string _SortWay = this.sort_SortWay.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?page=1".FormatThis(thisPage));

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            url.Append("&k=" + Server.UrlEncode(_Keyword));
        }

        //[查詢條件] - 日期選項(放在日期參數前)
        if (!string.IsNullOrWhiteSpace(_DateType))
        {
            url.Append("&DateType=" + Server.UrlEncode(_DateType));
        }

        //[查詢條件] - sDate
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        //[查詢條件] - eDate
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
        }

        //[查詢條件] - reqStat
        if (!string.IsNullOrWhiteSpace(_st))
        {
            url.Append("&st=" + Server.UrlEncode(_st));
        }
        //[查詢條件] - reqClass
        if (!string.IsNullOrWhiteSpace(_cls))
        {
            url.Append("&cls=" + Server.UrlEncode(_cls));
        }
        //[查詢條件] - reqRes
        if (!string.IsNullOrWhiteSpace(_res))
        {
            url.Append("&res=" + Server.UrlEncode(_res));
        }
        //[查詢條件] - emgStat
        if (!string.IsNullOrWhiteSpace(_emg))
        {
            url.Append("&emg=" + Server.UrlEncode(_emg));
        }

        //[查詢條件] - reqDept
        if (!string.IsNullOrWhiteSpace(_dept))
        {
            url.Append("&dept=" + Server.UrlEncode(_dept));
        }
        //[查詢條件] - reqWho
        if (!string.IsNullOrWhiteSpace(_who))
        {
            url.Append("&who=" + Server.UrlEncode(_who));
        }

        //[查詢條件] - 處理人員
        if (!string.IsNullOrWhiteSpace(_ProcWho))
        {
            url.Append("&Proc=" + Server.UrlEncode(_ProcWho));
        }

        //[排序條件]
        if (!string.IsNullOrWhiteSpace(_SortField))
        {
            url.Append("&sf=" + _SortField);
            url.Append("&sw=" + _SortWay);
        }

        return url.ToString();
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

    public string Req_Keyword
    {
        get
        {
            String _data = Request.QueryString["k"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_Keyword = value;
        }
    }
    private string _Req_Keyword;


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

    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_sDate = value;
        }
    }
    private string _Req_sDate;

    public string Req_eDate
    {
        get
        {
            String _data = Request.QueryString["eDate"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_eDate = value;
        }
    }
    private string _Req_eDate;

    /// <summary>
    /// ReqStatus
    /// </summary>
    public string Req_St
    {
        get
        {
            String _data = Request.QueryString["st"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_St = value;
        }
    }
    private string _Req_St;

    /// <summary>
    /// ReqClass
    /// </summary>
    public string Req_Cls
    {
        get
        {
            String _data = Request.QueryString["cls"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_Cls = value;
        }
    }
    private string _Req_Cls;

    /// <summary>
    /// ReqRes
    /// </summary>
    public string Req_Res
    {
        get
        {
            String _data = Request.QueryString["res"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_Res = value;
        }
    }
    private string _Req_Res;

    /// <summary>
    /// EmgStatus
    /// </summary>
    public string Req_Emg
    {
        get
        {
            String _data = Request.QueryString["emg"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_Emg = value;
        }
    }
    private string _Req_Emg;

    /// <summary>
    /// Dept
    /// </summary>
    public string Req_Dept
    {
        get
        {
            String _data = Request.QueryString["dept"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_Dept = value;
        }
    }
    private string _Req_Dept;

    /// <summary>
    /// Who
    /// </summary>
    public string Req_Who
    {
        get
        {
            String _data = Request.QueryString["who"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_Who = value;
        }
    }
    private string _Req_Who;

    /// <summary>
    /// 取得傳遞參數 - 處理人員
    /// </summary>
    /// <remarks>
    /// 網頁傳遞方式為字串
    /// 後端處理為陣列
    /// </remarks>
    public string Req_Proc
    {
        get
        {
            String _data = Request.QueryString["Proc"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            this._Req_Proc = value;
        }
    }
    private string _Req_Proc;

    /// <summary>
    /// Sort參數-欄位
    /// </summary>
    public string Req_Sf
    {
        get
        {
            String _data = Request.QueryString["sf"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Sf = value;
        }
    }

    /// <summary>
    /// Sort參數-方式
    /// </summary>
    private string _Req_Sf;
    public string Req_Sw
    {
        get
        {
            String _data = Request.QueryString["sw"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Sw = value;
        }
    }
    private string _Req_Sw;


    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}".FormatThis(FuncPath());
        }
        set
        {
            this._thisPage = value;
        }
    }
    private string _thisPage;


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
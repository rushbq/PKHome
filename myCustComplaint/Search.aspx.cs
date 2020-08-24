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

public partial class myCustComplaint_Search : SecurityCheck
{
    public string ErrMsg;
    public bool editAuth = false; //編輯權限(可在權限設定裡勾選)
    public bool closeAuth = false; //作廢權限(可在權限設定裡勾選)

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                /* [取得MenuID, MenuName]
                   取得對應的MenuID, 檢查是否有權限
                   取得對應的Type Name
                */
                int _ccType = Convert.ToInt32(Req_TypeID);
                var query = fn_Menu.GetOne_RefType(Req_Lang, _ccType, out ErrMsg);
                string menuID = query.MenuID.ToString();
                string typeName = query.Label;

                //取得功能名稱
                lt_TypeName.Text = typeName;
                //設定PageTitle
                Page.Title = typeName + " - 客訴清單";

                //[權限判斷] Start
                #region --權限--
                /* 
                 * 判斷對應的MENU ID
                 * 取得其他權限
                 */
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, menuID);
                bool exportAll, export301;

                switch (Req_TypeID)
                {
                    case "10":
                        //台灣工具
                        editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3234");
                        closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3235");
                        exportAll = fn_CheckAuth.Check(fn_Param.CurrentUser, "323201");
                        export301 = fn_CheckAuth.Check(fn_Param.CurrentUser, "323202");
                        break;

                    case "20":
                        //台灣科玩
                        editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3238");
                        closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3239");
                        exportAll = fn_CheckAuth.Check(fn_Param.CurrentUser, "323601");
                        export301 = fn_CheckAuth.Check(fn_Param.CurrentUser, "323602");
                        break;

                    case "30":
                        //中國工具
                        editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3242");
                        closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3243");
                        exportAll = fn_CheckAuth.Check(fn_Param.CurrentUser, "324001");
                        export301 = fn_CheckAuth.Check(fn_Param.CurrentUser, "324002");
                        break;

                    default:
                        //中國科玩
                        editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3246");
                        closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3247");
                        exportAll = fn_CheckAuth.Check(fn_Param.CurrentUser, "324401");
                        export301 = fn_CheckAuth.Check(fn_Param.CurrentUser, "324402");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //Export Button
                ph_ExportAll.Visible = exportAll;
                ph_ExportFlow301.Visible = export301;

                #endregion
                //[權限判斷] End

                #region --Request參數--
                //預設值 - Req_sDate
                if (!string.IsNullOrWhiteSpace(Req_sDate))
                {
                    filter_sDate.Text = Req_sDate;
                }
                //預設值 - Req_eDate
                if (!string.IsNullOrWhiteSpace(Req_eDate))
                {
                    filter_eDate.Text = Req_eDate;
                }

                #endregion



                //[產生選單]
                Get_ClassList("2", filter_CustType, _ccType, GetLocalResourceObject("txt_所有資料").ToString());
                Get_ClassList("1", filter_FlowStatus, _ccType, GetLocalResourceObject("txt_所有資料").ToString());
                Get_ClassList("11", filter_PlanType, _ccType, GetLocalResourceObject("txt_所有資料").ToString());

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
        int RecordsPerPage = 10;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        int DataCnt = 0;
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<

        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(Req_DType))
        {
            PageParam.Add("dt=" + Server.UrlEncode(Req_DType));
            filter_DateType.SelectedValue = Req_DType;

            //判斷&設定要查詢的日期別
            string _sDt, _eDt;
            switch (Req_DType)
            {
                case "A":
                    _sDt = "sDateA";
                    _eDt = "eDateA";
                    break;

                default:
                    _sDt = "sDateB";
                    _eDt = "eDateB";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(Req_sDate))
            {
                search.Add(_sDt, Req_sDate.ToDateString("yyyy/MM/dd"));
                PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
                filter_sDate.Text = Req_sDate;
            }
            if (!string.IsNullOrWhiteSpace(Req_eDate))
            {
                search.Add(_eDt, Req_eDate.ToDateString("yyyy/MM/dd"));
                PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
                filter_eDate.Text = Req_eDate;
            }
        }


        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);
            PageParam.Add("k=" + Server.UrlEncode(Req_Keyword));
            filter_Keyword.Text = Req_Keyword;
        }

        //[查詢條件] - CustType
        if (!string.IsNullOrWhiteSpace(Req_CustType))
        {
            search.Add("CustType", Req_CustType);
            PageParam.Add("ct=" + Server.UrlEncode(Req_CustType));
            filter_CustType.SelectedValue = Req_CustType;
        }
        //[查詢條件] - PlanType
        if (!string.IsNullOrWhiteSpace(Req_PlanType))
        {
            search.Add("PlanType", Req_PlanType);
            PageParam.Add("pt=" + Server.UrlEncode(Req_PlanType));
            filter_PlanType.SelectedValue = Req_PlanType;
        }
        //[查詢條件] - FlowStatus
        if (!string.IsNullOrWhiteSpace(Req_FlowStatus))
        {
            search.Add("FlowStatus", Req_FlowStatus);
            PageParam.Add("fs=" + Server.UrlEncode(Req_FlowStatus));
            filter_FlowStatus.SelectedValue = Req_FlowStatus;
        }
        //[查詢條件] - TraceID
        if (!string.IsNullOrWhiteSpace(Req_Trace))
        {
            search.Add("TraceID", Req_Trace);
            PageParam.Add("trace=" + Server.UrlEncode(Req_Trace));
            filter_TraceID.Text = Req_Trace;
        }

        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPList(search, Req_Lang, Convert.ToInt32(Req_TypeID)
            , StartRow, RecordsPerPage
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
            CustomExtension.setCookie("HomeList_CCPsetting", "", -1);
        }
        else
        {
            ph_EmptyData.Visible = false;
            ph_Data.Visible = true;
            lt_DataCnt.Text = query.Take(1).FirstOrDefault().unOpenCnt.ToString();  //填入未開案筆數

            //分頁設定
            string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                , thisPage, PageParam, false, true);

            lt_Pager.Text = getPager;

            //重新整理頁面Url
            string reSetPage = "{0}?page={1}{2}".FormatThis(
                thisPage
                , pageIndex
                , (PageParam.Count == 0 ? "" : "&") + string.Join("&", PageParam.ToArray()));

            //暫存頁面Url, 給其他頁使用
            CustomExtension.setCookie("HomeList_CCPsetting", Server.UrlEncode(reSetPage), 1);

            /* 多語系設定 -Table header */
            //((Literal)lvDataList.FindControl("lt_header1")).Text = GetLocalResourceObject("header1").ToString();
            //((Literal)lvDataList.FindControl("lt_header2")).Text = GetLocalResourceObject("header2").ToString();

        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {

    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;


                //取得資料:狀態
                Int32 _flow = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "FlowStatus"));
                bool hidebtn = (_flow.Equals(999) | _flow.Equals(998)); //結案或作廢

                //權限判斷 & 狀態判斷
                PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                PlaceHolder ph_Del = (PlaceHolder)e.Item.FindControl("ph_Del");
                ph_Edit.Visible = editAuth && (!hidebtn);
                ph_Del.Visible = editAuth && (!hidebtn);


                //客訴內容
                string _remark = DataBinder.Eval(dataItem.DataItem, "Remark").ToString();
                string _remarkChk = DataBinder.Eval(dataItem.DataItem, "Remark_Check").ToString();
                Literal lt_Content = (Literal)e.Item.FindControl("lt_Content");
                lt_Content.Text = "{0}".FormatThis(string.IsNullOrWhiteSpace(_remarkChk) ? _remark : _remarkChk);


                //Checkbox顯示否
                PlaceHolder ph_Checkbox = (PlaceHolder)e.Item.FindControl("ph_Checkbox");
                PlaceHolder ph_NoCheckbox = (PlaceHolder)e.Item.FindControl("ph_NoCheckbox");
                ph_Checkbox.Visible = _flow.Equals(401);
                ph_NoCheckbox.Visible = !_flow.Equals(401);

            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    /// <summary>
    /// 狀態CSS
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public string showStatus(string status)
    {
        switch (status)
        {
            case "998":
            case "999":
                return "grey";

            default:
                return "green";

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
    /// 終止流程
    /// </summary>
    protected void btn_doClose_Click(object sender, EventArgs e)
    {
        //取得值
        string _id = val_DataID.Text;
        string _reason = val_closeReason.Text;

        if (string.IsNullOrWhiteSpace(_id) || string.IsNullOrWhiteSpace(_reason))
        {
            CustomExtension.AlertMsg("操作錯誤,請重試", thisPage);
            return;
        }

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 方法:更新資料 -----
        var data = new CCPItem
        {
            Data_ID = new Guid(_id),
            FlowStatus = 0,
            nextFlow = 998,
            Finish_Remark = _reason,
            Update_Who = fn_Param.CurrentUser
        };

        if (false == _data.UpdateCCP_Data(data, out ErrMsg))
        {
            CustomExtension.AlertMsg("流程作廢失敗", "");
            return;
        }

        //[Flow Log]
        var log = new CCPLog
        {
            Parent_ID = new Guid(_id),
            LogType = 1,
            LogSubject = "[作廢]",
            LogDesc = "{0}".FormatThis(_reason),
            FlowID = 998,
            Create_Who = fn_Param.CurrentUser
        };
        _data.CreateCCP_Log(log, out ErrMsg);


        //release
        _data = null;

        //導向本頁
        Response.Redirect(thisPage);
    }


    /// <summary>
    /// 匯出全部
    /// </summary>
    protected void lbtn_Excel_All_Click(object sender, EventArgs e)
    {
        exportDT("All", out ErrMsg);
    }

    /// <summary>
    /// 匯出二線
    /// </summary>
    protected void btn_Excel_301_Click(object sender, EventArgs e)
    {
        exportDT("301", out ErrMsg);
    }

    #endregion


    #region -- 附加功能 --

    /// <summary>
    /// 取得類別資料 
    /// </summary>
    /// <param name="typeID">typeID</param>
    /// <param name="ddl">下拉選單object</param>
    /// <param name="rootName">第一選項顯示名稱</param>
    private void Get_ClassList(string typeID, DropDownList ddl, int _ccType, string rootName)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_RefClass(typeID, Req_Lang, _ccType, out ErrMsg);


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
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _dt = filter_DateType.SelectedValue;
        string _sDate = this.filter_sDate.Text;
        string _eDate = this.filter_eDate.Text;
        string _Keyword = filter_Keyword.Text;
        string _ct = filter_CustType.SelectedValue;
        string _pt = filter_PlanType.SelectedValue;
        string _fs = filter_FlowStatus.SelectedValue;
        string _trace = filter_TraceID.Text;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?page=1".FormatThis(thisPage));

        //[查詢條件] - DateType
        if (!string.IsNullOrWhiteSpace(_dt))
        {
            url.Append("&dt=" + Server.UrlEncode(_dt));
        }
        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
        }

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            url.Append("&k=" + Server.UrlEncode(_Keyword));
        }

        //[查詢條件] - CustType
        if (!string.IsNullOrWhiteSpace(_ct))
        {
            url.Append("&ct=" + Server.UrlEncode(_ct));
        }

        //[查詢條件] - PlanType
        if (!string.IsNullOrWhiteSpace(_pt))
        {
            url.Append("&pt=" + Server.UrlEncode(_pt));
        }

        //[查詢條件] - FlowStatus
        if (!string.IsNullOrWhiteSpace(_fs))
        {
            url.Append("&fs=" + Server.UrlEncode(_fs));
        }

        //[查詢條件] - TraceID
        if (!string.IsNullOrWhiteSpace(_trace))
        {
            url.Append("&trace=" + Server.UrlEncode(_trace));
        }

        return url.ToString();
    }


    /// <summary>
    /// 取得參考類別名稱
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string GetRefClassName(int id)
    {
        if (id.Equals(0))
        {
            return "";
        }

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var data = _data.GetOneCCP_RefClass(id.ToString(), Req_Lang, out ErrMsg).FirstOrDefault();

        try
        {
            if (data == null)
            {
                return "";
            }
            else
            {
                return data.Label;
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            data = null;
            _data = null;
        }

    }


    private void exportDT(string _type, out string ErrMsg)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        int _dataCnt;

        #region >> 條件篩選 <<
        //Params
        string _dt = filter_DateType.SelectedValue;
        string _sDate = this.filter_sDate.Text;
        string _eDate = this.filter_eDate.Text;
        string _Keyword = filter_Keyword.Text;
        string _ct = filter_CustType.SelectedValue;
        string _pt = filter_PlanType.SelectedValue;
        string _fs = filter_FlowStatus.SelectedValue;
        string _trace = filter_TraceID.Text;

        //[查詢條件] - DateType
        if (!string.IsNullOrWhiteSpace(_dt))
        {
            //判斷&設定要查詢的日期別
            string _sDt, _eDt;
            switch (_dt)
            {
                case "A":
                    _sDt = "sDateA";
                    _eDt = "eDateA";
                    break;

                default:
                    _sDt = "sDateB";
                    _eDt = "eDateB";
                    break;
            }

            if (!string.IsNullOrWhiteSpace(_sDate))
            {
                search.Add(_sDt, _sDate);
            }
            if (!string.IsNullOrWhiteSpace(_eDate))
            {
                search.Add(_eDt, _eDate);
            }
        }

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            search.Add("Keyword", _Keyword);
        }

        //[查詢條件] - CustType
        if (!string.IsNullOrWhiteSpace(_ct))
        {
            search.Add("CustType", _ct);
        }

        //[查詢條件] - PlanType
        if (!string.IsNullOrWhiteSpace(_pt))
        {
            search.Add("PlanType", _pt);
        }

        //[查詢條件] - FlowStatus
        if (!string.IsNullOrWhiteSpace(_fs))
        {
            search.Add("FlowStatus", _fs);
        }

        //[查詢條件] - TraceID
        if (!string.IsNullOrWhiteSpace(_trace))
        {
            search.Add("TraceID", _trace);
        }

        #endregion

        //----- 原始資料:取得所有資料 -----
        DataTable myDT;

        switch (_type.ToUpper())
        {
            case "ALL":
                //客訴清單
                var data1 = _data.GetCCPAllList(search, Req_Lang, Convert.ToInt32(Req_TypeID), out _dataCnt, out ErrMsg)
                     .Select(fld => new
                     {
                         CC_UID = fld.CC_UID,
                         ModelNo = fld.ModelNo,
                         Qty = fld.Qty,
                         CustTypeName = fld.CustTypeName,
                         RefCustName = fld.RefCustName,
                         RefMallName = fld.RefMallName,
                         Remark = string.IsNullOrWhiteSpace(fld.Remark_Check) ? fld.Remark : fld.Remark_Check,
                         FlowStatusName = fld.FlowStatusName,
                         Flow201_TypeName = fld.Flow201_TypeName,
                         Flow201_Desc = fld.Flow201_Desc,
                         Flow301_TypeName = fld.Flow301_TypeName,
                         Flow301_Desc = fld.Flow301_Desc,
                         Flow401_TypeName = fld.Flow401_TypeName,
                         Flow401_Desc = fld.Flow401_Desc
                     });

                myDT = CustomExtension.LINQToDataTable(data1);
                if (myDT.Rows.Count > 0)
                {
                    //重新命名欄位標頭
                    myDT.Columns["CC_UID"].ColumnName = "客訴編號";
                    myDT.Columns["ModelNo"].ColumnName = "品號";
                    myDT.Columns["Qty"].ColumnName = "數量";
                    myDT.Columns["CustTypeName"].ColumnName = "客戶類別";
                    myDT.Columns["RefCustName"].ColumnName = "客戶";
                    myDT.Columns["RefMallName"].ColumnName = "商城";
                    myDT.Columns["Remark"].ColumnName = "客訴內容";
                    myDT.Columns["FlowStatusName"].ColumnName = "流程狀態";
                    myDT.Columns["Flow201_TypeName"].ColumnName = "一線處理";
                    myDT.Columns["Flow201_Desc"].ColumnName = "一線說明";
                    myDT.Columns["Flow301_TypeName"].ColumnName = "二線處理";
                    myDT.Columns["Flow301_Desc"].ColumnName = "二線說明";
                    myDT.Columns["Flow401_TypeName"].ColumnName = "業務處理";
                    myDT.Columns["Flow401_Desc"].ColumnName = "業務說明";
                }

                break;

            default:
                //
                search.Add("Flow301_Export", "Y");

                //二線
                var data2 = _data.GetCCPAllList(search, Req_Lang, Convert.ToInt32(Req_TypeID), out _dataCnt, out ErrMsg)
                     .Select(fld => new
                     {
                         CC_UID = fld.CC_UID,
                         ModelNo = fld.ModelNo,
                         Qty = fld.Qty,
                         CustTypeName = fld.CustTypeName,
                         RefCustName = fld.RefCustName,
                         RefMallName = fld.RefMallName,
                         Remark = string.IsNullOrWhiteSpace(fld.Remark_Check) ? fld.Remark : fld.Remark_Check,
                         FlowStatusName = fld.FlowStatusName,
                         Flow301_TypeName = fld.Flow301_TypeName,
                         FixPrice = fld.FixPrice,
                         Flow301_Desc = fld.Flow301_Desc,
                         BadReasonName = fld.BadReasonName
                     });

                //將IQueryable轉成DataTable
                myDT = CustomExtension.LINQToDataTable(data2);
                if (myDT.Rows.Count > 0)
                {
                    //重新命名欄位標頭
                    myDT.Columns["CC_UID"].ColumnName = "客訴編號";
                    myDT.Columns["ModelNo"].ColumnName = "品號";
                    myDT.Columns["Qty"].ColumnName = "數量";
                    myDT.Columns["CustTypeName"].ColumnName = "客戶類別";
                    myDT.Columns["RefCustName"].ColumnName = "客戶";
                    myDT.Columns["RefMallName"].ColumnName = "商城";
                    myDT.Columns["Remark"].ColumnName = "客訴內容";
                    myDT.Columns["FlowStatusName"].ColumnName = "流程狀態";
                    myDT.Columns["Flow301_TypeName"].ColumnName = "處理方式";
                    myDT.Columns["FixPrice"].ColumnName = "報價金額";
                    myDT.Columns["Flow301_Desc"].ColumnName = "處理說明";
                    myDT.Columns["BadReasonName"].ColumnName = "不良原因";
                }

                break;
        }

        //Check
        if (myDT.Rows.Count == 0)
        {
            CustomExtension.AlertMsg("查無資料", "");
            return;
        }

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "Export-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
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
    /// 取得網址參數 - TypeID
    /// </summary>
    private string _Req_TypeID;
    public string Req_TypeID
    {
        get
        {
            String DataID = Page.RouteData.Values["typeID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
        }
        set
        {
            _Req_TypeID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/CustComplaint/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_TypeID);
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
            int data = Request.QueryString["page"] == null ? 1 : Convert.ToInt32(Request.QueryString["Page"]);
            return data;
        }
        set
        {
            _Req_PageIdx = value;
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
            _Req_Keyword = value;
        }
    }
    private string _Req_Keyword;

    /// <summary>
    /// Date Type
    /// </summary>
    public string Req_DType
    {
        get
        {
            String _data = Request.QueryString["dt"];
            return string.IsNullOrWhiteSpace(_data) ? "A" : _data;
        }
        set
        {
            _Req_DType = value;
        }
    }
    private string _Req_DType;

    /// <summary>
    /// 取得傳遞參數 - sDate
    /// 預設365日內
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
            string dt = DateTime.Now.ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_eDate = value;
        }
    }
    private string _Req_eDate;


    /// <summary>
    /// CustType
    /// </summary>
    public string Req_CustType
    {
        get
        {
            String _data = Request.QueryString["ct"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_CustType = value;
        }
    }
    private string _Req_CustType;

    /// <summary>
    /// PlanType
    /// </summary>
    public string Req_PlanType
    {
        get
        {
            String _data = Request.QueryString["pt"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_PlanType = value;
        }
    }
    private string _Req_PlanType;

    /// <summary>
    /// FlowStatus
    /// </summary>
    public string Req_FlowStatus
    {
        get
        {
            String _data = Request.QueryString["fs"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_FlowStatus = value;
        }
    }
    private string _Req_FlowStatus;


    /// <summary>
    /// Trace ID
    /// </summary>
    public string Req_Trace
    {
        get
        {
            String _data = Request.QueryString["trace"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_Trace = value;
        }
    }
    private string _Req_Trace;


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
            _thisPage = value;
        }
    }
    private string _thisPage;

    #endregion


}
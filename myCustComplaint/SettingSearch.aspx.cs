using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myCustComplaint_SettingSearch : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;
    public bool editAuth = false; //編輯權限(可在權限設定裡勾選)

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
                Page.Title = typeName + " - 開案中客訴";


                //[權限判斷] Start
                #region --權限--
                /* 
                 * 判斷對應的MENU ID
                 * 取得其他權限
                 */
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, menuID);

                switch (Req_TypeID)
                {
                    case "10":
                        //台灣工具
                        editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3234");
                        //closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3235");
                        break;

                    case "20":
                        //台灣科玩
                        editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3238");
                        //closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3239");
                        break;

                    case "30":
                        //中國工具
                        editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3242");
                        //closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3243");
                        break;

                    default:
                        //中國科玩
                        editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3246");
                        //closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3247");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }


                #endregion
                //[權限判斷] End


                //[產生選單]
                Get_ClassList("2", filter_CustType, _ccType, GetLocalResourceObject("txt_所有資料").ToString());
                Get_ClassList("3", filter_FrieghtType, _ccType, GetLocalResourceObject("txt_所有資料").ToString());

                ///* 多語系設定 */
                //Page.Title = GetLocalResourceObject("pageTitle").ToString();
                //filter_Keyword.Attributes.Add("placeholder", GetLocalResourceObject("sh_追蹤編號").ToString());
                //filter_sDate.Attributes.Add("placeholder", GetLocalResourceObject("sh_開始日").ToString());
                //filter_eDate.Attributes.Add("placeholder", GetLocalResourceObject("sh_結束日").ToString());
                //filter_Dept.Attributes.Add("placeholder", GetLocalResourceObject("sh_tip2").ToString());
                //filter_Emp.Attributes.Add("placeholder", GetLocalResourceObject("sh_tip2").ToString());


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

        //固定條件
        search.Add("doSearch", "Y");

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);
            PageParam.Add("k=" + Server.UrlEncode(Req_Keyword));
            filter_Keyword.Text = Req_Keyword;
        }
        //[查詢條件] - Status
        if (!string.IsNullOrWhiteSpace(Req_Status))
        {
            search.Add("Status", Req_Status);
            PageParam.Add("st=" + Server.UrlEncode(Req_Status));
            filter_Status.SelectedValue = Req_Status;
        }
        //[查詢條件] - CustType
        if (!string.IsNullOrWhiteSpace(Req_CustType))
        {
            search.Add("CustType", Req_CustType);
            PageParam.Add("ct=" + Server.UrlEncode(Req_CustType));
            filter_CustType.SelectedValue = Req_CustType;
        }
        //[查詢條件] - FreightType
        if (!string.IsNullOrWhiteSpace(Req_FreightType))
        {
            search.Add("FreightType", Req_FreightType);
            PageParam.Add("ft=" + Server.UrlEncode(Req_FreightType));
            filter_FrieghtType.SelectedValue = Req_FreightType;
        }

        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPTempList(search, Req_Lang, Convert.ToInt32(Req_TypeID)
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
            lt_DataCnt.Text = DataCnt.ToString();  //填入資料總筆數

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
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 方法:刪除資料 -----
        if (false == _data.Delete_CCPTemp(Get_DataID))
        {
            CustomExtension.AlertMsg("刪除失敗", "");
            return;
        }

        //release
        _data = null;

        //刪除FTP檔案
        _ftp.FTP_DelFolder(UploadFolder + Get_DataID);

        //導向本頁
        Response.Redirect(thisPage);
    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取得資料:商品數
                Int32 _DTCnt = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "DTCnt"));
                Panel pl_Cnt = (Panel)e.Item.FindControl("pl_Cnt");
                pl_Cnt.Visible = _DTCnt.Equals(0);

                //取得資料:客服是否填寫
                string _IsCS = DataBinder.Eval(dataItem.DataItem, "IsCS").ToString();
                Panel pl_CS = (Panel)e.Item.FindControl("pl_CS");
                pl_CS.Visible = _IsCS.Equals("N");

                //取得資料:收貨是否填寫
                string _IsFreight = DataBinder.Eval(dataItem.DataItem, "IsFreight").ToString();
                Panel pl_Freight = (Panel)e.Item.FindControl("pl_Freight");
                pl_Freight.Visible = _IsFreight.Equals("N");

                //待確認開案
                Panel pl_Invoke = (Panel)e.Item.FindControl("pl_Invoke");
                pl_Invoke.Visible = (_IsCS.Equals("Y") && _IsFreight.Equals("Y") && _DTCnt > 0);

                //權限判斷
                PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                PlaceHolder ph_Del = (PlaceHolder)e.Item.FindControl("ph_Del");
                ph_Edit.Visible = editAuth;
                ph_Del.Visible = editAuth;

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


    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<
        //Params
        string _Keyword = filter_Keyword.Text;
        string _st = filter_Status.SelectedValue;
        string _ct = filter_CustType.SelectedValue;
        string _ft = filter_FrieghtType.SelectedValue;


        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            search.Add("Keyword", _Keyword);
        }
        //[查詢條件] - Status
        if (!string.IsNullOrWhiteSpace(_st))
        {
            search.Add("Status", _st);
        }
        //[查詢條件] - CustType
        if (!string.IsNullOrWhiteSpace(_ct))
        {
            search.Add("CustType", _ct);
        }
        //[查詢條件] - FrieghtType
        if (!string.IsNullOrWhiteSpace(_ft))
        {
            search.Add("FreightType", _ft);
        }

        #endregion

        //----- 原始資料:取得所有資料 -----
        var _rowData = _data.Get_CCPTemp_ExportData(Convert.ToInt32(Req_TypeID), search, out ErrMsg);
        var data = _rowData.AsEnumerable()
            .Select(fld => new
            {
                建立日期 = fld.Field<DateTime>("Create_Time").ToString("yyyy/MM/dd"),
                追蹤碼 = fld.Field<string>("TraceID"),
                品號 = fld.Field<string>("ModelNo"),
                數量 = fld.Field<Int32?>("Qty"),
                客訴內容 = fld.Field<string>("Remark"),
                保固內 = fld.Field<string>("IsWarranty"),
                目前狀態 = ((fld.Field<string>("IsCS").Equals("N") ? "客服單位未填寫" : "")
                 + (fld.Field<string>("IsFreight").Equals("N") ? " 收貨單位未填寫" : "")
                 + (fld.Field<int>("DTCnt") == 0 ? " 商品資料未填寫" : "")).Trim(),
                計畫處理方式 = fld.Field<string>("PlanTypeName"),
                是否有發票退回 = fld.Field<string>("InvoiceIsBack"),
                客戶類別 = fld.Field<string>("CustTypeName"),
                聯絡人 = fld.Field<string>("BuyerName"),
                聯絡電話 = fld.Field<string>("BuyerPhone"),
                聯絡位址 = fld.Field<string>("BuyerAddr"),
                商城 = fld.Field<string>("RefMallName"),
                商城單號 = fld.Field<string>("Platform_ID"),
                備註 = fld.Field<string>("CustInput"),
                客服填寫人員 = fld.Field<string>("CS_Name"),
                收貨填寫人員 = fld.Field<string>("Freight_Name"),
                建立者 = fld.Field<string>("Create_Name")
            });
      
        //Convert to DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(data);

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
        string _Keyword = filter_Keyword.Text;
        string _st = filter_Status.SelectedValue;
        string _ct = filter_CustType.SelectedValue;
        string _ft = filter_FrieghtType.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?page=1".FormatThis(thisPage));

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            url.Append("&k=" + Server.UrlEncode(_Keyword));
        }
        //[查詢條件] - Status
        if (!string.IsNullOrWhiteSpace(_st))
        {
            url.Append("&st=" + Server.UrlEncode(_st));
        }
        //[查詢條件] - CustType
        if (!string.IsNullOrWhiteSpace(_ct))
        {
            url.Append("&ct=" + Server.UrlEncode(_ct));
        }
        //[查詢條件] - FrieghtType
        if (!string.IsNullOrWhiteSpace(_ft))
        {
            url.Append("&ft=" + Server.UrlEncode(_ft));
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
    /// Status
    /// </summary>
    public string Req_Status
    {
        get
        {
            String _data = Request.QueryString["st"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_Status = value;
        }
    }
    private string _Req_Status;

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
    /// FreightType
    /// </summary>
    public string Req_FreightType
    {
        get
        {
            String _data = Request.QueryString["ft"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_FreightType = value;
        }
    }
    private string _Req_FreightType;

    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}/Set".FormatThis(FuncPath());
        }
        set
        {
            _thisPage = value;
        }
    }
    private string _thisPage;


    #endregion


    #region -- 上傳參數 --

    /// <summary>
    /// 上傳根目錄
    /// </summary>
    private string _UploadFolder;
    public string UploadFolder
    {
        get
        {
            return "{0}CustComplaint/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
        }
        set
        {
            _UploadFolder = value;
        }
    }

    #endregion


}
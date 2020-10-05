using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ARData.Controllers;
using PKLib_Method.Methods;

public partial class myARdata_Search : SecurityCheck
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
                    case "3":
                        //SH
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3782");
                        break;

                    default:
                        //TW
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3781");
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
        ArrayList PageParam = new ArrayList();  //分類暫存條件參數

        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            #region >> 條件篩選 <<

            //固定參數
            search.Add("DBS", Req_CompID);

            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(Req_Keyword))
            {
                search.Add("Keyword", Req_Keyword);
                PageParam.Add("k=" + Server.UrlEncode(Req_Keyword));
                filter_Keyword.Text = Req_Keyword;
            }

            //[取得/檢查參數] - Date
            if (!string.IsNullOrWhiteSpace(Req_sDate))
            {
                search.Add("sDate", Req_sDate);
                PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
                filter_sDate.Text = Req_sDate;
            }
            if (!string.IsNullOrWhiteSpace(Req_eDate))
            {
                search.Add("eDate", Req_eDate);
                PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
                filter_eDate.Text = Req_eDate;
            }

            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetList(search, StartRow, RecordsPerPage
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
                //CustomExtension.setCookie("ARData", "", -1);
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
                string reSetPage = "{0}&page={1}{2}".FormatThis(
                    thisPage
                    , pageIndex
                    , (PageParam.Count == 0 ? "" : "&") + string.Join("&", PageParam.ToArray()));

                //暫存頁面Url, 給其他頁使用
                //CustomExtension.setCookie("ARData", Server.UrlEncode(reSetPage), 1);

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
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取得資料:狀態
                Int32 _flow = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "Status"));

                PlaceHolder ph_KeepGo = (PlaceHolder)e.Item.FindControl("ph_KeepGo");
                ph_KeepGo.Visible = !_flow.Equals(20);


                //取得資料:單身數
                Int32 _itemCnt = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "ItemCnt"));

                PlaceHolder ph_PDF = (PlaceHolder)e.Item.FindControl("ph_PDF");
                ph_PDF.Visible = !_itemCnt.Equals(0);

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

    #endregion


    #region -- 附加功能 --

    /// <summary>
    /// 含查詢條件的完整網址
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _sDate = filter_sDate.Text;
        string _eDate = filter_eDate.Text;
        string _Keyword = filter_Keyword.Text;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?page=1".FormatThis(thisPage));

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
    /// 取得網址參數 - Company ID(TW/SH/SZ)
    /// </summary>
    private string _Req_CompID;
    public string Req_CompID
    {
        get
        {
            String DataID = Page.RouteData.Values["CompID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
        }
        set
        {
            _Req_CompID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/ARInform/{3}".FormatThis(
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
            _Req_PageIdx = value;
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
    /// 取得傳遞參數 - sDate
    /// 預設30日內
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.AddDays(-30).ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            _Req_sDate = value;
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
            _Req_eDate = value;
        }
    }
    private string _Req_eDate;


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
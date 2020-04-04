using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

public partial class mySupInvCheck_SettingSearch : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
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
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<

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
        var query = _data.GetSupInvSendList(search, StartRow, RecordsPerPage
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
            CustomExtension.setCookie("SupInvCheckA", "", -1);
        }
        else
        {
            ph_EmptyData.Visible = false;
            ph_Data.Visible = true;

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
            CustomExtension.setCookie("SupInvCheckA", Server.UrlEncode(reSetPage), 1);

        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();

        try
        {
            //----- 方法:刪除資料 -----
            if (false == _data.Delete_SupInvSend(Get_DataID))
            {
                CustomExtension.AlertMsg("刪除失敗", "");
                return;
            }


            //導向本頁
            Response.Redirect(thisPage);
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //release
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

                //判斷是否已加入排程,顯示編輯鈕
                string _onTask = DataBinder.Eval(dataItem.DataItem, "IsOnTask").ToString();
                PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                ph_Edit.Visible = _onTask.Equals("N");


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
    /// 含查詢條件的完整網址(新查詢)
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

        //固定條件:Page
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

    public string Get_StatusName(string val)
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
            this._thisPage = value;
        }
    }
    private string _thisPage;

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
    /// 取得傳遞參數 - sDate
    /// 預設30 days內
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.AddDays(-30).ToShortDateString().ToDateString("yyyy/MM/dd");
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

    #endregion


}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using CustomController;
using Menu3000Data.Controllers;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class mySalesOrderStock_Search : SecurityCheck
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
                    //case "3":
                    //    //上海寶工
                    //    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4152");
                    //    break;


                    case "2":
                        //深圳寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3721");
                        break;

                        //default:
                        //    //TW
                        //    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4151");
                        //    break;
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


                #region --Request參數--
                //[取得/檢查參數] - SoNo
                if (!string.IsNullOrEmpty(Req_SoNo))
                {
                    this.filter_SoNo.Text = Req_SoNo;
                }

                //[取得/檢查參數] - Where
                if (!string.IsNullOrEmpty(Req_Where))
                {
                    this.filter_Where.SelectedIndex = this.filter_Where.Items.IndexOf(this.filter_Where.Items.FindByValue(Req_Where));
                }

                //[取得/檢查參數] - OrderNo
                if (!string.IsNullOrEmpty(Req_OrderNo))
                {
                    this.filter_OrderNo.Text = Req_OrderNo;
                }

                //[取得/檢查參數] - Cust
                if (!string.IsNullOrEmpty(Req_Cust))
                {
                    this.filter_Cust.Text = Req_Cust;
                    this.val_Cust.Text = Req_Cust;
                }
                //[取得/檢查參數] - ModelNo
                if (!string.IsNullOrEmpty(Req_ModelNo))
                {
                    this.filter_ModelNo.Text = Req_ModelNo;
                    this.val_ModelNo.Text = Req_ModelNo;
                }
                #endregion


                //Get Data
                LookupDataList(Req_PageIdx, out ErrMsg);

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
    /// ps:此計算頁數方式需修改 .19/11/18
    /// </summary>
    /// <param name="pageIndex"></param>
    private void LookupDataList(int pageIndex, out string ErrMsg)
    {
        //----- 宣告:分頁參數 -----
        int RecordsPerPage = 20;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<
        //[取得/檢查參數] - SoNo
        if (!string.IsNullOrEmpty(Req_SoNo))
        {
            search.Add("SoNo", Req_SoNo);

            PageParam.Add("SoNo=" + Server.UrlEncode(Req_SoNo));
        }

        //[取得/檢查參數] - Where
        if (!string.IsNullOrEmpty(Req_Where))
        {
            if (!Req_Where.Equals("ALL"))
            {
                search.Add("ShopWhere", Req_Where);
            }

            PageParam.Add("Where=" + Server.UrlEncode(Req_Where));
        }

        //[取得/檢查參數] - OrderNo
        if (!string.IsNullOrEmpty(Req_OrderNo))
        {
            search.Add("ShopOrderID", Req_OrderNo);

            PageParam.Add("OrderNo=" + Server.UrlEncode(Req_OrderNo));
        }

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrEmpty(Req_Cust))
        {
            search.Add("CustID", Req_Cust);

            PageParam.Add("Cust=" + Server.UrlEncode(Req_Cust));
        }
        //[取得/檢查參數] - ModelNo
        if (!string.IsNullOrEmpty(Req_ModelNo))
        {
            search.Add("ModelNo", Req_ModelNo);

            PageParam.Add("ModelNo=" + Server.UrlEncode(Req_ModelNo));
        }

        #endregion


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOrderStockStat(Req_CompID, search, out ErrMsg);


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
        }
        else
        {
            this.ph_EmptyData.Visible = false;
            this.ph_Data.Visible = true;

            //分頁
            string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                , thisPage, PageParam, false, true);

            Literal lt_Pager = (Literal)this.lvDataList.FindControl("lt_Pager");
            lt_Pager.Text = getPager;
        }
    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                ////取得資料
                //string Get_Ship = DataBinder.Eval(dataItem.DataItem, "Ship_Time").ToString();


                ////取得控制項
                //PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                //PlaceHolder ph_Send = (PlaceHolder)e.Item.FindControl("ph_Send");
                //PlaceHolder ph_Close = (PlaceHolder)e.Item.FindControl("ph_Close");


                ////判斷是否已出貨
                //if (string.IsNullOrEmpty(Get_Ship))
                //{
                //    ph_Edit.Visible = true;
                //    ph_Send.Visible = true;
                //    ph_Close.Visible = true;
                //}
                //else
                //{
                //    ph_Edit.Visible = false;
                //    ph_Send.Visible = false;
                //    ph_Close.Visible = false;
                //}

            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    /// <summary>
    /// 數字處理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected string formatNumberString(string value, string type)
    {
        switch (type)
        {
            case "td":
                return (Convert.ToInt32(value) < 0) ? "negative" : "positive";

            default:
                return (Convert.ToInt32(value) < 0) ? "<strong>{0}</strong>".FormatThis(value) : value;
        }

    }

    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// 查詢按鈕
    /// </summary>
    protected void btn_Search_Click(object sender, EventArgs e)
    {
        doSearch();
    }


    /// <summary>
    /// 執行查詢
    /// </summary>
    private void doSearch()
    {
        //Params
        string _SoNo = this.filter_SoNo.Text;
        string _Where = this.filter_Where.SelectedValue;
        string _OrderNo = this.filter_OrderNo.Text;
        string _Cust = this.filter_Cust.Text;
        string _ModelNo = this.filter_ModelNo.Text;

        //url string
        StringBuilder url = new StringBuilder();

        url.Append("{0}?Page=1".FormatThis(thisPage));

        //[查詢條件] - _SoNo
        if (!string.IsNullOrEmpty(_SoNo))
        {
            url.Append("&SoNo=" + Server.UrlEncode(_SoNo));
        }
        //[查詢條件] - _Where
        if (!string.IsNullOrEmpty(_Where))
        {
            url.Append("&Where=" + Server.UrlEncode(_Where));
        }
        //[查詢條件] - _OrderNo
        if (!string.IsNullOrEmpty(_OrderNo))
        {
            url.Append("&OrderNo=" + Server.UrlEncode(_OrderNo));
        }
        //[查詢條件] - _Cust
        if (!string.IsNullOrEmpty(_Cust))
        {
            url.Append("&Cust=" + Server.UrlEncode(_Cust));

        }
        //[查詢條件] - _ModelNo
        if (!string.IsNullOrEmpty(_ModelNo))
        {
            url.Append("&ModelNo=" + Server.UrlEncode(_ModelNo));
        }


        //執行轉頁
        Response.Redirect(url.ToString(), false);
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
            if (Page.RouteData.Values["CompID"] == null)
            {
                return "";
            }

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
        return "{0}{1}/{2}/SalesOrderStockStat/{3}/".FormatThis(
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


    /// <summary>
    /// 取得傳遞參數 - SoNo
    /// </summary>
    public string Req_SoNo
    {
        get
        {
            String _data = Request.QueryString["SoNo"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_SoNo = value;
        }
    }
    private string _Req_SoNo;


    /// <summary>
    /// 取得傳遞參數 - Where
    /// </summary>
    public string Req_Where
    {
        get
        {
            String _data = Request.QueryString["Where"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "官網線上下單";
        }
        set
        {
            this._Req_Where = value;
        }
    }
    private string _Req_Where;


    /// <summary>
    /// 取得傳遞參數 - OrderNo
    /// </summary>
    public string Req_OrderNo
    {
        get
        {
            String _data = Request.QueryString["OrderNo"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_OrderNo = value;
        }
    }
    private string _Req_OrderNo;


    /// <summary>
    /// 取得傳遞參數 - Cust
    /// </summary>
    public string Req_Cust
    {
        get
        {
            String _data = Request.QueryString["Cust"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Cust = value;
        }
    }
    private string _Req_Cust;


    /// <summary>
    /// 取得傳遞參數 - ModelNo
    /// </summary>
    public string Req_ModelNo
    {
        get
        {
            String _data = Request.QueryString["ModelNo"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_ModelNo = value;
        }
    }
    private string _Req_ModelNo;


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

    #endregion

}
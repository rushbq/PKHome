using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myOrderingStock_Search : SecurityCheck
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
                        //上海寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3762");
                        break;

                    case "2":
                        //深圳寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3763");
                        break;

                    default:
                        //TW
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3761");
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
                LookupDataList(out ErrMsg);

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
    private void LookupDataList(out string ErrMsg)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<
        //[取得/檢查參數] - SoNo
        if (!string.IsNullOrEmpty(Req_BoNo))
        {
            search.Add("BoNo", Req_BoNo);

            //PageParam.Add("SoNo=" + Server.UrlEncode(Req_BoNo));
            this.filter_BoNo.Text = Req_BoNo;
        }

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrEmpty(Req_Cust))
        {
            search.Add("CustID", Req_Cust);

            //PageParam.Add("Cust=" + Server.UrlEncode(Req_Cust));
            this.filter_Cust.Text = Req_Cust;
            this.val_Cust.Text = Req_Cust;
        }

        //[取得/檢查參數] - Date
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate);

            //PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
            filter_sDate.Text = Req_sDate;
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate);

            //PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
            filter_eDate.Text = Req_eDate;
        }

        #endregion


        //----- 原始資料:取得所有資料 -----
        DataTable query = _data.GetOrderingStockStat(Req_CompID, search, out ErrMsg);


        //----- 資料整理:繫結 ----- 
        lvDataList.DataSource = query;
        lvDataList.DataBind();


        //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
        if (query != null && query.Rows.Count == 0)
        {
            ph_EmptyData.Visible = true;
            ph_Data.Visible = false;
        }
        else
        {
            ph_EmptyData.Visible = false;
            ph_Data.Visible = true;


            #region -- 欄位依公司別判斷 --

            bool showTW = Req_CompID.Equals("TW");
            bool showSH = Req_CompID.Equals("SH");
            bool showSZ = Req_CompID.Equals("SZ");

            //Header visible check
            ph_headerTW.Visible = showTW;
            ph_headerSH.Visible = showSH;
            ph_headerSZ.Visible = showSZ;
            ph_subHeaderTW.Visible = showTW;
            ph_subHeaderSH.Visible = showSH;
            ph_subHeaderSZ.Visible = showSZ;

            //Body visible check
            ListView lst = lvDataList;
            for (int row = 0; row < lst.Items.Count; row++)
            {
                ((PlaceHolder)lst.Items[row].FindControl("ph_BodyTW")).Visible = showTW;
                ((PlaceHolder)lst.Items[row].FindControl("ph_TotalTW")).Visible = showTW;
                ((PlaceHolder)lst.Items[row].FindControl("ph_BodySH")).Visible = showSH;
                ((PlaceHolder)lst.Items[row].FindControl("ph_TotalSH")).Visible = showSH;
                ((PlaceHolder)lst.Items[row].FindControl("ph_BodySZ")).Visible = showSZ;
                ((PlaceHolder)lst.Items[row].FindControl("ph_TotalSZ")).Visible = showSZ;
            }
            #endregion
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
        string _BoNo = filter_BoNo.Text;
        string _Cust = filter_Cust.Text;
        string _sDate = filter_sDate.Text;
        string _eDate = filter_eDate.Text;

        //url string
        StringBuilder url = new StringBuilder();

        url.Append("{0}?Page=1".FormatThis(thisPage));

        //[查詢條件] - BoNo
        if (!string.IsNullOrEmpty(_BoNo))
        {
            url.Append("&BoNo=" + Server.UrlEncode(_BoNo));
        }

        //[查詢條件] - _Cust
        if (!string.IsNullOrEmpty(_Cust))
        {
            url.Append("&Cust=" + Server.UrlEncode(_Cust));

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
        return "{0}{1}/{2}/OrderStockStat/{3}/".FormatThis(
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
    /// 取得傳遞參數 - BoNo
    /// </summary>
    public string Req_BoNo
    {
        get
        {
            String _data = Request.QueryString["BoNo"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_BoNo = value;
        }
    }
    private string _Req_BoNo;


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
    /// 取得傳遞參數 - sDate
    /// 預設2 days內
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.AddDays(-1).ToShortDateString().ToDateString("yyyy/MM/dd");
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
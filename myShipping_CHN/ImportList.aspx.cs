using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ShipFreight_CN.Controllers;
using PKLib_Method.Methods;

public partial class myShipping_ImportList : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;

                //A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具
                switch (Req_DataType)
                {
                    case "A":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3775");
                        break;

                    case "B":
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3776");
                        break;

                    case "C":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3777");
                        break;

                    case "D":
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3778");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //取得公司別
                string _corpName = "中國內銷({0})".FormatThis(fn_Menu.GetShipping_RefType(Req_DataType));
                lt_CorpName.Text = _corpName;
                Page.Title += "-" + _corpName;

                //[權限判斷] End
                #endregion


                #region --Request參數--
                //[取得/檢查參數] - Req_sDate
                if (!string.IsNullOrWhiteSpace(Req_sDate))
                {
                    filter_sDate.Text = Req_sDate;
                }
                //[取得/檢查參數] - Req_eDate
                if (!string.IsNullOrWhiteSpace(Req_eDate))
                {
                    filter_eDate.Text = Req_eDate;
                }

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
    /// <param name="pageIndex"></param>
    private void LookupDataList(int pageIndex)
    {
        //----- 宣告:分頁參數 -----
        int RecordsPerPage = 20;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        ArrayList PageParam = new ArrayList();  //條件參數,for pager
        //固定參數
        PageParam.Add("dt=" + Server.UrlEncode(Req_DataType));

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<

        //[取得/檢查參數] - Date
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate.ToDateString("yyyy/MM/dd 00:00"));

            PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate.ToDateString("yyyy/MM/dd 23:59"));

            PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
        }

        #endregion

        //----- 原始資料:取得所有資料 -----
        string defComp = Req_DataType.Equals("1") ? "CHN1" : "CHN2";
        var query = _data.GetShipImportList(search, defComp, out ErrMsg);


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
        lvDataList.DataSource = data;
        lvDataList.DataBind();


        //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
        if (query.Count() == 0)
        {
            ph_EmptyData.Visible = true;
            ph_Data.Visible = false;
        }
        else
        {
            ph_EmptyData.Visible = false;
            ph_Data.Visible = true;

            //分頁
            string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                , FuncPath(), PageParam, false, true);

            Literal lt_Pager = (Literal)lvDataList.FindControl("lt_Pager");
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

                //取得資料:狀態
                Int32 _flow = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "Status"));

                PlaceHolder ph_KeepGo = (PlaceHolder)e.Item.FindControl("ph_KeepGo");
                ph_KeepGo.Visible = !_flow.Equals(30);


            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    /// <summary>
    /// 取得繼續匯入網址
    /// </summary>
    /// <param name="status"></param>
    /// <param name="dataID"></param>
    /// <returns></returns>
    public string keepGoUrl(int status, string dataID)
    {
        switch (status)
        {
            case 10:
                return "{0}/Step2/{1}?dt={2}".FormatThis(FuncPath(), dataID, Req_DataType);

            case 20:
                return "{0}/Step3/{1}?dt={2}".FormatThis(FuncPath(), dataID, Req_DataType);

            default:
                return "#";
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

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?page=1&dt={1}".FormatThis(FuncPath(), Req_DataType));

        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/ShipImportCHN".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
    }


    public string FuncPath_Ship()
    {
        return "{0}{1}/{2}/ShipFreight_CHN?dt={3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_DataType);
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
    /// 資料判別:A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具
    /// </summary>
    public string Req_DataType
    {
        get
        {
            string data = Request.QueryString["dt"] == null ? "A" : Request.QueryString["dt"].ToString();
            return data;
        }
        set
        {
            this._Req_DataType = value;
        }
    }
    private string _Req_DataType;


    /// <summary>
    /// 取得傳遞參數 - sDate
    /// 預設7日內
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.AddDays(-7).ToString().ToDateString("yyyy/MM/dd");
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
            return "{0}?dt={1}".FormatThis(FuncPath(), Req_DataType);
        }
        set
        {
            _thisPage = value;
        }
    }
    private string _thisPage;


    /// <summary>
    /// 設定參數 - 發貨明細url
    /// </summary>
    private string _Page_SearchUrl;
    public string Page_SearchUrl
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("HomeList_SPCHN");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath_Ship() : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion


}
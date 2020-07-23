using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;
using SZ_ecData.Controllers;

public partial class myECdata_SZ_SearchByMonth : SecurityCheck
{
    public string ErrMsg;

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
                string typeName = fn_Menu.GetECData_RefType(Convert.ToInt32(Req_TypeID)) + " (每月)";

                //取得功能名稱
                lt_TypeName.Text = typeName;
                //設定PageTitle
                Page.Title = typeName;


                //[權限判斷] Start
                #region --權限--
                /* 
                 * 判斷對應的MENU ID
                 * 取得其他權限
                 */
                bool isPass;

                switch (Req_TypeID)
                {
                    case "1":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3743");
                        break;

                    default:
                        //科玩
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3748");
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
                CreateMenu_Year(filter_Year);
                CreateMenu_Month(filter_Month);
                Get_ClassList(Req_TypeID, filter_Mall, "所有資料");

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
        SZ_ecDataRepository _data = new SZ_ecDataRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<

        //[查詢條件] - Year
        if (!string.IsNullOrWhiteSpace(Req_Year))
        {
            search.Add("Year", Req_Year);
            PageParam.Add("yy=" + Server.UrlEncode(Req_Year));
            filter_Year.Text = Req_Year;
        }
        //[查詢條件] - Month
        if (!string.IsNullOrWhiteSpace(Req_Month))
        {
            search.Add("Month", Req_Month);
            PageParam.Add("mm=" + Server.UrlEncode(Req_Month));
            filter_Month.Text = Req_Month;
        }
        //[查詢條件] - Mall
        if (!string.IsNullOrWhiteSpace(Req_Mall))
        {
            search.Add("Mall", Req_Mall);
            PageParam.Add("mall=" + Server.UrlEncode(Req_Mall));
            filter_Mall.SelectedValue = Req_Mall;
        }
        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetECDList_byMonth(search, Req_Lang, Convert.ToInt32(Req_TypeID)
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
            CustomExtension.setCookie("HomeList_ECMonth", "", -1);
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
            CustomExtension.setCookie("HomeList_ECMonth", Server.UrlEncode(reSetPage), 1);

        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        ////取得Key值
        //string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        ////----- 宣告:資料參數 -----
        //SZ_ecDataRepository _data = new SZ_ecDataRepository();

        ////----- 方法:刪除資料 -----
        //if (false == _data.Delete_CCPTemp(Get_DataID))
        //{
        //    CustomExtension.AlertMsg("刪除失敗", "");
        //    return;
        //}

        ////release
        //_data = null;


        ////導向本頁
        //Response.Redirect(thisPage);
    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                //ListViewDataItem dataItem = (ListViewDataItem)e.Item;


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
    /// 取得類別資料 
    /// </summary>
    /// <param name="typeID">typeID</param>
    /// <param name="ddl">下拉選單object</param>
    /// <param name="rootName">第一選項顯示名稱</param>
    private void Get_ClassList(string typeID, DropDownList ddl, string rootName)
    {
        //----- 宣告:資料參數 -----
        SZ_ecDataRepository _data = new SZ_ecDataRepository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetEC_RefMall(typeID, Req_Lang, out ErrMsg);


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

    protected void CreateMenu_Year(DropDownList item)
    {
        int currYear = DateTime.Now.Year;
        int prevYear = currYear - 5;
        int nextYear = currYear;

        item.Items.Clear();
        for (int itemY = prevYear; itemY <= nextYear; itemY++)
        {
            item.Items.Add(new ListItem(itemY.ToString(), itemY.ToString()));
        }

        //預設值
        item.SelectedValue = currYear.ToString();
    }

    protected void CreateMenu_Month(DropDownList item)
    {
        item.Items.Clear();
        item.Items.Add(new ListItem("所有資料", ""));
        for (int row = 1; row <= 12; row++)
        {
            item.Items.Add(new ListItem(row.ToString(), row.ToString()));
        }
    }

    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _yy = filter_Year.SelectedValue;
        string _mm = filter_Month.SelectedValue;
        string _mall = filter_Mall.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?page=1".FormatThis(thisPage));

        //[查詢條件] - Year
        if (!string.IsNullOrWhiteSpace(_yy))
        {
            url.Append("&yy=" + Server.UrlEncode(_yy));
        }
        //[查詢條件] - Month
        if (!string.IsNullOrWhiteSpace(_mm))
        {
            url.Append("&mm=" + Server.UrlEncode(_mm));
        }
        //[查詢條件] - Mall
        if (!string.IsNullOrWhiteSpace(_mall))
        {
            url.Append("&mall=" + Server.UrlEncode(_mall));
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
        return "{0}{1}/{2}/eCommerceData/{3}".FormatThis(
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


    /// <summary>
    /// Year
    /// </summary>
    public string Req_Year
    {
        get
        {
            String _data = Request.QueryString["yy"];
            string _defValue = DateTime.Now.Year.ToString(); //預設值
            return string.IsNullOrWhiteSpace(_data) ? _defValue : _data;
        }
        set
        {
            _Req_Year = value;
        }
    }
    private string _Req_Year;

    /// <summary>
    /// Month
    /// </summary>
    public string Req_Month
    {
        get
        {
            String _data = Request.QueryString["mm"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_Month = value;
        }
    }
    private string _Req_Month;

    /// <summary>
    /// Mall
    /// </summary>
    public string Req_Mall
    {
        get
        {
            String _data = Request.QueryString["mall"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_Mall = value;
        }
    }
    private string _Req_Mall;

    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}/Month".FormatThis(FuncPath());
        }
        set
        {
            _thisPage = value;
        }
    }
    private string _thisPage;

    #endregion



}
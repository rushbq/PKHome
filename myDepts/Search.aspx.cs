using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Data.Assets;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class myDepts_Search : SecurityCheck
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

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "9111"); ;

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[權限判斷] End
                #endregion


                #region --Request參數--
                //[取得/檢查參數] - Area
                if (!string.IsNullOrEmpty(Req_Area))
                {
                    filter_Area.SelectedIndex = filter_Area.Items.IndexOf(filter_Area.Items.FindByValue(Req_Area));
                }

                //[取得/檢查參數] - Dept
                if (!string.IsNullOrEmpty(Req_Dept))
                {
                    filter_Dept.Text = Req_Dept;
                    val_Dept.Text = Req_Dept;
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

        //----- 宣告:資料參數 -----
        DeptsRepository _data = new DeptsRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();


        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<
        //[取得/檢查參數] - Area
        if (!string.IsNullOrEmpty(Req_Area))
        {
            if (!Req_Area.Equals("ALL"))
            {
                search.Add((int)Common.DeptSearch.Area, Req_Area);
            }

            PageParam.Add("Area=" + Server.UrlEncode(Req_Area));
        }

        //[取得/檢查參數] - Dept
        if (!string.IsNullOrEmpty(Req_Dept))
        {
            search.Add((int)Common.DeptSearch.DataID, Req_Dept);

            PageParam.Add("Dept=" + Server.UrlEncode(Req_Dept));
        }

        #endregion


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetDepts(search)
            .OrderByDescending(o => o.Display);


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
                , thisPage, PageParam, false, true);

            Literal lt_Pager = (Literal)lvDataList.FindControl("lt_Pager");
            lt_Pager.Text = getPager;
        }
    }

    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        DeptsRepository _data = new DeptsRepository();

        try
        {
            //----- 方法:刪除資料 -----
            if (false == _data.Delete(Convert.ToInt32(Get_DataID), out ErrMsg))
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
        string _Area = filter_Area.SelectedValue;
        string _Dept = val_Dept.Text;

        //url string
        StringBuilder url = new StringBuilder();

        url.Append("{0}?Page=1".FormatThis(thisPage));

        //[查詢條件] - _Area
        if (!string.IsNullOrEmpty(_Area))
        {
            url.Append("&Area=" + Server.UrlEncode(_Area));
        }
        //[查詢條件] - _Dept
        if (!string.IsNullOrEmpty(_Dept))
        {
            url.Append("&Dept=" + Server.UrlEncode(_Dept));
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
            _Req_Lang = value;
        }
    }
    private string _Req_Lang;

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/Depts/".FormatThis(
            fn_Param.WebUrl
            , Req_Lang);
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
    /// 取得傳遞參數 - Area
    /// </summary>
    public string Req_Area
    {
        get
        {
            String _data = Request.QueryString["Area"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            _Req_Area = value;
        }
    }
    private string _Req_Area;


    /// <summary>
    /// 取得傳遞參數 - Dept
    /// </summary>
    public string Req_Dept
    {
        get
        {
            String _data = Request.QueryString["Dept"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            _Req_Dept = value;
        }
    }
    private string _Req_Dept;


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
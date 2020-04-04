using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu2000Data.Controllers;
using PKLib_Method.Methods;

public partial class myPostal_Print : SecurityCheck
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

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2405");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End

                //[資料顯示] 填入預設值
                filter_Date.Text = DateTime.Now.ToShortDateString();

                //[資料顯示] 取得郵式清單
                GetClassMenu();

                //[資料顯示] 日期(顯示星期幾)
                string weekDay = System.Globalization.DateTimeFormatInfo.GetInstance(new System.Globalization.CultureInfo("zh-TW")).DayNames[(byte)Convert.ToDateTime(Req_sDate).DayOfWeek];
                lt_headerDate.Text = "{0} ({1})".FormatThis(Req_sDate.ToDateString("yyyy/MM/dd"), weekDay.Right(1));

                //[資料顯示] 資料列表
                if (Req_doSearch.ToUpper().Equals("Y"))
                {
                    LookupDataList();

                    ph_NoData.Visible = false;
                    ph_Data.Visible = true;
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    #region -- 資料顯示 --
    /// <summary>
    /// 取得郵式清單
    /// </summary>
    private void GetClassMenu()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        try
        {
            //取得資料
            var data = _data.GetPostalClass(out ErrMsg);

            //設定繫結-SearchForm
            filter_PostType.DataSource = data;
            filter_PostType.DataTextField = "Label";
            filter_PostType.DataValueField = "ID";
            filter_PostType.DataBind();

            //新增root item
            filter_PostType.Items.Insert(0, new ListItem("所有郵式", ""));
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



    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupDataList()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            #region >> 條件篩選 <<

            //[取得/檢查參數] - Date
            if (!string.IsNullOrWhiteSpace(Req_sDate))
            {
                search.Add("sDate", Req_sDate);
                search.Add("eDate", Req_sDate);
                filter_Date.Text = Req_sDate;
            }
            //[取得/檢查參數] - PostType
            if (!string.IsNullOrWhiteSpace(Req_Type))
            {
                search.Add("PostType", Req_Type);
                filter_PostType.SelectedValue = Req_Type;
                lt_TypeName.Text = filter_PostType.SelectedItem.Text;
            }

            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetPostalData(search, out ErrMsg);

            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
            lvDataList.DataBind();

            //Sum
            if (query.Count() > 0)
            {
                double total = query.Select(fld => fld.PostPrice).Sum();
                Literal lt_Total = (Literal)(lvDataList.FindControl("lt_Total"));
                lt_Total.Text = total.ToString();
            }


            query = null;
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //Release
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

                Literal lt_Idx = (Literal)dataItem.FindControl("lt_Idx");
                int idx = dataItem.DataItemIndex + 1;
                lt_Idx.Text = idx.ToString();


            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion


    #region -- 附加功能 --

    protected void lbtn_Search_Click(object sender, EventArgs e)
    {
        Response.Redirect(filterUrl());
    }

    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _sDate = filter_Date.Text;
        string _Type = filter_PostType.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件
        url.Append("{0}?list=Y".FormatThis(thisPage));

        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }

        //[查詢條件] - Type
        if (!string.IsNullOrWhiteSpace(_Type))
        {
            url.Append("&type=" + Server.UrlEncode(_Type));
        }

        return url.ToString();
    }


    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}myPostal/Print.aspx".FormatThis(fn_Param.WebUrl);
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
    /// 取得傳遞參數 - sDate
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.ToShortDateString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_sDate = value;
        }
    }
    private string _Req_sDate;


    /// <summary>
    /// 取得傳遞參數 - Type
    /// </summary>
    public string Req_Type
    {
        get
        {
            String _data = Request.QueryString["type"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "5", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Type = value;
        }
    }
    private string _Req_Type;


    /// <summary>
    /// 取得傳遞參數 - Search
    /// </summary>
    public string Req_doSearch
    {
        get
        {
            String _data = Request.QueryString["list"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_doSearch = value;
        }
    }
    private string _Req_doSearch;
    #endregion


}
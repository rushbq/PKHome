using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

public partial class myPurProdCost_Search_Prod : SecurityCheck
{
    public string ErrMsg;
    public DataTable DTItems;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //[權限判斷] Start
                #region --權限--

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4885");

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
        ArrayList PageParam = new ArrayList();  //分類暫存條件參數

        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            #region >> 條件篩選 <<


            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(Req_Keyword))
            {
                search.Add("Keyword", Req_Keyword);
                PageParam.Add("k=" + Server.UrlEncode(Req_Keyword));
                filter_Keyword.Text = Req_Keyword;
            }

            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetCost_ProdList(search, StartRow, RecordsPerPage, true
                , out DataCnt, out ErrMsg);

            //----- 資料整理:取得總筆數 -----
            TotalRow = DataCnt;

            //----- 資料整理:頁數判斷 -----
            if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
            {
                StartRow = 0;
                pageIndex = 1;
            }

            /*
                將包材品號丟到暫存, 依目前頁選的品號篩選
                要在 DataBind 之前塞入
            */
            ArrayList _modelAry = new ArrayList();
            for (int row = 0; row < query.Rows.Count; row++)
            {
                _modelAry.Add(query.Rows[row]["ModelNo"]);

            }
            string _models = string.Join(",", _modelAry.OfType<string>());

            //篩選出的包材品號
            DTItems = _data.GetCostRel_Pack(_models, out ErrMsg);


            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
            lvDataList.DataBind();


            //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
            if (query.Rows.Count == 0)
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
                //string reSetPage = "{0}&page={1}{2}".FormatThis(
                //    thisPage
                //    , pageIndex
                //    , (PageParam.Count == 0 ? "" : "&") + string.Join("&", PageParam.ToArray()));

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

                //取得資料:品號
                string Get_CheckID = DataBinder.Eval(dataItem.DataItem, "ModelNo").ToString();

                //取得控制項
                Literal lt_Items = (Literal)e.Item.FindControl("lt_Items");

                /*
                 * 顯示暫存檔單身資料, 依品號篩選
                 */
                if (DTItems != null)
                {
                    StringBuilder html = new StringBuilder();
                    var _items = DTItems.AsEnumerable()
                        .Where(c => c.Field<string>("ModelNo").Equals(Get_CheckID));

                    foreach (DataRow item in _items)
                    {
                        html.Append("<tr>");
                        html.Append("<td class=\"center aligned\" style=\"width:20%\">{0}</td>".FormatThis(
                            item.Field<string>("DBS")));
                        html.Append("<td class=\"green-text text-darken-3\"><h5>{0}</h5></td>".FormatThis(
                            item.Field<string>("PackItemNo")));
                        html.Append("<td class=\"center aligned\" style=\"width:20%\"><h5>{0}</h5></td>".FormatThis(
                           Math.Round(item.Field<decimal>("PackQty"), 2)));

                        //Del Button
                        html.Append("<td class=\"collapsing\">");
                        html.Append("    <button type=\"button\" class=\"ui small vertical animated orange basic button doDel\" data-id=\"{0}\">".FormatThis(
                            item.Field<Int32>("DataID")
                            ));
                        html.Append("        <div class=\"hidden content\">刪除</div>");
                        html.Append("        <div class=\"visible content\"><i class=\"x icon\"></i></div>");
                        html.Append("    </button>");
                        html.Append("</td>");
                        html.Append("</tr>");

                    }

                    //output html
                    lt_Items.Text = html.ToString();
                }


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
        string _Keyword = filter_Keyword.Text.Trim();

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page
        url.Append("{0}?page=1".FormatThis(thisPage));

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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}myPurProdCost/Search_Prod.aspx".FormatThis(fn_Param.WebUrl);
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
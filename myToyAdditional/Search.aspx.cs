using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;
using ToyAdditionalData.Controllers;

public partial class myToyAdditional_Search : SecurityCheck
{
    public string ErrMsg;
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //權限判斷
                if (false == fn_CheckAuth.Check(fn_Param.CurrentUser, "3711"))
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[取得/檢查參數] - Keyword
                if (!string.IsNullOrEmpty(Req_Keyword))
                {
                    this.filter_Keyword.Text = Req_Keyword;
                }

                ////[取得/檢查參數] - Status
                //if (!string.IsNullOrEmpty(Req_Status))
                //{
                //    this.filter_Status.SelectedIndex = this.filter_Status.Items.IndexOf(this.filter_Status.Items.FindByValue(Req_Status));
                //}


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
        ArrayList PageParam = new ArrayList();  //條件參數

        //----- 宣告:資料參數 -----
        ToyAdditionalRepository _data = new ToyAdditionalRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();

        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<       
        //[取得參數] - Keyword
        if (!string.IsNullOrEmpty(Req_Keyword))
        {
            search.Add((int)mySearch.Keyword, Req_Keyword);

            PageParam.Add("keyword=" + Server.UrlEncode(Req_Keyword));
        }

        //[取得參數] - Status
        //if (!string.IsNullOrEmpty(Req_Status))
        //{
        //    search.Add((int)mySearch.Status, Req_Status);

        //    PageParam.Add("Status=" + Server.UrlEncode(Req_Status));
        //}

        #endregion


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetDataList(search);


        //----- 資料整理:取得總筆數 -----
        TotalRow = query.Count();

        //----- 資料整理:頁數判斷 -----
        if (pageIndex > TotalRow && TotalRow > 0)
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
            //Clear
            CustomExtension.setCookie("HomeList_ToyAdditional", "", -1);
        }
        else
        {
            string getPager = CustomExtension.PageControl(TotalRow, RecordsPerPage, pageIndex, 5, thisPage, PageParam, false
                , true, CustomExtension.myStyle.Goole);

            Literal lt_Pager = (Literal)this.lvDataList.FindControl("lt_Pager");
            lt_Pager.Text = getPager;

            //Literal lt_TopPager = (Literal)this.lvDataList.FindControl("lt_TopPager");
            //lt_TopPager.Text = getPager;

            //重新整理頁面Url
            string reSetPage = "{0}?Page={1}{2}".FormatThis(
                thisPage
                , pageIndex
                , "&" + string.Join("&", PageParam.ToArray()));


            //暫存頁面Url, 給其他頁使用
            CustomExtension.setCookie("HomeList_ToyAdditional", Server.UrlEncode(reSetPage), 1);
        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string dataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

            switch (e.CommandName.ToUpper())
            {
                case "DOCLOSE":
                    //----- 宣告:資料參數 -----
                    ToyAdditionalRepository _data = new ToyAdditionalRepository();

                    //----- 方法:刪除資料 -----
                    if (false == _data.Delete(dataID, out ErrMsg))
                    {
                        _data = null;

                        CustomExtension.AlertMsg("刪除失敗", "");

                        return;
                    }
                    else
                    {
                        //導向本頁
                        Response.Redirect("{0}?Page={1}".FormatThis(thisPage, Req_PageIdx));
                    }

                    break;
            }
        }
    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取得資料
                string Get_Ship = DataBinder.Eval(dataItem.DataItem, "Ship_Time").ToString();


                //取得控制項
                PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                PlaceHolder ph_Send = (PlaceHolder)e.Item.FindControl("ph_Send");
                PlaceHolder ph_Close = (PlaceHolder)e.Item.FindControl("ph_Close");


                //判斷是否已出貨
                if (string.IsNullOrEmpty(Get_Ship))
                {
                    ph_Edit.Visible = true;
                    ph_Send.Visible = true;
                    ph_Close.Visible = true;
                }
                else
                {
                    ph_Edit.Visible = false;
                    ph_Send.Visible = false;
                    ph_Close.Visible = false;
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
    /// 查詢按鈕
    /// </summary>
    protected void btn_KeySearch_Click(object sender, EventArgs e)
    {
        doSearch();
    }


    /// <summary>
    /// 執行查詢
    /// </summary>
    /// <param name="keyword"></param>
    private void doSearch()
    {
        StringBuilder url = new StringBuilder();
        string keyword = this.filter_Keyword.Text;
        //string status = this.filter_Status.SelectedValue;


        url.Append("{0}?Page=1".FormatThis(thisPage));


        //[查詢條件] - 關鍵字
        if (!string.IsNullOrEmpty(keyword))
        {
            url.Append("&Keyword=" + Server.UrlEncode(keyword));
        }

        //[查詢條件] - status
        //url.Append("&status=" + Server.UrlEncode(status));


        //執行轉頁
        Response.Redirect(url.ToString(), false);
    }


    protected void lbtn_Export_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        ToyAdditionalRepository _data = new ToyAdditionalRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();


        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<

        //[取得參數] - Keyword
        if (!string.IsNullOrEmpty(Req_Keyword))
        {
            search.Add((int)mySearch.Keyword, Req_Keyword);
        }

        #endregion


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetDataList(search)
            .Select(fld => new {
                SeqNo = fld.SeqNo,
                CustTypeName = fld.CustTypeName,
                CustName = fld.CustName,
                CustTel = fld.CustTel,
                CustAddr = fld.CustAddr,
                ModelNo = fld.ModelNo,
                Qty = fld.Qty,
                Remark1 = fld.Remark1,
                Remark2 = fld.Remark2,
                Remark3 = fld.Remark3,
                ShipDate = fld.ShipDate,
                ShipNo = fld.ShipNo,
                Freight = fld.Freight
            });

        //將IQueryable轉成DataTable

        DataTable myDT = CustomExtension.LINQToDataTable(query);

        //重新命名欄位標頭
        myDT.Columns["SeqNo"].ColumnName = "ID";
        myDT.Columns["CustTypeName"].ColumnName = "來源";
        myDT.Columns["CustName"].ColumnName = "聯繫人";
        myDT.Columns["CustTel"].ColumnName = "聯繫電話";
        myDT.Columns["CustAddr"].ColumnName = "配送地址";
        myDT.Columns["ModelNo"].ColumnName = "品號";
        myDT.Columns["Qty"].ColumnName = "數量";
        myDT.Columns["Remark1"].ColumnName = "Step 1.客戶反映狀況描述";
        myDT.Columns["Remark2"].ColumnName = "Step 2.解決方式回覆";
        myDT.Columns["Remark3"].ColumnName = "Step 3.需補備品明細";
        myDT.Columns["ShipDate"].ColumnName = "寄出日";
        myDT.Columns["ShipNo"].ColumnName = "貨運及單號";
        myDT.Columns["Freight"].ColumnName = "運費";

        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "科玩補件登記簿-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);

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
        return "{0}{1}/{2}/ToyAdditional/{3}".FormatThis(
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
    /// 取得傳遞參數 - Keyword
    /// </summary>
    public string Req_Keyword
    {
        get
        {
            String Keyword = Request.QueryString["Keyword"];
            return (CustomExtension.String_資料長度Byte(Keyword, "1", "50", out ErrMsg)) ? Keyword.Trim() : "";
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
            this._thisPage = value;
        }
    }
    private string _thisPage;

    #endregion


}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using CustRebate_China_Data.Controllers;
using PKLib_Method.Methods;

public partial class myRebate_Search : SecurityCheck
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
                isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3733");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[權限判斷] End
                #endregion

                //Create Menu
                CreateMenu_Year(this.filter_Year);
                CreateMenu_Month(this.filter_Month);


                #region --Request參數--
                //[取得/檢查參數] - Req_Year
                if (!string.IsNullOrWhiteSpace(Req_Year))
                {
                    this.filter_Year.SelectedValue = Req_Year;
                }
                //[取得/檢查參數] - Req_Month
                if (!string.IsNullOrWhiteSpace(Req_Month))
                {
                    this.filter_Month.SelectedValue = Req_Month;
                }
                //[取得/檢查參數] - Req_Cust
                if (!string.IsNullOrWhiteSpace(Req_Cust))
                {
                    this.filter_Cust.Text = Req_Cust;
                }
                //[取得/檢查參數] - Req_DateSet
                if (!string.IsNullOrWhiteSpace(Req_DateSet))
                {
                    this.filter_dateSet.SelectedValue = Req_DateSet;
                }
                #endregion


                //Get Data
                LookupDataList();

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
    private void LookupDataList()
    {
        //----- 宣告:網址參數 -----
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        CustRebate_China_Repository _data = new CustRebate_China_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        string inputY = Req_Year;
        string inputM = Req_Month;

        //----- 原始資料:條件篩選 -----
        PageParam.Add("yy=" + Server.UrlEncode(inputY));
        PageParam.Add("mm=" + Server.UrlEncode(inputM));
        PageParam.Add("dateset=" + Server.UrlEncode(Req_DateSet));

        #region >> 條件篩選 <<

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrWhiteSpace(Req_Cust))
        {
            search.Add("Cust", Req_Cust);

            PageParam.Add("Cust=" + Server.UrlEncode(Req_Cust));
        }
        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCustRebateList(inputY, inputM, search, Req_DateSet, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        this.lvDataList.DataSource = query;
        this.lvDataList.DataBind();


        //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
        if (query.Count() == 0)
        {
            this.ph_EmptyData.Visible = true;
            this.ph_Data.Visible = false;

            //Clear
            CustomExtension.setCookie("HomeList_Rebate", "", -1);
        }
        else
        {
            this.ph_EmptyData.Visible = false;
            this.ph_Data.Visible = true;

            //重新整理頁面Url
            string reSetPage = "{0}?s=1{1}".FormatThis(
                thisPage
                , "&" + string.Join("&", PageParam.ToArray()));

            //暫存頁面Url, 給其他頁使用
            CustomExtension.setCookie("HomeList_Rebate", Server.UrlEncode(reSetPage), 1);

            //header
            Literal lt_headerYear = (Literal)this.lvDataList.FindControl("lt_headerYear");
            lt_headerYear.Text = Req_Year;
            Literal lt_headerMonth = (Literal)this.lvDataList.FindControl("lt_headerMonth");
            lt_headerMonth.Text = Req_Month;

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
                string _reMark = DataBinder.Eval(dataItem.DataItem, "Remark").ToString();

                //取得控制項
                Literal lt_Remark = (Literal)e.Item.FindControl("lt_Remark");
                lt_Remark.Text = string.IsNullOrWhiteSpace(_reMark) ? "" :
                    "<a data-tooltip=\"{0}\" data-inverted=\"\" data-position=\"top center\">備註</a>".FormatThis(_reMark);

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


    /// <summary>
    /// [按鈕] - 匯出
    /// </summary>
    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        CustRebate_China_Repository _data = new CustRebate_China_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        DataTable DT = new DataTable();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrWhiteSpace(Req_Cust))
        {
            search.Add("Cust", Req_Cust);
        }

        #endregion

        //----- 方法:取得資料(輸出順序以此為主) -----
        var query = _data.GetCustRebateList(Req_Year, Req_Month, search, Req_DateSet, out ErrMsg)
            .Select(fld => new
            {
                DeptName = fld.DeptName,
                CustName = fld.CustName,
                Cnt_e = fld.Cnt_e,
                Cnt_f = fld.Cnt_f,
                Cnt_g = fld.Cnt_g,
                Cnt_h = fld.Cnt_h,
                CntBase_A = fld.CntBase_A,
                CntBase_F = fld.CntBase_F,
                CntBase_Fa = fld.CntBase_Fa,
                Cnt_a = fld.Cnt_a,
                Remark = fld.Remark,
                CntBase_D = fld.CntBase_D,
                CntBase_E = fld.CntBase_E,
                Cnt_c = fld.Cnt_c,
                CntBase_B = fld.CntBase_B,
                Cnt_d = fld.Cnt_d,
                CntBase_C = fld.CntBase_C,
                Cnt_b = fld.Cnt_b,
                ProfitA = Math.Round(fld.ProfitA, 2),
                ProfitB = Math.Round(fld.ProfitB, 2),
                ProfitC = Math.Round(fld.ProfitC, 2),
                DataYear = fld.DataYear
            });

        //Sum
        var Total = query
           .GroupBy(
            gp => gp.DataYear
           )
           .Select(
            el => new
            {
                sumA = el.Sum(f => f.CntBase_A),
                sumB = el.Sum(f => f.CntBase_B),
                sumC = el.Sum(f => f.CntBase_C),
                sumD = el.Sum(f => f.CntBase_D),
                sumE = el.Sum(f => f.CntBase_E),
                sumF = el.Sum(f => f.CntBase_F),
                suma = el.Sum(f => f.Cnt_a),
                sumb = el.Sum(f => f.Cnt_b),
                sumc = el.Sum(f => f.Cnt_c),
                sumd = el.Sum(f => f.Cnt_d),
                sume = el.Sum(f => f.Cnt_e),
                sumg = el.Sum(f => f.Cnt_g)
            }
           ).FirstOrDefault();

        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);

        if (myDT.Rows.Count > 0)
        {
            //重新命名欄位標頭
            myDT.Columns["DeptName"].ColumnName = "部門";
            myDT.Columns["CustName"].ColumnName = "客戶";
            myDT.Columns["Cnt_e"].ColumnName = "責任目標(含稅)";
            myDT.Columns["Cnt_f"].ColumnName = "回饋方式(目標)";
            myDT.Columns["Cnt_g"].ColumnName = "挑戰目標";
            myDT.Columns["Cnt_h"].ColumnName = "回饋方式(挑戰)";
            myDT.Columns["CntBase_A"].ColumnName = "目前系統業績";
            myDT.Columns["CntBase_F"].ColumnName = "單別2341 ";
            myDT.Columns["CntBase_Fa"].ColumnName = "B009 ";
            myDT.Columns["Cnt_a"].ColumnName = Req_Year + "實際返利業績(含已返利)";
            myDT.Columns["Remark"].ColumnName = "備註";
            myDT.Columns["CntBase_D"].ColumnName = "與挑戰目標差額";
            myDT.Columns["CntBase_E"].ColumnName = "與責任目標差額";
            myDT.Columns["Cnt_c"].ColumnName = "應回饋金額";
            myDT.Columns["CntBase_B"].ColumnName = "已回饋金額";
            myDT.Columns["Cnt_d"].ColumnName = "剩餘回饋金額";
            myDT.Columns["CntBase_C"].ColumnName = Req_Month + "月銷售金額(含稅)";
            myDT.Columns["Cnt_b"].ColumnName = "當月最高返利金額";
            myDT.Columns["ProfitA"].ColumnName = "返利前毛利率";
            myDT.Columns["ProfitB"].ColumnName = "返利後毛利率";
            myDT.Columns["ProfitC"].ColumnName = "全返後毛利率";
        }


        //在footer新增一列
        DataRow dtRow = myDT.NewRow();
        dtRow[1] = "總計";
        dtRow[2] = Total.sume;
        dtRow[4] = Total.sumg;
        dtRow[6] = Total.sumA;
        dtRow[7] = Total.sumF;
        dtRow[9] = Total.suma;
        dtRow[11] = Total.sumD;
        dtRow[12] = Total.sumE;
        dtRow[13] = Total.sumc;
        dtRow[14] = Total.sumB;
        dtRow[15] = Total.sumd;
        dtRow[16] = Total.sumC;
        dtRow[17] = Total.sumb;

        myDT.Rows.Add(dtRow);

        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "DataOutput-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }

    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 數字欄位顏色格式化
    /// </summary>
    /// <param name="inputValue"></param>
    /// <returns></returns>
    public object showNumber(object inputValue)
    {
        if (inputValue == null)
        {
            return "";
        }
        return (Convert.ToDouble(inputValue) < 0)
            ? "negative" : (inputValue.ToString().Equals("0"))
                ? "grey-text text-lighten-2" : "positive";
    }

    protected void CreateMenu_Year(DropDownList item)
    {
        int currYear = DateTime.Now.Year;
        int prevYear = currYear - 2;
        int nextYear = currYear;

        item.Items.Clear();
        for (int itemY = prevYear; itemY <= nextYear; itemY++)
        {
            item.Items.Add(new ListItem(itemY.ToString(), itemY.ToString()));
        }

        item.SelectedValue = currYear.ToString();
    }

    protected void CreateMenu_Month(DropDownList item)
    {
        int currMonth = DateTime.Now.Month;
        item.Items.Clear();
        for (int row = 1; row <= 12; row++)
        {
            item.Items.Add(new ListItem(row.ToString(), row.ToString()));
        }

        item.SelectedValue = currMonth.ToString();
    }


    /// <summary>
    /// 含查詢條件的完整網址
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _Year = this.filter_Year.SelectedValue;
        string _Month = this.filter_Month.SelectedValue;
        string _Cust = this.filter_Cust.Text;
        string _ds = filter_dateSet.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?s=1".FormatThis(thisPage));

        //[查詢條件] - Year
        if (!string.IsNullOrWhiteSpace(_Year))
        {
            url.Append("&yy=" + Server.UrlEncode(_Year));
        }
        //[查詢條件] - Month
        if (!string.IsNullOrWhiteSpace(_Month))
        {
            url.Append("&mm=" + Server.UrlEncode(_Month));
        }
        //[查詢條件] - Cust
        if (!string.IsNullOrWhiteSpace(_Cust))
        {
            url.Append("&Cust=" + Server.UrlEncode(_Cust));
        }
        //[查詢條件] - DateSet
        if (!string.IsNullOrWhiteSpace(_ds))
        {
            url.Append("&dateSet=" + Server.UrlEncode(_ds));
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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/RebateChina".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
    }

    #endregion


    #region -- 傳遞參數 --

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
    /// 取得傳遞參數 - Year
    /// </summary>
    public string Req_Year
    {
        get
        {
            String _data = Request.QueryString["yy"];
            string dt = DateTime.Now.Year.ToString();
            return (CustomExtension.String_資料長度Byte(_data, "1", "4", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_Year = value;
        }
    }
    private string _Req_Year;

    /// <summary>
    /// 取得傳遞參數 - Year
    /// </summary>
    public string Req_Month
    {
        get
        {
            String _data = Request.QueryString["mm"];
            string dt = ("0" + DateTime.Now.Month.ToString()).Right(2);
            return (CustomExtension.String_資料長度Byte(_data, "1", "2", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_Month = value;
        }
    }
    private string _Req_Month;

    /// <summary>
    /// 取得傳遞參數 - DateSet
    /// </summary>
    public string Req_DateSet
    {
        get
        {
            String _data = Request.QueryString["dateset"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "A";
        }
        set
        {
            this._Req_DateSet = value;
        }
    }
    private string _Req_DateSet;


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
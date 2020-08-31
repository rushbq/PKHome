using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myQuote_SearchByProd : SecurityCheck
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
                /* 
                 * 判斷對應的MENU ID
                 * 取得其他權限
                 */
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3151");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End

            }
        }
        catch (Exception)
        {

            throw;
        }
    }



    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 匯出
    /// </summary>
    protected void btn_Excel_Click(object sender, EventArgs e)
    {
        ////----- 宣告:資料參數 -----
        //Menu3000Repository _data = new Menu3000Repository();
        //Dictionary<string, string> search = new Dictionary<string, string>();
        //int DataCnt = 0;

        //string _Keyword = filter_Keyword.Text;
        //string _ModelNo = val_Prods.Text;
        //string _cls = filter_Class.SelectedValue;

        //#region >> 條件篩選 <<
        ////[查詢條件] - ModelNo
        //if (!string.IsNullOrWhiteSpace(_ModelNo))
        //{
        //    search.Add("ModelNo", _ModelNo);
        //}

        ////[查詢條件] - Keyword
        //if (!string.IsNullOrWhiteSpace(_Keyword))
        //{
        //    search.Add("Keyword", _Keyword);
        //}

        ////[查詢條件] - ClassID
        //if (!string.IsNullOrWhiteSpace(_cls))
        //{
        //    search.Add("ClassID", _cls);
        //}
        //#endregion

        ////----- 原始資料:取得所有資料 -----
        //DataTable myDT = _data.GetProdStockStat(search, Req_Lang, 0, 99999, out DataCnt, out ErrMsg);

        //if (myDT.Rows.Count > 0)
        //{
        //    #region ** 重新設定欄位位置 **
        //    //在重命名前先執行
        //    myDT.Columns["ModelNo"].SetOrdinal(0);
        //    myDT.Columns["ModelName"].SetOrdinal(1);
        //    myDT.Columns["StockQty_01"].SetOrdinal(2);
        //    myDT.Columns["PreSell_01"].SetOrdinal(3);
        //    myDT.Columns["PreIN_01"].SetOrdinal(4);
        //    myDT.Columns["PreSet_01"].SetOrdinal(5);
        //    myDT.Columns["PreGet_01"].SetOrdinal(6);
        //    myDT.Columns["StockQty_20"].SetOrdinal(7);
        //    myDT.Columns["PreSell_20"].SetOrdinal(8);
        //    myDT.Columns["PreIN_20"].SetOrdinal(9);
        //    myDT.Columns["PreSet_20"].SetOrdinal(10);
        //    myDT.Columns["PreGet_20"].SetOrdinal(11);
        //    myDT.Columns["StockQty_21"].SetOrdinal(12);
        //    myDT.Columns["PreSell_21"].SetOrdinal(13);
        //    myDT.Columns["PreIN_21"].SetOrdinal(14);
        //    myDT.Columns["PreSet_21"].SetOrdinal(15);
        //    myDT.Columns["PreGet_21"].SetOrdinal(16);
        //    myDT.Columns["StockQty_22"].SetOrdinal(17);
        //    myDT.Columns["PreSell_22"].SetOrdinal(18);
        //    myDT.Columns["PreIN_22"].SetOrdinal(19);
        //    myDT.Columns["PreSet_22"].SetOrdinal(20);
        //    myDT.Columns["PreGet_22"].SetOrdinal(21);
        //    myDT.Columns["StockQty_12"].SetOrdinal(22);
        //    myDT.Columns["PreSell_12"].SetOrdinal(23);
        //    myDT.Columns["PreIN_12"].SetOrdinal(24);
        //    myDT.Columns["PreSet_12"].SetOrdinal(25);
        //    myDT.Columns["PreGet_12"].SetOrdinal(26);
        //    myDT.Columns["StockQty_128"].SetOrdinal(27);
        //    myDT.Columns["PreSell_128"].SetOrdinal(28);
        //    myDT.Columns["PreIN_128"].SetOrdinal(29);
        //    myDT.Columns["PreSet_128"].SetOrdinal(30);
        //    myDT.Columns["PreGet_128"].SetOrdinal(31);
        //    myDT.Columns["StockQty_A01"].SetOrdinal(32);
        //    myDT.Columns["PreSell_A01"].SetOrdinal(33);
        //    myDT.Columns["PreIN_A01"].SetOrdinal(34);
        //    myDT.Columns["PreSet_A01"].SetOrdinal(35);
        //    myDT.Columns["PreGet_A01"].SetOrdinal(36);
        //    myDT.Columns["SZ_StockQty_A01"].SetOrdinal(37);
        //    myDT.Columns["SZ_PreSell_A01"].SetOrdinal(38);
        //    myDT.Columns["SZ_PreIN_A01"].SetOrdinal(39);
        //    myDT.Columns["SZ_PreSet_A01"].SetOrdinal(40);
        //    myDT.Columns["SZ_PreGet_A01"].SetOrdinal(41);
        //    myDT.Columns["Class_ID"].SetOrdinal(42);

        //    #endregion


        //    //重新命名欄位標頭
        //    #region ** 重新命名欄位標頭 **
        //    myDT.Columns["ModelNo"].ColumnName = "品號";
        //    myDT.Columns["ModelName"].ColumnName = "品名";
        //    myDT.Columns["Class_ID"].ColumnName = "類別";
        //    myDT.Columns["StockQty_01"].ColumnName = "01庫存(台灣)";
        //    myDT.Columns["PreSell_01"].ColumnName = "01預計銷";
        //    myDT.Columns["PreIN_01"].ColumnName = "01預計進";
        //    myDT.Columns["PreSet_01"].ColumnName = "01預計生";
        //    myDT.Columns["PreGet_01"].ColumnName = "01預計領";
        //    myDT.Columns["StockQty_20"].ColumnName = "20庫存(台灣)";
        //    myDT.Columns["PreSell_20"].ColumnName = "20預計銷";
        //    myDT.Columns["PreIN_20"].ColumnName = "20預計進";
        //    myDT.Columns["PreSet_20"].ColumnName = "20預計生";
        //    myDT.Columns["PreGet_20"].ColumnName = "20預計領";
        //    myDT.Columns["StockQty_21"].ColumnName = "21庫存(台灣)";
        //    myDT.Columns["PreSell_21"].ColumnName = "21預計銷";
        //    myDT.Columns["PreIN_21"].ColumnName = "21預計進";
        //    myDT.Columns["PreSet_21"].ColumnName = "21預計生";
        //    myDT.Columns["PreGet_21"].ColumnName = "21預計領";
        //    myDT.Columns["StockQty_22"].ColumnName = "22庫存(台灣)";
        //    myDT.Columns["PreSell_22"].ColumnName = "22預計銷";
        //    myDT.Columns["PreIN_22"].ColumnName = "22預計進";
        //    myDT.Columns["PreSet_22"].ColumnName = "22預計生";
        //    myDT.Columns["PreGet_22"].ColumnName = "22預計領";
        //    myDT.Columns["StockQty_12"].ColumnName = "12庫存(上海)";
        //    myDT.Columns["PreSell_12"].ColumnName = "12預計銷";
        //    myDT.Columns["PreIN_12"].ColumnName = "12預計進";
        //    myDT.Columns["PreSet_12"].ColumnName = "12預計生";
        //    myDT.Columns["PreGet_12"].ColumnName = "12預計領";
        //    myDT.Columns["StockQty_128"].ColumnName = "128庫存(上海)";
        //    myDT.Columns["PreSell_128"].ColumnName = "128預計銷";
        //    myDT.Columns["PreIN_128"].ColumnName = "128預計進";
        //    myDT.Columns["PreSet_128"].ColumnName = "128預計生";
        //    myDT.Columns["PreGet_128"].ColumnName = "128預計領";
        //    myDT.Columns["StockQty_A01"].ColumnName = "A01庫存(上海)";
        //    myDT.Columns["PreSell_A01"].ColumnName = "A01預計銷";
        //    myDT.Columns["PreIN_A01"].ColumnName = "A01預計進";
        //    myDT.Columns["PreSet_A01"].ColumnName = "A01預計生";
        //    myDT.Columns["PreGet_A01"].ColumnName = "A01預計領";
        //    myDT.Columns["SZ_StockQty_A01"].ColumnName = "A01庫存(SZ)";
        //    myDT.Columns["SZ_PreSell_A01"].ColumnName = "A01預計銷(SZ)";
        //    myDT.Columns["SZ_PreIN_A01"].ColumnName = "A01預計進(SZ)";
        //    myDT.Columns["SZ_PreSet_A01"].ColumnName = "A01預計生(SZ)";
        //    myDT.Columns["SZ_PreGet_A01"].ColumnName = "A01預計領(SZ)";
        //    #endregion

        //}

        ////匯出Excel
        //CustomExtension.ExportExcel(
        //    myDT
        //    , "ProdStock-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
        //    , false);
    }

    #endregion


    #region -- 附加功能 --


    #endregion


    #region -- 網址參數 --

    ///// <summary>
    ///// 取得網址參數 - 語系
    ///// </summary>
    //public string Req_Lang
    //{
    //    get
    //    {
    //        string myLang = Page.RouteData.Values["lang"] == null ? "auto" : Page.RouteData.Values["lang"].ToString();

    //        //若為auto, 就去抓cookie
    //        return myLang.Equals("auto") ? fn_Language.Get_Lang(Request.Cookies["PKHome_Lang"].Value) : myLang;
    //    }
    //    set
    //    {
    //        _Req_Lang = value;
    //    }
    //}
    //private string _Req_Lang;


    ///// <summary>
    ///// 取得網址參數 - RootID
    ///// </summary>
    //private string _Req_RootID;
    //public string Req_RootID
    //{
    //    get
    //    {
    //        String DataID = Page.RouteData.Values["rootID"].ToString();

    //        return DataID.ToLower().Equals("unknown") ? "" : DataID;
    //    }
    //    set
    //    {
    //        _Req_RootID = value;
    //    }
    //}

    ///// <summary>
    ///// 取得網址參數 - Company ID(TW/SH/SZ)
    ///// </summary>
    //private string _Req_CompID;
    //public string Req_CompID
    //{
    //    get
    //    {
    //        String DataID = Page.RouteData.Values["CompID"].ToString();

    //        return DataID.ToLower().Equals("unknown") ? "TW" : DataID;
    //    }
    //    set
    //    {
    //        this._Req_CompID = value;
    //    }
    //}

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "";
        //return "{0}{1}/{2}/ProdStockStat".FormatThis(
        //    fn_Param.WebUrl
        //    , Req_Lang
        //    , Req_RootID);
    }

    #endregion


    //#region -- 傳遞參數 --

    ///// <summary>
    ///// ModelNo
    ///// </summary>
    //public string Req_ModelNo
    //{
    //    get
    //    {
    //        String _data = Request.QueryString["m"];
    //        return string.IsNullOrWhiteSpace(_data) ? "" : _data;
    //    }
    //    set
    //    {
    //        this._Req_ModelNo = value;
    //    }
    //}
    //private string _Req_ModelNo;

    ///// <summary>
    ///// Keyword
    ///// </summary>
    //public string Req_Keyword
    //{
    //    get
    //    {
    //        String _data = Request.QueryString["k"];
    //        return string.IsNullOrWhiteSpace(_data) ? "" : _data;
    //    }
    //    set
    //    {
    //        this._Req_Keyword = value;
    //    }
    //}
    //private string _Req_Keyword;

    ///// <summary>
    ///// Class
    ///// </summary>
    //public string Req_Class
    //{
    //    get
    //    {
    //        String _data = Request.QueryString["cls"];
    //        return string.IsNullOrWhiteSpace(_data) ? "" : _data;
    //    }
    //    set
    //    {
    //        _Req_Class = value;
    //    }
    //}
    //private string _Req_Class;

    ///// <summary>
    ///// 設定參數 - 本頁Url
    ///// </summary>
    //public string thisPage
    //{
    //    get
    //    {
    //        return "{0}".FormatThis(FuncPath());
    //    }
    //    set
    //    {
    //        _thisPage = value;
    //    }
    //}
    //private string _thisPage;

    //#endregion

}
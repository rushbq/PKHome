using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

public partial class myPurPlan_SearchByProd : SecurityCheck
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
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4214");

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
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        int DataCnt = 0;

        string _stockType = "B"; //A=12, B=A01, C=合併倉
        string _ModelNo = val_Prods.Text;
        string _nDays = filter_Days.Text;
        string _CustomFilter = menuFilter.SelectedValue;

        #region >> 條件篩選 <<
        //必要條件 - nDays
        search.Add("nDays", string.IsNullOrWhiteSpace(_nDays) ? "90" : _nDays);

        //[查詢條件] - ModelNo
        if (!string.IsNullOrWhiteSpace(_ModelNo))
        {
            search.Add("ModelNo", _ModelNo);
        }

        //[查詢條件] - CustomFilter
        if (!string.IsNullOrWhiteSpace(_CustomFilter))
        {
            search.Add("CustomFilter", _CustomFilter);
        }
        #endregion

        //----- 方法:取得資料 -----
        var _rowData = _data.GetPurPlan_SH(_stockType, search, 0, 99999, out DataCnt, out ErrMsg);

        if (_rowData.Count() == 0)
        {
            CustomExtension.AlertMsg("目前條件查不到資料.", "");
            return;
        }

        //object to datatable
        DataTable myDT = CustomExtension.LINQToDataTable(_rowData);

        #region ** 填入指定欄位 **

        Dictionary<string, string> _col = new Dictionary<string, string>();
        _col.Add("ModelNo", "品號");
        _col.Add("Item_Type", "屬性");
        _col.Add("ModelName", "品名");
        _col.Add("ProdVol", "目錄");
        //_col.Add("ProdPage", "頁次");
        //_col.Add("StockQty_A01", "庫存A01");
        //_col.Add("PreIN_A01", "預計進A01");
        //_col.Add("VirIn_A01", "虛擬入A01");
        //_col.Add("PlanIN_A01", "計劃進A01");
        //_col.Add("WaitQty", "待驗收");
        //_col.Add("NowMonthTurn_A01", "現有周轉月A01");
        //_col.Add("PushQty", "催貨量");
        //_col.Add("PreSell_A01", "預計銷A01");
        //_col.Add("VirPreSell", "虛擬預計銷");
        //_col.Add("RealPreSell", "實際預計銷");
        //_col.Add("Qty_Days", "近N天用量");
        //_col.Add("Qty_Year", "全年平均月用量");
        //_col.Add("SZ_QtyOfYear", "深圳全年平均月用量");
        //_col.Add("Qty_Season", "去年當季平均用量");
        //_col.Add("MonthTurn_A01", "可用週轉月");
        //_col.Add("UsefulQty_A01", "可用量");
        //_col.Add("SafeQty_A01", "安全存量");
        //_col.Add("InBox_Qty", "內盒數量");
        //_col.Add("Qty_Packing", "一箱數量");
        //_col.Add("OutBox_Cuft", "整箱材積");
        //_col.Add("MOQ", "銷售MOQ");
        //_col.Add("ProdMsg", "產銷訊息");


        //將指定的欄位,轉成陣列
        string[] selectedColumns = _col.Keys.ToArray();

        //資料複製到新的Table(內容為指定的欄位資料)
        DataTable newDT = new DataView(myDT).ToTable(true, selectedColumns);


        #endregion


        #region ** 重新命名欄位,顯示為中文 **

        foreach (var item in _col)
        {
            string _id = item.Key;
            string _name = item.Value;

            newDT.Columns[_id].ColumnName = _name;

        }
        #endregion

        //匯出Excel
        CustomExtension.ExportExcel(
            newDT
            , "ExcelData-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }


    #endregion


    #region -- 附加功能 --


    #endregion


    #region -- 網址參數 --

    /// <summary>
    /// 取得網址參數 - Company ID(TW/SH/SZ)
    /// </summary>
    private string _Req_CompID;
    public string Req_CompID
    {
        get
        {
            String DataID = Request.QueryString["CompID"] == null ? "" : Request.QueryString["CompID"].ToString();

            return DataID.ToLower().Equals("") ? "SH" : DataID;
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
        return "";
        //return "{0}{1}/{2}/ProdStockStat".FormatThis(
        //    fn_Param.WebUrl
        //    , Req_Lang
        //    , Req_RootID);
    }

    #endregion

}
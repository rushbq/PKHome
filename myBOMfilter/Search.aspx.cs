using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

public partial class myBOMfilter_Search : SecurityCheck
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
                /* 
                 * 使用公司別代號，判斷對應的MENU ID
                 */
                bool isPass = false;
                string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

                switch (getCorpUid)
                {
                    case "3":
                        //上海寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4872");
                        //default dbs
                        filter_DBS.SelectedValue = "SH";

                        break;

                    default:
                        //TW
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4871");
                        //default dbs
                        filter_DBS.SelectedValue = "TW";

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
    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        int DataCnt = 0;

        string _ModelNo = filter_ModelNo.Text;
        string _Stop = filter_StopType.SelectedValue;
        string _DBS = filter_DBS.SelectedValue;

        //檢查必填
        if (string.IsNullOrWhiteSpace(_ModelNo) || string.IsNullOrWhiteSpace(_DBS))
        {
            CustomExtension.AlertMsg("品號 / 資料庫為必填", "");
            return;
        }

        #region >> 條件篩選 <<
        //[查詢條件] - ModelNo
        if (!string.IsNullOrWhiteSpace(_ModelNo))
        {
            search.Add("ModelNo", _ModelNo);
        }

        //[查詢條件] - Stop
        if (!string.IsNullOrWhiteSpace(_Stop))
        {
            search.Add("Stop", _Stop);
        }

        #endregion

        //----- 原始資料:取得所有資料 -----
        DataTable myDT = _data.GetBOMfilter(search, _DBS, 0, 99999, out DataCnt, out ErrMsg);

        if (myDT.Rows.Count > 0)
        {
            #region ** 重新設定欄位位置 **
            //在重命名前先執行
            myDT.Columns["PartModelNo"].SetOrdinal(0);
            myDT.Columns["MainModelNo"].SetOrdinal(1);
            myDT.Columns["MainModelName"].SetOrdinal(2);
            myDT.Columns["Qty"].SetOrdinal(3);
            myDT.Columns["SupName"].SetOrdinal(4);
            myDT.Columns["Ship_From"].SetOrdinal(5);
            myDT.Columns["MarketMsg"].SetOrdinal(6);
            myDT.Columns["StockProp"].SetOrdinal(7);
            myDT.Columns["Vol"].SetOrdinal(8);
            myDT.Columns["Page"].SetOrdinal(9);
            myDT.Columns["Date_Of_Listing"].SetOrdinal(10);
            myDT.Columns["SO_Date"].SetOrdinal(11);
            myDT.Columns["CustName"].SetOrdinal(12);
            myDT.Columns["SO_Qty"].SetOrdinal(13);
            myDT.Columns["YearQty"].SetOrdinal(14);
            myDT.Columns["ProdProp"].SetOrdinal(15);

            #endregion


            //重新命名欄位標頭
            #region ** 重新命名欄位標頭 **
            myDT.Columns["PartModelNo"].ColumnName = "品號";
            myDT.Columns["MainModelNo"].ColumnName = "工具組品號";
            myDT.Columns["MainModelName"].ColumnName = "工具組品名";
            myDT.Columns["Qty"].ColumnName = "用量";
            myDT.Columns["SupName"].ColumnName = "主供應商";
            myDT.Columns["Ship_From"].ColumnName = "出貨地";
            myDT.Columns["MarketMsg"].ColumnName = "產銷訊息";
            myDT.Columns["StockProp"].ColumnName = "倉管屬性";
            myDT.Columns["Vol"].ColumnName = "目錄";
            myDT.Columns["Page"].ColumnName = "頁次";
            myDT.Columns["Date_Of_Listing"].ColumnName = "上市日期";
            myDT.Columns["SO_Date"].ColumnName = "最近出貨時間";
            myDT.Columns["CustName"].ColumnName = "最近出貨客戶";
            myDT.Columns["SO_Qty"].ColumnName = "最近出貨數量";
            myDT.Columns["YearQty"].ColumnName = "近一年銷量";
            myDT.Columns["ProdProp"].ColumnName = "品號屬性";
            #endregion

        }

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "BOMList-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
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
        return "{0}{1}/{2}/BOMFilter/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
    }

    #endregion



}
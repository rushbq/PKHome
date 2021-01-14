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

public partial class myPurProdCost_Search : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //** 檢查必要參數 **
                if (string.IsNullOrEmpty(Req_CompID))
                {
                    Response.Redirect("{0}Error/參數錯誤".FormatThis(fn_Param.WebUrl));
                    return;
                }

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
                        //SH
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4882");
                        break;

                    default:
                        //TW
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4881");
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
    protected void btn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        int DataCnt = 0;

        string _keyword = filter_Keyword.Text;
        string _SupID = val_Sups.Text;
        string _ModelNo = val_Prods.Text;
        string _DBS = Req_CompID;

        //-----原始資料:條件篩選---- -
        #region >> 條件篩選 <<

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_keyword))
        {
            search.Add("Keyword", _keyword);
        }

        //[取得/檢查參數] - SupID
        if (!string.IsNullOrWhiteSpace(_SupID))
        {
            search.Add("SupID", _SupID);
        }

        //[取得/檢查參數] - ModelNo
        if (!string.IsNullOrWhiteSpace(_ModelNo))
        {
            search.Add("ModelNo", _ModelNo);
        }
        #endregion

        //----- 方法:取得資料 -----
        var _rowData = _data.GetCost_PurProd(search, _DBS, -1, -1, false, out DataCnt, out ErrMsg);

        if (_rowData == null)
        {
            CustomExtension.AlertMsg("目前條件查不到資料.", "");
            return;
        }

        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(_rowData);

        #region ** 填入指定欄位 **

        Dictionary<string, string> _col = new Dictionary<string, string>();
        _col.Add("ModelNo", "品號");
        _col.Add("Currency", "幣別");
        _col.Add("ModelPrice", "品號核價單價");
        _col.Add("PackSumPrice", "卡片核價單價*數量");
        _col.Add("ProdCost", "標準成本");
        _col.Add("PackItemNo", "卡片品號");
        _col.Add("PackPrice", "卡片核價單價");
        _col.Add("PackQty", "卡片數量");
        _col.Add("ProdNote", "品號備註");

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

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
        }
        set
        {
            _Req_CompID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/PurProdCost/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
    }

    #endregion


    //#region -- 傳遞參數 --
    ///// <summary>
    ///// 取得傳遞參數 - PageIdx(目前索引頁)
    ///// </summary>
    //public int Req_PageIdx
    //{
    //    get
    //    {
    //        int data = Request.QueryString["Page"] == null ? 1 : Convert.ToInt32(Request.QueryString["Page"]);
    //        return data;
    //    }
    //    set
    //    {
    //        _Req_PageIdx = value;
    //    }
    //}
    //private int _Req_PageIdx;

    ///// <summary>
    ///// 取得傳遞參數 - Keyword
    ///// </summary>
    //public string Req_Keyword
    //{
    //    get
    //    {
    //        String _data = Request.QueryString["k"];
    //        return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
    //    }
    //    set
    //    {
    //        this._Req_Keyword = value;
    //    }
    //}
    //private string _Req_Keyword;

    ///// <summary>
    ///// 取得傳遞參數 - ModelNo
    ///// </summary>
    //public string Req_ModelNo
    //{
    //    get
    //    {
    //        String _data = Request.QueryString["ModelNo"];
    //        return _data;
    //    }
    //    set
    //    {
    //        _Req_ModelNo = value;
    //    }
    //}
    //private string _Req_ModelNo;


    ///// <summary>
    ///// 取得傳遞參數 - SupID
    ///// </summary>
    //public string Req_SupID
    //{
    //    get
    //    {
    //        String _data = Request.QueryString["SupID"];
    //        return _data;
    //    }
    //    set
    //    {
    //        _Req_SupID = value;
    //    }
    //}
    //private string _Req_SupID;


    //#endregion


}
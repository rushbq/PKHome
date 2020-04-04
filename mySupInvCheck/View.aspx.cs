using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

public partial class mySupInvCheck_View : SecurityCheck
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

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4861");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End


                //Get Data
                LookupBase();
                LookupDataList();

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

        #region >> 條件篩選 <<

        search.Add("Parent_ID", Req_ParentID);
        search.Add("SupID", Req_SupID);

        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetSupInvReplyDetail(search, out ErrMsg)
            .Select(fld => new
            {
                ModelNo = fld.ModelNo,
                ModelName = fld.ModelName,
                Currency = fld.Currency,
                lastPurPrice = fld.lastPurPrice,
                StockNum = fld.StockNum,
                InputQty1 = fld.InputQty1,
                InputQty2 = fld.InputQty2,
                TotalQty = fld.InputQty1 + fld.InputQty2,
                CompareQty = fld.StockNum - (fld.InputQty1 + fld.InputQty2),
                inStockDate = fld.inStockDate,
                outStockDate = fld.outStockDate,
                anotherModel = fld.anotherModel
            });


        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);

        if (myDT.Rows.Count > 0)
        {
            //重新命名欄位標頭
            myDT.Columns["ModelNo"].ColumnName = "品號";
            myDT.Columns["ModelName"].ColumnName = "品名";
            myDT.Columns["Currency"].ColumnName = "幣別";
            myDT.Columns["lastPurPrice"].ColumnName = "最近核價";
            myDT.Columns["StockNum"].ColumnName = "寶工庫存";
            myDT.Columns["InputQty1"].ColumnName = "廠商盤點(未包裝數)";
            myDT.Columns["InputQty2"].ColumnName = "廠商盤點(已包裝未出貨數)";
            myDT.Columns["TotalQty"].ColumnName = "廠商盤點(總數)";
            myDT.Columns["CompareQty"].ColumnName = "差額數量";
            myDT.Columns["inStockDate"].ColumnName = "最近入庫日";
            myDT.Columns["outStockDate"].ColumnName = "最近出庫日";
            myDT.Columns["anotherModel"].ColumnName = "替代品號";
        }


        //release
        query = null;

        //匯出Excel
        string supName = lt_NavHeader.Text.Replace(" ", "");
        CustomExtension.ExportExcel(
            myDT
            , "{0}-{1}.xlsx".FormatThis(supName, DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }

    #endregion



    #region -- 資料顯示 --

    private void LookupBase()
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<

        search.Add("Parent_ID", Req_ParentID);
        search.Add("SupID", Req_SupID);

        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOneSupInvReply(search, out ErrMsg);


        //----- 資料整理:繫結 ----- 
        if (query.Count() > 0)
        {
            var data = query.FirstOrDefault();
            lt_NavHeader.Text = "({0}) {1}".FormatThis(data.SupID, data.SupName);
        }

        //release
        _data = null;
    }

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupDataList()
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<

        search.Add("Parent_ID", Req_ParentID);
        search.Add("SupID", Req_SupID);

        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetSupInvReplyDetail(search, out ErrMsg);


        //----- 資料整理:繫結 ----- 
        lvDataList.DataSource = query;
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
        }

        //release
        _data = null;
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
    /// 取得網址參數 - ParentID
    /// </summary>
    private string _Req_ParentID;
    public string Req_ParentID
    {
        get
        {
            String DataID = Page.RouteData.Values["id"].ToString();

            return DataID.ToLower().Equals("0") ? "" : DataID;
        }
        set
        {
            _Req_ParentID = value;
        }
    }


    /// <summary>
    /// 取得網址參數 - SupID
    /// </summary>
    private string _Req_SupID;
    public string Req_SupID
    {
        get
        {
            String DataID = Page.RouteData.Values["sup"].ToString();

            return DataID.ToLower().Equals("") ? "" : DataID;
        }
        set
        {
            _Req_SupID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/SupInvCheck".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
    }

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
    //        this._thisPage = value;
    //    }
    //}
    //private string _thisPage;


    /// <summary>
    /// 設定參數 - 列表頁Url
    /// </summary>
    private string _Page_SearchUrl;
    public string Page_SearchUrl
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("SupInvCheckB");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion


}
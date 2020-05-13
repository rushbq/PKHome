using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myShipping_Search : SecurityCheck
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
                    case "2":
                        //深圳寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3701");
                        break;

                    default:
                        //SH
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3702");
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


                #region --Request參數--
                //[取得/檢查參數] - Req_sDate
                if (!string.IsNullOrWhiteSpace(Req_sDate))
                {
                    this.filter_sDate.Text = Req_sDate;
                }
                //[取得/檢查參數] - Req_eDate
                if (!string.IsNullOrWhiteSpace(Req_eDate))
                {
                    this.filter_eDate.Text = Req_eDate;
                }

                //[取得/檢查參數] - 發貨日
                if (!string.IsNullOrWhiteSpace(Req_sDate_Ship))
                {
                    this.filter_sDate_Ship.Text = Req_sDate_Ship;
                }
                //[取得/檢查參數] - 發貨日
                if (!string.IsNullOrWhiteSpace(Req_eDate_Ship))
                {
                    this.filter_eDate_Ship.Text = Req_eDate_Ship;
                }
                //[取得/檢查參數] - Req_ErpNo
                if (!string.IsNullOrWhiteSpace(Req_ErpNo))
                {
                    this.filter_ErpNo.Text = Req_ErpNo;
                }
                //[取得/檢查參數] - Req_Cust
                if (!string.IsNullOrWhiteSpace(Req_Cust))
                {
                    this.filter_Cust.Text = Req_Cust;
                }

                //[取得/檢查參數] - Req_Pay
                this.cb_Pay1.Checked = Req_Pay1.Equals("1") ? true : false;
                this.cb_Pay2.Checked = Req_Pay2.Equals("1") ? true : false;
                this.cb_Pay3.Checked = Req_Pay3.Equals("1") ? true : false;

                //[取得/檢查參數] - 物流途徑
                if (!string.IsNullOrWhiteSpace(Req_Way))
                {
                    this.filter_ShipWay.SelectedIndex = this.filter_ShipWay.Items.IndexOf(this.filter_ShipWay.Items.FindByValue(Req_Way));
                }

                //[取得/檢查參數] - 出貨地
                if (!string.IsNullOrWhiteSpace(Req_ShipFrom))
                {
                    this.filter_ShipFrom.SelectedIndex = this.filter_ShipFrom.Items.IndexOf(this.filter_ShipFrom.Items.FindByValue(Req_ShipFrom));
                }

                //[取得/檢查參數] - 排序
                if (!string.IsNullOrWhiteSpace(Req_Sf))
                {
                    this.sort_SortField.SelectedIndex = this.sort_SortField.Items.IndexOf(this.sort_SortField.Items.FindByValue(Req_Sf));
                }

                if (!string.IsNullOrWhiteSpace(Req_Sw))
                {
                    this.sort_SortWay.SelectedIndex = this.sort_SortWay.Items.IndexOf(this.sort_SortWay.Items.FindByValue(Req_Sw));
                }

                #endregion


                //*** 設定母版的Menu ***
                Literal menu = (Literal)Page.Master.FindControl("lt_headerMenu");
                menu.Text = fn_Menu.GetTopMenu_ShipFreight(Req_Lang, Req_RootID, Req_CompID, Req_Tab);


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
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        Dictionary<string, string> sort = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<

        //固定條件:TOP選單
        PageParam.Add("tab=" + Server.UrlEncode(Req_Tab));

        //[取得/檢查參數] - Date (需轉為ERP格式)
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate.ToDateString("yyyyMMdd"));

            PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate.ToDateString("yyyyMMdd"));

            PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
        }

        //[取得/檢查參數] - ShipDate
        if (!string.IsNullOrWhiteSpace(Req_sDate_Ship))
        {
            search.Add("ShipsDate", Req_sDate_Ship);

            PageParam.Add("sDate_Ship=" + Server.UrlEncode(Req_sDate_Ship));
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate_Ship))
        {
            search.Add("ShipeDate", Req_eDate_Ship);

            PageParam.Add("eDate_Ship=" + Server.UrlEncode(Req_eDate_Ship));
        }

        //[取得/檢查參數] - ErpNo
        if (!string.IsNullOrWhiteSpace(Req_ErpNo))
        {
            search.Add("Keyword", Req_ErpNo);

            PageParam.Add("ErpNo=" + Server.UrlEncode(Req_ErpNo));
        }

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrWhiteSpace(Req_Cust))
        {
            search.Add("Cust", Req_Cust);

            PageParam.Add("Cust=" + Server.UrlEncode(Req_Cust));
        }

        //[取得/檢查參數] - Way
        if (!string.IsNullOrWhiteSpace(Req_Way))
        {
            if (!Req_Way.Equals("ALL"))
            {
                search.Add("Way", Req_Way);
            }

            PageParam.Add("Way=" + Server.UrlEncode(Req_Way));
        }

        //[取得/檢查參數] - ShipFrom
        if (!string.IsNullOrWhiteSpace(Req_ShipFrom))
        {
            if (!Req_ShipFrom.Equals("ALL"))
            {
                search.Add("ShipFrom", Req_ShipFrom);
            }

            PageParam.Add("ShipFrom=" + Server.UrlEncode(Req_ShipFrom));
        }

        if (Req_Pay1.Equals("1"))
        {
            search.Add("Pay1", Req_Pay1);
            PageParam.Add("Pay1=" + Server.UrlEncode(Req_Pay1));
        }
        if (Req_Pay2.Equals("1"))
        {
            search.Add("Pay2", Req_Pay2);
            PageParam.Add("Pay2=" + Server.UrlEncode(Req_Pay2));
        }
        if (Req_Pay3.Equals("1"))
        {
            search.Add("Pay3", Req_Pay3);
            PageParam.Add("Pay3=" + Server.UrlEncode(Req_Pay3));
        }

        //Sort
        if (!string.IsNullOrWhiteSpace(Req_Sf))
        {
            sort.Add("Field", Req_Sf);

            PageParam.Add("sf=" + Server.UrlEncode(Req_Sf));
        }
        if (!string.IsNullOrWhiteSpace(Req_Sw))
        {
            sort.Add("Way", Req_Sw);

            PageParam.Add("sw=" + Server.UrlEncode(Req_Sw));
        }
        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipFreightList(Req_CompID, search, sort, out ErrMsg);


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
        this.lvDataList.DataSource = data;
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

            //分頁
            string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                , thisPage, PageParam, false, true);

            Literal lt_Pager = (Literal)this.lvDataList.FindControl("lt_Pager");
            lt_Pager.Text = getPager;

            //重新整理頁面Url
            string reSetPage = "{0}?Page={1}{2}".FormatThis(
                thisPage
                , pageIndex
                , "&" + string.Join("&", PageParam.ToArray()));

            //暫存頁面Url, 給其他頁使用
            CustomExtension.setCookie("HomeList_Shipping", Server.UrlEncode(reSetPage), 1);

        }

    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;
                Menu3000Repository _data = new Menu3000Repository();

                //取得資料
                var shipWay = DataBinder.Eval(dataItem.DataItem, "ShipWay");
                string Get_Ship = shipWay == null ? "" : shipWay.ToString();

                //取得控制項
                Literal lt_ShipWay = (Literal)e.Item.FindControl("lt_ShipWay");
                lt_ShipWay.Text = _data.GetShipWay(Get_Ship);

                _data = null;

            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    /// <summary>
    /// 數字顏色格式化
    /// </summary>
    /// <param name="inputValue"></param>
    /// <returns></returns>
    public object showNumber(object inputValue)
    {
        if (inputValue == null)
        {
            return "";
        }

        if (inputValue.ToString().Equals("0"))
        {
            return "<span class=\"grey-text text-lighten-2\">{0}</span>".FormatThis(inputValue);

        }

        return inputValue;
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
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        Dictionary<string, string> sort = new Dictionary<string, string>();
        DataTable DT = new DataTable();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<

        //[取得/檢查參數] - Date (需轉為ERP格式)
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate.ToDateString("yyyyMMdd"));
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate.ToDateString("yyyyMMdd"));
        }

        //[取得/檢查參數] - ShipDate
        if (!string.IsNullOrWhiteSpace(Req_sDate_Ship))
        {
            search.Add("ShipsDate", Req_sDate_Ship);
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate_Ship))
        {
            search.Add("ShipeDate", Req_eDate_Ship);
        }

        //[取得/檢查參數] - ErpNo
        if (!string.IsNullOrWhiteSpace(Req_ErpNo))
        {
            search.Add("Keyword", Req_ErpNo);
        }

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrWhiteSpace(Req_Cust))
        {
            search.Add("Cust", Req_Cust);
        }

        //[取得/檢查參數] - Way
        if (!string.IsNullOrWhiteSpace(Req_Way))
        {
            if (!Req_Way.Equals("ALL"))
            {
                search.Add("Way", Req_Way);
            }
        }

        //[取得/檢查參數] - ShipFrom
        if (!string.IsNullOrWhiteSpace(Req_ShipFrom))
        {
            if (!Req_ShipFrom.Equals("ALL"))
            {
                search.Add("ShipFrom", Req_ShipFrom);
            }
        }

        search.Add("Pay1", Req_Pay1);
        search.Add("Pay2", Req_Pay2);
        search.Add("Pay3", Req_Pay3);

        //Sort
        if (!string.IsNullOrWhiteSpace(Req_Sf))
        {
            sort.Add("Field", Req_Sf);
        }
        if (!string.IsNullOrWhiteSpace(Req_Sw))
        {
            sort.Add("Way", Req_Sw);
        }

        #endregion

        //----- 方法:取得資料 -----
        var query = _data.GetShipFreightList(Req_CompID, search, sort, out ErrMsg)
            .Select(fld => new
            {
                CustName = fld.CustName,
                ErpNo = fld.Erp_SO_FID + "-" + fld.Erp_SO_SID,
                ErpDate = fld.Erp_SO_Date,
                ShipDate = fld.ShipDate,
                TotalPrice = fld.TotalPrice,
                ShipWay = _data.GetShipWay(fld.ShipWay),
                ShipCompName = fld.ShipCompName,
                ShipNo = fld.ShipNo,
                ShipWho = fld.ShipWho,
                StockName = fld.StockName,
                ShipCnt = fld.ShipCnt,
                Pay1 = fld.Pay1,
                Pay2 = fld.Pay2,
                Pay3 = fld.Pay3,
                AvgPay1 = fld.AvgPay1,
                AvgPay2 = fld.AvgPay2,
                AvgPay3 = fld.AvgPay3,
                Remark = fld.Remark
            });

        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);

        if (myDT.Rows.Count > 0)
        {
            //重新命名欄位標頭
            myDT.Columns["CustName"].ColumnName = "客戶";
            myDT.Columns["ErpNo"].ColumnName = "ERP單號";
            myDT.Columns["ErpDate"].ColumnName = "單據日";
            myDT.Columns["ShipDate"].ColumnName = "發貨日期";
            myDT.Columns["TotalPrice"].ColumnName = "銷貨金額";
            myDT.Columns["ShipWay"].ColumnName = "物流途徑";
            myDT.Columns["ShipCompName"].ColumnName = "貨運公司";
            myDT.Columns["ShipNo"].ColumnName = "物流單號";
            myDT.Columns["ShipWho"].ColumnName = "收貨人";
            myDT.Columns["StockName"].ColumnName = "出貨地";
            myDT.Columns["ShipCnt"].ColumnName = "件數";
            myDT.Columns["Pay1"].ColumnName = "到付$";
            myDT.Columns["Pay2"].ColumnName = "自付$";
            myDT.Columns["Pay3"].ColumnName = "墊付$";
            myDT.Columns["AvgPay1"].ColumnName = "平均運費-到付$";
            myDT.Columns["AvgPay2"].ColumnName = "平均運費-自付$";
            myDT.Columns["AvgPay3"].ColumnName = "平均運費-墊付$";
            myDT.Columns["Remark"].ColumnName = "備註";
        }

        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "DataOutput-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }


    ///// <summary>
    ///// [按鈕] - 轉入物流單
    ///// </summary>
    //protected void btn_Import_Click(object sender, EventArgs e)
    //{
    //    //查詢StoreProcedure (EF_ShippingNo)
    //    using (SqlCommand cmd = new SqlCommand())
    //    {
    //        cmd.Parameters.Clear();
    //        cmd.CommandType = CommandType.StoredProcedure;
    //        cmd.CommandText = "EF_ShippingNo";
    //        cmd.Parameters.AddWithValue("ERP_sDate", val_sDate.Text.ToDateString("yyyyMMdd"));
    //        cmd.Parameters.AddWithValue("ERP_eDate", val_eDate.Text.ToDateString("yyyyMMdd"));
    //        cmd.Parameters.AddWithValue("Creater", fn_Param.CurrentUser);
    //        cmd.Parameters.AddWithValue("CompanyID", Req_CompID);
    //        cmd.CommandTimeout = 120;

    //        //取得回傳值, 輸出參數
    //        SqlParameter Msg = cmd.Parameters.Add("@Msg", SqlDbType.NVarChar, 200);
    //        Msg.Direction = ParameterDirection.Output;

    //        if (!dbConn.ExecuteSql(cmd, out ErrMsg))
    //        {
    //            CustomExtension.AlertMsg("轉入失敗", "");
    //            Response.Write("<h2>Error Message:</h2><h3>{0}</h3>".FormatThis(ErrMsg));
    //            return;
    //        }
    //        else
    //        {
    //            CustomExtension.AlertMsg("已執行轉入..." + Msg.Value.ToString(), filterUrl());
    //        }

    //    }
    //}
    #endregion


    #region -- 附加功能 --

    /// <summary>
    /// 含查詢條件的完整網址
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _sDate = this.filter_sDate.Text;
        string _eDate = this.filter_eDate.Text;
        string _sDate_Ship = this.filter_sDate_Ship.Text;
        string _eDate_Ship = this.filter_eDate_Ship.Text;
        string _ErpNo = this.filter_ErpNo.Text;
        string _Cust = this.filter_Cust.Text;
        string _Way = this.filter_ShipWay.SelectedValue;
        string _ShipFrom = this.filter_ShipFrom.SelectedValue;
        string _SortField = this.sort_SortField.SelectedValue;
        string _SortWay = this.sort_SortWay.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?tab={1}&page=1".FormatThis(thisPage, Req_Tab));

        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
        }

        //[查詢條件] - ShipDate
        if (!string.IsNullOrWhiteSpace(_sDate_Ship))
        {
            url.Append("&sDate_Ship=" + Server.UrlEncode(_sDate_Ship));
        }
        if (!string.IsNullOrWhiteSpace(_eDate_Ship))
        {
            url.Append("&eDate_Ship=" + Server.UrlEncode(_eDate_Ship));
        }

        //[查詢條件] - ErpNo
        if (!string.IsNullOrWhiteSpace(_ErpNo))
        {
            url.Append("&ErpNo=" + Server.UrlEncode(_ErpNo));
        }

        //[查詢條件] - Cust
        if (!string.IsNullOrWhiteSpace(_Cust))
        {
            url.Append("&Cust=" + Server.UrlEncode(_Cust));
        }

        //[查詢條件] - Way
        if (!string.IsNullOrWhiteSpace(_Way))
        {
            url.Append("&Way=" + Server.UrlEncode(_Way));
        }

        //[取得/檢查參數] - ShipFrom
        if (!string.IsNullOrWhiteSpace(_ShipFrom))
        {
            url.Append("&ShipFrom=" + Server.UrlEncode(_ShipFrom));
        }

        //[查詢條件] - Pay
        if (this.cb_Pay1.Checked)
        {
            url.Append("&Pay1=1");
        }
        if (this.cb_Pay2.Checked)
        {
            url.Append("&Pay2=1");
        }
        if (this.cb_Pay3.Checked)
        {
            url.Append("&Pay3=1");
        }

        //[排序條件]
        if (!string.IsNullOrWhiteSpace(_SortField))
        {
            url.Append("&sf=" + _SortField);
            url.Append("&sw=" + _SortWay);
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
    /// 取得網址參數 - Company ID(TW/SH/SZ)
    /// </summary>
    private string _Req_CompID;
    public string Req_CompID
    {
        get
        {
            String DataID = Page.RouteData.Values["CompID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "SZ" : DataID;
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
        return "{0}{1}/{2}/ShipFreight/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
    }

    public string FuncPath_Import()
    {
        return "{0}{1}/{2}/ShipImport/{3}".FormatThis(
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


    public string Req_Tab
    {
        get
        {
            string data = Request.QueryString["tab"] == null ? "1" : Request.QueryString["tab"].ToString();
            return data;
        }
        set
        {
            this._Req_Tab = value;
        }
    }
    private string _Req_Tab;


    /// <summary>
    /// 取得傳遞參數 - sDate
    /// 預設7日內
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.AddDays(-7).ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_sDate = value;
        }
    }
    private string _Req_sDate;


    /// <summary>
    /// 取得傳遞參數 - eDate
    /// </summary>
    public string Req_eDate
    {
        get
        {
            String _data = Request.QueryString["eDate"];
            string dt = DateTime.Now.ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_eDate = value;
        }
    }
    private string _Req_eDate;


    /// <summary>
    /// 取得傳遞參數 - sDate_Ship
    /// </summary>
    public string Req_sDate_Ship
    {
        get
        {
            String _data = Request.QueryString["sDate_Ship"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_sDate_Ship = value;
        }
    }
    private string _Req_sDate_Ship;


    /// <summary>
    /// 取得傳遞參數 - eDate_Ship
    /// </summary>
    public string Req_eDate_Ship
    {
        get
        {
            String _data = Request.QueryString["eDate_Ship"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_eDate_Ship = value;
        }
    }
    private string _Req_eDate_Ship;


    /// <summary>
    /// 取得傳遞參數 - ErpNo
    /// </summary>
    public string Req_ErpNo
    {
        get
        {
            String _data = Request.QueryString["ErpNo"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_ErpNo = value;
        }
    }
    private string _Req_ErpNo;


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
    /// 取得傳遞參數 - Way
    /// </summary>
    public string Req_Way
    {
        get
        {
            String _data = Request.QueryString["Way"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "2", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Way = value;
        }
    }
    private string _Req_Way;


    /// <summary>
    /// 取得傳遞參數 - ShipFrom
    /// </summary>
    public string Req_ShipFrom
    {
        get
        {
            String _data = Request.QueryString["ShipFrom"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "2", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_ShipFrom = value;
        }
    }
    private string _Req_ShipFrom;


    /// <summary>
    /// 取得傳遞參數 - Pay1 (1=true / 0=false)
    /// </summary>
    public string Req_Pay1
    {
        get
        {
            String _data = Request.QueryString["Pay1"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "0";
        }
        set
        {
            this._Req_Pay1 = value;
        }
    }
    private string _Req_Pay1;


    /// <summary>
    /// 取得傳遞參數 - Pay2 (1=true / 0=false)
    /// </summary>
    public string Req_Pay2
    {
        get
        {
            String _data = Request.QueryString["Pay2"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "0";
        }
        set
        {
            this._Req_Pay2 = value;
        }
    }
    private string _Req_Pay2;


    /// <summary>
    /// 取得傳遞參數 - Pay3 (1=true / 0=false)
    /// </summary>
    public string Req_Pay3
    {
        get
        {
            String _data = Request.QueryString["Pay3"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "0";
        }
        set
        {
            this._Req_Pay3 = value;
        }
    }
    private string _Req_Pay3;

    /// <summary>
    /// Sort參數-欄位
    /// </summary>
    public string Req_Sf
    {
        get
        {
            String _data = Request.QueryString["sf"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Sf = value;
        }
    }
    private string _Req_Sf;

    /// <summary>
    /// Sort參數-方式
    /// </summary>
    public string Req_Sw
    {
        get
        {
            String _data = Request.QueryString["sw"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Sw = value;
        }
    }
    private string _Req_Sw;


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
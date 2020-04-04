using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myShipmentData_Search_TW : SecurityCheck
{
    public string ErrMsg;
    public IQueryable<ClassItem> _shipItem;
    public IQueryable<ClassItem> _placeItem;
    public IQueryable<ClassItem> _checkItem;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //[權限判斷] Start
                #region --權限--

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3773");

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
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
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
            //[取得/檢查參數] - Date
            if (!string.IsNullOrWhiteSpace(Req_sDate))
            {
                search.Add("sDate", Req_sDate);
                PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
                filter_sDate.Text = Req_sDate;
            }
            if (!string.IsNullOrWhiteSpace(Req_eDate))
            {
                search.Add("eDate", Req_eDate);
                PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
                filter_eDate.Text = Req_eDate;
            }

            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetCustomsData(search, "TW", StartRow, RecordsPerPage, out DataCnt, out ErrMsg);

            //----- 資料整理:取得總筆數 -----
            TotalRow = DataCnt;

            //----- 資料整理:頁數判斷 -----
            if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
            {
                StartRow = 0;
                pageIndex = 1;
            }

            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
            lvDataList.DataBind();


            //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
            if (query.Count() == 0)
            {
                ph_EmptyData.Visible = true;
                ph_Data.Visible = false;
                ph_Save.Visible = false;
            }
            else
            {
                ph_EmptyData.Visible = false;
                ph_Data.Visible = true;

                //分頁設定
                string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                    , FuncPath(), PageParam, false, true);
                lt_Pager.Text = getPager;

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


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        //string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                //ListViewDataItem dataItem = (ListViewDataItem)e.Item;


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
    /// [按鈕] - Save
    /// </summary>
    protected void btn_Save_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        try
        {
            //取得Listview控制項
            ListView _view = lvDataList;

            //Check null
            if (_view.Items.Count == 0)
            {
                CustomExtension.AlertMsg("無資料可設定,請確認ERP進口報單贖單.", filterUrl());
                return;
            }

            //宣告
            List<CustomsData_Item> dataList = new List<CustomsData_Item>();

            //取得各欄位資料
            for (int row = 0; row < _view.Items.Count; row++)
            {
                #region ** 取得欄位資料 **

                string _DataID = ((HiddenField)_view.Items[row].FindControl("hf_DataID")).Value;
                string _Redeem_FID = ((HiddenField)_view.Items[row].FindControl("hf_Redeem_FID")).Value;
                string _Redeem_SID = ((HiddenField)_view.Items[row].FindControl("hf_Redeem_SID")).Value;
                string _Cost_Customs = ((TextBox)_view.Items[row].FindControl("tb_Cost_Customs")).Text;
                string _Cost_LocalCharge = ((TextBox)_view.Items[row].FindControl("tb_Cost_LocalCharge")).Text;
                string _Cost_LocalBusiness = ((TextBox)_view.Items[row].FindControl("tb_Cost_LocalBusiness")).Text;
                string _Cost_Imports = ((TextBox)_view.Items[row].FindControl("tb_Cost_Imports")).Text;
                string _Cost_Trade = ((TextBox)_view.Items[row].FindControl("tb_Cost_Trade")).Text;
                string _Cost_ImportsBusiness = ((TextBox)_view.Items[row].FindControl("tb_Cost_ImportsBusiness")).Text;
                string _Cost_Service = ((TextBox)_view.Items[row].FindControl("tb_Cost_Service")).Text;
                string _Cost_Truck = ((TextBox)_view.Items[row].FindControl("tb_Cost_Truck")).Text;
                string _Remark = ((TextBox)_view.Items[row].FindControl("tb_Remark")).Text;

                #endregion

                //將值填入容器
                var dataItem = new CustomsData_Item
                {
                    Data_ID = string.IsNullOrWhiteSpace(_DataID) ? new Guid(CustomExtension.GetGuid()) : new Guid(_DataID),
                    Redeem_FID = _Redeem_FID,
                    Redeem_SID = _Redeem_SID,
                    Cost_Customs = string.IsNullOrWhiteSpace(_Cost_Customs) ? 0 : Convert.ToDouble(_Cost_Customs),
                    Cost_LocalCharge = string.IsNullOrWhiteSpace(_Cost_LocalCharge) ? 0 : Convert.ToDouble(_Cost_LocalCharge),
                    Cost_LocalBusiness = string.IsNullOrWhiteSpace(_Cost_LocalBusiness) ? 0 : Convert.ToDouble(_Cost_LocalBusiness),
                    Cost_Imports = string.IsNullOrWhiteSpace(_Cost_Imports) ? 0 : Convert.ToDouble(_Cost_Imports),
                    Cost_Trade = string.IsNullOrWhiteSpace(_Cost_Trade) ? 0 : Convert.ToDouble(_Cost_Trade),
                    Cost_ImportsBusiness = string.IsNullOrWhiteSpace(_Cost_ImportsBusiness) ? 0 : Convert.ToDouble(_Cost_ImportsBusiness),
                    Cost_Service = string.IsNullOrWhiteSpace(_Cost_Service) ? 0 : Convert.ToDouble(_Cost_Service),
                    Cost_Truck = string.IsNullOrWhiteSpace(_Cost_Truck) ? 0 : Convert.ToDouble(_Cost_Truck),
                    Remark = _Remark,
                    Create_Who = fn_Param.CurrentUser
                };

                //add to list
                dataList.Add(dataItem);
            }

            //Call function
            if (!_data.Check_CustomsData(dataList, out ErrMsg))
            {
                CustomExtension.AlertMsg("資料儲存失敗...", "");
                return;
            }

            //redirect page
            Response.Redirect(thisPage);


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
    /// [按鈕] - 匯出
    /// </summary>
    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);
        }
        //[取得/檢查參數] - Date
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate);
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate);
        }
        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetAllCustomsData(search, "TW", out ErrMsg)
            .Select(fld => new
            {
                報關日期 = fld.CustomsDate,
                贖單日期 = fld.RedeemDate,
                廠商 = fld.SupName + "(" + fld.SupID + ")",
                贖單單號 = fld.Redeem_FID + "-" + fld.Redeem_SID,
                件數 = fld.QtyMark,
                報關金額 = fld.CustomsPrice,
                採購金額幣別 = fld.Currency,
                採購金額 = fld.PurPrice,
                匯率 = fld.Tax,
                進口報單號碼 = fld.CustomsNo,
                報關費_NTD未稅 = fld.Cost_Customs,
                LocalCharge_NTD未稅 = fld.Cost_LocalCharge,
                營業稅 = fld.Cost_LocalBusiness,
                進口稅 = fld.Cost_Imports,
                貿推費 = fld.Cost_Trade,
                進口營業稅 = fld.Cost_ImportsBusiness,
                代墊款 = fld.Cnt_CostFee,
                總計 = fld.Cnt_Total,
                商港費 = fld.Cost_Service,
                進口費用百分比 = fld.Cnt_CostPercent,
                採購單張數 = fld.PurCnt,
                採購單Item數 = fld.PurItemCnt,
                卡車 = fld.Cost_Truck,
                備註 = fld.Remark
            });


        //Check null
        if (query.Count() == 0)
        {
            CustomExtension.AlertMsg("查無資料", filterUrl());
            return;
        }


        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);


        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "{0}-{1}.xlsx".FormatThis("出貨明細表-台灣進口", DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }

    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _sDate = filter_sDate.Text;
        string _eDate = filter_eDate.Text;
        string _Keyword = filter_Keyword.Text;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page
        url.Append("{0}?page=1".FormatThis(FuncPath()));

        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
        }

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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/CustomsData-TW".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
    }

    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            //Params
            string _sDate = filter_sDate.Text;
            string _eDate = filter_eDate.Text;
            string _Keyword = filter_Keyword.Text;

            //url string
            StringBuilder url = new StringBuilder();

            //固定條件:Page
            url.Append("{0}?page={1}".FormatThis(FuncPath(), Req_PageIdx));

            //[查詢條件] - Date
            if (!string.IsNullOrWhiteSpace(_sDate))
            {
                url.Append("&sDate=" + Server.UrlEncode(_sDate));
            }
            if (!string.IsNullOrWhiteSpace(_eDate))
            {
                url.Append("&eDate=" + Server.UrlEncode(_eDate));
            }

            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(_Keyword))
            {
                url.Append("&k=" + Server.UrlEncode(_Keyword));
            }


            return url.ToString();
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
    /// 取得傳遞參數 - sDate
    /// 預設月初
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.ToShortDateString().ToDateString("yyyy/MM/01");
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
    /// 預設月底
    /// </summary>
    public string Req_eDate
    {
        get
        {
            String _data = Request.QueryString["eDate"];
            DateTime nextMonth = Convert.ToDateTime(DateTime.Now.AddMonths(1).ToShortDateString().ToDateString("yyyy/MM/01"));
            string dt = nextMonth.AddDays(-1).ToShortDateString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_eDate = value;
        }
    }
    private string _Req_eDate;

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

    #endregion


}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3174");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End

                //建立選單
                GetProdClassMenu(ddl_Class);
                GetProdVolMenu(ddl_Vol);

                //讀取基本資料
                LookupBaseData();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料取得 --
    private void LookupBaseData()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        try
        {
            //----- 原始資料:取得資料 -----
            var data = _data.GetQuote_NowRate(out ErrMsg);

            if (data.Rows.Count > 0)
            {
                lb_twRate.Text = data.Rows[0]["SaleRate_tw"].ToString();
                lb_shRate.Text = data.Rows[0]["SaleRate_sh"].ToString();
            }

        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //release
            _data = null;
        }
    }


    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 匯出
    /// </summary>
    protected void btn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        int DataCnt = 0;

        string _keyword = filter_Keyword.Text;
        string _ClsID = ddl_Class.SelectedValue;
        string _itemNo = filter_ItemNo.Text;
        string _vol = ddl_Vol.SelectedValue;
        string _dateType = ddl_dateType.SelectedValue;
        string _sDate = filter_sDate.Text;
        string _eDate = filter_eDate.Text;

        //-----原始資料:條件篩選---- -
        #region >> 條件篩選 <<

        //[查詢條件] - Class
        if (!string.IsNullOrWhiteSpace(_ClsID) && (!_ClsID.Equals("-1")))
        {
            search.Add("ClassID", _ClsID);
        }

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_keyword))
        {
            search.Add("Keyword", _keyword);
        }

        //[查詢條件] - ItemNo
        if (!string.IsNullOrWhiteSpace(_itemNo))
        {
            search.Add("ItemNo", _itemNo);
        }

        //[查詢條件] - Vol
        if (!string.IsNullOrWhiteSpace(_vol) && (!_vol.Equals("-1")))
        {
            search.Add("Vol", _vol);
        }

        //[查詢條件] - Search Date
        switch (_dateType)
        {
            case "A":
                if (!string.IsNullOrWhiteSpace(_sDate))
                {
                    search.Add("sDateA", _sDate);
                }
                if (!string.IsNullOrWhiteSpace(_eDate))
                {
                    search.Add("eDateA", _eDate);
                }

                break;

            case "B":
                if (!string.IsNullOrWhiteSpace(_sDate))
                {
                    search.Add("sDateB", _sDate);
                }
                if (!string.IsNullOrWhiteSpace(_eDate))
                {
                    search.Add("eDateB", _eDate);
                }

                break;

            default:
                //do nothing
                break;
        }
        #endregion

        //----- 方法:取得資料 -----
        var _rowData = _data.GetQuote_AllComp(search, -1, -1, false, out DataCnt, out ErrMsg);

        if (_rowData.Rows.Count == 0)
        {
            CustomExtension.AlertMsg("目前條件查不到資料.", "");
            return;
        }

        //object to datatable
        DataTable myDT = _rowData;

        #region ** 填入指定欄位 **

        Dictionary<string, string> _col = new Dictionary<string, string>();
        _col.Add("Model_No", "品號");
        _col.Add("CatVol", "目錄");
        _col.Add("CatPage", "頁次");
        _col.Add("Ship_From", "主要出貨地");

        _col.Add("tw_StdCost", "標準成本-台灣成本(NTD)");
        _col.Add("tw_PurPrice", "採購最新核價-台灣成本(NTD)");
        _col.Add("tw_ChkDay", "核價日-台灣");
        _col.Add("sh_StdCost", "標準成本-上海成本(RMB)");
        _col.Add("sh_PurPrice", "採購最新核價-上海成本(RMB)");
        _col.Add("sh_ChkDay", "核價日-上海");

        _col.Add("tw_AgentPrice", "台灣Agent價");
        _col.Add("tw_Rate_AgentPrice", "利潤率%(台灣Agent)");
        _col.Add("tw_ValidDate", "台灣生效日");

        _col.Add("sh_AgentPrice", "上海Agent價");
        _col.Add("sh_Rate_AgentPrice", "利潤率%(上海Agent)");
        _col.Add("sh_ValidDate", "上海生效日");

        _col.Add("sh_LowestPrice", "業務底價-中國市場(RMB)");
        _col.Add("sh_Rate_LowestPrice", "利潤率%(業務底價)");

        _col.Add("sh_SellPrice", "中國經銷價-中國市場(RMB)");
        _col.Add("sh_Rate_SellPrice", "利潤率%(中國經銷價)");

        _col.Add("sh_NetPrice", "中國網路價-中國市場(RMB)");
        _col.Add("sh_Rate_NetPrice", "利潤率%(中國網路價)");

        _col.Add("PurPrice", "京東採購價-中國市場(RMB)");
        _col.Add("sh_Rate_PurPrice", "利潤率%(京東採購價)");

        _col.Add("ListPrice", "京東頁面價-中國市場(RMB)");
        _col.Add("sh_Rate_ListPrice", "利潤率%(京東頁面價)");

        _col.Add("sh_SalePrice", "零售價-中國市場(RMB)");
        _col.Add("sh_Rate_SalePrice", "利潤率%(零售價)");

        _col.Add("tw_NetPrice", "台灣網路價-台灣市場(NTD)");
        _col.Add("tw_Rate_NetPrice", "利潤率%(台灣網路價)");

        _col.Add("tw_InAgentPrice", "內銷經銷價-台灣市場(NTD)");
        _col.Add("tw_Rate_InAgentPrice", "利潤率%(內銷經銷價)");

        _col.Add("ModelName_TW", "中文品名");
        _col.Add("ModelName_EN", "英文品名");
        _col.Add("Packing", "包裝方式");
        _col.Add("InnerBox", "內盒產品數");
        _col.Add("CTNQty", "整箱數量");
        _col.Add("IB_Cuft", "材積");
        _col.Add("IB_NW", "淨重");
        _col.Add("IB_GW", "毛重");
        _col.Add("onlineDate", "上市日");
        _col.Add("offlineDate", "停售日");

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
            , "ExcelData-ALL-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }

    #endregion


    #region -- 附加功能 --

    /// <summary>
    /// 建立選單-產品銷售類別
    /// </summary>
    private void GetProdClassMenu(DropDownList ddl)
    {
        //控制項-銷售類別
        ddl.Items.Clear();
        ddl.Items.Insert(0, new ListItem("- 所有類別 -", "-1"));

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        try
        {
            //----- 原始資料:取得資料 -----
            var data = _data.GetProdClass("ZH-TW", out ErrMsg);

            foreach (var item in data)
            {
                ddl.Items.Add(new ListItem(item.ID + " - " + item.Label, item.ID.ToString()));
            }

        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //release
            _data = null;
        }
    }


    /// <summary>
    /// 建立選單-產品目錄
    /// </summary>
    private void GetProdVolMenu(DropDownList ddl)
    {
        //控制項-Vol
        ddl.Items.Clear();
        ddl.Items.Insert(0, new ListItem("- 不限 -", "-1"));

        try
        {
            //----- 原始資料:取得資料 -----
            var data = fn_Menu.GetProdVol(out ErrMsg);

            foreach (var item in data)
            {
                ddl.Items.Add(new ListItem(item.Label, item.Label));
            }

        }
        catch (Exception)
        {

            throw;
        }

    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "";
    }
    #endregion


}
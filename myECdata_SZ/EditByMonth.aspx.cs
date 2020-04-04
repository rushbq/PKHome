using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myECdata_SZ_EditByMonth : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            /* [取得MenuID, MenuName]
                Req_TypeID -> 取得對應的MenuID, 檢查是否有權限
                Req_TypeID + Req_Lang -> 取得對應的Type Name
            */
            string refType = fn_Menu.GetECData_RefType(Convert.ToInt32(Req_TypeID));
            string typeName = refType + " (每月)";

            //取得功能名稱
            lt_TypeName.Text = typeName;
            //設定PageTitle
            Page.Title = typeName;
            lt_PlatformType.Text = refType;

            //[權限判斷] Start
            #region --權限--
            /* 
             * 判斷對應的MENU ID
             * 取得其他權限
             */
            bool isPass = false;

            switch (Req_TypeID)
            {
                case "1":
                    //工具
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3743");
                    break;

                default:
                    //科玩
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3748");
                    break;
            }

            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }

            #endregion
            //[權限判斷] End


            if (!IsPostBack)
            {
                //[產生選單]
                CreateMenu_Year(ddl_Year);
                CreateMenu_Month(ddl_Month);
                Get_ClassList(Req_TypeID, ddl_Mall);

                //[參數判斷] - 資料編號
                if (Req_DataID.Equals("new"))
                {
                    //促銷費用單身(hide)
                    ph_Promo.Visible = false;

                    //填入預設值-月份
                    ddl_Month.SelectedValue = DateTime.Today.Month.ToString();
                }
                else
                {
                    //載入資料
                    LookupData();
                    //載入單身資料
                    LookupData_Detail();
                    //載入單身編輯資料
                    if (!string.IsNullOrWhiteSpace(Req_SubID))
                    {
                        Edit_Detail(Req_SubID);
                    }

                    //促銷費用單身(show)
                    ph_Promo.Visible = true;

                    //填入預設日期
                    tb_RecordDate.Text = DateTime.Today.AddDays(-1).ToString().ToDateString("yyyy/MM/dd");
                }

                /*
                  判斷類型,鎖定指定欄位
                */
                switch (Req_TypeID)
                {
                    case "1":
                        //工具
                        tb_Price_SalesRebate.Enabled = false;
                        tb_Price_Freight.Enabled = false;
                        break;

                    case "2":
                        //科玩
                        tb_Price_Cost.Enabled = false;
                        tb_Price_Profit.Enabled = false;
                        tb_Price_Purchase.Enabled = false;
                        tb_Price_Back.Enabled = false;
                        tb_Price_PurchaseRebate.Enabled = false;
                        break;
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示:基本資料 --

    /// <summary>
    /// 取得基本資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOneECD_byMonth(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }


        #region >> 欄位填寫 <<

        hf_DataID.Value = query.Data_ID.ToString();
        ddl_Year.SelectedValue = query.setYear.ToString();
        ddl_Month.SelectedValue = query.setMonth.ToString();
        ddl_Mall.SelectedValue = query.RefMall.ToString();
        tb_Price_Income.Text = query.Price_Income.ToString();
        tb_Price_SalesRebate.Text = query.Price_SalesRebate.ToString();
        tb_Price_Cost.Text = query.Price_Cost.ToString();
        tb_Price_Profit.Text = query.Price_Profit.ToString();
        tb_Price_Purchase.Text = query.Price_Purchase.ToString();
        tb_Price_Back.Text = query.Price_Back.ToString();
        tb_Price_PurchaseRebate.Text = query.Price_PurchaseRebate.ToString();
        tb_Price_Freight.Text = query.Price_Freight.ToString();
        lt_Price_Promo.Text = query.Price_Promo.ToString();
        //維護資訊
        info_Creater.Text = query.Create_Name;
        info_CreateTime.Text = query.Create_Time;
        info_Updater.Text = query.Update_Name;
        info_UpdateTime.Text = query.Update_Time;

        #endregion

        #region >> 欄位控制 <<

        ddl_Year.Enabled = false;
        ddl_Month.Enabled = false;
        ddl_Mall.Enabled = false;

        #endregion


        //Release
        _data = null;

    }

    #endregion


    #region -- 資料編輯:基本資料 --

    /// <summary>
    /// 資料新增
    /// </summary>
    /// <param name="type">1:Stay;2:List</param>
    private void Add_Data(string type)
    {
        try
        {
            //----- 宣告:資料參數 -----
            Menu3000Repository _data = new Menu3000Repository();

            //----- 設定:資料欄位 -----
            //產生Guid
            string guid = CustomExtension.GetGuid();
            Int16 _RefType = Convert.ToInt16(Req_TypeID);
            Int32 _RefMall = Convert.ToInt32(ddl_Mall.SelectedValue);
            Int32 _setYear = Convert.ToInt32(ddl_Year.SelectedValue);
            Int32 _setMonth = Convert.ToInt32(ddl_Month.SelectedValue);
            string _priceA = tb_Price_Income.Text;
            string _priceB = tb_Price_SalesRebate.Text;
            string _priceC = tb_Price_Cost.Text;
            string _priceD = tb_Price_Profit.Text;
            string _priceE = tb_Price_Purchase.Text;
            string _priceF = tb_Price_Back.Text;
            string _priceG = tb_Price_PurchaseRebate.Text;
            string _priceH = tb_Price_Freight.Text;

            var data = new ECDItem_Month
            {
                Data_ID = new Guid(guid),
                RefType = _RefType,
                RefMall = _RefMall,
                setYear = _setYear,
                setMonth = _setMonth,
                Price_Income = string.IsNullOrWhiteSpace(_priceA) ? 0 : Convert.ToDouble(_priceA),
                Price_SalesRebate = string.IsNullOrWhiteSpace(_priceB) ? 0 : Convert.ToDouble(_priceB),
                Price_Cost = string.IsNullOrWhiteSpace(_priceC) ? 0 : Convert.ToDouble(_priceC),
                Price_Profit = string.IsNullOrWhiteSpace(_priceD) ? 0 : Convert.ToDouble(_priceD),
                Price_Purchase = string.IsNullOrWhiteSpace(_priceE) ? 0 : Convert.ToDouble(_priceE),
                Price_Back = string.IsNullOrWhiteSpace(_priceF) ? 0 : Convert.ToDouble(_priceF),
                Price_PurchaseRebate = string.IsNullOrWhiteSpace(_priceG) ? 0 : Convert.ToDouble(_priceG),
                Price_Freight = string.IsNullOrWhiteSpace(_priceH) ? 0 : Convert.ToDouble(_priceH),
                Create_Who = fn_Param.CurrentUser
            };

            //----- 方法:判斷重複 -----
            if (_data.CheckECD_Month(data, out ErrMsg) > 0)
            {
                CustomExtension.AlertMsg("資料重複新增", thisPage);
                return;
            }

            //----- 方法:新增資料 -----
            if (!_data.CreateECD_Month(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }
            else
            {
                //更新本頁Url
                string thisNewUrl = "{0}/Month/Edit/{1}".FormatThis(FuncPath(), guid);

                if (type.Equals("1"))
                {
                    //導向本頁
                    Response.Redirect(thisNewUrl);
                }
                else
                {
                    //導向列表頁
                    Response.Redirect(Page_SearchUrl);
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    /// <summary>
    /// 資料修改
    /// </summary>
    /// <param name="type">1:Stay;2:List</param>
    private void Edit_Data(string type)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 設定:資料欄位 -----
        Int16 _RefType = Convert.ToInt16(Req_TypeID);
        Int32 _RefMall = Convert.ToInt32(ddl_Mall.SelectedValue);
        Int32 _setYear = Convert.ToInt32(ddl_Year.SelectedValue);
        Int32 _setMonth = Convert.ToInt32(ddl_Month.SelectedValue);
        string _priceA = tb_Price_Income.Text;
        string _priceB = tb_Price_SalesRebate.Text;
        string _priceC = tb_Price_Cost.Text;
        string _priceD = tb_Price_Profit.Text;
        string _priceE = tb_Price_Purchase.Text;
        string _priceF = tb_Price_Back.Text;
        string _priceG = tb_Price_PurchaseRebate.Text;
        string _priceH = tb_Price_Freight.Text;

        var data = new ECDItem_Month
        {
            Data_ID = new Guid(Req_DataID),
            RefType = _RefType,
            RefMall = _RefMall,
            setYear = _setYear,
            setMonth = _setMonth,
            Price_Income = string.IsNullOrWhiteSpace(_priceA) ? 0 : Convert.ToDouble(_priceA),
            Price_SalesRebate = string.IsNullOrWhiteSpace(_priceB) ? 0 : Convert.ToDouble(_priceB),
            Price_Cost = string.IsNullOrWhiteSpace(_priceC) ? 0 : Convert.ToDouble(_priceC),
            Price_Profit = string.IsNullOrWhiteSpace(_priceD) ? 0 : Convert.ToDouble(_priceD),
            Price_Purchase = string.IsNullOrWhiteSpace(_priceE) ? 0 : Convert.ToDouble(_priceE),
            Price_Back = string.IsNullOrWhiteSpace(_priceF) ? 0 : Convert.ToDouble(_priceF),
            Price_PurchaseRebate = string.IsNullOrWhiteSpace(_priceG) ? 0 : Convert.ToDouble(_priceG),
            Price_Freight = string.IsNullOrWhiteSpace(_priceH) ? 0 : Convert.ToDouble(_priceH),
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateECD_Month(data, out ErrMsg))
        {
            CustomExtension.AlertMsg("更新失敗", thisPage);
            return;
        }
        else
        {
            if (type.Equals("1"))
            {
                //導向本頁
                Response.Redirect(thisPage);
            }
            else
            {
                //導向列表頁
                Response.Redirect(Page_SearchUrl);
            }
        }

    }


    //SAVE-基本資料
    protected void btn_SaveStay_Click(object sender, EventArgs e)
    {
        doSave("1");
    }
    protected void btn_Save_Click(object sender, EventArgs e)
    {
        doSave("2");
    }

    private void doSave(string type)
    {
        string errTxt = "";

        if (string.IsNullOrWhiteSpace(ddl_Mall.SelectedValue))
        {
            errTxt += "平台未選擇\\n";
        }


        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        /* 執行新增/更新 */
        if (string.IsNullOrEmpty(hf_DataID.Value))
        {
            Add_Data(type);
        }
        else
        {
            Edit_Data(type);
        }
    }

    #endregion


    #region -- 單身:促銷費用 --

    /// <summary>
    /// 顯示單身編輯資料
    /// </summary>
    /// <param name="id"></param>
    private void Edit_Detail(string id)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", id);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetECD_MonthDT(search, out ErrMsg).Take(1).FirstOrDefault();

        //填入資料
        hf_DetailID.Value = query.Data_ID.ToString(); //單身編號
        tb_RecordDate.Text = query.RecordDate;
        ddl_RecordType.SelectedValue = query.RecordType.ToString();
        tb_RecordMoney.Text = query.RecordMoney.ToString();
        tb_CheckDate.Text = query.CheckDate;
        tb_CheckMoney.Text = query.CheckMoney.ToString();
        tb_CheckInvoiceDate.Text = query.CheckInvoiceDate;

        btn_SaveDetail.Text = "確認修改";

        //release
        _data = null;

    }

    /// <summary>
    /// 單身List
    /// </summary>
    private void LookupData_Detail()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("ParentID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetECD_MonthDT(search, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        lv_Detail.DataSource = query;
        lv_Detail.DataBind();

        //Release
        _data = null;
        query = null;
    }

    /// <summary>
    /// 新增/修改促銷費用
    /// </summary>
    protected void btn_SaveDetail_Click(object sender, EventArgs e)
    {
        string errTxt = "";
        string _DataID = hf_DetailID.Value;
        string _ParentID = Req_DataID;
        string _RecordDate = tb_RecordDate.Text;
        string _RecordType = ddl_RecordType.SelectedValue;
        string _RecordMoney = tb_RecordMoney.Text;
        string _CheckDate = tb_CheckDate.Text;
        string _CheckMoney = tb_CheckMoney.Text;
        string _CheckInvoiceDate = tb_CheckInvoiceDate.Text;

        if (string.IsNullOrWhiteSpace(_RecordDate))
        {
            errTxt += "記錄日期未填寫\\n";
        }
        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }


        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 設定:資料欄位 -----
        var data = new ECDItem_MonthDT
        {
            Data_ID = string.IsNullOrWhiteSpace(_DataID) ? 0 : Convert.ToInt64(_DataID),
            Parent_ID = new Guid(_ParentID),
            RecordDate = _RecordDate.ToDateString("yyyy/MM/dd"),
            RecordType = Convert.ToInt32(_RecordType),
            RecordMoney = Convert.ToDouble(_RecordMoney),
            CheckDate = _CheckDate,
            //對帳金額=NULL, 預設取RecordMoney
            CheckMoney = string.IsNullOrWhiteSpace(_CheckMoney) ? Convert.ToDouble(_RecordMoney) : Convert.ToDouble(_CheckMoney),
            CheckInvoiceDate = _CheckInvoiceDate,
            Create_Who = fn_Param.CurrentUser,
            Update_Who = fn_Param.CurrentUser
        };

        /* 執行新增/更新 */
        if (string.IsNullOrWhiteSpace(_DataID))
        {
            //Add
            if (!_data.CreateECD_MonthDT(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", thisPage);
                return;
            }
        }
        else
        {
            //Edit
            if (!_data.UpdateECD_MonthDT(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("更新失敗", thisPage);
                return;
            }
        }

        //release
        _data = null;
        //redirect
        Response.Redirect(thisPage + "#promoData");
    }

    #endregion

    #region -- 附加功能 --
    /// <summary>
    /// 取得類別資料 
    /// </summary>
    /// <param name="typeID">typeID</param>
    /// <param name="ddl">下拉選單object</param>
    private void Get_ClassList(string typeID, DropDownList ddl)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetEC_RefMall(typeID, Req_Lang, out ErrMsg);


        //----- 資料整理 -----
        ddl.Items.Clear();

        foreach (var item in query)
        {
            ddl.Items.Add(new ListItem(item.Label, item.ID.ToString()));
        }

        query = null;
    }

    protected void CreateMenu_Year(DropDownList item)
    {
        int currYear = DateTime.Now.Year;
        int prevYear = currYear - 5;
        int nextYear = currYear;

        item.Items.Clear();
        for (int itemY = prevYear; itemY <= nextYear; itemY++)
        {
            item.Items.Add(new ListItem(itemY.ToString(), itemY.ToString()));
        }

        //預設值
        item.SelectedValue = currYear.ToString();
    }

    protected void CreateMenu_Month(DropDownList item)
    {
        item.Items.Clear();
        for (int row = 1; row <= 12; row++)
        {
            item.Items.Add(new ListItem(row.ToString(), row.ToString()));
        }
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
    /// 取得網址參數 - TypeID
    /// </summary>
    private string _Req_TypeID;
    public string Req_TypeID
    {
        get
        {
            String DataID = Page.RouteData.Values["typeID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
        }
        set
        {
            _Req_TypeID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/eCommerceData/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_TypeID);
    }

    #endregion


    #region -- 傳遞參數 --

    /// <summary>
    /// 取得傳遞參數 - 資料編號
    /// </summary>
    private string _Req_DataID;
    public string Req_DataID
    {
        get
        {
            String DataID = Page.RouteData.Values["id"].ToString();

            return DataID;
        }
        set
        {
            _Req_DataID = value;
        }
    }

    /// <summary>
    /// Sub ID單身編號
    /// </summary>
    public string Req_SubID
    {
        get
        {
            String _data = Request.QueryString["sub"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_SubID = value;
        }
    }
    private string _Req_SubID;

    /// <summary>
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/Month/Edit/{1}".FormatThis(FuncPath(), Req_DataID);
        }
        set
        {
            _thisPage = value;
        }
    }


    /// <summary>
    /// 設定參數 - 列表頁Url
    /// </summary>
    private string _Page_SearchUrl;
    public string Page_SearchUrl
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("HomeList_ECMonth");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() + "/Month" : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion


}
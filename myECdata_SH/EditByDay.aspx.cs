﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Models;
using PKLib_Method.Methods;
using SH_ecData.Controllers;

public partial class myECdata_SH_EditByDay : SecurityCheck
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
            string typeName = refType + " (每日)";

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
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3804");
                    break;

                default:
                    //科玩
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3808");
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
                Get_ClassList(Req_TypeID, ddl_Mall);

                //[參數判斷] - 資料編號
                if (Req_DataID.Equals("new"))
                {
                    //填入預設日期
                    tb_setDate.Text = DateTime.Today.AddDays(-1).ToString().ToDateString("yyyy/MM/dd");
                }
                else
                {
                    //載入資料
                    LookupData();
                }

                /*
                  判斷類型,鎖定指定欄位
                */
                switch (Req_TypeID)
                {
                    case "2":
                        //科玩
                        tb_Price_RefProfit.Enabled = false;
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
        SH_ecDataRepository _data = new SH_ecDataRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOneECD_byDate(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }


        #region >> 欄位填寫 <<

        hf_DataID.Value = query.Data_ID.ToString();
        tb_setDate.Text = query.setDate.ToString();
        ddl_Mall.SelectedValue = query.RefMall.ToString();
        tb_Price_RefSales.Text = query.Price_RefSales.ToString();
        tb_Price_RefProfit.Text = query.Price_RefProfit.ToString();
        tb_Price_RealSales.Text = query.Price_RealSales.ToString();
        tb_Price_RealProfit.Text = query.Price_RealProfit.ToString();
        tb_Price_OrderPrice.Text = query.Price_OrderPrice.ToString();
        tb_Price_ROI.Text = query.Price_ROI.ToString();
        tb_Price_ClickCost.Text = query.Price_ClickCost.ToString();
        tb_Price_Adv1.Text = query.Price_Adv1.ToString();
        tb_Price_Adv2.Text = query.Price_Adv2.ToString();
        tb_Price_Adv3.Text = query.Price_Adv3.ToString();
        //維護資訊
        info_Creater.Text = query.Create_Name;
        info_CreateTime.Text = query.Create_Time;
        info_Updater.Text = query.Update_Name;
        info_UpdateTime.Text = query.Update_Time;

        #endregion

        #region >> 欄位控制 <<

        tb_setDate.Enabled = false;
        ddl_Mall.Enabled = false;
        ph_AddNext.Visible = true;
        
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
            SH_ecDataRepository _data = new SH_ecDataRepository();

            //----- 設定:資料欄位 -----
            //產生Guid
            string guid = CustomExtension.GetGuid();
            Int16 _RefType = Convert.ToInt16(Req_TypeID);
            Int32 _RefMall = Convert.ToInt32(ddl_Mall.SelectedValue);
            string _setDate = tb_setDate.Text.ToDateString("yyyy/MM/dd");
            string _priceA = tb_Price_RefSales.Text;
            string _priceB = tb_Price_RefProfit.Text;
            string _priceC = tb_Price_RealSales.Text;
            string _priceD = tb_Price_RealProfit.Text;
            string _priceE = tb_Price_OrderPrice.Text;
            string _priceF = tb_Price_ROI.Text;
            string _priceG = tb_Price_ClickCost.Text;
            string _priceH = tb_Price_Adv1.Text;
            string _priceI = tb_Price_Adv2.Text;
            string _priceJ = tb_Price_Adv3.Text;

            var data = new ECDItem_Date
            {
                Data_ID = new Guid(guid),
                RefType = _RefType,
                RefMall = _RefMall,
                setDate = _setDate,
                Price_RefSales = string.IsNullOrWhiteSpace(_priceA) ? 0 : Convert.ToDouble(_priceA),
                Price_RefProfit = string.IsNullOrWhiteSpace(_priceB) ? 0 : Convert.ToDouble(_priceB),
                Price_RealSales = string.IsNullOrWhiteSpace(_priceC) ? 0 : Convert.ToDouble(_priceC),
                Price_RealProfit = string.IsNullOrWhiteSpace(_priceD) ? 0 : Convert.ToDouble(_priceD),
                Price_OrderPrice = string.IsNullOrWhiteSpace(_priceE) ? 0 : Convert.ToDouble(_priceE),
                Price_ROI = string.IsNullOrWhiteSpace(_priceF) ? 0 : Convert.ToDouble(_priceF),
                Price_ClickCost = string.IsNullOrWhiteSpace(_priceG) ? 0 : Convert.ToDouble(_priceG),
                Price_Adv1 = string.IsNullOrWhiteSpace(_priceH) ? 0 : Convert.ToDouble(_priceH),
                Price_Adv2 = string.IsNullOrWhiteSpace(_priceI) ? 0 : Convert.ToDouble(_priceI),
                Price_Adv3 = string.IsNullOrWhiteSpace(_priceJ) ? 0 : Convert.ToDouble(_priceJ),
                Create_Who = fn_Param.CurrentUser
            };

            //----- 方法:判斷重複 -----
            if (_data.CheckECD_Date(data, out ErrMsg) > 0)
            {
                CustomExtension.AlertMsg("資料重複新增", thisPage);
                return;
            }

            //----- 方法:新增資料 -----
            if (!_data.CreateECD_Date(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }
            else
            {
                //更新本頁Url
                string thisNewUrl = "{0}/Date/Edit/{1}".FormatThis(FuncPath(), guid);

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
        SH_ecDataRepository _data = new SH_ecDataRepository();

        //----- 設定:資料欄位 -----
        Int16 _RefType = Convert.ToInt16(Req_TypeID);
        Int32 _RefMall = Convert.ToInt32(ddl_Mall.SelectedValue);
        string _setDate = tb_setDate.Text.ToDateString("yyyy/MM/dd");
        string _priceA = tb_Price_RefSales.Text;
        string _priceB = tb_Price_RefProfit.Text;
        string _priceC = tb_Price_RealSales.Text;
        string _priceD = tb_Price_RealProfit.Text;
        string _priceE = tb_Price_OrderPrice.Text;
        string _priceF = tb_Price_ROI.Text;
        string _priceG = tb_Price_ClickCost.Text;
        string _priceH = tb_Price_Adv1.Text;
        string _priceI = tb_Price_Adv2.Text;
        string _priceJ = tb_Price_Adv3.Text;

        var data = new ECDItem_Date
        {
            Data_ID = new Guid(Req_DataID),
            RefType = _RefType,
            RefMall = _RefMall,
            setDate = _setDate,
            Price_RefSales = string.IsNullOrWhiteSpace(_priceA) ? 0 : Convert.ToDouble(_priceA),
            Price_RefProfit = string.IsNullOrWhiteSpace(_priceB) ? 0 : Convert.ToDouble(_priceB),
            Price_RealSales = string.IsNullOrWhiteSpace(_priceC) ? 0 : Convert.ToDouble(_priceC),
            Price_RealProfit = string.IsNullOrWhiteSpace(_priceD) ? 0 : Convert.ToDouble(_priceD),
            Price_OrderPrice = string.IsNullOrWhiteSpace(_priceE) ? 0 : Convert.ToDouble(_priceE),
            Price_ROI = string.IsNullOrWhiteSpace(_priceF) ? 0 : Convert.ToDouble(_priceF),
            Price_ClickCost = string.IsNullOrWhiteSpace(_priceG) ? 0 : Convert.ToDouble(_priceG),
            Price_Adv1 = string.IsNullOrWhiteSpace(_priceH) ? 0 : Convert.ToDouble(_priceH),
            Price_Adv2 = string.IsNullOrWhiteSpace(_priceI) ? 0 : Convert.ToDouble(_priceI),
            Price_Adv3 = string.IsNullOrWhiteSpace(_priceJ) ? 0 : Convert.ToDouble(_priceJ),
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateECD_Date(data, out ErrMsg))
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
        if (string.IsNullOrEmpty(this.hf_DataID.Value))
        {
            Add_Data(type);
        }
        else
        {
            Edit_Data(type);
        }
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
        SH_ecDataRepository _data = new SH_ecDataRepository();

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
        return "{0}{1}/{2}/ecData/{3}".FormatThis(
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
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/Date/Edit/{1}".FormatThis(FuncPath(), Req_DataID);
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
            string tempUrl = CustomExtension.getCookie("HomeList_ECDay");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() + "/Date" : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion


}
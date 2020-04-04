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

public partial class myECdata_SZ_EditByYear : SecurityCheck
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
            string typeName = refType + " (年度)";

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
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3742");
                    break;

                default:
                    //科玩
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3747");
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
                Get_ClassList(Req_TypeID, ddl_Mall);

                //[參數判斷] - 資料編號
                if (Req_DataID.Equals("new"))
                {
                    //do something
                }
                else
                {
                    //載入資料
                    LookupData();

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
        var query = _data.GetOneECD_byYear(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }


        #region >> 欄位填寫 <<

        hf_DataID.Value = query.Data_ID.ToString();
        ddl_Year.SelectedValue = query.setYear.ToString();
        ddl_Mall.SelectedValue = query.RefMall.ToString();
        tb_Price_Sales.Text = query.Price_Sales.ToString();
        tb_Price_Rebate.Text = query.Price_Rebate.ToString();
        //維護資訊
        info_Creater.Text = query.Create_Name;
        info_CreateTime.Text = query.Create_Time;
        info_Updater.Text = query.Update_Name;
        info_UpdateTime.Text = query.Update_Time;

        #endregion

        #region >> 欄位控制 <<

        ddl_Year.Enabled = false;
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
            string _priceA = tb_Price_Sales.Text;
            string _priceB = tb_Price_Rebate.Text;

            var data = new ECDItem_Year
            {
                Data_ID = new Guid(guid),
                RefType = _RefType,
                RefMall = _RefMall,
                setYear = _setYear,
                Price_Sales = string.IsNullOrWhiteSpace(_priceA) ? 0 : Convert.ToDouble(_priceA),
                Price_Rebate = string.IsNullOrWhiteSpace(_priceB) ? 0 : Convert.ToDouble(_priceB),
                Create_Who = fn_Param.CurrentUser
            };

            //----- 方法:判斷重複 -----
            if (_data.CheckECD_Year(data, out ErrMsg) > 0)
            {
                CustomExtension.AlertMsg("資料重複新增", thisPage);
                return;
            }

            //----- 方法:新增資料 -----
            if (!_data.CreateECD_Year(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }
            else
            {
                //更新本頁Url
                string thisNewUrl = "{0}/Year/Edit/{1}".FormatThis(FuncPath(), guid);

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
        string _priceA = tb_Price_Sales.Text;
        string _priceB = tb_Price_Rebate.Text;

        var data = new ECDItem_Year
        {
            Data_ID = new Guid(Req_DataID),
            RefType = _RefType,
            RefMall = _RefMall,
            setYear = _setYear,
            Price_Sales = string.IsNullOrWhiteSpace(_priceA) ? 0 : Convert.ToDouble(_priceA),
            Price_Rebate = string.IsNullOrWhiteSpace(_priceB) ? 0 : Convert.ToDouble(_priceB),
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateECD_Year(data, out ErrMsg))
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

        if (string.IsNullOrWhiteSpace(tb_Price_Sales.Text) || string.IsNullOrWhiteSpace(tb_Price_Rebate.Text))
        {
            errTxt += "目標空白\\n";
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
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/Year/Edit/{1}".FormatThis(FuncPath(), Req_DataID);
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
            string tempUrl = CustomExtension.getCookie("HomeList_ECYear");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() + "/Year" : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using AuthData.Controllers;
using AuthData.Models;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class mySystem_AuthByFunc : SecurityCheck
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //權限判斷
                if (false == fn_CheckAuth.Check(fn_Param.CurrentUser, "9102"))
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //Get Users (init)
                Get_DBList(this.ddl_DB, true, Param_dbID);

                //權限表
                if (string.IsNullOrEmpty(Param_dbID))
                {
                    this.pl_Msg1.Visible = true;
                    this.ph_treeJS.Visible = false;
                    this.ph_treeHtml.Visible = false;

                    this.ph_SettingBtn.Visible = false;
                }
                else
                {
                    this.pl_Msg1.Visible = false;   //提示訊息1
                    this.pl_Msg4.Visible = true;
                    this.ph_treeJS.Visible = true;  //TreeView script
                    this.ph_treeHtml.Visible = true; //選單html

                    this.ph_SettingBtn.Visible = true;   //設定權限按鈕

                }

            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 取得資料 --

    /// <summary>
    /// 取得資料庫列表
    /// </summary>
    /// <param name="menu"></param>
    /// <param name="showRoot"></param>
    private void Get_DBList(DropDownList menu, bool showRoot, string inputValue)
    {
        //----- 宣告:資料參數 -----
        ParamsRepository _data = new ParamsRepository();
        menu.Items.Clear();


        //----- 原始資料:取得資料 -----
        var query = _data.GetDBList(null);

        //index 0
        if (showRoot)
        {
            menu.Items.Add(new ListItem("選擇資料庫", ""));
        }

        //Item list
        foreach (var item in query)
        {
            //Item Name  
            menu.Items.Add(new ListItem("{0} ({1})".FormatThis(item.DB_Desc, item.DB_Name), item.UID.ToString()));
        }

        //check inputvalue
        if (!string.IsNullOrEmpty(inputValue))
        {
            menu.SelectedIndex = menu.Items.IndexOf(menu.Items.FindByValue(inputValue.ToString()));
        }

        //Release
        query = null;
    }

    #endregion


    #region -- 資料編輯 --

    protected void lbtn_Go_Click(object sender, EventArgs e)
    {
        //取得輸入值
        string db = this.ddl_DB.SelectedValue;

        //判斷是否為空值
        if (string.IsNullOrEmpty(db))
        {
            this.ph_Require.Visible = true;
            return;
        }

        //重新導向, 帶出權限表
        Response.Redirect("{0}{1}/AuthFunc/{2}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , db));
    }


    /// <summary>
    /// 設定權限
    /// </summary>
    /// 
    protected void btn_Setting_Click(object sender, EventArgs e)
    {
        //reset
        this.pl_Msg2.Visible = false;
        this.pl_Msg3.Visible = false;

        //----- 判斷 -----
        //[欄位檢查] - 權限編號
        string inputValue = this.tb_Values.Text;
        string inputValue_User = this.tb_Values_User.Text;
        if (string.IsNullOrEmpty(inputValue) || string.IsNullOrEmpty(inputValue_User))
        {
            this.pl_Msg2.Visible = true;
            return;
        }

        //[取得參數值] - 編號組合
        string[] strAry = Regex.Split(inputValue, @"\,{1}");
        var query = from el in strAry
                    select new
                    {
                        Val = el.ToString().Trim()
                    };

        //----- 宣告 -----
        List<AuthRel> dataList = new List<AuthRel>();
        foreach (var item in query)
        {
            //加入項目
            var data = new AuthRel
            {
                MenuID = item.Val
            };

            //將項目加入至集合
            dataList.Add(data);
        }

        string[] strAry_User = Regex.Split(inputValue_User, @"\,{1}");
        var query_User = from el in strAry_User
                         select new
                         {
                             Val = el.ToString().Trim()
                         };

        //----- 宣告 -----
        List<AuthRel> dataList_User = new List<AuthRel>();
        foreach (var item in query_User)
        {
            //加入項目
            var data = new AuthRel
            {
                UserID = item.Val
            };

            //將項目加入至集合
            dataList_User.Add(data);
        }


        //----- 宣告:資料參數 -----
        AuthRepository _data = new AuthRepository();

        //----- 方法:更新資料 -----
        if (false == _data.Update_byFunc(Param_dbID, dataList, dataList_User))
        {
            this.pl_Msg3.Visible = true;
            return;
        }
        else
        {
            //導向本頁
            Response.Redirect(thisPage);
        }
    }
    #endregion


    #region -- 參數設定 --
    /// <summary>
    /// 取得傳遞參數 - 語系
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
    /// 取得傳遞參數 - DB資料編號
    /// </summary>
    private string _Param_dbID;
    public string Param_dbID
    {
        get
        {
            String DataID = Page.RouteData.Values["dbid"].ToString();

            return DataID.ToLower().Equals("new") ? "" : DataID;
        }
        set
        {
            this._Param_dbID = value;
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
            return "{0}{1}/AuthFunc/{2}".FormatThis(
                fn_Param.WebUrl
                , Req_Lang
                , Param_dbID
                );
        }
        set
        {
            this._thisPage = value;
        }
    }

    #endregion


}
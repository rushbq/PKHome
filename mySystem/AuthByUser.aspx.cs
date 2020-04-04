using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using AuthData.Controllers;
using AuthData.Models;
using CustomController;
using PKLib_Data.Assets;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class mySystem_AuthByUser : SecurityCheck
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //權限判斷
                if (false == fn_CheckAuth.Check(fn_Param.CurrentUser, "9101"))
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //Get Dept (init)
                Get_DeptList(this.ddl_Dept, true);

                //Get Users (init)
                Get_UserList(this.ddl_Who, "", true, Param_thisID);

                //Get Users (init)
                Get_DBList(this.ddl_DB, true, Param_dbID);

                //權限表
                if (string.IsNullOrEmpty(Param_thisID))
                {
                    this.pl_Msg1.Visible = true;
                    this.ph_treeJS.Visible = false;
                    this.ph_treeHtml.Visible = false;

                    this.ph_SettingBtn.Visible = false;
                }
                else
                {
                    this.pl_Msg1.Visible = false;   //提示訊息1
                    this.ph_treeJS.Visible = true;  //TreeView script
                    this.ph_treeHtml.Visible = true; //選單html

                    this.ph_SettingBtn.Visible = true;   //設定權限按鈕

                    //帶出資料
                    Get_UserInfo(Param_thisID);
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
    /// 
    /// </summary>
    /// <param name="dataID"></param>
    private void Get_UserInfo(string dataID)
    {
        //----- 宣告:資料參數 -----
        UsersRepository _data = new UsersRepository();


        //----- Filter -----
        Dictionary<int, string> search = new Dictionary<int, string>();
        if (!string.IsNullOrEmpty(dataID))
        {
            search.Add((int)Common.UserSearch.Guid, dataID);
        }


        //----- 原始資料:取得資料 -----
        var query = _data.GetUsers(search, null).FirstOrDefault();

        if (query != null)
        {
            this.lt_UserInfo.Text = "<small>&nbsp;&nbsp;<i class=\"material-icons\">person_pin</i>&nbsp;{0}&nbsp;({1})</small>".FormatThis(
                query.ProfID
                , query.ProfName
                );
        }

        //Release
        query = null;

    }

    /// <summary>
    /// 取得部門列表
    /// </summary>
    /// <param name="menu"></param>
    /// <param name="showRoot"></param>
    private void Get_DeptList(DropDownListGP menu, bool showRoot)
    {
        //----- 宣告:資料參數 -----
        DeptsRepository _data = new DeptsRepository();
        menu.Items.Clear();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetDepts();

        //index 0
        if (showRoot)
        {
            menu.Items.Add(new ListItem("全公司", ""));
        }

        //Item list
        foreach (var item in query)
        {
            //判斷GP_Rank, 若為第一項, 則輸出群組名稱
            if (item.GP_Rank.Equals(1))
            {
                menu.AddItemGroup(item.GroupName);
            }

            //Item Name  
            menu.Items.Add(new ListItem("{0} ({1})".FormatThis(item.DeptName, item.DeptID), item.DeptID));
        }

        //Release
        query = null;
    }


    /// <summary>
    /// 取得人員列表
    /// </summary>
    /// <param name="menu">選單object</param>
    /// <param name="deptID">部門代號</param>
    /// <param name="showRoot">是否顯示root</param>
    private void Get_UserList(DropDownListGP menu, string deptID, bool showRoot, string inputValue)
    {
        //----- 宣告:資料參數 -----
        UsersRepository _users = new UsersRepository();
        menu.Items.Clear();


        //----- Filter:Dept -----
        Dictionary<int, string> deptSearch = new Dictionary<int, string>();
        if (!string.IsNullOrEmpty(deptID))
        {
            deptSearch.Add(1, deptID);
        }


        //----- 原始資料:取得資料 -----
        var query = _users.GetUsers(null, deptSearch);

        //index 0
        if (showRoot)
        {
            menu.Items.Add(new ListItem("選擇人員", ""));
        }

        //Item list
        foreach (var item in query)
        {
            //判斷GP_Rank, 若為第一項, 則輸出群組名稱
            if (item.GP_Rank.Equals(1))
            {
                menu.AddItemGroup(item.DeptName);
            }

            //Item Name  
            menu.Items.Add(new ListItem("{0} ({1})".FormatThis(item.ProfID, item.ProfName), item.ProfGuid));
        }

        //check inputvalue
        if (!string.IsNullOrEmpty(inputValue))
        {
            menu.SelectedIndex = menu.Items.IndexOf(menu.Items.FindByValue(inputValue.ToString()));
        }

        //Release
        query = null;
    }



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
        string userid = this.ddl_Who.SelectedValue;

        //判斷是否為空值
        if (string.IsNullOrEmpty(db) || string.IsNullOrEmpty(userid))
        {
            this.ph_Require.Visible = true;
            return;
        }

        //重新導向, 帶出權限表
        Response.Redirect("{0}{1}/AuthUser/{2}/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , db
            , userid));
    }

    /// <summary>
    /// 選擇部門選單 onChange
    /// </summary>
    protected void ddl_Dept_SelectedIndexChanged(object sender, EventArgs e)
    {
        string dept = this.ddl_Dept.SelectedValue;

        //重置人員選單
        Get_UserList(this.ddl_Who, dept, true, "");
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
        if (string.IsNullOrEmpty(inputValue))
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



        //----- 宣告:資料參數 -----
        AuthRepository _data = new AuthRepository();

        //----- 方法:更新資料 -----
        if (false == _data.Update_byUser(Param_dbID, dataList, Param_thisID))
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
    /// 取得傳遞參數 - 資料編號
    /// </summary>
    private string _Param_thisID;
    public string Param_thisID
    {
        get
        {
            String DataID = Page.RouteData.Values["id"].ToString();

            return DataID.ToLower().Equals("new") ? "" : DataID;
        }
        set
        {
            this._Param_thisID = value;
        }
    }


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
            return "{0}{1}/AuthUser/{2}/{3}".FormatThis(
                fn_Param.WebUrl
                , Req_Lang
                , Param_dbID
                , Param_thisID
                );
        }
        set
        {
            this._thisPage = value;
        }
    }

    #endregion


}
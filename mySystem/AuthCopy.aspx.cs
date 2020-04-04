using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using AuthData.Controllers;
using CustomController;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class mySystem_AuthCopy : SecurityCheck
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //權限判斷
                if (false == fn_CheckAuth.Check(fn_Param.CurrentUser, "9103"))
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //Get Dept (init)
                Get_DeptList(this.ddl_Dept, true);
                Get_DeptList(this.ddl_tarDept, true);

                //Get Users (init)
                Get_UserList(this.ddl_Who, "", true, "");
                Get_UserList(this.ddl_tarWho, "", true, "");

                //Get Users (init)
                Get_DBList(this.ddl_DB, true, "");


            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 取得資料 --

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

    /// <summary>
    /// 選擇部門選單 onChange
    /// </summary>
    protected void ddl_Dept_SelectedIndexChanged(object sender, EventArgs e)
    {
        string dept = this.ddl_Dept.SelectedValue;

        //重置人員選單
        Get_UserList(this.ddl_Who, dept, true, "");
    }
    protected void ddl_tarDept_SelectedIndexChanged(object sender, EventArgs e)
    {
        string dept = this.ddl_tarDept.SelectedValue;

        //重置人員選單
        Get_UserList(this.ddl_tarWho, dept, true, "");
    }

    /// <summary>
    /// 設定權限
    /// </summary>
    /// 
    protected void btn_Setting_Click(object sender, EventArgs e)
    {
        //reset
        this.pl_Msg.Visible = false;
        this.ph_Require.Visible = false;

        //----- 判斷 -----
        string db = this.ddl_DB.SelectedValue;
        string sourceUser = this.ddl_Who.SelectedValue;
        string targetUser = this.ddl_tarWho.SelectedValue;
        string sourceUserName = this.ddl_Who.SelectedItem.Text;
        string targetUserName = this.ddl_tarWho.SelectedItem.Text;

        if (string.IsNullOrEmpty(db) || string.IsNullOrEmpty(sourceUser) || string.IsNullOrEmpty(targetUser))
        {
            this.ph_Require.Visible = true;
            return;
        }

        //----- 宣告:資料參數 -----
        AuthRepository _data = new AuthRepository();

        //----- 方法:更新資料 -----
        if (false == _data.Update_Copy(db, sourceUser, targetUser))
        {
            this.pl_Msg.Visible = true;
            return;
        }
        else
        {
            //導向本頁
            //Response.Redirect(thisPage);
            this.ph_SetDone.Visible = true;
            this.lt_done_Time.Text = DateTime.Now.ToString().ToDateString("yyyy-MM-dd HH:mm");
            this.lt_done_Source.Text = sourceUserName;
            this.lt_done_Target.Text = targetUserName;
            return;
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
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}{1}/AuthCopy".FormatThis(
                fn_Param.WebUrl
                , Req_Lang
                );
        }
        set
        {
            this._thisPage = value;
        }
    }

    #endregion


}
using System;
using System.Web.UI.WebControls;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class mySystem_AuthSearch : SecurityCheck
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //權限判斷
                if (false == fn_CheckAuth.Check(fn_Param.CurrentUser, "9104"))
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //Get Users (init)
                Get_DBList(this.ddl_DB, true, Param_dbID);

                //權限表
                if (string.IsNullOrEmpty(Param_dbID))
                {
                    this.ph_treeJS.Visible = false;
                    this.ph_treeHtml.Visible = false;
                    this.ph_treeUser.Visible = false;
                    this.ph_treeUserJS.Visible = false;
                }
                else
                {
                    this.ph_treeJS.Visible = true;  //TreeView script
                    this.ph_treeHtml.Visible = true; //選單html
                }

                //依功能查詢的人員清單
                if (string.IsNullOrEmpty(Param_thisID))
                {
                    this.ph_treeUserJS.Visible = false;
                }
                else
                {
                    this.ph_treeUserJS.Visible = true;
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
        Response.Redirect("{0}{1}/AuthSearch/{2}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , db));
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
            return "{0}{1}/AuthSearch/{2}".FormatThis(
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
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;
using System.Linq;

public partial class myOpcsStatus_Search : SecurityCheck
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


                //[權限判斷] Start
                /* 
                 * 使用公司別代號，判斷對應的MENU ID
                 */
                bool isPass = false;
                string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

                switch (getCorpUid)
                {
                    case "3":
                        //上海寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4152");
                        break;


                    //case "2":
                    //    //深圳寶工
                    //    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4154");
                    //    break;

                    default:
                        //TW
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4151");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }
                //[權限判斷] End


                //取得公司別
                this.lt_CorpName.Text = fn_Param.GetCorpName(getCorpUid);

                //Table header
                this.lt_TableHeader.Text = GetTableHead();


                //設定母版的Menu(只有TW)
                if (Req_CompID.Equals("TW"))
                {
                    string url = "{0}{1}/{2}/OpcsStatus/{3}".FormatThis(fn_Param.WebUrl, Req_Lang, Req_RootID, Req_CompID);

                    Literal menu = (Literal)Page.Master.FindControl("lt_headerMenu");
                    menu.Text += "<a class=\"item {2}\" href=\"{0}/?dept={1}\">總表</a>".FormatThis(url, "", Req_DeptID.Equals("") ? "active" : "");
                    menu.Text += "<a class=\"item {2}\" href=\"{0}/?dept={1}\">採購部</a>".FormatThis(url, "151", Req_DeptID.Equals("151") ? "active" : "");
                    menu.Text += "<a class=\"item {2}\" href=\"{0}/?dept={1}\">生產部</a>".FormatThis(url, "150", Req_DeptID.Equals("150") ? "active" : "");
                    menu.Text += "<a class=\"item {2}\" href=\"{0}/?dept={1}\">資材部</a>".FormatThis(url, "190", Req_DeptID.Equals("190") ? "active" : "");
                }
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    /// <summary>
    /// 取得表頭欄位
    /// </summary>
    /// <returns></returns>
    private string GetTableHead()
    {
        string html = "";
        Menu4000Repository _data = new Menu4000Repository();

        //所有欄位
        List<Menu4000Repository.OpcsColumn> allCol = _data.SetOpcsTableHeader();

        //要顯示的欄位(對應不同部門)
        List<Menu4000Repository.OpcsDept> deptRel = _data.SetOpcsDept(Req_CompID, Req_DeptID);

        //整理
        var query = allCol
            .Where(fld => deptRel.Select(f => f.colID).Contains(fld.colID))
            .OrderBy(fld => fld.colSort);
        foreach (var item in query)
        {
            html += "<th>" + item.colName + "</th>";
        }

        return html;
    }


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
            if (Page.RouteData.Values["CompID"] == null)
            {
                return "";
            }

            String DataID = Page.RouteData.Values["CompID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "TW" : DataID;
        }
        set
        {
            this._Req_CompID = value;
        }
    }

    /// <summary>
    /// Dept ID
    /// </summary>
    private string _Req_DeptID;
    public string Req_DeptID
    {
        get
        {
            //if (Request["dept"] == null)
            //{
            //    return "";
            //}

            String DataID = Req_CompID.Equals("SH") ?
                "999"
                : (Request["dept"] == null) ? "" : Request["dept"].ToString();

            return DataID;
        }
        set
        {
            this._Req_DeptID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/OpcsStatus/{3}/?dept={4}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID
            , Req_DeptID);
    }

    #endregion
}
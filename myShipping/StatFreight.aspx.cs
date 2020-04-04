using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myShipping_StatFreight : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                #region --權限--
                //[權限判斷] Start
                /* 
                 * 使用公司別代號，判斷對應的MENU ID
                 */
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3701"); ;

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[權限判斷] End
                #endregion


                //*** 設定母版的Menu ***
                Literal menu = (Literal)Page.Master.FindControl("lt_headerMenu");
                menu.Text = fn_Menu.GetTopMenu_ShipFreight(Req_Lang, Req_RootID, Req_CompID, Req_Tab);


                //Get Data
                LookupDataList();

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
    private void LookupDataList()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipStat_Year(Req_CompID, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        this.lvDataList.DataSource = query;
        this.lvDataList.DataBind();


        //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
        if (query.Count() == 0)
        {
            this.ph_EmptyData.Visible = true;
            this.ph_Data.Visible = false;
        }
        else
        {
            this.ph_EmptyData.Visible = false;
            this.ph_Data.Visible = true;

        }

    }

    public string setBg(object inputValue)
    {
        //小計變色
        if (inputValue.ToString().Equals("99"))
        {
            return "negative";

        }

        return "";
    }
    
    /// <summary>
    /// 數字顏色格式化
    /// </summary>
    /// <param name="inputValue"></param>
    /// <returns></returns>
    public object showNumber(object inputValue)
    {
        if (inputValue == null)
        {
            return "";
        }

        if (inputValue.ToString().Equals("0"))
        {
            return "<span class=\"grey-text text-lighten-2\">{0}</span>".FormatThis(inputValue);

        }

        return inputValue;
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
            String DataID = Page.RouteData.Values["CompID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "SZ" : DataID;
        }
        set
        {
            this._Req_CompID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/ShipFreightStat_Y/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
    }

    #endregion


    #region -- 傳遞參數 --
    public string Req_Tab
    {
        get
        {
            string data = Request.QueryString["tab"] == null ? "3" : Request.QueryString["tab"].ToString();
            return data;
        }
        set
        {
            this._Req_Tab = value;
        }
    }
    private string _Req_Tab;

    #endregion

}
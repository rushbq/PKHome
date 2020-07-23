using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using AuthData.Controllers;
using AuthData.Models;
using PKLib_Method.Methods;

public partial class Site : System.Web.UI.MasterPage
{
    public string cdnUrl = fn_Param.CDNUrl;
    public string webUrl = fn_Param.WebUrl;
    public string webName = fn_Param.WebName;
    private const string AntiXsrfTokenKey = "__PKHomeToken987";
    private const string AntiXsrfUserNameKey = "__PKHome";
    private string _antiXsrfTokenValue;


    protected void Page_Init(object sender, EventArgs e)
    {
        // 下面的程式碼有助於防禦 XSRF 攻擊
        var requestCookie = Request.Cookies[AntiXsrfTokenKey];
        Guid requestCookieGuidValue;
        if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
        {
            // 使用 Cookie 中的 Anti-XSRF 權杖
            _antiXsrfTokenValue = requestCookie.Value;
            Page.ViewStateUserKey = _antiXsrfTokenValue;
        }
        else
        {
            // 產生新的防 XSRF 權杖並儲存到 cookie
            _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
            Page.ViewStateUserKey = _antiXsrfTokenValue;

            var responseCookie = new HttpCookie(AntiXsrfTokenKey)
            {
                HttpOnly = true,
                Value = _antiXsrfTokenValue
            };
            if (System.Web.Security.FormsAuthentication.RequireSSL && Request.IsSecureConnection)
            {
                responseCookie.Secure = true;
            }
            Response.Cookies.Set(responseCookie);
        }

        Page.PreLoad += master_Page_PreLoad;
    }

    protected void master_Page_PreLoad(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            // 設定 Anti-XSRF 權杖
            ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
            ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
        }
        else
        {
            // 驗證 Anti-XSRF 權杖
            if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
            {
                throw new InvalidOperationException("Anti-XSRF 權杖驗證失敗。");
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                //判斷 & 轉換語系
                Check_Lang();


                #region -- 取得選單 --

                //----- 宣告:資料參數 -----
                AuthRepository _data = new AuthRepository();

                //----- 原始資料:取得資料 -----
                var queryAllmenu = _data.GetUserMenu(fn_Param.CurrentUser);

                this.lt_RootMenu.Text = showMenu_Root(queryAllmenu);
                this.lt_SubMenus.Text = showMenu(queryAllmenu);

                #endregion

            }
            catch
            {
                throw;
            }
        }
    }


    /// <summary>
    /// 取得根目錄選單
    /// </summary>
    /// <param name="myMenu"></param>
    /// <returns></returns>
    private string showMenu_Root(IQueryable<Auth> myMenu)
    {
        StringBuilder html = new StringBuilder();

        var menus = myMenu
            .Where(m => m.Lv.Equals(1));

        foreach (var menu in menus)
        {
            html.Append("<li class=\"tab\"><a href=\"#tab{0}\">{1}</a></li>".FormatThis(
                menu.MenuID
                , menu.MenuName));
        }

        return html.ToString();
    }


    /// <summary>
    /// 顯示子選單
    /// </summary>
    /// <param name="myMenu"></param>
    /// <returns></returns>
    private string showMenu(IQueryable<Auth> myMenu)
    {
        StringBuilder html = new StringBuilder();

        //--// 第一層 //--
        var menuLv1 = myMenu
            .Where(m => m.Lv.Equals(1));
        foreach (var itemLv1 in menuLv1)
        {
            html.Append("<div id=\"tab{0}\" class=\"card-panel grey lighten-5\">".FormatThis(itemLv1.MenuID));
            html.Append("<blockquote><h5>{0}</h5></blockquote>".FormatThis(itemLv1.MenuName));
            html.Append("<ul class=\"collapsible popout\" data-collapsible=\"accordion\">");



            #region >> 第二層選單 <<

            var menuLv2 = myMenu
                .Where(m => m.Lv.Equals(2) && m.ParentID.Equals(itemLv1.MenuID.ToString()));
            foreach (var itemLv2 in menuLv2)
            {
                html.Append("<li>");

                html.Append(" <div class=\"collapsible-header green white-text\"><b>{0}<i class=\"material-icons right\">keyboard_arrow_down</i></b></div>"
                    .FormatThis(itemLv2.MenuName));



                #region >> 第三層選單 <<

                var menuLv3 = myMenu
                    .Where(m => m.Lv.Equals(3) && m.ParentID.Equals(itemLv2.MenuID.ToString()));
                foreach (var itemLv3 in menuLv3)
                {
                    html.Append(" <div class=\"collapsible-body\">");
                    html.Append("  <ul class=\"collection\">");
                    html.Append("   <li class=\"collection-item\">");

                    //判斷是否有Url
                    string url = itemLv3.Url;
                    if (string.IsNullOrEmpty(url))
                    {
                        html.Append("<h6 class=\"grey-text text-darken-1\">{0}<i class=\"material-icons right\">more_vert</i></h6>".FormatThis(itemLv3.MenuName));
                    }
                    else
                    {
                        string urlTarget = itemLv3.Target;

                        //判斷連結(redirect:舊EF的URL)
                        html.Append("<a href=\"{0}\" target=\"{2}\"><h6>{1}</h6></a>".FormatThis(
                            urlTarget.Equals("redirect")
                                ? url
                                : "{0}{1}/{2}".FormatThis(webUrl, Req_Lang, url)
                            , itemLv3.MenuName
                            , urlTarget.Equals("redirect") ? "_blank" : urlTarget));
                    }



                    #region >> 第四層選單 <<

                    var menuLv4 = myMenu
                     .Where(m => m.Lv.Equals(4) && m.ParentID.Equals(itemLv3.MenuID.ToString()));

                    if (menuLv4.Count() > 0)
                    {
                        html.Append("<div class=\"collection\">");
                    }
                    foreach (var itemLv4 in menuLv4)
                    {
                        string urlLv4 = itemLv4.Url;
                        string urlTarget = itemLv4.Target;

                        //判斷連結
                        html.Append("<a href=\"{0}\" class=\"collection-item\" target=\"{2}\">{1}</a>".FormatThis(
                            urlTarget.Equals("redirect")
                                ? urlLv4
                                : "{0}{1}/{2}".FormatThis(webUrl, Req_Lang, urlLv4)
                            , itemLv4.MenuName
                            , urlTarget.Equals("redirect") ? "_blank" : urlTarget
                            ));

                    }

                    if (menuLv4.Count() > 0)
                    {
                        html.Append("</div>");
                    }

                    #endregion


                    html.Append("   </li>");
                    html.Append("  </ul>");
                    html.Append(" </div>");
                }


                #endregion



                html.Append("</li>");
            }

            #endregion

            html.Append("</ul>");

            //close選單鈕
            html.Append("<div class=\"right-align\">");
            html.Append("<a class=\"btn-flat waves-effect waves-teal closeTab\">Close<i class=\"material-icons right\">close</i></a>");
            html.Append("</div>");

            html.AppendLine("</div>");

        }


        return html.ToString();
    }


    #region -- 語系處理 --

    public string LangName(string lang)
    {
        switch (lang.ToUpper())
        {
            case "CN":
                return "简体中文";

            default:
                return "繁體中文";
        }
    }

    /// <summary>
    /// 判斷 & 轉換語系
    /// </summary>
    private void Check_Lang()
    {
        //取得目前語系cookie
        HttpCookie cLang = Request.Cookies["PKHome_Lang"];
        //將傳來的參數,轉換成完整語系參數
        string langCode = fn_Language.Get_LangCode(Req_Lang);

        //判斷傳入語系是否與目前語系相同, 若不同則執行語系變更
        if (cLang != null)
        {
            if (!cLang.Value.ToUpper().Equals(langCode.ToUpper()))
            {
                //重新註冊cookie
                Response.Cookies.Remove("PKHome_Lang");
                Response.Cookies.Add(new HttpCookie("PKHome_Lang", langCode));
                Response.Cookies["PKHome_Lang"].Expires = DateTime.Now.AddYears(1);

                //語系變換
                System.Globalization.CultureInfo currentInfo = new System.Globalization.CultureInfo(langCode);
                System.Threading.Thread.CurrentThread.CurrentCulture = currentInfo;
                System.Threading.Thread.CurrentThread.CurrentUICulture = currentInfo;

                //redirect
                Response.Redirect(Request.Url.AbsoluteUri);
            }
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
    /// 瀏覽器Title
    /// </summary>
    private string _Param_WebTitle;
    public string Param_WebTitle
    {
        get
        {
            if (string.IsNullOrEmpty(Page.Title))
            {
                return webName;
            }
            else
            {
                return "{0} | {1}".FormatThis(Page.Title, webName);
            }
        }
        set
        {
            this._Param_WebTitle = value;
        }
    }

    #endregion
}

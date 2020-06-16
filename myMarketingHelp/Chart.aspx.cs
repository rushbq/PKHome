using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu2000Data.Controllers;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class myMarketingHelp_Chart : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            #region --權限--
            //[權限判斷] Start
            /* 
             * 使用公司別代號，判斷對應的MENU ID
             */
            bool isPass = false;
            string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

            switch (getCorpUid)
            {
                //case "3":
                //    //上海寶工
                //    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "xxxx");
                //    break;

                case "2":
                    //深圳寶工
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2452");

                    break;

                default:
                    //TW
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2451");

                    break;
            }

            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }

            //[權限判斷] End
            #endregion

            if (!IsPostBack)
            {
                /* 多語系設定 */
                Page.Title = GetLocalResourceObject("pageTitle").ToString();

                //取得公司別
                lt_CorpName.Text = fn_Param.GetCorpName(getCorpUid);

                //[產生選單] 處理者(行企)
                Get_Processer(filter_ProcWho, "");
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 附加功能 --

    /// <summary>
    /// 取得處理人員(指定部門)
    /// </summary>
    /// <param name="ddl"></param>
    /// <param name="reqValue">傳入已勾選(以逗號分隔)</param>
    private void Get_Processer(DropDownList ddl, string reqValue)
    {
        //----- Clear -----
        ddl.Items.Clear();

        //----- 宣告 -----
        UsersRepository _user = new UsersRepository();
        Dictionary<int, string> _dept = new Dictionary<int, string>();

        //----- 取得資料 -----
        if (Req_CompID.Equals("TW"))
        {
            //條件:台灣行企
            _dept.Add(1, "180");
        }
        else
        {
            //條件:深圳行企
            _dept.Add(1, "314");
        }

        //呼叫並回傳資料
        var getUsers = _user.GetUsers(null, _dept);
        //選單設定root
        ddl.Items.Add(new ListItem("處理人員", ""));
        //選單設定選項
        foreach (var item in getUsers)
        {
            ddl.Items.Add(new ListItem("{0} ({1})".FormatThis(item.ProfName, item.NickName), item.ProfGuid));
        }

        //判斷已選擇的值, 並設為selected
        if (!string.IsNullOrWhiteSpace(reqValue))
        {
            //將來源字串轉為陣列,以逗號為分隔
            string[] strAry = Regex.Split(reqValue, @"\,{1}");
            //使用LINQ整理資料
            var query = from el in strAry
                        select new
                        {
                            selectedVal = el.ToString()
                        };
            //使用迴圈方式,將選項設為selected
            for (int row = 0; row < ddl.Items.Count; row++)
            {
                foreach (var item in query)
                {
                    if (ddl.Items[row].Value.Equals(item.selectedVal))
                    {
                        ddl.Items[row].Selected = true;
                    }
                }
            }

        }

        getUsers = null;
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

            return DataID.ToLower().Equals("unknown") ? "TW" : DataID;
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
        return "{0}{1}/{2}/MarketingHelp/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
    }

    #endregion


    #region -- 傳遞參數 --

    /// <summary>
    /// 設定參數 - 列表頁Url
    /// </summary>
    private string _Page_SearchUrl;
    public string Page_SearchUrl
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("HomeList_MKHelp");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }
    
    #endregion

}
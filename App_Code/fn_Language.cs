using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// 取得語系
/// </summary>
public class fn_Language
{

    /// <summary>
    /// 目前語系 - Cookie
    /// 若Cookie不存在，自動帶預設語系 en-US
    /// </summary>
    public static string Web_Lang
    {
        get
        {
            return HttpContext.Current.Request.Cookies["PKHome_Lang"] != null ?
              HttpContext.Current.Request.Cookies["PKHome_Lang"].Value.ToString() :
              "zh-TW";
        }
        private set
        {
            _Web_Lang = value;
        }
    }
    private static string _Web_Lang;


    /// <summary>
    /// 參數用語系, "-" 改 "_"
    /// DB語系欄位需開成像 xxx_zh_TW 的欄位名
    /// </summary>
    public static string Param_Lang
    {
        get
        {
            return Web_Lang.Replace("-", "_");
        }
        private set
        {
            _Param_Lang = value;
        }
    }
    private static string _Param_Lang;

    /// <summary>
    /// 取得資料庫語系字串
    /// </summary>
    /// <param name="lang">tw/cn/en</param>
    /// <returns></returns>
    public static string Get_DBLangCode(string lang)
    {
        switch (lang.ToUpper())
        {
            case "TW":
                return "zh_TW";

            case "CN":
                return "zh_CN";

            default:
                return "en_US";
        }
    }

    /// <summary>
    /// 取得語系完整字串
    /// </summary>
    /// <param name="lang">tw/cn/en</param>
    /// <returns></returns>
    public static string Get_LangCode(string lang)
    {
        switch (lang.ToUpper())
        {
            case "EN":
                return "en-US";

            case "CN":
                return "zh-CN";

            default:
                return "zh-TW";
        }
    }

    /// <summary>
    /// 取得語系簡易字串
    /// </summary>
    /// <param name="langCode">zh-tw/en-us</param>
    /// <returns></returns>
    public static string Get_Lang(string langCode)
    {
        switch (langCode.ToUpper())
        {
            case "ZH-TW":
                return "tw";

            case "ZH-CN":
                return "cn";

            default:
                return "en";
        }
    }

}
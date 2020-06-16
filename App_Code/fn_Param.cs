using System.Collections.Generic;
using System.Linq;
using PKLib_Data.Assets;
using PKLib_Data.Controllers;

/// <summary>
/// 常用參數
/// </summary>
public class fn_Param
{
    /// <summary>
    /// 網站名稱
    /// </summary>
    public static string WebName
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["WebName"];
        }
        set
        {
            _WebName = value;
        }
    }
    private static string _WebName;


    /// <summary>
    /// 網站網址
    /// </summary>
    public static string WebUrl
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["WebUrl"];
        }
        set
        {
            _WebUrl = value;
        }
    }
    private static string _WebUrl;


    /// <summary>
    /// CDN網址
    /// </summary>
    public static string CDNUrl
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["CDNUrl"];
        }
        set
        {
            _CDNUrl = value;
        }
    }
    private static string _CDNUrl;


    /// <summary>
    /// API網址
    /// </summary>
    public static string ApiUrl
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["ApiUrl"];
        }
        set
        {
            _ApiUrl = value;
        }
    }
    private static string _ApiUrl;

    /// <summary>
    /// Select PDF 元件金鑰
    /// </summary>
    public static string PDF_Key
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["PDF_Key"];
        }
        set
        {
            _PDF_Key = value;
        }
    }
    private static string _PDF_Key;

    /// <summary>
    /// Ref網址
    /// </summary>
    public static string RefUrl
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["RefUrl"];
        }
        set
        {
            _RefUrl = value;
        }
    }
    private static string _RefUrl;


    /// <summary>
    /// 系統寄件者
    /// </summary>
    public static string SysMail_Sender
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["SysMail_Sender"];
        }
        set
        {
            _SysMail_Sender = value;
        }
    }
    private static string _SysMail_Sender;
    

    /// <summary>
    /// 目前使用者GUID
    /// </summary>
    public static string CurrentUser
    {
        get
        {
            var id = UnobtrusiveSession.Session["Login_GUID"];

            return (id == null) ? "" : id.ToString();
        }
        set
        {
            _CurrentUser = value;
        }
    }
    private static string _CurrentUser;


    /// <summary>
    /// 目前使用者工號
    /// </summary>
    public static string CurrentUserAccount
    {
        get
        {
            var id = UnobtrusiveSession.Session["Login_UserID"];

            return (id == null) ? "" : id.ToString();
        }
        set
        {
            _CurrentUserAccount = value;
        }
    }
    private static string _CurrentUserAccount;


    
    /// <summary>
    /// 依傳入的公司區域簡稱回傳對應的UID
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetCorpUID(string name)
    {
        switch (name.ToUpper())
        {
            case "SH":
                return "3";

            case "SZ":
                return "2";

            default:
                //TW
                return "1";
        }
    }

    /// <summary>
    /// 取得公司別對應名稱
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetCorpName(string id)
    {
        //----- 宣告:資料參數 -----
        ParamsRepository _data = new ParamsRepository();

        Dictionary<int, string> search = new Dictionary<int, string>();
        search.Add((int)Common.mySearch.DataID, id);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCorpList(search).FirstOrDefault();

        return query.Corp_Name;

    }


    /// <summary>
    /// 取得部門代號
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string GetDeptID(string id)
    {
        //----- 宣告:資料參數 -----
        UsersRepository _data = new UsersRepository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOne(id).FirstOrDefault();

        return query.DeptID;

    }

    /// <summary>
    /// FTP_Username
    /// </summary>
    public static string ftp_Username
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["FTP_Username"];
        }
        set
        {
            _ftp_Username = value;
        }
    }
    private static string _ftp_Username;

    /// <summary>
    /// Password
    /// </summary>
    public static string ftp_Password
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["FTP_Password"];
        }
        set
        {
            _ftp_Password = value;
        }
    }
    private static string _ftp_Password;

    /// <summary>
    /// ServerUrl
    /// </summary>
    public static string ftp_ServerUrl
    {
        get
        {
            return System.Web.Configuration.WebConfigurationManager.AppSettings["FTP_Url"];
        }
        set
        {
            _ftp_ServerUrl = value;
        }
    }
    private static string _ftp_ServerUrl;

}
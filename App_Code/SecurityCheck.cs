using System;
using System.Collections.Specialized;
using System.Security.Principal;
using System.Web;
using PKLib_Method.Methods;

/// <summary>
///   檢查Session是否過期，重新取得登入資訊
/// </summary>
public class SecurityCheck : System.Web.UI.Page
{
    protected override void OnLoad(System.EventArgs e)
    {
        try
        {
            //[檢查參數] Session是否已過期
            if (HttpContext.Current.Session["Login_GUID"] == null)
            {
                //清除Session
                Session.Clear();

                CheckAD_Auto();

                base.OnLoad(e);
            }
            else
            {
                base.OnLoad(e);
            }
        }
        catch (Exception)
        {
            throw;
        }

    }

    /// <summary>
    /// AD登入驗證 - 網域電腦登入後自動驗證
    /// </summary>
    private void CheckAD_Auto()
    {
        //取得登入相關資訊
        IPrincipal userPrincipal = HttpContext.Current.User;
        WindowsIdentity windowsId = userPrincipal.Identity as WindowsIdentity;
        if (windowsId == null)
        {
            //找不到此SID, 導向登入錯誤頁, (請先登入網域)
            Response.Redirect(ErrPage("請先登入網域"));

            return;
        }
        else
        {
            SecurityIdentifier sid = windowsId.User;
            //取得屬性值(Sid / DisplayName / AccountName / Guid / 帳戶類型)
            StringCollection listAttr = ADService.getAttributesFromSID(sid.Value);
            if (listAttr == null)
            {
                //找不到此SID, 導向登入錯誤頁, (帳號未建立或未登入網域)
                Response.Redirect(ErrPage("帳號未建立或未登入網域"));
                return;
            }
            else
            {
                //取得登入名稱
                UnobtrusiveSession.Session["Login_UserName"] = listAttr[1];

                //取得登入帳號
                UnobtrusiveSession.Session["Login_UserID"] = listAttr[2];

                //取得AD GUID
                UnobtrusiveSession.Session["Login_GUID"] = listAttr[3];
                
            }
        }
    }

    string ErrPage(string ErrMsg)
    {
        return "{0}Error/{1}/".FormatThis(fn_Param.WebUrl, HttpUtility.UrlEncode(ErrMsg));
    }
}
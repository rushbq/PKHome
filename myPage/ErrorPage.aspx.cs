using System;
using System.Web.UI;

public partial class myPage_ErrorPage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    /// <summary>
    /// 取得傳遞參數 - DB資料編號
    /// </summary>
    private string _Req_Msg;
    public string Req_Msg
    {
        get
        {
            return (Page.RouteData.Values["msg"] == null) ? "出了一點小錯誤~" : Page.RouteData.Values["msg"].ToString();
        }
        set
        {
            this._Req_Msg = value;
        }
    }
}
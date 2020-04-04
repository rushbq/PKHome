using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.UI;

/// <summary>
/// 取得Url參數值 (for Routing)
/// </summary>
/// <remarks>
/// 如何取得參數
/// Page.RouteData.Values["ID"]
/// </remarks>
public class CustomRouteHandler : IRouteHandler
{
    public string VirtualPath
    {
        get;
        private set;
    }

    public CustomRouteHandler(string virtualPath)
    {
        this.VirtualPath = virtualPath;
    }

    #region IRouteHandler Members

    public IHttpHandler GetHttpHandler(RequestContext requestContext)
    {
        //從ReqestContext對像中獲取URL參數，並把值寫到HttpContext的Items集合中供路由​​目標頁面使用
        foreach (var urlParm in requestContext.RouteData.Values)
        {
            requestContext.HttpContext.Items[urlParm.Key] = urlParm.Value;
        }

        var page = BuildManager.CreateInstanceFromVirtualPath(VirtualPath, typeof(Page)) as IHttpHandler;
        return page;
    }
    #endregion
}
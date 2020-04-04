<%@ WebHandler Language="C#" Class="Ashx_GetProdImg" %>

using System;
using System.Web;
using PKLib_Method.Methods;

/// <summary>
/// 帶出產品圖
/// </summary>
public class Ashx_GetProdImg : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        //System.Threading.Thread.Sleep(2000);

        try
        {
            context.Response.ContentType = "text/html";

            //[接收參數]
            string _ID = context.Request["id"];
            string _source = context.Request["src"];
            string _modelNo = context.Request["model"];

            /* Check */
            if (string.IsNullOrWhiteSpace(_ID))
            {
                context.Response.Write("<h4>參數錯誤...請重新查詢!</h4>");
                return;
            }

            //Img url
            string _url = fn_Param.RefUrl;
            if (_source.ToUpper().Equals("0_SYS"))
            {
                //帶產品中心圖片
                _url += "ProductPic/" + _modelNo + "/1/" + _ID;
            }
            else
            {
                //新品-自行上傳的圖片
                _url += "PKHome/ProductPlan/" + _ID;
            }

            //response
            context.Response.Write("<img class=\"ui centered large image\" src=\"{0}\">".FormatThis(
                _url
                ));

        }
        catch (Exception ex)
        {
            context.Response.Write("<h4>資料查詢時發生錯誤!!若持續看到此訊息,請聯絡系統管理人員.</h4>" + ex.Message.ToString());
        }

    }



    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
<%@ WebHandler Language="C#" Class="Ashx_GetData" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Menu2000Data.Controllers;
using PKLib_Method.Methods;

/// <summary>
/// 取得OPCS所有資料
/// </summary>
public class Ashx_GetData : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg = "";

        System.Threading.Thread.Sleep(1000);

        try
        {
            //ContentType
            context.Response.ContentType = "text/html";

            //[接收參數]
            string _id = context.Request["id"];

            if (string.IsNullOrWhiteSpace(_id))
            {
                context.Response.Write("Load fail..");
                return;
            }

            //----- 宣告:資料參數 -----
            Menu2000Repository _data = new Menu2000Repository();

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetMKHelpAssignList(_id);


            //----- 資料整理:顯示Html -----
            StringBuilder html = new StringBuilder();
            int row = 0;

            html.Append("<i class=\"horizontally flipped level up alternate icon\"></i>");

            foreach (var item in query)
            {
                html.Append("<div class=\"ui basic label\">{0}</div>".FormatThis(item.Who));

                row++;
            }

            //若無資料
            if (row.Equals(0))
            {
                html.Clear();
                html.Append("<p class=\"red-text\">查無資料....</p>");
            }

            query = null;

            //輸出Html
            context.Response.Write(html.ToString());

        }
        catch (Exception ex)
        {
            context.Response.Write("<p class=\"red-text\">資料查詢時發生錯誤!!</p>" + ex.Message.ToString() + ErrMsg);
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
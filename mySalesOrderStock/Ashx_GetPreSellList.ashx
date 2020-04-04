<%@ WebHandler Language="C#" Class="Ashx_GetPreSellList" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

/// <summary>
/// 預計銷明細
/// </summary>
public class Ashx_GetPreSellList : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        //System.Threading.Thread.Sleep(2000);

        try
        {
            context.Response.ContentType = "text/html";

            //[接收參數]
            string _CompID = context.Request["CompID"];
            string _ModelNo = context.Request["ModelNo"];


            /* Check */
            if (string.IsNullOrEmpty(_CompID))
            {
                context.Response.Write("<h4>參數錯誤...請重新查詢!</h4>");
                return;
            }

            if (CheckReqNull(_ModelNo))
            {
                context.Response.Write("<h4>篩選條件太少...請重新加入條件!</h4>");
                return;
            }

            //----- 宣告:資料參數 -----
            Menu3000Repository _data = new Menu3000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();
            DataTable DT = new DataTable();
            ArrayList aryStocktype = new ArrayList();

            //判斷公司別,加入對應庫別
            switch (_CompID)
            {
                case "SZ":
                    aryStocktype.Add("A01");
                    aryStocktype.Add("B01");
                    break;

                default:
                    aryStocktype.Add("01");
                    break;

            }

            //----- 方法:取得資料 -----
            var datalist = _data.GetPreSellItems(_CompID, _ModelNo, aryStocktype);
            if (datalist.Count() == 0)
            {
                context.Response.Write("<h4>查無資料</h4>");
            }
            else
            {
                StringBuilder html = new StringBuilder();

                foreach (var item in datalist)
                {
                    //*** 填入Html ***
                    html.Append("<tr>");

                    //庫別
                    html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(item.StockType));

                    //數量
                    html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(item.Qty));

                    html.Append("</tr>");
                }

                //output
                context.Response.Write(html.ToString());
            }


            _data = null;

        }
        catch (Exception ex)
        {
            context.Response.Write("<h4>資料查詢時發生錯誤!!若持續看到此訊息,請聯絡系統管理人員.</h4>" + ex.Message.ToString());
        }
    }


    private bool CheckReqNull(string reqVal)
    {
        return string.IsNullOrEmpty(reqVal);
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
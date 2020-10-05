<%@ WebHandler Language="C#" Class="Ashx_GetPreIn" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Linq;
using System.Data.SqlClient;
using System.Collections;
using System.Text.RegularExpressions;
using PKLib_Method.Methods;

/// <summary>
/// 取得預計進
/// </summary>
public class Ashx_GetPreIn : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //System.Threading.Thread.Sleep(2000);

        try
        {
            context.Response.ContentType = "text/html";

            //[接收參數]
            string _DBS = context.Request["DBS"];
            string _ID = context.Request["id"];
            string _stock = context.Request["stock"];

            /* Check */
            if (string.IsNullOrWhiteSpace(_DBS) || string.IsNullOrWhiteSpace(_ID))
            {
                context.Response.Write("<h4>參數錯誤...請重新查詢!</h4>");
                return;
            }


            //拆解輸入字串(A_B_C)
            string[] aryStock = Regex.Split(_stock, "_");
            ArrayList aryLstStk = new ArrayList(aryStock);


            //----- 宣告:資料參數 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                string dbName = "";
                switch (_DBS.ToUpper())
                {
                    case "TW":
                        dbName = "prokit2";
                        break;

                    case "SH":
                        dbName = "SHPK2";
                        break;

                    case "SZ":
                        dbName = "ProUnion";
                        break;
                }

                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT TD007 AS StockType, RTRIM(TD004) AS ModelNo");
                sql.AppendLine("  , CONVERT(INT, (TD008 - TD015)) AS Qty, CONVERT(DATE, TD012, 111) AS WishDay");
                sql.AppendLine("  , TD001 AS FirstID, RTRIM(TD002) AS SubID");
                sql.AppendLine(" FROM [##dbname##].dbo.PURTD WITH (NOLOCK)");
                sql.AppendLine(" WHERE (TD016 = 'N') AND (TD018 = 'Y')");
                sql.AppendLine("  AND (TD004 = @modelno)");
                sql.AppendLine("  AND (TD007 IN ({0}))".FormatThis(CustomExtension.GetSQLParam(aryLstStk, "stock")));
                sql.AppendLine(" ORDER BY TD007");


                //SQL參數組成
                for (int row = 0; row < aryStock.Count(); row++)
                {
                    cmd.Parameters.AddWithValue("stock" + row, aryStock[row]);
                }

                //DB Name
                sql.Replace("##dbname##", dbName);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("modelno", _ID);


                using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                {
                    if (DT.Rows.Count == 0)
                    {
                        context.Response.Write("<h4>查無資料</h4>" + ErrMsg);
                    }
                    else
                    {
                        /*
                          Html Table
                        */
                        StringBuilder html = new StringBuilder();

                        for (int row = 0; row < DT.Rows.Count; row++)
                        {
                            //*** 填入Html ***
                            html.Append("<tr>");
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["StockType"]));
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["Qty"]));
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["WishDay"].ToString().ToDateString("yyyy/MM/dd")));
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["FirstID"]));
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["SubID"]));
                            html.Append("</tr>");
                        }

                        //output
                        context.Response.Write(html.ToString());
                    }

                }

            }
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
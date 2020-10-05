<%@ WebHandler Language="C#" Class="Ashx_GetPreSell" %>

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
/// 取得預計銷
/// </summary>
public class Ashx_GetPreSell : IHttpHandler
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
                sql.AppendLine(" SELECT DT.TD007 AS StockType, RTRIM(DT.TD004) AS ModelNo");
                sql.AppendLine("  , CONVERT(INT, (DT.TD008 - DT.TD009 + DT.TD024 - DT.TD025)) AS Qty, CONVERT(DATE, DT.TD013, 111) AS WishDay");
                sql.AppendLine("  , DT.TD001 AS FirstID, RTRIM(DT.TD002) AS SubID");
                sql.AppendLine("  , RTRIM(Cust.MA001) AS CustID, Cust.MA002 AS CustName");
                sql.AppendLine(" FROM [##dbname##].dbo.COPTC Base WITH (NOLOCK)");
                sql.AppendLine("  INNER JOIN [##dbname##].dbo.COPTD DT WITH (NOLOCK) ON Base.TC001 = DT.TD001 AND Base.TC002 = DT.TD002");
                sql.AppendLine("  INNER JOIN [##dbname##].dbo.COPMA Cust WITH (NOLOCK) ON Base.TC004 = Cust.MA001");
                sql.AppendLine(" WHERE (DT.TD016 = 'N') AND (DT.TD021 = 'Y')");
                sql.AppendLine("  AND (DT.TD004 = @modelno)");
                sql.AppendLine("  AND (DT.TD007 IN ({0}))".FormatThis(CustomExtension.GetSQLParam(aryLstStk, "stock")));
                sql.AppendLine(" ORDER BY DT.TD007");

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
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["CustID"]));
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["CustName"]));
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
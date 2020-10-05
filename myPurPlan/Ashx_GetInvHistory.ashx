<%@ WebHandler Language="C#" Class="Ashx_GetPreIn" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using PKLib_Method.Methods;

/// <summary>
/// 取得品號歷史異動
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
            string _nDays = context.Request["nDays"];


            /* Check */
            if (string.IsNullOrWhiteSpace(_DBS) || string.IsNullOrWhiteSpace(_ID))
            {
                context.Response.Write("<h4>參數錯誤...請重新查詢!</h4>");
                return;
            }

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
                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(LA001) AS ModelNo, CONVERT(DATE, LA004, 111) AS InvDay");
                sql.AppendLine("  , LA006 AS FirstID, RTRIM(LA007) AS SubID");
                sql.AppendLine("  , LA009 AS StockType, CONVERT(INT, LA011) AS Qty");
                sql.AppendLine("  , (CASE LA005 WHEN -1 THEN 'X' WHEN 1 THEN 'J' END) AS InvType");
                sql.AppendLine("  , RTRIM(LA010) AS Remark");
                sql.AppendLine(" FROM [##dbname##].dbo.INVLA WITH (NOLOCK)");
                sql.AppendLine(" WHERE (LA004 BETWEEN REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - @nDays, GETDATE()), 111),'/', '')");
                sql.AppendLine(" 		AND REPLACE(CONVERT(VARCHAR(10), DATEADD(day, 0, GETDATE()), 111), '/', ''))");
                sql.AppendLine("  AND (LA009 = @stock) AND (LA001 = @modelno)");
                sql.AppendLine(" ORDER BY LA004 DESC");

                //DB Name
                sql.Replace("##dbname##", dbName);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("nDays", Convert.ToInt16(_nDays));
                cmd.Parameters.AddWithValue("stock", _stock);
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
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["InvDay"].ToString().ToDateString("yyyy/MM/dd")));
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["FirstID"]));
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["SubID"]));
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["StockType"]));
                            html.Append("<td class=\"right aligned\">{0}</td>".FormatThis(DT.Rows[row]["Qty"]));
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["InvType"]));
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["Remark"]));
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
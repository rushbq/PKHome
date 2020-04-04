<%@ WebHandler Language="C#" Class="Ashx_GetOpcsData" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using PKLib_Method.Methods;

/// <summary>
/// 取得ERP訂單單號
/// </summary>
public class Ashx_GetOpcsData : IHttpHandler
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
            string _SoID = context.Request["SoID"];


            /* Check */
            if (string.IsNullOrWhiteSpace(_DBS) || string.IsNullOrWhiteSpace(_SoID))
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
                }

                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT RTRIM(COPTD.TD001) Fid, RTRIM(COPTD.TD002) Sid");
                sql.AppendLine(" FROM [##dbname##].dbo.COPTD WITH(NOLOCK)");
                sql.AppendLine(" 	INNER JOIN [##dbname##].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
                sql.AppendLine(" WHERE (RTRIM(COPTH.TH001) + RTRIM(COPTH.TH002)) = @SoID");
                sql.AppendLine(" GROUP BY COPTD.TD001, COPTD.TD002");

                sql.Replace("##dbname##", dbName);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("SoID", _SoID);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
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
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["Fid"]));
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["Sid"]));
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
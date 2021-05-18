<%@ WebHandler Language="C#" Class="Ashx_GetOpcsUpdData" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using PKLib_Method.Methods;

/// <summary>
/// 取得ERP訂單變更單版次
/// </summary>
public class Ashx_GetOpcsUpdData : IHttpHandler
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
                        //dbName = "ERP_GPTEST";
                        break;

                    case "SH":
                        dbName = "SHPK2";
                        //dbName = "SHPK2TEST";
                        break;
                }

                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT RTRIM(TE001) AS SO_Fid, RTRIM(TE002) AS SO_Sid, RTRIM(TE003) AS SO_Ver");
                sql.AppendLine(" , RTRIM(TE001) + RTRIM(TE002) + RTRIM(TE003) AS ErpID");
                sql.AppendLine(" FROM [##dbname##].dbo.COPTE WITH(NOLOCK)");
                sql.AppendLine(" WHERE (RTRIM(TE001) + RTRIM(TE002) = @SoID)");
                sql.AppendLine(" ORDER BY 3 DESC");

                sql.Replace("##dbname##", dbName);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("SoID", _SoID);

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
                            html.Append("<td class=\"center aligned\">{0}-{1}</td>".FormatThis(DT.Rows[row]["SO_Fid"], DT.Rows[row]["SO_Sid"]));
                            html.Append("<td class=\"center aligned teal-text text-darken-2\"><b>{0}</b></td>".FormatThis(DT.Rows[row]["SO_Ver"]));
                            html.Append("<td class=\"center aligned\"><a href=\"{0}\" target=\"_blank\">下載</a></td>".FormatThis(
                                fn_Param.WebUrl + "myOpcsRemark/OPCS_PdfDW.aspx?dbs=" + _DBS.ToUpper() + "&id=" + DT.Rows[row]["ErpID"]
                                ));
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
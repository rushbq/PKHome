<%@ WebHandler Language="C#" Class="Ashx_GetProdDesc" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Data.SqlClient;
using PKLib_Method.Methods;

/// <summary>
/// 取得產品說明
/// </summary>
public class Ashx_GetProdDesc : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //System.Threading.Thread.Sleep(2000);

        try
        {
            context.Response.ContentType = "text/html";

            //[接收參數]
            string _id = context.Request["id"];


            /* Check */
            if (string.IsNullOrWhiteSpace(_id))
            {
                context.Response.Write("<h4>參數錯誤...請重新查詢!</h4>");
                return;
            }

            //----- 宣告:資料參數 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT TOP 1 ISNULL(Info.Info1, '') AS ProdDesc, ISNULL(Info.Info2, '') AS ProdFeature");
                sql.AppendLine(" FROM Prod_Info AS Info");
                sql.AppendLine(" WHERE (Info.Model_No = @ModelNo) AND (UPPER(Info.Lang) = 'ZH-TW')");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ModelNo", _id);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Product, out ErrMsg))
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

                        //*** 填入Html ***
                        html.Append("<h4 class=\"ui horizontal divider header\">說明</h4>");
                        html.Append("<div>{0}</div>".FormatThis(DT.Rows[0]["ProdDesc"]));
                        html.Append("<h4 class=\"ui horizontal divider header\">特性</h4>");
                        html.Append("<div>{0}</div>".FormatThis(DT.Rows[0]["ProdFeature"]));

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
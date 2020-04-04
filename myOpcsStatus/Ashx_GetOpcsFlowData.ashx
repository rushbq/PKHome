<%@ WebHandler Language="C#" Class="Ashx_GetData" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

/// <summary>
/// 取得EFGP關聯資料 - 延遲出貨
/// </summary>
public class Ashx_GetData : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //System.Threading.Thread.Sleep(2000);

        try
        {
            context.Response.ContentType = "text/html";

            //[接收參數]
            string _CompID = context.Request["CompID"];
            string _OpcsNo = context.Request["OpcsNo"];


            /* Check */
            if (string.IsNullOrEmpty(_CompID))
            {
                context.Response.Write("<h4>參數錯誤...請重新查詢!</h4>");
                return;
            }

            if (CheckReqNull(_OpcsNo))
            {
                context.Response.Write("<h4>篩選條件太少...請重新加入條件!</h4>");
                return;
            }

            //----- 宣告:資料參數 -----
            Menu4000Repository _data = new Menu4000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();
            DataTable DT = new DataTable();

            //設定篩選參數
            search.Add("OpcsNo", _OpcsNo);


            //----- 方法:取得資料 -----
            using (DT = _data.GetOpcsFlow(_CompID, search, out ErrMsg))
            {
                if (DT.Rows.Count == 0)
                {
                    context.Response.Write("<h4>查無延遲出貨通知</h4>" + ErrMsg);
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

                        //填單日期
                        //html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["CreateDate"]));

                        //表單申請人
                        //html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["Creater"]));

                        //OPCS No
                        //html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["OpcsNo"]));

                        //客戶
                        //html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["Cust"]));

                        //品號
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["Model"]));

                        //預交日
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["PreDate"]));

                        //延遲原因
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["Remk1"]));

                        //出貨指示
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["Remk2"]));

                        html.Append("</tr>");
                    }


                    //output
                    context.Response.Write(html.ToString());
                }
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
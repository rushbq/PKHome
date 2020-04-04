<%@ WebHandler Language="C#" Class="Ashx_GetData" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Menu1000Data.Controllers;
using PKLib_Method.Methods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 取得資料, Json格式回傳
/// 表單使用,單筆資料
/// </summary>
public class Ashx_GetData : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //System.Threading.Thread.Sleep(2000);

        try
        {
            //[接收參數] 自訂查詢
            string _ModelNo = context.Request["ModelNo"];
            string _Year = context.Request["Year"];
            string _DataID = context.Request["DataID"]; //新品資料ID


            //----- 宣告:資料參數 -----
            Menu1000Repository _data = new Menu1000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();
            DataTable DT = new DataTable();


            //----- 原始資料:條件篩選 -----
            //[查詢條件] - ModelNo
            if (!string.IsNullOrWhiteSpace(_ModelNo))
            {
                search.Add("ModelNo", _ModelNo);
            }

            //[查詢條件] - Year
            if (!string.IsNullOrWhiteSpace(_Year))
            {
                search.Add("Keyword", _Year);
            }

            //[查詢條件] - DataID
            if (!string.IsNullOrWhiteSpace(_DataID))
            {
                search.Add("DataID", _DataID);
            }

            //----- 方法:取得資料 -----
            var data = _data.GetProdPlanData(search, Convert.ToInt16(_Year), out ErrMsg);

            string Jdata = JsonConvert.SerializeObject(data, Formatting.Indented);


            //輸出Json
            context.Response.ContentType = "application/json";
            context.Response.Write(Jdata);

        }
        catch (Exception ex)
        {
            context.Response.Write("#fail#<h5>資料查詢時發生錯誤!!若持續看到此訊息,請聯絡系統管理人員.</h5>" + ex.Message.ToString());
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
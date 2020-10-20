<%@ WebHandler Language="C#" Class="GetData_ChartData_CCP" %>

using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Menu3000Data.Controllers;

public class GetData_ChartData_CCP : IHttpHandler
{
    /// <summary>
    /// 製物工單統計資料
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //waiting for 1 sec.
        System.Threading.Thread.Sleep(1000);

        //[接收參數] 查詢字串
        string ErrMsg = "";
        string _type = context.Request["type"];
        string _jobtype = context.Request["jobtype"];
        string _sDate = context.Request["sdate"];
        string _eDate = context.Request["edate"];

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            search.Add("sDate", _sDate);
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            search.Add("eDate", _eDate);
        }
      

        //----- 原始資料:取得所有資料 -----
        var results = _data.GetCCP_ChartData(_type, _jobtype, search, out ErrMsg)
                .Select(fld =>
                    new
                    {
                        Label = fld.Label,
                        Cnt = fld.Cnt
                    });

        var data = new { results };


        //----- 資料整理:序列化 ----- 
        string jdata = JsonConvert.SerializeObject(data, Formatting.None);

        /*
         * [回傳格式] - Json
         * results：資料
         */

        //輸出Json
        context.Response.ContentType = "application/json";
        context.Response.Write(jdata);


    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
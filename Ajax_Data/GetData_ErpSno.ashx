<%@ WebHandler Language="C#" Class="GetData_ErpSno" %>

using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public class GetData_ErpSno : IHttpHandler
{
    /// <summary>
    /// 取得ERP單號資料(Ajax)
    /// 使用Semantic UI的Search UI
    /// 查詢未關聯的單號
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["q"];
        string _compID = context.Request["comp"];
        string _custID = context.Request["custID"];
        string ErrMsg = "";

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add("Keyword", searchVal);
        }
        //單據日,若為空則帶30日內
        DateTime dtNow = DateTime.Now;
        search.Add("sDate", dtNow.AddDays(-30).ToString().ToDateString("yyyyMMdd"));
        search.Add("eDate", dtNow.ToString().ToDateString("yyyyMMdd"));
        search.Add("Rel", "Y");
        search.Add("CustID", _custID);

        //----- 原始資料:取得所有資料 -----
        var results = _data.GetShipFreightList(_compID, search, null, out ErrMsg)
                .Select(fld =>
                    new
                    {
                        ID = fld.Erp_SO_FID + "-" + fld.Erp_SO_SID,
                        Label = fld.CustName
                    }).Take(50);

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
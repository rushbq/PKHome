<%@ WebHandler Language="C#" Class="GetData_ShipComp" %>

using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using ShipFreight_CN.Controllers;

public class GetData_ShipComp : IHttpHandler
{
    /// <summary>
    /// 取得物流公司資料(Ajax)
    /// 使用Semantic UI的Search UI
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["q"];
        string ErrMsg = "";

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add("Keyword", searchVal);
        }

        //----- 原始資料:取得所有資料 -----
        var results = _data.GetShipComp(search, out ErrMsg)
                .Select(fld =>
                    new
                    {
                        ID = fld.ID,
                        Label = fld.Label
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
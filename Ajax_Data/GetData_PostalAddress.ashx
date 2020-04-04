<%@ WebHandler Language="C#" Class="GetData_ShipData" %>

using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Menu2000Data.Controllers;

public class GetData_ShipData : IHttpHandler
{
    /// <summary>
    /// [郵寄登記]取得收件人資料(Ajax)
    /// 使用Semantic UI的Search UI
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["q"];
        string ErrMsg = "";

        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add("Keyword", searchVal);
        }

        //只帶自己的
        search.Add("Who", fn_Param.CurrentUser);

        //----- 原始資料:取得所有資料 -----
        var results = _data.GetPostalAddress(search, out ErrMsg)
                .Select(fld =>
                    new
                    {
                        ID = fld.CustomID,
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
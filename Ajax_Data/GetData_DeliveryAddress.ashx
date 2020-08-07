<%@ WebHandler Language="C#" Class="GetData_ShipData" %>

using System;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using DeliveryData.Controllers;
using System.Data;

public class GetData_ShipData : IHttpHandler
{
    /// <summary>
    /// [快遞貨運]取得收件人資料(Ajax)
    /// 使用Semantic UI的Search UI
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["q"];
        string ErrMsg = "";

        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add("Keyword", searchVal);
        }

        //只帶自己的
        search.Add("Who", fn_Param.CurrentUser);

        //----- 原始資料:取得所有資料 -----
        var results = _data.GetAddress(search, out ErrMsg).AsEnumerable()
                .Select(fld =>
                    new
                    {
                        Comp = fld.Field<string>("ToComp"),
                        Who = fld.Field<string>("ToWho"),
                        Addr = fld.Field<string>("ToAddr"),
                        Tel = fld.Field<string>("ToTel"),
                        Title = "[" + fld.Field<string>("ToComp") + "] " + fld.Field<string>("ToWho")
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
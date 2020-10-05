<%@ WebHandler Language="C#" Class="GetData_Supplier" %>

using System.Web;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using PKLib_Data.Controllers;
using PKLib_Data.Assets;

public class GetData_Supplier : IHttpHandler
{
    /// <summary>
    /// 取得供應商資料(Ajax)
    /// 使用Semantic UI的Search UI
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["q"];
        string corp = context.Request["corp"];
        string dbs = context.Request["dbs"];

        //dbs轉corp
        if (!string.IsNullOrWhiteSpace(dbs))
        {
            switch (dbs.ToUpper())
            {
                case "TW":
                    corp = "1";
                    break;

                case "SZ":
                    corp = "2";
                    break;

                case "SH":
                    corp = "3";
                    break;
            }
        }


        //----- 宣告:資料參數 -----
        SupplierRepository _data = new SupplierRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();
            

        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add((int)Common.mySearch.Keyword, searchVal);
        }
        
        if (!string.IsNullOrEmpty(corp))
        {
            search.Add((int)Common.mySearch.Corp, corp);
        }

        //----- 原始資料:取得所有資料 -----
        var results = _data.GetERPDataList(search)
                .Select(fld =>
                    new
                    {
                        ID = fld.ERP_SupID,
                        Label = "(" + fld.Corp_Name + ") " + fld.ERP_SupID,
                        FullLabel = "(" + fld.Corp_Name + ") " + fld.ERP_SupID + " - " + fld.ERP_SupName
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
<%@ WebHandler Language="C#" Class="GetData_Prod_v1" %>

using System.Web;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using PKLib_Data.Controllers;
using PKLib_Data.Assets;

public class GetData_Prod_v1 : IHttpHandler
{

    /// <summary>
    /// 取得產品資料(Ajax)
    /// 使用Semantic UI的Search UI
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["q"];
        bool showAll = context.Request["show"] == "Y" ? true : false;


        //----- 宣告:資料參數 -----
        ProductsRepository _product = new ProductsRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add((int)Common.ProdSearch.Keyword, searchVal);
        }


        //----- 原始資料:取得所有資料 -----
        var results = _product.GetProducts(Common.ColLang.TW, Common.DBSrc.Prokits, search, showAll)
                .OrderBy(o => o.ClassID)
                .ThenBy(o => o.ModelNo)
                .Select(fld =>
                    new
                    {
                        ID = fld.ModelNo,
                        Label = fld.Name_TW,
                        FullLabel = "(" + fld.ModelNo + ") " + fld.Name_TW,
                        Category = fld.ClassName_TW
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
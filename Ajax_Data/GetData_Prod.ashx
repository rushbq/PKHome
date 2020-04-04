<%@ WebHandler Language="C#" Class="GetData_Prod" %>

using System;
using System.Web;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using PKLib_Data.Controllers;
using PKLib_Data.Assets;

public class GetData_Prod : IHttpHandler
{

    /// <summary>
    /// 取得產品資料(Ajax)
    /// 使用jQuery的autocomplete UI
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["keyword"];
        
        
        //----- 宣告:資料參數 -----
        ProductsRepository _product = new ProductsRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add((int)Common.ProdSearch.Keyword, searchVal);
        }


        //----- 原始資料:取得所有資料 -----
        var query = _product.GetProducts(search);


        //----- 資料整理:顯示筆數 -----
        var prod = query
            .Select(fld =>
             new
             {
                 ID = fld.ModelNo,
                 Label = fld.Name_TW,
                 CategoryID = fld.ClassID,
                 Category = fld.ClassName_TW
             })
            .Take(100);

        
        //----- 資料整理:序列化 ----- 
        string jdata = JsonConvert.SerializeObject(prod, Formatting.Indented);

        /*
         * [回傳格式] - Json
         * data：資料
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
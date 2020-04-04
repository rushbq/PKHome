<%@ WebHandler Language="C#" Class="GetData_Depts" %>

using System.Web;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using PKLib_Data.Controllers;
using PKLib_Data.Assets;

public class GetData_Depts : IHttpHandler
{

    /// <summary>
    /// 取得部門資料(Ajax)
    /// 使用Semantic UI的Search UI
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["q"];


        //----- 宣告:資料參數 -----
        DeptsRepository _datalist = new DeptsRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add((int)Common.UserSearch.Keyword, searchVal);
        }

        //----- 原始資料:取得所有資料 -----
        var results = _datalist.GetDepts(search)
                .OrderBy(o => o.AreaSort)
                .ThenBy(o => o.Sort)
                .ThenBy(o => o.DeptID)
                .Select(fld =>
                    new
                    {
                        ID = fld.DeptID,
                        Label = fld.DeptName,
                        Email = fld.Email,
                        Category = fld.GroupName
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
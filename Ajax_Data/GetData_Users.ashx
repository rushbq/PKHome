<%@ WebHandler Language="C#" Class="GetData_Users" %>

using System.Web;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using PKLib_Data.Controllers;
using PKLib_Data.Assets;

public class GetData_Users : IHttpHandler
{

    /// <summary>
    /// 取得人員資料(Ajax)
    /// 使用Semantic UI的Search UI
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string searchVal = context.Request["q"];


        //----- 宣告:資料參數 -----
        UsersRepository _datalist = new UsersRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();


        //----- 原始資料:條件篩選 -----
        if (!string.IsNullOrEmpty(searchVal))
        {
            search.Add((int)Common.UserSearch.Keyword, searchVal);
        }

        //----- 原始資料:取得所有資料 -----
        var results = _datalist.GetUsers(search, null)
                .OrderBy(o => o.DeptID)
                .ThenBy(o => o.ProfID)
                .Select(fld =>
                    new
                    {
                        ID = fld.ProfID,
                        Label = fld.ProfName,
                        NickName = fld.NickName,
                        Email = fld.Email,
                        DeptID = fld.DeptID,
                        Guid = fld.ProfGuid,
                        Category = fld.DeptName
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
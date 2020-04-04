<%@ WebHandler Language="C#" Class="GetAuthList" %>

using System;
using System.Web;
using System.Linq;
using AuthData.Controllers;
using Newtonsoft.Json;

public class GetAuthList : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        //[接收參數] 查詢字串
        string search_ID = context.Request["id"];
        string search_DB = context.Request["db"];


        //----- 宣告:資料參數 -----
        AuthRepository _data = new AuthRepository();


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetDataList(search_DB, search_ID);


        //----- 資料整理:顯示欄位 -----
        /*
         * 使用jsTree 樹狀選單元件
         * 將資料整理成 jsTree json格式
         */
        var data = query
            .Select(fld =>
             new
             {
                 id = fld.MenuID,
                 parentID = fld.ParentID,
                 label = fld.MenuName,
                 selected = fld.ItemChecked,
                 open = fld.ItemChecked
             });


        //----- 資料整理:序列化 ----- 
        string jdata = JsonConvert.SerializeObject(data, Formatting.Indented);

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
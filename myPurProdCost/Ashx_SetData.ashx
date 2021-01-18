<%@ WebHandler Language="C#" Class="Ashx_SetData" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class Ashx_SetData : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //wait 1 sec.
        System.Threading.Thread.Sleep(1000);

        try
        {
            //[接收參數]
            string _jobType = context.Request["jobType"]; //ADD, DEL
            string _Model = context.Request["Model"];
            string _Qty = context.Request["Qty"];
            string _DBS = context.Request["DBS"];
            string _ItemID = context.Request["ItemID"];

            //Response ContentType
            context.Response.ContentType = "text/plain";


            //----- 宣告:資料參數 -----
            Menu4000Repository _data = new Menu4000Repository();

            if (_jobType.ToUpper().Equals("ADD"))
            {
                //----- 方法:更新資料 -----
                if (_data.CreatePackItem(_DBS, _Model, _ItemID, Convert.ToDecimal(_Qty), out ErrMsg))
                {
                    context.Response.Write("success");
                }
                else
                {
                    context.Response.Write("fail..." + ErrMsg);
                }
            }
            else
            {
                //----- 方法:刪除資料 -----
                if (_data.Delete_PackItem(_ItemID))
                {
                    context.Response.Write("success");
                }
                else
                {
                    context.Response.Write("fail...");
                }
            }


            _data = null;

        }
        catch (Exception ex)
        {
            context.Response.Write("例外錯誤!!" + ex.Message.ToString());
        }

    }


    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
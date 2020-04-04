<%@ WebHandler Language="C#" Class="Ashx_UpdateShipNo" %>

using System;
using System.Web;
using Menu3000Data.Controllers;
using Menu3000Data.Models;

public class Ashx_UpdateShipNo : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        //Delay 1 sec.
        System.Threading.Thread.Sleep(1000);

        string ErrMsg;

        //[接收參數]
        string _ParentID = context.Request["ParentID"];
        string _DataID = context.Request["DataID"];
        string _ShipNo = context.Request["ShipNo"];
        string _ShipCnt = context.Request["ShipCnt"];
        string _Pay1 = context.Request["Pay1"];
        string _Pay2 = context.Request["Pay2"];
        string _Pay3 = context.Request["Pay3"];

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 設定:資料欄位 -----
        var data = new ShipFreightDetail
        {
            Parent_ID = new Guid(_ParentID),
            Data_ID = Convert.ToInt16(_DataID),
            ShipNo = _ShipNo,
            ShipCnt = Convert.ToInt32(_ShipCnt),
            Pay1 = Convert.ToDouble(_Pay1),
            Pay2 = Convert.ToDouble(_Pay2),
            Pay3 = Convert.ToDouble(_Pay3)
        };

        //----- 方法:更新資料 -----
        context.Response.ContentType = "text/html";
        if (_data.UpdateShipFreightDetail(data, out ErrMsg))
        {
            context.Response.Write("success");
        }
        else
        {
            context.Response.Write("fail..." + ErrMsg);
        }

        _data = null;
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
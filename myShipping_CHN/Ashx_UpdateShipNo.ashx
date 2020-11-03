<%@ WebHandler Language="C#" Class="Ashx_UpdateShipNo" %>

using System;
using System.Web;
using ShipFreight_CN.Controllers;
using ShipFreight_CN.Models;

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
        string _Pay = context.Request["Pay"];

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

        //----- 設定:資料欄位 -----
        var data = new ShipFreightDetail
        {
            Parent_ID = new Guid(_ParentID),
            Data_ID = Convert.ToInt16(_DataID),
            ShipNo = _ShipNo,
            ShipCnt = Convert.ToInt32(_ShipCnt),
            Freight = Convert.ToDouble(_Pay),
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
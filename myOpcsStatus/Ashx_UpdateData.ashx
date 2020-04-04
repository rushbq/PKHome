<%@ WebHandler Language="C#" Class="Ashx_UpdateData" %>

using System;
using System.Web;
using Menu4000Data.Controllers;

/// <summary>
/// 資料儲存 - 資材理貨
/// </summary>
public class Ashx_UpdateData : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //延時
        System.Threading.Thread.Sleep(1000);

        //[接收參數]
        string _DataID = context.Request["DataID"];
        string _Act = context.Request["Act"];
        string _Type = context.Request["Type"];
        string _Area = "";

        //Check Null
        if (string.IsNullOrEmpty(_DataID))
        {
            context.Response.Write("fail:參數不可為空");
            return;
        }
        //ContentType
        context.Response.ContentType = "text/html";


        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();

        //判斷事件來源
        switch (_Type.ToUpper())
        {
            case "SHIP":
                //理貨區域
                switch (_Act.ToUpper())
                {
                    case "A":
                        _Area = "A";
                        break;

                    case "B":
                        _Area = "B";
                        break;

                    default:
                        _Area = "N";
                        break;
                }

                //----- 方法:更新資料 -----
                if (_data.Update_StockArea(_DataID, _Area, out ErrMsg))
                {
                    context.Response.Write("success");
                }
                else
                {
                    context.Response.Write("fail..." + ErrMsg);
                }

                break;

            case "BOX":
                //包裝資料
                //----- 方法:更新資料 -----
                if (_data.Update_BoxData(_DataID, _Act, out ErrMsg))
                {
                    context.Response.Write("success");
                }
                else
                {
                    context.Response.Write("fail..." + ErrMsg);
                }


                break;
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
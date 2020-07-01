<%@ WebHandler Language="C#" Class="Ashx_GetData" %>

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

/// <summary>
/// 取得BOM資料
/// 使用程式:Search.aspx
/// </summary>
public class Ashx_GetData : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //System.Threading.Thread.Sleep(2000);

        try
        {
            //[接收參數] 內建參數
            Int16 draw = Convert.ToInt16(context.Request["draw"]);      //為避免XSS攻擊，內建的控制
            Int16 start = Convert.ToInt16(context.Request["start"]);    //該頁的第一筆為所有資料的第n筆(從0開始)
            Int16 length = Convert.ToInt16(context.Request["length"]);  //每頁顯示筆數

            //[接收參數] UI內建查詢
            //string searchVal = context.Request["search[value]"];


            //[接收參數] 自訂查詢
            string _ModelNo = context.Request["ModelNo"];
            string _DBS = context.Request["DBS"];
            string _StopType = context.Request["StopType"];

            //----- 宣告:分頁參數 -----
            int TotalRow = 0;   //總筆數


            //----- 宣告:資料參數 -----
            Menu4000Repository _data = new Menu4000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();
            DataTable DT = new DataTable();
            int DataCnt;

            //----- 原始資料:條件篩選 -----
            //[查詢條件] - ModelNo
            if (!string.IsNullOrWhiteSpace(_ModelNo))
            {
                search.Add("ModelNo", _ModelNo);
            }

            //[查詢條件] - Stop
            if (!string.IsNullOrWhiteSpace(_StopType))
            {
                search.Add("Stop", "Stop");
            }

            //----- 方法:取得資料 -----
            using (DT = _data.GetBOMfilter(search, _DBS, start, length, out DataCnt, out ErrMsg))
            {
                //----- 資料整理:取得總筆數 -----
                TotalRow = DataCnt;

                //序列化DT
                string Jdata = JsonConvert.SerializeObject(DT, Formatting.Indented);

                //將Json加到data的屬性下
                JObject json = new JObject();

                json.Add(new JProperty("draw", draw));
                json.Add(new JProperty("recordsTotal", TotalRow));
                json.Add(new JProperty("recordsFiltered", TotalRow));
                json.Add(new JProperty("data", JsonConvert.DeserializeObject<JArray>(Jdata)));

                /*
                 * [回傳格式] - Json
                 * draw：內建函數(查詢次數)
                 * recordsTotal：篩選前的總資料數 (serverside模式)
                 * recordsFiltered：篩選後的總資料數 (serverside模式)
                 * data：該分頁所需要的資料
                 */

                //輸出Json
                context.Response.ContentType = "application/json";
                context.Response.Write(json);
            }

        }
        catch (Exception ex)
        {
            context.Response.Write("#fail#<h5>資料查詢時發生錯誤!!若持續看到此訊息,請聯絡系統管理人員.</h5>" + ex.Message.ToString());
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
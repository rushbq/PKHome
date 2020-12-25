<%@ WebHandler Language="C#" Class="Ashx_GetDataByComp" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 取得集團報價
/// 使用程式:SearchByCompany.aspx
/// </summary>
public class Ashx_GetDataByComp : IHttpHandler
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
            string _ClsID = context.Request["ClassID"];
            string _keyword = context.Request["Keyword"];
            string _itemNo = context.Request["ItemNo"];
            string _vol = context.Request["Vol"];
            string _dateType = context.Request["dateType"];
            string _sDate = context.Request["sDate"];
            string _eDate = context.Request["eDate"];


            //----- 宣告:分頁參數 -----
            int TotalRow = 0;   //總筆數


            //----- 宣告:資料參數 -----
            Menu3000Repository _data = new Menu3000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();
            DataTable DT = new DataTable();
            int DataCnt;

            //----- 原始資料:條件篩選 -----
            //[查詢條件] - Class
            if (!string.IsNullOrWhiteSpace(_ClsID) && (!_ClsID.Equals("-1")))
            {
                search.Add("ClassID", _ClsID);
            }

            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(_keyword))
            {
                search.Add("Keyword", _keyword);
            }

            //[查詢條件] - ItemNo
            if (!string.IsNullOrWhiteSpace(_itemNo))
            {
                search.Add("ItemNo", _itemNo);
            }

            //[查詢條件] - Vol
            if (!string.IsNullOrWhiteSpace(_vol) && (!_vol.Equals("-1")))
            {
                search.Add("Vol", _vol);
            }

            //[查詢條件] - Search Date
            switch (_dateType)
            {
                case "A":
                    if (!string.IsNullOrWhiteSpace(_sDate))
                    {
                        search.Add("sDateA", _sDate);
                    }
                    if (!string.IsNullOrWhiteSpace(_eDate))
                    {
                        search.Add("eDateA", _eDate);
                    }

                    break;

                case "B":
                    if (!string.IsNullOrWhiteSpace(_sDate))
                    {
                        search.Add("sDateB", _sDate);
                    }
                    if (!string.IsNullOrWhiteSpace(_eDate))
                    {
                        search.Add("eDateB", _eDate);
                    }

                    break;

                default:
                    //do nothing
                    break;
            }


            //----- 方法:取得資料 -----
            using (DT = _data.GetQuote_AllComp(search, start, length, true, out DataCnt, out ErrMsg))
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

                if (!string.IsNullOrWhiteSpace(ErrMsg))
                {
                    json.Add(new JProperty("error", ErrMsg));
                }

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
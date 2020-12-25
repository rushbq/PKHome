using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

/// <summary>
/// 自訂選單
/// </summary>
public class fn_Menu
{
    /// <summary>
    /// 發貨統計
    /// </summary>
    /// <param name="lang"></param>
    /// <param name="rootID"></param>
    /// <param name="compID"></param>
    /// <param name="tabID"></param>
    /// <returns></returns>
    public static string GetTopMenu_ShipFreight(string lang, string rootID, string compID, string tabID)
    {
        string menu = "";
        string url = "{0}{1}/{2}/ShipFreight/{3}".FormatThis(fn_Param.WebUrl, lang, rootID, compID);

        //發貨明細
        menu += "<a class=\"item {1}\" href=\"{0}/?tab=1\">發貨明細</a>".FormatThis(url, tabID.Equals("1") ? "active" : "");

        //發貨資料傳送
        url = "{0}{1}/{2}/ShipFreightSend/{3}".FormatThis(fn_Param.WebUrl, lang, rootID, compID);
        menu += "<a class=\"item {1}\" href=\"{0}/?tab=2\">發貨資料傳送</a>".FormatThis(url, tabID.Equals("2") ? "active" : "");

        //運費統計
        url = "{0}{1}/{2}/ShipFreightStat_Y/{3}".FormatThis(fn_Param.WebUrl, lang, rootID, compID);
        menu += "<a class=\"item {1}\" href=\"{0}/?tab=3\">運費統計</a>".FormatThis(url, tabID.Equals("3") ? "active" : "");

        //週統計
        url = "{0}{1}/{2}/ShipFreightStat_W/{3}".FormatThis(fn_Param.WebUrl, lang, rootID, compID);
        menu += "<a class=\"item {1}\" href=\"{0}/?tab=4\">週統計</a>".FormatThis(url, tabID.Equals("4") ? "active" : "");

        //舊版程式
        menu += "<a class=\"item\" href=\"{0}\" target=\"_blank\">歷史明細(深圳)</a>".FormatThis("http://ef.prokits.com.tw/employee/prounion/DailySalesGrid.asp");

        return menu;
    }


    /// <summary>
    /// 產品目錄
    /// </summary>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    public static IQueryable<ClassItem> GetProdVol(out string ErrMsg)
    {
        //----- 宣告 -----
        List<ClassItem> dataList = new List<ClassItem>();
        string sql = "";

        //----- 資料取得 -----
        using (SqlCommand cmd = new SqlCommand())
        {
            //----- SQL 查詢語法 -----
            sql = @"
                SELECT RTRIM(Catelog_Vol) AS Label
                FROM Prod_Item WITH(NOLOCK)
                WHERE (Catelog_Vol <> '')
                GROUP BY Catelog_Vol
                ORDER BY 1
                ";

            //----- SQL 執行 -----
            cmd.CommandText = sql.ToString();

            using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Product, out ErrMsg))
            {
                //LinQ 查詢
                var query = DT.AsEnumerable();

                //資料迴圈
                foreach (var item in query)
                {
                    //加入項目
                    var data = new ClassItem
                    {
                        Label = item.Field<string>("Label")
                    };

                    //將項目加入至集合
                    dataList.Add(data);

                }
            }

            //回傳集合
            return dataList.AsQueryable();

        }
    }


    /// <summary>
    /// 客訴來源
    /// </summary>
    /// <param name="lang"></param>
    /// <param name="typeID"></param>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    public static ClassItem GetOne_RefType(string lang, Int32 typeID, out string ErrMsg)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        ClassItem query = _data.GetCCP_RefType(lang, out ErrMsg)
            .Where(fld => fld.ID.Equals(typeID))
            .FirstOrDefault();

        _data = null;

        return query;
    }

    /// <summary>
    /// 來源類型
    /// </summary>
    /// <param name="typeID">A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具</param>
    /// <returns></returns>
    public static string GetShipping_RefType(string typeID)
    {
        switch (typeID)
        {
            case "A":
                return "電商工具";

            case "B":
                return "電商玩具";

            case "C":
                return "經銷商工具";

            default:
                return "經銷商玩具";
        }
    }

    /// <summary>
    /// 電商平台數據 - 來源類型
    /// </summary>
    /// <param name="typeID"></param>
    /// <returns></returns>
    public static string GetECData_RefType(Int32 typeID)
    {
        switch (typeID)
        {
            case 1:
                return "工具";

            default:
                return "科學玩具";
        }
    }

    /// <summary>
    /// 電商平台數據 - 促銷費用類型
    /// </summary>
    /// <param name="typeID"></param>
    /// <returns></returns>
    public static string GetECData_PromoType(Int32 typeID)
    {
        switch (typeID)
        {
            case 1:
                return "促銷費";

            case 2:
                return "優惠卷";

            case 3:
                return "非結構化返利";

            default:
                return "其他";
        }
    }
}
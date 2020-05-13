using System;
using System.Collections.Generic;
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
    /// 
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
        menu += "<a class=\"item {2}\" href=\"{0}/?tab=1\">發貨明細</a>".FormatThis(url, "", tabID.Equals("1") ? "active" : "");

        //發貨資料傳送
        url = "{0}{1}/{2}/ShipFreightSend/{3}".FormatThis(fn_Param.WebUrl, lang, rootID, compID);
        menu += "<a class=\"item {2}\" href=\"{0}/?tab=2\">發貨資料傳送</a>".FormatThis(url, "", tabID.Equals("2") ? "active" : "");

        //運費統計
        url = "{0}{1}/{2}/ShipFreightStat_Y/{3}".FormatThis(fn_Param.WebUrl, lang, rootID, compID);
        menu += "<a class=\"item {2}\" href=\"{0}/?tab=3\">運費統計</a>".FormatThis(url, "", tabID.Equals("3") ? "active" : "");

        //週統計
        url = "{0}{1}/{2}/ShipFreightStat_W/{3}".FormatThis(fn_Param.WebUrl, lang, rootID, compID);
        menu += "<a class=\"item {2}\" href=\"{0}/?tab=4\">週統計</a>".FormatThis(url, "", tabID.Equals("4") ? "active" : "");

        //舊版程式
        menu += "<a class=\"item\" href=\"{0}\" target=\"_blank\">歷史明細(深圳)</a>".FormatThis("http://ef.prokits.com.tw/employee/prounion/DailySalesGrid.asp");

        return menu;
    }


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
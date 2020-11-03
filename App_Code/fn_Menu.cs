﻿using System;
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
    /// 發貨統計-中國內銷
    /// </summary>
    /// <param name="lang"></param>
    /// <param name="rootID"></param>
    /// <param name="tabID">選單tab</param>
    /// <param name="dataType">1工具/2玩具</param>
    /// <returns></returns>
    public static string GetTopMenu_ShipFreight_CHN(string lang, string rootID, string tabID, string dataType)
    {
        string menu = "";
        string url = "{0}{1}/{2}/ShipFreight_CHN".FormatThis(fn_Param.WebUrl, lang, rootID);

        //發貨明細
        menu += "<a class=\"item {2}\" href=\"{0}/?dt={1}&tab=1\">發貨明細</a>".FormatThis(url, dataType, tabID.Equals("1") ? "active" : "");

        //發貨資料傳送
        url = "{0}{1}/{2}/ShipFreightSend_CHN".FormatThis(fn_Param.WebUrl, lang, rootID);
        menu += "<a class=\"item {2}\" href=\"{0}/?dt={1}&tab=2\">發貨資料傳送</a>".FormatThis(url, dataType, tabID.Equals("2") ? "active" : "");

        //運費統計
        url = "{0}{1}/{2}/ShipFreightStat_Y_CHN".FormatThis(fn_Param.WebUrl, lang, rootID);
        menu += "<a class=\"item {2}\" href=\"{0}/?dt={1}&tab=3\">運費統計</a>".FormatThis(url, dataType, tabID.Equals("3") ? "active" : "");

        //週統計
        url = "{0}{1}/{2}/ShipFreightStat_W_CHN".FormatThis(fn_Param.WebUrl, lang, rootID);
        menu += "<a class=\"item {2}\" href=\"{0}/?dt={1}&tab=4\">週統計</a>".FormatThis(url, dataType, tabID.Equals("4") ? "active" : "");

        return menu;
    }


    /// <summary>
    /// 發貨統計-中國內銷 (運費方式)
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public static string GetItem_ShipFrieghtWay(string val)
    {
        switch (val.ToUpper())
        {
            case "A":
                return "自付";

            case "B":
                return "墊付";

            case "C":
                return "到付";

            default:
                return "";
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
<%@ WebHandler Language="C#" Class="Ashx_OutputExcel" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

public class Ashx_OutputExcel : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;
        try
        {
            //[接收參數] 自訂查詢
            string _sDate = context.Request["sDate"];
            string _eDate = context.Request["eDate"];
            string _OpcsNo = context.Request["OpcsNo"];
            string _Cust = context.Request["Cust"];
            string _Comp = context.Request["Comp"];
            string _Reason = context.Request["Reason"];
            string _Supplier = context.Request["Supplier"];
            string _ModelNo = context.Request["ModelNo"];


            //----- 宣告:資料參數 -----
            Menu4000Repository _data = new Menu4000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();
            DataTable DT = new DataTable();

            //----- 原始資料:條件篩選 -----
            search.Add("OpcsNo", _OpcsNo);
            search.Add("Cust", _Cust);
            search.Add("sDate", _sDate);
            search.Add("eDate", _eDate);
            search.Add("Comp", _Comp);
            search.Add("Reason", _Reason);
            search.Add("Supplier", _Supplier);
            search.Add("ModelNo", _ModelNo);

            //----- 方法:取得資料 -----
            var query = _data.GetDelayShipStat(search, out ErrMsg)
                .Select(fld => new
                {
                    CompName = fld.CompName,
                    PendingDate = fld.PendingDate,
                    OrderDate = fld.OrderDate,
                    ShipDateOld = fld.ShipDateOld,
                    RangeDays = fld.RangeDays,
                    Opcs = fld.OrderNoType + fld.OrderNo,
                    CustName = fld.CustName,
                    ModelNo = fld.ModelNo,
                    OrderNum = fld.OrderNum,
                    NewQty = fld.NewQty.Equals(-999) ? "整批" : fld.NewQty.ToString(),
                    OrderNum_Pend = fld.OrderNum_Pend,
                    Currency = fld.Currency,
                    PendingPrice = fld.PendingPrice,
                    ShipDateNew = fld.ShipDateNew,
                    Supplier = fld.Supplier,
                    Purchaser = fld.Purchaser.Replace("<br>", "-"),
                    ReasonName = fld.ReasonName

                });

            //將IQueryable轉成DataTable
            DataTable myDT = CustomExtension.LINQToDataTable(query);

            if (myDT.Rows.Count > 0)
            {
                //重新命名欄位標頭
                myDT.Columns["CompName"].ColumnName = "公司別";
                myDT.Columns["PendingDate"].ColumnName = "發延日期";
                myDT.Columns["OrderDate"].ColumnName = "接單日期";
                myDT.Columns["ShipDateOld"].ColumnName = "原出貨日";
                myDT.Columns["RangeDays"].ColumnName = "交期天數";
                myDT.Columns["Opcs"].ColumnName = "Opcs";
                myDT.Columns["CustName"].ColumnName = "客戶";
                myDT.Columns["ModelNo"].ColumnName = "品號";
                myDT.Columns["OrderNum"].ColumnName = "訂單數量";
                myDT.Columns["NewQty"].ColumnName = "可出數量";
                myDT.Columns["OrderNum_Pend"].ColumnName = "未交數量";
                myDT.Columns["Currency"].ColumnName = "幣別";
                myDT.Columns["PendingPrice"].ColumnName = "未出金額";
                myDT.Columns["ShipDateNew"].ColumnName = "可出貨日期";
                myDT.Columns["Supplier"].ColumnName = "供應商";
                myDT.Columns["Purchaser"].ColumnName = "採購人員";
                myDT.Columns["ReasonName"].ColumnName = "延遲原因";
            }

            //release
            query = null;

            //匯出Excel
            CustomExtension.ExportExcel(
                myDT
                , "DataOutput-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
                , false);

        }
        catch (Exception)
        {
            throw;
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
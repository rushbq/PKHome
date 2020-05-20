using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Menu4000Data.Models;
using PKLib_Method.Methods;

/*
  [到貨狀況]-OpcsStatus:會JOIN舊版OPCS備註，故連線要連至PKANALYZER
  [延遲分析]-DelayShipStat:需要關聯至EFGP
  [外廠包材庫存盤點]-SupInvCheck
*/
namespace Menu4000Data.Controllers
{

    public class Menu4000Repository
    {
        public string ErrMsg;

        #region -----// Read //-----

        #region *** 到貨狀況 S ***

        /// <summary>
        /// 到貨狀況表(OpcsStatus)
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <returns></returns>
        public DataTable GetOpcsStatus(string CompID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @CheckDate AS VARCHAR(10)");
                sql.AppendLine(" SET @CheckDate = CONVERT(VARCHAR(10), GETDATE() - 365, 112)");
                sql.AppendLine(" ; WITH TblTotal AS(");

                /* 計算全部未出數量 */
                sql.AppendLine("      SELECT SUM(TotalQty) AS TotalQty, Tbl_Total.TD004 AS ModelNo");
                sql.AppendLine("      FROM");
                sql.AppendLine("      (");
                sql.AppendLine("          SELECT SUM(TD008 + TD024 - TD009) AS TotalQty, TD004");
                sql.AppendLine("          FROM [{0}].dbo.COPTD WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("          WHERE (TD021 = 'Y') AND (TD016 = 'N')");
                //[系統條件] 1年內//
                sql.AppendLine("          AND (COPTD.CREATE_DATE >= @CheckDate)");
                sql.AppendLine("          GROUP BY TD004");
                sql.AppendLine("          UNION ALL");
                sql.AppendLine("          SELECT (MOCTB.TB004 - MOCTB.TB005) AS TotalQty, MOCTB.TB003");
                sql.AppendLine("          FROM [{0}].dbo.MOCTA WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("              INNER JOIN [{0}].dbo.MOCTB WITH(NOLOCK) ON MOCTA.TA001 = MOCTB.TB001 AND MOCTA.TA002 = MOCTB.TB002".FormatThis(dbName));
                sql.AppendLine("          WHERE (MOCTA.TA011 IN ('1', '2', '3'))");
                sql.AppendLine("              AND (MOCTB.TB011 IN ('1', '2'))");
                sql.AppendLine("              AND (MOCTB.TB018 = 'Y')");
                sql.AppendLine("              AND (MOCTB.TB004 - MOCTB.TB005 > 0)");
                //[系統條件] 1年內//
                sql.AppendLine("          AND (MOCTA.CREATE_DATE >= @CheckDate)");

                //判斷公司別
                switch (CompID)
                {
                    case "TW":
                        sql.Append(" AND (MOCTA.TA001 IN ('510', '513', '520', '524', '525'))");
                        break;
                }

                sql.AppendLine("      ) AS Tbl_Total");
                sql.AppendLine("      GROUP BY Tbl_Total.TD004");
                sql.AppendLine("  )");
                sql.AppendLine(" , TblunStock AS (");
                /* 計算待入庫 */
                sql.AppendLine("     SELECT MOCTA.TA006 AS ModelNo");
                sql.AppendLine("      , (SUM(MOCTA.TA015 - MOCTA.TA017) - SUM(MOCTB.TB004 - MOCTB.TB005)) AS unStockQty");
                sql.AppendLine("     FROM [{0}].dbo.MOCTA WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("      INNER JOIN [{0}].dbo.MOCTB ON(MOCTB.TB001 = MOCTA.TA001) AND (MOCTB.TB002 = MOCTA.TA002) AND (MOCTB.TB003 = MOCTA.TA006)".FormatThis(dbName));
                sql.AppendLine("     WHERE (MOCTA.TA013 <> 'V')");
                sql.AppendLine("         AND (MOCTA.TA011 IN ('1', '2', '3'))");
                //[系統條件] 1年內//
                sql.AppendLine("         AND (MOCTA.CREATE_DATE >= @CheckDate)");
                //判斷公司別
                switch (CompID)
                {
                    case "TW":
                        sql.Append(" AND (MOCTA.TA001 IN('520', '513', '521', '525', '526', '527'))");
                        break;
                }

                sql.AppendLine("     GROUP BY MOCTA.TA006");
                sql.AppendLine(" )");
                sql.AppendLine(" , TblPreIn AS (");
                /* 計算預計進 */
                sql.AppendLine("     SELECT TD004 AS ModelNo");
                sql.AppendLine("      , SUM(TD008 - TD015) AS PreInQty");
                sql.AppendLine("     FROM [{0}].dbo.PURTD WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("     WHERE (TD016 = 'N') AND (TD018 = 'Y')");
                //判斷公司別
                switch (CompID)
                {
                    case "TW":
                        sql.Append(" AND (TD001 IN ('3301', '3302', '3304', '3307','3322'))");
                        break;
                }
                sql.AppendLine("     GROUP BY TD004");
                sql.AppendLine(" )");
                sql.AppendLine(" , TblPlanIn AS (");
                /* 計算計劃進 */
                sql.AppendLine("     SELECT TD004 AS ModelNo");
                sql.AppendLine("      , SUM(TD008 - TD015) AS PlanInQty");
                sql.AppendLine("     FROM [{0}].dbo.PURTD WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("     WHERE (TD016 = 'N') AND (TD018 = 'N') AND (TD007 = @StockType)");
                sql.AppendLine("     GROUP BY TD004");
                sql.AppendLine(" )");

                #region ** TblBase **
                sql.AppendLine(" , TblBase AS (");
                sql.AppendLine(" SELECT");
                sql.AppendLine("     RTRIM(COPTC.TC001) AS Order_FID");
                sql.AppendLine("     , RTRIM(COPTC.TC002) AS Order_SID");
                sql.AppendLine("     , RTRIM(UPPER(COPTC.TC004)) AS OldCustID");
                sql.AppendLine("     , RTRIM(UPPER(COPTC.TC004)) AS CustID");
                sql.AppendLine("    , COPTC.TC008 AS Currency"); //幣別
                sql.AppendLine("    , COPTC.TC013 AS TradeConditional"); //交易條件
                sql.AppendLine("    , COPTC.TC014 AS PaidConditional"); //付款條件
                sql.AppendLine("    , COPTC.TC005 AS OrderDeptID"); //訂單部門代號
                sql.AppendLine("    , COPTD.TD003 AS OrderSno"); //訂單單身序號
                sql.AppendLine("    , RTRIM(COPTD.TD004) AS ModelNo"); //品號
                sql.AppendLine("    , RTRIM(COPTD.TD005) AS ModelName"); //品名
                sql.AppendLine("    , CAST((COPTD.TD008 + COPTD.TD024) - (COPTD.TD009 + COPTD.TD025) AS INT) AS unShip_OrderQty"); //訂單未出數量 = (訂單數量+贈品量) - (已交數量+贈品已交量)
                sql.AppendLine("    , COPTD.TD016 AS OrderStatus"); //訂單結案碼(生管出貨狀態)
                sql.AppendLine("    , ISNULL(COPTH.TH020, 'N') AS ShipStatus"); //銷貨單確認碼:Y=已出貨(BLUE), 其他=未出貨(RED)(出貨狀態)
                sql.AppendLine("    , CAST(ISNULL(INVMC.MC007, 0) AS INT) AS StockQty_Main"); //主要倉庫存(依變數)
                sql.AppendLine("    , CAST(ISNULL((");
                sql.AppendLine("        SELECT MC007");
                sql.AppendLine("         FROM [{0}].dbo.INVMC WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("         WHERE(MC002 = '11') AND(MC001 = INVMB.MB001)");
                sql.AppendLine("      ), 0) AS INT) AS StockQty_11 "); //TW 11倉庫存
                sql.AppendLine("	, CAST(ISNULL((");
                sql.AppendLine("         SELECT MC007");
                sql.AppendLine("         FROM [{0}].dbo.INVMC WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("         WHERE(MC002 = '20') AND(MC001 = INVMB.MB001)");
                sql.AppendLine("     ), 0) AS INT) AS StockQty_20 "); //TW:20倉庫存
                sql.AppendLine("	, CAST(ISNULL((");
                sql.AppendLine("         SELECT MC007");
                sql.AppendLine("         FROM [{0}].dbo.INVMC WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("         WHERE(MC002 = '14') AND(MC001 = INVMB.MB001)");
                sql.AppendLine("     ), 0) AS INT) AS StockQty_14 "); //SH:14倉庫存
                sql.AppendLine("	, INVMB.MB025 AS ProdProperty"); //品號屬性
                sql.AppendLine("	, CONVERT(VARCHAR(10), CAST(COPTD.TD013 AS DATE), 111) AS OrderPreDate"); //訂單預交日
                sql.AppendLine("	, CAST(ISNULL(INVMC.MC004, 0) AS INT) AS SafeQty_Main"); //主要倉安全存量
                sql.AppendLine("	, INVMC.MC003 AS StockPos"); //儲位
                sql.AppendLine("	, RTRIM(PURMA.MA001) AS Main_SupplierID"); //主供應商ID
                sql.AppendLine("	, RTRIM(PURMA.MA002) AS Main_SupplierName"); //主供應商Name
                sql.AppendLine("	, COPTD.TD014 AS CustModel, COPTD.TD007 AS StockType"); //客戶品號, 庫別
                sql.AppendLine("	, COPTD.TD200 AS BoxNoStart, COPTD.TD201 AS BoxNoEnd"); //起始箱號, 截止箱號
                sql.AppendLine("	, INVMB.MB029 AS ProdImgDesc"); //產品圖號
                /* -- 資材 -- */
                sql.AppendLine("    , ISNULL(Stock.StockValue, '') AS StockValue");
                sql.AppendLine("    , ISNULL(Box.BoxValue, '') AS BoxValue");

                sql.AppendLine(" FROM [{0}].dbo.COPTC WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("   INNER JOIN [{0}].dbo.COPTD WITH (NOLOCK)ON COPTC.TC001 = COPTD.TD001 AND COPTC.TC002 = COPTD.TD002".FormatThis(dbName));
                sql.AppendLine("   INNER JOIN [{0}].dbo.INVMB WITH(NOLOCK) ON COPTD.TD004 = INVMB.MB001".FormatThis(dbName));
                //PURMA 廠商基本資料檔(帶出主供應商)
                sql.AppendLine("    LEFT JOIN [{0}].dbo.PURMA WITH(NOLOCK) ON INVMB.MB032 = PURMA.MA001".FormatThis(dbName));
                //INVMC 品號庫別檔
                sql.AppendLine("    LEFT JOIN [{0}].dbo.INVMC WITH(NOLOCK) ON INVMB.MB001 = INVMC.MC001 AND INVMC.MC002 = @StockType".FormatThis(dbName));
                //COPTH 銷貨單單身檔
                sql.AppendLine("    LEFT JOIN [{0}].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016".FormatThis(dbName));
                //PKSYS資材理貨
                sql.AppendLine("    LEFT JOIN OpcsStatus_Rel_Stock Stock ON (RTRIM(COPTD.TD001) + RTRIM(COPTD.TD002) + RTRIM(COPTD.TD003)) COLLATE Chinese_Taiwan_Stroke_BIN = Stock.ErpID");
                //PKSYS包裝資料
                sql.AppendLine("    LEFT JOIN OpcsStatus_Rel_Box Box ON (RTRIM(COPTD.TD001) + RTRIM(COPTD.TD002) + RTRIM(COPTD.TD003)) COLLATE Chinese_Taiwan_Stroke_BIN = Box.ErpID");
                //--Base 基本條件
                sql.AppendLine(" WHERE (COPTC.TC027 = 'Y') AND (COPTD.TD013 >= CONVERT(VARCHAR(10), GETDATE() - 60, 112))");
                sql.AppendLine(" )");

                #endregion


                #region ** TblPur **
                /* -- 採購/進貨 -- */
                sql.AppendLine(" , TblPur AS (");
                sql.AppendLine(" SELECT PURTD.TD004 AS ModelNo");
                sql.AppendLine("  , PURTD.TD013 AS Ref_FID");
                sql.AppendLine("  , PURTD.TD021 AS Ref_SID");
                sql.AppendLine("  , PURTD.TD023 AS Ref_OrderSno");
                sql.AppendLine("  , RTRIM(PURTD.TD001) AS PUR_FID"); //採購單別
                sql.AppendLine("  , RTRIM(PURTD.TD002) AS PUR_SID"); //採購單號
                sql.AppendLine("  , PURTD.TD018 AS PurConfirm"); //採購單確認碼=V->(作廢)..紅色 @採購單判斷
                sql.AppendLine("  , CONVERT(VARCHAR(10), CAST(PURTD.TD012 AS DATE), 111) AS PurPreDate"); //採購單預交日
                sql.AppendLine("  , CAST(ISNULL(PURTD.TD008, 0) AS INT) AS PurQty"); //採購數量(採購數量 < 未出訂單數量 -> 背景色FFFF00)
                sql.AppendLine("  , CAST(ISNULL(PURTH.TH015, 0) AS INT) AS GetInQty"); //進貨數量
                sql.AppendLine("  , CAST((ISNULL(PURTD.TD008, 0) - ISNULL(PURTH.TH015, 0)) AS INT) AS unGetInQty"); //未進貨數量=採購數量TD008-進貨數量TH015
                sql.AppendLine("  , RTRIM(PURTC.TC004) AS PurSupplierID"); //採購廠商ID
                sql.AppendLine("  , RTRIM(PURMA.MA002) AS PurSupplier"); //採購廠商
                //PURTD 採購單單身檔
                sql.AppendLine(" FROM [{0}].dbo.PURTD WITH (NOLOCK)".FormatThis(dbName));
                //PURTC 採購單單頭檔
                sql.AppendLine(" LEFT JOIN [{0}].dbo.PURTC WITH (NOLOCK) ON PURTC.TC001 = PURTD.TD001 AND PURTC.TC002 = PURTD.TD002".FormatThis(dbName));
                //PURTH 進貨單單身檔
                sql.AppendLine(" LEFT JOIN [{0}].dbo.PURTH WITH (NOLOCK) ON PURTH.TH011 = PURTD.TD001 AND PURTH.TH012 = PURTD.TD002 AND PURTH.TH013 = PURTD.TD003".FormatThis(dbName));
                //PURMA 採購廠商
                sql.AppendLine(" LEFT JOIN [{0}].dbo.PURMA WITH (NOLOCK) ON PURMA.MA001 = PURTC.TC004".FormatThis(dbName));
                //[系統條件] 1年內//
                sql.AppendLine(" WHERE (PURTD.CREATE_DATE >= @CheckDate)");
                sql.AppendLine(" )");

                #endregion


                #region ** TblMake **
                /* -- 生產/入庫 -- */
                sql.AppendLine(" , TblMake AS (");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  MOCTA.TA006 AS ModelNo");
                sql.AppendLine("  , MOCTA.TA026 AS Ref_FID");
                sql.AppendLine("  , MOCTA.TA027 AS Ref_SID");
                sql.AppendLine("  , MOCTA.TA028 AS Ref_OrderSno");
                sql.AppendLine("  , RTRIM(MOCTA.TA001) AS Make_FID"); //製令單別
                sql.AppendLine("  , RTRIM(MOCTA.TA002) AS Make_SID"); //製令單號
                sql.AppendLine("  , (CASE WHEN MOCTA.TA014 IS NULL OR MOCTA.TA014 = '' THEN '' ELSE CONVERT(VARCHAR(10), CAST(MOCTA.TA014 AS DATE), 111) END) AS FinishDate"); //實際完工日
                sql.AppendLine("  , MOCTA.TA011 AS MakeStatus"); //製令完工狀態(1:未生產, 2:已發料, 3:生產中, Y:已完工, y:指定完工)(查詢)
                sql.AppendLine("  , MOCTA.TA013 AS MakeConfirm"); //確認碼=V->(作廢)..紅色 @製令狀態欄判斷
                //MOCTA 製造命令單頭檔
                sql.AppendLine(" FROM [{0}].dbo.MOCTA WITH (NOLOCK)".FormatThis(dbName));
                //[系統條件] 1年內//
                sql.AppendLine(" WHERE (MOCTA.CREATE_DATE >= @CheckDate)");
                sql.AppendLine(" )");

                #endregion

                sql.AppendLine(" SELECT");
                sql.AppendLine("  ROW_NUMBER() OVER(PARTITION BY Order_FID, Order_SID ORDER BY Order_FID, Order_SID, OrderSno ASC) AS RowNumber");
                sql.AppendLine("  , TblBase.*");
                sql.AppendLine("  , TblPur.PUR_FID, TblPur.PUR_SID, TblPur.PurConfirm, TblPur.PurPreDate, TblPur.PurSupplierID, TblPur.PurSupplier");
                sql.AppendLine("  , ISNULL(TblPur.PurQty, 0) AS PurQty, ISNULL(TblPur.GetInQty, 0) AS GetInQty, ISNULL(TblPur.unGetInQty, 0) AS unGetInQty");
                sql.AppendLine("  , TblMake.Make_FID, TblMake.Make_SID, TblMake.FinishDate, TblMake.MakeStatus, TblMake.MakeConfirm");
                sql.AppendLine("  , RTRIM(COPMA.MA002) AS CustName");
                //不足量,判斷公司別
                switch (CompID)
                {
                    case "TW":
                        sql.AppendLine("  , (TblBase.StockQty_Main + TblBase.StockQty_11 - ISNULL(TblTotal.TotalQty, 0) + ISNULL(TblunStock.unStockQty, 0)) AS ShortQty"); //不足量(01+11倉 - 全部未出數量)
                        break;

                    default:
                        sql.AppendLine("  , (TblBase.StockQty_Main + TblBase.StockQty_14 - ISNULL(TblTotal.TotalQty, 0) + ISNULL(TblunStock.unStockQty, 0)) AS ShortQty"); //不足量(12+14倉 - 全部未出數量)
                        break;
                }
                sql.AppendLine("  , (");
                sql.AppendLine("     SELECT TOP 1 MG200 FROM [{0}].dbo.COPMG WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("     WHERE (MG001 = TblBase.OldCustID) AND (MG002 = TblBase.ModelNo)");
                sql.AppendLine("  ) AS ProdRemark"); //產品特別注意事項
                sql.AppendLine("  , ISNULL(CopRemk.REMK, ISNULL(ORDERRemk.REMK, '')) AS OrderRemark"); //客戶注意事項
                sql.AppendLine("  , CAST(ISNULL(TblunStock.unStockQty, 0) AS INT) AS unStockQty"); //生產待入庫
                sql.AppendLine("  , CAST(ISNULL(TblPreIn.PreInQty, 0) AS INT) AS PreInQty"); //預計進
                sql.AppendLine("  , CAST(ISNULL(TblPlanIn.PlanInQty, 0) AS INT) AS PlanInQty"); //計劃進
                sql.AppendLine("  , CAST(ISNULL(TblTotal.TotalQty, 0) AS INT) AS TotalQty"); //全部未出數量
                sql.AppendLine("  , CAST(ISNULL(MOCTG.TG011, 0) AS INT) AS MakeStockQty"); //入庫數量

                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  LEFT JOIN TblTotal ON TblBase.ModelNo = TblTotal.ModelNo");
                sql.AppendLine("  LEFT JOIN TblunStock ON TblBase.ModelNo = TblunStock.ModelNo");
                sql.AppendLine("  LEFT JOIN TblPreIn ON TblBase.ModelNo = TblPreIn.ModelNo");
                sql.AppendLine("  LEFT JOIN TblPlanIn ON TblBase.ModelNo = TblPlanIn.ModelNo");
                sql.AppendLine("  LEFT JOIN TblPur ON TblBase.Order_FID = TblPur.Ref_FID AND TblBase.Order_SID = TblPur.Ref_SID AND TblBase.OrderSno = TblPur.Ref_OrderSno AND TblBase.ModelNo = TblPur.ModelNo");
                sql.AppendLine("  LEFT JOIN TblMake ON TblBase.Order_FID = TblMake.Ref_FID AND TblBase.Order_SID = TblMake.Ref_SID AND TblBase.OrderSno = TblMake.Ref_OrderSno AND TblBase.ModelNo = TblMake.ModelNo");
                //客戶
                sql.AppendLine("  LEFT JOIN [{0}].dbo.COPMA WITH (NOLOCK) ON TblBase.CustID = COPMA.MA001".FormatThis(dbName));
                //EF 訂單備註(DB = PKANALYZER)
                sql.AppendLine("  LEFT JOIN [{0}].dbo.CopRemk ON CopRemk.TC001 = TblBase.Order_FID AND CopRemk.TC002 = TblBase.Order_SID".FormatThis(dbName));
                sql.AppendLine("  LEFT JOIN [{0}].dbo.ORDERRemk ON ORDERRemk.MA001 = TblBase.CustID".FormatThis(dbName));
                //MOCTG 生產入庫單身檔(筆數過多,放在最外層JOIN)
                sql.AppendLine("  LEFT JOIN [{0}].dbo.MOCTG WITH (NOLOCK) ON TblBase.ModelNo = MOCTG.TG004 AND TblMake.Make_FID = MOCTG.TG014 AND TblMake.Make_SID = MOCTG.TG015 AND TblBase.StockType = MOCTG.TG010".FormatThis(dbName));

                //Where
                sql.AppendLine(" WHERE (1=1)");

                /* Search */
                #region >> filter <<

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrEmpty(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "OpcsNo":
                                //--OPCS No
                                sql.Append(" AND (");
                                sql.Append("  (UPPER((TblBase.Order_FID + TblBase.Order_SID)) LIKE '%' + UPPER(@OpcsNo) + '%')");
                                sql.Append("  OR (UPPER(TblBase.Order_FID) = UPPER(@OpcsNo))");
                                sql.Append("  OR (UPPER(TblBase.Order_SID) LIKE '%' + UPPER(@OpcsNo) + '%')");
                                sql.Append("  OR (UPPER((TblBase.Order_FID + '-' + TblBase.Order_SID)) LIKE '%' + UPPER(@OpcsNo) + '%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("OpcsNo", item.Value);

                                break;


                            case "Cust":
                                //--客戶ID / Name
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(TblBase.CustID) LIKE '%' + UPPER(@Keyword) + '%')");
                                sql.Append("  OR (UPPER(RTRIM(COPMA.MA002)) LIKE '%' + UPPER(@Keyword) + '%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("Keyword", item.Value);

                                break;


                            case "sDate":
                                //--訂單預交日
                                sql.Append(" AND (TblBase.OrderPreDate >= @sDate)");

                                cmd.Parameters.AddWithValue("sDate", item.Value);

                                break;


                            case "eDate":
                                //--訂單預交日
                                sql.Append(" AND (TblBase.OrderPreDate <= @eDate)");

                                cmd.Parameters.AddWithValue("eDate", item.Value);

                                break;


                            case "ProdProperty":
                                //--品項屬性
                                sql.Append(" AND (TblBase.ProdProperty = @ProdProperty)");

                                cmd.Parameters.AddWithValue("ProdProperty", item.Value);
                                break;


                            case "MakeStatus":
                                //--完工狀態(製令狀態)(固定參數)
                                switch (item.Value)
                                {
                                    case "N":
                                        sql.Append(" AND (TblMake.MakeStatus IN ('1','2','3'))");
                                        break;

                                    case "Y":
                                        sql.Append(" AND (TblMake.MakeStatus = 'Y')");
                                        break;

                                    case "V":
                                        sql.Append(" AND (TblMake.MakeStatus IS NULL)");
                                        break;
                                }
                                break;


                            case "ShipStatus":
                                //--出貨狀態(固定參數訂單結案碼)
                                switch (item.Value)
                                {
                                    case "N":
                                        sql.Append(" AND (TblBase.OrderStatus = 'N')");
                                        break;

                                    case "Y":
                                        sql.Append(" AND (TblBase.OrderStatus IN ('Y','y'))");
                                        break;
                                }
                                break;


                            case "GetInStatus":
                                //--採購進貨狀態(固定參數GetInStatus)
                                switch (item.Value)
                                {
                                    case "A":
                                        sql.Append(" AND (TblPur.GetInQty < TblPur.PurQty)");
                                        break;

                                    case "B":
                                        sql.Append(" AND (TblPur.GetInQty >= 1)");
                                        break;
                                }
                                break;


                            case "StockStatus":
                                //--庫存狀態(固定參數StockStatus)
                                switch (item.Value)
                                {
                                    case "A":
                                        sql.Append(" AND ((TblBase.StockQty_Main + TblBase.StockQty_11 - ISNULL(TblTotal.TotalQty, 0)) <= -1)");
                                        break;
                                }
                                break;

                            case "PurStatus":
                                //--採購下單狀態(固定參數PurStatus)
                                switch (item.Value)
                                {
                                    case "A":
                                        sql.Append(" AND (TblBase.ProdProperty IN ('P','S'))");
                                        break;

                                    case "B":
                                        sql.Append(" AND (TblBase.ProdProperty IN ('P','S'))");
                                        sql.Append(" AND (((TblBase.StockQty_Main + TblBase.StockQty_11 - ISNULL(TblTotal.TotalQty, 0)) + ISNULL(TblPreIn.PreInQty, 0)) <= -1)");
                                        sql.Append(" AND LEFT(TblBase.Main_SupplierID, 1) = '{0}'".FormatThis(GetSupplierFirstID(CompID)));
                                        break;
                                }
                                break;

                            case "Dept":
                                //--訂單部門
                                sql.Append(" AND (TblBase.OrderDeptID = @OrderDept)");

                                cmd.Parameters.AddWithValue("OrderDept", item.Value);
                                break;

                            case "Fastmenu":
                                //--快查選單
                                switch (item.Value)
                                {
                                    case "r1":
                                        /* [欠料狀況]
                                            出貨狀態:未出貨
                                             AND 採購進貨狀態:已採購未進貨(採>0, 採>進)OR 自製件
                                             AND 完工狀態:未完工(NULL=無製令)
                                             AND 庫存狀態:不足量<0
                                             AND 訂單未出數量 > 0
                                        */
                                        sql.Append(" AND (");
                                        sql.Append("     (TblBase.OrderStatus IN ('y','N'))");
                                        sql.Append("     AND (");
                                        sql.Append("      ((TblPur.PurQty >= 1) AND (TblPur.PurQty > TblPur.GetInQty))");
                                        sql.Append("       OR (TblPur.PurQty = 0 AND TblPur.GetInQty = 0)");
                                        sql.Append("       OR (TblBase.ProdProperty = 'M')");
                                        sql.Append("     )");
                                        sql.Append("     AND (TblMake.MakeStatus IN ('1','2','3') OR (TblMake.MakeStatus IS NULL))");
                                        sql.Append("     AND (TblBase.StockQty_Main + TblBase.StockQty_11 - ISNULL(TblTotal.TotalQty, 0)) <= -1");
                                        sql.Append("     AND (TblBase.unShip_OrderQty > 0)");
                                        sql.Append(" )");

                                        break;

                                    case "r2":
                                        /* [庫存出貨]
                                           出貨狀態:未出貨
                                           未下單(製令單)
                                       */
                                        sql.Append(" AND (");
                                        sql.Append("     (TblBase.OrderStatus IN ('y','N'))");
                                        sql.Append("     AND (TblMake.Make_FID IS NULL)");
                                        sql.Append(" )");

                                        break;

                                    case "r3":
                                        /* [產品無條碼]
                                           產品圖號 = 00_貼紙
                                       */
                                        sql.Append(" AND (LEFT(TblBase.ProdImgDesc, 2) = '00')");

                                        break;

                                    case "r4":
                                        /* [產品無MIT]
                                           主供應商的代號為2開頭+富商
                                       */
                                        sql.Append(" AND (");
                                        sql.Append("     (LEFT(TblBase.Main_SupplierID, 1) = '2')");
                                        sql.Append("     OR (RTRIM(TblBase.Main_SupplierID) = '113001')");
                                        sql.Append("     OR (RTRIM(TblBase.Main_SupplierID) = '213007')");
                                        sql.Append(" )");

                                        break;
                                }
                                break;

                        }
                    }
                }
                #endregion


                //Order By
                //使用預交日排序,會造成排序錯亂,而使Group header未正確顯示(20200514)
                //bool customSort = false;
                //if (search != null)
                //{
                //    //過濾空值
                //    var thisSearch = search.Where(fld => !string.IsNullOrEmpty(fld.Value));

                //    //查詢內容
                //    foreach (var item in thisSearch)
                //    {
                //        if (item.Key.Equals("menuID") && item.Value.Equals("190"))
                //        {
                //            customSort = true;
                //            sql.AppendLine(" ORDER BY TblBase.OrderPreDate ASC");
                //        }

                //    }
                //}

                //if (false == customSort)
                //{
                //    sql.AppendLine(" ORDER BY TblBase.Order_FID, TblBase.Order_SID, TblBase.OrderSno, 1");
                //}
                sql.AppendLine(" ORDER BY TblBase.Order_FID, TblBase.Order_SID, TblBase.OrderSno, 1");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 360;   //單位:秒
                cmd.Parameters.AddWithValue("StockType", GetStockType(CompID));

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKSYSinANA, out ErrMsg))
                {
                    return DT;
                }
            }

        }


        /// <summary>
        /// BPM OPCS Data
        /// 使用功能:到貨狀況表
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <returns></returns>
        public DataTable GetOpcsFlow(string CompID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----

                string flowTable = GetFlowTable(CompID);

                //整批延遲
                sql.Append(" SELECT");
                sql.Append(" Base.SerialNumber_auto AS SerialNo");
                sql.Append(" , Base.txt_CreateDate AS CreateDate");
                sql.Append(" , Base.txtAppliDeptName + '-' + Base.txtAppliName + ' (' + Base.txtAppliId + ')' AS Creater");
                sql.Append(" , Base.txt_OpcsNo AS OpcsNo");
                sql.Append(" , '(' + Base.txtCustID + ') ' + Base.txtCustName AS Cust");
                sql.Append(" , Base.txtPreDate AS PreDate");
                sql.Append(" , Base.txaRemark1 AS Remk1");
                sql.Append(" , Base.txaRemark2 AS Remk2");
                sql.Append(" , Base.txtReasonID AS ReasonID");
                sql.Append(" , Base.txtReasonName AS ReasonName");
                sql.Append(" , '整批延遲' AS Model");
                sql.Append(" FROM {0} Base".FormatThis(flowTable));
                sql.Append(" WHERE");
                sql.Append("     ((");
                sql.Append("         SELECT COUNT(*)");
                sql.Append("         FROM {0}_grdNewData CfmDT".FormatThis(flowTable));
                sql.Append("         WHERE (CfmDT.formSerialNumber = Base.formSerialNumber)");
                sql.Append("     ) = 0)");

                #region >> filter <<

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrEmpty(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "OpcsNo":
                                //--OPCS No
                                sql.Append(" AND (Base.txt_OpcsNo = @OpcsNo1)");

                                cmd.Parameters.AddWithValue("OpcsNo1", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                sql.Append(" UNION ALL ");

                //--部份延遲
                sql.Append(" SELECT Base.SerialNumber_auto");
                sql.Append(" , Base.txt_CreateDate");
                sql.Append(" , Base.txtAppliDeptName + '-' + Base.txtAppliName + ' (' + Base.txtAppliId + ')' AS Creater");
                sql.Append(" , Base.txt_OpcsNo");
                sql.Append(" , '(' + Base.txtCustID + ') ' + Base.txtCustName AS Cust");
                sql.Append(" , CfmDT.gPreDate");
                sql.Append(" , CfmDT.gRemark1");
                sql.Append(" , CfmDT.gRemark2");
                sql.Append(" , CfmDT.gReasonID");
                sql.Append(" , CfmDT.gReasonName");
                sql.Append(" , CfmDT.gModelNo");
                sql.Append(" FROM {0} Base".FormatThis(flowTable));
                sql.Append("     INNER JOIN {0}_grdNewData CfmDT ON Base.formSerialNumber = CfmDT.formSerialNumber".FormatThis(flowTable));
                sql.Append(" WHERE (1 = 1)");

                /* Search */
                #region >> filter <<

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrEmpty(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "OpcsNo":
                                //--OPCS No
                                sql.Append(" AND (Base.txt_OpcsNo = @OpcsNo2)");

                                cmd.Parameters.AddWithValue("OpcsNo2", item.Value);

                                break;
                        }
                    }
                }
                #endregion


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.EFGP, out ErrMsg))
                {
                    return DT;
                }
            }

        }

        #endregion *** 到貨狀況 E ***


        #region *** 延遲分析 S ***
        /// <summary>
        /// 延遲分析表(DelayShipStat)
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<DelayShipItem> GetDelayShipStat(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<DelayShipItem> dataList = new List<DelayShipItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("; WITH TblEFGP AS(");

                /** TW **/
                #region EFGP-TW
                sql.AppendLine(" SELECT");
                sql.AppendLine(" Base.SerialNumber_auto AS SerialNo, 'TW' AS FlowComp, CfmDT.gNo AS ItemNo");
                sql.AppendLine(" , Base.txt_CreateDate AS PendingDate");
                sql.AppendLine(" , Base.txt_OpcsNo AS OpcsNo");
                sql.AppendLine(" , Base.txtCustID AS CustID");
                sql.AppendLine(" , Base.txaRemark1 AS Remk1");
                sql.AppendLine(" , Base.txaRemark2 AS Remk2");
                sql.AppendLine(" , Base.txtReasonID AS ReasonID");
                sql.AppendLine(" , Base.txtReasonName AS ReasonName");
                sql.AppendLine(" , CfmDT.gModelNo AS Model");
                sql.AppendLine(" , -999 AS NewQty");
                sql.AppendLine(" FROM EFGP.dbo.PK_FO_19 Base");
                sql.AppendLine("  INNER JOIN EFGP.dbo.ProcessInstance FlowBase ON Base.processSerialNumber = FlowBase.serialNumber AND (FlowBase.currentState = 3)");
                sql.AppendLine("  INNER JOIN EFGP.dbo.PK_FO_19_grdData CfmDT ON Base.formSerialNumber = CfmDT.formSerialNumber");
                sql.AppendLine(" WHERE");
                sql.AppendLine("     ((");
                sql.AppendLine("      SELECT COUNT(*)");
                sql.AppendLine("      FROM EFGP.dbo.PK_FO_19_grdNewData CfmDT");
                sql.AppendLine("      WHERE (CfmDT.formSerialNumber = Base.formSerialNumber)");
                sql.AppendLine("     ) = 0)");
                sql.AppendLine(" UNION ALL");
                sql.AppendLine(" SELECT Base.SerialNumber_auto, 'TW' AS FlowComp, CfmDT.gNo AS ItemNo");
                sql.AppendLine(" , Base.txt_CreateDate");
                sql.AppendLine(" , Base.txt_OpcsNo");
                sql.AppendLine(" , Base.txtCustID");
                sql.AppendLine(" , CfmDT.gRemark1");
                sql.AppendLine(" , CfmDT.gRemark2");
                sql.AppendLine(" , CfmDT.gReasonID");
                sql.AppendLine(" , CfmDT.gReasonName");
                sql.AppendLine(" , CfmDT.gModelNo");
                sql.AppendLine(" , CfmDT.gNewQty");
                sql.AppendLine(" FROM EFGP.dbo.PK_FO_19 Base");
                sql.AppendLine("  INNER JOIN EFGP.dbo.ProcessInstance FlowBase ON Base.processSerialNumber = FlowBase.serialNumber AND (FlowBase.currentState = 3)");
                sql.AppendLine("  INNER JOIN EFGP.dbo.PK_FO_19_grdNewData CfmDT ON Base.formSerialNumber = CfmDT.formSerialNumber");
                sql.AppendLine("  UNION");
                #endregion

                /** SH **/
                #region EFGP-SH
                sql.AppendLine(" SELECT");
                sql.AppendLine(" Base.SerialNumber_auto AS SerialNo, 'SH' AS FlowComp, CfmDT.gNo AS ItemNo");
                sql.AppendLine(" , Base.txt_CreateDate AS PendingDate");
                sql.AppendLine(" , Base.txt_OpcsNo AS OpcsNo");
                sql.AppendLine(" , Base.txtCustID AS CustID");
                sql.AppendLine(" , Base.txaRemark1 AS Remk1");
                sql.AppendLine(" , Base.txaRemark2 AS Remk2");
                sql.AppendLine(" , Base.txtReasonID AS ReasonID");
                sql.AppendLine(" , Base.txtReasonName AS ReasonName");
                sql.AppendLine(" , CfmDT.gModelNo AS Model");
                sql.AppendLine(" , -999 AS NewQty");
                sql.AppendLine(" FROM EFGP.dbo.SH_FO_19 Base");
                sql.AppendLine("  INNER JOIN EFGP.dbo.ProcessInstance FlowBase ON Base.processSerialNumber = FlowBase.serialNumber AND (FlowBase.currentState = 3)");
                sql.AppendLine("  INNER JOIN EFGP.dbo.SH_FO_19_grdData CfmDT ON Base.formSerialNumber = CfmDT.formSerialNumber");
                sql.AppendLine(" WHERE");
                sql.AppendLine("     ((");
                sql.AppendLine("      SELECT COUNT(*)");
                sql.AppendLine("      FROM EFGP.dbo.SH_FO_19_grdNewData CfmDT");
                sql.AppendLine("      WHERE (CfmDT.formSerialNumber = Base.formSerialNumber)");
                sql.AppendLine("     ) = 0)");
                sql.AppendLine(" UNION ALL");
                sql.AppendLine(" SELECT Base.SerialNumber_auto, 'SH' AS FlowComp, CfmDT.gNo AS ItemNo");
                sql.AppendLine(" , Base.txt_CreateDate");
                sql.AppendLine(" , Base.txt_OpcsNo");
                sql.AppendLine(" , Base.txtCustID");
                sql.AppendLine(" , CfmDT.gRemark1");
                sql.AppendLine(" , CfmDT.gRemark2");
                sql.AppendLine(" , CfmDT.gReasonID");
                sql.AppendLine(" , CfmDT.gReasonName");
                sql.AppendLine(" , CfmDT.gModelNo");
                sql.AppendLine(" , CfmDT.gNewQty");
                sql.AppendLine(" FROM EFGP.dbo.SH_FO_19 Base");
                sql.AppendLine("  INNER JOIN EFGP.dbo.ProcessInstance FlowBase ON Base.processSerialNumber = FlowBase.serialNumber AND (FlowBase.currentState = 3)");
                sql.AppendLine("  INNER JOIN EFGP.dbo.SH_FO_19_grdNewData CfmDT ON Base.formSerialNumber = CfmDT.formSerialNumber");
                sql.AppendLine(" UNION");
                #endregion

                /** TRI **/
                #region EFGP-TRI                
                sql.AppendLine(" SELECT");
                sql.AppendLine(" Base.SerialNumber_auto AS SerialNo, 'TRI' AS FlowComp, CfmDT.gNo AS ItemNo");
                sql.AppendLine(" , Base.txt_CreateDate AS PendingDate");
                sql.AppendLine(" , Base.txt_OpcsNo AS OpcsNo");
                sql.AppendLine(" , Base.txtCustID AS CustID");
                sql.AppendLine(" , Base.txaRemark1 AS Remk1");
                sql.AppendLine(" , Base.txaRemark2 AS Remk2");
                sql.AppendLine(" , Base.txtReasonID AS ReasonID");
                sql.AppendLine(" , Base.txtReasonName AS ReasonName");
                sql.AppendLine(" , CfmDT.gModelNo AS Model");
                sql.AppendLine(" , -999 AS NewQty");
                sql.AppendLine(" FROM EFGP.dbo.TRI_FO_19 Base");
                sql.AppendLine("  INNER JOIN EFGP.dbo.ProcessInstance FlowBase ON Base.processSerialNumber = FlowBase.serialNumber AND (FlowBase.currentState = 3)");
                sql.AppendLine("  INNER JOIN EFGP.dbo.TRI_FO_19_grdData CfmDT ON Base.formSerialNumber = CfmDT.formSerialNumber");
                sql.AppendLine(" WHERE");
                sql.AppendLine("     ((");
                sql.AppendLine("      SELECT COUNT(*)");
                sql.AppendLine("      FROM EFGP.dbo.TRI_FO_19_grdNewData CfmDT");
                sql.AppendLine("      WHERE (CfmDT.formSerialNumber = Base.formSerialNumber)");
                sql.AppendLine("     ) = 0)");
                sql.AppendLine(" UNION ALL");
                sql.AppendLine(" SELECT Base.SerialNumber_auto, 'TRI' AS FlowComp, CfmDT.gNo AS ItemNo");
                sql.AppendLine(" , Base.txt_CreateDate");
                sql.AppendLine(" , Base.txt_OpcsNo");
                sql.AppendLine(" , Base.txtCustID");
                sql.AppendLine(" , CfmDT.gRemark1");
                sql.AppendLine(" , CfmDT.gRemark2");
                sql.AppendLine(" , CfmDT.gReasonID");
                sql.AppendLine(" , CfmDT.gReasonName");
                sql.AppendLine(" , CfmDT.gModelNo");
                sql.AppendLine(" , CfmDT.gNewQty");
                sql.AppendLine(" FROM EFGP.dbo.TRI_FO_19 Base");
                sql.AppendLine("  INNER JOIN EFGP.dbo.ProcessInstance FlowBase ON Base.processSerialNumber = FlowBase.serialNumber AND (FlowBase.currentState = 3)");
                sql.AppendLine("  INNER JOIN EFGP.dbo.TRI_FO_19_grdNewData CfmDT ON Base.formSerialNumber = CfmDT.formSerialNumber");
                sql.AppendLine(" )");
                #endregion

                /** ReportCenter.R_ORDER **/
                #region TblBase
                sql.AppendLine(", TblBase AS (");
                sql.AppendLine(" SELECT ");
                sql.AppendLine("  Base.E_SYS_PKflow");
                sql.AppendLine(" , Base.E_IDE_OrderDate AS OrderDate");
                sql.AppendLine(" , Base.E_IDE_ShipDATE AS ShipDateNew");
                sql.AppendLine(" , Base.E_IDE_ShipDATE_Old AS ShipDateOld");
                sql.AppendLine(" , Base.E_IDE_CustomerID AS CustID");
                sql.AppendLine(" , Base.E_IDE_CustomerName AS CustName");
                sql.AppendLine(" , Base.E_IDE_OrderNoType AS OrderNoType");
                sql.AppendLine(" , RTRIM(Base.E_IDE_OrderNo) AS OrderNo");
                sql.AppendLine(" , Base.E_IDE_OrderSerial AS OrderSerial");
                sql.AppendLine(" , Base.E_IDE_ModelNo AS ModelNo");
                sql.AppendLine(" , Base.E_IDE_OrderNum AS OrderNum");
                sql.AppendLine(" , Base.E_IDE_CurrencyClass AS Currency");
                sql.AppendLine(" , (CASE Base.E_IDE_CurrencyClass");
                sql.AppendLine("	  WHEN 'NTD' THEN Base.E_NTD_ModelPrice");
                sql.AppendLine("	  WHEN 'RMB' THEN Base.E_RMB_ModelPrice");
                sql.AppendLine("	  ELSE Base.E_USD_ModelPrice END");
                sql.AppendLine("   ) AS PendingUnitPrice");
                sql.AppendLine("  , DATEDIFF(DAY, Base.E_IDE_OrderDate, Base.E_IDE_ShipDATE_Old) AS RangeDays");
                //主供應商:若為境外則回抓TW的資料
                sql.AppendLine("  , (");
                sql.AppendLine("    CASE Base.E_SYS_PKflow");
                sql.AppendLine("	 WHEN 'TTW' THEN");
                sql.AppendLine("	  (");
                sql.AppendLine("	   SELECT RTRIM(MA001)");
                sql.AppendLine("	   FROM [prokit2].dbo.INVMB WITH(NOLOCK)");
                sql.AppendLine("	    INNER JOIN [prokit2].dbo.PURMA WITH(NOLOCK) ON INVMB.MB032 = PURMA.MA001");
                sql.AppendLine("	   WHERE (INVMB.MB001 = Base.E_IDE_ModelNo)");
                sql.AppendLine("	  )");
                sql.AppendLine("	 WHEN 'TSH' THEN");
                sql.AppendLine("	  (");
                sql.AppendLine("	   SELECT RTRIM(MA001)");
                sql.AppendLine("	   FROM [SHPK2].dbo.INVMB WITH(NOLOCK)");
                sql.AppendLine("	    INNER JOIN [SHPK2].dbo.PURMA WITH(NOLOCK) ON INVMB.MB032 = PURMA.MA001");
                sql.AppendLine("	   WHERE (INVMB.MB001 = Base.E_IDE_ModelNo)");
                sql.AppendLine("	  )");
                sql.AppendLine("	ELSE");
                sql.AppendLine("	 Base.E_IDE_ProviderID");
                sql.AppendLine("	END");
                sql.AppendLine("   ) AS SupplierID");
                //公司別UID
                sql.AppendLine("  , (");
                sql.AppendLine("    CASE Base.E_SYS_PKflow");
                sql.AppendLine("	 WHEN 'SH' THEN 3");
                sql.AppendLine("	 WHEN 'SZ' THEN 2");
                sql.AppendLine("	 WHEN 'TSH' THEN 3");
                sql.AppendLine("	 ELSE 1");
                sql.AppendLine("	END ");
                sql.AppendLine("   ) AS Corp_UID");
                sql.AppendLine("   FROM ReportCenter.dbo.R_ORDER Base WITH(NOLOCK)");
                sql.AppendLine(")");
                #endregion

                /** 供應商資料 **/
                #region TblSupInfo
                sql.AppendLine(", TblSupInfo AS (");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  Corp.Corp_UID, Corp.Corp_Name");
                sql.AppendLine("  , RTRIM(ERP.MA001) ERP_SupID, RTRIM(ERP.MA002) ERP_SupName");
                sql.AppendLine("  , ISNULL(Prof.Account_Name, '') AS UserAccount, ISNULL(Prof.Display_Name, '') AS UserName");
                sql.AppendLine(" FROM Param_Corp Corp WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN Supplier_ERPData ERP WITH(NOLOCK) ON Corp.Corp_ID = RTRIM(ERP.COMPANY)");
                sql.AppendLine("  LEFT JOIN Supplier_ExtendedInfo Info WITH(NOLOCK) ON RTRIM(ERP.MA001) = Info.ERP_ID AND Corp.Corp_UID = Info.Corp_UID");
                sql.AppendLine("  LEFT JOIN User_Profile Prof WITH(NOLOCK) ON Info.Purchaser = Prof.Account_Name");
                sql.AppendLine(" WHERE (Corp.Display = 'Y')");
                sql.AppendLine(")");
                #endregion

                /** 開始取資料 **/
                sql.AppendLine(" SELECT");
                //流程公司別
                sql.AppendLine("  (CASE TblEFGP.FlowComp");
                sql.AppendLine("   WHEN 'TW' THEN '台灣'");
                sql.AppendLine("   WHEN 'SH' THEN '上海'");
                sql.AppendLine("   WHEN 'SZ' THEN '深圳'");
                sql.AppendLine("   ELSE '境外' END");
                sql.AppendLine("   ) AS CompName");
                sql.AppendLine("  , Base.OrderDate");
                sql.AppendLine("  , Base.ShipDateNew");
                sql.AppendLine("  , Base.ShipDateOld");
                sql.AppendLine("  , Base.CustID");
                sql.AppendLine("  , Base.CustName");
                sql.AppendLine("  , Base.OrderNoType");
                sql.AppendLine("  , Base.OrderNo");
                sql.AppendLine("  , Base.OrderSerial");
                sql.AppendLine("  , Base.ModelNo");
                sql.AppendLine("  , Base.OrderNum");
                sql.AppendLine("  , Base.Currency");

                //未出數量(整批=訂單數量 / 訂單數-EFGP預計出貨數)
                sql.AppendLine("  , (CASE WHEN TblEFGP.NewQty = -999");
                sql.AppendLine("       THEN Base.OrderNum");
                sql.AppendLine("       ELSE Base.OrderNum - TblEFGP.NewQty");
                sql.AppendLine("     END");
                sql.AppendLine("   ) AS OrderNum_Pend");
                //原單價
                sql.AppendLine("  , Base.PendingUnitPrice");
                //交期天數(訂單日-原預交日)
                sql.AppendLine("   , Base.RangeDays");

                sql.AppendLine("   , TblEFGP.PendingDate");  //EFGP-發延日期
                sql.AppendLine("   , TblEFGP.NewQty");  //可出數量=EFGP-預計出貨數
                sql.AppendLine("   , TblEFGP.ReasonName");
                sql.AppendLine("   , TblEFGP.SerialNo AS FlowNo");  //EFGP-表單序號
                sql.AppendLine("   , ISNULL(Sup.ERP_SupName, '') AS Supplier");
                sql.AppendLine("   , ISNULL(Sup.UserName, '') AS Purchaser");
                sql.AppendLine(" FROM TblBase Base");
                sql.AppendLine("  INNER JOIN TblEFGP ON RTRIM(Base.OrderNoType) + RTRIM(Base.OrderNo) = TblEFGP.OpcsNo");
                sql.AppendLine("   AND Base.CustID = TblEFGP.CustID");
                sql.AppendLine("   AND Base.ModelNo = TblEFGP.Model");
                sql.AppendLine("   AND Base.OrderSerial = TblEFGP.ItemNo");
                sql.AppendLine("  LEFT JOIN TblSupInfo Sup ON Base.SupplierID COLLATE Chinese_Taiwan_Stroke_BIN = Sup.ERP_SupID AND Base.Corp_UID = Sup.Corp_UID");
                sql.AppendLine(" WHERE (1=1)");

                /* Search */
                #region >> filter <<

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrEmpty(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "sDate":
                                sql.Append(" AND (TblEFGP.PendingDate >= @sDate)");

                                cmd.Parameters.AddWithValue("sDate", item.Value);

                                break;
                            case "eDate":
                                sql.Append(" AND (TblEFGP.PendingDate <= @eDate)");

                                cmd.Parameters.AddWithValue("eDate", item.Value);

                                break;


                            case "Comp":
                                switch (item.Value)
                                {
                                    case "A":
                                        //TW
                                        sql.Append(" AND (TblEFGP.FlowComp = 'TW')");
                                        break;

                                    case "B":
                                        //SH
                                        sql.Append(" AND (TblEFGP.FlowComp = 'SH')");
                                        break;

                                    case "C":
                                        //境外
                                        sql.Append(" AND (TblEFGP.FlowComp = 'TRI')");
                                        break;
                                }
                                break;

                            case "Reason":
                                //延遲原因
                                switch (item.Value)
                                {
                                    case "A":
                                        sql.Append(" AND (LEFT(TblEFGP.ReasonID, 1) = '1')");
                                        break;

                                    case "B":
                                        sql.Append(" AND (LEFT(TblEFGP.ReasonID, 1) <> '1')");
                                        break;
                                }
                                break;


                            case "Supplier":
                                //--供應商ID / Name
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(Sup.ERP_SupName) LIKE '%' + UPPER(@Supplier) + '%')");
                                sql.Append("  OR (UPPER(Sup.ERP_SupID) LIKE '%' + UPPER(@Supplier) + '%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("Supplier", item.Value);

                                break;


                            case "ModelNo":
                                //--品號
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(Base.ModelNo) LIKE '%' + UPPER(@ModelNo) + '%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("ModelNo", item.Value);

                                break;


                            case "Cust":
                                //--客戶ID / Name
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(Base.CustID) LIKE '%' + UPPER(@Cust) + '%')");
                                sql.Append("  OR (UPPER(Base.CustName) LIKE '%' + UPPER(@Cust) + '%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("Cust", item.Value);

                                break;

                            case "OpcsNo":
                                //--OPCS No
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(RTRIM(Base.OrderNoType) + RTRIM(Base.OrderNo)) LIKE '%' + UPPER(@OpcsNo) + '%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("OpcsNo", item.Value);

                                break;


                        }
                    }
                }
                #endregion

                sql.AppendLine(" ORDER BY TblEFGP.PendingDate, TblEFGP.SerialNo, Base.OrderSerial");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new DelayShipItem
                        {
                            CompName = item.Field<string>("CompName"),
                            OrderDate = item.Field<string>("OrderDate"),
                            //可出貨日期=ERP訂單預交日
                            ShipDateNew = item.Field<string>("ShipDateNew"),
                            //原出貨日=ERP訂單原預交日
                            ShipDateOld = item.Field<string>("ShipDateOld"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            OrderNoType = item.Field<string>("OrderNoType"),
                            OrderNo = item.Field<string>("OrderNo"),
                            OrderSerial = item.Field<string>("OrderSerial"),
                            ModelNo = item.Field<string>("ModelNo"),
                            //訂單數量
                            OrderNum = item.Field<int>("OrderNum"),
                            //未交數量(整批=訂單數量 / 訂單數量-EFGP預計出貨數)
                            OrderNum_Pend = item.Field<int>("OrderNum_Pend"),
                            //未出金額=訂單單價*未出數量
                            PendingPrice = (item.Field<decimal>("PendingUnitPrice") * item.Field<int>("OrderNum_Pend")),
                            RangeDays = item.Field<int>("RangeDays"),
                            Supplier = item.Field<string>("Supplier"),
                            PendingDate = item.Field<string>("PendingDate"),
                            //可出數量=EFGP-預計出貨數
                            NewQty = item.Field<int>("NewQty"),
                            ReasonName = item.Field<string>("ReasonName"),
                            FlowNo = item.Field<string>("FlowNo"),
                            Purchaser = item.Field<string>("Purchaser").Replace("-", "<br>"),
                            Currency = item.Field<string>("Currency")
                        };

                        //將項目加入至集合
                        dataList.Add(data);

                    }
                }

                //回傳集合
                return dataList.AsQueryable();
            }

        }


        #endregion *** 延遲分析 E ***


        #region *** 外廠包材庫存盤點 S ***

        /*======== ↓↓↓ 發送清單 ↓↓↓ ========*/
        /// <summary>
        /// [發送清單] 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<SupInvList> GetOneSupInvSend(Dictionary<string, string> search, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetSupInvSendList(search, 0, 1, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [發送清單] 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<SupInvList> GetSupInvSendList(Dictionary<string, string> search, int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                List<SupInvList> dataList = new List<SupInvList>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數

                //取得SQL語法
                subSql = SupInvSendListSQL(search);
                //取得SQL參數集合
                subParamList = SupInvSendListParams(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT COUNT(TbAll.Data_ID) AS TotalCnt FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    //sqlParamList.Add(new SqlParameter("@CC_Type", CCType));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, dbConn.DBS.PKEF, out ErrMsg))
                    {
                        //資料總筆數
                        if (DTCnt.Rows.Count > 0)
                        {
                            DataCnt = Convert.ToInt32(DTCnt.Rows[0]["TotalCnt"]);
                        }
                    }

                    //*** 在SqlParameterCollection同個循環內不可有重複的SqlParam,必須清除才能繼續使用. ***
                    cmdCnt.Parameters.Clear();
                }
                #endregion


                #region >> 主要資料SQL查詢 <<

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT TbAll.* FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    sql.AppendLine(" WHERE (TbAll.RowIdx >= @startRow) AND (TbAll.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TbAll.RowIdx");

                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                    sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new SupInvList
                            {
                                SeqNo = item.Field<Int32>("SeqNo"),
                                Data_ID = item.Field<Guid>("Data_ID"),
                                Subject = item.Field<string>("Subject"),
                                TaskTime = item.Field<DateTime?>("TaskTime").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                IsOnTask = item.Field<string>("IsOnTask"),
                                QueueCnt = item.Field<Int32>("QueueCnt"),
                                SentCnt = item.Field<Int32>("SentCnt"),
                                Remark = item.Field<string>("Remark"),
                                Create_Time = item.Field<DateTime?>("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Update_Time = item.Field<DateTime?>("Update_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Create_Name = item.Field<string>("Create_Name"),
                                Update_Name = item.Field<string>("Update_Name")
                            };


                            //將項目加入至集合
                            dataList.Add(data);

                        }
                    }

                    //回傳集合
                    return dataList.AsQueryable();
                }

                #endregion

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        /// <summary>
        /// [發送清單] 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="fieldLang">欄位語系(ex:zh_TW)</param>
        /// <returns></returns>
        private StringBuilder SupInvSendListSQL(Dictionary<string, string> search)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.SeqNo, Base.Data_ID");
            sql.AppendLine("  , Base.Subject, Base.TaskTime, Base.IsOnTask");
            sql.AppendLine("  , (SELECT COUNT(*) FROM SupInvCheck_Supplier WHERE Parent_ID = Base.Data_ID AND IsSend = 'N') AS QueueCnt");
            sql.AppendLine("  , (SELECT COUNT(*) FROM SupInvCheck_Supplier WHERE Parent_ID = Base.Data_ID AND IsSend = 'Y') AS SentCnt");
            sql.AppendLine("  , Base.Remark, Base.Create_Time, Base.Update_Time");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who)) AS Update_Name");
            sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY Base.Create_Time DESC) AS RowIdx");
            sql.AppendLine(" FROM SupInvCheck_List Base");

            sql.AppendLine(" WHERE (1 = 1)");

            /* Search */
            #region >> filter <<

            if (search != null)
            {
                //過濾空值
                var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                //查詢內容
                foreach (var item in thisSearch)
                {
                    switch (item.Key)
                    {
                        case "DataID":
                            sql.Append(" AND (Base.Data_ID = @Data_ID)");

                            break;

                        case "Keyword":
                            //關鍵字
                            sql.Append(" AND (");
                            sql.Append(" UPPER(Base.Subject) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(")");

                            break;

                        case "sDateA":
                            sql.Append(" AND (Base.TaskTime >= @sDate)");
                            break;
                        case "eDateA":
                            sql.Append(" AND (Base.TaskTime <= @eDate)");
                            break;
                    }
                }
            }
            #endregion

            return sql;
        }


        /// <summary>
        /// [發送清單] 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        private List<SqlParameter> SupInvSendListParams(Dictionary<string, string> search)
        {
            //declare
            List<SqlParameter> sqlParamList = new List<SqlParameter>();

            //get values
            if (search != null)
            {
                //過濾空值
                var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                //查詢內容
                foreach (var item in thisSearch)
                {
                    switch (item.Key)
                    {
                        case "DataID":
                            sqlParamList.Add(new SqlParameter("@Data_ID", item.Value));

                            break;

                        case "Keyword":
                            sqlParamList.Add(new SqlParameter("@keyword", item.Value));

                            break;

                        //--開案時間
                        case "sDateA":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value + " 00:00:00"));
                            break;
                        case "eDateA":
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value + " 23:59:59"));
                            break;

                    }
                }
            }


            return sqlParamList;
        }

        /*======== ↑↑↑ 發送清單 ↑↑↑ ========*/


        /*======== ↓↓↓ 回覆清單 ↓↓↓ ========*/
        /// <summary>
        /// [回覆清單] 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<SupInvSupplier> GetOneSupInvReply(Dictionary<string, string> search, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetSupInvReplyList(search, 0, 1, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [回覆清單] 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<SupInvSupplier> GetSupInvReplyList(Dictionary<string, string> search, int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                List<SupInvSupplier> dataList = new List<SupInvSupplier>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數

                //取得SQL語法
                subSql = SupInvReplyListSQL(search);
                //取得SQL參數集合
                subParamList = SupInvReplyListParams(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT COUNT(TbAll.Data_ID) AS TotalCnt FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    //sqlParamList.Add(new SqlParameter("@CC_Type", CCType));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, dbConn.DBS.PKEF, out ErrMsg))
                    {
                        //資料總筆數
                        if (DTCnt.Rows.Count > 0)
                        {
                            DataCnt = Convert.ToInt32(DTCnt.Rows[0]["TotalCnt"]);
                        }
                    }

                    //*** 在SqlParameterCollection同個循環內不可有重複的SqlParam,必須清除才能繼續使用. ***
                    cmdCnt.Parameters.Clear();
                }
                #endregion


                #region >> 主要資料SQL查詢 <<

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT TbAll.* FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    sql.AppendLine(" WHERE (TbAll.RowIdx >= @startRow) AND (TbAll.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TbAll.RowIdx");

                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                    sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new SupInvSupplier
                            {
                                Parent_ID = item.Field<Guid>("Parent_ID"),
                                Data_ID = item.Field<int>("Data_ID"),
                                SupID = item.Field<string>("SupID"),
                                SupName = item.Field<string>("SupName"),
                                SupMails = item.Field<string>("SupMails"),
                                PurWhoID = item.Field<string>("PurWhoID"),
                                PurWhoName = item.Field<string>("PurWhoName"),
                                PurWhoEmail = item.Field<string>("PurWhoEmail"),
                                Token = item.Field<string>("Token"),
                                StockShow = item.Field<string>("StockShow"),
                                WriteTime = item.Field<DateTime?>("WriteTime").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                IsWrite = item.Field<string>("IsWrite"),
                                SendTime = item.Field<DateTime?>("SendTime").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                IsSend = item.Field<string>("IsSend")
                            };


                            //將項目加入至集合
                            dataList.Add(data);

                        }
                    }

                    //回傳集合
                    return dataList.AsQueryable();
                }

                #endregion

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        /// <summary>
        /// [回覆清單] 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="fieldLang">欄位語系(ex:zh_TW)</param>
        /// <returns></returns>
        private StringBuilder SupInvReplyListSQL(Dictionary<string, string> search)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Parent_ID, Data_ID, SupID, SupName, SupMails");
            sql.AppendLine("  , PurWhoID, PurWhoName, PurWhoEmail, Token, StockShow");
            sql.AppendLine("  , WriteTime, IsWrite, SendTime, IsSend");
            sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY Parent_ID, Data_ID) AS RowIdx");
            sql.AppendLine(" FROM SupInvCheck_Supplier");
            sql.AppendLine(" WHERE (1 = 1)");

            /* Search */
            #region >> filter <<

            if (search != null)
            {
                //過濾空值
                var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                //查詢內容
                foreach (var item in thisSearch)
                {
                    switch (item.Key)
                    {
                        case "doSearch":
                            sql.Append(" AND (IsWrite = 'Y')");

                            break;

                        case "DataID":
                            sql.Append(" AND (Data_ID = @Data_ID)");

                            break;

                        case "ParentID":
                            sql.Append(" AND (Parent_ID = @Parent_ID)");

                            break;

                        case "SupID":
                            sql.Append(" AND (SupID = @SupID)");

                            break;

                        case "Keyword":
                            //關鍵字
                            sql.Append(" AND (");
                            sql.Append(" UPPER(Base.SupID) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(Base.SupName) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(Base.PurWhoID) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(Base.PurWhoName) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(")");

                            break;

                        case "sDateA":
                            sql.Append(" AND (WriteTime >= @sDate)");
                            break;
                        case "eDateA":
                            sql.Append(" AND (WriteTime <= @eDate)");
                            break;
                    }
                }
            }
            #endregion

            return sql;
        }


        /// <summary>
        /// [回覆清單] 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        private List<SqlParameter> SupInvReplyListParams(Dictionary<string, string> search)
        {
            //declare
            List<SqlParameter> sqlParamList = new List<SqlParameter>();

            //get values
            if (search != null)
            {
                //過濾空值
                var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                //查詢內容
                foreach (var item in thisSearch)
                {
                    switch (item.Key)
                    {
                        case "DataID":
                            sqlParamList.Add(new SqlParameter("@Data_ID", item.Value));

                            break;

                        case "ParentID":
                            sqlParamList.Add(new SqlParameter("@Parent_ID", item.Value));

                            break;

                        case "SupID":
                            sqlParamList.Add(new SqlParameter("@SupID", item.Value));

                            break;

                        case "Keyword":
                            sqlParamList.Add(new SqlParameter("@keyword", item.Value));

                            break;

                        case "sDateA":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value + " 00:00:00"));
                            break;
                        case "eDateA":
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value + " 23:59:59"));
                            break;

                    }
                }
            }


            return sqlParamList;
        }


        /// <summary>
        /// 回覆明細-品號清單
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<SupInvModel> GetSupInvReplyDetail(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<SupInvModel> dataList = new List<SupInvModel>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Base.ModelNo, Prod.MB002 AS ModelName, ISNULL(CAST(Stock.MC007 AS INT), 0) AS StockNum");
                sql.AppendLine(" , ISNULL(Base.InputQty1, 0) AS InputQty1, ISNULL(Base.InputQty2, 0) AS InputQty2");
                sql.AppendLine(" , Stock.MC012 AS inStockDate"); /* 最進入庫日 */
                sql.AppendLine(" , Stock.MC013 AS outStockDate"); /* 最近出庫日 */
                sql.AppendLine(" , Prod.MB202 AS anotherModel"); /* 替代品號 */
                sql.AppendLine(" , ISNULL((");
                sql.AppendLine("     SELECT TOP 1 PURMB.MB003");
                sql.AppendLine("     FROM [prokit2].dbo.PURMB WHERE PURMB.MB001 = Stock.MC001");
                sql.AppendLine("     ORDER BY PURMB.MB008 DESC");
                sql.AppendLine("  ), '') AS Currency");
                sql.AppendLine(" , CAST(ISNULL((");
                sql.AppendLine("     SELECT TOP 1 PURMB.MB011");
                sql.AppendLine("     FROM [prokit2].dbo.PURMB WHERE PURMB.MB001 = Stock.MC001");
                sql.AppendLine("     ORDER BY PURMB.MB008 DESC");
                sql.AppendLine("  ), 0) AS FLOAT) AS lastPurPrice");
                sql.AppendLine(" FROM SupInvCheck_Model Base");
                sql.AppendLine("  INNER JOIN [prokit2].dbo.INVMB Prod ON Base.ModelNo COLLATE Chinese_Taiwan_Stroke_BIN = Prod.MB001");
                sql.AppendLine("  LEFT JOIN [prokit2].dbo.INVMC Stock ON Base.SupID COLLATE Chinese_Taiwan_Stroke_BIN = LEFT(Stock.MC003, 6)");
                sql.AppendLine("   AND Base.ModelNo COLLATE Chinese_Taiwan_Stroke_BIN = Stock.MC001 AND Stock.MC002 = '04'");
                sql.AppendLine(" WHERE (1=1) ");

                /* Search */
                #region >> filter <<

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "Parent_ID":
                                sql.Append(" AND (Base.Parent_ID = @Parent_ID)");

                                cmd.Parameters.AddWithValue("Parent_ID", item.Value);

                                break;


                            case "SupID":
                                sql.Append(" AND (Base.SupID = @SupID)");

                                cmd.Parameters.AddWithValue("SupID", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                sql.AppendLine(" ORDER BY Base.ModelNo");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new SupInvModel
                        {
                            ModelNo = item.Field<string>("ModelNo"),
                            ModelName = item.Field<string>("ModelName"),
                            InputQty1 = item.Field<int>("InputQty1"),
                            InputQty2 = item.Field<int>("InputQty2"),
                            StockNum = item.Field<int>("StockNum"),
                            inStockDate = item.Field<string>("inStockDate"),
                            outStockDate = item.Field<string>("outStockDate"),
                            anotherModel = item.Field<string>("anotherModel"),
                            Currency = item.Field<string>("Currency"),
                            lastPurPrice = item.Field<double>("lastPurPrice")
                        };

                        //將項目加入至集合
                        dataList.Add(data);

                    }

                }

                //回傳集合
                return dataList.AsQueryable();
            }
        }

        /*======== ↑↑↑ 回覆清單 ↑↑↑ ========*/


        /// <summary>
        /// 供應商清單, 設定頁使用
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<SupInvSupplier> GetSupplierList(Dictionary<string, string> search, string parentID, out string ErrMsg)
        {
            //----- 宣告 -----
            List<SupInvSupplier> dataList = new List<SupInvSupplier>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" ;WITH TblMember AS (");
                sql.AppendLine("  SELECT Corp_UID, ERP_ID");
                sql.AppendLine("  ,(");
                sql.AppendLine("      SELECT Email + ';'");
                sql.AppendLine("      FROM [PKSYS].dbo.Supplier_Members AS Lst1");
                sql.AppendLine("      WHERE Lst1.ERP_ID = Lst2.ERP_ID AND (IsSendOrder = 'Y')");
                sql.AppendLine("      FOR XML PATH('')");
                sql.AppendLine("  ) AS Emails");
                sql.AppendLine("  FROM [PKSYS].dbo.Supplier_Members AS Lst2");
                sql.AppendLine("  WHERE (IsSendOrder = 'Y')");
                sql.AppendLine("  GROUP BY Corp_UID, ERP_ID");
                sql.AppendLine(" )");
                sql.AppendLine(" , TblBase AS (");
                sql.AppendLine("  SELECT Corp.Corp_UID");
                sql.AppendLine("   , RTRIM(ERP.MA001) ERP_SupID, RTRIM(ERP.MA002) ERP_SupName");
                sql.AppendLine("   , ISNULL(Prof.Account_Name, '') AS PurWhoID, ISNULL(Prof.Display_Name, '') AS PurWhoName, ISNULL(Prof.Email, '') AS PurWhoMail");
                sql.AppendLine("  FROM [PKSYS].dbo.Param_Corp Corp WITH(NOLOCK)");
                sql.AppendLine("   INNER JOIN [PKSYS].dbo.Supplier_ERPData ERP WITH(NOLOCK) ON Corp.Corp_ID = RTRIM(ERP.COMPANY)");
                sql.AppendLine("   INNER JOIN [PKSYS].dbo.Supplier_ExtendedInfo Info WITH(NOLOCK) ON RTRIM(ERP.MA001) = Info.ERP_ID AND Corp.Corp_UID = Info.Corp_UID");
                sql.AppendLine("   INNER JOIN [PKSYS].dbo.User_Profile Prof WITH(NOLOCK) ON Info.Purchaser = Prof.Account_Name");
                //filter:Corp_UID = 1(台灣)
                sql.AppendLine("  WHERE (Corp.Display = 'Y') AND (Corp.Corp_UID = 1)");

                //filter:04倉, (排除)=主供應商MB032 = 122002, 庫存 = 0
                sql.AppendLine("  AND (ERP.MA001 COLLATE Chinese_Taiwan_Stroke_BIN IN (");
                sql.AppendLine("    SELECT LEFT(INVMC.MC003, 6) AS ERP_SupID");
                sql.AppendLine("    FROM [prokit2].dbo.INVMC WITH(NOLOCK)");
                sql.AppendLine("     INNER JOIN [prokit2].dbo.INVMB WITH(NOLOCK) ON INVMC.MC001 = INVMB.MB001");
                sql.AppendLine("    WHERE (INVMC.MC002 = '04') AND (NOT (INVMB.MB032 = '122002' AND INVMC.MC007 = 0))");
                sql.AppendLine("  ))");

                sql.AppendLine(" )");
                sql.AppendLine(" SELECT TblBase.ERP_SupID, TblBase.ERP_SupName");
                sql.AppendLine(" , TblMember.Emails AS SupMails");
                sql.AppendLine(" , TblBase.PurWhoID, TblBase.PurWhoName, TblBase.PurWhoMail");
                sql.AppendLine(" , (");
                sql.AppendLine("     SELECT StockShow");
                sql.AppendLine("     FROM SupInvCheck_Supplier");
                sql.AppendLine("     WHERE (SupID = TblBase.ERP_SupID) AND (Parent_ID = @Parent_ID)");
                sql.AppendLine(" ) AS StockShow");
                //單身品號是否有資料
                sql.AppendLine(" , (");
                sql.AppendLine("     SELECT COUNT(*)");
                sql.AppendLine("     FROM SupInvCheck_Model");
                sql.AppendLine("     WHERE (SupID = TblBase.ERP_SupID) AND (Parent_ID = @Parent_ID)");
                sql.AppendLine(" ) AS DataCheck");

                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN TblMember ON TblBase.ERP_SupID = TblMember.ERP_ID AND TblBase.Corp_UID = TblMember.Corp_UID");
                sql.AppendLine(" WHERE (1=1)");

                /* Search */
                #region >> filter <<

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "UnCheck":
                                //未加入勾選的供應商
                                sql.AppendLine(" AND (TblBase.ERP_SupID NOT IN(");
                                sql.AppendLine("     SELECT SupID");
                                sql.AppendLine("     FROM SupInvCheck_Supplier");
                                sql.AppendLine("     WHERE (Parent_ID = @Parent_ID)");
                                sql.AppendLine(" ))");

                                break;


                            case "IsCheck":
                                //已加入勾選的供應商
                                sql.AppendLine(" AND (TblBase.ERP_SupID IN(");
                                sql.AppendLine("     SELECT SupID");
                                sql.AppendLine("     FROM SupInvCheck_Supplier");
                                sql.AppendLine("     WHERE (Parent_ID = @Parent_ID)");
                                sql.AppendLine(" ))");

                                break;


                            case "SupID":
                                sql.Append(" AND (TblBase.ERP_SupID = @SupID)");

                                cmd.Parameters.AddWithValue("SupID", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                sql.AppendLine(" ORDER BY TblBase.ERP_SupID");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", parentID);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new SupInvSupplier
                        {
                            SupID = item.Field<string>("ERP_SupID"),
                            SupName = item.Field<string>("ERP_SupName"),
                            SupMails = item.Field<string>("SupMails"),
                            PurWhoID = item.Field<string>("PurWhoID"),
                            PurWhoName = item.Field<string>("PurWhoName"),
                            PurWhoEmail = item.Field<string>("PurWhoMail"),
                            StockShow = item.Field<string>("StockShow"),
                            DataCheck = item.Field<int>("DataCheck")
                        };

                        //將項目加入至集合
                        dataList.Add(data);

                    }

                }

                //回傳集合
                return dataList.AsQueryable();
            }
        }



        #endregion *** 外廠包材庫存盤點 E ***


        #endregion



        #region -----// Create //-----


        #region *** 外廠包材庫存盤點 S ***
        /// <summary>
        /// [外廠包材庫存盤點] 建立資料
        /// </summary>
        /// <param name="instance">SupInvList</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateSupInvBase(SupInvList instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO SupInvCheck_List( ");
                sql.AppendLine("  Data_ID, Subject, TaskTime, IsOnTask");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @Subject, @TaskTime, @IsOnTask");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Subject", instance.Subject);
                cmd.Parameters.AddWithValue("TaskTime", instance.TaskTime);
                cmd.Parameters.AddWithValue("IsOnTask", instance.IsOnTask);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// 檢查庫別檔是否有資料
        /// </summary>
        /// <param name="supID"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public Int32 CheckSupplierModel(string supID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT COUNT(*) AS Cnt");
                sql.AppendLine(" FROM [prokit2].dbo.INVMC");
                sql.AppendLine(" WHERE (MC002 = '04') AND (LEFT(MC003, 6) = @SupID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("SupID", supID);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    return Convert.ToInt32(DT.Rows[0]["Cnt"]);
                }

            }
        }


        /// <summary>
        /// 加入供應商清單, 設定頁
        /// </summary>
        /// <param name="supID">供應商ID</param>
        /// <param name="dataID">主檔編號</param>
        /// <param name="stockShow">寶工庫存顯示</param>
        /// <returns></returns>
        public bool CreateSupplierItem(string supID, string dataID, string stockShow)
        {
            Dictionary<string, string> search = new Dictionary<string, string>();

            //----- 原始資料:條件篩選 -----
            search.Add("SupID", supID);

            //----- 原始資料:取得所有資料 -----
            var dataItem = GetSupplierList(search, dataID, out ErrMsg).FirstOrDefault();
            if (dataItem == null)
            {
                return false;
            }
            else
            {
                //取資料後,新增至SupInvCheck_Supplier, SupInvCheck_Model
                //----- 宣告 -----
                StringBuilder sql = new StringBuilder();

                string _token = Cryptograph.MD5("{0}{1}".FormatThis(dataItem.SupID, Cryptograph.GetCurrentTime().ToString()));

                //----- 資料查詢 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    //--供應商
                    sql.AppendLine("DECLARE @NewID AS INT");
                    sql.AppendLine(" SET @NewID = (");
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM SupInvCheck_Supplier WHERE (Parent_ID = @Parent_ID)");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO SupInvCheck_Supplier( ");
                    sql.AppendLine("  Data_ID, Parent_ID");
                    sql.AppendLine("  , SupID, SupName, SupMails");
                    sql.AppendLine("  , PurWhoID, PurWhoName, PurWhoEmail");
                    sql.AppendLine("  , Token, StockShow");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @NewID, @Parent_ID");
                    sql.AppendLine("  , @SupID, @SupName, @SupMails");
                    sql.AppendLine("  , @PurWhoID, @PurWhoName, @PurWhoEmail");
                    sql.AppendLine("  , @Token, @StockShow");
                    sql.AppendLine(" );");

                    //--品號清單
                    sql.AppendLine(" DECLARE @ItemMaxID AS INT");
                    sql.AppendLine("  SET @ItemMaxID = (");
                    sql.AppendLine("   SELECT ISNULL(MAX(Data_ID) ,0) FROM SupInvCheck_Model WHERE (Parent_ID = @Parent_ID)");
                    sql.AppendLine("  )");
                    sql.AppendLine(" INSERT INTO SupInvCheck_Model");
                    sql.AppendLine(" (Parent_ID, Data_ID, SupID, ModelNo)");
                    sql.AppendLine(" SELECT");
                    sql.AppendLine("  @Parent_ID AS ParentID");
                    sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY MC001) + @ItemMaxID AS RowIdx");
                    sql.AppendLine("  , @SupID AS SupID");
                    sql.AppendLine("  , RTRIM(MC001) AS ModelNo");
                    sql.AppendLine(" FROM [prokit2].dbo.INVMC");
                    sql.AppendLine("  INNER JOIN [prokit2].dbo.INVMB ON INVMC.MC001 = INVMB.MB001");
                    sql.AppendLine(" WHERE (INVMC.MC002 = '04') AND (NOT (INVMB.MB032 = '122002' AND INVMC.MC007 = 0))");
                    sql.AppendLine("  AND (LEFT(MC003, 6) = @SupID)");
                    sql.AppendLine(" ORDER BY LEFT(MC003, 6), MC001");
                    sql.AppendLine(" ;");

                    //--主檔
                    sql.AppendLine(" UPDATE SupInvCheck_List SET Update_Who = @Update_Who, Update_Time = GETDATE() WHERE (Data_ID = @Parent_ID);");

                    //SupInvCheck_Model

                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.AddWithValue("Parent_ID", dataID);
                    cmd.Parameters.AddWithValue("SupID", dataItem.SupID);
                    cmd.Parameters.AddWithValue("SupName", dataItem.SupName);
                    cmd.Parameters.AddWithValue("SupMails", dataItem.SupMails);
                    cmd.Parameters.AddWithValue("PurWhoID", dataItem.PurWhoID);
                    cmd.Parameters.AddWithValue("PurWhoName", dataItem.PurWhoName);
                    cmd.Parameters.AddWithValue("PurWhoEmail", dataItem.PurWhoEmail);
                    cmd.Parameters.AddWithValue("StockShow", stockShow);
                    cmd.Parameters.AddWithValue("Token", _token);
                    cmd.Parameters.AddWithValue("Update_Who", fn_Param.CurrentUser);

                    //execute
                    return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
                }

            }

        }


        #endregion


        #endregion



        #region -----// Update //-----

        #region *** 到貨狀況 S ***
        /// <summary>
        /// 資材理貨
        /// </summary>
        /// <param name="id">ERP訂單</param>
        /// <param name="act">動作</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_StockArea(string id, string act, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----

                sql.AppendLine(" IF (SELECT COUNT(*) FROM OpcsStatus_Rel_Stock WHERE (ErpID = @id)) > 0");
                sql.AppendLine(" BEGIN");

                sql.AppendLine(" UPDATE OpcsStatus_Rel_Stock");
                sql.AppendLine(" SET StockValue = @act, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (ErpID = @id);");

                sql.AppendLine(" END");
                sql.AppendLine(" ELSE");
                sql.AppendLine(" BEGIN");

                sql.AppendLine(" DECLARE @NewID AS BIGINT");
                sql.AppendLine(" SET @NewID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(UID), 0) + 1");
                sql.AppendLine("  FROM OpcsStatus_Rel_Stock");
                sql.AppendLine(" );");

                sql.AppendLine(" INSERT INTO OpcsStatus_Rel_Stock (");
                sql.AppendLine("  UID, ErpID, StockValue, Create_Time");
                sql.AppendLine("  ) VALUES (");
                sql.AppendLine("  @NewID, @id, @act, GETDATE()");
                sql.AppendLine(" );");

                sql.AppendLine(" END");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("act", act);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKSYSinANA, out ErrMsg);
            }
        }


        /// <summary>
        /// 資材包裝資料
        /// </summary>
        /// <param name="id">ERP訂單</param>
        /// <param name="val">值</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_BoxData(string id, string val, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----

                sql.AppendLine(" IF (SELECT COUNT(*) FROM OpcsStatus_Rel_Box WHERE (ErpID = @id)) > 0");
                sql.AppendLine(" BEGIN");

                sql.AppendLine(" UPDATE OpcsStatus_Rel_Box");
                sql.AppendLine(" SET BoxValue = @val, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (ErpID = @id);");

                sql.AppendLine(" END");
                sql.AppendLine(" ELSE");
                sql.AppendLine(" BEGIN");

                sql.AppendLine(" DECLARE @NewID AS BIGINT");
                sql.AppendLine(" SET @NewID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(UID), 0) + 1");
                sql.AppendLine("  FROM OpcsStatus_Rel_Box");
                sql.AppendLine(" );");

                sql.AppendLine(" INSERT INTO OpcsStatus_Rel_Box (");
                sql.AppendLine("  UID, ErpID, BoxValue, Create_Time");
                sql.AppendLine("  ) VALUES (");
                sql.AppendLine("  @NewID, @id, @val, GETDATE()");
                sql.AppendLine(" );");

                sql.AppendLine(" END");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("val", val);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKSYSinANA, out ErrMsg);
            }
        }

        #endregion *** 到貨狀況 E ***



        #region *** 外廠包材庫存盤點 S ***
        /// <summary>
        /// [外廠包材庫存盤點] 更新資料
        /// </summary>
        /// <param name="instance">SupInvList</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool UpdateSupInvBase(SupInvList instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 執行 -----
                sql.AppendLine(" UPDATE SupInvCheck_List");
                sql.AppendLine(" SET Subject = @Subject, TaskTime = @TaskTime, IsOnTask = @IsOnTask");
                sql.AppendLine(" , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID);");
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Subject", instance.Subject);
                cmd.Parameters.AddWithValue("TaskTime", instance.TaskTime);
                cmd.Parameters.AddWithValue("IsOnTask", instance.IsOnTask);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        #endregion


        #endregion



        #region -----// Delete //-----

        #region *** 外廠包材庫存盤點 S ***
        /// <summary>
        /// [開案中客訴] 刪除資料
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete_SupInvSend(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM SupInvCheck_Model WHERE (Parent_ID = @Data_ID);");
                sql.AppendLine(" DELETE FROM SupInvCheck_Supplier WHERE (Parent_ID = @Data_ID);");
                sql.AppendLine(" DELETE FROM SupInvCheck_List WHERE (Data_ID = @Data_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        public bool Delete_SupplierItem(string parentID, string supID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM SupInvCheck_Model WHERE (Parent_ID = @Parent_ID) AND (SupID = @SupID);");
                sql.AppendLine(" DELETE FROM SupInvCheck_Supplier WHERE (Parent_ID = @Parent_ID) AND (SupID = @SupID);");
                sql.AppendLine(" UPDATE SupInvCheck_List SET Update_Who = @Update_Who, Update_Time = GETDATE() WHERE (Data_ID = @Parent_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", parentID);
                cmd.Parameters.AddWithValue("SupID", supID);
                cmd.Parameters.AddWithValue("Update_Who", fn_Param.CurrentUser);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        #endregion *** 外廠包材庫存盤點 E ***

        #endregion



        #region -----// Others //-----

        /// <summary>
        /// 取資料庫名稱
        /// </summary>
        /// <param name="dbs">TW/SH/SZ</param>
        /// <returns></returns>
        private string GetDBName(string dbs)
        {
            switch (dbs.ToUpper())
            {
                case "SH":
                    return "SHPK2";

                case "SZ":
                    return "ProUnion";

                default:
                    return "prokit2";
            }
        }


        /// <summary>
        /// 回傳對應公司別的主要倉別
        /// </summary>
        /// <param name="dbs">TW/SH/SZ</param>
        /// <returns></returns>
        private string GetStockType(string dbs)
        {
            switch (dbs)
            {
                case "SH":
                    return "12";

                default:
                    //TW
                    return "01";
            }
        }


        /// <summary>
        /// 供應商開頭第一碼
        /// </summary>
        /// <param name="dbs">TW/SH/SZ</param>
        /// <returns></returns>
        private string GetSupplierFirstID(string dbs)
        {
            switch (dbs.ToUpper())
            {
                case "SH":
                    return "2";

                case "SZ":
                    return "6";

                default:
                    return "1";
            }
        }


        /// <summary>
        /// BPM FLOW Table Name
        /// </summary>
        /// <param name="dbs">TW/SH/SZ</param>
        /// <returns></returns>
        private string GetFlowTable(string dbs)
        {
            switch (dbs.ToUpper())
            {
                case "TW":
                    return "PK_FO_19";

                case "SH":
                    return "SH_FO_19";

                default:
                    return "TRI_FO_19";
            }
        }


        /// <summary>
        /// 到貨狀況 - 設定所有表頭欄位
        /// </summary>
        /// <returns></returns>
        public List<OpcsColumn> SetOpcsTableHeader()
        {
            /*
              AllCol = colID / colName / sort(標頭排序)
              DeptRel = DeptID / colID
            */
            //定義所有欄位
            List<OpcsColumn> allCol = new List<OpcsColumn>();
            allCol.Add(new OpcsColumn(1, "序號", 1));
            allCol.Add(new OpcsColumn(2, "產品", 2));
            allCol.Add(new OpcsColumn(3, "訂單<br />未出數量", 3));
            allCol.Add(new OpcsColumn(4, "全部<br />未出數量", 4));
            allCol.Add(new OpcsColumn(5, "庫存", 5));
            allCol.Add(new OpcsColumn(6, "不足量", 6));
            allCol.Add(new OpcsColumn(7, "生產待入庫", 7));
            allCol.Add(new OpcsColumn(8, "預計進", 8));
            allCol.Add(new OpcsColumn(9, "計畫進", 9));
            allCol.Add(new OpcsColumn(10, "屬性", 10));
            allCol.Add(new OpcsColumn(11, "預交日", 11));
            allCol.Add(new OpcsColumn(12, "安全<br />存量", 12));
            allCol.Add(new OpcsColumn(13, "儲位", 13));
            allCol.Add(new OpcsColumn(14, "主供<br />應商", 14));
            allCol.Add(new OpcsColumn(15, "採購<br />廠商", 15));
            allCol.Add(new OpcsColumn(16, "採購單號<br />預交日", 16));
            allCol.Add(new OpcsColumn(17, "採購數量", 17));
            allCol.Add(new OpcsColumn(18, "進貨數量", 18));
            allCol.Add(new OpcsColumn(19, "未進貨數量", 19));
            allCol.Add(new OpcsColumn(20, "產品<br />圖號", 20));
            allCol.Add(new OpcsColumn(21, "製令單號", 21));
            allCol.Add(new OpcsColumn(22, "實際完工日", 22));
            allCol.Add(new OpcsColumn(23, "製令狀態", 23));
            allCol.Add(new OpcsColumn(24, "入庫量", 24));
            allCol.Add(new OpcsColumn(25, "資材<br />理貨", 25));
            allCol.Add(new OpcsColumn(26, "客戶<br />品號", 27));
            allCol.Add(new OpcsColumn(27, "特別注意事項", 28));
            allCol.Add(new OpcsColumn(28, "箱號/<br />包裝資料", 26));

            return allCol;
        }


        /// <summary>
        /// 到貨狀況 - 設定指定部門關聯的顯示欄位
        /// </summary>
        /// <param name="deptID">部門代號</param>
        /// <returns></returns>
        public List<OpcsDept> SetOpcsDept(string compID, string deptID)
        {
            //Step1:塞入所有欄位
            List<OpcsDept> deptRel = new List<OpcsDept>();
            for (int row = 1; row <= 28; row++)
            {
                deptRel.Add(new OpcsDept(row, deptID));
            }

            //Step2:填入不要的欄位
            List<OpcsDept> deptUnRel = new List<OpcsDept>();

            //Step3:整理所需欄位
            List<OpcsDept> deptSetRel = new List<OpcsDept>();

            /*
              需客制欄位的部門
                150	生產部(24)
                151	採購部(20)
                190	資材部(22)
                999 上海(因上海都同樣版型，所以用 CompID 判斷)(19)
            */

            switch (deptID)
            {
                case "150":
                    //生產部(不要的欄位)
                    deptUnRel.Add(new OpcsDept(9, deptID));
                    deptUnRel.Add(new OpcsDept(13, deptID));
                    deptUnRel.Add(new OpcsDept(25, deptID));
                    deptUnRel.Add(new OpcsDept(28, deptID));


                    break;

                case "151":
                    //採購部(不要的欄位)
                    deptUnRel.Add(new OpcsDept(13, deptID));
                    deptUnRel.Add(new OpcsDept(21, deptID));
                    deptUnRel.Add(new OpcsDept(22, deptID));
                    deptUnRel.Add(new OpcsDept(23, deptID));
                    deptUnRel.Add(new OpcsDept(24, deptID));
                    deptUnRel.Add(new OpcsDept(25, deptID));
                    deptUnRel.Add(new OpcsDept(27, deptID));
                    deptUnRel.Add(new OpcsDept(28, deptID));

                    break;

                case "190":
                    //資材部(不要的欄位)
                    deptUnRel.Add(new OpcsDept(6, deptID));
                    deptUnRel.Add(new OpcsDept(7, deptID));
                    deptUnRel.Add(new OpcsDept(8, deptID));
                    deptUnRel.Add(new OpcsDept(9, deptID));
                    deptUnRel.Add(new OpcsDept(10, deptID));
                    deptUnRel.Add(new OpcsDept(12, deptID));

                    break;

                case "999":
                    //(不要的欄位)
                    deptUnRel.Add(new OpcsDept(7, deptID));
                    for (int row = 15; row <= 20; row++)
                    {
                        deptUnRel.Add(new OpcsDept(row, deptID));
                    }
                    deptUnRel.Add(new OpcsDept(25, deptID));
                    deptUnRel.Add(new OpcsDept(28, deptID));

                    break;

                default:
                    //ALL

                    break;
            }

            //過濾
            var query = deptRel
                .Where(fld => !deptUnRel.Select(f => f.colID).Contains(fld.colID));
            deptSetRel.AddRange(query);

            //return
            return deptSetRel;
        }

        #endregion


        public class OpcsColumn
        {
            public int colID
            {
                get { return this._colID; }
                set { this._colID = value; }
            }
            private int _colID;

            public string colName
            {
                get { return this._colName; }
                set { this._colName = value; }
            }
            private string _colName;

            public int colSort
            {
                get { return this._colSort; }
                set { this._colSort = value; }
            }
            private int _colSort;


            public OpcsColumn(int colID, string colName, int colSort)
            {
                this._colID = colID;
                this._colName = colName;
                this._colSort = colSort;
            }
        }


        public class OpcsDept
        {
            public int colID
            {
                get { return this._colID; }
                set { this._colID = value; }
            }
            private int _colID;

            public string deptID
            {
                get { return this._deptID; }
                set { this._deptID = value; }
            }
            private string _deptID;


            public OpcsDept(int colID, string deptID)
            {
                this._colID = colID;
                this._deptID = deptID;
            }
        }
    }
}

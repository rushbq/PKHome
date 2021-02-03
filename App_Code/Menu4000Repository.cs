using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Menu4000Data.Models;
using PKLib_Method.Methods;

/*
  [到貨狀況]-OpcsStatus:會JOIN舊版OPCS備註，故連線要連至PKANALYZER
  [延遲分析]-DelayShipStat:需要關聯至EFGP
  [外廠包材庫存盤點]-SupInvCheck
  [訂貨計劃]-PurPlan
  [標準成本]-PurProdCost
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

                /* 製令待領(未出數量計算用) */
                sql.AppendLine(" ;WITH TblWait AS (");
                sql.AppendLine(" 	SELECT p.ModelNo");
                sql.AppendLine(" 	 , p.[01] AS waitQty_01, p.[20] AS waitQty_20, p.[22] AS waitQty_22");
                sql.AppendLine(" 	 , p.[12] AS waitQty_12, p.[14] AS waitQty_14, p.[A01] AS waitQty_A01");
                sql.AppendLine(" 	FROM (");
                sql.AppendLine(" 		SELECT ISNULL(SUM(MOCTB.TB004 - MOCTB.TB005), 0) AS Qty");
                sql.AppendLine(" 		 , RTRIM(MOCTB.TB003) AS ModelNo");
                sql.AppendLine(" 		 , MOCTB.TB009 AS StockType");
                sql.AppendLine(" 		FROM [#dbname#].dbo.MOCTA WITH (NOLOCK)");
                sql.AppendLine(" 			INNER JOIN [#dbname#].dbo.MOCTB ON (MOCTB.TB001 = MOCTA.TA001) AND (MOCTB.TB002 = MOCTA.TA002)");
                sql.AppendLine(" 		WHERE (MOCTB.TB009 IN ('01', '20', '22', '12', '14', 'A01'))");
                sql.AppendLine(" 			AND (MOCTB.TB018 = 'Y')");
                sql.AppendLine(" 			AND (MOCTB.TB011 IN ('1', '2'))");
                sql.AppendLine(" 			AND (MOCTB.TB004 - MOCTB.TB005 > 0)");
                sql.AppendLine(" 			AND (MOCTA.TA011 IN ('1', '2', '3'))");
                //判斷公司別,新增條件
                switch (CompID)
                {
                    case "TW":
                        sql.Append(" AND (MOCTA.TA001 IN ('510', '513', '520', '524', '525'))");
                        break;
                }
                sql.AppendLine(" 		GROUP BY MOCTB.TB003, MOCTB.TB009");
                sql.AppendLine(" 	) t ");
                sql.AppendLine(" 	PIVOT (");
                sql.AppendLine(" 		SUM(Qty)");
                sql.AppendLine(" 		FOR StockType IN ([01], [20], [22], [12], [14], [A01])");
                sql.AppendLine(" 	) p");
                sql.AppendLine(" )");

                /* 待入庫 */
                sql.AppendLine(", TblunStock AS (");
                sql.AppendLine("    SELECT p.ModelNo");
                sql.AppendLine("     , p.[01] AS unStkQty_01, p.[20] AS unStkQty_20, p.[22] AS unStkQty_22");
                sql.AppendLine("     , p.[12] AS unStkQty_12, p.[14] AS unStkQty_14, p.[A01] AS unStkQty_A01");
                sql.AppendLine("    FROM (");
                sql.AppendLine("        SELECT ISNULL(SUM(MOCTA.TA015 - MOCTA.TA017) - SUM(MOCTB.TB004 - MOCTB.TB005), 0) AS Qty");
                sql.AppendLine("         , RTRIM(MOCTA.TA006) AS ModelNo");
                sql.AppendLine("         , MOCTB.TB009 AS StockType");
                sql.AppendLine("        FROM [#dbname#].dbo.MOCTA WITH (NOLOCK)");
                sql.AppendLine("            INNER JOIN [#dbname#].dbo.MOCTB ON (MOCTB.TB001 = MOCTA.TA001) AND (MOCTB.TB002 = MOCTA.TA002) AND (MOCTB.TB003 = MOCTA.TA006)");
                sql.AppendLine("        WHERE (MOCTB.TB009 IN ('01', '20', '22', '12', '14', 'A01'))");
                sql.AppendLine("            AND (MOCTA.TA013 <> 'V')");
                sql.AppendLine("            AND (MOCTA.TA011 IN ('1', '2', '3'))");
                //[系統條件] 1年內//
                sql.AppendLine("            AND (MOCTA.CREATE_DATE >= @CheckDate)");
                //判斷公司別,新增條件
                switch (CompID)
                {
                    case "TW":
                        sql.Append(" AND (MOCTA.TA001 IN('520', '513', '521', '525', '526', '527'))");
                        break;
                }
                sql.AppendLine("        GROUP BY MOCTA.TA006, MOCTB.TB009");
                sql.AppendLine("    ) t ");
                sql.AppendLine("    PIVOT (");
                sql.AppendLine("        SUM(Qty)");
                sql.AppendLine("        FOR StockType IN ([01], [20], [22], [12], [14], [A01])");
                sql.AppendLine("    ) p");
                sql.AppendLine(")");


                /* 庫存 */
                sql.AppendLine(", TblStock AS (");
                sql.AppendLine("    SELECT p.ModelNo");
                sql.AppendLine("     , p.[01] AS StkQty_01, p.[20] AS StkQty_20, p.[22] AS StkQty_22");
                sql.AppendLine("     , p.[12] AS StkQty_12, p.[14] AS StkQty_14, p.[A01] AS StkQty_A01");
                sql.AppendLine("    FROM(");
                sql.AppendLine("        SELECT ISNULL(SUM(MC007), 0) AS StkQty");
                sql.AppendLine("         , RTRIM(MC001) AS ModelNo");
                sql.AppendLine("         , MC002 AS StockType");
                sql.AppendLine("        FROM [#dbname#].dbo.INVMC WITH (NOLOCK)");
                sql.AppendLine("        WHERE (MC002 IN ('01', '20', '22', '12', '14', 'A01'))");
                sql.AppendLine("        GROUP BY MC001, MC002");
                sql.AppendLine("    ) t");
                sql.AppendLine("    PIVOT (");
                sql.AppendLine("        SUM(StkQty)");
                sql.AppendLine("        FOR StockType IN ([01], [20], [22], [12], [14], [A01])");
                sql.AppendLine("    ) p");
                sql.AppendLine(")");


                /* 預計銷(未出數量計算用) */
                sql.AppendLine(", TblPreSell AS (");
                sql.AppendLine("    SELECT p.ModelNo");
                sql.AppendLine("     , p.[01] AS PreSell_01, p.[20] AS PreSell_20, p.[22] AS PreSell_22");
                sql.AppendLine("     , p.[12] AS PreSell_12, p.[14] AS PreSell_14, p.[A01] AS PreSell_A01");
                sql.AppendLine("    FROM (");
                sql.AppendLine("        SELECT ISNULL(SUM(TD008 + TD024 - TD009), 0) AS PreSell");
                sql.AppendLine("         , RTRIM(TD004) AS ModelNo");
                sql.AppendLine("         , TD007 AS StockType");
                sql.AppendLine("        FROM [#dbname#].dbo.COPTD WITH (NOLOCK)");
                sql.AppendLine("        WHERE (TD021 = 'Y') AND (TD016 = 'N') AND (TD007 IN ('01', '20', '22', '12', '14', 'A01'))");
                sql.AppendLine("        GROUP BY TD004, TD007");
                sql.AppendLine("    ) t");
                sql.AppendLine("    PIVOT (");
                sql.AppendLine("        SUM(PreSell)");
                sql.AppendLine("        FOR StockType IN ([01], [20], [22], [12], [14], [A01])");
                sql.AppendLine("    ) p");
                sql.AppendLine(")");


                /* 預計進 */
                sql.AppendLine(", TblPreIn AS (");
                sql.AppendLine("	SELECT p.ModelNo");
                sql.AppendLine("	 , p.[01] AS PreIN_01, p.[20] AS PreIN_20, p.[22] AS PreIN_22");
                sql.AppendLine("	 , p.[12] AS PreIN_12, p.[14] AS PreIN_14, p.[A01] AS PreIN_A01");
                sql.AppendLine("	FROM (");
                sql.AppendLine("		SELECT ISNULL(SUM(TD008 - TD015), 0) AS PreIN");
                sql.AppendLine("		 , RTRIM(TD004) AS ModelNo");
                sql.AppendLine("		 , TD007 AS StockType");
                sql.AppendLine("		FROM [#dbname#].dbo.PURTD WITH (NOLOCK)");
                sql.AppendLine("		WHERE (TD016 = 'N') AND (TD018 = 'Y') AND (TD007 IN ('01', '20', '22', '12', '14', 'A01'))");
                //判斷公司別,新增條件
                switch (CompID)
                {
                    case "TW":
                        sql.Append(" AND (TD001 IN ('3301', '3302', '3304', '3307','3322'))");
                        break;
                }
                sql.AppendLine("		GROUP BY TD004, TD007");
                sql.AppendLine("	) t ");
                sql.AppendLine("	PIVOT (");
                sql.AppendLine("		SUM(PreIN)");
                sql.AppendLine("		FOR StockType IN ([01], [20], [22], [12], [14], [A01])");
                sql.AppendLine("	) p");
                sql.AppendLine(")");


                /* 計劃進 */
                sql.AppendLine(", TblPlanIn AS (");
                sql.AppendLine("	SELECT p.ModelNo");
                sql.AppendLine("	 , p.[01] AS PlanIN_01, p.[20] AS PlanIN_20, p.[22] AS PlanIN_22");
                sql.AppendLine("	 , p.[12] AS PlanIN_12, p.[14] AS PlanIN_14, p.[A01] AS PlanIN_A01");
                sql.AppendLine("	FROM (");
                sql.AppendLine("		SELECT ISNULL(SUM(TD008 - TD015), 0) AS PlanIN");
                sql.AppendLine("		 , RTRIM(TD004) AS ModelNo");
                sql.AppendLine("		 , TD007 AS StockType");
                sql.AppendLine("		FROM [#dbname#].dbo.PURTD WITH (NOLOCK)");
                sql.AppendLine("		WHERE (TD016 = 'N') AND (TD018 = 'N') AND (TD007 IN ('01', '20', '22', '12', '14', 'A01'))");
                sql.AppendLine("		GROUP BY TD004, TD007");
                sql.AppendLine("	) t ");
                sql.AppendLine("	PIVOT (");
                sql.AppendLine("		SUM(PlanIN)");
                sql.AppendLine("		FOR StockType IN ([01], [20], [22], [12], [14], [A01])");
                sql.AppendLine("	) p");
                sql.AppendLine(")");


                #region ** TblBase **
                sql.AppendLine(" , TblBase AS (");
                sql.AppendLine(" SELECT");
                sql.AppendLine("     RTRIM(COPTC.TC001) AS Order_FID");
                sql.AppendLine("     , RTRIM(COPTC.TC002) AS Order_SID");
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

                sql.AppendLine(" FROM [#dbname#].dbo.COPTC WITH(NOLOCK)");
                sql.AppendLine("   INNER JOIN [#dbname#].dbo.COPTD WITH (NOLOCK)ON COPTC.TC001 = COPTD.TD001 AND COPTC.TC002 = COPTD.TD002");
                sql.AppendLine("   INNER JOIN [#dbname#].dbo.INVMB WITH(NOLOCK) ON COPTD.TD004 = INVMB.MB001");
                //PURMA 廠商基本資料檔(帶出主供應商)
                sql.AppendLine("    LEFT JOIN [#dbname#].dbo.PURMA WITH(NOLOCK) ON INVMB.MB032 = PURMA.MA001");
                //INVMC 品號庫別檔
                sql.AppendLine("    LEFT JOIN [#dbname#].dbo.INVMC WITH(NOLOCK) ON INVMB.MB001 = INVMC.MC001 AND INVMC.MC002 = @StockType");
                //COPTH 銷貨單單身檔
                sql.AppendLine("    LEFT JOIN [#dbname#].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
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
                sql.AppendLine(" FROM [#dbname#].dbo.PURTD WITH (NOLOCK)");
                //PURTC 採購單單頭檔
                sql.AppendLine(" LEFT JOIN [#dbname#].dbo.PURTC WITH (NOLOCK) ON PURTC.TC001 = PURTD.TD001 AND PURTC.TC002 = PURTD.TD002");
                //PURTH 進貨單單身檔
                sql.AppendLine(" LEFT JOIN [#dbname#].dbo.PURTH WITH (NOLOCK) ON PURTH.TH011 = PURTD.TD001 AND PURTH.TH012 = PURTD.TD002 AND PURTH.TH013 = PURTD.TD003");
                //PURMA 採購廠商
                sql.AppendLine(" LEFT JOIN [#dbname#].dbo.PURMA WITH (NOLOCK) ON PURMA.MA001 = PURTC.TC004");
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
                sql.AppendLine("  , (");
                sql.AppendLine("     SELECT TOP 1 MG200 FROM [{0}].dbo.COPMG WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("     WHERE (MG001 = TblBase.CustID) AND (MG002 = TblBase.ModelNo)");
                sql.AppendLine("  ) AS ProdRemark"); //產品特別注意事項
                sql.AppendLine("  , ISNULL(CopRemk.REMK, ISNULL(ORDERRemk.REMK, '')) AS OrderRemark"); //客戶注意事項
                sql.AppendLine("  , CAST(ISNULL(MOCTG.TG011, 0) AS INT) AS MakeStockQty"); //入庫數量

                sql.AppendLine(" , CAST(ISNULL(TblStock.StkQty_01, 0) AS INT) AS StkQty01");    //庫存(01)
                sql.AppendLine(" , CAST(ISNULL(TblStock.StkQty_20, 0) AS INT) AS StkQty20");    //庫存(20)
                sql.AppendLine(" , CAST(ISNULL(TblStock.StkQty_22, 0) AS INT) AS StkQty22");    //庫存(22)
                sql.AppendLine(" , CAST(ISNULL(TblStock.StkQty_12, 0) AS INT) AS StkQty12");    //庫存(12)
                sql.AppendLine(" , CAST(ISNULL(TblStock.StkQty_14, 0) AS INT) AS StkQty14");    //庫存(14)
                sql.AppendLine(" , CAST(ISNULL(TblStock.StkQty_A01, 0) AS INT) AS StkQtyA01");    //庫存(A01)

                sql.AppendLine(" , CAST(ISNULL(TblPreIn.PreIN_01, 0) AS INT) AS PreInQty01");    //預計進(01)
                sql.AppendLine(" , CAST(ISNULL(TblPreIn.PreIN_20, 0) AS INT) AS PreInQty20");    //預計進(20)
                sql.AppendLine(" , CAST(ISNULL(TblPreIn.PreIN_22, 0) AS INT) AS PreInQty22");    //預計進(22)
                sql.AppendLine(" , CAST(ISNULL(TblPreIn.PreIN_12, 0) AS INT) AS PreInQty12");    //預計進(12)
                sql.AppendLine(" , CAST(ISNULL(TblPreIn.PreIN_14, 0) AS INT) AS PreInQty14");    //預計進(14)
                sql.AppendLine(" , CAST(ISNULL(TblPreIn.PreIN_A01, 0) AS INT) AS PreInQtyA01");    //預計進(A01)

                sql.AppendLine(" , CAST(ISNULL(TblPlanIn.PlanIn_01, 0) AS INT) AS PlanInQty01");    //計劃進(01)
                sql.AppendLine(" , CAST(ISNULL(TblPlanIn.PlanIn_20, 0) AS INT) AS PlanInQty20");    //計劃進(20)
                sql.AppendLine(" , CAST(ISNULL(TblPlanIn.PlanIn_22, 0) AS INT) AS PlanInQty22");    //計劃進(22)
                sql.AppendLine(" , CAST(ISNULL(TblPlanIn.PlanIn_12, 0) AS INT) AS PlanInQty12");    //計劃進(12)
                sql.AppendLine(" , CAST(ISNULL(TblPlanIn.PlanIn_14, 0) AS INT) AS PlanInQty14");    //計劃進(14)
                sql.AppendLine(" , CAST(ISNULL(TblPlanIn.PlanIn_A01, 0) AS INT) AS PlanInQtyA01");    //計劃進(A01)

                sql.AppendLine(" , CAST(ISNULL(TblunStock.unStkQty_01, 0) AS INT) AS unStockQty01");    //生產待入庫(01)
                sql.AppendLine(" , CAST(ISNULL(TblunStock.unStkQty_20, 0) AS INT) AS unStockQty20");    //生產待入庫(20)
                sql.AppendLine(" , CAST(ISNULL(TblunStock.unStkQty_22, 0) AS INT) AS unStockQty22");    //生產待入庫(22)
                sql.AppendLine(" , CAST(ISNULL(TblunStock.unStkQty_12, 0) AS INT) AS unStockQty12");    //生產待入庫(12)
                sql.AppendLine(" , CAST(ISNULL(TblunStock.unStkQty_14, 0) AS INT) AS unStockQty14");    //生產待入庫(14)
                sql.AppendLine(" , CAST(ISNULL(TblunStock.unStkQty_A01, 0) AS INT) AS unStockQtyA01");    //生產待入庫(A01)

                /* 計算全部未出數量(預計銷 + 製令待領) */
                sql.AppendLine(", CAST(ISNULL(TblPreSell.PreSell_01, 0) + ISNULL(TblWait.waitQty_01, 0) AS INT) AS unOutQty01");    //未出數量(01)
                sql.AppendLine(", CAST(ISNULL(TblPreSell.PreSell_20, 0) + ISNULL(TblWait.waitQty_20, 0) AS INT) AS unOutQty20");    //未出數量(20)
                sql.AppendLine(", CAST(ISNULL(TblPreSell.PreSell_22, 0) + ISNULL(TblWait.waitQty_22, 0) AS INT) AS unOutQty22");    //未出數量(22)
                sql.AppendLine(", CAST(ISNULL(TblPreSell.PreSell_12, 0) + ISNULL(TblWait.waitQty_12, 0) AS INT) AS unOutQty12");    //未出數量(12)
                sql.AppendLine(", CAST(ISNULL(TblPreSell.PreSell_14, 0) + ISNULL(TblWait.waitQty_14, 0) AS INT) AS unOutQty14");    //未出數量(14)
                sql.AppendLine(", CAST(ISNULL(TblPreSell.PreSell_A01, 0) + ISNULL(TblWait.waitQty_A01, 0) AS INT) AS unOutQtyA01 ");    //未出數量(A01)

                /* 不足量(庫存 + 生產待入庫 - 全部未出數量) */
                sql.AppendLine(" , (CAST(ISNULL(TblStock.StkQty_01, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_01, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_01, 0) + ISNULL(TblWait.waitQty_01, 0) AS INT)) AS ShortQty01");    //不足量(01)
                sql.AppendLine(" , (CAST(ISNULL(TblStock.StkQty_20, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_20, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_20, 0) + ISNULL(TblWait.waitQty_20, 0) AS INT)) AS ShortQty20");    //不足量(20)
                sql.AppendLine(" , (CAST(ISNULL(TblStock.StkQty_22, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_22, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_22, 0) + ISNULL(TblWait.waitQty_22, 0) AS INT)) AS ShortQty22");    //不足量(22)
                sql.AppendLine(" , (CAST(ISNULL(TblStock.StkQty_12, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_12, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_12, 0) + ISNULL(TblWait.waitQty_12, 0) AS INT)) AS ShortQty12");    //不足量(12)
                sql.AppendLine(" , (CAST(ISNULL(TblStock.StkQty_14, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_14, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_14, 0) + ISNULL(TblWait.waitQty_14, 0) AS INT)) AS ShortQty14");    //不足量(14)
                sql.AppendLine(" , (CAST(ISNULL(TblStock.StkQty_A01, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_A01, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_A01, 0) + ISNULL(TblWait.waitQty_A01, 0) AS INT)) AS ShortQtyA01");    //不足量(A01)

                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  LEFT JOIN TblStock ON TblBase.ModelNo = TblStock.ModelNo");
                sql.AppendLine("  LEFT JOIN TblunStock ON TblBase.ModelNo = TblunStock.ModelNo");
                sql.AppendLine("  LEFT JOIN TblWait ON TblBase.ModelNo = TblWait.ModelNo");
                sql.AppendLine("  LEFT JOIN TblPreSell ON TblBase.ModelNo = TblPreSell.ModelNo");
                sql.AppendLine("  LEFT JOIN TblPreIn ON TblBase.ModelNo = TblPreIn.ModelNo");
                sql.AppendLine("  LEFT JOIN TblPlanIn ON TblBase.ModelNo = TblPlanIn.ModelNo");
                sql.AppendLine("  LEFT JOIN TblPur ON TblBase.Order_FID = TblPur.Ref_FID AND TblBase.Order_SID = TblPur.Ref_SID AND TblBase.OrderSno = TblPur.Ref_OrderSno AND TblBase.ModelNo = TblPur.ModelNo");
                sql.AppendLine("  LEFT JOIN TblMake ON TblBase.Order_FID = TblMake.Ref_FID AND TblBase.Order_SID = TblMake.Ref_SID AND TblBase.OrderSno = TblMake.Ref_OrderSno AND TblBase.ModelNo = TblMake.ModelNo");
                //客戶
                sql.AppendLine("  LEFT JOIN [#dbname#].dbo.COPMA WITH (NOLOCK) ON TblBase.CustID = COPMA.MA001");
                //EF 訂單備註(DB = PKANALYZER)
                sql.AppendLine("  LEFT JOIN [#dbname#].dbo.CopRemk ON CopRemk.TC001 = TblBase.Order_FID AND CopRemk.TC002 = TblBase.Order_SID");
                sql.AppendLine("  LEFT JOIN [#dbname#].dbo.ORDERRemk ON ORDERRemk.MA001 = TblBase.CustID");
                //MOCTG 生產入庫單身檔(筆數過多,放在最外層JOIN)
                sql.AppendLine("  LEFT JOIN [#dbname#].dbo.MOCTG WITH (NOLOCK) ON TblBase.ModelNo = MOCTG.TG004 AND TblMake.Make_FID = MOCTG.TG014 AND TblMake.Make_SID = MOCTG.TG015 AND TblBase.StockType = MOCTG.TG010");

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
                                        //不足量 <= -1
                                        switch (CompID)
                                        {
                                            case "TW":
                                                sql.Append(" AND (");
                                                sql.Append(" (CAST(ISNULL(TblStock.StkQty_01, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_01, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_01, 0) + ISNULL(TblWait.waitQty_01, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_20, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_20, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_20, 0) + ISNULL(TblWait.waitQty_20, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_22, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_22, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_22, 0) + ISNULL(TblWait.waitQty_22, 0) AS INT)) <= -1");
                                                sql.Append(" )");
                                                break;

                                            case "SH":
                                                sql.Append(" AND (");
                                                sql.Append(" (CAST(ISNULL(TblStock.StkQty_12, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_12, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_12, 0) + ISNULL(TblWait.waitQty_12, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_14, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_14, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_14, 0) + ISNULL(TblWait.waitQty_14, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_A01, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_A01, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_A01, 0) + ISNULL(TblWait.waitQty_A01, 0) AS INT)) <= -1");
                                                sql.Append(" )");
                                                break;
                                        }

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
                                        sql.Append(" AND LEFT(TblBase.Main_SupplierID, 1) = '{0}'".FormatThis(GetSupplierFirstID(CompID)));
                                        //不足量+預計進 <= -1
                                        switch (CompID)
                                        {
                                            case "TW":
                                                sql.Append(" AND (");
                                                sql.Append(" (CAST(ISNULL(TblStock.StkQty_01, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_01, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_01, 0) + ISNULL(TblWait.waitQty_01, 0) AS INT) + CAST(ISNULL(TblPreIn.PreIN_01, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_20, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_20, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_20, 0) + ISNULL(TblWait.waitQty_20, 0) AS INT) + CAST(ISNULL(TblPreIn.PreIN_20, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_22, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_22, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_22, 0) + ISNULL(TblWait.waitQty_22, 0) AS INT) + CAST(ISNULL(TblPreIn.PreIN_22, 0) AS INT)) <= -1");
                                                sql.Append(" )");
                                                break;

                                            case "SH":
                                                sql.Append(" AND (");
                                                sql.Append(" (CAST(ISNULL(TblStock.StkQty_12, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_12, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_12, 0) + ISNULL(TblWait.waitQty_12, 0) AS INT) + CAST(ISNULL(TblPreIn.PreIN_12, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_14, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_14, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_14, 0) + ISNULL(TblWait.waitQty_14, 0) AS INT) + CAST(ISNULL(TblPreIn.PreIN_14, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_A01, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_A01, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_A01, 0) + ISNULL(TblWait.waitQty_A01, 0) AS INT) + CAST(ISNULL(TblPreIn.PreIN_A01, 0) AS INT)) <= -1");
                                                sql.Append(" )");
                                                break;
                                        }
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
                                        sql.Append("     AND (TblBase.unShip_OrderQty > 0)");

                                        //不足量 <= -1
                                        switch (CompID)
                                        {
                                            case "TW":
                                                sql.Append(" AND (");
                                                sql.Append(" (CAST(ISNULL(TblStock.StkQty_01, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_01, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_01, 0) + ISNULL(TblWait.waitQty_01, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_20, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_20, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_20, 0) + ISNULL(TblWait.waitQty_20, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_22, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_22, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_22, 0) + ISNULL(TblWait.waitQty_22, 0) AS INT)) <= -1");
                                                sql.Append(" )");
                                                break;

                                            case "SH":
                                                sql.Append(" AND (");
                                                sql.Append(" (CAST(ISNULL(TblStock.StkQty_12, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_12, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_12, 0) + ISNULL(TblWait.waitQty_12, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_14, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_14, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_14, 0) + ISNULL(TblWait.waitQty_14, 0) AS INT)) <= -1");
                                                sql.Append(" OR (CAST(ISNULL(TblStock.StkQty_A01, 0) AS INT) + CAST(ISNULL(TblunStock.unStkQty_A01, 0) AS INT) - CAST(ISNULL(TblPreSell.PreSell_A01, 0) + ISNULL(TblWait.waitQty_A01, 0) AS INT)) <= -1");
                                                sql.Append(" )");
                                                break;
                                        }

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


                //** replace dbname ##
                sql.Replace("#dbname#", dbName);

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

                        case "sDate":
                            sql.Append(" AND (WriteTime >= @sDate)");
                            break;
                        case "eDate":
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

                        case "sDate":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value + " 00:00:00"));
                            break;
                        case "eDate":
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


        #region *** BOM篩選-採購 S ***

        /// <summary>
        /// [BOM篩選] 所有資料 (GetBOMfilter)
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// 匯出時使用
        /// </remarks>
        public DataTable GetAllBOMfilter(Dictionary<string, string> search, string dbs, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetBOMfilter(search, dbs, 0, 9999999, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [BOM篩選] 資料清單 (GetBOMfilter)
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetBOMfilter(Dictionary<string, string> search, string dbs
            , int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataTable myDT = new DataTable();
                DataCnt = 0;    //資料總數

                //取得SQL語法
                subSql = GetSQL_BOMfilter(search, dbs);
                //取得SQL參數集合
                subParamList = GetParams_BOMfilter(search);

                string topParams = @"DECLARE @CheckDay AS VARCHAR(8), @DayOfYear AS VARCHAR(8)
SET @CheckDay = CONVERT(VARCHAR(8), GETDATE(), 112)
SET @DayOfYear = CONVERT(VARCHAR(8), DATEADD(DAY, -365, @CheckDay), 112)";


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    sql.AppendLine(topParams);

                    sql.AppendLine(" SELECT COUNT(TbAll.MainModelNo) AS TotalCnt FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    //sqlParamList.Add(new SqlParameter("@DataType", DataType));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, dbConn.DBS.PKExcel, out ErrMsg))
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

                    sql.AppendLine(topParams);

                    sql.AppendLine(" SELECT TbAll.* FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");
                    sql.AppendLine(" WHERE (TbAll.RowIdx >= @startRow) AND (TbAll.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TbAll.MainModelNo, TbAll.PartModelNo");

                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    //sqlParamList.Add(new SqlParameter("@DataType", DataType));
                    sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                    sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    myDT = dbConn.LookupDT(cmd, out ErrMsg);


                    //return
                    return myDT;
                }

                #endregion

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        /// <summary>
        /// [BOM篩選] 取得SQL查詢 (GetBOMfilter)
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <returns></returns>
        /// <see cref="GetShipData"/>
        private StringBuilder GetSQL_BOMfilter(Dictionary<string, string> search, string dbs)
        {
            StringBuilder sql = new StringBuilder();
            string dbName = "";
            string soTypeFilter = ""; //單別條件
            string langcode = "zh_TW";
            switch (dbs.ToUpper())
            {
                case "SH":
                    dbName = "SHPK2";
                    langcode = "zh_CN";
                    soTypeFilter = "'2313', '2315', '2341', '2342', '2343', '2345', '23B2', '23B3', '23B4', '23B6'";
                    break;

                default:
                    dbName = "prokit2";
                    soTypeFilter = "'2301', '2302', '2303', '2304', '2305', '2306', '2307', '2308', '2309', '2310', '2320', '2330', '2350'";
                    break;
            }

            //SQL查詢
            sql.AppendLine(" SELECT");
            sql.AppendLine("  RTRIM(Base.MC001) AS MainModelNo	--//主件品號");
            sql.AppendLine(" , RTRIM(DT.MD003) AS PartModelNo	--//子件品號");
            sql.AppendLine(" , Prod.Model_Name_zh_TW AS MainModelName	--//主件品名");
            sql.AppendLine(" , PartProd.Model_Name_{0} AS PartModelName	--//子件品名".FormatThis(langcode));
            sql.AppendLine(" , CAST(DT.MD006 AS INT) AS Qty	--//組成用量");
            sql.AppendLine(" , RTRIM(Prod.Provider) AS SupID, RTRIM(Sup.MA002) AS SupName	--//主供應商");
            sql.AppendLine(" , Prod.Ship_From	--//出貨地");
            sql.AppendLine(" , Prod.Substitute_Model_No_{0} AS MarketMsg	--//產銷訊息".FormatThis(dbs));
            sql.AppendLine(" , Prod.Warehouse_Class_ID, WareCls.Class_Name_{0} AS StockProp  --//倉管屬性".FormatThis(langcode));
            sql.AppendLine(" , REPLACE(Prod.Catelog_Vol, 'NULL', '') AS Vol	--//目錄");
            sql.AppendLine(" , REPLACE(Prod.Page, 'NULL', '') AS Page	--//頁次");
            sql.AppendLine(" , Prod.Date_Of_Listing	--//上市日期");
            sql.AppendLine(" , CONVERT(VARCHAR(10), Prod.Stop_Offer_Date, 111) AS Stop_Offer_Date  --//停售日");
            sql.AppendLine(" , ErpProd.MB025 AS ProdProp	--//品號屬性");
            sql.AppendLine(" , TblSOdata.SO_Date, TblSOdata.SO_CustID, Cust.MA002 AS CustName, ISNULL(TblSOdata.SO_Qty, 0) AS SO_Qty");
            sql.AppendLine(" , ISNULL(TblYearQty.YearQty, 0) AS YearQty");
            sql.AppendLine(" , ROW_NUMBER() OVER(ORDER BY Base.MC001, DT.MD003) AS RowIdx");
            sql.AppendLine(" FROM [##DBName##].dbo.BOMMC Base WITH(NOLOCK)");
            sql.AppendLine("  INNER JOIN [##DBName##].dbo.BOMMD DT WITH(NOLOCK) ON Base.MC001 = DT.MD001");
            sql.AppendLine("  INNER JOIN [##DBName##].dbo.INVMB ErpProd WITH(NOLOCK) ON Base.MC001 = ErpProd.MB001");
            sql.AppendLine("  INNER JOIN [ProductCenter].dbo.Prod_Item Prod ON Base.MC001 = Prod.Model_No COLLATE Chinese_Taiwan_Stroke_BIN");
            sql.AppendLine("  INNER JOIN [ProductCenter].dbo.Prod_Item PartProd ON DT.MD003 = PartProd.Model_No COLLATE Chinese_Taiwan_Stroke_BIN");
            sql.AppendLine("  LEFT JOIN [ProductCenter].dbo.Warehouse_Class WareCls ON WareCls.Class_ID = Prod.Warehouse_Class_ID");
            sql.AppendLine("  LEFT JOIN [##DBName##].dbo.PURMA Sup ON Sup.MA001 = Prod.Provider COLLATE Chinese_Taiwan_Stroke_BIN");
            /* 最近出貨資料(PARTITION排序後取第一筆) */
            sql.AppendLine(" LEFT JOIN (");
            sql.AppendLine("   SELECT TG003 AS SO_Date, TG004 AS SO_CustID, CAST(ISNULL(TH008, 0) AS INT) AS SO_Qty, TH004 AS SO_ModelNo");
            sql.AppendLine("   , RANK() OVER (");
            sql.AppendLine("      PARTITION BY TH004 /*依品號Group*/");
            sql.AppendLine("      ORDER BY TG003 DESC /*依日期排序*/");
            sql.AppendLine("     ) AS myTbSeq");
            sql.AppendLine("   FROM [##DBName##].dbo.COPTG WITH(NOLOCK)");
            sql.AppendLine("    INNER JOIN [##DBName##].dbo.COPTH WITH(NOLOCK) ON COPTG.TG001 = COPTH.TH001 AND COPTG.TG002 = COPTH.TH002");
            sql.AppendLine("   WHERE (COPTH.TH020 = 'Y')");
            /* 固定條件:單別篩選 */
            sql.AppendLine("    AND (COPTG.TG001 IN ({0}))".FormatThis(soTypeFilter));
            sql.AppendLine("  ) AS TblSOdata ON TblSOdata.SO_ModelNo = Base.MC001 AND TblSOdata.myTbSeq = 1");
            /* 一年內用量 */
            sql.AppendLine("  LEFT JOIN (");
            sql.AppendLine("     SELECT ISNULL(SUM(TH008), 0) AS YearQty, TH004 AS ModelNo");
            sql.AppendLine("     FROM [##DBName##].dbo.COPTG WITH(NOLOCK)");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.COPTH WITH(NOLOCK) ON COPTG.TG001 = COPTH.TH001 AND COPTG.TG002 = COPTH.TH002");
            sql.AppendLine("     WHERE (TG003 >= @DayOfYear) AND (TG003 <= @CheckDay) AND (COPTH.TH020 = 'Y')");
            /* 固定條件:單別篩選 */
            sql.AppendLine("      AND (COPTG.TG001 IN ({0}))".FormatThis(soTypeFilter));
            sql.AppendLine("     GROUP BY TH004");
            sql.AppendLine("  ) AS TblYearQty ON TblYearQty.ModelNo = Base.MC001");
            sql.AppendLine("  LEFT JOIN [##DBName##].dbo.COPMA Cust ON Cust.MA001 =TblSOdata.SO_CustID");

            /* --[預設條件] 確認碼=Y / 失效日>今日 OR 失效日空白 */
            sql.AppendLine(" WHERE (Base.MC016 = 'Y') AND ((DT.MD012 > @CheckDay) OR (DT.MD012 = ''))");

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
                        case "Stop":
                            //--[條件] 排除已停售
                            sql.Append(" AND (Prod.Provider NOT IN ('122002') AND (ISNULL(Prod.Stop_Offer_Date, '') = ''))");

                            break;

                        case "ModelNo":
                            //--[條件] 子件品號(必填)
                            sql.Append(" AND (UPPER(DT.MD003) = UPPER(@ModelNo))");

                            break;

                    }
                }
            }
            #endregion


            //Replace 指定字元
            sql.Replace("##DBName##", dbName);

            //return
            return sql;
        }


        /// <summary>
        /// [BOM篩選] 取得條件參數 (GetBOMfilter)
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetSQL_ShipData"/>
        private List<SqlParameter> GetParams_BOMfilter(Dictionary<string, string> search)
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
                        case "ModelNo":
                            sqlParamList.Add(new SqlParameter("@ModelNo", item.Value));

                            break;

                    }
                }
            }


            return sqlParamList;
        }


        #endregion *** BOM篩選-採購 E ***


        #region *** 訂貨計劃 S ***
        /// <summary>
        /// 訂貨計劃 - 上海 (dbName記得改)
        /// </summary>
        /// <param name="stockType">指定庫別(A/B/C),A=12, B=A01, C=合併倉</param>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow(從0開始)</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns>DataTable</returns>
        public IQueryable<PurPlanList> GetPurPlan_SH(string stockType,
            Dictionary<string, string> search
            , int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                StringBuilder sqlDeclare = new StringBuilder();
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                List<PurPlanList> dataList = new List<PurPlanList>(); //資料容器
                string mainSql = ""; //SQL主體
                string columnSql = ""; //SQL欄位
                string filterSQL = ""; //條件參數SQL
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> sqlParamList_Cnt = new List<SqlParameter>(); //SQL參數容器(cnt)
                DataTable myDT = new DataTable();
                DataCnt = 0;    //資料總數

                string dbName = "SHPK2"; //資料庫


                sqlDeclare.AppendLine("DECLARE @nDays AS INT");
                sqlDeclare.AppendLine("SET @nDays = @setDays");


                #region >> [前置作業] 篩選條件組合 <<

                //庫別固定條件
                string stockTarget = "";
                string qtyCount_Days = "";
                string qtyCount_Year = "";
                string qtyCount_Season = "";


                switch (stockType)
                {
                    case "A":
                        //12倉
                        stockTarget = "'12'";
                        qtyCount_Days = "ISNULL(TblQty_Days.Qty_12, 0)";
                        qtyCount_Year = "(ISNULL(TblQty_Year.Qty_12, 0))";
                        qtyCount_Season = "(ISNULL(TblQty_Season.Qty_12, 0))";
                        break;

                    case "B":
                        //A01倉
                        stockTarget = "'A01'";
                        qtyCount_Days = "ISNULL(TblQty_Days.Qty_A01, 0)";
                        qtyCount_Year = "(ISNULL(TblQty_Year.Qty_A01, 0))";
                        qtyCount_Season = "(ISNULL(TblQty_Season.Qty_A01, 0))";
                        break;

                    default:
                        stockTarget = "'A01','12'";
                        qtyCount_Days = "ISNULL(TblQty_Days.Qty_A01, 0) + ISNULL(TblQty_Days.Qty_12, 0)";
                        qtyCount_Year = "(ISNULL(TblQty_Year.Qty_A01, 0) + ISNULL(TblQty_Year.Qty_12, 0))";
                        qtyCount_Season = "(ISNULL(TblQty_Season.Qty_A01, 0) + ISNULL(TblQty_Season.Qty_12, 0))";
                        break;
                }


                #region search filter 

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "ModelNo":
                                //拆解輸入字串(1PK-036S#GE-123#MT-810)
                                string[] aryID = Regex.Split(item.Value, "#");
                                ArrayList aryLst = new ArrayList(aryID);

                                /*
                                 GetSQLParam:SQL WHERE IN的方法  ,ex:UPPER(ModelNo) IN ({0})
                                */
                                /* [篩選條件], 品號 */
                                filterSQL += " AND (ModelNo IN ({0}))".FormatThis(CustomExtension.GetSQLParam(aryLst, "pModel"));


                                //SQL參數組成
                                for (int row = 0; row < aryID.Count(); row++)
                                {
                                    sqlParamList.Add(new SqlParameter("@pModel" + row, aryID[row]));
                                    sqlParamList_Cnt.Add(new SqlParameter("@pModel" + row, aryID[row]));
                                }

                                break;


                            case "SupID":
                                /* [篩選條件], 供應商ID */
                                filterSQL += " AND (Supply_ID = @SupID)";

                                //SQL參數組成
                                sqlParamList.Add(new SqlParameter("@SupID", item.Value));
                                sqlParamList_Cnt.Add(new SqlParameter("@SupID", item.Value));

                                break;

                            case "nDays":
                                //SQL參數組成
                                sqlParamList.Add(new SqlParameter("@setDays", item.Value));
                                sqlParamList_Cnt.Add(new SqlParameter("@setDays", item.Value));

                                break;

                            case "CustomFilter":
                                /* 指定範圍條件 */
                                switch (stockType)
                                {
                                    case "A":
                                        //12倉
                                        #region --12範圍條件--
                                        switch (item.Value)
                                        {
                                            case "A":
                                                /* (A) 是否有擬定數量 = (庫存 + 預計進 + 計劃進 - 預計銷) < 安全存量 */
                                                filterSQL += " AND ((StockQty_12 + PreIN_12 + PlanIN_12 - PreSell_12) < SafeQty_12)";
                                                break;

                                            case "B":
                                                /* (B) 催貨量 < 0 */
                                                filterSQL += " AND ((StockQty_12 - PreSell_12) < 0)";
                                                break;

                                            case "C":
                                                /* (C) 安全存量不為0 */
                                                filterSQL += " AND (SafeQty_12 <> 0)";
                                                break;

                                            case "D":
                                                /* (D) 預計進 > 0 */
                                                filterSQL += " AND (PreIN_12 > 0)";
                                                break;

                                            case "E":
                                                /* (E) 計劃進 > 0 */
                                                filterSQL += " AND (PlanIN_12 > 0)";
                                                break;

                                            case "F":
                                                /* (F) 虛擬預計銷 > 0 */
                                                filterSQL += " AND (VirPreSell > 0)";
                                                break;

                                            case "G":
                                                /* (G) 近n天用量 > 0 */
                                                filterSQL += " AND (Qty_Days > 0)";
                                                break;

                                            case "H":
                                                /* (H) 近n天用量 = 0 */
                                                filterSQL += " AND (Qty_Days = 0)";
                                                break;

                                            case "I":
                                                /* (I) 近n天用量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (Qty_Days = 0) AND (UsefulQty > 0)";
                                                break;

                                            case "J":
                                                /* (J) 安全存量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (SafeQty_12 = 0) AND (UsefulQty > 0)";
                                                break;


                                            case "L":
                                                /* (L) 可用量 12 > 0 */
                                                filterSQL += " AND ((StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12) > 0)";
                                                break;

                                            case "M":
                                                /* 可用週轉月(M) ~ (S) */
                                                filterSQL += "AND (MonthTurn_12 < 1)";
                                                break;

                                            case "N":
                                                filterSQL += "AND (MonthTurn_12 < 2)";
                                                break;

                                            case "O":
                                                filterSQL += "AND (MonthTurn_12 < 2.5)";
                                                break;

                                            case "P":
                                                filterSQL += "AND (MonthTurn_12 < 3.5)";
                                                break;

                                            case "Q":
                                                filterSQL += "AND (MonthTurn_12 > 6)";
                                                break;

                                            case "R":
                                                filterSQL += "AND (MonthTurn_12 > 12)";
                                                break;

                                            case "S":
                                                filterSQL += "AND (MonthTurn_12 > 24)";
                                                break;

                                            default:
                                                break;
                                        }

                                        #endregion

                                        break;

                                    case "B":
                                        //A01倉
                                        #region --A01範圍條件--
                                        switch (item.Value)
                                        {
                                            case "A":
                                                /* (A) 是否有擬定數量 = (庫存 + 預計進 + 計劃進 - 預計銷) < 安全存量 */
                                                filterSQL += " AND ((StockQty_A01 + PreIN_A01 + PlanIN_A01 - PreSell_A01) < SafeQty_A01)";
                                                break;

                                            case "B":
                                                /* (B) 催貨量 < 0 */
                                                filterSQL += " AND ((StockQty_A01 - PreSell_A01) < 0)";
                                                break;

                                            case "C":
                                                /* (C) 安全存量不為0 */
                                                filterSQL += " AND (SafeQty_A01 <> 0)";
                                                break;

                                            case "D":
                                                /* (D) 預計進 > 0 */
                                                filterSQL += " AND (PreIN_A01 > 0)";
                                                break;

                                            case "E":
                                                /* (E) 計劃進 > 0 */
                                                filterSQL += " AND (PlanIN_A01 > 0)";
                                                break;

                                            case "F":
                                                /* (F) 虛擬預計銷 > 0 */
                                                filterSQL += " AND (VirPreSell > 0)";
                                                break;

                                            case "G":
                                                /* (G) 近n天用量 > 0 */
                                                filterSQL += " AND (Qty_Days > 0)";
                                                break;

                                            case "H":
                                                /* (H) 近n天用量 = 0 */
                                                filterSQL += " AND (Qty_Days = 0)";
                                                break;

                                            case "I":
                                                /* (I) 近n天用量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (Qty_Days = 0) AND (UsefulQty > 0)";
                                                break;

                                            case "J":
                                                /* (J) 安全存量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (SafeQty_A01 = 0) AND (UsefulQty > 0)";
                                                break;

                                            case "K":
                                                /* (K) 可用量 A01 > 0 */
                                                filterSQL += " AND ((StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) > 0)";
                                                break;

                                            //case "L":
                                            //    /* (L) 可用量 12 > 0 */
                                            //    filterSQL += " AND ((StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12) > 0)";
                                            //    break;

                                            case "M":
                                                /* 可用週轉月(M) ~ (S) */
                                                filterSQL += "AND (MonthTurn_A01 < 1)";
                                                break;

                                            case "N":
                                                filterSQL += "AND (MonthTurn_A01 < 2)";
                                                break;

                                            case "O":
                                                filterSQL += "AND (MonthTurn_A01 < 2.5)";
                                                break;

                                            case "P":
                                                filterSQL += "AND (MonthTurn_A01 < 3.5)";
                                                break;

                                            case "Q":
                                                filterSQL += "AND (MonthTurn_A01 > 6)";
                                                break;

                                            case "R":
                                                filterSQL += "AND (MonthTurn_A01 > 12)";
                                                break;

                                            case "S":
                                                filterSQL += "AND (MonthTurn_A01 > 24)";
                                                break;

                                            default:
                                                break;
                                        }

                                        #endregion
                                        break;

                                    default:
                                        //合併倉
                                        #region --合併倉範圍條件--
                                        switch (item.Value)
                                        {
                                            case "A":
                                                /* (A) 是否有擬定數量 = (庫存 + 預計進 + 計劃進 - 預計銷) < 安全存量 */
                                                filterSQL += " AND (((StockQty_A01 + PreIN_A01 + PlanIN_A01 - PreSell_A01) + (StockQty_12 + PreIN_12 + PlanIN_12 - PreSell_12)) < SafeQty_A01)";
                                                break;

                                            case "B":
                                                /* (B) 催貨量 < 0 */
                                                filterSQL += " AND (((StockQty_A01 - PreSell_A01) + (StockQty_12 - PreSell_12)) < 0)";
                                                break;

                                            case "C":
                                                /* (C) 安全存量不為0 */
                                                filterSQL += " AND ((SafeQty_A01 + SafeQty_12) <> 0)";
                                                break;

                                            case "D":
                                                /* (D) 預計進 > 0 */
                                                filterSQL += " AND ((PreIN_A01 + PreIN_12) > 0)";
                                                break;

                                            case "E":
                                                /* (E) 計劃進 > 0 */
                                                filterSQL += " AND ((PlanIN_A01 + PlanIN_12) > 0)";
                                                break;

                                            case "F":
                                                /* (F) 虛擬預計銷 > 0 */
                                                filterSQL += " AND (VirPreSell > 0)";
                                                break;

                                            case "G":
                                                /* (G) 近n天用量 > 0 */
                                                filterSQL += " AND (Qty_Days > 0)";
                                                break;

                                            case "H":
                                                /* (H) 近n天用量 = 0 */
                                                filterSQL += " AND (Qty_Days = 0)";
                                                break;

                                            case "I":
                                                /* (I) 近n天用量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (Qty_Days = 0) AND (UsefulQty > 0)";
                                                break;

                                            case "J":
                                                /* (J) 安全存量 = 0, 可用量 > 0 */
                                                filterSQL += " AND ((SafeQty_A01 + SafeQty_12) = 0) AND (UsefulQty > 0)";
                                                break;

                                            //case "K":
                                            //    /* (K) 可用量 A01 > 0 */
                                            //    filterSQL += " AND ((StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) > 0)";
                                            //    break;

                                            case "L":
                                                /* (L) 可用量 > 0 */
                                                filterSQL += " AND (((StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) + (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12)) > 0)";
                                                break;

                                            case "M":
                                                /* 可用週轉月(M) ~ (S) */
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) < 1)";
                                                break;

                                            case "N":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) < 2)";
                                                break;

                                            case "O":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) < 2.5)";
                                                break;

                                            case "P":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) < 3.5)";
                                                break;

                                            case "Q":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) > 6)";
                                                break;

                                            case "R":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) > 12)";
                                                break;

                                            case "S":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) > 24)";
                                                break;

                                            default:
                                                break;
                                        }

                                        #endregion
                                        break;
                                }


                                break;

                        }
                    }
                }
                #endregion

                #endregion


                #region >> [前置作業] SQL主體 <<

                mainSql = @"
/* 預計銷(訂單):TD016 = 結案碼, TD021 = 確認碼 */
;WITH Tbl_PreSell AS (
	SELECT p.ModelNo
	 , p.[12] AS PreSell_12, p.[A01] AS PreSell_A01
	FROM (
		SELECT ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0) AS PreSell
		 , RTRIM(TD004) AS ModelNo
		 , TD007 AS StockType
		FROM [##DBName##].dbo.COPTD WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TD016 = 'N') AND (TD021 = 'Y') AND (TD007 IN ('12', 'A01'))
		GROUP BY TD004, TD007
	) t 
	PIVOT (
		SUM(PreSell)
		FOR StockType IN ([12], [A01])
	) p
)

/* 虛擬預計銷(訂單):TD016 = 結案碼, TD021 = 確認碼 */
, Tbl_VirPreSell AS (
	SELECT ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0) AS VirPreSell
	 , RTRIM(TD004) AS ModelNo
	FROM [##DBName##].dbo.COPTD WITH (NOLOCK)
	WHERE (TD016 = 'N') AND (TD021 = 'Y') AND (TD001 = '22C1')
	GROUP BY TD004
)

/* 虛擬入(訂單):TD016 = 結案碼, TD021 = 確認碼 */
, Tbl_VirIn AS (
	SELECT p.ModelNo
	 , p.[12] AS VirIn_12, p.[A01] AS VirIn_A01
	FROM (
		SELECT ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0) AS VirIn
		 , RTRIM(TD004) AS ModelNo
		 , TD007 AS StockType
		FROM [##DBName##].dbo.COPTD WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TD016 = 'N') AND (TD021 = 'Y') AND (TD007 IN ('12', 'A01')) AND (TD001 = '2262')
		GROUP BY TD004, TD007
	) t 
	PIVOT (
		SUM(VirIn)
		FOR StockType IN ([12], [A01])
	) p
)

/* 預計進(採購單):TD016 = 結案碼, TD018 = 確認碼 */
, Tbl_PreIN AS (
	SELECT p.ModelNo
	 , p.[12] AS PreIN_12, p.[A01] AS PreIN_A01
	FROM (
		SELECT ISNULL(SUM(TD008 - TD015), 0) AS PreIN
		 , RTRIM(TD004) AS ModelNo
		 , TD007 AS StockType
		FROM [##DBName##].dbo.PURTD WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TD016 = 'N') AND (TD018 = 'Y') AND (TD007 IN ('12', 'A01'))
		GROUP BY TD004, TD007
	) t 
	PIVOT (
		SUM(PreIN)
		FOR StockType IN ([12], [A01])
	) p
)

/* 計劃進(採購單):TD016 = 結案碼, TD018 = 確認碼 */
, Tbl_PlanIN AS (
	SELECT p.ModelNo
	 , p.[12] AS PlanIN_12, p.[A01] AS PlanIN_A01
	FROM (
		SELECT ISNULL(SUM(TD008 - TD015), 0) AS PlanIN
		 , RTRIM(TD004) AS ModelNo
		 , TD007 AS StockType
		FROM [##DBName##].dbo.PURTD WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TD016 = 'N') AND (TD018 = 'N') AND (TD007 IN ('12', 'A01'))
		GROUP BY TD004, TD007
	) t 
	PIVOT (
		SUM(PlanIN)
		FOR StockType IN ([12], [A01])
	) p
)

/* 預計領:TA013,確認碼 <> 'V', TA011,狀態碼 = 1,2,3 */
, Tbl_PreGet AS (
	SELECT p.ModelNo
	 , p.[12] AS PreGet_12, p.[A01] AS PreGet_A01
	FROM (
		SELECT SUM(TB004 - TB005) AS PreGet
		 , RTRIM(TA006) AS ModelNo
		 , TA020 AS StockType
		FROM [##DBName##].dbo.MOCTA AS A WITH(NOLOCK)
		 INNER JOIN [##DBName##].dbo.MOCTB AS B WITH(NOLOCK) ON A.TA001 = B.TB001 AND A.TA002 = B.TB002 AND A.TA006 = B.TB003
		WHERE (A.TA013 <> 'V') AND (A.TA011 IN ('1', '2', '3'))
		 /* [指定條件] 所有庫別 */
		 AND (A.TA020 IN ('12', 'A01'))
		GROUP BY TA006, TA020
	) t 
	PIVOT (
		SUM(PreGet)
		FOR StockType IN ([12], [A01])
	) p
)

/* 計劃領 */
, Tbl_PlanOut AS (
	SELECT p.ModelNo
	 , p.[12] AS OutQty_12, p.[A01] AS OutQty_A01
	FROM (
		SELECT TB007 AS OutQty, RTRIM(TB005) AS ModelNo, TB008 AS StockType
		FROM [##DBName##].dbo.LRPTB WITH(NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TB008 IN ('12', 'A01'))
	) t 
	PIVOT (
		SUM(OutQty)
		FOR StockType IN ([12], [A01])
	) p
)

/* 庫存 */
, Tbl_Stock AS (
	SELECT p.ModelNo
	 , p.[12] AS StockQty_12, p.[A01] AS StockQty_A01
	FROM (
		SELECT MC007 AS StockQty, RTRIM(MC001) AS ModelNo, MC002 AS StockType
		FROM [##DBName##].dbo.INVMC WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (MC002 IN ('12', 'A01')) AND (MC001 <> '')
	) t 
	PIVOT (
		SUM(StockQty)
		FOR StockType IN ([12], [A01])
	) p
)
/* 安全庫存 */
, Tbl_SafeStock AS (
	SELECT p.ModelNo
	 , p.[12] AS SafeQty_12, p.[A01] AS SafeQty_A01
	FROM (
		SELECT MC004 AS StockQty, RTRIM(MC001) AS ModelNo, MC002 AS StockType
		FROM [##DBName##].dbo.INVMC WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (MC002 IN ('12', 'A01')) AND (MC001 <> '')
	) t 
	PIVOT (
		SUM(StockQty)
		FOR StockType IN ([12], [A01])
	) p
)

/* [欄位] - 近n天用量 (參數:nDays)(A01) */
, TblQty_Days AS (
	SELECT p.ModelNo
	 , p.[12] AS Qty_12, p.[A01] AS Qty_A01
	FROM (
		SELECT LA011 AS sumQty, RTRIM(LA001) AS ModelNo, LA009 AS StockType
		FROM [##DBName##].dbo.INVLA WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (LA005 = '-1') AND (LA009 IN ('12', 'A01'))
		 AND (LA006 IN ('S540','S550','2313','2315','2341','2342','2343','2345','23B1','23B2','23B3','23B4','23B6'))
		 AND (
			LA004 BETWEEN REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - @nDays, GETDATE()), 111),'/', '')
				AND REPLACE(CONVERT(VARCHAR(10), DATEADD(day, 0, GETDATE()), 111), '/', '')
			)
	) t 
	PIVOT (
		SUM(sumQty)
		FOR StockType IN ([12], [A01])
	) p
)

/* [欄位] - 全年平均月用量 */
, TblQty_Year AS (
	SELECT p.ModelNo
	 , p.[12] AS Qty_12, p.[A01] AS Qty_A01
	FROM (
		SELECT LA011 AS sumQty, RTRIM(LA001) AS ModelNo, LA009 AS StockType
		FROM [##DBName##].dbo.INVLA WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (LA005 = '-1') AND (LA009 IN ('12', 'A01'))
		 AND (LA006 IN ('S540','S550','2313','2315','2341','2342','2343','2345','23B1','23B2','23B3','23B4','23B6'))
		 AND (
			LA004 BETWEEN REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - 365, GETDATE()), 111), '/', '')
				AND REPLACE(CONVERT(VARCHAR(10), DATEADD(day, 0, GETDATE()), 111), '/', '')
			)
	) t 
	PIVOT (
		SUM(sumQty)
		FOR StockType IN ([12], [A01])
	) p
)

/* [欄位] - 去年當季平均用量 */
, TblQty_Season AS (
	SELECT p.ModelNo
	 , p.[12] AS Qty_12, p.[A01] AS Qty_A01
	FROM (
		SELECT LA011 AS sumQty, RTRIM(LA001) AS ModelNo, LA009 AS StockType
		FROM [##DBName##].dbo.INVLA WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (LA005 = '-1') AND (LA009 IN ('12', 'A01'))
		 AND (LA006 IN ('S540','S550','2313','2315','2341','2342','2343','2345','23B1','23B2','23B3','23B4','23B6'))
		 AND (
			LA004 BETWEEN REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - 365, GETDATE()), 111), '/', '')
				AND REPLACE(CONVERT(VARCHAR(10), GETDATE() - 275, 111), '/', '')
			)
	) t 
	PIVOT (
		SUM(sumQty)
		FOR StockType IN ([12], [A01])
	) p
)

, Tbl_WaitQty AS (
/*
	[欄位] - 待驗收(TH007)(A01)
	QMSTA 進貨檢驗單單頭檔
	PURTH 進貨單單身檔
	TA001, TH001 = 單別
	TA002, TH002 = 單號
	TA003, TH003 = 序號
	TA014, TH030 = 確認碼
	TH004 = 品號
*/
	SELECT RTRIM(PURTH.TH004) AS ModelNo, PURTH.TH007 AS WaitQty
	FROM [##DBName##].dbo.QMSTA WITH (NOLOCK)
		INNER JOIN [##DBName##].dbo.PURTH WITH (NOLOCK) ON QMSTA.TA001 = PURTH.TH001 AND QMSTA.TA002 = PURTH.TH002 AND QMSTA.TA003 = PURTH.TH003
	/* [指定條件] 單一庫別 */
	WHERE (QMSTA.TA014 = 'N') AND (PURTH.TH030 = 'N') AND (PURTH.TH009 IN (##tarStock##))	 
)

, Tbl_PurCnt AS (
	/* 進貨筆數加總(依指定天數)(參數:nDays)(A01) */
	SELECT RTRIM(PURTH.TH004) AS ModelNo, COUNT(*) AS CNT
	FROM [##DBName##].dbo.PURTG WITH (NOLOCK)
		INNER JOIN [##DBName##].dbo.PURTH WITH (NOLOCK) ON PURTG.TG001 = PURTH.TH001 AND PURTG.TG002 = PURTH.TH002
	/* [指定條件] 單一庫別 */
	WHERE (PURTH.TH009 IN (##tarStock##))
	 AND (PURTG.TG003 BETWEEN 
		REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - @nDays, GETDATE()), 111), '/', '') 
		AND REPLACE(CONVERT(VARCHAR(10), DATEADD(day, 0, GETDATE()), 111), '/', ''))
	GROUP BY PURTH.TH004
)
                    ";
                //置入條件SQL
                mainSql = mainSql.Replace("##tarStock##", stockTarget);
                mainSql = mainSql.Replace("##DBName##", dbName);

                #endregion


                #region >> SQL欄位 <<
                columnSql = @"
	SELECT TblPrint.*
		, RANK() OVER(ORDER BY TblPrint.ModelNo) AS RowIdx
	FROM (
		SELECT TblSection.* 
			/* 催貨量 = 庫存 - 預計銷 */
			, (StockQty_A01 - PreSell_A01) AS PushQty

			/* 可用量 A01 = 庫存 + 預計進 - 預計銷 + 計劃進 */
			, (StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) AS UsefulQty_A01
			/* 可用量 12 = 庫存 + 預計進 - 預計銷 + 計劃進 -預計領 - 計劃領 */
			, (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12) AS UsefulQty_12

			/*
			   擬定數量 = {
				 SET X = (庫存 + 預計進 + 計劃進 - 預計銷)
				 SET Y = ABS(安全存量 - X);
				 IF X >= 安全存量 THEN 0
				 ELSEIF Y > 最低補量 THEN ABS(Y)
				 ELSE 最低補量
				}
			*/
			, (CASE WHEN (StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) >= SafeQty_A01 THEN 0
				WHEN ABS(SafeQty_A01 - (StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01)) <= Min_Supply THEN Min_Supply
				ELSE ABS(SafeQty_A01 - (StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01))
				END) AS QTM_A01
			, (CASE WHEN (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12) >= SafeQty_12 THEN 0
				WHEN ABS(SafeQty_12 - (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12)) <= Min_Supply THEN Min_Supply
				ELSE ABS(SafeQty_12 - (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12))
				END) AS QTM_12

			/* 可用週轉月<MonthTurn_A01> A01 = (可用量 A01 / 年平均月用量) */
			, (CASE WHEN Qty_Year = 0 THEN 0
				ELSE
					CONVERT(FLOAT,ROUND(
						(StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) / Qty_Year
					, 2))
				END) AS MonthTurn_A01

			/* 可用週轉月<MonthTurn_12> 12 = (可用量 12 / 年平均月用量) */
			, (CASE WHEN Qty_Year = 0 THEN 0
				ELSE
					CONVERT(FLOAT,ROUND(
						(StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12) / Qty_Year
					, 2))
				END) AS MonthTurn_12

			/* 現有周轉月<NowMonthTurn_A01> ,依表指定A01 = ((庫存 - 預計銷) / 年平均月用量) */
			, (CASE WHEN Qty_Year = 0 THEN 0
				ELSE
					CONVERT(FLOAT,ROUND(
						(StockQty_A01 - PreSell_A01) / Qty_Year
					, 2))
				END) AS NowMonthTurn_A01

			/* 現有周轉月<NowMonthTurn_12> ,依表指定合併 = (總庫存 - 總預計銷 - 預計領12) / 年平均月用量 */
			, (CASE WHEN Qty_Year = 0 THEN 0
				ELSE
					CONVERT(FLOAT,ROUND(
						(StockQty_12 - (PreSell_A01 + PreSell_12) - PreGet_12) / Qty_Year
					, 2))
				END) AS NowMonthTurn_12
		FROM (
			SELECT RTRIM(INVMB.MB001) AS ModelNo
			 , INVMA.MA003 AS Item_Type /* 屬性 */
			 , RTRIM(INVMB.MB002) AS ModelName /* 品名 */
			 , ISNULL(REPLACE(INVMB.MB207, 'NULL', ''), '') AS ProdVol /* VOL */
			 , ISNULL(REPLACE(INVMB.MB208, 'NULL', ''), '') AS ProdPage /* PAGE */
			 , ISNULL(TblCheckPrice.Currency, '') AS Currency
 			 , CONVERT(FLOAT, ISNULL(TblCheckPrice.checkPrice, 0)) AS checkPrice /* 核價單價 */
			 , CONVERT(INT, ISNULL(Tbl_WaitQty.WaitQty, 0)) AS WaitQty /* 待驗收 */
			 , CONVERT(INT, ISNULL(Tbl_Stock.StockQty_A01, 0)) AS StockQty_A01 /* 庫存 A01 */
			 , CONVERT(INT, ISNULL(Tbl_SafeStock.SafeQty_A01, 0)) AS SafeQty_A01 /* 安全存量 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PreIN.PreIN_A01, 0)) AS PreIN_A01 /* 預計進 A01 */
			 , CONVERT(INT, ISNULL(Tbl_VirIn.VirIn_A01, 0)) AS VirIn_A01 /* 虛擬入 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PlanIN.PlanIN_A01, 0)) AS PlanIN_A01 /* 計劃進 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PreSell.PreSell_A01, 0)) AS PreSell_A01 /* 預計銷 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PreGet.PreGet_A01, 0)) AS PreGet_A01 /* 預計領 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PlanOut.OutQty_A01, 0)) AS PlanOut_A01 /* 計劃領 A01 */
			 , CONVERT(INT, ISNULL(Tbl_Stock.StockQty_12, 0)) AS StockQty_12 /* 庫存 12 */
			 , CONVERT(INT, ISNULL(Tbl_SafeStock.SafeQty_12, 0)) AS SafeQty_12 /* 安全存量 12 */
			 , CONVERT(INT, ISNULL(Tbl_PreIN.PreIN_12, 0)) AS PreIN_12 /* 預計進 12 */
			 , CONVERT(INT, ISNULL(Tbl_VirIn.VirIn_12, 0)) AS VirIn_12 /* 虛擬入 12 */
			 , CONVERT(INT, ISNULL(Tbl_PlanIN.PlanIN_12, 0)) AS PlanIN_12 /* 計劃進 12 */
			 , CONVERT(INT, ISNULL(Tbl_PreSell.PreSell_12, 0)) AS PreSell_12 /* 預計銷 12 */
			 , CONVERT(INT, ISNULL(Tbl_PreGet.PreGet_12, 0)) AS PreGet_12 /* 預計領 12 */
			 , CONVERT(INT, ISNULL(Tbl_PlanOut.OutQty_12, 0)) AS PlanOut_12 /* 計劃領 12 */

             , CONVERT(FLOAT, ##qtyDay##) AS Qty_Days /* 近N天用量 (依表指定A01/12) */
			 , CONVERT(FLOAT, ROUND(##qtyYear## / 12, 0)) AS Qty_Year /* 全年平均月用量(除12) (依表指定A01/12) */
             , CONVERT(FLOAT, ROUND(##qtySeason## / 3, 0)) AS Qty_Season /* 去年當季平均用量(除3) (依表指定A01/12) */

			 , CONVERT(INT, ISNULL(Tbl_VirPreSell.VirPreSell, 0)) AS VirPreSell /* 虛擬預計銷(ALL) */
			 , CONVERT(INT, ISNULL(Tbl_PurCnt.CNT, 0)) AS CNT /* 進貨筆數加總(依指定天數)(依表指定A01/12) */
			 , CONVERT(FLOAT, ROUND(ISNULL(SZInfo.QtyOfYear/12, 0), 0)) AS SZ_QtyOfYear /* 深圳全年平均月用量(除12)(歷史記錄) */
			 , CONVERT(INT, INVMB.MB039) AS Min_Supply /* 最低補量 */
			 , CONVERT(INT, INVMB.MB201) AS InBox_Qty /* 內盒數量 */
			 , CONVERT(INT, INVMB.MB073) AS Qty_Packing /* 一箱數量 */
			 , CONVERT(FLOAT, INVMB.MB071) AS OutBox_Cuft /* 整箱材積 */
             , CONVERT(INT, ISNULL(INVMB.MB220, 0)) AS MOQ /* 銷售MOQ */
			 , RTRIM(INVMB.MB202) AS Sub_item /* 產銷訊息 */
			 , PURMA.MA002 AS Supplier /* 供應商 */
			 , RTRIM(PURMA.MA001) AS Supply_ID
			FROM [##DBName##].dbo.INVMB WITH (NOLOCK) 
			 INNER JOIN [##DBName##].dbo.PURMA ON INVMB.MB032 = PURMA.MA001
			 INNER JOIN [##DBName##].dbo.INVMA ON INVMA.MA002 = INVMB.MB008
			 INNER JOIN [##DBName##].dbo.INVMC ON INVMB.MB001 = INVMC.MC001 AND INVMC.MC002 IN (##tarStock##) /* [指定條件] 單一庫別 */
			 LEFT JOIN Tbl_Stock ON INVMB.MB001 = Tbl_Stock.ModelNo
			 LEFT JOIN Tbl_SafeStock ON INVMB.MB001 = Tbl_SafeStock.ModelNo
			 LEFT JOIN Tbl_PreIN ON INVMB.MB001 = Tbl_PreIN.ModelNo
			 LEFT JOIN Tbl_PlanIN ON INVMB.MB001 = Tbl_PlanIN.ModelNo
			 LEFT JOIN Tbl_PreSell ON INVMB.MB001 = Tbl_PreSell.ModelNo
			 LEFT JOIN Tbl_VirIn ON INVMB.MB001 = Tbl_VirIn.ModelNo
			 LEFT JOIN Tbl_VirPreSell ON INVMB.MB001 = Tbl_VirPreSell.ModelNo
			 LEFT JOIN Tbl_PreGet ON INVMB.MB001 = Tbl_PreGet.ModelNo
			 LEFT JOIN TblQty_Days ON INVMB.MB001 = TblQty_Days.ModelNo
			 LEFT JOIN TblQty_Year ON INVMB.MB001 = TblQty_Year.ModelNo
			 LEFT JOIN TblQty_Season ON INVMB.MB001 = TblQty_Season.ModelNo
			 LEFT JOIN Tbl_WaitQty ON INVMB.MB001 = Tbl_WaitQty.ModelNo
			 LEFT JOIN Tbl_PurCnt ON INVMB.MB001 = Tbl_PurCnt.ModelNo
			 LEFT JOIN Tbl_PlanOut ON INVMB.MB001 = Tbl_PlanOut.ModelNo
 
			 /* 深圳資料庫用量欄位 */
			 LEFT JOIN [PKHistory].dbo.SQ_SZ_YearQty AS SZInfo ON INVMB.MB001 = SZInfo.ModelNo COLLATE Chinese_Taiwan_Stroke_BIN

			 /* 採購單資料 S (資料量大,不放在CTE) */
			 LEFT JOIN (
				SELECT
				(CASE WHEN ChkTb_1.ModelNo IS NULL THEN ChkTb_2.ModelNo ELSE ChkTb_1.ModelNo END) AS ModelNo 
				, (CASE WHEN ChkTb_1.checkPrice IS NULL THEN ChkTb_2.Price ELSE ChkTb_1.checkPrice END) AS checkPrice
				, (CASE WHEN ChkTb_1.SupID IS NULL THEN ChkTb_2.SupID ELSE ChkTb_1.SupID END) AS SupID
				, (CASE WHEN ChkTb_1.Currency IS NULL THEN ChkTb_2.Currency ELSE ChkTb_1.Currency END) AS Currency
				FROM (
					/*
					採購核價單
					[單頭]
					TL004 廠商代號 (Group Key)
					TL005 幣別
					TL003 核價日期 (ORDER BY)

					[單身]
					TM004 品號 (Group Key)
					TM010 單價
					TM015 失效日 = '' (條件)
					*/
					SELECT ChkPrice.ModelNo, ChkPrice.checkPrice, ChkPrice.SupID, ChkPrice.Currency
					FROM (
						SELECT Base.TL004 AS SupID, Base.TL005 AS Currency, DT.TM004 AS ModelNo, ISNULL(DT.TM010, 0) AS checkPrice
						, RANK() OVER (
							PARTITION BY Base.TL004, DT.TM004
							ORDER BY Base.TL003 DESC
						) AS myTbSeq	
						FROM [##DBName##].dbo.PURTL Base WITH(NOLOCK)
						 INNER JOIN [##DBName##].dbo.PURTM DT WITH(NOLOCK) ON Base.TL001 = DT.TM001 AND Base.TL002 = DT.TM002
						WHERE (Base.TL006 = 'Y') AND (DT.TM015 = '')
					) AS ChkPrice
					WHERE ChkPrice.myTbSeq = 1
				) AS ChkTb_1
				FULL OUTER JOIN
				(
					/*
					最近採購單價格
					[單頭]
					TG005 供應商代號
					TG007 幣別

					[單身]
					TH018 原幣單位進價
					TH004 品號 (Group Key)
					TH009 庫別 (條件)
					TH014 驗收日期 (ORDER BY)
					*/
					SELECT ChkDay.ModelNo, ChkDay.Price, ChkDay.SupID, ChkDay.Currency
					FROM (
						SELECT Base.TG005 AS SupID, Base.TG007 AS Currency, DT.TH004 AS ModelNo, ISNULL(DT.TH018, 0) AS Price
						, RANK() OVER (
							PARTITION BY DT.TH004
							ORDER BY DT.TH014 DESC, DT.TH001, DT.TH002 DESC
						) AS myTbSeq	
						FROM [##DBName##].dbo.PURTG Base WITH(NOLOCK)
							INNER JOIN [##DBName##].dbo.PURTH DT WITH(NOLOCK) ON Base.TG001 = DT.TH001 AND Base.TG002 = DT.TH002
						/* [指定條件] 單一庫別 */
						WHERE (DT.TH009 IN (##tarStock##))
					) AS ChkDay
					WHERE ChkDay.myTbSeq = 1
				) AS ChkTb_2
				ON ChkTb_1.ModelNo = ChkTb_2.ModelNo
			 ) AS TblCheckPrice ON TblCheckPrice.SupID = PURMA.MA001 and TblCheckPrice.ModelNo = INVMB.MB001
			 /* 採購單資料 E */

		) AS TblSection /* 算式欄位整理 */
	) AS TblPrint /* 條件式&列數 */
	WHERE (1=1)
";
                //置入條件SQL
                columnSql = columnSql.Replace("##tarStock##", stockTarget);
                columnSql = columnSql.Replace("##DBName##", dbName);
                columnSql = columnSql.Replace("##qtyDay##", qtyCount_Days);
                columnSql = columnSql.Replace("##qtyYear##", qtyCount_Year);
                columnSql = columnSql.Replace("##qtySeason##", qtyCount_Season);

                #endregion


                #region >> 主要資料SQL查詢 <<

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    //宣告語法
                    sql.Append(sqlDeclare);

                    //前置主體語法
                    sql.Append(mainSql);

                    sql.AppendLine("SELECT TblAll.* FROM (");

                    //欄位語法
                    sql.Append(columnSql);

                    //條件語法
                    sql.AppendLine(filterSQL);

                    sql.AppendLine(") AS TblAll");
                    sql.AppendLine(" WHERE (TblAll.RowIdx >= @startRow) AND (TblAll.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TblAll.RowIdx");


                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();
                    cmd.CommandTimeout = 120;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                    sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));


                    //加入參數陣列
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    myDT = dbConn.LookupDT(cmd, out ErrMsg);

                }

                #endregion


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    //宣告語法
                    sql.Append(sqlDeclare);

                    //前置主體語法
                    sql.Append(mainSql);

                    sql.AppendLine("SELECT COUNT(*) AS TotalCnt FROM (");

                    //欄位語法
                    sql.Append(columnSql);

                    //條件語法
                    sql.AppendLine(filterSQL);

                    sql.AppendLine(") AS TblAll");


                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 條件參數 -----
                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList_Cnt.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, out ErrMsg))
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


                /* 資料整理 */
                #region -- 資料整理 --

                //LinQ 查詢
                var query = myDT.AsEnumerable();

                //資料迴圈
                foreach (var item in query)
                {
                    int _UsefulQty_A01 = item.Field<int>("UsefulQty_A01");
                    int _UsefulQty_12 = item.Field<int>("UsefulQty_12");
                    int _QTM_A01 = item.Field<int>("QTM_A01");
                    int _QTM_12 = item.Field<int>("QTM_12");
                    double _Qty_Year = item.Field<double>("Qty_Year");
                    double _QTM_Month = 0;
                    int _RealPreSell = 0;
                    int _VirPreSell = item.Field<int>("VirPreSell");
                    int _PreSell_12 = item.Field<int>("PreSell_12");
                    int _PreSell_A01 = item.Field<int>("PreSell_A01");

                    /*
                        總擬定數可用周轉月<_QTM_Month> = ((可用量<UsefulQty_A01> + 擬定數量<QTM_A01>) / 年平均月用量<Qty_Year>)
                        實際預計銷 = 預計銷 - 虛擬預計銷
                    */
                    switch (stockType)
                    {
                        case "A":
                            //12倉
                            _QTM_Month = _Qty_Year > 0 ? (_UsefulQty_A01 + _QTM_A01) / _Qty_Year : 0;
                            _RealPreSell = _PreSell_12 - _VirPreSell;
                            break;

                        case "B":
                            //A01倉
                            _QTM_Month = _Qty_Year > 0 ? (_UsefulQty_12 + _QTM_12) / _Qty_Year : 0;
                            _RealPreSell = _PreSell_A01 - _VirPreSell;
                            break;

                        default:
                            _QTM_Month = _Qty_Year > 0 ? (_UsefulQty_A01 + _QTM_A01 + _UsefulQty_12 + _QTM_12) / _Qty_Year : 0;
                            _RealPreSell = (_PreSell_12 + _PreSell_A01) - _VirPreSell;
                            break;
                    }

                    //加入項目
                    var data = new PurPlanList
                    {
                        ModelNo = item.Field<string>("ModelNo"),
                        Item_Type = item.Field<string>("Item_Type"),
                        ModelName = item.Field<string>("ModelName"),
                        ProdVol = item.Field<string>("ProdVol"),
                        ProdPage = item.Field<string>("ProdPage"),
                        Currency = item.Field<string>("Currency"), /* 核價幣別<Currency> */
                        checkPrice = item.Field<double>("checkPrice"), /* 核價單價<checkPrice> */
                        WaitQty = item.Field<int>("WaitQty"), /* 待驗收<WaitQty> */
                        StockQty_A01 = item.Field<int>("StockQty_A01"), /* 庫存<StockQty_A01> */
                        SafeQty_A01 = item.Field<int>("SafeQty_A01"), /* 安全存量<SafeQty_A01> */
                        PreIN_A01 = item.Field<int>("PreIN_A01"), /* 預計進<PreIN_A01> */
                        VirIn_A01 = item.Field<int>("VirIn_A01"), /* 虛擬入<VirIn_A01> */
                        PlanIN_A01 = item.Field<int>("PlanIN_A01"), /* 計劃進<PlanIN_A01> */
                        PreSell_A01 = _PreSell_A01, /* 預計銷<PreSell_A01> */
                        PreGet_A01 = item.Field<int>("PreGet_A01"), /* 預計領<PreGet_A01> */
                        PlanOut_A01 = item.Field<int>("PlanOut_A01"), /* 計劃領<PlanOut_A01> */
                        StockQty_12 = item.Field<int>("StockQty_12"), /* 庫存<StockQty_12> */
                        SafeQty_12 = item.Field<int>("SafeQty_12"), /* 安全存量<SafeQty_12> */
                        PreIN_12 = item.Field<int>("PreIN_12"), /* 預計進<PreIN_12> */
                        VirIn_12 = item.Field<int>("VirIn_12"), /* 虛擬入<VirIn_12> */
                        PlanIN_12 = item.Field<int>("PlanIN_12"), /* 計劃進<PlanIN_12> */
                        PreSell_12 = _PreSell_12, /* 預計銷<PreSell_12> */
                        PreGet_12 = item.Field<int>("PreGet_12"), /* 預計領<PreGet_12> */
                        PlanOut_12 = item.Field<int>("PlanOut_12"), /* 計劃領<PlanOut_12> */
                        Qty_Days = item.Field<double>("Qty_Days"), /* 近N天用量<Qty_Days> */
                        Qty_Year = _Qty_Year, /* 全年平均月用量<Qty_Year> */
                        Qty_Season = item.Field<double>("Qty_Season"), /* 去年當季平均用量<Qty_Season> */
                        VirPreSell = item.Field<int>("VirPreSell"), /* 虛擬預計銷<VirPreSell> */
                        CNT = item.Field<int>("CNT"), /* 進貨筆數加總<CNT> */
                        SZ_QtyOfYear = item.Field<double>("SZ_QtyOfYear"), /* 深圳全年平均月用量<SZ_QtyOfYear> */
                        Min_Supply = item.Field<int>("Min_Supply"), /* 最低補量<Min_Supply> */
                        InBox_Qty = item.Field<int>("InBox_Qty"), /* 內盒數量<InBox_Qty> */
                        Qty_Packing = item.Field<int>("Qty_Packing"), /* 一箱數量<Qty_Packing> */
                        OutBox_Cuft = item.Field<double>("OutBox_Cuft"), /* 整箱材積<OutBox_Cuft> */
                        MOQ = item.Field<int>("MOQ"), /* 銷售MOQ<MOQ> */
                        ProdMsg = item.Field<string>("Sub_item"), /* 產銷訊息<ProdMsg> */
                        Supplier = item.Field<string>("Supplier"), /* 供應商Name<Supplier> */
                        Supply_ID = item.Field<string>("Supply_ID"), /* 供應商ID<Supply_ID> */
                        PushQty = item.Field<int>("PushQty"), /* 催貨量<PushQty> */
                        UsefulQty_A01 = _UsefulQty_A01, /* 可用量<UsefulQty_A01> */
                        UsefulQty_12 = _UsefulQty_12, /* 可用量<UsefulQty_12> */
                        QTM_A01 = _QTM_A01, /* 擬定數量<QTM_A01> */
                        QTM_12 = _QTM_12, /* 擬定數量<QTM_12> */
                        MonthTurn_A01 = Math.Round(item.Field<double>("MonthTurn_A01"), 2, MidpointRounding.AwayFromZero), /* 可用週轉月<MonthTurn_A01> */
                        MonthTurn_12 = Math.Round(item.Field<double>("MonthTurn_12"), 2, MidpointRounding.AwayFromZero), /* 可用週轉月<MonthTurn_12> */
                        NowMonthTurn_A01 = Math.Round(item.Field<double>("NowMonthTurn_A01"), 2, MidpointRounding.AwayFromZero), /* 現有周轉月<NowMonthTurn_A01> */
                        NowMonthTurn_12 = Math.Round(item.Field<double>("NowMonthTurn_12"), 2, MidpointRounding.AwayFromZero), /* 現有周轉月<NowMonthTurn_12> */
                        QTM_Month = Math.Round(_QTM_Month, 2, MidpointRounding.AwayFromZero), /* 總擬定數可用周轉月<QTM_Month> */
                        RealPreSell = _RealPreSell, /* 實際預計銷<RealPreSell> */
                        RowIdx = item.Field<Int64>("RowIdx")
                    };


                    //將項目加入至集合
                    dataList.Add(data);

                }

                #endregion

                //回傳集合
                return dataList.AsQueryable();


            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        /// <summary>
        /// 訂貨計劃 - 深圳 (dbName記得改)
        /// PS1:目前庫別只有A01, 故語法不作修正 (參考上海語法)
        /// PS2:未來可能會移除此段, 故不優化及整理
        /// </summary>
        /// <param name="stockType">指定庫別(A/B/C),A=12, B=A01, C=合併倉</param>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow(從0開始)</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns>DataTable</returns>
        public IQueryable<PurPlanList> GetPurPlan_SZ(string stockType,
            Dictionary<string, string> search
            , int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                StringBuilder sqlDeclare = new StringBuilder();
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                List<PurPlanList> dataList = new List<PurPlanList>(); //資料容器
                string mainSql = ""; //SQL主體
                string columnSql = ""; //SQL欄位
                string filterSQL = ""; //條件參數SQL
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> sqlParamList_Cnt = new List<SqlParameter>(); //SQL參數容器(cnt)
                DataTable myDT = new DataTable();
                DataCnt = 0;    //資料總數

                string dbName = "ProUnion"; //*** 資料庫 ***


                sqlDeclare.AppendLine("DECLARE @nDays AS INT");
                sqlDeclare.AppendLine("SET @nDays = @setDays");


                #region >> [前置作業] 條件組合 <<

                //庫別固定條件
                string stockTarget = "";
                string qtyCount_Days = "";
                string qtyCount_Year = "";
                string qtyCount_Season = "";


                switch (stockType)
                {
                    case "A":
                        //B01倉
                        stockTarget = "'B01'";
                        qtyCount_Days = "ISNULL(TblQty_Days.Qty_12, 0)";
                        qtyCount_Year = "(ISNULL(TblQty_Year.Qty_12, 0))";
                        qtyCount_Season = "(ISNULL(TblQty_Season.Qty_12, 0))";
                        break;

                    case "B":
                        //A01倉
                        stockTarget = "'A01'";
                        qtyCount_Days = "ISNULL(TblQty_Days.Qty_A01, 0)";
                        qtyCount_Year = "(ISNULL(TblQty_Year.Qty_A01, 0))";
                        qtyCount_Season = "(ISNULL(TblQty_Season.Qty_A01, 0))";
                        break;

                    default:
                        stockTarget = "'A01','B01'";
                        qtyCount_Days = "ISNULL(TblQty_Days.Qty_A01, 0) + ISNULL(TblQty_Days.Qty_12, 0)";
                        qtyCount_Year = "(ISNULL(TblQty_Year.Qty_A01, 0) + ISNULL(TblQty_Year.Qty_12, 0))";
                        qtyCount_Season = "(ISNULL(TblQty_Season.Qty_A01, 0) + ISNULL(TblQty_Season.Qty_12, 0))";
                        break;
                }


                #region search filter 

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "ModelNo":
                                //拆解輸入字串(1PK-036S#GE-123#MT-810)
                                string[] aryID = Regex.Split(item.Value, "#");
                                ArrayList aryLst = new ArrayList(aryID);

                                /*
                                 GetSQLParam:SQL WHERE IN的方法  ,ex:UPPER(ModelNo) IN ({0})
                                */
                                /* [篩選條件], 品號 */
                                filterSQL += " AND (ModelNo IN ({0}))".FormatThis(CustomExtension.GetSQLParam(aryLst, "pModel"));


                                //SQL參數組成
                                for (int row = 0; row < aryID.Count(); row++)
                                {
                                    sqlParamList.Add(new SqlParameter("@pModel" + row, aryID[row]));
                                    sqlParamList_Cnt.Add(new SqlParameter("@pModel" + row, aryID[row]));
                                }

                                break;


                            case "SupID":
                                /* [篩選條件], 供應商ID */
                                filterSQL += " AND (Supply_ID = @SupID)";

                                //SQL參數組成
                                sqlParamList.Add(new SqlParameter("@SupID", item.Value));
                                sqlParamList_Cnt.Add(new SqlParameter("@SupID", item.Value));

                                break;

                            case "nDays":
                                //SQL參數組成
                                sqlParamList.Add(new SqlParameter("@setDays", item.Value));
                                sqlParamList_Cnt.Add(new SqlParameter("@setDays", item.Value));

                                break;

                            case "CustomFilter":
                                /* 指定範圍條件 */
                                switch (stockType)
                                {
                                    case "A":
                                        //12倉
                                        #region --12範圍條件--
                                        switch (item.Value)
                                        {
                                            case "A":
                                                /* (A) 是否有擬定數量 = (庫存 + 預計進 + 計劃進 - 預計銷) < 安全存量 */
                                                filterSQL += " AND ((StockQty_12 + PreIN_12 + PlanIN_12 - PreSell_12) < SafeQty_12)";
                                                break;

                                            case "B":
                                                /* (B) 催貨量 < 0 */
                                                filterSQL += " AND ((StockQty_12 - PreSell_12) < 0)";
                                                break;

                                            case "C":
                                                /* (C) 安全存量不為0 */
                                                filterSQL += " AND (SafeQty_12 <> 0)";
                                                break;

                                            case "D":
                                                /* (D) 預計進 > 0 */
                                                filterSQL += " AND (PreIN_12 > 0)";
                                                break;

                                            case "E":
                                                /* (E) 計劃進 > 0 */
                                                filterSQL += " AND (PlanIN_12 > 0)";
                                                break;

                                            case "F":
                                                /* (F) 虛擬預計銷 > 0 */
                                                filterSQL += " AND (VirPreSell > 0)";
                                                break;

                                            case "G":
                                                /* (G) 近n天用量 > 0 */
                                                filterSQL += " AND (Qty_Days > 0)";
                                                break;

                                            case "H":
                                                /* (H) 近n天用量 = 0 */
                                                filterSQL += " AND (Qty_Days = 0)";
                                                break;

                                            case "I":
                                                /* (I) 近n天用量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (Qty_Days = 0) AND (UsefulQty > 0)";
                                                break;

                                            case "J":
                                                /* (J) 安全存量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (SafeQty_12 = 0) AND (UsefulQty > 0)";
                                                break;


                                            case "L":
                                                /* (L) 可用量 12 > 0 */
                                                filterSQL += " AND ((StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12) > 0)";
                                                break;

                                            case "M":
                                                /* 可用週轉月(M) ~ (S) */
                                                filterSQL += "AND (MonthTurn_12 < 1)";
                                                break;

                                            case "N":
                                                filterSQL += "AND (MonthTurn_12 < 2)";
                                                break;

                                            case "O":
                                                filterSQL += "AND (MonthTurn_12 < 2.5)";
                                                break;

                                            case "P":
                                                filterSQL += "AND (MonthTurn_12 < 3.5)";
                                                break;

                                            case "Q":
                                                filterSQL += "AND (MonthTurn_12 > 6)";
                                                break;

                                            case "R":
                                                filterSQL += "AND (MonthTurn_12 > 12)";
                                                break;

                                            case "S":
                                                filterSQL += "AND (MonthTurn_12 > 24)";
                                                break;

                                            default:
                                                break;
                                        }

                                        #endregion

                                        break;

                                    case "B":
                                        //A01倉
                                        #region --A01範圍條件--
                                        switch (item.Value)
                                        {
                                            case "A":
                                                /* (A) 是否有擬定數量 = (庫存 + 預計進 + 計劃進 - 預計銷) < 安全存量 */
                                                filterSQL += " AND ((StockQty_A01 + PreIN_A01 + PlanIN_A01 - PreSell_A01) < SafeQty_A01)";
                                                break;

                                            case "B":
                                                /* (B) 催貨量 < 0 */
                                                filterSQL += " AND ((StockQty_A01 - PreSell_A01) < 0)";
                                                break;

                                            case "C":
                                                /* (C) 安全存量不為0 */
                                                filterSQL += " AND (SafeQty_A01 <> 0)";
                                                break;

                                            case "D":
                                                /* (D) 預計進 > 0 */
                                                filterSQL += " AND (PreIN_A01 > 0)";
                                                break;

                                            case "E":
                                                /* (E) 計劃進 > 0 */
                                                filterSQL += " AND (PlanIN_A01 > 0)";
                                                break;

                                            case "F":
                                                /* (F) 虛擬預計銷 > 0 */
                                                filterSQL += " AND (VirPreSell > 0)";
                                                break;

                                            case "G":
                                                /* (G) 近n天用量 > 0 */
                                                filterSQL += " AND (Qty_Days > 0)";
                                                break;

                                            case "H":
                                                /* (H) 近n天用量 = 0 */
                                                filterSQL += " AND (Qty_Days = 0)";
                                                break;

                                            case "I":
                                                /* (I) 近n天用量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (Qty_Days = 0) AND (UsefulQty > 0)";
                                                break;

                                            case "J":
                                                /* (J) 安全存量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (SafeQty_A01 = 0) AND (UsefulQty > 0)";
                                                break;

                                            case "K":
                                                /* (K) 可用量 A01 > 0 */
                                                filterSQL += " AND ((StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) > 0)";
                                                break;

                                            //case "L":
                                            //    /* (L) 可用量 12 > 0 */
                                            //    filterSQL += " AND ((StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12) > 0)";
                                            //    break;

                                            case "M":
                                                /* 可用週轉月(M) ~ (S) */
                                                filterSQL += "AND (MonthTurn_A01 < 1)";
                                                break;

                                            case "N":
                                                filterSQL += "AND (MonthTurn_A01 < 2)";
                                                break;

                                            case "O":
                                                filterSQL += "AND (MonthTurn_A01 < 2.5)";
                                                break;

                                            case "P":
                                                filterSQL += "AND (MonthTurn_A01 < 3.5)";
                                                break;

                                            case "Q":
                                                filterSQL += "AND (MonthTurn_A01 > 6)";
                                                break;

                                            case "R":
                                                filterSQL += "AND (MonthTurn_A01 > 12)";
                                                break;

                                            case "S":
                                                filterSQL += "AND (MonthTurn_A01 > 24)";
                                                break;

                                            default:
                                                break;
                                        }

                                        #endregion
                                        break;

                                    default:
                                        //合併倉
                                        #region --合併倉範圍條件--
                                        switch (item.Value)
                                        {
                                            case "A":
                                                /* (A) 是否有擬定數量 = (庫存 + 預計進 + 計劃進 - 預計銷) < 安全存量 */
                                                filterSQL += " AND (((StockQty_A01 + PreIN_A01 + PlanIN_A01 - PreSell_A01) + (StockQty_12 + PreIN_12 + PlanIN_12 - PreSell_12)) < SafeQty_A01)";
                                                break;

                                            case "B":
                                                /* (B) 催貨量 < 0 */
                                                filterSQL += " AND (((StockQty_A01 - PreSell_A01) + (StockQty_12 - PreSell_12)) < 0)";
                                                break;

                                            case "C":
                                                /* (C) 安全存量不為0 */
                                                filterSQL += " AND ((SafeQty_A01 + SafeQty_12) <> 0)";
                                                break;

                                            case "D":
                                                /* (D) 預計進 > 0 */
                                                filterSQL += " AND ((PreIN_A01 + PreIN_12) > 0)";
                                                break;

                                            case "E":
                                                /* (E) 計劃進 > 0 */
                                                filterSQL += " AND ((PlanIN_A01 + PlanIN_12) > 0)";
                                                break;

                                            case "F":
                                                /* (F) 虛擬預計銷 > 0 */
                                                filterSQL += " AND (VirPreSell > 0)";
                                                break;

                                            case "G":
                                                /* (G) 近n天用量 > 0 */
                                                filterSQL += " AND (Qty_Days > 0)";
                                                break;

                                            case "H":
                                                /* (H) 近n天用量 = 0 */
                                                filterSQL += " AND (Qty_Days = 0)";
                                                break;

                                            case "I":
                                                /* (I) 近n天用量 = 0, 可用量 > 0 */
                                                filterSQL += " AND (Qty_Days = 0) AND (UsefulQty > 0)";
                                                break;

                                            case "J":
                                                /* (J) 安全存量 = 0, 可用量 > 0 */
                                                filterSQL += " AND ((SafeQty_A01 + SafeQty_12) = 0) AND (UsefulQty > 0)";
                                                break;

                                            //case "K":
                                            //    /* (K) 可用量 A01 > 0 */
                                            //    filterSQL += " AND ((StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) > 0)";
                                            //    break;

                                            case "L":
                                                /* (L) 可用量 > 0 */
                                                filterSQL += " AND (((StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) + (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12)) > 0)";
                                                break;

                                            case "M":
                                                /* 可用週轉月(M) ~ (S) */
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) < 1)";
                                                break;

                                            case "N":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) < 2)";
                                                break;

                                            case "O":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) < 2.5)";
                                                break;

                                            case "P":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) < 3.5)";
                                                break;

                                            case "Q":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) > 6)";
                                                break;

                                            case "R":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) > 12)";
                                                break;

                                            case "S":
                                                filterSQL += "AND ((MonthTurn_A01 + MonthTurn_12) > 24)";
                                                break;

                                            default:
                                                break;
                                        }

                                        #endregion
                                        break;
                                }


                                break;

                        }
                    }
                }
                #endregion

                #endregion


                #region >> [前置作業] SQL主體 <<

                mainSql = @"
/* 預計銷(訂單):TD016 = 結案碼, TD021 = 確認碼 */
;WITH Tbl_PreSell AS (
	SELECT p.ModelNo
	 , p.[12] AS PreSell_12, p.[A01] AS PreSell_A01
	FROM (
		SELECT ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0) AS PreSell
		 , RTRIM(TD004) AS ModelNo
		 , TD007 AS StockType
		FROM [##DBName##].dbo.COPTD WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TD016 = 'N') AND (TD021 = 'Y') AND (TD007 IN ('12', 'A01'))
		GROUP BY TD004, TD007
	) t 
	PIVOT (
		SUM(PreSell)
		FOR StockType IN ([12], [A01])
	) p
)

/* 虛擬預計銷(訂單):TD016 = 結案碼, TD021 = 確認碼 */
, Tbl_VirPreSell AS (
	SELECT ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0) AS VirPreSell
	 , RTRIM(TD004) AS ModelNo
	FROM [##DBName##].dbo.COPTD WITH (NOLOCK)
	WHERE (TD016 = 'N') AND (TD021 = 'Y') AND (TD001 = '22C1')
	GROUP BY TD004
)

/* 虛擬入(訂單):TD016 = 結案碼, TD021 = 確認碼 */
, Tbl_VirIn AS (
	SELECT p.ModelNo
	 , p.[12] AS VirIn_12, p.[A01] AS VirIn_A01
	FROM (
		SELECT ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0) AS VirIn
		 , RTRIM(TD004) AS ModelNo
		 , TD007 AS StockType
		FROM [##DBName##].dbo.COPTD WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TD016 = 'N') AND (TD021 = 'Y') AND (TD007 IN ('12', 'A01')) AND (TD001 = '2262')
		GROUP BY TD004, TD007
	) t 
	PIVOT (
		SUM(VirIn)
		FOR StockType IN ([12], [A01])
	) p
)

/* 預計進(採購單):TD016 = 結案碼, TD018 = 確認碼 */
, Tbl_PreIN AS (
	SELECT p.ModelNo
	 , p.[12] AS PreIN_12, p.[A01] AS PreIN_A01
	FROM (
		SELECT ISNULL(SUM(TD008 - TD015), 0) AS PreIN
		 , RTRIM(TD004) AS ModelNo
		 , TD007 AS StockType
		FROM [##DBName##].dbo.PURTD WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TD016 = 'N') AND (TD018 = 'Y') AND (TD007 IN ('12', 'A01'))
		GROUP BY TD004, TD007
	) t 
	PIVOT (
		SUM(PreIN)
		FOR StockType IN ([12], [A01])
	) p
)

/* 計劃進(採購單):TD016 = 結案碼, TD018 = 確認碼 */
, Tbl_PlanIN AS (
	SELECT p.ModelNo
	 , p.[12] AS PlanIN_12, p.[A01] AS PlanIN_A01
	FROM (
		SELECT ISNULL(SUM(TD008 - TD015), 0) AS PlanIN
		 , RTRIM(TD004) AS ModelNo
		 , TD007 AS StockType
		FROM [##DBName##].dbo.PURTD WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TD016 = 'N') AND (TD018 = 'N') AND (TD007 IN ('12', 'A01'))
		GROUP BY TD004, TD007
	) t 
	PIVOT (
		SUM(PlanIN)
		FOR StockType IN ([12], [A01])
	) p
)

/* 預計領:TA013,確認碼 <> 'V', TA011,狀態碼 = 1,2,3 */
, Tbl_PreGet AS (
	SELECT p.ModelNo
	 , p.[12] AS PreGet_12, p.[A01] AS PreGet_A01
	FROM (
		SELECT SUM(TB004 - TB005) AS PreGet
		 , RTRIM(TA006) AS ModelNo
		 , TA020 AS StockType
		FROM [##DBName##].dbo.MOCTA AS A WITH(NOLOCK)
		 INNER JOIN [##DBName##].dbo.MOCTB AS B WITH(NOLOCK) ON A.TA001 = B.TB001 AND A.TA002 = B.TB002 AND A.TA006 = B.TB003
		WHERE (A.TA013 <> 'V') AND (A.TA011 IN ('1', '2', '3'))
		 /* [指定條件] 所有庫別 */
		 AND (A.TA020 IN ('12', 'A01'))
		GROUP BY TA006, TA020
	) t 
	PIVOT (
		SUM(PreGet)
		FOR StockType IN ([12], [A01])
	) p
)

/* 計劃領 */
, Tbl_PlanOut AS (
	SELECT p.ModelNo
	 , p.[12] AS OutQty_12, p.[A01] AS OutQty_A01
	FROM (
		SELECT TB007 AS OutQty, RTRIM(TB005) AS ModelNo, TB008 AS StockType
		FROM [##DBName##].dbo.LRPTB WITH(NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (TB008 IN ('12', 'A01'))
	) t 
	PIVOT (
		SUM(OutQty)
		FOR StockType IN ([12], [A01])
	) p
)

/* 庫存 */
, Tbl_Stock AS (
	SELECT p.ModelNo
	 , p.[12] AS StockQty_12, p.[A01] AS StockQty_A01
	FROM (
		SELECT MC007 AS StockQty, RTRIM(MC001) AS ModelNo, MC002 AS StockType
		FROM [##DBName##].dbo.INVMC WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (MC002 IN ('12', 'A01')) AND (MC001 <> '')
	) t 
	PIVOT (
		SUM(StockQty)
		FOR StockType IN ([12], [A01])
	) p
)
/* 安全庫存 */
, Tbl_SafeStock AS (
	SELECT p.ModelNo
	 , p.[12] AS SafeQty_12, p.[A01] AS SafeQty_A01
	FROM (
		SELECT MC004 AS StockQty, RTRIM(MC001) AS ModelNo, MC002 AS StockType
		FROM [##DBName##].dbo.INVMC WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (MC002 IN ('12', 'A01')) AND (MC001 <> '')
	) t 
	PIVOT (
		SUM(StockQty)
		FOR StockType IN ([12], [A01])
	) p
)

/* [欄位] - 近n天用量 (參數:nDays)(A01) */
, TblQty_Days AS (
	SELECT p.ModelNo
	 , p.[12] AS Qty_12, p.[A01] AS Qty_A01
	FROM (
		SELECT LA011 AS sumQty, RTRIM(LA001) AS ModelNo, LA009 AS StockType
		FROM [##DBName##].dbo.INVLA WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (LA005 = '-1') AND (LA009 IN ('12', 'A01'))
		 AND (LA006 IN ('2341','2342','2343','2345','23B1','23B2','23B3','23B4','23B6'))
		 AND (
			LA004 BETWEEN REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - @nDays, GETDATE()), 111),'/', '')
				AND REPLACE(CONVERT(VARCHAR(10), DATEADD(day, 0, GETDATE()), 111), '/', '')
			)
	) t 
	PIVOT (
		SUM(sumQty)
		FOR StockType IN ([12], [A01])
	) p
)

/* [欄位] - 全年平均月用量 */
, TblQty_Year AS (
	SELECT p.ModelNo
	 , p.[12] AS Qty_12, p.[A01] AS Qty_A01
	FROM (
		SELECT LA011 AS sumQty, RTRIM(LA001) AS ModelNo, LA009 AS StockType
		FROM [##DBName##].dbo.INVLA WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (LA005 = '-1') AND (LA009 IN ('12', 'A01'))
		 AND (LA006 IN ('2341','2342','2343','2345','23B1','23B2','23B3','23B4','23B6'))
		 AND (
			LA004 BETWEEN REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - 365, GETDATE()), 111), '/', '')
				AND REPLACE(CONVERT(VARCHAR(10), DATEADD(day, 0, GETDATE()), 111), '/', '')
			)
	) t 
	PIVOT (
		SUM(sumQty)
		FOR StockType IN ([12], [A01])
	) p
)

/* [欄位] - 去年當季平均用量 */
, TblQty_Season AS (
	SELECT p.ModelNo
	 , p.[12] AS Qty_12, p.[A01] AS Qty_A01
	FROM (
		SELECT LA011 AS sumQty, RTRIM(LA001) AS ModelNo, LA009 AS StockType
		FROM [##DBName##].dbo.INVLA WITH (NOLOCK)
		/* [指定條件] 所有庫別 */
		WHERE (LA005 = '-1') AND (LA009 IN ('12', 'A01'))
		 AND (LA006 IN ('2341','2342','2343','2345','23B1','23B2','23B3','23B4','23B6'))
		 AND (
			LA004 BETWEEN REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - 365, GETDATE()), 111), '/', '')
				AND REPLACE(CONVERT(VARCHAR(10), GETDATE() - 275, 111), '/', '')
			)
	) t 
	PIVOT (
		SUM(sumQty)
		FOR StockType IN ([12], [A01])
	) p
)

, Tbl_WaitQty AS (
/*
	[欄位] - 待驗收(TH007)(A01)
	QMSTA 進貨檢驗單單頭檔
	PURTH 進貨單單身檔
	TA001, TH001 = 單別
	TA002, TH002 = 單號
	TA003, TH003 = 序號
	TA014, TH030 = 確認碼
	TH004 = 品號
*/
	SELECT RTRIM(PURTH.TH004) AS ModelNo, PURTH.TH007 AS WaitQty
	FROM [##DBName##].dbo.QMSTA WITH (NOLOCK)
		INNER JOIN [##DBName##].dbo.PURTH WITH (NOLOCK) ON QMSTA.TA001 = PURTH.TH001 AND QMSTA.TA002 = PURTH.TH002 AND QMSTA.TA003 = PURTH.TH003
	/* [指定條件] 單一庫別 */
	WHERE (QMSTA.TA014 = 'N') AND (PURTH.TH030 = 'N') AND (PURTH.TH009 IN (##tarStock##))	 
)

, Tbl_PurCnt AS (
	/* 進貨筆數加總(依指定天數)(參數:nDays)(A01) */
	SELECT RTRIM(PURTH.TH004) AS ModelNo, COUNT(*) AS CNT
	FROM [##DBName##].dbo.PURTG WITH (NOLOCK)
		INNER JOIN [##DBName##].dbo.PURTH WITH (NOLOCK) ON PURTG.TG001 = PURTH.TH001 AND PURTG.TG002 = PURTH.TH002
	/* [指定條件] 單一庫別 */
	WHERE (PURTH.TH009 IN (##tarStock##))
	 AND (PURTG.TG003 BETWEEN 
		REPLACE(CONVERT(VARCHAR(10), DATEADD(day, - @nDays, GETDATE()), 111), '/', '') 
		AND REPLACE(CONVERT(VARCHAR(10), DATEADD(day, 0, GETDATE()), 111), '/', ''))
	GROUP BY PURTH.TH004
)
                    ";
                //置入條件SQL
                mainSql = mainSql.Replace("##tarStock##", stockTarget);
                mainSql = mainSql.Replace("##DBName##", dbName);

                #endregion


                #region >> SQL欄位 <<
                columnSql = @"
	SELECT TblPrint.*
		, RANK() OVER(ORDER BY TblPrint.ModelNo) AS RowIdx
	FROM (
		SELECT TblSection.* 
			/* 催貨量 = 庫存 - 預計銷 */
			, (StockQty_A01 - PreSell_A01) AS PushQty

			/* 可用量 A01 = 庫存 + 預計進 - 預計銷 + 計劃進 */
			, (StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) AS UsefulQty_A01
			/* 可用量 12 = 庫存 + 預計進 - 預計銷 + 計劃進 -預計領 - 計劃領 */
			, (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12) AS UsefulQty_12

			/*
			   擬定數量 = {
				 SET X = (庫存 + 預計進 + 計劃進 - 預計銷)
				 SET Y = ABS(安全存量 - X);
				 IF X >= 安全存量 THEN 0
				 ELSEIF Y > 最低補量 THEN ABS(Y)
				 ELSE 最低補量
				}
			*/
			, (CASE WHEN (StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) >= SafeQty_A01 THEN 0
				WHEN ABS(SafeQty_A01 - (StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01)) <= Min_Supply THEN Min_Supply
				ELSE ABS(SafeQty_A01 - (StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01))
				END) AS QTM_A01
			, (CASE WHEN (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12) >= SafeQty_12 THEN 0
				WHEN ABS(SafeQty_12 - (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12)) <= Min_Supply THEN Min_Supply
				ELSE ABS(SafeQty_12 - (StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12))
				END) AS QTM_12

			/* 可用週轉月<MonthTurn_A01> A01 = (可用量 A01 / 年平均月用量) */
			, (CASE WHEN Qty_Year = 0 THEN 0
				ELSE
					CONVERT(FLOAT,ROUND(
						(StockQty_A01 + PreIN_A01 - PreSell_A01 + PlanIN_A01) / Qty_Year
					, 2))
				END) AS MonthTurn_A01

			/* 可用週轉月<MonthTurn_12> 12 = (可用量 12 / 年平均月用量) */
			, (CASE WHEN Qty_Year = 0 THEN 0
				ELSE
					CONVERT(FLOAT,ROUND(
						(StockQty_12 + PreIN_12 - PreSell_12 + PlanIN_12 - PreGet_12 - PlanOut_12) / Qty_Year
					, 2))
				END) AS MonthTurn_12

			/* 現有周轉月<NowMonthTurn_A01> ,依表指定A01 = ((庫存 - 預計銷) / 年平均月用量) */
			, (CASE WHEN Qty_Year = 0 THEN 0
				ELSE
					CONVERT(FLOAT,ROUND(
						(StockQty_A01 - PreSell_A01) / Qty_Year
					, 2))
				END) AS NowMonthTurn_A01

			/* 現有周轉月<NowMonthTurn_12> ,依表指定合併 = (總庫存 - 總預計銷 - 預計領12) / 年平均月用量 */
			, (CASE WHEN Qty_Year = 0 THEN 0
				ELSE
					CONVERT(FLOAT,ROUND(
						(StockQty_12 - (PreSell_A01 + PreSell_12) - PreGet_12) / Qty_Year
					, 2))
				END) AS NowMonthTurn_12
		FROM (
			SELECT RTRIM(INVMB.MB001) AS ModelNo
			 , INVMA.MA003 AS Item_Type /* 屬性 */
			 , RTRIM(INVMB.MB002) AS ModelName /* 品名 */
			 , ISNULL(REPLACE(INVMB.MB207, 'NULL', ''), '') AS ProdVol /* VOL */
			 , ISNULL(REPLACE(INVMB.MB208, 'NULL', ''), '') AS ProdPage /* PAGE */
			 , ISNULL(TblCheckPrice.Currency, '') AS Currency
 			 , CONVERT(FLOAT, ISNULL(TblCheckPrice.checkPrice, 0)) AS checkPrice /* 核價單價 */
			 , CONVERT(INT, ISNULL(Tbl_WaitQty.WaitQty, 0)) AS WaitQty /* 待驗收 */
			 , CONVERT(INT, ISNULL(Tbl_Stock.StockQty_A01, 0)) AS StockQty_A01 /* 庫存 A01 */
			 , CONVERT(INT, ISNULL(Tbl_SafeStock.SafeQty_A01, 0)) AS SafeQty_A01 /* 安全存量 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PreIN.PreIN_A01, 0)) AS PreIN_A01 /* 預計進 A01 */
			 , CONVERT(INT, ISNULL(Tbl_VirIn.VirIn_A01, 0)) AS VirIn_A01 /* 虛擬入 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PlanIN.PlanIN_A01, 0)) AS PlanIN_A01 /* 計劃進 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PreSell.PreSell_A01, 0)) AS PreSell_A01 /* 預計銷 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PreGet.PreGet_A01, 0)) AS PreGet_A01 /* 預計領 A01 */
			 , CONVERT(INT, ISNULL(Tbl_PlanOut.OutQty_A01, 0)) AS PlanOut_A01 /* 計劃領 A01 */
			 , CONVERT(INT, ISNULL(Tbl_Stock.StockQty_12, 0)) AS StockQty_12 /* 庫存 12 */
			 , CONVERT(INT, ISNULL(Tbl_SafeStock.SafeQty_12, 0)) AS SafeQty_12 /* 安全存量 12 */
			 , CONVERT(INT, ISNULL(Tbl_PreIN.PreIN_12, 0)) AS PreIN_12 /* 預計進 12 */
			 , CONVERT(INT, ISNULL(Tbl_VirIn.VirIn_12, 0)) AS VirIn_12 /* 虛擬入 12 */
			 , CONVERT(INT, ISNULL(Tbl_PlanIN.PlanIN_12, 0)) AS PlanIN_12 /* 計劃進 12 */
			 , CONVERT(INT, ISNULL(Tbl_PreSell.PreSell_12, 0)) AS PreSell_12 /* 預計銷 12 */
			 , CONVERT(INT, ISNULL(Tbl_PreGet.PreGet_12, 0)) AS PreGet_12 /* 預計領 12 */
			 , CONVERT(INT, ISNULL(Tbl_PlanOut.OutQty_12, 0)) AS PlanOut_12 /* 計劃領 12 */
             , CONVERT(FLOAT, ##qtyDay##) AS Qty_Days /* 近N天用量 (依表指定A01/12) */
			 , CONVERT(FLOAT, ROUND(##qtyYear## / 12, 0)) AS Qty_Year /* 全年平均月用量(除12) (依表指定A01/12) */
             , CONVERT(FLOAT, ROUND(##qtySeason## / 3, 0)) AS Qty_Season /* 去年當季平均用量(除3) (依表指定A01/12) */
			 , CONVERT(INT, ISNULL(Tbl_VirPreSell.VirPreSell, 0)) AS VirPreSell /* 虛擬預計銷(ALL) */
			 , CONVERT(INT, ISNULL(Tbl_PurCnt.CNT, 0)) AS CNT /* 進貨筆數加總(依指定天數)(依表指定A01/12) */
			 , CONVERT(INT, INVMB.MB039) AS Min_Supply /* 最低補量 */
			 , CONVERT(INT, INVMB.MB201) AS InBox_Qty /* 內盒數量 */
			 , CONVERT(INT, INVMB.MB073) AS Qty_Packing /* 一箱數量 */
			 , CONVERT(FLOAT, INVMB.MB071) AS OutBox_Cuft /* 整箱材積 */
             , CONVERT(INT, ISNULL(INVMB.MB220, 0)) AS MOQ /* 銷售MOQ */
			 , RTRIM(INVMB.MB202) AS Sub_item /* 產銷訊息 */
			 , PURMA.MA002 AS Supplier /* 供應商 */
			 , RTRIM(PURMA.MA001) AS Supply_ID
			FROM [##DBName##].dbo.INVMB WITH (NOLOCK) 
			 INNER JOIN [##DBName##].dbo.PURMA ON INVMB.MB032 = PURMA.MA001
			 INNER JOIN [##DBName##].dbo.INVMA ON INVMA.MA002 = INVMB.MB008
			 INNER JOIN [##DBName##].dbo.INVMC ON INVMB.MB001 = INVMC.MC001 AND INVMC.MC002 IN (##tarStock##) /* [指定條件] 單一庫別 */
			 LEFT JOIN Tbl_Stock ON INVMB.MB001 = Tbl_Stock.ModelNo
			 LEFT JOIN Tbl_SafeStock ON INVMB.MB001 = Tbl_SafeStock.ModelNo
			 LEFT JOIN Tbl_PreIN ON INVMB.MB001 = Tbl_PreIN.ModelNo
			 LEFT JOIN Tbl_PlanIN ON INVMB.MB001 = Tbl_PlanIN.ModelNo
			 LEFT JOIN Tbl_PreSell ON INVMB.MB001 = Tbl_PreSell.ModelNo
			 LEFT JOIN Tbl_VirIn ON INVMB.MB001 = Tbl_VirIn.ModelNo
			 LEFT JOIN Tbl_VirPreSell ON INVMB.MB001 = Tbl_VirPreSell.ModelNo
			 LEFT JOIN Tbl_PreGet ON INVMB.MB001 = Tbl_PreGet.ModelNo
			 LEFT JOIN TblQty_Days ON INVMB.MB001 = TblQty_Days.ModelNo
			 LEFT JOIN TblQty_Year ON INVMB.MB001 = TblQty_Year.ModelNo
			 LEFT JOIN TblQty_Season ON INVMB.MB001 = TblQty_Season.ModelNo
			 LEFT JOIN Tbl_WaitQty ON INVMB.MB001 = Tbl_WaitQty.ModelNo
			 LEFT JOIN Tbl_PurCnt ON INVMB.MB001 = Tbl_PurCnt.ModelNo
			 LEFT JOIN Tbl_PlanOut ON INVMB.MB001 = Tbl_PlanOut.ModelNo

			 /* 採購單資料 S (資料量大,不放在CTE) */
			 LEFT JOIN (
				SELECT
				(CASE WHEN ChkTb_1.ModelNo IS NULL THEN ChkTb_2.ModelNo ELSE ChkTb_1.ModelNo END) AS ModelNo 
				, (CASE WHEN ChkTb_1.checkPrice IS NULL THEN ChkTb_2.Price ELSE ChkTb_1.checkPrice END) AS checkPrice
				, (CASE WHEN ChkTb_1.SupID IS NULL THEN ChkTb_2.SupID ELSE ChkTb_1.SupID END) AS SupID
				, (CASE WHEN ChkTb_1.Currency IS NULL THEN ChkTb_2.Currency ELSE ChkTb_1.Currency END) AS Currency
				FROM (
					/*
					採購核價單
					[單頭]
					TL004 廠商代號 (Group Key)
					TL005 幣別
					TL003 核價日期 (ORDER BY)

					[單身]
					TM004 品號 (Group Key)
					TM010 單價
					TM015 失效日 = '' (條件)
					*/
					SELECT ChkPrice.ModelNo, ChkPrice.checkPrice, ChkPrice.SupID, ChkPrice.Currency
					FROM (
						SELECT Base.TL004 AS SupID, Base.TL005 AS Currency, DT.TM004 AS ModelNo, ISNULL(DT.TM010, 0) AS checkPrice
						, RANK() OVER (
							PARTITION BY Base.TL004, DT.TM004
							ORDER BY Base.TL003 DESC
						) AS myTbSeq	
						FROM [##DBName##].dbo.PURTL Base WITH(NOLOCK)
						 INNER JOIN [##DBName##].dbo.PURTM DT WITH(NOLOCK) ON Base.TL001 = DT.TM001 AND Base.TL002 = DT.TM002
						WHERE (Base.TL006 = 'Y') AND (DT.TM015 = '')
					) AS ChkPrice
					WHERE ChkPrice.myTbSeq = 1
				) AS ChkTb_1
				FULL OUTER JOIN
				(
					/*
					最近採購單價格
					[單頭]
					TG005 供應商代號
					TG007 幣別

					[單身]
					TH018 原幣單位進價
					TH004 品號 (Group Key)
					TH009 庫別 (條件)
					TH014 驗收日期 (ORDER BY)
					*/
					SELECT ChkDay.ModelNo, ChkDay.Price, ChkDay.SupID, ChkDay.Currency
					FROM (
						SELECT Base.TG005 AS SupID, Base.TG007 AS Currency, DT.TH004 AS ModelNo, ISNULL(DT.TH018, 0) AS Price
						, RANK() OVER (
							PARTITION BY DT.TH004
							ORDER BY DT.TH014 DESC, DT.TH001, DT.TH002 DESC
						) AS myTbSeq	
						FROM [##DBName##].dbo.PURTG Base WITH(NOLOCK)
							INNER JOIN [##DBName##].dbo.PURTH DT WITH(NOLOCK) ON Base.TG001 = DT.TH001 AND Base.TG002 = DT.TH002
						/* [指定條件] 單一庫別 */
						WHERE (DT.TH009 IN (##tarStock##))
					) AS ChkDay
					WHERE ChkDay.myTbSeq = 1
				) AS ChkTb_2
				ON ChkTb_1.ModelNo = ChkTb_2.ModelNo
			 ) AS TblCheckPrice ON TblCheckPrice.SupID = PURMA.MA001 and TblCheckPrice.ModelNo = INVMB.MB001
			 /* 採購單資料 E */

		) AS TblSection /* 算式欄位整理 */
	) AS TblPrint /* 條件式&列數 */
	WHERE (1=1)
";
                //置入條件SQL
                columnSql = columnSql.Replace("##tarStock##", stockTarget);
                columnSql = columnSql.Replace("##DBName##", dbName);
                columnSql = columnSql.Replace("##qtyDay##", qtyCount_Days);
                columnSql = columnSql.Replace("##qtyYear##", qtyCount_Year);
                columnSql = columnSql.Replace("##qtySeason##", qtyCount_Season);

                #endregion


                #region >> 主要資料SQL查詢 <<

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    //宣告語法
                    sql.Append(sqlDeclare);

                    //前置主體語法
                    sql.Append(mainSql);

                    sql.AppendLine("SELECT TblAll.* FROM (");

                    //欄位語法
                    sql.Append(columnSql);

                    //條件語法
                    sql.AppendLine(filterSQL);

                    sql.AppendLine(") AS TblAll");
                    sql.AppendLine(" WHERE (TblAll.RowIdx >= @startRow) AND (TblAll.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TblAll.RowIdx");


                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();
                    cmd.CommandTimeout = 360;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                    sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));


                    //加入參數陣列
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    myDT = dbConn.LookupDT(cmd, out ErrMsg);

                }

                #endregion


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    //宣告語法
                    sql.Append(sqlDeclare);

                    //前置主體語法
                    sql.Append(mainSql);

                    sql.AppendLine("SELECT COUNT(*) AS TotalCnt FROM (");

                    //欄位語法
                    sql.Append(columnSql);

                    //條件語法
                    sql.AppendLine(filterSQL);

                    sql.AppendLine(") AS TblAll");


                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    cmdCnt.CommandTimeout = 360;   //單位:秒

                    //----- SQL 條件參數 -----
                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList_Cnt.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, out ErrMsg))
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


                /* 資料整理 */
                //LinQ 查詢
                var query = myDT.AsEnumerable();

                //資料迴圈
                foreach (var item in query)
                {
                    int _UsefulQty_A01 = item.Field<int>("UsefulQty_A01");
                    int _UsefulQty_12 = item.Field<int>("UsefulQty_12");
                    int _QTM_A01 = item.Field<int>("QTM_A01");
                    int _QTM_12 = item.Field<int>("QTM_12");
                    double _Qty_Year = item.Field<double>("Qty_Year");
                    double _QTM_Month = 0;
                    int _RealPreSell = 0;
                    int _VirPreSell = item.Field<int>("VirPreSell");
                    int _PreSell_12 = item.Field<int>("PreSell_12");
                    int _PreSell_A01 = item.Field<int>("PreSell_A01");

                    /*
                        總擬定數可用周轉月<_QTM_Month> = ((可用量<UsefulQty_A01> + 擬定數量<QTM_A01>) / 年平均月用量<Qty_Year>)
                        實際預計銷 = 預計銷 - 虛擬預計銷
                    */
                    switch (stockType)
                    {
                        case "A":
                            //12倉
                            _QTM_Month = _Qty_Year > 0 ? (_UsefulQty_A01 + _QTM_A01) / _Qty_Year : 0;
                            _RealPreSell = _PreSell_12 - _VirPreSell;
                            break;

                        case "B":
                            //A01倉
                            _QTM_Month = _Qty_Year > 0 ? (_UsefulQty_12 + _QTM_12) / _Qty_Year : 0;
                            _RealPreSell = _PreSell_A01 - _VirPreSell;
                            break;

                        default:
                            _QTM_Month = _Qty_Year > 0 ? (_UsefulQty_A01 + _QTM_A01 + _UsefulQty_12 + _QTM_12) / _Qty_Year : 0;
                            _RealPreSell = (_PreSell_12 + _PreSell_A01) - _VirPreSell;
                            break;
                    }

                    //加入項目
                    var data = new PurPlanList
                    {
                        ModelNo = item.Field<string>("ModelNo"),
                        Item_Type = item.Field<string>("Item_Type"),
                        ModelName = item.Field<string>("ModelName"),
                        ProdVol = item.Field<string>("ProdVol"),
                        ProdPage = item.Field<string>("ProdPage"),
                        Currency = item.Field<string>("Currency"), /* 核價幣別<Currency> */
                        checkPrice = item.Field<double>("checkPrice"), /* 核價單價<checkPrice> */
                        WaitQty = item.Field<int>("WaitQty"), /* 待驗收<WaitQty> */
                        StockQty_A01 = item.Field<int>("StockQty_A01"), /* 庫存<StockQty_A01> */
                        SafeQty_A01 = item.Field<int>("SafeQty_A01"), /* 安全存量<SafeQty_A01> */
                        PreIN_A01 = item.Field<int>("PreIN_A01"), /* 預計進<PreIN_A01> */
                        VirIn_A01 = item.Field<int>("VirIn_A01"), /* 虛擬入<VirIn_A01> */
                        PlanIN_A01 = item.Field<int>("PlanIN_A01"), /* 計劃進<PlanIN_A01> */
                        PreSell_A01 = _PreSell_A01, /* 預計銷<PreSell_A01> */
                        PreGet_A01 = item.Field<int>("PreGet_A01"), /* 預計領<PreGet_A01> */
                        PlanOut_A01 = item.Field<int>("PlanOut_A01"), /* 計劃領<PlanOut_A01> */
                        StockQty_12 = item.Field<int>("StockQty_12"), /* 庫存<StockQty_12> */
                        SafeQty_12 = item.Field<int>("SafeQty_12"), /* 安全存量<SafeQty_12> */
                        PreIN_12 = item.Field<int>("PreIN_12"), /* 預計進<PreIN_12> */
                        VirIn_12 = item.Field<int>("VirIn_12"), /* 虛擬入<VirIn_12> */
                        PlanIN_12 = item.Field<int>("PlanIN_12"), /* 計劃進<PlanIN_12> */
                        PreSell_12 = _PreSell_12, /* 預計銷<PreSell_12> */
                        PreGet_12 = item.Field<int>("PreGet_12"), /* 預計領<PreGet_12> */
                        PlanOut_12 = item.Field<int>("PlanOut_12"), /* 計劃領<PlanOut_12> */
                        Qty_Days = item.Field<double>("Qty_Days"), /* 近N天用量<Qty_Days> */
                        Qty_Year = _Qty_Year, /* 全年平均月用量<Qty_Year> */
                        Qty_Season = item.Field<double>("Qty_Season"), /* 去年當季平均用量<Qty_Season> */
                        VirPreSell = item.Field<int>("VirPreSell"), /* 虛擬預計銷<VirPreSell> */
                        CNT = item.Field<int>("CNT"), /* 進貨筆數加總<CNT> */
                        Min_Supply = item.Field<int>("Min_Supply"), /* 最低補量<Min_Supply> */
                        InBox_Qty = item.Field<int>("InBox_Qty"), /* 內盒數量<InBox_Qty> */
                        Qty_Packing = item.Field<int>("Qty_Packing"), /* 一箱數量<Qty_Packing> */
                        OutBox_Cuft = item.Field<double>("OutBox_Cuft"), /* 整箱材積<OutBox_Cuft> */
                        MOQ = item.Field<int>("MOQ"), /* 銷售MOQ<MOQ> */
                        ProdMsg = item.Field<string>("Sub_item"), /* 產銷訊息<ProdMsg> */
                        Supplier = item.Field<string>("Supplier"), /* 供應商Name<Supplier> */
                        Supply_ID = item.Field<string>("Supply_ID"), /* 供應商ID<Supply_ID> */
                        PushQty = item.Field<int>("PushQty"), /* 催貨量<PushQty> */
                        UsefulQty_A01 = _UsefulQty_A01, /* 可用量<UsefulQty_A01> */
                        UsefulQty_12 = _UsefulQty_12, /* 可用量<UsefulQty_12> */
                        QTM_A01 = _QTM_A01, /* 擬定數量<QTM_A01> */
                        QTM_12 = _QTM_12, /* 擬定數量<QTM_12> */
                        MonthTurn_A01 = Math.Round(item.Field<double>("MonthTurn_A01"), 2, MidpointRounding.AwayFromZero), /* 可用週轉月<MonthTurn_A01> */
                        MonthTurn_12 = Math.Round(item.Field<double>("MonthTurn_12"), 2, MidpointRounding.AwayFromZero), /* 可用週轉月<MonthTurn_12> */
                        NowMonthTurn_A01 = Math.Round(item.Field<double>("NowMonthTurn_A01"), 2, MidpointRounding.AwayFromZero), /* 現有周轉月<NowMonthTurn_A01> */
                        NowMonthTurn_12 = Math.Round(item.Field<double>("NowMonthTurn_12"), 2, MidpointRounding.AwayFromZero), /* 現有周轉月<NowMonthTurn_12> */
                        QTM_Month = Math.Round(_QTM_Month, 2, MidpointRounding.AwayFromZero), /* 總擬定數可用周轉月<QTM_Month> */
                        RealPreSell = _RealPreSell, /* 實際預計銷<RealPreSell> */
                        RowIdx = item.Field<Int64>("RowIdx")
                    };


                    //將項目加入至集合
                    dataList.Add(data);

                }

                //回傳集合
                return dataList.AsQueryable();


            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }

        #endregion *** 訂貨計劃 E ***


        #region *** 標準成本 S ***
        /// <summary>
        /// [標準成本] 成本List
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">DBS</param>
        /// <param name="startRow">StartRow(從0開始)</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="doPaging">是否分頁</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns>DataTable</returns>
        public IQueryable<PurProdCostList> GetCost_PurProd(Dictionary<string, string> search, string dbs
            , int startRow, int endRow, bool doPaging
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";
            string AllErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                List<PurProdCostList> dataList = new List<PurProdCostList>(); //資料容器
                string mainSql = ""; //SQL主體
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> sqlParamList_Cnt = new List<SqlParameter>(); //SQL參數容器
                DataTable myDT = new DataTable();
                DataCnt = 0;    //資料總數

                #region >> [前置作業] SQL主體 <<

                mainSql = @"
                /*
                 採購核價
                 DB:prokit2
                  - 幣別:NTD
                */
                WITH TblChkPrice_TW AS (
                SELECT ChkPrice_TW.ModelNo, ChkPrice_TW.Price, ChkPrice_TW.SupID
                FROM (
	                SELECT
	                RTRIM([MB001]) AS ModelNo --AS [品號]
	                , [MB011] AS Price --AS [採購單價]
	                , MB002 AS SupID
	                , RANK() OVER (
		                PARTITION BY MB001 ORDER BY MB008 DESC
	                ) AS RankSeq  --依核價日排序
	                FROM [prokit2].dbo.PURMB
	                WHERE (MB003 = 'NTD')
	                 AND (MB014 <= CONVERT(CHAR(8),GETDATE(),112))
                ) AS ChkPrice_TW
                WHERE (ChkPrice_TW.RankSeq = 1)
                )

                /*
                 採購核價
                 DB:SHPK2
                  - 幣別:RMB
                */
                , TblChkPrice_SH AS (
                SELECT ChkPrice_SH.ModelNo, ChkPrice_SH.Price, ChkPrice_SH.SupID
                FROM (
	                SELECT
	                RTRIM([MB001]) AS ModelNo --AS [品號]
	                , [MB011] AS Price --AS [採購單價]
	                , MB002 AS SupID
	                , RANK() OVER (
		                PARTITION BY MB001 ORDER BY MB008 DESC
	                ) AS RankSeq  --依核價日排序
	                FROM [SHPK2].dbo.PURMB
	                WHERE (MB003 = 'RMB')
	                 AND (MB014 <= CONVERT(CHAR(8),GETDATE(),112))
                ) AS ChkPrice_SH
                WHERE (ChkPrice_SH.RankSeq = 1)
                )";

                #endregion


                #region >> 主要資料SQL查詢 <<

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    //append 主體語法
                    sql.Append(mainSql);

                    //欄位select
                    string subSql = @"
                    SELECT TbAll.*
                    FROM (
                      SELECT ISNULL(Rel.DBS, '') AS DBS
	                    , (CASE Rel.DBS WHEN 'SH' THEN 'RMB' WHEN 'TW' THEN 'NTD' ELSE '' END) AS Currency
	                    , RTRIM(Prod.Model_No) AS ModelNo, ISNULL(Rel.PackItemNo, '') PackItemNo, ISNULL(Rel.PackQty, 0) PackQty, Prod.Pub_Notes
	                    , CONVERT(numeric(10,2), ISNULL(tw_ModelPrice.Price, 0)) AS tw_PurPrice
	                    , CONVERT(numeric(10,2), ISNULL(sh_ModelPrice.Price, 0)) AS sh_PurPrice
	                    , CONVERT(numeric(10,2), ISNULL(tw_PackPrice.Price, 0)) AS tw_PackPurPrice
	                    , CONVERT(numeric(10,2), ISNULL(sh_PackPrice.Price, 0)) AS sh_PackPurPrice
	                    , ROW_NUMBER() OVER(
	                          PARTITION BY Prod.Ship_From, Prod.Model_No ORDER BY Prod.Model_No, Rel.PackItemNo
	                      ) AS RowGroupIdx
	                    , ROW_NUMBER() OVER(ORDER BY Prod.Model_No) AS RowIdx
	                    FROM [ProductCenter].dbo.Prod_Item Prod
	                     LEFT JOIN [ProductCenter].dbo.Prod_Rel_Package Rel ON Prod.Model_No = Rel.ModelNo AND Prod.Ship_From = Rel.DBS
	                     LEFT JOIN TblChkPrice_TW AS tw_ModelPrice ON Prod.Model_No COLLATE Chinese_Taiwan_Stroke_BIN = tw_ModelPrice.ModelNo AND Prod.Provider COLLATE Chinese_Taiwan_Stroke_BIN = tw_ModelPrice.SupID
	                     LEFT JOIN TblChkPrice_SH AS sh_ModelPrice ON Prod.Model_No COLLATE Chinese_Taiwan_Stroke_BIN = sh_ModelPrice.ModelNo AND Prod.Provider COLLATE Chinese_Taiwan_Stroke_BIN = sh_ModelPrice.SupID
	                     LEFT JOIN TblChkPrice_TW AS tw_PackPrice ON Rel.PackItemNo COLLATE Chinese_Taiwan_Stroke_BIN = tw_PackPrice.ModelNo AND Rel.SupID COLLATE Chinese_Taiwan_Stroke_BIN = tw_PackPrice.SupID
	                     LEFT JOIN TblChkPrice_SH AS sh_PackPrice ON Rel.PackItemNo COLLATE Chinese_Taiwan_Stroke_BIN = sh_PackPrice.ModelNo AND Rel.SupID COLLATE Chinese_Taiwan_Stroke_BIN = sh_PackPrice.SupID
	                    WHERE (Prod.Ship_From = @dbs) AND (LEFT(Prod.Model_No, 1) <> '0')";

                    //append sql
                    sql.Append(subSql);

                    #region >> 條件組合 <<

                    if (search != null)
                    {
                        //過濾空值
                        var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                        //查詢內容
                        foreach (var item in thisSearch)
                        {
                            switch (item.Key)
                            {
                                case "Keyword":
                                    sql.Append(" AND (");
                                    sql.Append("  (UPPER(Prod.Model_No) LIKE UPPER(@Keyword) + '%')");
                                    sql.Append(" )");

                                    sqlParamList.Add(new SqlParameter("@Keyword", item.Value));

                                    break;

                                case "ModelNo":
                                    //指定品號(多筆)
                                    string[] aryValID = Regex.Split(item.Value, ",");
                                    ArrayList aryValLst = new ArrayList(aryValID);

                                    /*
                                     GetSQLParam:SQL WHERE IN的方法 ,ex:UPPER(ModelNo) IN ({0})
                                    */
                                    string filterParams = CustomExtension.GetSQLParam(aryValLst, "pModel");
                                    sql.Append(" AND (UPPER(Prod.Model_No) IN ({0}))".FormatThis(filterParams));

                                    //SQL參數組成
                                    for (int row = 0; row < aryValID.Count(); row++)
                                    {
                                        sqlParamList.Add(new SqlParameter("@pModel" + row, aryValID[row]));
                                    }

                                    break;

                                case "SupID":
                                    //廠商
                                    sql.Append(" AND (Prod.Provider = @SupID)");

                                    sqlParamList.Add(new SqlParameter("@SupID", item.Value));

                                    break;

                            }
                        }
                    }
                    #endregion

                    sql.AppendLine(") AS TbAll");

                    //是否分頁
                    if (doPaging)
                    {
                        sql.AppendLine(" WHERE (TbAll.RowIdx >= @startRow) AND (TbAll.RowIdx <= @endRow)");

                        sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                        sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    }
                    sql.AppendLine(" ORDER BY TbAll.RowIdx");


                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();
                    cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@dbs", dbs.ToUpper()));


                    ////----- SQL 參數陣列 -----
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    myDT = dbConn.LookupDT(cmd, out ErrMsg);
                    AllErrMsg += ErrMsg;
                }

                #endregion


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    //append 主體語法
                    sql.Append(mainSql);

                    //欄位select
                    string subsql = @"
                    SELECT COUNT(Prod.Model_No) AS TotalCnt
                    FROM [ProductCenter].dbo.Prod_Item Prod
	                     LEFT JOIN [ProductCenter].dbo.Prod_Rel_Package Rel ON Prod.Model_No = Rel.ModelNo AND Prod.Ship_From = Rel.DBS
	                     LEFT JOIN TblChkPrice_TW AS tw_ModelPrice ON Prod.Model_No COLLATE Chinese_Taiwan_Stroke_BIN = tw_ModelPrice.ModelNo AND Prod.Provider COLLATE Chinese_Taiwan_Stroke_BIN = tw_ModelPrice.SupID
	                     LEFT JOIN TblChkPrice_SH AS sh_ModelPrice ON Prod.Model_No COLLATE Chinese_Taiwan_Stroke_BIN = sh_ModelPrice.ModelNo AND Prod.Provider COLLATE Chinese_Taiwan_Stroke_BIN = sh_ModelPrice.SupID
	                     LEFT JOIN TblChkPrice_TW AS tw_PackPrice ON Rel.PackItemNo COLLATE Chinese_Taiwan_Stroke_BIN = tw_PackPrice.ModelNo AND Rel.SupID COLLATE Chinese_Taiwan_Stroke_BIN = tw_PackPrice.SupID
	                     LEFT JOIN TblChkPrice_SH AS sh_PackPrice ON Rel.PackItemNo COLLATE Chinese_Taiwan_Stroke_BIN = sh_PackPrice.ModelNo AND Rel.SupID COLLATE Chinese_Taiwan_Stroke_BIN = sh_PackPrice.SupID
                    WHERE (Prod.Ship_From = @dbs) AND (LEFT(Prod.Model_No, 1) <> '0')";

                    //append
                    sql.Append(subsql);


                    #region >> 條件組合 <<

                    if (search != null)
                    {
                        //過濾空值
                        var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                        //查詢內容
                        foreach (var item in thisSearch)
                        {
                            switch (item.Key)
                            {
                                case "Keyword":
                                    sql.Append(" AND (");
                                    sql.Append("  (UPPER(Prod.Model_No) LIKE UPPER(@Keyword) + '%')");
                                    sql.Append(" )");

                                    sqlParamList_Cnt.Add(new SqlParameter("@Keyword", item.Value));

                                    break;

                                case "ModelNo":
                                    //指定品號(多筆)
                                    string[] aryValID = Regex.Split(item.Value, ",");
                                    ArrayList aryValLst = new ArrayList(aryValID);

                                    /*
                                     GetSQLParam:SQL WHERE IN的方法 ,ex:UPPER(ModelNo) IN ({0})
                                    */
                                    string filterParams = CustomExtension.GetSQLParam(aryValLst, "pModel");
                                    sql.Append(" AND (UPPER(Prod.Model_No) IN ({0}))".FormatThis(filterParams));

                                    //SQL參數組成
                                    for (int row = 0; row < aryValID.Count(); row++)
                                    {
                                        sqlParamList_Cnt.Add(new SqlParameter("@pModel" + row, aryValID[row]));
                                    }

                                    break;


                                case "SupID":
                                    //廠商
                                    sql.Append(" AND (Prod.Provider = @SupID)");

                                    sqlParamList_Cnt.Add(new SqlParameter("@SupID", item.Value));

                                    break;
                            }
                        }
                    }
                    #endregion


                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();

                    //----- SQL 固定參數 -----
                    sqlParamList_Cnt.Add(new SqlParameter("@dbs", dbs.ToUpper()));

                    //----- SQL 參數陣列 -----
                    cmdCnt.Parameters.AddRange(sqlParamList_Cnt.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, out ErrMsg))
                    {
                        //資料總筆數
                        if (DTCnt.Rows.Count > 0)
                        {
                            DataCnt = Convert.ToInt32(DTCnt.Rows[0]["TotalCnt"]);
                        }
                    }
                    AllErrMsg += ErrMsg;

                    //*** 在SqlParameterCollection同個循環內不可有重複的SqlParam,必須清除才能繼續使用. ***
                    cmdCnt.Parameters.Clear();
                }
                #endregion

                //return
                if (!string.IsNullOrWhiteSpace(AllErrMsg)) ErrMsg = AllErrMsg;


                /* 資料整理 */
                #region -- 資料整理 --

                //LinQ 查詢
                var query = myDT.AsEnumerable();

                //資料迴圈
                foreach (var item in query)
                {
                    /*
                      規則：
                       RowGroupIdx > 1 => (品號核價單價)ModelPrice = 0
                       依DBS取價 => TW = tw_xxxx ; SH = sh_oooo
                    */
                    /*
                       品號 = string ModelNo
                       卡片品號 = string PackItemNo
                       幣別 = string Currency
                       品號核價單價 = double ModelPrice
                       卡片核價單價 = double PackPrice
                       卡片數量 = Int16 PackQty
                       品號備註 = string ProdNote
                       標準成本 = double ProdCost (品號核價單價 + (卡片核價單價*數量))
                       卡片核價單價*數量 = double PackSumPrice
                    */

                    //品號核價單價
                    decimal _modelPrice = dbs.Equals("TW") ? item.Field<decimal>("tw_PurPrice") : item.Field<decimal>("sh_PurPrice");
                    Int64 _rowGroupIdx = item.Field<Int64>("RowGroupIdx");
                    if (_rowGroupIdx > 1) _modelPrice = 0;

                    //卡片核價單價
                    decimal _packPrice = dbs.Equals("TW") ? item.Field<decimal>("tw_PackPurPrice") : item.Field<decimal>("sh_PackPurPrice");

                    //卡片數量
                    decimal _packQty = item.Field<decimal>("PackQty");

                    //加入項目
                    var data = new PurProdCostList
                    {
                        ModelNo = item.Field<string>("ModelNo"),
                        PackItemNo = item.Field<string>("PackItemNo"),
                        Currency = item.Field<string>("Currency"),
                        ModelPrice = _modelPrice,
                        PackPrice = _packPrice,
                        PackQty = _packQty,
                        ProdNote = item.Field<string>("Pub_Notes").Replace("\r\n", "<br/>"),
                        ProdCost = Math.Round(_modelPrice + (_packPrice * _packQty), 1),
                        PackSumPrice = Math.Round(_packPrice * _packQty, 2),
                        RowIdx = item.Field<Int64>("RowIdx")
                    };


                    //將項目加入至集合
                    dataList.Add(data);

                }

                #endregion

                //回傳集合
                return dataList.AsQueryable();


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        /// <summary>
        /// [標準成本] 品號List
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow(從0開始)</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="doPaging">是否分頁</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns>DataTable</returns>
        public DataTable GetCost_ProdList(Dictionary<string, string> search
            , int startRow, int endRow, bool doPaging
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";
            string AllErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> sqlParamList_Cnt = new List<SqlParameter>(); //SQL參數容器
                DataTable myDT = new DataTable();
                DataCnt = 0;    //資料總數


                #region >> 主要資料SQL查詢 <<

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    string mainSql = @"
                    SELECT TbAll.*
                    FROM (
	                    SELECT RTRIM(Prod.Model_No) AS ModelNo, RTRIM(Prod.Model_Name_zh_TW) AS ModelName	
	                    , ROW_NUMBER() OVER(ORDER BY Prod.Model_No) AS RowIdx
	                    FROM [ProductCenter].dbo.Prod_Item Prod
	                    WHERE (LEFT(Prod.Model_No, 1) <> '0')";

                    //append sql
                    sql.Append(mainSql);

                    #region >> 條件組合 <<

                    if (search != null)
                    {
                        //過濾空值
                        var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                        //查詢內容
                        foreach (var item in thisSearch)
                        {
                            switch (item.Key)
                            {
                                case "Keyword":
                                    sql.Append(" AND (");
                                    sql.Append("  (UPPER(Prod.Model_No) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append(" )");

                                    sqlParamList.Add(new SqlParameter("@Keyword", item.Value));

                                    break;

                                    //case "ModelNo":
                                    //    //指定品號(多筆)
                                    //    string[] aryValID = Regex.Split(item.Value, ",");
                                    //    ArrayList aryValLst = new ArrayList(aryValID);

                                    //    /*
                                    //     GetSQLParam:SQL WHERE IN的方法 ,ex:UPPER(ModelNo) IN ({0})
                                    //    */
                                    //    string filterParams = CustomExtension.GetSQLParam(aryValLst, "pModel");
                                    //    sql.Append(" AND (UPPER(Prod.Model_No) IN ({0}))".FormatThis(filterParams));

                                    //    //SQL參數組成
                                    //    for (int row = 0; row < aryValID.Count(); row++)
                                    //    {
                                    //        sqlParamList.Add(new SqlParameter("@pModel" + row, aryValID[row]));
                                    //    }

                                    //    break;
                            }
                        }
                    }
                    #endregion

                    //Sql尾段
                    sql.AppendLine(") AS TbAll");

                    //是否分頁
                    if (doPaging)
                    {
                        sql.AppendLine(" WHERE (TbAll.RowIdx >= @startRow) AND (TbAll.RowIdx <= @endRow)");

                        sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                        sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    }
                    sql.AppendLine(" ORDER BY TbAll.RowIdx");


                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();

                    //----- SQL 參數陣列 -----
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    myDT = dbConn.LookupDT(cmd, out ErrMsg);
                    AllErrMsg += ErrMsg;
                }

                #endregion


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    string mainSql = @"
                    SELECT COUNT(Prod.Model_No) AS TotalCnt
	                FROM [ProductCenter].dbo.Prod_Item Prod
	                WHERE (LEFT(Prod.Model_No, 1) <> '0')";

                    //append
                    sql.Append(mainSql);


                    #region >> 條件組合 <<

                    if (search != null)
                    {
                        //過濾空值
                        var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                        //查詢內容
                        foreach (var item in thisSearch)
                        {
                            switch (item.Key)
                            {
                                case "Keyword":
                                    sql.Append(" AND (");
                                    sql.Append("  (UPPER(Prod.Model_No) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append(" )");

                                    sqlParamList_Cnt.Add(new SqlParameter("@Keyword", item.Value));

                                    break;


                                    //case "ModelNo":
                                    //    //指定品號(多筆)
                                    //    string[] aryValID = Regex.Split(item.Value, ",");
                                    //    ArrayList aryValLst = new ArrayList(aryValID);

                                    //    /*
                                    //     GetSQLParam:SQL WHERE IN的方法 ,ex:UPPER(ModelNo) IN ({0})
                                    //    */
                                    //    string filterParams = CustomExtension.GetSQLParam(aryValLst, "pModel");
                                    //    sql.Append(" AND (UPPER(Prod.Model_No) IN ({0}))".FormatThis(filterParams));

                                    //    //SQL參數組成
                                    //    for (int row = 0; row < aryValID.Count(); row++)
                                    //    {
                                    //        sqlParamList_Cnt.Add(new SqlParameter("@pModel" + row, aryValID[row]));
                                    //    }

                                    //    break;

                            }
                        }
                    }
                    #endregion


                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();

                    //----- SQL 參數陣列 -----
                    cmdCnt.Parameters.AddRange(sqlParamList_Cnt.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, out ErrMsg))
                    {
                        //資料總筆數
                        if (DTCnt.Rows.Count > 0)
                        {
                            DataCnt = Convert.ToInt32(DTCnt.Rows[0]["TotalCnt"]);
                        }
                    }
                    AllErrMsg += ErrMsg;

                    //*** 在SqlParameterCollection同個循環內不可有重複的SqlParam,必須清除才能繼續使用. ***
                    cmdCnt.Parameters.Clear();
                }
                #endregion

                //return
                if (!string.IsNullOrWhiteSpace(AllErrMsg)) ErrMsg = AllErrMsg;

                //回傳集合
                return myDT;


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        /// <summary>
        /// [標準成本] 品號關聯包材
        /// </summary>
        /// <param name="_models">逗號分隔的品號</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetCostRel_Pack(string _models, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    string sql = @"
                    SELECT Rel.UID AS DataID, Rel.ModelNo, Rel.DBS, Rel.PackItemNo, Rel.PackQty
                    FROM [ProductCenter].dbo.Prod_Rel_Package Rel
                    WHERE (1=1)";

                    if (!string.IsNullOrWhiteSpace(_models))
                    {
                        //指定品號(多筆)
                        string[] aryValID = Regex.Split(_models, ",");
                        ArrayList aryValLst = new ArrayList(aryValID);

                        /*
                         GetSQLParam:SQL WHERE IN的方法 ,ex:UPPER(ModelNo) IN ({0})
                        */
                        string filterParams = CustomExtension.GetSQLParam(aryValLst, "pModel");
                        sql += " AND (UPPER(Rel.ModelNo) IN ({0}))".FormatThis(filterParams);

                        //SQL參數組成
                        for (int row = 0; row < aryValID.Count(); row++)
                        {
                            cmd.Parameters.AddWithValue("@pModel" + row, aryValID[row]);
                        }

                    }

                    //----- SQL 執行 -----
                    cmd.CommandText = sql;

                    //return
                    return dbConn.LookupDT(cmd, out ErrMsg);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        #endregion *** 標準成本 E ***


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
        /// 檢查庫別檔是否有資料[prokit2]
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


        #region ** 標準成本 **
        /// <summary>
        /// [標準成本] 新增包材
        /// </summary>
        /// <param name="dbs"></param>
        /// <param name="modelNo"></param>
        /// <param name="packItem"></param>
        /// <param name="qty"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreatePackItem(string dbs, string modelNo, string packItem, decimal qty
            , out string ErrMsg)
        {
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    DECLARE @SupID AS VARCHAR(40)
                    SET @SupID = (
                    SELECT MB032 FROM [#dbName#].dbo.INVMB WHERE (MB001 = @PackItemNo)
                    )

                    IF (SELECT COUNT(*) FROM [ProductCenter].dbo.Prod_Rel_Package WHERE (DBS = @DBS) AND (ModelNo = @ModelNo) AND (PackItemNo = @PackItemNo)) = 0
                    BEGIN
                        INSERT INTO [ProductCenter].dbo.Prod_Rel_Package(
                            DBS, SupID, ModelNo, PackItemNo, PackQty
                            , Create_Who, Create_Time
                        ) VALUES (
                            @DBS, @SupID, @ModelNo, @PackItemNo, @PackQty
                            , @Create_Who, GETDATE()
                        )
                    END";

                sql = sql.Replace("#dbName#", dbs.Equals("TW") ? "prokit2" : "SHPK2");

                //----- SQL 執行 -----
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("DBS", dbs);
                cmd.Parameters.AddWithValue("ModelNo", modelNo);
                cmd.Parameters.AddWithValue("PackItemNo", packItem);
                cmd.Parameters.AddWithValue("PackQty", qty);
                cmd.Parameters.AddWithValue("Create_Who", fn_Param.CurrentUser);


                return dbConn.ExecuteSql(cmd, out ErrMsg);
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
        /// 刪除資料
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


        #region ** 標準成本 **
        public bool Delete_PackItem(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Prod_Rel_Package WHERE (UID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.Product, out ErrMsg);
            }
        }
        #endregion


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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LinqToExcel;
using Menu3000Data.Models;
using PKLib_Method.Methods;

/*
  [銷貨單庫存狀況]-OrderStockStat
  [訂單庫存狀況]-OrderingStockStat
  [發貨維護]-ShipFreight
  [客戶返利]-CustRebate
  [客訴]-CustComplaint
  [電商數據]-eCommerceData
  [出貨明細表]-ShipData
*/
namespace Menu3000Data.Controllers
{
    public class Menu3000Repository
    {
        public string ErrMsg;

        #region -----// Read //-----

        /// <summary>
        /// 取得Excel內容
        /// </summary>
        /// <param name="filePath">完整磁碟路徑</param>
        /// <param name="sheetName">工作表名稱</param>
        /// <returns></returns>
        /// <example>
        /// <table id="listTable" class="stripe" cellspacing="0" width="100%" style="width:100%;">
        ///     <asp:Literal ID="lt_tbBody" runat="server"></asp:Literal>
        /// </table>
        /// </example>
        public StringBuilder GetExcel_Html(string filePath, string sheetName)
        {
            try
            {
                //宣告
                StringBuilder html = new StringBuilder();

                //[Excel] - 取得原始資料
                var excelFile = new ExcelQueryFactory(filePath);

                //[HTML] - 取得欄位, 輸出標題欄 (GetColumnNames)
                var queryCols = excelFile.GetColumnNames(sheetName);

                html.Append("<thead>");
                html.Append("<tr>");
                foreach (var col in queryCols)
                {
                    html.Append("<th>{0}</th>".FormatThis(col.ToString()));
                }
                html.Append("</tr>");
                html.Append("</thead>");


                //[處理合併儲存格] - 暫存欄:ID
                string tmp_OrderID = "";

                //[HTML] - 取得欄位值, 輸出內容欄 (Worksheet)
                var queryVals = excelFile.Worksheet(sheetName);

                html.Append("<tbody>");
                foreach (var val in queryVals)
                {
                    //[處理合併儲存格] - 目前的ID
                    string curr_ID = val[0].ToString();

                    //[處理合併儲存格] - 目前欄位非空值, 填入暫存值
                    if (!string.IsNullOrEmpty(curr_ID))
                    {
                        tmp_OrderID = curr_ID;
                    }

                    //內容迴圈
                    html.Append("<tr>");

                    int myCol = 0;
                    foreach (var col in queryCols)
                    {
                        //ID為欄位的第1欄
                        if (myCol.Equals(0))
                        {
                            //若目前欄位為空值,則填入暫存值
                            html.Append("<td>{0}</td>".FormatThis(string.IsNullOrEmpty(curr_ID) ? tmp_OrderID : curr_ID));
                        }
                        else
                        {
                            html.Append("<td>{0}</td>".FormatThis(val[col]));
                        }

                        myCol++;
                    }

                    html.Append("</tr>");
                }

                html.Append("</tbody>");

                //output
                return html;
            }
            catch (Exception ex)
            {

                throw new Exception("請檢查Excel格式是否正確!!" + ex.Message.ToString());
            }
        }

        /// <summary>
        /// 取得產品類別
        /// </summary>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetProdClass(string lang, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT CAST(Class_ID AS INT) AS ID, Class_Name_{0} AS Label"
                    .FormatThis(fn_Language.Get_LangCode(lang).Replace("-", "_")));
                sql.AppendLine(" FROM Prod_Class WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Display = 'Y')");
                sql.AppendLine(" ORDER BY Sort");

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
                            ID = item.Field<int>("ID"),
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


        #region *** 銷貨單庫存狀況 S ***
        /// <summary>
        /// 銷貨單庫存狀況(OrderStockStat)
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<OrderStockItem> GetOrderStockStat(string CompID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<OrderStockItem> dataList = new List<OrderStockItem>();
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----

                /** 預計銷(訂單):TD016 = 結案碼, TD021 = 確認碼 **/
                #region --預計銷--
                sql.AppendLine(" ;WITH Tbl_PreSell AS (");
                sql.AppendLine("	SELECT p.ModelNo, p.A01 AS PreSell_A01, p.B01 AS PreSell_B01");
                sql.AppendLine("	FROM (");
                sql.AppendLine("		SELECT (ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0)");
                sql.AppendLine("		  - ISNULL((");
                sql.AppendLine("			SELECT SUM(ISNULL(INVTG.TG009, 0))");
                sql.AppendLine("			FROM [{0}].dbo.INVTG WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("			WHERE COPTD.TD004 = TG004 AND TG007 = COPTD.TD007 AND TG001 = '1302' AND TG008 = 'C01' AND TG024 = 'N'");
                sql.AppendLine("		  ),0)");
                sql.AppendLine("		 ) AS PreSell");
                sql.AppendLine("		 , RTRIM(TD004) AS ModelNo");
                sql.AppendLine("		 , TD007 AS StockType");
                sql.AppendLine("		FROM [{0}].dbo.COPTD WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("		WHERE (TD016 = 'N') AND (TD021 = 'Y') AND (TD007 IN ('A01','B01'))");
                sql.AppendLine("		GROUP BY TD004, TD007");
                sql.AppendLine("	) t ");
                sql.AppendLine("	PIVOT (");
                sql.AppendLine("		SUM(PreSell)");
                sql.AppendLine("		FOR StockType IN ([A01], [B01])");
                sql.AppendLine("	) p");
                sql.AppendLine(" )");
                #endregion


                /** 預計進(採購單):TD016 = 結案碼, TD018 = 確認碼 **/
                #region --預計進--
                sql.AppendLine(" , Tbl_PreIN AS (");
                sql.AppendLine("	SELECT p.ModelNo, p.A01 AS PreIN_A01, p.B01 AS PreIN_B01");
                sql.AppendLine("	FROM (");
                sql.AppendLine("		SELECT ISNULL(SUM(TD008 - TD015), 0) AS PreIN, RTRIM(TD004) AS ModelNo, TD007 AS StockType");
                sql.AppendLine("		FROM [{0}].dbo.PURTD WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("		WHERE (TD016 = 'N') AND (TD018 = 'Y') AND (TD007 IN ('A01','B01'))");
                sql.AppendLine("		GROUP BY TD004, TD007");
                sql.AppendLine("	) t ");
                sql.AppendLine("	PIVOT (");
                sql.AppendLine("		SUM(PreIN)");
                sql.AppendLine("		FOR StockType IN ([A01], [B01])");
                sql.AppendLine("	) p");
                sql.AppendLine(" )");
                #endregion


                #region --庫存--
                sql.AppendLine(" , Tbl_Stock AS (");
                sql.AppendLine("	SELECT p.ModelNo, p.A01 AS StockQty_A01, p.B01 AS StockQty_B01");
                sql.AppendLine("	FROM (");
                sql.AppendLine("		SELECT MC007 AS StockQty, RTRIM(MC001) AS ModelNo, MC002 AS StockType");
                sql.AppendLine("		FROM [{0}].dbo.INVMC WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("		WHERE (MC002 IN ('A01','B01'))");
                sql.AppendLine("	) t ");
                sql.AppendLine("	PIVOT (");
                sql.AppendLine("		SUM(StockQty)");
                sql.AppendLine("		FOR StockType IN ([A01], [B01])");
                sql.AppendLine("	) p");
                sql.AppendLine(" ) ");
                #endregion

                /** 開始取資料 **/
                sql.AppendLine("SELECT Tbl.* FROM (");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(COPTC.TC004) AS CustID");
                sql.AppendLine("  , RTRIM(COPMA.MA002) AS CustName");
                sql.AppendLine("  , COPTH.TH001 AS SO_Fid, RTRIM(COPTH.TH002) AS SO_Sid, COPTH.TH003 AS Sno, COPTH.TH004 AS ModelNo");
                sql.AppendLine("  , COPTH.TH007 AS StockType, COPTH.TH008 AS BuyCnt");
                sql.AppendLine("  , (CASE WHEN COPTC.TC201 = '' THEN 'ERP打單' ELSE COPTC.TC201 END) AS ShopWhere, COPTC.TC202 AS ShopOrderID");
                sql.AppendLine("  , ISNULL(stock.StockQty_A01, 0) AS StockQty_A01, ISNULL(sell.PreSell_A01, 0) AS PreSell_A01, ISNULL(buy.PreIN_A01, 0) AS PreIN_A01");
                sql.AppendLine("  , ISNULL(stock.StockQty_B01, 0) AS StockQty_B01, ISNULL(sell.PreSell_B01, 0) AS PreSell_B01, ISNULL(buy.PreIN_B01, 0) AS PreIN_B01");
                sql.AppendLine(" FROM [{0}].dbo.COPTH WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPTD WITH (NOLOCK) ON COPTH.TH014 = COPTD.TD001 AND COPTH.TH015 = COPTD.TD002 AND COPTH.TH016 = COPTD.TD003".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPTC WITH (NOLOCK) ON COPTD.TD001 = COPTC.TC001 AND COPTD.TD002 = COPTC.TC002".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPMA WITH (NOLOCK) ON COPTC.TC004 = COPMA.MA001".FormatThis(dbName));
                sql.AppendLine("  LEFT JOIN Tbl_Stock AS stock ON COPTH.TH004 = stock.ModelNo");
                sql.AppendLine("  LEFT JOIN Tbl_PreSell AS sell ON COPTH.TH004 = sell.ModelNo");
                sql.AppendLine("  LEFT JOIN Tbl_PreIN AS buy ON COPTH.TH004 = buy.ModelNo");
                //條件:未確認銷貨單
                sql.AppendLine("  WHERE (UPPER(RTRIM(COPTH.TH001)) + UPPER(RTRIM(COPTH.TH002)) IN (");
                sql.AppendLine("   SELECT UPPER(RTRIM(COPTG.TG001)) + UPPER(RTRIM(COPTG.TG002))");
                sql.AppendLine("   FROM [{0}].dbo.COPTG WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("   WHERE (TG023 = 'N')");
                sql.AppendLine("  ))");
                sql.AppendLine(" ) AS Tbl");
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
                            case "SoNo":
                                //銷貨單號(不含-)
                                sql.Append(" AND (UPPER(Tbl.SO_Fid) + UPPER(Tbl.SO_Sid) = UPPER(@SOID))");

                                cmd.Parameters.AddWithValue("SOID", item.Value);

                                break;

                            case "ShopWhere":
                                //來源平台
                                sql.Append(" AND (UPPER(Tbl.ShopWhere) = UPPER(@ShopWhere))");

                                cmd.Parameters.AddWithValue("ShopWhere", item.Value);

                                break;

                            case "ShopOrderID":
                                //平台單號
                                sql.Append(" AND (UPPER(Tbl.ShopOrderID) = UPPER(@ShopOrderID))");

                                cmd.Parameters.AddWithValue("ShopOrderID", item.Value);

                                break;

                            case "CustID":
                                //客戶代號
                                sql.Append(" AND (UPPER(Tbl.CustID) = UPPER(@CustID))");

                                cmd.Parameters.AddWithValue("CustID", item.Value);

                                break;

                            case "ModelNo":
                                //品號
                                sql.Append(" AND (UPPER(Tbl.ModelNo) = UPPER(@ModelNo))");

                                cmd.Parameters.AddWithValue("ModelNo", item.Value);

                                break;

                        }
                    }
                }
                #endregion

                sql.AppendLine(" ORDER BY Tbl.CustID, Tbl.SO_Fid, Tbl.SO_Sid, Tbl.Sno");

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
                        var data = new OrderStockItem
                        {
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            SO_Fid = item.Field<string>("SO_Fid"),
                            SO_Sid = item.Field<string>("SO_Sid"),
                            ModelNo = item.Field<string>("ModelNo"),
                            StockType = item.Field<string>("StockType"),
                            BuyCnt = Convert.ToInt32(item.Field<decimal>("BuyCnt")),
                            ShopWhere = item.Field<string>("ShopWhere"),
                            ShopOrderID = item.Field<string>("ShopOrderID"),
                            StockQty_A01 = Convert.ToInt32(item.Field<decimal>("StockQty_A01")),
                            PreSell_A01 = Convert.ToInt32(item.Field<decimal>("PreSell_A01")),
                            PreIN_A01 = Convert.ToInt32(item.Field<decimal>("PreIN_A01")),
                            gapA01 = Convert.ToInt32(item.Field<decimal>("StockQty_A01") - item.Field<decimal>("PreSell_A01")),

                            StockQty_B01 = Convert.ToInt32(item.Field<decimal>("StockQty_B01")),
                            PreSell_B01 = Convert.ToInt32(item.Field<decimal>("PreSell_B01")),
                            PreIN_B01 = Convert.ToInt32(item.Field<decimal>("PreIN_B01")),
                            gapB01 = Convert.ToInt32(item.Field<decimal>("StockQty_B01") - item.Field<decimal>("PreSell_B01"))
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
        /// 預計銷明細
        /// </summary>
        /// <param name="CompID"></param>
        /// <param name="modelNo"></param>
        /// <param name="stockType"></param>
        /// <returns></returns>
        public IQueryable<PreSellItems> GetPreSellItems(string CompID, string modelNo, ArrayList stockType)
        {
            //----- 宣告 -----
            List<PreSellItems> dataList = new List<PreSellItems>();
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT (ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0)");
                sql.AppendLine(" 	- ISNULL((");
                sql.AppendLine(" 	 SELECT SUM(ISNULL(INVTG.TG009, 0))");
                sql.AppendLine(" 	 FROM [{0}].dbo.INVTG WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine(" 	 WHERE COPTD.TD004 = TG004 AND TG007 = COPTD.TD007 AND TG001 = '1302' AND TG008 = 'C01' AND TG024 = 'N'");
                sql.AppendLine(" 	 ),0)");
                sql.AppendLine(" 	) AS PreSell");
                sql.AppendLine(" 	, RTRIM(TD004) AS ModelNo");
                sql.AppendLine(" 	, TD007 AS StockType");
                sql.AppendLine(" FROM [{0}].dbo.COPTD WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine(" WHERE (TD016 = 'N') AND (TD021 = 'Y')");

                //庫別
                if (stockType.Count > 0)
                {
                    //GetSQLParam:SQL WHERE IN的方法
                    sql.AppendLine(" AND RTRIM(TD007) IN ({0})".FormatThis(CustomExtension.GetSQLParam(stockType, "params")));
                    for (int row = 0; row < stockType.Count; row++)
                    {
                        cmd.Parameters.AddWithValue("params" + row, stockType[row]);
                    }
                }

                //品號
                if (!string.IsNullOrWhiteSpace(modelNo))
                {
                    sql.AppendLine("  AND (TD004 = @ModelNo)");

                    cmd.Parameters.AddWithValue("ModelNo", modelNo);
                }

                sql.AppendLine(" GROUP BY TD004, TD007");
                sql.AppendLine(" ORDER BY TD004, TD007");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new PreSellItems
                        {
                            ModelNo = item.Field<string>("ModelNo"),
                            Qty = Convert.ToInt32(item.Field<decimal>("PreSell")),
                            StockType = item.Field<string>("StockType")
                        };


                        //將項目加入至集合
                        dataList.Add(data);

                    }
                }

                //回傳集合
                return dataList.AsQueryable();


            }
        }

        #endregion *** 銷貨單庫存狀況 E ***


        #region *** 訂單庫存狀況 S ***
        /// <summary>
        /// 訂單庫存狀況(OrderingStockStat)
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetOrderingStockStat(string CompID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----

                /* SQL CTE */
                /** 預計銷(訂單):TD016 = 結案碼, TD021 = 確認碼 **/
                #region --預計銷--
                sql.AppendLine(" ;WITH Tbl_PreSell AS (");
                sql.AppendLine("	SELECT p.ModelNo");
                sql.AppendLine("     , p.[01] AS PreSell_01, p.[20] AS PreSell_20, p.[22] AS PreSell_22, p.[21] AS PreSell_21");
                sql.AppendLine("     , p.[12] AS PreSell_12, p.[128] AS PreSell_128");
                sql.AppendLine("     , p.[A01] AS PreSell_A01");
                sql.AppendLine("	FROM (");
                sql.AppendLine("		SELECT ISNULL(SUM(TD008 - TD009 + TD024 - TD025), 0) AS PreSell");
                sql.AppendLine("		 , RTRIM(TD004) AS ModelNo");
                sql.AppendLine("		 , TD007 AS StockType");
                sql.AppendLine("		FROM [{0}].dbo.COPTD WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("		WHERE (TD016 = 'N') AND (TD021 = 'Y') AND (TD007 IN ('01', '20', '22', '21', '12', '128', 'A01'))");
                sql.AppendLine("		GROUP BY TD004, TD007");
                sql.AppendLine("	) t ");
                sql.AppendLine("	PIVOT (");
                sql.AppendLine("		SUM(PreSell)");
                sql.AppendLine("		FOR StockType IN ([01], [20], [22], [21], [12], [128], [A01])");
                sql.AppendLine("	) p");
                sql.AppendLine(" )");
                #endregion


                /** 預計進(採購單):TD016 = 結案碼, TD018 = 確認碼 **/
                #region --預計進--
                sql.AppendLine(" , Tbl_PreIN AS (");
                sql.AppendLine("	SELECT p.ModelNo");
                sql.AppendLine("     , p.[01] AS PreIN_01, p.[20] AS PreIN_20, p.[22] AS PreIN_22, p.[21] AS PreIN_21");
                sql.AppendLine("     , p.[12] AS PreIN_12, p.[128] AS PreIN_128");
                sql.AppendLine("     , p.[A01] AS PreIN_A01");
                sql.AppendLine("	FROM (");
                sql.AppendLine("		SELECT ISNULL(SUM(TD008 - TD015), 0) AS PreIN");
                sql.AppendLine("		 , RTRIM(TD004) AS ModelNo");
                sql.AppendLine("		 , TD007 AS StockType");
                sql.AppendLine("		FROM [{0}].dbo.PURTD WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("		WHERE (TD016 = 'N') AND (TD018 = 'Y') AND (TD007 IN ('01', '20', '22', '21', '12', '128', 'A01'))");
                sql.AppendLine("		GROUP BY TD004, TD007");
                sql.AppendLine("	) t ");
                sql.AppendLine("	PIVOT (");
                sql.AppendLine("		SUM(PreIN)");
                sql.AppendLine("		FOR StockType IN ([01], [20], [22], [21], [12], [128], [A01])");
                sql.AppendLine("	) p");
                sql.AppendLine(" )");
                #endregion


                /** 預計生:TA013,確認碼 <> 'V', TA011,狀態碼 = 1,2,3 **/
                #region --預計生--
                sql.AppendLine(" , Tbl_PreSet AS (");
                sql.AppendLine(" 	SELECT p.ModelNo");
                sql.AppendLine(" 	 , p.[01] AS PreSet_01, p.[20] AS PreSet_20, p.[22] AS PreSet_22, p.[21] AS PreSet_21");
                sql.AppendLine("     , p.[12] AS PreSet_12, p.[128] AS PreSet_128");
                sql.AppendLine("     , p.[A01] AS PreSet_A01");
                sql.AppendLine(" 	FROM (");
                sql.AppendLine(" 		SELECT SUM(TA015 - TA017) AS PreSet");
                sql.AppendLine(" 		 , RTRIM(TA006) AS ModelNo");
                sql.AppendLine(" 		 , TA020 AS StockType");
                sql.AppendLine(" 		FROM [prokit2].dbo.MOCTA WITH(NOLOCK)");
                sql.AppendLine(" 		WHERE (TA013 <> 'V') AND (TA011 IN ('1', '2', '3'))");
                sql.AppendLine(" 		 AND (TA020 IN ('01', '20', '22', '21', '12', '128', 'A01'))");
                sql.AppendLine(" 		GROUP BY TA006, TA020");
                sql.AppendLine(" 	) t");
                sql.AppendLine(" 	PIVOT (");
                sql.AppendLine(" 		SUM(PreSet)");
                sql.AppendLine(" 		FOR StockType IN ([01], [20], [22], [21], [12], [128], [A01])");
                sql.AppendLine(" 	) p");
                sql.AppendLine(" )");
                #endregion


                /** 預計領:TA013,確認碼 <> 'V', TA011,狀態碼 = 1,2,3 **/
                #region --預計領--
                sql.AppendLine(" , Tbl_PreGet AS (");
                sql.AppendLine(" 	SELECT p.ModelNo");
                sql.AppendLine(" 	 , p.[01] AS PreGet_01, p.[20] AS PreGet_20, p.[22] AS PreGet_22, p.[21] AS PreGet_21");
                sql.AppendLine("     , p.[12] AS PreGet_12, p.[128] AS PreGet_128");
                sql.AppendLine("     , p.[A01] AS PreGet_A01");
                sql.AppendLine(" 	FROM (");
                sql.AppendLine(" 		SELECT SUM(TB004 - TB005) AS PreGet");
                sql.AppendLine(" 		 , RTRIM(TA006) AS ModelNo");
                sql.AppendLine(" 		 , TA020 AS StockType");
                sql.AppendLine(" 		FROM [prokit2].dbo.MOCTA AS A WITH(NOLOCK)");
                sql.AppendLine(" 		 INNER JOIN [prokit2].dbo.MOCTB AS B WITH(NOLOCK) ON A.TA001 = B.TB001 AND A.TA002 = B.TB002 AND A.TA006 = B.TB003");
                sql.AppendLine(" 		WHERE (A.TA013 <> 'V') AND (A.TA011 IN ('1', '2', '3'))");
                sql.AppendLine(" 		 AND (A.TA020 IN ('01', '20', '22', '21', '12', '128', 'A01'))");
                sql.AppendLine(" 		GROUP BY TA006, TA020");
                sql.AppendLine(" 	) t");
                sql.AppendLine(" 	PIVOT (");
                sql.AppendLine(" 		SUM(PreGet)");
                sql.AppendLine(" 		FOR StockType IN ([01], [20], [22], [21], [12], [128], [A01])");
                sql.AppendLine(" 	) p");
                sql.AppendLine(" )");
                #endregion


                #region --庫存--
                sql.AppendLine(" , Tbl_Stock AS (");
                sql.AppendLine("	SELECT p.ModelNo");
                sql.AppendLine("	 , p.[01] AS StockQty_01, p.[20] AS StockQty_20, p.[22] AS StockQty_22, p.[21] AS StockQty_21");
                sql.AppendLine("     , p.[12] AS StockQty_12, p.[128] AS StockQty_128");
                sql.AppendLine("     , p.[A01] AS StockQty_A01");
                sql.AppendLine("	FROM (");
                sql.AppendLine("		SELECT MC007 AS StockQty, RTRIM(MC001) AS ModelNo, MC002 AS StockType");
                sql.AppendLine("		FROM [{0}].dbo.INVMC WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("		WHERE (MC002 IN ('01', '20', '22', '21', '12', '128', 'A01'))");
                sql.AppendLine("	) t ");
                sql.AppendLine("	PIVOT (");
                sql.AppendLine("		SUM(StockQty)");
                sql.AppendLine("		FOR StockType IN ([01], [20], [22], [21], [12], [128], [A01])");
                sql.AppendLine("	) p");
                sql.AppendLine(" ) ");
                #endregion


                /** 開始取資料 **/
                sql.AppendLine("SELECT Tbl.* FROM (");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(COPTC.TC004) AS CustID");
                sql.AppendLine("  , RTRIM(COPMA.MA002) AS CustName");
                sql.AppendLine("  , COPTD.TD001 AS BO_Fid, RTRIM(COPTD.TD002) AS BO_Sid, COPTD.TD003 AS Sno, RTRIM(COPTD.TD004) AS ModelNo");
                sql.AppendLine("  , TD021 AS CfmCode, COPTD.TD007 AS StockType, CAST(COPTD.TD008 AS INT) AS BuyCnt");
                /* TW */
                sql.AppendLine("  , ISNULL(CAST(sell.PreSell_01 AS INT), 0) PreSell_01, ISNULL(CAST(sell.PreSell_20 AS INT), 0) PreSell_20, ISNULL(CAST(sell.PreSell_22 AS INT), 0) PreSell_22, ISNULL(CAST(sell.PreSell_21 AS INT), 0) PreSell_21");
                sql.AppendLine("  , ISNULL(CAST(buy.PreIN_01 AS INT), 0) PreIN_01, ISNULL(CAST(buy.PreIN_20 AS INT), 0) PreIN_20, ISNULL(CAST(buy.PreIN_22 AS INT), 0) PreIN_22, ISNULL(CAST(buy.PreIN_21 AS INT), 0) PreIN_21");
                sql.AppendLine("  , ISNULL(CAST(stock.StockQty_01 AS INT), 0) StockQty_01, ISNULL(CAST(stock.StockQty_20 AS INT), 0) StockQty_20, ISNULL(CAST(stock.StockQty_22 AS INT), 0) StockQty_22, ISNULL(CAST(stock.StockQty_21 AS INT), 0) StockQty_21");
                sql.AppendLine("  , ISNULL(CAST(itemSet.PreSet_01 AS INT), 0) PreSet_01, ISNULL(CAST(itemSet.PreSet_20 AS INT), 0) PreSet_20, ISNULL(CAST(itemSet.PreSet_22 AS INT), 0) PreSet_22, ISNULL(CAST(itemSet.PreSet_21 AS INT), 0) PreSet_21");
                sql.AppendLine("  , ISNULL(CAST(itemGet.PreGet_01 AS INT), 0) PreGet_01, ISNULL(CAST(itemGet.PreGet_20 AS INT), 0) PreGet_20, ISNULL(CAST(itemGet.PreGet_22 AS INT), 0) PreGet_22, ISNULL(CAST(itemGet.PreGet_21 AS INT), 0) PreGet_21");
                /* SH */
                sql.AppendLine("  , ISNULL(CAST(sell.PreSell_12 AS INT), 0) PreSell_12, ISNULL(CAST(sell.PreSell_128 AS INT), 0) PreSell_128");
                sql.AppendLine("  , ISNULL(CAST(buy.PreIN_12 AS INT), 0) PreIN_12, ISNULL(CAST(buy.PreIN_128 AS INT), 0) PreIN_128");
                sql.AppendLine("  , ISNULL(CAST(stock.StockQty_12 AS INT), 0) StockQty_12, ISNULL(CAST(stock.StockQty_128 AS INT), 0) StockQty_128");
                sql.AppendLine("  , ISNULL(CAST(itemSet.PreSet_12 AS INT), 0) PreSet_12, ISNULL(CAST(itemSet.PreSet_128 AS INT), 0) PreSet_128");
                sql.AppendLine("  , ISNULL(CAST(itemGet.PreGet_12 AS INT), 0) PreGet_12, ISNULL(CAST(itemGet.PreGet_128 AS INT), 0) PreGet_128");
                /* SZ */
                sql.AppendLine("  , ISNULL(CAST(sell.PreSell_A01 AS INT), 0) PreSell_A01");
                sql.AppendLine("  , ISNULL(CAST(buy.PreIN_A01 AS INT), 0) PreIN_A01");
                sql.AppendLine("  , ISNULL(CAST(stock.StockQty_A01 AS INT), 0) StockQty_A01");
                sql.AppendLine("  , ISNULL(CAST(itemSet.PreSet_A01 AS INT), 0) PreSet_A01");
                sql.AppendLine("  , ISNULL(CAST(itemGet.PreGet_A01 AS INT), 0) PreGet_A01");

                sql.AppendLine(" FROM [{0}].dbo.COPTC WITH (NOLOCK)".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPTD WITH (NOLOCK) ON COPTC.TC001 = COPTD.TD001 AND COPTC.TC002 = COPTD.TD002".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPMA WITH (NOLOCK) ON COPTC.TC004 = COPMA.MA001".FormatThis(dbName));
                sql.AppendLine("  LEFT JOIN Tbl_Stock AS stock ON COPTD.TD004 = stock.ModelNo");
                sql.AppendLine("  LEFT JOIN Tbl_PreSell AS sell ON COPTD.TD004 = sell.ModelNo");
                sql.AppendLine("  LEFT JOIN Tbl_PreIN AS buy ON COPTD.TD004 = buy.ModelNo");
                sql.AppendLine("  LEFT JOIN Tbl_PreSet AS itemSet ON COPTD.TD004 = itemSet.ModelNo");
                sql.AppendLine("  LEFT JOIN Tbl_PreGet AS itemGet ON COPTD.TD004 = itemGet.ModelNo");
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
                            case "BoNo":
                                //單號(不含-)
                                sql.Append(" AND (UPPER(COPTD.TD001 + COPTD.TD002) = UPPER(@BoNo))");

                                cmd.Parameters.AddWithValue("BoNo", item.Value);

                                break;

                            case "CustID":
                                //客戶代號
                                sql.Append(" AND (UPPER(COPMA.MA001) = UPPER(@CustID))");

                                cmd.Parameters.AddWithValue("CustID", item.Value);

                                break;

                            case "sDate":
                                //--單據日(yyyyMMdd)
                                sql.Append(" AND (COPTC.TC003 >= @sDate)");

                                cmd.Parameters.AddWithValue("sDate", item.Value.ToDateString("yyyyMMdd"));
                                break;

                            case "eDate":
                                //--單據日(yyyyMMdd)
                                sql.Append(" AND (COPTC.TC003 <= @eDate)");

                                cmd.Parameters.AddWithValue("eDate", item.Value.ToDateString("yyyyMMdd"));
                                break;

                                //case "ModelNo":
                                //    //品號
                                //    sql.Append(" AND (UPPER(Tbl.ModelNo) = UPPER(@ModelNo))");

                                //    cmd.Parameters.AddWithValue("ModelNo", item.Value);

                                //    break;

                        }
                    }
                }
                #endregion

                sql.AppendLine(" ) AS Tbl");
                sql.AppendLine(" WHERE (1=1)");

                sql.AppendLine(" ORDER BY Tbl.CustID, Tbl.BO_Fid, Tbl.BO_Sid, Tbl.Sno");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒

                return dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg);

            }

        }


        /// <summary>
        /// [訂單庫存狀況] 查詢產品庫存狀況
        /// 資料來源:PKSYS.View_ProdStockStat
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="startRow">StartRow(從0開始)</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns>DataTable</returns>
        public DataTable GetProdStockStat(Dictionary<string, string> search, string lang
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
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                //StringBuilder subSql = new StringBuilder(); //條件SQL取得
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                DataTable myDT = new DataTable();
                DataCnt = 0;    //資料總數
                string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系


                #region >> 主要資料SQL查詢 <<

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.AppendLine("SELECT TbAll.* FROM (");
                    sql.AppendLine(" SELECT *, ROW_NUMBER() OVER(ORDER BY ModelNo) AS RowIdx");
                    sql.AppendLine(" FROM View_ProdStockStat");
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
                                case "Keyword":
                                    sql.Append(" AND (");
                                    sql.Append("  (UPPER(ModelNo) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("  OR (UPPER(ModelName) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append(" )");
                                    sqlParamList.Add(new SqlParameter("@Keyword", item.Value));

                                    break;

                                case "ClassID":
                                    sql.Append(" AND (Class_ID = @Class_ID)");
                                    sqlParamList.Add(new SqlParameter("@Class_ID", item.Value));

                                    break;

                                case "ModelNo":
                                    sql.Append(" AND (UPPER(ModelNo) = UPPER(@ModelNo))");
                                    sqlParamList.Add(new SqlParameter("@ModelNo", item.Value));

                                    break;
                            }
                        }
                    }
                    #endregion
                    sql.AppendLine(") AS TbAll");
                    sql.AppendLine(" WHERE (TbAll.RowIdx >= @startRow) AND (TbAll.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TbAll.RowIdx");


                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

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
                    sqlParamList.Clear();

                    sql.AppendLine(" SELECT COUNT(TbAll.ModelNo) AS TotalCnt FROM View_ProdStockStat TbAll");
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
                                case "Keyword":
                                    sql.Append(" AND (");
                                    sql.Append("  (UPPER(TbAll.ModelNo) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("  OR (UPPER(TbAll.ModelName) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append(" )");
                                    sqlParamList.Add(new SqlParameter("@Keyword", item.Value));

                                    break;

                                case "ClassID":
                                    sql.Append(" AND (TbAll.Class_ID = @Class_ID)");
                                    sqlParamList.Add(new SqlParameter("@Class_ID", item.Value));

                                    break;

                                case "ModelNo":
                                    sql.Append(" AND (UPPER(ModelNo) = UPPER(@ModelNo))");
                                    sqlParamList.Add(new SqlParameter("@ModelNo", item.Value));

                                    break;
                            }
                        }
                    }
                    #endregion


                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 條件參數 -----
                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList.ToArray());

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

                //return
                return myDT;


            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        #endregion *** 訂單庫存狀況 E ***


        #region *** 發貨 S ***
        /// <summary>
        /// 發貨總表(ShipFreight)
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <param name="sort">排序參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipFreightItem> GetShipFreightList(string CompID, Dictionary<string, string> search, Dictionary<string, string> sort
            , out string ErrMsg)
        {
            //----- 宣告 -----
            List<ShipFreightItem> dataList = new List<ShipFreightItem>();
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                /*
                 已確認的銷貨單
                */
                sql.AppendLine(" ;WITH TblBase AS(");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid, Base.TG003 AS SO_Date, Base.TG004 AS CustID");
                sql.AppendLine("  , CAST((Base.TG045 + Base.TG046) AS money) AS TotalPrice");
                sql.AppendLine("  , (CASE WHEN DT.TH007 IN ('A01','Z11') THEN 'SH' ELSE 'SZ' END) AS StockType");
                sql.AppendLine("  , Base.TG066 AS ContactWho");
                sql.AppendLine("  , 'A' AS DataType");
                sql.AppendLine(" FROM [{0}].dbo.COPTG Base WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002".FormatThis(dbName));
                sql.AppendLine(" WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG003, Base.TG004, Base.TG066, Base.TG045, Base.TG046");
                sql.Append(", (CASE WHEN DT.TH007 IN ('A01','Z11') THEN 'SH' ELSE 'SZ' END)");

                sql.AppendLine(" UNION ALL");
                /*
                 無銷貨單的借出單
                 單別TG001 = 1302
                 確認碼TG022 = Y
                 結案碼TG024 <> Y
                */
                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid, OrdBase.TF003 AS SO_Date, OrdBase.TF005 AS CustID");
                sql.AppendLine("  , CAST(SUM(Base.TG013) AS money) AS TotalPrice");
                sql.AppendLine("  , (CASE WHEN Base.TG007 IN ('A01','Z11') THEN 'SH' ELSE 'SZ' END) AS StockType");
                sql.AppendLine("  , OrdBase.TF015 AS ContactWho");
                sql.AppendLine("  , 'B' AS DataType");
                sql.AppendLine(" FROM [{0}].dbo.INVTG Base WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002".FormatThis(dbName));
                sql.AppendLine(" WHERE (Base.TG001 = '1302') AND (Base.TG022 = 'Y') AND (Base.TG024 <> 'Y')");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG007, OrdBase.TF003, OrdBase.TF005, OrdBase.TF015");
                sql.AppendLine(" )");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  ISNULL((SELECT Parent_ID FROM [PKEF].dbo.ShipFreightRel WHERE (ShipBase.Data_ID = Rel_ID)), ShipBase.Data_ID) AS RelID");
                sql.AppendLine("  , TblBase.*");
                sql.AppendLine("  , RTRIM(Cust.MA002) AS CustName");
                sql.AppendLine("  , (CASE WHEN TblBase.StockType = 'SH' THEN '上海' ELSE '深圳' END) AS StockName");
                sql.AppendLine("  , ShipBase.Data_ID, ISNULL(ShipBase.CompID, 'SZ') CompID");
                sql.AppendLine("  , ShipBase.ShipDate, ShipBase.ShipComp, ShipBase.ShipWay");
                sql.AppendLine(" , (");
                sql.AppendLine("  CASE WHEN ISNULL(ShipBase.ShipWho, TblBase.ContactWho) = '' THEN");
                sql.AppendLine("   CASE WHEN TblBase.ContactWho = Cust.MA002 THEN Cust.MA005 ELSE TblBase.ContactWho END");
                sql.AppendLine("   ELSE ISNULL(ShipBase.ShipWho COLLATE Chinese_Taiwan_Stroke_BIN, TblBase.ContactWho) END");
                sql.AppendLine(" ) AS ShipWho");
                sql.AppendLine("  , ShipBase.Remark");
                sql.AppendLine("  , ShipDT.Data_ID AS DT_UID, ShipDT.ShipNo, ShipDT.ShipCnt, ISNULL(ShipDT.Pay1, 0) Pay1, ISNULL(ShipDT.Pay2, 0) Pay2, ISNULL(ShipDT.Pay3, 0) Pay3");
                sql.AppendLine("  , ShipComp.DisplayName AS ShipCompName");
                sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = ShipBase.Create_Who)) AS Create_Name");
                //是否已被合併
                sql.AppendLine("  , (SELECT COUNT(*) FROM [PKEF].dbo.ShipFreightRel WHERE (ShipBase.Data_ID = Rel_ID)) AS IsReled");
                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPMA Cust WITH(NOLOCK) ON TblBase.CustID = Cust.MA001".FormatThis(dbName));
                sql.AppendLine("  LEFT JOIN [PKEF].dbo.ShipFreight ShipBase ON TblBase.SO_Fid = ShipBase.ERP_So_Fid COLLATE Chinese_Taiwan_Stroke_BIN AND TblBase.SO_Sid = ShipBase.ERP_So_Sid COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("  LEFT JOIN [PKEF].dbo.ShipFreightDetail ShipDT ON ShipBase.Data_ID = ShipDT.Parent_ID");
                sql.AppendLine("  LEFT JOIN [PKEF].dbo.Logistics ShipComp ON ShipBase.ShipComp = ShipComp.Data_ID");
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
                            case "DataID":
                                sql.Append(" AND (ShipBase.Data_ID = @dataID)");

                                cmd.Parameters.AddWithValue("dataID", item.Value);
                                break;

                            case "ErpNo":
                                //--ERP單號(資料新增時使用)
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(TblBase.SO_Fid) + UPPER(TblBase.SO_Sid) = UPPER(@SOID))");
                                sql.Append("  OR (UPPER(TblBase.SO_Fid) +'-'+ UPPER(TblBase.SO_Sid) = UPPER(@SOID))");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("SOID", item.Value);
                                break;

                            case "sDate":
                                //--單據日
                                sql.Append(" AND (TblBase.SO_Date >= @sDate)");

                                cmd.Parameters.AddWithValue("sDate", item.Value);
                                break;

                            case "eDate":
                                //--單據日
                                sql.Append(" AND (TblBase.SO_Date <= @eDate)");

                                cmd.Parameters.AddWithValue("eDate", item.Value);
                                break;

                            case "Way":
                                //--物流途徑
                                sql.Append(" AND (UPPER(ShipBase.ShipWay) = UPPER(@ShipWay))");

                                cmd.Parameters.AddWithValue("ShipWay", item.Value);
                                break;

                            case "Cust":
                                //--客戶ID / Name
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(TblBase.CustID) LIKE '%' + UPPER(@Cust) + '%')");
                                sql.Append("  OR (UPPER(RTRIM(Cust.MA002)) LIKE '%' + UPPER(@Cust) + '%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("Cust", item.Value);

                                break;

                            case "CustID":
                                //--客戶ID
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(TblBase.CustID) = UPPER(@CustID))");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("CustID", item.Value);

                                break;

                            case "Keyword":
                                //--單號keyword/物流單號
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(TblBase.SO_Fid) + UPPER(TblBase.SO_Sid) LIKE '%' + UPPER(@keyword) + '%')");
                                sql.Append("  OR (UPPER(TblBase.SO_Fid) +'-'+ UPPER(TblBase.SO_Sid) LIKE '%' + UPPER(@keyword) + '%')");
                                sql.Append("  OR (ShipDT.ShipNo LIKE '%'+ @keyword +'%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("keyword", item.Value);

                                break;

                            case "ShipsDate":
                                //--發貨日
                                sql.Append(" AND (ShipBase.ShipDate >= @ShipsDate)");

                                cmd.Parameters.AddWithValue("ShipsDate", item.Value);
                                break;

                            case "ShipeDate":
                                //--發貨日
                                sql.Append(" AND (ShipBase.ShipDate <= @ShipeDate)");

                                cmd.Parameters.AddWithValue("ShipeDate", item.Value);
                                break;

                            case "Rel":
                                //--未關聯的單號 & 未填寫的發貨資料
                                sql.Append(" AND (TblBase.SO_Fid+TblBase.SO_Sid NOT IN (");
                                sql.Append("  SELECT ERP_SO_Fid+ERP_SO_Sid COLLATE Chinese_Taiwan_Stroke_BIN FROM [PKEF].dbo.ShipFreightRel");
                                sql.Append(" ))");
                                sql.Append(" AND (ShipBase.ERP_SO_Fid IS NULL)");

                                break;

                            case "ShipFrom":
                                //出貨地
                                sql.Append(" AND (TblBase.StockType = @ShipFrom)");

                                cmd.Parameters.AddWithValue("ShipFrom", item.Value);
                                break;

                            case "Pay1":
                                if (item.Value.Equals("1"))
                                {
                                    sql.Append(" AND (ShipDT.Pay1 > 0)");
                                }
                                break;
                            case "Pay2":
                                if (item.Value.Equals("1"))
                                {
                                    sql.Append(" AND (ShipDT.Pay2 > 0)");
                                }
                                break;
                            case "Pay3":
                                if (item.Value.Equals("1"))
                                {
                                    sql.Append(" AND (ShipDT.Pay3 > 0)");
                                }
                                break;
                        }
                    }
                }
                #endregion


                /* Sort */
                #region >> Sorting <<

                if (sort != null)
                {
                    //過濾空值
                    var thisSort = sort.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));
                    string thisSortField = "";
                    string thisSortWay = "";

                    //查詢內容
                    foreach (var item in thisSort)
                    {
                        switch (item.Key)
                        {
                            case "Field":
                                switch (item.Value)
                                {
                                    case "A":
                                        thisSortField = "TblBase.SO_Fid + TblBase.SO_Sid";
                                        break;

                                    default:
                                        thisSortField = "ShipBase.ShipDate";
                                        break;
                                }

                                break;

                            case "Way":
                                switch (item.Value)
                                {
                                    case "A":
                                        thisSortWay = "ASC";
                                        break;

                                    default:
                                        thisSortWay = "DESC";
                                        break;
                                }

                                break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(thisSortField))
                    {
                        sql.AppendLine(" ORDER BY {0} {1}".FormatThis(thisSortField, thisSortWay));
                    }
                    else
                    {
                        //預設排序(1=RelID)
                        //sql.AppendLine(" ORDER BY 1 DESC, ShipDT.ShipNo DESC, TblBase.SO_Date DESC, TblBase.SO_Fid, TblBase.SO_Sid");
                        sql.AppendLine(" ORDER BY TblBase.SO_Date DESC, TblBase.SO_Fid, TblBase.SO_Sid");
                    }

                }
                #endregion


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 90;   //單位:秒

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ShipFreightItem
                        {
                            Data_ID = item.Field<Guid?>("Data_ID"),
                            CompID = item.Field<string>("CompID"),
                            Erp_SO_FID = item.Field<string>("SO_Fid"),
                            Erp_SO_SID = item.Field<string>("SO_Sid"),
                            Erp_SO_Date = item.Field<string>("SO_Date"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            TotalPrice = item.Field<decimal>("TotalPrice"),
                            StockType = item.Field<string>("StockType"),
                            StockName = item.Field<string>("StockName"),
                            ShipDate = item.Field<DateTime?>("ShipDate").ToString().ToDateString("yyyy/MM/dd"),
                            ShipComp = item.Field<Int32?>("ShipComp"),
                            ShipCompName = item.Field<string>("ShipCompName"),
                            ShipWay = item.Field<string>("ShipWay"),
                            ShipWho = item.Field<string>("ShipWho"),
                            ShipCnt = item.Field<Int32?>("ShipCnt"),
                            Remark = item.Field<string>("Remark"),
                            Create_Name = item.Field<string>("Create_Name"),
                            IsReled = item.Field<int>("IsReled"),
                            ShipNo = item.Field<string>("ShipNo"),
                            Pay1 = item.Field<double?>("Pay1"),
                            Pay2 = item.Field<double?>("Pay2"),
                            Pay3 = item.Field<double?>("Pay3"),
                            AvgPay1 = item.Field<double?>("Pay1") > 0 ?
                              item.Field<Int32?>("ShipCnt") >= 1
                              ? Math.Round(item.Field<double>("Pay1") / Convert.ToDouble(item.Field<Int32?>("ShipCnt")), 2, MidpointRounding.AwayFromZero)
                              : 1
                             : 0,
                            AvgPay2 = item.Field<double?>("Pay2") > 0 ?
                              item.Field<Int32?>("ShipCnt") >= 1
                              ? Math.Round(item.Field<double>("Pay2") / Convert.ToDouble(item.Field<Int32?>("ShipCnt")), 2, MidpointRounding.AwayFromZero)
                              : 1
                             : 0,
                            AvgPay3 = item.Field<double?>("Pay3") > 0 ?
                              item.Field<Int32?>("ShipCnt") >= 1
                              ? Math.Round(item.Field<double>("Pay3") / Convert.ToDouble(item.Field<Int32?>("ShipCnt")), 2, MidpointRounding.AwayFromZero)
                              : 1
                             : 0,
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
        /// 取得明細資料 - 發貨(ShipFreight)
        /// </summary>
        /// <param name="id">主編號</param>
        /// <returns></returns>
        public IQueryable<ShipFreightDetail> GetShipFreightDetail(string id)
        {
            //----- 宣告 -----
            List<ShipFreightDetail> dataList = new List<ShipFreightDetail>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Data_ID, ShipNo, ShipCnt, Pay1, Pay2, Pay3");
                sql.AppendLine(" FROM ShipFreightDetail WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Parent_ID = @ParentID)");
                sql.AppendLine(" ORDER BY ShipNo");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", id);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ShipFreightDetail
                        {
                            Data_ID = item.Field<int>("Data_ID"),
                            ShipNo = item.Field<string>("ShipNo"),
                            ShipCnt = item.Field<Int32>("ShipCnt"),
                            Pay1 = item.Field<double>("Pay1"),
                            Pay2 = item.Field<double>("Pay2"),
                            Pay3 = item.Field<double>("Pay3")
                        };

                        //將項目加入至集合
                        dataList.Add(data);

                    }
                }
            }

            //回傳集合
            return dataList.AsQueryable();
        }


        /// <summary>
        /// 取得關聯資料 - 發貨(ShipFreight)
        /// </summary>
        /// <param name="id">主編號</param>
        /// <returns></returns>
        public IQueryable<ShipFreightRel> GetShipFreightRel(string id)
        {
            //----- 宣告 -----
            List<ShipFreightRel> dataList = new List<ShipFreightRel>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Rel_ID, Data_ID, ERP_SO_Fid, ERP_SO_Sid");
                sql.AppendLine(" FROM ShipFreightRel WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Parent_ID = @ParentID)");
                sql.AppendLine(" ORDER BY ERP_SO_Fid, ERP_SO_Sid");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", id);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ShipFreightRel
                        {
                            Data_ID = item.Field<int>("Data_ID"),
                            Rel_ID = item.Field<Guid>("Rel_ID"),
                            Erp_SO_FID = item.Field<string>("ERP_SO_Fid"),
                            Erp_SO_SID = item.Field<string>("ERP_SO_Sid")
                        };

                        //將項目加入至集合
                        dataList.Add(data);

                    }
                }
            }

            //回傳集合
            return dataList.AsQueryable();
        }


        /// <summary>
        /// 發貨EMAIL傳送(ShipFreightSend)
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <param name="sort">排序參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipFreightItem> GetShipFreightSendList(string CompID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ShipFreightItem> dataList = new List<ShipFreightItem>();
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                /*
                 已確認的銷貨單
                */
                sql.AppendLine(" ;WITH TblBase AS(");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid, Base.TG003 AS SO_Date, Base.TG004 AS CustID");
                sql.AppendLine("  , CAST((Base.TG045 + Base.TG046) AS money) AS TotalPrice");
                sql.AppendLine("  , (CASE WHEN DT.TH007 IN ('A01','Z11') THEN 'SH' ELSE 'SZ' END) AS StockType");
                sql.AppendLine("  , Base.TG066 AS ContactWho");
                sql.AppendLine("  , 'A' AS DataType");
                sql.AppendLine(" FROM [{0}].dbo.COPTG Base WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002".FormatThis(dbName));
                sql.AppendLine(" WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG003, Base.TG004, Base.TG066, Base.TG045, Base.TG046");
                sql.Append(", (CASE WHEN DT.TH007 IN ('A01','Z11') THEN 'SH' ELSE 'SZ' END)");

                sql.AppendLine(" UNION ALL");
                /*
                 無銷貨單的借出單
                 單別TG001 = 1302
                 確認碼TG022 = Y
                 結案碼TG024 <> Y
                */
                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid, OrdBase.TF003 AS SO_Date, OrdBase.TF005 AS CustID");
                sql.AppendLine("  , CAST(SUM(Base.TG013) AS money) AS TotalPrice");
                sql.AppendLine("  , (CASE WHEN Base.TG007 IN ('A01','Z11') THEN 'SH' ELSE 'SZ' END) AS StockType");
                sql.AppendLine("  , OrdBase.TF015 AS ContactWho");
                sql.AppendLine("  , 'B' AS DataType");
                sql.AppendLine(" FROM [{0}].dbo.INVTG Base WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002".FormatThis(dbName));
                sql.AppendLine(" WHERE (Base.TG001 = '1302') AND (Base.TG022 = 'Y') AND (Base.TG024 <> 'Y')");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG007, OrdBase.TF003, OrdBase.TF005, OrdBase.TF015");
                sql.AppendLine(" )");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  ISNULL((SELECT Parent_ID FROM [PKEF].dbo.ShipFreightRel WHERE (ShipBase.Data_ID = Rel_ID)), ShipBase.Data_ID) AS RelID");
                sql.AppendLine("  , TblBase.*");
                sql.AppendLine("  , RTRIM(Cust.MA002) AS CustName");
                sql.AppendLine("  , (CASE WHEN TblBase.StockType = 'SH' THEN '上海' ELSE '深圳' END) AS StockName");
                sql.AppendLine("  , ShipBase.Data_ID, ISNULL(ShipBase.CompID, 'SZ') CompID");
                sql.AppendLine("  , ShipBase.ShipDate, ShipBase.ShipComp, ShipBase.ShipWay");
                sql.AppendLine("  , (CASE WHEN TblBase.ContactWho = Cust.MA002 THEN Cust.MA005 ELSE ISNULL(ShipBase.ShipWho COLLATE Chinese_Taiwan_Stroke_BIN, TblBase.ContactWho) END) AS ShipWho");
                sql.AppendLine("  , ShipBase.Remark");
                sql.AppendLine("  , ShipDT.Data_ID AS DT_UID, ShipDT.ShipNo, ShipDT.ShipCnt, ISNULL(ShipDT.Pay1, 0) Pay1, ISNULL(ShipDT.Pay2, 0) Pay2, ISNULL(ShipDT.Pay3, 0) Pay3");
                sql.AppendLine("  , ShipComp.DisplayName AS ShipCompName");
                sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = ShipBase.Create_Who)) AS Create_Name");
                //是否已被合併
                sql.AppendLine("  , (SELECT COUNT(*) FROM [PKEF].dbo.ShipFreightRel WHERE (ShipBase.Data_ID = Rel_ID)) AS IsReled");
                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPMA Cust WITH(NOLOCK) ON TblBase.CustID = Cust.MA001".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [PKEF].dbo.ShipFreight ShipBase ON TblBase.SO_Fid = ShipBase.ERP_So_Fid COLLATE Chinese_Taiwan_Stroke_BIN AND TblBase.SO_Sid = ShipBase.ERP_So_Sid COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("  LEFT JOIN [PKEF].dbo.ShipFreightDetail ShipDT ON ShipBase.Data_ID = ShipDT.Parent_ID");
                sql.AppendLine("  LEFT JOIN [PKEF].dbo.Logistics ShipComp ON ShipBase.ShipComp = ShipComp.Data_ID");
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
                            case "Way":
                                //--物流途徑
                                sql.Append(" AND (UPPER(ShipBase.ShipWay) = UPPER(@ShipWay))");

                                cmd.Parameters.AddWithValue("ShipWay", item.Value);
                                break;

                            case "ShipsDate":
                                //--發貨日
                                sql.Append(" AND (ShipBase.ShipDate >= @ShipsDate)");

                                cmd.Parameters.AddWithValue("ShipsDate", item.Value);
                                break;

                            case "ShipeDate":
                                //--發貨日
                                sql.Append(" AND (ShipBase.ShipDate <= @ShipeDate)");

                                cmd.Parameters.AddWithValue("ShipeDate", item.Value);
                                break;

                            case "ShipFrom":
                                //出貨地
                                sql.Append(" AND (TblBase.StockType = @ShipFrom)");

                                cmd.Parameters.AddWithValue("ShipFrom", item.Value);

                                break;

                        }
                    }
                }
                #endregion


                /* Sort */
                sql.AppendLine(" ORDER BY 1 DESC, ShipDT.ShipNo DESC, TblBase.SO_Date DESC, TblBase.SO_Fid, TblBase.SO_Sid");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 90;   //單位:秒

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ShipFreightItem
                        {
                            Data_ID = item.Field<Guid?>("Data_ID"),
                            CompID = item.Field<string>("CompID"),
                            Erp_SO_FID = item.Field<string>("SO_Fid"),
                            Erp_SO_SID = item.Field<string>("SO_Sid"),
                            Erp_SO_Date = item.Field<string>("SO_Date"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            TotalPrice = item.Field<decimal>("TotalPrice"),
                            StockType = item.Field<string>("StockType"),
                            StockName = item.Field<string>("StockName"),
                            ShipDate = item.Field<DateTime?>("ShipDate").ToString().ToDateString("yyyy/MM/dd"),
                            ShipComp = item.Field<Int32?>("ShipComp"),
                            ShipCompName = item.Field<string>("ShipCompName"),
                            ShipWay = item.Field<string>("ShipWay"),
                            ShipWho = item.Field<string>("ShipWho"),
                            ShipCnt = item.Field<Int32?>("ShipCnt"),
                            Remark = item.Field<string>("Remark"),
                            Create_Name = item.Field<string>("Create_Name"),
                            IsReled = item.Field<int>("IsReled"),
                            ShipNo = item.Field<string>("ShipNo"),
                            Pay1 = item.Field<double?>("Pay1"),
                            Pay2 = item.Field<double?>("Pay2"),
                            Pay3 = item.Field<double?>("Pay3"),
                            AvgPay1 = item.Field<double?>("Pay1") > 0 ?
                              item.Field<Int32?>("ShipCnt") >= 1
                              ? Math.Round(item.Field<double>("Pay1") / Convert.ToDouble(item.Field<Int32?>("ShipCnt")), 2, MidpointRounding.AwayFromZero)
                              : 1
                             : 0,
                            AvgPay2 = item.Field<double?>("Pay2") > 0 ?
                              item.Field<Int32?>("ShipCnt") >= 1
                              ? Math.Round(item.Field<double>("Pay2") / Convert.ToDouble(item.Field<Int32?>("ShipCnt")), 2, MidpointRounding.AwayFromZero)
                              : 1
                             : 0,
                            AvgPay3 = item.Field<double?>("Pay3") > 0 ?
                              item.Field<Int32?>("ShipCnt") >= 1
                              ? Math.Round(item.Field<double>("Pay3") / Convert.ToDouble(item.Field<Int32?>("ShipCnt")), 2, MidpointRounding.AwayFromZero)
                              : 1
                             : 0,
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
        /// 運費統計(ShipFreightStat_Y)
        /// </summary>
        /// <param name="CompID"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipStat_Year> GetShipStat_Year(string CompID, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ShipStat_Year> dataList = new List<ShipStat_Year>();
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                /*
                 已確認的銷貨單(一年內)
                */
                sql.AppendLine(" ;WITH TblBase AS(");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  CAST(SUM(DT.TH037 + DT.TH038) AS money) AS TotalPrice");
                sql.AppendLine("  , COUNT(DT.TH004) AS ItemCnt");
                sql.AppendLine("  , RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid");
                sql.AppendLine(" FROM [{0}].dbo.COPTG Base WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002".FormatThis(dbName));
                sql.AppendLine(" WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')");
                sql.AppendLine("  AND SUBSTRING(Base.TG003,1,4) BETWEEN YEAR(GETDATE()-365) and YEAR(GETDATE())");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002");

                sql.AppendLine(" UNION ALL");
                /*
                 無銷貨單的借出單(一年內)
                 單別TG001 = 1302
                 確認碼TG022 = Y
                 結案碼TG024 <> Y
                */
                sql.AppendLine(" SELECT");
                sql.AppendLine("  SUM(Base.TG013) AS TotalPrice");
                sql.AppendLine("  , COUNT(Base.TG004) AS ItemCnt");
                sql.AppendLine("  , RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid");
                sql.AppendLine(" FROM [{0}].dbo.INVTG Base WITH(NOLOCK)".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002".FormatThis(dbName));
                sql.AppendLine(" WHERE (Base.TG001 = '1302') AND (Base.TG022 = 'Y') AND (Base.TG024 <> 'Y')");
                sql.AppendLine("  AND SUBSTRING(OrdBase.TF003, 1, 4) BETWEEN YEAR(GETDATE()-365) and YEAR(GETDATE())");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002");
                sql.AppendLine(" )");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  ISNULL(YEAR(ShipBase.ShipDate), 9999) AS ShipYear");
                sql.AppendLine("  , ISNULL(MONTH(ShipBase.ShipDate), 99) AS ShipMonth");
                sql.AppendLine("  , SUM(TblBase.TotalPrice) AS TotalPrice");
                sql.AppendLine("  , SUM(ItemCnt) AS ItemCnt");
                sql.AppendLine("  , SUM(ShipDT.ShipCnt) AS ShipCnt");
                sql.AppendLine("  , SUM(ISNULL(ShipDT.Pay1, 0)) AS Pay1");
                sql.AppendLine("  , SUM(ISNULL(ShipDT.Pay2, 0)) AS Pay2");
                sql.AppendLine("  , SUM(ISNULL(ShipDT.Pay3, 0)) AS Pay3");
                sql.AppendLine("  , ISNULL(SUM(ShipDT.Pay1 + ShipDT.Pay2 + ShipDT.Pay3),0) AS AllPay");
                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN [PKEF].dbo.ShipFreight ShipBase ON TblBase.SO_Fid = ShipBase.ERP_So_Fid COLLATE Chinese_Taiwan_Stroke_BIN AND TblBase.SO_Sid = ShipBase.ERP_So_Sid COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("  INNER JOIN [PKEF].dbo.ShipFreightDetail ShipDT ON ShipBase.Data_ID = ShipDT.Parent_ID");
                sql.AppendLine(" WHERE (1=1)");
                sql.AppendLine(" GROUP BY ROLLUP(YEAR(ShipBase.ShipDate), MONTH(ShipBase.ShipDate))");

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
                        int getYear = item.Field<int>("ShipYear");
                        int getMonth = item.Field<int>("ShipMonth");
                        string sDate = "";
                        string eDate = "";
                        double totalPrice = Convert.ToDouble(item.Field<decimal>("TotalPrice"));

                        //不顯示總計
                        if (!getYear.Equals(9999))
                        {
                            //設定開始/結束日(Url使用)
                            if (!getMonth.Equals(99))
                            {
                                //月初
                                DateTime dateStart = new DateTime(getYear, getMonth, 1);
                                //月底
                                DateTime dateEnd = dateStart.AddMonths(1).AddDays(-1);

                                sDate = dateStart.ToString().ToDateString("yyyy/MM/dd");
                                eDate = dateEnd.ToString().ToDateString("yyyy/MM/dd");
                            }
                            else
                            {
                                sDate = "{0}/1/1".FormatThis(getYear);
                                eDate = "{0}/12/31".FormatThis(getYear);
                            }

                            //加入項目
                            var data = new ShipStat_Year
                            {
                                showYM = "{0}-{1}".FormatThis(getYear, getMonth.Equals(99) ? "小計" : getMonth.ToString()),
                                Month = getMonth,
                                sDate = sDate,
                                eDate = eDate,
                                TotalPrice = totalPrice,
                                ItemCnt = item.Field<Int32>("ItemCnt"),
                                ShipCnt = item.Field<Int32>("ShipCnt"),
                                Pay1 = item.Field<double>("Pay1"),
                                Pay2 = item.Field<double>("Pay2"),
                                Pay3 = item.Field<double>("Pay3"),
                                avgPercent = totalPrice > 0
                                  ? Math.Round((item.Field<double>("AllPay") / totalPrice) * 100, 2, MidpointRounding.AwayFromZero)
                                  : 0,
                            };

                            //將項目加入至集合
                            dataList.Add(data);
                        }
                    }
                }

                //回傳集合
                return dataList.AsQueryable();
            }

        }


        /// <summary>
        /// 週統計(ShipFreightStat_W)
        /// </summary>
        /// <param name="CompID"></param>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetShipStat_Week(string CompID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----

                sql.Append(" DECLARE @year INT");
                sql.Append(" SET @year = @setYear;");

                sql.Append(" WITH YearWeekRange AS");
                sql.Append(" (");
                sql.Append("  SELECT DATEDIFF(wk,0, STR(@year) + '/1/1') AS startWeekNum");
                sql.Append("   , DATEADD(wk, DATEDIFF(wk,0, STR(@year) + '/1/1'), -1) AS startDate");
                sql.Append("   , 0 AS seq");

                sql.Append(" UNION ALL");

                sql.Append("  SELECT startWeekNum");
                sql.Append("   , DATEADD(wk, startWeekNum + seq + 1, -1) AS startDate");
                sql.Append("   , seq + 1 AS seq");
                sql.Append("  FROM YearWeekRange YW");
                sql.Append("  WHERE seq < 51");
                sql.Append(" )");
                sql.Append(" , TblData AS (");
                sql.Append("  SELECT COUNT(*) AS Cnt, DATEPART(wk, ShipBase.ShipDate) AS myWeek, ShipBase.CustID");
                sql.Append("  FROM [PKEF].dbo.ShipFreight ShipBase");
                sql.Append("  WHERE (YEAR(ShipBase.ShipDate) = @year)");

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
                            case "Year":
                                //年份(必填)
                                cmd.Parameters.AddWithValue("setYear", item.Value);

                                break;

                            case "ShipFrom":
                                //出貨地
                                sql.Append(" AND (ShipBase.ShipFrom = @ShipFrom)");

                                cmd.Parameters.AddWithValue("ShipFrom", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                sql.Append("  GROUP BY DATEPART(wk, ShipBase.ShipDate), ShipBase.CustID");
                sql.Append(" )");
                // --補上該週的最後一天的日期(endDate)
                sql.Append(" SELECT * FROM (");
                sql.Append(" 	SELECT");
                sql.Append(" 	(seq+1) AS showWeek");
                sql.Append(" 	, RTRIM(Cust.MA002) AS CustName");
                sql.Append(" 	, ISNULL(TblData.Cnt, 0) AS ItemCnt");
                sql.Append(" 	FROM YearWeekRange YWR");
                sql.Append(" 	 INNER JOIN TblData ON (YWR.seq + 1) = TblData.myWeek");
                sql.Append(" 	 LEFT JOIN Customer Cust ON TblData.CustID = Cust.MA001 AND DBS = DBC   ");
                sql.Append(" ) AS P");
                sql.Append(" PIVOT");
                sql.Append(" (");
                sql.Append("  SUM(ItemCnt)");
                sql.Append("  FOR showWeek");
                sql.Append("  IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[24],[25],[26],[27],[28],[29],[30],[31],[32],[33],[34],[35],[36],[37],[38],[39],[40],[41],[42],[43],[44],[45],[46],[47],[48],[49],[50],[51],[52])");
                sql.Append(" ) AS Pvt");




                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    return DT;
                }
            }

        }


        /// <summary>
        /// 取得物流公司
        /// </summary>
        /// <param name="compID"></param>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipComp> GetShipComp(string compID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ShipComp> dataList = new List<ShipComp>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.Append(" SELECT Data_ID AS ID, DisplayName AS Label, Display, Sort");
                sql.Append(" FROM Logistics");
                sql.Append(" WHERE (CompID = UPPER(@CompID))");

                /* Search */
                #region >> filter <<

                if (search == null)
                {
                    //無條件時使用 (ex:列表頁)
                    sql.Append(" AND (Display = 'Y')");
                }
                else
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrEmpty(fld.Value));

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "ID":
                                sql.Append(" AND (Data_ID = @ID)");

                                cmd.Parameters.AddWithValue("ID", item.Value);
                                break;

                            case "Keyword":
                                sql.Append(" AND (Display = 'Y')");
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(DisplayName) LIKE '%' + UPPER(@Keyword) + '%')");
                                sql.Append(" )");

                                cmd.Parameters.AddWithValue("Keyword", item.Value);
                                break;

                            case "Show":
                                //維護頁使用

                                break;
                        }
                    }
                }
                #endregion

                sql.Append(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("CompID", compID);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ShipComp
                        {
                            ID = item.Field<Int32>("ID"),
                            Label = item.Field<string>("Label"),
                            Display = item.Field<string>("Display"),
                            Sort = item.Field<Int16>("Sort")
                        };

                        //將項目加入至集合
                        dataList.Add(data);

                    }
                }
            }

            //回傳集合
            return dataList.AsQueryable();
        }


        /// <summary>
        /// 取得物流途徑(ShipFreight)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetShipWay(string value)
        {
            switch (value)
            {
                case "A":
                    return "客戶自提";

                case "B":
                    return "自行送貨";

                default:
                    return "其它";
            }
        }


        /// <summary>
        /// 取得Excel必要欄位,用來轉入單身資料
        /// </summary>
        /// <param name="filePath">完整磁碟路徑</param>
        /// <param name="sheetName">工作表名稱</param>
        /// <param name="traceID">trace id</param>
        /// <returns></returns>
        public IQueryable<ShipImportDataDT> GetExcel_DT(string filePath, string sheetName, string traceID)
        {
            try
            {
                //----- 宣告 -----
                List<ShipImportDataDT> dataList = new List<ShipImportDataDT>();

                //[Excel] - 取得原始資料
                var excelFile = new ExcelQueryFactory(filePath);
                var queryVals = excelFile.Worksheet(sheetName);

                //宣告各內容參數
                string myShipNo = "";
                string myShipDate = "";
                int myQty = 0;
                double myPrice = 0;

                //[處理合併儲存格] - 暫存欄:ID
                string tmp_OrderID = "";

                //資料迴圈
                foreach (var val in queryVals)
                {
                    #region >> 欄位處理:單號 <<

                    //[處理合併儲存格] - 目前的單號(Key)
                    string curr_OrderID;

                    curr_OrderID = val[0].ToString();

                    //[處理合併儲存格] - 目前欄位非空值, 填入暫存值
                    if (!string.IsNullOrEmpty(curr_OrderID))
                    {
                        tmp_OrderID = curr_OrderID;
                    }

                    //[設定參數] - ID
                    myShipNo = string.IsNullOrEmpty(curr_OrderID) ? tmp_OrderID : curr_OrderID;

                    //Check null
                    if (string.IsNullOrEmpty(myShipNo))
                    {
                        break;
                    }

                    #endregion


                    #region >> 欄位處理:其他欄位 <<

                    myShipNo = val[0];
                    myShipDate = val[1];
                    myQty = Convert.ToInt32(val[2]);
                    myPrice = Convert.ToDouble(val[3]);

                    #endregion


                    //加入項目
                    var data = new ShipImportDataDT
                    {
                        ShipNo = myShipNo.Trim(),
                        ShipDate = myShipDate.Trim(),
                        Qty = myQty,
                        Freight = myPrice
                    };

                    //將項目加入至集合
                    dataList.Add(data);

                }


                //回傳集合
                return dataList.AsQueryable();
            }
            catch (Exception ex)
            {

                throw new Exception("請檢查Excel格式是否正確!!格式可參考Excel範本." + ex.Message.ToString());
            }
        }


        /// <summary>
        /// [發貨匯入] 取得匯入清單
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipImportData> GetShipImportList(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ShipImportData> dataList = new List<ShipImportData>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Base.Data_ID, Base.TraceID");
                sql.AppendLine("   , Base.erpSDate, Base.erpEDate, Base.Status, Base.Upload_File, Base.Sheet_Name");
                sql.AppendLine("   , Base.Create_Who, Base.Create_Time, Base.Update_Who, Base.Update_Time");
                sql.AppendLine("   , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Create_Who)) AS Create_Name");
                sql.AppendLine("   , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Update_Who)) AS Update_Name");
                sql.AppendLine(" FROM Ship_ImportData Base");
                sql.AppendLine(" WHERE (1 = 1) ");

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
                                sql.Append(" AND (Base.Data_ID = @dataID)");

                                cmd.Parameters.AddWithValue("dataID", item.Value);
                                break;

                            case "sDate":
                                sql.Append(" AND (Base.Create_Time >= @sDate)");

                                cmd.Parameters.AddWithValue("sDate", item.Value);
                                break;

                            case "eDate":
                                sql.Append(" AND (Base.Create_Time <= @eDate)");

                                cmd.Parameters.AddWithValue("eDate", item.Value);
                                break;

                        }
                    }
                }
                #endregion

                sql.AppendLine(" ORDER BY Base.Status, Base.Create_Time DESC");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ShipImportData
                        {
                            Data_ID = item.Field<Guid>("Data_ID"),
                            TraceID = item.Field<string>("TraceID"),
                            erpSDate = item.Field<string>("erpSDate"),
                            erpEDate = item.Field<string>("erpEDate"),
                            Status = item.Field<Int16>("Status"),
                            StatusName = GetShipStatusName(item.Field<Int16>("Status")),
                            Upload_File = item.Field<string>("Upload_File"),
                            Sheet_Name = item.Field<string>("Sheet_Name"),

                            Create_Time = item.Field<DateTime?>("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),
                            Update_Time = item.Field<DateTime?>("Update_Time").ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),

                            Create_Who = item.Field<string>("Create_Who"),
                            Update_Who = item.Field<string>("Update_Who"),
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

        }

        /// <summary>
        /// 取得狀態名稱
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        /// <remarks>
        /// 10=上傳資料待確認, 20=資料待轉入, 30=finish
        /// </remarks>
        private string GetShipStatusName(int val)
        {
            switch (val)
            {
                case 10:
                    return "上傳資料待確認";

                case 20:
                    return "資料待轉入";

                case 30:
                    return "已完成";

                default:
                    return "";
            }
        }

        #endregion *** 發貨 E ***


        #region *** 客戶返利 S ***

        /// <summary>
        /// 客戶返利統計(CustRebate) - SZ/SH
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="inputYear">年</param>
        /// <param name="inputMonth">月</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// 必填:CompID / inputYear / inputMonth(當月)
        /// SQL View:已指定資料庫
        /// </remarks>
        public IQueryable<CustRebateItem> GetCustRebateList(string CompID, string inputYear, string inputMonth
            , Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<CustRebateItem> dataList = new List<CustRebateItem>();
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);
            string _View_PaperCost = "";
            string _View_SalesOrder = "";
            string _View_SalesReback = "";

            //取得對應的View
            switch (CompID.ToUpper())
            {
                case "SZ":
                    _View_PaperCost = "SZ_PaperCost";
                    _View_SalesOrder = "SZ_SalesOrder";
                    _View_SalesReback = "SZ_SalesReback";
                    break;

                default:
                    _View_PaperCost = "SH_PaperCost";
                    _View_SalesOrder = "SH_SalesOrder";
                    _View_SalesReback = "SH_SalesReback";
                    break;

            }


            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                /* 銷貨單資料 (全年)
                   PKSYS.View (因應客戶合併計算,寫在View裡面)
                */
                sql.AppendLine(" ;WITH TblSO AS (");
                sql.AppendLine("     SELECT Base.TG001, Base.TG002, Base.TG003, Base.TG004");
                sql.AppendLine("     , Base.TH037, Base.TH038, Base.TH004, Base.TH013");
                sql.AppendLine("     FROM [PKSYS].dbo.{0} Base".FormatThis(_View_SalesOrder));
                sql.AppendLine("     WHERE (LEFT(Base.TG003, 4) = @paramYear)");
                sql.AppendLine(" )");
                /* 銷退單資料 (全年)
                   PKSYS.View (因應客戶合併計算,寫在View裡面)
                */
                sql.AppendLine(" , TblSOReback AS (");
                sql.AppendLine("     SELECT Reb.TI001, Reb.TI002, Reb.TI003, Reb.TI004, Reb.TJ033, Reb.TJ034");
                sql.AppendLine("     FROM [PKSYS].dbo.{0} Reb".FormatThis(_View_SalesReback));
                sql.AppendLine("     WHERE (LEFT(Reb.TI003, 4) = @paramYear)");
                sql.AppendLine(" )");

                /* 基本資料 */
                sql.AppendLine(" ,TblBase AS (");
                sql.AppendLine(" 	SELECT Base.Data_ID, Cust.MA001 AS CustID, Cust.MA002 AS CustName, Dept.ME002 AS DeptName");
                sql.AppendLine(" 	 , Base.DataYear, Base.RespMoney, Base.RespPercent");
                sql.AppendLine(" 	 , ISNULL(Base.FightMoney, 0) FightMoney, ISNULL(Base.FightPercent, 0) FightPercent");
                sql.AppendLine(" 	 , Base.Formula, Base.Remark");
                sql.AppendLine("     , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name");
                sql.AppendLine("     , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who)) AS Update_Name");
                sql.AppendLine(" 	FROM [PKEF].dbo.Rebate_Data Base");
                sql.AppendLine(" 	 INNER JOIN [{0}].dbo.COPMA Cust WITH(NOLOCK) ON Base.CustID COLLATE Chinese_Taiwan_Stroke_BIN = Cust.MA001".FormatThis(dbName));
                sql.AppendLine(" 	 LEFT JOIN [{0}].dbo.CMSME Dept WITH(NOLOCK) ON Cust.MA015 = Dept.ME001".FormatThis(dbName));
                sql.AppendLine(" 	WHERE (Base.DataYear = @paramYear)");
                sql.AppendLine(" )");

                /*--目前業績(整年) --[A]*/
                sql.AppendLine(" , TblCnt_Range AS (");
                sql.AppendLine(" 	SELECT ISNULL(SUM(Base.TH037) + SUM(Base.TH038) ,0) AS RangeSell, ISNULL(SUM(Base.TH037), 0) AS RangeSell_NoTax, Base.TG004 AS CustID");
                sql.AppendLine(" 	FROM TblSO Base");
                sql.AppendLine(" 	 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID");
                sql.AppendLine(" 	WHERE (DT.DataYear = @paramYear)");
                sql.AppendLine(" 	GROUP BY Base.TG004");
                sql.AppendLine(" )");

                /*--成本 (整年) --[cA]*/
                sql.AppendLine(" , TblCost_Range AS (");
                sql.AppendLine("    SELECT myCost.CustID AS CustID, SUM(myCost.PaperCost) AS Cost");
                sql.AppendLine("    FROM [PKEF].dbo.Rebate_Data DT");
                sql.AppendLine("     INNER JOIN {0} myCost ON DT.DataYear = myCost.SaleYear COLLATE Chinese_Taiwan_Stroke_BIN".FormatThis(_View_PaperCost));
                sql.AppendLine("      AND DT.CustID = myCost.CustID COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("    WHERE (DT.DataYear = @paramYear)");
                sql.AppendLine("    GROUP BY myCost.CustID");
                sql.AppendLine(" )");

                /*--2341單別 (整年) --[F]*/
                sql.AppendLine(" , TblCnt_Except AS (");
                sql.AppendLine("    SELECT ISNULL(SUM(Base.TH037) + SUM(Base.TH038) ,0) AS RangeSellExcept, ISNULL(SUM(Base.TH037), 0) AS RangeSellExcept_NoTax, Base.TG004 AS CustID");
                sql.AppendLine("    FROM TblSO Base");
                sql.AppendLine("     INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID");
                sql.AppendLine("    WHERE (Base.TG001 IN ('2341')) AND (DT.DataYear = @paramYear)");
                sql.AppendLine("    GROUP BY Base.TG004");
                sql.AppendLine(" )");

                /*--銷退(整年)--[Ab]*/
                sql.AppendLine(" , TblCnt_RangeBack AS (");
                sql.AppendLine("     SELECT ISNULL(SUM(Reb.TJ033) + SUM(Reb.TJ034), 0) AS RangeSellBack, ISNULL(SUM(Reb.TJ033), 0) AS RangeSellBack_NoTax, Reb.TI004 AS CustID");
                sql.AppendLine("     FROM TblSOReback Reb");
                sql.AppendLine("     WHERE (NOT (Reb.TI001 IN ('2405', '2406')))");
                sql.AppendLine("     GROUP BY Reb.TI004");
                sql.AppendLine(" )");

                /*--已回饋金額(W003)(整年)--[B]*/
                sql.AppendLine(" , TblCnt_Rebate AS (");
                sql.AppendLine(" 	SELECT -ISNULL(SUM(Base.TH013), 0) AS RebateMoney, -ISNULL(SUM(Base.TH037), 0) AS RebateMoney_NoTax, Base.TG004 AS CustID");
                sql.AppendLine(" 	FROM TblSO Base");
                sql.AppendLine(" 	 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID");
                sql.AppendLine(" 	WHERE (DT.DataYear = @paramYear) AND (Base.TH004 = 'W003')");
                sql.AppendLine(" 	GROUP BY Base.TG004");
                sql.AppendLine(" )");

                /*--已回饋金額(W003)(當月)--[cB]*/
                sql.AppendLine(" , TblCnt_NowRebate AS (");
                sql.AppendLine(" 	SELECT -SUM(Base.TH013) AS NowRebateMoney, Base.TG004 AS CustID");
                sql.AppendLine(" 	FROM TblSO Base");
                sql.AppendLine(" 	 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID");
                sql.AppendLine(" 	WHERE (DT.DataYear = @paramYear) AND (LEFT(Base.TG003, 6) = @paramYear+@paramMonth) AND (Base.TH004 = 'W003')");
                sql.AppendLine(" 	GROUP BY Base.TG004");
                sql.AppendLine(" )");

                /*--銷售金額(當月)--[C]*/
                sql.AppendLine(" , TblCnt_Now AS (");
                sql.AppendLine(" 	SELECT ISNULL(SUM(Base.TH037) + SUM(Base.TH038) ,0) AS NowSell, ISNULL(SUM(Base.TH037), 0) AS NowSell_NoTax, Base.TG004 AS CustID");
                sql.AppendLine(" 	FROM TblSO Base");
                sql.AppendLine(" 	 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID");
                sql.AppendLine(" 	WHERE (DT.DataYear = @paramYear) AND (LEFT(Base.TG003, 6) = @paramYear+@paramMonth)");
                sql.AppendLine(" 	GROUP BY Base.TG004");
                sql.AppendLine(" )");

                /*--2341單別 (當月) --[G]*/
                sql.AppendLine(" , TblCnt_NowExcept AS (");
                sql.AppendLine(" 	SELECT ISNULL(SUM(Base.TH037) + SUM(Base.TH038) ,0) AS NowSellExcept, ISNULL(SUM(Base.TH037), 0) AS NowSellExcept_NoTax, Base.TG004 AS CustID");
                sql.AppendLine(" 	FROM TblSO Base");
                sql.AppendLine(" 	 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID");
                sql.AppendLine(" 	WHERE (Base.TG001 IN ('2341'))");
                sql.AppendLine(" 	 AND (DT.DataYear = @paramYear) AND (LEFT(Base.TG003, 6) = @paramYear+@paramMonth)");
                sql.AppendLine(" 	GROUP BY Base.TG004");
                sql.AppendLine(" )");

                /*--銷退(當月)--[Cb]*/
                sql.AppendLine(" , TblCnt_NowBack AS (");
                sql.AppendLine("     SELECT ISNULL(SUM(Reb.TJ033) + SUM(Reb.TJ034), 0) AS NowSellBack, ISNULL(SUM(Reb.TJ033), 0) AS NowSellBack_NoTax, Reb.TI004 AS CustID");
                sql.AppendLine("     FROM TblSOReback Reb");
                sql.AppendLine("     WHERE (NOT (Reb.TI001 IN ('2405', '2406')))");
                sql.AppendLine("      AND (LEFT(Reb.TI003, 6) = @paramYear+@paramMonth)");
                sql.AppendLine("     GROUP BY Reb.TI004");
                sql.AppendLine(" )");

                sql.AppendLine(" SELECT TblBase.*");
                sql.AppendLine("  , (ISNULL(TblCnt_Range.RangeSell, 0) - ISNULL(TblCnt_RangeBack.RangeSellBack, 0)) AS CntBase_A --[A] 目前業績(含稅)");
                sql.AppendLine("  , (ISNULL(TblCnt_Range.RangeSell_NoTax, 0) - ISNULL(TblCnt_RangeBack.RangeSellBack_NoTax, 0)) AS CntBase_utA --[utA] 目前業績(未稅)");
                sql.AppendLine("  , ISNULL(TblCost_Range.Cost, 0) AS CntBase_costA  --[costA] 成本");
                sql.AppendLine("  , ISNULL(TblCnt_Rebate.RebateMoney, 0) AS CntBase_B --[B] 已回饋金額(整年)");
                sql.AppendLine("  , ISNULL(TblCnt_Rebate.RebateMoney_NoTax, 0) AS CntBase_utB --[B] 已回饋金額(整年)(未稅)");
                sql.AppendLine("  , ISNULL(TblCnt_NowRebate.NowRebateMoney, 0) AS CntBase_cB --[cB] 已回饋金額(當月)");
                sql.AppendLine("  , (ISNULL(TblCnt_Now.NowSell, 0) - ISNULL(TblCnt_NowBack.NowSellBack, 0)) AS CntBase_C --[C] 當月業績(含稅)");
                sql.AppendLine("  , (ISNULL(TblCnt_Now.NowSell_NoTax, 0) - ISNULL(TblCnt_NowBack.NowSellBack_NoTax, 0)) AS CntBase_utC --[utC] 當月業績(未稅)");
                sql.AppendLine("  , ISNULL(TblCnt_Except.RangeSellExcept, 0) AS CntBase_F --[F] 2341整年(含稅)");
                sql.AppendLine("  , ISNULL(TblCnt_NowExcept.NowSellExcept, 0) AS CntBase_G --[G] 2341當月(含稅)");
                sql.AppendLine("  , ISNULL(TblCnt_Except.RangeSellExcept_NoTax, 0) AS CntBase_utF --[F] 2341整年(未稅)");
                sql.AppendLine("  , ISNULL(TblCnt_NowExcept.NowSellExcept_NoTax, 0) AS CntBase_utG --[G] 2341當月(未稅)");
                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN TblCnt_Range ON TblBase.CustID = TblCnt_Range.CustID");
                sql.AppendLine("  LEFT JOIN TblCost_Range ON TblBase.CustID COLLATE Chinese_Taiwan_Stroke_BIN = TblCost_Range.CustID");
                sql.AppendLine("  LEFT JOIN TblCnt_Rebate ON TblBase.CustID = TblCnt_Rebate.CustID");
                sql.AppendLine("  LEFT JOIN TblCnt_NowRebate ON TblBase.CustID = TblCnt_NowRebate.CustID");
                sql.AppendLine("  LEFT JOIN TblCnt_Now ON TblBase.CustID = TblCnt_Now.CustID");
                sql.AppendLine("  LEFT JOIN TblCnt_RangeBack ON TblBase.CustID = TblCnt_RangeBack.CustID");
                sql.AppendLine("  LEFT JOIN TblCnt_NowBack ON TblBase.CustID = TblCnt_NowBack.CustID");
                sql.AppendLine("  LEFT JOIN TblCnt_Except ON TblBase.CustID = TblCnt_Except.CustID");
                sql.AppendLine("  LEFT JOIN TblCnt_NowExcept ON TblBase.CustID = TblCnt_NowExcept.CustID");
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
                            case "Cust":
                                //客戶代號
                                sql.Append(" AND (UPPER(TblBase.CustID) = UPPER(@CustID))");

                                cmd.Parameters.AddWithValue("CustID", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                sql.AppendLine(" ORDER BY TblBase.DeptName");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒
                cmd.Parameters.AddWithValue("paramYear", inputYear);
                cmd.Parameters.AddWithValue("paramMonth", ("0" + inputMonth).Right(2)); //月份補0

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        #region ** 公式計算 **
                        //取得計算參數
                        string _formula = item.Field<string>("Formula");
                        double _cntBase_A = Convert.ToDouble(item.Field<decimal>("CntBase_A"));    //--[A] 目前業績(含稅)
                        double _cntBase_utA = Convert.ToDouble(item.Field<decimal>("CntBase_utA"));    //--[utA] 目前業績(未稅)
                        double _cntBase_costA = Convert.ToDouble(item.Field<decimal>("CntBase_costA"));    //--[costA] 成本
                        double _cntBase_B = Convert.ToDouble(item.Field<decimal>("CntBase_B"));    //--[B] 已回饋金額(整年)
                        double _cntBase_utB = Convert.ToDouble(item.Field<decimal>("CntBase_utB"));    //--[utB] 已回饋金額(整年)(未稅)
                        double _cntBase_cB = Convert.ToDouble(item.Field<decimal>("CntBase_cB"));    //--[cB] 已回饋金額(當月)
                        double _cntBase_C = Convert.ToDouble(item.Field<decimal>("CntBase_C"));    //--[C] 當月業績(含稅)
                        double _cntBase_utC = Convert.ToDouble(item.Field<decimal>("CntBase_utC"));    //--[utC] 當月業績(未稅)
                        double _cntBase_D = 0;   //--[D] 與挑戰目標差額(A-g)
                        double _cntBase_E = 0;   //--[E] 與責任目標差額(A-e)
                        double _cntBase_F = Convert.ToDouble(item.Field<decimal>("CntBase_F"));    //--[F] 2341整年(含稅)
                        double _cntBase_G = Convert.ToDouble(item.Field<decimal>("CntBase_G"));    //--[G] 2341當月(含稅)
                        double _cntBase_utF = Convert.ToDouble(item.Field<decimal>("CntBase_utF"));    //--[utF] 2341整年(未稅)
                        double _cntBase_utG = Convert.ToDouble(item.Field<decimal>("CntBase_utG"));    //--[utG] 2341當月(未稅)
                        double _cnt_a = 0, _cnt_uta = 0, _cnt_b = 0, _cnt_c = 0, _cnt_utc = 0, _cnt_d = 0;
                        double _cnt_e = item.Field<double>("RespMoney");
                        double _cnt_f = item.Field<double>("RespPercent") / 100;
                        double _cnt_g = item.Field<double>("FightMoney");
                        double _cnt_h = item.Field<double>("FightPercent") / 100;
                        double _profitA = 0, _profitB = 0, _profitC = 0;  //--返利前毛利率,返利後毛利率,全返後毛利率

                        /* [計算公式]
                        ----- 公式A -----
                        (責任目標) e = RespMoney, f = RespPercent(回饋%)
                        (挑戰目標) g = FightMoney, h = FightPercent(回饋%)
                        [A](目前系統業績)
                        [B](已回饋金額)
                        [C](當月銷售金額)
                        [a](實際業績)
                          a = A + B

                        [b](最高返利金額)
                          b = (c + cB) * 0.5

                        [c](應回饋金額)
                         (a >= ISNULL(g, 0) && g>0) --> c=a*h
                         (ELSE) --> c=a*f

                        [d](剩餘回饋金額)
                          d=c-B

                        [D](與挑戰目標差額)
                          D=A-g
                        [E](與責任目標差額)
                          E=A-e
                          
                        ----- 公式B -----
                        [c](應回饋金額)
                         (a >= ISNULL(g, 0) && g>0) --> (a-e)*h + e*f
                         (ELSE) --> c=a*f

                        */

                        //[D] 與挑戰目標差額(A-g)
                        _cntBase_D = _cntBase_A - _cnt_g;
                        //[E] 與責任目標差額(A-e)
                        _cntBase_E = _cntBase_A - _cnt_e;

                        //(實際業績)-a = A + B - F
                        _cnt_a = _cntBase_A + _cntBase_B - _cntBase_F; //(tax)
                        //_cnt_uta = _cntBase_utA + _cntBase_B - _cntBase_utF; //(no tax)
                        _cnt_uta = _cntBase_utA;

                        //(最高返利金額)-b
                        _cnt_b = (_cntBase_C + _cntBase_cB) * 0.5;


                        /* 判斷選擇哪種計算公式 */
                        switch (item.Field<string>("Formula"))
                        {
                            case "B":
                                //(應回饋金額)-c
                                if (_cnt_a >= _cnt_g && _cnt_g > 0)
                                {
                                    _cnt_c = (_cnt_a - _cnt_e) * _cnt_h + (_cnt_e * _cnt_f);
                                    //未稅
                                    _cnt_utc = (_cnt_uta - _cnt_e) * _cnt_h + (_cnt_e * _cnt_f);
                                }
                                else
                                {
                                    _cnt_c = _cnt_a * _cnt_f;
                                    //未稅
                                    _cnt_utc = _cnt_uta * _cnt_f;
                                }

                                //(剩餘回饋金額)-d
                                _cnt_d = _cnt_c - _cntBase_B;

                                break;

                            default:
                                //(應回饋金額)-c
                                if (_cnt_a >= _cnt_g && _cnt_g > 0)
                                {
                                    _cnt_c = _cnt_a * _cnt_h;
                                    //未稅
                                    _cnt_utc = _cnt_uta * _cnt_h;
                                }
                                else
                                {
                                    _cnt_c = _cnt_a * _cnt_f;
                                    //未稅
                                    _cnt_utc = _cnt_uta * _cnt_f;
                                }

                                //(剩餘回饋金額)-d
                                _cnt_d = _cnt_c - _cntBase_B;

                                break;
                        }


                        //Check E > 0
                        if (_cntBase_E < 0)
                        {
                            _cnt_c = 0;
                        }


                        /* 返利前毛利率 = (未稅A - 成本)/未稅A */
                        _profitA = _cntBase_utA > 0
                            ? ((_cntBase_utA - _cntBase_costA) / _cntBase_utA) * 100
                            : 0;

                        /* 返利後毛利率 = ((未稅A - 未稅已回饋B) - 成本)/(未稅A - 未稅已回饋B) */
                        _profitB = (_cntBase_utA - _cntBase_utB) > 0
                            ? (((_cntBase_utA - _cntBase_utB) - _cntBase_costA) / (_cntBase_utA - _cntBase_utB)) * 100
                            : 0;

                        /* 全返利後毛利率 = ((未稅A - 未稅應回饋c) - 成本)/(未稅A - 未稅應回饋c) */
                        _profitC = (_cntBase_utA - _cnt_utc) > 0
                            ? (((_cntBase_utA - _cnt_utc) - _cntBase_costA) / (_cntBase_utA - _cnt_utc)) * 100
                            : 0;
                        #endregion


                        //加入項目
                        var data = new CustRebateItem
                        {
                            Data_ID = item.Field<Guid>("Data_ID"),
                            DataYear = item.Field<string>("DataYear"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            DeptName = item.Field<string>("DeptName"),
                            Formula = _formula,
                            Remark = item.Field<string>("Remark"),
                            CntBase_A = _cntBase_A,
                            CntBase_utA = _cntBase_utA,
                            CntBase_costA = _cntBase_costA,
                            CntBase_B = _cntBase_B,
                            CntBase_C = _cntBase_C,
                            CntBase_utC = _cntBase_utC,
                            CntBase_D = _cntBase_D,
                            CntBase_E = _cntBase_E,
                            CntBase_F = _cntBase_F,
                            CntBase_G = _cntBase_G,
                            CntBase_utF = _cntBase_utF,
                            CntBase_utG = _cntBase_utG,
                            Cnt_a = _cnt_a,
                            Cnt_uta = _cnt_uta,
                            Cnt_b = _cnt_b,
                            Cnt_c = _cnt_c,
                            Cnt_d = _cnt_d,
                            Cnt_e = _cnt_e,
                            Cnt_f = _cnt_f,
                            Cnt_g = _cnt_g,
                            Cnt_h = _cnt_h,
                            ProfitA = _profitA,
                            ProfitB = _profitB,
                            ProfitC = _profitC,
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

        }


        /// <summary>
        /// 客戶返利維護(CustRebate/Edit)
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CustRebateItem> GetCustRebateBase(string CompID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<CustRebateItem> dataList = new List<CustRebateItem>();
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Base.Data_ID, RTRIM(Cust.MA001) AS CustID, RTRIM(Cust.MA002) AS CustName, RTRIM(Dept.ME002) AS DeptName");
                sql.AppendLine("  , Base.DataYear, Base.RespMoney, Base.RespPercent");
                sql.AppendLine("  , ISNULL(Base.FightMoney, 0) FightMoney, ISNULL(Base.FightPercent, 0) FightPercent");
                sql.AppendLine("  , Base.Formula, Base.Remark");
                sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name");
                sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who)) AS Update_Name");
                sql.AppendLine(" FROM [PKEF].dbo.Rebate_Data Base");
                sql.AppendLine("  INNER JOIN [{0}].dbo.COPMA Cust WITH(NOLOCK) ON Base.CustID COLLATE Chinese_Taiwan_Stroke_BIN = Cust.MA001".FormatThis(dbName));
                sql.AppendLine("  INNER JOIN [{0}].dbo.CMSME Dept WITH(NOLOCK) ON Cust.MA015 = Dept.ME001".FormatThis(dbName));
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
                            case "DataID":
                                //客戶代號
                                sql.Append(" AND (UPPER(Base.Data_ID) = UPPER(@ID))");

                                cmd.Parameters.AddWithValue("ID", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new CustRebateItem
                        {
                            Data_ID = item.Field<Guid>("Data_ID"),
                            DataYear = item.Field<string>("DataYear"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            DeptName = item.Field<string>("DeptName"),
                            Formula = item.Field<string>("Formula"),
                            Remark = item.Field<string>("Remark"),
                            Cnt_e = item.Field<double>("RespMoney"),
                            Cnt_f = item.Field<double>("RespPercent"),
                            Cnt_g = item.Field<double>("FightMoney"),
                            Cnt_h = item.Field<double>("FightPercent"),
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
        }

        #endregion *** 客戶返利 E ***


        #region *** 客訴 S ***

        /// <summary>
        /// [開案中客訴] 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="CCType">客訴來源</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CCPTempItem> GetOneCCPTemp(Dictionary<string, string> search, string lang
            , int CCType, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetCCPTempList(search, lang, CCType, 0, 1, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [開案中客訴] 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="CCType">客訴來源</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CCPTempItem> GetCCPTempList(Dictionary<string, string> search, string lang
            , int CCType, int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                List<CCPTempItem> dataList = new List<CCPTempItem>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數
                string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

                //取得SQL語法
                subSql = CCP_TempListSQL(search, fieldLang);
                //取得SQL參數集合
                subParamList = CCP_TempListParams(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT COUNT(TbAll.TraceID) AS TotalCnt FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@CC_Type", CCType));

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
                    sqlParamList.Add(new SqlParameter("@CC_Type", CCType));
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
                            var data = new CCPTempItem
                            {
                                Data_ID = item.Field<Guid>("Data_ID"),
                                TraceID = item.Field<string>("TraceID"),
                                CC_Type = item.Field<Int16>("CC_Type"),
                                CC_TypeName = item.Field<string>("CC_TypeName"),
                                PlanType = item.Field<Int32>("PlanType"),
                                PlanTypeName = item.Field<string>("PlanTypeName"),
                                InvoiceIsBack = item.Field<string>("InvoiceIsBack"),
                                CustType = item.Field<Int32>("CustType"),
                                CustTypeName = item.Field<string>("CustTypeName"),
                                CustInput = item.Field<string>("CustInput"),
                                RefCustID = item.Field<string>("RefCustID"),
                                RefMallID = item.Field<Int32?>("RefMallID"),
                                RefCustName = item.Field<string>("RefCustName"),
                                RefMallName = item.Field<string>("RefMallName"),
                                FreightType = item.Field<Int32>("FreightType"),
                                FreightTypeName = item.Field<string>("FreightTypeName"),
                                FreightInput = item.Field<string>("FreightInput"),
                                BuyerName = item.Field<string>("BuyerName"),
                                BuyerPhone = item.Field<string>("BuyerPhone"),
                                BuyerAddr = item.Field<string>("BuyerAddr"),
                                Platform_ID = item.Field<string>("Platform_ID"),
                                ERP_ID = item.Field<string>("ERP_ID"),
                                FreightGetDate = item.Field<DateTime?>("FreightGetDate").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                InvoiceNumber = item.Field<string>("InvoiceNumber"),
                                InvoicePrice = item.Field<double>("InvoicePrice"),
                                ShipComp = item.Field<string>("ShipComp"),
                                ShipWho = item.Field<string>("ShipWho"),
                                ShipTel = item.Field<string>("ShipTel"),
                                ShipAddr = item.Field<string>("ShipAddr"),


                                //填寫人/時間(客服&收貨)
                                CS_Time = item.Field<DateTime?>("CS_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                CS_Who = item.Field<string>("CS_Who"),
                                CS_Name = item.Field<string>("CS_Name"),
                                Freight_Time = item.Field<DateTime?>("Freight_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Freight_Who = item.Field<string>("Freight_Who"),
                                Freight_Name = item.Field<string>("Freight_Name"),
                                Invoke_Time = item.Field<DateTime?>("Invoke_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                IsInvoke = item.Field<string>("IsInvoke"),

                                DtCnt = item.Field<Int32>("DTCnt"), //商品資料數
                                IsCS = item.Field<string>("IsCS"), //客服是否填寫
                                IsFreight = item.Field<string>("IsFreight"),    //收貨是否填寫
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
        /// [開案中客訴] 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="fieldLang">欄位語系(ex:zh_TW)</param>
        /// <returns></returns>
        /// <see cref="GetCCPTempList"/>
        private StringBuilder CCP_TempListSQL(Dictionary<string, string> search, string fieldLang)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.Data_ID, Base.TraceID");
            sql.AppendLine("  , Base.CC_Type, (RefType.Class_Name_{0}) AS CC_TypeName".FormatThis(fieldLang));
            sql.AppendLine("  , ISNULL(Base.PlanType, 0) AS PlanType, (ISNULL(RefPlanType.Class_Name_{0}, '')) AS PlanTypeName, Base.InvoiceIsBack".FormatThis(fieldLang));
            sql.AppendLine("  , ISNULL(Base.CustType, 0) AS CustType, (RefCustType.Class_Name_{0}) AS CustTypeName, Base.CustInput".FormatThis(fieldLang));
            sql.AppendLine("  , Base.RefCustID, Base.RefMallID");
            sql.AppendLine("  , (SELECT TOP 1 RTRIM(MA002) + ' (' + RTRIM(MA001) + ')' FROM PKSYS.dbo.Customer WITH(NOLOCK) WHERE (DBS = DBC) AND(MA001 = Base.RefCustID)) AS RefCustName");
            sql.AppendLine("  , (SELECT Class_Name_{0} FROM Cust_Complaint_RefMall WHERE (Class_Type = @CC_Type) AND(Class_ID = Base.RefMallID)) AS RefMallName".FormatThis(fieldLang));
            sql.AppendLine("  , Base.BuyerName, Base.BuyerPhone, Base.BuyerAddr, Base.Platform_ID, Base.ERP_ID");
            sql.AppendLine("  , ISNULL(Base.FreightType, 0) AS FreightType, (RefFreightType.Class_Name_zh_TW) AS FreightTypeName, Base.FreightInput");
            sql.AppendLine("  , Base.FreightGetDate, Base.InvoiceNumber");
            sql.AppendLine("  , ISNULL(Base.InvoicePrice, 0) AS InvoicePrice, Base.ShipComp, Base.ShipWho, Base.ShipTel, Base.ShipAddr");
            sql.AppendLine("  , Base.CS_Time, Base.CS_Who, Base.Freight_Time, Base.Freight_Who");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.CS_Who)) AS CS_Name");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Freight_Who)) AS Freight_Name");
            sql.AppendLine("  , Base.Invoke_Who, Base.Invoke_Time, Base.IsInvoke");
            sql.AppendLine("  , Base.Create_Time, Base.Update_Time");
            sql.AppendLine("  , (SELECT COUNT(*) FROM Cust_Complaint_TempDT DT WHERE (DT.Parent_ID = Base.Data_ID)) AS DTCnt");
            sql.AppendLine("  , (CASE WHEN Base.CS_Time IS NULL THEN 'N' ELSE 'Y' END) AS IsCS");
            sql.AppendLine("  , (CASE WHEN Base.Freight_Time IS NULL THEN 'N' ELSE 'Y' END) AS IsFreight");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who)) AS Update_Name");
            sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY Base.Create_Time) AS RowIdx");
            sql.AppendLine(" FROM Cust_Complaint_Temp Base");
            sql.AppendLine("  INNER JOIN Cust_Complaint_RefType RefType ON Base.CC_Type = RefType.Class_ID");
            sql.AppendLine("  LEFT JOIN Cust_Complaint_RefClass RefCustType ON Base.CustType = RefCustType.Class_ID AND RefCustType.Class_Type = 2");
            sql.AppendLine("  LEFT JOIN Cust_Complaint_RefClass RefFreightType ON Base.FreightType = RefFreightType.Class_ID AND RefFreightType.Class_Type = 3");
            sql.AppendLine("  LEFT JOIN Cust_Complaint_RefClass RefPlanType ON Base.PlanType = RefPlanType.Class_ID AND RefPlanType.Class_Type = 11");

            sql.AppendLine(" WHERE (Base.CC_Type = @CC_Type)");

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

                        case "TraceID":
                            sql.Append(" AND (Base.TraceID = @TraceID)");

                            break;

                        case "Keyword":
                            //關鍵字
                            sql.Append(" AND (");
                            sql.Append(" Base.TraceID LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR Base.FreightInput LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR Base.ShipWho LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR Base.ShipTel LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR Base.Data_ID IN (");
                            sql.Append("  SELECT Parent_ID FROM Cust_Complaint_TempDT WHERE (ModelNo LIKE '%' + UPPER(@keyword) + '%')");
                            sql.Append("   )");
                            sql.Append(")");

                            break;

                        case "Status":
                            switch (item.Value)
                            {
                                case "A":
                                    //客服單位未填寫
                                    sql.Append(" AND (Base.CS_Time IS NULL)");
                                    break;

                                case "B":
                                    //收貨單位未填寫
                                    sql.Append(" AND (Base.Freight_Time IS NULL)");
                                    break;

                                case "C":
                                    //商品資料未填寫
                                    sql.Append(" AND ((SELECT COUNT(*) FROM Cust_Complaint_TempDT DT WHERE (DT.Parent_ID = Base.Data_ID)) = 0)");
                                    break;

                                default:
                                    sql.Append(" AND (Base.CS_Time IS NOT NULL) AND (Base.Freight_Time IS NOT NULL)");
                                    sql.Append(" AND ((SELECT COUNT(*) FROM Cust_Complaint_TempDT DT WHERE (DT.Parent_ID = Base.Data_ID)) > 0)");
                                    break;
                            }

                            break;

                        case "CustType":
                            //客戶類別
                            sql.Append(" AND (Base.CustType = @CustType)");

                            break;

                        case "FreightType":
                            //收貨方式
                            sql.Append(" AND (Base.FreightType = @FreightType)");

                            break;

                        case "doSearch":
                            sql.Append(" AND (Base.IsInvoke = 'N')");
                            break;
                    }
                }
            }
            #endregion

            return sql;
        }


        /// <summary>
        /// [開案中客訴] 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetCCPTempList"/>
        private List<SqlParameter> CCP_TempListParams(Dictionary<string, string> search)
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

                        case "TraceID":
                            sqlParamList.Add(new SqlParameter("@TraceID", item.Value));

                            break;

                        case "Keyword":
                            sqlParamList.Add(new SqlParameter("@keyword", item.Value));

                            break;

                        case "CustType":
                            //客戶類別
                            sqlParamList.Add(new SqlParameter("@CustType", item.Value));

                            break;

                        case "FreightType":
                            //收貨方式
                            sqlParamList.Add(new SqlParameter("@FreightType", item.Value));

                            break;

                    }
                }
            }


            return sqlParamList;
        }



        /// <summary>
        /// [開案中客訴] 匯出
        /// </summary>
        /// <param name="CCType">客訴來源</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable Get_CCPTemp_ExportData(int CCType, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("SELECT Base.Create_Time, Base.TraceID");
                sql.AppendLine(" , DT.ModelNo, DT.Qty, DT.Remark, DT.IsWarranty");
                sql.AppendLine(" , (ISNULL(RefPlanType.Class_Name_zh_TW, '')) AS PlanTypeName");
                sql.AppendLine(" , Base.InvoiceIsBack");
                sql.AppendLine(" , (RefCustType.Class_Name_zh_TW) AS CustTypeName");
                sql.AppendLine(" , Base.BuyerName, Base.BuyerPhone, Base.BuyerAddr");
                sql.AppendLine(" , (SELECT Class_Name_zh_TW FROM Cust_Complaint_RefMall WHERE (Class_Type = @CCType) AND (Class_ID = Base.RefMallID)) AS RefMallName");
                sql.AppendLine(" , Base.Platform_ID, Base.CustInput");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE([Guid] = Base.CS_Who)) AS CS_Name");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE([Guid] = Base.Freight_Who)) AS Freight_Name");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE([Guid] = Base.Create_Who)) AS Create_Name ");
                sql.AppendLine(" , (SELECT COUNT(*) FROM Cust_Complaint_TempDT DT WHERE (DT.Parent_ID = Base.Data_ID)) AS DTCnt");
                sql.AppendLine(" , (CASE WHEN Base.CS_Time IS NULL THEN 'N' ELSE 'Y' END) AS IsCS");
                sql.AppendLine(" , (CASE WHEN Base.Freight_Time IS NULL THEN 'N' ELSE 'Y' END) AS IsFreight");
                sql.AppendLine("FROM Cust_Complaint_Temp Base");
                sql.AppendLine(" INNER JOIN Cust_Complaint_RefType RefType ON Base.CC_Type = RefType.Class_ID");
                sql.AppendLine(" LEFT JOIN Cust_Complaint_TempDT DT ON Base.Data_ID = DT.Parent_ID");
                sql.AppendLine(" LEFT JOIN Cust_Complaint_RefClass RefCustType ON Base.CustType = RefCustType.Class_ID AND RefCustType.Class_Type = 2");
                sql.AppendLine(" LEFT JOIN Cust_Complaint_RefClass RefFreightType ON Base.FreightType = RefFreightType.Class_ID AND RefFreightType.Class_Type = 3");
                sql.AppendLine(" LEFT JOIN Cust_Complaint_RefClass RefPlanType ON Base.PlanType = RefPlanType.Class_ID AND RefPlanType.Class_Type = 11");
                sql.AppendLine("WHERE (Base.CC_Type = @CCType) AND (Base.IsInvoke = 'N')");

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
                            case "TraceID":
                                sql.Append(" AND (Base.TraceID = @TraceID)");

                                cmd.Parameters.AddWithValue("TraceID", item.Value);

                                break;

                            case "Keyword":
                                //關鍵字
                                sql.Append(" AND (");
                                sql.Append(" Base.TraceID LIKE '%' + UPPER(@keyword) + '%'");
                                sql.Append(" OR Base.FreightInput LIKE '%' + UPPER(@keyword) + '%'");
                                sql.Append(" OR Base.ShipWho LIKE '%' + UPPER(@keyword) + '%'");
                                sql.Append(" OR Base.ShipTel LIKE '%' + UPPER(@keyword) + '%'");
                                sql.Append(" OR Base.Data_ID IN (");
                                sql.Append("  SELECT Parent_ID FROM Cust_Complaint_TempDT WHERE (ModelNo LIKE '%' + UPPER(@keyword) + '%')");
                                sql.Append("   )");
                                sql.Append(")");

                                cmd.Parameters.AddWithValue("keyword", item.Value);

                                break;

                            case "Status":
                                switch (item.Value)
                                {
                                    case "A":
                                        //客服單位未填寫
                                        sql.Append(" AND (Base.CS_Time IS NULL)");
                                        break;

                                    case "B":
                                        //收貨單位未填寫
                                        sql.Append(" AND (Base.Freight_Time IS NULL)");
                                        break;

                                    case "C":
                                        //商品資料未填寫
                                        sql.Append(" AND ((SELECT COUNT(*) FROM Cust_Complaint_TempDT DT WHERE (DT.Parent_ID = Base.Data_ID)) = 0)");
                                        break;

                                    default:
                                        sql.Append(" AND (Base.CS_Time IS NOT NULL) AND (Base.Freight_Time IS NOT NULL)");
                                        sql.Append(" AND ((SELECT COUNT(*) FROM Cust_Complaint_TempDT DT WHERE (DT.Parent_ID = Base.Data_ID)) > 0)");
                                        break;
                                }

                                break;

                            case "CustType":
                                //客戶類別
                                sql.Append(" AND (Base.CustType = @CustType)");

                                cmd.Parameters.AddWithValue("CustType", item.Value);

                                break;

                            case "FreightType":
                                //收貨方式
                                sql.Append(" AND (Base.FreightType = @FreightType)");

                                cmd.Parameters.AddWithValue("FreightType", item.Value);

                                break;

                        }
                    }
                }
                #endregion


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒
                cmd.Parameters.AddWithValue("CCType", CCType);

                return dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg);

            }

        }



        /// <summary>
        /// [未開案客訴] 取得收貨圖片
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <remarks>
        /// 檔名已加上ParentID的資料夾名(ex:0123xxxx+photo.jpg), 避免在客訴頁顯示時的困擾(要取Temp的Guid)
        /// 所以在顯示URL時,要注意路徑.
        /// </remarks>
        public IQueryable<CCPAttachment> GetCCPTempFileList(Dictionary<string, string> search)
        {
            //----- 宣告 -----
            List<CCPAttachment> dataList = new List<CCPAttachment>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Atta.Data_ID, CAST(Atta.Parent_ID AS VARCHAR(38)) AS FolderName");
                sql.AppendLine(" , Atta.AttachFile, Atta.AttachFile_Org");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Atta.Create_Who)) AS Create_Name");
                sql.AppendLine(" FROM Cust_Complaint_Temp Base");
                sql.AppendLine("  INNER JOIN Cust_Complaint_TempAttachment Atta ON Base.Data_ID = Atta.Parent_ID");
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
                            case "DataID":
                                sql.Append(" AND (Base.Data_ID = @Data_ID)");
                                cmd.Parameters.AddWithValue("Data_ID", item.Value);

                                break;

                            case "TraceID":
                                sql.Append(" AND (Base.TraceID = @TraceID)");
                                cmd.Parameters.AddWithValue("TraceID", item.Value);

                                break;
                        }
                    }
                }
                #endregion

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
                        var data = new CCPAttachment
                        {
                            Data_ID = item.Field<Int64>("Data_ID"),
                            FilePath = "{0}/{1}".FormatThis(item.Field<string>("FolderName"), item.Field<string>("AttachFile")),
                            AttachFile = item.Field<string>("AttachFile"),
                            AttachFile_Org = item.Field<string>("AttachFile_Org"),
                            Create_Name = item.Field<string>("Create_Name")
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
        /// [未開案客訴] 取得要開案的資料 (trigger:開案時)
        /// </summary>
        /// <param name="id">Data ID</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CCPItem> GetCCP_InvokeData(string id, out string ErrMsg)
        {
            ErrMsg = "";
            try
            {
                //----- 宣告 -----
                List<CCPItem> dataList = new List<CCPItem>(); //資料容器
                StringBuilder sql = new StringBuilder(); //SQL語法容器

                #region >> 主要資料SQL查詢 <<

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----

                    sql.AppendLine(" SELECT Base.TraceID, Base.CC_Type");
                    sql.AppendLine(" , Base.PlanType, Base.CustType, Base.RefCustID, Base.ERP_ID, Base.RefMallID, Base.Platform_ID");
                    sql.AppendLine(" , DT.ModelNo, DT.Qty, DT.Remark, DT.IsSplit, DT.IsWarranty");
                    sql.AppendLine(" FROM Cust_Complaint_Temp Base");
                    sql.AppendLine("  INNER JOIN Cust_Complaint_TempDT DT ON Base.Data_ID = DT.Parent_ID");
                    sql.AppendLine(" WHERE (Base.Data_ID = @Data_ID)");

                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    cmd.Parameters.AddWithValue("Data_ID", id);

                    //Execute
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new CCPItem
                            {
                                TraceID = item.Field<string>("TraceID"),
                                CC_Type = item.Field<Int16>("CC_Type"),
                                PlanType = item.Field<Int32>("PlanType"),
                                CustType = item.Field<Int32>("CustType"),
                                RefCustID = item.Field<string>("RefCustID"),
                                RefMallID = item.Field<Int32?>("RefMallID"),
                                RefERP_ID = item.Field<string>("ERP_ID"),
                                RefPlatform_ID = item.Field<string>("Platform_ID"),
                                ModelNo = item.Field<string>("ModelNo"),
                                Qty = item.Field<Int32>("Qty"),
                                Remark = item.Field<string>("Remark"),
                                IsSplit = item.Field<string>("IsSplit"),
                                IsWarranty = item.Field<string>("IsWarranty")
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
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// 取得Excel欄位,用來轉入資料
        /// </summary>
        /// <param name="filePath">完整磁碟路徑</param>
        /// <param name="sheetName">工作表名稱</param>
        /// <returns></returns>
        public IQueryable<CCPDetail> GetCCP_ExcelData(string filePath, string sheetName)
        {
            try
            {
                //----- 宣告 -----
                List<CCPDetail> dataList = new List<CCPDetail>();

                //[Excel] - 取得原始資料
                var excelFile = new ExcelQueryFactory(filePath);
                var queryVals = excelFile.Worksheet(sheetName);

                //宣告各內容參數
                string _ModelNo = "";
                int _Qty = 0;
                string _Remark = "";
                string _IsSplit = "";
                string _IsWarranty = "";

                //資料迴圈
                foreach (var val in queryVals)
                {
                    _ModelNo = val[0];
                    _Qty = Convert.ToInt32(val[1]);
                    _IsSplit = val[2];
                    _IsWarranty = val[3];
                    _Remark = val[4];

                    //加入項目
                    var data = new CCPDetail
                    {
                        ModelNo = _ModelNo,
                        Qty = _Qty,
                        IsSplit = _IsSplit,
                        IsWarranty = _IsWarranty,
                        Remark = _Remark
                    };

                    //將項目加入至集合
                    dataList.Add(data);

                }

                //回傳集合
                return dataList.AsQueryable();
            }
            catch (Exception ex)
            {

                throw new Exception("請檢查Excel格式是否正確!!格式可參考Excel範本." + ex.Message.ToString());
            }
        }

        //------------------ [客訴清單] ------------------
        /// <summary>
        /// [已開案客訴] 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="CCType">客訴來源</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CCPItem> GetOneCCP(Dictionary<string, string> search, string lang
            , int CCType, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetCCPList(search, lang, CCType, 0, 1, out dataCnt, out ErrMsg);
        }

        public IQueryable<CCPItem> GetCCPAllList(Dictionary<string, string> search, string lang
            , int CCType, out int DataCnt, out string ErrMsg)
        {
            return GetCCPList(search, lang, CCType, 0, 9999999, out DataCnt, out ErrMsg);
        }

        /// <summary>
        /// [已開案客訴] 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="CCType">客訴來源</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CCPItem> GetCCPList(Dictionary<string, string> search, string lang
            , int CCType, int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                List<CCPItem> dataList = new List<CCPItem>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數
                string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

                //取得SQL語法
                subSql = CCP_ListSQL(search, fieldLang);
                //取得SQL參數集合
                subParamList = CCP_ListParams(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT COUNT(TbAll.TraceID) AS TotalCnt FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@CC_Type", CCType));

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
                    sqlParamList.Add(new SqlParameter("@CC_Type", CCType));
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
                            var data = new CCPItem
                            {
                                Data_ID = item.Field<Guid>("Data_ID"),
                                SeqNo = item.Field<Int32>("SeqNo"),
                                TraceID = item.Field<string>("TraceID"),
                                CC_Type = item.Field<Int16>("CC_Type"),
                                CC_TypeName = item.Field<string>("CC_TypeName"),
                                CC_UID = item.Field<string>("CC_UID"),
                                PlanType = item.Field<Int32>("PlanType"),
                                PlanTypeName = item.Field<string>("PlanTypeName"),
                                CustType = item.Field<Int32>("CustType"),
                                CustTypeName = item.Field<string>("CustTypeName"),
                                RefCustID = item.Field<string>("RefCustID"),
                                RefMallID = item.Field<Int32?>("RefMallID"),
                                RefCustName = item.Field<string>("RefCustName"),
                                RefMallName = item.Field<string>("RefMallName"),
                                ModelNo = item.Field<string>("ModelNo"),
                                Qty = item.Field<Int32>("Qty"),
                                Remark = item.Field<string>("Remark"),
                                Remark_Check = item.Field<string>("Remark_Check") ?? "",
                                IsWarranty = item.Field<string>("IsWarranty"),
                                FlowStatus = item.Field<int>("FlowStatus"),
                                FlowStatusName = item.Field<string>("FlowStatusName"),

                                //Flow Data
                                Flow201_Type = item.Field<Int32?>("Flow201_Type"),
                                Flow201_Desc = item.Field<string>("Flow201_Desc"),
                                Flow201_Time = item.Field<DateTime?>("Flow201_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Flow201_WhoName = item.Field<string>("Flow201_WhoName"),
                                Flow201_TypeName = item.Field<string>("Flow201_TypeName"),
                                Flow301_Type = item.Field<Int32?>("Flow301_Type"),
                                Flow301_Desc = item.Field<string>("Flow301_Desc"),
                                Flow301_Time = item.Field<DateTime?>("Flow301_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Flow301_WhoName = item.Field<string>("Flow301_WhoName"),
                                Flow301_TypeName = item.Field<string>("Flow301_TypeName"),
                                Flow401_Type = item.Field<Int32?>("Flow401_Type"),
                                Flow401_Desc = item.Field<string>("Flow401_Desc"),
                                Flow401_Time = item.Field<DateTime?>("Flow401_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Flow401_WhoName = item.Field<string>("Flow401_WhoName"),
                                Flow401_TypeName = item.Field<string>("Flow401_TypeName"),
                                Flow501_Type = item.Field<Int32?>("Flow501_Type"),
                                Flow501_Desc = item.Field<string>("Flow501_Desc"),
                                Flow501_Time = item.Field<DateTime?>("Flow501_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Flow501_WhoName = item.Field<string>("Flow501_WhoName"),
                                Flow501_TypeName = item.Field<string>("Flow501_TypeName"),
                                FixPrice = item.Field<double?>("FixPrice"),
                                FixWishDate = item.Field<DateTime?>("FixWishDate").ToString().ToDateString("yyyy/MM/dd"),  //維修預計完成日
                                FixOkDate = item.Field<DateTime?>("FixOkDate").ToString().ToDateString("yyyy/MM/dd"),  //維修完成日
                                ERP_No1 = item.Field<string>("ERP_No1"),  //客訴銷單號
                                ERP_No2 = item.Field<string>("ERP_No2"),  //借出單號
                                ERP_No3 = item.Field<string>("ERP_No3"),  //歸還單號
                                ERP_No4 = item.Field<string>("ERP_No4"),  //銷退單號
                                ERP_No5 = item.Field<string>("ERP_No5"),  //維修費訂單
                                ERP_No6 = item.Field<string>("ERP_No6"),  //維修費銷貨單

                                FixTotalPrice = item.Field<double?>("FixTotalPrice"),   //維修費
                                ShipComp = item.Field<string>("ShipComp"),
                                ShipNo = item.Field<string>("ShipNo"),  //物流單號
                                ShipDate = item.Field<DateTime?>("ShipDate").ToString().ToDateString("yyyy/MM/dd"),    //寄貨日
                                Finish_Time = item.Field<DateTime?>("Finish_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Finish_WhoName = item.Field<string>("Finish_WhoName"),
                                Finish_Remark = item.Field<string>("Finish_Remark"),
                                Create_Time = item.Field<DateTime?>("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Update_Time = item.Field<DateTime?>("Update_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Create_Name = item.Field<string>("Create_Name"),
                                Update_Name = item.Field<string>("Update_Name"),
                                unOpenCnt = item.Field<Int32>("unOpenCnt")
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
        /// [已開案客訴] 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="fieldLang">欄位語系(ex:zh_TW)</param>
        /// <returns></returns>
        /// <see cref="GetCCPTempList"/>
        private StringBuilder CCP_ListSQL(Dictionary<string, string> search, string fieldLang)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.Data_ID, Base.SeqNo, Base.TraceID, Base.CC_UID");
            sql.AppendLine("  , Base.CC_Type, (RefType.Class_Name_{0}) AS CC_TypeName".FormatThis(fieldLang));
            sql.AppendLine("  , ISNULL(Base.PlanType, 0) AS PlanType, (RefPlanType.Class_Name_{0}) AS PlanTypeName".FormatThis(fieldLang));
            sql.AppendLine("  , ISNULL(Base.CustType, 0) AS CustType, (RefCustType.Class_Name_{0}) AS CustTypeName".FormatThis(fieldLang));
            sql.AppendLine("  , Base.RefCustID, Base.RefMallID");
            sql.AppendLine("  , (SELECT TOP 1 RTRIM(MA002) + ' (' + RTRIM(MA001) + ')' FROM PKSYS.dbo.Customer WITH(NOLOCK) WHERE (DBS = DBC) AND (MA001 = Base.RefCustID)) AS RefCustName");
            sql.AppendLine("  , (SELECT Class_Name_{0} FROM Cust_Complaint_RefMall WHERE (Class_Type = @CC_Type) AND (Class_ID = Base.RefMallID)) AS RefMallName".FormatThis(fieldLang));
            sql.AppendLine("  , Base.ModelNo, Base.Qty, Base.Remark, Base.Remark_Check, Base.IsWarranty");
            sql.AppendLine("  , Base.FlowStatus, RefFlow.Class_Name_{0} AS FlowStatusName".FormatThis(fieldLang));
            sql.AppendLine("  , Base.Flow201_Type, Base.Flow201_Desc, Base.Flow201_Time, Base.Flow201_Who");
            sql.AppendLine("  , (SELECT Rel.Class_Name_zh_TW FROM Cust_Complaint_RefClass Rel WHERE Rel.Class_ID = Base.Flow201_Type AND Rel.CC_Type = @CC_Type) AS Flow201_TypeName");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Flow201_Who)) AS Flow201_WhoName");
            sql.AppendLine("  , Base.Flow301_Type, Base.Flow301_Desc, Base.Flow301_Time, Base.Flow301_Who");
            sql.AppendLine("  , (SELECT Rel.Class_Name_zh_TW FROM Cust_Complaint_RefClass Rel WHERE Rel.Class_ID = Base.Flow301_Type AND Rel.CC_Type = @CC_Type) AS Flow301_TypeName");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Flow301_Who)) AS Flow301_WhoName");
            sql.AppendLine("  , Base.Flow401_Type, Base.Flow401_Desc, Base.Flow401_Time, Base.Flow401_Who");
            sql.AppendLine("  , (SELECT Rel.Class_Name_zh_TW FROM Cust_Complaint_RefClass Rel WHERE Rel.Class_ID = Base.Flow401_Type AND Rel.CC_Type = @CC_Type) AS Flow401_TypeName");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Flow401_Who)) AS Flow401_WhoName");
            sql.AppendLine("  , Base.Flow501_Type, Base.Flow501_Desc, Base.Flow501_Time, Base.Flow501_Who");
            sql.AppendLine("  , (SELECT Rel.Class_Name_zh_TW FROM Cust_Complaint_RefClass Rel WHERE Rel.Class_ID = Base.Flow501_Type AND Rel.CC_Type = @CC_Type) AS Flow501_TypeName");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Flow501_Who)) AS Flow501_WhoName");
            sql.AppendLine("  , ISNULL(Base.FixPrice, 0) FixPrice, Base.FixWishDate, Base.FixOkDate, ISNULL(Base.FixTotalPrice, 0) FixTotalPrice");
            sql.AppendLine("  , Base.ERP_No1, Base.ERP_No2, Base.ERP_No3, Base.ERP_No4, Base.ERP_No5, Base.ERP_No6");
            sql.AppendLine("  , Base.ShipComp, Base.ShipNo, Base.ShipDate, Base.Finish_Time, Base.Finish_Who, Base.Finish_Remark");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Finish_Who)) AS Finish_WhoName");
            sql.AppendLine("  , Base.Create_Time, Base.Update_Time");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who)) AS Update_Name");
            sql.AppendLine("  , (SELECT COUNT(*) FROM Cust_Complaint_Temp WHERE (CC_Type = Base.CC_Type) AND (IsInvoke = 'N')) AS unOpenCnt");
            sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY Base.FlowStatus, Base.Create_Time DESC) AS RowIdx");
            sql.AppendLine(" FROM Cust_Complaint Base");
            sql.AppendLine("  INNER JOIN Cust_Complaint_RefType RefType ON Base.CC_Type = RefType.Class_ID");
            sql.AppendLine("  LEFT JOIN Cust_Complaint_RefClass RefCustType ON Base.CustType = RefCustType.Class_ID AND RefCustType.Class_Type = 2");
            sql.AppendLine("  LEFT JOIN Cust_Complaint_RefClass RefFlow ON Base.FlowStatus = RefFlow.Class_ID AND RefFlow.Class_Type = 1");
            sql.AppendLine("  LEFT JOIN Cust_Complaint_RefClass RefPlanType ON Base.PlanType = RefPlanType.Class_ID AND RefPlanType.Class_Type = 11");
            sql.AppendLine(" WHERE (Base.CC_Type = @CC_Type)");

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
                            sql.Append(" Base.CC_UID LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR Base.ModelNo LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR Base.ShipNo LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR Base.RefCustID LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR Base.RefCustID IN (");
                            sql.Append(" (SELECT RTRIM(MA001) FROM PKSYS.dbo.Customer WITH(NOLOCK) WHERE (DBS = DBC) AND (MA002 LIKE '%' + UPPER(@keyword) + '%'))");
                            sql.Append(" )");
                            sql.Append(")");

                            break;

                        //--開案時間
                        case "sDateA":
                            sql.Append(" AND (Base.Create_Time >= @sDate)");
                            break;
                        case "eDateA":
                            sql.Append(" AND (Base.Create_Time <= @eDate)");
                            break;

                        //--結案時間
                        case "sDateB":
                            sql.Append(" AND (Base.Finish_Time >= @sDate)");
                            break;
                        case "eDateB":
                            sql.Append(" AND (Base.Finish_Time <= @eDate)");
                            break;

                        case "CustType":
                            //客戶類別
                            sql.Append(" AND (Base.CustType = @CustType)");

                            break;

                        case "PlanType":
                            //客戶類別
                            sql.Append(" AND (Base.PlanType = @PlanType)");

                            break;

                        case "FlowStatus":
                            //流程狀態
                            sql.Append(" AND (Base.FlowStatus = @FlowStatus)");

                            break;

                        case "TraceID":
                            //追蹤編號
                            sql.Append(" AND (Base.TraceID = @TraceID)");

                            break;

                        case "Range_CCUID":
                            //客訴編號集合(批量回覆使用)

                            string[] aryID = Regex.Split(item.Value, ",");
                            ArrayList aryLst = new ArrayList(aryID);

                            //GetSQLParam:SQL WHERE IN的方法
                            sql.AppendLine(" AND (Base.CC_UID IN ({0}))".FormatThis(CustomExtension.GetSQLParam(aryLst, "params")));

                            break;

                        case "Flow301_Export":
                            //二線匯出
                            sql.Append(" AND (ISNULL(Base.Flow301_Desc, '') <> '')");

                            break;
                    }
                }
            }
            #endregion

            return sql;
        }


        /// <summary>
        /// [已開案客訴] 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetCCPTempList"/>
        private List<SqlParameter> CCP_ListParams(Dictionary<string, string> search)
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

                        //--結案時間
                        case "sDateB":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value + " 00:00:00"));
                            break;
                        case "eDateB":
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value + " 23:59:59"));
                            break;

                        case "CustType":
                            //客戶類別
                            sqlParamList.Add(new SqlParameter("@CustType", item.Value));

                            break;

                        case "PlanType":
                            //計畫處理方式
                            sqlParamList.Add(new SqlParameter("@PlanType", item.Value));

                            break;

                        case "FlowStatus":
                            //流程狀態
                            sqlParamList.Add(new SqlParameter("@FlowStatus", item.Value));

                            break;

                        case "TraceID":
                            //追蹤編號
                            sqlParamList.Add(new SqlParameter("@TraceID", item.Value));

                            break;

                        case "Range_CCUID":
                            //客訴編號集合(批量回覆使用)

                            string[] aryID = Regex.Split(item.Value, ",");
                            for (int row = 0; row < aryID.Count(); row++)
                            {
                                sqlParamList.Add(new SqlParameter("@params" + row, aryID[row]));
                            }

                            break;
                    }
                }
            }


            return sqlParamList;
        }
        //------------------ [客訴清單] ------------------


        /// <summary>
        /// [已開案客訴] 取得檔案附件
        /// </summary>
        /// <param name="parentID">所屬資料編號</param>
        /// <param name="lang">語系</param>
        /// <returns></returns>
        public IQueryable<CCPAttachment> GetCCPFileList(string parentID, string lang)
        {
            //----- 宣告 -----
            List<CCPAttachment> dataList = new List<CCPAttachment>();
            StringBuilder sql = new StringBuilder();
            string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Base.Data_ID");
                sql.AppendLine(" , Base.FlowID, Cls.Class_Name_{0} AS FlowName".FormatThis(fieldLang));
                sql.AppendLine(" , Base.AttachFile, Base.AttachFile_Org");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name");
                sql.AppendLine(" FROM Cust_Complaint_Attachment Base");
                sql.AppendLine("  INNER JOIN Cust_Complaint_RefClass Cls ON Base.FlowID = Cls.Class_ID");
                sql.AppendLine(" WHERE (Base.Parent_ID = @Parent_ID)");

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
                        var data = new CCPAttachment
                        {
                            Data_ID = item.Field<Int64>("Data_ID"),
                            FlowID = item.Field<int>("FlowID"),
                            FlowName = item.Field<string>("FlowName"),
                            AttachFile = item.Field<string>("AttachFile"),
                            AttachFile_Org = item.Field<string>("AttachFile_Org"),
                            Create_Name = item.Field<string>("Create_Name")
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
        /// [已開案客訴] 取得Log
        /// </summary>
        /// <param name="parentID">所屬資料編號</param>
        /// <param name="logType">1:Userflow 2:System log</param>
        /// <returns></returns>
        public IQueryable<CCPLog> GetCCPLogList(string parentID, Int16 logType)
        {
            //----- 宣告 -----
            List<CCPLog> dataList = new List<CCPLog>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Base.Data_ID");
                sql.AppendLine(" , Base.LogSubject, Base.LogDesc, Base.Create_Time, Base.FlowID");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name");
                sql.AppendLine(" FROM Cust_Complaint_Log Base");
                sql.AppendLine(" WHERE (Base.Parent_ID = @Parent_ID) AND (Base.LogType = @LogType)");
                sql.AppendLine(" ORDER BY Base.Create_Time");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", parentID);
                cmd.Parameters.AddWithValue("LogType", logType);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new CCPLog
                        {
                            LogSubject = item.Field<string>("LogSubject"),
                            LogDesc = item.Field<string>("LogDesc"),
                            FlowID = item.Field<Int32>("FlowID"),
                            Create_Name = item.Field<string>("Create_Name"),
                            Create_Time = item.Field<DateTime>("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm")
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
        /// [已開案客訴] 通知設定
        /// </summary>
        /// <param name="search"></param>
        /// <param name="CCType">客訴來源</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        ///  Class_Type=1:FlowStatus
        /// </remarks>
        public IQueryable<CCPInform> GetCCPInformCfgList(Dictionary<string, string> search, int CCType, out string ErrMsg)
        {
            //----- 宣告 -----
            List<CCPInform> dataList = new List<CCPInform>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Inform.Data_ID, Inform.FlowID, RefFlow.Class_Name_zh_TW FlowName");
                sql.AppendLine(" , Inform.Who, Inform.Email, Inform.CC_Type");
                sql.AppendLine(" , Prof.Display_Name, Prof.Account_Name, Prof.NickName");
                sql.AppendLine(" FROM Cust_Complaint_InformFlow Inform");
                sql.AppendLine("  INNER JOIN PKSYS.dbo.User_Profile Prof ON Inform.Who = Prof.[Guid]");
                sql.AppendLine("  INNER JOIN Cust_Complaint_RefClass RefFlow ON Inform.FlowID = RefFlow.Class_ID AND RefFlow.Class_Type = 1");
                sql.AppendLine(" WHERE (Inform.CC_Type = @CC_Type)");

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
                            case "FlowID":
                                sql.Append(" AND (Inform.FlowID = @FlowID)");

                                cmd.Parameters.AddWithValue("FlowID", item.Value);

                                break;


                            case "Who":
                                sql.Append(" AND (UPPER(Inform.Who) = UPPER(@Who))");

                                cmd.Parameters.AddWithValue("Who", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("CC_Type", CCType);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new CCPInform
                        {
                            Data_ID = item.Field<Int32>("Data_ID"),
                            FlowID = item.Field<Int32>("FlowID"),
                            FlowName = item.Field<string>("FlowName"),
                            Who = item.Field<string>("Who"),
                            Email = item.Field<string>("Email"),
                            DisplayName = "{0} ({1})".FormatThis(item.Field<string>("Account_Name"), item.Field<string>("Display_Name"))
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
        /// 取得參考分類
        /// </summary>
        /// <param name="type">資料type(詳見備註)</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="CCType">客訴來源</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// [資料type]
        /// 1:FlowStatus, 2:客戶類別, 3:收貨方式, 4:一線處理方式, 5:二線處理方式, 6:業務確認處理方式, 7:資材處理方式
        /// </remarks>
        public IQueryable<ClassItem> GetCCP_RefClass(string type, string lang, int CCType, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name_{0} AS Label, ISNULL(Invoke_To, 0) AS Invoke_To"
                    .FormatThis(fn_Language.Get_LangCode(lang).Replace("-", "_")));
                sql.AppendLine(" FROM Cust_Complaint_RefClass WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Display = 'Y')");

                //指定ClassType(流程相關),需要判斷客訴來源
                switch (type)
                {
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                        sql.Append(" AND (CC_Type = @CC_Type)");
                        cmd.Parameters.AddWithValue("CC_Type", CCType);

                        break;

                    default:
                        break;
                }

                //資料類型(可為空)
                if (!string.IsNullOrWhiteSpace(type))
                {
                    sql.Append(" AND (Class_Type = @type)");
                    cmd.Parameters.AddWithValue("type", Convert.ToInt16(type));
                }

                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ClassItem
                        {
                            ID = item.Field<int>("ID"),
                            Label = item.Field<string>("Label"),
                            Invoke_To = item.Field<int>("Invoke_To")
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
        /// 取得指定對應類別 (Cust_Complaint_RefClass)
        /// </summary>
        /// <param name="id">指定ID</param>
        /// <param name="lang">Lang</param>
        /// <param name="CCType">客訴來源</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetOneCCP_RefClass(string id, string lang, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name_{0} AS Label, ISNULL(Invoke_To, 0) AS Invoke_To"
                    .FormatThis(fn_Language.Get_LangCode(lang).Replace("-", "_")));
                sql.AppendLine(" FROM Cust_Complaint_RefClass WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Class_ID = @id)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("id", Convert.ToInt32(id));

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ClassItem
                        {
                            ID = item.Field<int>("ID"),
                            Label = item.Field<string>("Label"),
                            Invoke_To = item.Field<int>("Invoke_To")
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
        /// 取得客訴來源
        /// </summary>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetCCP_RefType(string lang, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name_{0} AS Label, RefMenuID"
                    .FormatThis(fn_Language.Get_LangCode(lang).Replace("-", "_")));
                sql.AppendLine(" FROM Cust_Complaint_RefType WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Display = 'Y')");
                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ClassItem
                        {
                            ID = item.Field<int>("ID"),
                            Label = item.Field<string>("Label"),
                            MenuID = item.Field<Int32>("RefMenuID")
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
        /// 取得商城 - 依客訴來源區分
        /// </summary>
        /// <param name="type">客訴來源</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetCCP_RefMall(string type, string lang, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name_{0} AS Label"
                    .FormatThis(fn_Language.Get_LangCode(lang).Replace("-", "_")));
                sql.AppendLine(" FROM Cust_Complaint_RefMall WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Class_Type = @type) AND (Display = 'Y')");
                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("type", Convert.ToInt32(type));

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ClassItem
                        {
                            ID = item.Field<int>("ID"),
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
        /// [開案中客訴] 取得商品資料
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CCPDetail> GetCCP_Detail(string parentID, out string ErrMsg)
        {
            //----- 宣告 -----
            List<CCPDetail> dataList = new List<CCPDetail>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Data_ID, ModelNo, Qty, Remark, IsSplit, IsWarranty");
                sql.AppendLine(" FROM Cust_Complaint_TempDT WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID)");
                sql.AppendLine(" ORDER BY ModelNo");

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
                        var data = new CCPDetail
                        {
                            Data_ID = item.Field<Int64>("Data_ID"),
                            ModelNo = item.Field<string>("ModelNo"),
                            Qty = item.Field<Int32>("Qty"),
                            Remark = item.Field<string>("Remark"),
                            IsSplit = item.Field<string>("IsSplit"),
                            IsWarranty = item.Field<string>("IsWarranty")
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
        /// [已開案客訴] 固定收信清單(依流程不同)
        /// </summary>
        /// <param name="type">客訴來源</param>
        /// <param name="flowID">FlowID(空值為所有人)</param>
        /// <returns></returns>
        public IQueryable<MailReceiver> GetCCP_MailReceiver(string type, string flowID)
        {
            //----- 宣告 -----
            List<MailReceiver> dataList = new List<MailReceiver>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Email");
                sql.AppendLine(" FROM Cust_Complaint_InformFlow");
                sql.AppendLine(" WHERE (CC_Type = @CC_Type)");

                if (!string.IsNullOrWhiteSpace(flowID))
                {
                    sql.Append(" AND (FlowID = @FlowID)");

                    cmd.Parameters.AddWithValue("FlowID", flowID);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("CC_Type", Convert.ToInt32(type));


                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new MailReceiver
                        {
                            Email = item.Field<string>("Email"),
                        };

                        //將項目加入至集合
                        dataList.Add(data);

                    }
                }

                //回傳集合
                return dataList.AsQueryable();
            }
        }


        #endregion *** 客訴 E ***


        #region *** 電商數據 S ***

        #region >> 年表 <<
        /// <summary>
        /// [電商數據年表] 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="DataType">類型</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ECDItem_Year> GetOneECD_byYear(Dictionary<string, string> search, string lang
            , int DataType, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetECDList_byYear(search, lang, DataType, 0, 1, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [電商數據年表] 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="DataType">類型</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ECDItem_Year> GetECDList_byYear(Dictionary<string, string> search, string lang
            , int DataType, int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                List<ECDItem_Year> dataList = new List<ECDItem_Year>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數
                string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

                //取得SQL語法
                subSql = ECDSQL_byYear(search, fieldLang);
                //取得SQL參數集合
                subParamList = ECDParams_byYear(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT COUNT(TbAll.SeqNo) AS TotalCnt FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@DataType", DataType));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, dbConn.DBS.Report, out ErrMsg))
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
                    sqlParamList.Add(new SqlParameter("@DataType", DataType));
                    sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                    sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Report, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new ECDItem_Year
                            {
                                SeqNo = item.Field<Int32>("SeqNo"),
                                Data_ID = item.Field<Guid>("Data_ID"),
                                RefType = item.Field<Int16>("RefType"),
                                RefMall = item.Field<Int32>("RefMall"),
                                MallName = item.Field<string>("MallName"),
                                setYear = item.Field<Int32>("setYear"),
                                Price_Sales = item.Field<double>("Price_Sales"),
                                Price_Rebate = item.Field<double>("Price_Rebate"),
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
        /// [電商數據年表] 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="fieldLang">欄位語系(ex:zh_TW)</param>
        /// <returns></returns>
        /// <see cref="GetECDList_byYear"/>
        private StringBuilder ECDSQL_byYear(Dictionary<string, string> search, string fieldLang)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.SeqNo, Base.Data_ID, Base.RefType");
            sql.AppendLine("  , Base.RefMall, (RefMall.Class_Name_{0}) AS MallName".FormatThis(fieldLang));
            sql.AppendLine("  , Base.setYear, Base.Price_Sales, Base.Price_Rebate");
            sql.AppendLine("  , Base.Create_Time, Base.Update_Time");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who COLLATE Chinese_Taiwan_Stroke_BIN)) AS Create_Name");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who COLLATE Chinese_Taiwan_Stroke_BIN)) AS Update_Name");
            sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY Base.Create_Time) AS RowIdx");
            sql.AppendLine(" FROM SZ_eCommerce_setYear Base");
            sql.AppendLine("  INNER JOIN SZ_eCommerce_RefMall RefMall ON Base.RefMall = RefMall.Class_ID");
            sql.AppendLine(" WHERE (Base.RefType = @DataType)");


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

                        case "Year":
                            sql.Append(" AND (Base.setYear = @Year)");

                            break;

                        case "Mall":
                            sql.Append(" AND (Base.RefMall = @Mall)");

                            break;
                    }
                }
            }
            #endregion

            return sql;
        }


        /// <summary>
        /// [電商數據年表] 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetECDList_byYear"/>
        private List<SqlParameter> ECDParams_byYear(Dictionary<string, string> search)
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

                        case "Year":
                            sqlParamList.Add(new SqlParameter("@Year", item.Value));

                            break;

                        case "Mall":
                            sqlParamList.Add(new SqlParameter("@Mall", item.Value));

                            break;
                    }
                }
            }


            return sqlParamList;
        }
        #endregion


        #region >> 月表 <<
        /// <summary>
        /// [電商數據月表] 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="DataType">類型</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ECDItem_Month> GetOneECD_byMonth(Dictionary<string, string> search, string lang
            , int DataType, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetECDList_byMonth(search, lang, DataType, 0, 1, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [電商數據月表] 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="DataType">類型</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ECDItem_Month> GetECDList_byMonth(Dictionary<string, string> search, string lang
            , int DataType, int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                List<ECDItem_Month> dataList = new List<ECDItem_Month>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數
                string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

                //取得SQL語法
                subSql = ECDSQL_byMonth(search, fieldLang);
                //取得SQL參數集合
                subParamList = ECDParams_byMonth(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT COUNT(TbAll.SeqNo) AS TotalCnt FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@DataType", DataType));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, dbConn.DBS.Report, out ErrMsg))
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
                    sqlParamList.Add(new SqlParameter("@DataType", DataType));
                    sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                    sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Report, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new ECDItem_Month
                            {
                                SeqNo = item.Field<Int32>("SeqNo"),
                                Data_ID = item.Field<Guid>("Data_ID"),
                                RefType = item.Field<Int16>("RefType"),
                                RefMall = item.Field<Int32>("RefMall"),
                                MallName = item.Field<string>("MallName"),
                                setYear = item.Field<Int32>("setYear"),
                                setMonth = item.Field<Int32>("setMonth"),
                                Price_Income = item.Field<double?>("Price_Income"),
                                Price_SalesRebate = item.Field<double?>("Price_SalesRebate"),
                                Price_Cost = item.Field<double?>("Price_Cost"),
                                Price_Profit = item.Field<double?>("Price_Profit"),
                                Price_Purchase = item.Field<double?>("Price_Purchase"),
                                Price_Back = item.Field<double?>("Price_Back"),
                                Price_PurchaseRebate = item.Field<double?>("Price_PurchaseRebate"),
                                Price_Promo = item.Field<double?>("Price_Promo"),
                                Price_Freight = item.Field<double?>("Price_Freight"),
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
        /// [電商數據月表] 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="fieldLang">欄位語系(ex:zh_TW)</param>
        /// <returns></returns>
        /// <see cref="GetECDList_byMonth"/>
        private StringBuilder ECDSQL_byMonth(Dictionary<string, string> search, string fieldLang)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.SeqNo, Base.Data_ID, Base.RefType");
            sql.AppendLine("  , Base.RefMall, (RefMall.Class_Name_{0}) AS MallName".FormatThis(fieldLang));
            sql.AppendLine("  , Base.setYear, Base.setMonth");
            sql.AppendLine("  , Base.Price_Income, Base.Price_SalesRebate, Base.Price_Cost, Base.Price_Profit");
            sql.AppendLine("  , Base.Price_Purchase, Base.Price_Back, Base.Price_PurchaseRebate, Base.Price_Promo, Base.Price_Freight");
            sql.AppendLine("  , Base.Create_Time, Base.Update_Time");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who COLLATE Chinese_Taiwan_Stroke_BIN)) AS Create_Name");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who COLLATE Chinese_Taiwan_Stroke_BIN)) AS Update_Name");
            sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY Base.Create_Time) AS RowIdx");
            sql.AppendLine(" FROM SZ_eCommerce_setMonth Base");
            sql.AppendLine("  INNER JOIN SZ_eCommerce_RefMall RefMall ON Base.RefMall = RefMall.Class_ID");
            sql.AppendLine(" WHERE (Base.RefType = @DataType)");

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

                        case "Year":
                            sql.Append(" AND (Base.setYear = @Year)");

                            break;

                        case "Month":
                            sql.Append(" AND (Base.setMonth = @Month)");

                            break;

                        case "Mall":
                            sql.Append(" AND (Base.RefMall = @Mall)");

                            break;
                    }
                }
            }
            #endregion

            return sql;
        }


        /// <summary>
        /// [電商數據月表] 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetECDList_byMonth"/>
        private List<SqlParameter> ECDParams_byMonth(Dictionary<string, string> search)
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

                        case "Year":
                            sqlParamList.Add(new SqlParameter("@Year", item.Value));

                            break;

                        case "Month":
                            sqlParamList.Add(new SqlParameter("@Month", item.Value));

                            break;

                        case "Mall":
                            sqlParamList.Add(new SqlParameter("@Mall", item.Value));

                            break;
                    }
                }
            }


            return sqlParamList;
        }


        /// <summary>
        /// [電商數據月表] 單身-促銷費用
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ECDItem_MonthDT> GetECD_MonthDT(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ECDItem_MonthDT> dataList = new List<ECDItem_MonthDT>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Base.Data_ID, Base.Parent_ID");
                sql.AppendLine(" , Base.RecordDate, Base.RecordType, Base.RecordMoney");
                sql.AppendLine(" , Base.CheckDate, Base.CheckMoney, Base.CheckInvoiceDate");
                sql.AppendLine(" , Base.Create_Time, Base.Update_Time");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who COLLATE Chinese_Taiwan_Stroke_BIN)) AS Create_Name");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who COLLATE Chinese_Taiwan_Stroke_BIN)) AS Update_Name");
                sql.AppendLine(" FROM SZ_eCommerce_PromoDT Base");
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
                            case "DataID":
                                sql.Append(" AND (Base.Data_ID = @Data_ID)");
                                cmd.Parameters.AddWithValue("Data_ID", item.Value);

                                break;

                            case "ParentID":
                                sql.Append(" AND (Base.Parent_ID = @Parent_ID)");
                                cmd.Parameters.AddWithValue("Parent_ID", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Report, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ECDItem_MonthDT
                        {
                            Data_ID = item.Field<Int64>("Data_ID"),
                            Parent_ID = item.Field<Guid>("Parent_ID"),
                            RecordDate = item.Field<DateTime?>("RecordDate").ToString().ToDateString("yyyy/MM/dd"),
                            RecordType = item.Field<Int32>("RecordType"),
                            RecordMoney = item.Field<double?>("RecordMoney"),
                            CheckDate = item.Field<DateTime?>("CheckDate").ToString().ToDateString("yyyy/MM/dd"),
                            CheckMoney = item.Field<double?>("CheckMoney"),
                            CheckInvoiceDate = item.Field<DateTime?>("CheckInvoiceDate").ToString().ToDateString("yyyy/MM/dd"),
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
        }


        #endregion


        #region >> 日表 <<
        /// <summary>
        /// [電商數據日表] 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="DataType">類型</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ECDItem_Date> GetOneECD_byDate(Dictionary<string, string> search, string lang
            , int DataType, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetECDList_byDate(search, lang, DataType, 0, 1, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [電商數據日表] 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="DataType">類型</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ECDItem_Date> GetECDList_byDate(Dictionary<string, string> search, string lang
            , int DataType, int startRow, int endRow
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                List<ECDItem_Date> dataList = new List<ECDItem_Date>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數
                string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

                //取得SQL語法
                subSql = ECDSQL_byDate(search, fieldLang);
                //取得SQL參數集合
                subParamList = ECDParams_byDate(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();
                    sql.AppendLine(" SELECT COUNT(TbAll.SeqNo) AS TotalCnt FROM (");

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" ) AS TbAll");

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();
                    sqlParamList.Clear();
                    //cmd.CommandTimeout = 60;   //單位:秒

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@DataType", DataType));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmdCnt.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, dbConn.DBS.Report, out ErrMsg))
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
                    sqlParamList.Add(new SqlParameter("@DataType", DataType));
                    sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                    sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

                    //加入參數陣列
                    cmd.Parameters.AddRange(sqlParamList.ToArray());

                    //Execute
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Report, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new ECDItem_Date
                            {
                                SeqNo = item.Field<Int32>("SeqNo"),
                                Data_ID = item.Field<Guid>("Data_ID"),
                                RefType = item.Field<Int16>("RefType"),
                                RefMall = item.Field<Int32>("RefMall"),
                                MallName = item.Field<string>("MallName"),
                                setDate = item.Field<DateTime?>("setDate").ToString().ToDateString("yyyy/MM/dd"),
                                Price_RefSales = item.Field<double?>("Price_RefSales"),
                                Price_RefProfit = item.Field<double?>("Price_RefProfit"),
                                Price_RealSales = item.Field<double?>("Price_RealSales"),
                                Price_RealProfit = item.Field<double?>("Price_RealProfit"),
                                Price_OrderPrice = item.Field<double?>("Price_OrderPrice"),
                                Price_ROI = item.Field<double?>("Price_ROI"),
                                Price_ClickCost = item.Field<double?>("Price_ClickCost"),
                                Price_Adv1 = item.Field<double?>("Price_Adv1"),
                                Price_Adv2 = item.Field<double?>("Price_Adv2"),
                                Price_Adv3 = item.Field<double?>("Price_Adv3"),
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
        /// [電商數據日表] 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="fieldLang">欄位語系(ex:zh_TW)</param>
        /// <returns></returns>
        /// <see cref="GetECDList_byDate"/>
        private StringBuilder ECDSQL_byDate(Dictionary<string, string> search, string fieldLang)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.SeqNo, Base.Data_ID, Base.RefType");
            sql.AppendLine("  , Base.RefMall, (RefMall.Class_Name_{0}) AS MallName".FormatThis(fieldLang));
            sql.AppendLine("  , Base.setDate");
            sql.AppendLine("  , Base.Price_RefSales, Base.Price_RefProfit, Base.Price_RealSales, Base.Price_RealProfit");
            sql.AppendLine("  , Base.Price_OrderPrice, Base.Price_ROI, Base.Price_ClickCost");
            sql.AppendLine("  , Base.Price_Adv1, Base.Price_Adv2, Base.Price_Adv3");
            sql.AppendLine("  , Base.Create_Time, Base.Update_Time");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who COLLATE Chinese_Taiwan_Stroke_BIN)) AS Create_Name");
            sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who COLLATE Chinese_Taiwan_Stroke_BIN)) AS Update_Name");
            sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY Base.Create_Time) AS RowIdx");
            sql.AppendLine(" FROM SZ_eCommerce_setDate Base");
            sql.AppendLine("  INNER JOIN SZ_eCommerce_RefMall RefMall ON Base.RefMall = RefMall.Class_ID");
            sql.AppendLine(" WHERE (Base.RefType = @DataType)");

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

                        case "sDate":
                            sql.Append(" AND (Base.setDate >= @sDate)");

                            break;

                        case "eDate":
                            sql.Append(" AND (Base.setDate <= @eDate)");

                            break;

                        case "Mall":
                            sql.Append(" AND (Base.RefMall = @Mall)");

                            break;
                    }
                }
            }
            #endregion

            return sql;
        }


        /// <summary>
        /// [電商數據日表] 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetECDList_byDate"/>
        private List<SqlParameter> ECDParams_byDate(Dictionary<string, string> search)
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

                        case "sDate":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value + " 00:00:00"));
                            break;

                        case "eDate":
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value + " 23:59:59"));
                            break;

                        case "Mall":
                            sqlParamList.Add(new SqlParameter("@Mall", item.Value));

                            break;
                    }
                }
            }


            return sqlParamList;
        }

        #endregion


        /// <summary>
        /// 取得商城 - 依類型區分
        /// </summary>
        /// <param name="type">1:工具 2:科玩</param>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetEC_RefMall(string type, string lang, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name_{0} AS Label"
                    .FormatThis(fn_Language.Get_LangCode(lang).Replace("-", "_")));
                sql.AppendLine(" FROM SZ_eCommerce_RefMall WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Class_Type = @type) AND (Display = 'Y')");
                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("type", Convert.ToInt32(type));

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Report, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ClassItem
                        {
                            ID = item.Field<int>("ID"),
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
        /// 取得Excel欄位,用來轉入資料
        /// </summary>
        /// <param name="filePath">完整磁碟路徑</param>
        /// <param name="sheetName">工作表名稱</param>
        /// <returns></returns>
        public IQueryable<ECDItem_PriceList> GetEC_ExcelData(string filePath, string sheetName)
        {
            try
            {
                //----- 宣告 -----
                List<ECDItem_PriceList> dataList = new List<ECDItem_PriceList>();

                //[Excel] - 取得原始資料
                var excelFile = new ExcelQueryFactory(filePath);
                var queryVals = excelFile.Worksheet(sheetName);

                //宣告各內容參數
                string _ProdID = "";
                double _Price1 = 0;

                //資料迴圈
                foreach (var val in queryVals)
                {
                    _ProdID = val[0];
                    _Price1 = Convert.ToDouble(val[1]);

                    //加入項目
                    var data = new ECDItem_PriceList
                    {
                        ProdID = _ProdID,
                        Price1 = _Price1
                    };

                    //將項目加入至集合
                    dataList.Add(data);

                }

                //回傳集合
                return dataList.AsQueryable();
            }
            catch (Exception ex)
            {

                throw new Exception("請檢查Excel格式是否正確!!格式可參考Excel範本." + ex.Message.ToString());
            }
        }

        #endregion *** 電商數據 E ***


        #region *** 出貨明細表(外銷) S ***

        /// <summary>
        /// [出貨明細表](外銷) 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipData_Item> GetOneShipData(Dictionary<string, string> search, string dbs, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetShipData(search, dbs, 0, 1, out dataCnt, out ErrMsg);
        }

        /// <summary>
        /// [出貨明細表](外銷) 所有資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipData_Item> GetAllShipData(Dictionary<string, string> search, string dbs, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetShipData(search, dbs, 0, 9999999, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [出貨明細表](外銷) 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipData_Item> GetShipData(Dictionary<string, string> search, string dbs
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
                List<ShipData_Item> dataList = new List<ShipData_Item>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數
                //string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

                //取得SQL語法
                subSql = GetSQL_ShipData(search, dbs);
                //取得SQL參數集合
                subParamList = GetParams_ShipData(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" SELECT COUNT(TblERP.Ship_SID) AS TotalCnt");
                    sql.AppendLine(" FROM TblERP");

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

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" SELECT TblERP.*");
                    sql.AppendLine(" , Base.Data_ID");
                    sql.AppendLine(" , Base.BoxCnt, Base.Pallet, Base.Weight, Base.Cuft");
                    sql.AppendLine(" , Base.TradeTerms, Base.Cost_Customs, Base.Cost_LocalCharge, Base.Cost_Cert");
                    sql.AppendLine(" , Base.Cost_Freight, Base.Cost_Business, Base.Cost_Shipment");
                    sql.AppendLine(" , Base.Cost_Fee, Base.FWD, Base.Cost_Trade");
                    sql.AppendLine(" , Base.Cost_Service, Base.Cost_Use, Base.Remark, Base.TrackingNo");
                    sql.AppendLine(" , RefShip.Class_ID AS ShipID, RefShip.Class_Name_zh_TW AS ShipName");
                    sql.AppendLine(" , RefPlace.Class_ID AS PlaceID, RefPlace.Class_Name_zh_TW AS PlaceName");
                    sql.AppendLine(" , RefCheck.Class_ID AS CheckID, RefCheck.Class_Name_zh_TW AS CheckName");
                    sql.AppendLine(" , Base.Create_Who, Base.Create_Time, Base.Update_Who, Base.Update_Time");
                    sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Create_Who)) AS Create_Name");
                    sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Update_Who)) AS Update_Name");

                    sql.AppendLine(" FROM TblERP");
                    sql.AppendLine("  LEFT JOIN Shipment_Data Base ON TblERP.Ship_FID = Base.Ship_FID AND TblERP.Ship_SID = Base.Ship_SID AND TblERP.SO_FID = Base.SO_FID AND TblERP.SO_SID = Base.SO_SID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass RefShip ON Base.ShipID = RefShip.Class_ID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass RefPlace ON Base.PlaceID = RefPlace.Class_ID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass RefCheck ON Base.CheckID = RefCheck.Class_ID");
                    sql.AppendLine(" WHERE (TblERP.RowIdx >= @startRow) AND (TblERP.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TblERP.RowIdx");

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
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            #region ** 欄位運算 **

                            //取得運算值
                            double _localPrice = item.Field<double>("localPrice");  //(M)
                            double _Cost_Customs = item.Field<double?>("Cost_Customs") ?? 0;    //(N)
                            double _Cost_LocalCharge = item.Field<double?>("Cost_LocalCharge") ?? 0;    //(O)
                            double _Cost_Cert = item.Field<double?>("Cost_Cert") ?? 0;  //(O1)
                            double _Cost_Freight = item.Field<double?>("Cost_Freight") ?? 0;    //(P)
                            double _Cost_Business = item.Field<double?>("Cost_Business") ?? 0;  //(Q)
                            double _Cost_Shipment = item.Field<double?>("Cost_Shipment") ?? 0;  //(T)
                            double _Cost_Fee = item.Field<double?>("Cost_Fee") ?? 0;    //(U)
                            double _Tax = item.Field<double>("Tax");    //(W)

                            //** 出口費用小計(R) (R = N + O + O1 + P + Q) **
                            double _Cnt_CostExport = _Cost_Customs + _Cost_LocalCharge + _Cost_Cert + _Cost_Freight + _Cost_Business;

                            //** 代收費用(V) (V = U * W) **
                            double _Cnt_CostLocalFee = _Cost_Fee * _Tax;

                            //** 實際出口費用(X) (X = N + O + O1 + P + T - V) **
                            double _Cnt_CostFullExport = (_Cost_Customs + _Cost_LocalCharge + _Cost_Cert + _Cost_Freight + _Cost_Shipment) - _Cnt_CostLocalFee;

                            //** 費用比率(Y) (Y = X / M) * 100 **
                            double _Cnt_CostPercent = (_localPrice > 0) ? (_Cnt_CostFullExport / _localPrice) * 100 : 0;

                            #endregion


                            //加入項目
                            var data = new ShipData_Item
                            {
                                Ship_FID = item.Field<string>("Ship_FID"),
                                Ship_SID = item.Field<string>("Ship_SID"),
                                SO_FID = item.Field<string>("SO_FID"),
                                SO_SID = item.Field<string>("SO_SID"),
                                SO_Date = item.Field<DateTime>("SO_Date").ToString().ToDateString("yyyy/MM/dd"),
                                BoxDate = item.Field<DateTime>("BoxDate").ToString().ToDateString("yyyy/MM/dd"),
                                CustID = item.Field<string>("CustID"),
                                CustName = item.Field<string>("CustName"),
                                InvNo = item.Field<string>("InvNo"),
                                PayTerms = item.Field<string>("PayTerms"),
                                Currency = item.Field<string>("Currency"),
                                Price = item.Field<double>("Price"),
                                localPrice = item.Field<double>("localPrice"),
                                Tax = _Tax,
                                CLS = item.Field<DateTime>("CLS") == null ? "" : item.Field<DateTime>("CLS").ToShortDateString().ToDateString("yyyy/MM/dd"),
                                ETD = item.Field<DateTime>("ETD") == null ? "" : item.Field<DateTime>("ETD").ToShortDateString().ToDateString("yyyy/MM/dd"),
                                ETA = item.Field<DateTime>("ETA") == null ? "" : item.Field<DateTime>("ETA").ToShortDateString().ToDateString("yyyy/MM/dd"),
                                Ship_NO = item.Field<string>("Ship_NO"),
                                OpcsCnt = item.Field<Int32>("OpcsCnt"),
                                OpcsItemCnt = item.Field<Int32>("OpcsItemCnt"),
                                diffDays = item.Field<Int32>("diffDays"),
                                SalesName = item.Field<string>("SalesName"),

                                //平台維護欄位
                                Data_ID = item.Field<Guid?>("Data_ID"),
                                BoxCnt = item.Field<Int32?>("BoxCnt"),
                                Pallet = item.Field<string>("Pallet"),
                                Weight = item.Field<double?>("Weight"),
                                Cuft = item.Field<double?>("Cuft"),
                                TradeTerms = item.Field<string>("TradeTerms"),

                                Cost_Customs = _Cost_Customs,
                                Cost_LocalCharge = _Cost_LocalCharge,
                                Cost_Cert = _Cost_Cert,
                                Cost_Freight = _Cost_Freight,
                                Cost_Business = _Cost_Business,
                                Cnt_CostExport = _Cnt_CostExport, //(R)

                                Cost_Shipment = item.Field<double?>("Cost_Shipment"), //(T)
                                Cost_Fee = item.Field<double?>("Cost_Fee"),
                                Cnt_CostLocalFee = _Cnt_CostLocalFee, //(V)
                                Cnt_CostFullExport = _Cnt_CostFullExport, //(X)
                                Cnt_CostPercent = Math.Round(_Cnt_CostPercent, 2), //(Y)

                                FWD = item.Field<string>("FWD"),
                                Cost_Trade = item.Field<double?>("Cost_Trade"),
                                Cost_Service = item.Field<double?>("Cost_Service"),
                                Cost_Use = item.Field<double?>("Cost_Use"),
                                Remark = item.Field<string>("Remark"),
                                TrackingNo = item.Field<string>("TrackingNo"),
                                ShipID = item.Field<Int32?>("ShipID") ?? 0,
                                ShipName = item.Field<string>("ShipName"),
                                PlaceID = item.Field<Int32?>("PlaceID") ?? 0,
                                PlaceName = item.Field<string>("PlaceName"),
                                CheckID = item.Field<Int32?>("CheckID") ?? 0,
                                CheckName = item.Field<string>("CheckName"),

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
        /// [出貨明細表](外銷) 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <returns></returns>
        /// <see cref="GetShipData"/>
        private StringBuilder GetSQL_ShipData(Dictionary<string, string> search, string dbs)
        {
            StringBuilder sql = new StringBuilder();
            string dbName = "";
            switch (dbs.ToUpper())
            {
                case "SH":
                    dbName = "SHPK2";
                    break;

                case "OIN1":
                    dbName = "Oin1";
                    break;

                default:
                    dbName = "prokit2";
                    break;
            }

            //SQL查詢
            sql.AppendLine(" ;WITH TblDataSource AS (");
            sql.AppendLine("     SELECT Base.TA001, Base.TA002, SaleDT.TH001, SaleDT.TH002");
            sql.AppendLine("     FROM [##DBName##].dbo.EPSTA AS Base WITH(NOLOCK)");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.EPSTB AS DT WITH(NOLOCK) ON Base.TA001 = DT.TB001 AND Base.TA002 = DT.TB002");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.COPTD AS OrderDT WITH(NOLOCK) ON DT.TB004 = OrderDT.TD001 AND DT.TB005 = OrderDT.TD002 AND DT.TB006 = OrderDT.TD003");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.COPTH AS SaleDT WITH(NOLOCK) ON OrderDT.TD001 = SaleDT.TH014 AND OrderDT.TD002 = SaleDT.TH015 AND OrderDT.TD003 = SaleDT.TH016");
            sql.AppendLine("     WHERE (1=1)");
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
                        case "sDate":
                            sql.Append(" AND (Base.TA053 >= @sDate)");

                            break;

                        case "eDate":
                            sql.Append(" AND (Base.TA053 <= @eDate)");

                            break;
                    }
                }
            }
            #endregion
            sql.AppendLine("     GROUP BY Base.TA001, Base.TA002, SaleDT.TH001, SaleDT.TH002");
            sql.AppendLine(" )");
            sql.AppendLine(" , TblShip AS (");
            sql.AppendLine("  SELECT");
            sql.AppendLine("   Base.TA001 COLLATE Chinese_Taiwan_Stroke_BIN AS Ship_FID, RTRIM(Base.TA002) COLLATE Chinese_Taiwan_Stroke_BIN AS Ship_SID");
            sql.AppendLine("   , CONVERT(DATE, Base.TA053, 111) AS BoxDate, Base.TA042 AS InvNo");
            sql.AppendLine("   , CONVERT(DATE, Base.TA052, 111) AS CLS, CONVERT(DATE, Base.TA040, 111) AS ETD, CONVERT(DATE, Base.TA039, 111) AS ETA, Base.TA051 AS Ship_NO");
            sql.AppendLine("  FROM [##DBName##].dbo.EPSTA AS Base WITH(NOLOCK)");
            sql.AppendLine("   INNER JOIN TblDataSource ON Base.TA001 = TblDataSource.TA001 AND Base.TA002 = TblDataSource.TA002");
            sql.AppendLine("  GROUP BY Base.TA001, Base.TA002, Base.TA053, Base.TA042, Base.TA052, Base.TA040, Base.TA039, Base.TA051");
            sql.AppendLine(" )");
            sql.AppendLine(" , TblERP AS (");
            sql.AppendLine(" SELECT");
            sql.AppendLine("  TblShip.Ship_FID, TblShip.Ship_SID");
            sql.AppendLine(" , SO.TG001 COLLATE Chinese_Taiwan_Stroke_BIN AS SO_FID, RTRIM(SO.TG002) COLLATE Chinese_Taiwan_Stroke_BIN AS SO_SID");
            sql.AppendLine(" , CONVERT(DATE, SO.TG003, 111) AS SO_Date, TblShip.BoxDate");
            sql.AppendLine(" , RTRIM(Cust.MA001) AS CustID, RTRIM(Cust.MA002) AS CustName");
            sql.AppendLine(" , TblShip.InvNo, Terms.NA003 AS PayTerms");
            sql.AppendLine(" , SO.TG011 AS Currency, CAST(SO.TG013 AS FLOAT) AS Price");
            sql.AppendLine(" , CAST(SO.TG045 AS FLOAT) AS localPrice, CAST(SO.TG012 AS FLOAT) AS Tax");
            sql.AppendLine(" , TblShip.CLS, TblShip.ETD, TblShip.ETA, TblShip.Ship_NO");
            sql.AppendLine(" , (");
            sql.AppendLine("   SELECT COUNT(Tbl.id) FROM (");
            sql.AppendLine("     SELECT COPTD.TD001+COPTD.TD002 AS id");
            sql.AppendLine("     FROM [##DBName##].dbo.COPTD WITH(NOLOCK)");
            sql.AppendLine("         INNER JOIN [##DBName##].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
            sql.AppendLine("     WHERE COPTH.TH001 = SO.TG001 AND COPTH.TH002 = SO.TG002");
            sql.AppendLine("     GROUP BY COPTD.TD001, COPTD.TD002");
            sql.AppendLine("   ) AS Tbl");
            sql.AppendLine(" ) AS OpcsCnt");
            sql.AppendLine(" , (");
            sql.AppendLine("     SELECT COUNT(*)");
            sql.AppendLine("     FROM [##DBName##].dbo.COPTD WITH(NOLOCK)");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
            sql.AppendLine("     WHERE COPTH.TH001 = SO.TG001 AND COPTH.TH002 = SO.TG002");
            sql.AppendLine(" ) AS OpcsItemCnt");
            sql.AppendLine(" , DATEDIFF(DD, CAST(SO.TG003 AS date), CAST(TblShip.BoxDate AS date)) AS diffDays");
            sql.AppendLine(" , Emp.MV002 AS SalesName");
            sql.AppendLine(" , ROW_NUMBER() OVER(ORDER BY SO.TG003, TblShip.BoxDate, TblShip.Ship_FID, TblShip.Ship_SID) AS RowIdx");
            sql.AppendLine(" FROM [##DBName##].dbo.COPTG AS SO WITH(NOLOCK)");
            sql.AppendLine("  INNER JOIN TblDataSource ON SO.TG001 = TblDataSource.TH001 AND SO.TG002 = TblDataSource.TH002");
            sql.AppendLine("  INNER JOIN TblShip ON TblDataSource.TA001 = TblShip.Ship_FID AND TblDataSource.TA002 = TblShip.Ship_SID");
            sql.AppendLine("  INNER JOIN [##DBName##].dbo.COPMA AS Cust WITH(NOLOCK) ON SO.TG004 = Cust.MA001");
            sql.AppendLine("  LEFT JOIN [##DBName##].dbo.CMSNA AS Terms WITH(NOLOCK) ON SO.TG047 = Terms.NA002 AND Terms.NA001 = '2'");
            sql.AppendLine("  LEFT JOIN [##DBName##].dbo.CMSMV AS Emp WITH(NOLOCK) ON SO.TG006 = Emp.MV001");
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
                        case "sDate":
                            sql.Append(" AND (TblShip.BoxDate >= @sDate)");

                            break;

                        case "eDate":
                            sql.Append(" AND (TblShip.BoxDate <= @eDate)");

                            break;

                        case "Keyword":
                            //關鍵字
                            sql.Append(" AND (");
                            sql.Append(" UPPER(Cust.MA001) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(Cust.MA002) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(TblShip.InvNo) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(")");

                            break;
                    }
                }
            }
            #endregion

            sql.AppendLine(" )");


            //Replace 指定字元
            sql.Replace("##DBName##", dbName);

            //return
            return sql;
        }


        /// <summary>
        /// [出貨明細表](外銷) 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetSQL_ShipData"/>
        private List<SqlParameter> GetParams_ShipData(Dictionary<string, string> search)
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
                        case "sDate":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value.ToDateString("yyyyMMdd")));

                            break;

                        case "eDate":
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value.ToDateString("yyyyMMdd")));

                            break;

                        case "Keyword":
                            sqlParamList.Add(new SqlParameter("@keyword", item.Value));

                            break;

                    }
                }
            }


            return sqlParamList;
        }


        /// <summary>
        /// [出貨明細表](外銷) 取得參考類別
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="type"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetRefClass_ShipData(string lang, string type, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name_{0} AS Label"
                    .FormatThis(fn_Language.Get_LangCode(lang).Replace("-", "_")));
                sql.AppendLine(" FROM Shipment_RefClass WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Display = 'Y') AND (Class_Type = @type)");
                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("type", type);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ClassItem
                        {
                            ID = item.Field<int>("ID"),
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


        #endregion *** 出貨明細表(外銷) E ***


        #region *** 出貨明細表(進口) S ***

        /// <summary>
        /// [出貨明細表](進口) 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CustomsData_Item> GetOneCustomsData(Dictionary<string, string> search, string dbs, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetCustomsData(search, dbs, 0, 1, out dataCnt, out ErrMsg);
        }

        /// <summary>
        /// [出貨明細表](進口) 所有資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CustomsData_Item> GetAllCustomsData(Dictionary<string, string> search, string dbs, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetCustomsData(search, dbs, 0, 9999999, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [出貨明細表](進口) 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CustomsData_Item> GetCustomsData(Dictionary<string, string> search, string dbs
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
                List<CustomsData_Item> dataList = new List<CustomsData_Item>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數
                //string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

                //取得SQL語法
                subSql = GetSQL_CustomsData(search, dbs);
                //取得SQL參數集合
                subParamList = GetParams_CustomsData(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" SELECT COUNT(TblERP.Redeem_SID) AS TotalCnt");
                    sql.AppendLine(" FROM TblERP");

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

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" SELECT TblERP.*");
                    sql.AppendLine(" , Base.Data_ID");
                    sql.AppendLine(" , Base.Cost_Customs, Base.Cost_LocalCharge, Base.Cost_LocalBusiness");
                    sql.AppendLine(" , Base.Cost_Imports, Base.Cost_Trade, Base.Cost_ImportsBusiness");
                    sql.AppendLine(" , Base.Cost_Service, Base.Cost_Truck, Base.Remark");
                    sql.AppendLine(" , Base.Create_Who, Base.Create_Time, Base.Update_Who, Base.Update_Time");
                    sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Create_Who)) AS Create_Name");
                    sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Update_Who)) AS Update_Name");

                    sql.AppendLine(" FROM TblERP");
                    sql.AppendLine("  LEFT JOIN Customs_Data Base ON TblERP.Redeem_FID = Base.Redeem_FID AND TblERP.Redeem_SID = Base.Redeem_SID");
                    sql.AppendLine(" WHERE (TblERP.RowIdx >= @startRow) AND (TblERP.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TblERP.RowIdx");

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
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //** 代墊款(P=M+N+O) / 總計(Q=J+K+L+M+N+O) **
                            double _CustomsPrice = item.Field<double?>("CustomsPrice") ?? 0; //F
                            double _Cost_Customs = item.Field<double?>("Cost_Customs") ?? 0; //J
                            double _Cost_LocalCharge = item.Field<double?>("Cost_LocalCharge") ?? 0; //K
                            double _Cost_LocalBusiness = item.Field<double?>("Cost_LocalBusiness") ?? 0; //L
                            double _Cost_Imports = item.Field<double?>("Cost_Imports") ?? 0; //M
                            double _Cost_Trade = item.Field<double?>("Cost_Trade") ?? 0; //N
                            double _Cost_ImportsBusiness = item.Field<double?>("Cost_ImportsBusiness") ?? 0; //O

                            double _Cnt_CostFee = _Cost_Imports + _Cost_Trade + _Cost_ImportsBusiness;
                            double _Cnt_Total = _Cost_Customs + _Cost_LocalCharge + _Cost_LocalBusiness
                                + _Cost_Imports + _Cost_Trade + _Cost_ImportsBusiness;

                            //** 進口費用%(S) **
                            double _Cnt_CostPercent = (_CustomsPrice > 0) ? ((_Cost_Customs + _Cost_LocalCharge) / _CustomsPrice) * 100 : 0;

                            //加入項目
                            var data = new CustomsData_Item
                            {
                                Redeem_FID = item.Field<string>("Redeem_FID"),
                                Redeem_SID = item.Field<string>("Redeem_SID"),
                                CustomsDate = item.Field<DateTime>("CustomsDate").ToString().ToDateString("yyyy/MM/dd"),
                                RedeemDate = item.Field<DateTime>("RedeemDate").ToString().ToDateString("yyyy/MM/dd"),
                                SupID = item.Field<string>("SupID"),
                                SupName = item.Field<string>("SupName"),
                                QtyMark = item.Field<string>("QtyMark"),
                                CustomsPrice = item.Field<double>("CustomsPrice"),
                                Currency = item.Field<string>("Currency"),
                                PurPrice = item.Field<double>("PurPrice"),
                                Tax = item.Field<double>("Tax"),
                                CustomsNo = item.Field<string>("CustomsNo"),
                                PurCnt = item.Field<Int32>("PurCnt"),
                                PurItemCnt = item.Field<Int32>("PurItemCnt"),

                                //平台維護欄位
                                Data_ID = item.Field<Guid?>("Data_ID"),
                                Cost_Customs = _Cost_Customs,
                                Cost_LocalCharge = _Cost_LocalCharge,
                                Cost_LocalBusiness = _Cost_LocalBusiness,
                                Cost_Imports = _Cost_Imports,
                                Cost_Trade = _Cost_Trade,
                                Cost_ImportsBusiness = _Cost_ImportsBusiness,
                                Cnt_CostFee = _Cnt_CostFee,
                                Cnt_Total = _Cnt_Total,
                                Cnt_CostPercent = Math.Round(_Cnt_CostPercent, 2),
                                Cost_Service = item.Field<double?>("Cost_Service"),
                                Cost_Truck = item.Field<double?>("Cost_Truck"),
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
        /// [出貨明細表](進口) 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <returns></returns>
        /// <see cref="GetCustomsData"/>
        private StringBuilder GetSQL_CustomsData(Dictionary<string, string> search, string dbs)
        {
            StringBuilder sql = new StringBuilder();
            string dbName = "";
            switch (dbs.ToUpper())
            {
                case "SH":
                    dbName = "SHPK2";
                    break;

                default:
                    dbName = "prokit2";
                    break;
            }

            //SQL查詢
            sql.AppendLine(" ;WITH TblERP AS (");
            sql.AppendLine(" SELECT");
            sql.AppendLine("  Base.TG001 COLLATE Chinese_Taiwan_Stroke_BIN AS Redeem_FID, RTRIM(Base.TG002) COLLATE Chinese_Taiwan_Stroke_BIN AS Redeem_SID");
            sql.AppendLine(" , CONVERT(DATE, Base.TG003, 111) AS CustomsDate");
            sql.AppendLine(" , CONVERT(DATE, Base.TG041, 111) AS RedeemDate");
            sql.AppendLine(" , Base.TG006 AS SupID, RTRIM(Sup.MA002) AS SupName");
            sql.AppendLine(" , Base.TG038 AS QtyMark");
            sql.AppendLine(" , CAST(Base.TG063 AS FLOAT) AS CustomsPrice");
            sql.AppendLine(" , Base.TG017 AS Currency");
            sql.AppendLine(" , CAST(Base.TG018 AS FLOAT) AS Tax");
            sql.AppendLine(" , CAST(Base.TG019 AS FLOAT) AS PurPrice");
            sql.AppendLine(" , Base.TG004 AS CustomsNo");
            sql.AppendLine(" , (");
            sql.AppendLine("   SELECT COUNT(Tbl.id) FROM (");
            sql.AppendLine(" 	SELECT PURTD.TD001+PURTD.TD002 AS id");
            sql.AppendLine(" 	FROM [##DBName##].dbo.PURTD WITH(NOLOCK)");
            sql.AppendLine(" 		INNER JOIN [##DBName##].dbo.IPSTI WITH(NOLOCK) ON PURTD.TD001 = IPSTI.TI004 AND PURTD.TD002 = IPSTI.TI005 AND PURTD.TD003 = IPSTI.TI006");
            sql.AppendLine(" 	WHERE IPSTI.TI001 = Base.TG001 AND IPSTI.TI002 = Base.TG002");
            sql.AppendLine(" 	GROUP BY PURTD.TD001+PURTD.TD002");
            sql.AppendLine("   ) AS Tbl");
            sql.AppendLine(" ) AS PurCnt");
            sql.AppendLine(" , (");
            sql.AppendLine(" 	SELECT COUNT(*)");
            sql.AppendLine(" 	FROM [##DBName##].dbo.PURTD WITH(NOLOCK)");
            sql.AppendLine(" 		INNER JOIN [##DBName##].dbo.IPSTI WITH(NOLOCK) ON PURTD.TD001 = IPSTI.TI004 AND PURTD.TD002 = IPSTI.TI005 AND PURTD.TD003 = IPSTI.TI006");
            sql.AppendLine(" 	WHERE IPSTI.TI001 = Base.TG001 AND IPSTI.TI002 = Base.TG002");
            sql.AppendLine(" ) AS PurItemCnt");
            sql.AppendLine(" , ROW_NUMBER() OVER(ORDER BY Base.TG003, Base.TG041, Base.TG001, Base.TG002) AS RowIdx");
            sql.AppendLine(" FROM [##DBName##].dbo.IPSTG Base WITH(NOLOCK)");
            sql.AppendLine("  INNER JOIN [##DBName##].dbo.PURMA Sup WITH(NOLOCK) ON Base.TG006 = Sup.MA001");
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
                        case "sDate":
                            sql.Append(" AND (Base.TG003 >= @sDate)");

                            break;

                        case "eDate":
                            sql.Append(" AND (Base.TG003 <= @eDate)");

                            break;

                        case "Keyword":
                            //關鍵字
                            sql.Append(" AND (");
                            sql.Append(" UPPER(Base.TG004) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(Sup.MA001) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(Sup.MA002) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(")");

                            break;
                    }
                }
            }
            #endregion

            sql.AppendLine(" )");


            //Replace 指定字元
            sql.Replace("##DBName##", dbName);

            //return
            return sql;
        }


        /// <summary>
        /// [出貨明細表](進口) 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetSQL_CustomsData"/>
        private List<SqlParameter> GetParams_CustomsData(Dictionary<string, string> search)
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
                        case "sDate":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value.ToDateString("yyyyMMdd")));

                            break;

                        case "eDate":
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value.ToDateString("yyyyMMdd")));

                            break;

                        case "Keyword":
                            sqlParamList.Add(new SqlParameter("@keyword", item.Value));

                            break;

                    }
                }
            }


            return sqlParamList;
        }

        #endregion *** 出貨明細表(進口) E ***


        #region *** 出貨明細表(內銷) S ***

        /// <summary>
        /// [出貨明細表](內銷) 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipData_LocalItem> GetOneShipLocalData(Dictionary<string, string> search, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetShipLocalData(search, 0, 1, out dataCnt, out ErrMsg);
        }

        /// <summary>
        /// [出貨明細表](內銷) 所有資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipData_LocalItem> GetAllShipLocalData(Dictionary<string, string> search, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetShipLocalData(search, 0, 9999999, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [出貨明細表](內銷) 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipData_LocalItem> GetShipLocalData(Dictionary<string, string> search
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
                List<ShipData_LocalItem> dataList = new List<ShipData_LocalItem>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數

                //取得SQL語法
                subSql = GetSQL_ShipLocalData(search);
                //取得SQL參數集合
                subParamList = GetParams_ShipLocalData(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" SELECT COUNT(TblERP.SO_SID) AS TotalCnt");
                    sql.AppendLine(" FROM TblERP");

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

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" SELECT TblERP.*");
                    sql.AppendLine(" , Base.Data_ID");
                    sql.AppendLine(" , Base.BoxCnt, ISNULL(Ft.Freight, Base.Freight) AS Freight");
                    sql.AppendLine(" , Base.SendNo, Base.Remark");
                    sql.AppendLine(" , RefShip.Class_ID AS ShipID, RefShip.Class_Name_zh_TW AS ShipName");
                    sql.AppendLine(" , RefCust.Class_ID AS CustType, RefCust.Class_Name_zh_TW AS CustTypeName");
                    sql.AppendLine(" , RefProd.Class_ID AS ProdType, RefProd.Class_Name_zh_TW AS ProdTypeName");
                    sql.AppendLine(" , Ref_Send.Class_ID AS SendType, RefProd.Class_Name_zh_TW AS SendTypeName");
                    sql.AppendLine(" , Base.Create_Who, Base.Create_Time, Base.Update_Who, Base.Update_Time");
                    sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Create_Who)) AS Create_Name");
                    sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Update_Who)) AS Update_Name");
                    sql.AppendLine(" , (CASE WHEN ISNULL(Base.ShipNo, '') = ''");
                    sql.AppendLine("    THEN ISNULL(ISNULL(TblBBC_Mall.ShipmentNo, TblBBC_twSales.ShipNo), '')");
                    sql.AppendLine("    ELSE Base.ShipNo");
                    sql.AppendLine("   END) AS ShipNo");
                    sql.AppendLine(" FROM TblERP");
                    sql.AppendLine("  LEFT JOIN TblBBC_Mall ON TblERP.SO_FID = TblBBC_Mall.SO_FID AND TblERP.SO_SID = TblBBC_Mall.SO_SID");
                    sql.AppendLine("  LEFT JOIN TblBBC_twSales ON TblERP.SO_FID = TblBBC_twSales.SO_FID AND TblERP.SO_SID = TblBBC_twSales.SO_SID");
                    sql.AppendLine("  LEFT JOIN Shipment_Local_Data Base ON TblERP.SO_FID = Base.SO_FID AND TblERP.SO_SID = Base.SO_SID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass RefShip ON Base.ShipID = RefShip.Class_ID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass RefCust ON Base.CustType = RefCust.Class_ID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass RefProd ON Base.ProdType = RefProd.Class_ID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass Ref_Send ON Base.SendType = Ref_Send.Class_ID");
                    sql.AppendLine("  LEFT JOIN Shipment_Local_Freight Ft ON Ft.ShipNo = ISNULL(ISNULL(TblBBC_Mall.ShipmentNo, TblBBC_twSales.ShipNo), '')");
                    sql.AppendLine(" WHERE (TblERP.RowIdx >= @startRow) AND (TblERP.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TblERP.RowIdx");

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
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            #region ** 欄位運算 **

                            //取得運算值
                            double _Price = item.Field<double>("Price");  //(G)
                            double _Freight = item.Field<double?>("Freight") ?? 0; //(L)

                            //** 運費比(M) (M = L / G) * 100 **
                            double _Cnt_FreightPercent = (_Price > 0) ? Math.Round((_Freight / _Price) * 100, 2) : 0;

                            #endregion


                            //加入項目
                            var data = new ShipData_LocalItem
                            {
                                SO_FID = item.Field<string>("SO_FID"),
                                SO_SID = item.Field<string>("SO_SID"),
                                SO_Date = item.Field<DateTime>("SO_Date").ToString().ToDateString("yyyy/MM/dd"),
                                CustID = item.Field<string>("CustID"),
                                CustName = item.Field<string>("CustName"),
                                Price = _Price, //(G)
                                InvNo_Start = item.Field<string>("InvNo_Start"),
                                InvNo_End = item.Field<string>("InvNo_End"),
                                OpcsCnt = item.Field<Int32>("OpcsCnt"),
                                OpcsItemCnt = item.Field<Int32>("OpcsItemCnt"),
                                SalesName = item.Field<string>("SalesName"),

                                //平台維護欄位
                                Data_ID = item.Field<Guid?>("Data_ID"),
                                BoxCnt = item.Field<Int32?>("BoxCnt") ?? 0,
                                ShipNo = item.Field<string>("ShipNo"),
                                Freight = _Freight, //(L)
                                Cnt_FreightPercent = _Cnt_FreightPercent, //運費比(%)(M=L/G)
                                SendNo = item.Field<string>("SendNo"),
                                Remark = item.Field<string>("Remark"),
                                ShipID = item.Field<Int32?>("ShipID") ?? 0,
                                ShipName = item.Field<string>("ShipName"),
                                CustType = item.Field<Int32?>("CustType") ?? 0,
                                CustTypeName = item.Field<string>("CustTypeName"),
                                ProdType = item.Field<Int32?>("ProdType") ?? 0,
                                ProdTypeName = item.Field<string>("ProdTypeName"),
                                SendType = item.Field<Int32?>("SendType") ?? 0,
                                SendTypeName = item.Field<string>("SendTypeName"),

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
        /// [出貨明細表](內銷) 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetShipLocalData"/>
        private StringBuilder GetSQL_ShipLocalData(Dictionary<string, string> search)
        {
            StringBuilder sql = new StringBuilder();

            //SQL查詢
            sql.AppendLine(" ;WITH TblERP AS (");
            sql.AppendLine(" SELECT");
            sql.AppendLine("  SO.TG001 COLLATE Chinese_Taiwan_Stroke_BIN AS SO_FID, RTRIM(SO.TG002) COLLATE Chinese_Taiwan_Stroke_BIN AS SO_SID");
            sql.AppendLine(" , CONVERT(DATE, SO.TG003, 111) AS SO_Date, CAST(SO.TG045 + SO.TG046 AS FLOAT) AS Price");
            sql.AppendLine(" , ISNULL(SO.TG098, '') AS InvNo_Start, ISNULL(SO.TG014, '') AS InvNo_End");
            sql.AppendLine(" , RTRIM(Cust.MA001) AS CustID, RTRIM(Cust.MA002) AS CustName");
            sql.AppendLine(" , (");
            sql.AppendLine("   SELECT COUNT(Tbl.id) FROM (");
            sql.AppendLine("     SELECT COPTD.TD001+COPTD.TD002 AS id");
            sql.AppendLine("     FROM [prokit2].dbo.COPTD WITH(NOLOCK)");
            sql.AppendLine("         INNER JOIN [prokit2].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
            sql.AppendLine("     WHERE COPTH.TH001 = SO.TG001 AND COPTH.TH002 = SO.TG002");
            sql.AppendLine("     GROUP BY COPTD.TD001, COPTD.TD002");
            sql.AppendLine("   ) AS Tbl");
            sql.AppendLine(" ) AS OpcsCnt");
            sql.AppendLine(" , (");
            sql.AppendLine("     SELECT COUNT(*)");
            sql.AppendLine("     FROM [prokit2].dbo.COPTD WITH(NOLOCK)");
            sql.AppendLine("      INNER JOIN [prokit2].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
            sql.AppendLine("     WHERE COPTH.TH001 = SO.TG001 AND COPTH.TH002 = SO.TG002");
            sql.AppendLine(" ) AS OpcsItemCnt");
            sql.AppendLine(" , Emp.MV002 AS SalesName");
            sql.AppendLine(" , ROW_NUMBER() OVER(ORDER BY SO.TG003, SO.TG001, SO.TG002) AS RowIdx");
            sql.AppendLine(" FROM [prokit2].dbo.COPTG AS SO WITH(NOLOCK)");
            sql.AppendLine("  INNER JOIN [prokit2].dbo.COPMA AS Cust WITH(NOLOCK) ON SO.TG004 = Cust.MA001");
            sql.AppendLine("  LEFT JOIN [prokit2].dbo.CMSMV AS Emp WITH(NOLOCK) ON SO.TG006 = Emp.MV001");
            sql.AppendLine(" WHERE (SO.TG005 = '130')");

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
                        case "sDate":
                            sql.Append(" AND (SO.TG003 >= @sDate)");

                            break;

                        case "eDate":
                            sql.Append(" AND (SO.TG003 <= @eDate)");

                            break;

                        case "Keyword":
                            //關鍵字
                            sql.Append(" AND (");
                            sql.Append(" UPPER(Cust.MA001) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(Cust.MA002) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(")");

                            break;
                    }
                }
            }
            #endregion

            sql.AppendLine(" )");

            /* 台灣電商BBC,物流資料 */
            sql.AppendLine(" , TblBBC_Mall AS (");
            sql.AppendLine(" SELECT");
            sql.AppendLine("  RTRIM(COPTC.TC004) AS CustID, COPTH.TH001 AS SO_FID, COPTH.TH002 AS SO_SID");
            sql.AppendLine("  , DT.ShipmentNo");
            sql.AppendLine(" FROM [prokit2].dbo.COPTC WITH(NOLOCK)");
            sql.AppendLine("  INNER JOIN [prokit2].dbo.COPTD WITH(NOLOCK) ON COPTC.TC001 = COPTD.TD001 AND COPTC.TC002 = COPTD.TD002");
            sql.AppendLine("  INNER JOIN [prokit2].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
            sql.AppendLine("  INNER JOIN [PKEF].dbo.TWBBC_Mall_ImportData_DT DT ON DT.OrderID = COPTC.TC012 COLLATE Chinese_Taiwan_Stroke_CI_AS AND DT.ERP_ModelNo = COPTD.TD004 COLLATE Chinese_Taiwan_Stroke_CI_AS");
            sql.AppendLine("  INNER JOIN [PKEF].dbo.TWBBC_Mall_ImportData Base ON Base.Data_ID = DT.Parent_ID AND Base.CustID = COPTC.TC004 COLLATE Chinese_Taiwan_Stroke_CI_AS");
            sql.AppendLine(" WHERE (COPTC.TC200 = 'Y') AND (DT.IsPass = 'Y')");
            sql.AppendLine(" GROUP BY COPTC.TC004, COPTH.TH001, COPTH.TH002, DT.ShipmentNo");
            sql.AppendLine(" )");

            /* 內業BBC,物流資料 */
            sql.AppendLine(" , TblBBC_twSales AS (");
            sql.AppendLine(" SELECT");
            sql.AppendLine("  RTRIM(COPTC.TC004) AS CustID, COPTH.TH001 AS SO_FID, COPTH.TH002 AS SO_SID");
            sql.AppendLine("  , DT.ShipNo");
            sql.AppendLine(" FROM [prokit2].dbo.COPTC WITH(NOLOCK)");
            sql.AppendLine("  INNER JOIN [prokit2].dbo.COPTD WITH(NOLOCK) ON COPTC.TC001 = COPTD.TD001 AND COPTC.TC002 = COPTD.TD002");
            sql.AppendLine("  INNER JOIN [prokit2].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
            sql.AppendLine("  INNER JOIN [PKEF].dbo.Order_Shipping DT ON DT.OrderID = COPTC.TC012 COLLATE Chinese_Taiwan_Stroke_CI_AS");
            sql.AppendLine(" WHERE (COPTC.TC200 = 'Y')");
            sql.AppendLine(" GROUP BY COPTC.TC004, COPTH.TH001, COPTH.TH002, DT.ShipNo");
            sql.AppendLine(" )");

            //return
            return sql;
        }


        /// <summary>
        /// [出貨明細表](內銷) 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetSQL_ShipLocalData"/>
        private List<SqlParameter> GetParams_ShipLocalData(Dictionary<string, string> search)
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
                        case "sDate":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value.ToDateString("yyyyMMdd")));

                            break;

                        case "eDate":
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value.ToDateString("yyyyMMdd")));

                            break;

                        case "Keyword":
                            sqlParamList.Add(new SqlParameter("@keyword", item.Value));

                            break;

                    }
                }
            }


            return sqlParamList;
        }


        /// <summary>
        /// [出貨明細表](內銷) 取得Excel欄位
        /// 回傳Excel資料
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sheetName"></param>
        public IQueryable<ShipData_LocalItem> GetExcel_ShipNoData(string filePath, string sheetName)
        {
            try
            {
                //----- 宣告 -----
                List<ShipData_LocalItem> dataList = new List<ShipData_LocalItem>();

                //[Excel] - 取得原始資料
                var excelFile = new ExcelQueryFactory(filePath);
                var queryVals = excelFile.Worksheet(sheetName);

                //宣告各內容參數
                string _ShipNo = ""; //物流單號
                double _Freight = 0; //運費

                //資料迴圈
                foreach (var val in queryVals)
                {
                    _ShipNo = val[4];
                    _Freight = Convert.ToDouble(val[19]);

                    //加入項目
                    var data = new ShipData_LocalItem
                    {
                        ShipNo = _ShipNo,
                        Freight = _Freight
                    };

                    //將項目加入至集合
                    dataList.Add(data);

                }

                //回傳集合
                return dataList.AsQueryable();
            }
            catch (Exception ex)
            {

                throw new Exception("請檢查Excel格式是否正確!!" + ex.Message.ToString());
            }
        }

        #endregion *** 出貨明細表(內銷) E ***



        #region *** 出貨明細表(上海) S ***

        /// <summary>
        /// [出貨明細表](上海) 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(TW/SH/Oin1)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipDataSH_Item> GetOneShipData_SH(Dictionary<string, string> search, string dbs, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetShipData_SH(search, 0, 1, out dataCnt, out ErrMsg);
        }

        /// <summary>
        /// [出貨明細表](上海) 所有資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipDataSH_Item> GetAllShipData_SH(Dictionary<string, string> search, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetShipData_SH(search, 0, 9999999, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [出貨明細表](上海) 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipDataSH_Item> GetShipData_SH(Dictionary<string, string> search
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
                List<ShipDataSH_Item> dataList = new List<ShipDataSH_Item>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數
                //string fieldLang = fn_Language.Get_LangCode(lang).Replace("-", "_");  //欄位語系

                //取得SQL語法
                subSql = GetSQL_ShipData_SH(search);
                //取得SQL參數集合
                subParamList = GetParams_ShipData_SH(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" SELECT COUNT(TblERP.Ship_SID) AS TotalCnt");
                    sql.AppendLine(" FROM TblERP");

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

                    //子查詢SQL
                    sql.Append(subSql);

                    sql.AppendLine(" SELECT TblERP.*");
                    sql.AppendLine(" , Base.Data_ID");
                    sql.AppendLine(" , Base.BoxCnt, Base.Pallet, Base.Weight, Base.Cuft, Base.TradeTerms");
                    sql.AppendLine(" , Base.Price1, Base.Price2, Base.Price3, Base.Price4, Base.Price5, Base.Price6, Base.Price7");
                    sql.AppendLine(" , Base.Cost_ExportTax, Base.Cost_Freight");
                    sql.AppendLine(" , Base.Cost_Shipment, Base.Cost_Fee, Base.FWD, Base.Remark");
                    sql.AppendLine(" , RefShip.Class_ID AS ShipID, RefShip.Class_Name_zh_TW AS ShipName");
                    sql.AppendLine(" , RefCheck.Class_ID AS CheckID, RefCheck.Class_Name_zh_TW AS CheckName");
                    sql.AppendLine(" , Base.Create_Who, Base.Create_Time, Base.Update_Who, Base.Update_Time");
                    sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Create_Who)) AS Create_Name");
                    sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Update_Who)) AS Update_Name");

                    sql.AppendLine(" FROM TblERP");
                    sql.AppendLine("  LEFT JOIN Shipment_Data_SH Base ON TblERP.Ship_FID = Base.Ship_FID AND TblERP.Ship_SID = Base.Ship_SID AND TblERP.SO_FID = Base.SO_FID AND TblERP.SO_SID = Base.SO_SID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass_SH RefShip ON Base.ShipID = RefShip.Class_ID");
                    sql.AppendLine("  LEFT JOIN Shipment_RefClass_SH RefCheck ON Base.CheckID = RefCheck.Class_ID");
                    sql.AppendLine(" WHERE (TblERP.RowIdx >= @startRow) AND (TblERP.RowIdx <= @endRow)");
                    sql.AppendLine(" ORDER BY TblERP.RowIdx");

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
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            #region ** 欄位運算 **

                            //取得運算值
                            double _localPrice = item.Field<double>("localPrice");  //(M)
                            double _Price1 = item.Field<double?>("Price1") ?? 0;    //(N)
                            double _Price2 = item.Field<double?>("Price2") ?? 0;    //(O)
                            double _Price3 = item.Field<double?>("Price3") ?? 0;    //(P)
                            double _Price4 = item.Field<double?>("Price4") ?? 0;    //(Q)
                            double _Price5 = item.Field<double?>("Price5") ?? 0;    //(R)
                            double _Price6 = item.Field<double?>("Price6") ?? 0;    //(S)
                            double _Price7 = item.Field<double?>("Price7") ?? 0;    //(T)
                            double _Cost_ExportTax = item.Field<double?>("Cost_ExportTax") ?? 0;    //(V)
                            double _Cost_Freightx = item.Field<double?>("Cost_Freight") ?? 0;    //(W)
                            double _Cost_Shipment = item.Field<double?>("Cost_Shipment") ?? 0;    //(Z)
                            double _Cost_Fee = item.Field<double?>("Cost_Fee") ?? 0;    //(AA)
                            double _Tax = item.Field<double>("Tax");    //(AC)

                            //** 出口費用小計未稅(U) (N + O + P + Q + R + S + T) **
                            double _Cnt_CostExport_NoTax = _Price1 + _Price2 + _Price3 + _Price4 + _Price5 + _Price6 + _Price7;

                            //** 出口費用小計含稅(X) (U + V + W) **
                            double _Cnt_CostExport_Full = _Cnt_CostExport_NoTax + _Cost_ExportTax + _Cost_Freightx;

                            //** 代收費用(AB) (AA * AC) **
                            double _Cnt_CostLocalFee = _Cost_Fee * _Tax;

                            //** 實際出口費用(AD) (X - V - AB) **
                            double _Cnt_CostFullExport = _Cnt_CostExport_Full - _Cost_ExportTax - _Cnt_CostLocalFee;

                            //** 費用比率,含卡車費(AE) (AD / M) * 100 **
                            double _Cnt_CostPercent_NoTruck = (_localPrice > 0) ? (_Cnt_CostFullExport / _localPrice) * 100 : 0;

                            //** 費用比率,不含卡車費(AF) ((AD+Z) / M) * 100 **
                            double _Cnt_CostPercent = (_localPrice > 0) ? ((_Cnt_CostFullExport + _Cost_Shipment) / _localPrice) * 100 : 0;

                            #endregion


                            //加入項目
                            var data = new ShipDataSH_Item
                            {
                                Ship_FID = item.Field<string>("Ship_FID"),
                                Ship_SID = item.Field<string>("Ship_SID"),
                                SO_FID = item.Field<string>("SO_FID"),
                                SO_SID = item.Field<string>("SO_SID"),
                                SO_Date = item.Field<DateTime>("SO_Date").ToString().ToDateString("yyyy/MM/dd"),
                                BoxDate = item.Field<DateTime>("BoxDate").ToString().ToDateString("yyyy/MM/dd"),
                                CustID = item.Field<string>("CustID"),
                                CustName = item.Field<string>("CustName"),
                                InvNo = item.Field<string>("InvNo"),
                                PayTerms = item.Field<string>("PayTerms"),
                                Currency = item.Field<string>("Currency"),
                                Price = item.Field<double>("Price"),
                                localPrice = item.Field<double>("localPrice"),
                                Tax = _Tax,
                                CLS = item.Field<DateTime>("CLS") == null ? "" : item.Field<DateTime>("CLS").ToShortDateString().ToDateString("yyyy/MM/dd"),
                                ETD = item.Field<DateTime>("ETD") == null ? "" : item.Field<DateTime>("ETD").ToShortDateString().ToDateString("yyyy/MM/dd"),
                                ETA = item.Field<DateTime>("ETA") == null ? "" : item.Field<DateTime>("ETA").ToShortDateString().ToDateString("yyyy/MM/dd"),

                                OpcsCnt = item.Field<Int32>("OpcsCnt"),
                                OpcsItemCnt = item.Field<Int32>("OpcsItemCnt"),
                                diffDays = item.Field<Int32>("diffDays"),
                                SalesName = item.Field<string>("SalesName"),

                                //平台維護欄位
                                Data_ID = item.Field<Guid?>("Data_ID"),
                                BoxCnt = item.Field<Int32?>("BoxCnt"),
                                Pallet = item.Field<string>("Pallet"),
                                Weight = item.Field<double?>("Weight"),
                                Cuft = item.Field<double?>("Cuft"),
                                TradeTerms = item.Field<string>("TradeTerms"),

                                Price1 = item.Field<double?>("Price1"),
                                Price2 = item.Field<double?>("Price2"),
                                Price3 = item.Field<double?>("Price3"),
                                Price4 = item.Field<double?>("Price4"),
                                Price5 = item.Field<double?>("Price5"),
                                Price6 = item.Field<double?>("Price6"),
                                Price7 = item.Field<double?>("Price7"),
                                Cost_ExportTax = item.Field<double?>("Cost_ExportTax"),
                                Cost_Freight = item.Field<double?>("Cost_Freight"),
                                Cost_Shipment = item.Field<double?>("Cost_Shipment"),
                                Cost_Fee = item.Field<double?>("Cost_Fee"),

                                Cnt_CostExport_NoTax = _Cnt_CostExport_NoTax,
                                Cnt_CostExport_Full = _Cnt_CostExport_Full,
                                Cnt_CostLocalFee = _Cnt_CostLocalFee,
                                Cnt_CostFullExport = _Cnt_CostFullExport,
                                Cnt_CostPercent_NoTruck = Math.Round(_Cnt_CostPercent_NoTruck, 2),
                                Cnt_CostPercent = Math.Round(_Cnt_CostPercent, 2),

                                FWD = item.Field<string>("FWD"),
                                Remark = item.Field<string>("Remark"),
                                ShipID = item.Field<Int32?>("ShipID") ?? 0,
                                ShipName = item.Field<string>("ShipName"),
                                CheckID = item.Field<Int32?>("CheckID") ?? 0,
                                CheckName = item.Field<string>("CheckName"),

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
        /// [出貨明細表](上海) 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="dbs">資料出處(SH)</param>
        /// <returns></returns>
        /// <see cref="GetShipData_SH"/>
        private StringBuilder GetSQL_ShipData_SH(Dictionary<string, string> search)
        {
            StringBuilder sql = new StringBuilder();
            string dbName = "SHPK2";

            //SQL查詢
            sql.AppendLine(" ;WITH TblDataSource AS (");
            sql.AppendLine("     SELECT Base.TA001, Base.TA002, SaleDT.TH001, SaleDT.TH002, SaleBase.TG003");
            sql.AppendLine("     FROM [##DBName##].dbo.EPSTA AS Base WITH(NOLOCK)");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.EPSTB AS DT WITH(NOLOCK) ON Base.TA001 = DT.TB001 AND Base.TA002 = DT.TB002");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.COPTD AS OrderDT WITH(NOLOCK) ON DT.TB004 = OrderDT.TD001 AND DT.TB005 = OrderDT.TD002 AND DT.TB006 = OrderDT.TD003");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.COPTH AS SaleDT WITH(NOLOCK) ON OrderDT.TD001 = SaleDT.TH014 AND OrderDT.TD002 = SaleDT.TH015 AND OrderDT.TD003 = SaleDT.TH016");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.COPTG AS SaleBase WITH(NOLOCK) ON SaleBase.TG001 = SaleDT.TH001 AND SaleBase.TG002 = SaleDT.TH002 AND SaleBase.TG003 = Base.TA003");
            sql.AppendLine("     WHERE (1=1)");
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
                        case "sDate":
                            sql.Append(" AND (SaleBase.TG003 >= @sDate)");

                            break;

                        case "eDate":
                            sql.Append(" AND (SaleBase.TG003 <= @eDate)");

                            break;
                    }
                }
            }
            #endregion
            sql.AppendLine("     GROUP BY Base.TA001, Base.TA002, SaleDT.TH001, SaleDT.TH002, SaleBase.TG003");
            sql.AppendLine(" )");
            sql.AppendLine(" , TblShip AS (");
            sql.AppendLine("  SELECT");
            sql.AppendLine("   Base.TA001 COLLATE Chinese_Taiwan_Stroke_BIN AS Ship_FID, RTRIM(Base.TA002) COLLATE Chinese_Taiwan_Stroke_BIN AS Ship_SID");
            sql.AppendLine("   , CONVERT(DATE, Base.TA053, 111) AS BoxDate, Base.TA042 AS InvNo");
            sql.AppendLine("   , CONVERT(DATE, Base.TA052, 111) AS CLS, CONVERT(DATE, Base.TA040, 111) AS ETD, CONVERT(DATE, Base.TA039, 111) AS ETA, Base.TA051 AS Ship_NO");
            sql.AppendLine("  FROM [##DBName##].dbo.EPSTA AS Base WITH(NOLOCK)");
            sql.AppendLine("   INNER JOIN TblDataSource ON Base.TA001 = TblDataSource.TA001 AND Base.TA002 = TblDataSource.TA002 AND Base.TA003 = TblDataSource.TG003");
            sql.AppendLine("  GROUP BY Base.TA001, Base.TA002, Base.TA053, Base.TA042, Base.TA052, Base.TA040, Base.TA039, Base.TA051");
            sql.AppendLine(" )");
            sql.AppendLine(" , TblERP AS (");
            sql.AppendLine(" SELECT");
            sql.AppendLine("  TblShip.Ship_FID, TblShip.Ship_SID");
            sql.AppendLine(" , SO.TG001 COLLATE Chinese_Taiwan_Stroke_BIN AS SO_FID, RTRIM(SO.TG002) COLLATE Chinese_Taiwan_Stroke_BIN AS SO_SID");
            sql.AppendLine(" , CONVERT(DATE, SO.TG003, 111) AS SO_Date, TblShip.BoxDate");
            sql.AppendLine(" , RTRIM(Cust.MA001) AS CustID, RTRIM(Cust.MA002) AS CustName");
            sql.AppendLine(" , TblShip.InvNo, Terms.NA003 AS PayTerms");
            sql.AppendLine(" , SO.TG011 AS Currency, CAST(SO.TG013 AS FLOAT) AS Price");
            sql.AppendLine(" , CAST(SO.TG045 AS FLOAT) AS localPrice, CAST(SO.TG012 AS FLOAT) AS Tax");
            sql.AppendLine(" , TblShip.CLS, TblShip.ETD, TblShip.ETA, TblShip.Ship_NO");
            sql.AppendLine(" , (");
            sql.AppendLine("   SELECT COUNT(Tbl.id) FROM (");
            sql.AppendLine("     SELECT COPTD.TD001+COPTD.TD002 AS id");
            sql.AppendLine("     FROM [##DBName##].dbo.COPTD WITH(NOLOCK)");
            sql.AppendLine("         INNER JOIN [##DBName##].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
            sql.AppendLine("     WHERE COPTH.TH001 = SO.TG001 AND COPTH.TH002 = SO.TG002");
            sql.AppendLine("     GROUP BY COPTD.TD001, COPTD.TD002");
            sql.AppendLine("   ) AS Tbl");
            sql.AppendLine(" ) AS OpcsCnt");
            sql.AppendLine(" , (");
            sql.AppendLine("     SELECT COUNT(*)");
            sql.AppendLine("     FROM [##DBName##].dbo.COPTD WITH(NOLOCK)");
            sql.AppendLine("      INNER JOIN [##DBName##].dbo.COPTH WITH(NOLOCK) ON COPTD.TD001 = COPTH.TH014 AND COPTD.TD002 = COPTH.TH015 AND COPTD.TD003 = COPTH.TH016");
            sql.AppendLine("     WHERE COPTH.TH001 = SO.TG001 AND COPTH.TH002 = SO.TG002");
            sql.AppendLine(" ) AS OpcsItemCnt");
            sql.AppendLine(" , DATEDIFF(DD, CAST(SO.TG003 AS date), CAST(TblShip.BoxDate AS date)) AS diffDays");
            sql.AppendLine(" , Emp.MV002 AS SalesName");
            sql.AppendLine(" , ROW_NUMBER() OVER(ORDER BY SO.TG003, TblShip.BoxDate, TblShip.Ship_FID, TblShip.Ship_SID) AS RowIdx");
            sql.AppendLine(" FROM [##DBName##].dbo.COPTG AS SO WITH(NOLOCK)");
            sql.AppendLine("  INNER JOIN TblDataSource ON SO.TG001 = TblDataSource.TH001 AND SO.TG002 = TblDataSource.TH002");
            sql.AppendLine("  INNER JOIN TblShip ON TblDataSource.TA001 = TblShip.Ship_FID AND TblDataSource.TA002 = TblShip.Ship_SID");
            sql.AppendLine("  INNER JOIN [##DBName##].dbo.COPMA AS Cust WITH(NOLOCK) ON SO.TG004 = Cust.MA001");
            sql.AppendLine("  LEFT JOIN [##DBName##].dbo.CMSNA AS Terms WITH(NOLOCK) ON SO.TG047 = Terms.NA002 AND Terms.NA001 = '2'");
            sql.AppendLine("  LEFT JOIN [##DBName##].dbo.CMSMV AS Emp WITH(NOLOCK) ON SO.TG006 = Emp.MV001");
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
                        case "sDate":
                            sql.Append(" AND (SO.TG003 >= @sDate)");

                            break;

                        case "eDate":
                            sql.Append(" AND (SO.TG003 <= @eDate)");

                            break;

                        case "Keyword":
                            //關鍵字
                            sql.Append(" AND (");
                            sql.Append(" UPPER(Cust.MA001) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(Cust.MA002) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(" OR UPPER(TblShip.InvNo) LIKE '%' + UPPER(@keyword) + '%'");
                            sql.Append(")");

                            break;
                    }
                }
            }
            #endregion

            sql.AppendLine(" )");


            //Replace 指定字元
            sql.Replace("##DBName##", dbName);

            //return
            return sql;
        }


        /// <summary>
        /// [出貨明細表](上海) 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetSQL_ShipData"/>
        private List<SqlParameter> GetParams_ShipData_SH(Dictionary<string, string> search)
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
                        case "sDate":
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value.ToDateString("yyyyMMdd")));

                            break;

                        case "eDate":
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value.ToDateString("yyyyMMdd")));

                            break;

                        case "Keyword":
                            sqlParamList.Add(new SqlParameter("@keyword", item.Value));

                            break;

                    }
                }
            }


            return sqlParamList;
        }


        /// <summary>
        /// [出貨明細表](上海) 取得參考類別
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="type"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetRefClass_ShipData_SH(string lang, string type, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name_{0} AS Label"
                    .FormatThis(fn_Language.Get_LangCode(lang).Replace("-", "_")));
                sql.AppendLine(" FROM Shipment_RefClass_SH WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Display = 'Y') AND (Class_Type = @type)");
                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("type", type);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ClassItem
                        {
                            ID = item.Field<int>("ID"),
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


        #endregion *** 出貨明細表(上海) E ***


        #endregion



        #region -----// Create //-----

        #region *** 發貨 S ***

        /// <summary>
        /// 建立發貨資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool CreateShipFreight(ShipFreightItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO ShipFreight( ");
                sql.AppendLine("  Data_ID, CompID, CustID");
                sql.AppendLine("  , ERP_SO_Fid, ERP_SO_Sid, ShipDate");
                sql.AppendLine("  , ShipComp, ShipWay, ShipWho, ShipFrom");
                sql.AppendLine("  , Remark");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @CompID, @CustID");
                sql.AppendLine("  , @ERP_SO_Fid, @ERP_SO_Sid, @ShipDate");
                sql.AppendLine("  , @ShipComp, @ShipWay, @ShipWho, @ShipFrom");
                sql.AppendLine("  , @Remark");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("CompID", instance.CompID);
                cmd.Parameters.AddWithValue("CustID", instance.CustID);
                cmd.Parameters.AddWithValue("ERP_SO_Fid", instance.Erp_SO_FID);
                cmd.Parameters.AddWithValue("ERP_SO_Sid", instance.Erp_SO_SID);
                cmd.Parameters.AddWithValue("ShipDate", instance.ShipDate);
                cmd.Parameters.AddWithValue("ShipComp", instance.ShipComp == null ? (object)DBNull.Value : instance.ShipComp);
                cmd.Parameters.AddWithValue("ShipWay", instance.ShipWay);
                cmd.Parameters.AddWithValue("ShipWho", instance.ShipWho);
                cmd.Parameters.AddWithValue("ShipFrom", instance.StockType);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// 建立物流單號 (發貨)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateShipFreightDetail(ShipFreightDetail instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @NewDataID AS INT");
                sql.AppendLine(" SET @NewDataID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(Data_ID), 0) + 1 FROM ShipFreightDetail WHERE (Parent_ID = @ParentID)");
                sql.AppendLine(" );");
                sql.AppendLine(" INSERT INTO ShipFreightDetail( ");
                sql.AppendLine("  Parent_ID, Data_ID");
                sql.AppendLine("  , ShipNo, ShipCnt, Pay1, Pay2, Pay3");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @ParentID, @NewDataID");
                sql.AppendLine("  , @ShipNo, @ShipCnt, @Pay1, @Pay2, @Pay3");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("ShipNo", instance.ShipNo);
                cmd.Parameters.AddWithValue("ShipCnt", instance.ShipCnt);
                cmd.Parameters.AddWithValue("Pay1", instance.Pay1);
                cmd.Parameters.AddWithValue("Pay2", instance.Pay2);
                cmd.Parameters.AddWithValue("Pay3", instance.Pay3);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// 建立關聯單號 (發貨)(合併運費)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateShipFreightRel(ShipFreightRel instance, ShipFreightItem baseInst, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" IF (SELECT COUNT(*) FROM ShipFreightRel WHERE Rel_ID = @RelID) = 0");
                sql.AppendLine(" BEGIN");
                sql.AppendLine(" DECLARE @NewDataID AS INT");
                sql.AppendLine(" SET @NewDataID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(Data_ID), 0) + 1 FROM ShipFreightRel");
                sql.AppendLine(" );");
                sql.AppendLine(" INSERT INTO ShipFreightRel( ");
                sql.AppendLine("  Parent_ID, Rel_ID, Data_ID");
                sql.AppendLine("  , ERP_SO_Fid, ERP_SO_Sid");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @ParentID, @RelID, @NewDataID");
                sql.AppendLine("  , @Erp_SO_FID, @Erp_SO_SID");
                sql.AppendLine(" );");
                sql.AppendLine(" END");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("RelID", instance.Rel_ID);
                cmd.Parameters.AddWithValue("Erp_SO_FID", instance.Erp_SO_FID);
                cmd.Parameters.AddWithValue("Erp_SO_SID", instance.Erp_SO_SID);

                if (!dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    ErrMsg = "關聯建立失敗." + ErrMsg;
                    return false;
                }
            }

            //自動新增關聯單號的發貨資料
            if (!CreateShipFreight(baseInst, out ErrMsg))
            {
                ErrMsg = "目標單號資料建立失敗." + ErrMsg;
                return false;
            }

            //ok
            return true;
        }


        /// <summary>
        /// 建立貨運公司
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public Int32 CreateShipComp(ShipComp instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //[SQL] - 取得最新編號
                Int32 New_ID;
                sql.AppendLine(" SELECT (ISNULL(MAX(Data_ID), 0) + 1) AS New_ID FROM Logistics");
                cmd.CommandText = sql.ToString();
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    New_ID = Convert.ToInt32(DT.Rows[0]["New_ID"]);
                }

                //[SQL] - 清除參數設定
                cmd.Parameters.Clear();
                sql.Clear();


                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Logistics(");
                sql.AppendLine("  Data_ID, CompID, DisplayName, Display, Sort");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @DataID, @CompID, @DisplayName, @Display, @Sort");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", New_ID);
                cmd.Parameters.AddWithValue("CompID", instance.CompID);
                cmd.Parameters.AddWithValue("DisplayName", instance.Label);
                cmd.Parameters.AddWithValue("Display", instance.Display);
                cmd.Parameters.AddWithValue("Sort", instance.Sort);


                if (!dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    return 0;
                }
                else
                {
                    return New_ID;
                }
            }

        }


        /// <summary>
        /// 建立匯入基本資料 - Step1執行
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool CreateShipImport(ShipImportData instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Ship_ImportData( ");
                sql.AppendLine("  Data_ID, TraceID, Status, Upload_File");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @TraceID, 10, @Upload_File");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("TraceID", instance.TraceID);
                cmd.Parameters.AddWithValue("Upload_File", instance.Upload_File);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// 建立單身, 更新主檔欄位 - Step2執行
        /// </summary>
        /// <param name="baseData">單頭資料(填入ERP區間日期)</param>
        /// <param name="query">單身資料</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateShipImportDT(ShipImportData baseData, IQueryable<ShipImportDataDT> query, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Ship_ImportData_DT WHERE (Parent_ID = @DataID);");
                sql.AppendLine(" UPDATE Ship_ImportData SET Status = 20, Sheet_Name = @Sheet_Name, Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine("  , erpSDate = @erpSDate, erpEDate = @erpEDate");
                sql.AppendLine("  WHERE (Data_ID = @DataID);");

                sql.AppendLine(" DECLARE @NewID AS INT ");

                foreach (var item in query)
                {
                    if (!string.IsNullOrEmpty(item.ShipNo))
                    {
                        sql.AppendLine(" SET @NewID = (");
                        sql.AppendLine("  SELECT ISNULL(MAX(Data_ID), 0) + 1 AS NewID ");
                        sql.AppendLine("  FROM Ship_ImportData_DT ");
                        sql.AppendLine("  WHERE Parent_ID = @DataID ");
                        sql.AppendLine(" )");

                        sql.AppendLine(" INSERT INTO Ship_ImportData_DT( ");
                        sql.AppendLine("  Parent_ID, Data_ID, ShipNo, ShipDate, Qty, Freight");
                        sql.AppendLine(" ) VALUES (");
                        sql.AppendLine("  @DataID, @NewID, '{0}', '{1}', {2}, {3}".FormatThis(
                            item.ShipNo, item.ShipDate, item.Qty, item.Freight));
                        sql.AppendLine(" );");
                    }
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", baseData.Data_ID);
                cmd.Parameters.AddWithValue("Sheet_Name", baseData.Sheet_Name);
                cmd.Parameters.AddWithValue("Update_Who", baseData.Update_Who);
                cmd.Parameters.AddWithValue("erpSDate", baseData.erpSDate);
                cmd.Parameters.AddWithValue("erpEDate", baseData.erpEDate);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }



        #endregion *** 發貨 E ***


        #region *** 客戶返利 S ***

        /// <summary>
        /// 建立客戶返利目標
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCustRebate(CustRebateItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Rebate_Data( ");
                sql.AppendLine("  Data_ID, CompID");
                sql.AppendLine("  , DataYear, CustID, Formula");
                sql.AppendLine("  , RespMoney, RespPercent, FightMoney, FightPercent");
                sql.AppendLine("  , Remark");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @CompID");
                sql.AppendLine("  , @DataYear, @CustID, @Formula");
                sql.AppendLine("  , @RespMoney, @RespPercent, @FightMoney, @FightPercent");
                sql.AppendLine("  , @Remark");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("CompID", instance.CompID);
                cmd.Parameters.AddWithValue("DataYear", instance.DataYear);
                cmd.Parameters.AddWithValue("CustID", instance.CustID);
                cmd.Parameters.AddWithValue("Formula", instance.Formula);
                cmd.Parameters.AddWithValue("RespMoney", instance.Cnt_e);
                cmd.Parameters.AddWithValue("RespPercent", instance.Cnt_f);
                cmd.Parameters.AddWithValue("FightMoney", instance.Cnt_g);
                cmd.Parameters.AddWithValue("FightPercent", instance.Cnt_h);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }

        #endregion *** 客戶返利 E ***


        #region *** 客訴 S ***

        /// <summary>
        /// [開案中客訴] 建立客訴資料 - 開案中的基本資料
        /// </summary>
        /// <param name="instance">CCPTempItem</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCCP_Temp(CCPTempItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Cust_Complaint_Temp( ");
                sql.AppendLine("  Data_ID, CC_Type, TraceID");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @CC_Type, @TraceID");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("CC_Type", instance.CC_Type);
                cmd.Parameters.AddWithValue("TraceID", instance.TraceID);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [開案中客訴] 建立商品資料 (單筆新增)
        /// </summary>
        /// <param name="instance">CCPDetail</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCCP_Detail(CCPDetail instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("DECLARE @NewID AS INT");
                sql.AppendLine(" SET @NewID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM Cust_Complaint_TempDT");
                sql.AppendLine(" )");
                sql.AppendLine(" INSERT INTO Cust_Complaint_TempDT(");
                sql.AppendLine("  Parent_ID, Data_ID");
                sql.AppendLine("  , ModelNo, Qty, Remark, IsSplit, IsWarranty");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Parent_ID, @NewID");
                sql.AppendLine("  , @ModelNo, @Qty, @Remark, @IsSplit, @IsWarranty");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //Update Last updater
                sql.AppendLine(" UPDATE Cust_Complaint_Temp");
                sql.AppendLine(" SET Update_Who = @Create_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Parent_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("ModelNo", instance.ModelNo);
                cmd.Parameters.AddWithValue("Qty", instance.Qty);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("IsSplit", instance.IsSplit);
                cmd.Parameters.AddWithValue("IsWarranty", instance.IsWarranty);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [開案中客訴] 建立商品資料 (多筆匯入)
        /// </summary>
        /// <param name="instance">IQueryable<CCPDetail></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCCP_Detail(IQueryable<CCPDetail> instance, string parentID, string currentWho, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Cust_Complaint_TempDT WHERE (Parent_ID = @Parent_ID)");
                sql.AppendLine(" DECLARE @NewID AS INT");

                foreach (var item in instance)
                {
                    sql.AppendLine(" SET @NewID = (");
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM Cust_Complaint_TempDT");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO Cust_Complaint_TempDT(");
                    sql.AppendLine("  Parent_ID, Data_ID");
                    sql.AppendLine("  , ModelNo, Qty, Remark, IsSplit, IsWarranty");
                    sql.AppendLine("  , Create_Who, Create_Time");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @Parent_ID, @NewID");
                    sql.AppendLine("  , N'{0}', {1}, N'{2}', '{3}', '{4}'".FormatThis(
                         item.ModelNo, item.Qty, item.Remark, item.IsSplit, item.IsWarranty
                        ));
                    sql.AppendLine("  , @Create_Who, GETDATE()");
                    sql.AppendLine(" );");
                }

                //Update Last updater
                sql.AppendLine(" UPDATE Cust_Complaint_Temp");
                sql.AppendLine(" SET Update_Who = @Create_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Parent_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", parentID);
                cmd.Parameters.AddWithValue("Create_Who", currentWho);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [開案中客訴] 建立新客訴 (確認開案)
        /// </summary>
        /// <param name="instance">資料來源</param>
        /// <param name="tempGuid">暫存檔Guid</param>
        /// <param name="type">客訴來源</param>
        /// <param name="nextFlowID">流程狀態</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCCP_Invoke(List<CCPItem> instance, string tempGuid, string type, string nextFlowID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @GetToday AS VARCHAR(6), @GetNewID AS INT, @GetNewDtID AS INT");
                sql.AppendLine(" , @GetFullID AS VARCHAR(20), @GetGuid AS uniqueidentifier");

                for (int row = 0; row < instance.Count; row++)
                {
                    //[自訂編號]取得前置代碼(日期)(FstID)
                    sql.AppendLine(" SET @GetToday = RIGHT(CONVERT(VARCHAR(10), GETDATE(), 112), 6)");

                    //[自訂編號]取得新序號(SecID)
                    sql.AppendLine(" SET @GetNewID = (");
                    sql.AppendLine("     SELECT ISNULL(MAX(SecID), 0) + 1");
                    sql.AppendLine("     FROM Cust_Complaint");
                    sql.AppendLine("     WHERE (FstID = @GetToday) AND (CC_Type = @CCType)");
                    sql.AppendLine(" )");
                    //[自訂編號]設定完整編號(CC_UID)
                    sql.AppendLine(" SET @GetFullID = CAST(@CCType AS VARCHAR(2)) + @GetToday + RIGHT('00' + CAST(@GetNewID AS VARCHAR(3)), 3)");
                    //設定系統編號(Data_ID)
                    sql.AppendLine(" SET @GetGuid = (SELECT NEWID())");

                    //建立新客訴
                    sql.AppendLine(" INSERT INTO Cust_Complaint (");
                    sql.AppendLine("  Data_ID, TraceID, CC_Type, CC_UID, FstID, SecID");
                    sql.AppendLine("  , PlanType, CustType, RefCustID, RefMallID");
                    sql.AppendLine("  , ModelNo, Qty, Remark, IsWarranty, FlowStatus");
                    sql.AppendLine("  , Create_Who, Create_Time");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @GetGuid, @TraceID, @CCType, @GetFullID, @GetToday, @GetNewID");
                    sql.AppendLine("  , @PlanType, @CustType, @RefCustID, @RefMallID");
                    sql.AppendLine("  , @ModelNo_{0}, @Qty_{0}, @Remark_{0}, @IsWarranty_{0}, @FlowStatus".FormatThis(row));
                    sql.AppendLine("  , @Create_Who, GETDATE()");
                    sql.AppendLine(" );");

                    //[UserFlow] 建立Log
                    sql.AppendLine(" INSERT INTO Cust_Complaint_Log (");
                    sql.AppendLine("  Data_ID, Parent_ID, LogSubject, LogDesc, FlowID");
                    sql.AppendLine("  , Create_Who, Create_Time");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @GetGuid, @GetGuid, '開案', '開案:' + @GetFullID, 0");
                    sql.AppendLine("  , @Create_Who, GETDATE()");
                    sql.AppendLine(" );");

                    cmd.Parameters.AddWithValue("ModelNo_" + row, instance[row].ModelNo);
                    cmd.Parameters.AddWithValue("Qty_" + row, instance[row].Qty);
                    cmd.Parameters.AddWithValue("Remark_" + row, instance[row].Remark);
                    cmd.Parameters.AddWithValue("IsWarranty_" + row, instance[row].IsWarranty);

                }

                //將開案中客訴,設為已開案
                sql.AppendLine(" UPDATE Cust_Complaint_Temp");
                sql.AppendLine(" SET IsInvoke = 'Y', Invoke_Who = @Create_Who, Invoke_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @tempGuid);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("tempGuid", tempGuid);
                cmd.Parameters.AddWithValue("CCType", Convert.ToInt16(type));
                cmd.Parameters.AddWithValue("TraceID", instance[0].TraceID);
                cmd.Parameters.AddWithValue("PlanType", instance[0].PlanType);
                cmd.Parameters.AddWithValue("CustType", instance[0].CustType);
                cmd.Parameters.AddWithValue("RefCustID", instance[0].RefCustID);
                cmd.Parameters.AddWithValue("RefMallID", instance[0].RefMallID);
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);
                cmd.Parameters.AddWithValue("FlowStatus", nextFlowID);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [開案中客訴] 上傳圖片
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCCPTemp_Attachment(List<CCPAttachment> instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("DECLARE @NewID AS INT");

                for (int row = 0; row < instance.Count; row++)
                {
                    sql.AppendLine(" SET @NewID = (");
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM Cust_Complaint_TempAttachment");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO Cust_Complaint_TempAttachment(");
                    sql.AppendLine("  Parent_ID, Data_ID, AttachFile, AttachFile_Org");
                    sql.AppendLine("  , Create_Who, Create_Time");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @Parent_ID, @NewID, @AttachFile_{0}, @AttachFile_Org_{0}".FormatThis(row));
                    sql.AppendLine("  , @Create_Who, GETDATE()");
                    sql.AppendLine(" );");

                    cmd.Parameters.AddWithValue("AttachFile_{0}".FormatThis(row), instance[row].AttachFile);
                    cmd.Parameters.AddWithValue("AttachFile_Org_{0}".FormatThis(row), instance[row].AttachFile_Org);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance[0].Parent_ID);
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }

        /// <summary>
        /// [已開案客訴] 上傳附件
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCCP_Attachment(List<CCPAttachment> instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("DECLARE @NewID AS INT");

                for (int row = 0; row < instance.Count; row++)
                {
                    sql.AppendLine(" SET @NewID = (");
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM Cust_Complaint_Attachment");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO Cust_Complaint_Attachment(");
                    sql.AppendLine("  Parent_ID, Data_ID, FlowID, AttachFile, AttachFile_Org");
                    sql.AppendLine("  , Create_Who, Create_Time");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @Parent_ID, @NewID, @FlowID, @AttachFile_{0}, @AttachFile_Org_{0}".FormatThis(row));
                    sql.AppendLine("  , @Create_Who, GETDATE()");
                    sql.AppendLine(" );");

                    cmd.Parameters.AddWithValue("AttachFile_{0}".FormatThis(row), instance[row].AttachFile);
                    cmd.Parameters.AddWithValue("AttachFile_Org_{0}".FormatThis(row), instance[row].AttachFile_Org);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance[0].Parent_ID);
                cmd.Parameters.AddWithValue("FlowID", instance[0].FlowID);
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [客訴] 建立客訴Log
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCCP_Log(CCPLog instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @GetGuid AS uniqueidentifier");
                sql.AppendLine(" SET @GetGuid = (SELECT NEWID())");
                sql.AppendLine(" INSERT INTO Cust_Complaint_Log (");
                sql.AppendLine("  Data_ID, Parent_ID, LogType, LogSubject, LogDesc, FlowID");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @GetGuid, @Parent_ID, @LogType, @LogSubject, @LogDesc, @FlowID");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" )");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("LogType", instance.LogType);
                cmd.Parameters.AddWithValue("LogSubject", instance.LogSubject);
                cmd.Parameters.AddWithValue("LogDesc", instance.LogDesc);
                cmd.Parameters.AddWithValue("FlowID", instance.FlowID);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// [客訴] 通知名單
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCCP_Inform(CCPInform instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @NewID AS INT");
                sql.AppendLine(" SET @NewID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM Cust_Complaint_InformFlow");
                sql.AppendLine(" )");
                sql.AppendLine(" INSERT INTO Cust_Complaint_InformFlow( ");
                sql.AppendLine("  Data_ID, CC_Type, FlowID, Who, Email");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @NewID, @CC_Type, @FlowID, @Who, @Email");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("CC_Type", instance.CC_Type);
                cmd.Parameters.AddWithValue("FlowID", instance.FlowID);
                cmd.Parameters.AddWithValue("Who", instance.Who);
                cmd.Parameters.AddWithValue("Email", instance.Email);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }

        #endregion *** 客訴 E ***


        #region *** 電商平台數據 S ***

        /// <summary>
        /// [電商平台數據] 判斷是否重複新增 - Year
        /// </summary>
        /// <param name="instance">ECDItem_Year</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public Int32 CheckECD_Year(ECDItem_Year instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT COUNT(*) AS Cnt FROM SZ_eCommerce_setYear");
                sql.AppendLine(" WHERE (RefType = @RefType) AND (RefMall = @RefMall) AND (setYear = @setYear)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefType", instance.RefType);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setYear", instance.setYear);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Report, out ErrMsg))
                {
                    return Convert.ToInt32(DT.Rows[0]["Cnt"]);
                }

            }
        }

        /// <summary>
        /// [電商平台數據] 判斷是否重複新增 - Month
        /// </summary>
        /// <param name="instance">ECDItem_Month</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public Int32 CheckECD_Month(ECDItem_Month instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT COUNT(*) AS Cnt FROM SZ_eCommerce_setMonth");
                sql.AppendLine(" WHERE (RefType = @RefType) AND (RefMall = @RefMall) AND (setYear = @setYear) AND (setMonth = @setMonth)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefType", instance.RefType);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setYear", instance.setYear);
                cmd.Parameters.AddWithValue("setMonth", instance.setMonth);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Report, out ErrMsg))
                {
                    return Convert.ToInt32(DT.Rows[0]["Cnt"]);
                }

            }
        }

        /// <summary>
        /// [電商平台數據] 判斷是否重複新增 - Date
        /// </summary>
        /// <param name="instance">ECDItem_Date</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public Int32 CheckECD_Date(ECDItem_Date instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT COUNT(*) AS Cnt FROM SZ_eCommerce_setDate");
                sql.AppendLine(" WHERE (RefType = @RefType) AND (RefMall = @RefMall) AND (setDate = @setDate)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefType", instance.RefType);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setDate", instance.setDate);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Report, out ErrMsg))
                {
                    return Convert.ToInt32(DT.Rows[0]["Cnt"]);
                }

            }
        }


        /// <summary>
        /// [電商平台數據] 建立資料 - Year
        /// </summary>
        /// <param name="instance">ECDItem_Year</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateECD_Year(ECDItem_Year instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO SZ_eCommerce_setYear( ");
                sql.AppendLine("  Data_ID, RefType, RefMall, setYear");
                sql.AppendLine("  , Price_Sales, Price_Rebate");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @RefType, @RefMall, @setYear");
                sql.AppendLine("  , @Price_Sales, @Price_Rebate");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefType", instance.RefType);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setYear", instance.setYear);
                cmd.Parameters.AddWithValue("Price_Sales", instance.Price_Sales);
                cmd.Parameters.AddWithValue("Price_Rebate", instance.Price_Rebate);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }

        }

        /// <summary>
        /// [電商平台數據] 建立資料 - Month
        /// </summary>
        /// <param name="instance">ECDItem_Month</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateECD_Month(ECDItem_Month instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO SZ_eCommerce_setMonth( ");
                sql.AppendLine("  Data_ID, RefType, RefMall, setYear, setMonth");
                sql.AppendLine("  , Price_Income, Price_SalesRebate, Price_Cost, Price_Profit");
                sql.AppendLine("  , Price_Purchase, Price_Back, Price_PurchaseRebate, Price_Freight");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @RefType, @RefMall, @setYear, @setMonth");
                sql.AppendLine("  , @Price_Income, @Price_SalesRebate, @Price_Cost, @Price_Profit");
                sql.AppendLine("  , @Price_Purchase, @Price_Back, @Price_PurchaseRebate, @Price_Freight");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefType", instance.RefType);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setYear", instance.setYear);
                cmd.Parameters.AddWithValue("setMonth", instance.setMonth);
                cmd.Parameters.AddWithValue("Price_Income", instance.Price_Income);
                cmd.Parameters.AddWithValue("Price_SalesRebate", instance.Price_SalesRebate);
                cmd.Parameters.AddWithValue("Price_Cost", instance.Price_Cost);
                cmd.Parameters.AddWithValue("Price_Profit", instance.Price_Profit);
                cmd.Parameters.AddWithValue("Price_Purchase", instance.Price_Purchase);
                cmd.Parameters.AddWithValue("Price_Back", instance.Price_Back);
                cmd.Parameters.AddWithValue("Price_PurchaseRebate", instance.Price_PurchaseRebate);
                cmd.Parameters.AddWithValue("Price_Freight", instance.Price_Freight);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }

        }

        /// <summary>
        /// [電商平台數據] 建立資料 - Date
        /// </summary>
        /// <param name="instance">ECDItem_Year</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateECD_Date(ECDItem_Date instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO SZ_eCommerce_setDate( ");
                sql.AppendLine("  Data_ID, RefType, RefMall, setDate");
                sql.AppendLine("  , Price_RefSales, Price_RefProfit, Price_RealSales, Price_RealProfit");
                sql.AppendLine("  , Price_OrderPrice, Price_ROI, Price_ClickCost");
                sql.AppendLine("  , Price_Adv1, Price_Adv2, Price_Adv3");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @RefType, @RefMall, @setDate");
                sql.AppendLine("  , @Price_RefSales, @Price_RefProfit, @Price_RealSales, @Price_RealProfit");
                sql.AppendLine("  , @Price_OrderPrice, @Price_ROI, @Price_ClickCost");
                sql.AppendLine("  , @Price_Adv1, @Price_Adv2, @Price_Adv3");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefType", instance.RefType);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setDate", instance.setDate);
                cmd.Parameters.AddWithValue("Price_RefSales", instance.Price_RefSales);
                cmd.Parameters.AddWithValue("Price_RefProfit", instance.Price_RefProfit);
                cmd.Parameters.AddWithValue("Price_RealSales", instance.Price_RealSales);
                cmd.Parameters.AddWithValue("Price_RealProfit", instance.Price_RealProfit);
                cmd.Parameters.AddWithValue("Price_OrderPrice", instance.Price_OrderPrice);
                cmd.Parameters.AddWithValue("Price_ROI", instance.Price_ROI);
                cmd.Parameters.AddWithValue("Price_ClickCost", instance.Price_ClickCost);
                cmd.Parameters.AddWithValue("Price_Adv1", instance.Price_Adv1);
                cmd.Parameters.AddWithValue("Price_Adv2", instance.Price_Adv2);
                cmd.Parameters.AddWithValue("Price_Adv3", instance.Price_Adv3);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }

        }


        /// <summary>
        /// [電商平台數據] 建立單身資料 - Month
        /// </summary>
        /// <param name="instance">ECDItem_Month</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateECD_MonthDT(ECDItem_MonthDT instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("DECLARE @NewID AS INT");
                sql.AppendLine(" SET @NewID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM SZ_eCommerce_PromoDT");
                sql.AppendLine(" )");
                sql.AppendLine(" INSERT INTO SZ_eCommerce_PromoDT( ");
                sql.AppendLine("  Data_ID, Parent_ID");
                sql.AppendLine("  , RecordDate, RecordType, RecordMoney");
                sql.AppendLine("  , CheckDate, CheckMoney, CheckInvoiceDate");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @NewID, @Parent_ID");
                sql.AppendLine("  , @RecordDate, @RecordType, @RecordMoney");
                sql.AppendLine("  , @CheckDate, @CheckMoney, @CheckInvoiceDate");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                /* Update 單頭 Price_Promo欄位 */
                sql.AppendLine(" UPDATE SZ_eCommerce_setMonth");
                sql.AppendLine(" SET Price_Promo =");
                sql.AppendLine(" (");
                sql.AppendLine("   SELECT ISNULL(SUM(RecordMoney), 0)");
                sql.AppendLine("   FROM SZ_eCommerce_PromoDT");
                sql.AppendLine("   WHERE (Parent_ID = @Parent_ID)");
                sql.AppendLine(" )");
                sql.AppendLine(" WHERE (Data_ID = @Parent_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("RecordDate", instance.RecordDate);
                cmd.Parameters.AddWithValue("RecordType", instance.RecordType);
                cmd.Parameters.AddWithValue("RecordMoney", instance.RecordMoney);
                cmd.Parameters.AddWithValue("CheckDate", string.IsNullOrWhiteSpace(instance.CheckDate) ? (object)DBNull.Value : instance.CheckDate);
                cmd.Parameters.AddWithValue("CheckMoney", instance.CheckMoney);
                cmd.Parameters.AddWithValue("CheckInvoiceDate", string.IsNullOrWhiteSpace(instance.CheckInvoiceDate) ? (object)DBNull.Value : instance.CheckInvoiceDate);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }

        }


        /// <summary>
        /// [電商平台數據] 建立價格資料
        /// </summary>
        /// <param name="instance">ECDItem_PriceList</param>
        /// <param name="typeID">1工具;2玩具</param>
        /// <param name="mallID">Mall</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateECD_PriceList(IQueryable<ECDItem_PriceList> instance, Int64 typeID, Int32 mallID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM SZ_Prod_PriceList WHERE (RefType = @RefType) AND (RefMall = @RefMall)");
                sql.AppendLine(" DECLARE @NewID AS INT");

                foreach (var item in instance)
                {
                    sql.AppendLine(" SET @NewID = (");
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM SZ_Prod_PriceList");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO SZ_Prod_PriceList(");
                    sql.AppendLine("  Data_ID, RefType, RefMall");
                    sql.AppendLine("  , ProdID, ListPrice");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @NewID, @RefType, @RefMall");
                    sql.AppendLine("  , N'{0}', {1}".FormatThis(item.ProdID, item.Price1));
                    sql.AppendLine(" );");
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("RefType", typeID);
                cmd.Parameters.AddWithValue("RefMall", mallID);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }

        }

        #endregion *** 電商平台數據 E ***


        #region *** 出貨明細表(外銷) S ***
        /// <summary>
        /// [出貨明細表] 建立 & 更新出貨資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Check_ShipData(List<ShipData_Item> instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                for (int row = 0; row < instance.Count; row++)
                {
                    var item = instance[row];

                    sql.AppendLine("IF (SELECT COUNT(*) FROM Shipment_Data WHERE (Data_ID = @Data_ID_#idx#)) > 0");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    UPDATE Shipment_Data");
                    sql.AppendLine("    SET BoxCnt = @BoxCnt_#idx#, Pallet = @Pallet_#idx#, [Weight] = @Weight_#idx#, Cuft = @Cuft_#idx#, TradeTerms = @TradeTerms_#idx#");
                    sql.AppendLine("        , Cost_Customs = @Cost_Customs_#idx#, Cost_LocalCharge = @Cost_LocalCharge_#idx#, Cost_Cert = @Cost_Cert_#idx#, Cost_Freight = @Cost_Freight_#idx#, Cost_Business = @Cost_Business_#idx#");
                    sql.AppendLine("        , ShipID = @ShipID_#idx#, Cost_Shipment = @Cost_Shipment_#idx#, Cost_Fee = @Cost_Fee_#idx#, FWD = @FWD_#idx#");
                    sql.AppendLine("        , PlaceID = @PlaceID_#idx#, Cost_Trade = @Cost_Trade_#idx#, Cost_Service = @Cost_Service_#idx#, Cost_Use = @Cost_Use_#idx#");
                    sql.AppendLine("        , CheckID = @CheckID_#idx#, Remark = @Remark_#idx#, TrackingNo = @TrackingNo_#idx#, Update_Who = @Create_Who, Update_Time = GETDATE()");
                    sql.AppendLine("    WHERE (Data_ID = @Data_ID_#idx#)");
                    sql.AppendLine("  END");
                    sql.AppendLine(" ELSE");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    INSERT INTO Shipment_Data (");
                    sql.AppendLine("        Data_ID, Ship_FID, Ship_SID, SO_FID, SO_SID");
                    sql.AppendLine("        , BoxCnt, Pallet, [Weight], Cuft, TradeTerms");
                    sql.AppendLine("        , Cost_Customs, Cost_LocalCharge, Cost_Cert, Cost_Freight, Cost_Business");
                    sql.AppendLine("        , ShipID, Cost_Shipment, Cost_Fee, FWD");
                    sql.AppendLine("        , PlaceID, Cost_Trade, Cost_Service, Cost_Use");
                    sql.AppendLine("        , CheckID, Remark, TrackingNo, Create_Who");
                    sql.AppendLine("    ) VALUES (");
                    sql.AppendLine("        @Data_ID_#idx#, @Ship_FID_#idx#, @Ship_SID_#idx#, @SO_FID_#idx#, @SO_SID_#idx#");
                    sql.AppendLine("        , @BoxCnt_#idx#, @Pallet_#idx#, @Weight_#idx#, @Cuft_#idx#, @TradeTerms_#idx#");
                    sql.AppendLine("        , @Cost_Customs_#idx#, @Cost_LocalCharge_#idx#, @Cost_Cert_#idx#, @Cost_Freight_#idx#, @Cost_Business_#idx#");
                    sql.AppendLine("        , @ShipID_#idx#, @Cost_Shipment_#idx#, @Cost_Fee_#idx#, @FWD_#idx#");
                    sql.AppendLine("        , @PlaceID_#idx#, @Cost_Trade_#idx#, @Cost_Service_#idx#, @Cost_Use_#idx#");
                    sql.AppendLine("        , @CheckID_#idx#, @Remark_#idx#, @TrackingNo_#idx#, @Create_Who");
                    sql.AppendLine("    )");
                    sql.AppendLine("  END");

                    //replace idx number
                    sql.Replace("#idx#", row.ToString());

                    //add params
                    cmd.Parameters.AddWithValue("Data_ID_" + row, item.Data_ID);
                    cmd.Parameters.AddWithValue("Ship_FID_" + row, item.Ship_FID);
                    cmd.Parameters.AddWithValue("Ship_SID_" + row, item.Ship_SID);
                    cmd.Parameters.AddWithValue("SO_FID_" + row, item.SO_FID);
                    cmd.Parameters.AddWithValue("SO_SID_" + row, item.SO_SID);
                    cmd.Parameters.AddWithValue("BoxCnt_" + row, item.BoxCnt);
                    cmd.Parameters.AddWithValue("Pallet_" + row, item.Pallet);
                    cmd.Parameters.AddWithValue("Weight_" + row, item.Weight);
                    cmd.Parameters.AddWithValue("Cuft_" + row, item.Cuft);
                    cmd.Parameters.AddWithValue("TradeTerms_" + row, item.TradeTerms);
                    cmd.Parameters.AddWithValue("Cost_Customs_" + row, item.Cost_Customs);
                    cmd.Parameters.AddWithValue("Cost_LocalCharge_" + row, item.Cost_LocalCharge);
                    cmd.Parameters.AddWithValue("Cost_Cert_" + row, item.Cost_Cert);
                    cmd.Parameters.AddWithValue("Cost_Freight_" + row, item.Cost_Freight);
                    cmd.Parameters.AddWithValue("Cost_Business_" + row, item.Cost_Business);
                    cmd.Parameters.AddWithValue("ShipID_" + row, item.ShipID.Equals(0) ? DBNull.Value : (object)item.ShipID);
                    cmd.Parameters.AddWithValue("Cost_Shipment_" + row, item.Cost_Shipment);
                    cmd.Parameters.AddWithValue("Cost_Fee_" + row, item.Cost_Fee);
                    cmd.Parameters.AddWithValue("FWD_" + row, item.FWD);
                    cmd.Parameters.AddWithValue("PlaceID_" + row, item.PlaceID.Equals(0) ? DBNull.Value : (object)item.PlaceID);
                    cmd.Parameters.AddWithValue("Cost_Trade_" + row, item.Cost_Trade);
                    cmd.Parameters.AddWithValue("Cost_Service_" + row, item.Cost_Service);
                    cmd.Parameters.AddWithValue("Cost_Use_" + row, item.Cost_Use);
                    cmd.Parameters.AddWithValue("CheckID_" + row, item.CheckID.Equals(0) ? DBNull.Value : (object)item.CheckID);
                    cmd.Parameters.AddWithValue("Remark_" + row, item.Remark);
                    cmd.Parameters.AddWithValue("TrackingNo_" + row, item.TrackingNo);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }


        }


        #endregion *** 出貨明細表(外銷) E ***


        #region *** 出貨明細表(進口) S ***
        /// <summary>
        /// [出貨明細表](進口) 建立 & 更新資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Check_CustomsData(List<CustomsData_Item> instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                for (int row = 0; row < instance.Count; row++)
                {
                    var item = instance[row];

                    sql.AppendLine("IF (SELECT COUNT(*) FROM Customs_Data WHERE (Data_ID = @Data_ID_#idx#)) > 0");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    UPDATE Customs_Data");
                    sql.AppendLine("    SET Cost_Customs = @Cost_Customs_#idx#, Cost_LocalCharge = @Cost_LocalCharge_#idx#, Cost_LocalBusiness = @Cost_LocalBusiness_#idx#");
                    sql.AppendLine("        , Cost_Imports = @Cost_Imports_#idx#, Cost_Trade = @Cost_Trade_#idx#, Cost_ImportsBusiness = @Cost_ImportsBusiness_#idx#");
                    sql.AppendLine("        , Cost_Service = @Cost_Service_#idx#, Cost_Truck = @Cost_Truck_#idx#");
                    sql.AppendLine("        , Remark = @Remark_#idx#, Update_Who = @Create_Who, Update_Time = GETDATE()");
                    sql.AppendLine("    WHERE (Data_ID = @Data_ID_#idx#)");
                    sql.AppendLine("  END");
                    sql.AppendLine(" ELSE");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    INSERT INTO Customs_Data (");
                    sql.AppendLine("        Data_ID, Redeem_FID, Redeem_SID");
                    sql.AppendLine("        , Cost_Customs, Cost_LocalCharge, Cost_LocalBusiness");
                    sql.AppendLine("        , Cost_Imports, Cost_Trade, Cost_ImportsBusiness");
                    sql.AppendLine("        , Cost_Service, Cost_Truck");
                    sql.AppendLine("        , Remark, Create_Who");
                    sql.AppendLine("    ) VALUES (");
                    sql.AppendLine("        @Data_ID_#idx#, @Redeem_FID_#idx#, @Redeem_SID_#idx#");
                    sql.AppendLine("        , @Cost_Customs_#idx#, @Cost_LocalCharge_#idx#, @Cost_LocalBusiness_#idx#");
                    sql.AppendLine("        , @Cost_Imports_#idx#, @Cost_Trade_#idx#, @Cost_ImportsBusiness_#idx#");
                    sql.AppendLine("        , @Cost_Service_#idx#, @Cost_Truck_#idx#");
                    sql.AppendLine("        , @Remark_#idx#, @Create_Who");
                    sql.AppendLine("    )");
                    sql.AppendLine("  END");

                    //replace idx number
                    sql.Replace("#idx#", row.ToString());

                    //add params
                    cmd.Parameters.AddWithValue("Data_ID_" + row, item.Data_ID);
                    cmd.Parameters.AddWithValue("Redeem_FID_" + row, item.Redeem_FID);
                    cmd.Parameters.AddWithValue("Redeem_SID_" + row, item.Redeem_SID);
                    cmd.Parameters.AddWithValue("Cost_Customs_" + row, item.Cost_Customs);
                    cmd.Parameters.AddWithValue("Cost_LocalCharge_" + row, item.Cost_LocalCharge);
                    cmd.Parameters.AddWithValue("Cost_LocalBusiness_" + row, item.Cost_LocalBusiness);
                    cmd.Parameters.AddWithValue("Cost_Imports_" + row, item.Cost_Imports);
                    cmd.Parameters.AddWithValue("Cost_Trade_" + row, item.Cost_Trade);
                    cmd.Parameters.AddWithValue("Cost_ImportsBusiness_" + row, item.Cost_ImportsBusiness);
                    cmd.Parameters.AddWithValue("Cost_Service_" + row, item.Cost_Service);
                    cmd.Parameters.AddWithValue("Cost_Truck_" + row, item.Cost_Truck);
                    cmd.Parameters.AddWithValue("Remark_" + row, item.Remark);

                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }


        }


        #endregion *** 出貨明細表(進口) E ***


        #region *** 出貨明細表(內銷) S ***
        /// <summary>
        /// [出貨明細表][內銷] 建立 & 更新出貨資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Check_ShipLocalData(List<ShipData_LocalItem> instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                for (int row = 0; row < instance.Count; row++)
                {
                    var item = instance[row];

                    sql.AppendLine("IF (SELECT COUNT(*) FROM Shipment_Local_Data WHERE (Data_ID = @Data_ID_#idx#)) > 0");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    UPDATE Shipment_Local_Data");
                    sql.AppendLine("    SET CustType = @CustType_#idx#, ProdType = @ProdType_#idx#, BoxCnt = @BoxCnt_#idx#");
                    sql.AppendLine("        , ShipID = @ShipID_#idx#, ShipNo = @ShipNo_#idx#, Freight = @Freight_#idx#");
                    sql.AppendLine("        , SendType = @SendType_#idx#, SendNo = @SendNo_#idx#, Remark = @Remark_#idx#");
                    sql.AppendLine("        , Update_Who = @Create_Who, Update_Time = GETDATE()");
                    sql.AppendLine("    WHERE (Data_ID = @Data_ID_#idx#)");
                    sql.AppendLine("  END");
                    sql.AppendLine(" ELSE");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    INSERT INTO Shipment_Local_Data (");
                    sql.AppendLine("        Data_ID, SO_FID, SO_SID");
                    sql.AppendLine("        , CustType, ProdType, BoxCnt");
                    sql.AppendLine("        , ShipID, ShipNo, Freight");
                    sql.AppendLine("        , SendType, SendNo, Remark");
                    sql.AppendLine("        , Create_Who");
                    sql.AppendLine("    ) VALUES (");
                    sql.AppendLine("        @Data_ID_#idx#, @SO_FID_#idx#, @SO_SID_#idx#");
                    sql.AppendLine("        , @CustType_#idx#, @ProdType_#idx#, @BoxCnt_#idx#");
                    sql.AppendLine("        , @ShipID_#idx#, @ShipNo_#idx#, @Freight_#idx#");
                    sql.AppendLine("        , @SendType_#idx#, @SendNo_#idx#, @Remark_#idx#");
                    sql.AppendLine("        , @Create_Who");
                    sql.AppendLine("    )");
                    sql.AppendLine("  END");

                    //replace idx number
                    sql.Replace("#idx#", row.ToString());

                    //add params
                    cmd.Parameters.AddWithValue("Data_ID_" + row, item.Data_ID);
                    cmd.Parameters.AddWithValue("SO_FID_" + row, item.SO_FID);
                    cmd.Parameters.AddWithValue("SO_SID_" + row, item.SO_SID);
                    cmd.Parameters.AddWithValue("CustType_" + row, item.CustType.Equals(0) ? DBNull.Value : (object)item.CustType);
                    cmd.Parameters.AddWithValue("ProdType_" + row, item.ProdType.Equals(0) ? DBNull.Value : (object)item.ProdType);
                    cmd.Parameters.AddWithValue("BoxCnt_" + row, item.BoxCnt);
                    cmd.Parameters.AddWithValue("ShipID_" + row, item.ShipID.Equals(0) ? DBNull.Value : (object)item.ShipID);
                    cmd.Parameters.AddWithValue("ShipNo_" + row, item.ShipNo);
                    cmd.Parameters.AddWithValue("Freight_" + row, item.Freight);
                    cmd.Parameters.AddWithValue("SendType_" + row, item.SendType.Equals(0) ? DBNull.Value : (object)item.SendType);
                    cmd.Parameters.AddWithValue("SendNo_" + row, item.SendNo);
                    cmd.Parameters.AddWithValue("Remark_" + row, item.Remark);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }


        }


        /// <summary>
        /// 回寫物流單號, 運費
        /// </summary>
        /// <param name="instance">excel來源資料</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Check_ShipLocalData_Freight(IQueryable<ShipData_LocalItem> instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                var _data = instance
                    .Where(el => !el.ShipNo.Equals(""));

                foreach (var item in _data)
                {
                    sql.AppendLine("IF (SELECT COUNT(*) FROM Shipment_Local_Freight WHERE (ShipNo = N'{0}')) > 0".FormatThis(item.ShipNo));
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    UPDATE Shipment_Local_Freight SET Freight = {0} WHERE (ShipNo = N'{1}')".FormatThis(
                        item.Freight, item.ShipNo));
                    sql.AppendLine("  END");
                    sql.AppendLine(" ELSE");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    INSERT INTO Shipment_Local_Freight (Freight, ShipNo)");
                    sql.AppendLine("    VALUES ({0}, N'{1}')".FormatThis(item.Freight, item.ShipNo));
                    sql.AppendLine("  END");

                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }

        }

        #endregion *** 出貨明細表(內銷) E ***


        #region *** 出貨明細表(上海) S ***
        /// <summary>
        /// [出貨明細表] 建立 & 更新出貨資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Check_ShipData_SH(List<ShipDataSH_Item> instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                for (int row = 0; row < instance.Count; row++)
                {
                    var item = instance[row];

                    sql.AppendLine("IF (SELECT COUNT(*) FROM Shipment_Data_SH WHERE (Data_ID = @Data_ID_#idx#)) > 0");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    UPDATE Shipment_Data_SH");
                    sql.AppendLine("    SET BoxCnt = @BoxCnt_#idx#, Pallet = @Pallet_#idx#, [Weight] = @Weight_#idx#, Cuft = @Cuft_#idx#, TradeTerms = @TradeTerms_#idx#");
                    sql.AppendLine("        , Price1 = @Price1_#idx#, Price2 = @Price2_#idx#, Price3 = @Price3_#idx#, Price4 = @Price4_#idx#");
                    sql.AppendLine("        , Price5 = @Price5_#idx#, Price6 = @Price6_#idx#, Price7 = @Price7_#idx#");
                    sql.AppendLine("        , Cost_ExportTax = @Cost_ExportTax_#idx#, Cost_Freight = @Cost_Freight_#idx#, Cost_Shipment = @Cost_Shipment_#idx#, Cost_Fee = @Cost_Fee_#idx#, FWD = @FWD_#idx#");
                    sql.AppendLine("        , ShipID = @ShipID_#idx#, CheckID = @CheckID_#idx#, Remark = @Remark_#idx#, Update_Who = @Create_Who, Update_Time = GETDATE()");
                    sql.AppendLine("    WHERE (Data_ID = @Data_ID_#idx#)");
                    sql.AppendLine("  END");
                    sql.AppendLine(" ELSE");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("    INSERT INTO Shipment_Data_SH (");
                    sql.AppendLine("        Data_ID, Ship_FID, Ship_SID, SO_FID, SO_SID");
                    sql.AppendLine("        , BoxCnt, Pallet, [Weight], Cuft, TradeTerms");
                    sql.AppendLine("        , Price1, Price2, Price3, Price4");
                    sql.AppendLine("        , Price5, Price6, Price7");
                    sql.AppendLine("        , Cost_ExportTax, Cost_Freight, Cost_Shipment, Cost_Fee, FWD");
                    sql.AppendLine("        , ShipID, CheckID, Remark, Create_Who");
                    sql.AppendLine("    ) VALUES (");
                    sql.AppendLine("        @Data_ID_#idx#, @Ship_FID_#idx#, @Ship_SID_#idx#, @SO_FID_#idx#, @SO_SID_#idx#");
                    sql.AppendLine("        , @BoxCnt_#idx#, @Pallet_#idx#, @Weight_#idx#, @Cuft_#idx#, @TradeTerms_#idx#");
                    sql.AppendLine("        , @Price1_#idx#, @Price2_#idx#, @Price3_#idx#, @Price4_#idx#");
                    sql.AppendLine("        , @Price5_#idx#, @Price6_#idx#, @Price7_#idx#");
                    sql.AppendLine("        , @Cost_ExportTax_#idx#, @Cost_Freight_#idx#, @Cost_Shipment_#idx#, @Cost_Fee_#idx#, @FWD_#idx#");
                    sql.AppendLine("        , @ShipID_#idx#, @CheckID_#idx#, @Remark_#idx#, @Create_Who");
                    sql.AppendLine("    )");
                    sql.AppendLine("  END");

                    //replace idx number
                    sql.Replace("#idx#", row.ToString());

                    //add params
                    cmd.Parameters.AddWithValue("Data_ID_" + row, item.Data_ID);
                    cmd.Parameters.AddWithValue("Ship_FID_" + row, item.Ship_FID);
                    cmd.Parameters.AddWithValue("Ship_SID_" + row, item.Ship_SID);
                    cmd.Parameters.AddWithValue("SO_FID_" + row, item.SO_FID);
                    cmd.Parameters.AddWithValue("SO_SID_" + row, item.SO_SID);
                    cmd.Parameters.AddWithValue("BoxCnt_" + row, item.BoxCnt);
                    cmd.Parameters.AddWithValue("Pallet_" + row, item.Pallet);
                    cmd.Parameters.AddWithValue("Weight_" + row, item.Weight);
                    cmd.Parameters.AddWithValue("Cuft_" + row, item.Cuft);
                    cmd.Parameters.AddWithValue("TradeTerms_" + row, item.TradeTerms);
                    cmd.Parameters.AddWithValue("Price1_" + row, item.Price1);
                    cmd.Parameters.AddWithValue("Price2_" + row, item.Price2);
                    cmd.Parameters.AddWithValue("Price3_" + row, item.Price3);
                    cmd.Parameters.AddWithValue("Price4_" + row, item.Price4);
                    cmd.Parameters.AddWithValue("Price5_" + row, item.Price5);
                    cmd.Parameters.AddWithValue("Price6_" + row, item.Price6);
                    cmd.Parameters.AddWithValue("Price7_" + row, item.Price7);

                    cmd.Parameters.AddWithValue("Cost_ExportTax_" + row, item.Cost_ExportTax);
                    cmd.Parameters.AddWithValue("Cost_Freight_" + row, item.Cost_Freight);
                    cmd.Parameters.AddWithValue("Cost_Shipment_" + row, item.Cost_Shipment);
                    cmd.Parameters.AddWithValue("Cost_Fee_" + row, item.Cost_Fee);
                    cmd.Parameters.AddWithValue("FWD_" + row, item.FWD);
                    cmd.Parameters.AddWithValue("ShipID_" + row, item.ShipID.Equals(0) ? DBNull.Value : (object)item.ShipID);
                    cmd.Parameters.AddWithValue("CheckID_" + row, item.CheckID.Equals(0) ? DBNull.Value : (object)item.CheckID);
                    cmd.Parameters.AddWithValue("Remark_" + row, item.Remark);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }


        }


        #endregion *** 出貨明細表(上海) E ***

        #endregion



        #region -----// Update //-----

        #region *** 發貨 S ***

        /// <summary>
        /// 更新發貨資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool UpdateShipFreight(ShipFreightItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE ShipFreight SET");
                sql.AppendLine("  ShipDate = @ShipDate, ShipComp = @ShipComp, ShipWay = @ShipWay");
                sql.AppendLine("  , ShipWho = @ShipWho, Remark = @Remark");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("ShipDate", instance.ShipDate);
                cmd.Parameters.AddWithValue("ShipComp", instance.ShipComp);
                cmd.Parameters.AddWithValue("ShipWay", instance.ShipWay);
                cmd.Parameters.AddWithValue("ShipWho", instance.ShipWho);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// 更新資料 - 物流單號
        /// </summary>
        /// <returns></returns>
        public bool UpdateShipFreightDetail(ShipFreightDetail instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE ShipFreightDetail SET");
                sql.AppendLine("  ShipNo = @ShipNo, ShipCnt = @ShipCnt");
                sql.AppendLine("  , Pay1 = @Pay1, Pay2 = @Pay2, Pay3 = @Pay3");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID) AND (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("ShipNo", instance.ShipNo);
                cmd.Parameters.AddWithValue("ShipCnt", instance.ShipCnt);
                cmd.Parameters.AddWithValue("Pay1", instance.Pay1);
                cmd.Parameters.AddWithValue("Pay2", instance.Pay2);
                cmd.Parameters.AddWithValue("Pay3", instance.Pay3);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// 更新貨運公司資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool UpdateShipComp(ShipComp instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Logistics SET ");
                sql.AppendLine("  DisplayName = @DisplayName, Display = @Display, Sort = @Sort");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.ID);
                cmd.Parameters.AddWithValue("DisplayName", instance.Label);
                cmd.Parameters.AddWithValue("Display", instance.Display);
                cmd.Parameters.AddWithValue("Sort", instance.Sort);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// [發貨匯入] 物流單轉入 (A)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool UpdateShipImport_A(ShipImportData instance, out string ErrMsg)
        {
            try
            {
                //Exec StoreProcedure (EF_ShippingNo)
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "EF_ShippingNo";
                    cmd.Parameters.AddWithValue("ERP_sDate", instance.erpSDate);
                    cmd.Parameters.AddWithValue("ERP_eDate", instance.erpEDate);
                    cmd.Parameters.AddWithValue("Creater", instance.Update_Who);
                    cmd.Parameters.AddWithValue("CompanyID", "SZ");
                    cmd.CommandTimeout = 120;

                    //取得回傳值, 輸出參數
                    SqlParameter Msg = cmd.Parameters.Add("@Msg", SqlDbType.NVarChar, 200);
                    Msg.Direction = ParameterDirection.Output;

                    return dbConn.ExecuteSql(cmd, out ErrMsg);
                }
            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message.ToString();
                return false;
            }
        }


        /// <summary>
        /// [發貨匯入] 運費更新 (B)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool UpdateShipImport_B(ShipImportData instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE ShipFreightDetail");
                sql.AppendLine(" SET ShipFreightDetail.ShipCnt = RelDT.Qty");
                sql.AppendLine(" , ShipFreightDetail.Pay2 = RelDT.Freight");
                sql.AppendLine(" FROM Ship_ImportData_DT RelDT");
                sql.AppendLine(" WHERE (ShipFreightDetail.ShipNo = RelDT.ShipNo)");
                sql.AppendLine("  AND (RelDT.Parent_ID = @DataID)");
                sql.AppendLine("  AND (ShipFreightDetail.Parent_ID IN (");
                sql.AppendLine("   SELECT Data_ID FROM ShipFreight WHERE IsAuto = 'Y'");
                sql.AppendLine("  ));");

                sql.AppendLine(" UPDATE Ship_ImportData_DT");
                sql.AppendLine(" SET IsPass = 'Y'");
                sql.AppendLine(" WHERE (Parent_ID = @DataID)");
                sql.AppendLine(" AND (ShipNo IN");
                sql.AppendLine("  (SELECT ShipNo FROM ShipFreightDetail)");
                sql.AppendLine(" );");

                sql.AppendLine(" UPDATE Ship_ImportData");
                sql.AppendLine(" SET Status = 30, Update_Who = @Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        #endregion *** 發貨 E ***


        #region *** 客戶返利 S ***

        /// <summary>
        /// 更新客戶返利
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool UpdateCustRebate(CustRebateItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Rebate_Data SET ");
                sql.AppendLine("  DataYear = @DataYear, CustID = @CustID, Formula = @Formula, Remark = @Remark");
                sql.AppendLine("  , RespMoney = @RespMoney, RespPercent = @RespPercent, FightMoney = @FightMoney, FightPercent = @FightPercent");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("DataYear", instance.DataYear);
                cmd.Parameters.AddWithValue("CustID", instance.CustID);
                cmd.Parameters.AddWithValue("Formula", instance.Formula);
                cmd.Parameters.AddWithValue("RespMoney", instance.Cnt_e);
                cmd.Parameters.AddWithValue("RespPercent", instance.Cnt_f);
                cmd.Parameters.AddWithValue("FightMoney", instance.Cnt_g);
                cmd.Parameters.AddWithValue("FightPercent", instance.Cnt_h);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        #endregion *** 客戶返利 E ***


        #region *** 客訴 S ***

        /// <summary>
        /// [開案中客訴] - Update
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dataType"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool UpdateCCP_Temp(CCPTempItem instance, string dataType, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                switch (dataType)
                {
                    case "1":
                        //客服
                        sql.AppendLine(" UPDATE Cust_Complaint_Temp");
                        sql.AppendLine(" SET CustType = @CustType");
                        sql.AppendLine(" , RefCustID = @RefCustID, RefMallID = @RefMallID, CustInput = @CustInput");
                        sql.AppendLine(" , BuyerName = @BuyerName, BuyerPhone = @BuyerPhone, BuyerAddr = @BuyerAddr");
                        sql.AppendLine(" , PlanType = @PlanType, InvoiceIsBack = @InvoiceIsBack, Platform_ID = @Platform_ID, ERP_ID = @ERP_ID");
                        sql.AppendLine(" , CS_Who = @Update_Who, CS_Time = GETDATE()");
                        sql.AppendLine(" WHERE (Data_ID = @DataID);");

                        cmd.Parameters.AddWithValue("CustType", instance.CustType);
                        cmd.Parameters.AddWithValue("RefCustID", instance.RefCustID);
                        cmd.Parameters.AddWithValue("RefMallID", instance.RefMallID);
                        cmd.Parameters.AddWithValue("CustInput", instance.CustInput);
                        cmd.Parameters.AddWithValue("BuyerName", instance.BuyerName);
                        cmd.Parameters.AddWithValue("BuyerPhone", instance.BuyerPhone);
                        cmd.Parameters.AddWithValue("BuyerAddr", instance.BuyerAddr);
                        cmd.Parameters.AddWithValue("PlanType", instance.PlanType);
                        cmd.Parameters.AddWithValue("InvoiceIsBack", instance.InvoiceIsBack);
                        cmd.Parameters.AddWithValue("Platform_ID", instance.Platform_ID);
                        cmd.Parameters.AddWithValue("ERP_ID", instance.ERP_ID);

                        break;

                    default:
                        //收貨
                        sql.AppendLine(" UPDATE Cust_Complaint_Temp");
                        sql.AppendLine(" SET FreightType = @FreightType");
                        sql.AppendLine(" , FreightInput = @FreightInput, FreightGetDate = @FreightGetDate, InvoiceNumber = @InvoiceNumber");
                        sql.AppendLine(" , ShipComp = @ShipComp, ShipWho = @ShipWho, ShipTel = @ShipTel, ShipAddr = @ShipAddr, InvoicePrice = @InvoicePrice");
                        sql.AppendLine(" , Freight_Who = @Update_Who, Freight_Time = GETDATE()");
                        sql.AppendLine(" WHERE (Data_ID = @DataID);");

                        cmd.Parameters.AddWithValue("FreightType", instance.FreightType);
                        cmd.Parameters.AddWithValue("FreightInput", instance.FreightInput);
                        cmd.Parameters.AddWithValue("FreightGetDate", instance.FreightGetDate);
                        cmd.Parameters.AddWithValue("InvoiceNumber", instance.InvoiceNumber);
                        cmd.Parameters.AddWithValue("ShipComp", instance.ShipComp);
                        cmd.Parameters.AddWithValue("ShipWho", instance.ShipWho);
                        cmd.Parameters.AddWithValue("ShipTel", instance.ShipTel);
                        cmd.Parameters.AddWithValue("ShipAddr", instance.ShipAddr);
                        cmd.Parameters.AddWithValue("InvoicePrice", instance.InvoicePrice);

                        break;
                }

                //----- SQL 執行 -----
                //Update Last updater
                sql.AppendLine(" UPDATE Cust_Complaint_Temp");
                sql.AppendLine(" SET Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID);");
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// [已開案客訴] - Update
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool UpdateCCP_Data(CCPItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                int _flowNow = instance.FlowStatus;
                int _flowNext = instance.nextFlow;
                string _type = instance.inputType;
                string _desc = instance.inputDesc;

                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Cust_Complaint");
                sql.AppendLine(" SET Update_Who = @Update_Who, Update_Time = GETDATE()");

                #region >> 依Flow更新欄位 <<

                switch (_flowNow)
                {
                    case 201:
                        //一線
                        sql.Append(", FlowStatus = @FlowStatus, Flow201_Type = @Flow201_Type, Flow201_Desc = @Flow201_Desc");
                        sql.Append(", Remark_Check = @Remark_Check");
                        sql.Append(", Flow201_Who = @Update_Who, Flow201_Time = GETDATE()");

                        cmd.Parameters.AddWithValue("Flow201_Type", _type);
                        cmd.Parameters.AddWithValue("Flow201_Desc", _desc);
                        cmd.Parameters.AddWithValue("Remark_Check", instance.Remark_Check);

                        break;

                    case 301:
                        //二線
                        sql.Append(", FlowStatus = @FlowStatus, Flow301_Type = @Flow301_Type, Flow301_Desc = @Flow301_Desc");
                        sql.Append(", FixPrice = @FixPrice, FixWishDate = @FixWishDate, FixOkDate = @FixOkDate");
                        sql.Append(", Flow301_Who = @Update_Who, Flow301_Time = GETDATE()");

                        cmd.Parameters.AddWithValue("Flow301_Type", _type);
                        cmd.Parameters.AddWithValue("Flow301_Desc", _desc);
                        cmd.Parameters.AddWithValue("FixPrice", instance.FixPrice);
                        cmd.Parameters.AddWithValue("FixWishDate", string.IsNullOrWhiteSpace(instance.FixWishDate) ? (object)DBNull.Value : instance.FixWishDate);
                        cmd.Parameters.AddWithValue("FixOkDate", string.IsNullOrWhiteSpace(instance.FixOkDate) ? (object)DBNull.Value : instance.FixOkDate);

                        break;

                    case 401:
                        //業務
                        sql.Append(", FlowStatus = @FlowStatus, Flow401_Type = @Flow401_Type, Flow401_Desc = @Flow401_Desc");
                        sql.Append(", FixTotalPrice = @FixTotalPrice");
                        sql.Append(", ERP_No1 = @ERP_No1, ERP_No2 = @ERP_No2, ERP_No3 = @ERP_No3");
                        sql.Append(", ERP_No4 = @ERP_No4, ERP_No5 = @ERP_No5, ERP_No6 = @ERP_No6");
                        sql.Append(", Flow401_Who = @Update_Who, Flow401_Time = GETDATE()");

                        cmd.Parameters.AddWithValue("Flow401_Type", _type);
                        cmd.Parameters.AddWithValue("Flow401_Desc", _desc);
                        cmd.Parameters.AddWithValue("FixTotalPrice", instance.FixTotalPrice);
                        cmd.Parameters.AddWithValue("ERP_No1", instance.ERP_No1);
                        cmd.Parameters.AddWithValue("ERP_No2", instance.ERP_No2);
                        cmd.Parameters.AddWithValue("ERP_No3", instance.ERP_No3);
                        cmd.Parameters.AddWithValue("ERP_No4", instance.ERP_No4);
                        cmd.Parameters.AddWithValue("ERP_No5", instance.ERP_No5);
                        cmd.Parameters.AddWithValue("ERP_No6", instance.ERP_No6);

                        break;

                    case 501:
                        //資材
                        sql.Append(", FlowStatus = @FlowStatus, Flow501_Type = @Flow501_Type, Flow501_Desc = @Flow501_Desc");
                        sql.Append(", ShipComp = @ShipComp, ShipNo = @ShipNo, ShipDate = @ShipDate");
                        sql.Append(", Flow501_Who = @Update_Who, Flow501_Time = GETDATE()");

                        cmd.Parameters.AddWithValue("Flow501_Type", _type);
                        cmd.Parameters.AddWithValue("Flow501_Desc", _desc);
                        cmd.Parameters.AddWithValue("ShipComp", instance.ShipComp);
                        cmd.Parameters.AddWithValue("ShipNo", instance.ShipNo);
                        cmd.Parameters.AddWithValue("ShipDate", string.IsNullOrWhiteSpace(instance.ShipDate) ? DBNull.Value : (object)instance.ShipDate);

                        break;

                    default:
                        switch (_flowNext)
                        {
                            case 998:
                                //作廢
                                sql.Append(", FlowStatus = @FlowStatus, Finish_Remark = @Finish_Remark");

                                cmd.Parameters.AddWithValue("Finish_Remark", instance.Finish_Remark);

                                break;


                            case 999:
                                //結案
                                sql.Append(", FlowStatus = @FlowStatus, Finish_Time = GETDATE(), Finish_Who = @Update_Who");

                                break;
                        }

                        break;
                }

                #endregion

                sql.AppendLine(" WHERE (Data_ID = @DataID);");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 120;  //單位:秒
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);
                cmd.Parameters.AddWithValue("FlowStatus", _flowNext);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        #endregion *** 客訴 E ***


        #region *** 電商平台數據 S ***

        /// <summary>
        /// [電商平台數據] 更新資料 - Year
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool UpdateECD_Year(ECDItem_Year instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 執行 -----
                sql.AppendLine(" UPDATE SZ_eCommerce_setYear");
                sql.AppendLine(" SET RefMall = @RefMall, setYear = @setYear");
                sql.AppendLine(" , Price_Sales = @Price_Sales, Price_Rebate = @Price_Rebate");
                sql.AppendLine(" , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID);");
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setYear", instance.setYear);
                cmd.Parameters.AddWithValue("Price_Sales", instance.Price_Sales);
                cmd.Parameters.AddWithValue("Price_Rebate", instance.Price_Rebate);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }
        }

        /// <summary>
        /// [電商平台數據] 更新資料 - Month
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// Price_Promo = 由單身計算
        /// </remarks>
        public bool UpdateECD_Month(ECDItem_Month instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 執行 -----
                sql.AppendLine(" UPDATE SZ_eCommerce_setMonth");
                sql.AppendLine(" SET RefMall = @RefMall, setYear = @setYear, setMonth = @setMonth");
                sql.AppendLine("  , Price_Income = @Price_Income, Price_SalesRebate = @Price_SalesRebate, Price_Cost = @Price_Cost");
                sql.AppendLine("  , Price_Profit = @Price_Profit, Price_Purchase = @Price_Purchase, Price_Back = @Price_Back");
                sql.AppendLine("  , Price_PurchaseRebate = @Price_PurchaseRebate, Price_Freight = @Price_Freight");
                sql.AppendLine(" , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID);");
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setYear", instance.setYear);
                cmd.Parameters.AddWithValue("setMonth", instance.setMonth);
                cmd.Parameters.AddWithValue("Price_Income", instance.Price_Income);
                cmd.Parameters.AddWithValue("Price_SalesRebate", instance.Price_SalesRebate);
                cmd.Parameters.AddWithValue("Price_Cost", instance.Price_Cost);
                cmd.Parameters.AddWithValue("Price_Profit", instance.Price_Profit);
                cmd.Parameters.AddWithValue("Price_Purchase", instance.Price_Purchase);
                cmd.Parameters.AddWithValue("Price_Back", instance.Price_Back);
                cmd.Parameters.AddWithValue("Price_PurchaseRebate", instance.Price_PurchaseRebate);
                cmd.Parameters.AddWithValue("Price_Freight", instance.Price_Freight);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }
        }

        /// <summary>
        /// [電商平台數據] 更新資料 - Date
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool UpdateECD_Date(ECDItem_Date instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 執行 -----
                sql.AppendLine(" UPDATE SZ_eCommerce_setDate");
                sql.AppendLine(" SET RefMall = @RefMall, setDate = @setDate");
                sql.AppendLine(" , Price_RefSales = @Price_RefSales, Price_RefProfit = @Price_RefProfit, Price_RealSales = @Price_RealSales");
                sql.AppendLine(" , Price_RealProfit = @Price_RealProfit, Price_OrderPrice = @Price_OrderPrice, Price_ROI = @Price_ROI");
                sql.AppendLine(" , Price_ClickCost = @Price_ClickCost, Price_Adv1 = @Price_Adv1, Price_Adv2 = @Price_Adv2, Price_Adv3 = @Price_Adv3");
                sql.AppendLine(" , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID);");
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("RefMall", instance.RefMall);
                cmd.Parameters.AddWithValue("setDate", instance.setDate);
                cmd.Parameters.AddWithValue("Price_RefSales", instance.Price_RefSales);
                cmd.Parameters.AddWithValue("Price_RefProfit", instance.Price_RefProfit);
                cmd.Parameters.AddWithValue("Price_RealSales", instance.Price_RealSales);
                cmd.Parameters.AddWithValue("Price_RealProfit", instance.Price_RealProfit);
                cmd.Parameters.AddWithValue("Price_OrderPrice", instance.Price_OrderPrice);
                cmd.Parameters.AddWithValue("Price_ROI", instance.Price_ROI);
                cmd.Parameters.AddWithValue("Price_ClickCost", instance.Price_ClickCost);
                cmd.Parameters.AddWithValue("Price_Adv1", instance.Price_Adv1);
                cmd.Parameters.AddWithValue("Price_Adv2", instance.Price_Adv2);
                cmd.Parameters.AddWithValue("Price_Adv3", instance.Price_Adv3);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }
        }


        /// <summary>
        /// [電商平台數據] 更新單身資料 - Month
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// Price_Promo = 由單身計算
        /// </remarks>
        public bool UpdateECD_MonthDT(ECDItem_MonthDT instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 執行 -----
                sql.AppendLine(" UPDATE SZ_eCommerce_PromoDT");
                sql.AppendLine(" SET RecordDate = @RecordDate, RecordType = @RecordType, RecordMoney = @RecordMoney");
                sql.AppendLine(" , CheckDate = @CheckDate, CheckMoney = @CheckMoney, CheckInvoiceDate = @CheckInvoiceDate");
                sql.AppendLine(" , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID);");

                /* Update 單頭 Price_Promo欄位 */
                sql.AppendLine(" UPDATE SZ_eCommerce_setMonth");
                sql.AppendLine(" SET Price_Promo =");
                sql.AppendLine(" (");
                sql.AppendLine("   SELECT ISNULL(SUM(RecordMoney), 0)");
                sql.AppendLine("   FROM SZ_eCommerce_PromoDT");
                sql.AppendLine("   WHERE (Parent_ID = @Parent_ID)");
                sql.AppendLine(" )");
                sql.AppendLine(" WHERE (Data_ID = @Parent_ID)");

                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Parent_ID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("RecordDate", instance.RecordDate);
                cmd.Parameters.AddWithValue("RecordType", instance.RecordType);
                cmd.Parameters.AddWithValue("RecordMoney", instance.RecordMoney);
                cmd.Parameters.AddWithValue("CheckDate", string.IsNullOrWhiteSpace(instance.CheckDate) ? (object)DBNull.Value : instance.CheckDate);
                cmd.Parameters.AddWithValue("CheckMoney", instance.CheckMoney);
                cmd.Parameters.AddWithValue("CheckInvoiceDate", string.IsNullOrWhiteSpace(instance.CheckInvoiceDate) ? (object)DBNull.Value : instance.CheckInvoiceDate);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.Report, out ErrMsg);
            }
        }

        #endregion *** 電商平台數據 E ***

        #endregion



        #region -----// Delete //-----

        #region *** 發貨 S ***
        /// <summary>
        /// 刪除資料 - 物流單號
        /// </summary>
        /// <param name="parentID">上層編號</param>
        /// <param name="dataID">ID</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool DeleteShipFreightDetail(string parentID, string dataID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM ShipFreightDetail WHERE (Parent_ID = @ParentID) AND (Data_ID = @DataID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", parentID);
                cmd.Parameters.AddWithValue("DataID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        /// <summary>
        /// 刪除資料 - 關聯單號
        /// </summary>
        /// <param name="relID">被關聯的單頭ID</param>
        /// <param name="dataID">關聯ID</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool DeleteShipFreightRel(string relID, string dataID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM ShipFreightRel WHERE (Data_ID = @DataID);");
                sql.AppendLine(" DELETE FROM ShipFreight WHERE (Data_ID = @RelID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);
                cmd.Parameters.AddWithValue("RelID", relID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// 刪除資料 - 貨運公司
        /// </summary>
        /// <param name="dataID">ID</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// 若資料已使用,則設為隱藏
        /// </remarks>
        public bool DeleteShipComp(string dataID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" IF (SELECT COUNT(*) FROM ShipFreight WHERE (ShipComp = @DataID)) > 0");
                sql.AppendLine("  BEGIN");
                sql.AppendLine("   UPDATE Logistics SET Display = 'N' WHERE (Data_ID = @DataID);");
                sql.AppendLine("  END");
                sql.AppendLine(" ELSE");
                sql.AppendLine("  BEGIN");
                sql.AppendLine("   DELETE FROM Logistics WHERE (Data_ID = @DataID);");
                sql.AppendLine("  END");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// [發貨] 刪除匯入
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete_ShipImport(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Ship_ImportData_DT WHERE (Parent_ID = @Data_ID);");
                sql.AppendLine(" DELETE FROM Ship_ImportData WHERE (Data_ID = @Data_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        #endregion  *** 發貨 E ***


        #region *** 客訴 S ***
        /// <summary>
        /// [開案中客訴] 刪除資料
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete_CCPTemp(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Cust_Complaint_TempDT WHERE (Parent_ID = @Data_ID);");
                sql.AppendLine(" DELETE FROM Cust_Complaint_TempAttachment WHERE (Parent_ID = @Data_ID);");
                sql.AppendLine(" DELETE FROM Cust_Complaint_Temp WHERE (Data_ID = @Data_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// [開案中客訴] 刪除商品資料
        /// </summary>
        /// <param name="inst"></param>
        /// <returns></returns>
        public bool Delete_CCPDetailData(CCPDetail inst)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Cust_Complaint_TempDT");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID);");

                //Update Last updater
                sql.AppendLine(" UPDATE Cust_Complaint_Temp");
                sql.AppendLine(" SET Update_Who = @Create_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Parent_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", inst.Parent_ID);
                cmd.Parameters.AddWithValue("Data_ID", inst.Data_ID);
                cmd.Parameters.AddWithValue("Create_Who", inst.Create_Who);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// [開案中客訴] 刪除檔案資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Delete_CCPTempFiles(CCPAttachment instance)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Cust_Complaint_TempAttachment");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// [已開案客訴] 刪除檔案資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Delete_CCPFiles(CCPAttachment instance)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Cust_Complaint_Attachment");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        /// <summary>
        /// [已開案客訴] 刪除通知名單
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete_CCPInform(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Cust_Complaint_InformFlow WHERE (Data_ID = @Data_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        #endregion *** 客訴 E ***

        #endregion



        #region -----// Others //-----

        /// <summary>
        /// 依代號取得資料庫實體名稱
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


        #endregion


        #region -- 通知信 --
        /// <summary>
        /// 發通知信
        /// </summary>
        /// <param name="sendType">A:新客訴案件/B:其他/X:結案</param>
        /// <param name="id">資料編號</param>
        /// <param name="flowID">流程編號</param>
        /// <param name="funcUrl">此功能網址</param>
        /// <param name="lang">Lang</param>
        /// <param name="typeID">TypeID:客訴來源</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool doSendInformMail(string sendType, string id, string flowID, string funcUrl
            , string lang, string typeID, out string ErrMsg)
        {
            ErrMsg = "";
            int DataCnt = 0;
            string mailSubject = "";

            //----- 宣告:資料參數 -----
            Dictionary<string, string> search = new Dictionary<string, string>();

            //判斷寄送Type, 執行資料篩選
            switch (sendType)
            {
                case "A":
                    //新客訴案件
                    search.Add("TraceID", id);
                    break;

                default:
                    //結案 & 其他
                    search.Add("DataID", id);
                    break;
            }

            //[資料取得] - 基本資料
            var baseData = GetCCPList(search, lang, Convert.ToInt32(typeID),
                0, 998, out DataCnt, out ErrMsg).Take(1).FirstOrDefault();
            if (baseData == null)
            {
                ErrMsg = "查無資料";
                return false;
            }

            //Get values
            string ccTypeName = baseData.CC_TypeName;
            string flowName = baseData.FlowStatusName;
            string model = baseData.ModelNo;
            string ccUid = baseData.CC_UID;


            //[設定] 郵件主旨
            switch (sendType)
            {
                case "A":
                    //新客訴案件
                    mailSubject = "[客訴][{0}][{1}]新客訴案件成立, 共 {2} 件".FormatThis(ccTypeName, flowName, DataCnt);
                    break;

                case "X":
                    //結案
                    mailSubject = "[客訴][{0}][結案通知] {1} #{2}".FormatThis(ccTypeName, model, ccUid);
                    break;

                default:
                    mailSubject = "[客訴][{0}][{1}] {2} #{3}".FormatThis(ccTypeName, flowName, model, ccUid);
                    break;
            }


            //[設定] 郵件內容
            StringBuilder mailBoday = Get_MailContent(sendType, funcUrl, lang, baseData);

            //[設定] 取得收件人
            ArrayList mailList = Get_MailList(flowID, sendType, typeID);

            //判斷是否有收件人
            if (mailList.Count == 0)
            {
                ErrMsg = "無收件人";
                return false;
            }

            //開始發送通知信
            if (!CustomExtension.Send_Email(fn_Param.SysMail_Sender, "寶工客訴", mailList, mailSubject, mailBoday, out ErrMsg))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 取得收件人清單
        /// </summary>
        /// <param name="flowID">關卡ID</param>
        /// <param name="sendType">類型</param>
        /// <param name="ccType">客訴來源</param>
        /// <returns></returns>
        private ArrayList Get_MailList(string flowID, string sendType, string ccType)
        {
            //----- 宣告:資料參數 -----
            ArrayList mailList = new ArrayList();

            switch (sendType)
            {
                case "X":
                    //結案(所有人)
                    var dataX = GetCCP_MailReceiver(ccType, "");
                    foreach (var item in dataX)
                    {
                        mailList.Add(item.Email);
                    }
                    dataX = null;

                    break;


                default:
                    var dataA = GetCCP_MailReceiver(ccType, flowID);
                    foreach (var item in dataA)
                    {
                        mailList.Add(item.Email);
                    }
                    dataA = null;

                    break;
            }

            return mailList;
        }

        /// <summary>
        /// 設定郵件內容
        /// </summary>
        /// <param name="sendType">寄送類型</param>
        /// <param name="funcUrl">功能網址</param>
        /// <param name="lang">語系</param>
        /// <param name="baseData">基本資料</param>
        /// <returns></returns>
        private StringBuilder Get_MailContent(string sendType, string funcUrl, string lang, CCPItem baseData)
        {
            //宣告
            StringBuilder html = new StringBuilder();
            Menu3000Repository _data = new Menu3000Repository();

            //Html模版路徑(From CDN)
            string url = "{0}PKHome/CustComplaint/Mail_{1}.html?v=1.0".FormatThis(fn_Param.CDNUrl, lang.ToUpper());

            //取得HTML模版(Html不可放在本機)
            string htmlPage = CustomExtension.WebRequest_byGET(url);

            //加入模版內容
            html.Append(htmlPage);

            //[取代指定內容]:郵件固定內容
            string msg = "";
            string id = baseData.Data_ID.ToString();
            string editUrl = "{0}/Edit/{1}#flow{2}".FormatThis(funcUrl, id, baseData.FlowStatus);
            string viewUrl = "{0}/View/{1}".FormatThis(funcUrl, id);
            string pageUrl = "";
            switch (sendType)
            {
                case "A":
                    msg = "新客訴案件已成立,請前往處理.<p>追蹤碼：{0}</p>".FormatThis(baseData.TraceID);
                    msg += "<p>客戶類別：{0} ({1})</p>".FormatThis(baseData.CustTypeName, (baseData.RefCustName) ?? baseData.RefMallName);

                    pageUrl = "{0}?trace={1}".FormatThis(funcUrl, baseData.TraceID);

                    //不顯示基本資料
                    html.Replace("#Disp1#", "display:none");

                    break;

                case "X":
                    msg = "此案件已結案";
                    pageUrl = viewUrl;
                    break;

                default:
                    msg = "您有客訴案件待處理.";
                    pageUrl = editUrl;
                    break;

            }
            html.Replace("#informMessage#", msg);
            html.Replace("#ProcUrl#", pageUrl);
            html.Replace("#今年#", DateTime.Now.Year.ToString());


            //[取代指定內容]:基本資料
            html.Replace("#TraceID#", baseData.TraceID);
            html.Replace("#CreateDate#", baseData.Create_Time.ToDateString("yyyy/MM/dd"));
            html.Replace("#CCUID#", baseData.CC_UID);
            html.Replace("#FlowName#", baseData.FlowStatusName);
            html.Replace("#ModelNo#", baseData.ModelNo);
            html.Replace("#CustType#", "{0}：{1}".FormatThis(baseData.CustTypeName, (baseData.RefCustName) ?? baseData.RefMallName));
            html.Replace("#content#", baseData.Remark.Replace("\r", "<br/>"));


            baseData = null;


            //[資料取得] - 處理進度
            //var replyData = _data.GetMKHelpReplyList(guid);
            //html.Replace("#Disp2#", replyData.Count() == 0 ? "display:none" : "");

            //if (replyData.Count() > 0)
            //{
            //    string loopHtml = "";
            //    foreach (var data in replyData)
            //    {
            //        loopHtml += "<tr>";
            //        loopHtml += " <td colspan=\"4\">";
            //        loopHtml += " <p><font style=\"font-size: 12px; color: #4183c4;\">{0}</font>&nbsp;<font style=\"font-size: 12px; color: #757575 ;\">{1}</font>"
            //            .FormatThis(data.Create_Name, data.Create_Time);
            //        loopHtml += " </p>";
            //        loopHtml += " <p>{0}</p>".FormatThis(data.Reply_Content.ToString().Replace("\r", "<br/>"));
            //        loopHtml += " </td>";
            //        loopHtml += "</tr>";
            //    }

            //    html.Replace("#replyItems#", loopHtml);
            //}
            //replyData = null;


            //[資料取得] - 附件清單
            //var fileData = _data.GetMKHelpFileList(guid);
            //html.Replace("#Disp3#", fileData.Count() == 0 ? "display:none" : "");

            //if (fileData.Count() > 0)
            //{
            //    string loopHtml = "";
            //    foreach (var data in fileData)
            //    {
            //        loopHtml += "<div style=\"background-color: #43a047; border: 1px solid #43a047; text-align: center; margin: 10px;\">";
            //        loopHtml += "<a href=\"{0}{1}/{2}\" target=\"_blank\" style=\"color: #FFFFFF; text-decoration: none; display: block; padding: 5px;\">{3}</a>"
            //            .FormatThis(
            //                fn_Param.RefUrl + UploadFolder
            //                , guid
            //                , data.AttachFile
            //                , data.AttachFile_Org);
            //        loopHtml += "</div>";
            //    }

            //    html.Replace("#filesItems#", loopHtml);
            //}
            //fileData = null;

            return html;
        }


        #endregion

    }
}

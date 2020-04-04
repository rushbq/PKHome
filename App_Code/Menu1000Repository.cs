using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Menu1000Data.Models;
using PKLib_Method.Methods;

/*
  [製物工單]-MarketingHelp
  [郵件寄送登記]-Postal
*/
namespace Menu1000Data.Controllers
{

    public class Menu1000Repository
    {
        public string ErrMsg;

        #region -----// Read //-----


        #region *** 產品開發計劃 S ***
        /// <summary>
        /// 取得分類階層選單
        /// </summary>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CateItem> GetMenuList(string lang, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<CateItem> dataList = new List<CateItem>();
            StringBuilder sql = new StringBuilder();
            string langCode = fn_Language.Get_DBLangCode(lang);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Menu.Menu_ID AS ID, Menu.MenuName_{0} AS Label".FormatThis(langCode));
                sql.AppendLine(" , Menu.Parent_ID, Menu.Menu_Level, Menu.Class_ID");
                sql.AppendLine(" , Menu.MenuName_zh_TW AS NameTW, Menu.MenuName_zh_CN AS NameCN, Menu.MenuName_en_US AS NameEN");
                sql.AppendLine(" , Menu.Display, Menu.Sort");
                sql.AppendLine(" FROM PKCatalog.dbo.Catalog_Menu Menu");
                sql.AppendLine(" WHERE (Menu.Display = 'Y')");

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
                            case "Level":
                                switch (item.Value)
                                {
                                    case "1":
                                        sql.Append(" AND (Menu.Menu_Level = 1)");
                                        break;

                                    case "2":
                                        sql.Append(" AND (Menu.Menu_Level = 2)");
                                        break;

                                    default:
                                        sql.Append(" AND (Menu.Menu_Level = 3)");
                                        break;
                                }
                                break;


                            case "ClassID":
                                sql.Append(" AND (Menu.Class_ID = @Class_ID)");

                                cmd.Parameters.AddWithValue("Class_ID", item.Value);

                                break;


                            case "DataID":
                                sql.Append(" AND (Menu.Menu_ID = @DataID)");

                                cmd.Parameters.AddWithValue("DataID", item.Value);

                                break;


                            case "ParentID":
                                sql.Append(" AND (Menu.Parent_ID = @Parent_ID)");

                                cmd.Parameters.AddWithValue("Parent_ID", item.Value);

                                break;

                        }
                    }
                }
                #endregion

                //order by
                sql.AppendLine(" ORDER BY Menu.Sort, Menu.Menu_ID");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                //cmd.CommandTimeout = 60;   //單位:秒

                using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                {
                    if (DT != null)
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new CateItem
                            {
                                ID = item.Field<int>("ID"),
                                Label = item.Field<string>("Label"),
                                Parent_ID = item.Field<int>("Parent_ID"),
                                Menu_Level = item.Field<int>("Menu_Level"),
                                Class_ID = item.Field<int>("Class_ID"),
                                NameEN = item.Field<string>("NameEN"),
                                NameTW = item.Field<string>("NameTW"),
                                NameCN = item.Field<string>("NameCN"),
                                Display = item.Field<string>("Display"),
                                Sort = item.Field<int>("Sort")
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
        /// 取得產品銷售類別
        /// </summary>
        /// <param name="lang">語系(tw/en/cn)</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CateItem> GetProdClassList(string lang, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<CateItem> dataList = new List<CateItem>();
            StringBuilder sql = new StringBuilder();
            string langCode = fn_Language.Get_DBLangCode(lang);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT CAST(Class_ID AS INT) AS ID, Class_Name_{0} AS Label".FormatThis(langCode));
                sql.AppendLine(" FROM ProductCenter.dbo.Prod_Class");
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
                                sql.Append(" AND (Class_ID = @DataID)");

                                cmd.Parameters.AddWithValue("DataID", item.Value);

                                break;

                            case "IsShow":
                                sql.Append(" AND (Display = 'Y')");

                                break;
                        }
                    }
                }
                #endregion

                //order by
                sql.AppendLine(" ORDER BY Sort, Class_ID");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Product, out ErrMsg))
                {
                    if (DT != null)
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new CateItem
                            {
                                ID = item.Field<int>("ID"),
                                Label = item.Field<string>("Label")
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
        /// [產品開發計劃] 取得資料
        /// </summary>
        /// <param name="search"></param>
        /// <param name="setYear">Year</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ProdPlanRptItem> GetProdPlanData(Dictionary<string, string> search, int setYear, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ProdPlanRptItem> dataList = new List<ProdPlanRptItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                ///* 供應商 */
                sql.AppendLine(" ;WITH TblSup AS (");
                sql.AppendLine("  SELECT RTRIM(MA001) AS SupID, MA002 AS SupName");
                sql.AppendLine("   , (CASE UPPER(COMPANY)");
                sql.AppendLine(" 	 WHEN 'PROUNION' THEN 'SZ'");
                sql.AppendLine(" 	 WHEN 'SHPK2' THEN 'SH'");
                sql.AppendLine(" 	 WHEN 'prokit(II)' THEN 'TW'");
                sql.AppendLine(" 	 ELSE 'UNKNOWN'");
                sql.AppendLine(" 	END) AS SupDB");
                sql.AppendLine("  FROM [PKSYS].[dbo].[Supplier_ERPData]");
                sql.AppendLine(" )");
                sql.AppendLine(" SELECT TblAll.* FROM (");
                sql.AppendLine(" SELECT");
                sql.AppendLine(" 	'0_SYS' AS FromData");
                sql.AppendLine(" 	, SalesData.ItemNo");
                sql.AppendLine(" 	, Prod_Catalog.ModelNo");
                sql.AppendLine(" 	, SalesData.Class_ID");
                sql.AppendLine(" 	, Prod_Class.Class_Name_zh_TW AS Class_Name");
                sql.AppendLine(" 	, Prod_Item.Model_Name_zh_TW AS Model_Name");
                sql.AppendLine(" 	, REPLACE(ISNULL(Prod_Item.Catelog_Vol, ''), 'NULL', '') AS Vol");
                sql.AppendLine(" 	, REPLACE(ISNULL(Prod_Item.Page, ''), 'NULL', '') AS [Page]");
                sql.AppendLine(" 	, Prod_Item.Ship_From");
                sql.AppendLine(" 	, Prod_Catalog.ListPic");
                sql.AppendLine(" 	, Prod_Catalog.Menu_Lv1");
                sql.AppendLine(" 	, Prod_Catalog.MenuNameLv1");
                sql.AppendLine(" 	, Prod_Catalog.Menu_Lv2");
                sql.AppendLine(" 	, Prod_Catalog.MenuNameLv2");
                sql.AppendLine(" 	, Prod_Catalog.Menu_Lv3");
                sql.AppendLine(" 	, Prod_Catalog.MenuNameLv3");
                sql.AppendLine(" 	, Prod_Catalog.StyleID");
                sql.AppendLine(" 	, Prod_Catalog.StyleName");
                sql.AppendLine(" 	, SUBSTRING([Prod_Item].Date_Of_Listing, 1, 4) AS TargetMonth");
                sql.AppendLine(" 	, [SalesData].SaleYear");
                sql.AppendLine(" 	, [SalesData].[C_IDE_SaleNumAMO] AS SalesNum"); /* 集團銷售量 (I) */
                sql.AppendLine(" 	, [SalesData].[C_USD_AMO] AS SalesAmount"); /* 集團銷售額USD (J) */
                sql.AppendLine(" 	, [SalesData].[C_USD_PaperCost] AS PaperCost"); /* 單位成本 , 平均單位成本=SUM(成本)/集團銷售量 */
                sql.AppendLine(" 	, ISNULL(TblSup.SupName, '') AS SupName");
                sql.AppendLine(" 	, Remk.Remark");
                sql.AppendLine(" 	, ISNULL(Info.Info1, '') AS ProdDesc");
                sql.AppendLine(" 	, ISNULL(Info.Info2, '') AS ProdFeature");
                sql.AppendLine(" 	, NULL AS DataID");
                sql.AppendLine(" FROM");
                sql.AppendLine(" (");
                ///* 報表中心-銷售資料 */
                sql.AppendLine(" 	SELECT");
                sql.AppendLine(" 		 RTRIM([E_IDE_ItemNo]) AS ItemNo");
                sql.AppendLine(" 		, E_IDE_SaleClassID AS Class_ID");
                sql.AppendLine(" 		, E_IDE_SaleYear AS SaleYear");
                sql.AppendLine(" 		, ISNULL(CAST(SUM([C_IDE_SaleNumAMO]) AS INT), 0) AS C_IDE_SaleNumAMO");
                sql.AppendLine(" 		, ISNULL(CAST(SUM([C_USD_AMO]) AS FLOAT), 0) AS C_USD_AMO");
                sql.AppendLine(" 		, ISNULL(CAST(SUM([C_USD_PaperCost]) AS FLOAT), 0) AS C_USD_PaperCost");
                sql.AppendLine(" 	FROM [ReportCenter].dbo.V_SALE2");
                sql.AppendLine(" 	WHERE RTRIM ([E_IDE_CustomerName]) NOT IN ('暢聯', '上海寶工','台灣寶工','PK LEONARD')");
                ///* [固定參數]:年 */
                sql.AppendLine(" 	 AND ([E_IDE_SaleYear] = @setYear)");
                sql.AppendLine(" 	GROUP BY RTRIM(E_IDE_ItemNo), E_IDE_SaleClassID, E_IDE_SaleYear");
                sql.AppendLine(" ) SalesData");
                sql.AppendLine(" LEFT JOIN");
                sql.AppendLine(" (");
                ///* 電子目錄/產品資料 */
                sql.AppendLine(" 	SELECT");
                sql.AppendLine(" 		RTRIM(Rel.Model_No) AS ModelNo");
                sql.AppendLine(" 		, RTRIM(Rel.Item_No) AS ItemNo");
                sql.AppendLine(" 		, (CASE WHEN ISNULL(Photo.Pic02, '') = '' THEN ");
                sql.AppendLine(" 			CASE WHEN ISNULL(Photo.Pic01, '') = '' THEN");
                sql.AppendLine(" 			CASE WHEN ISNULL(Photo.Pic03, '') = '' THEN '' ELSE Photo.Pic03 END");
                sql.AppendLine(" 			ELSE Photo.Pic01 END");
                sql.AppendLine(" 			ELSE Photo.Pic02 END) AS ListPic");
                sql.AppendLine(" 		, Style.StyleID");
                sql.AppendLine(" 		, Style.StyleName_zh_TW AS StyleName");
                sql.AppendLine(" 		, Prod_Rel_Menu.Menu_Lv1");
                sql.AppendLine(" 		, (SELECT MenuName_zh_TW FROM [PKCatalog].[dbo].Catalog_Menu WITH(NOLOCK) WHERE Menu_ID = RelMenu.Menu_Lv1) AS MenuNameLv1");
                sql.AppendLine(" 		, Prod_Rel_Menu.Menu_Lv2");
                sql.AppendLine(" 		, (SELECT MenuName_zh_TW FROM [PKCatalog].[dbo].Catalog_Menu WITH(NOLOCK) WHERE Menu_ID = RelMenu.Menu_Lv2) AS MenuNameLv2");
                sql.AppendLine(" 		, Prod_Rel_Menu.Menu_Lv3");
                sql.AppendLine(" 		, (SELECT MenuName_zh_TW FROM [PKCatalog].[dbo].Catalog_Menu WITH(NOLOCK) WHERE Menu_ID = RelMenu.Menu_Lv3) AS MenuNameLv3");
                sql.AppendLine(" 	FROM [PKSYS].[dbo].[ModelNo_Rel_ItemNo] Rel");
                sql.AppendLine(" 		LEFT JOIN [PKCatalog].[dbo].Prod_Rel_Menu Prod_Rel_Menu ON Rel.Model_No = Prod_Rel_Menu.ModelNo");
                sql.AppendLine(" 		LEFT JOIN [PKCatalog].[dbo].Prod_Rel_Menu RelMenu ON Prod_Rel_Menu.ModelNo = RelMenu.ModelNo");
                sql.AppendLine(" 		LEFT JOIN [PKCatalog].[dbo].Prod_Rel_Style RelStyle ON Prod_Rel_Menu.ModelNo = RelStyle.ModelNo");
                sql.AppendLine(" 		LEFT JOIN [PKCatalog].[dbo].Catalog_Style Style ON RelStyle.StyleID = Style.StyleID");
                sql.AppendLine(" 		LEFT JOIN [ProductCenter].dbo.ProdPic_Photo Photo ON Prod_Rel_Menu.ModelNo = Photo.Model_No");
                sql.AppendLine(" ) Prod_Catalog ON SalesData.ItemNo = Prod_Catalog.ItemNo COLLATE Chinese_Taiwan_Stroke_CI_AS");
                sql.AppendLine(" INNER JOIN [ProductCenter].[dbo].[Prod_Class] ON SalesData.Class_ID = Prod_Class.Class_ID COLLATE Chinese_Taiwan_Stroke_CI_AS");
                sql.AppendLine(" INNER JOIN [ProductCenter].[dbo].[Prod_Item] WITH (NOLOCK) ON Prod_Item.Model_No = Prod_Catalog.ModelNo");
                sql.AppendLine(" LEFT JOIN [ProductCenter].dbo.[Prod_Info] AS Info ON Prod_Item.Model_No = Info.Model_No AND UPPER(Info.Lang) = 'ZH-TW'");
                sql.AppendLine(" LEFT JOIN PKExcel.dbo.ProductPlan_Remark AS Remk ON Prod_Item.Model_No = Remk.ModelNo");
                sql.AppendLine(" LEFT JOIN TblSup ON Prod_Item.Ship_From = TblSup.SupDB AND Prod_Item.Provider = TblSup.SupID");

                sql.AppendLine(" UNION ALL");

                sql.AppendLine(" SELECT '1_NEW' AS FromData");
                sql.AppendLine(" 	, Base.ModelNo COLLATE Chinese_Taiwan_Stroke_CI_AS AS ItemNo");
                sql.AppendLine(" 	, Base.ModelNo");
                sql.AppendLine(" 	, Base.Class_ID");
                sql.AppendLine(" 	, Prod_Class.Class_Name_zh_TW AS Class_Name");
                sql.AppendLine(" 	, Base.ModelName");
                sql.AppendLine(" 	, '' AS Vol");
                sql.AppendLine(" 	, '' AS [Page]");
                sql.AppendLine(" 	, Base.ShipFrom");
                sql.AppendLine(" 	, ISNULL(Base.Pic, '') AS ListPic");
                sql.AppendLine(" 	, Base.Class_Lv1");
                sql.AppendLine(" 	, (SELECT MenuName_zh_TW FROM [PKCatalog].[dbo].Catalog_Menu WITH(NOLOCK) WHERE Menu_ID = Base.Class_Lv1) AS MenuNameLv1");
                sql.AppendLine(" 	, Class_Lv2");
                sql.AppendLine(" 	, (SELECT MenuName_zh_TW FROM [PKCatalog].[dbo].Catalog_Menu WITH(NOLOCK) WHERE Menu_ID = Base.Class_Lv2) AS MenuNameLv2");
                sql.AppendLine(" 	, 0 AS Lv3");
                sql.AppendLine(" 	, '' AS Lv3Name");
                sql.AppendLine(" 	, 0 AS StyleID");
                sql.AppendLine(" 	, '' AS StyleName");
                sql.AppendLine(" 	, Base.TargetMonth");
                sql.AppendLine(" 	, '' AS SaleYear");
                sql.AppendLine(" 	, 0 AS SaleNumAMO");
                sql.AppendLine(" 	, 0 AS AMO");
                sql.AppendLine(" 	, 0 AS PaperCost");
                sql.AppendLine(" 	, Base.Supplier");
                sql.AppendLine(" 	, Base.Remark");
                sql.AppendLine(" 	, '' AS ProdDesc");
                sql.AppendLine(" 	, '' AS ProdFeature");
                sql.AppendLine(" 	, Base.Data_ID AS DataID");
                sql.AppendLine(" FROM PKExcel.dbo.ProductPlan_Data Base");
                sql.AppendLine("  LEFT JOIN [ProductCenter].[dbo].[Prod_Class] ON Base.Class_ID = Prod_Class.Class_ID");

                sql.AppendLine(" ) AS TblAll");

                ///* 參數:銷售類別 / 一階類別 / 二階類別 */
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
                            case "Class_ID":
                                sql.Append(" AND (TblAll.Class_ID = @Class_ID)");

                                cmd.Parameters.AddWithValue("Class_ID", item.Value);
                                break;

                            case "Menu_Lv1":
                                sql.Append(" AND (TblAll.Menu_Lv1 = @Menu_Lv1)");

                                cmd.Parameters.AddWithValue("Menu_Lv1", item.Value);
                                break;

                            case "Menu_Lv2":
                                sql.Append(" AND (TblAll.Menu_Lv2 >= @Menu_Lv2)");

                                cmd.Parameters.AddWithValue("Menu_Lv2", item.Value);
                                break;

                            case "ModelNo":
                                sql.Append(" AND (TblAll.ModelNo = @ModelNo)");

                                cmd.Parameters.AddWithValue("ModelNo", item.Value);
                                break;

                            case "DataID":
                                sql.Append(" AND (TblAll.DataID = @DataID)");

                                cmd.Parameters.AddWithValue("DataID", item.Value);
                                break;
                        }
                    }
                }
                #endregion

                //Order by
                sql.AppendLine(" ORDER BY TblAll.Class_ID, TblAll.Menu_Lv1, TblAll.Menu_Lv2, TblAll.FromData, TblAll.StyleID, TblAll.ModelNo");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("setYear", setYear);

                using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //取得運算子
                        int _SalesNum = item.Field<int>("SalesNum");    //銷售量
                        double _SalesAmount = item.Field<double>("SalesAmount"); //銷售金額
                        double _PaperCost = item.Field<double>("PaperCost");    //單位成本

                        //運算-平均銷售單價 (銷售金額/銷售量)
                        double _avgSalesAmount = _SalesNum > 0 ? (_SalesAmount / _SalesNum) : 0;
                        //運算-平均單位成本 (單位成本/銷售量)
                        double _avgPaperCost = _PaperCost > 0 ? (_PaperCost / _SalesNum) : 0;

                        //加入項目
                        var data = new ProdPlanRptItem
                        {
                            FromData = item.Field<string>("FromData"),
                            ItemNo = item.Field<string>("ItemNo"),
                            ModelNo = item.Field<string>("ModelNo"),
                            Model_Name = item.Field<string>("Model_Name"),
                            Class_ID = item.Field<int>("Class_ID"),
                            Class_Name = item.Field<string>("Class_Name"),
                            Vol = item.Field<string>("Vol"),
                            Page = item.Field<string>("Page"),
                            Ship_From = item.Field<string>("Ship_From"),
                            ListPic = item.Field<string>("ListPic"),
                            Menu_Lv1 = item.Field<int?>("Menu_Lv1"),
                            MenuNameLv1 = item.Field<string>("MenuNameLv1"),
                            Menu_Lv2 = item.Field<int?>("Menu_Lv2"),
                            MenuNameLv2 = item.Field<string>("MenuNameLv2"),
                            Menu_Lv3 = item.Field<int?>("Menu_Lv3"),
                            MenuNameLv3 = item.Field<string>("MenuNameLv3"),
                            StyleID = item.Field<int?>("StyleID"),
                            StyleName = item.Field<string>("StyleName"),
                            TargetMonth = item.Field<string>("TargetMonth"),
                            SaleYear = item.Field<string>("SaleYear"),
                            SalesNum = _SalesNum,
                            SalesAmount = _SalesAmount,
                            PaperCost = _PaperCost,
                            avgSalesAmount = _avgSalesAmount,
                            avgPaperCost = _avgPaperCost,
                            SupName = item.Field<string>("SupName"),
                            Remark = item.Field<string>("Remark"),
                            ProdDesc = item.Field<string>("ProdDesc"),
                            ProdFeature = item.Field<string>("ProdFeature"),
                            DataID = item.Field<Guid?>("DataID")
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


        #endregion



        #region -----// Create //-----


        #region *** 產品開發計劃 S ***

        /// <summary>
        /// [產品開發計劃] 新增資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Create_PostalData(ProdPlanDataItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @GetGuid AS uniqueidentifier");
                sql.AppendLine(" SET @GetGuid = (SELECT NEWID())");
                sql.AppendLine(" INSERT INTO ProductPlan_Data (");
                sql.AppendLine("  Data_ID, Class_ID, Class_Lv1, Class_Lv2");
                sql.AppendLine("  , ModelNo, ModelName, Pic, ShipFrom");
                sql.AppendLine("  , Supplier, TargetMonth, Remark");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @GetGuid, @Class_ID, @Class_Lv1, @Class_Lv2");
                sql.AppendLine("  , @ModelNo, @ModelName, @Pic, @ShipFrom");
                sql.AppendLine("  , @Supplier, @TargetMonth, @Remark");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" )");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Class_ID", instance.Class_ID);
                cmd.Parameters.AddWithValue("Class_Lv1", instance.Class_Lv1);
                cmd.Parameters.AddWithValue("Class_Lv2", instance.Class_Lv2);
                cmd.Parameters.AddWithValue("ModelNo", instance.ModelNo);
                cmd.Parameters.AddWithValue("ModelName", instance.ModelName);
                cmd.Parameters.AddWithValue("Pic", instance.Pic);
                cmd.Parameters.AddWithValue("ShipFrom", instance.ShipFrom);
                cmd.Parameters.AddWithValue("Supplier", instance.Supplier);
                cmd.Parameters.AddWithValue("TargetMonth", instance.TargetMonth);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }


        #endregion


        #endregion



        #region -----// Update //-----


        #region *** 產品開發計劃 S ***
        /// <summary>
        /// [產品開發計劃] 更新資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_PostalData(ProdPlanDataItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE ProductPlan_Data");
                sql.AppendLine(" SET ModelNo = @ModelNo, ModelName = @ModelName");
                sql.AppendLine("  , Pic = @Pic, ShipFrom = @ShipFrom");
                sql.AppendLine("  , Supplier = @Supplier, TargetMonth = @TargetMonth, Remark = @Remark");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("ModelNo", instance.ModelNo);
                cmd.Parameters.AddWithValue("ModelName", instance.ModelName);
                cmd.Parameters.AddWithValue("Pic", instance.Pic);
                cmd.Parameters.AddWithValue("ShipFrom", instance.ShipFrom);
                cmd.Parameters.AddWithValue("Supplier", instance.Supplier);
                cmd.Parameters.AddWithValue("TargetMonth", instance.TargetMonth);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }

        }


        /// <summary>
        /// [產品開發計劃] 備註填寫
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Check_ProdPlanRemarkData(ProdPlanRptItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                sql.AppendLine("IF (SELECT COUNT(*) FROM ProductPlan_Remark WHERE (SaleYear = @SaleYear) AND (ModelNo = @ModelNo)) > 0");
                sql.AppendLine("  BEGIN");
                sql.AppendLine("    UPDATE ProductPlan_Remark");
                sql.AppendLine("    SET SaleYear = @SaleYear, Remark = @Remark");
                sql.AppendLine("    WHERE (SaleYear = @SaleYear) AND (ModelNo = @ModelNo)");
                sql.AppendLine("  END");
                sql.AppendLine(" ELSE");
                sql.AppendLine("  BEGIN");
                sql.AppendLine("    INSERT INTO ProductPlan_Remark (");
                sql.AppendLine("        SaleYear, ModelNo, Remark");
                sql.AppendLine("    ) VALUES (");
                sql.AppendLine("        @SaleYear, @ModelNo, @Remark");
                sql.AppendLine("    )");
                sql.AppendLine("  END");

                //add params
                cmd.Parameters.AddWithValue("SaleYear", instance.SaleYear);
                cmd.Parameters.AddWithValue("ModelNo", instance.ModelNo);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }


        }


        #endregion *** 產品開發計劃 E ***


        #endregion



        #region -----// Delete //-----


        #region *** 產品開發計劃 S ***
        /// <summary>
        /// [製物工單] 刪除製物工單-所有資料
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete_PostalData(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM ProductPlan_Data WHERE (Data_ID = @DataID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
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

        #endregion

    }
}

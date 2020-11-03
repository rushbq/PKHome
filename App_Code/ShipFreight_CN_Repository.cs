using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using LinqToExcel;
using PKLib_Method.Methods;
using ShipFreight_CN.Models;

/*
  [發貨維護]-ShipFreight_CN
*/
namespace ShipFreight_CN.Controllers
{
    public class ShipFreight_CN_Repository
    {
        public string ErrMsg;

        #region -----// Read //-----

        #region *** 發貨 S ***
        /// <summary>
        /// 發貨總表(ShipFreight_CHN)
        /// </summary>
        /// <param name="search">查詢參數</param>
        /// <param name="sort">排序參數</param>
        /// <param name="type">資料區分:1=工具 / 2=玩具</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipFreightItem> GetShipFreightList(Dictionary<string, string> search, Dictionary<string, string> sort
            , string type, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ShipFreightItem> dataList = new List<ShipFreightItem>();
            StringBuilder sql = new StringBuilder();

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
                sql.AppendLine("  , 'SH' AS StockType");
                sql.AppendLine("  , Base.TG066 AS ContactWho");
                sql.AppendLine("  , 'A' AS DataType");
                sql.AppendLine(" FROM [SHPK2].dbo.COPTG Base WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002");
                sql.AppendLine(" WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')");

                switch (type)
                {
                    case "1":
                        //指定銷貨單別:內銷工具
                        sql.Append(" AND (Base.TG001 IN ('2313','2341','2342','2343','2345','23B2','23B3','23B6'))");
                        break;

                    default:
                        //指定銷貨單別:內銷科學玩具
                        sql.Append(" AND (Base.TG001 IN ('2380','2381','2382','2383'))");
                        break;

                }

                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG003, Base.TG004, Base.TG066, Base.TG045, Base.TG046");

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
                sql.AppendLine("  , 'SH' AS StockType");
                sql.AppendLine("  , OrdBase.TF015 AS ContactWho");
                sql.AppendLine("  , 'B' AS DataType");
                sql.AppendLine(" FROM [SHPK2].dbo.INVTG Base WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002");
                sql.AppendLine(" WHERE (Base.TG001 = '1302') AND (Base.TG022 = 'Y') AND (Base.TG024 <> 'Y')");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG007, OrdBase.TF003, OrdBase.TF005, OrdBase.TF015");
                sql.AppendLine(" )");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  ISNULL((SELECT Parent_ID FROM [PKEF].dbo.ShipFreightRel_CHN WHERE (ShipBase.Data_ID = Rel_ID)), ShipBase.Data_ID) AS RelID");
                sql.AppendLine("  , TblBase.*");
                sql.AppendLine("  , RTRIM(Cust.MA002) AS CustName");
                sql.AppendLine("  , ShipBase.Data_ID");
                sql.AppendLine("  , ShipBase.ShipDate, ShipBase.ShipComp, ShipBase.ShipWay");
                sql.AppendLine(" , (");
                sql.AppendLine("  CASE WHEN ISNULL(ShipBase.ShipWho, TblBase.ContactWho) = '' THEN");
                sql.AppendLine("   CASE WHEN TblBase.ContactWho = Cust.MA002 THEN Cust.MA005 ELSE TblBase.ContactWho END");
                sql.AppendLine("   ELSE ISNULL(ShipBase.ShipWho COLLATE Chinese_Taiwan_Stroke_BIN, TblBase.ContactWho) END");
                sql.AppendLine(" ) AS ShipWho");
                sql.AppendLine("  , ShipBase.Remark");
                sql.AppendLine("  , ShipDT.Data_ID AS DT_UID, ShipDT.ShipNo, ShipDT.ShipCnt, ISNULL(ShipDT.Freight, 0) Freight, ISNULL(ShipDT.FreightWay, '') FreightWay");
                sql.AppendLine("  , ShipComp.DisplayName AS ShipCompName");
                sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = ShipBase.Create_Who)) AS Create_Name");
                //是否已被合併
                sql.AppendLine("  , (SELECT COUNT(*) FROM [PKEF].dbo.ShipFreightRel_CHN WHERE (ShipBase.Data_ID = Rel_ID)) AS IsReled");
                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.COPMA Cust WITH(NOLOCK) ON TblBase.CustID = Cust.MA001");
                sql.AppendLine("  LEFT JOIN [PKEF].dbo.ShipFreight_CHN ShipBase ON TblBase.SO_Fid = ShipBase.ERP_So_Fid COLLATE Chinese_Taiwan_Stroke_BIN AND TblBase.SO_Sid = ShipBase.ERP_So_Sid COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("  LEFT JOIN [PKEF].dbo.ShipFreightDetail_CHN ShipDT ON ShipBase.Data_ID = ShipDT.Parent_ID");
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
                                //--單號keyword/物流單號/收件人
                                sql.Append(" AND (");
                                sql.Append("  (UPPER(TblBase.SO_Fid) + UPPER(TblBase.SO_Sid) LIKE '%' + UPPER(@keyword) + '%')");
                                sql.Append("  OR (UPPER(TblBase.SO_Fid) +'-'+ UPPER(TblBase.SO_Sid) LIKE '%' + UPPER(@keyword) + '%')");
                                sql.Append("  OR (ShipDT.ShipNo LIKE '%'+ @keyword +'%')");
                                sql.Append("  OR ((");
                                sql.Append("       CASE WHEN ISNULL(ShipBase.ShipWho, TblBase.ContactWho) = '' THEN");
                                sql.Append("       CASE WHEN TblBase.ContactWho = Cust.MA002 THEN Cust.MA005 ELSE TblBase.ContactWho END");
                                sql.Append("       ELSE ISNULL(ShipBase.ShipWho COLLATE Chinese_Taiwan_Stroke_BIN, TblBase.ContactWho) END");
                                sql.Append("   ) LIKE '%'+ @keyword +'%')");
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
                                sql.Append("  SELECT ERP_SO_Fid+ERP_SO_Sid COLLATE Chinese_Taiwan_Stroke_BIN FROM [PKEF].dbo.ShipFreightRel_CHN");
                                sql.Append(" ))");
                                sql.Append(" AND (ShipBase.ERP_SO_Fid IS NULL)");

                                break;

                            case "ShipComp":
                                //貨運公司
                                sql.Append(" AND (ShipBase.ShipComp = @ShipComp)");

                                cmd.Parameters.AddWithValue("ShipComp", item.Value);
                                break;

                            case "FreightWay":
                                sql.Append(" AND (ShipDT.FreightWay = @FreightWay)");

                                cmd.Parameters.AddWithValue("FreightWay", item.Value);
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
                        sql.AppendLine(" ORDER BY TblBase.SO_Date, TblBase.CustID, TblBase.SO_Fid, TblBase.SO_Sid");
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
                            Erp_SO_FID = item.Field<string>("SO_Fid"),
                            Erp_SO_SID = item.Field<string>("SO_Sid"),
                            Erp_SO_Date = item.Field<string>("SO_Date"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            TotalPrice = item.Field<decimal>("TotalPrice"),
                            StockType = item.Field<string>("StockType"),
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
                            Freight = item.Field<double?>("Freight"),
                            FreightWay = item.Field<string>("FreightWay")

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
        /// 取得明細資料 - 發貨(ShipFreight_CHN)
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
                sql.AppendLine(" SELECT Data_ID, ShipNo, ShipCnt, Freight, FreightWay");
                sql.AppendLine(" FROM ShipFreightDetail_CHN WITH(NOLOCK)");
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
                            Freight = item.Field<double>("Freight"),
                            FreightWay = item.Field<string>("FreightWay")
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
        /// 取得關聯資料 - 發貨(ShipFreight_CHN)
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
                sql.AppendLine(" FROM ShipFreightRel_CHN WITH(NOLOCK)");
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
        /// <param name="search">查詢參數</param>
        /// <param name="type">資料區分:1=工具 / 2=玩具</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipFreightItem> GetShipFreightSendList(Dictionary<string, string> search, string type, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ShipFreightItem> dataList = new List<ShipFreightItem>();
            StringBuilder sql = new StringBuilder();

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
                sql.AppendLine("  , 'SH' AS StockType");
                sql.AppendLine("  , Base.TG066 AS ContactWho");
                sql.AppendLine("  , 'A' AS DataType");
                sql.AppendLine(" FROM [SHPK2].dbo.COPTG Base WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002");
                sql.AppendLine(" WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')");
                switch (type)
                {
                    case "1":
                        //指定銷貨單別:內銷工具
                        sql.Append(" AND (Base.TG001 IN ('2313','2341','2342','2343','2345','23B2','23B3','23B6'))");
                        break;

                    default:
                        //指定銷貨單別:內銷科學玩具
                        sql.Append(" AND (Base.TG001 IN ('2380','2381','2382','2383'))");
                        break;

                }
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG003, Base.TG004, Base.TG066, Base.TG045, Base.TG046");

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
                sql.AppendLine("  , 'SH' AS StockType");
                sql.AppendLine("  , OrdBase.TF015 AS ContactWho");
                sql.AppendLine("  , 'B' AS DataType");
                sql.AppendLine(" FROM [SHPK2].dbo.INVTG Base WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002");
                sql.AppendLine(" WHERE (Base.TG001 = '1302') AND (Base.TG022 = 'Y') AND (Base.TG024 <> 'Y')");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG007, OrdBase.TF003, OrdBase.TF005, OrdBase.TF015");
                sql.AppendLine(" )");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  ISNULL((SELECT Parent_ID FROM [PKEF].dbo.ShipFreightRel_CHN WHERE (ShipBase.Data_ID = Rel_ID)), ShipBase.Data_ID) AS RelID");
                sql.AppendLine("  , TblBase.*");
                sql.AppendLine("  , RTRIM(Cust.MA002) AS CustName");
                /* CompID無資料時,帶入目前的ReqCompID */
                sql.AppendLine("  , ShipBase.Data_ID");
                sql.AppendLine("  , ShipBase.ShipDate, ShipBase.ShipComp, ShipBase.ShipWay");
                sql.AppendLine("  , (CASE WHEN TblBase.ContactWho = Cust.MA002 THEN Cust.MA005 ELSE ISNULL(ShipBase.ShipWho COLLATE Chinese_Taiwan_Stroke_BIN, TblBase.ContactWho) END) AS ShipWho");
                sql.AppendLine("  , ShipBase.Remark");
                sql.AppendLine("  , ShipDT.Data_ID AS DT_UID, ShipDT.ShipNo, ShipDT.ShipCnt, ISNULL(ShipDT.Freight, 0) Freight, ISNULL(ShipDT.FreightWay, '') FreightWay");
                sql.AppendLine("  , ShipComp.DisplayName AS ShipCompName");
                sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = ShipBase.Create_Who)) AS Create_Name");
                //是否已被合併
                sql.AppendLine("  , (SELECT COUNT(*) FROM [PKEF].dbo.ShipFreightRel_CHN WHERE (ShipBase.Data_ID = Rel_ID)) AS IsReled");
                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.COPMA Cust WITH(NOLOCK) ON TblBase.CustID = Cust.MA001");
                sql.AppendLine("  INNER JOIN [PKEF].dbo.ShipFreight_CHN ShipBase ON TblBase.SO_Fid = ShipBase.ERP_So_Fid COLLATE Chinese_Taiwan_Stroke_BIN AND TblBase.SO_Sid = ShipBase.ERP_So_Sid COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("  LEFT JOIN [PKEF].dbo.ShipFreightDetail_CHN ShipDT ON ShipBase.Data_ID = ShipDT.Parent_ID");
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

                                //case "ShipFrom":
                                //    //出貨地
                                //    sql.Append(" AND (TblBase.StockType = @ShipFrom)");

                                //    cmd.Parameters.AddWithValue("ShipFrom", item.Value);

                                //    break;

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
                            Erp_SO_FID = item.Field<string>("SO_Fid"),
                            Erp_SO_SID = item.Field<string>("SO_Sid"),
                            Erp_SO_Date = item.Field<string>("SO_Date"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            TotalPrice = item.Field<decimal>("TotalPrice"),
                            StockType = item.Field<string>("StockType"),
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
                            Freight = item.Field<double?>("Freight"),
                            FreightWay = item.Field<string>("FreightWay")
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
        /// <param name="type">資料區分:1=工具 / 2=玩具</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipStat_Year> GetShipStat_Year(string type, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ShipStat_Year> dataList = new List<ShipStat_Year>();
            StringBuilder sql = new StringBuilder();

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
                sql.AppendLine(" FROM [SHPK2].dbo.COPTG Base WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002");
                sql.AppendLine(" WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')");
                sql.AppendLine("  AND SUBSTRING(Base.TG003,1,4) BETWEEN YEAR(GETDATE()-365) and YEAR(GETDATE())");

                switch (type)
                {
                    case "1":
                        //指定銷貨單別:內銷工具
                        sql.Append(" AND (Base.TG001 IN ('2313','2341','2342','2343','2345','23B2','23B3','23B6'))");
                        break;

                    default:
                        //指定銷貨單別:內銷科學玩具
                        sql.Append(" AND (Base.TG001 IN ('2380','2381','2382','2383'))");
                        break;

                }

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
                sql.AppendLine(" FROM [SHPK2].dbo.INVTG Base WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002");
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
                sql.AppendLine("  , SUM(ISNULL(ShipDT.Freight, 0)) AS Freight");
                sql.AppendLine("  , ISNULL(SUM(ShipDT.Freight),0) AS AllPay");
                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN [PKEF].dbo.ShipFreight_CHN ShipBase ON TblBase.SO_Fid = ShipBase.ERP_So_Fid COLLATE Chinese_Taiwan_Stroke_BIN AND TblBase.SO_Sid = ShipBase.ERP_So_Sid COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("  INNER JOIN [PKEF].dbo.ShipFreightDetail_CHN ShipDT ON ShipBase.Data_ID = ShipDT.Parent_ID");
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
                                Freight = item.Field<double>("Freight"),
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
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetShipStat_Week(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

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
                sql.Append("  FROM [PKEF].dbo.ShipFreight_CHN ShipBase");
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

                                //case "ShipFrom":
                                //    //出貨地
                                //    sql.Append(" AND (ShipBase.ShipFrom = @ShipFrom)");

                                //    cmd.Parameters.AddWithValue("ShipFrom", item.Value);

                                //    break;
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
                sql.Append(" 	 LEFT JOIN Customer Cust ON TblData.CustID = Cust.MA001 AND DBS = DBC");
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
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// Logistics 為共用Table, 使用CompID判別
        /// </remarks>
        public IQueryable<ShipComp> GetShipComp(Dictionary<string, string> search, out string ErrMsg)
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
                sql.Append(" WHERE (CompID = 'CHN')");

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
        /// 取得物流途徑(ShipFreight_CHN)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string GetShipWay(string value)
        {
            switch (value)
            {
                case "A":
                    return "自發";

                case "B":
                    return "代發";

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
                string myErpID = "";
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

                    myErpID = val[35]; //單別-單號
                    myShipNo = val[4];
                    myShipDate = val[6].ToString().ToDateString("yyyy/MM/dd");
                    myQty = Convert.ToInt32(val[20]);
                    myPrice = Convert.ToDouble(val[40]);

                    #endregion


                    //加入項目
                    var data = new ShipImportDataDT
                    {
                        erpID = myErpID.Trim(),
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

                //[HTML] - 指定標題欄
                html.Append("<thead>");
                html.Append("<tr>");
                html.Append("<th>运单号</th><th>开单时间</th><th>货物件数</th><th>订单号</th><th>总运费</th>");
                html.Append("</tr>");
                html.Append("</thead>");

                //[HTML] - 取得欄位值, 輸出內容欄 (Worksheet)
                var queryVals = excelFile.Worksheet(sheetName);

                html.Append("<tbody>");
                foreach (var val in queryVals)
                {
                    //內容迴圈
                    html.Append("<tr>");
                    html.Append("<td>{0}</td>".FormatThis(val[4]));
                    html.Append("<td>{0}</td>".FormatThis(val[6]));
                    html.Append("<td>{0}</td>".FormatThis(val[20]));
                    html.Append("<td>{0}</td>".FormatThis(val[35]));
                    html.Append("<td>{0}</td>".FormatThis(val[40]));

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
        /// [發貨匯入] 取得匯入清單
        /// </summary>
        /// <param name="search"></param>
        /// <param name="compID">CHN1=內銷工具/CHN2=內銷玩具</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipImportData> GetShipImportList(Dictionary<string, string> search, string compID, out string ErrMsg)
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
                sql.AppendLine(" WHERE (CompID = @CompID)");

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
                cmd.Parameters.AddWithValue("CompID", compID);

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
                sql.AppendLine(" INSERT INTO ShipFreight_CHN( ");
                sql.AppendLine("  Data_ID, CustID");
                sql.AppendLine("  , ERP_SO_Fid, ERP_SO_Sid, ShipDate");
                sql.AppendLine("  , ShipComp, ShipWay, ShipWho");
                sql.AppendLine("  , Remark");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @CustID");
                sql.AppendLine("  , @ERP_SO_Fid, @ERP_SO_Sid, @ShipDate");
                sql.AppendLine("  , @ShipComp, @ShipWay, @ShipWho");
                sql.AppendLine("  , @Remark");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("CustID", instance.CustID);
                cmd.Parameters.AddWithValue("ERP_SO_Fid", instance.Erp_SO_FID);
                cmd.Parameters.AddWithValue("ERP_SO_Sid", instance.Erp_SO_SID);
                cmd.Parameters.AddWithValue("ShipDate", instance.ShipDate);
                cmd.Parameters.AddWithValue("ShipComp", instance.ShipComp == null ? (object)DBNull.Value : instance.ShipComp);
                cmd.Parameters.AddWithValue("ShipWay", instance.ShipWay);
                cmd.Parameters.AddWithValue("ShipWho", instance.ShipWho);
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
                sql.AppendLine("  SELECT ISNULL(MAX(Data_ID), 0) + 1 FROM ShipFreightDetail_CHN WHERE (Parent_ID = @ParentID)");
                sql.AppendLine(" );");
                sql.AppendLine(" INSERT INTO ShipFreightDetail_CHN( ");
                sql.AppendLine("  Parent_ID, Data_ID");
                sql.AppendLine("  , ShipNo, ShipCnt, Freight, FreightWay");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @ParentID, @NewDataID");
                sql.AppendLine("  , @ShipNo, @ShipCnt, @Freight, @FreightWay");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("ShipNo", instance.ShipNo);
                cmd.Parameters.AddWithValue("ShipCnt", instance.ShipCnt);
                cmd.Parameters.AddWithValue("Freight", instance.Freight);
                cmd.Parameters.AddWithValue("FreightWay", instance.FreightWay);

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
                sql.AppendLine(" IF (SELECT COUNT(*) FROM ShipFreightRel_CHN WHERE Rel_ID = @RelID) = 0");
                sql.AppendLine(" BEGIN");
                sql.AppendLine(" DECLARE @NewDataID AS INT");
                sql.AppendLine(" SET @NewDataID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(Data_ID), 0) + 1 FROM ShipFreightRel_CHN");
                sql.AppendLine(" );");
                sql.AppendLine(" INSERT INTO ShipFreightRel_CHN( ");
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
        /// <remarks>
        /// Logistics 為共用Table, 使用CompID判別
        /// </remarks>
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
                sql.AppendLine("  @DataID, 'CHN', @DisplayName, @Display, @Sort");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", New_ID);
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
        /// <param name="compID">CHN1=內銷工具/CHN2=內銷玩具</param>
        /// <returns></returns>
        /// <remarks>
        /// Ship_ImportData / Ship_ImportData_DT 為共用Table, 使用CompID判別
        /// </remarks>
        public bool CreateShipImport(ShipImportData instance, string compID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Ship_ImportData( ");
                sql.AppendLine("  Data_ID, TraceID, Status, Upload_File");
                sql.AppendLine("  , Create_Who, Create_Time, CompID");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @TraceID, 10, @Upload_File");
                sql.AppendLine("  , @Create_Who, GETDATE(), @CompID");
                sql.AppendLine(" );");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("CompID", compID);
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
                        sql.AppendLine("  Parent_ID, Data_ID, ShipNo, ShipDate, Qty, Freight, ERP_ID");
                        sql.AppendLine(" ) VALUES (");
                        sql.AppendLine("  @DataID, @NewID, N'{0}', N'{1}', {2}, {3}, N'{4}'".FormatThis(
                            item.ShipNo, item.ShipDate, item.Qty, item.Freight, item.erpID));
                        sql.AppendLine(" );");
                    }
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", baseData.Data_ID);
                cmd.Parameters.AddWithValue("Sheet_Name", baseData.Sheet_Name);
                cmd.Parameters.AddWithValue("Update_Who", baseData.Update_Who);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }



        #endregion *** 發貨 E ***

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
                sql.AppendLine(" UPDATE ShipFreight_CHN SET");
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
                sql.AppendLine(" UPDATE ShipFreightDetail_CHN SET");
                sql.AppendLine("  ShipNo = @ShipNo, ShipCnt = @ShipCnt");
                sql.AppendLine("  , Freight = @Freight");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID) AND (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("ShipNo", instance.ShipNo);
                cmd.Parameters.AddWithValue("ShipCnt", instance.ShipCnt);
                cmd.Parameters.AddWithValue("Freight", instance.Freight);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// 更新貨運公司資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        /// <remarks>
        /// Logistics 為共用Table, 使用CompID判別
        /// </remarks>
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
        /// [發貨匯入] 物流單資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool UpdateShipImport(ShipImportData instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" ;WITH TblData AS (");
                sql.AppendLine(" 	SELECT Data_ID AS ImpID, ShipNo, ShipDate, Qty, Freight, ERP_ID");
                sql.AppendLine(" 	FROM [PKEF].[dbo].[Ship_ImportData_DT]");
                sql.AppendLine(" 	WHERE (Parent_ID = @RefID)");
                sql.AppendLine(" )");
                sql.AppendLine(" , TblBase AS (");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid");
                sql.AppendLine("  , Base.TG003 AS SO_Date, Base.TG004 AS CustID");
                sql.AppendLine("  , Base.TG066 AS ContactWho");
                sql.AppendLine(" FROM [SHPK2].dbo.COPTG Base WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002");
                sql.AppendLine("  INNER JOIN TblData ON (RTRIM(Base.TG001) + '-' + RTRIM(Base.TG002)) = TblData.ERP_ID COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine(" WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG003, Base.TG004, Base.TG066");

                sql.AppendLine(" UNION ALL");

                sql.AppendLine(" SELECT");
                sql.AppendLine("  RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid");
                sql.AppendLine("  , OrdBase.TF003 AS SO_Date, OrdBase.TF005 AS CustID");
                sql.AppendLine("  , OrdBase.TF015 AS ContactWho");
                sql.AppendLine(" FROM [SHPK2].dbo.INVTG Base WITH(NOLOCK)");
                sql.AppendLine("  INNER JOIN [SHPK2].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002");
                sql.AppendLine("  INNER JOIN TblData ON (RTRIM(Base.TG001) + '-' + RTRIM(Base.TG002)) = TblData.ERP_ID COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine(" WHERE (Base.TG001 = '1302') AND (Base.TG022 = 'Y') AND (Base.TG024 <> 'Y')");
                sql.AppendLine(" GROUP BY Base.TG001, Base.TG002, Base.TG007, OrdBase.TF003, OrdBase.TF005, OrdBase.TF015");
                sql.AppendLine(" )");

                /* 新增至主檔 (確認Insert及Select欄位是否相符) */
                sql.AppendLine(" INSERT INTO [PKEF].dbo.ShipFreight_CHN(");
                sql.AppendLine("  Data_ID, CustID");
                sql.AppendLine("  , ERP_SO_Fid, ERP_SO_Sid, ShipDate");
                sql.AppendLine("  , ShipWay, ShipWho");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine("  , IsAuto, IsUpdate");
                sql.AppendLine("  , AutoShipNo, ShipCnt, ShipFreight");
                sql.AppendLine("  , RefID");
                sql.AppendLine(" )");
                sql.AppendLine(" SELECT NEWID(), TblBase.CustID");
                sql.AppendLine("  , TblBase.SO_Fid, TblBase.SO_Sid, TblData.ShipDate");
                sql.AppendLine("  , 'A' AS ShipWay, LEFT(TblBase.ContactWho, 50) AS ShipWho");
                sql.AppendLine("  , @Who AS CreateWho, GETDATE() AS CreateTime");
                sql.AppendLine("  , 'Y' AS IsAuto, 'N' AS IsUpdate");
                sql.AppendLine("  , LEFT(TblData.ShipNo, 50) AS AutoShipNo, TblData.Qty, TblData.Freight");
                sql.AppendLine("  , @RefID");
                sql.AppendLine(" FROM TblBase");
                sql.AppendLine("  INNER JOIN TblData ON (RTRIM(TblBase.SO_Fid) + '-' + RTRIM(TblBase.SO_Sid)) = TblData.ERP_ID COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine(" WHERE (TblData.ShipNo <> '')");
                sql.AppendLine("  AND ((TblBase.SO_Fid + TblBase.SO_Sid) COLLATE Chinese_Taiwan_Stroke_BIN NOT IN (");
                sql.AppendLine("   SELECT ERP_SO_Fid + ERP_SO_Sid");
                sql.AppendLine("   FROM [PKEF].dbo.ShipFreight_CHN");
                sql.AppendLine("  ))");

                /* 新增資料至物流關聯檔 */
                sql.AppendLine(" BEGIN");
                sql.AppendLine("  INSERT INTO [PKEF].dbo.ShipFreightDetail_CHN(");
                sql.AppendLine("   Parent_ID, Data_ID");
                sql.AppendLine("   , ShipNo, ShipCnt, Freight, FreightWay");
                sql.AppendLine("  )");
                sql.AppendLine("  SELECT Data_ID");
                sql.AppendLine("   , (SELECT ISNULL(MAX(Data_ID), 0) + 1 FROM [PKEF].dbo.ShipFreightDetail_CHN WHERE (Parent_ID = Src.Data_ID))");
                sql.AppendLine("   , Src.AutoShipNo, Src.ShipCnt, Src.ShipFreight, 'A'");
                sql.AppendLine("  FROM [PKEF].dbo.ShipFreight_CHN Src");
                sql.AppendLine("  WHERE (RefID = @RefID)");
                sql.AppendLine(" END");

                /* 更新狀態 IsUpdate=Y */
                sql.AppendLine(" BEGIN");
                sql.AppendLine("  UPDATE [PKEF].dbo.ShipFreight_CHN");
                sql.AppendLine("  SET IsUpdate = 'Y'");
                sql.AppendLine("  WHERE (RefID = @RefID)");
                sql.AppendLine(" END");

                //Update 匯入檔
                sql.AppendLine(" UPDATE Ship_ImportData_DT");
                sql.AppendLine(" SET IsPass = 'Y'");
                sql.AppendLine(" WHERE (Parent_ID = @RefID)");
                sql.AppendLine(" AND (ShipNo IN");
                sql.AppendLine("  (SELECT ShipNo FROM ShipFreightDetail_CHN)");
                sql.AppendLine(" );");

                sql.AppendLine(" UPDATE Ship_ImportData");
                sql.AppendLine(" SET Status = 30, Update_Who = @Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @RefID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("RefID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        #endregion *** 發貨 E ***

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
                sql.AppendLine(" DELETE FROM ShipFreightDetail_CHN WHERE (Parent_ID = @ParentID) AND (Data_ID = @DataID);");

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
                sql.AppendLine(" DELETE FROM ShipFreightRel_CHN WHERE (Data_ID = @DataID);");
                sql.AppendLine(" DELETE FROM ShipFreight_CHN WHERE (Data_ID = @RelID);");

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
                sql.AppendLine(" IF (SELECT COUNT(*) FROM ShipFreight_CHN WHERE (ShipComp = @DataID)) > 0");
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

        #endregion


    }
}

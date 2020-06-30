using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Menu3000Data.Models;
using PKLib_Method.Methods;

/*
 應收帳款對帳 - 營業
*/
namespace ARData.Controllers
{
    public class ARdataRepository
    {
        #region -----// Read //-----

        /// <summary>
        /// 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ARData_Base> GetOne(Dictionary<string, string> search
            , out string ErrMsg)
        {
            int dataCnt = 0;
            return GetList(search, 0, 1, out dataCnt, out ErrMsg);
        }

        public IQueryable<ARData_Base> GetAllList(Dictionary<string, string> search
            , out int DataCnt, out string ErrMsg)
        {
            return GetList(search, 0, 9999999, out DataCnt, out ErrMsg);
        }

        /// <summary>
        /// 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ARData_Base> GetList(Dictionary<string, string> search
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
                List<ARData_Base> dataList = new List<ARData_Base>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數

                //取得SQL語法
                subSql = _ListSQL(search);
                //取得SQL參數集合
                subParamList = _ListParams(search);


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
                    //sqlParamList.Add(new SqlParameter("@Lang", lang.ToUpper()));

                    //----- SQL 條件參數 -----
                    //加入篩選後的參數
                    sqlParamList.AddRange(subParamList);

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
                    using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new ARData_Base
                            {
                                Data_ID = item.Field<Guid>("Data_ID"),
                                SeqNo = item.Field<Int32>("SeqNo"),
                                TraceID = item.Field<string>("TraceID"),
                                CustID = item.Field<string>("CustID"),
                                CustName = item.Field<string>("CustName"),
                                CustFullName = item.Field<string>("CustFullName"),
                                DBS = item.Field<string>("DBS"),
                                erp_sDate = item.Field<DateTime>("erp_sDate").ToString().ToDateString("yyyy/MM/dd"),
                                erp_eDate = item.Field<DateTime>("erp_eDate").ToString().ToDateString("yyyy/MM/dd"),
                                Status = item.Field<Int16>("Status"),
                                // 10:填寫中 / 20:已寄送
                                StatusName = item.Field<Int16>("Status").Equals(20) ? "已寄送" : "填寫中",
                                ZipCode = item.Field<string>("ZipCode"),
                                Addr = item.Field<string>("Addr"),
                                AddrRemark = item.Field<string>("AddrRemark"),
                                Fax = item.Field<string>("Fax"),
                                Tel = item.Field<string>("Tel"),

                                ErrMessage = item.Field<string>("ErrMessage"),
                                ErrTime = item.Field<DateTime?>("ErrTime").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Send_Time = item.Field<DateTime?>("Send_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
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
        /// 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetTempList"/>
        private StringBuilder _ListSQL(Dictionary<string, string> search)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.SeqNo, Base.Data_ID, Base.TraceID");
            sql.AppendLine(" , Base.CustID, Base.DBS, Base.erp_sDate, Base.erp_eDate");
            sql.AppendLine(" , Base.[Status], Base.Send_Time");
            sql.AppendLine(" , ISNULL(Base.ErrMessage, '') AS ErrMessage, Base.ErrTime");
            sql.AppendLine(" , Base.Create_Who, Base.Create_Time, Base.Update_Who, Base.Update_Time");
            sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Create_Who)) AS Create_Name");
            sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Update_Who)) AS Update_Name");
            sql.AppendLine(" , ROW_NUMBER() OVER(ORDER BY Base.[Status], Base.Create_Time DESC) AS RowIdx");
            sql.AppendLine(" , RTRIM(Cust.MA002) AS CustName, RTRIM(Cust.MA003) AS CustFullName");
            sql.AppendLine(" , Cust.MA040 AS ZipCode, Cust.MA025 AS Addr, Cust.MA026 AS AddrRemark, Cust.MA008 AS Fax, Cust.MA006 AS Tel");
            sql.AppendLine(" FROM [PKExcel].dbo.AR_Data Base");
            sql.AppendLine("  LEFT JOIN [PKSYS].dbo.Customer Cust ON Base.CustID COLLATE Chinese_Taiwan_Stroke_BIN = Cust.MA001 AND Cust.DBS = Cust.DBC");
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

                            break;

                        case "Keyword":
                            //關鍵字
                            sql.Append(" AND (");
                            sql.Append(" (UPPER(Base.TraceID) LIKE '%' + UPPER(@keyword) + '%')");
                            sql.Append(" OR (UPPER(Base.CustID) LIKE '%' + UPPER(@keyword) + '%')");
                            sql.Append(" OR (");
                            sql.Append("  Base.CustID IN (SELECT TOP 1 RTRIM(MA001) FROM PKSYS.dbo.Customer WHERE (MA001 = Base.CustID) AND (DBS = DBC) AND (UPPER(RTRIM(MA002)) LIKE '%' + UPPER(@keyword) + '%'))");
                            sql.Append(" )");
                            sql.Append(")");

                            break;

                        case "DBS":
                            sql.Append(" AND (Base.DBS = @DBS)");
                            break;

                        case "sDate":
                            //建立日期
                            sql.Append(" AND (Base.Create_Time >= @sDate)");
                            break;
                        case "eDate":
                            sql.Append(" AND (Base.Create_Time <= @eDate)");
                            break;

                    }
                }
            }
            #endregion

            return sql;
        }


        /// <summary>
        /// 取得條件參數
        /// ** SQL參數設定寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetTempList"/>
        private List<SqlParameter> _ListParams(Dictionary<string, string> search)
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

                        case "DBS":
                            sqlParamList.Add(new SqlParameter("@DBS", item.Value));
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
        /// 取得結帳單, Step2
        /// </summary>
        /// <param name="dbs"></param>
        /// <param name="parentID"></param>
        /// <param name="search">日期格式為yyyyMMdd</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ARData_Details> GetErpList(string dbs, string parentID, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ARData_Details> dataList = new List<ARData_Details>();
            StringBuilder sql = new StringBuilder();
            /* 設定DB Name */
            string _dbName;
            //來源DB
            switch (dbs.ToUpper())
            {
                case "SZ":
                    _dbName = "ProUnion";
                    break;

                case "SH":
                    _dbName = "SHPK2";
                    break;

                default:
                    _dbName = "prokit2";
                    break;
            }

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT RTRIM(Base.TA001) AS AR_Fid, RTRIM(Base.TA002) AS AR_Sid");
                sql.AppendLine("  , Terms.NA002 AS TermID, Terms.NA003 AS TermName");
                sql.AppendLine("  , Base.TA003 AS ArDate, Base.TA015 AS BillNo, Base.TA020 AS PreGetDay, Base.TA009 AS Currency");
                sql.AppendLine("  , CAST(Base.TA029 AS float) AS Price, CAST(Base.TA042 AS float) AS TaxPrice, CAST(Base.TA031 AS float) AS GetPrice");
                sql.AppendLine("  , Base.TA022 AS OrderRemark");
                sql.AppendLine("  , ROW_NUMBER() OVER(ORDER BY Base.TA002) AS SerialNo");
                sql.AppendLine(" FROM [##dbName##].dbo.ACRTA Base");
                sql.AppendLine("  INNER JOIN [##dbName##].dbo.CMSNA Terms ON Base.TA043 = Terms.NA002 AND Terms.NA001 = 2");
                sql.AppendLine(" WHERE (TA025 = 'Y') AND (TA027 = 'N')");
                //--排除重複(20200623-Annie說不要擋)
                //sql.AppendLine(" AND (RTRIM(Base.TA001)+'-'+RTRIM(Base.TA002) NOT IN (");
                //sql.AppendLine(" 	SELECT subItems.Erp_AR_ID COLLATE Chinese_Taiwan_Stroke_BIN");
                //sql.AppendLine(" 	FROM [PKExcel].dbo.AR_DataItems subItems");
                //sql.AppendLine(" 	WHERE (subItems.Parent_ID <> @parentID)");
                //sql.AppendLine(" ))");

                #region >> filter <<
                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "CustID":
                                sql.Append(" AND (Base.TA004 = @CustID)");

                                cmd.Parameters.AddWithValue("CustID", item.Value);
                                break;


                            case "StartDate":
                                sql.Append(" AND (Base.TA003 >= @StartDate)");

                                cmd.Parameters.AddWithValue("StartDate", item.Value);
                                break;


                            case "EndDate":
                                sql.Append(" AND (Base.TA003 <= @EndDate)");

                                cmd.Parameters.AddWithValue("EndDate", item.Value);
                                break;

                        }
                    }
                }
                #endregion

                //order by
                sql.AppendLine(" ORDER BY Base.TA002");

                //Replace DB 前置詞
                sql.Replace("##dbName##", _dbName);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("parentID", parentID);

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
                            var data = new ARData_Details
                            {
                                SerialNo = item.Field<Int64>("SerialNo"),
                                AR_Fid = item.Field<string>("AR_Fid"),
                                AR_Sid = item.Field<string>("AR_Sid"),
                                //付款條件
                                TermID = item.Field<string>("TermID"),
                                TermName = item.Field<string>("TermName"),
                                ArDate = item.Field<string>("ArDate"),
                                BillNo = item.Field<string>("BillNo"),
                                PreGetDay = item.Field<string>("PreGetDay"),
                                Currency = item.Field<string>("Currency"),
                                Price = item.Field<double>("Price"), //應收金額
                                TaxPrice = item.Field<double>("TaxPrice"), //本幣營業稅額
                                GetPrice = item.Field<double>("GetPrice"), //已收金額
                                OrderRemark = item.Field<string>("OrderRemark")
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
        /// 取得對帳明細, Step3 / View
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="dbs"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ARData_Details> GetGrid(string parentID, string dbs, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ARData_Details> dataList = new List<ARData_Details>();
            StringBuilder sql = new StringBuilder();
            /* 設定DB Name */
            string _dbName;
            //來源DB
            switch (dbs.ToUpper())
            {
                case "SZ":
                    _dbName = "ProUnion";
                    break;

                case "SH":
                    _dbName = "SHPK2";
                    break;

                default:
                    _dbName = "prokit2";
                    break;
            }

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT RTRIM(Base.TA001) AS AR_Fid, RTRIM(Base.TA002) AS AR_Sid");
                sql.AppendLine("  , Base.TA004 AS CustID, Base.TA008 AS CustName");
                sql.AppendLine("  , DT.TB005 AS AT_Fid, DT.TB006 AS AT_Sid, DT.TB007 AS AT_Tid");
                sql.AppendLine("  , Terms.NA002 AS TermID, Terms.NA003 AS TermName");
                sql.AppendLine("  , Base.TA003 AS ArDate, Base.TA015 AS BillNo, Base.TA020 AS PreGetDay, Base.TA009 AS Currency");
                sql.AppendLine("  , CAST(Base.TA029 AS float) AS Price, CAST(Base.TA042 AS float) AS TaxPrice, CAST(Base.TA031 AS float) AS GetPrice");
                sql.AppendLine("  , Base.TA022 AS OrderRemark");
                sql.AppendLine(" FROM [##dbName##].dbo.ACRTA Base");
                sql.AppendLine("  INNER JOIN [##dbName##].dbo.ACRTB DT ON Base.TA001 = DT.TB001 AND Base.TA002 = DT.TB002");
                sql.AppendLine("  INNER JOIN [##dbName##].dbo.CMSNA Terms ON Base.TA043 = Terms.NA002 AND Terms.NA001 = 2");
                sql.AppendLine(" WHERE (TA025 = 'Y') AND (TA027 = 'N')");
                sql.AppendLine(" AND (RTRIM(Base.TA001)+'-'+RTRIM(Base.TA002) IN (");
                sql.AppendLine("     SELECT subItems.Erp_AR_ID COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("     FROM [PKExcel].dbo.AR_Data subBase");
                sql.AppendLine("      INNER JOIN [PKExcel].dbo.AR_DataItems subItems ON subBase.Data_ID = subItems.Parent_ID");
                sql.AppendLine("     WHERE (subBase.Data_ID = @parentID)");
                sql.AppendLine(" ))");

                //order by
                sql.AppendLine(" ORDER BY Base.TA002");

                //Replace DB 前置詞
                sql.Replace("##dbName##", _dbName);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("parentID", parentID);

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
                            var data = new ARData_Details
                            {
                                AR_Fid = item.Field<string>("AR_Fid"),
                                AR_Sid = item.Field<string>("AR_Sid"),
                                CustID = item.Field<string>("CustID"),
                                CustName = item.Field<string>("CustName"),
                                //憑證號碼
                                AT_Fid = item.Field<string>("AT_Fid"),
                                AT_Sid = item.Field<string>("AT_Sid"),
                                AT_Tid = item.Field<string>("AT_Tid"),
                                //付款條件
                                TermID = item.Field<string>("TermID"),
                                TermName = item.Field<string>("TermName"),
                                ArDate = item.Field<string>("ArDate"),
                                BillNo = item.Field<string>("BillNo"),
                                PreGetDay = item.Field<string>("PreGetDay"),
                                Currency = item.Field<string>("Currency"),
                                Price = item.Field<double>("Price"), //應收金額
                                TaxPrice = item.Field<double>("TaxPrice"), //本幣營業稅額
                                GetPrice = item.Field<double>("GetPrice"), //已收金額
                                OrderRemark = item.Field<string>("OrderRemark")
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
        /// 價格資訊
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="dbs"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ARData_PriceInfo> GetFooterInfo(string parentID, string dbs, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ARData_PriceInfo> dataList = new List<ARData_PriceInfo>();
            StringBuilder sql = new StringBuilder();
            /* 設定DB Name */
            string _dbName;
            //來源DB
            switch (dbs.ToUpper())
            {
                case "SZ":
                    _dbName = "ProUnion";
                    break;

                case "SH":
                    _dbName = "SHPK2";
                    break;

                default:
                    _dbName = "prokit2";
                    break;
            }

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" ;WITH TblPreGet AS (");
                sql.AppendLine("     SELECT ISNULL(SUM(TA029+TA042), 0) AS PrePrice");
                sql.AppendLine("     , COUNT(*) AS Cnt");
                sql.AppendLine("     , TA004 AS CustID");
                sql.AppendLine("     FROM [##dbName##].dbo.ACRTA");
                sql.AppendLine("     LEFT JOIN [PKExcel].dbo.AR_Data Base ON TA004 = Base.CustID COLLATE Chinese_Taiwan_Stroke_BIN AND TA003 < Base.erp_sDate");
                sql.AppendLine("     WHERE (TA025 = 'Y') AND (TA027 = 'N') AND (Base.Data_ID = @parentID)");
                sql.AppendLine("     GROUP BY TA004");
                sql.AppendLine(" )");
                sql.AppendLine(" , TblWishGet AS (");
                sql.AppendLine("     SELECT ISNULL(SUM(TA029+TA042), 0) AS TotalPrice");
                sql.AppendLine("     , ISNULL(SUM(TA029), 0) AS TotalPrice_NoTax");
                sql.AppendLine("     , ISNULL(SUM(TA042), 0) AS TotalTaxPrice");
                sql.AppendLine("     , COUNT(*) AS Cnt");
                sql.AppendLine("     , TA004 AS CustID");
                sql.AppendLine("     FROM [##dbName##].dbo.ACRTA");
                sql.AppendLine("     WHERE (TA025 = 'Y') AND (TA027 = 'N')");
                sql.AppendLine("     AND (RTRIM(TA001)+'-'+RTRIM(TA002) IN (");
                sql.AppendLine("         SELECT subItems.Erp_AR_ID COLLATE Chinese_Taiwan_Stroke_BIN");
                sql.AppendLine("         FROM [PKExcel].dbo.AR_DataItems subItems");
                sql.AppendLine("         WHERE (subItems.Parent_ID = @parentID)");
                sql.AppendLine("     ))");
                sql.AppendLine("     GROUP BY TA004");
                sql.AppendLine(" )");
                sql.AppendLine(" SELECT");
                sql.AppendLine("  ISNULL(CAST(TblPreGet.PrePrice AS float), 0) AS PrePrice");
                sql.AppendLine("  , ISNULL(CAST(TblPreGet.Cnt AS int), 0) AS PreCnt");
                sql.AppendLine("  , ISNULL(CAST(TblWishGet.TotalPrice AS float), 0) AS TotalPrice");
                sql.AppendLine("  , ISNULL(CAST(TblWishGet.TotalPrice_NoTax AS float), 0) AS TotalPrice_NoTax");
                sql.AppendLine("  , ISNULL(CAST(TblWishGet.TotalTaxPrice AS float), 0) AS TotalTaxPrice");
                sql.AppendLine("  , ISNULL(CAST(TblWishGet.Cnt AS int), 0) AS Cnt");
                sql.AppendLine(" FROM TblPreGet FULL OUTER JOIN TblWishGet ON TblPreGet.CustID = TblWishGet.CustID");


                //Replace DB 前置詞
                sql.Replace("##dbName##", _dbName);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("parentID", parentID);

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
                            var data = new ARData_PriceInfo
                            {
                                PrePrice = item.Field<double>("PrePrice"), //前期未收款
                                PreCnt = item.Field<int>("PreCnt"), //前期未收款筆數
                                TotalPrice = item.Field<double>("TotalPrice"), //本期應收總額
                                TotalPrice_NoTax = item.Field<double>("TotalPrice_NoTax"), //本幣未稅金額
                                TotalTaxPrice = item.Field<double>("TotalTaxPrice"), //本幣稅額
                                Cnt = item.Field<int>("Cnt"),
                                AllPrice = item.Field<double>("PrePrice") + item.Field<double>("TotalPrice")
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


        public IQueryable<ARData_Addressbook> GetAddressbook(string custID, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ARData_Addressbook> dataList = new List<ARData_Addressbook>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Email");
                sql.AppendLine(" FROM [PKSYS].dbo.Customer_Addressbook");
                sql.AppendLine(" WHERE (ERP_ID = @CustID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("CustID", custID);

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
                            var data = new ARData_Addressbook
                            {
                                Email = item.Field<string>("Email")
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
        /// 待收款的客戶
        /// </summary>
        /// <param name="dbs"></param>
        /// <param name="search">日期格式為yyyyMMdd</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ARData_Base> GetCustList(string dbs, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ARData_Base> dataList = new List<ARData_Base>();
            StringBuilder sql = new StringBuilder();
            /* 設定DB Name */
            string _dbName;
            //來源DB
            switch (dbs.ToUpper())
            {
                case "SZ":
                    _dbName = "ProUnion";
                    break;

                case "SH":
                    _dbName = "SHPK2";
                    break;

                default:
                    _dbName = "prokit2";
                    break;
            }

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT RTRIM(Cust.MA001) AS CustID, RTRIM(Cust.MA002) AS CustName, Terms.NA003 AS TermName");
                sql.AppendLine(" FROM [##dbName##].dbo.ACRTA Base");
                sql.AppendLine("  INNER JOIN [##dbName##].dbo.CMSNA Terms ON Base.TA043 = Terms.NA002 AND Terms.NA001 = 2");
                sql.AppendLine("  INNER JOIN [##dbName##].dbo.COPMA Cust ON Base.TA004 = Cust.MA001");
                sql.AppendLine(" WHERE (Base.TA025 = 'Y') AND (Base.TA027 = 'N') AND (Base.TA009 = 'NTD')");

                #region >> filter <<
                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            //case "CustID":
                            //    sql.Append(" AND (Base.TA004 = @CustID)");

                            //    cmd.Parameters.AddWithValue("CustID", item.Value);
                            //    break;


                            case "StartDate":
                                sql.Append(" AND (Base.TA003 >= @StartDate)");

                                cmd.Parameters.AddWithValue("StartDate", item.Value);
                                break;


                            case "EndDate":
                                sql.Append(" AND (Base.TA003 <= @EndDate)");

                                cmd.Parameters.AddWithValue("EndDate", item.Value);
                                break;

                        }
                    }
                }
                #endregion


                //order by
                sql.AppendLine(" GROUP BY Cust.MA001, Cust.MA002, Terms.NA003");
                sql.AppendLine(" ORDER BY Cust.MA001, Cust.MA002");

                //Replace DB 前置詞
                sql.Replace("##dbName##", _dbName);

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

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
                            var data = new ARData_Base
                            {
                                CustID = item.Field<string>("CustID"),
                                CustName = item.Field<string>("CustName"),
                                TermName = item.Field<string>("TermName")
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


        #endregion


        #region -----// Create //-----
        /// <summary>
        /// [Step1] 建立基本資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Create(ARData_Base instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO AR_Data (");
                sql.AppendLine("  Data_ID, TraceID, CustID, DBS");
                sql.AppendLine("  , erp_sDate, erp_eDate, Status");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @TraceID, @CustID, @DBS");
                sql.AppendLine("  , @erp_sDate, @erp_eDate, @Status");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" )");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("TraceID", instance.TraceID);
                cmd.Parameters.AddWithValue("CustID", instance.CustID);
                cmd.Parameters.AddWithValue("DBS", instance.DBS);
                cmd.Parameters.AddWithValue("erp_sDate", instance.erp_sDate.ToString().ToDateString("yyyy/MM/dd"));
                cmd.Parameters.AddWithValue("erp_eDate", instance.erp_eDate.ToString().ToDateString("yyyy/MM/dd"));
                cmd.Parameters.AddWithValue("Status", 10);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }

        }


        /// <summary>
        /// [Step2] 建立單身, 更新主檔欄位
        /// </summary>
        /// <param name="parentID">單頭ID</param>
        /// <param name="query">單身資料</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateDetail(string parentID, IQueryable<ARData_Items> query, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM AR_DataItems WHERE (Parent_ID = @DataID);");
                sql.AppendLine(" UPDATE AR_Data SET Update_Who = @Update_Who, Update_Time = GETDATE() WHERE (Data_ID = @DataID);");

                sql.AppendLine(" DECLARE @NewID AS INT ");

                foreach (var item in query)
                {
                    if (!string.IsNullOrWhiteSpace(item.Erp_AR_ID))
                    {
                        sql.AppendLine(" SET @NewID = (");
                        sql.AppendLine("  SELECT ISNULL(MAX(Data_ID), 0) + 1 AS NewID");
                        sql.AppendLine("  FROM AR_DataItems");
                        sql.AppendLine("  WHERE (Parent_ID = @DataID)");
                        sql.AppendLine(" )");

                        sql.AppendLine(" INSERT INTO AR_DataItems( ");
                        sql.AppendLine("  Parent_ID, Data_ID, Erp_AR_ID");
                        sql.AppendLine(" ) VALUES (");
                        sql.AppendLine("  @DataID, @NewID, '{0}'".FormatThis(
                            item.Erp_AR_ID));
                        sql.AppendLine(" );");
                    }
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", parentID);
                cmd.Parameters.AddWithValue("Update_Who", fn_Param.CurrentUser);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }

        }


        #endregion



        #region -----// Update //-----

        public bool UpdateStatus(string dataID,out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE AR_Data SET ");
                sql.AppendLine("  Status = 20, Send_Time = GETDATE()");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);
                cmd.Parameters.AddWithValue("Update_Who", fn_Param.CurrentUser);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }


        #endregion



        #region -----// Delete //-----

        /// <summary>
        /// 刪除所有資料
        /// </summary>
        /// <param name="dataID">資料編號</param>
        /// <returns></returns>
        public bool Delete(string dataID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM AR_DataItems WHERE (Parent_ID = @DataID);");
                sql.AppendLine(" DELETE FROM AR_Data WHERE (Data_ID = @DataID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }

        #endregion

    }
}

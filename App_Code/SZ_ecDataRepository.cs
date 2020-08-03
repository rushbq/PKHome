using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using LinqToExcel;
using Menu3000Data.Models;
using PKLib_Method.Methods;

/*
 電商平台業績 - SZ
  DB = ReportCenter
*/
namespace SZ_ecData.Controllers
{
    public class SZ_ecDataRepository
    {
        #region -----// Read //-----

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
                                Profit = item.Field<double?>("Profit"),
                                Profit_Percent = item.Field<double?>("Profit_Percent"),
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
            sql.AppendLine("  , Base.Profit, Base.Profit_Percent");
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


        #endregion


        #region -----// Create //-----


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
                sql.AppendLine("  , Profit, Profit_Percent");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @RefType, @RefMall, @setYear, @setMonth");
                sql.AppendLine("  , @Price_Income, @Price_SalesRebate, @Price_Cost, @Price_Profit");
                sql.AppendLine("  , @Price_Purchase, @Price_Back, @Price_PurchaseRebate, @Price_Freight");
                sql.AppendLine("  , @Profit, @Profit_Percent");
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
                cmd.Parameters.AddWithValue("Profit", instance.Profit);
                cmd.Parameters.AddWithValue("Profit_Percent", instance.Profit_Percent);
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


        #endregion



        #region -----// Update //-----

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
                sql.AppendLine("  , Profit = @Profit, Profit_Percent = @Profit_Percent");
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
                cmd.Parameters.AddWithValue("Profit", instance.Profit);
                cmd.Parameters.AddWithValue("Profit_Percent", instance.Profit_Percent);
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


        #endregion


    }
}

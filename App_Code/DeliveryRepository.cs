using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using LinqToExcel;
using Menu3000Data.Models;
using PKLib_Method.Methods;

/*
 應收帳款對帳 - 營業
*/
namespace DeliveryData.Controllers
{
    public class DeliveryRepository
    {
        #region -----// Read //-----

        /// <summary>
        /// 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<Delivery_Base> GetOne(Dictionary<string, string> search
            , out string ErrMsg)
        {
            int dataCnt = 0;
            return GetList(search, 0, 1, out dataCnt, out ErrMsg);
        }

        public IQueryable<Delivery_Base> GetAllList(Dictionary<string, string> search
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
        public IQueryable<Delivery_Base> GetList(Dictionary<string, string> search
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
                List<Delivery_Base> dataList = new List<Delivery_Base>(); //資料容器
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
                    using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                    {
                        //LinQ 查詢
                        var query = DT.AsEnumerable();

                        //資料迴圈
                        foreach (var item in query)
                        {
                            //加入項目
                            var data = new Delivery_Base
                            {
                                Data_ID = item.Field<Guid>("Data_ID"),
                                SeqNo = item.Field<Int32>("SeqNo"),
                                TraceID = item.Field<string>("TraceID"),
                                ShipType = item.Field<Int16>("ShipType"),
                                ShipTypeName = GetTypeDesc(item.Field<Int16>("ShipType")),
                                ShipWay = item.Field<Int32>("ShipWay"),
                                ShipWayName = item.Field<string>("ShipWayName"),
                                PayWay = item.Field<Int32>("PayWay"),
                                PayWayName = item.Field<string>("PayWayName"),
                                ShipWho = item.Field<string>("ShipWho"),
                                ShipWho_Name = item.Field<string>("ShipWho_Name"),
                                SendDate = item.Field<DateTime>("SendDate").ToString().ToDateString("yyyy/MM/dd"),
                                SendComp = item.Field<string>("SendComp"),
                                SendWho = item.Field<string>("SendWho"),
                                SendAddr = item.Field<string>("SendAddr"),
                                SendTel = item.Field<string>("SendTel"),
                                ShipNo = item.Field<string>("ShipNo"),
                                ShipPay = item.Field<double>("ShipPay"),
                                Box = item.Field<Int32?>("Box"),
                                BoxClass1 = item.Field<Int32?>("BoxClass1"),
                                BoxClass1Name = item.Field<string>("BoxClass1Name"),
                                BoxClass2 = item.Field<Int32?>("BoxClass2"),
                                BoxClass2Name = item.Field<string>("BoxClass2Name"),
                                TargetClass = item.Field<Int32?>("TargetClass"),
                                TargetName = item.Field<string>("TargetName"),
                                Remark1 = item.Field<string>("Remark1"),
                                Remark2 = item.Field<string>("Remark2"),
                                PurNo = item.Field<string>("PurNo"),
                                SaleNo = item.Field<string>("SaleNo"),
                                InvoiceNo = item.Field<string>("InvoiceNo"),
                                IsClose = item.Field<string>("IsClose"),
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
            string sqlstr = @"
 SELECT Base.SeqNo, Base.Data_ID, Base.TraceID
 , Base.ShipType, Base.ShipWho, Base.SendDate, Base.SendComp
 , Base.SendWho, Base.SendAddr, Base.SendTel
 , Base.ShipNo, Base.ShipPay, Base.Box
 , Cls1.Class_ID AS ShipWay, Cls1.Class_Name AS ShipWayName
 , Cls2.Class_ID AS PayWay, Cls2.Class_Name AS PayWayName
 , Cls3.Class_ID AS BoxClass1, Cls3.Class_Name AS BoxClass1Name
 , Cls4.Class_ID AS BoxClass2, Cls4.Class_Name AS BoxClass2Name
 , Cls5.Class_ID AS TargetClass, Cls5.Class_Name AS TargetName
 , Base.Remark1, Base.Remark2, Base.Purno, Base.SaleNo, Base.InvoiceNo, Base.IsClose
 , Base.Create_Who, Base.Create_Time, Base.Update_Who, Base.Update_Time
 , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Create_Who)) AS Create_Name
 , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Update_Who)) AS Update_Name
 , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Account_Name = Base.ShipWho)) AS ShipWho_Name
 , ROW_NUMBER() OVER(ORDER BY Base.Create_Time DESC) AS RowIdx
 FROM Delivery_Data Base
  --//寄送方式(ShipWay)
  INNER JOIN Delivery_RefClass Cls1 ON Base.ShipWay = Cls1.Class_ID
  --//付款方式(PayWay)
  INNER JOIN Delivery_RefClass Cls2 ON Base.PayWay = Cls2.Class_ID
  --//內容物分類1(BoxClass1)
  LEFT JOIN Delivery_RefClass Cls3 ON Base.BoxClass1 = Cls3.Class_ID
  --//內容物分類2(BoxClass2)
  LEFT JOIN Delivery_RefClass Cls4 ON Base.BoxClass2 = Cls4.Class_ID
  --//對象分類(TargetClass)
  LEFT JOIN Delivery_RefClass Cls5 ON Base.TargetClass = Cls5.Class_ID
 WHERE (1=1)
";

            sql.Append(sqlstr);

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
                            sql.Append(" OR (UPPER(Base.SendComp) LIKE '%' + UPPER(@keyword) + '%')");
                            sql.Append(")");

                            break;

                        case "ShipWay":
                            //寄送方式
                            sql.Append(" AND (Base.ShipWay = @ShipWay)");

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

                        case "ShipWay":
                            //寄送方式
                            sqlParamList.Add(new SqlParameter("@ShipWay", item.Value));

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
        /// 類別描述
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private string GetTypeDesc(int val)
        {
            switch (val)
            {
                case 1:
                    return "寄件";

                case 2:
                    return "收件";

                case 3:
                    return "來回件";

                default:
                    return "";
            }
        }


        /// <summary>
        /// 類別
        /// </summary>
        /// <param name="type"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// 1.寄送方式 2.付款方式 3.內容物分類一 4.內容物分類二 5.對象分類
        /// </remarks>
        public IQueryable<ClassItem> GetRefClass(string type, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name AS Label");
                sql.AppendLine(" FROM Delivery_RefClass WITH(NOLOCK)");
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



        /// <summary>
        /// 取得Excel欄位 - 匯入物流單號時使用
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="sheetName"></param>
        /// <returns>
        /// </returns>
        public IQueryable<Delivery_Import> GetExcel_ShipNoData(string filePath, string sheetName)
        {
            try
            {
                //----- 宣告 -----
                List<Delivery_Import> dataList = new List<Delivery_Import>();

                //[Excel] - 取得原始資料
                var excelFile = new ExcelQueryFactory(filePath);
                var queryVals = excelFile.Worksheet(sheetName);

                //宣告各內容參數
                string _TraceID = ""; //單號
                string _ShipNo = ""; //物流單號
                double _Freight = 0; //運費

                //資料迴圈
                foreach (var val in queryVals)
                {
                    _TraceID = val[0];
                    _ShipNo = val[1];
                    _Freight = Convert.ToDouble(val[2]);

                    //加入項目
                    var data = new Delivery_Import
                    {
                        TraceID = _TraceID,
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


        #endregion


        #region -----// Create //-----
        /// <summary>
        /// 建立資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Create(Delivery_Base instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                //取得單號(共12碼:西元年 + 月 + 日 + 4碼流水號)
                sql.AppendLine(" DECLARE @NewID AS INT, @OrderDate AS VARCHAR(8), @NewOrderID AS VARCHAR(12)");
                sql.AppendLine(" SET @OrderDate = (SELECT CONVERT(VARCHAR(8),GETDATE(), 112))");
                sql.AppendLine(" SET @NewID = (");
                sql.AppendLine("  SELECT ISNULL(MAX(RIGHT(TraceID,4)), 0) + 1 AS NewID");
                sql.AppendLine("  FROM Delivery_Data");
                sql.AppendLine("  WHERE LEFT(TraceID, 8) = @OrderDate");
                sql.AppendLine(" )");
                sql.AppendLine(" SET @NewOrderID = @OrderDate + RIGHT('000' + CAST(@NewID AS VARCHAR), 4);");

                //新增語法
                sql.AppendLine(" INSERT INTO Delivery_Data (");
                sql.AppendLine("  Data_ID, TraceID, ShipType, ShipWay, PayWay");
                sql.AppendLine("  , ShipWho, SendDate, SendComp, SendWho, SendAddr, SendTel");
                sql.AppendLine("  , ShipNo, ShipPay, Box, BoxClass1, BoxClass2, TargetClass");
                sql.AppendLine("  , Remark1, Remark2, PurNo, SaleNo, InvoiceNo, IsClose");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @NewOrderID, @ShipType, @ShipWay, @PayWay");
                sql.AppendLine("  , @ShipWho, @SendDate, @SendComp, @SendWho, @SendAddr, @SendTel");
                sql.AppendLine("  , @ShipNo, @ShipPay, @Box, @BoxClass1, @BoxClass2, @TargetClass");
                sql.AppendLine("  , @Remark1, @Remark2, @PurNo, @SaleNo, @InvoiceNo, 'N'");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" )");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("ShipType", instance.ShipType);
                cmd.Parameters.AddWithValue("ShipWay", instance.ShipWay);
                cmd.Parameters.AddWithValue("PayWay", instance.PayWay);
                cmd.Parameters.AddWithValue("ShipWho", instance.ShipWho);
                cmd.Parameters.AddWithValue("SendDate", instance.SendDate);
                cmd.Parameters.AddWithValue("SendComp", instance.SendComp);
                cmd.Parameters.AddWithValue("SendWho", instance.SendWho);
                cmd.Parameters.AddWithValue("SendAddr", instance.SendAddr);
                cmd.Parameters.AddWithValue("SendTel", instance.SendTel);
                cmd.Parameters.AddWithValue("ShipNo", instance.ShipNo);
                cmd.Parameters.AddWithValue("ShipPay", instance.ShipPay);
                cmd.Parameters.AddWithValue("Box", instance.Box.Equals(0) ? 1 : (object)instance.Box);
                cmd.Parameters.AddWithValue("BoxClass1", instance.BoxClass1.Equals(0) ? DBNull.Value : (object)instance.BoxClass1);
                cmd.Parameters.AddWithValue("BoxClass2", instance.BoxClass2.Equals(0) ? DBNull.Value : (object)instance.BoxClass2);
                cmd.Parameters.AddWithValue("TargetClass", instance.TargetClass.Equals(0) ? DBNull.Value : (object)instance.TargetClass);
                cmd.Parameters.AddWithValue("Remark1", instance.Remark1);
                cmd.Parameters.AddWithValue("Remark2", instance.Remark2);
                cmd.Parameters.AddWithValue("PurNo", instance.PurNo);
                cmd.Parameters.AddWithValue("SaleNo", instance.SaleNo);
                cmd.Parameters.AddWithValue("InvoiceNo", instance.InvoiceNo);
                cmd.Parameters.AddWithValue("Create_Who", fn_Param.CurrentUser);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }

        }

        #endregion


        #region -----// Update //-----

        /// <summary>
        /// 更新資料
        /// </summary>
        /// <param name="dataID"></param>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update(string dataID, Delivery_Base instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Delivery_Data SET ");
                sql.AppendLine("  ShipType = @ShipType, ShipWay = @ShipWay, PayWay = @PayWay");
                sql.AppendLine("  , ShipWho = @ShipWho, SendDate = @SendDate, SendComp = @SendComp");
                sql.AppendLine("  , SendWho = @SendWho, SendAddr = @SendAddr, SendTel = @SendTel");
                sql.AppendLine("  , ShipNo = @ShipNo, ShipPay = @ShipPay, Box = @Box");
                sql.AppendLine("  , BoxClass1 = @BoxClass1, BoxClass2 = @BoxClass2, TargetClass = @TargetClass");
                sql.AppendLine("  , Remark1 = @Remark1, Remark2 = @Remark2, PurNo = @PurNo, SaleNo = @SaleNo, InvoiceNo = @InvoiceNo");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);
                cmd.Parameters.AddWithValue("ShipType", instance.ShipType);
                cmd.Parameters.AddWithValue("ShipWay", instance.ShipWay);
                cmd.Parameters.AddWithValue("PayWay", instance.PayWay);
                cmd.Parameters.AddWithValue("ShipWho", instance.ShipWho);
                cmd.Parameters.AddWithValue("SendDate", instance.SendDate);
                cmd.Parameters.AddWithValue("SendComp", instance.SendComp);
                cmd.Parameters.AddWithValue("SendWho", instance.SendWho);
                cmd.Parameters.AddWithValue("SendAddr", instance.SendAddr);
                cmd.Parameters.AddWithValue("SendTel", instance.SendTel);
                cmd.Parameters.AddWithValue("ShipNo", instance.ShipNo);
                cmd.Parameters.AddWithValue("ShipPay", instance.ShipPay);
                cmd.Parameters.AddWithValue("Box", instance.Box.Equals(0) ? DBNull.Value : (object)instance.Box);
                cmd.Parameters.AddWithValue("BoxClass1", instance.BoxClass1.Equals(0) ? DBNull.Value : (object)instance.BoxClass1);
                cmd.Parameters.AddWithValue("BoxClass2", instance.BoxClass2.Equals(0) ? DBNull.Value : (object)instance.BoxClass2);
                cmd.Parameters.AddWithValue("TargetClass", instance.TargetClass.Equals(0) ? DBNull.Value : (object)instance.TargetClass);
                cmd.Parameters.AddWithValue("Remark1", instance.Remark1);
                cmd.Parameters.AddWithValue("Remark2", instance.Remark2);
                cmd.Parameters.AddWithValue("PurNo", instance.PurNo);
                cmd.Parameters.AddWithValue("SaleNo", instance.SaleNo);
                cmd.Parameters.AddWithValue("InvoiceNo", instance.InvoiceNo);
                cmd.Parameters.AddWithValue("Update_Who", fn_Param.CurrentUser);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }


        /// <summary>
        /// 更新狀態
        /// </summary>
        /// <param name="dataID"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_Status(string dataID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Delivery_Data SET ");
                sql.AppendLine("  IsClose = 'Y'");
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


        /// <summary>
        /// 回寫物流單號
        /// </summary>
        /// <param name="instance">excel來源資料</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public bool Update_ShipNo(IQueryable<Delivery_Import> instance, out string ErrMsg)
        {
            //回寫至ShipmentNo
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                foreach (var item in instance)
                {
                    sql.AppendLine(" UPDATE Delivery_Data");
                    sql.AppendLine(" SET ShipNo = '{1}', ShipPay = {2} WHERE (TraceID = '{0}');".FormatThis(
                         item.TraceID
                         , item.ShipNo
                         , item.Freight
                        ));
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                //Execute
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
                sql.AppendLine(" DELETE FROM Delivery_Data WHERE (Data_ID = @DataID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }

        #endregion

    }
}

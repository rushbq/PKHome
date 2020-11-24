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

        #region *** 出貨資料 S ***
        /// <summary>
        /// [出貨明細表](內銷) 指定資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="type">A=內銷工具 / B=內銷科學玩具</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipFreightItem> GetOneShipData(Dictionary<string, string> search, string type, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetShipData(search, type, 0, 1, out dataCnt, out ErrMsg);
        }

        /// <summary>
        /// [出貨明細表](內銷) 所有資料
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="type">A=內銷工具 / B=內銷科學玩具</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipFreightItem> GetAllShipData(Dictionary<string, string> search, string type, out string ErrMsg)
        {
            int dataCnt = 0;
            return GetShipData(search, type, 0, 9999999, out dataCnt, out ErrMsg);
        }


        /// <summary>
        /// [出貨明細表](內銷) 資料清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="type">A=內銷工具 / B=內銷科學玩具</param>
        /// <param name="startRow">StartRow</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ShipFreightItem> GetShipData(Dictionary<string, string> search, string type
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
                List<ShipFreightItem> dataList = new List<ShipFreightItem>(); //資料容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> subParamList = new List<SqlParameter>(); //SQL參數取得
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                StringBuilder subSql = new StringBuilder(); //條件SQL取得
                DataCnt = 0;    //資料總數

                //取得SQL語法
                string cteSql = GetSQL_CTE(type);
                subSql = GetSQL_ShipData(search);

                //取得SQL參數集合
                subParamList = GetParams_ShipData(search);


                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    sql.Clear();

                    //CTE SQL
                    sql.Append(cteSql);


                    sql.AppendLine(" SELECT COUNT(TblAll.SO_Date) AS TotalCnt FROM (");
                    //子查詢SQL
                    sql.Append(subSql);
                    sql.AppendLine(" ) AS TblAll");

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

                    //CTE SQL
                    sql.Append(cteSql);

                    sql.AppendLine(" SELECT TblAll.* FROM (");
                    //子查詢SQL
                    sql.Append(subSql);
                    sql.AppendLine(" ) AS TblAll");
                    sql.AppendLine(" WHERE (TblAll.RowIdx >= @startRow) AND (TblAll.RowIdx <= @endRow)");
                    //sql.AppendLine(" ORDER BY TblAll.RowIdx");

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
                            #region ** 欄位 **

                            //加入項目
                            var data = new ShipFreightItem
                            {
                                Data_ID = item.Field<Guid?>("Data_ID"),
                                Erp_SO_FID = item.Field<string>("SO_Fid"),
                                Erp_SO_SID = item.Field<string>("SO_Sid"),
                                Erp_SO_FullID = "{0}-{1}".FormatThis(item.Field<string>("SO_Fid"), item.Field<string>("SO_Sid")),
                                Erp_SO_Date = item.Field<string>("SO_Date"),
                                CustID = item.Field<string>("CustID"),
                                CustName = item.Field<string>("CustName"),
                                TotalPrice = item.Field<decimal>("TotalPrice"),
                                CfmCode = item.Field<string>("CfmCode"),
                                ShipNo = item.Field<string>("ShipNo"),
                                Freight = item.Field<double?>("Freight"),
                                BoxCnt = item.Field<int?>("BoxCnt"),
                                ShipWho = item.Field<string>("ShipWho"),
                                ShipDate = item.Field<DateTime?>("ShipDate").ToString().ToDateString("yyyy/MM/dd"),
                                ShipTel = item.Field<string>("ShipTel"),
                                ShipAddr1 = item.Field<string>("ShipAddr1"),
                                ShipAddr2 = item.Field<string>("ShipAddr2"),
                                CfmWhoName = item.Field<string>("CfmWhoName"),

                                ShipComp = item.Field<Int32?>("ShipComp") ?? 0,
                                ShipCompName = item.Field<string>("ShipCompName"),
                                ShipWay = item.Field<Int32?>("ShipWay") ?? 0,
                                ShipWayName = item.Field<string>("ShipWayName"),
                                SendType = item.Field<Int32?>("SendType") ?? 0,
                                SendTypeName = item.Field<string>("SendTypeName"),
                                UserCheck1 = item.Field<string>("UserCheck1"),
                                Check_Time1 = item.Field<DateTime?>("Check_Time1").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Remark = item.Field<string>("Remark"),
                                EmptyCol = item.Field<string>("EmptyCol"),
                                Create_Time = item.Field<DateTime?>("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Update_Time = item.Field<DateTime?>("Update_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                                Create_Name = item.Field<string>("Create_Name"),
                                Update_Name = item.Field<string>("Update_Name")
                            };


                            #endregion


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
        /// 取得SQL CTE區語法
        /// </summary>
        /// <param name="type">A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具</param>
        /// <returns></returns>
        /// <remarks>
        /// SQL上半段
        /// </remarks>
        private string GetSQL_CTE(string type)
        {
            string param1 = "";
            switch (type.ToUpper())
            {
                case "A":
                    /* 指定條件:電商工具(164電商部) */
                    param1 = " AND (Base.TG001 IN ('2341','2342','2343','2345','23B2','23B3','23B6'))";
                    param1 += @" AND (Base.TG005 IN ('164'))";
                    break;

                case "B":
                    /* 指定條件:電商科學玩具 */
                    param1 = " AND (Base.TG001 IN ('2380','2381','2382','2383'))";
                    param1 += @" AND (Base.TG005 IN ('164'))";
                    break;

                case "C":
                    /* 指定條件:經銷商工具(102業務部) */
                    param1 = " AND (Base.TG001 IN ('2341','2342','2343','2345','23B2','23B3','23B6'))";
                    param1 += @" AND (Base.TG005 IN ('102'))";
                    break;

                case "D":
                    /* 指定條件:經銷商科學玩具 */
                    param1 = " AND (Base.TG001 IN ('2380','2381','2382','2383'))";
                    param1 += @" AND (Base.TG005 IN ('102'))";
                    break;
            }

            string sql = @"
                /*
                銷貨單(730 Days)
                */
                 ;WITH TblERP AS(
                 SELECT
                    RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid, Base.TG003 AS SO_Date, Base.TG004 AS CustID
                  , CAST((Base.TG045 + Base.TG046) AS money) AS TotalPrice
                  , Base.TG066 AS ContactWho
                  , Base.TG078 AS ShipTel
                  , Base.TG008 AS ShipAddr1
                  , Base.TG009 AS ShipAddr2
                  , Base.TG023 AS CfmCode
                  , Base.TG043 AS CfmWho
                  , (CASE WHEN CHARINDEX('-', REVERSE(Base.TG020)) > 0 THEN
                     REVERSE(SUBSTRING(REVERSE(Base.TG020), 1, CHARINDEX('-', REVERSE(Base.TG020))-1 ))
                    ELSE '' END
                  ) AS ShipNo
                 FROM [SHPK2].dbo.COPTG Base WITH(NOLOCK)
                  INNER JOIN [SHPK2].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002
                 WHERE (DT.TH007 <> 'C01') AND (SUBSTRING(Base.TG003,1,4) BETWEEN YEAR(GETDATE()-730) AND YEAR(GETDATE()))

                 ##param1##

                 GROUP BY Base.TG001, Base.TG002, Base.TG003, Base.TG004, Base.TG066, Base.TG045, Base.TG046, Base.TG023, Base.TG078, Base.TG008, Base.TG009, Base.TG043, Base.TG020

                 UNION ALL
                /*
                無銷貨單的借出單
                單別TG001 = 1302
                結案碼TG024 <> Y
                */
                 SELECT
                  RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid, OrdBase.TF003 AS SO_Date, OrdBase.TF005 AS CustID
                  , CAST(SUM(Base.TG013) AS money) AS TotalPrice
                  , OrdBase.TF015 AS ContactWho
                  , '' AS Tel
                  , OrdBase.TF016 AS Addr1
                  , OrdBase.TF017 AS Addr2
                  , Base.TG022 AS CfmCode
                  , OrdBase.TF025
                  , (CASE WHEN CHARINDEX('-', REVERSE(OrdBase.TF014)) > 0 THEN
                      REVERSE(SUBSTRING(REVERSE(OrdBase.TF014), 1, CHARINDEX('-', REVERSE(OrdBase.TF014))-1 ))
                     ELSE '' END
                  ) AS ShipNo
                 FROM [SHPK2].dbo.INVTG Base WITH(NOLOCK)
                  INNER JOIN [SHPK2].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002
                 WHERE (Base.TG001 = '1302') AND (Base.TG024 <> 'Y')
                 GROUP BY Base.TG001, Base.TG002, Base.TG007, OrdBase.TF003, OrdBase.TF005, OrdBase.TF015, Base.TG022, OrdBase.TF016, OrdBase.TF017, OrdBase.TF025, OrdBase.TF014
                 )";

            //指定單別
            sql = sql.Replace("##param1##", param1);

            return sql;
        }


        /// <summary>
        /// [出貨明細表](內銷) 取得SQL查詢
        /// ** TSQL查詢條件寫在此 **
        /// </summary>
        /// <param name="search">search集合</param>
        /// <returns></returns>
        /// <see cref="GetShipData"/>
        /// <remarks>
        /// SQL下半段
        /// </remarks>
        private StringBuilder GetSQL_ShipData(Dictionary<string, string> search)
        {
            StringBuilder sql = new StringBuilder();

            //SQL查詢
            string strSql = @" SELECT
              TblERP.SO_Fid, TblERP.SO_Sid, TblERP.SO_Date
              , TblERP.CustID, RTRIM(Cust.MA002) AS CustName, TblERP.TotalPrice, TblERP.CfmCode
              , ShipBase.Data_ID
              , (
               CASE WHEN ISNULL(ShipBase.ShipWho, TblERP.ContactWho) = '' THEN
                CASE WHEN TblERP.ContactWho = Cust.MA002 THEN Cust.MA005 ELSE TblERP.ContactWho END
                ELSE ISNULL(ShipBase.ShipWho COLLATE Chinese_Taiwan_Stroke_BIN, TblERP.ContactWho) END
              ) AS ShipWho
              , ISNULL(ShipBase.ShipTel, TblERP.ShipTel) AS ShipTel
              , ISNULL(ShipBase.ShipAddr1, TblERP.ShipAddr1) AS ShipAddr1
              , ISNULL(ShipBase.ShipAddr2, TblERP.ShipAddr2) AS ShipAddr2
              , ISNULL(ShipBase.ShipNo, TblERP.ShipNo) AS ShipNo
              , ShipBase.ShipDate, ISNULL(ShipBase.BoxCnt, 0) BoxCnt, ISNULL(ShipBase.Freight, 0) Freight
              , ShipBase.ShipComp, ShipBase.ShipWay, ShipBase.SendType
              , ISNULL(RefShipComp.Class_Name_zh_CN, '') AS ShipCompName
              , ISNULL(RefShipWay.Class_Name_zh_CN, '') AS ShipWayName
              , ISNULL(RefSendType.Class_Name_zh_CN, '') AS SendTypeName
              , ShipBase.Remark
              , ShipBase.UserCheck1
              , RTRIM(Emp.MA002) AS CfmWhoName
              , '' AS EmptyCol
              , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = ShipBase.Create_Who)) AS Create_Name
              , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = ShipBase.Update_Who)) AS Update_Name
              , ShipBase.Create_Time, ShipBase.Update_Time, ShipBase.Check_Time1
              , RANK() OVER (ORDER BY TblERP.SO_Date DESC, TblERP.CustID, TblERP.SO_Fid, TblERP.SO_Sid) AS RowIdx
             FROM TblERP
              INNER JOIN [SHPK2].dbo.COPMA Cust WITH(NOLOCK) ON TblERP.CustID = Cust.MA001
              INNER JOIN [DSCSYS].dbo.DSCMA Emp WITH(NOLOCK) ON TblERP.CfmWho = Emp.MA001
              LEFT JOIN [PKExcel].dbo.Shipment_Data_CHN ShipBase ON TblERP.SO_Fid = ShipBase.SO_FID COLLATE Chinese_Taiwan_Stroke_BIN AND TblERP.SO_Sid = ShipBase.SO_SID COLLATE Chinese_Taiwan_Stroke_BIN
              LEFT JOIN [PKExcel].dbo.Shipment_RefClass_CHN RefShipComp ON ShipBase.ShipComp = RefShipComp.Class_ID
              LEFT JOIN [PKExcel].dbo.Shipment_RefClass_CHN RefShipWay ON ShipBase.ShipWay = RefShipWay.Class_ID
              LEFT JOIN [PKExcel].dbo.Shipment_RefClass_CHN RefSendType ON ShipBase.SendType = RefSendType.Class_ID
            WHERE (1=1)";

            //append SQL
            sql.Append(strSql);

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
                            //--單據日S
                            sql.Append(" AND (TblERP.SO_Date >= @sDate)");
                            break;

                        case "eDate":
                            //--單據日E
                            sql.Append(" AND (TblERP.SO_Date <= @eDate)");
                            break;

                        case "Cust":
                            //--客戶ID / Name
                            sql.Append(" AND (");
                            sql.Append("  (UPPER(TblERP.CustID) LIKE '%' + UPPER(@Cust) + '%')");
                            sql.Append("  OR (UPPER(RTRIM(Cust.MA002)) LIKE '%' + UPPER(@Cust) + '%')");
                            sql.Append(" )");

                            break;

                        //case "CustID":
                        //    //--客戶ID
                        //    sql.Append(" AND (");
                        //    sql.Append("  (UPPER(TblERP.CustID) = UPPER(@CustID))");
                        //    sql.Append(" )");

                        //    break;

                        case "Keyword":
                            //--單號keyword/物流單號/收件人
                            sql.Append(" AND (");
                            sql.Append("  (UPPER(TblERP.SO_Fid) + UPPER(TblERP.SO_Sid) LIKE '%' + UPPER(@keyword) + '%')");
                            sql.Append("  OR (UPPER(TblERP.SO_Fid) +'-'+ UPPER(TblERP.SO_Sid) LIKE '%' + UPPER(@keyword) + '%')");
                            sql.Append("  OR (ShipBase.ShipNo LIKE '%'+ @keyword +'%')");
                            sql.Append("  OR ((");
                            sql.Append("       CASE WHEN ISNULL(ShipBase.ShipWho, TblERP.ContactWho) = '' THEN");
                            sql.Append("       CASE WHEN TblERP.ContactWho = Cust.MA002 THEN Cust.MA005 ELSE TblERP.ContactWho END");
                            sql.Append("       ELSE ISNULL(ShipBase.ShipWho COLLATE Chinese_Taiwan_Stroke_BIN, TblERP.ContactWho) END");
                            sql.Append("   ) LIKE '%'+ @keyword +'%')");
                            sql.Append(" )");

                            break;

                        case "ShipsDate":
                            //--發貨日
                            sql.Append(" AND (ShipBase.ShipDate >= @ShipsDate)");
                            break;

                        case "ShipeDate":
                            //--發貨日
                            sql.Append(" AND (ShipBase.ShipDate <= @ShipeDate)");
                            break;

                        case "ShipComp":
                            //貨運公司
                            sql.Append(" AND (ShipBase.ShipComp = @ShipComp)");
                            break;

                        case "Way":
                            //--物流途徑
                            sql.Append(" AND (ShipBase.ShipWay = @ShipWay)");
                            break;

                        case "FreightWay":
                            //運費方式
                            sql.Append(" AND (ShipBase.SendType = @FreightWay)");
                            break;

                    }
                }
            }
            #endregion


            //return
            return sql;
        }


        /// <summary>
        /// [出貨明細表](內銷) 取得條件參數
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
                            //--單據日S
                            sqlParamList.Add(new SqlParameter("@sDate", item.Value));
                            break;

                        case "eDate":
                            //--單據日E
                            sqlParamList.Add(new SqlParameter("@eDate", item.Value));
                            break;

                        case "Cust":
                            //--客戶ID / Name
                            sqlParamList.Add(new SqlParameter("@Cust", item.Value));

                            break;

                        case "Keyword":
                            //--單號keyword/物流單號/收件人
                            sqlParamList.Add(new SqlParameter("@keyword", item.Value));

                            break;

                        case "ShipsDate":
                            //--發貨日
                            sqlParamList.Add(new SqlParameter("@ShipsDate", item.Value));
                            break;

                        case "ShipeDate":
                            //--發貨日
                            sqlParamList.Add(new SqlParameter("@ShipeDate", item.Value));
                            break;

                        case "ShipComp":
                            //貨運公司
                            sqlParamList.Add(new SqlParameter("@ShipComp", item.Value));
                            break;

                        case "Way":
                            //--物流途徑
                            sqlParamList.Add(new SqlParameter("@ShipWay", item.Value));
                            break;

                        case "FreightWay":
                            //運費方式
                            sqlParamList.Add(new SqlParameter("@FreightWay", item.Value));
                            break;

                    }
                }
            }


            return sqlParamList;
        }



        #endregion *** 出貨資料 E ***


        #region *** 匯入 S ***

        /// <summary>
        /// 取得Excel必要欄位,用來轉入單身資料
        /// </summary>
        /// <param name="filePath">完整磁碟路徑</param>
        /// <param name="sheetName">工作表名稱</param>
        /// <param name="traceID">trace id</param>
        /// <param name="_type">A=電商, B=經銷商</param>
        /// <returns></returns>
        public IQueryable<ShipImportDataDT> GetExcel_DT(string filePath, string sheetName, string traceID, string _type)
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


                //資料迴圈
                foreach (var val in queryVals)
                {
                    #region >> 欄位處理 <<

                    if (_type.Equals("A"))
                    {
                        //電商
                        myShipNo = val[0]; //物流單號
                        myShipDate = val[1].ToString().ToDateString("yyyy/MM/dd");
                        myQty = Convert.ToInt32(val[2]);
                        myPrice = Convert.ToDouble(val[3]);
                    }
                    else
                    {
                        //經銷商
                        myErpID = val[35]; //銷貨單別-單號
                        myShipNo = val[4]; //物流單號
                        myShipDate = val[6].ToString().ToDateString("yyyy/MM/dd");
                        //myQty = Convert.ToInt32(val[20]);
                        myPrice = Convert.ToDouble(val[40]);
                    }

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
        /// <param name="_type">A=電商, B=經銷商</param>
        /// <returns></returns>
        /// <example>
        /// <table id="listTable" class="stripe" cellspacing="0" width="100%" style="width:100%;">
        ///     <asp:Literal ID="lt_tbBody" runat="server"></asp:Literal>
        /// </table>
        /// </example>
        public StringBuilder GetExcel_Html(string filePath, string sheetName, string _type)
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
                if (_type.Equals("A"))
                {
                    //電商
                    html.Append("<th>运单号</th><th>出貨日期</th><th>件数</th><th>总运费</th>");
                }
                else
                {
                    //經銷商
                    html.Append("<th>运单号</th><th>开单时间</th><th>货物件数</th><th>订单号</th><th>总运费</th>");
                }
                html.Append("</tr>");
                html.Append("</thead>");

                //[HTML] - 取得欄位值, 輸出內容欄 (Worksheet)
                var queryVals = excelFile.Worksheet(sheetName);

                html.Append("<tbody>");
                if (_type.Equals("A"))
                {
                    //電商
                    foreach (var val in queryVals)
                    {
                        //內容迴圈
                        html.Append("<tr>");
                        html.Append("<td>{0}</td>".FormatThis(val[0]));
                        html.Append("<td>{0}</td>".FormatThis(val[1]));
                        html.Append("<td>{0}</td>".FormatThis(val[2]));
                        html.Append("<td>{0}</td>".FormatThis(val[3]));
                        html.Append("</tr>");
                    }
                }
                else
                {
                    //經銷商
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
                sql.AppendLine("   , Base.Upload_Type, Base.Status, Base.Upload_File, Base.Sheet_Name");
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
                            Status = item.Field<Int16>("Status"),
                            StatusName = GetShipStatusName(item.Field<Int16>("Status")),
                            Upload_File = item.Field<string>("Upload_File"),
                            Upload_Type = item.Field<string>("Upload_Type"),
                            Upload_TypeName = item.Field<string>("Upload_Type").Equals("A") ? "電商" : "經銷商",
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

        #endregion *** 匯入 E ***


        /// <summary>
        /// 取得參考類別
        /// </summary>
        /// <param name="lang">語系(zh-TW)</param>
        /// <param name="type">類型</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// 1:貨運公司, 2:物流途徑, 3:運費方式
        /// </remarks>
        public IQueryable<ClassItem> GetRefClass(string lang, string type, out string ErrMsg)
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
                sql.AppendLine(" FROM Shipment_RefClass_CHN WITH(NOLOCK)");
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

        #endregion



        #region -----// Create //-----

        #region *** 匯入 S ***

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
                sql.AppendLine("  Data_ID, TraceID, Status, Upload_File, Upload_Type");
                sql.AppendLine("  , Create_Who, Create_Time, CompID");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @TraceID, 10, @Upload_File, @Upload_Type");
                sql.AppendLine("  , @Create_Who, GETDATE(), @CompID");
                sql.AppendLine(" );");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("CompID", compID);
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("TraceID", instance.TraceID);
                cmd.Parameters.AddWithValue("Upload_File", instance.Upload_File);
                cmd.Parameters.AddWithValue("Upload_Type", instance.Upload_Type);
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
                cmd.CommandTimeout = 180;   //單位:秒

                cmd.Parameters.AddWithValue("DataID", baseData.Data_ID);
                cmd.Parameters.AddWithValue("Sheet_Name", baseData.Sheet_Name);
                cmd.Parameters.AddWithValue("Update_Who", baseData.Update_Who);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        #endregion *** 匯入 E ***

        #endregion



        #region -----// Update //-----

        #region *** 匯入 S ***

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
            string strSQL = "";

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                if (instance.Upload_Type.Equals("A"))
                {
                    //電商
                    #region -- 電商 --

                    strSQL += @"
;WITH TblData AS (
	SELECT Data_ID AS ImpID, ShipNo, ShipDate, Qty, Freight, ERP_ID
	FROM [PKEF].[dbo].[Ship_ImportData_DT]
	WHERE (Parent_ID = @RefID)
)
, TblBase AS (
SELECT
 RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid
 , Base.TG003 AS SO_Date, Base.TG004 AS CustID
 , Base.TG066 AS ContactWho
 , (CASE WHEN CHARINDEX('-', REVERSE(Base.TG020)) > 0 THEN 
    REVERSE(SUBSTRING(REVERSE(Base.TG020), 1, CHARINDEX('-', REVERSE(Base.TG020))-1 )) 
   ELSE '' END
 ) AS ShipNo
 , Base.TG078 AS ShipTel
 , Base.TG008 AS ShipAddr1
 , Base.TG009 AS ShipAddr2
FROM [SHPK2].dbo.COPTG Base WITH(NOLOCK)
 INNER JOIN [SHPK2].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002
WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')

AND (SUBSTRING(Base.TG003,1,4) BETWEEN YEAR(GETDATE()-730) AND YEAR(GETDATE()))
GROUP BY Base.TG001, Base.TG002, Base.TG003, Base.TG004, Base.TG066, Base.TG020, Base.TG078, Base.TG008, Base.TG009

UNION ALL

SELECT
 RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid
 , OrdBase.TF003 AS SO_Date, OrdBase.TF005 AS CustID
 , OrdBase.TF015 AS ContactWho
 , (CASE WHEN CHARINDEX('-', REVERSE(OrdBase.TF014)) > 0 THEN 
    REVERSE(SUBSTRING(REVERSE(OrdBase.TF014), 1, CHARINDEX('-', REVERSE(OrdBase.TF014))-1 )) 
   ELSE '' END
 ) AS ShipNo
  , '' AS Tel
  , OrdBase.TF016 AS Addr1
  , OrdBase.TF017 AS Addr2
FROM [SHPK2].dbo.INVTG Base WITH(NOLOCK)
 INNER JOIN [SHPK2].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002
WHERE (Base.TG001 = '1302') AND (Base.TG022 = 'Y') AND (Base.TG024 <> 'Y')

AND (SUBSTRING(Base.TG003,1,4) BETWEEN YEAR(GETDATE()-365) AND YEAR(GETDATE()))
GROUP BY Base.TG001, Base.TG002, Base.TG007, OrdBase.TF003, OrdBase.TF005, OrdBase.TF015, OrdBase.TF014, OrdBase.TF016, OrdBase.TF017
)
/* 新增至暫存檔 (確認Insert及Select欄位是否相符) */
INSERT INTO [PKExcel].dbo.Shipment_Data_CHN_Temp(
 Data_ID
 , SO_FID, SO_SID, ShipDate
 , ShipComp, ShipWay, SendType
 , ShipWho, BoxCnt, ShipNo, Freight, UserCheck1
 , ShipTel, ShipAddr1, ShipAddr2
 , Create_Who, Create_Time
 , Ref_ID
)
SELECT
 NEWID()
 , TblBase.SO_Fid, TblBase.SO_Sid, TblData.ShipDate
 , 1 AS ShipComp, 8 AS ShipWay, 10 AS SendType /*預設值(貨運=1德邦;8=自發;10=自付)*/
 , LEFT(TblBase.ContactWho, 50) AS ShipWho
 , TblData.Qty AS BoxCnt
 , LEFT(TblData.ShipNo, 50) AS ShipNo
 , TblData.Freight, 'Y' /*預設值UserCheck1=Y*/
 , TblBase.ShipTel, TblBase.ShipAddr1, TblBase.ShipAddr2
 , @Who AS CreateWho, GETDATE() AS CreateTime
 , @RefID
FROM TblBase
 INNER JOIN TblData ON TblBase.ShipNo = TblData.ShipNo COLLATE Chinese_Taiwan_Stroke_BIN
WHERE (TblData.ShipNo <> '')
";


                    #endregion
                }
                else
                {
                    //經銷商
                    #region -- 經銷商 --

                    strSQL += @"
;WITH TblData AS (
	SELECT Data_ID AS ImpID, ShipNo, ShipDate, Qty, Freight, ERP_ID
	FROM [PKEF].[dbo].[Ship_ImportData_DT]
	WHERE (Parent_ID = @RefID)
)
, TblBase AS (
SELECT
 RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid
 , Base.TG003 AS SO_Date, Base.TG004 AS CustID
 , Base.TG066 AS ContactWho
 , Base.TG078 AS ShipTel
 , Base.TG008 AS ShipAddr1
 , Base.TG009 AS ShipAddr2
FROM [SHPK2].dbo.COPTG Base WITH(NOLOCK)
 INNER JOIN [SHPK2].dbo.COPTH DT WITH(NOLOCK) ON DT.TH001 = Base.TG001 AND DT.TH002 = Base.TG002
 INNER JOIN TblData ON (RTRIM(Base.TG001) + '-' + RTRIM(Base.TG002)) = TblData.ERP_ID COLLATE Chinese_Taiwan_Stroke_BIN
WHERE (Base.TG023 = 'Y') AND (DT.TH007 <> 'C01')
GROUP BY Base.TG001, Base.TG002, Base.TG003, Base.TG004, Base.TG066, Base.TG078, Base.TG008, Base.TG009

UNION ALL

SELECT
 RTRIM(Base.TG001) AS SO_Fid, RTRIM(Base.TG002) AS SO_Sid
 , OrdBase.TF003 AS SO_Date, OrdBase.TF005 AS CustID
 , OrdBase.TF015 AS ContactWho
 , '' AS Tel
 , OrdBase.TF016 AS Addr1
 , OrdBase.TF017 AS Addr2
FROM [SHPK2].dbo.INVTG Base WITH(NOLOCK)
 INNER JOIN [SHPK2].dbo.INVTF OrdBase WITH(NOLOCK) ON Base.TG001 = OrdBase.TF001 AND Base.TG002 = OrdBase.TF002
 INNER JOIN TblData ON (RTRIM(Base.TG001) + '-' + RTRIM(Base.TG002)) = TblData.ERP_ID COLLATE Chinese_Taiwan_Stroke_BIN
WHERE (Base.TG001 = '1302') AND (Base.TG022 = 'Y') AND (Base.TG024 <> 'Y')
GROUP BY Base.TG001, Base.TG002, Base.TG007, OrdBase.TF003, OrdBase.TF005, OrdBase.TF015, OrdBase.TF016, OrdBase.TF017
)

/* 新增至暫存檔 (確認Insert及Select欄位是否相符) */
INSERT INTO [PKExcel].dbo.Shipment_Data_CHN_Temp(
 Data_ID
 , SO_FID, SO_SID, ShipDate
 , ShipComp, ShipWay, SendType
 , ShipWho, BoxCnt, ShipNo, Freight, UserCheck1
 , ShipTel, ShipAddr1, ShipAddr2
 , Create_Who, Create_Time
 , Ref_ID
)
SELECT
 NEWID()
 , TblBase.SO_Fid, TblBase.SO_Sid, TblData.ShipDate
 , 1 AS ShipComp, 8 AS ShipWay, 10 AS SendType /*預設值(貨運=1德邦;8=自發;10=自付)*/
 , LEFT(TblBase.ContactWho, 50) AS ShipWho
 , TblData.Qty AS BoxCnt
 , LEFT(TblData.ShipNo, 50) AS ShipNo
 , TblData.Freight, 'N' /*預設值UserCheck1*/
 , TblBase.ShipTel, TblBase.ShipAddr1, TblBase.ShipAddr2
 , @Who AS CreateWho, GETDATE() AS CreateTime
 , @RefID
FROM TblBase
 INNER JOIN TblData ON (RTRIM(TblBase.SO_Fid) + '-' + RTRIM(TblBase.SO_Sid)) = TblData.ERP_ID COLLATE Chinese_Taiwan_Stroke_BIN
WHERE (TblData.ShipNo <> '')
";


                    #endregion
                }

                strSQL += @"
/* 新增資料至主檔(重複的銷貨單號不寫入) */
INSERT INTO [PKExcel].dbo.Shipment_Data_CHN(
 Data_ID
 , SO_FID, SO_SID, ShipDate
 , ShipComp, ShipWay, SendType
 , ShipWho, BoxCnt, ShipNo, Freight, UserCheck1
 , ShipTel, ShipAddr1, ShipAddr2
 , Create_Who, Create_Time
)
SELECT Data_ID
 , SO_FID, SO_SID, ShipDate
 , ShipComp, ShipWay, SendType
 , ShipWho, BoxCnt, ShipNo, Freight, UserCheck1
 , ShipTel, ShipAddr1, ShipAddr2
 , Create_Who, Create_Time
FROM [PKExcel].dbo.Shipment_Data_CHN_Temp
WHERE (Ref_ID = @RefID)
 AND ((SO_FID + SO_SID) COLLATE Chinese_Taiwan_Stroke_BIN NOT IN (
  SELECT SO_FID + SO_SID
  FROM [PKExcel].dbo.Shipment_Data_CHN
 ))

/* Update其他欄位至主檔(重複的銷貨單號會更新) */
UPDATE [PKExcel].dbo.Shipment_Data_CHN
SET ShipDate = Ref.ShipDate
 , BoxCnt = Ref.BoxCnt, ShipNo = Ref.ShipNo, Freight = Ref.Freight
 , Update_Who = @Who, Update_Time = GETDATE()
FROM [PKExcel].dbo.Shipment_Data_CHN_Temp Ref
WHERE (Shipment_Data_CHN.SO_FID = Ref.SO_FID) AND (Shipment_Data_CHN.SO_SID = Ref.SO_SID)
 AND (Ref.Ref_ID = @RefID)
 AND ((Ref.SO_FID + Ref.SO_SID) COLLATE Chinese_Taiwan_Stroke_BIN IN (
  SELECT SO_FID + SO_SID
  FROM [PKExcel].dbo.Shipment_Data_CHN
 ))

 
/* 清空暫存檔 */
DELETE FROM [PKExcel].dbo.Shipment_Data_CHN_Temp WHERE (Ref_ID = @RefID)
";


                /* Update 匯入檔狀態 */
                strSQL += @"
UPDATE Ship_ImportData_DT
SET IsPass = 'Y'
WHERE (Parent_ID = @RefID)
AND (ShipNo IN
 (SELECT ShipNo FROM [PKExcel].dbo.Shipment_Data_CHN)
);

UPDATE Ship_ImportData
SET Status = 30, Update_Who = @Who, Update_Time = GETDATE()
WHERE (Data_ID = @RefID)
";


                //----- SQL 執行 -----
                cmd.CommandText = strSQL;
                cmd.CommandTimeout = 180;   //單位:秒
                cmd.Parameters.AddWithValue("RefID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        #endregion *** 匯入 E ***

        public bool Update_ShipData(List<ShipFreightItem> instance, out string ErrMsg)
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


                    sql.AppendLine("IF (SELECT COUNT(*) FROM Shipment_Data_CHN WHERE (Data_ID = @Data_ID_#idx#)) > 0");
                    sql.AppendLine("  BEGIN");
                    sql.AppendLine("   UPDATE Shipment_Data_CHN");
                    sql.AppendLine("   SET ShipComp = @ShipComp_#idx#, ShipWay = @ShipWay_#idx#, SendType = @SendType_#idx#");
                    sql.AppendLine("    , ShipDate = @ShipDate_#idx#, ShipNo = @ShipNo_#idx#, ShipWho = @ShipWho_#idx#");
                    sql.AppendLine("    , Remark = @Remark_#idx#, UserCheck1 = @UserCheck1_#idx#");
                    sql.AppendLine("    , ShipTel = @ShipTel_#idx#, ShipAddr1 = @ShipAddr1_#idx#, ShipAddr2 = @ShipAddr2_#idx#");
                    sql.AppendLine("    , BoxCnt = @BoxCnt_#idx#, Freight = @Freight_#idx#");
                    sql.AppendLine("    , Update_Who = @Who, Update_Time = GETDATE(), Check_Time1 = @Check_Time1");
                    sql.AppendLine("   WHERE (Data_ID = @Data_ID_#idx#)");
                    sql.AppendLine("  END");
                    sql.AppendLine(" ELSE");
                    sql.AppendLine("  BEGIN");

                    sql.AppendLine("   IF (SELECT COUNT(*) FROM Shipment_Data_CHN WHERE (SO_FID = @SO_FID_#idx#) AND (SO_SID = @SO_SID_#idx#)) = 0");
                    sql.AppendLine("   BEGIN");
                    sql.AppendLine("    INSERT INTO Shipment_Data_CHN (");
                    sql.AppendLine("        Data_ID, SO_FID, SO_SID, ShipDate");
                    sql.AppendLine("        , ShipComp, ShipWay, SendType");
                    sql.AppendLine("        , ShipWho, ShipNo, Remark, UserCheck1");
                    sql.AppendLine("        , ShipTel, ShipAddr1, ShipAddr2");
                    sql.AppendLine("        , BoxCnt, Freight");
                    sql.AppendLine("        , Create_Who, Check_Time1");
                    sql.AppendLine("    ) VALUES (");
                    sql.AppendLine("        @Data_ID_#idx#, @SO_FID_#idx#, @SO_SID_#idx#, @ShipDate_#idx#");
                    sql.AppendLine("        , @ShipComp_#idx#, @ShipWay_#idx#, @SendType_#idx#");
                    sql.AppendLine("        , @ShipWho_#idx#, @ShipNo_#idx#, @Remark_#idx#, @UserCheck1_#idx#");
                    sql.AppendLine("        , @ShipTel_#idx#, @ShipAddr1_#idx#, @ShipAddr2_#idx#");
                    sql.AppendLine("        , @BoxCnt_#idx#, @Freight_#idx#");
                    sql.AppendLine("        , @Who, @Check_Time1");
                    sql.AppendLine("    )");
                    sql.AppendLine("   END");
                    sql.AppendLine("  END");


                    //replace idx number
                    sql.Replace("#idx#", row.ToString());

                    //add params
                    cmd.Parameters.AddWithValue("Data_ID_" + row, item.Data_ID);
                    cmd.Parameters.AddWithValue("SO_FID_" + row, item.Erp_SO_FID);
                    cmd.Parameters.AddWithValue("SO_SID_" + row, item.Erp_SO_SID);
                    cmd.Parameters.AddWithValue("ShipComp_" + row, item.ShipComp.Equals(0) ? DBNull.Value : (object)item.ShipComp);
                    cmd.Parameters.AddWithValue("ShipWay_" + row, item.ShipWay.Equals(0) ? DBNull.Value : (object)item.ShipWay);
                    cmd.Parameters.AddWithValue("SendType_" + row, item.SendType.Equals(0) ? DBNull.Value : (object)item.SendType);
                    cmd.Parameters.AddWithValue("ShipWho_" + row, item.ShipWho);
                    cmd.Parameters.AddWithValue("ShipNo_" + row, item.ShipNo);
                    cmd.Parameters.AddWithValue("ShipDate_" + row, string.IsNullOrWhiteSpace(item.ShipDate) ? DBNull.Value : (object)item.ShipDate);

                    cmd.Parameters.AddWithValue("ShipTel_" + row, item.ShipTel);
                    cmd.Parameters.AddWithValue("ShipAddr1_" + row, item.ShipAddr1);
                    cmd.Parameters.AddWithValue("ShipAddr2_" + row, item.ShipAddr2);
                    cmd.Parameters.AddWithValue("BoxCnt_" + row, item.BoxCnt);
                    cmd.Parameters.AddWithValue("Freight_" + row, item.Freight);

                    cmd.Parameters.AddWithValue("UserCheck1_" + row, item.UserCheck1);
                    cmd.Parameters.AddWithValue("Remark_" + row, item.Remark);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Who", instance[0].Create_Who);
                cmd.Parameters.AddWithValue("Check_Time1", string.IsNullOrWhiteSpace(instance[0].Check_Time1) ? DBNull.Value : (object)instance[0].Check_Time1.ToDateString("yyyy/MM/dd HH:mm:ss"));

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }


        }


        #endregion



        #region -----// Delete //-----
        /// <summary>
        /// 刪除
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Shipment_Data_CHN WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }

        /// <summary>
        /// 刪除匯入
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

        #endregion


    }
}

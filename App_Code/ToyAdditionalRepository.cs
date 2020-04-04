using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using PKLib_Method.Methods;
using ToyAdditionalData.Models;

namespace ToyAdditionalData.Controllers
{
    /// <summary>
    /// 查詢參數
    /// </summary>
    public enum mySearch : int
    {
        DataID = 1,
        Keyword = 2
    }

    public class ToyAdditionalRepository
    {
        public string ErrMsg;

        #region -----// Read //-----

        /// <summary>
        /// 取得所有資料(傳入預設參數)
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 預設值為(null)
        /// </remarks>
        public IQueryable<Items> GetDataList()
        {
            return GetDataList(null);
        }


        /// <summary>
        /// 取得所有資料
        /// </summary>
        /// <param name="search">查詢參數</param>
        /// <returns></returns>
        public IQueryable<Items> GetDataList(Dictionary<int, string> search)
        {
            //----- 宣告 -----
            List<Items> dataList = new List<Items>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("SELECT Base.*");
                sql.AppendLine(" , Prod.Model_Name_zh_TW AS ModelName, Cls.Class_Name AS CustTypeName");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who)) AS Update_Name");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Ship_Who)) AS Ship_Name");             
                sql.AppendLine(" FROM Toy_Additional Base");
                sql.AppendLine("  INNER JOIN [ProductCenter].dbo.Prod_Item Prod ON Base.ModelNo = Prod.Model_No");
                sql.AppendLine("  LEFT JOIN SZBBC_Toy_RefClass Cls ON Base.CustType = Cls.Class_ID");
                sql.AppendLine(" WHERE (1=1) ");

                /* Search */
                #region >> filter <<

                if (search != null)
                {
                    foreach (var item in search)
                    {
                        switch (item.Key)
                        {
                            case (int)mySearch.DataID:
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                    sql.Append(" AND (Base.Data_ID = @DataID)");

                                    cmd.Parameters.AddWithValue("DataID", item.Value);
                                }

                                break;

                            case (int)mySearch.Keyword:
                                if (!string.IsNullOrEmpty(item.Value))
                                {
                                    sql.Append(" AND (");
                                    sql.Append("  (UPPER(Base.ModelNo) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("  OR (UPPER(Base.CustName) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("  OR (UPPER(Base.CustTel) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("  OR (UPPER(Base.CustAddr) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("  OR (UPPER(Base.CustName) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("  OR (UPPER(Base.ShipNo) LIKE '%' + UPPER(@Keyword) + '%')");                                    
                                    sql.Append(" )");
                                    
                                    cmd.Parameters.AddWithValue("Keyword", item.Value);
                                }

                                break;

                        }
                    }
                }
                #endregion

                sql.AppendLine(" ORDER BY Base.SeqNo DESC");

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
                        var data = new Items
                        {
                            SeqNo = item.Field<int>("SeqNo"),
                            Data_ID = item.Field<Guid>("Data_ID"),
                            CompID = item.Field<string>("CompID"),
                            CustType = item.Field<short>("CustType"),
                            CustTypeName = item.Field<string>("CustTypeName"),
                            CustName = item.Field<string>("CustName"),
                            CustTel = item.Field<string>("CustTel"),
                            CustAddr = item.Field<string>("CustAddr"),
                            ModelNo = item.Field<string>("ModelNo"),
                            ModelName = item.Field<string>("ModelName"),
                            Qty = item.Field<int>("Qty"),
                            Remark1 = item.Field<string>("Remark1"),
                            Remark2 = item.Field<string>("Remark2"),
                            Remark3 = item.Field<string>("Remark3"),
                            ShipDate = item.Field<DateTime?>("ShipDate").ToString().ToDateString("yyyy/MM/dd"),
                            ShipNo = item.Field<string>("ShipNo"),
                            Freight = item.Field<double>("Freight"),

                            Create_Time = item.Field<DateTime?>("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                            Update_Time = item.Field<DateTime?>("Update_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                            Ship_Time = item.Field<DateTime?>("Ship_Time").ToString().ToDateString("yyyy/MM/dd HH:mm"),
                            Create_Name = item.Field<string>("Create_Name"),
                            Update_Name = item.Field<string>("Update_Name"),
                            Ship_Name = item.Field<string>("Ship_Name")

                        };

                        //將項目加入至集合
                        dataList.Add(data);

                    }
                }
            }

            //回傳集合
            return dataList.AsQueryable();
        }


        #endregion


        #region -----// Create //-----

        /// <summary>
        /// 建立基本資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Create(Items instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Toy_Additional( ");
                sql.AppendLine("  Data_ID, CompID, CustType");
                sql.AppendLine("  , CustName, CustTel, CustAddr");
                sql.AppendLine("  , ModelNo, Qty");
                sql.AppendLine("  , Remark1, Remark2, Remark3");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @CompID, @CustType");
                sql.AppendLine("  , @CustName, @CustTel, @CustAddr");
                sql.AppendLine("  , @ModelNo, @Qty");
                sql.AppendLine("  , @Remark1, @Remark2, @Remark3");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("CompID", instance.CompID);
                cmd.Parameters.AddWithValue("CustType", instance.CustType);
                cmd.Parameters.AddWithValue("CustName", instance.CustName);
                cmd.Parameters.AddWithValue("CustTel", instance.CustTel);
                cmd.Parameters.AddWithValue("CustAddr", instance.CustAddr);
                cmd.Parameters.AddWithValue("ModelNo", instance.ModelNo);
                cmd.Parameters.AddWithValue("Qty", instance.Qty);
                cmd.Parameters.AddWithValue("Remark1", instance.Remark1);
                cmd.Parameters.AddWithValue("Remark2", instance.Remark2);
                cmd.Parameters.AddWithValue("Remark3", instance.Remark3);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd,dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        #endregion


        #region -----// Update //-----

        /// <summary>
        /// 更新基本資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Update(Items instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Toy_Additional SET ");
                sql.AppendLine("  CustType = @CustType, CustName = @CustName, CustTel = @CustTel, CustAddr = @CustAddr");
                sql.AppendLine("  , ModelNo = @ModelNo, Qty = @Qty, Remark1 = @Remark1, Remark2 = @Remark2, Remark3 = @Remark3");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");
                
                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("CustType", instance.CustType);
                cmd.Parameters.AddWithValue("CustName", instance.CustName);
                cmd.Parameters.AddWithValue("CustTel", instance.CustTel);
                cmd.Parameters.AddWithValue("CustAddr", instance.CustAddr);
                cmd.Parameters.AddWithValue("ModelNo", instance.ModelNo);
                cmd.Parameters.AddWithValue("Qty", instance.Qty);
                cmd.Parameters.AddWithValue("Remark1", instance.Remark1);
                cmd.Parameters.AddWithValue("Remark2", instance.Remark2);
                cmd.Parameters.AddWithValue("Remark3", instance.Remark3);              
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        public bool Update_Ship(Items instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Toy_Additional SET ");
                sql.AppendLine("  ShipDate = @ShipDate, ShipNo = @ShipNo, Freight = @Freight");
                sql.AppendLine("  , Ship_Who = @Ship_Who, Ship_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("ShipDate", string.IsNullOrEmpty(instance.ShipDate) ? DBNull.Value : (object)instance.ShipDate.ToDateString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("ShipNo", instance.ShipNo);
                cmd.Parameters.AddWithValue("Freight", instance.Freight);
                cmd.Parameters.AddWithValue("Ship_Who", instance.Ship_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }

        #endregion


        #region -----// Delete //-----

        /// <summary>
        /// 刪除所有資料
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete(string dataID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Toy_Additional WHERE (Data_ID = @DataID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        #endregion
    }
}

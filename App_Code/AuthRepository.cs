using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AuthData.Models;
using PKLib_Method.Methods;

namespace AuthData.Controllers
{
    public class AuthRepository
    {
        public string ErrMsg;


        #region -----// Read //-----

        /// <summary>
        /// 取得權限表(傳入預設參數)
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// 預設值為(null)
        /// </remarks>
        public IQueryable<Auth> GetDataList(string userGuid)
        {
            return GetDataList("1", userGuid);
        }

        /// <summary>
        /// 取得權限表
        /// </summary>
        /// <param name="dbs">資料庫編號</param>
        /// <param name="userGuid">AD Guid</param>
        /// <returns></returns>
        public IQueryable<Auth> GetDataList(string dbs, string userGuid)
        {
            //----- 宣告 -----
            List<Auth> dataList = new List<Auth>();

            //----- 資料取得 -----
            string dbName = Get_DBName(dbs);
            using (DataTable DT = LookupRawData(dbs, dbName, userGuid))
            {
                //LinQ 查詢
                var query = DT.AsEnumerable();

                //資料迴圈
                foreach (var item in query)
                {
                    //加入項目
                    var data = new Auth
                    {
                        MenuID = item.Field<int>("ID").ToString(),
                        ParentID = item.Field<int>("ParentID").ToString(),
                        MenuName = item.Field<int>("Level").Equals(1)
                            ? "【{0}】".FormatThis(item.Field<string>("MenuName"))
                            : item.Field<string>("MenuName"),
                        ItemChecked = item.Field<string>("IsChecked").Equals("Y")
                    };

                    //將項目加入至集合
                    dataList.Add(data);

                }
            }

            //回傳集合
            return dataList.AsQueryable();
        }


        /// <summary>
        /// 取得使用者有權限的選單
        /// </summary>
        /// <param name="userGuid"></param>
        /// <returns></returns>
        public IQueryable<Auth> GetUserMenu(string userGuid)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            List<Auth> dataList = new List<Auth>();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("SELECT Base.Menu_ID AS MenuID, Base.Parent_ID AS ParentID, Base.MenuName_{0} AS MenuName".FormatThis(fn_Language.Param_Lang));
                sql.AppendLine("  , Base.Menu_Level AS Lv, Base.Url AS Url, Base.OpenTarget");
                sql.AppendLine(" FROM Home_Menu Base WITH (NOLOCK)");
                sql.AppendLine(" INNER JOIN Home_Menu_UserAuth Auth WITH (NOLOCK) ON Base.Menu_ID = Auth.Menu_ID");
                sql.AppendLine(" WHERE (Auth.User_Guid = @userID) AND (Base.Display = 'Y') AND (Base.DisplayInMenu = 'Y')");
                sql.AppendLine(" ORDER BY Base.Sort, Base.Menu_ID");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("userID", userGuid);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new Auth
                        {
                            MenuID = item.Field<int>("MenuID").ToString(),
                            ParentID = item.Field<int>("ParentID").ToString(),
                            MenuName = item.Field<string>("MenuName"),
                            Lv = item.Field<int>("Lv"),
                            Url = item.Field<string>("Url"),
                            Target = item.Field<string>("OpenTarget")
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
        /// 取得所有使用者 - 樹狀選單格式
        /// </summary>
        /// <returns></returns>
        public IQueryable<Auth> GetUserList()
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            List<Auth> dataList = new List<Auth>();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.Append("SELECT Tbl.* FROM ");
                sql.Append("( ");
                sql.Append("    SELECT CAST(SID AS NVARCHAR) AS MenuID, '0' AS ParentID, '【' + SName + '】' AS MenuName, CAST(Sort AS NVARCHAR) AS Sort, 'Y' AS IsOpen");
                sql.Append("    FROM Shipping WITH (NOLOCK) ");
                sql.Append("    WHERE Display = 'Y' ");
                sql.Append("    UNION ALL ");
                sql.Append("    SELECT CAST(Dept.DeptID AS NVARCHAR) AS MenuID, Dept.Area AS ParentID, Dept.DeptName AS MenuName, CAST(100 + Dept.Sort AS NVARCHAR) AS Sort, 'N' AS IsOpen");
                sql.Append("    FROM User_Dept Dept WITH (NOLOCK) ");
                sql.Append("    WHERE (Dept.Display = 'Y') ");
                sql.Append("    UNION ALL ");

                //使用v_+ 工號, 用來判斷此為要取用的值, 並在寫入時replace 'v_'為空白
                sql.Append("    SELECT 'v_' + Prof.Guid AS MenuID, Prof.DeptID AS ParentID, Prof.Display_Name AS MenuName, Prof.Account_Name AS Sort, 'N' AS IsOpen");
                sql.Append("    FROM User_Profile Prof WITH (NOLOCK) ");
                sql.Append("        INNER JOIN User_Dept Dept WITH (NOLOCK) ON Prof.DeptID = Dept.DeptID ");
                sql.Append("    WHERE (Dept.Display = 'Y') AND (Prof.Display = 'Y')");
                sql.Append(") AS Tbl ");
                sql.Append("ORDER BY Tbl.Sort ");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new Auth
                        {
                            MenuID = item.Field<string>("MenuID"),
                            ParentID = item.Field<string>("ParentID").ToString(),
                            MenuName = item.Field<string>("MenuName"),
                            ItemChecked = item.Field<string>("IsOpen").Equals("Y")
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
        /// 取得有權限的使用者(依功能查詢) - 樹狀選單格式
        /// </summary>
        /// <param name="dbs">資料庫別</param>
        /// <param name="menuID">功能編號</param>
        /// <returns></returns>
        public IQueryable<Auth> GetUserList(string dbs, string menuID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            List<Auth> dataList = new List<Auth>();
            string dbName = Get_DBName(dbs);

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("SELECT Tbl.* FROM ");
                sql.AppendLine("( ");
                sql.AppendLine("    SELECT CAST(SID AS NVARCHAR) AS MenuID, '0' AS ParentID, '【' + SName + '】' AS MenuName, CAST(Sort AS NVARCHAR) AS Sort, 'Y' AS IsOpen ");
                sql.AppendLine("    FROM Shipping WITH (NOLOCK) ");
                sql.AppendLine("    WHERE Display = 'Y' ");
                sql.AppendLine("    UNION ALL ");
                sql.AppendLine("    SELECT CAST(Dept.DeptID AS NVARCHAR) AS MenuID, Dept.Area AS ParentID, Dept.DeptName AS MenuName, CAST(Dept.Sort AS NVARCHAR) AS Sort, 'Y' AS IsOpen ");
                sql.AppendLine("    FROM User_Dept Dept WITH (NOLOCK) ");
                sql.AppendLine("    WHERE (Dept.Display = 'Y') AND (Dept.DeptID IN ( ");
                sql.AppendLine("        SELECT DeptID FROM User_Profile inProf WITH(NOLOCK) ");

                //判斷資料庫別
                switch (dbs)
                {
                    case "1":
                        sql.Append(" INNER JOIN Home_Menu_UserAuth inAuth WITH(NOLOCK) ON inAuth.User_Guid = inProf.Guid AND inAuth.Menu_ID = @menuID ");
                        break;

                    default:
                        sql.Append(" INNER JOIN [{0}].dbo.User_Profile_Rel_Program inAuth WITH(NOLOCK) ON inAuth.Guid = inProf.Guid AND inAuth.Prog_ID = @menuID ".FormatThis(dbName));
                        break;
                }
                sql.AppendLine("    )) ");

                sql.AppendLine("    UNION ALL ");

                sql.AppendLine("    SELECT Prof.Guid AS MenuID, Prof.DeptID AS ParentID, Prof.Display_Name AS MenuName, Prof.Account_Name AS Sort, 'Y' AS IsOpen ");
                sql.AppendLine("    FROM User_Profile Prof WITH (NOLOCK) ");
                sql.AppendLine("        INNER JOIN User_Dept Dept WITH (NOLOCK) ON Prof.DeptID = Dept.DeptID ");

                //判斷資料庫別
                switch (dbs)
                {
                    case "1":
                        sql.Append(" INNER JOIN Home_Menu_UserAuth Auth WITH(NOLOCK) ON Auth.User_Guid = Prof.Guid AND Auth.Menu_ID = @menuID ");
                        break;

                    default:
                        sql.Append(" INNER JOIN [{0}].dbo.User_Profile_Rel_Program Auth WITH(NOLOCK) ON Auth.Guid = Prof.Guid AND Auth.Prog_ID = @menuID ".FormatThis(dbName));
                        break;
                }

                sql.AppendLine("    WHERE (Dept.Display = 'Y') AND (Prof.Display = 'Y') ");
                sql.AppendLine(") AS Tbl ");
                sql.AppendLine("ORDER BY Tbl.Sort ");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("menuID", menuID);


                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new Auth
                        {
                            MenuID = item.Field<string>("MenuID"),
                            ParentID = item.Field<string>("ParentID").ToString(),
                            MenuName = item.Field<string>("MenuName"),
                            ItemChecked = item.Field<string>("IsOpen").Equals("Y")
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


        #region -----// Check //-----

        /// <summary>
        /// 判斷是否有使用權限(Local)
        /// </summary>
        /// <param name="userID">使用者guid</param>
        /// <param name="menuID">選單編號</param>
        /// <returns></returns>
        public bool Check_Auth(string userID, string menuID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT COUNT(*) AS AuthCnt");
                sql.AppendLine(" FROM Home_Menu_UserAuth Auth WITH (NOLOCK)");
                sql.AppendLine(" WHERE (Auth.User_Guid = @userID) AND (Auth.Menu_ID = @menuID)");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("userID", userID);
                cmd.Parameters.AddWithValue("menuID", menuID);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                {
                    int myCnt = Convert.ToInt32(DT.Rows[0]["AuthCnt"]);

                    return myCnt.Equals(0) ? false : true;
                }

            }
        }

        #endregion


        #region -----// Update - 依使用者 //-----

        /// <summary>
        /// 更新權限 - byUser
        /// </summary>
        /// <param name="dataList">Menu List</param>
        /// <param name="userID">User Guid</param>
        /// <returns></returns>
        public bool Update_byUser(string dbs, List<AuthRel> dataList, string userID)
        {
            try
            {
                //----- 判斷資料庫 -----
                switch (dbs)
                {
                    case "1":
                        return GetUpdate_byUser_Local(dataList, userID);

                    default:
                        return GetUpdate_byUser(dataList, userID, dbs);

                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        /// <summary>
        /// 更新權限 (DB=Local)
        /// </summary>
        /// <param name="dataList">ID List</param>
        /// <param name="userID">guid</param>
        /// <returns></returns>
        private bool GetUpdate_byUser_Local(List<AuthRel> dataList, string userID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Home_Menu_UserAuth WHERE (User_Guid = @userID);");

                int row = 0;
                foreach (var data in dataList)
                {
                    sql.AppendLine(" INSERT INTO Home_Menu_UserAuth(Menu_ID, User_Guid) VALUES (@menuID_{0}, @userID);"
                        .FormatThis(row));

                    cmd.Parameters.AddWithValue("menuID_{0}".FormatThis(row), data.MenuID);

                    row++;
                }

                cmd.Parameters.AddWithValue("userID", userID);


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();


                return dbConn.ExecuteSql(cmd, dbConn.DBS.Local, out ErrMsg);
            }
        }


        /// <summary>
        /// 更新權限
        /// </summary>
        /// <param name="dataList">ID List</param>
        /// <param name="userID">guid</param>
        /// <returns></returns>
        private bool GetUpdate_byUser(List<AuthRel> dataList, string userID, string dbs)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            string dbName = Get_DBName(dbs);

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM [{0}].dbo.User_Profile_Rel_Program WHERE (Guid = @userID);".FormatThis(dbName));

                int row = 0;
                foreach (var data in dataList)
                {
                    sql.AppendLine(" INSERT INTO [{1}].dbo.User_Profile_Rel_Program(Prog_ID, Guid) VALUES (@menuID_{0}, @userID);".FormatThis(
                        row
                        , dbName));

                    cmd.Parameters.AddWithValue("menuID_{0}".FormatThis(row), data.MenuID);

                    row++;
                }

                cmd.Parameters.AddWithValue("userID", userID);


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();


                return dbConn.ExecuteSql(cmd, dbConn.DBS.Local, out ErrMsg);
            }
        }

        #endregion


        #region -----// Update - 依功能 //-----

        /// <summary>
        /// 更新權限 - byFunc
        /// </summary>
        /// <param name="dbs">資料庫別</param>
        /// <param name="menuList">Menu checked id</param>
        /// <param name="userList">User checked id</param>
        /// <returns></returns>
        public bool Update_byFunc(string dbs, List<AuthRel> menuList, List<AuthRel> userList)
        {
            try
            {
                //----- 判斷資料庫 -----
                switch (dbs)
                {
                    case "1":
                        return GetUpdate_byFunc_Local(menuList, userList);

                    default:
                        //權限中心
                        return GetUpdate_byFunc(menuList, userList, dbs);

                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        /// <summary>
        /// 更新權限 (DB=權限中心)
        /// </summary>
        /// <param name="menuList">Menu checked id</param>
        /// <param name="userList">User checked id</param>
        /// <returns></returns>
        private bool GetUpdate_byFunc_Local(List<AuthRel> menuList, List<AuthRel> userList)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                int row = 0;
                foreach (var menu in menuList)
                {
                    foreach (var user in userList)
                    {
                        sql.AppendLine(" IF NOT EXISTS (SELECT Menu_ID FROM Home_Menu_UserAuth WHERE (Menu_ID = @menuID_{0}) AND (User_Guid = @userID_{0}))"
                            .FormatThis(row));
                        sql.AppendLine(" BEGIN");
                        sql.AppendLine("  INSERT INTO Home_Menu_UserAuth(Menu_ID, User_Guid) VALUES (@menuID_{0}, @userID_{0});"
                            .FormatThis(row));
                        sql.AppendLine(" END");


                        cmd.Parameters.AddWithValue("menuID_{0}".FormatThis(row), menu.MenuID);
                        cmd.Parameters.AddWithValue("userID_{0}".FormatThis(row), user.UserID);
                        row++;
                    }
                }


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();


                return dbConn.ExecuteSql(cmd, dbConn.DBS.Local, out ErrMsg);
            }
        }

        /// <summary>
        /// 更新權限 (DB=產品中心)
        /// </summary>
        /// <param name="menuList">Menu checked id</param>
        /// <param name="userList">User checked id</param>
        /// <returns></returns>
        private bool GetUpdate_byFunc(List<AuthRel> menuList, List<AuthRel> userList, string dbs)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();
            string dbName = Get_DBName(dbs);

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                int row = 0;
                foreach (var menu in menuList)
                {
                    foreach (var user in userList)
                    {
                        sql.AppendLine(" IF NOT EXISTS (SELECT Prog_ID FROM [{1}].dbo.User_Profile_Rel_Program WHERE (Prog_ID = @menuID_{0}) AND (Guid = @userID_{0}))".FormatThis(
                            row
                            , dbName));
                        sql.AppendLine(" BEGIN");
                        sql.AppendLine("  INSERT INTO [{1}].dbo.User_Profile_Rel_Program(Prog_ID, Guid) VALUES (@menuID_{0}, @userID_{0});".FormatThis(
                            row
                            , dbName));
                        sql.AppendLine(" END");


                        cmd.Parameters.AddWithValue("menuID_{0}".FormatThis(row), menu.MenuID);
                        cmd.Parameters.AddWithValue("userID_{0}".FormatThis(row), user.UserID);
                        row++;
                    }
                }


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();


                return dbConn.ExecuteSql(cmd, dbConn.DBS.Local, out ErrMsg);
            }
        }


        #endregion


        #region -----// Update - 複製 //-----
        /// <summary>
        /// 人員權限複製
        /// </summary>
        /// <param name="sourceUserID"></param>
        /// <param name="targetUserID"></param>
        /// <returns></returns>
        public bool Update_Copy(string dbs, string sourceUserID, string targetUserID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 語法 -----
                switch (dbs)
                {
                    case "1":
                        //Local

                        // - 刪除目標權限
                        sql.AppendLine("DELETE FROM Home_Menu_UserAuth WHERE (User_Guid = @targetUserID); ");

                        // - 複製來源權限
                        sql.AppendLine("INSERT INTO Home_Menu_UserAuth (User_Guid,Menu_ID) ");
                        sql.AppendLine("SELECT @targetUserID, Menu_ID FROM Home_Menu_UserAuth WHERE (User_Guid = @sourceUserID); ");

                        break;

                    default:
                        //其他
                        string dbName = Get_DBName(dbs);
                        // - 刪除目標權限
                        sql.AppendLine("DELETE FROM {0}.dbo.User_Profile_Rel_Program WHERE (Guid = @targetUserID); ".FormatThis(dbName));

                        // - 複製來源權限
                        sql.AppendLine("INSERT INTO {0}.dbo.User_Profile_Rel_Program (Guid,Prog_ID) ".FormatThis(dbName));
                        sql.AppendLine("SELECT @targetUserID, Prog_ID FROM {0}.dbo.User_Profile_Rel_Program WHERE (Guid = @sourceUserID); ".FormatThis(dbName));


                        break;
                }


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("sourceUserID", sourceUserID);
                cmd.Parameters.AddWithValue("targetUserID", targetUserID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.Local, out ErrMsg);
            }
        }

        #endregion

        #region -- 取得原始資料 --


        /// <summary>
        /// 取得資料庫實體名稱
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string Get_DBName(string id)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT DB_Name FROM Home_DB WITH (NOLOCK)");
                sql.AppendLine(" WHERE (UID = @ID)");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ID", id);

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, out ErrMsg))
                {
                    if (DT.Rows.Count > 0)
                    {
                        return DT.Rows[0]["DB_Name"].ToString();
                    }
                    else
                    {
                        return "";
                    }
                }

            }
        }

        /// <summary>
        /// 取得原始資料
        /// </summary>
        /// <param name="dbs">資料庫別</param>
        /// <param name="dbName">資料庫名</param>
        /// <param name="userGuid">使用者GUID</param>
        /// <returns></returns>
        private DataTable LookupRawData(string dbs, string dbName, string userGuid)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                switch (dbs)
                {
                    case "1":
                        //Home Local
                        sql.Append(GetSQL_Local());
                        break;

                    default:
                        sql.Append(GetSQL(dbName));
                        break;
                }


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("UserGuid", userGuid);

                //----- 回傳資料 -----
                return dbConn.LookupDT(cmd, out ErrMsg);
            }
        }

        /// <summary>
        /// SQL - 權限中心
        /// </summary>
        /// <returns></returns>
        private StringBuilder GetSQL_Local()
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.Menu_ID AS ID, Base.Parent_ID AS ParentID, Base.MenuName_zh_TW AS MenuName, Base.Menu_Level Level");
            sql.AppendLine("  , (CASE WHEN Auth.User_Guid IS NULL THEN 'N' ELSE 'Y' END) AS IsChecked");
            sql.AppendLine(" FROM Home_Menu Base WITH (NOLOCK)");
            sql.AppendLine("  LEFT JOIN Home_Menu_UserAuth Auth WITH (NOLOCK) ON Base.Menu_ID = Auth.Menu_ID AND (Auth.User_Guid = @UserGuid)");
            sql.AppendLine(" WHERE (Base.Display = 'Y')");
            sql.AppendLine(" ORDER BY Base.Sort, Base.Menu_ID");

            return sql;
        }


        private StringBuilder GetSQL(string dbName)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine(" SELECT Base.Prog_ID AS ID, Base.Up_Id AS ParentID, Base.Prog_Name_zh_TW AS MenuName, Base.lv Level");
            sql.AppendLine("  , (CASE WHEN Auth.Guid IS NULL THEN 'N' ELSE 'Y' END) AS IsChecked");
            sql.AppendLine(" FROM [{0}].dbo.Program Base WITH (NOLOCK)".FormatThis(dbName));
            sql.AppendLine("  LEFT JOIN [{0}].dbo.User_Profile_Rel_Program Auth WITH (NOLOCK) ON Base.Prog_ID = Auth.Prog_ID AND (Auth.Guid = @UserGuid)".FormatThis(dbName));
            sql.AppendLine(" WHERE (Base.Display = 'Y')");
            sql.AppendLine(" ORDER BY Base.Sort, Base.Prog_ID");

            return sql;
        }

        #endregion

    }
}

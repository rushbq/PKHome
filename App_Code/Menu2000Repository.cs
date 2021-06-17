using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Menu2000Data.Models;
using PKLib_Method.Methods;

/*
  [製物工單]-MarketingHelp
  [郵件寄送登記]-Postal
*/
namespace Menu2000Data.Controllers
{

    public class Menu2000Repository
    {
        public string ErrMsg;

        #region -----// Read //-----


        #region *** 製物工單 S ***
        /// <summary>
        /// [製物工單] 資料列表
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<MKHelpItem> GetMKHelpList(string CompID, Dictionary<string, string> search, Dictionary<string, string> sort
            , out string ErrMsg)
        {
            //----- 宣告 -----
            List<MKHelpItem> dataList = new List<MKHelpItem>();
            StringBuilder sql = new StringBuilder();
            string dbName = GetDBName(CompID);

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @TODAY nChar(8)");
                sql.AppendLine(" SET @TODAY = CONVERT(nChar(8),GETDATE(),112)");

                sql.AppendLine(" SELECT Base.Data_ID, Base.TraceID, Base.CompID, Base.Req_Who, Base.Req_Dept");
                sql.AppendLine("  , Base.Req_Subject, Base.Req_Content, Base.Req_Qty");
                sql.AppendLine("  , Base.Req_Status, Base.Req_Class, Base.Req_Res, Base.Emg_Status");
                sql.AppendLine("  , Base.Wish_Date, Base.Est_Date, Base.Finish_Hours, Base.Finish_Date");
                sql.AppendLine("  , Base.Create_Time, Base.Update_Time");
                sql.AppendLine("  , ClsSt.Class_Name AS StName"); //處理狀態
                sql.AppendLine("  , ClsSt.CustomID AS StDisp"); //處理狀態:顏色判斷用編號
                sql.AppendLine("  , ClsType.Class_Name AS TypeName"); //需求類別
                sql.AppendLine("  , ClsRes.Class_Name AS ResName"); //需求資源
                sql.AppendLine("  , ClsEmg.Class_Name AS EmgName"); //緊急度
                sql.AppendLine("  , (CASE WHEN @TODAY > Base.Est_Date AND Base.Req_Status NOT IN (4,8) THEN 'Y' ELSE 'N' END) AS IsTimeOut"); //是否逾時(已結案的不判斷)
                sql.AppendLine("  , (SELECT TOP 1 Display_Name FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE(Account_Name = Base.Req_Who)) AS Req_Name");
                sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE([Guid] = Base.Create_Who)) AS Create_Name");
                sql.AppendLine("  , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE([Guid] = Base.Update_Who)) AS Update_Name");
                sql.AppendLine("  , Prof.Email AS Req_Email");
                sql.AppendLine("  , (SELECT COUNT(*) FROM MK_Help_Assigned WHERE (Parent_ID = Base.Data_ID)) AS ProcCnt");
                sql.AppendLine(" FROM MK_Help Base");
                sql.AppendLine("  INNER JOIN MK_ParamClass ClsSt ON Base.Req_Status = ClsSt.Class_ID");
                sql.AppendLine("  INNER JOIN MK_ParamClass ClsType ON Base.Req_Class = ClsType.Class_ID");
                sql.AppendLine("  INNER JOIN MK_ParamClass ClsRes ON Base.Req_Res = ClsRes.Class_ID");
                sql.AppendLine("  INNER JOIN MK_ParamClass ClsEmg ON Base.Emg_Status = ClsEmg.Class_ID");
                sql.AppendLine("  LEFT JOIN PKSYS.dbo.User_Profile Prof ON Base.Req_Who = Prof.Account_Name");
                sql.AppendLine(" WHERE (Base.CompID = @CompID)");

                /* Search */
                #region >> filter <<

                if (search != null)
                {
                    //過濾空值
                    var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));
                    string filterDateType = "Base.Create_Time";

                    //查詢內容
                    foreach (var item in thisSearch)
                    {
                        switch (item.Key)
                        {
                            case "DataID":
                                sql.Append(" AND (Base.Data_ID = @Data_ID)");

                                cmd.Parameters.AddWithValue("Data_ID", item.Value);

                                break;

                            case "Keyword":
                                //關鍵字(追蹤碼/主旨)
                                sql.Append(" AND (");
                                sql.Append(" Base.TraceID LIKE '%' + UPPER(@keyword) + '%'");
                                sql.Append(" OR Base.Req_Subject LIKE '%' + UPPER(@keyword) + '%'");
                                sql.Append(")");

                                cmd.Parameters.AddWithValue("keyword", item.Value);

                                break;

                            case "DateType":
                                //** 放在日期之前 **
                                switch (item.Value)
                                {
                                    case "A":
                                        filterDateType = "Base.Create_Time";
                                        break;

                                    case "B":
                                        filterDateType = "Base.Finish_Date";
                                        break;

                                    default:
                                        filterDateType = "Base.Create_Time";
                                        break;
                                }

                                break;

                            case "sDate":
                                sql.Append(" AND ({0} >= @sDate)".FormatThis(filterDateType));
                                cmd.Parameters.AddWithValue("sDate", item.Value + " 00:00");

                                break;
                            case "eDate":
                                sql.Append(" AND ({0} <= @eDate)".FormatThis(filterDateType));
                                cmd.Parameters.AddWithValue("eDate", item.Value + " 23:59");

                                break;
                                

                            case "reqStat":
                                //處理狀態
                                sql.Append(" AND (Base.Req_Status = @reqStat)");

                                cmd.Parameters.AddWithValue("reqStat", item.Value);

                                break;

                            case "reqClass":
                                //需求類別
                                sql.Append(" AND (Base.Req_Class = @reqClass)");

                                cmd.Parameters.AddWithValue("reqClass", item.Value);

                                break;

                            case "reqRes":
                                //需求資源
                                sql.Append(" AND (Base.Req_Res = @reqRes)");

                                cmd.Parameters.AddWithValue("reqRes", item.Value);

                                break;

                            case "emgStat":
                                //緊急度
                                sql.Append(" AND (Base.Emg_Status = @emgStat)");

                                cmd.Parameters.AddWithValue("emgStat", item.Value);

                                break;

                            case "reqDept":
                                //需求部門
                                sql.Append(" AND (Base.Req_Dept = @reqDept)");

                                cmd.Parameters.AddWithValue("reqDept", item.Value);

                                break;

                            case "reqWho":
                                //需求者
                                sql.Append(" AND (Base.Req_Who = @reqWho)");

                                cmd.Parameters.AddWithValue("reqWho", item.Value);

                                break;

                            case "Proc":
                                //處理人員:傳入值為字串
                                //將來源字串轉為陣列,以逗號為分隔
                                string[] strAry = Regex.Split(item.Value, @"\,{1}");
                                ArrayList aryList = new ArrayList(strAry);

                                //GetSQLParam:SQL WHERE IN的方法
                                sql.AppendLine(" AND (Base.Data_ID IN (");
                                sql.AppendLine("  SELECT Rel.Parent_ID FROM PKSYS.dbo.User_Profile Prof");
                                sql.AppendLine("   INNER JOIN MK_Help_Assigned Rel ON Prof.[Guid] = Rel.Who");
                                sql.AppendLine("  WHERE (Prof.Account_Name IN ({0}))".FormatThis(CustomExtension.GetSQLParam(aryList, "params")));
                                sql.AppendLine(" ))");
                                for (int row = 0; row < aryList.Count; row++)
                                {
                                    cmd.Parameters.AddWithValue("params" + row, aryList[row]);
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
                                        //登記日
                                        thisSortField = "Base.Create_Time";
                                        break;

                                    case "B":
                                        //希望完成日
                                        thisSortField = "Base.Wish_Date";
                                        break;

                                    case "C":
                                        //預計完成日
                                        thisSortField = "Base.Est_Date";
                                        break;

                                    default:
                                        //結案日
                                        thisSortField = "Base.Finish_Date";
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
                        //預設排序
                        sql.AppendLine(" ORDER BY Base.Create_Time DESC");
                    }

                }
                #endregion


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒
                cmd.Parameters.AddWithValue("CompID", CompID);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new MKHelpItem
                        {
                            Data_ID = item.Field<Guid>("Data_ID"),
                            CompID = item.Field<string>("CompID"),
                            TraceID = item.Field<string>("TraceID"),
                            Req_Who = item.Field<string>("Req_Who"),
                            Req_Name = item.Field<string>("Req_Name"),
                            Req_Email = item.Field<string>("Req_Email"),
                            Req_Dept = item.Field<string>("Req_Dept"),
                            Req_Subject = item.Field<string>("Req_Subject"),
                            Req_Content = item.Field<string>("Req_Content"),
                            Req_Qty = item.Field<int>("Req_Qty"),
                            Req_Status = item.Field<int>("Req_Status"),
                            Req_Class = item.Field<int>("Req_Class"),
                            Req_Res = item.Field<int>("Req_Res"),
                            Emg_Status = item.Field<int>("Emg_Status"),
                            StName = item.Field<string>("StName"),
                            StDisp = item.Field<string>("StDisp"),
                            TypeName = item.Field<string>("TypeName"),
                            ResName = item.Field<string>("ResName"),
                            EmgName = item.Field<string>("EmgName"),
                            Wish_Date = item.Field<DateTime?>("Wish_Date").ToString().ToDateString("yyyy/MM/dd"),
                            Est_Date = item.Field<DateTime?>("Est_Date").ToString().ToDateString("yyyy/MM/dd"),
                            Finish_Date = item.Field<DateTime?>("Finish_Date").ToString().ToDateString("yyyy/MM/dd"),
                            Finish_Hours = item.Field<double?>("Finish_Hours"),
                            IsTimeOut = item.Field<string>("IsTimeOut"),
                            ProcCnt = item.Field<int>("ProcCnt"),

                            Create_Time = item.Field<DateTime?>("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),
                            Update_Time = item.Field<DateTime?>("Update_Time").ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),
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
        /// [製物工單] 取得目前處理狀態(CustomID)
        /// </summary>
        /// <param name="id">MKHelp.DataID</param>
        /// <returns></returns>
        public string GetOne_MKHelpStatus(string id)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Cls.CustomID");
                sql.AppendLine(" FROM MK_Help Base");
                sql.AppendLine("  INNER JOIN MK_ParamClass Cls ON Base.Req_Status = Cls.Class_ID");
                sql.AppendLine(" WHERE (Base.Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", id);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    if (DT == null)
                    {
                        return "";
                    }
                    else
                    {
                        return DT.Rows[0]["CustomID"].ToString();
                    }
                }
            }
        }

        /// <summary>
        /// [製物工單] 取得檔案附件
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public IQueryable<MKHelpAttachment> GetMKHelpFileList(string parentID)
        {
            //----- 宣告 -----
            List<MKHelpAttachment> dataList = new List<MKHelpAttachment>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Data_ID, AttachFile, AttachFile_Org");
                sql.AppendLine(" FROM MK_Help_Attachment WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID)");
                sql.AppendLine(" ORDER BY Create_Time");


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
                        var data = new MKHelpAttachment
                        {
                            Data_ID = item.Field<int>("Data_ID"),
                            AttachFile = item.Field<string>("AttachFile"),
                            AttachFile_Org = item.Field<string>("AttachFile_Org")
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
        /// [製物工單] 取得轉寄人員
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public IQueryable<MKHelpCC> GetMKHelpCCList(string parentID)
        {
            //----- 宣告 -----
            List<MKHelpCC> dataList = new List<MKHelpCC>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Prof.Display_Name AS Who, Base.CC_Email AS Email");
                sql.AppendLine(" FROM MK_Help_CC Base");
                sql.AppendLine("  INNER JOIN PKSYS.dbo.User_Profile Prof ON Base.CC_Who = Prof.[Guid]");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID)");
                sql.AppendLine(" ORDER BY Prof.DeptID, Prof.Account_Name");

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
                        var data = new MKHelpCC
                        {
                            CC_Who = item.Field<string>("Who"),
                            CC_Email = item.Field<string>("Email")
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
        /// [製物工單] 取得處理人員
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public IQueryable<MKHelpAssigned> GetMKHelpAssignList(string parentID)
        {
            //----- 宣告 -----
            List<MKHelpAssigned> dataList = new List<MKHelpAssigned>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Prof.Display_Name AS Who, Prof.Account_Name AS WhoID, Prof.Email AS Email");
                sql.AppendLine(" FROM MK_Help_Assigned Base");
                sql.AppendLine("  INNER JOIN PKSYS.dbo.User_Profile Prof ON Base.Who = Prof.[Guid]");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID)");
                sql.AppendLine(" ORDER BY Prof.DeptID, Prof.Account_Name");

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
                        var data = new MKHelpAssigned
                        {
                            Who = item.Field<string>("Who"),
                            WhoID = item.Field<string>("WhoID"),
                            Email = item.Field<string>("Email")
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
        /// [製物工單] 取得進度說明
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public IQueryable<MKHelpReply> GetMKHelpReplyList(string parentID)
        {
            //----- 宣告 -----
            List<MKHelpReply> dataList = new List<MKHelpReply>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Base.Data_ID, Prof.Display_Name AS Who, Base.Reply_Content, Base.Create_Time");
                sql.AppendLine(" FROM MK_Help_Reply Base");
                sql.AppendLine("  INNER JOIN PKSYS.dbo.User_Profile Prof ON Base.Create_Who = Prof.[Guid]");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID)");
                sql.AppendLine(" ORDER BY Base.Create_Time DESC");

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
                        var data = new MKHelpReply
                        {
                            Data_ID = item.Field<int>("Data_ID"),
                            Reply_Content = item.Field<string>("Reply_Content"),
                            Create_Time = item.Field<DateTime>("Create_Time").ToString().ToDateString("yy/MM/dd HH:mm:ss"),
                            Create_Name = item.Field<string>("Who")
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
        /// [製物工單] 固定收信清單
        /// </summary>
        /// <param name="type">TW新需求=10 / TW結案=15 / SZ新需求=20 / SZ結案=25</param>
        /// <returns></returns>
        public IQueryable<MKHelpReceiver> GetMKHelpReceiver(string type)
        {
            //----- 宣告 -----
            List<MKHelpReceiver> dataList = new List<MKHelpReceiver>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT MailAddress AS Email");
                sql.AppendLine(" FROM MK_Help_Receiver");
                sql.AppendLine(" WHERE (MailType = @type) AND (Display = 'Y')");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("type", type);


                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new MKHelpReceiver
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

        /// <summary>
        /// [製物工單] 需求類別(處理狀態/緊急度/需求類別/需求資源)
        /// </summary>
        /// <param name="compID"></param>
        /// <param name="typeName"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetMKHelpClass(string compID, string typeName, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name AS Label, CustomID");
                sql.AppendLine(" FROM MK_ParamClass WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Class_Type = @type) AND (UPPER(CompID) = UPPER(@compID)) AND (Display = 'Y')");
                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("compID", compID);
                cmd.Parameters.AddWithValue("type", typeName);

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
                            CustomID = item.Field<string>("CustomID")
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
        /// [製物工單] 反查需求類別ID, 以自訂編號查詢該公司別的ID
        /// </summary>
        /// <param name="compID"></param>
        /// <param name="typeName"></param>
        /// <param name="customID">自訂編號</param>
        /// <returns></returns>
        public Int32 GetOne_MKHelpClassID(string compID, string typeName, string customID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID");
                sql.AppendLine(" FROM MK_ParamClass WITH(NOLOCK)");
                sql.AppendLine(" WHERE (Class_Type = @type) AND (UPPER(CompID) = UPPER(@compID)) AND (CustomID = @customID)");
                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("compID", compID);
                cmd.Parameters.AddWithValue("type", typeName);
                cmd.Parameters.AddWithValue("customID", customID);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    if (DT == null)
                    {
                        return 0;
                    }

                    return Convert.ToInt32(DT.Rows[0]["ID"]);
                }

            }
        }


        /// <summary>
        /// [製物工單] 取得圖表統計資料
        /// </summary>
        /// <param name="CompID"></param>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ChartData> GetMKHelpChartData(string CompID, string type, Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ChartData> dataList = new List<ChartData>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                switch (type.ToUpper())
                {
                    case "WHO":
                        //依派工人員(不含時數)
                        sql.AppendLine(" SELECT (Prof.Display_Name) AS Label");
                        sql.AppendLine("  , COUNT(Base.Data_ID) AS GroupCnt");
                        sql.AppendLine("  , SUM(Base.Req_Qty) AS SumQty, SUM(Finish_Hours) AS GroupHours");
                        sql.AppendLine(" FROM MK_Help Base WITH(NOLOCK)");
                        sql.AppendLine("  INNER JOIN MK_Help_Assigned Assign ON Base.Data_ID = Assign.Parent_ID");
                        sql.AppendLine("  INNER JOIN PKSYS.dbo.User_Profile Prof ON Assign.Who = Prof.Guid");
                        sql.AppendLine(" WHERE (Base.CompID = @CompID) AND (Base.Req_Status = @Status)");

                        break;

                    default:
                        //依類別
                        sql.AppendLine(" SELECT (Cls.Class_Name) AS Label, COUNT(Base.Data_ID) AS GroupCnt, SUM(Base.Req_Qty) AS SumQty, SUM(Finish_Hours) AS GroupHours");
                        sql.AppendLine(" FROM MK_Help Base WITH(NOLOCK)");
                        sql.AppendLine("  INNER JOIN MK_ParamClass Cls WITH(NOLOCK) ON Base.Req_Res = Cls.Class_ID");
                        sql.AppendLine(" WHERE (Cls.Display = 'Y')");
                        sql.AppendLine("  AND (Base.CompID = @CompID) AND (Base.Req_Status = @Status)");

                        break;
                }

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
                                //登記日期-Start
                                sql.Append(" AND (Base.Create_Time >= @sDate)");

                                cmd.Parameters.AddWithValue("sDate", item.Value + " 00:00");

                                break;

                            case "eDate":
                                //登記日期-End
                                sql.Append(" AND (Base.Create_Time <= @eDate)");

                                cmd.Parameters.AddWithValue("eDate", item.Value + " 23:59:59");

                                break;

                            case "procWho":
                                //處理人員
                                switch (type.ToUpper())
                                {
                                    case "WHO":
                                        //依派工人員
                                        sql.Append(" AND (Assign.Who = @Who)");

                                        break;

                                    default:
                                        //依類別
                                        sql.AppendLine(" AND (Base.Data_ID IN (");
                                        sql.AppendLine("  SELECT Parent_ID");
                                        sql.AppendLine("  FROM MK_Help_Assigned");
                                        sql.AppendLine("  WHERE Who = @Who");
                                        sql.AppendLine(" ))");

                                        break;
                                }

                                cmd.Parameters.AddWithValue("Who", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                //Group BY
                switch (type.ToUpper())
                {
                    case "WHO":
                        //依派工人員
                        sql.AppendLine(" GROUP BY Prof.Display_Name");

                        break;

                    default:
                        //依類別
                        sql.AppendLine(" GROUP BY Cls.Class_Name");

                        break;
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒
                cmd.Parameters.AddWithValue("CompID", CompID);
                cmd.Parameters.AddWithValue("Status", CompID.Equals("TW") ? 4 : 8); //處理狀態=已結案

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new ChartData
                        {
                            Label = item.Field<string>("Label"),
                            Cnt = item.Field<int>("GroupCnt"), //件數
                            Hours = item.Field<double>("GroupHours"), //時數
                            SumQty = item.Field<int>("SumQty") //數量
                        };

                        //將項目加入至集合
                        dataList.Add(data);
                    }
                }

                //回傳集合
                return dataList.AsQueryable();
            }

        }

        #endregion *** 製物工單 E ***


        #region *** 郵件寄送登記 S ***
        /// <summary>
        /// [郵件寄送登記] 收件人設定清單
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<ClassItem> GetPostalAddress(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT SeqNo, ToWho, ToAddr");
                sql.AppendLine(" FROM Postal_Address");
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
                            case "Who":
                                sql.Append(" AND (Create_Who = @Who)");

                                cmd.Parameters.AddWithValue("Who", item.Value);

                                break;

                            case "Keyword":
                                sql.Append(" AND (");
                                sql.Append(" ToWho LIKE '%' + UPPER(@keyword) + '%'");
                                sql.Append(" OR ToAddr LIKE '%' + UPPER(@keyword) + '%'");
                                sql.Append(")");

                                cmd.Parameters.AddWithValue("keyword", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                //----- 資料取得 -----
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
                            ID = item.Field<Int32>("SeqNo"),
                            CustomID = item.Field<string>("ToWho"),
                            Label = item.Field<string>("ToAddr")
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
        /// [郵件寄送登記] 取得開關狀態
        /// </summary>
        /// <param name="targetDate"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public string GetPostalStatus(string targetDate, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" IF(SELECT COUNT(*) FROM Postal_DayCheck WHERE(PostDate = @targetDay)) = 0");
                sql.AppendLine("  BEGIN");
                sql.AppendLine("   INSERT INTO Postal_DayCheck(PostDate, IsClose)");
                sql.AppendLine("   VALUES(@targetDay, 'N')");
                sql.AppendLine("  END");
                sql.AppendLine(" SELECT IsClose");
                sql.AppendLine(" FROM Postal_DayCheck");
                sql.AppendLine(" WHERE(PostDate = @targetDay)");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("targetDay", targetDate.ToDateString("yyyy/MM/dd"));

                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                {
                    return DT.Rows[0]["IsClose"].ToString();

                }
            }
        }


        /// <summary>
        /// [郵件寄送登記] 取得郵式清單
        /// </summary>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetPostalClass(out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Class_ID AS ID, Class_Name AS Label");
                sql.AppendLine(" FROM Postal_RefClass");
                sql.AppendLine(" WHERE (Display = 'Y')");
                sql.AppendLine(" ORDER BY Sort");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                {
                    return DT;
                }

            }
        }


        /// <summary>
        /// [郵件寄送登記] 取得資料
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<PostalItem> GetPostalData(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<PostalItem> dataList = new List<PostalItem>();
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Base.SeqNo, Base.Data_ID");
                sql.AppendLine(" , Base.PostDate, Base.PostWho, Dept.DeptName, Prof.Display_Name");
                sql.AppendLine(" , Base.PostType, Ref.Class_Name AS PostTypeName, Base.PostNo");
                sql.AppendLine(" , Base.ToWho, Base.ToAddr, Base.PackageWeight, Base.PostDesc, Base.PostPrice");
                sql.AppendLine(" , Base.Create_Who, Base.Create_Time, Base.Update_Who, Base.Update_Time");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Create_Who)) AS Create_Name");
                sql.AppendLine(" , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Update_Who)) AS Update_Name");
                sql.AppendLine(" FROM Postal_Data Base");
                sql.AppendLine("  INNER JOIN Postal_RefClass Ref ON Base.PostType = Ref.Class_ID");
                sql.AppendLine("  INNER JOIN [PKSYS].dbo.User_Profile Prof ON Base.PostWho = Prof.Account_Name");
                sql.AppendLine("  INNER JOIN [PKSYS].dbo.User_Dept Dept ON Prof.DeptID = Dept.DeptID");
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

                            case "PostType":
                                sql.Append(" AND (Base.PostType = @PostType)");

                                cmd.Parameters.AddWithValue("PostType", item.Value);
                                break;

                            case "sDate":
                                sql.Append(" AND (Base.PostDate >= @sDate)");

                                cmd.Parameters.AddWithValue("sDate", item.Value);
                                break;

                            case "eDate":
                                sql.Append(" AND (Base.PostDate <= @eDate)");

                                cmd.Parameters.AddWithValue("eDate", item.Value);
                                break;

                            case "Keyword":
                                sql.Append(" AND (");
                                sql.Append(" (Base.PostNo LIKE '%' + UPPER(@keyword) + '%')");
                                sql.Append(" OR (Base.ToWho LIKE '%' + UPPER(@keyword) + '%')");
                                sql.Append(")");

                                cmd.Parameters.AddWithValue("keyword", item.Value);
                                break;
                        }
                    }
                }
                #endregion

                sql.AppendLine(" ORDER BY Base.PostDate DESC, Base.Create_Time DESC");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new PostalItem
                        {
                            Data_ID = item.Field<Guid>("Data_ID"),
                            SeqNo = item.Field<Int32>("SeqNo"),
                            PostDate = item.Field<DateTime>("PostDate").ToString().ToDateString("yyyy/MM/dd"),
                            PostWho = item.Field<string>("PostWho"),
                            Post_WhoName = item.Field<string>("Display_Name"),
                            Post_DeptName = item.Field<string>("DeptName"),
                            PostType = item.Field<Int32>("PostType"),
                            PostTypeName = item.Field<string>("PostTypeName"),
                            PostNo = item.Field<string>("PostNo"),
                            ToWho = item.Field<string>("ToWho"),
                            ToAddr = item.Field<string>("ToAddr"),
                            PackageWeight = item.Field<double>("PackageWeight"),
                            PostDesc = item.Field<string>("PostDesc"),
                            PostPrice = item.Field<double>("PostPrice"),

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
        /// [郵件寄送登記] 月統計
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetPostalStat(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----

                sql.Append(" DECLARE @year INT");
                sql.Append(" SET @year = @setYear;");
                sql.Append(" WITH YearRange AS");
                sql.Append(" (");
                sql.Append("  SELECT  1 AS showDate");
                sql.Append("   , 0 AS seq");
                sql.Append(" UNION ALL");
                sql.Append("  SELECT showDate + 1 AS showDate");
                sql.Append("   , seq + 1 AS seq");
                sql.Append("  FROM YearRange YW");
                sql.Append("  WHERE seq < 11");
                sql.Append(" )");
                sql.Append(" , TblData AS (");
                sql.Append("  SELECT ISNULL((Base.PostPrice), 0) AS Price, DATEPART(MM, Base.PostDate) AS myMonth");
                sql.Append("  , Dept.DeptName, Dept.Sort, Dept.DeptID");
                sql.Append("  FROM Postal_Data Base");
                sql.Append("   INNER JOIN [PKSYS].dbo.User_Profile Prof ON Base.PostWho = Prof.Account_Name");
                sql.Append("   INNER JOIN [PKSYS].dbo.User_Dept Dept ON Prof.DeptID = Dept.DeptID");
                sql.Append("  WHERE (YEAR(PostDate) = @year)");
                sql.Append(" )");
                sql.Append(" SELECT Pvt.DeptName");
                sql.Append("  , Pvt.[1], Pvt.[2], Pvt.[3], Pvt.[4], Pvt.[5], Pvt.[6]");
                sql.Append("  , Pvt.[7], Pvt.[8], Pvt.[9], Pvt.[10], Pvt.[11], Pvt.[12]");
                sql.Append(" FROM (");
                sql.Append(" 	SELECT YWR.showDate");
                sql.Append(" 	, TblData.DeptName AS DeptName, TblData.Sort, TblData.DeptID");
                sql.Append(" 	, ISNULL(TblData.Price, 0) AS ItemPrice");
                sql.Append(" 	FROM YearRange YWR");
                sql.Append(" 	 INNER JOIN TblData ON (YWR.showDate) = TblData.myMonth	");
                sql.Append(" ) AS P");
                sql.Append(" PIVOT");
                sql.Append(" (");
                sql.Append("  SUM(ItemPrice)");
                sql.Append("  FOR showDate");
                sql.Append("  IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12])");
                sql.Append(" ) AS Pvt");
                sql.Append(" ORDER BY Pvt.Sort, Pvt.DeptID");

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

                        }
                    }
                }
                #endregion

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKExcel, out ErrMsg))
                {
                    return DT;
                }
            }

        }

        #endregion


        #endregion



        #region -----// Create //-----


        #region *** 製物工單 S ***
        /// <summary>
        /// [製物工單] 建立製物工單-基本資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateMKHelp_Base(MKHelpItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine("DECLARE @NewID AS VARCHAR(4), @TraceID AS VARCHAR(6), @thisDate AS VARCHAR(2)");
                //前2碼:年
                sql.AppendLine(" SET @thisDate = LEFT(CONVERT(nChar(8),GETDATE(),2), 2)");

                sql.AppendLine(" SET @NewID = (");
                //後4碼:流水號4碼
                sql.AppendLine("  SELECT ISNULL(MAX(CAST(RIGHT(TraceID ,4) AS INT)) ,0) + 1 FROM MK_Help");
                sql.AppendLine("  WHERE (SUBSTRING(TraceID,1,2) = @thisDate) AND (CompID = @CompID)");
                sql.AppendLine(" )");
                sql.AppendLine(" SET @TraceID = @thisDate + RIGHT('000' + @NewID, 4)");

                sql.AppendLine(" INSERT INTO MK_Help(");
                sql.AppendLine("  Data_ID, TraceID, CompID, Req_Who, Req_Dept");
                sql.AppendLine("  , Req_Subject, Req_Content, Req_Qty");
                sql.AppendLine("  , Req_Status, Req_Class, Req_Res, Emg_Status");
                sql.AppendLine("  , Wish_Date, Est_Date, Finish_Hours, Finish_Date");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, @TraceID, @CompID, @Req_Who, @Req_Dept");
                sql.AppendLine("  , @Req_Subject, @Req_Content, @Req_Qty");
                sql.AppendLine("  , @Req_Status, @Req_Class, @Req_Res, @Emg_Status");
                sql.AppendLine("  , @Wish_Date, @Est_Date, @Finish_Hours, @Finish_Date");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" )");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("CompID", instance.CompID);
                cmd.Parameters.AddWithValue("Req_Who", instance.Req_Who);
                cmd.Parameters.AddWithValue("Req_Dept", instance.Req_Dept);
                cmd.Parameters.AddWithValue("Req_Subject", instance.Req_Subject);
                cmd.Parameters.AddWithValue("Req_Content", instance.Req_Content);
                cmd.Parameters.AddWithValue("Req_Qty", instance.Req_Qty);
                cmd.Parameters.AddWithValue("Req_Status", instance.Req_Status);
                cmd.Parameters.AddWithValue("Req_Class", instance.Req_Class);
                cmd.Parameters.AddWithValue("Req_Res", instance.Req_Res);
                cmd.Parameters.AddWithValue("Emg_Status", instance.Emg_Status);
                cmd.Parameters.AddWithValue("Wish_Date", string.IsNullOrWhiteSpace(instance.Wish_Date) ? (object)DBNull.Value : instance.Wish_Date);
                cmd.Parameters.AddWithValue("Est_Date", string.IsNullOrWhiteSpace(instance.Est_Date) ? (object)DBNull.Value : instance.Est_Date);
                cmd.Parameters.AddWithValue("Finish_Hours", instance.Finish_Hours == null ? (object)DBNull.Value : instance.Finish_Hours);
                cmd.Parameters.AddWithValue("Finish_Date", string.IsNullOrWhiteSpace(instance.Finish_Date) ? (object)DBNull.Value : instance.Finish_Date);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [製物工單] 建立製物工單-附件
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateMKHelp_Attachment(List<MKHelpAttachment> instance, out string ErrMsg)
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
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM MK_Help_Attachment");
                    sql.AppendLine("  WHERE (Parent_ID = @Parent_ID)");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO MK_Help_Attachment(");
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
        /// [製物工單] 建立製物工單-轉寄通知
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateMKHelp_Inform(List<MKHelpCC> instance, out string ErrMsg)
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
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM MK_Help_CC");
                    sql.AppendLine("  WHERE (Parent_ID = @Parent_ID)");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO MK_Help_CC(");
                    sql.AppendLine("  Parent_ID, Data_ID, CC_Who");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @Parent_ID, @NewID, @CC_Who_{0}".FormatThis(row));
                    sql.AppendLine(" );");

                    cmd.Parameters.AddWithValue("CC_Who_{0}".FormatThis(row), instance[row].CC_Who);
                }

                //Update & Delete Null
                sql.AppendLine(" UPDATE MK_Help_CC");
                sql.AppendLine(" SET CC_Email = Prof.Email");
                sql.AppendLine(" FROM PKSYS.dbo.User_Profile Prof");
                sql.AppendLine(" WHERE (CC_Who = Prof.[Guid]) AND (Parent_ID = @Parent_ID);");

                sql.AppendLine(" DELETE FROM MK_Help_CC");
                sql.AppendLine(" WHERE (CC_Email IS NULL) AND (Parent_ID = @Parent_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance[0].Parent_ID);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [製物工單] 建立製物工單-派案(可重新派案)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateMKHelp_Assign(List<MKHelpAssigned> instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @NewID AS INT ");
                //刪除已派案(重派)
                sql.AppendLine(" DELETE FROM MK_Help_Assigned");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID);");

                for (int row = 0; row < instance.Count; row++)
                {
                    sql.AppendLine(" IF (SELECT COUNT(*) FROM MK_Help_Assigned WHERE (Parent_ID = @Parent_ID) AND (Who = @Who_{0})) = 0 ".FormatThis(row));
                    sql.AppendLine(" BEGIN ");
                    sql.AppendLine(" SET @NewID = (");
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM MK_Help_Assigned");
                    sql.AppendLine("  WHERE (Parent_ID = @Parent_ID)");
                    sql.AppendLine(" );");
                    sql.AppendLine(" INSERT INTO MK_Help_Assigned(");
                    sql.AppendLine("  Parent_ID, Data_ID, Who");
                    sql.AppendLine("  , Create_Who, Create_Time");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @Parent_ID, @NewID, @Who_{0}".FormatThis(row));
                    sql.AppendLine("  , @Create_Who, GETDATE()");
                    sql.AppendLine(" );");
                    sql.AppendLine(" END ");

                    //工號
                    cmd.Parameters.AddWithValue("Who_{0}".FormatThis(row), instance[row].Who);
                }

                //Update Guid:將工號Update為Guid
                sql.AppendLine(" UPDATE MK_Help_Assigned");
                sql.AppendLine(" SET Who = Prof.[Guid]");
                sql.AppendLine(" FROM PKSYS.dbo.User_Profile Prof");
                sql.AppendLine(" WHERE (Who = Prof.Account_Name) AND (Parent_ID = @Parent_ID);");

                //Update Status:更新狀態為處理中(C)
                sql.AppendLine(" UPDATE MK_Help SET Req_Status = @reqStatus, Update_Who = @Create_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Parent_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance[0].Parent_ID);
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);
                cmd.Parameters.AddWithValue("reqStatus", GetOne_MKHelpClassID(instance[0].CompID, "處理狀態", "C"));

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [製物工單] 建立製物工單-處理進度
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateMKHelp_Reply(List<MKHelpReply> instance, out string ErrMsg)
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
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM MK_Help_Reply");
                    sql.AppendLine("  WHERE (Parent_ID = @Parent_ID)");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO MK_Help_Reply(");
                    sql.AppendLine("  Parent_ID, Data_ID, Reply_Content");
                    sql.AppendLine("  , Create_Who, Create_Time");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @Parent_ID, @NewID, @Reply_Content_{0}".FormatThis(row));
                    sql.AppendLine("  , @Create_Who, GETDATE()");
                    sql.AppendLine(" );");

                    cmd.Parameters.AddWithValue("Reply_Content_{0}".FormatThis(row), instance[row].Reply_Content);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance[0].Parent_ID);
                cmd.Parameters.AddWithValue("Create_Who", instance[0].Create_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        #endregion *** 製物工單 E ***


        #region *** 郵件寄送登記 S ***

        /// <summary>
        /// [郵件寄送登記] 新增收件名單
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Create_PostalAddress(ClassItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Postal_Address( ");
                sql.AppendLine("  ToWho, ToAddr, Create_Who");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @ToWho, @ToAddr, @Create_Who");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ToWho", instance.CustomID);
                cmd.Parameters.AddWithValue("ToAddr", instance.Label);
                cmd.Parameters.AddWithValue("Create_Who", fn_Param.CurrentUser);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }

        }

        /// <summary>
        /// [郵件寄送登記] 新增資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="isAdm">是否為管理者</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Create_PostalData(PostalItem instance, bool isAdm, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DECLARE @GetGuid AS uniqueidentifier");
                sql.AppendLine(" SET @GetGuid = (SELECT NEWID())");
                sql.AppendLine(" INSERT INTO Postal_Data (");
                sql.AppendLine("  Data_ID, PostDate, PostWho, PostType");
                sql.AppendLine("  , ToWho, ToAddr, PackageWeight, PostDesc");
                if (isAdm)
                {
                    //管理者欄位
                    sql.Append("  , PostNo, PostPrice ");
                }
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @GetGuid, @PostDate, @PostWho, @PostType");
                sql.AppendLine("  , @ToWho, @ToAddr, @PackageWeight, @PostDesc");
                if (isAdm)
                {
                    sql.Append("  , @PostNo, @PostPrice");
                }
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" )");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("PostDate", instance.PostDate);
                cmd.Parameters.AddWithValue("PostWho", instance.PostWho);
                cmd.Parameters.AddWithValue("PostType", instance.PostType);
                cmd.Parameters.AddWithValue("ToWho", instance.ToWho);
                cmd.Parameters.AddWithValue("ToAddr", instance.ToAddr);
                cmd.Parameters.AddWithValue("PackageWeight", instance.PackageWeight);
                cmd.Parameters.AddWithValue("PostDesc", instance.PostDesc);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);
                if (isAdm)
                {
                    cmd.Parameters.AddWithValue("PostNo", instance.PostNo);
                    cmd.Parameters.AddWithValue("PostPrice", instance.PostPrice);
                }

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }


        #endregion


        #endregion



        #region -----// Update //-----


        #region *** 製物工單 S ***
        /// <summary>
        /// [製物工單] 更新製物工單-基本資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_MKHelpBase(MKHelpItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE MK_Help");
                sql.AppendLine(" SET Req_Who = @Req_Who, Req_Dept = @Req_Dept");
                sql.AppendLine("  , Req_Subject = @Req_Subject, Req_Content = @Req_Content, Req_Qty = @Req_Qty");
                sql.AppendLine("  , Req_Class = @Req_Class, Req_Res = @Req_Res, Emg_Status = @Emg_Status");
                sql.AppendLine("  , Wish_Date = @Wish_Date, Est_Date = @Est_Date");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Req_Who", instance.Req_Who);
                cmd.Parameters.AddWithValue("Req_Dept", instance.Req_Dept);
                cmd.Parameters.AddWithValue("Req_Subject", instance.Req_Subject);
                cmd.Parameters.AddWithValue("Req_Content", instance.Req_Content);
                cmd.Parameters.AddWithValue("Req_Qty", instance.Req_Qty);
                cmd.Parameters.AddWithValue("Req_Class", instance.Req_Class);
                cmd.Parameters.AddWithValue("Req_Res", instance.Req_Res);
                cmd.Parameters.AddWithValue("Emg_Status", instance.Emg_Status);
                cmd.Parameters.AddWithValue("Wish_Date", string.IsNullOrWhiteSpace(instance.Wish_Date) ? (object)DBNull.Value : instance.Wish_Date);
                cmd.Parameters.AddWithValue("Est_Date", string.IsNullOrWhiteSpace(instance.Est_Date) ? (object)DBNull.Value : instance.Est_Date);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [製物工單] 退件
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_MKHelpReBack(MKHelpItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE MK_Help");
                sql.AppendLine(" SET Reback_Desc = @Reback_Desc");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Reback_Desc", instance.Reback_Desc);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [製物工單] 更新製物工單-結案(狀態更新是另一個function)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_MKHelpClose(MKHelpItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE MK_Help");
                sql.AppendLine(" SET Finish_Hours = @Finish_Hours, Finish_Date = @Finish_Date");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("Finish_Hours", instance.Finish_Hours);
                cmd.Parameters.AddWithValue("Finish_Date", instance.Finish_Date);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [製物工單] 更新製物工單-更新處理狀態
        /// </summary>
        /// <param name="compID"></param>
        /// <param name="dataID"></param>
        /// <param name="customID">A/B/C/D/E</param>
        /// <param name="who"></param>
        /// <returns></returns>
        public bool Update_MKHelpStatus(string compID, string dataID, string customID, string who)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE MK_Help SET Req_Status = @reqStatus, Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);
                cmd.Parameters.AddWithValue("Update_Who", who);
                cmd.Parameters.AddWithValue("reqStatus", GetOne_MKHelpClassID(compID, "處理狀態", customID));

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        #endregion *** 製物工單 E ***


        #region *** 郵件寄送登記 S ***
        /// <summary>
        /// [郵件寄送登記] 更新資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="isAdm">是否為管理者</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_PostalData(PostalItem instance, bool isAdm, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Postal_Data SET");
                sql.AppendLine("  PostDate = @PostDate, PostWho = @PostWho, PostType = @PostType");
                sql.AppendLine("  , ToWho = @ToWho, ToAddr = @ToAddr, PackageWeight = @PackageWeight, PostDesc = @PostDesc");
                if (isAdm)
                {
                    //管理者欄位
                    sql.Append("  , PostNo = @PostNo, PostPrice = @PostPrice");
                }
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("PostDate", instance.PostDate);
                cmd.Parameters.AddWithValue("PostWho", instance.PostWho);
                cmd.Parameters.AddWithValue("PostType", instance.PostType);
                cmd.Parameters.AddWithValue("ToWho", instance.ToWho);
                cmd.Parameters.AddWithValue("ToAddr", instance.ToAddr);
                cmd.Parameters.AddWithValue("PackageWeight", instance.PackageWeight);
                cmd.Parameters.AddWithValue("PostDesc", instance.PostDesc);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);
                if (isAdm)
                {
                    cmd.Parameters.AddWithValue("PostNo", instance.PostNo);
                    cmd.Parameters.AddWithValue("PostPrice", instance.PostPrice);
                }

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }


        /// <summary>
        /// [郵件寄送登記] 更新截止狀態
        /// </summary>
        /// <param name="targetDate">目標日期</param>
        /// <param name="isClose">是否關閉</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_Postal_DayCheck(string targetDate, string isClose, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Postal_DayCheck SET IsClose = @IsClose");
                sql.AppendLine(" WHERE (PostDate = @PostDate)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("PostDate", targetDate.ToDateString("yyyy/MM/dd"));
                cmd.Parameters.AddWithValue("IsClose", isClose.ToUpper());

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }


        #endregion


        #endregion



        #region -----// Delete //-----


        #region *** 製物工單 S ***
        /// <summary>
        /// [製物工單] 刪除製物工單-所有資料
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete_MKHelpData(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM MK_Help WHERE (Data_ID = @DataID);");
                sql.AppendLine(" DELETE FROM MK_Help_Attachment WHERE (Parent_ID = @DataID);");
                sql.AppendLine(" DELETE FROM MK_Help_CC WHERE (Parent_ID = @DataID);");
                sql.AppendLine(" DELETE FROM MK_Help_Assigned WHERE (Parent_ID = @DataID);");
                sql.AppendLine(" DELETE FROM MK_Help_Reply WHERE (Parent_ID = @DataID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        /// <summary>
        /// [製物工單] 刪除製物工單-檔案資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Delete_MKHelpFiles(MKHelpAttachment instance)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM MK_Help_Attachment");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID) AND (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        /// <summary>
        /// [製物工單] 刪除製物工單-進度說明
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Delete_MKHelpReply(MKHelpReply instance)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM MK_Help_Reply");
                sql.AppendLine(" WHERE (Parent_ID = @Parent_ID) AND (Data_ID = @Data_ID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Parent_ID", instance.Parent_ID);
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        #endregion *** 製物工單 E ***


        #region *** 郵件寄送登記 S ***

        /// <summary>
        /// [郵件寄送登記] 刪除收件名單
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete_PostalAddress(string dataID)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Postal_Address WHERE (SeqNo = @Data_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKExcel, out ErrMsg);
            }
        }

        /// <summary>
        /// [郵件寄送登記] 刪除資料
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
                sql.AppendLine(" DELETE FROM Postal_Data WHERE (Data_ID = @Data_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

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

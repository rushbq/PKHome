using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using twMGMTData.Models;
using PKLib_Method.Methods;


namespace twMGMTData.Controllers
{
    public class MGMTRepository
    {
        public string ErrMsg;

        #region -----// Read //-----


        #region *** 管理工作需求 S ***
        /// <summary>
        /// [管理工作需求] 取得不分頁的清單
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<MgHelpData> GetOne_MGhelp(Dictionary<string, string> search, out string ErrMsg)
        {
            int DataCnt = 0;
            return Get_MGhelpList(search, 0, 0, false, out DataCnt, out ErrMsg);
        }

        /// <summary>
        /// [管理工作需求] 取得已設定的清單
        /// </summary>
        /// <param name="search">search集合</param>
        /// <param name="startRow">StartRow(從0開始)</param>
        /// <param name="endRow">RecordsPerPage</param>
        /// <param name="doPaging">是否分頁</param>
        /// <param name="DataCnt">傳址參數(資料總筆數)</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<MgHelpData> Get_MGhelpList(Dictionary<string, string> search
            , int startRow, int endRow, bool doPaging
            , out int DataCnt, out string ErrMsg)
        {
            ErrMsg = "";
            string AllErrMsg = "";

            try
            {
                /* 開始/結束筆數計算 */
                int cntStartRow = startRow + 1;
                int cntEndRow = startRow + endRow;

                //----- 宣告 -----
                StringBuilder sql = new StringBuilder(); //SQL語法容器
                List<SqlParameter> sqlParamList = new List<SqlParameter>(); //SQL參數容器
                List<SqlParameter> sqlParamList_Cnt = new List<SqlParameter>(); //SQL參數容器
                List<MgHelpData> dataList = new List<MgHelpData>();
                DataCnt = 0;    //資料總數

                #region >> 資料筆數SQL查詢 <<
                using (SqlCommand cmdCnt = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    string mainSql = @"
                    SELECT COUNT(Base.SeqNo) AS TotalCnt
                    FROM twMG_Help Base
                      INNER JOIN twMG_Help_ParamClass ReqClass ON Base.Req_Class = ReqClass.Class_ID
                      INNER JOIN twMG_Help_ParamClass HelpStatus ON Base.Help_Status = HelpStatus.Class_ID
                      INNER JOIN [PKSYS].dbo.User_Profile Prof ON Base.Req_Who = Prof.Account_Name
                      INNER JOIN [PKSYS].dbo.User_Dept Dept ON Base.Req_Dept = Dept.DeptID
                    WHERE (1 = 1)";

                    //append
                    sql.Append(mainSql);


                    #region >> 條件組合 <<

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
                                    //指定資料編號
                                    sql.Append(" AND (Base.DataID = @DataID)");

                                    sqlParamList_Cnt.Add(new SqlParameter("@DataID", item.Value));

                                    break;

                                case "Keyword":
                                    //主旨, TraceID
                                    sql.Append(" AND (");
                                    sql.Append("   (UPPER(Base.TraceID) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("   OR (UPPER(Base.Help_Subject) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append(" )");

                                    sqlParamList_Cnt.Add(new SqlParameter("@Keyword", item.Value));

                                    break;

                                case "DateType":
                                    switch (item.Value)
                                    {
                                        case "A":
                                            filterDateType = "Base.Create_Time";
                                            break;

                                        case "B":
                                            filterDateType = "Base.Finish_Time";
                                            break;

                                        default:
                                            filterDateType = "Base.Create_Time";
                                            break;
                                    }

                                    break;

                                case "sDate":
                                    sql.Append(" AND ({0} >= @sDate)".FormatThis(filterDateType));
                                    sqlParamList.Add(new SqlParameter("@sDate", item.Value + " 00:00:00"));

                                    break;
                                case "eDate":
                                    sql.Append(" AND ({0} <= @eDate)".FormatThis(filterDateType));
                                    sqlParamList.Add(new SqlParameter("@eDate", item.Value + " 23:59:59"));

                                    break;

                                case "ReqClass":
                                    //--問題類別
                                    sql.Append(" AND (Base.Req_Class = @ReqClass)");

                                    sqlParamList_Cnt.Add(new SqlParameter("@ReqClass", item.Value));

                                    break;

                                case "HelpStatus":
                                    //--處理狀態
                                    sql.Append(" AND (Base.Help_Status = @HelpStatus)");

                                    sqlParamList_Cnt.Add(new SqlParameter("@HelpStatus", item.Value));

                                    break;

                                case "Dept":
                                    //需求部門
                                    sql.Append(" AND (Base.Req_Dept = @Dept)");

                                    sqlParamList_Cnt.Add(new SqlParameter("@Dept", item.Value));

                                    break;

                                case "ReqWho":
                                    //需求人
                                    sql.Append(" AND (Base.Req_Who = @ReqWho)");

                                    sqlParamList_Cnt.Add(new SqlParameter("@ReqWho", item.Value));

                                    break;

                                case "FinishWho":
                                    //結案人
                                    sql.Append(" AND (Base.Finish_Who IN (");
                                    sql.Append("   SELECT [Guid] FROM [PKSYS].dbo.User_Profile WHERE Account_Name = @FinishWho");
                                    sql.Append(" ))");

                                    sqlParamList_Cnt.Add(new SqlParameter("@FinishWho", item.Value));

                                    break;

                                case "unClose":
                                    //--未結案需求
                                    sql.Append(" AND (Base.Help_Status <> 140)");

                                    break;
                            }
                        }

                    }
                    #endregion

                    //----- SQL 執行 -----
                    cmdCnt.CommandText = sql.ToString();
                    cmdCnt.Parameters.Clear();

                    //----- SQL 參數陣列 -----
                    cmdCnt.Parameters.AddRange(sqlParamList_Cnt.ToArray());

                    //Execute
                    using (DataTable DTCnt = dbConn.LookupDT(cmdCnt, dbConn.DBS.PKEF, out ErrMsg))
                    {
                        //資料總筆數
                        if (DTCnt.Rows.Count > 0)
                        {
                            DataCnt = Convert.ToInt32(DTCnt.Rows[0]["TotalCnt"]);
                        }
                    }
                    AllErrMsg += ErrMsg;

                    //*** 在SqlParameterCollection同個循環內不可有重複的SqlParam,必須清除才能繼續使用. ***
                    cmdCnt.Parameters.Clear();
                }
                #endregion


                #region >> 主要資料SQL查詢 <<
                sql.Clear();

                //----- 資料取得 -----
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    string mainSql = @"
                    SELECT TbAll.*
                    FROM (
                    SELECT Base.SeqNo, Base.DataID, Base.TraceID, CONVERT(VARCHAR(10), Base.Create_Time, 111) AS CreateDay
                      , Base.Help_Subject, ISNULL(Base.Help_Content, '') Help_Content, ISNULL(Base.Help_Benefit, '') Help_Benefit
                      , Help_Way

                      /* 處理狀態, 需求類別 */
                      , Base.Help_Status, Base.Req_Class
                      , HelpStatus.Class_Name AS Help_StatusName, ReqClass.Class_Name AS Req_ClassName

                      /* 權限申請 */
                      , Base.IsAgree, Base.Agree_Time
                      , ISNULL((SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WHERE (Guid = Base.Agree_Who)), '') AS Agree_WhoName
  
                      /* 需求單位 */
                      , Base.Req_Who, Base.Req_Dept, Prof.Email AS Req_Email, ISNULL(Prof.Tel_Ext, '') AS Req_TelExt
                      , Prof.Display_Name AS Req_WhoName, ISNULL(Prof.NickName, '') AS Req_NickName, Dept.DeptName AS Req_DeptName

                      /* 驗收 */
                      , Base.IsRate, Base.RateScore, Base.RateContent
                      , ISNULL((SELECT Account_Name + ' (' + Display_Name + ')' FROM [PKSYS].dbo.User_Profile WHERE (Guid = Base.RateWho)), '') AS RateWhoName

                      /* 回覆資料 */
                      , ISNULL(Base.Reply_Content, '') Reply_Content, Base.onTopWho
                      , (CASE WHEN Base.onTopWho = @currUser THEN Base.onTop ELSE 'N' END) AS onTop
                      , Base.Finish_Hours, Base.Finish_Time, Base.Finish_Who, Base.Wish_Time
                      , ISNULL((SELECT Account_Name + ' (' + Display_Name + ')' FROM [PKSYS].dbo.User_Profile WHERE (Guid = Base.Finish_Who)), '') AS Finish_WhoName
                      /* 最新進度 */
                      , ISNULL(
                       (SELECT TOP 1 Cls.Class_Name + '_' + ISNULL(CONVERT(VARCHAR(20), DT.Proc_Time, 111), '') 
                        FROM twMG_Help_DT DT
	                     INNER JOIN twMG_Help_ParamClass Cls ON DT.Class_ID = Cls.Class_ID
	                    WHERE (DT.ParentID = Base.DataID) ORDER BY DT.DetailID DESC)
                      , '') AS ProcInfo
                      , CONVERT(VARCHAR(20), Base.Create_Time, 120) AS Create_Time
                      , ISNULL(CONVERT(VARCHAR(20), Base.Update_Time, 120), '') AS Update_Time
                      , ISNULL((SELECT Account_Name + ' (' + Display_Name + ')' FROM [PKSYS].dbo.User_Profile WHERE (Guid = Base.Create_Who)), '') AS Create_Name
                      , ISNULL((SELECT Account_Name + ' (' + Display_Name + ')' FROM [PKSYS].dbo.User_Profile WHERE (Guid = Base.Update_Who)), '') AS Update_Name
                      , ROW_NUMBER() OVER (ORDER BY (CASE WHEN Base.onTopWho = @currUser THEN Base.onTop ELSE 'N' END) DESC, HelpStatus.Sort ASC, Base.Create_Time DESC) AS RowIdx
                    FROM twMG_Help Base
                      INNER JOIN twMG_Help_ParamClass ReqClass ON Base.Req_Class = ReqClass.Class_ID
                      INNER JOIN twMG_Help_ParamClass HelpStatus ON Base.Help_Status = HelpStatus.Class_ID
                      INNER JOIN [PKSYS].dbo.User_Profile Prof ON Base.Req_Who = Prof.Account_Name
                      INNER JOIN [PKSYS].dbo.User_Dept Dept ON Base.Req_Dept = Dept.DeptID
                    WHERE (1 = 1)";

                    //append sql
                    sql.Append(mainSql);

                    #region >> 條件組合 <<

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
                                    //指定資料編號
                                    sql.Append(" AND (Base.DataID = @DataID)");

                                    sqlParamList.Add(new SqlParameter("@DataID", item.Value));

                                    break;

                                case "Keyword":
                                    //主旨, TraceID
                                    sql.Append(" AND (");
                                    sql.Append("   (UPPER(Base.TraceID) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append("   OR (UPPER(Base.Help_Subject) LIKE '%' + UPPER(@Keyword) + '%')");
                                    sql.Append(" )");

                                    sqlParamList.Add(new SqlParameter("@Keyword", item.Value));

                                    break;


                                case "DateType":
                                    switch (item.Value)
                                    {
                                        case "A":
                                            filterDateType = "Base.Create_Time";
                                            break;

                                        case "B":
                                            filterDateType = "Base.Finish_Time";
                                            break;

                                        default:
                                            filterDateType = "Base.Create_Time";
                                            break;
                                    }

                                    break;

                                case "sDate":
                                    sql.Append(" AND ({0} >= @sDate)".FormatThis(filterDateType));
                                    sqlParamList.Add(new SqlParameter("@sDate", item.Value + " 00:00:00"));

                                    break;
                                case "eDate":
                                    sql.Append(" AND ({0} <= @eDate)".FormatThis(filterDateType));
                                    sqlParamList.Add(new SqlParameter("@eDate", item.Value + " 23:59:59"));

                                    break;


                                case "ReqClass":
                                    //--問題類別
                                    sql.Append(" AND (Base.Req_Class = @ReqClass)");

                                    sqlParamList.Add(new SqlParameter("@ReqClass", item.Value));

                                    break;

                                case "HelpStatus":
                                    //--處理狀態
                                    sql.Append(" AND (Base.Help_Status = @HelpStatus)");

                                    sqlParamList.Add(new SqlParameter("@HelpStatus", item.Value));

                                    break;

                                case "Dept":
                                    //需求部門
                                    sql.Append(" AND (Base.Req_Dept = @Dept)");

                                    sqlParamList.Add(new SqlParameter("@Dept", item.Value));

                                    break;

                                case "ReqWho":
                                    //需求人
                                    sql.Append(" AND (Base.Req_Who = @ReqWho)");

                                    sqlParamList.Add(new SqlParameter("@ReqWho", item.Value));

                                    break;

                                case "FinishWho":
                                    //結案人
                                    sql.Append(" AND (Base.Finish_Who IN (");
                                    sql.Append("   SELECT [Guid] FROM [PKSYS].dbo.User_Profile WHERE Account_Name = @FinishWho");
                                    sql.Append(" ))");

                                    sqlParamList.Add(new SqlParameter("@FinishWho", item.Value));

                                    break;

                                case "unClose":
                                    //--未結案需求
                                    sql.Append(" AND (Base.Help_Status <> 140)");

                                    break;
                            }
                        }
                    }
                    #endregion

                    //Sql尾段
                    sql.AppendLine(") AS TbAll");

                    //是否分頁
                    if (doPaging)
                    {
                        sql.AppendLine(" WHERE (TbAll.RowIdx >= @startRow) AND (TbAll.RowIdx <= @endRow)");

                        sqlParamList.Add(new SqlParameter("@startRow", cntStartRow));
                        sqlParamList.Add(new SqlParameter("@endRow", cntEndRow));

                    }
                    sql.AppendLine(" ORDER BY TbAll.RowIdx");


                    //----- SQL 執行 -----
                    cmd.CommandText = sql.ToString();
                    cmd.Parameters.Clear();

                    //----- SQL 固定參數 -----
                    sqlParamList.Add(new SqlParameter("@currUser", fn_Param.CurrentUser)); //用來判斷onTop參數

                    //----- SQL 參數陣列 -----
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
                            var data = new MgHelpData
                            {
                                SeqNo = item.Field<Int64>("SeqNo"),
                                DataID = item.Field<Guid>("DataID"),
                                TraceID = item.Field<string>("TraceID"),
                                CreateDay = item.Field<string>("CreateDay"),
                                Req_Class = item.Field<Int32>("Req_Class"),
                                Req_ClassName = item.Field<string>("Req_ClassName"),
                                Req_Who = item.Field<string>("Req_Who"),
                                Req_WhoName = item.Field<string>("Req_WhoName"),
                                Req_NickName = item.Field<string>("Req_NickName"),
                                Req_Email = item.Field<string>("Req_Email"),
                                Req_TelExt = item.Field<string>("Req_TelExt"),
                                Req_Dept = item.Field<string>("Req_Dept"),
                                Req_DeptName = item.Field<string>("Req_DeptName"),
                                Help_Subject = item.Field<string>("Help_Subject"),
                                Help_Content = item.Field<string>("Help_Content"),
                                Help_Benefit = item.Field<string>("Help_Benefit"),
                                Help_Status = item.Field<Int32>("Help_Status"),
                                Help_StatusName = item.Field<string>("Help_StatusName"),
                                Help_Way = item.Field<Int16>("Help_Way"),
                                Reply_Content = item.Field<string>("Reply_Content"),
                                ProcInfo = item.Field<string>("ProcInfo"),
                                onTop = item.Field<string>("onTop"),
                                onTopWho = item.Field<string>("onTopWho"),
                                IsRate = item.Field<string>("IsRate"),
                                RateScore = item.Field<Int16>("RateScore"),
                                RateContent = item.Field<string>("RateContent"),
                                RateWhoName = item.Field<string>("RateWhoName"),
                                Finish_Hours = item.Field<double?>("Finish_Hours"),
                                Finish_Time = item.Field<DateTime?>("Finish_Time").ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),
                                Finish_WhoName = item.Field<string>("Finish_WhoName"),
                                Wish_Time = item.Field<DateTime?>("Wish_Time").ToString().ToDateString("yyyy/MM/dd"),

                                Create_Time = item.Field<string>("Create_Time").ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),
                                Update_Time = item.Field<string>("Update_Time").ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),
                                Create_Name = item.Field<string>("Create_Name"),
                                Update_Name = item.Field<string>("Update_Name"),
                                Agree_Time = item.Field<DateTime?>("Agree_Time").ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),
                                Agree_WhoName = item.Field<string>("Agree_WhoName"),
                                IsAgree = item.Field<string>("IsAgree")
                            };


                            //將項目加入至集合
                            dataList.Add(data);

                        }
                    }

                    //return err
                    if (!string.IsNullOrWhiteSpace(AllErrMsg)) ErrMsg = AllErrMsg;

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
        /// [管理工作需求] 取得類別
        /// </summary>
        /// <param name="clsType">A:處理狀態, B:需求類別, C:處理記錄類別</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetClass_MGhelp(string clsType, out string ErrMsg)
        {
            //----- 宣告 -----
            ErrMsg = "";

            try
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    //----- SQL 查詢語法 -----
                    string sql = @"
                    SELECT Class_ID AS ID, Class_Name AS Label
                    FROM [PKEF].dbo.twMG_Help_ParamClass
                    WHERE (Class_Type = @clsType) AND (Display = 'Y')
                    ORDER BY Sort, Class_ID";

                    //----- SQL 執行 -----
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("clsType", clsType);

                    //return
                    return dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg);
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message.ToString() + "_Error:_" + ErrMsg);
            }
        }


        /// <summary>
        /// [管理工作需求] 取得目前處理狀態
        /// </summary>
        /// <param name="id">DataID</param>
        /// <returns></returns>
        public string GetOne_MGHelpStatus(string id)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Cls.Class_ID");
                sql.AppendLine(" FROM twMG_Help Base");
                sql.AppendLine("  INNER JOIN twMG_Help_ParamClass Cls ON Base.Help_Status = Cls.Class_ID");
                sql.AppendLine(" WHERE (Base.DataID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", id);

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    if (DT == null)
                    {
                        return "";
                    }
                    else
                    {
                        return DT.Rows[0]["Class_ID"].ToString();
                    }
                }
            }
        }


        /// <summary>
        /// [管理工作需求] 取得檔案附件
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="detailID"></param>
        /// <param name="_type"></param>
        /// <returns></returns>
        public IQueryable<MgHelpAttachment> GetMgHelpFileList(string parentID, string detailID, string _type)
        {
            //----- 宣告 -----
            List<MgHelpAttachment> dataList = new List<MgHelpAttachment>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT DetailID, AttachID, AttachFile, AttachFile_Org, ISNULL(Create_Who, '') AS Create_Who");
                sql.AppendLine(" FROM twMG_Help_Attach WITH(NOLOCK)");
                sql.AppendLine(" WHERE (AttachType = @type) AND (ParentID = @ParentID)");

                //單身關聯ID
                if (!string.IsNullOrWhiteSpace(detailID))
                {
                    sql.AppendLine(" AND (DetailID = @DetailID)");

                    cmd.Parameters.AddWithValue("DetailID", detailID);
                }

                sql.AppendLine(" ORDER BY Create_Time");


                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("type", _type);
                cmd.Parameters.AddWithValue("ParentID", parentID);


                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new MgHelpAttachment
                        {
                            AttachID = item.Field<int>("AttachID"),
                            AttachFile = item.Field<string>("AttachFile"),
                            AttachFile_Org = item.Field<string>("AttachFile_Org"),
                            Create_Who = item.Field<string>("Create_Who")
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
        /// [管理工作需求] 取得轉寄人員
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public IQueryable<MgHelpCC> GetMgHelpCCList(string parentID)
        {
            //----- 宣告 -----
            List<MgHelpCC> dataList = new List<MgHelpCC>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT Prof.Display_Name AS Who, Base.CC_Email AS Email");
                sql.AppendLine(" FROM twMG_Help_CC Base");
                sql.AppendLine("  INNER JOIN PKSYS.dbo.User_Profile Prof ON Base.CC_Who = Prof.Guid");
                sql.AppendLine(" WHERE (ParentID = @ParentID)");
                sql.AppendLine(" ORDER BY Prof.DeptID, Prof.Account_Name");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", parentID);


                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new MgHelpCC
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
        /// [管理工作需求] 取得處理進度
        /// </summary>
        /// <param name="parentID"></param>
        /// <returns></returns>
        public IQueryable<MgHelpProc> GetMgHelpProcList(string parentID)
        {
            //----- 宣告 -----
            List<MgHelpProc> dataList = new List<MgHelpProc>();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    SELECT Base.DetailID, Base.Proc_Desc, Base.Proc_Time, Base.Confirm_Time, Base.Wish_Time, Base.Create_Time
                     , Cls.Class_Name
                     , ISNULL((SELECT Account_Name + ' (' + Display_Name + ')' FROM [PKSYS].dbo.User_Profile WHERE (Guid = Base.Create_Who)), '') AS Create_WhoName
                    FROM twMG_Help_DT Base
                     INNER JOIN twMG_Help_ParamClass Cls ON Base.Class_ID = Cls.Class_ID
                    WHERE (Base.ParentID = @ParentID)
                    ORDER BY Base.Proc_Time DESC";

                //----- SQL 執行 -----
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("ParentID", parentID);


                //----- 資料取得 -----
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new MgHelpProc
                        {
                            DetailID = item.Field<int>("DetailID"),
                            Class_Name = item.Field<string>("Class_Name"),
                            Proc_Desc = item.Field<string>("Proc_Desc"),
                            Proc_Time = item.Field<DateTime?>("Proc_Time").ToString().ToDateString("yy/MM/dd HH:mm:ss"),
                            Confirm_Time = item.Field<DateTime?>("Confirm_Time").ToString().ToDateString("yy/MM/dd HH:mm:ss"),
                            Wish_Time = item.Field<DateTime?>("Wish_Time").ToString().ToDateString("yy/MM/dd"),
                            Create_Time = item.Field<DateTime>("Create_Time").ToString().ToDateString("yy/MM/dd HH:mm:ss"),
                            Create_WhoName = item.Field<string>("Create_WhoName")
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
        /// [管理工作需求] 固定收信清單
        /// </summary>
        /// <param name="type">新需求=1 / 結案=2</param>
        /// <returns></returns>
        public IQueryable<MgHelpReceiver> GetMgHelpReceiver(string type)
        {
            //----- 宣告 -----
            List<MgHelpReceiver> dataList = new List<MgHelpReceiver>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" SELECT MailAddress AS Email");
                sql.AppendLine(" FROM twMG_Help_Receiver");
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
                        var data = new MgHelpReceiver
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
        /// 回傳狀態描述(不同css)
        /// </summary>
        /// <param name="_val">判斷值</param>
        /// <param name="_label">顯示文字</param>
        /// <returns></returns>
        public string GetMgHelp_StatusLabel(string _val, string _label)
        {
            string css = "";
            string icon = "";

            switch (_val)
            {
                case "110":
                    //待處理
                    css = "ui blue basic label";
                    icon = "<i class=\"clock icon\"></i>";
                    break;

                case "115":
                case "120":
                case "125":
                case "130":
                    //評估中;詢價中;處理中;報修中;
                    css = "ui green label";
                    icon = "<i class=\"tasks icon\"></i>";
                    break;

                case "135":
                    //驗收中
                    css = "ui orange label";
                    icon = "<i class=\"bug icon\"></i>";
                    break;

                case "140":
                    //已結案
                    css = "ui green basic label";
                    icon = "<i class=\"coffee icon\"></i>";
                    break;
            }

            return "<div class=\"{0}\">{2}{1}</div>".FormatThis(css, _label, icon);
        }

        #endregion *** 管理工作需求 E ***


        #endregion



        #region -----// Create //-----

        #region *** 管理工作需求 S ***
        /// <summary>
        /// [管理工作需求] 建立管理工作需求-基本資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateMgHelp_Base(MgHelpData instance, out string ErrMsg)
        {
            //----- 宣告 -----
            string sql = "";

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql = @"
                    DECLARE @DeptID AS VARCHAR(5)

                    /* 取得部門ID */
                    SET @DeptID = (SELECT DeptID FROM [PKSYS].dbo.User_Profile WHERE (Account_Name = @Req_Who))

                    INSERT INTO twMG_Help(
                     DataID, TraceID
                     , Req_Class, Req_Who, Req_Dept
                     , Help_Subject, Help_Content, Help_Benefit, Help_Status, Help_Way
                     , Create_Who, Create_Time
                    ) VALUES (
                     @NewGuid, @NewTraceID
                     , @Req_Class, @Req_Who, @DeptID
                     , @Help_Subject, @Help_Content, @Help_Benefit, @Help_Status, @Help_Way
                     , @WhoGuid, GETDATE()
                    )";

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("NewGuid", instance.DataID);
                cmd.Parameters.AddWithValue("NewTraceID", instance.TraceID);
                cmd.Parameters.AddWithValue("Req_Class", instance.Req_Class);
                cmd.Parameters.AddWithValue("Req_Who", instance.Req_Who);
                cmd.Parameters.AddWithValue("Help_Subject", instance.Help_Subject);
                cmd.Parameters.AddWithValue("Help_Content", instance.Help_Content);
                cmd.Parameters.AddWithValue("Help_Benefit", instance.Help_Benefit);
                cmd.Parameters.AddWithValue("Help_Status", 110); //待處理
                cmd.Parameters.AddWithValue("Help_Way", instance.Help_Way);
                cmd.Parameters.AddWithValue("WhoGuid", fn_Param.CurrentUser);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [管理工作需求] 建立管理工作需求-附件
        /// </summary>
        /// <param name="_parentID">單頭編號</param>
        /// <param name="_detailID">單身編號(可null)</param>
        /// <param name="_type">類型(A需求/B處理記錄)</param>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateMgHelp_Attachment(string _parentID, string _detailID, string _type
            , List<MgHelpAttachment> instance, out string ErrMsg)
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
                    sql.AppendLine("  SELECT ISNULL(MAX(AttachID) ,0) + 1 FROM twMG_Help_Attach");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO twMG_Help_Attach(");
                    sql.AppendLine("  AttachID, ParentID, DetailID, AttachFile, AttachFile_Org");
                    sql.AppendLine("  , AttachType, Create_Who, Create_Time");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @NewID, @ParentID, @DetailID, @AttachFile_{0}, @AttachFile_Org_{0}".FormatThis(row));
                    sql.AppendLine("  , @AttachType, @Create_Who, GETDATE()");
                    sql.AppendLine(" );");

                    cmd.Parameters.AddWithValue("AttachFile_{0}".FormatThis(row), instance[row].AttachFile);
                    cmd.Parameters.AddWithValue("AttachFile_Org_{0}".FormatThis(row), instance[row].AttachFile_Org);
                }

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", _parentID);
                cmd.Parameters.AddWithValue("DetailID", string.IsNullOrWhiteSpace(_detailID) ? (object)DBNull.Value : Convert.ToInt32(_detailID));
                cmd.Parameters.AddWithValue("AttachType", _type);
                cmd.Parameters.AddWithValue("Create_Who", fn_Param.CurrentUser);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [管理工作需求] 建立管理工作需求-轉寄通知
        /// </summary>
        /// <param name="_parentID">單頭編號</param>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateMgHelp_Inform(string _parentID, List<MgHelpCC> instance, out string ErrMsg)
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
                    sql.AppendLine("  SELECT ISNULL(MAX(Data_ID) ,0) + 1 FROM twMG_Help_CC");
                    sql.AppendLine("  WHERE (ParentID = @ParentID)");
                    sql.AppendLine(" )");
                    sql.AppendLine(" INSERT INTO twMG_Help_CC(");
                    sql.AppendLine("  ParentID, Data_ID, CC_Who");
                    sql.AppendLine(" ) VALUES (");
                    sql.AppendLine("  @ParentID, @NewID, @CC_Who_{0}".FormatThis(row));
                    sql.AppendLine(" );");

                    cmd.Parameters.AddWithValue("CC_Who_{0}".FormatThis(row), instance[row].CC_Who);
                }

                //Update & Delete Null
                sql.AppendLine(" UPDATE twMG_Help_CC");
                sql.AppendLine(" SET CC_Email = Prof.Email");
                sql.AppendLine(" FROM [PKSYS].dbo.User_Profile Prof");
                sql.AppendLine(" WHERE (CC_Who = Prof.Guid) AND (ParentID = @ParentID);"); //工號對應

                sql.AppendLine(" DELETE FROM twMG_Help_CC");
                sql.AppendLine(" WHERE (CC_Email IS NULL) AND (ParentID = @ParentID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", _parentID);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [管理工作需求] 建立處理記錄
        /// </summary>
        /// <param name="_parentID"></param>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public Int32 CreateMgHelp_Proc(string _parentID, MgHelpProc instance, out string ErrMsg)
        {
            //----- 宣告 -----
            string sql = "";

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql = @"
                    /* 單身筆數為0, 將狀態設為處理中 */
                    IF (SELECT COUNT(*) FROM twMG_Help_DT WHERE (ParentID = @ParentID)) = 0
                    BEGIN
                     UPDATE twMG_Help
                     SET Help_Status = 115
                     WHERE (DataID = @ParentID)
                    END

                    DECLARE @NewID AS INT
                     SET @NewID = (
                      SELECT ISNULL(MAX(DetailID) ,0) + 1 FROM twMG_Help_DT
                      WHERE (ParentID = @ParentID)
                     )
                     INSERT INTO twMG_Help_DT(
                      ParentID, DetailID, Class_ID
                      , Proc_Desc, Proc_Time, Confirm_Time, Wish_Time
                      , Create_Who, Create_Time
                     ) VALUES (
                      @ParentID, @NewID, @Class_ID
                      , @Proc_Desc, @Proc_Time, @Confirm_Time, @Wish_Time
                      , @WhoGuid, GETDATE()
                     )

                     SELECT @NewID AS ID";

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", _parentID);
                cmd.Parameters.AddWithValue("Class_ID", instance.Class_ID);
                cmd.Parameters.AddWithValue("Proc_Desc", instance.Proc_Desc);
                cmd.Parameters.AddWithValue("Proc_Time", instance.Proc_Time.ToDateString("yyyy/MM/dd HH:mm"));
                cmd.Parameters.AddWithValue("Confirm_Time", string.IsNullOrWhiteSpace(instance.Confirm_Time) ? (object)DBNull.Value : instance.Confirm_Time.ToDateString("yyyy/MM/dd HH:mm"));
                cmd.Parameters.AddWithValue("Wish_Time", string.IsNullOrWhiteSpace(instance.Wish_Time) ? (object)DBNull.Value : instance.Wish_Time.ToDateString("yyyy/MM/dd"));
                cmd.Parameters.AddWithValue("WhoGuid", fn_Param.CurrentUser);

                //Execute
                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg))
                {
                    return Convert.ToInt32(DT.Rows[0]["ID"]);
                }
            }

        }


        #endregion *** 管理工作需求 E ***


        #endregion



        #region -----// Update //-----

        #region *** 管理工作需求 S ***
        /// <summary>
        /// [管理工作需求] 更新需求資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_MGHelpBase(MgHelpData instance, out string ErrMsg)
        {
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    DECLARE @DeptID AS VARCHAR(5)
                    /* 取得部門ID */
                    SET @DeptID = (SELECT DeptID FROM [PKSYS].dbo.User_Profile WHERE (Account_Name = @Req_Who))
                    UPDATE twMG_Help
                    SET Req_Who = @Req_Who, Req_Dept = @DeptID
                    , Help_Way = @Help_Way, Help_Status = @Help_Status
                    , Help_Subject = @Help_Subject, Help_Content = @Help_Content, Help_Benefit = @Help_Benefit
                    , Update_Who = @WhoGuid, Update_Time = GETDATE()
                    WHERE (DataID = @DataID)";


                //----- SQL 執行 -----
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("DataID", instance.DataID);
                cmd.Parameters.AddWithValue("Req_Who", instance.Req_Who);
                cmd.Parameters.AddWithValue("Help_Way", instance.Help_Way);
                cmd.Parameters.AddWithValue("Help_Status", instance.Help_Status);
                cmd.Parameters.AddWithValue("Help_Subject", instance.Help_Subject);
                cmd.Parameters.AddWithValue("Help_Content", instance.Help_Content);
                cmd.Parameters.AddWithValue("Help_Benefit", instance.Help_Benefit);
                cmd.Parameters.AddWithValue("WhoGuid", fn_Param.CurrentUser);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [管理工作需求] 更新回覆資料
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_MGHelpReply(MgHelpData instance, out string ErrMsg)
        {
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    UPDATE twMG_Help
                    SET Req_Class = @Req_Class, Finish_Hours = @Finish_Hours, Reply_Content = @Reply_Content
                    , Wish_Time = @Wish_Time
                    , Update_Who = @WhoGuid, Update_Time = GETDATE()
                    WHERE (DataID = @DataID)";

                //----- SQL 執行 -----
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("DataID", instance.DataID);
                cmd.Parameters.AddWithValue("Req_Class", instance.Req_Class);
                cmd.Parameters.AddWithValue("Finish_Hours", instance.Finish_Hours.Equals(0) ? (object)DBNull.Value : instance.Finish_Hours);
                cmd.Parameters.AddWithValue("Wish_Time", string.IsNullOrWhiteSpace(instance.Wish_Time) ? (object)DBNull.Value : instance.Wish_Time);
                cmd.Parameters.AddWithValue("Reply_Content", instance.Reply_Content);
                cmd.Parameters.AddWithValue("WhoGuid", fn_Param.CurrentUser);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [管理工作需求] 更新驗收意見
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_MGHelpRate(MgHelpData instance, out string ErrMsg)
        {
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    UPDATE twMG_Help
                    SET IsRate = 'Y', RateScore = @RateScore, RateContent = @RateContent, RateWho = @WhoGuid
                    , Update_Who = @WhoGuid, Update_Time = GETDATE()
                    WHERE (DataID = @DataID)";

                //----- SQL 執行 -----
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("DataID", instance.DataID);
                cmd.Parameters.AddWithValue("RateScore", instance.RateScore);
                cmd.Parameters.AddWithValue("RateContent", instance.RateContent);
                cmd.Parameters.AddWithValue("WhoGuid", fn_Param.CurrentUser);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        /// <summary>
        /// [管理工作需求] 更新資料-置頂
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_MGHelpSetTop(string _id, out string ErrMsg)
        {
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"UPDATE twMG_Help SET onTop = 'Y', onTopWho = @Who WHERE (DataID = @DataID)";

                //----- SQL 執行 -----
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("DataID", _id);
                cmd.Parameters.AddWithValue("Who", fn_Param.CurrentUser);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }

        /// <summary>
        /// [管理工作需求] 更新資料-結案(140)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Update_MGHelpClose(MgHelpData instance, out string ErrMsg)
        {
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    UPDATE twMG_Help
                    SET Req_Class = @Req_Class, Reply_Content = @Reply_Content, Wish_Time = @Wish_Time
                     , Finish_Time = @Finish_Time, Finish_Hours = @Finish_Hours, Finish_Who = @Who
                     , Help_Status = @Help_Status, onTop = 'N'
                    WHERE (DataID = @DataID)";


                //----- SQL 執行 -----
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("DataID", instance.DataID);
                cmd.Parameters.AddWithValue("Req_Class", instance.Req_Class);
                cmd.Parameters.AddWithValue("Reply_Content", instance.Reply_Content);
                cmd.Parameters.AddWithValue("Wish_Time", string.IsNullOrWhiteSpace(instance.Wish_Time) ? (object)DBNull.Value : instance.Wish_Time);
                cmd.Parameters.AddWithValue("Finish_Time", instance.Finish_Time);
                cmd.Parameters.AddWithValue("Finish_Hours", instance.Finish_Hours);
                cmd.Parameters.AddWithValue("Help_Status", 140);
                cmd.Parameters.AddWithValue("Who", fn_Param.CurrentUser);

                //Execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }


        ///// <summary>
        ///// [管理工作需求] 更新資料-處理狀態
        ///// </summary>
        ///// <param name="dataID"></param>
        ///// <param name="customID">A:待處理,B:處理中,C:測試中,D:已結案</param>
        ///// <returns></returns>
        //public bool Update_MGHelpStatus(string dataID, string customID)
        //{
        //    //----- 宣告 -----
        //    int _clsID = 110;
        //    switch (customID.ToUpper())
        //    {
        //        case "B":
        //            _clsID = 115;
        //            break;

        //        case "C":
        //            _clsID = 120;
        //            break;

        //        case "D":
        //            _clsID = 140;
        //            break;
        //    }

        //    //----- 資料查詢 -----
        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        //----- SQL 查詢語法 -----
        //        string sql = "UPDATE twMG_Help SET Help_Status = @Help_Status, Update_Who = @Who, Update_Time = GETDATE() WHERE (DataID = @DataID)";


        //        //----- SQL 執行 -----
        //        cmd.CommandText = sql;
        //        cmd.Parameters.AddWithValue("DataID", dataID);
        //        cmd.Parameters.AddWithValue("Help_Status", _clsID);
        //        cmd.Parameters.AddWithValue("Who", fn_Param.CurrentUser);

        //        //Execute
        //        return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
        //    }
        //}

        #endregion *** 管理工作需求 E ***

        #endregion



        #region -----// Delete //-----

        #region *** 管理工作需求 S ***
        /// <summary>
        /// [管理工作需求] 刪除-檔案資料
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool Delete_MGHelpFiles(string _id)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM twMG_Help_Attach");
                sql.AppendLine(" WHERE (AttachID = @AttachID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("AttachID", _id);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        /// <summary>
        /// [管理工作需求] - 刪除處理記錄
        /// </summary>
        /// <param name="_parentID"></param>
        /// <param name="_id"></param>
        /// <returns></returns>
        public bool Delete_MGHelpProcItem(string _parentID, string _id)
        {
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    DELETE FROM twMG_Help_Attach WHERE (ParentID = @ParentID) AND (DetailID = @DetailID);
                    DELETE FROM twMG_Help_DT WHERE (ParentID = @ParentID) AND (DetailID = @DetailID);
                    ";

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", _parentID);
                cmd.Parameters.AddWithValue("DetailID", _id);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }

        /// <summary>
        /// [管理工作需求] - 刪除全部
        /// </summary>
        /// <param name="_id"></param>
        /// <returns></returns>
        public bool Delete_MGHelp(string _id)
        {
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    DELETE FROM twMG_Help_CC WHERE (ParentID = @ParentID);
                    DELETE FROM twMG_Help_Attach WHERE (ParentID = @ParentID);
                    DELETE FROM twMG_Help_DT WHERE (ParentID = @ParentID);
                    DELETE FROM twMG_Help WHERE (DataID = @ParentID)
                    ";

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("ParentID", _id);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        #endregion *** 管理工作需求 E ***

        #endregion

    }
}

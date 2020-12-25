using System;
using System.Data;
using System.Data.SqlClient;

public class dbConn
{
    public enum DBS
    {
        Local = 1,
        PKEF = 2,
        Product = 3,
        PKSYSinANA = 4,
        EFGP = 5,
        Report = 6,
        PKExcel = 7,
        ClickLog = 8
    }

    /// <summary>
    /// 連線字串
    /// </summary>
    /// <param name="dbs">資料庫別</param>
    /// <returns></returns>
    private static string ConnString(DBS dbs)
    {
        switch ((int)dbs)
        {
            case 2:
                return System.Web.Configuration.WebConfigurationManager.AppSettings["dbCon_PKEF"];

            case 3:
                return System.Web.Configuration.WebConfigurationManager.AppSettings["dbCon_Product"];

            case 4:
                return System.Web.Configuration.WebConfigurationManager.AppSettings["dbCon_PKSYS_ANA"];

            case 5:
                return System.Web.Configuration.WebConfigurationManager.AppSettings["dbCon_EFGP"];

            case 6:
                return System.Web.Configuration.WebConfigurationManager.AppSettings["dbCon_Report"];

            case 7:
                return System.Web.Configuration.WebConfigurationManager.AppSettings["dbCon_PKExcel"];

            default:
                return System.Web.Configuration.WebConfigurationManager.AppSettings["dbCon_PKSYS"];
        }
    }

    /// <summary>
    /// (Override) 取得資料列表, 分頁 (DB=Local)
    /// </summary>
    /// <returns>DataTable</returns>
    /// <remarks>
    /// 若未填寫資料連線來源，預設為 Local 資料庫
    /// </remarks>
    public static DataTable LookupDTwithPage(SqlCommand cmd, SqlCommand cmdTotalCnt, out int totalCnt, out string errMsg)
    {
        return LookupDTwithPage(cmd, cmdTotalCnt, DBS.Local, out totalCnt, out errMsg);
    }

    /// <summary>
    /// 取得資料列表, 分頁
    /// </summary>
    /// <param name="cmd">SqlCommand</param>
    /// <param name="cmdTotalCnt">SqlCommand</param>
    /// <param name="dbs">資料連線來源</param>
    /// <param name="totalCnt">總筆數</param>
    /// <param name="errMsg">錯誤訊息</param>
    /// <returns>DataTable</returns>
    public static DataTable LookupDTwithPage(SqlCommand cmd, SqlCommand cmdTotalCnt, DBS dbs
        , out int totalCnt, out string errMsg)
    {
        SqlConnection connSql = new SqlConnection(ConnString(dbs));
        SqlDataAdapter dataAdapterSql = new SqlDataAdapter();
        totalCnt = 0;
        try
        {
            connSql.Open();
            cmd.Connection = connSql;
            cmdTotalCnt.Connection = connSql;

            //取得資料總數
            SqlDataReader reader = default(SqlDataReader);
            reader = cmdTotalCnt.ExecuteReader();
            reader.Read();
            totalCnt = Convert.ToInt32(reader[0]);
            reader.Close();

            //建立DataAdapter
            dataAdapterSql.SelectCommand = cmd;
            //取得DataTable
            DataTable DTSql = new DataTable();
            dataAdapterSql.Fill(DTSql);
            connSql.Close();
            errMsg = "";

            return DTSql;

        }
        catch (SqlException sqlex)
        {
            errMsg = sqlex.Message.ToString();
            return null;
        }
        catch (Exception ex)
        {
            errMsg = ex.Message.ToString();
            return null;

        }
        finally
        {
            dataAdapterSql.Dispose();
            cmd.Dispose();
            cmdTotalCnt.Dispose();
            connSql.Close();
            connSql.Dispose();
        }
    }


    /// <summary>
    /// (Override) 取得資料列表, 無分頁 (DB=Local)
    /// </summary>
    /// <returns>DataTable</returns>
    /// <remarks>
    /// 若未填寫資料連線來源，預設為 Local 資料庫
    /// </remarks>
    public static DataTable LookupDT(SqlCommand cmd, out string errMsg)
    {
        return LookupDT(cmd, DBS.Local, out errMsg);
    }
    
    /// <summary>
    /// 取得資料列表, 無分頁
    /// </summary>
    /// <param name="cmd">SqlCommand</param>
    /// <param name="dbs">資料連線來源</param>
    /// <param name="errMsg">錯誤訊息</param>
    /// <returns>DataTable</returns>
    public static DataTable LookupDT(SqlCommand cmd, DBS dbs, out string errMsg)
    {
        SqlConnection connSql = new SqlConnection(ConnString(dbs));
        try
        {
            connSql.Open();
            cmd.Connection = connSql;

            //建立DataAdapter
            SqlDataAdapter dataAdapterSql = new SqlDataAdapter();
            dataAdapterSql.SelectCommand = cmd;

            //取得DataTable
            DataTable DTSql = new DataTable("queryTable");
            dataAdapterSql.Fill(DTSql);
            connSql.Close();
            errMsg = "";

            return DTSql;

        }
        catch (Exception ex)
        {
            errMsg = ex.Message.ToString();
            return null;

        }
        finally
        {
            cmd.Dispose();
            connSql.Close();
            connSql.Dispose();
        }
    }


    /// <summary>
    /// (Override) 執行SQL (DB=Local)
    /// </summary>
    /// <returns>bool</returns>
    /// <remarks>
    /// 若未填寫資料連線來源，預設為 Local 資料庫
    /// </remarks>
    public static bool ExecuteSql(SqlCommand cmd, out string errMsg)
    {
        return ExecuteSql(cmd, DBS.Local, out errMsg);
    }

    /// <summary>
    /// 執行SQL
    /// </summary>
    /// <param name="cmd">SqlCommand</param>
    /// <param name="dbs">資料連線來源</param>
    /// <param name="errMsg">錯誤訊息</param>
    /// <returns>bool</returns>
    public static bool ExecuteSql(SqlCommand cmd, DBS dbs, out string errMsg)
    {
        SqlConnection connSql = new SqlConnection(ConnString(dbs));
        SqlTransaction transActSql = default(SqlTransaction);
        try
        {
            connSql.Open();
            transActSql = connSql.BeginTransaction();
            cmd.Connection = connSql;
            cmd.Transaction = transActSql;
            cmd.ExecuteNonQuery();
            transActSql.Commit();
            errMsg = "";

            return true;
        }
        catch (System.Exception ex)
        {
            errMsg = ex.Message.ToString();
            transActSql.Rollback();
            return false;
        }
        finally
        {
            connSql.Close();
            connSql.Dispose();
            transActSql.Dispose();
            cmd.Dispose();
        }
    }

}
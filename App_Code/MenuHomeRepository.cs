using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using MenuHomeData.Models;
using PKLib_Method.Methods;


namespace MenuHomeData.Controllers
{

    public class MenuHomeRepository
    {
        public string ErrMsg;

        #region -----// Read //-----

        /// <summary>
        /// 取得部門Email清單
        /// </summary>
        /// <returns></returns>
        public IQueryable<MailtoList> GetDeptMailList(Dictionary<string, string> search)
        {
            //----- 宣告 -----
            List<MailtoList> dataList = new List<MailtoList>();
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----

                sql.AppendLine(" SELECT Email AS MailAddress, DeptName AS MailName ");
                sql.AppendLine(" FROM User_Dept WITH(NOLOCK) ");
                sql.AppendLine(" WHERE (Display = 'Y') AND (Email IS NOT NULL)");

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
                            case "DeptRange":
                                sql.Append(" AND (DeptID IN ({0}))".FormatThis(item.Value));

                                break;


                        }
                    }
                }
                #endregion


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
                        var data = new MailtoList
                        {
                            mailAddr = item.Field<string>("MailAddress"),
                            mailName = item.Field<string>("MailName")

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

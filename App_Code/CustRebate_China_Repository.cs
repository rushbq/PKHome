using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Menu3000Data.Models;
using PKLib_Method.Methods;

/*
 返利 - 中國區, DB涵蓋SH/SZ
*/
namespace CustRebate_China_Data.Controllers
{
    public class CustRebate_China_Repository
    {
        #region -----// Read //-----

        /// <summary>
        /// 客戶返利統計(CustRebate) - SZ+SH
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="inputYear">年</param>
        /// <param name="inputMonth">月</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        /// <remarks>
        /// 必填:inputYear / inputMonth(當月)
        /// </remarks>
        public IQueryable<CustRebateItem> GetCustRebateList(string inputYear, string inputMonth
            , Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<CustRebateItem> dataList = new List<CustRebateItem>();


            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                ;WITH TblSO AS (
	                /* 銷貨單資料 (全年), 12/25以前 */
	                SELECT Base.TG001, Base.TG002, Base.TG003, Base.TG004
	                , Base.TH004, Base.TH013, Base.TH037, Base.TH038
	                FROM [ReportCenter].dbo.Ref_SalesOrder Base
	                WHERE (LEFT(Base.TG003, 4) = @paramYear) AND (Base.DBS IN ('SH','SZ')) AND (Base.CREATE_DATE <= @paramYear+'1225')
                )
                , TblSOReback AS (
	                /* 銷退單資料 (全年), 12/25以前 */
	                SELECT Reb.TI001, Reb.TI002, Reb.TI003, Reb.TI004, Reb.TJ033, Reb.TJ034
	                FROM [ReportCenter].[dbo].[Ref_SalesReback] Reb
	                WHERE (LEFT(Reb.TI003, 4) = @paramYear) AND (Reb.DBS IN ('SH','SZ')) AND (Reb.CREATE_DATE <= @paramYear+'1225')
                )
                , TblPaperCost AS (
	                /* 成本資料 (全年), 來源:報表中心底層 */
	                SELECT
	                Ref.ParentCustID COLLATE Chinese_Taiwan_Stroke_BIN AS CustID
	                , Base.E_IDE_SaleYear AS SaleYear
	                , Base.C_RMB_SHSZPaperCostAMO AS PaperCost
	                FROM [ReportCenter].dbo.V_SALE2 Base WITH (NOLOCK)
	                INNER JOIN [PKEF].dbo.Rebate_RefCust Ref ON Base.E_IDE_CustomerID = Ref.CustID COLLATE Chinese_Taiwan_Stroke_BIN
	                WHERE (Base.E_IDE_SaleYear = @paramYear) AND (Base.E_SYS_PKflow IN ('SZ','SH'))
	                UNION ALL
	                SELECT
	                Base.E_IDE_CustomerID AS CustID
	                , Base.E_IDE_SaleYear AS SaleYear
	                , Base.C_RMB_SHSZPaperCostAMO AS PaperCost
	                FROM [ReportCenter].dbo.V_SALE2 Base WITH (NOLOCK)
	                WHERE (Base.E_IDE_SaleYear = @paramYear) AND (Base.E_SYS_PKflow IN ('SZ','SH'))
	                 AND (Base.E_IDE_CustomerID NOT IN (
		                SELECT Ref.CustID COLLATE Chinese_Taiwan_Stroke_BIN
		                FROM [PKEF].dbo.Rebate_RefCust Ref
	                 ))
                )
                , TblBase AS (
	                SELECT Base.Data_ID, RTRIM(Cust.MA001) COLLATE Chinese_Taiwan_Stroke_BIN AS CustID, RTRIM(Cust.MA002) AS CustName
	                , ISNULL(DeptSH.ME002, DeptSZ.ME002) AS DeptName
	                 , Base.DataYear, Base.RespMoney, Base.RespPercent
	                 , ISNULL(Base.FightMoney, 0) FightMoney, ISNULL(Base.FightPercent, 0) FightPercent
	                 , Base.Formula, Base.Remark
	                 , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE([Guid] = Base.Create_Who)) AS Create_Name
	                 , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE([Guid] = Base.Update_Who)) AS Update_Name
	                FROM [PKEF].dbo.Rebate_Data Base
		                INNER JOIN [PKSYS].dbo.Customer Cust ON Base.CustID = Cust.MA001 AND Cust.DBS = Cust.DBC
		                LEFT JOIN [SHPK2].dbo.CMSME DeptSH WITH(NOLOCK) ON Cust.MA015 COLLATE Chinese_Taiwan_Stroke_BIN = DeptSH.ME001
		                LEFT JOIN [ProUnion].dbo.CMSME DeptSZ WITH(NOLOCK) ON Cust.MA015 COLLATE Chinese_Taiwan_Stroke_BIN = DeptSZ.ME001
	                WHERE (Base.DataYear = @paramYear) AND (Base.CompID = 'CHN')
                )
                , TblCnt_Range AS (
	                /* 目前業績 (整年) --[A] */
	                SELECT ISNULL(SUM(Base.TH037) + SUM(Base.TH038) ,0) AS RangeSell, ISNULL(SUM(Base.TH037), 0) AS RangeSell_NoTax, Base.TG004 AS CustID
	                FROM TblSO Base
	                 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID
	                WHERE (DT.DataYear = @paramYear) AND (DT.CompID = 'CHN')
	                GROUP BY Base.TG004
                )
                , TblCost_Range AS (
	                /* 成本 (整年) --[cA] */
	                SELECT myCost.CustID AS CustID, SUM(myCost.PaperCost) AS Cost
	                FROM [PKEF].dbo.Rebate_Data DT
	                 INNER JOIN TblPaperCost myCost ON DT.DataYear = myCost.SaleYear COLLATE Chinese_Taiwan_Stroke_BIN
	                  AND DT.CustID = myCost.CustID COLLATE Chinese_Taiwan_Stroke_BIN
	                WHERE (DT.DataYear = @paramYear) AND (DT.CompID = 'CHN')
	                GROUP BY myCost.CustID
                )
                , TblCnt_Except AS (
	                /* 2341單別 (整年) --[F] */
	                SELECT ISNULL(SUM(Base.TH037) + SUM(Base.TH038) ,0) AS RangeSellExcept, ISNULL(SUM(Base.TH037), 0) AS RangeSellExcept_NoTax, Base.TG004 AS CustID
	                FROM TblSO Base
	                 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID
	                WHERE (Base.TG001 IN ('2341')) AND (DT.DataYear = @paramYear) AND (DT.CompID = 'CHN')
	                GROUP BY Base.TG004
                )
                , TblCnt_RangeBack AS (
	                /* 銷退 (整年) --[Ab] */
	                SELECT ISNULL(SUM(Reb.TJ033) + SUM(Reb.TJ034), 0) AS RangeSellBack, ISNULL(SUM(Reb.TJ033), 0) AS RangeSellBack_NoTax, Reb.TI004 AS CustID
	                FROM TblSOReback Reb
	                WHERE (NOT (Reb.TI001 IN ('2405', '2406')))
	                GROUP BY Reb.TI004
                )
                , TblCnt_Rebate AS (
	                /* 已回饋金額 (W003)(整年) --[B] */
	                SELECT -ISNULL(SUM(Base.TH013), 0) AS RebateMoney, -ISNULL(SUM(Base.TH037), 0) AS RebateMoney_NoTax, Base.TG004 AS CustID
	                FROM TblSO Base
	                 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID
	                WHERE (DT.DataYear = @paramYear) AND (DT.CompID = 'CHN')
	                 AND (Base.TH004 = 'W003')
	                GROUP BY Base.TG004
                )
                , TblCnt_NowRebate AS (
	                /* 已回饋金額 (W003)(當月) --[cB] */
	                SELECT -SUM(Base.TH013) AS NowRebateMoney, Base.TG004 AS CustID
	                FROM TblSO Base
	                 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID
	                WHERE (DT.DataYear = @paramYear) AND (DT.CompID = 'CHN') AND (LEFT(Base.TG003, 6) = @paramYear+@paramMonth)
	                 AND (Base.TH004 = 'W003')
	                GROUP BY Base.TG004
                )
                , TblCnt_Now AS (
	                /* 銷售金額 (當月) --[C] */
	                SELECT ISNULL(SUM(Base.TH037) + SUM(Base.TH038) ,0) AS NowSell, ISNULL(SUM(Base.TH037), 0) AS NowSell_NoTax, Base.TG004 AS CustID
	                FROM TblSO Base
	                 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID
	                WHERE (DT.DataYear = @paramYear) AND (DT.CompID = 'CHN') AND (LEFT(Base.TG003, 6) = @paramYear+@paramMonth)
	                GROUP BY Base.TG004
                )
                , TblCnt_NowExcept AS (
	                /* 2341單別 (當月) --[G] */
	                SELECT ISNULL(SUM(Base.TH037) + SUM(Base.TH038) ,0) AS NowSellExcept, ISNULL(SUM(Base.TH037), 0) AS NowSellExcept_NoTax, Base.TG004 AS CustID
	                FROM TblSO Base
	                 INNER JOIN [PKEF].dbo.Rebate_Data DT ON Base.TG004 COLLATE Chinese_Taiwan_Stroke_BIN = DT.CustID
	                WHERE (Base.TG001 IN ('2341'))
	                 AND (DT.DataYear = @paramYear) AND (DT.CompID = 'CHN') AND (LEFT(Base.TG003, 6) = @paramYear+@paramMonth)
	                GROUP BY Base.TG004
                )
                , TblCnt_NowBack AS (
	                /* 銷退 (當月) --[Cb] */
	                SELECT ISNULL(SUM(Reb.TJ033) + SUM(Reb.TJ034), 0) AS NowSellBack, ISNULL(SUM(Reb.TJ033), 0) AS NowSellBack_NoTax, Reb.TI004 AS CustID
	                FROM TblSOReback Reb
	                WHERE (NOT (Reb.TI001 IN ('2405', '2406')))
	                 AND (LEFT(Reb.TI003, 6) = @paramYear+@paramMonth)
	                GROUP BY Reb.TI004
                )
                SELECT TblBase.*
                 , (ISNULL(TblCnt_Range.RangeSell, 0) - ISNULL(TblCnt_RangeBack.RangeSellBack, 0)) AS CntBase_A --[A] 目前業績(含稅)
                 , (ISNULL(TblCnt_Range.RangeSell_NoTax, 0) - ISNULL(TblCnt_RangeBack.RangeSellBack_NoTax, 0)) AS CntBase_utA --[utA] 目前業績(未稅)
                 , ISNULL(TblCost_Range.Cost, 0) AS CntBase_costA  --[costA] 成本
                 , ISNULL(TblCnt_Rebate.RebateMoney, 0) AS CntBase_B --[B] 已回饋金額(整年)
                 , ISNULL(TblCnt_Rebate.RebateMoney_NoTax, 0) AS CntBase_utB --[B] 已回饋金額(整年)(未稅)
                 , ISNULL(TblCnt_NowRebate.NowRebateMoney, 0) AS CntBase_cB --[cB] 已回饋金額(當月)
                 , (ISNULL(TblCnt_Now.NowSell, 0) - ISNULL(TblCnt_NowBack.NowSellBack, 0)) AS CntBase_C --[C] 當月業績(含稅)
                 , (ISNULL(TblCnt_Now.NowSell_NoTax, 0) - ISNULL(TblCnt_NowBack.NowSellBack_NoTax, 0)) AS CntBase_utC --[utC] 當月業績(未稅)
                 , ISNULL(TblCnt_Except.RangeSellExcept, 0) AS CntBase_F --[F] 2341整年(含稅)
                 , ISNULL(TblCnt_NowExcept.NowSellExcept, 0) AS CntBase_G --[G] 2341當月(含稅)
                 , ISNULL(TblCnt_Except.RangeSellExcept_NoTax, 0) AS CntBase_utF --[F] 2341整年(未稅)
                 , ISNULL(TblCnt_NowExcept.NowSellExcept_NoTax, 0) AS CntBase_utG --[G] 2341當月(未稅)
                FROM TblBase
                 INNER JOIN TblCnt_Range ON TblBase.CustID = TblCnt_Range.CustID
                 LEFT JOIN TblCost_Range ON TblBase.CustID COLLATE Chinese_Taiwan_Stroke_BIN = TblCost_Range.CustID
                 LEFT JOIN TblCnt_Rebate ON TblBase.CustID = TblCnt_Rebate.CustID
                 LEFT JOIN TblCnt_NowRebate ON TblBase.CustID = TblCnt_NowRebate.CustID
                 LEFT JOIN TblCnt_Now ON TblBase.CustID = TblCnt_Now.CustID
                 LEFT JOIN TblCnt_RangeBack ON TblBase.CustID = TblCnt_RangeBack.CustID
                 LEFT JOIN TblCnt_NowBack ON TblBase.CustID = TblCnt_NowBack.CustID
                 LEFT JOIN TblCnt_Except ON TblBase.CustID = TblCnt_Except.CustID
                 LEFT JOIN TblCnt_NowExcept ON TblBase.CustID = TblCnt_NowExcept.CustID
                WHERE (1=1)
                ";

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
                            case "Cust":
                                //客戶代號
                                sql += " AND (UPPER(TblBase.CustID) = UPPER(@CustID))";

                                cmd.Parameters.AddWithValue("CustID", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                sql += " ORDER BY TblBase.DeptName, TblBase.CustID";

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.CommandTimeout = 60;   //單位:秒
                cmd.Parameters.AddWithValue("paramYear", inputYear);
                cmd.Parameters.AddWithValue("paramMonth", ("0" + inputMonth).Right(2)); //月份補0

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        #region ** 公式計算 **
                        //取得計算參數
                        string _formula = item.Field<string>("Formula");
                        double _cntBase_A = Convert.ToDouble(item.Field<decimal>("CntBase_A"));    //--[A] 目前業績(含稅)
                        double _cntBase_utA = Convert.ToDouble(item.Field<decimal>("CntBase_utA"));    //--[utA] 目前業績(未稅)
                        double _cntBase_costA = Convert.ToDouble(item.Field<decimal>("CntBase_costA"));    //--[costA] 成本
                        double _cntBase_B = Convert.ToDouble(item.Field<decimal>("CntBase_B"));    //--[B] 已回饋金額(整年)
                        double _cntBase_utB = Convert.ToDouble(item.Field<decimal>("CntBase_utB"));    //--[utB] 已回饋金額(整年)(未稅)
                        double _cntBase_cB = Convert.ToDouble(item.Field<decimal>("CntBase_cB"));    //--[cB] 已回饋金額(當月)
                        double _cntBase_C = Convert.ToDouble(item.Field<decimal>("CntBase_C"));    //--[C] 當月業績(含稅)
                        double _cntBase_utC = Convert.ToDouble(item.Field<decimal>("CntBase_utC"));    //--[utC] 當月業績(未稅)
                        double _cntBase_D = 0;   //--[D] 與挑戰目標差額(A-g)
                        double _cntBase_E = 0;   //--[E] 與責任目標差額(A-e)
                        double _cntBase_F = Convert.ToDouble(item.Field<decimal>("CntBase_F"));    //--[F] 2341整年(含稅)
                        double _cntBase_G = Convert.ToDouble(item.Field<decimal>("CntBase_G"));    //--[G] 2341當月(含稅)
                        double _cntBase_utF = Convert.ToDouble(item.Field<decimal>("CntBase_utF"));    //--[utF] 2341整年(未稅)
                        double _cntBase_utG = Convert.ToDouble(item.Field<decimal>("CntBase_utG"));    //--[utG] 2341當月(未稅)
                        double _cnt_a = 0, _cnt_uta = 0, _cnt_b = 0, _cnt_c = 0, _cnt_utc = 0, _cnt_d = 0;
                        double _cnt_e = item.Field<double>("RespMoney");
                        double _cnt_f = item.Field<double>("RespPercent") / 100;
                        double _cnt_g = item.Field<double>("FightMoney");
                        double _cnt_h = item.Field<double>("FightPercent") / 100;
                        double _profitA = 0, _profitB = 0, _profitC = 0;  //--返利前毛利率,返利後毛利率,全返後毛利率

                        /* [計算公式]
                        ----- 公式A -----
                        (責任目標) e = RespMoney, f = RespPercent(回饋%)
                        (挑戰目標) g = FightMoney, h = FightPercent(回饋%)
                        [A](目前系統業績)
                        [B](已回饋金額)
                        [C](當月銷售金額)
                        [a](實際業績)
                          a = A + B - F

                        [b](最高返利金額)
                          b = (c + cB) * 0.5

                        [c](應回饋金額)(改)
                         (a >= ISNULL(g, 0) && g>0) --> c=a*h
                         (ELSE) --> c=a*f

                        [d](剩餘回饋金額)
                          d = c - B

                        [D](與挑戰目標差額)
                          D = A + B - g

                        [E](與責任目標差額)
                          E = A + B - e
                          
                        ----- 公式B -----
                        [c](應回饋金額)
                         (a >= ISNULL(g, 0) && g>0) --> (a-e)*h + e*f
                         (ELSE) --> c=a*f

                        */

                        //[D] 與挑戰目標差額(A + B - g)
                        _cntBase_D = (_cnt_g.Equals(0)) ? 0 : _cntBase_A + _cntBase_B - _cnt_g;
                        //[E] 與責任目標差額(A + B - e)
                        _cntBase_E = _cntBase_A + _cntBase_B - _cnt_e;

                        //(實際業績)-a = A + B - F
                        _cnt_a = _cntBase_A + _cntBase_B - _cntBase_F; //(tax)
                        _cnt_uta = _cntBase_utA;

                        //(最高返利金額)-b
                        _cnt_b = (_cntBase_C + _cntBase_cB) * 0.5;


                        /* 判斷選擇哪種計算公式 */
                        switch (item.Field<string>("Formula"))
                        {
                            default:
                                //(應回饋金額)-c
                                /*
                                IF((A+B+?) < e
                                 0 
                                else
                                 IF (g=0)
	                                (A+B-F)*f
                                 else IF((A+B+?)>g 
                                     (((A+B-F)-e)*h)+(e*f)
	                                else IF((A+B+?)>e
	                                  (A+B-F)*f
                                */
                                if ((_cntBase_A + _cntBase_B) < _cnt_e)
                                {
                                    _cnt_c = 0;
                                }
                                else
                                {
                                    if (_cnt_g.Equals(0))
                                    {
                                        //(A + B - F) * f
                                        _cnt_c = (_cntBase_A + _cntBase_B - _cntBase_F) * _cnt_f;
                                        //未稅
                                        _cnt_utc = (_cntBase_utA + _cntBase_utB - _cntBase_utF) * _cnt_f;
                                    }
                                    else
                                    {
                                        if ((_cntBase_A + _cntBase_B) > _cnt_g)
                                        {
                                            //(((A + B - F) - e) * h) + (e * f)
                                            _cnt_c = (((_cntBase_A + _cntBase_B - _cntBase_F) - _cnt_e) * _cnt_h) + (_cnt_e * _cnt_f);
                                            //未稅
                                            _cnt_utc = (((_cntBase_utA + _cntBase_utB - _cntBase_utF) - _cnt_e) * _cnt_h) + (_cnt_e * _cnt_f);
                                        }
                                        else
                                        {
                                            //(A + B - F) * f
                                            _cnt_c = (_cntBase_A + _cntBase_B - _cntBase_F) * _cnt_f;
                                            //未稅
                                            _cnt_utc = (_cntBase_utA + _cntBase_utB - _cntBase_utF) * _cnt_f;
                                        }
                                    }

                                }

                                //(剩餘回饋金額)-d = c - B
                                _cnt_d = _cnt_c - _cntBase_B;

                                break;
                        }


                        //Check E > 0
                        if (_cntBase_E < 0)
                        {
                            _cnt_c = 0;
                        }


                        /* 返利前毛利率 = (未稅A - 成本)/未稅A */
                        _profitA = _cntBase_utA > 0
                            ? ((_cntBase_utA - _cntBase_costA) / _cntBase_utA) * 100
                            : 0;

                        /* 返利後毛利率 = ((未稅A - 未稅已回饋B) - 成本)/(未稅A - 未稅已回饋B) */
                        _profitB = (_cntBase_utA - _cntBase_utB) > 0
                            ? (((_cntBase_utA - _cntBase_utB) - _cntBase_costA) / (_cntBase_utA - _cntBase_utB)) * 100
                            : 0;

                        /* 全返利後毛利率 = ((未稅A - 未稅應回饋c) - 成本)/(未稅A - 未稅應回饋c) */
                        _profitC = (_cntBase_utA - _cnt_utc) > 0
                            ? (((_cntBase_utA - _cnt_utc) - _cntBase_costA) / (_cntBase_utA - _cnt_utc)) * 100
                            : 0;
                        #endregion


                        //加入項目
                        var data = new CustRebateItem
                        {
                            Data_ID = item.Field<Guid>("Data_ID"),
                            DataYear = item.Field<string>("DataYear"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            DeptName = item.Field<string>("DeptName"),
                            Formula = _formula,
                            Remark = item.Field<string>("Remark"),
                            CntBase_A = _cntBase_A,
                            CntBase_utA = _cntBase_utA,
                            CntBase_costA = _cntBase_costA,
                            CntBase_B = _cntBase_B,
                            CntBase_C = _cntBase_C,
                            CntBase_utC = _cntBase_utC,
                            CntBase_D = _cntBase_D,
                            CntBase_E = _cntBase_E,
                            CntBase_F = _cntBase_F,
                            CntBase_G = _cntBase_G,
                            CntBase_utF = _cntBase_utF,
                            CntBase_utG = _cntBase_utG,
                            Cnt_a = _cnt_a,
                            Cnt_uta = _cnt_uta,
                            Cnt_b = _cnt_b,
                            Cnt_c = _cnt_c,
                            Cnt_d = _cnt_d,
                            Cnt_e = _cnt_e,
                            Cnt_f = _cnt_f,
                            Cnt_g = _cnt_g,
                            Cnt_h = _cnt_h,
                            ProfitA = _profitA,
                            ProfitB = _profitB,
                            ProfitC = _profitC,
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
        /// 客戶返利維護(CustRebate/Edit)
        /// </summary>
        /// <param name="CompID">公司別</param>
        /// <param name="search">查詢參數</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public IQueryable<CustRebateItem> GetCustRebateBase(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<CustRebateItem> dataList = new List<CustRebateItem>();

            //----- 資料取得 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    SELECT Base.Data_ID, RTRIM(Cust.MA001) AS CustID, RTRIM(Cust.MA002)AS CustName
                     , Base.DataYear, Base.RespMoney, Base.RespPercent
                     , ISNULL(Base.FightMoney, 0) FightMoney, ISNULL(Base.FightPercent, 0) FightPercent
                     , Base.Formula, Base.Remark
                     , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Create_Who)) AS Create_Name
                     , (SELECT Account_Name + ' (' + Display_Name + ')' FROM PKSYS.dbo.User_Profile WITH(NOLOCK) WHERE ([Guid] = Base.Update_Who)) AS Update_Name
                    FROM [PKEF].dbo.Rebate_Data Base
                     INNER JOIN [PKSYS].dbo.Customer Cust ON Base.CustID = Cust.MA001 AND Cust.DBS = Cust.DBC
                    WHERE (1=1)
                ";

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
                                //客戶代號
                                sql += " AND (UPPER(Base.Data_ID) = UPPER(@ID))";

                                cmd.Parameters.AddWithValue("ID", item.Value);

                                break;
                        }
                    }
                }
                #endregion

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                using (DataTable DT = dbConn.LookupDT(cmd, dbConn.DBS.Local, out ErrMsg))
                {
                    //LinQ 查詢
                    var query = DT.AsEnumerable();

                    //資料迴圈
                    foreach (var item in query)
                    {
                        //加入項目
                        var data = new CustRebateItem
                        {
                            Data_ID = item.Field<Guid>("Data_ID"),
                            DataYear = item.Field<string>("DataYear"),
                            CustID = item.Field<string>("CustID"),
                            CustName = item.Field<string>("CustName"),
                            DeptName = item.Field<string>("DeptName"),
                            Formula = item.Field<string>("Formula"),
                            Remark = item.Field<string>("Remark"),
                            Cnt_e = item.Field<double>("RespMoney"),
                            Cnt_f = item.Field<double>("RespPercent"),
                            Cnt_g = item.Field<double>("FightMoney"),
                            Cnt_h = item.Field<double>("FightPercent"),
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
        /// 子母公司設定
        /// </summary>
        /// <param name="search"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public DataTable GetCustRelList(Dictionary<string, string> search, out string ErrMsg)
        {
            //----- 宣告 -----
            List<ClassItem> dataList = new List<ClassItem>();
            
            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                string sql = @"
                    SELECT Base.SeqNo
                     , Base.CustID, Cust1.MA002 CustName
                     , Base.ParentCustID, Cust2.MA002 ParentCustName
                    FROM [PKEF].[dbo].[Rebate_RefCust] Base
                     INNER JOIN [PKSYS].dbo.Customer Cust1 ON Base.CustID = Cust1.MA001 AND Cust1.DBS = Cust1.DBC
                     INNER JOIN [PKSYS].dbo.Customer Cust2 ON Base.ParentCustID = Cust2.MA001 AND Cust2.DBS = Cust2.DBC
                    WHERE (1=1)
                    ";

                /* Search */
                //#region >> filter <<

                //if (search != null)
                //{
                //    //過濾空值
                //    var thisSearch = search.Where(fld => !string.IsNullOrWhiteSpace(fld.Value));

                //    //查詢內容
                //    foreach (var item in thisSearch)
                //    {
                //        switch (item.Key)
                //        {
                //            case "Keyword":
                //                sql.Append(" AND (");
                //                sql.Append(" ToWho LIKE '%' + UPPER(@keyword) + '%'");
                //                sql.Append(" OR ToComp LIKE '%' + UPPER(@keyword) + '%'");
                //                sql.Append(" OR ToAddr LIKE '%' + UPPER(@keyword) + '%'");
                //                sql.Append(")");

                //                cmd.Parameters.AddWithValue("keyword", item.Value);

                //                break;
                //        }
                //    }
                //}
                //#endregion

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();

                //----- 資料取得 -----
                return dbConn.LookupDT(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        #endregion



        #region -----// Create //-----

        /// <summary>
        /// 建立客戶返利目標
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool CreateCustRebate(CustRebateItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Rebate_Data( ");
                sql.AppendLine("  Data_ID, CompID");
                sql.AppendLine("  , DataYear, CustID, Formula");
                sql.AppendLine("  , RespMoney, RespPercent, FightMoney, FightPercent");
                sql.AppendLine("  , Remark");
                sql.AppendLine("  , Create_Who, Create_Time");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @Data_ID, 'CHN'");
                sql.AppendLine("  , @DataYear, @CustID, @Formula");
                sql.AppendLine("  , @RespMoney, @RespPercent, @FightMoney, @FightPercent");
                sql.AppendLine("  , @Remark");
                sql.AppendLine("  , @Create_Who, GETDATE()");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", instance.Data_ID);
                cmd.Parameters.AddWithValue("DataYear", instance.DataYear);
                cmd.Parameters.AddWithValue("CustID", instance.CustID);
                cmd.Parameters.AddWithValue("Formula", instance.Formula);
                cmd.Parameters.AddWithValue("RespMoney", instance.Cnt_e);
                cmd.Parameters.AddWithValue("RespPercent", instance.Cnt_f);
                cmd.Parameters.AddWithValue("FightMoney", instance.Cnt_g);
                cmd.Parameters.AddWithValue("FightPercent", instance.Cnt_h);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("Create_Who", instance.Create_Who);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }

        /// <summary>
        /// 新增名單
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public bool Create_CustRel(RebateCust instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" INSERT INTO Rebate_RefCust( ");
                sql.AppendLine("  CustID, ParentCustID");
                sql.AppendLine(" ) VALUES (");
                sql.AppendLine("  @CustID, @ParentCustID");
                sql.AppendLine(" );");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("CustID", instance.CustID);
                cmd.Parameters.AddWithValue("ParentCustID", instance.ParentCustID);


                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }

        }

        #endregion



        #region -----// Update //-----

        /// <summary>
        /// 更新客戶返利
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public bool UpdateCustRebate(CustRebateItem instance, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" UPDATE Rebate_Data SET ");
                sql.AppendLine("  DataYear = @DataYear, CustID = @CustID, Formula = @Formula, Remark = @Remark");
                sql.AppendLine("  , RespMoney = @RespMoney, RespPercent = @RespPercent, FightMoney = @FightMoney, FightPercent = @FightPercent");
                sql.AppendLine("  , Update_Who = @Update_Who, Update_Time = GETDATE()");
                sql.AppendLine(" WHERE (Data_ID = @DataID)");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("DataID", instance.Data_ID);
                cmd.Parameters.AddWithValue("DataYear", instance.DataYear);
                cmd.Parameters.AddWithValue("CustID", instance.CustID);
                cmd.Parameters.AddWithValue("Formula", instance.Formula);
                cmd.Parameters.AddWithValue("RespMoney", instance.Cnt_e);
                cmd.Parameters.AddWithValue("RespPercent", instance.Cnt_f);
                cmd.Parameters.AddWithValue("FightMoney", instance.Cnt_g);
                cmd.Parameters.AddWithValue("FightPercent", instance.Cnt_h);
                cmd.Parameters.AddWithValue("Remark", instance.Remark);
                cmd.Parameters.AddWithValue("Update_Who", instance.Update_Who);

                //execute
                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }


        #endregion


        #region -----// Delete //-----
        /// <summary>
        /// 刪除名單
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        public bool Delete_CustRel(string dataID, out string ErrMsg)
        {
            //----- 宣告 -----
            StringBuilder sql = new StringBuilder();

            //----- 資料查詢 -----
            using (SqlCommand cmd = new SqlCommand())
            {
                //----- SQL 查詢語法 -----
                sql.AppendLine(" DELETE FROM Rebate_RefCust WHERE (SeqNo = @Data_ID);");

                //----- SQL 執行 -----
                cmd.CommandText = sql.ToString();
                cmd.Parameters.AddWithValue("Data_ID", dataID);

                return dbConn.ExecuteSql(cmd, dbConn.DBS.PKEF, out ErrMsg);
            }
        }
        #endregion
    }
}

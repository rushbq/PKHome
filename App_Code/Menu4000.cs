using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Menu4000Data.Models
{
    /// <summary>
    /// 延遲分析
    /// </summary>
    public class DelayShipItem
    {
        public string CompName { get; set; }
        public string OrderDate { get; set; }
        public string ShipDateNew { get; set; }
        public string ShipDateOld { get; set; }
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string OrderNoType { get; set; }
        public string OrderNo { get; set; }
        public string OrderSerial { get; set; }
        public string ModelNo { get; set; }
        public int OrderNum { get; set; }
        public int OrderNum_Pend { get; set; }
        public decimal PendingPrice { get; set; }
        public int RangeDays { get; set; }
        public string Supplier { get; set; }
        public string PendingDate { get; set; }
        public int NewQty { get; set; }
        public string ReasonName { get; set; }
        public string FlowNo { get; set; }
        public string Purchaser { get; set; }
        public string Currency { get; set; }


    }


    /// <summary>
    /// 外廠包材庫存盤點:發送清單
    /// </summary>
    public class SupInvList
    {
        public int SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public string Subject { get; set; }
        public string TaskTime { get; set; }
        public string IsOnTask { get; set; }
        public int QueueCnt { get; set; }
        public int SentCnt { get; set; }
        public string Remark { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Who { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Name { get; set; }
    }

    public class SupInvSupplier
    {
        public Guid Parent_ID { get; set; }
        public int Data_ID { get; set; }
        public string SupID { get; set; }
        public string SupName { get; set; }
        public string SupMails { get; set; }
        public string PurWhoID { get; set; }
        public string PurWhoName { get; set; }
        public string PurWhoEmail { get; set; }
        public string Token { get; set; }
        public string StockShow { get; set; }
        public string WriteTime { get; set; }
        public string IsWrite { get; set; }
        public string SendTime { get; set; }
        public string IsSend { get; set; }
        public int DataCheck { get; set; }
    }

    public class SupInvModel
    {
        //public Guid Parent_ID { get; set; }
        //public int Data_ID { get; set; }
        //public string SupID { get; set; }
        public string ModelNo { get; set; }
        public string ModelName { get; set; }
        public int InputQty1 { get; set; }
        public int InputQty2 { get; set; }

        /* 其他關聯欄位 */
        //寶工庫存
        public int StockNum { get; set; }
        //最進入庫日
        public string inStockDate { get; set; }
        //最近出庫日
        public string outStockDate { get; set; }
        //替代品號
        public string anotherModel { get; set; }
        public string Currency { get; set; }
        public double lastPurPrice { get; set; }
    }


    public class PurPlanList
    {
        public string ModelNo { get; set; }
        public string Item_Type { get; set; }
        public string ModelName { get; set; }
        public string ProdVol { get; set; }
        public string ProdPage { get; set; }
        public string Currency { get; set; }
        public double checkPrice { get; set; }
        public int WaitQty { get; set; }
        public int StockQty_A01 { get; set; }
        public int SafeQty_A01 { get; set; }
        public int PreIN_A01 { get; set; }
        public int VirIn_A01 { get; set; }
        public int PlanIN_A01 { get; set; }
        public int PreSell_A01 { get; set; }
        public int PreGet_A01 { get; set; }
        public int PlanOut_A01 { get; set; }
        public int StockQty_12 { get; set; }
        public int SafeQty_12 { get; set; }
        public int PreIN_12 { get; set; }
        public int VirIn_12 { get; set; }
        public int PlanIN_12 { get; set; }
        public int PreSell_12 { get; set; }
        public int PreGet_12 { get; set; }
        public int PlanOut_12 { get; set; }
        public double Qty_Days { get; set; }
        public double Qty_Year { get; set; }
        public double Qty_Season { get; set; }
        public int VirPreSell { get; set; }
        public int RealPreSell { get; set; }
        public int CNT { get; set; }
        public double SZ_QtyOfYear { get; set; }
        public int Min_Supply { get; set; }
        public int InBox_Qty { get; set; }
        public int Qty_Packing { get; set; }
        public double OutBox_Cuft { get; set; }
        public int MOQ { get; set; }
        public string ProdMsg { get; set; }
        public string Supplier { get; set; }
        public string Supply_ID { get; set; }

        public int PushQty { get; set; }
        public int UsefulQty_A01 { get; set; }
        public int UsefulQty_12 { get; set; }
        public int QTM_A01 { get; set; }
        public int QTM_12 { get; set; }

        public double MonthTurn_A01 { get; set; }
        public double MonthTurn_12 { get; set; }
        public double NowMonthTurn_A01 { get; set; }
        public double NowMonthTurn_12 { get; set; }
        public double QTM_Month { get; set; }
        public Int64 RowIdx { get; set; }

    }

    public class PurProdCostList
    {
        /// <summary>
        /// 品號
        /// </summary>
        public string ModelNo { get; set; }

        /// <summary>
        /// 包材品號
        /// </summary>
        public string PackItemNo { get; set; }

        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 廠商
        /// </summary>
        public string SupName { get; set; }

        /// <summary>
        /// 品號核價單價
        /// </summary>
        public decimal ModelPrice { get; set; }

        /// <summary>
        /// 包材核價單價
        /// </summary>
        public decimal PackPrice { get; set; }

        /// <summary>
        /// 包材數量
        /// </summary>
        public decimal PackQty { get; set; }

        /// <summary>
        /// 品號備註
        /// </summary>
        public string ProdNote { get; set; }

        /// <summary>
        /// 計算標準成本
        /// 品號核價單價 + (卡片核價單價 * 數量)
        /// </summary>
        public decimal ProdCost { get; set; }

        /// <summary>
        /// ERP標準成本
        /// </summary>
        public decimal ERPStdCost { get; set; }

        /// <summary>
        /// 包材核價單價 * 數量
        /// </summary>
        public decimal PackSumPrice { get; set; }

        public Int64 RowIdx { get; set; }
    }

}

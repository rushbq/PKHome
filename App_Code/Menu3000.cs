using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Menu3000Data.Models
{
    #region -- 銷貨單庫存狀況 --
    /// <summary>
    /// 銷貨單庫存狀況
    /// </summary>
    public class OrderStockItem
    {
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string SO_Fid { get; set; }
        public string SO_Sid { get; set; }
        public string ModelNo { get; set; }
        public string StockType { get; set; }
        public int BuyCnt { get; set; }
        public string ShopWhere { get; set; }
        public string ShopOrderID { get; set; }
        public int StockQty_A01 { get; set; }
        public int PreSell_A01 { get; set; }
        public int PreIN_A01 { get; set; }
        public int gapA01 { get; set; }
        public int StockQty_B01 { get; set; }
        public int PreSell_B01 { get; set; }
        public int PreIN_B01 { get; set; }
        public int gapB01 { get; set; }

    }

    /// <summary>
    /// 預計銷
    /// </summary>
    public class PreSellItems
    {
        public string ModelNo { get; set; }
        public int Qty { get; set; }
        public string StockType { get; set; }

    }

    #endregion


    #region -- 發貨/運費 --
    /// <summary>
    /// 發貨/運費
    /// </summary>
    public class ShipFreightItem
    {
        public Guid? Data_ID { get; set; }
        public string CompID { get; set; }
        public string Erp_SO_FID { get; set; }
        public string Erp_SO_SID { get; set; }
        public string Erp_SO_Date { get; set; }
        public string CustID { get; set; }
        public string CustName { get; set; }
        public decimal TotalPrice { get; set; }
        public string StockType { get; set; }
        public string StockName { get; set; }

        //發貨日期
        public string ShipDate { get; set; }
        //貨運公司
        public Int32? ShipComp { get; set; }
        public string ShipCompName { get; set; }
        //物流途徑
        public string ShipWay { get; set; }
        //收貨人
        public string ShipWho { get; set; }
        //件數
        public Int32? ShipCnt { get; set; }
        //備註
        public string Remark { get; set; }
        public string Create_Who { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }

        #region 僅顯示
        //物流單號
        public string ShipNo { get; set; }
        //到付
        public double? Pay1 { get; set; }
        //自付
        public double? Pay2 { get; set; }
        //墊付
        public double? Pay3 { get; set; }
        //平均到付
        public double AvgPay1 { get; set; }
        //平均自付
        public double AvgPay2 { get; set; }
        //平均墊付
        public double AvgPay3 { get; set; }
        #endregion

        //是否已被關聯
        public int IsReled { get; set; }
    }

    public class ShipFreightDetail
    {
        public Guid Parent_ID { get; set; }
        public int Data_ID { get; set; }
        //物流單號
        public string ShipNo { get; set; }
        public Int32 ShipCnt { get; set; }
        //到付
        public double Pay1 { get; set; }
        //自付
        public double Pay2 { get; set; }
        //墊付
        public double Pay3 { get; set; }
    }

    public class ShipFreightRel
    {
        //目前單子ID
        public Guid Parent_ID { get; set; }
        //關聯對象ID
        public Guid Rel_ID { get; set; }
        public int Data_ID { get; set; }
        public string Erp_SO_FID { get; set; }
        public string Erp_SO_SID { get; set; }
    }

    //流物公司(PKEF.Logistics)
    public class ShipComp
    {
        public Int32 ID { get; set; }
        public string CompID { get; set; }
        public string Label { get; set; }
        public string Display { get; set; }
        public Int16 Sort { get; set; }
    }

    /// <summary>
    /// 運費統計
    /// </summary>
    public class ShipStat_Year
    {
        public string showYM { get; set; }
        public int Month { get; set; }
        public string sDate { get; set; }
        public string eDate { get; set; }
        public double TotalPrice { get; set; }
        public Int32 ItemCnt { get; set; }
        public Int32 ShipCnt { get; set; }
        //到付
        public double Pay1 { get; set; }
        //自付
        public double Pay2 { get; set; }
        //墊付
        public double Pay3 { get; set; }
        public double avgPercent { get; set; }
    }


    /// <summary>
    /// 運費匯入單頭
    /// </summary>
    public class ShipImportData
    {
        #region -- 資料庫欄位 --

        public int SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public string TraceID { get; set; }
        public string erpSDate { get; set; } //(format:yyyyMMdd)
        public string erpEDate { get; set; } //(format:yyyyMMdd)      
        public decimal Status { get; set; }
        public string StatusName { get; set; }
        public string Upload_File { get; set; }
        public string Sheet_Name { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Who { get; set; }
        public string Update_Time { get; set; }
        public string Remark { get; set; }


        #endregion


        #region -- 關聯欄位 --

        public string Create_Name { get; set; }
        public string Update_Name { get; set; }

        #endregion
    }

    /// <summary>
    /// 運費匯入單身
    /// </summary>
    public class ShipImportDataDT
    {
        public int Data_ID { get; set; }
        public string ShipNo { get; set; }
        public string ShipDate { get; set; }
        public int Qty { get; set; }
        public double Freight { get; set; }
        public string IsPass { get; set; }
        public string doWhat { get; set; }
    }

    #endregion


    #region -- 客戶返利 --
    public class CustRebateItem
    {
        public Guid Data_ID { get; set; }
        public string CompID { get; set; }
        public string DataYear { get; set; }
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string DeptName { get; set; }
        public string Formula { get; set; }
        public string Remark { get; set; }

        //-- 計算元素 Start --
        //(A) 目前系統業績(含稅)
        public double CntBase_A { get; set; }
        //(utA) 目前系統業績(未稅)
        public double CntBase_utA { get; set; }
        //(costA) 成本
        public double CntBase_costA { get; set; }
        //(B) 已回饋金額(整年)
        public double CntBase_B { get; set; }
        //(cB) 已回饋金額(當月)
        public double CntBase_cB { get; set; }
        //(C) 當月銷售金額(含稅)
        public double CntBase_C { get; set; }
        //(utC) 當月銷售金額(未稅)
        public double CntBase_utC { get; set; }
        //(D) 與挑戰目標差額
        public double CntBase_D { get; set; }
        //(E) 與責任目標差額
        public double CntBase_E { get; set; }
        //(F) 2341整年(含稅)
        public double CntBase_F { get; set; }
        //(Fa) B009整年(含稅)
        public double CntBase_Fa { get; set; }
        //(G) 2341當月(含稅)
        public double CntBase_G { get; set; }
        //(utF) 2341整年(未稅)
        public double CntBase_utF { get; set; }
        //(utG) 2341當月(未稅)
        public double CntBase_utG { get; set; }
        //(a) 實際業績(含稅)
        public double Cnt_a { get; set; }
        //(a) 實際業績(未稅)
        public double Cnt_uta { get; set; }
        //(b) 最高返利金額
        public double Cnt_b { get; set; }
        //(c) 應回饋金額
        public double Cnt_c { get; set; }
        //(d) 剩餘回饋金額
        public double Cnt_d { get; set; }

        //(e)
        public double Cnt_e { get; set; }
        //(f)
        public double Cnt_f { get; set; }
        //(g)
        public double? Cnt_g { get; set; }
        //(h)
        public double? Cnt_h { get; set; }

        //(profitA) 返利前毛利率
        public double ProfitA { get; set; }
        //(profitB) 返利後毛利率
        public double ProfitB { get; set; }
        //(profitC) 全返後毛利率
        public double ProfitC { get; set; }

        //-- 計算元素 End --

        public string Create_Who { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }

    }


    public class RebateCust
    {
        public string CustID { get; set; }
        //public string CustName { get; set; }
        public string ParentCustID { get; set; }
        //public string ParentCustName { get; set; }
    }

    #endregion


    #region -- 客訴 --

    /// <summary>
    /// 開案中客訴-基本資料
    /// </summary>
    public class CCPTempItem
    {
        public Guid Data_ID { get; set; }
        public Int16 CC_Type { get; set; }
        public string CC_TypeName { get; set; }
        public string TraceID { get; set; }
        public Int32? PlanType { get; set; }
        public string PlanTypeName { get; set; }
        public Int32? BadReason { get; set; }
        public string BadReasonName { get; set; }
        public string InvoiceIsBack { get; set; }
        public Int32? CustType { get; set; }
        public string CustTypeName { get; set; }
        public string CustInput { get; set; }
        public string RefCustID { get; set; }
        public Int32? RefMallID { get; set; }
        public string RefCustName { get; set; }
        public string RefMallName { get; set; }
        public string BuyerName { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerAddr { get; set; }
        public string Platform_ID { get; set; }
        public string ERP_ID { get; set; }
        public string CS_Time { get; set; }
        public string CS_Who { get; set; }
        public string CS_Name { get; set; }
        public Int32? FreightType { get; set; }
        public string FreightTypeName { get; set; }
        public string FreightInput { get; set; }
        public string FreightGetDate { get; set; }
        public string Freight_Time { get; set; }
        public string Freight_Who { get; set; }
        public string Freight_Name { get; set; }
        public string InvoiceNumber { get; set; }
        public double? InvoicePrice { get; set; }
        public string ShipComp { get; set; }
        public string ShipWho { get; set; }
        public string ShipTel { get; set; }
        public string ShipAddr { get; set; }

        public string Invoke_Who { get; set; }
        public string Invoke_Time { get; set; }
        public string IsInvoke { get; set; }

        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }

        /// <summary>
        /// 商品資料數
        /// </summary>
        public Int32 DtCnt { get; set; }

        /// <summary>
        /// 客服是否填寫(Y/N)
        /// </summary>
        public string IsCS { get; set; }

        /// <summary>
        /// 收貨是否填寫(Y/N)
        /// </summary>
        public string IsFreight { get; set; }
    }

    /// <summary>
    /// 開案中客訴-商品資料
    /// </summary>
    public class CCPDetail
    {
        public Guid Parent_ID { get; set; }
        public Int64 Data_ID { get; set; }
        public string ModelNo { get; set; }
        public Int32 Qty { get; set; }
        public string Remark { get; set; }
        public string IsSplit { get; set; }
        public string IsWarranty { get; set; }
        public string Create_Who { get; set; }
    }

    /// <summary>
    /// 客訴
    /// </summary>
    public class CCPItem
    {
        public Guid Data_ID { get; set; }
        public Int32 SeqNo { get; set; }
        //客訴來源
        public Int16 CC_Type { get; set; }
        public string CC_TypeName { get; set; }
        //客訴編號
        public string CC_UID { get; set; }
        //追蹤編號
        public string TraceID { get; set; }
        public Int32 PlanType { get; set; }
        public string PlanTypeName { get; set; }
        public Int32 BadReason { get; set; }
        public string BadReasonName { get; set; }
        //客戶類別
        public Int32 CustType { get; set; }
        public string CustTypeName { get; set; }
        public string CustInput { get; set; }
        public string RefCustID { get; set; }
        public Int32? RefMallID { get; set; }
        public string RefCustName { get; set; }
        public string RefMallName { get; set; }
        public string RefERP_ID { get; set; }
        public string RefPlatform_ID { get; set; }
        public string ModelNo { get; set; }
        public Int32 Qty { get; set; }
        //客訴內容
        public string Remark { get; set; }
        //客訴判斷說明
        public string Remark_Check { get; set; }
        public string IsSplit { get; set; } //開案時判斷用
        public string IsWarranty { get; set; }

        //流程狀態:FlowID
        public Int32 FlowStatus { get; set; }
        public string FlowStatusName { get; set; }
        //處理方式
        public Int32? Flow201_Type { get; set; }
        //處理說明
        public string Flow201_Desc { get; set; }
        //處理時間
        public string Flow201_Time { get; set; }
        public string Flow201_Who { get; set; }
        public string Flow201_WhoName { get; set; }
        public string Flow201_TypeName { get; set; }

        public Int32? Flow301_Type { get; set; }
        public string Flow301_Desc { get; set; }
        public string Flow301_Time { get; set; }
        public string Flow301_Who { get; set; }
        public string Flow301_WhoName { get; set; }
        public string Flow301_TypeName { get; set; }
        public Int32? Flow401_Type { get; set; }
        public string Flow401_Desc { get; set; }
        public string Flow401_Time { get; set; }
        public string Flow401_Who { get; set; }
        public string Flow401_WhoName { get; set; }
        public string Flow401_TypeName { get; set; }
        public Int32? Flow501_Type { get; set; }
        public string Flow501_Desc { get; set; }
        public string Flow501_Time { get; set; }
        public string Flow501_Who { get; set; }
        public string Flow501_WhoName { get; set; }
        public string Flow501_TypeName { get; set; }
        //報價金額
        public double? FixPrice { get; set; }
        //維修預計完成日
        public string FixWishDate { get; set; }
        //維修完成日
        public string FixOkDate { get; set; }

        //客訴銷單號
        public string ERP_No1 { get; set; }
        //借出單號
        public string ERP_No2 { get; set; }
        //歸還單號
        public string ERP_No3 { get; set; }
        //銷退單號
        public string ERP_No4 { get; set; }
        //維修費訂單
        public string ERP_No5 { get; set; }
        //維修費銷貨單
        public string ERP_No6 { get; set; }
        //維修費
        public double? FixTotalPrice { get; set; }
        public string ShipComp { get; set; }
        //物流單號
        public string ShipNo { get; set; }
        //寄貨日
        public string ShipDate { get; set; }
        //結案時間
        public string Finish_Time { get; set; }
        public string Finish_Who { get; set; }
        public string Finish_WhoName { get; set; }
        //結案(作廢)說明
        public string Finish_Remark { get; set; }
        //開案中資料數
        public Int32 unOpenCnt { get; set; }

        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }

        //輸入時的共用欄位
        public string inputType { get; set; }
        public string inputDesc { get; set; }
        public int nextFlow { get; set; }
    }

    /// <summary>
    /// 客訴:附件
    /// </summary>
    public class CCPAttachment
    {
        public Guid Parent_ID { get; set; }
        public Int64 Data_ID { get; set; }
        public Int32 FlowID { get; set; }
        public string FlowName { get; set; }
        public string FilePath { get; set; }
        public string AttachFile { get; set; }
        public string AttachFile_Org { get; set; }
        public string Create_Who { get; set; }
        public string Create_Name { get; set; }
    }


    /// <summary>
    /// 客訴:通知設定
    /// </summary>
    public class CCPInform
    {
        public Int32 Data_ID { get; set; }
        public Int32 FlowID { get; set; }
        public string FlowName { get; set; }
        public Int16 CC_Type { get; set; } //客訴來源

        public string Who { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }

    }


    /// <summary>
    /// Log
    /// </summary>
    public class CCPLog
    {
        public Guid Parent_ID { get; set; }
        public Guid Data_ID { get; set; }

        /// <summary>
        /// 1:UserFlow 2:System
        /// </summary>
        public Int16 LogType { get; set; }
        public string LogSubject { get; set; }
        public string LogDesc { get; set; }
        public Int32 FlowID { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Create_Name { get; set; }
    }


    /// <summary>
    /// 圖表資料欄
    /// </summary>
    public class CCP_ChartData
    {
        public string Label { get; set; }
        public int Cnt { get; set; }
    }

    #endregion


    #region -- 電商平台資料 --
    public class ECDItem_Year
    {
        public Int32 SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public Int16 RefType { get; set; }
        public Int32 RefMall { get; set; }
        public string MallName { get; set; }
        public Int32 setYear { get; set; }
        public double Price_Sales { get; set; }
        public double Price_Rebate { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }

    }

    public class ECDItem_Month
    {
        public Int32 SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public Int16 RefType { get; set; }
        public Int32 RefMall { get; set; }
        public string MallName { get; set; }
        public Int32 setYear { get; set; }
        public Int32 setMonth { get; set; }
        public double? Price_Income { get; set; }
        public double? Price_SalesRebate { get; set; }
        public double? Price_Cost { get; set; }
        public double? Price_Profit { get; set; }
        public double? Price_Purchase { get; set; }
        public double? Price_Back { get; set; }
        public double? Price_PurchaseRebate { get; set; }
        public double? Price_Promo { get; set; }
        public double? Price_Freight { get; set; }
        public double? Profit { get; set; }
        public double? Profit_Percent { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }

    }

    public class ECDItem_Date
    {
        public Int32 SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public Int16 RefType { get; set; }
        public Int32 RefMall { get; set; }
        public string MallName { get; set; }
        public string setDate { get; set; }
        public double? Price_RefSales { get; set; }
        public double? Price_RefProfit { get; set; }
        public double? Price_RealSales { get; set; }
        public double? Price_RealProfit { get; set; }
        public double? Price_OrderPrice { get; set; }
        public double? Price_ROI { get; set; }
        public double? Price_ClickCost { get; set; }
        public double? Price_Adv1 { get; set; }
        public double? Price_Adv2 { get; set; }
        public double? Price_Adv3 { get; set; }

        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }

    }

    public class ECDItem_MonthDT
    {
        public Int64 Data_ID { get; set; }
        public Guid Parent_ID { get; set; }
        public string RecordDate { get; set; }
        public Int32 RecordType { get; set; }
        public double? RecordMoney { get; set; }
        public string CheckDate { get; set; }
        public double? CheckMoney { get; set; }
        public string CheckInvoiceDate { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
    }

    public class ECDItem_PriceList
    {
        public Int16 RefType { get; set; }
        public Int32 RefMall { get; set; }
        public string ProdID { get; set; }
        public double Price1 { get; set; }
    }

    public class ECDItem_SalesAmount
    {
        public Int16 setYear { get; set; }
        public Int16 setMonth { get; set; }
        public Int16 RefType { get; set; }
        public Int16 RefMall { get; set; }
        public string ProdID { get; set; }
        public double Price1 { get; set; }
        public int Qty { get; set; }
    }
    #endregion


    #region -- 出貨明細 --

    public class ShipData_Item
    {
        /*** ERP資料欄位 ***/
        public string Ship_FID { get; set; }
        public string Ship_SID { get; set; }
        public string SO_FID { get; set; }
        public string SO_SID { get; set; }
        public string SO_Date { get; set; } //進倉日
        public string BoxDate { get; set; } //報關日
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string InvNo { get; set; }
        public string PayTerms { get; set; } //付款條件
        public string Currency { get; set; } //報關幣別
        public double Price { get; set; } //報關金額
        public double localPrice { get; set; } //客戶金額(M)
        public double Tax { get; set; } //匯率(W)
        public string CLS { get; set; }
        public string ETD { get; set; }
        public string ETA { get; set; }
        public string Ship_NO { get; set; } //報單號碼
        public int OpcsCnt { get; set; }
        public int OpcsItemCnt { get; set; }
        public int diffDays { get; set; } //報關差異天數
        public string SalesName { get; set; } //業務

        /*** 平台維護欄位 ***/
        public Guid? Data_ID { get; set; }
        public int? BoxCnt { get; set; }
        public string Pallet { get; set; }
        public double? Weight { get; set; }
        public double? Cuft { get; set; }
        public string TradeTerms { get; set; }
        public double? Cost_Customs { get; set; } //報關費(N)
        public double? Cost_LocalCharge { get; set; } //local charge(O)
        public double? Cost_Cert { get; set; } //產證費用(O1)
        public double? Cost_Freight { get; set; } //海運費(P)
        public double? Cost_Business { get; set; } //營業稅(Q)
        public double Cnt_CostExport { get; set; } //出口費用小計(N+O+P+Q)
        public double? Cost_Shipment { get; set; }
        public double? Cost_Fee { get; set; } //代收費用(原幣)(U)
        public double Cnt_CostLocalFee { get; set; } //代收費用(本幣)(U*W)
        public double Cnt_CostFullExport { get; set; } //實際出口費用(X=N+O+P-V)
        public double Cnt_CostPercent { get; set; } //費用比率(%)(Y=X/M)
        public string FWD { get; set; }
        public double? Cost_Trade { get; set; }
        public double? Cost_Service { get; set; }
        public double? Cost_Use { get; set; }
        public string Remark { get; set; }
        public string TrackingNo { get; set; }
        public int? ShipID { get; set; }
        public string ShipName { get; set; }
        public int? PlaceID { get; set; }
        public string PlaceName { get; set; }
        public int? CheckID { get; set; }
        public string CheckName { get; set; }


        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
    }


    public class ShipData_LocalItem
    {
        /*** ERP資料欄位 ***/
        public string SO_FID { get; set; }
        public string SO_SID { get; set; }
        public string SO_Date { get; set; } //銷貨日
        public string CustID { get; set; }
        public string CustName { get; set; }
        public double Price { get; set; } //銷貨金額
        public string InvNo_Start { get; set; } //發票號碼(起)
        public string InvNo_End { get; set; } //發票號碼(訖)
        public int OpcsCnt { get; set; }
        public int OpcsItemCnt { get; set; }
        public string SalesName { get; set; } //業務

        /*** 平台維護欄位 ***/
        public Guid? Data_ID { get; set; }
        public int? BoxCnt { get; set; }
        public string ShipNo { get; set; } //托運單號
        public double? Freight { get; set; } //運費
        public double Cnt_FreightPercent { get; set; } //運費比(%)(M=L/G)
        public string SendNo { get; set; } //發票寄送單號
        public string Remark { get; set; }
        public int? ShipID { get; set; }
        public string ShipName { get; set; }
        public int? CustType { get; set; }
        public string CustTypeName { get; set; }
        public int? ProdType { get; set; }
        public string ProdTypeName { get; set; }
        public int? SendType { get; set; }
        public string SendTypeName { get; set; }


        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
    }


    public class CustomsData_Item
    {
        /*** ERP資料欄位 ***/
        public string Redeem_FID { get; set; }
        public string Redeem_SID { get; set; }
        public string CustomsDate { get; set; } //報關日期
        public string RedeemDate { get; set; } //贖單日期
        public string SupID { get; set; }
        public string SupName { get; set; }
        public string QtyMark { get; set; } //件數
        public double CustomsPrice { get; set; } //報關金額(F)
        public string Currency { get; set; } //幣別
        public double PurPrice { get; set; } //採購金額
        public double Tax { get; set; } //匯率(H)
        public string CustomsNo { get; set; } //進口報單號碼
        public int PurCnt { get; set; }
        public int PurItemCnt { get; set; }

        /*** 平台維護欄位 ***/
        public Guid? Data_ID { get; set; }
        public double? Cost_Customs { get; set; } //報關費(J)
        public double? Cost_LocalCharge { get; set; } //local charge(K)
        public double? Cost_LocalBusiness { get; set; } //營業稅(L)
        public double Cost_Imports { get; set; } //進口稅(M)
        public double? Cost_Trade { get; set; } //貿推費(N)
        public double? Cost_ImportsBusiness { get; set; } //進口營業稅(O)
        public double Cnt_CostFee { get; set; } //代墊款(P=L+M+N)
        public double Cnt_Total { get; set; } //Total(Q=I+J+K+L+M+N)
        public double? Cost_Service { get; set; } //商港費(R)
        public double Cnt_CostPercent { get; set; } //進口費用% (J+K)/F
        public double? Cost_Truck { get; set; }
        public string Remark { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
    }


    public class ShipDataSH_Item
    {
        /*** ERP資料欄位 ***/
        public string Ship_FID { get; set; }
        public string Ship_SID { get; set; }
        public string SO_FID { get; set; }
        public string SO_SID { get; set; }
        public string SO_Date { get; set; } //進倉日
        public string BoxDate { get; set; } //報關日
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string InvNo { get; set; }
        public string PayTerms { get; set; } //付款條件
        public string Currency { get; set; } //報關幣別
        public double Price { get; set; } //報關金額
        public double localPrice { get; set; } //客戶金額(M)
        public double Tax { get; set; } //匯率(AC)
        public string CLS { get; set; }
        public string ETD { get; set; }
        public string ETA { get; set; }
        public int OpcsCnt { get; set; }
        public int OpcsItemCnt { get; set; }
        public int diffDays { get; set; } //報關差異天數
        public string SalesName { get; set; } //業務

        /*** 平台維護欄位 ***/
        public Guid? Data_ID { get; set; }
        public int? BoxCnt { get; set; }
        public string Pallet { get; set; }
        public double? Weight { get; set; }
        public double? Cuft { get; set; }
        public string TradeTerms { get; set; }
        public double? Price1 { get; set; } //包干費(N)
        public double? Price2 { get; set; } //操作費(O)
        public double? Price3 { get; set; } //文件費(P)
        public double? Price4 { get; set; } //報關費(Q)
        public double? Price5 { get; set; } //ENS(R)
        public double? Price6 { get; set; } //VGM(S)
        public double? Price7 { get; set; } //艙單費(T)
        public double Cnt_CostExport_NoTax { get; set; } //出口費用小計未稅(N+O+P+Q+R+S+T)
        public double? Cost_ExportTax { get; set; } //出口費用稅金(V)
        public double? Cost_Freight { get; set; } //海運費(W)
        public double Cnt_CostExport_Full { get; set; } //出口費用小計含稅(U+V+W)
        public double? Cost_Shipment { get; set; } //卡車/物流費用(Z)
        public double? Cost_Fee { get; set; } //代收費用(原幣)(AA)
        public double Cnt_CostLocalFee { get; set; } //代收費用(本幣)(V*Tax)
        public double Cnt_CostFullExport { get; set; } //實際出口費用(RMB)未稅(AD)(U-AA)
        public double? Cnt_CostPercent_NoTruck { get; set; } //費用比率-未含卡車費(%)(AD/M)
        public double? Cnt_CostPercent { get; set; } //費用比率-含卡車費(%)(AD+Z)/M
        public string FWD { get; set; }
        public string Remark { get; set; }
        public int? ShipID { get; set; }
        public string ShipName { get; set; }
        public int? CheckID { get; set; }
        public string CheckName { get; set; }


        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
    }


    #endregion


    #region -- 應收帳款 --
    public class ARData_Base
    {
        public int SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public string TraceID { get; set; }
        public string CustID { get; set; }
        public string CustName { get; set; }
        public string CustFullName { get; set; }
        public string TermName { get; set; }
        public string DBS { get; set; }
        public string erp_sDate { get; set; }
        public string erp_eDate { get; set; }

        /// <summary>
        /// 10:填寫中 / 20:已寄送
        /// </summary>
        public Int16 Status { get; set; }
        public string StatusName { get; set; }

        public string Send_Time { get; set; }
        public string Create_Who { get; set; }
        public string Create_Name { get; set; }
        public string Create_Time { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
        public string Update_Time { get; set; }
        public string ZipCode { get; set; }
        public string Addr { get; set; }
        public string AddrRemark { get; set; }
        public string Fax { get; set; }
        public string Tel { get; set; }
        public Int32 ItemCnt { get; set; }
        /// <summary>
        /// 報錯時的訊息
        /// </summary>
        public string ErrMessage { get; set; }

        /// <summary>
        /// 報錯時間
        /// </summary>
        public string ErrTime { get; set; }
    }


    public class ARData_Items
    {
        public string Erp_AR_ID { get; set; }
    }


    public class ARData_Addressbook
    {
        public string Email { get; set; }
    }


    public class ARData_Details
    {
        public Int64 SerialNo { get; set; }
        public string AR_Fid { get; set; }
        public string AR_Sid { get; set; }
        public string SO_Fid { get; set; }
        public string SO_Sid { get; set; }
        public string CustID { get; set; }
        public string CustName { get; set; }
        /// <summary>
        /// 憑證號碼
        /// </summary>
        public string AT_Fid { get; set; }
        public string AT_Sid { get; set; }
        public string AT_Tid { get; set; }
        /// <summary>
        /// 付款條件
        /// </summary>
        public string TermID { get; set; }
        public string TermName { get; set; }
        public string ArDate { get; set; }
        public string BillNo { get; set; }
        public string InvoiceNo { get; set; }
        public string PreGetDay { get; set; }
        public string Currency { get; set; }

        /// <summary>
        /// TA029 = 應收金額
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// TA042 = 本幣營業稅額
        /// </summary>
        public double TaxPrice { get; set; }

        /// <summary>
        /// TA031 = 已收金額
        /// </summary>
        public double GetPrice { get; set; }

        /// <summary>
        /// 銷貨單備註
        /// </summary>
        public string OrderRemark { get; set; }
    }


    public class ARData_PriceInfo
    {
        /// <summary>
        /// 預收款
        /// </summary>
        public double PreGetPrice { get; set; }

        /// <summary>
        /// 前期未收款
        /// </summary>
        public double unGetPrice { get; set; }

        /// <summary>
        /// 前期未收款筆數
        /// </summary>
        public int unGetCnt { get; set; }

        /// <summary>
        /// 本期應收總額
        /// </summary>
        public double TotalPrice { get; set; }

        /// <summary>
        /// 本幣未稅金額
        /// </summary>
        public double TotalPrice_NoTax { get; set; }

        /// <summary>
        /// 本幣稅額
        /// </summary>
        public double TotalTaxPrice { get; set; }
        public int Cnt { get; set; }

        public double AllPrice { get; set; }
    }


    #endregion


    #region -- 快遞貨運 --

    public class Delivery_Base
    {
        public int SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        /// <summary>
        /// 登記單號(共12碼:西元年 + 月 + 日 + 4碼流水號)
        /// </summary>
        public string TraceID { get; set; }
        /// <summary>
        /// 類別
        /// </summary>
        public Int16 ShipType { get; set; }
        public string ShipTypeName { get; set; }

        /// <summary>
        /// 寄送方式(Delivery_RefClass,1)
        /// </summary>
        public Int32 ShipWay { get; set; }
        public string ShipWayName { get; set; }

        /// <summary>
        /// 付款方式(Delivery_RefClass,2)
        /// </summary>
        public Int32 PayWay { get; set; }
        public string PayWayName { get; set; }

        public string ShipWho { get; set; }
        public string ShipWho_Name { get; set; }

        public string SendDate { get; set; }
        public string SendComp { get; set; }
        public string SendWho { get; set; }
        public string SendAddr { get; set; }
        public string SendTel1 { get; set; }
        public string SendTel2 { get; set; }
        public string ShipNo { get; set; }
        /// <summary>
        /// 運費
        /// </summary>
        public double ShipPay { get; set; }
        //總箱數
        public Int32? Box { get; set; }

        /// <summary>
        /// 內容物分類1(Delivery_RefClass,3)
        /// </summary>
        public Int32? BoxClass1 { get; set; }
        public string BoxClass1Name { get; set; }

        /// <summary>
        /// 內容物分類2(Delivery_RefClass,4)
        /// </summary>
        public Int32? BoxClass2 { get; set; }
        public string BoxClass2Name { get; set; }

        /// <summary>
        /// 對象分類(Delivery_RefClass,5)
        /// </summary>
        public Int32? TargetClass { get; set; }
        public string TargetName { get; set; }

        public string Remark1 { get; set; }
        public string Remark2 { get; set; }
        public string PurNo { get; set; }
        public string SaleNo { get; set; }
        public string InvoiceNo { get; set; }
        public string IsClose { get; set; }

        public string Create_Who { get; set; }
        public string Create_Name { get; set; }
        public string Create_Time { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
        public string Update_Time { get; set; }

    }


    public class Delivery_Import
    {
        public string TraceID { get; set; }
        public string ShipNo { get; set; }
        public double Freight { get; set; }
    }


    public class AddressBook
    {
        public string ToComp { get; set; }
        public string ToWho { get; set; }
        public string ToAddr { get; set; }
        public string ToTel { get; set; }

    }
    #endregion



    /// <summary>
    /// 類別
    /// </summary>
    public class ClassItem
    {
        public int ID { get; set; }
        public string Label { get; set; }
        public int MenuID { get; set; }
        public int Invoke_To { get; set; }

    }


    public class MailReceiver
    {
        public string Email { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Menu1000Data.Models
{
    #region -- 新品 --

    /// <summary>
    /// 自訂新品資料
    /// </summary>
    /// <remarks>
    /// Class_Lv1 關聯一階類別
    /// Class_Lv2 關聯二階類別
    /// ShipFrom = TW/SH/SZ
    /// </remarks>
    public class ProdPlanDataItem
    {
        public int SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public int Class_ID { get; set; }
        public int Class_Lv1 { get; set; }
        public int Class_Lv2 { get; set; }
        public string ModelNo { get; set; }
        public string ModelName { get; set; }
        public string Pic { get; set; }
        public string ShipFrom { get; set; }
        public string Supplier { get; set; }
        public string TargetMonth { get; set; }
        public string Remark { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
    }

    /// <summary>
    /// 報表關聯資料
    /// </summary>
    public class ProdPlanRptItem
    {
        /// <summary>
        /// 資料來源
        /// 0_SYS=報表, 1_NEW=自填新品
        /// </summary>
        public string FromData { get; set; }
        public string ItemNo { get; set; }
        public string ModelNo { get; set; }
        public int Class_ID { get; set; }
        public string Class_Name { get; set; }
        public string Model_Name { get; set; }
        public string Vol { get; set; }
        public string Page { get; set; }
        public string Ship_From { get; set; }
        public string ListPic { get; set; }
        public int? Menu_Lv1 { get; set; }
        public string MenuNameLv1 { get; set; }
        public int? Menu_Lv2 { get; set; }
        public string MenuNameLv2 { get; set; }
        public int? Menu_Lv3 { get; set; }
        public string MenuNameLv3 { get; set; }
        public int? StyleID { get; set; }
        public string StyleName { get; set; }
        public string TargetMonth { get; set; }
        public string SaleYear { get; set; }
        /// <summary>
        /// 銷售量
        /// </summary>
        public int SalesNum { get; set; }
        /// <summary>
        /// 銷售額
        /// </summary>
        public double SalesAmount { get; set; }
        /// <summary>
        /// 單位成本
        /// </summary>
        public double PaperCost { get; set; }
        public double avgSalesAmount { get; set; }
        public double avgPaperCost { get; set; }
        public string SupName { get; set; }
        public string Remark { get; set; }
        public string ProdDesc { get; set; }
        public string ProdFeature { get; set; }
        public Guid? DataID { get; set; }

    }


    public class CateItem
    {
        public int ID { get; set; }
        public string Label { get; set; }
        public int Parent_ID { get; set; }
        public int Menu_Level { get; set; }
        public int Class_ID { get; set; }
        public string NameEN { get; set; }
        public string NameTW { get; set; }
        public string NameCN { get; set; }
        public string Display { get; set; }
        public int Sort { get; set; }
    }
    #endregion
}

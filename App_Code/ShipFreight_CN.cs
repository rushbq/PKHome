using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShipFreight_CN.Models
{
    #region -- 發貨/運費 --
    /// <summary>
    /// 發貨/運費
    /// </summary>
    public class ShipFreightItem
    {
        public Guid? Data_ID { get; set; }
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
        //運費
        public double? Freight { get; set; }
        public string FreightWay { get; set; }
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
        public double? Freight { get; set; }
        public string FreightWay { get; set; }
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
        public double Freight { get; set; }
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
    /// 物流單匯入單身
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
        public string erpID { get; set; }
    }

    #endregion
}

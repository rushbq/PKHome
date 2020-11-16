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
        public string Erp_SO_FullID { get; set; }
        public string Erp_SO_FID { get; set; }
        public string Erp_SO_SID { get; set; }
        public string Erp_SO_Date { get; set; }
        public string CustID { get; set; }
        public string CustName { get; set; }
        public decimal TotalPrice { get; set; }
        public string CfmCode { get; set; }
        //物流單號
        public string ShipNo { get; set; }
        //收貨人
        public string ShipWho { get; set; }
        //收貨Tel
        public string ShipTel { get; set; }
        //收貨Addr1
        public string ShipAddr1 { get; set; }
        //收貨Addr2
        public string ShipAddr2 { get; set; }
        //運費
        public double? Freight { get; set; }
        //件數
        public int? BoxCnt { get; set; }
        //發貨日期
        public string ShipDate { get; set; }
        public string CfmWhoName { get; set; }

        //貨運公司
        public Int32? ShipComp { get; set; }
        //物流途徑
        public Int32? ShipWay { get; set; }
        //運費方式
        public Int32? SendType { get; set; }
        public string ShipCompName { get; set; }
        public string ShipWayName { get; set; }
        public string SendTypeName { get; set; }

        public string UserCheck1 { get; set; }
        public string Remark { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Who { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
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
        public decimal Status { get; set; }
        public string StatusName { get; set; }
        public string Upload_File { get; set; }
        public string Upload_Type { get; set; }
        public string Upload_TypeName { get; set; }
        public string Sheet_Name { get; set; }
        public string Remark { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Who { get; set; }
        public string Update_Time { get; set; }

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


    /// <summary>
    /// Public Class Menu
    /// </summary>
    public class ClassItem
    {
        public int ID { get; set; }
        public string Label { get; set; }

    }

    #endregion
}

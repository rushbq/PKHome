using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ToyAdditionalData.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Items
    {
        public int SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public string CompID { get; set; }
        public short CustType { get; set; }
        public string CustTypeName { get; set; }
        public string CustName { get; set; }
        public string CustTel { get; set; }
        public string CustAddr { get; set; }
        public string ModelNo { get; set; }
        public string ModelName { get; set; }
        public int Qty { get; set; }
        public string Remark1 { get; set; }
        public string Remark2 { get; set; }
        public string Remark3 { get; set; }
        public string ShipDate { get; set; }
        public string ShipNo { get; set; }
        public double Freight { get; set; }

        public string Create_Who { get; set; }
        public string Create_Name { get; set; }
        public string Create_Time { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
        public string Update_Time { get; set; }
        public string Ship_Who { get; set; }
        public string Ship_Name { get; set; }
        public string Ship_Time { get; set; }

    }
}

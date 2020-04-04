using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Menu2000Data.Models
{
    #region -- 製物工單 --
    /// <summary>
    /// 製物工單:基本資料
    /// </summary>
    public class MKHelpItem
    {        
        public Guid Data_ID { get; set; }
        public string TraceID { get; set; }
        public string CompID { get; set; }
        public string Req_Who { get; set; }
        public string Req_Name { get; set; }
        public string Req_Email { get; set; }
        public string Req_Dept { get; set; }
        public string Req_Subject { get; set; }
        public string Req_Content { get; set; }

        public int Req_Qty { get; set; }
        public int Req_Status { get; set; }
        public int Req_Class { get; set; }
        public int Req_Res { get; set; }
        public int Emg_Status { get; set; }
        public string StName { get; set; }
        public string StDisp { get; set; }
        public string TypeName { get; set; }
        public string ResName { get; set; }
        public string EmgName { get; set; }

        public string Wish_Date { get; set; }
        public string Est_Date { get; set; }
        public string Finish_Date { get; set; }
        public double? Finish_Hours { get; set; }

        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
        public string IsTimeOut { get; set; }
        public int ProcCnt { get; set; }

    }

    /// <summary>
    /// 製物工單:附件
    /// </summary>
    public class MKHelpAttachment
    {
        public Guid Parent_ID { get; set; }
        public int Data_ID { get; set; }
        public string AttachFile { get; set; }
        public string AttachFile_Org { get; set; }
        public string Create_Who { get; set; }

    }

    /// <summary>
    /// 製物工單:轉寄通知
    /// </summary>
    public class MKHelpCC
    {
        public Guid Parent_ID { get; set; }
        public int Data_ID { get; set; }
        public string CC_Who { get; set; }
        public string CC_Email { get; set; }

    }

    /// <summary>
    /// 製物工單:派案
    /// </summary>
    public class MKHelpAssigned
    {
        public Guid Parent_ID { get; set; }
        public int Data_ID { get; set; }
        public string Who { get; set; }
        public string WhoID { get; set; }
        public string Email { get; set; }
        public string Create_Who { get; set; }
        public string CompID { get; set; }

    }

    /// <summary>
    /// 製物工單:處理進度
    /// </summary>
    public class MKHelpReply
    {
        public Guid Parent_ID { get; set; }
        public int Data_ID { get; set; }
        public string Reply_Content { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Create_Name { get; set; }
    }

    /// <summary>
    /// 製物工單:固定收信清單
    /// </summary>
    public class MKHelpReceiver
    {
        public string Email { get; set; }
    }

    /// <summary>
    /// 類別
    /// </summary>
    public class ClassItem
    {
        public int ID { get; set; }
        public string Label { get; set; }
        public string CustomID { get; set; }

    }


    /// <summary>
    /// 圖表資料欄
    /// </summary>
    public class ChartData
    {
        public string Label { get; set; }
        public int Cnt { get; set; }
        public int SumQty { get; set; }
        public double Hours { get; set; }
    }
    #endregion


    #region -- 郵件寄送登記 --

    public class PostalItem
    {
        public int SeqNo { get; set; }
        public Guid Data_ID { get; set; }
        public string PostDate { get; set; }
        public string PostWho { get; set; }
        public string Post_WhoName { get; set; }
        public string Post_DeptName { get; set; }
        public int PostType { get; set; }
        public string PostTypeName { get; set; }
        public string PostNo { get; set; }
        public string ToWho { get; set; }
        public string ToAddr { get; set; }
        public double PackageWeight { get; set; }
        public string PostDesc { get; set; }
        public double PostPrice { get; set; }
        public string Create_Who { get; set; }
        public string Create_Time { get; set; }
        public string Update_Time { get; set; }
        public string Create_Name { get; set; }
        public string Update_Who { get; set; }
        public string Update_Name { get; set; }
    }
 
    #endregion
}

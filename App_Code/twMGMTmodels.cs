using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace twMGMTData.Models
{    
    #region *** 管理工作需求 ***
    public class MgHelpData
    {
        public Int64 SeqNo { get; set; }
        public Guid DataID { get; set; }
        public string TraceID { get; set; }
        public Int32 Req_Class { get; set; }
        public string Req_ClassName { get; set; }
        public string Req_Who { get; set; }
        public string Req_WhoName { get; set; }
        public string Req_NickName { get; set; }
        public string Req_Email { get; set; }
        public string Req_TelExt { get; set; }
        public string Req_Dept { get; set; }
        public string Req_DeptName { get; set; }
        public string Help_Subject { get; set; }
        public string Help_Content { get; set; }
        public string Help_Benefit { get; set; }
        public Int32 Help_Status { get; set; }
        public string Help_StatusName { get; set; }
        public Int16 Help_Way { get; set; }
        public string Reply_Content { get; set; }
        public string ProcInfo { get; set; }
        public string onTop { get; set; }
        public string onTopWho { get; set; }
        public string IsRate { get; set; }
        public Int16 RateScore { get; set; }
        public string RateContent { get; set; }
        public string RateWhoName { get; set; }
        public double? Finish_Hours { get; set; }
        public string Finish_Time { get; set; }
        public string Finish_WhoName { get; set; }
        public string CreateDay { get; set; }
        public string Create_Name { get; set; }
        public string Create_Time { get; set; }
        public string Update_Name { get; set; }
        public string Update_Time { get; set; }
        public string Agree_WhoName { get; set; }
        public string Agree_Time { get; set; }
        public string IsAgree { get; set; }
        public string Wish_Time { get; set; }
        public Int32 IsDeptManager { get; set; }
    }


    /// <summary>
    /// 管理工作需求:附件
    /// </summary>
    public class MgHelpAttachment
    {
        public int AttachID { get; set; }
        public string AttachFile { get; set; }
        public string AttachFile_Org { get; set; }
        public string Create_Who { get; set; }

        /// <summary>
        /// 關聯用
        /// </summary>
        public Guid ParentID { get; set; }
        /// <summary>
        /// 關聯用
        /// </summary>
        public int DetailID { get; set; }

    }

    /// <summary>
    /// 管理工作需求:轉寄通知
    /// </summary>
    public class MgHelpCC
    {
        public string CC_Who { get; set; }
        public string CC_Email { get; set; }

    }

    /// <summary>
    /// 管理工作需求:處理進度
    /// </summary>
    public class MgHelpProc
    {
        //public Guid Parent_ID { get; set; }
        public int DetailID { get; set; }
        public int Class_ID { get; set; }
        public string Class_Name { get; set; }
        public string Proc_Desc { get; set; }
        public string Proc_Time { get; set; }
        public string Confirm_Time { get; set; }
        public string Wish_Time { get; set; }
        public string Create_Time { get; set; }
        public string Create_WhoName { get; set; }
    }

    /// <summary>
    /// 管理工作需求:固定收信清單
    /// </summary>
    public class MgHelpReceiver
    {
        public string Email { get; set; }
    }
    #endregion

}
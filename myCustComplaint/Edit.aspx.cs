using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myCustComplaint_Edit : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;
    public bool editAuth = false; //編輯權限(可在權限設定裡勾選)
    public bool closeAuth = false; //作廢權限(可在權限設定裡勾選)

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            /* [取得MenuID, MenuName]
                Req_TypeID -> 取得對應的MenuID, 檢查是否有權限
                Req_TypeID + Req_Lang -> 取得對應的Type Name
            */
            int _ccType = Convert.ToInt32(Req_TypeID);
            var query = fn_Menu.GetOne_RefType(Req_Lang, _ccType, out ErrMsg);
            string menuID = query.MenuID.ToString();
            string typeName = query.Label;

            //取得功能名稱
            lt_TypeName.Text = typeName;
            //設定PageTitle
            Page.Title = typeName + " - " + GetLocalResourceObject("pageTitle").ToString();

            //[權限判斷] Start
            #region --權限--
            /* 
             * 判斷對應的MENU ID
             * 取得其他權限
             */
            bool isPass = false;

            switch (Req_TypeID)
            {
                case "10":
                    //台灣工具
                    editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3234");
                    closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3235");
                    break;

                case "20":
                    //台灣科玩
                    editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3238");
                    closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3239");
                    break;

                case "30":
                    //中國工具
                    editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3242");
                    closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3243");
                    break;

                default:
                    //中國科玩
                    editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3246");
                    closeAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3247");
                    break;
            }

            isPass = editAuth;
            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }


            #endregion
            //[權限判斷] End

            if (!IsPostBack)
            {
                //[產生選單]
                //1:FlowStatus, 2:客戶類別, 3:收貨方式, 4:一線處理方式, 5:二線處理方式, 6:業務確認處理方式, 7:資材處理方式
                Get_ClassList("4", ddl_Flow201_Type, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());
                Get_ClassList("5", ddl_Flow301_Type, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());
                Get_ClassList("6", ddl_Flow401_Type, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());
                Get_ClassList("7", ddl_Flow501_Type, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());


                //載入資料
                LookupData();

            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示:基本資料 --

    /// <summary>
    /// 取得基本資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOneCCP(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }

        //取得狀態,設定判斷參數
        int _flowStatus = query.FlowStatus;  //FlowID
        bool _use201 = _flowStatus.Equals(201);
        bool _use301 = _flowStatus.Equals(301);
        bool _use401 = _flowStatus.Equals(401);
        bool _use501 = _flowStatus.Equals(501);

        //Check:結案或作廢
        if (_flowStatus.Equals(999) | _flowStatus.Equals(998))
        {
            //導至檢視頁
            Response.Redirect(ViewPage);
            return;
        }

        #region >> 欄位填寫 <<

        //--- 填入基本資料 ---
        lt_CCUID.Text = query.CC_UID;   //客訴編號
        lt_Status.Text = query.FlowStatusName;  //流程狀態名稱
        Page.Title += "&nbsp;[{0}]".FormatThis(query.FlowStatusName);

        lt_PlanTypeName.Text = query.PlanTypeName;
        lt_CustTypeName.Text = "{0}：{1}".FormatThis(query.CustTypeName, (query.RefCustName) ?? query.RefMallName);
        lt_ModelNo.Text = query.ModelNo;
        lt_Qty.Text = query.Qty.ToString();
        lt_Remark.Text = query.Remark.Replace("\r", "<br/>");
        if (!string.IsNullOrWhiteSpace(query.Remark_Check))
        {
            lt_Remark_Check.Text = query.Remark_Check.Replace("\r", "<br/>");
        }


        //--- 一線F201 ---
        ddl_Flow201_Type.SelectedValue = query.Flow201_Type.ToString();
        tb_Flow201_Desc.Text = query.Flow201_Desc;
        lt_Flow201_Who.Text = query.Flow201_WhoName ?? "待填寫...";
        lt_Flow201_Time.Text = query.Flow201_Time;
        tb_Remark_Check.Text = query.Remark_Check;
        //判斷是否啟用
        ddl_Flow201_Type.Enabled = _use201;
        tb_Flow201_Desc.Enabled = _use201;
        ph_tip201.Visible = _use201;


        //--- 二線F301 ---
        ddl_Flow301_Type.SelectedValue = query.Flow301_Type.ToString();
        tb_Flow301_Desc.Text = query.Flow301_Desc;
        lt_Flow301_Who.Text = query.Flow301_WhoName ?? "待填寫...";
        lt_Flow301_Time.Text = query.Flow301_Time;
        tb_FixPrice.Text = query.FixPrice.ToString();
        tb_FixWishDate.Text = query.FixWishDate;
        tb_FixOkDate.Text = query.FixOkDate;
        //判斷是否啟用
        ddl_Flow301_Type.Enabled = _use301;
        tb_Flow301_Desc.Enabled = _use301;
        tb_FixPrice.Enabled = _use301;
        tb_FixWishDate.Enabled = _use301;
        tb_FixOkDate.Enabled = _use301;
        ph_tip301.Visible = _use301;


        //--- 業務F401 ---
        ddl_Flow401_Type.SelectedValue = query.Flow401_Type.ToString();
        tb_Flow401_Desc.Text = query.Flow401_Desc;
        lt_Flow401_Who.Text = query.Flow401_WhoName ?? "待填寫...";
        lt_Flow401_Time.Text = query.Flow401_Time;
        tb_FixTotalPrice.Text = query.FixTotalPrice.ToString();

        tb_ERP_No1.Text = query.ERP_No1;
        tb_ERP_No2.Text = query.ERP_No2;
        tb_ERP_No3.Text = query.ERP_No3;
        tb_ERP_No4.Text = query.ERP_No4;
        tb_ERP_No5.Text = query.ERP_No5;
        tb_ERP_No6.Text = query.ERP_No6;
        //判斷是否啟用
        ddl_Flow401_Type.Enabled = _use401;
        tb_Flow401_Desc.Enabled = _use401;
        tb_FixTotalPrice.Enabled = _use401;

        ph_tip401.Visible = _use401;
        tb_ERP_No1.Enabled = _use401;
        tb_ERP_No2.Enabled = _use401;
        tb_ERP_No3.Enabled = _use401;
        tb_ERP_No4.Enabled = _use401;
        tb_ERP_No5.Enabled = _use401;
        tb_ERP_No6.Enabled = _use401;


        //--- 資材F501 ---
        ddl_Flow501_Type.SelectedValue = query.Flow501_Type.ToString();
        tb_Flow501_Desc.Text = query.Flow501_Desc;
        lt_Flow501_Who.Text = query.Flow501_WhoName ?? "待填寫...";
        lt_Flow501_Time.Text = query.Flow501_Time;
        tb_ShipNo.Text = query.ShipNo;
        tb_ShipDate.Text = query.ShipDate;
        tb_ShipComp.Text = query.ShipComp;
        //判斷是否啟用
        ddl_Flow501_Type.Enabled = _use501;
        tb_Flow501_Desc.Enabled = _use501;
        tb_ShipNo.Enabled = _use501;
        tb_ShipDate.Enabled = _use501;
        ph_tip501.Visible = _use501;

        //hidden field
        hf_FlowStatus.Value = _flowStatus.ToString();

        #endregion


        //-- 載入其他資料 --
        LookupData_Files();
        LookupData_FlowLog();
        LookupData_Info(query.TraceID);
        LookupData_Photos(query.TraceID);


        //維護資訊
        info_Creater.Text = query.Create_Name;
        info_CreateTime.Text = query.Create_Time;
        info_Updater.Text = query.Update_Name;
        info_UpdateTime.Text = query.Update_Time;

        //Release
        _data = null;

    }

    #endregion


    #region -- 資料編輯:基本資料 --

    //Save
    protected void btn_doSave_Click(object sender, EventArgs e)
    {
        dataProcess(false);
    }

    //Invoke
    protected void btn_doInvoke_Click(object sender, EventArgs e)
    {
        dataProcess(true);
    }

    /// <summary>
    /// 資料處理
    /// </summary>
    /// <param name="passToNext">false:本頁存檔/true:派送下關</param>
    private void dataProcess(bool passToNext)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        int _flowNow = Convert.ToInt32(hf_FlowStatus.Value);    //此關卡ID
        int _flowNext = Convert.ToInt32(hf_FlowStatus.Value);  //下關關卡ID (預設=此關)
        int _ccType = Convert.ToInt32(Req_TypeID);  //客訴來源
        string _flowNowName = "";
        string _flowNextName = "";
        string _procTypeName = "";
        string _procType = "";
        string _procDesc = "";
        bool _descCheck = true;
        string errTxt = "";


        //----- 檢查:必填欄位 -----
        #region ** 流程判斷 **
        switch (_flowNow)
        {
            //取得各流程必填欄位內容
            case 201:
                _procType = ddl_Flow201_Type.SelectedValue;
                _procDesc = tb_Flow201_Desc.Text;

                break;


            case 301:
                _procType = ddl_Flow301_Type.SelectedValue;
                _procDesc = tb_Flow301_Desc.Text;

                break;


            case 401:
                _procType = ddl_Flow401_Type.SelectedValue;
                _procDesc = tb_Flow401_Desc.Text;

                break;


            case 501:
                _procType = ddl_Flow501_Type.SelectedValue;
                _procDesc = tb_Flow501_Desc.Text;
                _descCheck = false;

                break;

        }
        #endregion

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_procType))
        {
            errTxt += "請選擇「處理方式」\\n";
        }
        if (string.IsNullOrWhiteSpace(_procDesc) && _descCheck)
        {
            errTxt += "請填寫「處理說明」\\n";
        }

        #endregion

        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, thisPage + "#flow" + _flowNow);
            return;
        }

        //----- [派送下關]取得必要資料 -----
        //派送下關的按鈕使用
        if (passToNext)
        {
            //取得下關流程ID(in:處理方式,Lang)
            var _GetInvoke = _data.GetOneCCP_RefClass(_procType, Req_Lang, out ErrMsg).FirstOrDefault();
            if (!string.IsNullOrEmpty(_GetInvoke.Invoke_To.ToString()))
            {
                _flowNext = _GetInvoke.Invoke_To;
            }
            //處理方式名稱
            _procTypeName = _GetInvoke.Label;

            /*[取得Class對應的顯示名稱]*/
            //目前流程名稱
            _flowNowName = _data.GetOneCCP_RefClass(_flowNow.ToString(), Req_Lang, out ErrMsg).FirstOrDefault().Label;
            //下關流程名稱
            _flowNextName = _data.GetOneCCP_RefClass(_flowNext.ToString(), Req_Lang, out ErrMsg).FirstOrDefault().Label;
            //處理方式名稱
            //_procTypeName = _data.GetOneCCP_RefClass(_procType, Req_Lang, out ErrMsg).FirstOrDefault().Label;
        }

        //----- 取得其他欄位 -----
        double _FixPrice = Convert.ToDouble(tb_FixPrice.Text);
        double _FixTotalPrice = Convert.ToDouble(tb_FixTotalPrice.Text);
        string _FixWishDate = tb_FixWishDate.Text.ToDateString("yyyy/MM/dd");
        string _FixOkDate = tb_FixOkDate.Text.ToDateString("yyyy/MM/dd");
        string _ShipDate = tb_ShipDate.Text.ToDateString("yyyy/MM/dd");

        string _ShipComp = tb_ShipComp.Text;
        string _ShipNo = tb_ShipNo.Text;
        string _Remark_Check = tb_Remark_Check.Text;
        string _ERP_No1 = tb_ERP_No1.Text;
        string _ERP_No2 = tb_ERP_No2.Text;
        string _ERP_No3 = tb_ERP_No3.Text;
        string _ERP_No4 = tb_ERP_No4.Text;
        string _ERP_No5 = tb_ERP_No5.Text;
        string _ERP_No6 = tb_ERP_No6.Text;

        //----- 設定:資料欄位 -----
        var dataItem = new CCPItem
        {
            Data_ID = new Guid(Req_DataID),
            FlowStatus = _flowNow,
            inputType = _procType,
            inputDesc = _procDesc,
            nextFlow = _flowNext,
            FixPrice = _FixPrice,
            FixWishDate = _FixWishDate,
            FixOkDate = _FixOkDate,
            FixTotalPrice = _FixTotalPrice,
            ShipComp = _ShipComp,
            ShipNo = _ShipNo,
            ShipDate = _ShipDate,
            Remark_Check = _Remark_Check,
            ERP_No1 = _ERP_No1,
            ERP_No2 = _ERP_No2,
            ERP_No3 = _ERP_No3,
            ERP_No4 = _ERP_No4,
            ERP_No5 = _ERP_No5,
            ERP_No6 = _ERP_No6,
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateCCP_Data(dataItem, out ErrMsg))
        {
            //[System Log] CreateCCP_Log
            var log = new CCPLog
            {
                Parent_ID = new Guid(Req_DataID),
                LogType = 2,
                LogSubject = "資料更新失敗(UpdateCCP_Data)",
                LogDesc = ErrMsg,
                Create_Who = fn_Param.CurrentUser
            };
            _data.CreateCCP_Log(log, out ErrMsg);

            //Show Message
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("資料更新失敗", "");
            return;
        }


        //[Log & Mail]判斷是否派送下關
        if (passToNext)
        {
            //下一關訊息
            string _flowNextMsg = "";
            if (_flowNext.Equals(999))
            {
                //結案
                _flowNextMsg = "<b class=\"red-text text-darken-4\">--- {0} ---</b>".FormatThis(_flowNextName);
            }
            else
            {
                //下一關
                _flowNextMsg = "{0}:<span class=\"green-text text-darken-3\">{1}</u>".FormatThis("下一關", _flowNextName);
            }

            //[Flow Log]
            var log = new CCPLog
            {
                Parent_ID = new Guid(Req_DataID),
                LogType = 1,
                LogSubject = "[{0}]".FormatThis(_flowNowName),
                LogDesc = "<strong>({0})</strong><p>{1}</p><div>{2}</div>".FormatThis(_procTypeName, _procDesc, _flowNextMsg),
                FlowID = _flowNow,
                Create_Who = fn_Param.CurrentUser
            };
            _data.CreateCCP_Log(log, out ErrMsg);


            /*
              [寄通知信]:下一關人員
              - X=結案(通知所有人), B=其他
            */
            if (!_data.doSendInformMail(_flowNext.Equals(999) ? "X" : "B"
                , Req_DataID, _flowNext.ToString(), FuncPath()
                , Req_Lang, Req_TypeID, out ErrMsg))
            {
                //[System Log] CreateCCP_Log
                var logMail = new CCPLog
                {
                    Parent_ID = new Guid(Req_DataID),
                    LogType = 2,
                    LogSubject = "通知信發送失敗, flowNext={0}".FormatThis(_flowNext.ToString()),
                    LogDesc = ErrMsg,
                    Create_Who = fn_Param.CurrentUser
                };
                _data.CreateCCP_Log(logMail, out ErrMsg);

                //Show Message
                CustomExtension.AlertMsg("關卡已派送，但通知信發送失敗.", "");
            }

        }


        //release 
        _data = null;


        //[導向]判斷是否派送下關
        if (passToNext)
        {

            //導向列表頁
            Response.Redirect(Page_SearchUrl);
        }
        else
        {
            //導向本頁
            Response.Redirect(thisPage + "#flow" + _flowNow);
        }

    }

    #endregion


    #region -- 資料顯示:檔案附件 --

    /// <summary>
    /// 顯示檔案附件
    /// </summary>
    private void LookupData_Files()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPFileList(Req_DataID, Req_Lang);

        //----- 資料整理:繫結 ----- 
        lv_Attachment.DataSource = query;
        lv_Attachment.DataBind();

        //Release
        query = null;
        _data = null;
    }

    protected void lv_Attachment_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //----- 宣告:資料參數 -----
            Menu3000Repository _data = new Menu3000Repository();

            try
            {
                //取得Key值
                string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;
                string Get_FileName = ((HiddenField)e.Item.FindControl("hf_FileName")).Value;

                //----- 設定:資料欄位 -----
                var data = new CCPAttachment
                {
                    Parent_ID = new Guid(Req_DataID),
                    Data_ID = Convert.ToInt32(Get_DataID)
                };

                //----- 方法:刪除資料 -----
                if (false == _data.Delete_CCPFiles(data))
                {
                    CustomExtension.AlertMsg("檔案刪除失敗", "");
                    return;
                }
                else
                {
                    //刪除檔案
                    string ftpFolder = UploadFolder + Req_DataID;
                    _ftp.FTP_DelFile(ftpFolder, Get_FileName);

                    //導向本頁
                    Response.Redirect(thisPage + "#attfiles");
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                _data = null;
            }

        }
    }

    #endregion


    #region -- 資料顯示:客服收貨資料 --

    /// <summary>
    /// 顯示客服收貨資料
    /// </summary>
    /// <param name="traceID"></param>
    private void LookupData_Info(string traceID)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("TraceID", traceID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOneCCPTemp(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            return;
        }

        #region >> 欄位填寫 <<

        //--- 填入客服資料 ---
        lb_CustType.Text = query.CustTypeName;
        //-客戶選單
        string _refCustID = query.RefCustID;
        string _refCustName = query.RefCustName;
        if (!string.IsNullOrWhiteSpace(_refCustID))
        {
            lb_CustName.Text = _refCustName;
        }

        lb_Mall.Text = query.RefMallName ?? "&nbsp;";
        lb_CustInput.Text = string.IsNullOrWhiteSpace(query.CustInput) ? "&nbsp;" : query.CustInput;
        lb_BuyerName.Text = query.BuyerName ?? "&nbsp;";
        lb_BuyerPhone.Text = query.BuyerPhone ?? "&nbsp;";
        lb_BuyerAddr.Text = query.BuyerAddr ?? "&nbsp;";
        lt_CS_Who.Text = query.CS_Name;
        lt_CS_Time.Text = query.CS_Time;
        lb_PlanType.Text = query.PlanTypeName.ToString();
        lb_InvoiceIsBack.Text = query.InvoiceIsBack.ToString();
        lb_ERP_ID.Text = query.ERP_ID;
        lb_Platform_ID.Text = query.Platform_ID;


        //--- 填入收貨資料 ---
        lb_FreightType.Text = query.FreightTypeName ?? "&nbsp;";
        lb_FreightInput.Text = string.IsNullOrWhiteSpace(query.FreightInput) ? "&nbsp;" : query.FreightInput;
        lb_FreightGetDate.Text = string.IsNullOrWhiteSpace(query.FreightGetDate) ? "&nbsp;" : query.FreightGetDate;
        lb_InvoiceNumber.Text = string.IsNullOrWhiteSpace(query.InvoiceNumber) ? "&nbsp;" : query.InvoiceNumber;
        lb_InvoicePrice.Text = query.InvoicePrice.ToString();
        lb_ShipComp.Text = string.IsNullOrWhiteSpace(query.ShipComp) ? "&nbsp;" : query.ShipComp;
        lb_ShipWho.Text = string.IsNullOrWhiteSpace(query.ShipWho) ? "&nbsp;" : query.ShipWho;
        lb_ShipTel.Text = string.IsNullOrWhiteSpace(query.ShipTel) ? "&nbsp;" : query.ShipTel;
        lb_ShipAddr.Text = string.IsNullOrWhiteSpace(query.ShipAddr) ? "&nbsp;" : query.ShipAddr;
        lt_Freight_Who.Text = query.Freight_Name;
        lt_Freight_Time.Text = query.Freight_Time;

        #endregion

        //Release
        _data = null;

    }



    /// <summary>
    /// 顯示收貨圖片
    /// </summary>
    /// <param name="traceID"></param>
    private void LookupData_Photos(string traceID)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("TraceID", traceID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPTempFileList(search);

        //----- 資料整理:繫結 ----- 
        lv_Photos.DataSource = query;
        lv_Photos.DataBind();

        //Release
        query = null;
        _data = null;
    }
    #endregion


    #region -- 資料顯示:流程記錄 --

    /// <summary>
    /// 顯示流程記錄
    /// </summary>
    private void LookupData_FlowLog()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        //1=UserFlow
        var query = _data.GetCCPLogList(Req_DataID, 1);

        //----- 資料整理:繫結 ----- 
        lv_Timeline.DataSource = query;
        lv_Timeline.DataBind();

        //Release
        query = null;
        _data = null;
    }

    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// 上傳附件
    /// </summary>
    protected void btn_SaveDetail_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        #region ** 檔案上傳判斷 **

        //宣告
        List<IOTempParam> fileList = new List<IOTempParam>();
        Random rnd = new Random();

        int GetFileCnt = 0;

        //取得上傳檔案集合
        HttpFileCollection hfc = Request.Files;

        //--- 限制上傳數量 ---
        for (int idx = 0; idx <= hfc.Count - 1; idx++)
        {
            //取得個別檔案
            HttpPostedFile hpf = hfc[idx];

            if (hpf.ContentLength > 0)
            {
                GetFileCnt++;
            }
        }
        if (GetFileCnt > FileCountLimit)
        {
            //[提示]
            errTxt += "單次上傳檔案數超出限制, 每次上傳僅限 {0} 個檔案.\\n".FormatThis(FileCountLimit);
        }


        //--- 檔案檢查 ---
        for (int idx = 0; idx <= hfc.Count - 1; idx++)
        {
            //取得個別檔案
            HttpPostedFile hpf = hfc[idx];

            if (hpf.ContentLength > FileSizeLimit)
            {
                //[提示]
                errTxt += "大小超出限制, 每個檔案限制為 {0} MB\\n".FormatThis(FileSizeLimit / 1024000);
            }

            if (hpf.ContentLength > 0)
            {
                //取得原始檔名
                string OrgFileName = Path.GetFileName(hpf.FileName);
                //取得副檔名
                string FileExt = Path.GetExtension(OrgFileName).ToLower();
                if (false == CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //[提示]
                    errTxt += "「{0}」副檔名不符規定, 僅可上傳「{1}」\\n".FormatThis(OrgFileName, FileExtLimit.Replace("|", ", "));
                }
            }
        }


        //--- 檔案暫存List ---
        for (int idx = 0; idx <= hfc.Count - 1; idx++)
        {
            //取得個別檔案
            HttpPostedFile hpf = hfc[idx];

            if (hpf.ContentLength > 0)
            {
                //取得原始檔名
                string OrgFileName = Path.GetFileName(hpf.FileName);
                //取得副檔名
                string FileExt = Path.GetExtension(OrgFileName).ToLower();

                //設定檔名, 重新命名
                string myFullFile = String.Format(@"{0:yyMMddHHmmssfff}{1}{2}"
                    , DateTime.Now
                    , rnd.Next(0, 99)
                    , FileExt);


                //判斷副檔名, 未符合規格的檔案不上傳
                if (CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //設定暫存-檔案
                    fileList.Add(new IOTempParam(myFullFile, OrgFileName, hpf));
                }
            }
        }

        #endregion

        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        #region ** 儲存檔案 **

        /* 注意檔案路徑及資料夾(站台資料夾+guid) */
        if (fileList.Count > 0)
        {
            int errCnt = 0;
            string ftpFolder = UploadFolder + Req_DataID; //ftp資料夾(站台資料夾+guid)

            //判斷資料夾, 不存在則建立
            _ftp.FTP_CheckFolder(ftpFolder);

            //暫存檔案List
            for (int row = 0; row < fileList.Count; row++)
            {
                //取得個別檔案
                HttpPostedFile hpf = fileList[row].Param_hpf;

                //執行上傳
                if (false == _ftp.FTP_doUpload(hpf, ftpFolder, fileList[row].Param_FileName))
                {
                    errCnt++;
                }
            }

            if (errCnt > 0)
            {
                ph_ErrMessage.Visible = true;
                lt_ShowMsg.Text = "檔案上傳失敗, 失敗筆數為 {0} 筆, 請重新整理後再上傳!".FormatThis(errCnt);
                return;
            }
        }

        #endregion


        #region ** 資料處理:檔案附件 **

        if (fileList.Count > 0)
        {
            //----- 宣告:資料參數 -----
            Menu3000Repository _data = new Menu3000Repository();
            List<CCPAttachment> dataItems = new List<CCPAttachment>();

            try
            {
                //----- 設定:資料欄位 -----
                for (int row = 0; row < fileList.Count; row++)
                {
                    var dataItem = new CCPAttachment
                    {
                        Parent_ID = new Guid(Req_DataID),
                        FlowID = Convert.ToInt32(hf_FlowStatus.Value),
                        AttachFile = fileList[row].Param_FileName,
                        AttachFile_Org = fileList[row].Param_OrgFileName,
                        Create_Who = fn_Param.CurrentUser
                    };

                    dataItems.Add(dataItem);
                }

                //----- 方法:更新資料 -----
                if (false == _data.CreateCCP_Attachment(dataItems, out ErrMsg))
                {
                    CustomExtension.AlertMsg("新增檔案失敗", "");
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                _data = null;
                dataItems = null;
            }

        }

        #endregion


        //導向本頁
        Response.Redirect(thisPage + "#attfiles");

    }
    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 取得商城資料 
    /// </summary>
    /// <param name="typeID">typeID</param>
    /// <param name="ddl">下拉選單object</param>
    /// <param name="rootName">第一選項顯示名稱</param>
    private void Get_MallList(string typeID, DropDownList ddl, string rootName)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_RefMall(typeID, Req_Lang, out ErrMsg);

        //----- 資料整理 -----
        ddl.Items.Clear();

        if (!string.IsNullOrEmpty(rootName))
        {
            ddl.Items.Add(new ListItem(rootName, ""));
        }

        foreach (var item in query)
        {
            ddl.Items.Add(new ListItem(item.Label, item.ID.ToString()));
        }

        //release
        query = null;
        _data = null;
    }


    /// <summary>
    /// 取得類別資料 
    /// </summary>
    /// <param name="typeID">Class Type</param>
    /// <param name="ddl">下拉選單object</param>
    /// <param name="rootName">第一選項顯示名稱</param>
    private void Get_ClassList(string typeID, DropDownList ddl, int _ccType, string rootName)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_RefClass(typeID, Req_Lang, _ccType, out ErrMsg);

        //----- 資料整理 -----
        ddl.Items.Clear();

        if (!string.IsNullOrEmpty(rootName))
        {
            ddl.Items.Add(new ListItem(rootName, ""));
        }

        foreach (var item in query)
        {
            ddl.Items.Add(new ListItem(item.Label, item.ID.ToString()));
        }

        //release
        query = null;
        _data = null;
    }

    #endregion


    #region -- 網址參數 --

    /// <summary>
    /// 取得網址參數 - 語系
    /// </summary>
    public string Req_Lang
    {
        get
        {
            string myLang = Page.RouteData.Values["lang"] == null ? "auto" : Page.RouteData.Values["lang"].ToString();

            //若為auto, 就去抓cookie
            return myLang.Equals("auto") ? fn_Language.Get_Lang(Request.Cookies["PKHome_Lang"].Value) : myLang;
        }
        set
        {
            _Req_Lang = value;
        }
    }
    private string _Req_Lang;


    /// <summary>
    /// 取得網址參數 - RootID
    /// </summary>
    private string _Req_RootID;
    public string Req_RootID
    {
        get
        {
            String DataID = Page.RouteData.Values["rootID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
        }
        set
        {
            _Req_RootID = value;
        }
    }

    /// <summary>
    /// 取得網址參數 - TypeID
    /// </summary>
    private string _Req_TypeID;
    public string Req_TypeID
    {
        get
        {
            String DataID = Page.RouteData.Values["typeID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
        }
        set
        {
            _Req_TypeID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/CustComplaint/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_TypeID);
    }

    #endregion


    #region -- 傳遞參數 --

    /// <summary>
    /// 取得傳遞參數 - 資料編號
    /// </summary>
    private string _Req_DataID;
    public string Req_DataID
    {
        get
        {
            String DataID = Page.RouteData.Values["id"].ToString();

            return DataID;
        }
        set
        {
            _Req_DataID = value;
        }
    }


    /// <summary>
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/Edit/{1}".FormatThis(FuncPath(), Req_DataID);
        }
        set
        {
            _thisPage = value;
        }
    }


    /// <summary>
    /// View網址
    /// </summary>
    private string _ViewPage;
    public string ViewPage
    {
        get
        {
            return "{0}/View/{1}".FormatThis(FuncPath(), Req_DataID);
        }
        set
        {
            _ViewPage = value;
        }
    }


    /// <summary>
    /// 設定參數 - 列表頁Url
    /// </summary>
    private string _Page_SearchUrl;
    public string Page_SearchUrl
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("HomeList_CCPsetting");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion


    #region -- 上傳參數 --
    /// <summary>
    /// 限制上傳的副檔名
    /// </summary>
    private string _FileExtLimit;
    public string FileExtLimit
    {
        get
        {
            return "jpg|png|pdf|docx|xlsx|pptx|rar|zip";
        }
        set
        {
            _FileExtLimit = value;
        }
    }

    /// <summary>
    /// 限制上傳的檔案大小(1MB = 1024000), 50MB
    /// </summary>
    private int _FileSizeLimit;
    public int FileSizeLimit
    {
        get
        {
            return 51200000;
        }
        set
        {
            _FileSizeLimit = value;
        }
    }

    /// <summary>
    /// 限制單次上傳檔案數
    /// </summary>
    private int _FileCountLimit;
    public int FileCountLimit
    {
        get
        {
            return 5;
        }
        set
        {
            _FileCountLimit = value;
        }
    }

    /// <summary>
    /// 上傳根目錄
    /// </summary>
    private string _UploadFolder;
    public string UploadFolder
    {
        get
        {
            return "{0}CustComplaint/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
        }
        set
        {
            _UploadFolder = value;
        }
    }

    /// <summary>
    /// 暫存參數
    /// </summary>
    public class IOTempParam
    {
        /// <summary>
        /// [參數] - 檔名
        /// </summary>
        private string _Param_FileName;
        public string Param_FileName
        {
            get { return _Param_FileName; }
            set { _Param_FileName = value; }
        }

        /// <summary>
        /// [參數] -原始檔名
        /// </summary>
        private string _Param_OrgFileName;
        public string Param_OrgFileName
        {
            get { return _Param_OrgFileName; }
            set { _Param_OrgFileName = value; }
        }


        private HttpPostedFile _Param_hpf;
        public HttpPostedFile Param_hpf
        {
            get { return _Param_hpf; }
            set { _Param_hpf = value; }
        }

        /// <summary>
        /// 設定參數值
        /// </summary>
        /// <param name="Param_FileName">系統檔名</param>
        /// <param name="Param_OrgFileName">原始檔名</param>
        /// <param name="Param_hpf">上傳檔案</param>
        /// <param name="Param_FileKind">檔案類別</param>
        public IOTempParam(string Param_FileName, string Param_OrgFileName, HttpPostedFile Param_hpf)
        {
            _Param_FileName = Param_FileName;
            _Param_OrgFileName = Param_OrgFileName;
            _Param_hpf = Param_hpf;
        }

    }
    #endregion


}
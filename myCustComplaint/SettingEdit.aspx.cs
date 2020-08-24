using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LinqToExcel;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myCustComplaint_SettingEdit : SecurityCheck
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
                Get_MallList(_ccType.ToString(), ddl_Mall, GetLocalResourceObject("ddl_請選擇").ToString());
                Get_ClassList("2", ddl_CustType, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());
                Get_ClassList("3", ddl_FreightType, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());
                Get_ClassList("11", ddl_PlanType, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());
                Get_ClassList("12", ddl_BadReason, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());
                

                //    /* 多語系設定 */
                //    lt_TraceID.Text = GetLocalResourceObject("txt_系統自動產生").ToString();
                //    tb_ReplyContent.Attributes.Add("placeholder", GetLocalResourceObject("tip8").ToString());


                //[參數判斷] - 資料編號
                if (Req_DataID.Equals("new"))
                {
                    ph_LockModal.Visible = true;
                    ph_LockScript.Visible = true;
                }
                else
                {
                    //載入資料
                    LookupData();

                }


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
        var query = _data.GetOneCCPTemp(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }

        int _DTCnt = query.DtCnt;    //商品填寫數
        string _IsCS = query.IsCS;   //客服填寫
        string _IsFreight = query.IsFreight; //收貨填寫


        #region >> 欄位填寫 <<

        //--- 填入基本資料 ---
        lt_TraceID.Text = query.TraceID;
        lt_CreateDate.Text = query.Create_Time.ToDateString("yyyy/MM/dd");


        //--- 填入客服資料 ---
        ddl_PlanType.SelectedValue = query.PlanType.ToString();
        ddl_BadReason.SelectedValue = query.BadReason.ToString();
        ddl_InvoiceIsBack.SelectedValue = query.InvoiceIsBack.ToString();
        ddl_CustType.SelectedValue = query.CustType.ToString(); //-客戶類別
        //-客戶選單
        string _refCustID = query.RefCustID;
        string _refCustName = query.RefCustName;
        if (!string.IsNullOrWhiteSpace(_refCustID))
        {
            tb_Cust.Text = _refCustID;
            val_CustID.Text = _refCustID;
            lb_CustName.Text = _refCustName;
        }

        ddl_Mall.SelectedValue = query.RefMallID.ToString();  //-商城選單
        tb_CustInput.Text = query.CustInput;
        tb_BuyerName.Text = query.BuyerName;
        tb_BuyerPhone.Text = query.BuyerPhone;
        tb_BuyerAddr.Text = query.BuyerAddr;
        tb_ERP_ID.Text = query.ERP_ID;
        tb_Platform_ID.Text = query.Platform_ID;
        lt_CS_Who.Text = query.CS_Name ?? "待填寫...";
        lt_CS_Time.Text = query.CS_Time;


        //--- 填入收貨資料 ---
        ddl_FreightType.SelectedValue = query.FreightType.ToString();   //-收貨方式
        tb_FreightInput.Text = query.FreightInput;
        tb_FreightGetDate.Text = query.FreightGetDate;
        tb_InvoiceNumber.Text = query.InvoiceNumber;
        tb_InvoicePrice.Text = query.InvoicePrice.ToString();
        tb_ShipComp.Text = query.ShipComp;
        tb_ShipWho.Text = query.ShipWho;
        tb_ShipTel.Text = query.ShipTel;
        tb_ShipAddr.Text = query.ShipAddr;
        lt_Freight_Who.Text = query.Freight_Name ?? "待填寫...";
        lt_Freight_Time.Text = query.Freight_Time;

        #endregion


        #region >> 欄位顯示控制 <<

        ph_ProdCnt.Visible = _DTCnt.Equals(0);
        ph_CS.Visible = _IsCS.Equals("N");
        ph_Freight.Visible = _IsFreight.Equals("N");
        ph_Status.Visible = (_IsCS.Equals("Y") && _IsFreight.Equals("Y") && _DTCnt > 0);

        //-按鈕
        ph_Invoke.Visible = (_DTCnt > 0 & _IsCS.Equals("Y") & _IsFreight.Equals("Y")); //確認開案(符合三項條件後顯示)

        #endregion


        #region >> 其他功能 <<

        //-- 載入其他資料 --
        LookupData_Detail();
        LookupData_Files();

        #endregion


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
    /// <summary>
    /// 執行自動新增基本資料
    /// </summary>
    protected void btn_NewJob_Click(object sender, EventArgs e)
    {
        AutoAdd_Data();
    }

    /// <summary>
    /// 自動新增基本資料(Modal視窗按下確認後)
    /// </summary>
    private void AutoAdd_Data()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 設定:資料欄位 -----
        //產生Guid
        string guid = CustomExtension.GetGuid();
        string traceID = GetNewTraceID();

        var data = new CCPTempItem
        {
            Data_ID = new Guid(guid),
            CC_Type = Convert.ToInt16(Req_TypeID),
            TraceID = traceID,
            Create_Who = fn_Param.CurrentUser
        };

        //----- 方法:新增資料 -----
        if (!_data.CreateCCP_Temp(data, out ErrMsg))
        {
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("新增失敗", "");
            return;
        }

        //release
        _data = null;

        //更新本頁Url
        string thisUrl = "{0}/SetEdit/{1}".FormatThis(FuncPath(), guid);

        //導向本頁
        Response.Redirect(thisUrl);
    }

    /// <summary>
    /// 產生追蹤碼
    /// </summary>
    /// <returns></returns>
    private string GetNewTraceID()
    {
        //產生TraceID
        long ts = Cryptograph.GetCurrentTime();

        Random rnd = new Random();
        int myRnd = rnd.Next(1, 99);

        return "{0}{1}".FormatThis(ts, myRnd);
    }


    //客服資料-SAVE
    protected void btn_doSave101_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 檢查:必填欄位 -----
        string _planType = ddl_PlanType.SelectedValue;
        string _badReason = ddl_BadReason.SelectedValue;
        string _custType = ddl_CustType.SelectedValue;
        string _custID = val_CustID.Text;
        string _mallID = ddl_Mall.SelectedValue;
        string _buyerName = tb_BuyerName.Text;
        string _buyerPhone = tb_BuyerPhone.Text;
        string _buyerAddr = tb_BuyerAddr.Text;
        string _remark = tb_CustInput.Text;
        string _invoiceIsBack = ddl_InvoiceIsBack.SelectedValue;
        string _platform_ID = tb_Platform_ID.Text;
        string _ERP_ID = tb_ERP_ID.Text;

        string errTxt = "";

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_planType))
        {
            errTxt += "請選擇「計劃處理方式」\\n";
        }
        if (string.IsNullOrWhiteSpace(_badReason))
        {
            errTxt += "請選擇「不良原因」\\n";
        }
        if (string.IsNullOrWhiteSpace(_custType))
        {
            errTxt += "請選擇「客戶類別」\\n";
        }
        if (string.IsNullOrWhiteSpace(_buyerName))
        {
            errTxt += "請填寫「聯絡人」\\n";
        }
        if (string.IsNullOrWhiteSpace(_buyerPhone))
        {
            errTxt += "請填寫「聯絡電話」\\n";
        }
        if (string.IsNullOrWhiteSpace(_buyerAddr))
        {
            errTxt += "請填寫「聯絡地址」\\n";
        }

        #endregion
        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        //----- 設定:資料欄位 -----
        var dataItem = new CCPTempItem
        {
            Data_ID = new Guid(Req_DataID),
            PlanType = Convert.ToInt32(_planType),
            BadReason = Convert.ToInt32(_badReason),
            CustType = Convert.ToInt32(_custType),
            RefCustID = _custID,
            RefMallID = string.IsNullOrWhiteSpace(_mallID) ? 0 : Convert.ToInt32(_mallID),
            CustInput = _remark,
            BuyerName = _buyerName,
            BuyerPhone = _buyerPhone,
            BuyerAddr = _buyerAddr,
            InvoiceIsBack = _invoiceIsBack,
            Platform_ID = _platform_ID,
            ERP_ID = _ERP_ID,
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateCCP_Temp(dataItem, "1", out ErrMsg))
        {
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("資料更新失敗", "");
            return;
        }

        //release 
        _data = null;

        //導向本頁
        Response.Redirect(thisPage + "#flow101");
    }

    //收貨資料-SAVE
    protected void btn_doSave102_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 檢查:必填欄位 -----
        string _freightType = ddl_FreightType.SelectedValue;
        string _freightGetDate = tb_FreightGetDate.Text;
        string _invoiceNumber = tb_InvoiceNumber.Text;
        string _remark = tb_FreightInput.Text;
        string _shipComp = tb_ShipComp.Text;
        string _shipWho = tb_ShipWho.Text.Trim();
        string _shipTel = tb_ShipTel.Text.Trim();
        string _shipAddr = tb_ShipAddr.Text.Trim();
        string _invoicePrice = tb_InvoicePrice.Text;
        string errTxt = "";

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_freightType))
        {
            errTxt += "請選擇「收貨方式」\\n";
        }
        if (string.IsNullOrWhiteSpace(_freightGetDate))
        {
            errTxt += "請填寫「收貨時間」\\n";
        }

        #endregion
        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        //----- 設定:資料欄位 -----
        var dataItem = new CCPTempItem
        {
            Data_ID = new Guid(Req_DataID),
            FreightType = Convert.ToInt32(_freightType),
            FreightInput = _remark,
            FreightGetDate = _freightGetDate.ToDateString("yyyy/MM/dd HH:mm"),
            InvoiceNumber = _invoiceNumber,
            InvoicePrice = Convert.ToDouble(_invoicePrice),
            ShipComp = _shipComp,
            ShipWho = _shipWho,
            ShipTel = _shipTel,
            ShipAddr = _shipAddr,
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateCCP_Temp(dataItem, "2", out ErrMsg))
        {
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("資料更新失敗", "");
            return;
        }

        //release 
        _data = null;

        //導向本頁
        Response.Redirect(thisPage + "#flow102");
    }

    #endregion


    #region -- 資料顯示:商品資料 --

    /// <summary>
    /// 顯示商品資料
    /// </summary>
    private void LookupData_Detail()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_Detail(Req_DataID, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        lv_Detail.DataSource = query;
        lv_Detail.DataBind();

        //Release
        _data = null;
        query = null;
    }

    protected void lv_Detail_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            /* 刪除商品資料 */
            //取得Key值
            string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

            //----- 宣告:資料參數 -----
            Menu3000Repository _data = new Menu3000Repository();

            //Set Instance
            var data = new CCPDetail
            {
                Parent_ID = new Guid(Req_DataID),
                Data_ID = Convert.ToInt64(Get_DataID),
                Create_Who = fn_Param.CurrentUser
            };

            //----- 方法:刪除資料 -----
            if (false == _data.Delete_CCPDetailData(data))
            {
                CustomExtension.AlertMsg("資料刪除失敗", "");
                return;
            }

            //Release
            _data = null;

            //導向本頁
            Response.Redirect(thisPage + "#prodData");

        }
    }

    #endregion


    #region -- 資料顯示:收貨圖片 --

    /// <summary>
    /// 顯示收貨圖片
    /// </summary>
    private void LookupData_Files()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPTempFileList(search);

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
                if (false == _data.Delete_CCPTempFiles(data))
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


    #region -- 資料編輯:商品資料 --

    /// <summary>
    /// 匯入商品
    /// </summary>
    protected void btn_Import_Click(object sender, EventArgs e)
    {
        #region -- 檔案處理 --

        //宣告
        List<IOTempParam> ITempList = new List<IOTempParam>();
        Random rnd = new Random();
        string Message = "";
        string ftpFolder = UploadFolder + Req_DataID + "/"; //FTP資料夾
        string thisFileName = ""; //檔名
        string thisUrl = thisPage + "#prodData";

        if (fu_File.PostedFile.ContentLength == 0)
        {
            CustomExtension.AlertMsg("請選擇要上傳的檔案", thisUrl);
            return;
        }

        //取得上傳檔案集合
        HttpFileCollection hfc = Request.Files;

        //--- 檔案檢查 ---
        for (int idx = 0; idx <= hfc.Count - 1; idx++)
        {
            //取得個別檔案
            HttpPostedFile hpf = hfc[idx];

            if (hpf.ContentLength > FileSizeLimit)
            {
                //[提示]
                Message = "檔案大小超出限制, 每個檔案大小限制為 {0} MB".FormatThis(FileSizeLimit);
                CustomExtension.AlertMsg(Message, thisUrl);
                return;
            }

            if (hpf.ContentLength > 0)
            {
                //取得原始檔名
                string OrgFileName = System.IO.Path.GetFileName(hpf.FileName);
                //取得副檔名
                string FileExt = System.IO.Path.GetExtension(OrgFileName).ToLower();
                if (false == CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //[提示]
                    Message = "檔案副檔名不符規定, 僅可上傳副檔名為 {0}".FormatThis(FileExtLimit.Replace("|", ", "));
                    CustomExtension.AlertMsg(Message, thisUrl);
                    return;
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
                string OrgFileName = System.IO.Path.GetFileName(hpf.FileName);
                //取得副檔名
                string FileExt = System.IO.Path.GetExtension(OrgFileName).ToLower();

                //設定檔名, 重新命名
                string myFullFile = String.Format(@"{0:yyMMddHHmmssfff}{1}{2}"
                    , DateTime.Now
                    , rnd.Next(0, 99)
                    , FileExt);

                //暫存檔名
                thisFileName = myFullFile;

                //判斷副檔名, 未符合規格的檔案不上傳
                if (CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //設定暫存-檔案
                    ITempList.Add(new IOTempParam(myFullFile, OrgFileName, hpf));
                }
            }
        }

        #endregion

        //Check Null
        if (ITempList.Count == 0)
        {
            CustomExtension.AlertMsg("請選擇要上傳的檔案", thisUrl);
            return;
        }

        #region -- 儲存檔案 --

        int errCnt = 0;

        //判斷資料夾, 不存在則建立
        _ftp.FTP_CheckFolder(ftpFolder);

        //暫存檔案List
        for (int row = 0; row < ITempList.Count; row++)
        {
            //取得個別檔案
            HttpPostedFile hpf = ITempList[row].Param_hpf;

            //執行上傳
            if (false == _ftp.FTP_doUpload(hpf, ftpFolder, ITempList[row].Param_FileName))
            {
                errCnt++;
            }
        }

        if (errCnt > 0)
        {
            Message = "檔案上傳失敗, 失敗筆數為 {0} 筆, 請重新整理後再上傳!".FormatThis(errCnt);
            CustomExtension.AlertMsg(Message, thisUrl);
            return;
        }

        #endregion


        #region -- 資料處理 --

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //設定完整路徑
        string _filePath = @"{0}{1}{2}".FormatThis(
            System.Web.Configuration.WebConfigurationManager.AppSettings["FTP_DiskUrl"]
            , ftpFolder.Replace("/", "\\")
            , thisFileName);

        //查詢Excel
        var excelFile = new ExcelQueryFactory(_filePath);

        //取得Excel 第一個頁籤名稱
        var sheetData = excelFile.GetWorksheetNames().FirstOrDefault();

        //取得Excel資料欄位
        var query_Xls = _data.GetCCP_ExcelData(_filePath, sheetData);

        try
        {
            //儲存資料
            if (!_data.CreateCCP_Detail(query_Xls, Req_DataID, fn_Param.CurrentUser, out ErrMsg))
            {
                CustomExtension.AlertMsg("資料匯入失敗", thisUrl);
                return;
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //刪除檔案
            _ftp.FTP_DelFile(ftpFolder, thisFileName);
            _data = null;
        }
        #endregion


        //Redirect
        Response.Redirect(thisUrl);
    }

    /// <summary>
    /// 新增商品
    /// </summary>
    protected void btn_SaveDetail_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 設定:資料欄位 -----
        string _modelNo = val_ModelNo.Text;
        string _qty = tb_Qty.Text;
        string _remark = HttpUtility.HtmlEncode(tb_Remark.Text);
        string _isSplit = ddl_IsSplit.SelectedValue;
        string _isWarranty = ddl_IsWarranty.SelectedValue;
        string errTxt = "";

        #region ** 基本欄位判斷 **

        if (string.IsNullOrWhiteSpace(_modelNo))
        {
            errTxt += "請選擇正確的「品號」\\n";
        }
        if (string.IsNullOrWhiteSpace(_qty))
        {
            errTxt += "請填寫「數量」\\n";
        }
        if (string.IsNullOrWhiteSpace(_remark))
        {
            errTxt += "請填寫「客訴內容」\\n";
        }

        #endregion

        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        //Set Instance
        var data = new CCPDetail
        {
            Parent_ID = new Guid(Req_DataID),
            ModelNo = _modelNo,
            Qty = Convert.ToInt32(_qty),
            Remark = _remark,
            IsSplit = _isSplit,
            IsWarranty = _isWarranty,
            Create_Who = fn_Param.CurrentUser
        };

        //----- 方法:新增資料 -----
        if (!_data.CreateCCP_Detail(data, out ErrMsg))
        {
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("新增失敗", "");
            return;
        }
        else
        {
            //導向本頁
            Response.Redirect(thisPage + "#prodData");
        }
    }

    #endregion


    #region -- 按鈕事件 --
    /// <summary>
    /// 開案
    /// </summary>
    protected void btn_Invoke_Click(object sender, EventArgs e)
    {
        /*
          1. 拆分商品資料, 複製其他欄位至客訴資料
          2. Update Invoke = 'Y', Invoke_Who/Invoke_Time
          3. Log Create
          4. 通知下一關人員Cust_Complaint_InformFlow(FlowID=201)
        */

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        List<CCPItem> dataList = new List<CCPItem>(); //資料容器

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_InvokeData(Req_DataID, out ErrMsg);
        var baseData = query.FirstOrDefault();
        string _traceID = baseData.TraceID;
        string _planType = baseData.PlanType.ToString();
        string _platformID = baseData.RefPlatform_ID;
        string _mallID = baseData.RefMallID.ToString();
        string _NextFlowID = "201";  //下一關預設FLOW
        int _CCType = Convert.ToInt16(Req_TypeID);
        /*
          [Check] CN需求: PlanType = 52(退貨), MallID <> '' => 直接派至401
        */
        switch (_CCType)
        {
            case 30:
            case 40:
                if (_planType.Equals("52") && !string.IsNullOrWhiteSpace(_platformID) && !string.IsNullOrWhiteSpace(_mallID))
                {
                    _NextFlowID = "401";
                }

                break;

            default:
                break;
        }

        //取得要拆的資料
        var dataSplit_Y = query.Where(fld => fld.IsSplit.Equals("Y"));

        #region >> 拆分數量 <<
        foreach (var item in dataSplit_Y)
        {
            //取得數量
            int loop = item.Qty;

            //依數量複製資料,產生新客訴編號
            for (int row = 1; row <= loop; row++)
            {
                //加入項目
                var data = new CCPItem
                {
                    TraceID = item.TraceID,
                    PlanType = item.PlanType,
                    CustType = item.CustType,
                    RefCustID = item.RefCustID,
                    RefMallID = item.RefMallID,
                    ModelNo = item.ModelNo,
                    Qty = 1,
                    Remark = item.Remark,
                    IsWarranty = item.IsWarranty,
                    Create_Who = fn_Param.CurrentUser
                };


                //將項目加入至集合
                dataList.Add(data);
            }
        }
        #endregion


        //取得不拆的資料
        var dataSplit_N = query.Where(fld => fld.IsSplit.Equals("N"));

        #region >> 不拆數量 <<
        foreach (var item in dataSplit_N)
        {
            //加入項目
            var data = new CCPItem
            {
                TraceID = item.TraceID,
                PlanType = item.PlanType,
                CustType = item.CustType,
                RefCustID = item.RefCustID,
                RefMallID = item.RefMallID,
                ModelNo = item.ModelNo,
                Qty = item.Qty,
                Remark = item.Remark,
                IsWarranty = item.IsWarranty,
                Create_Who = fn_Param.CurrentUser
            };


            //將項目加入至集合
            dataList.Add(data);
        }

        #endregion


        //----- 方法:新增資料 -----
        //PS:[UserFLow]開案LOG已在Create時記錄
        if (!_data.CreateCCP_Invoke(dataList, Req_DataID, Req_TypeID, _NextFlowID, out ErrMsg))
        {
            //[System Log] CreateCCP_Log
            var log = new CCPLog
            {
                Parent_ID = new Guid(Req_DataID),
                LogType = 2,
                LogSubject = "開案失敗(CreateCCP_Invoke)," + _traceID,
                LogDesc = ErrMsg,
                Create_Who = fn_Param.CurrentUser
            };
            _data.CreateCCP_Log(log, out ErrMsg);

            //Show Message
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("開案失敗", thisPage);
            return;
        }


        /* [寄通知信]:下一關人員(201)
           - A=開案, 201=第一關客訴(一線), 401=業務確認(計劃處理方式=退貨,商城,平台單號)
           - 開案信內容與其他關卡通知信不同
        */
        if (!_data.doSendInformMail("A", _traceID, _NextFlowID, FuncPath()
            , Req_Lang, Req_TypeID, out ErrMsg))
        {
            //[System Log] CreateCCP_Log
            var log = new CCPLog
            {
                Parent_ID = new Guid(Req_DataID),
                LogType = 2,
                LogSubject = "通知信發送失敗(doSendInformMail)," + _traceID,
                LogDesc = ErrMsg,
                Create_Who = fn_Param.CurrentUser
            };
            _data.CreateCCP_Log(log, out ErrMsg);

            //Show Message
            CustomExtension.AlertMsg("開案成功, 但通知信發送失敗.", "");
        }
        else
        {
            //導向列表頁
            CustomExtension.AlertMsg("開案成功!, 已通知下一關人員." + _NextFlowID, Page_SearchUrl);
        }

    }

    /// <summary>
    /// 上傳附件-收貨圖片
    /// </summary>
    protected void btn_Upload_Click(object sender, EventArgs e)
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

        #region ** 檔案處理:儲存檔案 **

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
                        AttachFile = fileList[row].Param_FileName,
                        AttachFile_Org = fileList[row].Param_OrgFileName,
                        Create_Who = fn_Param.CurrentUser
                    };

                    dataItems.Add(dataItem);
                }

                //----- 方法:更新資料 -----
                if (false == _data.CreateCCPTemp_Attachment(dataItems, out ErrMsg))
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
    /// <param name="typeID">Class Type</param>
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
    private void Get_ClassList(string typeID, DropDownList ddl, int ccType, string rootName)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_RefClass(typeID, Req_Lang, ccType, out ErrMsg);


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
            return "{0}/SetEdit/{1}".FormatThis(FuncPath(), Req_DataID);
        }
        set
        {
            _thisPage = value;
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

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() + "/Set" : Server.UrlDecode(tempUrl);
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
            return "jpg|png|pdf|xlsx";
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
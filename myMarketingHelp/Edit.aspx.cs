using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu2000Data.Controllers;
using Menu2000Data.Models;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class myMarketingHelp_Edit : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;
    public bool masterAuth = false; //主管權限(可在權限設定裡勾選)
    public bool replyAuth = false; //回覆權限(可在權限設定裡勾選)

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            #region --權限--
            //[權限判斷] Start
            /* 
             * 使用公司別代號，判斷對應的MENU ID
             */
            bool isPass = false;
            string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

            switch (getCorpUid)
            {
                //case "3":
                //    //上海寶工
                //    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "xxxx");
                //    break;

                case "2":
                    //深圳寶工
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2452");
                    masterAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2458");
                    replyAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2457");

                    break;

                default:
                    //TW
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2451");
                    masterAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2456");
                    replyAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2455");

                    break;
            }

            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }

            //[權限判斷] End
            #endregion

            if (!IsPostBack)
            {
                #region --狀態判斷--
                //取得目前狀態
                string currSt = Get_CurrentStatus(Req_DataID);

                //[派案人員進入] 更新狀態為派案中(B):回覆權限 && 待處理 && 有資料
                if (replyAuth && currSt.Equals("A") && !Req_DataID.Equals("new"))
                {
                    //----- 宣告:資料參數 -----
                    Menu2000Repository _data = new Menu2000Repository();

                    if (!_data.Update_MKHelpStatus(Req_CompID, Req_DataID, "B", fn_Param.CurrentUser))
                    {
                        CustomExtension.AlertMsg("狀態更新時發生錯誤", "");
                    }

                    _data = null;

                    //**重新取得狀態**
                    currSt = Get_CurrentStatus(Req_DataID);
                }

                //處理狀態<>未處理 / 無回覆權限 / 無主管權限 -> 不可編輯
                if (currSt.Equals("") || currSt.Equals("A"))
                {
                    //皆可編輯
                }
                else
                {
                    if (!masterAuth && !replyAuth)
                    {
                        CustomExtension.AlertMsg("此案件已開始被處理，不可修改。", Page_SearchUrl);
                        return;
                    }
                }
                //判斷是否結案, 顯示Lock畫面
                if (currSt.Equals("D"))
                {
                    ph_LockModal.Visible = true;
                    ph_LockScript.Visible = true;
                    ph_ControlBtns.Visible = false;
                    return;
                }
                #endregion


                //取得公司別
                lt_CorpName.Text = fn_Param.GetCorpName(getCorpUid);

                //[產生選單]
                Get_ClassList(Req_CompID, "需求類別", ddl_ReqClass, GetLocalResourceObject("txt_請選擇").ToString());
                Get_ClassList(Req_CompID, "需求資源", ddl_ReqRes, GetLocalResourceObject("txt_請選擇").ToString());
                Get_ClassList(Req_CompID, "緊急度", ddl_EmgStatus, GetLocalResourceObject("txt_請選擇").ToString());

                /* 多語系設定 */
                Page.Title = GetLocalResourceObject("pageTitle").ToString();
                lt_TraceID.Text = GetLocalResourceObject("txt_系統自動產生").ToString();
                lt_ReqStatus.Text = GetLocalResourceObject("txt_資料建立中").ToString();
                lt_CreateDate.Text = GetLocalResourceObject("txt_資料建立中").ToString();
                tb_ReqSubject.Attributes.Add("placeholder", GetLocalResourceObject("tip2").ToString());
                tb_ReqContent.Attributes.Add("placeholder", GetLocalResourceObject("tip3").ToString());
                tb_EstDate.Attributes.Add("placeholder", GetLocalResourceObject("txt_系統自動產生").ToString());
                filter_Emp.Attributes.Add("placeholder", GetLocalResourceObject("tip6").ToString());
                lb_Emp.Text = GetLocalResourceObject("tip7").ToString();
                lt_FinishHours.Text = GetLocalResourceObject("txt_案件處理中").ToString();
                lt_FinishDate.Text = GetLocalResourceObject("txt_案件處理中").ToString();
                tb_ReplyContent.Attributes.Add("placeholder", GetLocalResourceObject("tip8").ToString());


                //[參數判斷] - 資料編號
                if (Req_DataID.Equals("new"))
                {
                    /* 填入預設資料 */
                    //希望完成日(今日)
                    tb_WishDate.Text = DateTime.Now.ToShortDateString().ToDateString("yyyy/MM/dd");

                    //緊急度(一般件)
                    ddl_EmgStatus.SelectedIndex = 1;

                    //需求者(目前使用者)
                    UsersRepository _user = new UsersRepository();
                    var getUser = _user.GetOne(fn_Param.CurrentUserAccount).FirstOrDefault();
                    filter_Emp.Text = getUser.ProfID;
                    lb_Emp.Text = "{0} ({1})".FormatThis(getUser.ProfName, getUser.Email);
                    val_Emp.Text = getUser.ProfID;
                    val_Dept.Text = getUser.DeptID;
                    _user = null;
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
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        string currSt = Get_CurrentStatus(Req_DataID); //目前狀態

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpList(Req_CompID, search, null, out ErrMsg).Take(1).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }

        #region >> 欄位填寫 <<

        //填入資料
        hf_DataID.Value = query.Data_ID.ToString();
        lt_TraceID.Text = query.TraceID;
        lt_ReqStatus.Text = query.StName;
        lt_CreateDate.Text = query.Create_Time.ToDateString("yyyy/MM/dd");
        tb_ReqSubject.Text = query.Req_Subject;
        tb_ReqContent.Text = query.Req_Content;
        ddl_EmgStatus.SelectedValue = query.Emg_Status.ToString();
        tb_WishDate.Text = query.Wish_Date;
        tb_EstDate.Text = query.Est_Date;
        ddl_ReqClass.SelectedValue = query.Req_Class.ToString();
        ddl_ReqRes.SelectedValue = query.Req_Res.ToString();
        tb_ReqQty.Text = query.Req_Qty.ToString();

        string finishHours = query.Finish_Hours.ToString();
        lt_FinishHours.Text = string.IsNullOrWhiteSpace(finishHours) ? "案件處理中" : finishHours;
        string finishDate = query.Finish_Date.ToString();
        lt_FinishDate.Text = string.IsNullOrWhiteSpace(finishDate) ? "案件處理中" : finishDate;

        //-需求者
        filter_Emp.Text = query.Req_Who;
        lb_Emp.Text = query.Req_Name;
        val_Emp.Text = query.Req_Who;
        val_Dept.Text = query.Req_Dept;

        //-flag
        hf_Flag.Value = "Edit";

        #endregion


        #region >> 欄位顯示控制 <<

        //-Display block
        ph_InformWho.Visible = false; //轉寄人員Tree
        ph_InformList.Visible = true; //已轉寄清單
        ph_Comments.Visible = true; //處理進度
        ph_fileEmpty.Visible = false; //附件上傳訊息

        //派案:回覆權限 && 待處理+處理中
        ph_Assign.Visible = replyAuth && (currSt.Equals("B") || currSt.Equals("C"));
        pl_AssignMsg.Visible = replyAuth && currSt.Equals("B"); //待處理顯示訊息

        //結案:回覆權限 && 處理中
        ph_FinishCase.Visible = replyAuth && currSt.Equals("C");

        //回覆區填寫:回覆權限 && 處理中
        ph_ReplyForm.Visible = replyAuth && currSt.Equals("C");

        //退件:主管權限 && 不為結案
        ph_Inform.Visible = masterAuth && (currSt.Equals("B") || currSt.Equals("C"));

        #endregion


        #region >> 其他功能 <<

        //-- 載入其他資料 --
        LookupData_Files();
        LookupData_Inform();
        LookupData_Assigned();
        LookupData_Reply();

        //[產生選單] 處理者(行企)(要填入預設值,放在LookupData_Assigned之後)
        Get_Processer(ddl_ProcWho, hf_AssignWho.Value);

        #endregion


        ////維護資訊
        //this.lt_Creater.Text = query.Create_Name;
        //this.lt_CreateTime.Text = query.Create_Time;
        //this.lt_Updater.Text = query.Update_Name;
        //this.lt_UpdateTime.Text = query.Update_Time;

    }

    #endregion

    #region -- 資料顯示:檔案附件 --

    /// <summary>
    /// 顯示檔案附件
    /// </summary>
    private void LookupData_Files()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpFileList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Attachment.DataSource = query;
        this.lv_Attachment.DataBind();

        //Release
        query = null;
    }

    protected void lv_Attachment_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;
            string Get_FileName = ((HiddenField)e.Item.FindControl("hf_FileName")).Value;

            //----- 宣告:資料參數 -----
            Menu2000Repository _data = new Menu2000Repository();


            //----- 設定:資料欄位 -----
            var data = new MKHelpAttachment
            {
                Parent_ID = new Guid(Req_DataID),
                Data_ID = Convert.ToInt32(Get_DataID)
            };

            //----- 方法:刪除資料 -----
            if (false == _data.Delete_MKHelpFiles(data))
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
                Response.Redirect(thisPage);
            }

        }
    }

    #endregion

    #region -- 資料顯示:轉寄人員 --

    /// <summary>
    /// 顯示轉寄人員
    /// </summary>
    private void LookupData_Inform()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpCCList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Inform.DataSource = query;
        this.lv_Inform.DataBind();

        //Release
        query = null;
    }

    #endregion

    #region -- 資料顯示:處理人員 --

    /// <summary>
    /// 顯示處理人員
    /// </summary>
    private void LookupData_Assigned()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpAssignList(Req_DataID);

        //----- 資料整理:繫結 ----- 
        this.lv_Assigned.DataSource = query;
        this.lv_Assigned.DataBind();

        if (query.Count() > 0)
        {
            var _who = query.Select(x => x.WhoID).ToArray();
            hf_AssignWho.Value = string.Join(",", _who);
        }

        //Release
        query = null;
    }

    #endregion

    #region -- 資料顯示:進度說明 --

    /// <summary>
    /// 顯示進度說明
    /// </summary>
    private void LookupData_Reply()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpReplyList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Reply.DataSource = query;
        this.lv_Reply.DataBind();

        //Release
        query = null;
    }

    protected void lv_Reply_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

            //----- 宣告:資料參數 -----
            Menu2000Repository _data = new Menu2000Repository();


            //----- 設定:資料欄位 -----
            var data = new MKHelpReply
            {
                Parent_ID = new Guid(Req_DataID),
                Data_ID = Convert.ToInt32(Get_DataID)
            };

            //----- 方法:刪除資料 -----
            if (false == _data.Delete_MKHelpReply(data))
            {
                CustomExtension.AlertMsg("進度說明刪除失敗", "");
                return;
            }
            else
            {
                //導向本頁
                Response.Redirect(thisPage + "#replyComments");
            }

        }
    }

    #endregion


    #region -- 資料編輯:基本資料 --

    protected void btn_Save_Click(object sender, EventArgs e)
    {
        //flag=新增OR修改狀態
        string flag = hf_Flag.Value;
        string currDataID = hf_DataID.Value;
        string errTxt = "";

        /* 需求者修改的當下，有可能狀態已改變，故在此要再卡一關 */
        //取得目前狀態
        string currSt = Get_CurrentStatus(Req_DataID);
        //處理狀態<>未處理 / 無回覆權限 / 無主管權限 -> 不可編輯
        if (currSt.Equals("") || currSt.Equals("A"))
        {
            //皆可編輯
        }
        else
        {
            if (!masterAuth && !replyAuth)
            {
                CustomExtension.AlertMsg("此案件已開始被處理，無法繼續編輯。\\n頁面將轉回列表頁..", Page_SearchUrl);
                return;
            }
        }

        #region ** 基本欄位判斷 **

        if (string.IsNullOrWhiteSpace(this.tb_ReqSubject.Text))
        {
            errTxt += "請填寫「主旨」\\n";
        }
        if (string.IsNullOrWhiteSpace(this.tb_ReqContent.Text))
        {
            errTxt += "請填寫「詳細說明」\\n";
        }
        if (this.ddl_EmgStatus.SelectedIndex == 0)
        {
            errTxt += "請選擇「緊急度」\\n";
        }
        if (string.IsNullOrWhiteSpace(this.tb_WishDate.Text))
        {
            errTxt += "請填寫「希望完成日」\\n";
        }
        if (this.ddl_ReqClass.SelectedIndex == 0)
        {
            errTxt += "請選擇「需求類別」\\n";
        }
        if (this.ddl_ReqRes.SelectedIndex == 0)
        {
            errTxt += "請選擇「需求資源」\\n";
        }
        if (string.IsNullOrWhiteSpace(this.tb_ReqQty.Text))
        {
            errTxt += "請填寫「需求數量」\\n";
        }
        if (string.IsNullOrWhiteSpace(this.val_Emp.Text))
        {
            errTxt += "請填寫「需求者」\\n";
        }

        #endregion

        #region ** 檔案上傳判斷 **

        //宣告
        List<IOTempParam> ITempList = new List<IOTempParam>();
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
                    ITempList.Add(new IOTempParam(myFullFile, OrgFileName, hpf));
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


        /* 產生必要資料 Start */
        //產生Guid(使用:資料/目錄產生)
        string guid = flag.ToUpper().Equals("ADD") ? CustomExtension.GetGuid() : currDataID;

        #region ** 計算預計完成日 **

        //取得緊急度
        string emgStatus = ddl_EmgStatus.SelectedValue;
        //取得希望日
        string wishDate = tb_WishDate.Text;
        int days;

        //新增時判斷
        if (flag.ToUpper().Equals("ADD"))
        {
            if (Convert.ToDateTime(wishDate) < DateTime.Today)
            {
                CustomExtension.AlertMsg("希望完成日 不可小於 今日", "");
                return;
            }
        }
        else
        {
            if (Convert.ToDateTime(wishDate) < Convert.ToDateTime(lt_CreateDate.Text))
            {
                CustomExtension.AlertMsg("希望完成日 不可小於 登記日", "");
                return;
            }
        }

        //判斷緊急度(MK_ParamClass:固定ID)
        switch (emgStatus)
        {
            //加5天
            case "9":
            case "12":
                days = 5;
                break;

            //加2天
            case "10":
            case "13":
                days = 2;
                break;

            default:
                days = 0;
                break;
        }
        //計算預計日(工作日)
        DateTime estDate = CustomExtension.GetWorkDate(Convert.ToDateTime(wishDate), days);
        tb_EstDate.Text = estDate.ToShortDateString().ToDateString("yyyy/MM/dd");

        #endregion

        /* 產生必要資料 End */


        #region ** 儲存檔案 **

        /* 注意檔案路徑及資料夾(站台資料夾+guid) */
        if (ITempList.Count > 0)
        {
            int errCnt = 0;
            string ftpFolder = UploadFolder + guid; //ftp資料夾(站台資料夾+guid)

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
                ph_ErrMessage.Visible = true;
                lt_ShowMsg.Text = "檔案上傳失敗, 失敗筆數為 {0} 筆, 請重新整理後再上傳!".FormatThis(errCnt);
                return;
            }
        }

        #endregion


        /* 執行新增/更新 */
        if (string.IsNullOrWhiteSpace(currDataID))
        {
            Add_Data(ITempList, guid);
        }
        else
        {
            Edit_Data(ITempList, currDataID);
        }
    }

    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data(List<IOTempParam> fileList, string guid)
    {
        bool sendCC = false;

        #region 資料處理:基本資料

        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 設定:資料欄位 -----

        var data = new MKHelpItem
        {
            Data_ID = new Guid(guid),
            CompID = Req_CompID,
            Req_Who = val_Emp.Text,
            Req_Dept = val_Dept.Text,
            Req_Subject = tb_ReqSubject.Text.Left(40),
            Req_Content = tb_ReqContent.Text.Left(3000),
            Req_Qty = Convert.ToInt16(tb_ReqQty.Text),
            Req_Status = Get_ClassID(Req_CompID, "處理狀態", "A"), //新增時預設:未處理
            Req_Class = Convert.ToInt16(ddl_ReqClass.SelectedValue),
            Req_Res = Convert.ToInt16(ddl_ReqRes.SelectedValue),
            Emg_Status = Convert.ToInt16(ddl_EmgStatus.SelectedValue),
            Wish_Date = tb_WishDate.Text,
            Est_Date = tb_EstDate.Text,
            Finish_Date = "",
            Finish_Hours = null,
            Create_Who = fn_Param.CurrentUser
        };

        //----- 方法:新增資料 -----
        if (!_data.CreateMKHelp_Base(data, out ErrMsg))
        {
            this.ph_ErrMessage.Visible = true;
            this.lt_ShowMsg.Text = "<b>資料新增失敗</b><p>{0}</p><p>{1}</p>".FormatThis("遇到無法排除的錯誤，請聯絡系統管理員。", ErrMsg);

            CustomExtension.AlertMsg("新增失敗", "");
            return;
        }

        #endregion

        #region 資料處理:檔案附件

        if (fileList.Count > 0)
        {
            //----- 宣告:資料參數 -----
            List<MKHelpAttachment> dataItems = new List<MKHelpAttachment>();

            //----- 設定:資料欄位 -----
            for (int row = 0; row < fileList.Count; row++)
            {
                var dataItem = new MKHelpAttachment
                {
                    Parent_ID = new Guid(guid),
                    AttachFile = fileList[row].Param_FileName,
                    AttachFile_Org = fileList[row].Param_OrgFileName,
                    Create_Who = fn_Param.CurrentUser
                };

                dataItems.Add(dataItem);
            }

            //----- 方法:更新資料 -----
            if (false == _data.CreateMKHelp_Attachment(dataItems, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增檔案失敗", "");
            }
        }

        #endregion

        #region 資料處理:轉寄人員

        string infomWho = tb_InfoWho.Text;
        if (!string.IsNullOrWhiteSpace(infomWho))
        {
            //將來源字串轉為陣列,以逗號為分隔
            string[] strAry = Regex.Split(infomWho, @"\,{1}");
            //使用LINQ整理資料
            var query = from el in strAry
                        select new
                        {
                            who = el.ToString()
                        };

            //----- 宣告:資料參數 -----
            List<MKHelpCC> dataItems = new List<MKHelpCC>();

            //----- 設定:資料欄位 -----
            foreach (var item in query)
            {
                var dataItem = new MKHelpCC
                {
                    Parent_ID = new Guid(guid),
                    CC_Who = item.who
                };

                dataItems.Add(dataItem);
            }

            //----- 方法:更新資料 -----
            if (false == _data.CreateMKHelp_Inform(dataItems, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增轉寄人員失敗", "");
            }

            //do send
            sendCC = true;
        }

        #endregion

        /* [寄通知信]:執行單位 */
        if (!doSendInformMail("A", guid, "", out ErrMsg))
        {
            CustomExtension.AlertMsg("新需求通知信發送失敗.", Page_SearchUrl);
        }

        /* [寄通知信]:轉寄人員 */
        if (sendCC)
        {
            if (!doSendInformMail("B", guid, "", out ErrMsg))
            {
                CustomExtension.AlertMsg("轉寄人員通知信發送失敗.", Page_SearchUrl);
            }
        }

        //更新本頁Url
        //string thisUrl = "{0}/Edit/{1}".FormatThis(FuncPath(), guid);

        //導向列長頁
        Response.Redirect(Page_SearchUrl);
    }

    /// <summary>
    /// 資料修改
    /// </summary>
    private void Edit_Data(List<IOTempParam> fileList, string guid)
    {

        #region 資料處理:基本資料

        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 設定:資料欄位 -----

        var data = new MKHelpItem
        {
            Data_ID = new Guid(guid),
            Req_Who = val_Emp.Text,
            Req_Dept = val_Dept.Text,
            Req_Subject = tb_ReqSubject.Text.Left(40),
            Req_Content = tb_ReqContent.Text.Left(3000),
            Req_Qty = Convert.ToInt16(tb_ReqQty.Text),
            Req_Class = Convert.ToInt16(ddl_ReqClass.SelectedValue),
            Req_Res = Convert.ToInt16(ddl_ReqRes.SelectedValue),
            Emg_Status = Convert.ToInt16(ddl_EmgStatus.SelectedValue),
            Wish_Date = tb_WishDate.Text,
            Est_Date = tb_EstDate.Text,
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:新增資料 -----
        if (!_data.Update_MKHelpBase(data, out ErrMsg))
        {
            this.ph_ErrMessage.Visible = true;
            this.lt_ShowMsg.Text = "<b>資料修改失敗</b><p>{0}</p><p>{1}</p>".FormatThis("遇到無法排除的錯誤，請聯絡系統管理員。", ErrMsg);

            CustomExtension.AlertMsg("修改失敗", "");
            return;
        }

        #endregion

        #region 資料處理:檔案附件

        if (fileList.Count > 0)
        {
            //----- 宣告:資料參數 -----
            List<MKHelpAttachment> dataItems = new List<MKHelpAttachment>();

            //----- 設定:資料欄位 -----
            for (int row = 0; row < fileList.Count; row++)
            {
                var dataItem = new MKHelpAttachment
                {
                    Parent_ID = new Guid(guid),
                    AttachFile = fileList[row].Param_FileName,
                    AttachFile_Org = fileList[row].Param_OrgFileName,
                    Create_Who = fn_Param.CurrentUser
                };

                dataItems.Add(dataItem);
            }

            //----- 方法:更新資料 -----
            if (false == _data.CreateMKHelp_Attachment(dataItems, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增檔案失敗", "");
            }
        }

        #endregion

        //導向本頁
        Response.Redirect(thisPage);
    }

    #endregion


    #region -- 按鈕事件 --
    /// <summary>
    /// 派案
    /// </summary>
    protected void btn_Assign_Click(object sender, EventArgs e)
    {
        string procWho = val_Proc.Text;
        if (string.IsNullOrWhiteSpace(procWho))
        {
            CustomExtension.AlertMsg("未選擇處理人員,請確認!", "");
            return;
        }
        //將來源字串轉為陣列,以逗號為分隔
        string[] strAry = Regex.Split(procWho, @"\,{1}");


        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        List<MKHelpAssigned> dataItems = new List<MKHelpAssigned>();

        //----- 設定:資料欄位 -----
        var query = strAry
            .Select(fld => new
            {
                myVal = fld.ToString()

            });
        foreach (var item in query)
        {
            var dataItem = new MKHelpAssigned
            {
                Parent_ID = new Guid(Req_DataID),
                Who = item.myVal,
                Create_Who = fn_Param.CurrentUser,
                CompID = Req_CompID
            };

            dataItems.Add(dataItem);
        }

        //----- 方法:新增資料 -----
        if (!_data.CreateMKHelp_Assign(dataItems, out ErrMsg))
        {
            CustomExtension.AlertMsg("人員指派失敗", "");
            return;
        }
        else
        {
            /* [寄通知信]:處理人員 */
            if (!doSendInformMail("C", Req_DataID, "", out ErrMsg))
            {
                CustomExtension.AlertMsg("處理人員通知信發送失敗.", thisPage);
            }

            //導向本頁
            Response.Redirect(thisPage + "#replyComments");
        }
    }


    /// <summary>
    /// *** 自訂通知信 ***
    /// </summary>
    protected void btn_Inform_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        //[檢查] 有權限者才能編輯
        if (!masterAuth)
        {
            CustomExtension.AlertMsg("無法執行。\\n頁面將轉回列表頁..", Page_SearchUrl);
            return;
        }

        //取得欄位資料
        string _id = hf_DataID.Value;
        string _cont = val_MailCont.Text;

        #region ** 欄位判斷 **
        if (string.IsNullOrWhiteSpace(_id))
        {
            errTxt += "「資料編號空白」\\n";
        }
        if (string.IsNullOrWhiteSpace(_cont))
        {
            errTxt += "請填寫「內文」\\n";
        }

        #endregion

        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }


        #region ** 資料處理 **
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        try
        {
            #region 資料處理:基本資料

            //----- 設定:資料欄位 -----

            var data = new MKHelpItem
            {
                Data_ID = new Guid(Req_DataID),
                Reback_Desc = _cont,
                Update_Who = fn_Param.CurrentUser
            };

            //----- 方法:更新資料 -----
            if (!_data.Update_MKHelpReBack(data, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>退件失敗</b><p>{0}</p><p>{1}</p>".FormatThis("遇到無法排除的錯誤，請聯絡系統管理員。", ErrMsg);

                CustomExtension.AlertMsg("退件失敗", "");
                return;
            }

            //----- 方法:更新狀態 -----
            if (!_data.Update_MKHelpStatus(Req_CompID, Req_DataID, "E", fn_Param.CurrentUser))
            {
                CustomExtension.AlertMsg("退件成功，但狀態更新時發生錯誤，請聯絡系統管理員", "");
            }

            #endregion


            #region ### 通知信發送 ###

            /* [寄通知信]:自訂通知(需求者) */
            if (!doSendInformMail("E", _id, _cont.Replace("\r\n", "<br/>"), out ErrMsg))
            {
                CustomExtension.AlertMsg("狀態修改成功，但通知信發送失敗.", "");
                return;
            }

            #endregion
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            _data = null;
        }

        #endregion


        //導向列表頁
        CustomExtension.AlertMsg("執行成功，將導至列表頁.", Page_SearchUrl);
    }


    /// <summary>
    /// 結案
    /// </summary>
    protected void btn_Finish_Click(object sender, EventArgs e)
    {
        //取得輸入值
        string hours = val_FinishHours.Text;
        string dt = val_FinishDate.Text;

        //判斷空值
        if (string.IsNullOrWhiteSpace(hours) || string.IsNullOrWhiteSpace(dt))
        {
            CustomExtension.AlertMsg("請填寫工時及結案日!", "");
            return;
        }
        //判斷處理進度是否填寫
        if (lv_Reply.Items.Count == 0)
        {
            CustomExtension.AlertMsg("處理進度未填寫，至少要填寫一筆!", "");
            return;
        }


        #region 資料處理:基本資料

        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 設定:資料欄位 -----

        var data = new MKHelpItem
        {
            Data_ID = new Guid(Req_DataID),
            Finish_Date = dt.ToDateString("yyyy/MM/dd"),
            Finish_Hours = Convert.ToDouble(hours),
            Update_Who = fn_Param.CurrentUser
        };

        //----- 方法:更新資料 -----
        if (!_data.Update_MKHelpClose(data, out ErrMsg))
        {
            this.ph_ErrMessage.Visible = true;
            this.lt_ShowMsg.Text = "<b>結案失敗</b><p>{0}</p><p>{1}</p>".FormatThis("遇到無法排除的錯誤，請聯絡系統管理員。", ErrMsg);

            CustomExtension.AlertMsg("結案失敗", "");
            return;
        }

        //----- 方法:更新狀態 -----
        if (!_data.Update_MKHelpStatus(Req_CompID, Req_DataID, "D", fn_Param.CurrentUser))
        {
            CustomExtension.AlertMsg("結案成功，但狀態更新時發生錯誤，請聯絡系統管理員", "");
        }

        #endregion


        /* [寄通知信]:結案通知所有人 */
        if (!doSendInformMail("D", Req_DataID, "", out ErrMsg))
        {
            CustomExtension.AlertMsg("結案成功，通知信發送失敗.", Page_SearchUrl);
            return;
        }


        //導至列表頁
        CustomExtension.AlertMsg("結案成功，將導至列表頁.", Page_SearchUrl);

    }

    /// <summary>
    /// 回覆處理進度
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btn_Reply_Click(object sender, EventArgs e)
    {
        string errTxt = "";
        string replyCont = this.tb_ReplyContent.Text;
        if (string.IsNullOrWhiteSpace(replyCont))
        {
            errTxt += "請填寫「進度說明」\\n";
        }
        if (replyCont.Length > 2000)
        {
            errTxt += "「進度說明」最多2000字\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        //新增資料
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        List<MKHelpReply> dataItems = new List<MKHelpReply>();

        //----- 設定:資料欄位 -----
        var dataItem = new MKHelpReply
        {
            Parent_ID = new Guid(Req_DataID),
            Reply_Content = replyCont,
            Create_Who = fn_Param.CurrentUser
        };

        dataItems.Add(dataItem);


        //----- 方法:新增資料 -----
        if (!_data.CreateMKHelp_Reply(dataItems, out ErrMsg))
        {
            CustomExtension.AlertMsg("進度說明新增失敗", "");
            return;
        }
        else
        {
            //導向本頁
            Response.Redirect(thisPage + "#replyComments");
        }
    }
    #endregion

    #region -- 附加功能 --
    /// <summary>
    /// 取得類別資料 
    /// </summary>
    /// <param name="compID">compID</param>
    /// <param name="typeName">類別Type</param>
    /// <param name="ddl">下拉選單object</param>
    /// <param name="rootName">第一選項顯示名稱</param>
    private void Get_ClassList(string compID, string typeName, DropDownList ddl, string rootName)
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpClass(compID, typeName, out ErrMsg);


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

        query = null;
    }


    /// <summary>
    /// 取得對應的類別代號(目前使用:處理狀態)
    /// </summary>
    /// <param name="compID">公司別</param>
    /// <param name="typeName">類型</param>
    /// <param name="customID">自訂編號</param>
    /// <returns></returns>
    private int Get_ClassID(string compID, string typeName, string customID)
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMKHelpClass(compID, typeName, out ErrMsg)
            .Where(fld => fld.CustomID.Equals(customID))
            .FirstOrDefault();

        return query.ID;
    }


    /// <summary>
    /// 取得目前狀態
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private string Get_CurrentStatus(string id)
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        return _data.GetOne_MKHelpStatus(id);
    }


    /// <summary>
    /// 取得處理人員(指定部門)
    /// </summary>
    /// <param name="ddl"></param>
    /// <param name="reqValue">傳入已勾選(以逗號分隔)</param>
    private void Get_Processer(ListBox ddl, string reqValue)
    {
        //----- Clear -----
        ddl.Items.Clear();

        //----- 宣告 -----
        UsersRepository _user = new UsersRepository();
        Dictionary<int, string> _dept = new Dictionary<int, string>();

        //----- 取得資料 -----        
        if (Req_CompID.Equals("TW"))
        {
            //Comp=TW
            //加入條件:台灣行企
            _dept.Add(1, "180");
        }
        else
        {
            //Comp=SZ
            //加入條件:台灣行企,深圳行企
            _dept.Add(1, "314");
            _dept.Add(2, "180");
        }

        //呼叫並回傳資料
        var getUsers = _user.GetUsers(null, _dept);
        //選單設定root
        ddl.Items.Add(new ListItem("未指定", ""));
        //選單設定選項
        foreach (var item in getUsers)
        {
            ddl.Items.Add(new ListItem("{0} ({1})".FormatThis(item.ProfName, item.NickName), item.ProfID));
        }

        //判斷已選擇的值, 並設為selected
        if (!string.IsNullOrWhiteSpace(reqValue))
        {
            //將來源字串轉為陣列,以逗號為分隔
            string[] strAry = Regex.Split(reqValue, @"\,{1}");
            //使用LINQ整理資料
            var query = from el in strAry
                        select new
                        {
                            selectedVal = el.ToString()
                        };
            //使用迴圈方式,將選項設為selected
            for (int row = 0; row < ddl.Items.Count; row++)
            {
                foreach (var item in query)
                {
                    if (ddl.Items[row].Value.Equals(item.selectedVal))
                    {
                        ddl.Items[row].Selected = true;
                    }
                }
            }

        }

        getUsers = null;
    }

    #endregion


    #region -- 通知信 --

    /// <summary>
    /// 發通知信
    /// </summary>
    /// <param name="sendType">A:新需求/B:轉寄通知/C:指派處理人員/D:結案</param>
    /// <param name="guid">資料編號</param>
    /// <param name="cont">自訂內文</param>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    private bool doSendInformMail(string sendType, string guid, string cont, out string ErrMsg)
    {
        ErrMsg = "";
        bool doSend = true;
        string mailSubject = "";

        //設定主旨前置文字
        switch (sendType)
        {
            case "A":
                //新需求
                mailSubject = "[製物工單][新需求]";
                break;

            case "B":
                //轉寄通知
                mailSubject = "[製物工單][轉寄通知]";
                break;

            case "C":
                //指派處理人員
                mailSubject = "[製物工單][派案通知]";
                break;

            case "D":
                //結案
                mailSubject = "[製物工單][結案通知]";
                break;

            case "E":
                //退件
                mailSubject = "[製物工單][退件通知]";
                break;

            default:
                //錯誤的type
                doSend = false;
                break;
        }
        if (!doSend)
        {
            ErrMsg = "錯誤的type";
            return false;
        }

        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", guid);

        //[資料取得] - 基本資料
        var baseData = _data.GetMKHelpList(Req_CompID, search, null, out ErrMsg).Take(1).FirstOrDefault();
        if (baseData == null)
        {
            ErrMsg = "查無資料";
            return false;
        }
        _data = null;

        //[設定] 郵件主旨
        mailSubject += "{0} ({1})".FormatThis(baseData.Req_Subject, baseData.TraceID);

        //[設定] 郵件內容
        StringBuilder mailBoday = Get_MailContent(sendType, guid, cont, baseData);

        //[設定] 取得收件人
        ArrayList mailList = Get_MailList(sendType, guid, baseData.Req_Email);

        //判斷是否有收件人
        if (mailList.Count == 0)
        {
            ErrMsg = "無收件人";
            return false;
        }

        //開始發送通知信
        if (!Send_Email(mailList, mailSubject, mailBoday, sendType, out ErrMsg))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 取得收件人清單
    /// </summary>
    /// <param name="sendType"></param>
    /// <param name="dataID"></param>
    /// <param name="reqEmail">需求者Email</param>
    /// <returns></returns>
    private ArrayList Get_MailList(string sendType, string dataID, string reqEmail)
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        ArrayList mailList = new ArrayList();

        switch (sendType)
        {
            case "A":
                //新需求
                var dataA = _data.GetMKHelpReceiver(Req_CompID.Equals("TW") ? "10" : "20");
                foreach (var item in dataA)
                {
                    mailList.Add(item.Email);
                }
                dataA = null;
                break;

            case "B":
                //轉寄通知
                var dataB = _data.GetMKHelpCCList(dataID);
                foreach (var item in dataB)
                {
                    mailList.Add(item.CC_Email);
                }
                dataB = null;

                break;

            case "C":
                //指派處理人員
                var dataC = _data.GetMKHelpAssignList(dataID);
                foreach (var item in dataC)
                {
                    mailList.Add(item.Email);
                }
                dataC = null;
                break;

            case "D":
                //結案
                //固定收信人員
                var dataD = _data.GetMKHelpReceiver(Req_CompID.Equals("TW") ? "15" : "25");
                foreach (var item in dataD)
                {
                    mailList.Add(item.Email);
                }
                dataD = null;

                //轉寄人員
                var dataD1 = _data.GetMKHelpCCList(dataID);
                foreach (var item in dataD1)
                {
                    mailList.Add(item.CC_Email);
                }
                dataD1 = null;

                //需求者
                mailList.Add(reqEmail);

                break;


            case "E":
                //退件,需求者
                mailList.Add(reqEmail);

                break;
        }

        _data = null;

        return mailList;
    }

    /// <summary>
    /// 設定郵件內容
    /// </summary>
    /// <param name="sendType"></param>
    /// <param name="guid"></param>
    /// <param name="cont">自訂內文</param>
    /// <param name="baseData"></param>
    /// <returns></returns>
    private StringBuilder Get_MailContent(string sendType, string guid, string cont, MKHelpItem baseData)
    {
        //宣告
        StringBuilder html = new StringBuilder();
        Menu2000Repository _data = new Menu2000Repository();

        //Html模版路徑(From CDN)
        string url = "{0}PKHome/MarketingHelp/Mail_{1}.html?v=1.0".FormatThis(fn_Param.CDNUrl, Req_Lang.ToUpper());

        //取得HTML模版(Html不可放在本機)
        string htmlPage = CustomExtension.WebRequest_byGET(url);

        //加入模版內容
        html.Append(htmlPage);

        //[取代指定內容]:郵件固定內容
        string msg = "";
        string editUrl = "{0}/Edit/{1}".FormatThis(FuncPath(), guid);
        string viewUrl = "{0}/View/{1}".FormatThis(FuncPath(), guid);
        string pageUrl = "";
        switch (sendType)
        {
            case "A":
                msg = "<h3>目前有新的製物工單需求，請前往指派處理人員。</h3>";
                pageUrl = editUrl;
                break;

            case "B":
                msg = "<h3>此信為製物工單轉寄通知。</h3>";
                pageUrl = viewUrl;
                break;

            case "C":
                msg = "<h3>你已被指派處理此案件。</h3>";
                pageUrl = editUrl;
                break;

            case "D":
                msg = "<h3>案件已完成。</h3>";
                pageUrl = viewUrl;
                break;

            case "E":
                msg = "<h3>你申請的案件已被退回，原因如下:</h3>" + cont;
                pageUrl = viewUrl;
                break;


            default:
                msg = "";
                pageUrl = viewUrl;
                break;

        }
        html.Replace("#informMessage#", msg);
        html.Replace("#ProcUrl#", pageUrl);
        html.Replace("#今年#", DateTime.Now.Year.ToString());


        //[取代指定內容]:基本資料
        html.Replace("#TraceID#", baseData.TraceID);
        html.Replace("#CreateDate#", baseData.Create_Time.ToDateString("yyyy/MM/dd"));
        html.Replace("#EmgName#", baseData.EmgName);
        html.Replace("#TypeName#", baseData.TypeName);
        html.Replace("#ResName#", baseData.ResName);
        html.Replace("#ReqName#", baseData.Req_Name);
        html.Replace("#FinishHours#", baseData.Finish_Hours.ToString());
        html.Replace("#FinishDate#", baseData.Finish_Date);
        html.Replace("#subject#", baseData.Req_Subject);
        html.Replace("#content#", baseData.Req_Content.Replace("\r", "<br/>"));
        //工時欄位display
        html.Replace("#Disp1#", baseData.StDisp.Equals("D") ? "" : "display:none");

        baseData = null;


        //[資料取得] - 處理進度
        var replyData = _data.GetMKHelpReplyList(guid);
        html.Replace("#Disp2#", replyData.Count() == 0 ? "display:none" : "");

        if (replyData.Count() > 0)
        {
            string loopHtml = "";
            foreach (var data in replyData)
            {
                loopHtml += "<tr>";
                loopHtml += " <td colspan=\"4\">";
                loopHtml += " <p><font style=\"font-size: 12px; color: #4183c4;\">{0}</font>&nbsp;<font style=\"font-size: 12px; color: #757575 ;\">{1}</font>"
                    .FormatThis(data.Create_Name, data.Create_Time);
                loopHtml += " </p>";
                loopHtml += " <p>{0}</p>".FormatThis(data.Reply_Content.ToString().Replace("\r", "<br/>"));
                loopHtml += " </td>";
                loopHtml += "</tr>";
            }

            html.Replace("#replyItems#", loopHtml);
        }
        replyData = null;


        //[資料取得] - 附件清單
        var fileData = _data.GetMKHelpFileList(guid);
        html.Replace("#Disp3#", fileData.Count() == 0 ? "display:none" : "");

        if (fileData.Count() > 0)
        {
            string loopHtml = "";
            foreach (var data in fileData)
            {
                loopHtml += "<div style=\"background-color: #43a047; border: 1px solid #43a047; text-align: center; margin: 10px;\">";
                loopHtml += "<a href=\"{0}{1}/{2}\" target=\"_blank\" style=\"color: #FFFFFF; text-decoration: none; display: block; padding: 5px;\">{3}</a>"
                    .FormatThis(
                        fn_Param.RefUrl + UploadFolder
                        , guid
                        , data.AttachFile
                        , data.AttachFile_Org);
                loopHtml += "</div>";
            }

            html.Replace("#filesItems#", loopHtml);
        }
        fileData = null;

        return html;
    }


    /// <summary>
    /// 開始發信
    /// </summary>
    /// <param name="mailList">MailList</param>
    /// <param name="subject">主旨</param>
    /// <param name="sendType">寄件模式</param>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    /// <remarks>
    /// [sendType]
    /// A:新需求, 發給行企全體(MK_Help_Receiver.MailType=1), 告知派案
    /// B:轉寄通知, 發給勾選人員, 只通知(MK_Help_CC)
    /// C:派案完成, 通知勾選的處理人員(MK_Help_Assigned)
    /// D:結案, 通知所有人(MK_Help_Receiver.MailType=2)/(MK_Help_CC)
    /// </remarks>
    private bool Send_Email(ArrayList mailList, string subject, StringBuilder mailBody, string sendType, out string ErrMsg)
    {
        try
        {
            //開始發信
            using (MailMessage Msg = new MailMessage())
            {
                //寄件人
                Msg.From = new MailAddress(fn_Param.SysMail_Sender, "寶工內部網站");

                //收件人
                foreach (string email in mailList)
                {
                    Msg.To.Add(new MailAddress(email));
                }

                //主旨
                Msg.Subject = subject;

                //Body:取得郵件內容
                Msg.Body = mailBody.ToString();

                Msg.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();

                smtp.Send(Msg);
                smtp.Dispose();

                //OK
                ErrMsg = "";
                return true;
            }
        }
        catch (Exception ex)
        {
            ErrMsg = "郵件發送失敗..." + ex.Message.ToString();
            return false;
        }
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
            this._Req_Lang = value;
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
            this._Req_RootID = value;
        }
    }

    /// <summary>
    /// 取得網址參數 - Company ID(TW/SH/SZ)
    /// </summary>
    private string _Req_CompID;
    public string Req_CompID
    {
        get
        {
            String DataID = Page.RouteData.Values["CompID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "TW" : DataID;
        }
        set
        {
            this._Req_CompID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/MarketingHelp/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
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
            this._Req_DataID = value;
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
            this._thisPage = value;
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
            string tempUrl = CustomExtension.getCookie("HomeList_MKHelp");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            this._Page_SearchUrl = value;
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
            this._FileExtLimit = value;
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
            this._FileSizeLimit = value;
        }
    }

    /// <summary>
    /// 限制上傳檔案數
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
            this._FileCountLimit = value;
        }
    }

    /// <summary>
    /// 上傳目錄
    /// </summary>
    private string _UploadFolder;
    public string UploadFolder
    {
        get
        {
            return "{0}MarketingHelp/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
        }
        set
        {
            this._UploadFolder = value;
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
            get { return this._Param_FileName; }
            set { this._Param_FileName = value; }
        }

        /// <summary>
        /// [參數] -原始檔名
        /// </summary>
        private string _Param_OrgFileName;
        public string Param_OrgFileName
        {
            get { return this._Param_OrgFileName; }
            set { this._Param_OrgFileName = value; }
        }


        private HttpPostedFile _Param_hpf;
        public HttpPostedFile Param_hpf
        {
            get { return this._Param_hpf; }
            set { this._Param_hpf = value; }
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
            this._Param_FileName = Param_FileName;
            this._Param_OrgFileName = Param_OrgFileName;
            this._Param_hpf = Param_hpf;
        }

    }
    #endregion

}
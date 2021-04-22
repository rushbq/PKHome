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
using PKLib_Data.Controllers;
using PKLib_Method.Methods;
using twMGMTData.Controllers;
using twMGMTData.Models;


public partial class MGHelp_Edit : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    //回覆權限
    public bool _ReplyAuth = false;
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //[權限判斷]
            bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2412");
            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }
            //取得回覆權限
            _ReplyAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2413");

            if (!IsPostBack)
            {
                //Get Class(A:處理狀態, B:需求類別, C:處理記錄類別)
                Get_ClassList("B", ddl_ReqClass, "請選擇", "99"); //需求類別
                Get_ClassList("C", ddl_ProcClass, "請選擇", ""); //處理類別


                //是否顯示單身資料
                bool _showDetail = false;

                //[參數判斷] - 資料編號
                if (string.IsNullOrWhiteSpace(Req_DataID))
                {
                    /* 填入預設資料 */

                    //需求者(目前使用者)
                    string _accID = fn_Param.CurrentUserAccount;
                    filter_Emp.Text = _accID;
                    lb_Emp.Text = fn_Param.CurrentUserName;
                    val_Emp.Text = _accID;
                }
                else
                {
                    //載入資料
                    LookupData();

                    _showDetail = true;
                }

                //區塊顯示/隱藏
                if (!_showDetail || !_ReplyAuth)
                {
                    //隱藏其他單身區塊
                    ph_section1.Visible = false;
                    ph_section2.Visible = false;
                    ph_section3.Visible = false;
                    ph_naviBar.Visible = false; //跳轉選單
                }
                else
                {
                    //顯示其他單身區塊
                    ph_section1.Visible = true;
                    ph_section2.Visible = true;
                    ph_section3.Visible = true;
                    ph_naviBar.Visible = true; //跳轉選單
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
        MGMTRepository _data = new MGMTRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            search.Add("DataID", Req_DataID);

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetOne_MGhelp(search, out ErrMsg).Take(1).FirstOrDefault();

            //----- 資料整理:繫結 ----- 
            if (query == null)
            {
                CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
                return;
            }

            #region >> 欄位填寫:需求資料 <<

            //處理狀態ID
            string _currStatus = query.Help_Status.ToString();
            //需求類別ID
            string _currReqCls = query.Req_Class.ToString();

            //填入資料
            hf_DataID.Value = query.DataID.ToString();
            lt_TraceID.Text = query.TraceID;
            lt_ReqStatus.Text = _data.GetMgHelp_StatusLabel(query.Help_Status.ToString(), query.Help_StatusName);  //處理狀態
            lt_CreateDate.Text = query.Create_Time.ToDateString("yyyy/MM/dd");
            tb_ReqSubject.Text = query.Help_Subject;
            tb_ReqContent.Text = query.Help_Content;
            tb_Help_Benefit.Text = query.Help_Benefit;
            lt_ReqClass.Text = query.Req_ClassName; //需求類別

            //報修方式
            rbl_Help_Way.SelectedValue = query.Help_Way.ToString();
            rbl_Help_Way.Enabled = _ReplyAuth;

            //需求者
            filter_Emp.Text = query.Req_Who;
            lb_Emp.Text = query.Req_WhoName + " (" + query.Req_NickName + ") #" + query.Req_TelExt;
            val_Emp.Text = query.Req_Who;

            //主管同意狀態
            ph_Agree.Visible = !query.IsAgree.Equals("E");
            lt_AuthAgree.Text = query.IsAgree.Equals("N") ? "未同意&nbsp;" + query.Agree_Time.ToString().ToDateString("yyyy/MM/dd HH:mm")
                : "{0} 於 {1} 同意".FormatThis(
                    query.Agree_WhoName
                    , query.Agree_Time.ToString().ToDateString("yyyy/MM/dd HH:mm")
                );
            lbtn_doApprove.Visible = query.IsAgree.Equals("E");
            #endregion


            #region >> 欄位填寫:回覆資料 <<

            string _ontop = query.onTop;
            if (_ontop.Equals("Y"))
            {
                //已加入置頂,不顯示按鈕
                lbtn_doTrace.Visible = false;
            }

            //需求類別
            ddl_ReqClass.SelectedValue = _currReqCls;
            //工時
            tb_Finish_Hours.Text = query.Finish_Hours.ToString();
            tb_ReplyContent.Text = query.Reply_Content;
            tb_Wish_Date.Text = query.Wish_Time.ToDateString("yyyy/MM/dd");

            //結案日
            string _fDate = query.Finish_Time.ToString();
            lt_Finish_Time.Text = string.IsNullOrWhiteSpace(_fDate) ? "案件處理中" : _fDate.ToDateString("yyyy/MM/dd");

            //結案人
            string _fWho = query.Finish_WhoName.ToString();
            lt_Finish_Who.Text = string.IsNullOrWhiteSpace(_fWho) ? "案件處理中" : _fWho;


            //Lock Modal Page
            show_TraceID.Text = query.TraceID;
            show_FinishTime.Text = _fDate.ToDateString("yyyy/MM/dd");
            show_FinishWho.Text = _fWho;

            #endregion


            #region >> 欄位填寫:驗收資料 <<

            rbl_RateScore.SelectedValue = query.RateScore.ToString();
            tb_RateContent.Text = query.RateContent;
            lt_RateWho.Text = query.RateWhoName;

            #endregion


            #region >> 欄位顯示控制 <<

            //顯示區塊
            ph_InformWho.Visible = false; //轉寄人員Tree (新增時才可使用)
            ph_InformList.Visible = true; //已轉寄清單
            ph_fileEmpty.Visible = false; //附件上傳訊息

            //需求資料:按鈕
            lt_SaveBase.Text = "存檔";

            //改善效益Lock
            tb_Help_Benefit.Enabled = _currStatus.Equals("110");

            string _viewPage = "{0}twManagement/MgHelp_View.aspx?id={1}".FormatThis(fn_Param.WebUrl, Req_DataID);

            //不可編輯的狀態(不為 待處理 的狀態)
            if (!_currStatus.Equals("110") && !_ReplyAuth)
            {
                Response.Redirect(_viewPage);
                return;
            }

            //結案權限判斷
            if (_currStatus.Equals("140") && !_ReplyAuth)
            {
                //無權限不能繼續編輯
                CustomExtension.AlertMsg("案件已結案,即將導至檢視頁.", _viewPage);
                return;
            }

            //結案:隱藏部份按鈕
            if (_currStatus.Equals("140"))
            {
                //區塊:回覆資料
                lbtn_doTrace.Visible = false;
                ph_finish.Visible = false;

                //區塊:驗收意見
                ph_unClose.Visible = false;
                ph_section3_data.Visible = true;

                //Lock Modal
                ph_LockScript.Visible = true;
                ph_LockModal.Visible = true;
            }
            else
            {
                //區塊:驗收意見
                ph_unClose.Visible = true;
                ph_section3_data.Visible = false;
            }

            #endregion


            #region >> 其他功能 <<

            //-- 載入其他資料 --
            LookupData_Files();
            LookupData_Inform();
            LookupData_Processes();

            //default value
            tb_ProcTime.Text = DateTime.Now.ToString().ToDateString("yyyy/MM/dd HH:mm");

            #endregion


            //維護資訊
            info_Creater.Text = query.Create_Name;
            info_CreateTime.Text = query.Create_Time.ToDateString("yyyy-MM-dd HH:mm");
            info_Updater.Text = query.Update_Name;
            info_UpdateTime.Text = query.Update_Time.ToDateString("yyyy-MM-dd HH:mm");

        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //Release
            _data = null;
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
        MGMTRepository _data = new MGMTRepository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMgHelpFileList(Req_DataID, "", "A");

        //----- 資料整理:繫結 ----- 
        this.lv_Attachment.DataSource = query;
        this.lv_Attachment.DataBind();

        //Release
        query = null;
        _data = null;
    }

    protected void lv_Attachment_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;
            string Get_FileName = ((HiddenField)e.Item.FindControl("hf_FileName")).Value;
            string Get_TraceID = lt_TraceID.Text;

            //----- 宣告:資料參數 -----
            MGMTRepository _data = new MGMTRepository();

            //----- 方法:刪除資料 -----
            if (false == _data.Delete_MGHelpFiles(Get_DataID))
            {
                CustomExtension.AlertMsg("檔案刪除失敗", "");
                return;
            }
            else
            {
                //刪除檔案
                string ftpFolder = UploadFolder + Get_TraceID;
                _ftp.FTP_DelFile(ftpFolder, Get_FileName);

                //導向本頁
                Response.Redirect(thisPage);
            }

        }
    }

    protected void lv_Attachment_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取資料
                string _traceID = lt_TraceID.Text;
                string _attFile = DataBinder.Eval(dataItem.DataItem, "AttachFile").ToString();
                string _attFileOrg = DataBinder.Eval(dataItem.DataItem, "AttachFile_Org").ToString();
                string _CheckID = DataBinder.Eval(dataItem.DataItem, "Create_Who").ToString();

                //Set Url
                string url = "<a href=\"{0}{1}{2}/{3}\" target=\"_blank\">{4}</a>".FormatThis(
                    fn_Param.RefUrl
                    , UploadFolder
                    , string.IsNullOrEmpty(_CheckID) ? "" : _traceID //舊版沒有資料夾分類(故以此判斷)
                    , _attFile
                    , _attFileOrg
                    );

                //Set object
                Literal lt_FileUrl = (Literal)e.Item.FindControl("lt_FileUrl");
                lt_FileUrl.Text = url;
            }
        }
        catch (Exception)
        {
            throw;
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
        MGMTRepository _data = new MGMTRepository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMgHelpCCList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Inform.DataSource = query;
        this.lv_Inform.DataBind();

        //Release
        query = null;
    }

    #endregion


    #region -- 資料顯示:處理記錄 --
    /// <summary>
    /// 顯示處理記錄
    /// </summary>
    private void LookupData_Processes()
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMgHelpProcList(Req_DataID);

        //----- 資料整理:繫結 ----- 
        this.lv_ProcList.DataSource = query;
        this.lv_ProcList.DataBind();

        //Release
        query = null;
    }

    protected void lv_ProcList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string _ParentID = hf_DataID.Value;
            string _DetailID = ((HiddenField)e.Item.FindControl("hf_ID")).Value;
            string _TraceID = lt_TraceID.Text;

            //----- 宣告:資料參數 -----
            MGMTRepository _data = new MGMTRepository();

            //----- 方法:刪除資料 -----
            if (false == _data.Delete_MGHelpProcItem(_ParentID, _DetailID))
            {
                CustomExtension.AlertMsg("檔案刪除失敗", "");
                return;
            }
            else
            {
                //刪除檔案
                string ftpFolder = UploadFolder + _TraceID + "/" + _DetailID;
                _ftp.FTP_DelFolder(ftpFolder);

                //導向本頁
                Response.Redirect(thisPage + "#section2");
            }

        }
    }

    protected void lv_ProcList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                string _id = DataBinder.Eval(dataItem.DataItem, "DetailID").ToString();
                string _traceID = lt_TraceID.Text;

                //取得&設定時間:預計完成, 確認時間
                string _Wish_Time = DataBinder.Eval(dataItem.DataItem, "Wish_Time").ToString();
                Literal lt_TimeDesc1 = (Literal)e.Item.FindControl("lt_TimeDesc1");
                lt_TimeDesc1.Text = string.IsNullOrWhiteSpace(_Wish_Time) ? "" : "<div class=\"ui basic label\">預計完成<div class=\"detail\">" + _Wish_Time + "</div></div>";

                string _Confirm_Time = DataBinder.Eval(dataItem.DataItem, "Confirm_Time").ToString();
                Literal lt_TimeDesc2 = (Literal)e.Item.FindControl("lt_TimeDesc2");
                lt_TimeDesc2.Text = string.IsNullOrWhiteSpace(_Confirm_Time) ? "" : "<div class=\"ui basic label\">確認時間<div class=\"detail\">" + _Confirm_Time + "</div></div>";


                #region >> 附件 <<

                //附件Html
                Literal lt_AttachDesc = (Literal)e.Item.FindControl("lt_AttachDesc");
                lt_AttachDesc.Text = "";

                //----- 宣告:資料參數 -----
                MGMTRepository _data = new MGMTRepository();

                try
                {
                    var query = _data.GetMgHelpFileList(Req_DataID, _id, "B");
                    foreach (var item in query)
                    {
                        lt_AttachDesc.Text += "<a href=\"{0}\" class=\"ui tiny button\" target=\"_blank\"><i class=\"paperclip icon\"></i>{1}</a>".FormatThis(
                                fn_Param.RefUrl + UploadFolder + _traceID + "/" + _id + "/" + item.AttachFile
                                , item.AttachFile_Org
                            );
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

                #endregion

            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion



    #region -- 資料編輯:基本資料 --

    protected void btn_doSaveBase_Click(object sender, EventArgs e)
    {
        string currDataID = hf_DataID.Value;
        string errTxt = "";

        /* 需求者修改的當下，有可能狀態已改變，故在此卡一關 */
        //取得目前狀態
        string currSt = Get_CurrentStatus(Req_DataID);
        //處理狀態=未處理 -> 可編輯
        if (currSt.Equals("") || currSt.Equals("110"))
        {
            //皆可編輯
        }
        else
        {
            //有權限者才能編輯
            if (!_ReplyAuth)
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


        //*** 產生Guid ***
        string _guid = string.IsNullOrWhiteSpace(hf_DataID.Value) ? CustomExtension.GetGuid() : currDataID;
        //*** 產生TraceID ***
        string _traceID = string.IsNullOrWhiteSpace(hf_DataID.Value) ? NewTraceID() : lt_TraceID.Text;

        #region ** 儲存檔案 **

        /* 注意檔案路徑及資料夾(站台資料夾+guid) */
        if (ITempList.Count > 0)
        {
            int errCnt = 0;
            string ftpFolder = UploadFolder + _traceID; //ftp資料夾(站台資料夾+TraceId)

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
            Add_Data(ITempList, _guid, _traceID);
        }
        else
        {
            Edit_Data(ITempList, _guid);
        }
    }

    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data(List<IOTempParam> fileList, string guid, string traceID)
    {
        //是否轉寄
        bool sendCC = false;

        #region 資料處理:基本資料

        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        //----- 設定:資料欄位 -----
        string _Req_Class = ddl_ReqClass.SelectedValue;

        var data = new MgHelpData
        {
            DataID = new Guid(guid),
            TraceID = traceID,
            Help_Way = Convert.ToInt16(rbl_Help_Way.SelectedValue),
            Req_Class = Convert.ToInt16(_Req_Class),
            Req_Who = val_Emp.Text,
            Help_Subject = tb_ReqSubject.Text.Left(50),
            Help_Content = tb_ReqContent.Text.Left(5000),
            Help_Benefit = tb_Help_Benefit.Text.Left(5000)
        };


        //----- 方法:新增資料 -----
        if (!_data.CreateMgHelp_Base(data, out ErrMsg))
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
            List<MgHelpAttachment> dataItems = new List<MgHelpAttachment>();

            //----- 設定:資料欄位 -----
            for (int row = 0; row < fileList.Count; row++)
            {
                var dataItem = new MgHelpAttachment
                {
                    AttachFile = fileList[row].Param_FileName,
                    AttachFile_Org = fileList[row].Param_OrgFileName
                };

                dataItems.Add(dataItem);
            }

            //----- 方法:更新資料 -----
            if (false == _data.CreateMgHelp_Attachment(guid, "", "A", dataItems, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>新增檔案失敗</b><p>{0}</p>".FormatThis(ErrMsg);

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
            List<MgHelpCC> dataItems = new List<MgHelpCC>();

            //----- 設定:資料欄位 -----
            foreach (var item in query)
            {
                var dataItem = new MgHelpCC
                {
                    CC_Who = item.who
                };

                dataItems.Add(dataItem);
            }

            //----- 方法:更新資料 -----
            if (false == _data.CreateMgHelp_Inform(guid, dataItems, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>新增轉寄人員失敗</b><p>{0}</p>".FormatThis(ErrMsg);

                CustomExtension.AlertMsg("新增轉寄人員失敗", "");
            }

            //do send
            sendCC = true;
        }

        #endregion


        #region ### 通知信發送 ###

        /* [寄通知信]:執行單位 */
        if (!doSendInformMail("A", guid, out ErrMsg))
        {
            CustomExtension.AlertMsg("新需求通知信發送失敗.", "");
        }

        /* [寄通知信]:轉寄人員 */
        if (sendCC)
        {
            if (!doSendInformMail("B", guid, out ErrMsg))
            {
                CustomExtension.AlertMsg("轉寄人員通知信發送失敗.", "");
            }
        }

        /*[寄通知信]:權限申請,主管*/
        //if (_Req_Class.Equals("12"))
        //{
        //    if (!doSendInformMail("E", guid, out ErrMsg))
        //    {
        //        CustomExtension.AlertMsg("權限申請通知信發送失敗.", "");
        //    }

        //}

        #endregion


        //更新本頁Url
        //string thisUrl = "{0}/Edit/{1}".FormatThis(FuncPath(), guid);

        //導向列表頁
        Response.Redirect(Page_SearchUrl);
    }

    /// <summary>
    /// 需求資料:資料修改
    /// </summary>
    private void Edit_Data(List<IOTempParam> fileList, string guid)
    {
        #region 資料處理:基本資料

        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        //----- 設定:資料欄位 -----

        var data = new MgHelpData
        {
            DataID = new Guid(guid),
            Help_Way = Convert.ToInt16(rbl_Help_Way.SelectedValue),
            Req_Who = val_Emp.Text,
            Help_Subject = tb_ReqSubject.Text.Left(50),
            Help_Content = tb_ReqContent.Text.Left(5000),
            Help_Benefit = tb_Help_Benefit.Text.Left(5000)
        };

        //----- 方法:修改資料 -----
        if (!_data.Update_MGHelpBase(data, out ErrMsg))
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
            List<MgHelpAttachment> dataItems = new List<MgHelpAttachment>();

            //----- 設定:資料欄位 -----
            for (int row = 0; row < fileList.Count; row++)
            {
                var dataItem = new MgHelpAttachment
                {
                    AttachFile = fileList[row].Param_FileName,
                    AttachFile_Org = fileList[row].Param_OrgFileName
                };

                dataItems.Add(dataItem);
            }

            //----- 方法:更新資料 -----
            if (false == _data.CreateMgHelp_Attachment(guid, "", "A", dataItems, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增檔案失敗", "");
            }
        }

        #endregion


        //導向本頁
        Response.Redirect(thisPage);
    }

    #endregion


    #region -- 資料編輯:回覆資料 --
    protected void btn_doSaveReply_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        //[檢查] 有權限者才能編輯
        if (!_ReplyAuth)
        {
            CustomExtension.AlertMsg("無法編輯。\\n頁面將轉回列表頁..", Page_SearchUrl);
            return;
        }

        //取得欄位資料
        string _id = hf_DataID.Value;
        string _reqCls = ddl_ReqClass.SelectedValue;
        string _hour = tb_Finish_Hours.Text;
        string _wishdate = tb_Wish_Date.Text;
        string _replyCont = tb_ReplyContent.Text;

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_reqCls))
        {
            errTxt += "請填寫「需求類別」\\n";
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
        MGMTRepository _data = new MGMTRepository();

        try
        {
            //----- 設定:資料欄位 -----
            var data = new MgHelpData
            {
                DataID = new Guid(_id),
                Req_Class = Convert.ToInt32(_reqCls),
                Finish_Hours = string.IsNullOrWhiteSpace(_hour) ? 0 : Convert.ToDouble(_hour),
                Wish_Time = _wishdate,
                Reply_Content = _replyCont.Left(500),
            };

            //----- 方法:修改資料 -----
            if (!_data.Update_MGHelpReply(data, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>回覆資料儲存失敗</b><p>{0}</p><p>{1}</p>".FormatThis("被你用壞掉啦~~ 快求救!!!", ErrMsg);

                CustomExtension.AlertMsg("資料儲存失敗", "");
                return;
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

        #endregion


        //導向本頁
        Response.Redirect(thisPage + "#section1");

    }

    #endregion


    #region -- 資料編輯:處理記錄 --

    /// <summary>
    /// 新增處理記錄
    /// </summary>
    /// <remarks>
    /// 新增第一筆後，會將狀態設為處理中
    /// </remarks>
    protected void btn_doSaveProc_Click(object sender, EventArgs e)
    {
        string errTxt = "";
        Int32 _newDetailID = 0;

        //[檢查] 有權限者才能編輯
        if (!_ReplyAuth)
        {
            CustomExtension.AlertMsg("無法編輯。\\n頁面將轉回列表頁..", Page_SearchUrl);
            return;
        }

        //取得欄位資料
        string _traceID = lt_TraceID.Text;
        string _id = hf_DataID.Value;
        string _ProcClass = ddl_ProcClass.SelectedValue; //處理類別
        string _ProcDesc = tb_ProcDesc.Text; //說明
        string _ProcTime = tb_ProcTime.Text; //處理時間
        string _ConfirmTime = tb_ConfirmTime.Text; //需求確認時間
        string _WishTime = tb_WishTime.Text;

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_ProcClass))
        {
            errTxt += "請填寫「處理類別」\\n";
        }
        if (string.IsNullOrWhiteSpace(_ProcDesc))
        {
            errTxt += "請填寫「說明」\\n";
        }
        if (string.IsNullOrWhiteSpace(_ProcTime))
        {
            errTxt += "請填寫「處理時間」\\n";
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
        MGMTRepository _data = new MGMTRepository();

        try
        {
            //----- 設定:資料欄位 -----
            var data = new MgHelpProc
            {
                Class_ID = Convert.ToInt32(_ProcClass),
                Proc_Desc = _ProcDesc,
                Proc_Time = _ProcTime,
                Confirm_Time = _ConfirmTime,
                Wish_Time = _WishTime
            };

            //----- 方法:修改資料 -----
            _newDetailID = _data.CreateMgHelp_Proc(_id, data, out ErrMsg);
            if (_newDetailID.Equals(0))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>處理記錄儲存失敗</b><p>{0}</p><p>{1}</p>".FormatThis("被你用壞掉啦~~ 快求救!!!", ErrMsg);

                CustomExtension.AlertMsg("資料儲存失敗", "");
                return;
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

        #endregion

        //附件上傳處理
        doProcUpload(_id, _newDetailID.ToString(), _traceID);

        //導向本頁
        Response.Redirect(thisPage + "#section2");

    }

    /// <summary>
    /// 附件上傳
    /// </summary>
    /// <param name="_id">單頭編號</param>
    /// <param name="_detailID">單身編號</param>
    /// <param name="_traceID">追蹤編號</param>
    /// <remarks>
    /// 資料夾+TraceId+單身編號
    /// </remarks>
    private void doProcUpload(string _id, string _detailID, string _traceID)
    {
        string errTxt = "";

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

            //*不鎖副檔名*
            //if (hpf.ContentLength > 0)
            //{
            //    //取得原始檔名
            //    string OrgFileName = Path.GetFileName(hpf.FileName);
            //    //取得副檔名
            //    string FileExt = Path.GetExtension(OrgFileName).ToLower();
            //    if (false == CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
            //    {
            //        //[提示]
            //        errTxt += "「{0}」副檔名不符規定, 僅可上傳「{1}」\\n".FormatThis(OrgFileName, FileExtLimit.Replace("|", ", "));
            //    }
            //}
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


                //設定暫存-檔案
                ITempList.Add(new IOTempParam(myFullFile, OrgFileName, hpf));
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

        /* 注意檔案路徑及資料夾*/
        if (ITempList.Count > 0)
        {
            int errCnt = 0;
            string ftpFolder = UploadFolder + _traceID + "/" + _detailID; //ftp資料夾(站台資料夾+TraceId+單身編號)

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


        #region 資料處理:檔案附件

        if (ITempList.Count > 0)
        {
            //----- 宣告:資料參數 -----
            MGMTRepository _data = new MGMTRepository();
            List<MgHelpAttachment> dataItems = new List<MgHelpAttachment>();

            try
            {
                //----- 設定:資料欄位 -----
                for (int row = 0; row < ITempList.Count; row++)
                {
                    var dataItem = new MgHelpAttachment
                    {
                        AttachFile = ITempList[row].Param_FileName,
                        AttachFile_Org = ITempList[row].Param_OrgFileName
                    };

                    dataItems.Add(dataItem);
                }

                //----- 方法:更新資料 -----
                if (false == _data.CreateMgHelp_Attachment(_id, _detailID, "B", dataItems, out ErrMsg))
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
            }

        }

        #endregion
    }


    #endregion


    #region -- 資料編輯:驗收意見 --
    protected void btn_doSaveRate_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        //取得欄位資料
        string _id = hf_DataID.Value;
        string _RateScore = rbl_RateScore.SelectedValue;
        string _RateContent = tb_RateContent.Text;

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_RateContent))
        {
            errTxt += "請填寫「驗收意見」\\n";
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
        MGMTRepository _data = new MGMTRepository();

        try
        {
            //----- 設定:資料欄位 -----
            var data = new MgHelpData
            {
                DataID = new Guid(_id),
                RateScore = Convert.ToInt16(_RateScore),
                RateContent = _RateContent.Left(400),
            };

            //----- 方法:修改資料 -----
            if (!_data.Update_MGHelpRate(data, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>驗收意見儲存失敗</b><p>{0}</p><p>{1}</p>".FormatThis("遇到無法排除的錯誤，請聯絡系統管理員。", ErrMsg);

                CustomExtension.AlertMsg("驗收意見失敗", "");
                return;
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
        #endregion


        //導向List
        CustomExtension.AlertMsg("填寫完畢。\\n頁面將轉回列表頁..", Page_SearchUrl);

    }

    #endregion


    #region -- 資料編輯:管理者專用按鈕 --
    /// <summary>
    /// *** 結案 ***
    /// </summary>
    protected void btn_Finish_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        //[檢查] 有權限者才能編輯
        if (!_ReplyAuth)
        {
            CustomExtension.AlertMsg("無法編輯。\\n頁面將轉回列表頁..", Page_SearchUrl);
            return;
        }

        //取得欄位資料
        string _id = hf_DataID.Value;
        string _reqCls = ddl_ReqClass.SelectedValue;
        string _hour = tb_Finish_Hours.Text;
        string _wishdate = tb_Wish_Date.Text;
        string _replyCont = tb_ReplyContent.Text;
        string _FinishTime = val_FinishTime.Text;

        #region ** 欄位判斷 **
        if (string.IsNullOrWhiteSpace(_hour))
        {
            errTxt += "請填寫「工時」\\n";
        }
        if (string.IsNullOrWhiteSpace(_FinishTime))
        {
            errTxt += "請填寫「結案時間」\\n";
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
        MGMTRepository _data = new MGMTRepository();

        try
        {
            //----- 設定:資料欄位 -----
            var data = new MgHelpData
            {
                DataID = new Guid(_id),
                Req_Class = Convert.ToInt32(_reqCls),
                Reply_Content = _replyCont.Left(500),
                Wish_Time = _wishdate,
                Finish_Hours = string.IsNullOrWhiteSpace(_hour) ? 0 : Convert.ToDouble(_hour),
                Finish_Time = _FinishTime.ToDateString("yyyy/MM/dd HH:mm"),
            };

            //----- 方法:修改資料 -----
            if (!_data.Update_MGHelpClose(data, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>結案失敗</b><p>{0}</p><p>{1}</p>".FormatThis("被你用壞掉啦~~ 快求救!!!", ErrMsg);

                CustomExtension.AlertMsg("結案失敗", "");
                return;
            }

            #region ### 通知信發送 ###
            /* [寄通知信]:結案通知(固定收件人/轉寄人員/需求者) */
            if (!doSendInformMail("D", _id, out ErrMsg))
            {
                CustomExtension.AlertMsg("結案成功，通知信發送失敗.", "");
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


        //導向List
        Response.Redirect(Page_SearchUrl);

    }

    /// <summary>
    /// *** 置頂 ***
    /// </summary>
    protected void lbtn_doTrace_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        try
        {
            if (!_data.Update_MGHelpSetTop(Req_DataID, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>追蹤失敗</b><p>{0}</p><p>{1}</p>".FormatThis("被你用壞掉啦~~ 快求救!!!", ErrMsg);

                CustomExtension.AlertMsg("追蹤失敗", "");
                return;
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

        //導向本頁
        Response.Redirect(thisPage + "#section1");
    }

    /// <summary>
    /// *** 自訂通知信 ***
    /// </summary>
    protected void btn_Inform_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        //[檢查] 有權限者才能編輯
        if (!_ReplyAuth)
        {
            CustomExtension.AlertMsg("無法執行。\\n頁面將轉回列表頁..", Page_SearchUrl);
            return;
        }

        //取得欄位資料
        string _id = hf_DataID.Value;
        //string _type = val_InformType.Text;
        string _cont = val_MailCont.Text;

        #region ** 欄位判斷 **
        if (string.IsNullOrWhiteSpace(_id))
        {
            errTxt += "「資料編號空白」\\n";
        }
        //if (string.IsNullOrWhiteSpace(_type))
        //{
        //    errTxt += "請選擇「通知類型」\\n";
        //}
        if (string.IsNullOrWhiteSpace(_cont))
        {
            errTxt += "請填寫「通知內文」\\n";
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
        MGMTRepository _data = new MGMTRepository();

        try
        {
            ////----- 方法:修改資料 -----
            //if (_type.Equals("B"))
            //{
            //    if (!_data.Update_MGHelpStatus(_id, "C"))
            //    {
            //        this.ph_ErrMessage.Visible = true;
            //        this.lt_ShowMsg.Text = "<b>狀態修改失敗,設定「測試中」</b><p>{0}</p><p>{1}</p>".FormatThis("被你用壞掉啦~~ 快求救!!!", ErrMsg);

            //        CustomExtension.AlertMsg("狀態修改失敗", "");
            //        return;
            //    }
            //}


            #region ### 通知信發送 ###

            /* [寄通知信]:自訂通知(需求者) */
            if (!doSendInformMail("C", _id, _cont.Replace("\r\n", "<br/>"), out ErrMsg))
            {
                CustomExtension.AlertMsg("通知信發送失敗.", "");
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


        //導向本頁
        Response.Redirect(thisPage + "#section1");
    }


    /// <summary>
    /// 主管核准
    /// </summary>
    protected void lbtn_doApprove_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        //[檢查] 有權限者才能編輯
        if (!_ReplyAuth)
        {
            CustomExtension.AlertMsg("無法執行。\\n頁面將轉回列表頁..", Page_SearchUrl);
            return;
        }

        //取得欄位資料
        string _id = hf_DataID.Value;

        #region ** 欄位判斷 **
        if (string.IsNullOrWhiteSpace(_id))
        {
            errTxt += "「資料編號空白」\\n";
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
        MGMTRepository _data = new MGMTRepository();

        try
        {
            //----- 方法:修改資料 -----
            if (!_data.Update_MGHelpSetApprove(_id, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>狀態修改失敗,設定「主管同意」</b><p>{0}</p><p>{1}</p>".FormatThis("被你用壞掉啦~~ 快求救!!!", ErrMsg);

                CustomExtension.AlertMsg("狀態修改失敗", "");
                return;
            }


            #region ### 通知信發送 ###

            /* [寄通知信]:核准申請 */
            if (!doSendInformMail("E", _id, out ErrMsg))
            {
                CustomExtension.AlertMsg("核准申請通知信發送失敗.", "");
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


        //導向本頁
        CustomExtension.AlertMsg("已通知主管", thisPage + "#section1");
        return;
    }

    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// New TraceID
    /// </summary>
    /// <returns></returns>
    private string NewTraceID()
    {
        //產生TraceID
        long ts = Cryptograph.GetCurrentTime();

        Random rnd = new Random();
        int myRnd = rnd.Next(1, 99);

        return "{0}{1}".FormatThis(ts, myRnd);
    }


    /// <summary>
    /// 取得目前最新狀態
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private string Get_CurrentStatus(string id)
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        try
        {
            return _data.GetOne_MGHelpStatus(id);
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


    /// <summary>
    /// 取得類別
    /// </summary>
    /// <param name="clsType">A:處理狀態, B:需求類別, C:處理記錄類別</param>
    /// <param name="ddl"></param>
    /// <param name="rootName"></param>
    /// <param name="inputValue"></param>
    private void Get_ClassList(string clsType, DropDownList ddl, string rootName, string inputValue)
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        //----- 原始資料:取得所有資料 -----
        DataTable query = _data.GetClass_MGhelp(clsType, out ErrMsg);

        //----- 資料整理 -----
        ddl.Items.Clear();

        if (!string.IsNullOrEmpty(rootName))
        {
            ddl.Items.Add(new ListItem(rootName, ""));
        }

        for (int row = 0; row < query.Rows.Count; row++)
        {
            ddl.Items.Add(new ListItem(
                query.Rows[row]["Label"].ToString()
                , query.Rows[row]["ID"].ToString()
                ));
        }

        //被選擇值
        if (!string.IsNullOrWhiteSpace(inputValue))
        {
            ddl.SelectedIndex = ddl.Items.IndexOf(ddl.Items.FindByValue(inputValue));
        }

        query = null;
    }

    #endregion


    #region -- 通知信 --
    private bool doSendInformMail(string sendType, string guid, out string ErrMsg)
    {
        return doSendInformMail(sendType, guid, "", out ErrMsg);
    }

    /// <summary>
    /// 發通知信
    /// </summary>
    /// <param name="_sendType">A:新需求/B:轉寄通知/C:自訂通知/D:結案/E:主管核准</param>
    /// <param name="_guid">資料編號</param>
    /// <param name="_customInfo">自訂通知內文(type=C時使用)</param>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    private bool doSendInformMail(string _sendType, string _guid, string _customInfo, out string ErrMsg)
    {
        ErrMsg = "";
        bool doSend = true;
        string mailSubject = "";

        //設定主旨前置文字
        switch (_sendType.ToUpper())
        {
            case "A":
                //新需求
                mailSubject = "[管理工作需求][新需求]";
                break;

            case "B":
                //轉寄通知
                mailSubject = "[管理工作需求][轉寄]";
                break;

            case "C":
                //自訂通知
                mailSubject = "[管理工作需求][處理通知]";
                break;

            case "D":
                //結案
                mailSubject = "[管理工作需求][結案]";
                break;

            case "E":
                //需求核准
                mailSubject = "[管理工作需求][需求核准]";
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
        MGMTRepository _data = new MGMTRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", _guid);

        //[資料取得] - 基本資料
        var baseData = _data.GetOne_MGhelp(search, out ErrMsg).Take(1).FirstOrDefault();
        if (baseData == null)
        {
            ErrMsg = "查無資料";
            return false;
        }
        _data = null;

        //[設定] 郵件主旨
        mailSubject += "{0} #{1}".FormatThis(baseData.Help_Subject, baseData.TraceID);

        //[設定] 郵件內容(Call Get_MailContent)
        StringBuilder mailBoday = Get_MailContent(_sendType, _guid, _customInfo, baseData);

        //[設定] 取得收件人(Call Get_MailList)
        ArrayList mailList = Get_MailList(_sendType, _guid, baseData.Req_Email, baseData.Req_Dept);

        //判斷是否有收件人
        if (mailList.Count == 0)
        {
            ErrMsg = "無收件人";
            return false;
        }

        //*** 開始發送通知信 ***
        if (!Send_Email(mailList, mailSubject, mailBoday, out ErrMsg))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 取得收件人清單
    /// </summary>
    /// <param name="sendType">A:新需求,B:轉寄通知,C:自訂通知,D:結案,E:權限申請</param>
    /// <param name="dataID"></param>
    /// <param name="reqEmail">需求者Email</param>
    /// <param name="reqDeptID">需求者部門代號</param>
    /// <returns></returns>
    private ArrayList Get_MailList(string sendType, string dataID, string reqEmail, string reqDeptID)
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();
        ArrayList mailList = new ArrayList();

        switch (sendType)
        {
            case "A":
                //新需求:固定收件人(新需求=1 / 結案=2)
                var dataA = _data.GetMgHelpReceiver("1");
                foreach (var item in dataA)
                {
                    mailList.Add(item.Email);
                }
                dataA = null;

                ////一律通知主管20210304
                //var dataAup = fn_CustomUI.emailReceiver_Supervisor(reqDeptID);
                //foreach (var item in dataAup)
                //{
                //    mailList.Add(item);
                //}
                //dataAup = null;

                break;

            case "B":
                //轉寄通知:CC設定人員
                var dataB = _data.GetMgHelpCCList(dataID);
                foreach (var item in dataB)
                {
                    mailList.Add(item.CC_Email);
                }
                dataB = null;

                break;

            case "C":
                //自訂通知:通知需求者
                mailList.Add(reqEmail);

                ////一律通知主管20210304
                //var dataCup = fn_CustomUI.emailReceiver_Supervisor(reqDeptID);
                //foreach (var item in dataCup)
                //{
                //    mailList.Add(item);
                //}
                //dataCup = null;

                break;

            case "D":
                //結案:固定收件人(新需求=1 / 結案=2)
                var dataD = _data.GetMgHelpReceiver("2");
                foreach (var item in dataD)
                {
                    mailList.Add(item.Email);
                }
                dataD = null;
                //轉寄人員
                var dataD1 = _data.GetMgHelpCCList(dataID);
                foreach (var item in dataD1)
                {
                    mailList.Add(item.CC_Email);
                }
                dataD1 = null;

                ////一律通知主管20210304
                //var dataDup = fn_CustomUI.emailReceiver_Supervisor(reqDeptID);
                //foreach (var item in dataDup)
                //{
                //    mailList.Add(item);
                //}
                //dataDup = null;


                //需求者
                mailList.Add(reqEmail);

                break;

            case "E":
                //主管核准通知信:部門主管Email
                UsersRepository _datalist = new UsersRepository();
                var dataE = _datalist.GetDeptSupervisor(reqDeptID);
                foreach (var item in dataE)
                {
                    mailList.Add(item.Email);
                }
                dataE = null;
                _datalist = null;

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
    /// <param name="_customInfo">自訂通知內文</param>
    /// <param name="baseData"></param>
    /// <returns></returns>
    private StringBuilder Get_MailContent(string sendType, string guid, string _customInfo, MgHelpData baseData)
    {
        //宣告
        StringBuilder html = new StringBuilder();
        MGMTRepository _data = new MGMTRepository();

        //Html模版路徑(From CDN)
        string url = "{0}PKEF/twMGHelp/Mail_TW.html?v=1.0".FormatThis(fn_Param.CDNUrl);

        //取得HTML模版(Html不可放在本機)
        string htmlPage = CustomExtension.WebRequest_byGET(url);

        //加入模版內容
        html.Append(htmlPage);

        //[取代指定內容]:郵件固定內容
        string msg = "";
        string editUrl = "{0}MGHelp_Edit.aspx?id={1}".FormatThis(FuncPath(), guid);
        string viewUrl = "{0}MGHelp_View.aspx?id={1}".FormatThis(FuncPath(), guid);
        string pageUrl = "";
        switch (sendType)
        {
            case "A":
                msg = "<p>目前有新的管理工作需求，請前往處理。</p>";
                pageUrl = editUrl + "#section1";
                break;

            case "B":
                msg = "此信為管理工作需求轉寄通知。";
                pageUrl = viewUrl;
                break;

            case "C":
                msg = _customInfo;
                pageUrl = editUrl;
                break;

            case "D":
                msg = "需求已結案，請前往填寫滿意度調查及驗收意見。";
                pageUrl = viewUrl + "#section3";
                break;

            case "E":
                msg = "本件需求需要主管核准，請點下方按鈕「前往查看更多」進行處理。";
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
        string _traceID = baseData.TraceID;
        html.Replace("#TraceID#", _traceID);
        html.Replace("#CreateDate#", baseData.Create_Time.ToDateString("yyyy/MM/dd"));
        html.Replace("#TypeName#", baseData.Req_ClassName);
        html.Replace("#ReqName#", baseData.Req_WhoName + " (" + baseData.Req_NickName + ") #" + baseData.Req_TelExt);
        html.Replace("#subject#", baseData.Help_Subject);
        html.Replace("#content#", baseData.Help_Content.Replace("\r", "<br/>"));

        //工時欄位display
        //html.Replace("#Disp1#", baseData.Help_Status.Equals(140) ? "" : "display:none");
        //html.Replace("#FinishHours#", baseData.Finish_Hours.ToString());
        //html.Replace("#FinishDate#", baseData.Finish_Time.ToDateString("yyyy/MM/dd"));

        baseData = null;


        //[資料取得] - 處理進度
        //var replyData = _data.GetMKHelpReplyList(guid);
        //html.Replace("#Disp2#", replyData.Count() == 0 ? "display:none" : "");

        //if (replyData.Count() > 0)
        //{
        //    string loopHtml = "";
        //    foreach (var data in replyData)
        //    {
        //        loopHtml += "<tr>";
        //        loopHtml += " <td colspan=\"4\">";
        //        loopHtml += " <p><font style=\"font-size: 12px; color: #4183c4;\">{0}</font>&nbsp;<font style=\"font-size: 12px; color: #757575 ;\">{1}</font>"
        //            .FormatThis(data.Create_Name, data.Create_Time);
        //        loopHtml += " </p>";
        //        loopHtml += " <p>{0}</p>".FormatThis(data.Reply_Content.ToString().Replace("\r", "<br/>"));
        //        loopHtml += " </td>";
        //        loopHtml += "</tr>";
        //    }

        //    html.Replace("#replyItems#", loopHtml);
        //}
        //replyData = null;


        //[資料取得] - 附件清單
        var fileData = _data.GetMgHelpFileList(guid, "", "A");
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
                        , _traceID
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
    private bool Send_Email(ArrayList mailList, string subject, StringBuilder mailBody, out string ErrMsg)
    {
        try
        {
            //開始發信
            using (MailMessage Msg = new MailMessage())
            {
                //寄件人
                Msg.From = new MailAddress(fn_Param.SysMail_Sender, "寶工鋼鐵人");

                //收件人
                var query = from string x in mailList
                            group x by x.ToString() into g
                            select new
                            {
                                Text = g.Key
                            };

                foreach (var email in query)
                {
                    Msg.To.Add(new MailAddress(email.Text));
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

    /// <summary>
    /// 前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}twManagement/".FormatThis(fn_Param.WebUrl);
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
            String _data = Request.QueryString["id"];

            return string.IsNullOrWhiteSpace(_data) ? "" : _data.Trim();
        }
        set
        {
            _Req_DataID = value;
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
            string tempUrl = CustomExtension.getCookie("MGHelp");

            return string.IsNullOrWhiteSpace(tempUrl) ? fn_Param.WebUrl + "twManagement/MGHelp_Search.aspx" : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
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
            return "{0}twManagement/MGHelp_Edit.aspx?id={1}".FormatThis(fn_Param.WebUrl, Req_DataID);
        }
        set
        {
            _thisPage = value;
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
            return "{0}MG_Help/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myOpcsRemark_CustEdit : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            #region --權限--
            //[權限判斷] Start
            bool isPass = false;
            switch (Req_DBS)
            {
                case "SH":
                    //SH
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3312");
                    break;

                default:
                    //TW
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3311");
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
                Page.Title += "(" + Req_DBS + ")";

                //無資料,自動新增
                if (string.IsNullOrWhiteSpace(Req_DataID))
                {
                    Add_Data();
                    return;
                }

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
        var query = _data.GetOne_CustRemk(search, Req_DBS, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }


        #region >> 欄位填寫 <<

        //--- 填入基本資料 ---
        string _custID = query.Rows[0]["CustID"].ToString();
        lt_CustID.Text = _custID;
        lt_CustName.Text = query.Rows[0]["CustName"].ToString();
        tb_Remk_Normal.Text = query.Rows[0]["Remk_Normal"].ToString();
        tb_Remk_2210.Text = query.Rows[0]["Remk_2210"].ToString();

        hf_CustID.Value = _custID;
        hf_DataID.Value = query.Rows[0]["Data_ID"].ToString();

        #endregion

        //維護資訊
        info_Creater.Text = query.Rows[0]["Create_Name"].ToString();
        info_CreateTime.Text = query.Rows[0]["Create_Time"].ToString();
        info_Updater.Text = query.Rows[0]["Update_Name"].ToString();
        info_UpdateTime.Text = query.Rows[0]["Update_Time"].ToString();

        //-- 載入其他資料 --
        LookupData_Files();


        //Release
        _data = null;

    }

    #endregion


    #region -- 資料編輯:基本資料 --

    //Save
    protected void btn_doSave_Click(object sender, EventArgs e)
    {
        Edit_Data();
    }

    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        try
        {
            //----- 方法:新增資料 -----
            Int32 _id = _data.Check_CustRemk(Req_CustID, Req_DBS, out ErrMsg);

            if (_id == 0)
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>資料新增失敗</b><p>{0}</p><p>{1}</p>".FormatThis("遇到無法排除的錯誤，請聯絡系統管理員。", ErrMsg);

                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }

            //更新本頁Url
            string thisUrl = "{0}CustEdit.aspx?dbs={1}&id={2}".FormatThis(FuncPath(), Req_DBS, _id);
            //Redirect
            Response.Redirect(thisUrl);
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
    /// 需求資料:資料修改
    /// </summary>
    private void Edit_Data()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        try
        {
            //----- 設定:資料欄位 -----
            string _id = hf_DataID.Value;
            string _remk1 = tb_Remk_Normal.Text;
            string _remk2 = tb_Remk_2210.Text;

            //----- 方法:修改資料 -----
            if (!_data.Update_CustRemk(_id, _remk1, _remk2, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>資料修改失敗</b><p>{0}</p><p>{1}</p>".FormatThis("遇到無法排除的錯誤，請聯絡系統管理員。", ErrMsg);

                CustomExtension.AlertMsg("修改失敗", "");
                return;
            }


            //導向本頁
            CustomExtension.AlertMsg("修改成功", thisPage);
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


    #region -- 資料顯示:檔案附件 --

    /// <summary>
    /// 顯示檔案附件
    /// </summary>
    private void LookupData_Files()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.Get_OpcsRemkFiles(Req_DataID, "A", out ErrMsg);

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
                string _custID = hf_CustID.Value;

                //----- 方法:刪除資料 -----
                if (false == _data.Delete_OpcsRemkFile(Get_DataID))
                {
                    CustomExtension.AlertMsg("檔案刪除失敗", "");
                    return;
                }
                else
                {
                    //刪除檔案
                    string ftpFolder = UploadFolder + Req_DBS + "/" + _custID;
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



    #region -- 按鈕事件 --

    /// <summary>
    /// 上傳附件
    /// </summary>
    protected void btn_SaveDetail_Click(object sender, EventArgs e)
    {
        string errTxt = "";
        string _custID = hf_CustID.Value;

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
            string ftpFolder = UploadFolder + Req_DBS + "/" + _custID; //ftp資料夾(站台資料夾+guid)

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
                        AttachFile = fileList[row].Param_FileName,
                        AttachFile_Org = fileList[row].Param_OrgFileName,
                        Create_Who = fn_Param.CurrentUser
                    };

                    dataItems.Add(dataItem);
                }

                //----- 方法:更新資料 -----
                if (false == _data.Create_OpcsRemkFiles(dataItems, Req_DataID, "A", out ErrMsg))
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


    #region -- 網址參數 --
    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}myOpcsRemark/".FormatThis(fn_Param.WebUrl);
    }

    #endregion


    #region -- 傳遞參數 --
    /// <summary>
    /// 取得傳遞參數 - DBS
    /// </summary>
    public string Req_DBS
    {
        get
        {
            String _data = Request.QueryString["dbs"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "4", out ErrMsg)) ? _data.Trim() : "TW";
        }
        set
        {
            this._Req_DBS = value;
        }
    }
    private string _Req_DBS;


    /// <summary>
    /// 取得傳遞參數 - 客編
    /// </summary>
    private string _Req_CustID;
    public string Req_CustID
    {
        get
        {
            String data = Request.QueryString["cust"];

            return data;
        }
        set
        {
            _Req_CustID = value;
        }
    }


    /// <summary>
    /// 取得傳遞參數 - 資料編號
    /// </summary>
    private string _Req_DataID;
    public string Req_DataID
    {
        get
        {
            String DataID = Request.QueryString["id"];

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
            return "{0}CustEdit.aspx?dbs={1}&id={2}".FormatThis(FuncPath(), Req_DBS, Req_DataID);
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
            string tempUrl = CustomExtension.getCookie("CustRemk");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() + "CustSearch.aspx" : Server.UrlDecode(tempUrl);
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
            return "jpg|png|pdf|docx|xlsx";
        }
        set
        {
            _FileExtLimit = value;
        }
    }

    /// <summary>
    /// 限制上傳的檔案大小(1MB = 1024000), 10MB
    /// </summary>
    private int _FileSizeLimit;
    public int FileSizeLimit
    {
        get
        {
            return 10240000;
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
            return "{0}OpcsRemk/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using LinqToExcel;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myECdata_SZ_ImportPrice : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {

            //[權限判斷] Start
            #region --權限--
            /* 
             * 判斷對應的MENU ID
             * 取得其他權限
             */
            bool isPass = false;
            isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3750");


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
                Get_ClassList("1", ddl_Mall1);
                Get_ClassList("2", ddl_Mall2);

            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料處理 --

    protected void btn_Save1_Click(object sender, EventArgs e)
    {
        string _mallID = ddl_Mall1.SelectedValue;
        string _mallName = "工具-" + ddl_Mall1.SelectedItem.Text;
        do_Upload(1, _mallID, _mallName, fu_File1);
    }

    protected void btn_Save2_Click(object sender, EventArgs e)
    {
        string _mallID = ddl_Mall2.SelectedValue;
        string _mallName = "科學玩具-" + ddl_Mall1.SelectedItem.Text;
        do_Upload(2, _mallID, _mallName, fu_File2);
    }

    private void do_Upload(Int16 _type, string _mallID, string _mallName, FileUpload filectrl)
    {
        #region -- 檔案處理 --

        //宣告
        List<IOTempParam> ITempList = new List<IOTempParam>();
        Random rnd = new Random();
        string Message = "";
        string ftpFolder = UploadFolder; //FTP資料夾
        string thisFileName = ""; //檔名

        if (filectrl.PostedFile.ContentLength == 0)
        {
            CustomExtension.AlertMsg("請選擇要上傳的檔案", thisPage);
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
                CustomExtension.AlertMsg(Message, thisPage);
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
                    CustomExtension.AlertMsg(Message, thisPage);
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
            CustomExtension.AlertMsg("請選擇要上傳的檔案", thisPage);
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
            CustomExtension.AlertMsg(Message, thisPage);
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

        ////取得Excel資料欄位
        var query_Xls = _data.GetEC_ExcelData(_filePath, sheetData);

        try
        {
            //儲存資料
            if (!_data.CreateECD_PriceList(query_Xls, _type, Convert.ToInt32(_mallID), out ErrMsg))
            {
                CustomExtension.AlertMsg("資料匯入失敗", thisPage);
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
        CustomExtension.AlertMsg(_mallName + ",匯入完成.", thisPage);
    }

    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 取得類別資料 
    /// </summary>
    /// <param name="typeID">typeID</param>
    /// <param name="ddl">下拉選單object</param>
    private void Get_ClassList(string typeID, DropDownList ddl)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetEC_RefMall(typeID, "zh_TW", out ErrMsg);


        //----- 資料整理 -----
        ddl.Items.Clear();

        foreach (var item in query)
        {
            ddl.Items.Add(new ListItem(item.Label, item.ID.ToString()));
        }

        query = null;
    }

    #endregion


    #region -- 傳遞參數 --

    /// <summary>
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}myECdata_SZ/ImportPrice.aspx".FormatThis(fn_Param.WebUrl);
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
            return "xlsx";
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
            return 1;
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
            return "{0}eCommerceData/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LinqToExcel;
using PKLib_Method.Methods;
using ShipFreight_CN.Controllers;
using ShipFreight_CN.Models;

public partial class myShipping_ImportList : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;
                isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3779");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[權限判斷] End
                #endregion


                //Create Menu
                CreateMenu_Year(this.filter_Year);
                CreateMenu_Month(this.filter_Month);


                #region --Request參數--
                //[取得/檢查參數] - Req_Year
                if (!string.IsNullOrWhiteSpace(Req_Year))
                {
                    this.filter_Year.SelectedValue = Req_Year;
                }
                //[取得/檢查參數] - Req_Month
                if (!string.IsNullOrWhiteSpace(Req_Month))
                {
                    this.filter_Month.SelectedValue = Req_Month;
                }
                #endregion

                //Get Data
                LookupDataList();

            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示 --

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupDataList()
    {        
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        string inputY = Req_Year;
        string inputM = Req_Month;


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCustCntStat(inputY, inputM, search, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        this.lvDataList.DataSource = query;
        this.lvDataList.DataBind();


        //----- 分區顯示 ----- 
        if (query.Rows.Count == 0)
        {
            this.ph_EmptyData.Visible = true;
            this.ph_Data.Visible = false;

        }
        else
        {
            this.ph_EmptyData.Visible = false;
            this.ph_Data.Visible = true;

        }
    }

    int total1 = 0;
    int total2 = 0;

    //金額加總
    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                total1 += Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "doSentCnt"));
                total2 += Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "SendCnt"));
            }

        }
        catch (Exception)
        {
            throw;
        }
    }

    //顯示總計金額
    protected void lvDataList_DataBound(object sender, EventArgs e)
    {
        try
        {
            if (lvDataList.Items.Count > 0)
            {
                //代發筆數
                Label lb_Tot1 = (Label)lvDataList.FindControl("lb_Tot1");
                lb_Tot1.Text = total1.ToString();

                Label head_Tot1 = (Label)lvDataList.FindControl("head_Tot1");
                head_Tot1.Text = total1.ToString();


                //限制筆數
                Label lb_Tot2 = (Label)lvDataList.FindControl("lb_Tot2");
                lb_Tot2.Text = total2.ToString();

                Label head_Tot2 = (Label)lvDataList.FindControl("head_Tot2");
                head_Tot2.Text = total2.ToString();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }
    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 查詢
    /// </summary>
    protected void btn_Search_Click(object sender, EventArgs e)
    {
        //執行查詢
        Response.Redirect(filterUrl(), false);
    }

    #endregion


    #region -- 附加功能 --

    protected void CreateMenu_Year(DropDownList item)
    {
        int currYear = DateTime.Now.Year;
        int prevYear = currYear - 1;
        int nextYear = currYear;

        item.Items.Clear();
        for (int itemY = prevYear; itemY <= nextYear; itemY++)
        {
            item.Items.Add(new ListItem(itemY.ToString(), itemY.ToString()));
        }

        item.SelectedValue = currYear.ToString();
    }

    protected void CreateMenu_Month(DropDownList item)
    {
        int currMonth = DateTime.Now.Month;
        item.Items.Clear();
        for (int row = 1; row <= 12; row++)
        {
            item.Items.Add(new ListItem(row.ToString(), row.ToString()));
        }

        item.SelectedValue = currMonth.ToString();
    }

    /// <summary>
    /// 含查詢條件的完整網址
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _Year = this.filter_Year.SelectedValue;
        string _Month = this.filter_Month.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?s=1".FormatThis(thisPage));

        //[查詢條件] - Year
        if (!string.IsNullOrWhiteSpace(_Year))
        {
            url.Append("&yy=" + Server.UrlEncode(_Year));
        }
        //[查詢條件] - Month
        if (!string.IsNullOrWhiteSpace(_Month))
        {
            url.Append("&mm=" + Server.UrlEncode(_Month));
        }

        return url.ToString();
    }

    /// <summary>
    /// [按鈕] - 匯入客戶代發
    /// </summary>
    protected void btn_ImportShipNo_Click(object sender, EventArgs e)
    {
        string goUrl = thisPage;

        #region -- 檔案處理 --

        //宣告
        List<IOTempParam> ITempList = new List<IOTempParam>();
        Random rnd = new Random();
        string Message = "";
        string ftpFolder = UploadFolder; //FTP資料夾
        string thisFileName = ""; //檔名

        //Check 上傳控制項
        if (shipNoImport.PostedFile.ContentLength == 0)
        {
            CustomExtension.AlertMsg("請選擇要上傳的檔案", goUrl);
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
                CustomExtension.AlertMsg(Message, goUrl);
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
                    CustomExtension.AlertMsg(Message, goUrl);
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
            CustomExtension.AlertMsg("請選擇要上傳的檔案", goUrl);
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
            CustomExtension.AlertMsg(Message, goUrl);
            return;
        }

        #endregion


        #region -- 資料處理 --

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

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
        var query_Xls = _data.GetExcel_CustCntData(_filePath, sheetData);

        try
        {
            //回寫
            if (!_data.Check_CustCnt(query_Xls, out ErrMsg))
            {
                //Response.Write(ErrMsg);
                CustomExtension.AlertMsg("資料匯入失敗", goUrl);
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
        CustomExtension.AlertMsg("匯入完成.", goUrl);

    }

    #endregion


    #region -- 傳遞參數 --
    /// <summary>
    /// 取得傳遞參數 - Year
    /// </summary>
    public string Req_Year
    {
        get
        {
            String _data = Request.QueryString["yy"];
            string dt = DateTime.Now.Year.ToString();
            return (CustomExtension.String_資料長度Byte(_data, "1", "4", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_Year = value;
        }
    }
    private string _Req_Year;

    /// <summary>
    /// 取得傳遞參數 - Year
    /// </summary>
    public string Req_Month
    {
        get
        {
            String _data = Request.QueryString["mm"];
            string dt = ("0" + DateTime.Now.Month.ToString()).Right(2);
            return (CustomExtension.String_資料長度Byte(_data, "1", "2", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_Month = value;
        }
    }
    private string _Req_Month;

    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}myShipping_CHN/CustSendCntStat.aspx".FormatThis(fn_Param.WebUrl);
        }
        set
        {
            this._thisPage = value;
        }
    }
    private string _thisPage;
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
            return "xls|xlsx";
        }
        set
        {
            this._FileExtLimit = value;
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
            return 1;
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
            return "{0}ShipImport/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
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
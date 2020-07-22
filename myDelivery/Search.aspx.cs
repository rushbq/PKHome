using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DeliveryData.Controllers;
using LinqToExcel;
using PKLib_Method.Methods;

public partial class myDelivery_Search : SecurityCheck
{
    public string ErrMsg;
    public bool masterAuth = false; //管理者權限(可在權限設定裡勾選)

    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;

                isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3786");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }
                masterAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3787");

                //[權限判斷] End
                #endregion

                //產生選單
                GetClassMenu("1", filter_ShipWay, "全部資料");

                //Get Data
                LookupDataList(Req_PageIdx);

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
    private void LookupDataList(int pageIndex)
    {
        //----- 宣告:網址參數 -----
        int RecordsPerPage = 10;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        int DataCnt = 0;
        ArrayList PageParam = new ArrayList();  //分類暫存條件參數

        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            #region >> 條件篩選 <<

            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(Req_Keyword))
            {
                search.Add("Keyword", Req_Keyword);
                PageParam.Add("k=" + Server.UrlEncode(Req_Keyword));
                filter_Keyword.Text = Req_Keyword;
            }

            //[查詢條件] - 寄送方式
            if (!string.IsNullOrWhiteSpace(Req_ShipWay))
            {
                search.Add("ShipWay", Req_ShipWay);
                PageParam.Add("sw=" + Server.UrlEncode(Req_ShipWay));
                filter_ShipWay.SelectedValue = Req_ShipWay;
            }

            //[取得/檢查參數] - Date
            if (!string.IsNullOrWhiteSpace(Req_sDate))
            {
                search.Add("sDate", Req_sDate);
                PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
                filter_sDate.Text = Req_sDate;
            }
            if (!string.IsNullOrWhiteSpace(Req_eDate))
            {
                search.Add("eDate", Req_eDate);
                PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
                filter_eDate.Text = Req_eDate;
            }

            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetList(search, StartRow, RecordsPerPage
                , out DataCnt, out ErrMsg);

            //----- 資料整理:取得總筆數 -----
            TotalRow = DataCnt;

            //----- 資料整理:頁數判斷 -----
            if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
            {
                StartRow = 0;
                pageIndex = 1;
            }

            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
            lvDataList.DataBind();


            //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
            if (query.Count() == 0)
            {
                ph_EmptyData.Visible = true;
                ph_Data.Visible = false;

                //Clear
                CustomExtension.setCookie("Devliery", "", -1);
            }
            else
            {
                ph_EmptyData.Visible = false;
                ph_Data.Visible = true;

                //分頁設定
                string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                    , thisPage, PageParam, false, true);

                Literal lt_Pager = (Literal)this.lvDataList.FindControl("lt_Pager");
                lt_Pager.Text = getPager;

                //重新整理頁面Url
                string reSetPage = "{0}&page={1}{2}".FormatThis(
                    thisPage
                    , pageIndex
                    , (PageParam.Count == 0 ? "" : "&") + string.Join("&", PageParam.ToArray()));

                //暫存頁面Url, 給其他頁使用
                CustomExtension.setCookie("Devliery", Server.UrlEncode(reSetPage), 1);

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

    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取得資料:是否關閉
                string _isClose = DataBinder.Eval(dataItem.DataItem, "IsClose").ToString();

                PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                PlaceHolder ph_Del = (PlaceHolder)e.Item.FindControl("ph_Del");

                //預設
                ph_Del.Visible = false;

                //有管理權限
                if (masterAuth)
                {
                    //開啟Del
                    ph_Del.Visible = true;
                }

                //一般使用者權限(關閉=Y, 無管理權限)
                if (_isClose.Equals("Y") && !masterAuth)
                {
                    //隱藏Edit
                    ph_Edit.Visible = false;
                }

            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();

        try
        {
            //----- 方法:刪除資料 -----
            if (false == _data.Delete(Get_DataID, out ErrMsg))
            {
                CustomExtension.AlertMsg("刪除失敗", "");
                return;
            }
            else
            {
                //導向本頁
                Response.Redirect(thisPage);
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


    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 查詢
    /// </summary>
    protected void btn_Search_Click(object sender, EventArgs e)
    {
        //執行查詢
        Response.Redirect(filterUrl(), false);
    }


    /// <summary>
    /// 新竹格式匯出
    /// </summary>
    protected void lbtn_Export1_Click(object sender, EventArgs e)
    {
        //Params
        string _sDate = this.filter_sDate.Text;
        string _eDate = this.filter_eDate.Text;
        int dataCnt = 0;

        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        DataTable DT = new DataTable();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<

        //[取得/檢查參數] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            search.Add("sDate", _sDate);
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            search.Add("eDate", _eDate);
        }

        //[固定參數] - ShipWay
        search.Add("ShipWay", "1");

        #endregion

        //----- 方法:取得資料 -----
        var query = _data.GetAllList(search, out dataCnt, out ErrMsg);

        if (query.Count() == 0)
        {
            CustomExtension.AlertMsg("查無資料,請重新確認條件.", "");
            return;
        }


        /* [開始匯出] */
        var _newDT = query
            .Select(fld => new
            {
                序號 = "",
                登記單號 = fld.TraceID,
                收件人姓名 = fld.SendComp + "-" + fld.SendWho,
                收件人地址 = fld.SendAddr,
                收件人電話 = fld.SendTel,
                託運備註 = fld.Remark2,
                商品別編號 = "",
                商品數量 = fld.Box,
                才積重量 = 1,
                代收貨款 = "",
                指定配送日期 = "",
                指定配送時間 = ""
            });

        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(_newDT);


        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "新竹格式-{0}.xlsx".FormatThis(DateTime.Now.ToString().ToDateString("yyyyMMddhhmm"))
            , false);
    }


    /// <summary>
    /// 上傳&匯入物流單號
    /// </summary>
    protected void lbtn_JobUpload_Click(object sender, EventArgs e)
    {
        #region -- 檔案處理 --

        //宣告
        List<IOTempParam> ITempList = new List<IOTempParam>();
        Random rnd = new Random();
        string Message = "";
        string ftpFolder = UploadFolder; //FTP資料夾
        string thisFileName = ""; //檔名

        if (fu_ShipFile.PostedFile.ContentLength == 0)
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
        DeliveryRepository _data = new DeliveryRepository();

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
        var query_Xls = _data.GetExcel_ShipNoData(_filePath, sheetData);

        try
        {
            //回寫物流單號
            if (!_data.Update_ShipNo(query_Xls, out ErrMsg))
            {
                //lt_errMsg.Text = "匯入失敗,請重新上傳：" + ErrMsg;
                CustomExtension.AlertMsg("資料匯入失敗", "");
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
        CustomExtension.AlertMsg("匯入完成.", thisPage);
    }


    #endregion


    #region -- 附加功能 --

    /// <summary>
    /// 含查詢條件的完整網址
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _sDate = filter_sDate.Text;
        string _eDate = filter_eDate.Text;
        string _Keyword = filter_Keyword.Text;
        string _sw = filter_ShipWay.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?page=1".FormatThis(thisPage));

        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
        }

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            url.Append("&k=" + Server.UrlEncode(_Keyword));
        }

        //[查詢條件] - 寄送方式
        if (!string.IsNullOrWhiteSpace(_sw))
        {
            url.Append("&sw=" + Server.UrlEncode(_sw));
        }


        return url.ToString();
    }


    /// <summary>
    /// 取得下拉清單
    /// </summary>
    private void GetClassMenu(string type, DropDownList ddl, string rootName)
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();

        try
        {
            //取得資料
            var data = _data.GetRefClass(type, out ErrMsg);

            //----- 資料整理 -----
            ddl.Items.Clear();

            if (!string.IsNullOrEmpty(rootName))
            {
                ddl.Items.Add(new ListItem(rootName, ""));
            }

            foreach (var item in data)
            {
                ddl.Items.Add(new ListItem(item.Label, item.ID.ToString()));
            }

            //release
            data = null;

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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/Delivery".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
    }

    #endregion


    #region -- 傳遞參數 --
    /// <summary>
    /// 取得傳遞參數 - PageIdx(目前索引頁)
    /// </summary>
    public int Req_PageIdx
    {
        get
        {
            int data = Request.QueryString["Page"] == null ? 1 : Convert.ToInt32(Request.QueryString["Page"]);
            return data;
        }
        set
        {
            _Req_PageIdx = value;
        }
    }
    private int _Req_PageIdx;

    /// <summary>
    /// 取得傳遞參數 - Keyword
    /// </summary>
    public string Req_Keyword
    {
        get
        {
            String _data = Request.QueryString["k"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Keyword = value;
        }
    }
    private string _Req_Keyword;


    /// <summary>
    /// 取得傳遞參數 - ShipWay
    /// </summary>
    public string Req_ShipWay
    {
        get
        {
            String _data = Request.QueryString["sw"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_ShipWay = value;
        }
    }
    private string _Req_ShipWay;


    /// <summary>
    /// 取得傳遞參數 - sDate
    /// 預設7日內
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.AddDays(-7).ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            _Req_sDate = value;
        }
    }
    private string _Req_sDate;


    /// <summary>
    /// 取得傳遞參數 - eDate
    /// </summary>
    public string Req_eDate
    {
        get
        {
            String _data = Request.QueryString["eDate"];
            string dt = DateTime.Now.ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            _Req_eDate = value;
        }
    }
    private string _Req_eDate;


    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("Delivery");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            _thisPage = value;
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
            return "{0}Delivery/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
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
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using ShipFreight_CN.Controllers;
using ShipFreight_CN.Models;
using PKLib_Method.Methods;

public partial class myShipping_ImportStep1 : SecurityCheck
{
    public string ErrMsg;

    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;

                //A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具
                switch (Req_DataType)
                {
                    case "A":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3775");
                        break;

                    case "B":
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3776");
                        break;

                    case "C":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3777");
                        break;

                    case "D":
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3778");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //取得公司別
                string _corpName = "中國內銷({0})".FormatThis(fn_Menu.GetShipping_RefType(Req_DataType));
                lt_CorpName.Text = _corpName;
                Page.Title += "-" + _corpName;

                //[權限判斷] End
                #endregion                

            }
            catch (Exception)
            {

                throw;
            }

        }
    }

    #region -- 按鈕事件 --


    /// <summary>
    /// 經銷商匯入
    /// </summary>
    protected void lbtn_Job_B_Click(object sender, EventArgs e)
    {
        doImport("B");
    }

    /// <summary>
    /// 電商匯入
    /// </summary>
    protected void lbtn_Job_A_Click(object sender, EventArgs e)
    {
        doImport("A");
    }


    private void doImport(string _type)
    {
        //Check
        HttpPostedFile hpf;
        if (_type.Equals("A"))
        {
            hpf = fu_File_A.PostedFile;
        }
        else
        {
            hpf = fu_File_B.PostedFile;
        }

        if (hpf.ContentLength == 0)
        {
            CustomExtension.AlertMsg("請確認檔案是否已選擇!!", thisPage);
            return;
        }

        //資料處理
        string[] myData = Add_Data(_type);

        //取得回傳參數
        string DataID = myData[0];
        string ProcCode = myData[1];
        string Message = myData[2];

        //判斷是否處理成功
        if (!ProcCode.Equals("200"))
        {
            lt_ShowMsg.Text = Message;
            ph_ErrMessage.Visible = true;
            return;
        }
        else
        {
            //導至下一步
            Response.Redirect("{0}/Step2/{1}?dt={2}".FormatThis(FuncPath(), DataID, Req_DataType));
            return;
        }
    }

    #endregion


    #region -- 資料編輯 Start --

    /// <summary>
    /// 資料新增
    /// </summary>
    /// <returns></returns>
    private string[] Add_Data(string _type)
    {
        //回傳參數初始化
        string DataID = "";
        string ProcCode = "0";
        string Message = "";
        //Get TraceID
        string myTraceID = NewTraceID();


        #region -- 檔案處理 --

        //宣告
        List<IOTempParam> ITempList = new List<IOTempParam>();
        Random rnd = new Random();


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
                return new string[] { DataID, ProcCode, Message };
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
                    return new string[] { DataID, ProcCode, Message };
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


                //判斷副檔名, 未符合規格的檔案不上傳
                if (CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //設定暫存-檔案
                    ITempList.Add(new IOTempParam(myFullFile, OrgFileName, hpf));
                }
            }
        }

        #endregion


        #region -- 儲存檔案 --

        if (ITempList.Count > 0)
        {
            int errCnt = 0;
            //判斷資料夾, 不存在則建立
            _ftp.FTP_CheckFolder(UploadFolder(myTraceID));

            //暫存檔案List
            for (int row = 0; row < ITempList.Count; row++)
            {
                //取得個別檔案
                HttpPostedFile hpf = ITempList[row].Param_hpf;

                //執行上傳
                if (false == _ftp.FTP_doUpload(hpf, UploadFolder(myTraceID), ITempList[row].Param_FileName))
                {
                    errCnt++;
                }
            }

            if (errCnt > 0)
            {
                Message = "檔案上傳失敗, 失敗筆數為 {0} 筆, 請重新整理後再上傳!".FormatThis(errCnt);
                return new string[] { DataID, ProcCode, Message };
            }
        }

        #endregion


        #region -- 資料處理 --

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();


        //----- 設定:資料欄位 -----
        string guid = CustomExtension.GetGuid();

        var data = new ShipImportData
        {
            Data_ID = new Guid(guid),
            TraceID = myTraceID,
            Upload_Type = _type, //A=電商, B=經銷商
            Upload_File = ITempList[0].Param_FileName,
            Create_Who = fn_Param.CurrentUser
        };


        //----- 方法:建立資料 -----
        string defComp = Req_DataType.Equals("1") ? "CHN1" : "CHN2";
        if (!_data.CreateShipImport(data, defComp, out ErrMsg))
        {
            //刪除檔案
            _ftp.FTP_DelFolder(UploadFolder(myTraceID));

            //顯示錯誤
            Message = "資料建立失敗<br/>({0})".FormatThis(ErrMsg);
            return new string[] { DataID, ProcCode, Message };
        }
        else
        {
            DataID = guid;
            ProcCode = "200";
            return new string[] { DataID, ProcCode, Message };
        }


        #endregion
    }


    #endregion -- 資料編輯 End --


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
    /// 資料判別:A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具
    /// </summary>
    public string Req_DataType
    {
        get
        {
            string data = Request.QueryString["dt"] == null ? "A" : Request.QueryString["dt"].ToString();
            return data;
        }
        set
        {
            this._Req_DataType = value;
        }
    }
    private string _Req_DataType;


    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/ShipImportCHN".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
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
            return "{0}/Step1?dt=".FormatThis(FuncPath(), Req_DataType);
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
            return FuncPath() + "?dt=" + Req_DataType;
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }

    #endregion


    #region -- 上傳參數 --
    /// <summary>
    /// 上傳目錄(+TraceID)
    /// </summary>
    /// <param name="traceID"></param>
    /// <returns></returns>
    private string UploadFolder(string traceID)
    {
        return "{0}ShipImport/{1}/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"], traceID);
    }

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
            this._FileExtLimit = value;
        }
    }

    /// <summary>
    /// 限制上傳的檔案大小(1MB = 1024000), 5MB
    /// </summary>
    private int _FileSizeLimit;
    public int FileSizeLimit
    {
        get
        {
            return 5120000;
        }
        set
        {
            this._FileSizeLimit = value;
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
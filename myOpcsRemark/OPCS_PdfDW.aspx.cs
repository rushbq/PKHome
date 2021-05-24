using System;
using PKLib_Method.Methods;
using SelectPdf;

public partial class myOpcsRemark_OPCS_PdfDW : System.Web.UI.Page
{
    public string ErrMsg;

    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    protected void Page_Load(object sender, EventArgs e)
    {
        //Check null
        if (string.IsNullOrWhiteSpace(Req_DataID) || string.IsNullOrWhiteSpace(Req_DBS))
        {
            ph_ErrMessage.Visible = true;
            return;
        }

        /* PDF download
        string url = "{0}myOpcsRemark/PDF_Html_TW.aspx?dbs={1}&id={2}".FormatThis(fn_Param.WebUrl, Req_DBS, Req_DataID);

        //20210524:轉pdf太慢，先用html顯示
        Response.Redirect(url);
         */
        /* PDF download */
        string _dwUrl = Upload_Pdf(out ErrMsg);
        if (string.IsNullOrWhiteSpace(_dwUrl))
        {
            ph_Loading.Visible = false;
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = "產生PDF失敗...<br>" + ErrMsg;
        }
        else
        {
            //檔案下載
            string ftpFolder = UploadFolder() + Req_DBS;

            _ftp.FTP_doDownload(ftpFolder, _dwUrl, _dwUrl);
            //Response.Redirect(_dwUrl);
        }
    }

    #region *** PDF產生 ***

    /// <summary>
    /// 產生&上傳PDF
    /// </summary>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    /// <remarks>
    /// 取得Html -> 產生PDF -> 上傳至FTP -> User下載
    /// </remarks>
    private string Upload_Pdf(out string ErrMsg)
    {
        try
        {
            ErrMsg = "";

            //[Step1] 取得要做成PDF的頁面(使用元件轉換,內部站台不能用api)
            string url = "{0}myOpcsRemark/PDF_Html_TW.aspx?dbs={1}&id={2}".FormatThis(fn_Param.WebUrl, Req_DBS, Req_DataID);

            //[Step2] 產生PDF轉成byte
            byte[] pdfByte = convertPDF(url);

            //[Step3] 使用byte方式上傳至FTP
            string ftpFolder = UploadFolder() + Req_DBS;

            //判斷資料夾, 不存在則建立
            _ftp.FTP_CheckFolder(ftpFolder);

            //執行上傳
            string _fileName = Req_DataID + ".pdf";
            bool isOK = _ftp.FTP_doUploadWithByte(pdfByte, ftpFolder, _fileName);
            if (isOK)
            {
                //string _dwUrl = "{0}{1}{2}/{3}".FormatThis(fn_Param.RefUrl, UploadFolder(), Req_DBS, _fileName);
                //return _dwUrl;

                return _fileName;
            }
            else
            {
                return "";
            }


        }
        catch (Exception ex)
        {
            ErrMsg = ex.Message.ToString();
            return "";
        }

    }



    /// <summary>
    /// 轉換PDF
    /// </summary>
    /// <param name="url"></param>
    private byte[] convertPDF(string url)
    {
        //宣告 html to pdf converter
        HtmlToPdf converter = new HtmlToPdf();

        #region -- PDF Options --

        //LicenseKey(重要)
        SelectPdf.GlobalProperties.LicenseKey = fn_Param.PDF_Key;
        //指定 Select.Html.dep 路徑(重要)
        SelectPdf.GlobalProperties.HtmlEngineFullPath = System.Web.HttpContext.Current.Server.MapPath("~/bin/Select.Html.dep");

        //-PageSize
        converter.Options.PdfPageSize = PdfPageSize.A4;
        //-Page orientation, 直向-Portrait, 橫向-Landscape
        converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
        //-Web page options
        //converter.Options.WebPageWidth = 800;  //預設1024
        //converter.Options.WebPageHeight = 0;  //預設auto

        //set timeout(預設60秒)
        converter.Options.MaxPageLoadTime = 180;


        //-Page margins
        converter.Options.MarginTop = 10;
        converter.Options.MarginRight = 10;
        converter.Options.MarginBottom = 0; //若加入footer就不要設bottom邊界, 不然會多出空白頁
        converter.Options.MarginLeft = 15;

        //-footer 
        converter.Options.DisplayFooter = true;
        converter.Footer.DisplayOnFirstPage = true;
        converter.Footer.DisplayOnOddPages = true;
        converter.Footer.DisplayOnEvenPages = true;
        converter.Footer.Height = 30;


        // page numbers can be added using a PdfTextSection object
        PdfTextSection text = new PdfTextSection(0, 10, "Page: {page_number} / {total_pages}", new System.Drawing.Font("Arial", 10));
        text.HorizontalAlign = PdfTextHorizontalAlign.Right;
        converter.Footer.Add(text);


        #endregion

        //ConvertHtml
        //string urlContent = CustomExtension.WebRequest_GET(url, true);
        //PdfDocument doc = converter.ConvertHtmlString(urlContent, url);

        //取得Url並轉換PDF
        PdfDocument doc = converter.ConvertUrl(url);


        // save pdf document
        byte[] byteDoc = doc.Save();

        // close pdf document
        doc.Close();


        return byteDoc;
    }


    /// <summary>
    /// 上傳目錄
    /// </summary>
    /// <param name="traceID"></param>
    /// <returns></returns>
    private string UploadFolder()
    {
        return "{0}OpcsRemk/PDF/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
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
            return (CustomExtension.String_資料長度Byte(_data, "1", "4", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_DBS = value;
        }
    }
    private string _Req_DBS;


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
    #endregion

}
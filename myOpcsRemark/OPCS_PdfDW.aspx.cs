using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;
using SelectPdf;

public partial class myOpcsRemark_OPCS_PdfDW : System.Web.UI.Page
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        //Check null
        if (string.IsNullOrWhiteSpace(Req_DataID) || string.IsNullOrWhiteSpace(Req_DBS))
        {
            ph_ErrMessage.Visible = true;
            return;
        }

        /* PDF download */
        //[Step1] 取得要做成PDF的頁面(使用元件轉換,內部站台不能用api)
        string url = "{0}myOpcsRemark/PDF_Html_TW.aspx?dbs={1}&id={2}".FormatThis(fn_Param.WebUrl, Req_DBS, Req_DataID);

        //[Step2] 產生PDF轉成byte
        byte[] pdfByte = convertPDF(url);

        //將檔案輸出至瀏覽器
        Response.Clear();
        Response.AddHeader("Content-Disposition", "attachment;filename={0}.pdf".FormatThis(Req_DataID));
        Response.ContentType = "application/octet-stream";
        Response.OutputStream.Write(pdfByte, 0, pdfByte.Length);
        Response.OutputStream.Flush();
        Response.OutputStream.Close();
        Response.Flush();
        Response.End();
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
        //


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
        //string urlContent = html;
        //PdfDocument doc = converter.ConvertHtmlString(urlContent);
        //取得Url並轉換PDF
        PdfDocument doc = converter.ConvertUrl(url);


        // save pdf document
        byte[] byteDoc = doc.Save();

        // close pdf document
        doc.Close();


        return byteDoc;
    }


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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using ARData.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;
using SelectPdf;


public partial class myARdata_ImportStep2 : SecurityCheck
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
                //** 檢查必要參數 **
                if (string.IsNullOrEmpty(Req_CompID))
                {
                    Response.Redirect("{0}Error/參數錯誤".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #region --權限--
                //[權限判斷] Start
                /* 
                 * 使用公司別代號，判斷對應的MENU ID
                 */
                bool isPass = false;
                string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

                switch (getCorpUid)
                {
                    case "3":
                        //SH
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3782");
                        break;

                    default:
                        //TW
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3781");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //取得公司別
                string _corpName = fn_Param.GetCorpName(getCorpUid);
                lt_CorpName.Text = _corpName;
                Page.Title += "-" + _corpName;

                lb_DBS.Text = Req_CompID;

                //[權限判斷] End
                #endregion


                //讀取資料
                LookupData();

            }
            catch (Exception)
            {

                throw;
            }

        }
    }

    #region -- 資料讀取 --
    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //固定參數
            search.Add("DataID", Req_DataID);

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetOne(search, out ErrMsg);
            if (query == null)
            {
                CustomExtension.AlertMsg("查無資料", FuncPath());
                return;
            }

            var data = query.Take(1).FirstOrDefault();


            //----- 資料整理:填入資料 -----
            string _traceID = data.TraceID;
            string _dataID = data.Data_ID.ToString();
            string _dbs = data.DBS;
            string _custID = data.CustID;
            string _custName = data.CustName;
            string _sDate = data.erp_sDate.ToString().ToDateString("yyyy/MM/dd");
            string _eDate = data.erp_eDate.ToString().ToDateString("yyyy/MM/dd");


            //填入表單欄位
            lb_TraceID.Text = _traceID;
            lb_DBS.Text = _dbs;
            lb_Cust.Text = "{0} ({1})".FormatThis(_custName, _custID);
            lb_sDate.Text = _sDate;
            lb_eDate.Text = _eDate;
            hf_DataID.Value = _dataID;
            hf_CustID.Value = _custID;


            //載入單身資料
            LookupData_Detail(_dbs, _custID, _sDate.ToDateString("yyyyMMdd"), _eDate.ToDateString("yyyyMMdd"));

        }
        catch (Exception ex)
        {
            //Show Error
            string msg = "載入單頭資料時發生錯誤;" + ex.Message.ToString();
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
        }
        finally
        {
            //release
            _data = null;
        }

    }


    /// <summary>
    /// 單身資料
    /// </summary>
    /// <param name="dbs"></param>
    /// <param name="custID">客編</param>
    /// <param name="startDate">erp日期格式(yyyyMMdd)</param>
    /// <param name="endDate">erp日期格式(yyyyMMdd)</param>
    private void LookupData_Detail(string dbs, string custID, string startDate, string endDate)
    {
        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        string _guid = hf_DataID.Value;

        //填入參數
        search.Add("CustID", custID);
        search.Add("StartDate", startDate);
        search.Add("EndDate", endDate);

        try
        {
            //----- 原始資料:取得所有資料 -----
            var query = _data.GetErpList(dbs, _guid, search, out ErrMsg);

            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
            lvDataList.DataBind();

            //Show Error
            if (!string.IsNullOrWhiteSpace(ErrMsg))
            {
                ph_ErrMessage.Visible = true;
                lt_ShowMsg.Text = ErrMsg;
                return;
            }


        }
        catch (Exception ex)
        {
            //Show Error
            string msg = "載入單身資料時發生錯誤;" + ex.Message.ToString();
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
        }

        finally
        {
            _data = null;
        }
    }


    #endregion


    #region -- 按鈕事件 --
    /// <summary>
    /// 下一步
    /// </summary>
    protected void btn_Next_Click(object sender, EventArgs e)
    {
        //Params
        string _guid = hf_DataID.Value;
        string _custID = hf_CustID.Value;

        //取得已勾選
        string cbxGroup = tb_CbxValues.Text;

        //將已勾選單號加入集合
        List<ARData_Items> dataList = new List<ARData_Items>();

        string[] aryGP = Regex.Split(cbxGroup, @"\,{1}");
        foreach (string item in aryGP)
        {
            //加入項目
            var data = new ARData_Items
            {
                Erp_AR_ID = item
            };

            //將項目加入至集合
            dataList.Add(data);
        }

        //寫入單身資料檔
        ARdataRepository _data = new ARdataRepository();

        try
        {
            //[Job1] 開始寫入單身
            if (!_data.CreateDetail(_guid, dataList.AsQueryable(), out ErrMsg))
            {
                CustomExtension.AlertMsg("資料處理失敗-新增單身資料, 請通知資訊人員.", "");
                return;
            }

            //[Job2]產生PDF
            if (!Upload_Pdf(_guid, _custID, out ErrMsg))
            {
                CustomExtension.AlertMsg("PDF產生失敗,請重試.", "");
                return;
            }

            //OK
            Response.Redirect("{0}/Step3/{1}".FormatThis(FuncPath(), _guid));

        }
        catch (Exception ex)
        {
            //Show Error
            string msg = "資料處理時發生錯誤;" + ex.Message.ToString();
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
        }

        finally
        {
            _data = null;
        }

    }


    /// <summary>
    /// 產生&上傳PDF
    /// </summary>
    /// <param name="dataID">資料編號</param>
    /// <param name="custID">客編-檔名用</param>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    private bool Upload_Pdf(string dataID, string custID, out string ErrMsg)
    {
        try
        {
            //[Step1] 取得要做成PDF的頁面(使用元件轉換,內部站台不能用api)
            string url = "{0}myARdata/PDF_Html.aspx?id={1}".FormatThis(fn_Param.WebUrl, dataID);

            //[Step2] 產生PDF轉成byte
            byte[] pdfByte = convertPDF(url);
            
            //[Step3] 使用byte方式上傳至FTP
            bool isOK = _ftp.FTP_doUploadWithByte(pdfByte, UploadFolder(), custID + ".pdf");

            ErrMsg = "";

            return isOK;
        }
        catch (Exception ex)
        {
            ErrMsg = ex.Message.ToString();
            return false;
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
        //-Page orientation
        converter.Options.PdfPageOrientation = PdfPageOrientation.Landscape; //直向-Portrait, 橫向-Landscape
        //-Web page options
        //converter.Options.WebPageWidth = 800;  //預設1024
        //converter.Options.WebPageHeight = 0;  //預設auto

        //-Page margins
        converter.Options.MarginTop = 10;
        converter.Options.MarginRight = 15;
        converter.Options.MarginBottom = 0; //若加入footer就不要設bottom邊界, 不然會多出空白頁
        converter.Options.MarginLeft = 15;

        //-footer 
        converter.Options.DisplayFooter = true;
        converter.Footer.DisplayOnFirstPage = true;
        converter.Footer.DisplayOnOddPages = true;
        converter.Footer.DisplayOnEvenPages = true;
        converter.Footer.Height = 30;


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
    

    /// <summary>
    /// 刪除本次轉入
    /// </summary>
    protected void btn_Back_Click(object sender, EventArgs e)
    {
        //Params
        string _guid = hf_DataID.Value;

        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();

        try
        {
            if (!_data.Delete(_guid, out ErrMsg))
            {
                CustomExtension.AlertMsg("資料處理失敗-刪除暫存, 請通知資訊人員.", "");
                return;
            }

            //返回上一步
            Response.Redirect(FuncPath() + "/Step1");

        }
        catch (Exception ex)
        {
            //Show Error
            string msg = "刪除暫存時發生錯誤;" + ex.Message.ToString();
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
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

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
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
        return "{0}{1}/{2}/ARInform/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
    }


    /// <summary>
    /// 上傳目錄
    /// </summary>
    /// <param name="traceID"></param>
    /// <returns></returns>
    private string UploadFolder()
    {
        return "{0}ARData/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
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
            return "{0}/Step2".FormatThis(FuncPath());
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
            return FuncPath();
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }

    #endregion

}
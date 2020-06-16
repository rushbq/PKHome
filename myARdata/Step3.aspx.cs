using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.UI;
using ARData.Controllers;
using PKLib_Method.Methods;

public partial class myARdata_ImportStep3 : SecurityCheck
{
    public string ErrMsg;


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

                //lb_DBS.Text = Req_CompID;

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
            //lb_DBS.Text = _dbs;
            lb_Cust.Text = "{0} ({1})".FormatThis(_custName, _custID);
            lb_sDate.Text = _sDate;
            lb_eDate.Text = _eDate;
            hf_DataID.Value = _dataID;
            hf_CustID.Value = _custID;


            //載入單身資料
            LookupData_Detail(_dbs, _dataID);
            //載入總計資料
            LookupData_PriceInfo(_dbs, _dataID);
            //Email
            LookupData_Emails(_custID);

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
    /// <param name="parentID"></param>
    private void LookupData_Detail(string dbs, string parentID)
    {
        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();

        try
        {
            //----- 原始資料:取得所有資料 -----
            var query = _data.GetGrid(parentID, dbs, out ErrMsg);

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


    /// <summary>
    /// 價格資訊
    /// </summary>
    /// <param name="dbs"></param>
    /// <param name="parentID"></param>
    private void LookupData_PriceInfo(string dbs, string parentID)
    {
        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();

        try
        {
            //----- 原始資料:取得所有資料 -----
            var query = _data.GetFooterInfo(parentID, dbs, out ErrMsg);
            if (query == null)
            {
                CustomExtension.AlertMsg("查無價格資料", "");
                return;
            }

            var data = query.Take(1).FirstOrDefault();

            //----- 資料整理:填入資料 -----
            lb_PrePrice.Text = data.PrePrice.ToString().ToMoneyString();
            lt_PreCnt.Text = data.PreCnt.ToString();
            lb_TotalPrice_NoTax.Text = data.TotalPrice_NoTax.ToString().ToMoneyString();
            lb_TotalPrice.Text = data.TotalPrice.ToString().ToMoneyString();
            lb_TotalTaxPrice.Text = data.TotalTaxPrice.ToString().ToMoneyString();
            lt_Cnt.Text = data.Cnt.ToString();


        }
        catch (Exception ex)
        {
            //Show Error
            string msg = "載入價格資料時發生錯誤;" + ex.Message.ToString();
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
    /// Email
    /// </summary>
    /// <param name="custID"></param>
    private void LookupData_Emails(string custID)
    {
        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();

        try
        {
            //----- 原始資料:取得所有資料 -----
            var query = _data.GetAddressbook(custID, out ErrMsg);

            //----- 資料整理:繫結 ----- 
            lvAddressBook.DataSource = query;
            lvAddressBook.DataBind();

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
            string msg = "載入Email資料時發生錯誤;" + ex.Message.ToString();
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

        //宣告
        ARdataRepository _data = new ARdataRepository();
        ArrayList emails = new ArrayList();

        try
        {
            /* 取得寄件名單 */
            var getMail = _data.GetAddressbook(_custID, out ErrMsg);
            if (getMail.Count() == 0)
            {
                CustomExtension.AlertMsg("發信名單未設定,無法進行發送!", "");
                return;
            }
            else
            {
                foreach (var mail in getMail)
                {
                    //加入Email
                    emails.Add(mail.Email);
                }
            }


            /* 取得PDF,轉成byte */
            string url = "{0}PKHome/ARData/{1}.pdf".FormatThis(fn_Param.RefUrl, _custID);
            string pdfName = _custID + ".pdf";
            WebClient myClient = new WebClient();
            //file to byte
            byte[] pdfBytes = myClient.DownloadData(url);

            if (pdfBytes.Length == 0)
            {
                CustomExtension.AlertMsg("PDF抓不到,請確認PDF已產生!", "");
                return;
            }


            /* 取得發信內文 */
            string subject = "【重要訊息通知】{0} 年 {1} 月份對帳明細通知【寶工實業股份有限公司】".FormatThis(
                DateTime.Today.Year, DateTime.Today.Month
                );
            StringBuilder mailBody = Get_MailBody();
            if (mailBody.Length == 0)
            {
                CustomExtension.AlertMsg("無法取得信件內文,請通知MIS.", "");
                return;
            }


            /* 開始發信 */
            if (!Send_Email(emails, subject, mailBody, pdfBytes, pdfName, out ErrMsg))
            {
                CustomExtension.AlertMsg("發信失敗,請重試.", "");
                return;
            }


            /* 變更狀態 */
            if (!_data.UpdateStatus(_guid, out ErrMsg))
            {
                CustomExtension.AlertMsg("狀態變更失敗, 請通知MIS, 不要重新執行.", FuncPath());
                return;
            }


            //OK
            Response.Redirect("{0}/Step4/{1}".FormatThis(FuncPath(), _guid));

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

    #endregion



    #region -- 附加功能 --

    /// <summary>
    /// 取得寄信內容
    /// </summary>
    /// <returns></returns>
    private StringBuilder Get_MailBody()
    {
        //Html Url(CDN)
        string myHtmlUrl = "{0}PKHome/ARData/Mail_{1}.html".FormatThis(fn_Param.CDNUrl, Req_CompID);

        //宣告
        StringBuilder html = new StringBuilder();

        //取得html內容
        html.Append(CustomExtension.WebRequest_byGET(myHtmlUrl).ToString());

        //填入資料
        html.Replace("#CreateDate#", DateTime.Today.ToShortDateString());
        html.Replace("#今年#", DateTime.Now.Year.ToString());

        //return
        return html;
    }

    /// <summary>
    /// 發信
    /// </summary>
    /// <param name="mailList">收件人Array</param>
    /// <param name="subject">主旨</param>
    /// <param name="mailBody">Body</param>
    /// <param name="fileBytes">檔案byte</param>
    /// <param name="fileName">附件中顯示的檔名</param>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    /// <example>
    /// Send_Email(email, "mysubject0612", "body", bytes, "hello.pdf", out ErrMsg)
    /// </example>
    private bool Send_Email(ArrayList mailList, string subject, StringBuilder mailBody
        , byte[] fileBytes, string fileName, out string ErrMsg)
    {
        try
        {
            //Check
            if (mailList.Count == 0)
            {
                ErrMsg = "請設定收件人";
                return false;
            }
            if (string.IsNullOrWhiteSpace(subject))
            {
                ErrMsg = "請填寫主旨";
                return false;
            }

            //開始發信
            using (MailMessage Msg = new MailMessage())
            {
                //寄件人
                Msg.From = new MailAddress(fn_Param.SysMail_Sender, "ProsKit Mail System");

                //收件人(多人)
                foreach (string email in mailList)
                {
                    Msg.To.Add(new MailAddress(email));
                }

                //固定傳送MAIL:系統收件箱(功能穩定後可考慮移除)
                Msg.Bcc.Add(new MailAddress("ITInform@mail.prokits.com.tw"));


                //主旨
                Msg.Subject = subject;


                //Body
                Msg.Body = mailBody.ToString();


                //附件
                if (fileBytes.Length > 0)
                {
                    Attachment att = new Attachment(new MemoryStream(fileBytes), fileName);
                    Msg.Attachments.Add(att);
                }

                //Html Body
                Msg.IsBodyHtml = true;


                //SMTPClient
                var client = new SmtpClient();
                // Do not remove this using. In .NET 4.0 SmtpClient implements IDisposable.
                using (client as IDisposable)
                {
                    client.Send(Msg);
                }

                //OK
                ErrMsg = "";
                return true;
            }
        }
        catch (Exception ex)
        {
            ErrMsg = "發送失敗...訊息:" + ex.Message.ToString();
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
            return "{0}/Step3".FormatThis(FuncPath());
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
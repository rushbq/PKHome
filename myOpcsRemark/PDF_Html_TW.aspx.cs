using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myOpcsRemark_PDF_Html_TW : System.Web.UI.Page
{
    public string ErrMsg;

    protected override void Render(HtmlTextWriter writer)
    {
        StringBuilder sb = new StringBuilder();
        StringWriter sw = new StringWriter(sb);
        HtmlTextWriter hWriter = new HtmlTextWriter(sw);
        base.Render(hWriter);
        string html = sb.ToString();
        html = Regex.Replace(html, "<input[^>]*id=\"(__VIEWSTATE)\"[^>]*>", string.Empty, RegexOptions.IgnoreCase);
        writer.Write(html);
    }


    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //檢查ID是否為空
                if (string.IsNullOrEmpty(Req_DataID))
                {
                    Response.Write("<h1>不正確的操作,無法取得資料.</h1>");
                    return;
                }

                //取得資料
                LookupData();

            }


        }
        catch (Exception)
        {

            throw;
        }

    }


    #region -- 資料讀取 --
    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        try
        {
            //----- 原始資料:取得所有資料 -----
            var query = _data.Get_OpcsUpdForm(Req_DataID, Req_DBS, out ErrMsg);

            //----- 資料整理:繫結 ----- 
            if (query == null || query.Rows.Count == 0)
            {
                lt_CompName.Text = "<h1>無法取得資料</h1>";
                return;
            }

            //header
            lt_CompName.Text = Req_DBS.Equals("TW") ? "寶工實業股份有限公司" : "上海寶工工具有限公司";
            lt_OrderTypeName.Text = query.Rows[0]["TE001Name"].ToString();

            //--- 填入資料 ---
            lt_SO_FID.Text = query.Rows[0]["TE001"].ToString();
            lt_SO_SID.Text = query.Rows[0]["TE002"].ToString();
            lt_SO_TypeName.Text = query.Rows[0]["TE001Name"].ToString();

            //Cust
            string _custID = query.Rows[0]["TE007"].ToString();
            TE007.Text = _custID;
            TE007Name.Text = query.Rows[0]["TE007Name"].ToString();
            /* 課稅別 */
            TE018.Text = Get_TypeName("TAX", query.Rows[0]["TE018"].ToString());
            TE118.Text = Get_TypeName("TAX", query.Rows[0]["TE118"].ToString());
            TE003.Text = query.Rows[0]["TE003"].ToString(); /* 變更版次 */
            TE004.Text = query.Rows[0]["TE004"].ToString(); /* 變更日期 */
            TE005.Text = query.Rows[0]["TE005"].ToString(); /* 整張結案 */
            TE029.Text = query.Rows[0]["TE029"].ToString(); /* 確認碼 */
            TE010.Text = query.Rows[0]["TE010"].ToString();
            TE010Name.Text = query.Rows[0]["TE010Name"].ToString(); /* 新出貨廠別 */
            TE006.Text = query.Rows[0]["TE006"].ToString(); /* 變更原因 */
            TE011.Text = query.Rows[0]["TE011"].ToString();
            TE111.Text = query.Rows[0]["TE111"].ToString(); /* 交易幣別 */
            TE012.Text = (Math.Round(Convert.ToDecimal(query.Rows[0]["TE012"]), 2)).ToString();
            TE112.Text = (Math.Round(Convert.ToDecimal(query.Rows[0]["TE112"]), 2)).ToString(); /* 匯率 */
            TE017.Text = query.Rows[0]["TE017"].ToString();
            TE117.Text = query.Rows[0]["TE117"].ToString(); /* 付款條件 */

            TE015.Text = query.Rows[0]["TE015"].ToString();
            TE115.Text = query.Rows[0]["TE115"].ToString(); /* 客戶單號 */
            TE016.Text = query.Rows[0]["TE016"].ToString();
            TE116.Text = query.Rows[0]["TE016"].ToString(); /* 新價格條件 */

            TE008Name.Text = query.Rows[0]["TE008Name"].ToString();
            TE108Name.Text = query.Rows[0]["TE108Name"].ToString(); /* 部門代號 */
            TE009Name.Text = query.Rows[0]["TE009Name"].ToString();
            TE109Name.Text = query.Rows[0]["TE109Name"].ToString(); /* 業務人員 */
            lt_CfmWho.Text = query.Rows[0]["CfmWho"].ToString(); /* 確認者 */
            TE040.Text = (Math.Round(Convert.ToDecimal(query.Rows[0]["TE040"]) * 100, 0)).ToString();
            TE136.Text = (Math.Round(Convert.ToDecimal(query.Rows[0]["TE136"]) * 100, 0)).ToString(); /* 營業稅率 */
            TE049.Text = Get_TypeName("BOX", query.Rows[0]["TE049"].ToString());
            TE143.Text = Get_TypeName("BOX", query.Rows[0]["TE143"].ToString()); /* 材積單位 */
            TE013.Text = query.Rows[0]["TE013"].ToString();
            TE014.Text = query.Rows[0]["TE014"].ToString(); /* 新送貨地址1_2 */
            TE113.Text = query.Rows[0]["TE113"].ToString();
            TE114.Text = query.Rows[0]["TE114"].ToString(); /* 原送貨地址1_2 */

            //footer
            //string micTxt1 = query.Rows[0]["TE047"].ToString();
            //string micTxt2 = query.Rows[0]["TE048"].ToString();
            //lt_MicTxt1.Text = micTxt1.Replace("\n", "<br>");
            //lt_MicTxt2.Text = micTxt2.Replace("\n", "<br>");
            //lt_OldMicTxt1.Text = query.Rows[0]["TE141"].ToString().Replace("\n", "<br>");
            //lt_OldMicTxt2.Text = query.Rows[0]["TE142"].ToString().Replace("\n", "<br>");
            //pl_MicTxt.Visible = !string.IsNullOrWhiteSpace(micTxt1) || !string.IsNullOrWhiteSpace(micTxt2);


            //[嘜頭圖取得] 嘜頭圖路徑
            string _url = "http://ref.prokits.com.tw/ERP_Files/Cust_Mark/{0}/EPS/"
                .FormatThis(Req_DBS.Equals("TW") ? "prokit(II)" : "SHPK2");

            #region - 嘜頭處理:New -

            //** 嘜頭資料 **
            string _MarkPic_New = query.Rows[0]["MarkPic_New"].ToString(); //依條件判斷的嘜頭圖片
            string _MicTxt1_New = query.Rows[0]["MicTxt1_New"].ToString(); //訂單嘜頭文字, 正嘜
            string _MicTxt2_New = query.Rows[0]["MicTxt2_New"].ToString(); //訂單嘜頭文字, 側嘜

            //[嘜頭圖取得] 正嘜圖
            string _filename1_New = _MarkPic_New + "1.jpg";
            //[嘜頭圖取得] 側嘜圖
            string _filename2_New = _MarkPic_New + "2.jpg";

            //[嘜頭圖取得] 判斷圖檔是否存在, 不存在抓預設圖
            string _chkFile1_New = checkFileExists(_url, _filename1_New);
            string _chkMicFile1_New = checkStandardImg(Req_DBS, _chkFile1_New, _custID, "101", _MicTxt1_New);

            string _chkFile2_New = checkFileExists(_url, _filename2_New);
            string _chkMicFile2_New = checkStandardImg(Req_DBS, _chkFile2_New, _custID, "102", _MicTxt2_New);


            //輸出正嘜文字
            lt_MicTxt1_New.Text = _MicTxt1_New.Replace("\n", "<br>");
            /* 判斷:指定客戶,有嘜頭文字,不帶嘜頭圖 */
            if (checkTargetCust(_MicTxt1_New, _custID))
            {
                //輸出嘜頭圖片
                lt_MicPic1_New.Text = !string.IsNullOrWhiteSpace(_chkMicFile1_New)
                    ? "<img src=\"{0}\" alt=\"Pic1\" width=\"270\" align=\"middle\">".FormatThis(_chkMicFile1_New)
                    : "";
            }

            //輸出側嘜文字
            lt_MicTxt2_New.Text = _MicTxt2_New.Replace("\n", "<br>");
            /* 判斷:指定客戶,有嘜頭文字,不帶嘜頭圖 */
            if (checkTargetCust(_MicTxt2_New, _custID))
            {
                //輸出嘜頭圖片
                lt_MicPic2_New.Text = !string.IsNullOrWhiteSpace(_chkMicFile2_New)
                    ? "<img src=\"{0}\" alt=\"Pic1\" width=\"270\" align=\"middle\">".FormatThis(_chkMicFile2_New)
                    : "";
            }

            #endregion


            #region - 嘜頭處理:Old -

            //** 嘜頭資料 **
            string _MarkPic_Old = query.Rows[0]["MarkPic_Old"].ToString(); //依條件判斷的嘜頭圖片
            string _MicTxt1_Old = query.Rows[0]["MicTxt1_Old"].ToString(); //訂單嘜頭文字, 正嘜
            string _MicTxt2_Old = query.Rows[0]["MicTxt2_Old"].ToString(); //訂單嘜頭文字, 側嘜

            //[嘜頭圖取得] 正嘜圖
            string _filename1_Old = _MarkPic_Old + "1.jpg";
            //[嘜頭圖取得] 側嘜圖
            string _filename2_Old = _MarkPic_Old + "2.jpg";

            //[嘜頭圖取得] 判斷圖檔是否存在, 不存在抓預設圖
            string _chkFile1_Old = checkFileExists(_url, _filename1_Old);
            string _chkMicFile1_Old = checkStandardImg(Req_DBS, _chkFile1_Old, _custID, "101", _MicTxt1_Old);

            string _chkFile2_Old = checkFileExists(_url, _filename2_Old);
            string _chkMicFile2_Old = checkStandardImg(Req_DBS, _chkFile2_Old, _custID, "102", _MicTxt2_Old);


            //輸出正嘜文字
            lt_MicTxt1_Old.Text = _MicTxt1_Old.Replace("\n", "<br>");
            /* 判斷:指定客戶,有嘜頭文字,不帶嘜頭圖 */
            if (checkTargetCust(_MicTxt1_Old, _custID))
            {
                //輸出嘜頭圖片
                lt_MicPic1_Old.Text = !string.IsNullOrWhiteSpace(_chkMicFile1_Old)
                    ? "<img src=\"{0}\" alt=\"Pic1\" width=\"270\" align=\"middle\">".FormatThis(_chkMicFile1_Old)
                    : "";
            }

            //輸出側嘜文字
            lt_MicTxt2_Old.Text = _MicTxt2_Old.Replace("\n", "<br>");
            /* 判斷:指定客戶,有嘜頭文字,不帶嘜頭圖 */
            if (checkTargetCust(_MicTxt2_Old, _custID))
            {
                //輸出嘜頭圖片
                lt_MicPic2_Old.Text = !string.IsNullOrWhiteSpace(_chkMicFile2_Old)
                    ? "<img src=\"{0}\" alt=\"Pic1\" width=\"270\" align=\"middle\">".FormatThis(_chkMicFile2_Old)
                    : "";
            }

            #endregion


            //----- 資料整理:繫結 ----- 
            string _lineorder = query.Rows[0]["lineOrder"].ToString();

            if (!string.IsNullOrWhiteSpace(_lineorder))
            {
                lvDataList.DataSource = query;
                lvDataList.DataBind();
            }

        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            //release
            _data = null;
        }

    }

    /// <summary>
    /// 代號名稱
    /// </summary>
    /// <param name="_type"></param>
    /// <param name="_id"></param>
    /// <returns></returns>
    private string Get_TypeName(string _type, string _id)
    {
        switch (_type)
        {
            case "TAX":
                //課稅別
                //1.內含、2.外加、3.零稅率、4.免稅、9.不計稅
                switch (_id)
                {
                    case "1":
                        return "內含";
                    case "2":
                        return "外加";
                    case "3":
                        return "零稅率";
                    case "4":
                        return "免稅";
                    default:
                        return "不計稅";
                }

            case "BOX":
                //材積單位
                //1:CUFT, 2:CBM
                switch (_id)
                {
                    case "1":
                        return "CUFT";
                    case "2":
                        return "CBM";
                    default:
                        return "";
                }

            default:
                return "";
        }

    }

    /// <summary>
    /// 虛線
    /// </summary>
    /// <param name="_id"></param>
    /// <returns></returns>
    protected string Get_StyleLine(string _id)
    {
        if (_id.Equals("1"))
        {
            return "border-bottom:1px dashed;";
        }
        else
        {
            return "";
        }
    }


    #endregion


    #region -- 嘜頭Function --

    /// <summary>
    /// [嘜頭]指定客戶判斷
    /// **有嘜頭文字,不顯示指定嘜頭圖**
    /// </summary>
    /// <param name="_micTxt">嘜頭文字</param>
    /// <param name="_custID">客戶代號</param>
    /// <returns></returns>
    bool checkTargetCust(string _micTxt, string _custID)
    {
        //指定客戶:Steren, Teco 
        switch (_custID)
        {
            case "1525501":
            case "2002054001":
                if (string.IsNullOrWhiteSpace(_micTxt))
                {
                    return true;
                }
                else
                {
                    //有字就不show圖
                    return false;
                }

            default:
                //其他
                return true;
        }
    }


    /// <summary>
    /// [嘜頭]判斷檔案是否存在 (http)
    /// </summary>
    /// <param name="_url">資料夾路徑</param>
    /// <param name="_fileName">檔名</param>
    /// <returns></returns>
    /// <remarks>
    /// string _url = "http://ref.prokits.com.tw/ERP_Files/Cust_Mark/prokit(II)/EPS/"
    /// string _filename = "Z99933011.jpg"
    /// </remarks>
    string checkFileExists(String _url, String _fileName)
    {
        if (string.IsNullOrWhiteSpace(_fileName))
        {
            return "";
        }

        string url = _url + _fileName;
        // note : you may also need
        // HttpURLConnection.setInstanceFollowRedirects(false)
        HttpWebResponse response = null;
        var request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "HEAD";

        try
        {
            response = (HttpWebResponse)request.GetResponse();

            return url;
        }
        catch (WebException ex)
        {
            /* A WebException will be thrown if the status of the response is not `200 OK` */
            return "";
        }
        finally
        {
            // Don't forget to close your response.
            if (response != null)
            {
                response.Close();
            }
        }

    }


    /// <summary>
    /// [嘜頭]自訂嘜頭圖:當指定客戶圖片檔不存在時,取得預設圖片;其他客戶直接使用預設圖
    /// </summary>
    /// <param name="_dbs">DBS</param>
    /// <param name="imgUrl">圖檔完整路徑</param>
    /// <param name="custID">客戶代號</param>
    /// <param name="lastFileName">檔名尾數(目前僅需使用正101/側102)</param>
    /// <param name="markDesc">正側嘜文字</param>
    /// <returns></returns>
    string checkStandardImg(string _dbs, string imgUrl, string custID, string lastFileName, string markDesc)
    {
        //指定資料夾
        String frontFileName = "http://ref.prokits.com.tw/ERP_Files/Cust_Mark/Standard{0}/"
            .FormatThis(_dbs.Equals("TW") ? "" : "-SH");
        String fullFileName = "";

        //指定客戶:Steren, Teco 
        if (custID.Equals("1525501") || custID.Equals("2002054001"))
        {
            if (imgUrl.Equals(""))
            {
                fullFileName = frontFileName + custID + "-" + lastFileName + ".jpg";

            }
            else {
                fullFileName = imgUrl;

            }

        }
        else {
            if (imgUrl.Equals("") && markDesc.Equals(""))
            {
                //無圖無字:帶預設圖(圖片有文字)
                fullFileName = frontFileName + "STD" + lastFileName + "_txt.jpg";
            }
            else if (imgUrl.Equals(""))
            {
                //無圖有字:帶預設圖(圖片無文字)
                fullFileName = frontFileName + "STD" + lastFileName + ".jpg";

            }
            else {
                fullFileName = imgUrl;

            }
        }

        return fullFileName;
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
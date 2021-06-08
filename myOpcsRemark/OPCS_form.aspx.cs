using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myOpcsRemark_OPCS_form : System.Web.UI.Page
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        //Get data
        LookupData();

        //output excel
        Response.Clear();
        Response.Buffer = true;
        Response.Charset = "UTF-8";
        Response.AppendHeader("Content-Disposition", "attachment;filename=OPCS_{0}.xls".FormatThis(Req_DataID));
        Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
        Response.ContentType = "application/vnd.ms-excel";
        this.EnableViewState = false;
        System.IO.StringWriter objStringWriter = new System.IO.StringWriter();
        System.Web.UI.HtmlTextWriter objHtmlTextWriter = new System.Web.UI.HtmlTextWriter(objStringWriter);
        this.RenderControl(objHtmlTextWriter);
        Response.Write(objStringWriter.ToString());
        Response.End();
    }


    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.Get_OpcsRemkForm(Req_DataID, Req_DBS, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        if (query == null || query.Rows.Count == 0)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", "SalesOrderSearch.aspx?dbs=" + Req_DBS);
            return;
        }

        //header
        lt_CompName.Text = Req_DBS.Equals("TW") ? "寶工實業股份有限公司" : "上海寶工工具有限公司";
        lt_OrderTypeName.Text = query.Rows[0]["OrderTypeName"].ToString();

        //--- 填入資料 ---
        string _custID = query.Rows[0]["CustID"].ToString();
        lt_CheckDate.Text = query.Rows[0]["CheckDate"].ToString().ToDateString_ERP("/");
        lt_OrderDate.Text = query.Rows[0]["OrderDate"].ToString().ToDateString_ERP("/");
        lt_SO_FID.Text = query.Rows[0]["SO_Fid"].ToString();
        lt_SO_SID.Text = query.Rows[0]["SO_Sid"].ToString();
        lt_CustName.Text = query.Rows[0]["CustName"].ToString();
        lt_CustID.Text = _custID;
        lt_CustPO.Text = query.Rows[0]["CustPO"].ToString();
        lt_PreDate.Text = query.Rows[0]["PreDate"].ToString().ToDateString_ERP("/");
        lt_SalesWho.Text = query.Rows[0]["SalesWho"].ToString();
        lt_CfmWho.Text = query.Rows[0]["CfmWho"].ToString();
        lt_TradeTerm.Text = query.Rows[0]["TradeTerm"].ToString();
        lt_PayTerm.Text = query.Rows[0]["PayTerm"].ToString();
        lt_TradeCurrency.Text = query.Rows[0]["TradeCurrency"].ToString();
        lt_OrderRemark.Text = query.Rows[0]["OrderRemark"].ToString().Replace("\n", "<br>");

        //** 嘜頭資料 **
        string _MarkPic = query.Rows[0]["MarkPic"].ToString();
        string _MicTxt1 = query.Rows[0]["MicTxt1"].ToString(); //訂單嘜頭文字
        string _MicTxt2 = query.Rows[0]["MicTxt2"].ToString(); //訂單嘜頭文字

        //[嘜頭圖取得] 嘜頭圖路徑
        string _url = "http://ref.prokits.com.tw/ERP_Files/Cust_Mark/{0}/EPS/"
            .FormatThis(Req_DBS.Equals("TW") ? "prokit(II)" : "SHPK2");
        //[嘜頭圖取得] 正嘜圖
        string _filename1 = _MarkPic + "1.jpg";
        //[嘜頭圖取得] 側嘜圖
        string _filename2 = _MarkPic + "2.jpg";

        //[嘜頭圖取得] 判斷圖檔是否存在, 不存在抓預設圖
        string _chkFile1 = checkFileExists(_url, _filename1);
        string _chkMicFile1 = checkStandardImg(Req_DBS, _chkFile1, _custID, "101", _MicTxt1);

        string _chkFile2 = checkFileExists(_url, _filename2);
        string _chkMicFile2 = checkStandardImg(Req_DBS, _chkFile2, _custID, "102", _MicTxt2);


        //輸出正嘜
        lt_MicTxt1.Text = _MicTxt1.Replace("\n", "<br>");
        /* 指定客戶,有嘜頭文字,不帶嘜頭圖 */
        if (checkTargetCust(_MicTxt1, _custID))
        {
            lt_MicPic1.Text = !string.IsNullOrWhiteSpace(_chkMicFile1)
                ? "<img src=\"{0}\" alt=\"Pic1\" width=\"270\" align=\"middle\">".FormatThis(_chkMicFile1)
                : "";
        }

        //輸出側嘜
        lt_MicTxt2.Text = _MicTxt2.Replace("\n", "<br>");
        /* 指定客戶,有嘜頭文字,不帶嘜頭圖 */
        if (checkTargetCust(_MicTxt2, _custID))
        {
            lt_MicPic2.Text = !string.IsNullOrWhiteSpace(_chkMicFile2)
                ? "<img src=\"{0}\" alt=\"Pic1\" width=\"270\" align=\"middle\">".FormatThis(_chkMicFile2)
                : "";
        }


        //----- 資料整理:繫結 ----- 
        if (Req_DBS.Equals("TW"))
        {
            lvDataList_TW.Visible = true;
            lvDataList_TW.DataSource = query;
            lvDataList_TW.DataBind();
        }
        else
        {
            lvDataList_SH.Visible = true;
            lvDataList_SH.DataSource = query;
            lvDataList_SH.DataBind();
        }


        //Release
        _data = null;

    }

    /// <summary>
    /// 指定客戶判斷
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
    /// 判斷檔案是否存在 (http)
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
    /// 自訂嘜頭圖:當指定客戶圖片檔不存在時,取得預設圖片,其他客戶直接使用預設圖
    /// </summary>
    /// <param name="_dbs">DBS</param>
    /// <param name="imgUrl">圖檔完整路徑</param>
    /// <param name="custID">客戶代號</param>
    /// <param name="lastFileName">檔名尾數(目前僅需使用正101/側102)</param>
    /// <param name="markDesc">正側嘜文字</param>
    /// <returns></returns>
    string checkStandardImg(string _dbs, string imgUrl, string custID, string lastFileName, string markDesc)
    {
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
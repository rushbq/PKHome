using System;
using System.IO;
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
            TE007.Text = query.Rows[0]["TE007"].ToString();
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
            TE040.Text = (Math.Round(Convert.ToDecimal(query.Rows[0]["TE040"]) * 100, 0)).ToString();
            TE136.Text = (Math.Round(Convert.ToDecimal(query.Rows[0]["TE136"]) * 100, 0)).ToString(); /* 營業稅率 */
            TE049.Text = Get_TypeName("BOX", query.Rows[0]["TE049"].ToString());
            TE143.Text = Get_TypeName("BOX", query.Rows[0]["TE143"].ToString()); /* 材積單位 */
            TE013.Text = query.Rows[0]["TE013"].ToString();
            TE014.Text = query.Rows[0]["TE014"].ToString(); /* 新送貨地址1_2 */
            TE113.Text = query.Rows[0]["TE113"].ToString();
            TE114.Text = query.Rows[0]["TE114"].ToString(); /* 原送貨地址1_2 */

            //footer
            string micTxt1 = query.Rows[0]["TE047"].ToString();
            string micTxt2 = query.Rows[0]["TE048"].ToString();
            lt_MicTxt1.Text = micTxt1.Replace("\n", "<br>");
            lt_MicTxt2.Text = micTxt2.Replace("\n", "<br>");
            lt_OldMicTxt1.Text = query.Rows[0]["TE141"].ToString().Replace("\n", "<br>");
            lt_OldMicTxt2.Text = query.Rows[0]["TE142"].ToString().Replace("\n", "<br>");
            pl_MicTxt.Visible = !string.IsNullOrWhiteSpace(micTxt1) || !string.IsNullOrWhiteSpace(micTxt2);


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
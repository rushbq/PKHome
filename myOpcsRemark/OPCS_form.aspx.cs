using System;
using System.Collections.Generic;
using System.Linq;
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
        lt_CheckDate.Text = query.Rows[0]["CheckDate"].ToString().ToDateString_ERP("/");
        lt_OrderDate.Text = query.Rows[0]["OrderDate"].ToString().ToDateString_ERP("/");
        lt_SO_FID.Text = query.Rows[0]["SO_Fid"].ToString();
        lt_SO_SID.Text = query.Rows[0]["SO_Sid"].ToString();
        lt_CustName.Text = query.Rows[0]["CustName"].ToString();
        lt_CustID.Text = query.Rows[0]["CustID"].ToString();
        lt_CustPO.Text = query.Rows[0]["CustPO"].ToString();
        lt_PreDate.Text = query.Rows[0]["PreDate"].ToString().ToDateString_ERP("/");
        lt_SalesWho.Text = query.Rows[0]["SalesWho"].ToString();
        lt_TradeTerm.Text = query.Rows[0]["TradeTerm"].ToString();
        lt_PayTerm.Text = query.Rows[0]["PayTerm"].ToString();
        lt_TradeCurrency.Text = query.Rows[0]["TradeCurrency"].ToString();
        lt_OrderRemark.Text = query.Rows[0]["OrderRemark"].ToString().Replace("\n", "<br>");
        lt_MicTxt1.Text = query.Rows[0]["MicTxt1"].ToString().ToString().Replace("\n", "<br>");
        lt_MicTxt2.Text = query.Rows[0]["MicTxt2"].ToString().ToString().Replace("\n", "<br>");


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
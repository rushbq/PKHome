using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using ARData.Controllers;
using PKLib_Method.Methods;

public partial class myARdata_PDF_Html_SH : System.Web.UI.Page
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
                CustomExtension.AlertMsg("查無資料", "");
                return;
            }

            var data = query.Take(1).FirstOrDefault();


            //----- 資料整理:填入資料 -----
            string _dataID = data.Data_ID.ToString();
            string _dbs = data.DBS;
            string _custID = data.CustID;
            string _custName = data.CustFullName;
            string _sDate = data.erp_sDate.ToString().ToDateString("yyyy/MM/dd");
            string _eDate = data.erp_eDate.ToString().ToDateString("yyyy/MM/dd");


            //填入表單欄位
            lb_Cust.Text = "{0} ({1})".FormatThis(_custName, _custID);
            lt_sDate.Text = _sDate;
            lt_eDate.Text = _eDate;
            lt_ZipCode.Text = data.ZipCode;
            lt_Addr.Text = data.Addr;
            lt_AddrRemark.Text = data.AddrRemark;
            lt_today.Text = DateTime.Today.ToShortDateString();
            lt_Fax.Text = data.Fax;
            lt_Tel.Text = data.Tel;

            //載入單身資料
            LookupData_Detail(_dbs, _dataID);
            //載入總計資料
            LookupData_PriceInfo(_dbs, _dataID);

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
            lb_PrePrice.Text = data.unGetPrice.ToString().ToMoneyString();
            lt_PreCnt.Text = data.unGetCnt.ToString();
            //lb_TotalPrice_NoTax.Text = data.TotalPrice_NoTax.ToString().ToMoneyString();
            lb_TotalPrice.Text = data.TotalPrice.ToString().ToMoneyString();
            //lb_TotalTaxPrice.Text = data.TotalTaxPrice.ToString().ToMoneyString();
            lt_Cnt.Text = data.Cnt.ToString();
            lb_AllPrice.Text = data.AllPrice.ToString().ToMoneyString();
            lt_TotalCnt.Text = (data.unGetCnt + data.Cnt).ToString();
            lb_PreGetPrice.Text = data.PreGetPrice.ToString().ToMoneyString();
           

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message.ToString());
        }
        finally
        {
            //release
            _data = null;
        }

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
            String DataID = Request.QueryString["id"].ToString();

            return DataID;
        }
        set
        {
            this._Req_DataID = value;
        }
    }


    #endregion
}
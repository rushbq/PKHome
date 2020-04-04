using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using LinqToExcel;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myShipping_ImportStep3 : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3701"); ;

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[權限判斷] End
                #endregion


                //取得資料
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
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipImportList(search, out ErrMsg).Take(1)
            .Select(fld => new
            {
                TraceID = fld.TraceID,
                erpSDate = fld.erpSDate,
                erpEDate = fld.erpEDate,
                status = fld.Status

            }).FirstOrDefault();

        //----- 資料整理:填入資料 -----
        lb_TraceID.Text = query.TraceID;
        lb_Dates.Text = "{0} ~ {1}".FormatThis(query.erpSDate, query.erpEDate);
        hf_sDate.Value = query.erpSDate;
        hf_eDate.Value = query.erpEDate;
        decimal _status = query.status;

        query = null;
        _data = null;

        //判斷是否已結案
        if (_status.Equals(30))
        {
            Response.Redirect(FuncPath());
        }
    }

    #endregion


    #region -- 按鈕事件 --
   
    /// <summary>
    /// 下一步
    /// </summary>
    protected void lbtn_Next_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //取得參數
        string _erp_sDate = hf_sDate.Value;
        string _erp_eDate = hf_eDate.Value;

        //填入基本資料Inst
        var baseData = new ShipImportData
        {
            Data_ID = new Guid(Req_DataID),
            erpSDate = _erp_sDate,
            erpEDate = _erp_eDate,
            Update_Who = fn_Param.CurrentUser
        };

        //轉流單轉入
        if (!_data.UpdateShipImport_A(baseData,  out ErrMsg))
        {
            string msg = "轉流單轉入失敗 (Step3);" + ErrMsg;

            //Show Error
            this.ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
        }

        //運費轉入
        if (!_data.UpdateShipImport_B(baseData, out ErrMsg))
        {
            string msg = "運費轉入失敗 (Step3);" + ErrMsg;

            //Show Error
            this.ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
        }

        _data = null;

        //導至下一步
        ph_ErrMessage.Visible = false;
        Response.Redirect("{0}/Step4/{1}".FormatThis(FuncPath(), Req_DataID));

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

            return DataID.ToLower().Equals("unknown") ? "SZ" : DataID;
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
        return "{0}{1}/{2}/ShipImport/{3}".FormatThis(
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

    #endregion

}
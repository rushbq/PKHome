using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using ARData.Controllers;
using PKLib_Method.Methods;

public partial class myARdata_ImportStep4 : SecurityCheck
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
                Page.Title += "-" + _corpName;

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
            lb_Cust.Text = "{0} ({1})".FormatThis(_custName, _custID);
            lb_sDate.Text = _sDate;
            lb_eDate.Text = _eDate;


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

    #endregion

}
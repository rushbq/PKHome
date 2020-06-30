using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using ARData.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;


public partial class myARdata_CustList : SecurityCheck
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

                lb_DBS.Text = Req_CompID;

                //[權限判斷] End
                #endregion


                //固定參數(上月26 ~ 本月25)
                DateTime _lastMonth = DateTime.Today.AddMonths(-1);
                DateTime _thisMonth = DateTime.Today;
                DateTime _sDate = new DateTime(_lastMonth.Year, _lastMonth.Month, 26);
                DateTime _eDate = new DateTime(_thisMonth.Year, _thisMonth.Month, 25);
                string filter_sDate = _sDate.ToString().ToDateString("yyyyMMdd");
                string filter_eDate = _eDate.ToString().ToDateString("yyyyMMdd");
               
                //Show
                lb_sDate.Text = filter_sDate;
                lb_eDate.Text = filter_eDate;
                //新增用欄位 
                hf_sDate.Value = filter_sDate;
                hf_eDate.Value = filter_eDate;

                //讀取資料(日期格式yyyyMMdd)
                LookupData_Detail(Req_CompID, filter_sDate, filter_eDate);

            }
            catch (Exception)
            {

                throw;
            }

        }
    }

    #region -- 資料讀取 --
    /// <summary>
    /// 單身資料
    /// </summary>
    /// <param name="dbs"></param>
    /// <param name="startDate">erp日期格式(yyyyMMdd)</param>
    /// <param name="endDate">erp日期格式(yyyyMMdd)</param>
    private void LookupData_Detail(string dbs, string startDate, string endDate)
    {
        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //填入參數
        search.Add("StartDate", startDate);
        search.Add("EndDate", endDate);

        try
        {
            //----- 原始資料:取得所有資料 -----
            var query = _data.GetCustList(dbs, search, out ErrMsg);

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
            string msg = "載入資料時發生錯誤;" + ex.Message.ToString();
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
        }

        finally
        {
            _data = null;
        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得CustID
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_CustID")).Value;

        //資料處理
        string[] myData = Add_Data(Get_DataID);

        //取得回傳參數
        string DataID = myData[0];
        string ProcCode = myData[1];
        string Message = myData[2];

        //判斷是否處理成功
        if (!ProcCode.Equals("200"))
        {
            lt_ShowMsg.Text = Message;
            ph_ErrMessage.Visible = true;
            return;
        }
        else
        {
            //導至Step2
            Response.Redirect("{0}/Step2/{1}".FormatThis(FuncPath(), DataID));
            return;
        }
    }
    #endregion


    #region -- 資料編輯 Start --

    /// <summary>
    /// 資料新增
    /// </summary>
    /// <param name="custID"></param>
    /// <returns></returns>
    /// <remarks>
    /// (日期格式yyyy/MM/dd)
    /// </remarks>
    private string[] Add_Data(string custID)
    {
        //回傳參數初始化
        string DataID = "";
        string ProcCode = "0";
        string Message = "";
        //TraceID
        string myTraceID = NewTraceID();

        string _cust = custID;
        string _sDate = hf_sDate.Value;
        string _eDate = hf_eDate.Value;


        #region -- 資料處理 --

        //----- 宣告:資料參數 -----
        ARdataRepository _data = new ARdataRepository();

        //----- 設定:資料欄位 -----
        string guid = CustomExtension.GetGuid();

        var data = new ARData_Base
        {
            Data_ID = new Guid(guid),
            TraceID = myTraceID,
            CustID = _cust,
            DBS = Req_CompID,
            erp_sDate = _sDate.ToDateString_ERP("/"), /* 注意日期格式yyyy/MM/dd */
            erp_eDate = _eDate.ToDateString_ERP("/"),
            Create_Who = fn_Param.CurrentUser
        };


        //----- 方法:建立資料 -----      
        if (!_data.Create(data, out ErrMsg))
        {
            //顯示錯誤
            Message = "資料建立失敗<br/>({0})".FormatThis(ErrMsg);
            return new string[] { DataID, ProcCode, Message };
        }
        else
        {
            DataID = guid;
            ProcCode = "200";
            return new string[] { DataID, ProcCode, Message };
        }


        #endregion
    }


    #endregion -- 資料編輯 End --


    #region -- 附加功能 --
    /// <summary>
    /// New TraceID
    /// </summary>
    /// <returns></returns>
    private string NewTraceID()
    {
        //產生TraceID
        long ts = Cryptograph.GetCurrentTime();

        Random rnd = new Random();
        int myRnd = rnd.Next(1, 99);

        return "{0}{1}".FormatThis(ts, myRnd);
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

    ///// <summary>
    ///// 本頁網址
    ///// </summary>
    //private string _thisPage;
    //public string thisPage
    //{
    //    get
    //    {
    //        return "{0}/CustList".FormatThis(FuncPath());
    //    }
    //    set
    //    {
    //        this._thisPage = value;
    //    }
    //}


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
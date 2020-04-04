using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;
using ToyAdditionalData.Controllers;
using ToyAdditionalData.Models;

public partial class myToyAdditional_View : SecurityCheck
{
    public string ErrMsg;
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //權限判斷
                if (false == fn_CheckAuth.Check(fn_Param.CurrentUser, "3711"))
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[參數判斷] - 判斷是否有資料編號
                if (!string.IsNullOrEmpty(Req_DataID))
                {
                    //載入資料
                    LookupData();
                }

            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料取得 --

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        ToyAdditionalRepository _data = new ToyAdditionalRepository();
        Dictionary<int, string> search = new Dictionary<int, string>();


        //----- 原始資料:條件篩選 -----
        search.Add((int)mySearch.DataID, Req_DataID);


        //----- 原始資料:取得所有資料 -----
        var query = _data.GetDataList(search).Take(1).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            this.ph_ErrMessage.Visible = true;
            this.ph_Data.Visible = false;
            this.lt_ShowMsg.Text = "無法取得資料";
            return;
        }

        //填入資料
        this.lt_DataID.Text = query.SeqNo.ToString();
        this.lt_CustTypeName.Text = query.CustTypeName;
        this.lt_CustName.Text = query.CustName;
        this.lt_CustTel.Text = query.CustTel;
        this.lt_CustAddr.Text = query.CustAddr;
        this.lt_Prod.Text = "({0}) {1}".FormatThis(query.ModelNo, query.ModelName);
        this.lt_Qty.Text = query.Qty.ToString();
        this.lt_Remark1.Text = query.Remark1.Replace("\n","<br/>");
        this.lt_Remark2.Text = query.Remark2.Replace("\n", "<br/>");
        this.lt_Remark3.Text = query.Remark3.Replace("\n", "<br/>");
        this.lt_ShipDate.Text = query.ShipDate.ToDateString("yyyy/MM/dd");
        this.lt_ShipNo.Text = query.ShipNo;
        this.lt_Freight.Text = query.Freight.ToString();

        //維護資訊
        this.lt_Creater.Text = query.Create_Name;
        this.lt_CreateTime.Text = query.Create_Time;
        this.lt_Updater.Text = query.Update_Name;
        this.lt_UpdateTime.Text = query.Update_Time;
        this.lt_Shipper.Text = query.Ship_Name;
        this.lt_ShipTime.Text = query.Ship_Time;
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

            return DataID.ToLower().Equals("unknown") ? "TW" : DataID;
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
        return "{0}{1}/{2}/ToyAdditional/{3}".FormatThis(
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

            return DataID.ToLower().Equals("new") ? "" : DataID;
        }
        set
        {
            this._Req_DataID = value;
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
            String Url;
            if (Session["BackListUrl"] == null)
            {
                Url = FuncPath();
            }
            else
            {
                Url = Session["BackListUrl"].ToString();
            }

            return Url;
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }

    #endregion

}
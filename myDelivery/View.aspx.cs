using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using DeliveryData.Controllers;
using Menu3000Data.Models;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class myDelivery_View : SecurityCheck
{
    public string ErrMsg;
    public bool masterAuth = false; //管理者權限(可在權限設定裡勾選)


    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;

                isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3786");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }
                masterAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3787");

                //[權限判斷] End
                #endregion

                if (!IsPostBack)
                {
                    //載入資料
                    LookupData();

                }

            }
            catch (Exception)
            {

                throw;
            }

        }
    }

    #region -- 資料顯示 --

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:取得所有資料 -----
            search.Add("DataID", Req_DataID);

            var query = _data.GetOne(search, out ErrMsg).FirstOrDefault();

            //----- 資料整理:繫結 ----- 
            if (query == null)
            {
                CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
                return;
            }


            #region >> 欄位填寫 <<
            lb_TraceID.Text = query.TraceID;
            lt_ShipType.Text = query.ShipTypeName.ToString();
            lt_ShipWay.Text = query.ShipWayName.ToString();
            lt_PayWay.Text = query.PayWayName.ToString();
            lt_ShipWho.Text = query.ShipWho_Name;
            lt_BoxClass1.Text = query.BoxClass1Name;
            lt_BoxClass2.Text = query.BoxClass2Name;
            lt_TargetClass.Text = query.TargetName;

            lt_SendDate.Text = query.SendDate.ToDateString("yyyy/MM/dd");
            lt_SendComp.Text = query.SendComp;
            lt_SendWho.Text = query.SendWho;
            lt_SendAddr.Text = query.SendAddr;
            lt_SendTel.Text = query.SendTel;
            lt_ShipNo.Text = query.ShipNo;
            lt_ShipPay.Text = query.ShipPay.ToString();
            lt_Box.Text = query.Box.ToString();
            lt_Remark1.Text = query.Remark1.Replace("\r", "<br/>");
            lt_Remark2.Text = query.Remark2.Replace("\r", "<br/>");
            lt_PurNo.Text = query.PurNo;
            lt_SaleNo.Text = query.SaleNo;
            lt_InvoiceNo.Text = query.InvoiceNo;

            #endregion

        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //Release
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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/Delivery".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
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
    /// 設定參數 - 列表頁Url
    /// </summary>
    private string _Page_SearchUrl;
    public string Page_SearchUrl
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("Delivery");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }

    #endregion

}
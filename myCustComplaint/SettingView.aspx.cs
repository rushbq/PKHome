using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myCustComplaint_SettingEdit : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            /* [取得MenuID, MenuName]
                Req_TypeID -> 取得對應的MenuID, 檢查是否有權限
                Req_TypeID + Req_Lang -> 取得對應的Type Name
            */
            var query = fn_Menu.GetOne_RefType(Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg);
            string menuID = query.MenuID.ToString();
            string typeName = query.Label;

            //取得功能名稱
            lt_TypeName.Text = typeName;
            //設定PageTitle
            Page.Title = typeName + " - " + GetLocalResourceObject("pageTitle").ToString();

            //[權限判斷] Start
            #region --權限--
            /* 
             * 判斷對應的MENU ID
             * 取得其他權限
             */
            bool isPass = false;

            switch (Req_TypeID)
            {
                case "10":
                    //台灣工具
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3233");
                    break;

                case "20":
                    //台灣科玩
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3237");
                    break;

                case "30":
                    //中國工具
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3241");
                    break;

                default:
                    //中國科玩
                    isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3245");
                    break;
            }

            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }

            #endregion
            //[權限判斷] End


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


    #region -- 資料顯示:基本資料 --

    /// <summary>
    /// 取得基本資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOneCCPTemp(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }

        int _DTCnt = query.DtCnt;    //商品填寫數
        string _IsCS = query.IsCS;   //客服填寫
        string _IsFreight = query.IsFreight; //收貨填寫


        #region >> 欄位填寫 <<

        //--- 填入基本資料 ---
        lt_TraceID.Text = query.TraceID;
        lt_CreateDate.Text = query.Create_Time.ToDateString("yyyy/MM/dd");


        //--- 填入客服資料 ---
        lb_CustType.Text = query.CustTypeName;
        //-客戶選單
        string _refCustID = query.RefCustID;
        string _refCustName = query.RefCustName;
        if (!string.IsNullOrWhiteSpace(_refCustID))
        {
            lb_CustName.Text = _refCustName;
        }

        lb_Mall.Text = query.RefMallName ?? "&nbsp;";
        lb_CustInput.Text = query.CustInput ?? "&nbsp;";
        lb_BuyerName.Text = query.BuyerName ?? "&nbsp;";
        lb_BuyerPhone.Text = query.BuyerPhone ?? "&nbsp;";
        lb_BuyerAddr.Text = query.BuyerAddr ?? "&nbsp;";
        lt_CS_Who.Text = query.CS_Name ?? "待填寫...";
        lt_CS_Time.Text = query.CS_Time;
        lb_PlanType.Text = query.PlanTypeName.ToString();
        lb_InvoiceIsBack.Text = query.InvoiceIsBack.ToString();
        lb_ERP_ID.Text = query.ERP_ID;
        lb_Platform_ID.Text = query.Platform_ID;
        lb_BadReason.Text = query.BadReasonName;

        //--- 填入收貨資料 ---
        lb_FreightType.Text = query.FreightTypeName ?? "&nbsp;";
        lb_FreightInput.Text = query.FreightInput ?? "&nbsp;";
        lb_FreightGetDate.Text = string.IsNullOrWhiteSpace(query.FreightGetDate) ? "&nbsp;" : query.FreightGetDate;
        lb_InvoiceNumber.Text = query.InvoiceNumber ?? "&nbsp;";
        lb_InvoicePrice.Text = query.InvoicePrice.ToString();
        lb_ShipComp.Text = query.ShipComp ?? "&nbsp;";
        lb_ShipWho.Text = query.ShipWho ?? "&nbsp;";
        lb_ShipTel.Text = query.ShipTel ?? "&nbsp;";
        lb_ShipAddr.Text = query.ShipAddr ?? "&nbsp;";
        lt_Freight_Who.Text = query.Freight_Name ?? "待填寫...";
        lt_Freight_Time.Text = query.Freight_Time;

        #endregion


        #region >> 欄位顯示控制 <<

        ph_ProdCnt.Visible = _DTCnt.Equals(0);
        ph_CS.Visible = _IsCS.Equals("N");
        ph_Freight.Visible = _IsFreight.Equals("N");
        ph_Status.Visible = (_IsCS.Equals("Y") && _IsFreight.Equals("Y") && _DTCnt > 0);


        #endregion


        #region >> 其他功能 <<

        //-- 載入其他資料 --
        LookupData_Detail();
        LookupData_Files();

        #endregion


        //維護資訊
        info_Creater.Text = query.Create_Name;
        info_CreateTime.Text = query.Create_Time;
        info_Updater.Text = query.Update_Name;
        info_UpdateTime.Text = query.Update_Time;

        //Release
        _data = null;

    }

    #endregion

    #region -- 資料顯示:收貨圖片 --

    /// <summary>
    /// 顯示收貨圖片
    /// </summary>
    private void LookupData_Files()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPTempFileList(search);

        //----- 資料整理:繫結 ----- 
        lv_Attachment.DataSource = query;
        lv_Attachment.DataBind();

        //Release
        query = null;
        _data = null;
    }

    #endregion

    #region -- 資料顯示:商品資料 --

    /// <summary>
    /// 顯示商品資料
    /// </summary>
    private void LookupData_Detail()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_Detail(Req_DataID, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        lv_Detail.DataSource = query;
        lv_Detail.DataBind();

        //Release
        _data = null;
        query = null;
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
            _Req_Lang = value;
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
            _Req_RootID = value;
        }
    }

    /// <summary>
    /// 取得網址參數 - TypeID
    /// </summary>
    private string _Req_TypeID;
    public string Req_TypeID
    {
        get
        {
            String DataID = Page.RouteData.Values["typeID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "" : DataID;
        }
        set
        {
            _Req_TypeID = value;
        }
    }

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/CustComplaint/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_TypeID);
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
            _Req_DataID = value;
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
            string tempUrl = CustomExtension.getCookie("HomeList_CCPsetting");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() + "/Set" : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion

    #region -- 上傳參數 --

    /// <summary>
    /// 上傳根目錄
    /// </summary>
    private string _UploadFolder;
    public string UploadFolder
    {
        get
        {
            return "{0}CustComplaint/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
        }
        set
        {
            _UploadFolder = value;
        }
    }

    #endregion
}
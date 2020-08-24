using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myCustComplaint_Edit : SecurityCheck
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
            int _ccType = Convert.ToInt32(Req_TypeID);
            var query = fn_Menu.GetOne_RefType(Req_Lang, _ccType, out ErrMsg);
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
                //[產生選單]
                //1:FlowStatus, 2:客戶類別, 3:收貨方式, 4:一線處理方式, 5:二線處理方式, 6:業務確認處理方式, 7:資材處理方式
                Get_ClassList("4", ddl_Flow201_Type, _ccType, "待處理");
                Get_ClassList("5", ddl_Flow301_Type, _ccType, "待處理");
                Get_ClassList("6", ddl_Flow401_Type, _ccType, "待處理");
                Get_ClassList("7", ddl_Flow501_Type, _ccType, "待處理");


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
        var query = _data.GetOneCCP(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
            return;
        }

        //取得狀態,設定判斷參數
        int _flowStatus = query.FlowStatus;  //FlowID


        #region >> 欄位填寫 <<

        //--- 填入基本資料 ---
        lt_CCUID.Text = query.CC_UID;   //客訴編號
        lt_Status.Text = query.FlowStatusName;  //流程狀態名稱
        Page.Title += "&nbsp;[{0}]".FormatThis(query.FlowStatusName);

        lt_PlanTypeName.Text = query.PlanTypeName;
        lt_CustTypeName.Text = "{0}：{1}".FormatThis(query.CustTypeName, (query.RefCustName) ?? query.RefMallName);
        lt_ModelNo.Text = query.ModelNo;
        lt_Qty.Text = query.Qty.ToString();
        lt_Remark.Text = query.Remark.Replace("\r", "<br/>");
        lt_Remark_Check.Text = query.Remark_Check.Replace("\r", "<br/>");


        //--- 一線F201 ---
        ddl_Flow201_Type.SelectedValue = query.Flow201_Type.ToString();
        lb_Flow201_Desc.Text = string.IsNullOrWhiteSpace(query.Flow201_Desc) ? "&nbsp;" : query.Flow201_Desc.Replace("\r", "<br/>") + "<br/>&nbsp;";
        lt_Flow201_Who.Text = query.Flow201_WhoName ?? "";
        lt_Flow201_Time.Text = query.Flow201_Time;


        //--- 二線F301 ---
        ddl_Flow301_Type.SelectedValue = query.Flow301_Type.ToString();
        lb_Flow301_Desc.Text = string.IsNullOrWhiteSpace(query.Flow301_Desc) ? "&nbsp;" : query.Flow301_Desc.Replace("\r", "<br/>") + "<br/>&nbsp;";
        lt_Flow301_Who.Text = query.Flow301_WhoName ?? "";
        lt_Flow301_Time.Text = query.Flow301_Time;
        lb_FixPrice.Text = query.FixPrice.ToString();
        lb_FixWishDate.Text = string.IsNullOrWhiteSpace(query.FixWishDate) ? "&nbsp;" : query.FixWishDate;
        lb_FixOkDate.Text = string.IsNullOrWhiteSpace(query.FixOkDate) ? "&nbsp;" : query.FixOkDate;
        lb_BadReason.Text = query.BadReasonName;


        //--- 業務F401 ---
        ddl_Flow401_Type.SelectedValue = query.Flow401_Type.ToString();
        lb_Flow401_Desc.Text = string.IsNullOrWhiteSpace(query.Flow401_Desc) ? "&nbsp;" : query.Flow401_Desc.Replace("\r", "<br/>") + "<br/>&nbsp;";
        lt_Flow401_Who.Text = query.Flow401_WhoName ?? "";
        lt_Flow401_Time.Text = query.Flow401_Time;
        lb_FixTotalPrice.Text = query.FixTotalPrice.ToString() ?? "&nbsp;";
        lb_ERP_No1.Text = query.ERP_No1 ?? "&nbsp;";
        lb_ERP_No2.Text = query.ERP_No2 ?? "&nbsp;";
        lb_ERP_No3.Text = query.ERP_No3 ?? "&nbsp;";
        lb_ERP_No4.Text = query.ERP_No4 ?? "&nbsp;";
        lb_ERP_No5.Text = query.ERP_No5 ?? "&nbsp;";
        lb_ERP_No6.Text = query.ERP_No6 ?? "&nbsp;";


        //--- 資材F501 ---
        ddl_Flow501_Type.SelectedValue = query.Flow501_Type.ToString();
        lb_Flow501_Desc.Text = string.IsNullOrWhiteSpace(query.Flow501_Desc) ? "&nbsp;" : query.Flow501_Desc.Replace("\r", "<br/>") + "<br/>&nbsp;";
        lt_Flow501_Who.Text = query.Flow501_WhoName ?? "";
        lt_Flow501_Time.Text = query.Flow501_Time;
        lb_ShipNo.Text = string.IsNullOrWhiteSpace(query.ShipNo) ? "&nbsp;" : query.ShipNo;
        lb_ShipDate.Text = string.IsNullOrWhiteSpace(query.ShipDate) ? "&nbsp;" : query.ShipDate;
        lb_ShipCorp.Text = string.IsNullOrWhiteSpace(query.ShipComp) ? "&nbsp;" : query.ShipComp;

        #endregion


        //-- 載入其他資料 --
        LookupData_Files();
        LookupData_FlowLog();
        LookupData_Info(query.TraceID);
        LookupData_Photos(query.TraceID);


        //維護資訊
        info_Creater.Text = query.Create_Name;
        info_CreateTime.Text = query.Create_Time;
        info_Updater.Text = query.Update_Name;
        info_UpdateTime.Text = query.Update_Time;

        //Release
        _data = null;

    }


    #endregion


    #region -- 資料顯示:檔案附件 --

    /// <summary>
    /// 顯示檔案附件
    /// </summary>
    private void LookupData_Files()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPFileList(Req_DataID, Req_Lang);

        //----- 資料整理:繫結 ----- 
        lv_Attachment.DataSource = query;
        lv_Attachment.DataBind();

        //Release
        query = null;
        _data = null;
    }

    #endregion


    #region -- 資料顯示:客服收貨資料 --

    /// <summary>
    /// 顯示客服收貨資料
    /// </summary>
    /// <param name="traceID"></param>
    private void LookupData_Info(string traceID)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("TraceID", traceID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetOneCCPTemp(search, Req_Lang, Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            return;
        }

        #region >> 欄位填寫 <<

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
        lb_CustInput.Text = string.IsNullOrWhiteSpace(query.CustInput) ? "&nbsp;" : query.CustInput;
        lb_BuyerName.Text = query.BuyerName ?? "&nbsp;";
        lb_BuyerPhone.Text = query.BuyerPhone ?? "&nbsp;";
        lb_BuyerAddr.Text = query.BuyerAddr ?? "&nbsp;";
        lt_CS_Who.Text = query.CS_Name;
        lt_CS_Time.Text = query.CS_Time;
        lb_PlanType.Text = query.PlanTypeName.ToString();
        lb_InvoiceIsBack.Text = query.InvoiceIsBack.ToString();
        lb_ERP_ID.Text = query.ERP_ID;
        lb_Platform_ID.Text = query.Platform_ID;


        //--- 填入收貨資料 ---
        lb_FreightType.Text = query.FreightTypeName ?? "&nbsp;";
        lb_FreightInput.Text = string.IsNullOrWhiteSpace(query.FreightInput) ? "&nbsp;" : query.FreightInput;
        lb_FreightGetDate.Text = string.IsNullOrWhiteSpace(query.FreightGetDate) ? "&nbsp;" : query.FreightGetDate;
        lb_InvoiceNumber.Text = string.IsNullOrWhiteSpace(query.InvoiceNumber) ? "&nbsp;" : query.InvoiceNumber;
        lb_InvoicePrice.Text = query.InvoicePrice.ToString();
        lb_ShipComp.Text = string.IsNullOrWhiteSpace(query.ShipComp) ? "&nbsp;" : query.ShipComp;
        lb_ShipWho.Text = string.IsNullOrWhiteSpace(query.ShipWho) ? "&nbsp;" : query.ShipWho;
        lb_ShipTel.Text = string.IsNullOrWhiteSpace(query.ShipTel) ? "&nbsp;" : query.ShipTel;
        lb_ShipAddr.Text = string.IsNullOrWhiteSpace(query.ShipAddr) ? "&nbsp;" : query.ShipAddr;
        lt_Freight_Who.Text = query.Freight_Name;
        lt_Freight_Time.Text = query.Freight_Time;

        #endregion

        //Release
        _data = null;

    }


    /// <summary>
    /// 顯示收貨圖片
    /// </summary>
    /// <param name="traceID"></param>
    private void LookupData_Photos(string traceID)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("TraceID", traceID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPTempFileList(search);

        //----- 資料整理:繫結 ----- 
        lv_Photos.DataSource = query;
        lv_Photos.DataBind();

        //Release
        query = null;
        _data = null;
    }

    #endregion


    #region -- 資料顯示:流程記錄 --

    /// <summary>
    /// 顯示流程記錄
    /// </summary>
    private void LookupData_FlowLog()
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        //1=UserFlow
        var query = _data.GetCCPLogList(Req_DataID, 1);

        //----- 資料整理:繫結 ----- 
        lv_Timeline.DataSource = query;
        lv_Timeline.DataBind();

        //Release
        query = null;
        _data = null;
    }

    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 取得類別資料 
    /// </summary>
    /// <param name="typeID">Class Type</param>
    /// <param name="ddl">下拉選單object</param>
    /// <param name="rootName">第一選項顯示名稱</param>
    private void Get_ClassList(string typeID, DropDownList ddl, int _ccType, string rootName)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_RefClass(typeID, Req_Lang, _ccType, out ErrMsg);

        //----- 資料整理 -----
        ddl.Items.Clear();

        if (!string.IsNullOrEmpty(rootName))
        {
            ddl.Items.Add(new ListItem(rootName, ""));
        }

        foreach (var item in query)
        {
            ddl.Items.Add(new ListItem(item.Label, item.ID.ToString()));
        }

        //release
        query = null;
        _data = null;
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

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }


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
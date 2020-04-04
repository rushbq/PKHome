using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myCustComplaint_InformConfig : SecurityCheck
{
    public string ErrMsg;
    public bool editAuth = false; //編輯權限(可在權限設定裡勾選)
    public bool closeAuth = false; //作廢權限(可在權限設定裡勾選)

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //[權限判斷] Start
                #region --權限--

                /* [取得MenuID, MenuName]
                  取得對應的MenuID, 檢查是否有權限
                  取得對應的Type Name
               */
                int _ccType = Convert.ToInt32(Req_TypeID);
                var query = fn_Menu.GetOne_RefType(Req_Lang, _ccType, out ErrMsg);
                string menuID = query.MenuID.ToString();
                string typeName = query.Label;

                //取得功能名稱
                lt_TypeName.Text = typeName;
                //設定PageTitle
                Page.Title = typeName + " - 通知群組設定";

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3249");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End


                //[產生選單]
                Get_ClassList("1", filter_FlowStatus, _ccType, GetLocalResourceObject("txt_所有資料").ToString());

                //Get Data
                LookupDataList(Req_PageIdx);

            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示 --

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupDataList(int pageIndex)
    {
        //----- 宣告:網址參數 -----
        int RecordsPerPage = 10;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<

        //[查詢條件] - FlowStatus
        if (!string.IsNullOrWhiteSpace(Req_FlowStatus))
        {
            search.Add("FlowID", Req_FlowStatus);
            PageParam.Add("fs=" + Server.UrlEncode(Req_FlowStatus));
            filter_FlowStatus.SelectedValue = Req_FlowStatus;
        }

        //[查詢條件] - Who
        if (!string.IsNullOrWhiteSpace(Req_Who))
        {
            search.Add("Who", Req_Who);
            PageParam.Add("who=" + Server.UrlEncode(Req_Who));
            val_Emp.Text = Req_Who;
            filter_Emp.Text = Req_Who;
        }
        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPInformCfgList(search, Convert.ToInt32(Req_TypeID), out ErrMsg);

        //----- 資料整理:取得總筆數 -----
        TotalRow = query.Count();

        //----- 資料整理:頁數判斷 -----
        if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
        {
            StartRow = 0;
            pageIndex = 1;
        }

        //----- 資料整理:選取每頁顯示筆數 -----
        var data = query.Skip(StartRow).Take(RecordsPerPage);

        //----- 資料整理:繫結 ----- 
        lvDataList.DataSource = data;
        lvDataList.DataBind();


        //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
        if (query.Count() == 0)
        {
            ph_EmptyData.Visible = true;
            ph_Data.Visible = false;

        }
        else
        {
            ph_EmptyData.Visible = false;
            ph_Data.Visible = true;

            //分頁設定
            string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                , thisPage, PageParam, false, true);

            lt_Pager.Text = getPager;

        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 方法:刪除資料 -----
        if (false == _data.Delete_CCPInform(Get_DataID))
        {
            CustomExtension.AlertMsg("刪除失敗", "");
            return;
        }

        //release
        _data = null;

        //導向本頁
        Response.Redirect(thisPage);
    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {

    }


    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 查詢
    /// </summary>
    protected void btn_Search_Click(object sender, EventArgs e)
    {
        //執行查詢
        Response.Redirect(filterUrl(), false);
    }


    /// <summary>
    /// [按鈕] - 新增
    /// </summary>
    protected void btn_Insert_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 檢查:必填欄位 -----
        string _flowID = filter_FlowStatus.SelectedValue;
        string _who = val_Emp.Text;
        string _mail = val_EMail.Text;
        string errTxt = "";

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_flowID))
        {
            errTxt += "請選擇「流程」\\n";
        }
        if (string.IsNullOrWhiteSpace(_who))
        {
            errTxt += "請選擇「人員」\\n";
        }
        if (string.IsNullOrWhiteSpace(_mail))
        {
            errTxt += "「EMail」空白,請至PKEF基本資料填寫\\n";
        }

        #endregion
        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        //----- 設定:資料欄位 -----
        var dataItem = new CCPInform
        {
            CC_Type = Convert.ToInt16(Req_TypeID),
            FlowID = Convert.ToInt32(_flowID),
            Who = _who,
            Email = _mail
        };

        //----- 方法:建立資料 -----
        if (!_data.CreateCCP_Inform(dataItem, out ErrMsg))
        {
            CustomExtension.AlertMsg("新增失敗", "");
            return;
        }

        //release 
        _data = null;

        //導向本頁
        Response.Redirect(filterUrl());
    }
    #endregion


    #region -- 附加功能 --

    /// <summary>
    /// 取得類別資料 
    /// </summary>
    /// <param name="typeID">typeID</param>
    /// <param name="ddl">下拉選單object</param>
    /// <param name="rootName">第一選項顯示名稱</param>
    private void Get_ClassList(string typeID, DropDownList ddl, int _ccType, string rootName)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCP_RefClass(typeID, Req_Lang, _ccType, out ErrMsg)
            .Where(fld => !(fld.ID.Equals(998)));


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

        query = null;
    }


    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _fs = filter_FlowStatus.SelectedValue;
        string _who = val_Emp.Text;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?page=1".FormatThis(thisPage));


        //[查詢條件] - FlowStatus
        if (!string.IsNullOrWhiteSpace(_fs))
        {
            url.Append("&fs=" + Server.UrlEncode(_fs));
        }
        //[查詢條件] - Who
        if (!string.IsNullOrWhiteSpace(_who))
        {
            url.Append("&who=" + Server.UrlEncode(_who));
        }

        return url.ToString();
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
    /// 取得傳遞參數 - PageIdx(目前索引頁)
    /// </summary>
    public int Req_PageIdx
    {
        get
        {
            int data = Request.QueryString["page"] == null ? 1 : Convert.ToInt32(Request.QueryString["Page"]);
            return data;
        }
        set
        {
            _Req_PageIdx = value;
        }
    }
    private int _Req_PageIdx;


    /// <summary>
    /// FlowStatus
    /// </summary>
    public string Req_FlowStatus
    {
        get
        {
            String _data = Request.QueryString["fs"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_FlowStatus = value;
        }
    }
    private string _Req_FlowStatus;


    /// <summary>
    /// Who
    /// </summary>
    public string Req_Who
    {
        get
        {
            String _data = Request.QueryString["who"];
            return string.IsNullOrWhiteSpace(_data) ? "" : _data;
        }
        set
        {
            _Req_Who = value;
        }
    }
    private string _Req_Who;


    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}/Inform".FormatThis(FuncPath());
        }
        set
        {
            _thisPage = value;
        }
    }
    private string _thisPage;


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

    #endregion

}
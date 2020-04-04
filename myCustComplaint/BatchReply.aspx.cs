using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myCustComplaint_BatchReply : SecurityCheck
{
    public string ErrMsg;
    public bool editAuth = false; //編輯權限(可在權限設定裡勾選)

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
                    editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3234");
                    break;

                case "20":
                    //台灣科玩
                    editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3238");
                    break;

                case "30":
                    //中國工具
                    editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3242");
                    break;

                default:
                    //中國科玩
                    editAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3246");
                    break;
            }

            isPass = editAuth;
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
                Get_ClassList("6", ddl_Flow401_Type, _ccType, GetLocalResourceObject("ddl_請選擇").ToString());


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
        //取得Cookie內容
        string strCCuid = "";
        HttpCookie reqCookie = Request.Cookies["PKHome_CCPInfo"];
        if (reqCookie == null && reqCookie.Values.Count == 0)
        {
            CustomExtension.AlertMsg("目前回覆清單為空白,請在清單頁勾選加入.", Page_SearchUrl);
            return;
        }
        else
        {
            ArrayList ary = new ArrayList();
            foreach (string item in reqCookie.Values)
            {
                ary.Add(item);
            }
            strCCuid = string.Join(",", ary.ToArray());
        }

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        int DataCnt = 0;

        #region >> 條件篩選 <<

        search.Add("Range_CCUID", strCCuid);

        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetCCPAllList(search, Req_Lang, Convert.ToInt32(Req_TypeID)
            , out DataCnt, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        lvDataList.DataSource = query;
        lvDataList.DataBind();

    }

    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //客訴內容
                string _remark = DataBinder.Eval(dataItem.DataItem, "Remark").ToString();
                string _remarkChk = DataBinder.Eval(dataItem.DataItem, "Remark_Check").ToString();
                Literal lt_Content = (Literal)e.Item.FindControl("lt_Content");
                lt_Content.Text = "{0}".FormatThis(string.IsNullOrWhiteSpace(_remarkChk) ? _remark : _remarkChk);

            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    #endregion


    #region -- 資料編輯:基本資料 --

    //Invoke
    protected void btn_doInvoke_Click(object sender, EventArgs e)
    {
        dataProcess(true);
    }

    /// <summary>
    /// 資料處理
    /// </summary>
    /// <param name="passToNext">true:派送下關</param>
    private void dataProcess(bool passToNext)
    {
        //----- [來源資料] 取得Cookie內容 ----- 
        List<string> dataList = new List<string>();
        HttpCookie reqCookie = Request.Cookies["PKHome_CCPInfo"];
        if (reqCookie == null && reqCookie.Values.Count == 0)
        {
            CustomExtension.AlertMsg("目前回覆清單為空白,請在清單頁勾選加入.", Page_SearchUrl);
            return;
        }
        else
        {
            //取得Data Guid
            foreach (string item in reqCookie.Values)
            {
                dataList.Add(reqCookie.Values[item]);
            }
        }


        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        int _flowNow = 401;    //此關卡ID
        int _flowNext = 401;  //下關關卡ID (預設=此關)
        int _ccType = Convert.ToInt32(Req_TypeID);
        string _flowNowName = "";
        string _flowNextName = "";
        string _procTypeName = "";
        string _procType = "";
        string _procDesc = "";
        bool _descCheck = true;
        string errTxt = "";


        //----- 檢查:必填欄位 -----
        _procType = ddl_Flow401_Type.SelectedValue;
        _procDesc = tb_Flow401_Desc.Text;

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_procType))
        {
            errTxt += "請選擇「處理方式」\\n";
        }
        if (string.IsNullOrWhiteSpace(_procDesc) && _descCheck)
        {
            errTxt += "請填寫「處理說明」\\n";
        }

        #endregion

        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, thisPage + "#flow" + _flowNow);
            return;
        }


        //----- [派送下關]取得必要資料 -----
        //派送下關的按鈕使用
        if (passToNext)
        {
            //取得下關ID
            var _GetInvoke = _data.GetOneCCP_RefClass(_procType, Req_Lang, out ErrMsg).FirstOrDefault();
            if (!string.IsNullOrEmpty(_GetInvoke.Invoke_To.ToString()))
            {
                _flowNext = _GetInvoke.Invoke_To;
            }
            //處理方式名稱
            _procTypeName = _GetInvoke.Label;

            //取得Class對應的顯示名稱
            _flowNowName = _data.GetOneCCP_RefClass(_flowNow.ToString(), Req_Lang, out ErrMsg).FirstOrDefault().Label;
            _flowNextName = _data.GetOneCCP_RefClass(_flowNext.ToString(), Req_Lang,out ErrMsg).FirstOrDefault().Label;
            
        }

        //----- 取得其他欄位 -----
        double _FixTotalPrice = Convert.ToDouble(tb_FixTotalPrice.Text);
        string _ERP_No1 = tb_ERP_No1.Text;
        string _ERP_No2 = tb_ERP_No2.Text;
        string _ERP_No3 = tb_ERP_No3.Text;
        string _ERP_No4 = tb_ERP_No4.Text;
        string _ERP_No5 = tb_ERP_No5.Text;
        string _ERP_No6 = tb_ERP_No6.Text;

        //******* 資料迴圈 Start *******//
        for (int row = 0; row < dataList.Count; row++)
        {
            #region -- 資料處理 --

            //Get ID
            string _dataID = dataList[row];

            //----- 設定:資料欄位 -----
            var dataItem = new CCPItem
            {
                Data_ID = new Guid(_dataID),
                FlowStatus = _flowNow,
                inputType = _procType,
                inputDesc = _procDesc,
                nextFlow = _flowNext,
                FixTotalPrice = _FixTotalPrice,
                ERP_No1 = _ERP_No1,
                ERP_No2 = _ERP_No2,
                ERP_No3 = _ERP_No3,
                ERP_No4 = _ERP_No4,
                ERP_No5 = _ERP_No5,
                ERP_No6 = _ERP_No6,
                Update_Who = fn_Param.CurrentUser
            };

            //----- 方法:更新資料 -----
            if (!_data.UpdateCCP_Data(dataItem, out ErrMsg))
            {
                //[System Log] CreateCCP_Log
                var log = new CCPLog
                {
                    Parent_ID = new Guid(_dataID),
                    LogType = 2,
                    LogSubject = "資料更新失敗(UpdateCCP_Data)",
                    LogDesc = ErrMsg,
                    Create_Who = fn_Param.CurrentUser
                };
                _data.CreateCCP_Log(log, out ErrMsg);

                //Show Message
                ph_ErrMessage.Visible = true;
                lt_ShowMsg.Text = ErrMsg + _dataID;
                CustomExtension.AlertMsg("資料更新失敗", "");
                return;
            }


            //[Log & Mail]判斷是否派送下關
            if (passToNext)
            {
                //下一關訊息
                string _flowNextMsg = "";
                if (_flowNext.Equals(999))
                {
                    //結案
                    _flowNextMsg = "<b class=\"red-text text-darken-4\">--- {0} ---</b>".FormatThis(_flowNextName);
                }
                else
                {
                    //下一關
                    _flowNextMsg = "{0}:<span class=\"green-text text-darken-3\">{1}</u>".FormatThis("下一關", _flowNextName);
                }

                //[Flow Log]
                var log = new CCPLog
                {
                    Parent_ID = new Guid(_dataID),
                    LogType = 1,
                    LogSubject = "[{0}]".FormatThis(_flowNowName),
                    LogDesc = "<strong>({0})</strong><p>{1}</p><div>{2}</div>".FormatThis(_procTypeName, _procDesc, _flowNextMsg),
                    FlowID = _flowNow,
                    Create_Who = fn_Param.CurrentUser
                };
                _data.CreateCCP_Log(log, out ErrMsg);


                /*
                  [寄通知信]:下一關人員
                  - X=結案(通知所有人), B=其他
                */
                if (!_data.doSendInformMail(_flowNext.Equals(999) ? "X" : "B"
                    , _dataID, _flowNext.ToString(), FuncPath()
                    , Req_Lang, Req_TypeID, out ErrMsg))
                {
                    //[System Log] CreateCCP_Log
                    var logMail = new CCPLog
                    {
                        Parent_ID = new Guid(_dataID),
                        LogType = 2,
                        LogSubject = "通知信發送失敗, flowNext={0}".FormatThis(_flowNext.ToString()),
                        LogDesc = ErrMsg,
                        Create_Who = fn_Param.CurrentUser
                    };
                    _data.CreateCCP_Log(logMail, out ErrMsg);

                    //Show Message
                    CustomExtension.AlertMsg("關卡已派送，但通知信發送失敗.", "");
                }

            }

            #endregion
        }

        //release 
        _data = null;

        //******* 資料迴圈 End *******//


        //清空Cookie
        HttpCookie setCookie = new HttpCookie("PKHome_CCPInfo");
        //set expire
        setCookie.Expires = DateTime.Now.AddDays(-1);
        setCookie.Values.Clear();
        //reset
        Response.Cookies.Set(setCookie);

        //導向列表頁
        Response.Redirect(Page_SearchUrl);

    }

    #endregion

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
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/BatchReply".FormatThis(FuncPath());
        }
        set
        {
            _thisPage = value;
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

    #endregion


}
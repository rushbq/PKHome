using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using DeliveryData.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myPostal_InformConfig : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //[權限判斷] Start
                #region --權限--

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3786");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End


                //Get Data
                LookupDataList();

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
    private void LookupDataList()
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        #region >> 條件篩選 <<

        //[查詢條件] - Who
        search.Add("Who", fn_Param.CurrentUser);

        #endregion

        //----- 原始資料:取得所有資料 -----
        var data = _data.GetAddress(search, out ErrMsg);

        //----- 資料整理:繫結 ----- 
        lvDataList.DataSource = data;
        lvDataList.DataBind();
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();

        try
        {
            //----- 方法:刪除資料 -----
            if (false == _data.Delete_Address(Get_DataID, out ErrMsg))
            {
                CustomExtension.AlertMsg("刪除失敗", "");
                return;
            }
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

        //導向本頁
        Response.Redirect(thisPage);
    }

    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 新增
    /// </summary>
    protected void btn_Insert_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();

        try
        {
            //----- 檢查:必填欄位 -----
            string _toComp = tb_ToComp.Text.Trim();
            string _toWho = tb_ToWho.Text.Trim();
            string _toAddr = tb_ToAddr.Text.Trim();
            string _toTel = tb_ToTel.Text.Trim();
            string errTxt = "";

            #region ** 欄位判斷 **

            if (string.IsNullOrWhiteSpace(_toWho))
            {
                errTxt += "請填寫「收件人」\\n";
            }
            if (string.IsNullOrWhiteSpace(_toAddr))
            {
                errTxt += "請填寫「收件地址」\\n";
            }

            #endregion
            //顯示不符規則的警告
            if (!string.IsNullOrEmpty(errTxt))
            {
                CustomExtension.AlertMsg(errTxt, "");
                return;
            }

            //----- 設定:資料欄位 -----
            var dataItem = new AddressBook
            {
                ToComp = _toComp,
                ToWho = _toWho,
                ToAddr = _toAddr,
                ToTel = _toTel
            };

            //----- 方法:建立資料 -----
            if (!_data.Create_Address(dataItem, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }
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

        //導向本頁
        Response.Redirect(thisPage);
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
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}/Address".FormatThis(FuncPath());
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
            string tempUrl = CustomExtension.getCookie("Delivery");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myCustComplaint_Print1 : System.Web.UI.Page
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
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
        var query = _data.GetOneCCP(search, "CN", Convert.ToInt32(Req_TypeID), out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料.", "");
            return;
        }

        #region >> 欄位填寫 <<

        //--- 填入基本資料 ---
        lt_CCUID.Text = query.CC_UID;   //客訴編號
        Page.Title += "-" + query.CC_UID;

        lt_CustTypeName.Text = "{0}：{1}".FormatThis(query.CustTypeName, (query.RefCustName) ?? query.RefMallName);
        lt_ModelNo.Text = query.ModelNo;
        lt_Qty.Text = query.Qty.ToString();

        info_CreateTime.Text = query.Create_Time;

        #endregion

        //Release
        _data = null;

    }


    #endregion


    #region -- 網址參數 --

    /// <summary>
    /// 取得網址參數 - TypeID
    /// </summary>
    private string _Req_TypeID;
    public string Req_TypeID
    {
        get
        {
            String DataID = Request.QueryString["typeID"];

            return DataID;
        }
        set
        {
            _Req_TypeID = value;
        }
    }

    /// <summary>
    /// 取得傳遞參數 - 資料編號
    /// </summary>
    private string _Req_DataID;
    public string Req_DataID
    {
        get
        {
            String DataID = Request.QueryString["id"];

            return DataID;
        }
        set
        {
            _Req_DataID = value;
        }
    }
    #endregion


}
using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;

public partial class myPurPlan_SearchByProd : SecurityCheck
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
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4214");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End

            }
        }
        catch (Exception)
        {

            throw;
        }
    }



    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 匯出
    /// </summary>
    protected void btn_Excel_Click(object sender, EventArgs e)
    {
     
    }

    #endregion


    #region -- 附加功能 --


    #endregion


    #region -- 網址參數 --

    /// <summary>
    /// 取得網址參數 - Company ID(TW/SH/SZ)
    /// </summary>
    private string _Req_CompID;
    public string Req_CompID
    {
        get
        {
            String DataID = Request.QueryString["CompID"] == null ? "" : Request.QueryString["CompID"].ToString();

            return DataID.ToLower().Equals("") ? "SH" : DataID;
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
        return "";
        //return "{0}{1}/{2}/ProdStockStat".FormatThis(
        //    fn_Param.WebUrl
        //    , Req_Lang
        //    , Req_RootID);
    }

    #endregion

}
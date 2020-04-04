﻿using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CustomController;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class myDemo_Search : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //[權限判斷] 
                //bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4152");

                //if (!isPass)
                //{
                //    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                //    return;
                //}


                //Pagination
                this.lt_Pager.Text = CustomExtension.Pagination(85, 10, 2, 5, fn_Param.WebUrl, null, false, true);
                
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    protected void btn_Search_Click(object sender, EventArgs e)
    {
       // Response.Write(cb_Pay1.Checked);
    }


    #region -- 網址參數 --

    ///// <summary>
    ///// 取得網址參數 - 語系
    ///// </summary>
    //public string Req_Lang
    //{
    //    get
    //    {
    //        string myLang = Page.RouteData.Values["lang"] == null ? "auto" : Page.RouteData.Values["lang"].ToString();

    //        //若為auto, 就去抓cookie
    //        return myLang.Equals("auto") ? fn_Language.Get_Lang(Request.Cookies["PKHome_Lang"].Value) : myLang;
    //    }
    //    set
    //    {
    //        this._Req_Lang = value;
    //    }
    //}
    //private string _Req_Lang;


    ///// <summary>
    ///// 取得網址參數 - RootID
    ///// </summary>
    //private string _Req_RootID;
    //public string Req_RootID
    //{
    //    get
    //    {
    //        String DataID = Page.RouteData.Values["rootID"].ToString();

    //        return DataID.ToLower().Equals("unknown") ? "" : DataID;
    //    }
    //    set
    //    {
    //        this._Req_RootID = value;
    //    }
    //}


    ///// <summary>
    ///// 取得此功能的前置路徑
    ///// </summary>
    ///// <returns></returns>
    //public string FuncPath()
    //{
    //    return "{0}{1}/{2}/DelayShipStat".FormatThis(
    //        fn_Param.WebUrl
    //        , Req_Lang
    //        , Req_RootID);
    //}

    #endregion


}
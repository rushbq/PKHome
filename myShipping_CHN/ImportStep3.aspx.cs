using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using PKLib_Method.Methods;
using ShipFreight_CN.Controllers;
using ShipFreight_CN.Models;

public partial class myShipping_ImportStep3 : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;

                //A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具
                switch (Req_DataType)
                {
                    case "A":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3775");
                        break;

                    case "B":
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3776");
                        break;

                    case "C":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3777");
                        break;

                    case "D":
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3778");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //取得公司別
                string _corpName = "中國內銷({0})".FormatThis(fn_Menu.GetShipping_RefType(Req_DataType));
                lt_CorpName.Text = _corpName;
                Page.Title += "-" + _corpName;

                //[權限判斷] End
                #endregion


                //取得資料
                LookupData();

            }
            catch (Exception)
            {

                throw;
            }

        }
    }


    #region -- 資料讀取 --
    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----
        search.Add("DataID", Req_DataID);


        //----- 原始資料:取得所有資料 -----
        string defComp = Req_DataType.Equals("1") ? "CHN1" : "CHN2";
        var query = _data.GetShipImportList(search, defComp, out ErrMsg).Take(1)
            .Select(fld => new
            {
                TraceID = fld.TraceID,
                UploadType = fld.Upload_Type,
                UploadTypeName = fld.Upload_TypeName,
                status = fld.Status

            }).FirstOrDefault();

        //----- 資料整理:填入資料 -----
        lb_TraceID.Text = query.TraceID;
        hf_Type.Value = query.UploadType;
        lb_TypeName.Text = query.UploadTypeName;
        decimal _status = query.status;

        query = null;
        _data = null;

        //判斷是否已結案
        if (_status.Equals(30))
        {
            Response.Redirect(FuncPath() + "?dt=" + Req_DataType);
        }
    }

    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// 下一步
    /// </summary>
    protected void lbtn_Next_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

        //填入基本資料Inst
        var baseData = new ShipImportData
        {
            Data_ID = new Guid(Req_DataID),
            Upload_Type = hf_Type.Value,
            Update_Who = fn_Param.CurrentUser
        };

        //單號,運費轉入
        if (!_data.UpdateShipImport(baseData, out ErrMsg))
        {
            string msg = "物流單轉入失敗 (Step3);" + ErrMsg;

            //Show Error
            this.ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
        }

        _data = null;

        //導至下一步
        ph_ErrMessage.Visible = false;
        Response.Redirect("{0}/Step4/{1}?dt={2}".FormatThis(FuncPath(), Req_DataID, Req_DataType));

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
    /// 資料判別:A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具
    /// </summary>
    public string Req_DataType
    {
        get
        {
            string data = Request.QueryString["dt"] == null ? "A" : Request.QueryString["dt"].ToString();
            return data;
        }
        set
        {
            this._Req_DataType = value;
        }
    }
    private string _Req_DataType;

    /// <summary>
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/ShipImportCHN".FormatThis(
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
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/Step3?dt=".FormatThis(FuncPath(), Req_DataType);
        }
        set
        {
            this._thisPage = value;
        }
    }

    #endregion

}
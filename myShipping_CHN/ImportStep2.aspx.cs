﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using LinqToExcel;
using ShipFreight_CN.Controllers;
using ShipFreight_CN.Models;
using PKLib_Method.Methods;

public partial class myShipping_ImportStep2 : SecurityCheck
{
    public string ErrMsg;

    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

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
                FileName = fld.Upload_File,
                UploadType = fld.Upload_Type,
                UploadTypeName = fld.Upload_TypeName,
                status = fld.Status

            }).FirstOrDefault();

        //----- 資料整理:填入資料 -----
        string _traceID = query.TraceID;
        string _fileName = query.FileName;
        string _typeID = query.UploadType;
        string _typeName = query.UploadTypeName;
        decimal _status = query.status;

        query = null;
        _data = null;


        //判斷是否已結案
        if (_status.Equals(30))
        {
            Response.Redirect(FuncPath());
            return;
        }

        //完整路徑
        string filePath = @"{0}{1}{2}".FormatThis(
            System.Web.Configuration.WebConfigurationManager.AppSettings["FTP_DiskUrl"]
            , UploadFolder(_traceID).Replace("/", "\\")
            , _fileName);

        //填入表單欄位
        lb_TraceID.Text = _traceID;
        hf_FullFileName.Value = filePath; //完整路徑
        hf_TraceID.Value = _traceID;
        hf_Type.Value = _typeID;
        lb_TypeName.Text = _typeName;

        //----- [元件][LinqToExcel] - 取得工作表 -----
        Set_SheetMenu(filePath);
    }


    /// <summary>
    /// 產生工作表選單
    /// </summary>
    /// <param name="filePath"></param>
    private void Set_SheetMenu(string filePath)
    {
        //查詢Excel
        var excelFile = new ExcelQueryFactory(filePath);

        //取得Excel 頁籤
        var data = excelFile.GetWorksheetNames();

        this.ddl_Sheet.Items.Clear();
        this.ddl_Sheet.Items.Add(new ListItem("選擇要匯入的工作表", ""));

        foreach (var item in data)
        {
            this.ddl_Sheet.Items.Add(new ListItem(item.ToString(), item.ToString()));
        }
    }
    #endregion


    #region -- 按鈕事件 --
    /// <summary>
    /// 重新上傳:刪除資料及檔案
    /// </summary>
    protected void lbtn_ReNew_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

        //----- 方法:刪除資料 -----
        if (false == _data.Delete_ShipImport(Req_DataID))
        {
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = "重新上傳操作失敗";
            return;
        }
        else
        {
            //刪除整個Folder檔案
            _ftp.FTP_DelFolder(UploadFolder(hf_TraceID.Value));

            //導向至Step1
            Response.Redirect("{0}/Step1?dt={1}".FormatThis(FuncPath(), Req_DataType));
        }

    }

    /// <summary>
    /// 下一步
    /// </summary>
    protected void lbtn_Next_Click(object sender, EventArgs e)
    {
        #region -- [Check] 欄位檢查 --

        //declare
        string errTxt = "";
        string _sheet = ddl_Sheet.SelectedValue;

        //Check Sheet
        if (string.IsNullOrWhiteSpace(_sheet))
        {
            errTxt += "[檢查] 請選擇工作表";
        }

        //show alert
        if (!string.IsNullOrWhiteSpace(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        #endregion

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

        //[Excel] - 取得參數
        string _traceID = hf_TraceID.Value;
        string _sheetName = ddl_Sheet.SelectedValue;
        string _filePath = hf_FullFileName.Value;
        string _type = hf_Type.Value;

        //填入基本資料Inst
        var baseData = new ShipImportData
        {
            Data_ID = new Guid(Req_DataID),
            TraceID = _traceID,
            Sheet_Name = _sheetName,
            Update_Who = fn_Param.CurrentUser
        };

        //填入單身資料Inst - 取得Excel資料欄位
        var query_Xls = _data.GetExcel_DT(_filePath, _sheetName, _traceID, _type);


        //寫入單身資料, 更新單頭欄位
        if (!_data.CreateShipImportDT(baseData, query_Xls, out ErrMsg))
        {
            string msg = "單身資料填入失敗 (Step2);" + ErrMsg;

            //Show Error
            this.ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = msg;
            return;
        }

        _data = null;

        //導至下一步
        ph_ErrMessage.Visible = false;
        Response.Redirect("{0}/Step3/{1}?dt={2}".FormatThis(FuncPath(), Req_DataID, Req_DataType));

    }


    /// <summary>
    /// 選擇工作表, 產生預覽資料 - onChange
    /// </summary>
    protected void ddl_Sheet_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.ddl_Sheet.SelectedIndex > 0 && !string.IsNullOrEmpty(this.hf_FullFileName.Value))
        {
            //宣告
            StringBuilder html = new StringBuilder();
            var filePath = hf_FullFileName.Value;
            string sheetName = ddl_Sheet.SelectedValue;
            string typeID = hf_Type.Value; //A=電商;B=經銷商

            //取得資料
            ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

            html = _data.GetExcel_Html(filePath, sheetName, typeID);

            //Output Html
            this.lt_tbBody.Text = html.ToString();
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
    /// 資料判別:A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具具
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


    /// <summary>
    /// 上傳目錄(+TraceID)
    /// </summary>
    /// <param name="traceID"></param>
    /// <returns></returns>
    private string UploadFolder(string traceID)
    {
        return "{0}ShipImport/{1}/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"], traceID);
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
            return "{0}/Step2?dt=".FormatThis(FuncPath(), Req_DataType);
        }
        set
        {
            this._thisPage = value;
        }
    }

    #endregion

}
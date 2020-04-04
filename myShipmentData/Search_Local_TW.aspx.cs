using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LinqToExcel;
using Menu3000Data.Controllers;
using Menu3000Data.Models;
using PKLib_Method.Methods;

public partial class myShipmentData_Search_Local_TW : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;
    public IQueryable<ClassItem> _shipItem;
    public IQueryable<ClassItem> _custItem;
    public IQueryable<ClassItem> _prodItem;
    public IQueryable<ClassItem> _sendItem;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //[權限判斷] Start
                #region --權限--

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3774");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End


                //載入表格前,先取得下拉清單
                _shipItem = GetClassMenu("4");
                _custItem = GetClassMenu("5");
                _prodItem = GetClassMenu("6");
                _sendItem = GetClassMenu("7");

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
        int DataCnt = 0;
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            #region >> 條件篩選 <<

            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(Req_Keyword))
            {
                search.Add("Keyword", Req_Keyword);
                PageParam.Add("k=" + Server.UrlEncode(Req_Keyword));
                filter_Keyword.Text = Req_Keyword;
            }
            //[取得/檢查參數] - Date
            if (!string.IsNullOrWhiteSpace(Req_sDate))
            {
                search.Add("sDate", Req_sDate);
                PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
                filter_sDate.Text = Req_sDate;
            }
            if (!string.IsNullOrWhiteSpace(Req_eDate))
            {
                search.Add("eDate", Req_eDate);
                PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
                filter_eDate.Text = Req_eDate;
            }

            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetShipLocalData(search, StartRow, RecordsPerPage, out DataCnt, out ErrMsg);

            //----- 資料整理:取得總筆數 -----
            TotalRow = DataCnt;

            //----- 資料整理:頁數判斷 -----
            if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
            {
                StartRow = 0;
                pageIndex = 1;
            }

            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
            lvDataList.DataBind();


            //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
            if (query.Count() == 0)
            {
                ph_EmptyData.Visible = true;
                ph_Data.Visible = false;
                ph_Save.Visible = false;
            }
            else
            {
                ph_EmptyData.Visible = false;
                ph_Data.Visible = true;

                //分頁設定
                string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                    , FuncPath(), PageParam, false, true);
                lt_Pager.Text = getPager;

            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            _data = null;
        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        //string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                #region >> 下拉選單處理 <<

                //-- 貨運公司 --
                DropDownList _ship = (DropDownList)e.Item.FindControl("ddl_Ship");

                //設定繫結
                _ship.DataSource = _shipItem;
                _ship.DataTextField = "Label";
                _ship.DataValueField = "ID";
                _ship.DataBind();

                //新增root item
                _ship.Items.Insert(0, new ListItem("請選擇", ""));


                //取得設定值
                string _shipID = DataBinder.Eval(dataItem.DataItem, "ShipID").ToString();
                //勾選已設定值
                _ship.SelectedIndex = _ship.Items.IndexOf(_ship.Items.FindByValue(_shipID));


                //-- 客戶別 --
                DropDownList _cust = (DropDownList)e.Item.FindControl("ddl_CustType");

                //設定繫結
                _cust.DataSource = _custItem;
                _cust.DataTextField = "Label";
                _cust.DataValueField = "ID";
                _cust.DataBind();

                //新增root item
                _cust.Items.Insert(0, new ListItem("請選擇", ""));

                //取得設定值
                string _custTypeID = DataBinder.Eval(dataItem.DataItem, "CustType").ToString();
                //勾選已設定值
                _cust.SelectedIndex = _cust.Items.IndexOf(_cust.Items.FindByValue(_custTypeID));



                //-- 商品類別 --
                DropDownList _prodType = (DropDownList)e.Item.FindControl("ddl_ProdType");

                //設定繫結
                _prodType.DataSource = _prodItem;
                _prodType.DataTextField = "Label";
                _prodType.DataValueField = "ID";
                _prodType.DataBind();

                //新增root item
                _prodType.Items.Insert(0, new ListItem("請選擇", ""));

                //取得設定值
                string _prodTypeID = DataBinder.Eval(dataItem.DataItem, "ProdType").ToString();
                //勾選已設定值
                _prodType.SelectedIndex = _prodType.Items.IndexOf(_prodType.Items.FindByValue(_prodTypeID));



                //-- 發票寄送方式 --
                DropDownList _sendType = (DropDownList)e.Item.FindControl("ddl_SendType");

                //設定繫結
                _sendType.DataSource = _sendItem;
                _sendType.DataTextField = "Label";
                _sendType.DataValueField = "ID";
                _sendType.DataBind();

                //新增root item
                _sendType.Items.Insert(0, new ListItem("請選擇", ""));

                //取得設定值
                string _sendTypeID = DataBinder.Eval(dataItem.DataItem, "SendType").ToString();
                //勾選已設定值
                _sendType.SelectedIndex = _sendType.Items.IndexOf(_sendType.Items.FindByValue(_sendTypeID));

                #endregion


            }
        }
        catch (Exception)
        {
            throw;
        }
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
    /// [按鈕] - Save
    /// </summary>
    protected void btn_Save_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        try
        {
            //取得Listview控制項
            ListView _view = lvDataList;

            //Check null
            if (_view.Items.Count == 0)
            {
                CustomExtension.AlertMsg("無資料可設定,請確認ERP銷貨單.", filterUrl());
                return;
            }

            //宣告
            List<ShipData_LocalItem> dataList = new List<ShipData_LocalItem>();

            //取得各欄位資料
            for (int row = 0; row < _view.Items.Count; row++)
            {
                #region ** 取得欄位資料 **

                string _DataID = ((HiddenField)_view.Items[row].FindControl("hf_DataID")).Value;
                string _SO_FID = ((HiddenField)_view.Items[row].FindControl("hf_SO_FID")).Value;
                string _SO_SID = ((HiddenField)_view.Items[row].FindControl("hf_SO_SID")).Value;
                string _CustType = ((DropDownList)_view.Items[row].FindControl("ddl_CustType")).SelectedValue;
                string _ProdType = ((DropDownList)_view.Items[row].FindControl("ddl_ProdType")).SelectedValue;
                string _BoxCnt = ((TextBox)_view.Items[row].FindControl("tb_BoxCnt")).Text;
                string _ShipID = ((DropDownList)_view.Items[row].FindControl("ddl_Ship")).SelectedValue;
                string _ShipNo = ((TextBox)_view.Items[row].FindControl("tb_ShipNo")).Text;
                string _Freight = ((TextBox)_view.Items[row].FindControl("tb_Freight")).Text;
                string _SendType = ((DropDownList)_view.Items[row].FindControl("ddl_SendType")).SelectedValue;
                string _SendNo = ((TextBox)_view.Items[row].FindControl("tb_SendNo")).Text;
                string _Remark = ((TextBox)_view.Items[row].FindControl("tb_Remark")).Text;

                #endregion

                //將值填入容器
                var dataItem = new ShipData_LocalItem
                {
                    Data_ID = string.IsNullOrWhiteSpace(_DataID) ? new Guid(CustomExtension.GetGuid()) : new Guid(_DataID),
                    SO_FID = _SO_FID,
                    SO_SID = _SO_SID,
                    CustType = string.IsNullOrWhiteSpace(_CustType) ? 0 : Convert.ToUInt16(_CustType),
                    ProdType = string.IsNullOrWhiteSpace(_ProdType) ? 0 : Convert.ToUInt16(_ProdType),
                    BoxCnt = string.IsNullOrWhiteSpace(_BoxCnt) ? 0 : Convert.ToUInt16(_BoxCnt),
                    ShipID = string.IsNullOrWhiteSpace(_ShipID) ? 0 : Convert.ToUInt16(_ShipID),
                    ShipNo = _ShipNo,
                    Freight = string.IsNullOrWhiteSpace(_Freight) ? 0 : Convert.ToUInt16(_Freight),
                    SendType = string.IsNullOrWhiteSpace(_SendType) ? 0 : Convert.ToUInt16(_SendType),
                    SendNo = _SendNo,
                    Remark = _Remark,
                    Create_Who = fn_Param.CurrentUser
                };

                //add to list
                dataList.Add(dataItem);
            }

            //Call function
            if (!_data.Check_ShipLocalData(dataList, out ErrMsg))
            {
                CustomExtension.AlertMsg("資料儲存失敗...", "");
                return;
            }

            //redirect page
            Response.Redirect(thisPage);

        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            _data = null;
        }

    }


    /// <summary>
    /// [按鈕] - 匯出
    /// </summary>
    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);
        }
        //[取得/檢查參數] - Date
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate);
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate);
        }
        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetAllShipLocalData(search, out ErrMsg)
            .Select(fld => new
            {
                日期 = fld.SO_Date,
                客戶 = fld.CustName + "(" + fld.CustID + ")",
                客戶別 = fld.CustTypeName,
                商品類別 = fld.ProdTypeName,
                銷貨單號 = fld.SO_FID + "-" + fld.SO_SID,
                出貨單金額 = fld.Price,
                OPCS筆數 = fld.OpcsCnt,
                件數 = fld.BoxCnt,
                出貨方式 = fld.ShipName,
                托運單號 = fld.ShipNo,
                運費金額 = fld.Freight,
                運費比 = fld.Cnt_FreightPercent,
                業務 = fld.SalesName,
                發票號碼_起 = fld.InvNo_Start,
                發票號碼_迄 = fld.InvNo_End,
                發票寄送方式 = fld.SendTypeName,
                發票寄出單號 = fld.SendNo,
                備註 = fld.Remark
            });
  
        //Check null
        if (query.Count() == 0)
        {
            CustomExtension.AlertMsg("查無資料", filterUrl());
            return;
        }


        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);


        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "{0}-{1}.xlsx".FormatThis("出貨明細表-台灣內銷", DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }



    /// <summary>
    /// [按鈕] - 匯入運費
    /// </summary>
    protected void btn_Import_Click(object sender, EventArgs e)
    {
        string goUrl = thisPage;

        #region -- 檔案處理 --

        //宣告
        List<IOTempParam> ITempList = new List<IOTempParam>();
        Random rnd = new Random();
        string Message = "";
        string ftpFolder = UploadFolder; //FTP資料夾
        string thisFileName = ""; //檔名

        if (freightImport.PostedFile.ContentLength == 0)
        {
            CustomExtension.AlertMsg("請選擇要上傳的檔案", goUrl);
            return;
        }

        //取得上傳檔案集合
        HttpFileCollection hfc = Request.Files;

        //--- 檔案檢查 ---
        for (int idx = 0; idx <= hfc.Count - 1; idx++)
        {
            //取得個別檔案
            HttpPostedFile hpf = hfc[idx];

            if (hpf.ContentLength > FileSizeLimit)
            {
                //[提示]
                Message = "檔案大小超出限制, 每個檔案大小限制為 {0} MB".FormatThis(FileSizeLimit);
                CustomExtension.AlertMsg(Message, goUrl);
                return;
            }

            if (hpf.ContentLength > 0)
            {
                //取得原始檔名
                string OrgFileName = System.IO.Path.GetFileName(hpf.FileName);
                //取得副檔名
                string FileExt = System.IO.Path.GetExtension(OrgFileName).ToLower();
                if (false == CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //[提示]
                    Message = "檔案副檔名不符規定, 僅可上傳副檔名為 {0}".FormatThis(FileExtLimit.Replace("|", ", "));
                    CustomExtension.AlertMsg(Message, goUrl);
                    return;
                }
            }
        }


        //--- 檔案暫存List ---
        for (int idx = 0; idx <= hfc.Count - 1; idx++)
        {
            //取得個別檔案
            HttpPostedFile hpf = hfc[idx];

            if (hpf.ContentLength > 0)
            {
                //取得原始檔名
                string OrgFileName = System.IO.Path.GetFileName(hpf.FileName);
                //取得副檔名
                string FileExt = System.IO.Path.GetExtension(OrgFileName).ToLower();

                //設定檔名, 重新命名
                string myFullFile = String.Format(@"{0:yyMMddHHmmssfff}{1}{2}"
                    , DateTime.Now
                    , rnd.Next(0, 99)
                    , FileExt);

                //暫存檔名
                thisFileName = myFullFile;

                //判斷副檔名, 未符合規格的檔案不上傳
                if (CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //設定暫存-檔案
                    ITempList.Add(new IOTempParam(myFullFile, OrgFileName, hpf));
                }
            }
        }

        #endregion

        //Check Null
        if (ITempList.Count == 0)
        {
            CustomExtension.AlertMsg("請選擇要上傳的檔案", goUrl);
            return;
        }

        #region -- 儲存檔案 --

        int errCnt = 0;

        //判斷資料夾, 不存在則建立
        _ftp.FTP_CheckFolder(ftpFolder);

        //暫存檔案List
        for (int row = 0; row < ITempList.Count; row++)
        {
            //取得個別檔案
            HttpPostedFile hpf = ITempList[row].Param_hpf;

            //執行上傳
            if (false == _ftp.FTP_doUpload(hpf, ftpFolder, ITempList[row].Param_FileName))
            {
                errCnt++;
            }
        }

        if (errCnt > 0)
        {
            Message = "檔案上傳失敗, 失敗筆數為 {0} 筆, 請重新整理後再上傳!".FormatThis(errCnt);
            CustomExtension.AlertMsg(Message, goUrl);
            return;
        }

        #endregion


        #region -- 資料處理 --

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //設定完整路徑
        string _filePath = @"{0}{1}{2}".FormatThis(
            System.Web.Configuration.WebConfigurationManager.AppSettings["FTP_DiskUrl"]
            , ftpFolder.Replace("/", "\\")
            , thisFileName);


        //查詢Excel
        var excelFile = new ExcelQueryFactory(_filePath);

        //取得Excel 第一個頁籤名稱
        var sheetData = excelFile.GetWorksheetNames().FirstOrDefault();

        //取得Excel資料欄位
        var query_Xls = _data.GetExcel_ShipNoData(_filePath, sheetData);

        try
        {
            //回寫
            if (!_data.Check_ShipLocalData_Freight(query_Xls, out ErrMsg))
            {
                //Response.Write(ErrMsg);
                CustomExtension.AlertMsg("資料匯入失敗", goUrl);
                return;
            }
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //刪除檔案
            _ftp.FTP_DelFile(ftpFolder, thisFileName);
            _data = null;
        }
        #endregion


        //Redirect
        CustomExtension.AlertMsg("匯入完成.", goUrl);

    }
    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 取得下拉清單
    /// </summary>
    private IQueryable<ClassItem> GetClassMenu(string type)
    {
        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        try
        {
            //取得資料
            var data = _data.GetRefClass_ShipData(Req_Lang, type, out ErrMsg);

            return data;

        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            _data = null;
        }
    }

    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _sDate = filter_sDate.Text;
        string _eDate = filter_eDate.Text;
        string _Keyword = filter_Keyword.Text;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page
        url.Append("{0}?page=1".FormatThis(FuncPath()));

        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
        }

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            url.Append("&k=" + Server.UrlEncode(_Keyword));
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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/ShipLocalData-TW".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
    }

    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            //Params
            string _sDate = filter_sDate.Text;
            string _eDate = filter_eDate.Text;
            string _Keyword = filter_Keyword.Text;

            //url string
            StringBuilder url = new StringBuilder();

            //固定條件:Page
            url.Append("{0}?page={1}".FormatThis(FuncPath(), Req_PageIdx));

            //[查詢條件] - Date
            if (!string.IsNullOrWhiteSpace(_sDate))
            {
                url.Append("&sDate=" + Server.UrlEncode(_sDate));
            }
            if (!string.IsNullOrWhiteSpace(_eDate))
            {
                url.Append("&eDate=" + Server.UrlEncode(_eDate));
            }

            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(_Keyword))
            {
                url.Append("&k=" + Server.UrlEncode(_Keyword));
            }


            return url.ToString();
        }
        set
        {
            this._thisPage = value;
        }
    }
    private string _thisPage;

    #endregion


    #region -- 傳遞參數 --

    /// <summary>
    /// 取得傳遞參數 - PageIdx(目前索引頁)
    /// </summary>
    public int Req_PageIdx
    {
        get
        {
            int data = Request.QueryString["Page"] == null ? 1 : Convert.ToInt32(Request.QueryString["Page"]);
            return data;
        }
        set
        {
            this._Req_PageIdx = value;
        }
    }
    private int _Req_PageIdx;


    /// <summary>
    /// 取得傳遞參數 - sDate
    /// 預設月初
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.ToShortDateString().ToDateString("yyyy/MM/01");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_sDate = value;
        }
    }
    private string _Req_sDate;


    /// <summary>
    /// 取得傳遞參數 - eDate
    /// 預設月底
    /// </summary>
    public string Req_eDate
    {
        get
        {
            String _data = Request.QueryString["eDate"];
            DateTime nextMonth = Convert.ToDateTime(DateTime.Now.AddMonths(1).ToShortDateString().ToDateString("yyyy/MM/01"));
            string dt = nextMonth.AddDays(-1).ToShortDateString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_eDate = value;
        }
    }
    private string _Req_eDate;

    /// <summary>
    /// 取得傳遞參數 - Keyword
    /// </summary>
    public string Req_Keyword
    {
        get
        {
            String _data = Request.QueryString["k"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Keyword = value;
        }
    }
    private string _Req_Keyword;

    #endregion


    #region -- 上傳參數 --
    /// <summary>
    /// 限制上傳的副檔名
    /// </summary>
    private string _FileExtLimit;
    public string FileExtLimit
    {
        get
        {
            return "xls|xlsx";
        }
        set
        {
            this._FileExtLimit = value;
        }
    }

    /// <summary>
    /// 限制上傳的檔案大小(1MB = 1024000), 10MB
    /// </summary>
    private int _FileSizeLimit;
    public int FileSizeLimit
    {
        get
        {
            return 10240000;
        }
        set
        {
            this._FileSizeLimit = value;
        }
    }

    /// <summary>
    /// 限制上傳檔案數
    /// </summary>
    private int _FileCountLimit;
    public int FileCountLimit
    {
        get
        {
            return 1;
        }
        set
        {
            this._FileCountLimit = value;
        }
    }

    /// <summary>
    /// 上傳目錄
    /// </summary>
    private string _UploadFolder;
    public string UploadFolder
    {
        get
        {
            return "{0}ShipLocalTW/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
        }
        set
        {
            this._UploadFolder = value;
        }
    }

    /// <summary>
    /// 暫存參數
    /// </summary>
    public class IOTempParam
    {
        /// <summary>
        /// [參數] - 檔名
        /// </summary>
        private string _Param_FileName;
        public string Param_FileName
        {
            get { return this._Param_FileName; }
            set { this._Param_FileName = value; }
        }

        /// <summary>
        /// [參數] -原始檔名
        /// </summary>
        private string _Param_OrgFileName;
        public string Param_OrgFileName
        {
            get { return this._Param_OrgFileName; }
            set { this._Param_OrgFileName = value; }
        }


        private HttpPostedFile _Param_hpf;
        public HttpPostedFile Param_hpf
        {
            get { return this._Param_hpf; }
            set { this._Param_hpf = value; }
        }

        /// <summary>
        /// 設定參數值
        /// </summary>
        /// <param name="Param_FileName">系統檔名</param>
        /// <param name="Param_OrgFileName">原始檔名</param>
        /// <param name="Param_hpf">上傳檔案</param>
        /// <param name="Param_FileKind">檔案類別</param>
        public IOTempParam(string Param_FileName, string Param_OrgFileName, HttpPostedFile Param_hpf)
        {
            this._Param_FileName = Param_FileName;
            this._Param_OrgFileName = Param_OrgFileName;
            this._Param_hpf = Param_hpf;
        }

    }
    #endregion
}
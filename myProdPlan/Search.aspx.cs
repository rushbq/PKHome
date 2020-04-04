using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu1000Data.Controllers;
using Menu1000Data.Models;
using PKLib_Method.Methods;

public partial class myShipmentData_Search_TW : SecurityCheck
{
    //設定FTP連線參數
    private FtpMethod _ftp = new FtpMethod(
        fn_Param.ftp_Username, fn_Param.ftp_Password, fn_Param.ftp_ServerUrl);

    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //[權限判斷] Start
                #region --權限--

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "1601");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End


                //建立年份選單
                CreateMenu_Year(ddl_Year);

                //建立分類選單
                GetProdClassMenu();

                //建立一階選單
                CreateMenu(ddl_Lv1, "1", "", "");

                //Get Data
                if (Req_doSearch.Equals("1"))
                {
                    ph_Tip.Visible = false;
                    LookupDataList(Req_PageIdx);
                }

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
        //int RecordsPerPage = 10;    //每頁筆數
        //int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        //int TotalRow = 0;   //總筆數
        //int DataCnt = 0;
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu1000Repository _data = new Menu1000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            #region >> 條件篩選 <<

            //[取得/檢查參數] - Class ID
            if (!string.IsNullOrWhiteSpace(Req_ClassID))
            {
                search.Add("Class_ID", Req_ClassID);
                PageParam.Add("cid=" + Server.UrlEncode(Req_ClassID));
                ddl_Class.SelectedValue = Req_ClassID;
            }
            //[取得/檢查參數] - Menu Lv1
            if (!string.IsNullOrWhiteSpace(Req_MenuLv1))
            {
                search.Add("Menu_Lv1", Req_MenuLv1);
                PageParam.Add("lv1=" + Server.UrlEncode(Req_MenuLv1));
                ddl_Lv1.SelectedValue = Req_MenuLv1;
            }

            //判斷一階是否有選擇
            if (!string.IsNullOrWhiteSpace(Req_MenuLv1))
            {
                //呼叫二階
                CreateMenu(ddl_Lv2, "2", "", Req_MenuLv1);
            }
            //[取得/檢查參數] - Menu Lv2
            if (!string.IsNullOrWhiteSpace(Req_MenuLv2))
            {
                search.Add("Menu_Lv2", Req_MenuLv2);
                PageParam.Add("lv2=" + Server.UrlEncode(Req_MenuLv2));
                ddl_Lv2.SelectedValue = Req_MenuLv2;
            }

            //固定條件
            ddl_Year.SelectedValue = Req_Year;

            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetProdPlanData(search, Convert.ToInt16(Req_Year), out ErrMsg);

            ////----- 資料整理:取得總筆數 -----
            //TotalRow = DataCnt;

            ////----- 資料整理:頁數判斷 -----
            //if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
            //{
            //    StartRow = 0;
            //    pageIndex = 1;
            //}

            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
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
                //string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                //    , FuncPath(), PageParam, false, true);
                //lt_Pager.Text = getPager;

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
        /* 自建新品才有刪除功能 */
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_ListDataID")).Value;
        string Get_OldFile = ((HiddenField)e.Item.FindControl("hf_ListOldFile")).Value;

        /* 刪除資料 */
        Menu1000Repository _data = new Menu1000Repository();

        if (false == _data.Delete_PostalData(Get_DataID))
        {
            CustomExtension.AlertMsg("刪除失敗", "");
            return;
        }
        else
        {
            //刪除檔案
            if (!string.IsNullOrWhiteSpace(Get_OldFile))
            {
                _ftp.FTP_DelFile(UploadFolder, Get_OldFile);
            }

            //導向本頁
            Response.Redirect(filterUrl());
        }
    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取得資料
                string _prodImg = DataBinder.Eval(dataItem.DataItem, "ListPic").ToString();
                string _prodDesc = DataBinder.Eval(dataItem.DataItem, "ProdDesc").ToString();
                string _prodFea = DataBinder.Eval(dataItem.DataItem, "ProdFeature").ToString();
                string _fromData = DataBinder.Eval(dataItem.DataItem, "FromData").ToString();

                //Image
                PlaceHolder ph_ProdImg = (PlaceHolder)e.Item.FindControl("ph_ProdImg");
                ph_ProdImg.Visible = !string.IsNullOrWhiteSpace(_prodImg);

                //Desc
                PlaceHolder ph_Desc = (PlaceHolder)e.Item.FindControl("ph_Desc");
                ph_Desc.Visible = !string.IsNullOrWhiteSpace(_prodDesc) || !string.IsNullOrWhiteSpace(_prodFea);


                //buttons
                PlaceHolder ph_SysData = (PlaceHolder)e.Item.FindControl("ph_SysData");
                PlaceHolder ph_NewData = (PlaceHolder)e.Item.FindControl("ph_NewData");

                ph_SysData.Visible = _fromData.Equals("0_SYS");
                ph_NewData.Visible = _fromData.Equals("1_NEW");

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
    /// [按鈕] - 匯出
    /// </summary>
    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {
        ////----- 宣告:資料參數 -----
        //Menu1000Repository _data = new Menu1000Repository();
        //Dictionary<string, string> search = new Dictionary<string, string>();

        ////----- 原始資料:條件篩選 -----
        //#region >> 條件篩選 <<

        ////[查詢條件] - Keyword
        //if (!string.IsNullOrWhiteSpace(Req_Keyword))
        //{
        //    search.Add("Keyword", Req_Keyword);
        //}
        ////[取得/檢查參數] - Date
        //if (!string.IsNullOrWhiteSpace(Req_sDate))
        //{
        //    search.Add("sDate", Req_sDate);
        //}
        //if (!string.IsNullOrWhiteSpace(Req_eDate))
        //{
        //    search.Add("eDate", Req_eDate);
        //}
        //#endregion

        ////----- 原始資料:取得所有資料 -----
        //var query = _data.GetProdPlanData(search, "TW", out ErrMsg)
        //    .Select(fld => new
        //    {
        //        報關日期 = fld.CustomsDate,
        //        贖單日期 = fld.RedeemDate,
        //        廠商 = fld.SupName + "(" + fld.SupID + ")",
        //        贖單單號 = fld.Redeem_FID + "-" + fld.Redeem_SID,
        //        件數 = fld.QtyMark,
        //        報關金額 = fld.CustomsPrice,
        //        採購金額幣別 = fld.Currency,
        //        採購金額 = fld.PurPrice,
        //        匯率 = fld.Tax,
        //        進口報單號碼 = fld.CustomsNo,
        //        報關費_NTD未稅 = fld.Cost_Customs,
        //        LocalCharge_NTD未稅 = fld.Cost_LocalCharge,
        //        營業稅 = fld.Cost_LocalBusiness,
        //        進口稅 = fld.Cost_Imports,
        //        貿推費 = fld.Tax,
        //        進口營業稅 = fld.Cost_ImportsBusiness,
        //        代墊款 = fld.Cnt_CostFee,
        //        總計 = fld.Cnt_Total,
        //        商港費 = fld.Cost_Service,
        //        進口費用百分比 = fld.Cnt_CostPercent,
        //        採購單張數 = fld.PurCnt,
        //        採購單Item數 = fld.PurItemCnt,
        //        卡車 = fld.Cost_Truck,
        //        備註 = fld.Remark
        //    });


        ////Check null
        //if (query.Count() == 0)
        //{
        //    CustomExtension.AlertMsg("查無資料", filterUrl());
        //    return;
        //}


        ////將IQueryable轉成DataTable
        //DataTable myDT = CustomExtension.LINQToDataTable(query);


        ////release
        //query = null;

        ////匯出Excel
        //CustomExtension.ExportExcel(
        //    myDT
        //    , "{0}-{1}.xlsx".FormatThis("出貨明細表-台灣進口", DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
        //    , false);
    }

    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _year = ddl_Year.SelectedValue;
        string _cid = ddl_Class.SelectedValue;
        string _lv1 = ddl_Lv1.SelectedValue;
        string _lv2 = ddl_Lv2.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page
        url.Append("{0}?do=1&page=1".FormatThis(FuncPath()));

        //[查詢條件] - Year
        if (!string.IsNullOrWhiteSpace(_year))
        {
            url.Append("&year=" + Server.UrlEncode(_year));
        }

        //[查詢條件] - ClassID
        if (!string.IsNullOrWhiteSpace(_cid))
        {
            url.Append("&cid=" + Server.UrlEncode(_cid));
        }

        //[查詢條件] - Menu Lv1
        if (!string.IsNullOrWhiteSpace(_lv1))
        {
            url.Append("&lv1=" + Server.UrlEncode(_lv1));
        }

        //[查詢條件] -  Menu Lv2
        if (!string.IsNullOrWhiteSpace(_lv2))
        {
            url.Append("&lv2=" + Server.UrlEncode(_lv2));
        }

        return url.ToString();
    }


    /// <summary>
    /// 建立選單-產品銷售類別
    /// </summary>
    private void GetProdClassMenu()
    {
        //控制項-銷售類別
        ddl_Class.Items.Clear();
        ddl_Class.Items.Insert(0, new ListItem("- 所有類別 -", "-1"));

        //----- 宣告:資料參數 -----
        Menu1000Repository _data = new Menu1000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        try
        {
            //----- 原始資料:取得資料 -----
            var data = _data.GetProdClassList(Req_Lang.ToUpper(), search, out ErrMsg);

            foreach (var item in data)
            {
                ddl_Class.Items.Add(new ListItem(item.ID + " - " + item.Label, item.ID.ToString()));
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
    }


    /// <summary>
    /// 建立選單-分類階層選單
    /// </summary>
    /// <param name="drp">控制項</param>
    /// <param name="_lv">階層</param>
    /// <param name="_cid">銷售類別</param>
    /// <param name="_pid">上層編號</param>
    private void CreateMenu(DropDownList drp, string _lv, string _cid, string _pid)
    {
        //清空選項
        drp.Items.Clear();

        //取得資料
        var data = GetMenuList(_lv, _cid, _pid);

        //建立子項
        foreach (var item in data)
        {
            drp.Items.Add(new ListItem(item.Label, item.ID.ToString()));
        }

        //建立root item
        drp.Items.Insert(0, new ListItem("- 所有類別 -", "-1"));
    }


    /// <summary>
    /// 資料回傳-取得分類階層選單
    /// </summary>
    /// <param name="_lv">階層</param>
    /// <param name="_cid">銷售類別</param>
    /// <param name="_pid">上層編號</param>
    /// <returns></returns>
    private IQueryable<CateItem> GetMenuList(string _lv, string _cid, string _pid)
    {
        //----- 宣告:資料參數 -----
        Menu1000Repository _data = new Menu1000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        //[取得/檢查參數] - Level
        if (!string.IsNullOrWhiteSpace(_lv))
        {
            search.Add("Level", _lv);
        }

        //[取得/檢查參數] - ClassID
        if (!string.IsNullOrWhiteSpace(_cid))
        {
            search.Add("ClassID", _cid);
        }

        //[取得/檢查參數] - ParentID
        if (!string.IsNullOrWhiteSpace(_pid))
        {
            search.Add("ParentID", _pid);
        }

        //----- 原始資料:取得所有資料 -----
        var results = _data.GetMenuList("TW", search, out ErrMsg);

        //release
        _data = null;

        return results;

    }


    /// <summary>
    /// 銷售類別onchange
    /// </summary>
    protected void ddl_Class_SelectedIndexChanged(object sender, EventArgs e)
    {
        //執行查詢
        Response.Redirect(filterUrl(), false);
    }

    /// <summary>
    /// 一階onchange, 帶出二階
    /// </summary>
    protected void ddl_Lv1_SelectedIndexChanged(object sender, EventArgs e)
    {
        //呼叫二階
        //CreateMenu(ddl_Lv2, "2", "", ddl_Lv1.SelectedValue);
        //執行查詢
        Response.Redirect(filterUrl(), false);
    }


    protected void ddl_Year_SelectedIndexChanged(object sender, EventArgs e)
    {
        Response.Redirect(filterUrl(), false);
    }

    /// <summary>
    /// 建立選單-年份
    /// </summary>
    /// <param name="item"></param>
    protected void CreateMenu_Year(DropDownList item)
    {
        int currYear = DateTime.Now.Year;
        int prevYear = currYear - 2;
        int nextYear = currYear - 1;

        item.Items.Clear();
        for (int itemY = prevYear; itemY <= nextYear; itemY++)
        {
            item.Items.Add(new ListItem(itemY.ToString(), itemY.ToString()));
        }

        //預設值
        item.SelectedValue = nextYear.ToString();
    }


    /// <summary>
    /// 表格背景變更
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    public string statusColor(string val)
    {
        if (val.ToUpper().Equals("1_NEW"))
        {
            return "yellow-bg lighten-3";
        }

        return "";
    }
    #endregion


    #region -- 資料編輯 --
    /// <summary>
    /// 備註填寫
    /// </summary>
    protected void btn_SaveRemark_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu1000Repository _data = new Menu1000Repository();
        try
        {
            //----- 設定:資料欄位 -----
            string _ModelNo = hf_ModelNo.Value;
            string _Year = hf_Year.Value;
            string _Remark = formSysRemark.Text.Trim();

            var data = new ProdPlanRptItem
            {
                ModelNo = _ModelNo,
                SaleYear = _Year,
                Remark = _Remark
            };

            //----- 方法:更新資料 -----
            if (!_data.Check_ProdPlanRemarkData(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("備註填寫失敗", "");
                return;
            }
            else
            {
                //導向本頁
                Response.Redirect(filterUrl());
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

    /// <summary>
    /// 新品建立
    /// </summary>
    protected void btn_SaveProd_Click(object sender, EventArgs e)
    {

        //Check Null
        string errTxt = "";

        #region ** 基本欄位判斷 **

        if (string.IsNullOrWhiteSpace(form_ModelNo.Text))
        {
            errTxt += "品號空白\\n";
        }
        if (string.IsNullOrWhiteSpace(form_ModelName.Text))
        {
            errTxt += "品名空白\\n";
        }

        #endregion


        #region ** 檔案上傳判斷 **

        //宣告
        List<IOTempParam> ITempList = new List<IOTempParam>();
        Random rnd = new Random();

        int GetFileCnt = 0;

        //取得上傳檔案集合
        HttpFileCollection hfc = Request.Files;

        //--- 限制上傳數量 ---
        for (int idx = 0; idx <= hfc.Count - 1; idx++)
        {
            //取得個別檔案
            HttpPostedFile hpf = hfc[idx];

            if (hpf.ContentLength > 0)
            {
                GetFileCnt++;
            }
        }
        if (GetFileCnt > FileCountLimit)
        {
            //[提示]
            errTxt += "單次上傳檔案數超出限制, 每次上傳僅限 {0} 個檔案.\\n".FormatThis(FileCountLimit);
        }


        //--- 檔案檢查 ---
        for (int idx = 0; idx <= hfc.Count - 1; idx++)
        {
            //取得個別檔案
            HttpPostedFile hpf = hfc[idx];

            if (hpf.ContentLength > FileSizeLimit)
            {
                //[提示]
                errTxt += "大小超出限制, 每個檔案限制為 {0} MB\\n".FormatThis(FileSizeLimit / 1024000);
            }

            if (hpf.ContentLength > 0)
            {
                //取得原始檔名
                string OrgFileName = Path.GetFileName(hpf.FileName);
                //取得副檔名
                string FileExt = Path.GetExtension(OrgFileName).ToLower();
                if (false == CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //[提示]
                    errTxt += "「{0}」副檔名不符規定, 僅可上傳「{1}」\\n".FormatThis(OrgFileName, FileExtLimit.Replace("|", ", "));
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
                string OrgFileName = Path.GetFileName(hpf.FileName);
                //取得副檔名
                string FileExt = Path.GetExtension(OrgFileName).ToLower();

                //設定檔名, 重新命名
                string myFullFile = String.Format(@"{0:yyMMddHHmmssfff}{1}{2}"
                    , DateTime.Now
                    , rnd.Next(0, 99)
                    , FileExt);


                //判斷副檔名, 未符合規格的檔案不上傳
                if (CustomExtension.CheckStrWord(FileExt, FileExtLimit, "|", 1))
                {
                    //設定暫存-檔案
                    ITempList.Add(new IOTempParam(myFullFile, OrgFileName, hpf));
                }
            }
        }

        #endregion

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        #region ** 儲存檔案 **

        /* 注意檔案路徑及資料夾(站台資料夾+功能資料夾) */
        if (ITempList.Count > 0)
        {
            int errCnt = 0;
            string ftpFolder = UploadFolder; //ftp資料夾

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
                CustomExtension.AlertMsg("檔案上傳失敗, 請重新整理後再上傳!", "");
                return;
            }

            //刪除舊檔案
            if (!string.IsNullOrWhiteSpace(hf_OldFile.Value))
            {
                _ftp.FTP_DelFile(ftpFolder, hf_OldFile.Value);
            }
        }

        #endregion

        /* 執行新增/更新 */
        if (string.IsNullOrEmpty(hf_DataID.Value))
        {
            Add_Data(ITempList);
        }
        else
        {
            Edit_Data(ITempList);
        }
    }

    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data(List<IOTempParam> fileList)
    {
        //----- 宣告:資料參數 -----
        Menu1000Repository _data = new Menu1000Repository();
        try
        {
            //----- 設定:資料欄位 -----
            string _Class_ID = hf_ClassID.Value;
            string _Class_Lv1 = hf_MenuLv1ID.Value;
            string _Class_Lv2 = hf_MenuLv2ID.Value;
            string _ModelNo = form_ModelNo.Text.Trim();
            string _ModelName = form_ModelName.Text.Trim();
            string _Pic = fileList.Count > 0 ? fileList[0].Param_FileName : "";
            string _ShipFrom = form_ShipFrom.Text.Trim();
            string _Supplier = form_Supplier.Text.Trim();
            string _TargetMonth = form_TargetMonth.Text.Trim();
            string _Remark = form_Remark.Text.Trim();

            var data = new ProdPlanDataItem
            {
                Class_ID = Convert.ToInt16(_Class_ID),
                Class_Lv1 = Convert.ToInt16(_Class_Lv1),
                Class_Lv2 = Convert.ToInt16(_Class_Lv2),
                ModelNo = _ModelNo,
                ModelName = _ModelName,
                Pic = _Pic,
                ShipFrom = _ShipFrom,
                Supplier = _Supplier,
                TargetMonth = _TargetMonth,
                Remark = _Remark,
                Create_Who = fn_Param.CurrentUser
            };

            //----- 方法:新增資料 -----
            if (!_data.Create_PostalData(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }
            else
            {
                Response.Redirect(filterUrl());
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


    /// <summary>
    /// 資料修改
    /// </summary>
    private void Edit_Data(List<IOTempParam> fileList)
    {
        //----- 宣告:資料參數 -----
        Menu1000Repository _data = new Menu1000Repository();
        try
        {
            //----- 設定:資料欄位 -----
            string _ModelNo = form_ModelNo.Text.Trim();
            string _ModelName = form_ModelName.Text.Trim();
            string _Pic = fileList.Count > 0 ? fileList[0].Param_FileName : hf_OldFile.Value;
            string _ShipFrom = form_ShipFrom.Text.Trim();
            string _Supplier = form_Supplier.Text.Trim();
            string _TargetMonth = form_TargetMonth.Text.Trim();
            string _Remark = form_Remark.Text.Trim();
            string _dataID = hf_DataID.Value;

            var data = new ProdPlanDataItem
            {
                Data_ID = new Guid(_dataID),
                ModelNo = _ModelNo,
                ModelName = _ModelName,
                Pic = _Pic,
                ShipFrom = _ShipFrom,
                Supplier = _Supplier,
                TargetMonth = _TargetMonth,
                Remark = _Remark,
                Update_Who = fn_Param.CurrentUser
            };

            //----- 方法:更新資料 -----
            if (!_data.Update_PostalData(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("更新失敗", "");
                return;
            }
            else
            {
                //導向本頁
                Response.Redirect(filterUrl());
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
        return "{0}{1}/{2}/ProductPlan".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
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
    /// 取得傳遞參數 - doSearch
    /// </summary>
    public string Req_doSearch
    {
        get
        {
            String _data = Request.QueryString["do"];
            return string.IsNullOrWhiteSpace(_data) ? "0" : _data;
        }
        set
        {
            _Req_doSearch = value;
        }
    }
    private string _Req_doSearch;


    /// <summary>
    /// 取得傳遞參數 - Year
    /// </summary>
    public string Req_Year
    {
        get
        {
            String _data = Request.QueryString["year"];
            string _defValue = (DateTime.Now.Year - 1).ToString(); //預設值
            return string.IsNullOrWhiteSpace(_data) ? _defValue : _data;
        }
        set
        {
            _Req_Year = value;
        }
    }
    private string _Req_Year;


    /// <summary>
    /// 取得傳遞參數 - ClassID
    /// </summary>
    public string Req_ClassID
    {
        get
        {
            String _data = Request.QueryString["cid"];
            return string.IsNullOrWhiteSpace(_data) || (_data.Equals("-1")) ? "" : _data;
        }
        set
        {
            _Req_ClassID = value;
        }
    }
    private string _Req_ClassID;


    /// <summary>
    /// 取得傳遞參數 - MenuLv1
    /// </summary>
    public string Req_MenuLv1
    {
        get
        {
            String _data = Request.QueryString["lv1"];
            return string.IsNullOrWhiteSpace(_data) || (_data.Equals("-1")) ? "" : _data;
        }
        set
        {
            _Req_MenuLv1 = value;
        }
    }
    private string _Req_MenuLv1;


    /// <summary>
    /// 取得傳遞參數 - MenuLv2
    /// </summary>
    public string Req_MenuLv2
    {
        get
        {
            String _data = Request.QueryString["lv2"];
            return string.IsNullOrWhiteSpace(_data) || (_data.Equals("-1")) ? "" : _data;
        }
        set
        {
            _Req_MenuLv2 = value;
        }
    }
    private string _Req_MenuLv2;

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
            return "jpg|png";
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
            return "{0}ProductPlan/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
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
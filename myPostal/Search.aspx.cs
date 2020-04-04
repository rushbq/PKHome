using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu2000Data.Controllers;
using Menu2000Data.Models;
using PKLib_Method.Methods;

public partial class myPostal_Search : SecurityCheck
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

                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2405");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #endregion
                //[權限判斷] End


                //[資料顯示] 填入預設值
                tb_PostDate.Text = DateTime.Now.ToShortDateString();

                //[資料顯示] 取得郵式清單
                GetClassMenu();

                //[資料判斷] 取得關閉狀態               
                string today = DateTime.Today.ToShortDateString();
                string st = CheckStatus(today);
                ph_Close_No.Visible = st.Equals("N");
                ph_Close_Yes.Visible = st.Equals("Y");

                //[資料顯示] 資料列表
                LookupDataList();

                //[資料判斷] 是否為編輯模式
                if (Req_Edit.Equals("Y"))
                {
                    //顯示編輯資料
                    LookupEditData();
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
    /// 取得郵式清單
    /// </summary>
    private void GetClassMenu()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        try
        {
            //取得資料
            var data = _data.GetPostalClass(out ErrMsg);

            //設定繫結-AddForm
            ddl_PostType.DataSource = data;
            ddl_PostType.DataTextField = "Label";
            ddl_PostType.DataValueField = "ID";
            ddl_PostType.DataBind();

            //新增root item
            ddl_PostType.Items.Insert(0, new ListItem("請選擇郵式", ""));


            //設定繫結-SearchForm
            filter_PostType.DataSource = data;
            filter_PostType.DataTextField = "Label";
            filter_PostType.DataValueField = "ID";
            filter_PostType.DataBind();

            //新增root item
            filter_PostType.Items.Insert(0, new ListItem("所有郵式", ""));
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
    /// 取得關閉狀態
    /// </summary>
    private string CheckStatus(string targetDate)
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        try
        {
            return _data.GetPostalStatus(targetDate, out ErrMsg);
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
    /// 取得資料
    /// </summary>
    private void LookupDataList()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            #region >> 條件篩選 <<

            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(Req_Keyword))
            {
                search.Add("Keyword", Req_Keyword);
                filter_Keyword.Text = Req_Keyword;
            }
            //[取得/檢查參數] - Date
            if (!string.IsNullOrWhiteSpace(Req_sDate))
            {
                search.Add("sDate", Req_sDate);
                filter_sDate.Text = Req_sDate;
            }
            if (!string.IsNullOrWhiteSpace(Req_eDate))
            {
                search.Add("eDate", Req_eDate);
                filter_eDate.Text = Req_eDate;
            }
            if (!string.IsNullOrWhiteSpace(Req_Type))
            {
                search.Add("PostType", Req_Type);
                filter_PostType.SelectedValue = Req_Type;
            }
            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetPostalData(search, out ErrMsg);

            //----- 資料整理:繫結 ----- 
            lvDataList.DataSource = query;
            lvDataList.DataBind();

            query = null;
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //Release
            _data = null;
        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();

        try
        {
            //----- 方法:刪除資料 -----
            string url = filterUrl();
            if (false == _data.Delete_PostalData(Get_DataID))
            {
                CustomExtension.AlertMsg("刪除失敗", url);
                return;
            }
            else
            {
                //導向本頁
                Response.Redirect(url);
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


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                ////判斷是否已加入排程,顯示編輯鈕
                //string _onTask = DataBinder.Eval(dataItem.DataItem, "IsOnTask").ToString();
                //PlaceHolder ph_Edit = (PlaceHolder)e.Item.FindControl("ph_Edit");
                //ph_Edit.Visible = _onTask.Equals("N");


            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    /// <summary>
    /// 取得編輯資料
    /// </summary>
    private void LookupEditData()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            search.Add("DataID", Req_ID);

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetPostalData(search, out ErrMsg).FirstOrDefault();

            //----- 資料整理:繫結 ----- 
            if (query == null)
            {
                CustomExtension.AlertMsg("無法載入資料", thisPage);
                return;
            }

            //填入編輯資料
            hf_DataID.Value = query.Data_ID.ToString();
            lt_SeqNo.Text = query.SeqNo.ToString();
            tb_PostDate.Text = query.PostDate;
            filter_Emp.Text = query.Post_WhoName;
            val_Emp.Text = query.PostWho;
            ddl_PostType.SelectedValue = query.PostType.ToString();
            tb_PackageWeight.Text = query.PackageWeight.ToString();
            tb_PostPrice.Text = query.PostPrice.ToString();
            tb_PostNo.Text = query.PostNo;
            tb_ToWho.Text = query.ToWho;
            tb_ToAddr.Text = query.ToAddr;
            tb_PostDesc.Text = query.PostDesc;


            //release
            query = null;
        }
        catch (Exception)
        {

            throw;
        }
        finally
        {
            //Release
            _data = null;
        }
    }

    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 存檔
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btn_doSave_Click(object sender, EventArgs e)
    {
        //Check Null
        string errTxt = "";

        if (string.IsNullOrWhiteSpace(val_Emp.Text))
        {
            errTxt += "寄件人空白\\n";
        }
        if (string.IsNullOrWhiteSpace(ddl_PostType.SelectedValue))
        {
            errTxt += "郵式空白\\n";
        }
        if (string.IsNullOrWhiteSpace(tb_ToWho.Text))
        {
            errTxt += "收件人空白\\n";
        }
        if (string.IsNullOrWhiteSpace(tb_ToAddr.Text))
        {
            errTxt += "收件地址空白\\n";
        }
        if (string.IsNullOrWhiteSpace(tb_PostDesc.Text))
        {
            errTxt += "內容空白\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        /* 執行新增/更新 */
        if (string.IsNullOrEmpty(hf_DataID.Value))
        {
            Add_Data();
        }
        else
        {
            Edit_Data();
        }
    }


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
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
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
        if (!string.IsNullOrWhiteSpace(Req_Type))
        {
            search.Add("PostType", Req_Type);
        }
        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetPostalData(search, out ErrMsg)
            .Select(fld => new
            {
                日期 = fld.PostDate,
                公司名稱 = fld.ToWho,
                內容 = fld.PostDesc,
                郵式 = fld.PostTypeName,
                部門 = fld.Post_DeptName,
                金額 = fld.PostPrice
            })
            .OrderBy(fld => fld.日期);

        //Check null
        if (query.Count() == 0)
        {
            CustomExtension.AlertMsg("查無資料", filterUrl());
            return;
        }

        //取得指定欄位合計
        double Total = query.Sum(fld => fld.金額);


        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);

        if (myDT.Rows.Count > 0)
        {
            //在footer新增一列
            DataRow dtRow = myDT.NewRow();
            dtRow[4] = "總計:";
            dtRow[5] = Total;

            myDT.Rows.Add(dtRow);
        }

        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "{0}-{1}.xlsx".FormatThis("郵寄明細表", DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }

    #endregion


    #region -- 資料編輯 --
    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        try
        {
            //----- 設定:資料欄位 -----
            string _postDate = tb_PostDate.Text.ToDateString("yyyy/MM/dd");
            string _postWho = val_Emp.Text;
            string _postType = ddl_PostType.SelectedValue;
            string _weight = tb_PackageWeight.Text;
            string _toWho = tb_ToWho.Text.Trim();
            string _toAddr = tb_ToAddr.Text.Trim();
            string _postDesc = tb_PostDesc.Text.Trim();
            string _postNo = tb_PostNo.Text;
            string _price = tb_PostPrice.Text;

            var data = new PostalItem
            {
                PostDate = _postDate,
                PostWho = _postWho,
                PostType = Convert.ToInt16(_postType),
                ToWho = _toWho,
                ToAddr = _toAddr,
                PackageWeight = string.IsNullOrWhiteSpace(_weight) ? 0 : Convert.ToDouble(_weight),
                PostDesc = _postDesc,
                PostNo = _postNo,
                PostPrice = string.IsNullOrWhiteSpace(_price) ? 0 : Convert.ToDouble(_price),
                Create_Who = fn_Param.CurrentUser
            };

            //----- 方法:判斷截止 -----
            //string st = CheckStatus("");
            //if (st.Equals("Y"))
            //{
            //    CustomExtension.AlertMsg("今日登記已截止,不可新增資料.", thisPage);
            //    return;
            //}

            //----- 方法:新增資料 -----
            if (!_data.Create_PostalData(data, true, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }
            else
            {
                Response.Redirect(thisPage);
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
    private void Edit_Data()
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        try
        {
            //----- 設定:資料欄位 -----
            string _postDate = tb_PostDate.Text.ToDateString("yyyy/MM/dd");
            string _postWho = val_Emp.Text;
            string _postType = ddl_PostType.SelectedValue;
            string _weight = tb_PackageWeight.Text;
            string _toWho = tb_ToWho.Text.Trim();
            string _toAddr = tb_ToAddr.Text.Trim();
            string _postDesc = tb_PostDesc.Text.Trim();
            string _postNo = tb_PostNo.Text;
            string _price = tb_PostPrice.Text;
            string _dataID = hf_DataID.Value;

            var data = new PostalItem
            {
                Data_ID = new Guid(_dataID),
                PostDate = _postDate,
                PostWho = _postWho,
                PostType = Convert.ToInt16(_postType),
                ToWho = _toWho,
                ToAddr = _toAddr,
                PackageWeight = string.IsNullOrWhiteSpace(_weight) ? 0 : Convert.ToDouble(_weight),
                PostDesc = _postDesc,
                PostNo = _postNo,
                PostPrice = string.IsNullOrWhiteSpace(_price) ? 0 : Convert.ToDouble(_price),
                Update_Who = fn_Param.CurrentUser
            };

            //----- 方法:更新資料 -----
            if (!_data.Update_PostalData(data, true, out ErrMsg))
            {
                CustomExtension.AlertMsg("更新失敗", "");
                return;
            }
            else
            {
                //導向本頁
                Response.Redirect(thisPage);
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
    /// 設為截止狀態
    /// </summary>
    protected void btn_doClose_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        Menu2000Repository _data = new Menu2000Repository();
        string _today = DateTime.Today.ToString().ToDateString("yyyy/MM/dd");

        try
        {
            //----- 方法:更新資料 -----
            if (false == _data.Update_Postal_DayCheck(_today, "Y", out ErrMsg))
            {
                CustomExtension.AlertMsg("設定失敗", "");
            }
            else
            {
                CustomExtension.AlertMsg("設定成功", thisPage);
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


    #region -- 附加功能 --

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
        string _Type = filter_PostType.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件
        url.Append("{0}?list=Y".FormatThis(thisPage));

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
        //[查詢條件] - Type
        if (!string.IsNullOrWhiteSpace(_Type))
        {
            url.Append("&type=" + Server.UrlEncode(_Type));
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
        return "{0}{1}/{2}/PostalAdmin".FormatThis(
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
            return "{0}".FormatThis(FuncPath());
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
    /// 取得傳遞參數 - sDate
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.ToShortDateString().ToDateString("yyyy/MM/dd");
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
    /// </summary>
    public string Req_eDate
    {
        get
        {
            String _data = Request.QueryString["eDate"];
            string dt = DateTime.Now.ToShortDateString().ToDateString("yyyy/MM/dd");
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


    /// <summary>
    /// 取得傳遞參數 - Type
    /// </summary>
    public string Req_Type
    {
        get
        {
            String _data = Request.QueryString["type"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "5", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Type = value;
        }
    }
    private string _Req_Type;


    /// <summary>
    /// 是否進入編輯模式(Y/N)
    /// </summary>
    public string Req_Edit
    {
        get
        {
            String _data = Request.QueryString["edit"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "1", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Edit = value;
        }
    }
    private string _Req_Edit;

    /// <summary>
    /// 編輯資料ID
    /// </summary>
    public string Req_ID
    {
        get
        {
            String _data = Request.QueryString["id"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "38", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_ID = value;
        }
    }
    private string _Req_ID;

    #endregion

}
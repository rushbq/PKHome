using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using MenuHomeData.Controllers;
using PKLib_Method.Methods;

public partial class myShipping_SendList : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                #region --權限--
                //[權限判斷] Start
                /* 
                 * 使用公司別代號，判斷對應的MENU ID
                 */
                bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3701"); ;

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[權限判斷] End
                #endregion


                #region --Request參數--
                //[取得/檢查參數] - 發貨日
                if (!string.IsNullOrWhiteSpace(Req_sDate_Ship))
                {
                    this.filter_sDate_Ship.Text = Req_sDate_Ship;
                }
                //[取得/檢查參數] - 發貨日
                if (!string.IsNullOrWhiteSpace(Req_eDate_Ship))
                {
                    this.filter_eDate_Ship.Text = Req_eDate_Ship;
                }

                //[取得/檢查參數] - 物流途徑
                if (!string.IsNullOrWhiteSpace(Req_Way))
                {
                    this.filter_ShipWay.SelectedIndex = this.filter_ShipWay.Items.IndexOf(this.filter_ShipWay.Items.FindByValue(Req_Way));
                }

                //[取得/檢查參數] - 出貨地
                if (!string.IsNullOrWhiteSpace(Req_ShipFrom))
                {
                    this.filter_ShipFrom.SelectedIndex = this.filter_ShipFrom.Items.IndexOf(this.filter_ShipFrom.Items.FindByValue(Req_ShipFrom));
                }

                #endregion


                //*** 設定母版的Menu ***
                Literal menu = (Literal)Page.Master.FindControl("lt_headerMenu");
                menu.Text = fn_Menu.GetTopMenu_ShipFreight(Req_Lang, Req_RootID, Req_CompID, Req_Tab);


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
    /// <param name="pageIndex"></param>
    private void LookupDataList(int pageIndex)
    {
        //----- 宣告:分頁參數 -----
        int RecordsPerPage = 20;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<

        //固定條件:TOP選單
        PageParam.Add("tab=" + Server.UrlEncode(Req_Tab));

        //[取得/檢查參數] - ShipDate
        if (!string.IsNullOrWhiteSpace(Req_sDate_Ship))
        {
            search.Add("ShipsDate", Req_sDate_Ship);

            PageParam.Add("sDate_Ship=" + Server.UrlEncode(Req_sDate_Ship));
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate_Ship))
        {
            search.Add("ShipeDate", Req_eDate_Ship);

            PageParam.Add("eDate_Ship=" + Server.UrlEncode(Req_eDate_Ship));
        }

        //[取得/檢查參數] - Way
        if (!string.IsNullOrWhiteSpace(Req_Way))
        {
            if (!Req_Way.Equals("ALL"))
            {
                search.Add("Way", Req_Way);
            }

            PageParam.Add("Way=" + Server.UrlEncode(Req_Way));
        }

        //[取得/檢查參數] - ShipFrom
        if (!string.IsNullOrWhiteSpace(Req_ShipFrom))
        {
            if (!Req_ShipFrom.Equals("ALL"))
            {
                search.Add("ShipFrom", Req_ShipFrom);
            }

            PageParam.Add("ShipFrom=" + Server.UrlEncode(Req_ShipFrom));
        }

        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipFreightSendList(Req_CompID, search, out ErrMsg);


        //----- 資料整理:取得總筆數 -----
        TotalRow = query.Count();

        //----- 資料整理:頁數判斷 -----
        if (pageIndex > ((TotalRow / RecordsPerPage) + ((TotalRow % RecordsPerPage) > 0 ? 1 : 0)) && TotalRow > 0)
        {
            StartRow = 0;
            pageIndex = 1;
        }

        //----- 資料整理:選取每頁顯示筆數 -----
        var data = query.Skip(StartRow).Take(RecordsPerPage);

        //----- 資料整理:繫結 ----- 
        this.lvDataList.DataSource = data;
        this.lvDataList.DataBind();


        //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
        if (query.Count() == 0)
        {
            this.ph_EmptyData.Visible = true;
            this.ph_Data.Visible = false;
        }
        else
        {
            this.ph_EmptyData.Visible = false;
            this.ph_Data.Visible = true;

            //分頁
            string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                , thisPage, PageParam, false, true);

            Literal lt_Pager = (Literal)this.lvDataList.FindControl("lt_Pager");
            lt_Pager.Text = getPager;
        }

    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;
                Menu3000Repository _data = new Menu3000Repository();

                //取得資料
                var shipWay = DataBinder.Eval(dataItem.DataItem, "ShipWay");
                string Get_Ship = shipWay == null ? "" : shipWay.ToString();

                //取得控制項
                Literal lt_ShipWay = (Literal)e.Item.FindControl("lt_ShipWay");
                lt_ShipWay.Text = _data.GetShipWay(Get_Ship);

                _data = null;

            }
        }
        catch (Exception)
        {
            throw;
        }
    }


    /// <summary>
    /// 數字顏色格式化
    /// </summary>
    /// <param name="inputValue"></param>
    /// <returns></returns>
    public object showNumber(object inputValue)
    {
        if (inputValue == null)
        {
            return "";
        }

        if (inputValue.ToString().Equals("0"))
        {
            return "<span class=\"grey-text text-lighten-2\">{0}</span>".FormatThis(inputValue);

        }

        return inputValue;
    }


    #endregion


    #region -- 按鈕事件 --

    /// <summary>
    /// [按鈕] - 查詢
    /// </summary>
    protected void btn_Search_Click(object sender, EventArgs e)
    {
        Response.Redirect(filterUrl(), false);
    }


    /// <summary>
    /// [按鈕] - 匯出
    /// </summary>
    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {
        //Get DataList
        DataTable myDT = dataList();

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "DataOutput-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }


    /// <summary>
    /// [按鈕] - 發送Email
    /// </summary>
    protected void lbtn_SendMail_Click(object sender, EventArgs e)
    {
        /* 取得收件人清單 */
        //----- 宣告:資料參數 -----
        MenuHomeRepository _data = new MenuHomeRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 ----- 
        switch (Req_CompID)
        {
            default:
                //條件:部門代號(SZ)
                search.Add("DeptRange", "'280','304','314','317','106'");
                break;
        }

        //[資料取得] - 部門Email
        var query = _data.GetDeptMailList(search);

        //收件人清單
        ArrayList mailList = new ArrayList();
        foreach (var item in query)
        {
            mailList.Add(item.mailAddr);
        }

        if (mailList.Count == 0)
        {
            CustomExtension.AlertMsg("查無收件人信箱", "");
            return;
        }

        //呼叫發信
        string dtToday = DateTime.Now.ToString().ToDateString("yyyy/MM/dd");
        if (Send_Email(mailList, "[{0}]发货资料明细 {1}".FormatThis(Req_CompID, dtToday), out ErrMsg))
        {
            CustomExtension.AlertMsg("發送成功!", filterUrl());
            return;
        }
        else
        {
            CustomExtension.AlertMsg("發送失敗..." + ErrMsg, "");
            return;
        }

    }

    /// <summary>
    /// 開始發信
    /// </summary>
    /// <param name="mailList">MailList</param>
    /// <param name="subject">主旨</param>
    /// <param name="ErrMsg"></param>
    /// <returns></returns>
    private bool Send_Email(ArrayList mailList, string subject, out string ErrMsg)
    {
        try
        {
            //開始發信
            using (MailMessage Msg = new MailMessage())
            {
                //寄件人
                Msg.From = new MailAddress(fn_Param.SysMail_Sender, "寶工內部網站");

                //收件人
                foreach (string email in mailList)
                {
                    Msg.To.Add(new MailAddress(email));
                }

                //主旨
                Msg.Subject = subject;

                //內文組成
                #region -- Html組成 --
                //Params
                string _sDate_Ship = this.filter_sDate_Ship.Text;
                string _eDate_Ship = this.filter_eDate_Ship.Text;

                //Get Html
                StringBuilder html = new StringBuilder();
                
                //Html模版路徑(From CDN)
                string url = "{0}PKHome/ShipFreight/SendListEmail.html?v=1.0".FormatThis(fn_Param.CDNUrl);

                //取得HTML模版(Html不可放在本機)
                string htmlPage = CustomExtension.WebRequest_byGET(url);

                //加入模版內容
                html.Append(htmlPage);

                //取代指定內容
                html.Replace("#資料區間#", "{0} ~ {1}".FormatThis(_sDate_Ship, _eDate_Ship));
                html.Replace("#今年#", DateTime.Now.Year.ToString());
                html.Replace("#來源網址#", filterUrl());

                //[資料取得] - 取得迴圈內容
                DataTable myDT = dataList();

                //Table內容組成
                string innerHtml = "";
                //add header row
                innerHtml += "<tr>";
                for (int i = 0; i < myDT.Columns.Count; i++)
                    innerHtml += "<th>" + myDT.Columns[i].ColumnName + "</th>";
                innerHtml += "</tr>";
                //add rows
                for (int i = 0; i < myDT.Rows.Count; i++)
                {
                    innerHtml += "<tr>";
                    for (int j = 0; j < myDT.Columns.Count; j++)
                        innerHtml += "<td>" + myDT.Rows[i][j].ToString() + "</td>";
                    innerHtml += "</tr>";
                }

                //填入table
                html.Replace("#LoopItems#", innerHtml);

                #endregion

                //Body
                Msg.Body = html.ToString();


                Msg.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient();

                smtp.Send(Msg);
                smtp.Dispose();

                //OK
                ErrMsg = "";
                return true;
            }
        }
        catch (Exception ex)
        {
            ErrMsg = "郵件發送失敗..." + ex.Message.ToString();
            return false;
        }
    }

    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 回傳資料欄位
    /// </summary>
    /// <param name="search"></param>
    /// <returns></returns>
    private DataTable dataList()
    {
        //Params
        string _sDate_Ship = this.filter_sDate_Ship.Text;
        string _eDate_Ship = this.filter_eDate_Ship.Text;
        string _Way = this.filter_ShipWay.SelectedValue;
        string _ShipFrom = this.filter_ShipFrom.SelectedValue;

        //----- 宣告:資料參數 -----
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<

        //[取得/檢查參數] - ShipDate
        if (!string.IsNullOrWhiteSpace(_sDate_Ship))
        {
            search.Add("ShipsDate", _sDate_Ship);
        }
        if (!string.IsNullOrWhiteSpace(_eDate_Ship))
        {
            search.Add("ShipeDate", _eDate_Ship);
        }

        //[取得/檢查參數] - Way
        if (!string.IsNullOrWhiteSpace(_Way))
        {
            if (!_Way.Equals("ALL"))
            {
                search.Add("Way", _Way);
            }
        }

        //[取得/檢查參數] - ShipFrom
        if (!string.IsNullOrWhiteSpace(_ShipFrom))
        {
            if (!_ShipFrom.Equals("ALL"))
            {
                search.Add("ShipFrom", _ShipFrom);
            }
        }

        #endregion

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();

        //----- 方法:取得資料 -----
        var query = _data.GetShipFreightSendList(Req_CompID, search, out ErrMsg)
            .Select(fld => new
            {
                CustName = fld.CustName,
                ErpNo = fld.Erp_SO_FID + "-" + fld.Erp_SO_SID,
                ShipDate = fld.ShipDate,
                TotalPrice = fld.TotalPrice.ToString().ToMoneyString(),
                ShipWay = _data.GetShipWay(fld.ShipWay),
                ShipCompName = fld.ShipCompName,
                ShipNo = fld.ShipNo,
                ShipWho = fld.ShipWho,
                StockName = fld.StockName,
                ShipCnt = fld.ShipCnt,
                Pay1 = fld.Pay1,
                Pay2 = fld.Pay2,
                Pay3 = fld.Pay3,
                AvgPay1 = fld.AvgPay1,
                AvgPay2 = fld.AvgPay2,
                AvgPay3 = fld.AvgPay3,
                Remark = fld.Remark
            });

        //Sum
        var sumShipCnt = query.Select(o => o.ShipCnt).Sum();
        var sumPay1 = query.Select(o => o.Pay1).Sum();
        var sumPay2 = query.Select(o => o.Pay2).Sum();
        var sumPay3 = query.Select(o => o.Pay3).Sum();

        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);

        if (myDT.Rows.Count > 0)
        {
            //重新命名欄位標頭
            myDT.Columns["CustName"].ColumnName = "客户";
            myDT.Columns["ErpNo"].ColumnName = "ERP单号";
            myDT.Columns["ShipDate"].ColumnName = "发货日期";
            myDT.Columns["TotalPrice"].ColumnName = "销货金额";
            myDT.Columns["ShipWay"].ColumnName = "物流途径";
            myDT.Columns["ShipCompName"].ColumnName = "货运公司";
            myDT.Columns["ShipNo"].ColumnName = "物流单号";
            myDT.Columns["ShipWho"].ColumnName = "收货人";
            myDT.Columns["StockName"].ColumnName = "出货地";
            myDT.Columns["ShipCnt"].ColumnName = "件数";
            myDT.Columns["Pay1"].ColumnName = "到付$";
            myDT.Columns["Pay2"].ColumnName = "自付$";
            myDT.Columns["Pay3"].ColumnName = "垫付$";
            myDT.Columns["AvgPay1"].ColumnName = "平均到付$";
            myDT.Columns["AvgPay2"].ColumnName = "平均自付$";
            myDT.Columns["AvgPay3"].ColumnName = "平均垫付$";
            myDT.Columns["Remark"].ColumnName = "备注";
        }

        //在footer新增一列
        DataRow dtRow = myDT.NewRow();
        dtRow[8] = "-- 合計 --";
        dtRow[9] = sumShipCnt;
        dtRow[10] = sumPay1;
        dtRow[11] = sumPay2;
        dtRow[12] = sumPay3;

        myDT.Rows.Add(dtRow);

        //release
        query = null;

        //return
        return myDT;
    }


    /// <summary>
    /// 含查詢條件的完整網址
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _sDate_Ship = this.filter_sDate_Ship.Text;
        string _eDate_Ship = this.filter_eDate_Ship.Text;
        string _Way = this.filter_ShipWay.SelectedValue;
        string _ShipFrom = this.filter_ShipFrom.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?tab={1}&page=1".FormatThis(thisPage, Req_Tab));

        //[查詢條件] - ShipDate
        if (!string.IsNullOrWhiteSpace(_sDate_Ship))
        {
            url.Append("&sDate_Ship=" + Server.UrlEncode(_sDate_Ship));
        }
        if (!string.IsNullOrWhiteSpace(_eDate_Ship))
        {
            url.Append("&eDate_Ship=" + Server.UrlEncode(_eDate_Ship));
        }

        //[查詢條件] - Way
        if (!string.IsNullOrWhiteSpace(_Way))
        {
            url.Append("&Way=" + Server.UrlEncode(_Way));
        }

        //[取得/檢查參數] - ShipFrom
        if (!string.IsNullOrWhiteSpace(_ShipFrom))
        {
            url.Append("&ShipFrom=" + Server.UrlEncode(_ShipFrom));
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
    /// 取得網址參數 - Company ID(TW/SH/SZ)
    /// </summary>
    private string _Req_CompID;
    public string Req_CompID
    {
        get
        {
            String DataID = Page.RouteData.Values["CompID"].ToString();

            return DataID.ToLower().Equals("unknown") ? "SZ" : DataID;
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
        return "{0}{1}/{2}/ShipFreightSend/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
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


    public string Req_Tab
    {
        get
        {
            string data = Request.QueryString["tab"] == null ? "2" : Request.QueryString["tab"].ToString();
            return data;
        }
        set
        {
            this._Req_Tab = value;
        }
    }
    private string _Req_Tab;


    /// <summary>
    /// 取得傳遞參數 - 發貨日
    /// </summary>
    public string Req_sDate_Ship
    {
        get
        {
            String _data = Request.QueryString["sDate_Ship"];
            string dt = DateTime.Now.ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_sDate_Ship = value;
        }
    }
    private string _Req_sDate_Ship;


    /// <summary>
    /// 取得傳遞參數 - 發貨日
    /// </summary>
    public string Req_eDate_Ship
    {
        get
        {
            String _data = Request.QueryString["eDate_Ship"];
            string dt = DateTime.Now.ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_eDate_Ship = value;
        }
    }
    private string _Req_eDate_Ship;


    /// <summary>
    /// 取得傳遞參數 - Way
    /// </summary>
    public string Req_Way
    {
        get
        {
            String _data = Request.QueryString["Way"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "2", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Way = value;
        }
    }
    private string _Req_Way;


    /// <summary>
    /// 取得傳遞參數 - ShipFrom
    /// </summary>
    public string Req_ShipFrom
    {
        get
        {
            String _data = Request.QueryString["ShipFrom"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "2", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_ShipFrom = value;
        }
    }
    private string _Req_ShipFrom;


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

}
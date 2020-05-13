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

public partial class myShipping_StatWeek : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //** 檢查必要參數 **
                if (string.IsNullOrEmpty(Req_CompID))
                {
                    Response.Redirect("{0}Error/參數錯誤".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #region --權限--
                //[權限判斷] Start
                /* 
                 * 使用公司別代號，判斷對應的MENU ID
                 */
                bool isPass = false;
                string getCorpUid = fn_Param.GetCorpUID(Req_CompID);

                switch (getCorpUid)
                {
                    case "2":
                        //深圳寶工
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3701");
                        break;

                    default:
                        //SH
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3702");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //取得公司別
                string _corpName = fn_Param.GetCorpName(getCorpUid);
                lt_CorpName.Text = _corpName;
                Page.Title += "-" + _corpName;

                //[權限判斷] End
                #endregion


                #region --Request參數--
                //[取得/檢查參數] - Year
                if (!string.IsNullOrWhiteSpace(Req_Year))
                {
                    this.filter_Year.Text = Req_Year;
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
        //----- 宣告:分頁參數 -----
        //ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<

        //固定條件:TOP選單
        //PageParam.Add("tab=" + Server.UrlEncode(Req_Tab));

        //[取得/檢查參數] - Year
        if (!string.IsNullOrWhiteSpace(Req_Year))
        {
            search.Add("Year", Req_Year);

            //PageParam.Add("Year=" + Server.UrlEncode(Req_Year));
        }

        //[取得/檢查參數] - ShipFrom
        if (!string.IsNullOrWhiteSpace(Req_ShipFrom))
        {
            if (!Req_ShipFrom.Equals("ALL"))
            {
                search.Add("ShipFrom", Req_ShipFrom);
            }

            //PageParam.Add("ShipFrom=" + Server.UrlEncode(Req_ShipFrom));
        }

        #endregion

        //----- 原始資料:取得所有資料 -----
        DataTable myDT = _data.GetShipStat_Week(Req_CompID, search, out ErrMsg);

        //----- 資料整理:顯示分頁(放在DataBind之後) ----- 
        if (myDT.Rows.Count == 0)
        {
            this.ph_EmptyData.Visible = true;
            this.ph_Data.Visible = false;
        }
        else
        {
            this.ph_EmptyData.Visible = false;
            this.ph_Data.Visible = true;


            //重新命名欄位標頭
            myDT.Columns["CustName"].ColumnName = "客户";

            //Table內容組成
            string _header = "";
            string _body = "";
            int sumCol = 0;
            Dictionary<int, int> sumRow = new Dictionary<int, int>();

            //-- add header row
            _header += "<tr>";
            for (int i = 0; i < myDT.Columns.Count; i++)
            {
                //column name
                string _colName = myDT.Columns[i].ColumnName;
                _header += "<th class=\"center aligned\">{0}</th>".FormatThis(_colName);

                //初始化直排計算欄
                //排除第一欄(客戶名)
                if (i > 0)
                {
                    sumRow.Add(i, 0);
                }
            }
            _header += "<th class=\"center aligned\">合計</th>";
            _header += "</tr>";

            //-- add body rows
            for (int i = 0; i < myDT.Rows.Count; i++)
            {
                //橫向加總-初始化
                sumCol = 0;

                _body += "<tr>";
                for (int j = 0; j < myDT.Columns.Count; j++)
                {
                    //取得每個欄位值
                    string val = myDT.Rows[i][j].ToString();
                    int getVal = 0;

                    //排除第一欄(客戶名)
                    if (j > 0)
                    {
                        //取得值
                        getVal = string.IsNullOrWhiteSpace(val) ? 0 : Convert.ToInt16(val);
                        //縱向加總
                        sumRow[j] = Convert.ToInt32(sumRow[j]) + getVal;
                    }

                    //顯示欄
                    _body += "<td class=\"center aligned\">" + val + "</td>";

                    //橫向加總-Count
                    sumCol += getVal;

                }
                //顯示橫向合計欄
                _body += "<td class=\"center aligned negative\">" + sumCol + "</td>";
                _body += "</tr>";
            }

            //footer合計
            _body += "<tr class=\"negative\"><td class=\"center aligned\">合計</td>";

            //顯示縱向合計
            var query = sumRow
                .Select(fld => new
                {
                    key = fld.Key,
                    val = fld.Value
                })
                .OrderBy(o => o.key);
            foreach (var item in query)
            {
                _body += "<td class=\"center aligned\">" + item.val + "</td>";
            }

            _body += "<td></td></tr>";

            //output
            this.lt_header.Text = _header;
            this.lt_body.Text = _body;
        }

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


    #endregion


    #region -- 附加功能 --

    /// <summary>
    /// 含查詢條件的完整網址
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _Year = this.filter_Year.Text;
        string _ShipFrom = this.filter_ShipFrom.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?tab={1}".FormatThis(thisPage, Req_Tab));

        //[查詢條件] - Year
        if (!string.IsNullOrWhiteSpace(_Year))
        {
            url.Append("&Year=" + Server.UrlEncode(_Year));
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
        return "{0}{1}/{2}/ShipFreightStat_W/{3}".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID
            , Req_CompID);
    }

    #endregion


    #region -- 傳遞參數 --

    public string Req_Tab
    {
        get
        {
            string data = Request.QueryString["tab"] == null ? "4" : Request.QueryString["tab"].ToString();
            return data;
        }
        set
        {
            this._Req_Tab = value;
        }
    }
    private string _Req_Tab;


    /// <summary>
    /// 取得傳遞參數 - Year
    /// </summary>
    public string Req_Year
    {
        get
        {
            String _data = Request.QueryString["Year"];
            string dt = string.IsNullOrWhiteSpace(_data) ? DateTime.Now.Year.ToString() : _data;

            return (CustomExtension.String_資料長度Byte(_data, "1", "4", out ErrMsg)) ? dt.Trim() : dt;
        }
        set
        {
            this._Req_Year = value;
        }
    }
    private string _Req_Year;


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
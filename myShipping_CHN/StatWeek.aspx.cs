using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ShipFreight_CN.Controllers;
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
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;

                switch (Req_DataType)
                {
                    case "1":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3703");
                        break;

                    default:
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3704");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //取得公司別
                string _corpName = "中國內銷({0})".FormatThis(fn_Menu.GetECData_RefType(Convert.ToInt16(Req_DataType)));
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

                #endregion


                //*** 設定母版的Menu ***
                Literal menu = (Literal)Page.Master.FindControl("lt_headerMenu");
                menu.Text = fn_Menu.GetTopMenu_ShipFreight_CHN(Req_Lang, Req_RootID, Req_Tab, Req_DataType);


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
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
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

        #endregion

        //----- 原始資料:取得所有資料 -----
        DataTable myDT = _data.GetShipStat_Week(search, out ErrMsg);

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

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?tab={1}&dt={2}".FormatThis(FuncPath(), Req_Tab, Req_DataType));

        //[查詢條件] - Year
        if (!string.IsNullOrWhiteSpace(_Year))
        {
            url.Append("&Year=" + Server.UrlEncode(_Year));
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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/{2}/ShipFreightStat_W_CHN".FormatThis(
            fn_Param.WebUrl
            , Req_Lang
            , Req_RootID);
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
    /// 資料判別:1=工具/2=玩具
    /// </summary>
    public string Req_DataType
    {
        get
        {
            string data = Request.QueryString["dt"] == null ? "1" : Request.QueryString["dt"].ToString();
            return data;
        }
        set
        {
            this._Req_DataType = value;
        }
    }
    private string _Req_DataType;


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
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}?dt={1}&tab={2}".FormatThis(FuncPath(), Req_DataType, Req_Tab);
        }
        set
        {
            this._thisPage = value;
        }
    }
    private string _thisPage;
    #endregion

}
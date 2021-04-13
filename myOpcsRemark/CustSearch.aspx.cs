using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu3000Data.Controllers;
using PKLib_Method.Methods;


public partial class CustSearch : SecurityCheck
{    
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                //** 檢查必要參數 **
                if (string.IsNullOrEmpty(Req_DBS))
                {
                    Response.Redirect("{0}Error/參數錯誤".FormatThis(fn_Param.WebUrl));
                    return;
                }

                #region --權限--
                //[權限判斷] Start
                bool isPass = false;
                switch (Req_DBS)
                {
                    case "SH":
                        //SH
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3312");
                        break;

                    default:
                        //TW
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3311");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //[權限判斷] End
                #endregion

                //page title
                Page.Title += "(" + Req_DBS + ")";

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
        //----- 宣告:網址參數 -----
        int RecordsPerPage = 20;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        int DataCnt = 0;
        ArrayList PageParam = new ArrayList();  //分類暫存條件參數

        //----- 宣告:資料參數 -----
        Menu3000Repository _data = new Menu3000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            #region >> 條件篩選 <<

            //固定參數
            search.Add("dbs", Req_DBS);
            PageParam.Add("dbs=" + Server.UrlEncode(Req_DBS));

            //Params
            string _Keyword = Req_Keyword;
                        
            //[查詢條件] - Keyword
            if (!string.IsNullOrWhiteSpace(_Keyword))
            {
                search.Add("Keyword", _Keyword);
                PageParam.Add("k=" + Server.UrlEncode(_Keyword));
                filter_Keyword.Text = _Keyword;
            }
            #endregion

            //----- 原始資料:取得所有資料 -----
            var query = _data.Get_CustRemkList(search, Req_DBS, StartRow, RecordsPerPage, true
                , out DataCnt, out ErrMsg);

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
            if (query.Rows.Count == 0)
            {
                ph_EmptyData.Visible = true;
                ph_Data.Visible = false;

                //Clear
                CustomExtension.setCookie("CustRemk", "", -1);
            }
            else
            {
                ph_EmptyData.Visible = false;
                ph_Data.Visible = true;

                //分頁設定
                string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                    , thisPage, PageParam, false, true);

                Literal lt_Pager = (Literal)this.lvDataList.FindControl("lt_Pager");
                lt_Pager.Text = getPager;

                //重新整理頁面Url
                string reSetPage = "{0}?page={1}{2}".FormatThis(
                    thisPage
                    , pageIndex
                    , (PageParam.Count == 0 ? "" : "&") + string.Join("&", PageParam.ToArray()));

                //暫存頁面Url, 給其他頁使用
                CustomExtension.setCookie("CustRemk", Server.UrlEncode(reSetPage), 1);

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
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        ////----- 宣告:資料參數 -----
        //MGMTRepository _data = new MGMTRepository();
        //try
        //{
        //    //----- 方法:刪除資料 -----
        //    if (false == _data.Delete_MGHelp(Get_DataID))
        //    {
        //        CustomExtension.AlertMsg("刪除失敗", ErrMsg);
        //        return;
        //    }
        //    else
        //    {
        //        //刪除檔案
        //        string ftpFolder = "{0}MG_Help/{1}".FormatThis(
        //            System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]
        //            , Get_TraceID);
        //        _ftp.FTP_DelFolder(ftpFolder);


        //        //導向本頁(帶參數)
        //        Response.Redirect(filterUrl());
        //    }
        //}
        //catch (Exception)
        //{

        //    throw;
        //}
        //finally
        //{
        //    _data = null;
        //}

    }


    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                //Declare
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                string _Remk_Normal = DataBinder.Eval(dataItem.DataItem, "Remk_Normal").ToString();
                string _Remk_2210 = DataBinder.Eval(dataItem.DataItem, "Remk_2210").ToString();

                PlaceHolder ph_Modal_r1 = (PlaceHolder)e.Item.FindControl("ph_Modal_r1");
                PlaceHolder ph_Modal_r2 = (PlaceHolder)e.Item.FindControl("ph_Modal_r2");

                ph_Modal_r1.Visible = !string.IsNullOrWhiteSpace(_Remk_Normal);
                ph_Modal_r2.Visible = !string.IsNullOrWhiteSpace(_Remk_2210);

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
    
    #endregion


    #region -- 附加功能 --
    
    /// <summary>
    /// 含查詢條件的完整網址(新查詢)
    /// </summary>
    /// <returns></returns>
    public string filterUrl()
    {
        //Params
        string _Keyword = this.filter_Keyword.Text;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page
        url.Append("{0}?page=1&dbs={1}".FormatThis(thisPage, Req_DBS));
        
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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}myOpcsRemark/".FormatThis(fn_Param.WebUrl);
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
    /// 取得傳遞參數 - DBS
    /// </summary>
    public string Req_DBS
    {
        get
        {
            String _data = Request.QueryString["dbs"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "4", out ErrMsg)) ? _data.Trim() : "TW";
        }
        set
        {
            this._Req_DBS = value;
        }
    }
    private string _Req_DBS;


    /// <summary>
    /// 設定參數 - 本頁Url
    /// </summary>
    public string thisPage
    {
        get
        {
            return "{0}CustSearch.aspx".FormatThis(FuncPath());
        }
        set
        {
            this._thisPage = value;
        }
    }
    private string _thisPage;

    #endregion
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;
using twMGMTData.Controllers;
using twMGMTData.Models;


public partial class MGHelp_View : SecurityCheck
{
    //回覆權限
    public bool _ReplyAuth = false;
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //[權限判斷]
            bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "2412");
            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }

            //取得回覆權限
            _ReplyAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "2413");

            if (!IsPostBack)
            {
                //[參數判斷] - 資料編號
                if (string.IsNullOrWhiteSpace(Req_DataID))
                {
                    CustomExtension.AlertMsg("錯誤操作,即將返回列表頁.", Page_SearchUrl);
                    return;
                }
                else
                {
                    //載入資料
                    LookupData();

                }


            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示:基本資料 --

    /// <summary>
    /// 取得基本資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            search.Add("DataID", Req_DataID);

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetOne_MGhelp(search, out ErrMsg).Take(1).FirstOrDefault();

            //----- 資料整理:繫結 ----- 
            if (query == null)
            {
                CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
                return;
            }

            #region >> 欄位填寫:需求資料 <<

            //處理狀態ID
            string _currStatus = query.Help_Status.ToString();
            //需求類別ID
            string _currReqCls = query.Req_Class.ToString();

            //填入資料
            lt_TraceID.Text = query.TraceID;
            lt_ReqStatus.Text = _data.GetMgHelp_StatusLabel(query.Help_Status.ToString(), query.Help_StatusName);  //處理狀態
            lt_CreateDate.Text = query.Create_Time.ToDateString("yyyy/MM/dd");
            tb_ReqSubject.Text = query.Help_Subject;
            tb_ReqContent.Text = query.Help_Content.Replace("\r\n", "<br/>");
            tb_Help_Benefit.Text = query.Help_Benefit.Replace("\r\n", "<br/>");
            ph_Benefit.Visible = !string.IsNullOrWhiteSpace(query.Help_Benefit);
            lt_ReqClass.Text = query.Req_ClassName; //需求類別

            //報修方式
            rbl_Help_Way.Text = GetName_HelpWay(query.Help_Way.ToString());
            //需求者
            lb_Emp.Text = query.Req_WhoName + " (" + query.Req_NickName + ") #" + query.Req_TelExt;

            ////主管同意, 權限申請(需求類別=12時顯示)
            //lt_AuthAgree.Text = query.IsAgree.Equals("N") ? "未同意"
            //    : "{0} 於 {1} 同意申請".FormatThis(
            //        query.Agree_WhoName
            //        , query.Agree_Time.ToString().ToDateString("yyyy/MM/dd HH:mm")
            //    );
            //ph_Agree.Visible = _currReqCls.Equals("12");

            #endregion


            #region >> 欄位填寫:回覆資料 <<
            //工時
            lt_Finish_Hours.Text = query.Finish_Hours.ToString();
            tb_ReplyContent.Text = query.Reply_Content.Replace("\r\n", "<br/>");
            lt_Wish_Date.Text = query.Wish_Time.ToDateString("yyyy/MM/dd");

            //結案日
            string _fDate = query.Finish_Time.ToString();
            lt_Finish_Time.Text = string.IsNullOrWhiteSpace(_fDate) ? "案件處理中" : _fDate.ToDateString("yyyy/MM/dd");

            //結案人
            string _fWho = query.Finish_WhoName.ToString();
            lt_Finish_Who.Text = string.IsNullOrWhiteSpace(_fWho) ? "案件處理中" : _fWho;


            #endregion


            #region >> 欄位填寫:驗收資料 <<

            rbl_RateScore.SelectedValue = query.RateScore.ToString();
            tb_RateContent.Text = query.RateContent;
            lt_RateWho.Text = query.RateWhoName;
            ph_IsDoneWrite.Visible = !string.IsNullOrWhiteSpace(query.RateWhoName);

            #endregion


            #region >> 欄位顯示控制 <<
            pl_doJob.Visible = _ReplyAuth;

            //結案:隱藏部份按鈕
            if (_currStatus.Equals("140"))
            {
                //區塊:驗收意見
                ph_unClose.Visible = false;
                ph_section3_data.Visible = true;

            }
            else
            {
                //區塊:驗收意見
                ph_unClose.Visible = true;
                ph_section3_data.Visible = false;
            }

            #endregion


            #region >> 其他功能 <<

            //-- 載入其他資料 --
            LookupData_Files();
            LookupData_Inform();
            LookupData_Processes();

            #endregion


            //維護資訊
            info_Creater.Text = query.Create_Name;
            info_CreateTime.Text = query.Create_Time.ToDateString("yyyy-MM-dd HH:mm");
            info_Updater.Text = query.Update_Name;
            info_UpdateTime.Text = query.Update_Time.ToDateString("yyyy-MM-dd HH:mm");

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


    /// <summary>
    /// 報修方式
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private string GetName_HelpWay(string id)
    {
        switch (id)
        {
            case "1":
                return "即時通";
            case "2":
                return "電話";
            case "3":
                return "面談";

            default:
                return "";
        }
    }
    #endregion


    #region -- 資料顯示:檔案附件 --

    /// <summary>
    /// 顯示檔案附件
    /// </summary>
    private void LookupData_Files()
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMgHelpFileList(Req_DataID, "", "A");

        //----- 資料整理:繫結 ----- 
        this.lv_Attachment.DataSource = query;
        this.lv_Attachment.DataBind();

        //Release
        query = null;
        _data = null;
    }

    protected void lv_Attachment_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取資料
                string _traceID = lt_TraceID.Text;
                string _attFile = DataBinder.Eval(dataItem.DataItem, "AttachFile").ToString();
                string _attFileOrg = DataBinder.Eval(dataItem.DataItem, "AttachFile_Org").ToString();
                string _CheckID = DataBinder.Eval(dataItem.DataItem, "Create_Who").ToString();

                //Set Url
                string url = "<a href=\"{0}{1}{2}/{3}\" target=\"_blank\">{4}</a>".FormatThis(
                    fn_Param.RefUrl
                    , UploadFolder
                    , string.IsNullOrEmpty(_CheckID) ? "" : _traceID //舊版沒有資料夾分類(故以此判斷)
                    , _attFile
                    , _attFileOrg
                    );

                //Set object
                Literal lt_FileUrl = (Literal)e.Item.FindControl("lt_FileUrl");
                lt_FileUrl.Text = url;
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion


    #region -- 資料顯示:轉寄人員 --

    /// <summary>
    /// 顯示轉寄人員
    /// </summary>
    private void LookupData_Inform()
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMgHelpCCList(Req_DataID);


        //----- 資料整理:繫結 ----- 
        this.lv_Inform.DataSource = query;
        this.lv_Inform.DataBind();

        //Release
        query = null;
    }

    #endregion


    #region -- 資料顯示:處理記錄 --
    /// <summary>
    /// 顯示處理記錄
    /// </summary>
    private void LookupData_Processes()
    {
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetMgHelpProcList(Req_DataID);

        //----- 資料整理:繫結 ----- 
        this.lv_ProcList.DataSource = query;
        this.lv_ProcList.DataBind();

        //Release
        query = null;
    }

    protected void lv_ProcList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                string _id = DataBinder.Eval(dataItem.DataItem, "DetailID").ToString();
                string _traceID = lt_TraceID.Text;

                //取得&設定時間:預計完成, 確認時間
                string _Wish_Time = DataBinder.Eval(dataItem.DataItem, "Wish_Time").ToString();
                Literal lt_TimeDesc1 = (Literal)e.Item.FindControl("lt_TimeDesc1");
                lt_TimeDesc1.Text = string.IsNullOrWhiteSpace(_Wish_Time) ? "" : "<div class=\"ui basic label\">預計完成<div class=\"detail\">" + _Wish_Time + "</div></div>";

                string _Confirm_Time = DataBinder.Eval(dataItem.DataItem, "Confirm_Time").ToString();
                Literal lt_TimeDesc2 = (Literal)e.Item.FindControl("lt_TimeDesc2");
                lt_TimeDesc2.Text = string.IsNullOrWhiteSpace(_Confirm_Time) ? "" : "<div class=\"ui basic label\">確認時間<div class=\"detail\">" + _Confirm_Time + "</div></div>";


                #region >> 附件 <<

                //附件Html
                Literal lt_AttachDesc = (Literal)e.Item.FindControl("lt_AttachDesc");
                lt_AttachDesc.Text = "";

                //----- 宣告:資料參數 -----
                MGMTRepository _data = new MGMTRepository();

                try
                {
                    var query = _data.GetMgHelpFileList(Req_DataID, _id, "B");
                    foreach (var item in query)
                    {
                        lt_AttachDesc.Text += "<a href=\"{0}\" class=\"ui tiny button\" target=\"_blank\"><i class=\"paperclip icon\"></i>{1}</a>".FormatThis(
                                fn_Param.RefUrl + UploadFolder + _traceID + "/" + _id + "/" + item.AttachFile
                                , item.AttachFile_Org
                            );
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

                #endregion

            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    #endregion




    #region -- 資料編輯:驗收意見 --
    protected void btn_doSaveRate_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        //取得欄位資料
        string _id = Req_DataID;
        string _RateScore = rbl_RateScore.SelectedValue;
        string _RateContent = tb_RateContent.Text;

        #region ** 欄位判斷 **

        if (string.IsNullOrWhiteSpace(_RateContent))
        {
            errTxt += "請填寫「驗收意見」\\n";
        }

        #endregion

        //顯示不符規則的警告
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        #region ** 資料處理 **
        //----- 宣告:資料參數 -----
        MGMTRepository _data = new MGMTRepository();

        try
        {
            //----- 設定:資料欄位 -----
            var data = new MgHelpData
            {
                DataID = new Guid(_id),
                RateScore = Convert.ToInt16(_RateScore),
                RateContent = _RateContent.Left(400),
            };

            //----- 方法:修改資料 -----
            if (!_data.Update_MGHelpRate(data, out ErrMsg))
            {
                this.ph_ErrMessage.Visible = true;
                this.lt_ShowMsg.Text = "<b>驗收意見儲存失敗</b><p>{0}</p><p>{1}</p>".FormatThis("遇到無法排除的錯誤，請聯絡系統管理員。", ErrMsg);

                CustomExtension.AlertMsg("驗收意見失敗", "");
                return;
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
        #endregion


        //導向本頁
        Response.Redirect(thisPage + "#section3");

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
            String _data = Request.QueryString["id"];

            return string.IsNullOrWhiteSpace(_data) ? "" : _data.Trim();
        }
        set
        {
            _Req_DataID = value;
        }
    }

    /// <summary>
    /// 設定參數 - 列表頁Url
    /// </summary>
    private string _Page_SearchUrl;
    public string Page_SearchUrl
    {
        get
        {
            string tempUrl = CustomExtension.getCookie("MGHelp");

            return string.IsNullOrWhiteSpace(tempUrl) ? fn_Param.WebUrl + "twManagement/MGHelp_Search.aspx" : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
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
            return "{0}twManagement/MGHelp_View.aspx?id={1}".FormatThis(fn_Param.WebUrl, Req_DataID);
        }
        set
        {
            _thisPage = value;
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
            return "{0}MG_Help/".FormatThis(System.Web.Configuration.WebConfigurationManager.AppSettings["File_Folder"]);
        }
        set
        {
            this._UploadFolder = value;
        }
    }

    #endregion

}
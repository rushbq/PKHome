using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Menu4000Data.Controllers;
using Menu4000Data.Models;
using PKLib_Method.Methods;

public partial class mySupInvCheck_SettingEdit : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //[權限判斷] Start
            #region --權限--

            bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "4861");

            if (!isPass)
            {
                Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                return;
            }

            #endregion
            //[權限判斷] End


            if (!IsPostBack)
            {
                //[參數判斷] - 資料編號
                if (Req_DataID.Equals("new"))
                {
                    ph_Details.Visible = false;
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
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            search.Add("DataID", Req_DataID);

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetOneSupInvSend(search, out ErrMsg).FirstOrDefault();

            //----- 資料整理:繫結 ----- 
            if (query == null)
            {
                CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
                return;
            }

            //Check
            if (query.IsOnTask.Equals("Y"))
            {
                CustomExtension.AlertMsg("資料已加入排程", Page_SearchUrl);
                return;
            }

            #region >> 欄位填寫 <<

            //--- 填入基本資料 ---
            lt_SeqNo.Text = query.SeqNo.ToString();
            hf_DataID.Value = query.Data_ID.ToString();
            tb_Subject.Text = query.Subject;
            rbl_IsOnTask.SelectedValue = query.IsOnTask;
            tb_TaskTime.Text = query.TaskTime;
            ph_Details.Visible = true;
            lt_SaveBase.Text = "修改設定";

            #endregion


            #region >> 其他功能 <<

            //-- 載入其他資料 --
            LookupData_UnCheckList();
            LookupData_CheckList();

            #endregion


            //維護資訊
            info_Creater.Text = query.Create_Name;
            info_CreateTime.Text = query.Create_Time;
            info_Updater.Text = query.Update_Name;
            info_UpdateTime.Text = query.Update_Time;
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


    #region -- 資料編輯:基本資料 --
    //SAVE-基本資料
    protected void btn_doSaveBase_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        if (string.IsNullOrWhiteSpace(tb_Subject.Text))
        {
            errTxt += "主旨空白\\n";
        }
        if (string.IsNullOrWhiteSpace(tb_TaskTime.Text))
        {
            errTxt += "排程時間空白\\n";
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
    /// 資料新增
    /// </summary>
    private void Add_Data()
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();

        try
        {
            //----- 設定:資料欄位 -----
            //產生Guid
            string guid = CustomExtension.GetGuid();
            string _Subject = tb_Subject.Text;
            string _TaskTime = tb_TaskTime.Text;
            string _IsOnTask = rbl_IsOnTask.SelectedValue;

            var data = new SupInvList
            {
                Data_ID = new Guid(guid),
                Subject = _Subject,
                TaskTime = _TaskTime.ToDateString("yyyy/MM/dd HH:mm"),
                IsOnTask = _IsOnTask,
                Create_Who = fn_Param.CurrentUser
            };

            //----- 方法:新增資料 -----
            if (!_data.CreateSupInvBase(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }
            else
            {
                //更新本頁Url
                string thisNewUrl = "{0}/SetEdit/{1}".FormatThis(FuncPath(), guid);

                //導向本頁
                Response.Redirect(thisNewUrl);
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
        Menu4000Repository _data = new Menu4000Repository();

        try
        {
            //----- 設定:資料欄位 -----
            string _id = hf_DataID.Value;
            string _Subject = tb_Subject.Text;
            string _TaskTime = tb_TaskTime.Text;
            string _IsOnTask = rbl_IsOnTask.SelectedValue;

            var data = new SupInvList
            {
                Data_ID = new Guid(_id),
                Subject = _Subject,
                TaskTime = _TaskTime.ToDateString("yyyy/MM/dd HH:mm"),
                IsOnTask = _IsOnTask,
                Update_Who = fn_Param.CurrentUser
            };

            //----- 方法:更新資料 -----
            if (!_data.UpdateSupInvBase(data, out ErrMsg))
            {
                CustomExtension.AlertMsg("更新失敗", thisPage);
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

    #endregion


    #region -- 資料顯示:供應商清單 --

    /// <summary>
    /// 顯示供應商清單, 未加入
    /// </summary>
    private void LookupData_UnCheckList()
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            search.Add("UnCheck", "Y");

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetSupplierList(search, Req_DataID, out ErrMsg);

            //----- 資料整理:繫結 ----- 
            lv_SelectSup.DataSource = query;
            lv_SelectSup.DataBind();

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

    protected void lv_SelectSup_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string Get_SupID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;
            string Get_Shown = ((DropDownList)e.Item.FindControl("ddl_IsShow")).SelectedValue;

            //----- 宣告:資料參數 -----
            Menu4000Repository _data = new Menu4000Repository();

            try
            {
                //----- 方法:判斷是否有品號資料 -----
                if (_data.CheckSupplierModel(Get_SupID, out ErrMsg) == 0)
                {
                    CustomExtension.AlertMsg("品號庫別檔查無資料", "");
                    return;
                }

                //----- 方法:建立資料 -----
                if (false == _data.CreateSupplierItem(Get_SupID, Req_DataID, Get_Shown))
                {
                    CustomExtension.AlertMsg("資料建立失敗", "");
                    return;
                }


                //導向本頁
                Response.Redirect(thisPage + "#section1");
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
    }

    #endregion


    #region -- 資料顯示:已勾選的供應商清單 --

    /// <summary>
    /// 顯示供應商清單, 未加入
    /// </summary>
    private void LookupData_CheckList()
    {
        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            search.Add("IsCheck", "Y");

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetSupplierList(search, Req_DataID, out ErrMsg);

            //----- 資料整理:繫結 ----- 
            lv_CheckedSup.DataSource = query;
            lv_CheckedSup.DataBind();

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

    protected void lv_CheckedSup_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string Get_SupID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

            //----- 宣告:資料參數 -----
            Menu4000Repository _data = new Menu4000Repository();

            try
            {
                //----- 方法:刪除資料 -----
                if (false == _data.Delete_SupplierItem(Req_DataID, Get_SupID))
                {
                    CustomExtension.AlertMsg("資料刪除失敗", "");
                    return;
                }

                //導向本頁
                Response.Redirect(thisPage + "#section2");
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
    }

    protected void lv_CheckedSup_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //取得資料:檢查品項數
                Int32 _DTCnt = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "DataCheck"));
                Literal lt_ChkMsg = (Literal)e.Item.FindControl("lt_ChkMsg");
                if (_DTCnt == 0)
                {
                    lt_ChkMsg.Text = "<span class=\"red-text text-darken-1\">未建立品項</span>";
                }

            }
        }
        catch (Exception)
        {
            throw;
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
        return "{0}{1}/{2}/SupInvCheck".FormatThis(
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
            _Req_DataID = value;
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
            return "{0}/SetEdit/{1}".FormatThis(FuncPath(), Req_DataID);
        }
        set
        {
            _thisPage = value;
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
            string tempUrl = CustomExtension.getCookie("SupInvCheckA");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() + "/Set" : Server.UrlDecode(tempUrl);
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion

}
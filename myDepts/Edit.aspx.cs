using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Data.Models;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class myDepts_Edit : SecurityCheck
{
    public string ErrMsg;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //[權限判斷] Start
            #region --權限--

            bool isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "9111");

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
                    //ph_Details.Visible = false;
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
        DeptsRepository _data = new DeptsRepository();
        //Dictionary<int, string> search = new Dictionary<int, string>();

        try
        {
            //----- 原始資料:取得所有資料 -----
            var query = _data.GetOne(Req_DataID).FirstOrDefault();

            //----- 資料整理:繫結 ----- 
            if (query == null)
            {
                CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
                return;
            }


            #region >> 欄位填寫 <<

            //--- 填入基本資料 ---
            ddl_Area.SelectedValue = query.AreaCode;
            tb_DeptID.Text = query.DeptID;
            tb_DeptName.Text = query.DeptName;
            rbl_Display.SelectedValue = query.Display;
            tb_ErpID.Text = query.ERP_DeptID;
            tb_Email.Text = query.Email;

            ddl_Area.Enabled = false;
            tb_DeptID.Enabled = false;


            #endregion


            #region >> 其他功能 <<

            //-- 載入其他資料 --
            ph_Details.Visible = true;
            LookupData_Supervisors();
            LookupData_UserList();

            #endregion

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
        string _areaCode = ddl_Area.SelectedValue;
        string _deptID = tb_DeptID.Text.Trim();
        string _deptName = tb_DeptName.Text.Trim();
        string _disp = rbl_Display.SelectedValue;
        string _erpID = tb_ErpID.Text.Trim();
        string _email = tb_Email.Text.Trim();

        if (string.IsNullOrWhiteSpace(_deptID))
        {
            errTxt += "部門代號空白\\n";
        }
        if (string.IsNullOrWhiteSpace(_deptName))
        {
            errTxt += "部門名稱空白\\n";
        }
        if (string.IsNullOrWhiteSpace(_erpID))
        {
            errTxt += "對應ERP代號空白\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }


        #region -- 執行新增/更新 --

        //----- 宣告:資料參數 -----
        DeptsRepository _data = new DeptsRepository();

        //----- 設定:資料欄位 -----
        var data = new PKDept
        {
            AreaCode = _areaCode,
            DeptID = _deptID,
            DeptName = _deptName,
            Display = _disp,
            ERP_DeptID = _erpID,
            Email = _email
        };
        //----- 方法:新增/更新資料 -----
        if (!_data.Update(data, out ErrMsg))
        {
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("設定失敗", "");
            return;
        }
        else
        {
            ph_ErrMessage.Visible = false;
            CustomExtension.AlertMsg("設定成功", "");

            //更新本頁Url
            string thisNewUrl = "{0}/Edit/{1}".FormatThis(FuncPath(), _deptID);

            //導向本頁
            Response.Redirect(thisNewUrl);
        }

        #endregion


    }

    #endregion


    #region -- 資料顯示:主管清單 --

    /// <summary>
    /// 顯示主管清單
    /// </summary>
    private void LookupData_Supervisors()
    {
        //----- 宣告:資料參數 -----
        DeptsRepository _data = new DeptsRepository();

        try
        {
            //----- 原始資料:取得所有資料 -----
            var query = _data.Get_Supervisor(Req_DataID, out ErrMsg);

            //----- 資料整理:繫結 ----- 
            lv_Suplist.DataSource = query;
            lv_Suplist.DataBind();

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

    protected void lv_Suplist_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string Get_ID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

            //----- 宣告:資料參數 -----
            DeptsRepository _data = new DeptsRepository();

            try
            {
                //----- 方法:刪除資料 -----
                if (false == _data.Delete_Supervisor(Convert.ToInt32(Get_ID), out ErrMsg))
                {
                    CustomExtension.AlertMsg("資料刪除失敗", "");
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


    protected void btn_SaveDetail_Click(object sender, EventArgs e)
    {
        string errTxt = "";
        string _empID = val_Emp.Text.Trim();

        if (string.IsNullOrWhiteSpace(_empID))
        {
            errTxt += "未選擇人員\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        #region -- 執行新增/更新 --

        //----- 宣告:資料參數 -----
        DeptsRepository _data = new DeptsRepository();

        //----- 設定:資料欄位 -----
        var data = new PKDept_Supervisor
        {
            DeptID = Req_DataID,
            AccountName = _empID
        };

        //----- 方法:新增資料 -----
        if (!_data.Create_Supervisor(data, out ErrMsg))
        {
            ph_ErrMessage.Visible = true;
            lt_ShowMsg.Text = ErrMsg;
            CustomExtension.AlertMsg("設定失敗", "");
            return;
        }
        else
        {
            ph_ErrMessage.Visible = false;

            //導向本頁
            Response.Redirect(thisPage + "#section1");
        }

        #endregion
    }

    #endregion


    #region -- 資料顯示:部門成員 --

    /// <summary>
    /// 顯示部門成員
    /// </summary>
    private void LookupData_UserList()
    {
        //----- 宣告:資料參數 -----
        UsersRepository _data = new UsersRepository();
        Dictionary<int, string> depts = new Dictionary<int, string>();

        try
        {
            //----- 原始資料:條件篩選 -----
            depts.Add(1, Req_DataID);

            //----- 原始資料:取得所有資料 -----
            var query = _data.GetUsers(null, depts);

            //----- 資料整理:繫結 ----- 
            lv_Emplist.DataSource = query;
            lv_Emplist.DataBind();

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
    /// 取得此功能的前置路徑
    /// </summary>
    /// <returns></returns>
    public string FuncPath()
    {
        return "{0}{1}/Depts/".FormatThis(
            fn_Param.WebUrl
            , Req_Lang);
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
            return "{0}/Edit/{1}".FormatThis(FuncPath(), Req_DataID);
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
            return FuncPath();
        }
        set
        {
            _Page_SearchUrl = value;
        }
    }

    #endregion

}
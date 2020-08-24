using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using DeliveryData.Controllers;
using Menu3000Data.Models;
using PKLib_Data.Controllers;
using PKLib_Method.Methods;

public partial class myDelivery_Edit : SecurityCheck
{
    public string ErrMsg;
    public bool masterAuth = false; //管理者權限(可在權限設定裡勾選)


    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            try
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;

                isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3786");

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }
                masterAuth = fn_CheckAuth.Check(fn_Param.CurrentUser, "3787");

                //[權限判斷] End
                #endregion

                if (!IsPostBack)
                {
                    //產生選單
                    GetClassMenu("1", ddl_ShipWay, "請選擇");
                    GetClassMenu("2", ddl_PayWay, "請選擇");
                    GetClassMenu("3", ddl_BoxClass1, "請選擇");
                    GetClassMenu("4", ddl_BoxClass2, "請選擇");
                    GetClassMenu("5", ddl_TargetClass, "請選擇");


                    //[參數判斷] - 資料編號
                    if (Req_DataID.Equals("new"))
                    {
                        //登記人員預設值 (目前使用者)
                        UsersRepository _user = new UsersRepository();
                        var getUser = _user.GetOne(fn_Param.CurrentUserAccount).FirstOrDefault();
                        filter_Emp.Text = getUser.ProfID;
                        lb_Emp.Text = getUser.ProfName;
                        val_Emp.Text = getUser.ProfID;
                        _user = null;

                        //指定寄件日
                        tb_SendDate.Text = DateTime.Today.ToShortDateString();
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
    }

    #region -- 資料顯示 --

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        try
        {
            //----- 原始資料:取得所有資料 -----
            search.Add("DataID", Req_DataID);

            var query = _data.GetOne(search, out ErrMsg).FirstOrDefault();

            //----- 資料整理:繫結 ----- 
            if (query == null)
            {
                CustomExtension.AlertMsg("無法取得資料,即將返回列表頁.", Page_SearchUrl);
                return;
            }


            #region >> 欄位填寫 <<
            hf_DataID.Value = query.Data_ID.ToString();
            lb_TraceID.Text = query.TraceID;
            rbl_ShipType.SelectedValue = query.ShipType.ToString();
            ddl_ShipWay.SelectedValue = query.ShipWay.ToString();
            ddl_PayWay.SelectedValue = query.PayWay.ToString();

            filter_Emp.Text = query.ShipWho;
            val_Emp.Text = query.ShipWho;
            lb_Emp.Text = query.ShipWho_Name;

            ddl_BoxClass1.SelectedValue = query.BoxClass1.ToString();
            ddl_BoxClass2.SelectedValue = query.BoxClass2.ToString();
            ddl_TargetClass.SelectedValue = query.TargetClass.ToString();

            tb_SendDate.Text = query.SendDate.ToDateString("yyyy/MM/dd");
            tb_SendComp.Text = query.SendComp;
            show_SendWho.Text = query.SendWho;
            tb_SendWho.Text = query.SendWho;
            tb_SendAddr.Text = query.SendAddr;
            tb_SendTel1.Text = query.SendTel1;
            tb_SendTel2.Text = query.SendTel2;
            tb_ShipNo.Text = query.ShipNo;
            tb_ShipPay.Text = query.ShipPay.ToString();
            tb_Box.Text = query.Box.ToString();
            tb_Remark1.Text = query.Remark1;
            tb_Remark2.Text = query.Remark2;
            tb_PurNo.Text = query.PurNo;
            tb_SaleNo.Text = query.SaleNo;
            tb_InvoiceNo.Text = query.InvoiceNo;

            #endregion

            //show/hide
            btn_Close.Visible = masterAuth;

            //判斷是否結案, Y=無管理權限的人要跳走
            if (query.IsClose.Equals("Y"))
            {
                if (!masterAuth)
                {
                    Response.Redirect(Page_SearchUrl);
                }
            }


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


    #region -- 資料編輯 --

    /// <summary>
    /// 資料新增
    /// </summary>
    /// <param name="_type">1:留在本頁/ 2:回列表</param>
    private void Add_Data(string _type)
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();

        try
        {
            //----- 設定:資料欄位 -----
            //產生Guid
            string guid = CustomExtension.GetGuid();

            Int16 _ShipType = Convert.ToInt16(rbl_ShipType.SelectedValue);
            Int32 _ShipWay = Convert.ToInt32(ddl_ShipWay.SelectedValue);
            Int32 _PayWay = Convert.ToInt32(ddl_PayWay.SelectedValue);
            string _ShipWho = val_Emp.Text;
            string _SendDate = tb_SendDate.Text;
            string _SendComp = tb_SendComp.Text;
            string _SendWho = string.IsNullOrWhiteSpace(tb_SendWho.Text) ? show_SendWho.Text.Trim() : tb_SendWho.Text;
            string _SendAddr = tb_SendAddr.Text;
            string _SendTel1 = tb_SendTel1.Text;
            string _SendTel2 = tb_SendTel2.Text;
            string _ShipNo = tb_ShipNo.Text;
            string _ShipPay = tb_ShipPay.Text;
            string _Box = tb_Box.Text;
            string _BoxClass1 = ddl_BoxClass1.SelectedValue;
            string _BoxClass2 = ddl_BoxClass2.SelectedValue;
            string _TargetClass = ddl_TargetClass.SelectedValue;
            string _Remark1 = tb_Remark1.Text;
            string _Remark2 = tb_Remark2.Text;
            string _PurNo = tb_PurNo.Text;
            string _SaleNo = tb_SaleNo.Text;
            string _InvoiceNo = tb_InvoiceNo.Text;

            //instance
            var data = new Delivery_Base
            {
                Data_ID = new Guid(guid),
                ShipType = _ShipType,
                ShipWay = _ShipWay,
                PayWay = _PayWay,
                ShipWho = _ShipWho,
                SendDate = _SendDate,
                SendComp = _SendComp,
                SendWho = _SendWho,
                SendAddr = _SendAddr,
                SendTel1 = _SendTel1,
                SendTel2 = _SendTel2,
                ShipNo = _ShipNo,
                ShipPay = string.IsNullOrWhiteSpace(_ShipPay) ? 0 : Convert.ToDouble(_ShipPay),
                Box = string.IsNullOrWhiteSpace(_Box) ? 0 : Convert.ToInt32(_Box),
                BoxClass1 = string.IsNullOrWhiteSpace(_BoxClass1) ? 0 : Convert.ToInt32(_BoxClass1),
                BoxClass2 = string.IsNullOrWhiteSpace(_BoxClass2) ? 0 : Convert.ToInt32(_BoxClass2),
                TargetClass = string.IsNullOrWhiteSpace(_TargetClass) ? 0 : Convert.ToInt32(_TargetClass),
                Remark1 = _Remark1,
                Remark2 = _Remark2,
                PurNo = _PurNo,
                SaleNo = _SaleNo,
                InvoiceNo = _InvoiceNo
            };


            //----- 方法:新增資料 -----
            if (!_data.Create(data, out ErrMsg))
            {
                ph_ErrMessage.Visible = true;
                lt_ShowMsg.Text = ErrMsg;
                CustomExtension.AlertMsg("新增失敗", "");
                return;
            }
            else
            {
                if (_type.Equals("1"))
                {
                    //導向本頁
                    string url = FuncPath() + "/Edit/" + guid;
                    Response.Redirect(url);
                }
                else
                {
                    //導向列表頁
                    Response.Redirect(Page_SearchUrl);
                }
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
    /// <param name="_type">1:留在本頁/ 2:回列表</param>
    private void Edit_Data(string _type)
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();

        try
        {
            //----- 設定:資料欄位 -----
            string _dataID = hf_DataID.Value;

            Int16 _ShipType = Convert.ToInt16(rbl_ShipType.SelectedValue);
            Int32 _ShipWay = Convert.ToInt32(ddl_ShipWay.SelectedValue);
            Int32 _PayWay = Convert.ToInt32(ddl_PayWay.SelectedValue);
            string _ShipWho = val_Emp.Text;
            string _SendDate = tb_SendDate.Text;
            string _SendComp = tb_SendComp.Text;
            string _SendWho = string.IsNullOrWhiteSpace(tb_SendWho.Text) ? show_SendWho.Text.Trim() : tb_SendWho.Text;
            string _SendAddr = tb_SendAddr.Text;
            string _SendTel1 = tb_SendTel1.Text;
            string _SendTel2 = tb_SendTel2.Text;
            string _ShipNo = tb_ShipNo.Text;
            string _ShipPay = tb_ShipPay.Text;
            string _Box = tb_Box.Text;
            string _BoxClass1 = ddl_BoxClass1.SelectedValue;
            string _BoxClass2 = ddl_BoxClass2.SelectedValue;
            string _TargetClass = ddl_TargetClass.SelectedValue;
            string _Remark1 = tb_Remark1.Text;
            string _Remark2 = tb_Remark2.Text;
            string _PurNo = tb_PurNo.Text;
            string _SaleNo = tb_SaleNo.Text;
            string _InvoiceNo = tb_InvoiceNo.Text;

            //instance
            var data = new Delivery_Base
            {
                ShipType = _ShipType,
                ShipWay = _ShipWay,
                PayWay = _PayWay,
                ShipWho = _ShipWho,
                SendDate = _SendDate,
                SendComp = _SendComp,
                SendWho = _SendWho,
                SendAddr = _SendAddr,
                SendTel1 = _SendTel1,
                SendTel2 = _SendTel2,
                ShipNo = _ShipNo,
                ShipPay = string.IsNullOrWhiteSpace(_ShipPay) ? 0 : Convert.ToDouble(_ShipPay),
                Box = string.IsNullOrWhiteSpace(_Box) ? 0 : Convert.ToInt32(_Box),
                BoxClass1 = string.IsNullOrWhiteSpace(_BoxClass1) ? 0 : Convert.ToInt32(_BoxClass1),
                BoxClass2 = string.IsNullOrWhiteSpace(_BoxClass2) ? 0 : Convert.ToInt32(_BoxClass2),
                TargetClass = string.IsNullOrWhiteSpace(_TargetClass) ? 0 : Convert.ToInt32(_TargetClass),
                Remark1 = _Remark1,
                Remark2 = _Remark2,
                PurNo = _PurNo,
                SaleNo = _SaleNo,
                InvoiceNo = _InvoiceNo
            };

            //----- 方法:更新資料 -----
            if (!_data.Update(_dataID, data, out ErrMsg))
            {
                ph_ErrMessage.Visible = true;
                lt_ShowMsg.Text = ErrMsg;
                CustomExtension.AlertMsg("更新失敗", thisPage);
                return;
            }
            else
            {
                if (_type.Equals("1"))
                {
                    //導向本頁
                    Response.Redirect(thisPage);
                }
                else
                {
                    //導向列表頁
                    Response.Redirect(Page_SearchUrl);
                }

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


    #region -- 按鈕事件 --

    protected void btn_Save_Click(object sender, EventArgs e)
    {
        doSave("2");
    }
    protected void btn_SaveStay_Click(object sender, EventArgs e)
    {
        doSave("1");
    }

    /// <summary>
    /// SAVE
    /// </summary>
    /// <param name="_type">1:留在本頁/ 2:回列表</param>
    private void doSave(string _type)
    {
        string errTxt = "";

        if (string.IsNullOrWhiteSpace(rbl_ShipType.SelectedValue))
        {
            errTxt += "類別\\n";
        }
        if (string.IsNullOrWhiteSpace(ddl_ShipWay.SelectedValue))
        {
            errTxt += "寄送方式\\n";
        }
        if (string.IsNullOrWhiteSpace(ddl_PayWay.SelectedValue))
        {
            errTxt += "運費付款方式\\n";
        }
        if (string.IsNullOrWhiteSpace(val_Emp.Text))
        {
            errTxt += "登記人員\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg("=== 請檢查以下欄位 ===\\n" + errTxt, "");
            return;
        }

        /* 執行新增/更新 */
        if (string.IsNullOrEmpty(hf_DataID.Value))
        {
            Add_Data(_type);
        }
        else
        {
            Edit_Data(_type);
        }
    }


    /// <summary>
    /// 設為結案
    /// </summary>
    protected void btn_Close_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();

        try
        {
            //----- 設定:資料欄位 -----
            string _dataID = hf_DataID.Value;

            //----- 方法:更新資料 -----
            if (!_data.Update_Status(_dataID, out ErrMsg))
            {
                ph_ErrMessage.Visible = true;
                lt_ShowMsg.Text = ErrMsg;
                CustomExtension.AlertMsg("更新失敗", thisPage);
                return;
            }

            //導向列表頁
            Response.Redirect(Page_SearchUrl);
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
    /// 取得下拉清單
    /// </summary>
    private void GetClassMenu(string type, DropDownList ddl, string rootName)
    {
        //----- 宣告:資料參數 -----
        DeliveryRepository _data = new DeliveryRepository();

        try
        {
            //取得資料
            var data = _data.GetRefClass(type, out ErrMsg);

            //----- 資料整理 -----
            ddl.Items.Clear();

            if (!string.IsNullOrEmpty(rootName))
            {
                ddl.Items.Add(new ListItem(rootName, ""));
            }

            foreach (var item in data)
            {
                ddl.Items.Add(new ListItem(item.Label, item.ID.ToString()));
            }

            //release
            data = null;

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
        return "{0}{1}/{2}/Delivery".FormatThis(
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
            this._Req_DataID = value;
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
            this._thisPage = value;
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
            string tempUrl = CustomExtension.getCookie("Delivery");

            return string.IsNullOrWhiteSpace(tempUrl) ? FuncPath() : Server.UrlDecode(tempUrl);
        }
        set
        {
            this._Page_SearchUrl = value;
        }
    }

    #endregion

}
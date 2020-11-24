using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;
using ShipFreight_CN.Models;
using ShipFreight_CN.Controllers;

public partial class myShipping_Search : SecurityCheck
{
    public string ErrMsg;
    public bool isAdmin = false;
    public IQueryable<ClassItem> _shipItem;
    public IQueryable<ClassItem> _freightItem;
    public IQueryable<ClassItem> _compItem;

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                #region --權限--
                //[權限判斷] Start
                bool isPass = false;

                //A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具
                switch (Req_DataType)
                {
                    case "A":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3775");
                        isAdmin = fn_CheckAuth.Check(fn_Param.CurrentUser, "377501");
                        break;

                    case "B":
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3776");
                        isAdmin = fn_CheckAuth.Check(fn_Param.CurrentUser, "377601");
                        break;

                    case "C":
                        //工具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3777");
                        isAdmin = fn_CheckAuth.Check(fn_Param.CurrentUser, "377701");
                        break;

                    case "D":
                        //玩具
                        isPass = fn_CheckAuth.Check(fn_Param.CurrentUser, "3778");
                        isAdmin = fn_CheckAuth.Check(fn_Param.CurrentUser, "377801");
                        break;
                }

                if (!isPass)
                {
                    Response.Redirect("{0}Error/您無使用權限".FormatThis(fn_Param.WebUrl));
                    return;
                }

                //取得公司別
                string _corpName = "中國內銷({0})".FormatThis(fn_Menu.GetShipping_RefType(Req_DataType));
                lt_CorpName.Text = _corpName;
                Page.Title += "-" + _corpName;

                //[權限判斷] End
                #endregion

                /*
                    [預先載入-下拉選單]
                    載入表格前,先取得下拉清單
                    1:貨運公司, 2:物流途徑, 3:運費方式
                */
                _shipItem = GetClassMenu("2");
                _freightItem = GetClassMenu("3");
                _compItem = GetClassMenu("1");


                /*
                    [查詢區選單]
                */
                SetClassMenu(filter_ShipWay, _shipItem, Req_Way, "-- 全部 --");
                SetClassMenu(filter_FreightWay, _freightItem, Req_FreightWay, "-- 全部 --");
                SetClassMenu(filter_ShipComp, _compItem, Req_ShipComp, "-- 全部 --");

                #region --Request參數--
                //[取得/檢查參數] - Req_sDate
                if (!string.IsNullOrWhiteSpace(Req_sDate))
                {
                    this.filter_sDate.Text = Req_sDate;
                }
                //[取得/檢查參數] - Req_eDate
                if (!string.IsNullOrWhiteSpace(Req_eDate))
                {
                    this.filter_eDate.Text = Req_eDate;
                }

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
                //[取得/檢查參數] - 關鍵字查詢
                if (!string.IsNullOrWhiteSpace(Req_Keyword))
                {
                    this.filter_Keyword.Text = Req_Keyword;
                }
                //[取得/檢查參數] - 客戶查詢
                if (!string.IsNullOrWhiteSpace(Req_Cust))
                {
                    this.filter_Cust.Text = Req_Cust;
                }

                //[取得/檢查參數] - 運費方式
                if (!string.IsNullOrWhiteSpace(Req_FreightWay))
                {
                    this.filter_FreightWay.SelectedIndex = this.filter_FreightWay.Items.IndexOf(this.filter_FreightWay.Items.FindByValue(Req_FreightWay));
                }

                //[取得/檢查參數] - 物流途徑
                if (!string.IsNullOrWhiteSpace(Req_Way))
                {
                    this.filter_ShipWay.SelectedIndex = this.filter_ShipWay.Items.IndexOf(this.filter_ShipWay.Items.FindByValue(Req_Way));
                }

                //[取得/檢查參數] - 貨運公司
                if (!string.IsNullOrWhiteSpace(Req_ShipComp))
                {
                    this.filter_ShipComp.SelectedIndex = this.filter_ShipComp.Items.IndexOf(this.filter_ShipComp.Items.FindByValue(Req_ShipComp));
                }

                #endregion


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
        int RecordsPerPage = 10;    //每頁筆數
        int StartRow = (pageIndex - 1) * RecordsPerPage;    //第n筆開始顯示
        int TotalRow = 0;   //總筆數
        int DataCnt = 0;
        ArrayList PageParam = new ArrayList();  //條件參數,for pager

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();


        //----- 原始資料:條件篩選 -----

        #region >> 條件篩選 <<

        //固定條件:TOP選單/資料判別
        PageParam.Add("dt=" + Server.UrlEncode(Req_DataType));


        //[取得/檢查參數] - Date (需轉為ERP格式)
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate.ToDateString("yyyyMMdd"));

            PageParam.Add("sDate=" + Server.UrlEncode(Req_sDate));
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate.ToDateString("yyyyMMdd"));

            PageParam.Add("eDate=" + Server.UrlEncode(Req_eDate));
        }

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

        //[取得/檢查參數] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);

            PageParam.Add("k=" + Server.UrlEncode(Req_Keyword));
        }

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrWhiteSpace(Req_Cust))
        {
            search.Add("Cust", Req_Cust);

            PageParam.Add("Cust=" + Server.UrlEncode(Req_Cust));
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

        //[取得/檢查參數] - FW
        if (!string.IsNullOrWhiteSpace(Req_FreightWay))
        {
            if (!Req_FreightWay.Equals("ALL"))
            {
                search.Add("FreightWay", Req_FreightWay);
            }

            PageParam.Add("fw=" + Server.UrlEncode(Req_FreightWay));
        }

        //[取得/檢查參數] - ShipComp
        if (!string.IsNullOrWhiteSpace(Req_ShipComp))
        {
            search.Add("ShipComp", Req_ShipComp);
            PageParam.Add("ShipComp=" + Server.UrlEncode(Req_ShipComp));
        }

        //尾碼
        PageParam.Add("t={0}#formData".FormatThis(Cryptograph.GetCurrentTime()));

        #endregion

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipData(search, Req_DataType.ToUpper(), StartRow, RecordsPerPage, out DataCnt, out ErrMsg);


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
            string getPager = CustomExtension.Pagination(TotalRow, RecordsPerPage, pageIndex, 5
                , FuncPath(), PageParam, false, true);
            lt_Pager.Text = getPager;

        }
    }


    protected void lvDataList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        //取得Key值
        string Get_DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                List<ShipFreightItem> dataList = new List<ShipFreightItem>();

                //取得Key值
                string dataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

                #region ** 取得欄位資料 **

                string _DataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;
                string _SO_FID = ((HiddenField)e.Item.FindControl("hf_SO_FID")).Value;
                string _SO_SID = ((HiddenField)e.Item.FindControl("hf_SO_SID")).Value;
                string _ShipComp = ((DropDownList)e.Item.FindControl("lst_ShipComp")).SelectedValue;
                string _FreightWay = ((DropDownList)e.Item.FindControl("lst_FreightWay")).SelectedValue;
                string _ShipWay = ((DropDownList)e.Item.FindControl("lst_ShipWay")).SelectedValue;
                string _ShipDate = ((TextBox)e.Item.FindControl("tb_ShipDate")).Text;
                string _ShipNo = ((TextBox)e.Item.FindControl("tb_ShipNo")).Text;
                string _ShipWho = ((TextBox)e.Item.FindControl("tb_ShipWho")).Text;
                string _ShipTel = ((TextBox)e.Item.FindControl("tb_ShipTel")).Text;
                string _ShipAddr1 = ((TextBox)e.Item.FindControl("tb_ShipAddr1")).Text;
                string _ShipAddr2 = ((TextBox)e.Item.FindControl("tb_ShipAddr2")).Text;
                string _BoxCnt = ((TextBox)e.Item.FindControl("tb_BoxCnt")).Text;
                string _Freight = ((TextBox)e.Item.FindControl("tb_Freight")).Text;
                string _Remark = ((TextBox)e.Item.FindControl("tb_Remark")).Text;
                string _UserCheck1 = ((HiddenField)e.Item.FindControl("hf_UserCheck1")).Value;
                string _OldCheckTime1 = ((HiddenField)e.Item.FindControl("hf_OldCheckTime1")).Value;

                #endregion
                var dataItem = new ShipFreightItem();

                //Switch 
                switch (e.CommandName.ToUpper())
                {
                    case "DOCLOSE":
                        //----- 方法:刪除資料 -----
                        string url = filterUrl(1);
                        if (false == _data.Delete(Get_DataID))
                        {
                            CustomExtension.AlertMsg("刪除失敗", url);
                            return;
                        }
                        else
                        {
                            //導向本頁
                            Response.Redirect(url + "#formData");
                        }

                        break;

                    case "DOSAVE":
                        //*** 存檔 ****
                        //將值填入容器
                        dataItem = new ShipFreightItem
                        {
                            Data_ID = string.IsNullOrWhiteSpace(_DataID) ? new Guid(CustomExtension.GetGuid()) : new Guid(_DataID),
                            Erp_SO_FID = _SO_FID,
                            Erp_SO_SID = _SO_SID,
                            ShipNo = _ShipNo,
                            ShipWho = _ShipWho,
                            ShipTel = _ShipTel,
                            ShipAddr1 = _ShipAddr1,
                            ShipAddr2 = _ShipAddr2,
                            BoxCnt = string.IsNullOrWhiteSpace(_BoxCnt) ? 0 : Convert.ToInt16(_BoxCnt),
                            Freight = string.IsNullOrWhiteSpace(_Freight) ? 0 : Convert.ToDouble(_Freight),
                            ShipDate = _ShipDate.ToDateString("yyyy/MM/dd"),
                            ShipComp = string.IsNullOrWhiteSpace(_ShipComp) ? 0 : Convert.ToUInt16(_ShipComp),
                            ShipWay = string.IsNullOrWhiteSpace(_ShipWay) ? 0 : Convert.ToUInt16(_ShipWay),
                            SendType = string.IsNullOrWhiteSpace(_FreightWay) ? 0 : Convert.ToUInt16(_FreightWay),
                            Remark = _Remark,
                            UserCheck1 = _UserCheck1,
                            Check_Time1 = string.IsNullOrWhiteSpace(_OldCheckTime1) ? "" : _OldCheckTime1,
                            Create_Who = fn_Param.CurrentUser
                        };

                        //add to list
                        dataList.Add(dataItem);

                        //Call function
                        if (!_data.Update_ShipData(dataList, out ErrMsg))
                        {
                            //Response.Write(ErrMsg);
                            CustomExtension.AlertMsg("資料儲存失敗...", "");
                            return;
                        }

                        //redirect page
                        Response.Redirect(filterUrl(Req_PageIdx) + "#formData");

                        break;



                    case "DOCHECK_YES":
                        //*** 設為確認 ****
                        dataItem = new ShipFreightItem
                        {
                            Data_ID = string.IsNullOrWhiteSpace(_DataID) ? new Guid(CustomExtension.GetGuid()) : new Guid(_DataID),
                            Erp_SO_FID = _SO_FID,
                            Erp_SO_SID = _SO_SID,
                            ShipNo = _ShipNo,
                            ShipWho = _ShipWho,
                            ShipTel = _ShipTel,
                            ShipAddr1 = _ShipAddr1,
                            ShipAddr2 = _ShipAddr2,
                            BoxCnt = string.IsNullOrWhiteSpace(_BoxCnt) ? 0 : Convert.ToInt16(_BoxCnt),
                            Freight = string.IsNullOrWhiteSpace(_Freight) ? 0 : Convert.ToDouble(_Freight),
                            ShipDate = _ShipDate.ToDateString("yyyy/MM/dd"),
                            ShipComp = string.IsNullOrWhiteSpace(_ShipComp) ? 0 : Convert.ToUInt16(_ShipComp),
                            ShipWay = string.IsNullOrWhiteSpace(_ShipWay) ? 0 : Convert.ToUInt16(_ShipWay),
                            SendType = string.IsNullOrWhiteSpace(_FreightWay) ? 0 : Convert.ToUInt16(_FreightWay),
                            Remark = _Remark,
                            UserCheck1 = "Y",
                            Check_Time1 = DateTime.Now.ToString().ToDateString("yyyy/MM/dd HH:mm:ss"),
                            Create_Who = fn_Param.CurrentUser
                        };

                        //add to list
                        dataList.Add(dataItem);

                        //Call function
                        if (!_data.Update_ShipData(dataList, out ErrMsg))
                        {
                            //Response.Write(ErrMsg);
                            CustomExtension.AlertMsg("資料儲存失敗...", "");
                            return;
                        }

                        //redirect page
                        Response.Redirect(filterUrl(Req_PageIdx) + "#formData");

                        break;


                    case "DOCHECK_NO":
                        //*** 移除確認 ****
                        dataItem = new ShipFreightItem
                        {
                            Data_ID = string.IsNullOrWhiteSpace(_DataID) ? new Guid(CustomExtension.GetGuid()) : new Guid(_DataID),
                            Erp_SO_FID = _SO_FID,
                            Erp_SO_SID = _SO_SID,
                            ShipNo = _ShipNo,
                            ShipWho = _ShipWho,
                            ShipTel = _ShipTel,
                            ShipAddr1 = _ShipAddr1,
                            ShipAddr2 = _ShipAddr2,
                            BoxCnt = string.IsNullOrWhiteSpace(_BoxCnt) ? 0 : Convert.ToInt16(_BoxCnt),
                            Freight = string.IsNullOrWhiteSpace(_Freight) ? 0 : Convert.ToDouble(_Freight),
                            ShipDate = _ShipDate.ToDateString("yyyy/MM/dd"),
                            ShipComp = string.IsNullOrWhiteSpace(_ShipComp) ? 0 : Convert.ToUInt16(_ShipComp),
                            ShipWay = string.IsNullOrWhiteSpace(_ShipWay) ? 0 : Convert.ToUInt16(_ShipWay),
                            SendType = string.IsNullOrWhiteSpace(_FreightWay) ? 0 : Convert.ToUInt16(_FreightWay),
                            Remark = _Remark,
                            UserCheck1 = "N",
                            Check_Time1 = "",
                            Create_Who = fn_Param.CurrentUser
                        };

                        //add to list
                        dataList.Add(dataItem);

                        //Call function
                        if (!_data.Update_ShipData(dataList, out ErrMsg))
                        {
                            //Response.Write(ErrMsg);
                            CustomExtension.AlertMsg("資料儲存失敗...", "");
                            return;
                        }

                        //redirect page
                        Response.Redirect(filterUrl(Req_PageIdx) + "#formData");

                        break;

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

    protected void lvDataList_ItemDataBound(object sender, ListViewItemEventArgs e)
    {
        try
        {
            if (e.Item.ItemType == ListViewItemType.DataItem)
            {
                ListViewDataItem dataItem = (ListViewDataItem)e.Item;

                //ID
                string _DataID = DataBinder.Eval(dataItem.DataItem, "Data_ID") == null ? "" : DataBinder.Eval(dataItem.DataItem, "Data_ID").ToString();

                //取得共用控制項
                PlaceHolder ph_Save = (PlaceHolder)e.Item.FindControl("ph_Save");
                PlaceHolder ph_Del = (PlaceHolder)e.Item.FindControl("ph_Del");
                ph_Del.Visible = false;


                //各欄位控制項
                DropDownList lst_ShipComp = (DropDownList)e.Item.FindControl("lst_ShipComp");
                DropDownList lst_FreightWay = (DropDownList)e.Item.FindControl("lst_FreightWay");
                DropDownList lst_ShipWay = (DropDownList)e.Item.FindControl("lst_ShipWay");
                TextBox tb_ShipDate = (TextBox)e.Item.FindControl("tb_ShipDate");
                TextBox tb_ShipNo = (TextBox)e.Item.FindControl("tb_ShipNo");
                TextBox tb_ShipWho = (TextBox)e.Item.FindControl("tb_ShipWho");
                TextBox tb_ShipTel = (TextBox)e.Item.FindControl("tb_ShipTel");
                TextBox tb_ShipAddr1 = (TextBox)e.Item.FindControl("tb_ShipAddr1");
                TextBox tb_ShipAddr2 = (TextBox)e.Item.FindControl("tb_ShipAddr2");
                TextBox tb_BoxCnt = (TextBox)e.Item.FindControl("tb_BoxCnt");
                TextBox tb_Freight = (TextBox)e.Item.FindControl("tb_Freight");
                LinkButton lbtn_CheckY = (LinkButton)e.Item.FindControl("lbtn_CheckY"); //設為y, 目前為n
                LinkButton lbtn_CheckN = (LinkButton)e.Item.FindControl("lbtn_CheckN"); //設為n, 目前為y


                #region >> 下拉選單處理 <<

                //-- 貨運公司 --
                Int32 _ShipComp = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "ShipComp")); //取得設定值
                SetClassMenu(lst_ShipComp, _compItem, _ShipComp == 0 ? "1" : _ShipComp.ToString(), "請選擇");


                //-- 運費方式 --
                Int32 _SendType = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "SendType")); //取得設定值
                SetClassMenu(lst_FreightWay, _freightItem, _SendType == 0 ? "10" : _SendType.ToString(), "請選擇");


                //-- 物流途徑 --
                Int32 _ShipWayD = Convert.ToInt32(DataBinder.Eval(dataItem.DataItem, "ShipWay")); //取得設定值
                SetClassMenu(lst_ShipWay, _shipItem, _ShipWayD == 0 ? "8" : _ShipWayD.ToString(), "請選擇");

                #endregion


                #region >> 判斷資材確認 <<

                bool showCheck = false;
                lbtn_CheckN.Visible = false;

                //檢查是否有資料
                var _usrChk1Data = DataBinder.Eval(dataItem.DataItem, "UserCheck1");
                if (_usrChk1Data != null)
                {
                    showCheck = true;
                }

                if (showCheck)
                {
                    //資材確認設定(Y/N)
                    string _Chk = DataBinder.Eval(dataItem.DataItem, "UserCheck1").ToString();
                    lbtn_CheckY.Visible = !_Chk.Equals("Y");
                    lbtn_CheckN.Visible = _Chk.Equals("Y");

                }

                /* 暫不鎖定 2020-11-19 */
                /*
                if (showCheck)
                {
                    //資材確認設定(Y/N)
                    string _Chk = DataBinder.Eval(dataItem.DataItem, "UserCheck1").ToString();
                    bool _isLock = _Chk.Equals("Y");
                    lst_Check.Checked = _isLock; //ischeckd

                    //物流單號
                    string _ShipNo = DataBinder.Eval(dataItem.DataItem, "ShipNo").ToString();
                    //運費
                    double _Freight = Convert.ToDouble(DataBinder.Eval(dataItem.DataItem, "Freight"));

                    //判斷鎖定:確認=Y + 物流單號 + 運費
                    if (_Chk.Equals("Y") && !string.IsNullOrWhiteSpace(_ShipNo) && _Freight > 0)
                    {
                        //資材已確認,不可修改                        
                        lst_Check.Enabled = false;
                        ph_Save.Visible = false;
                    }
                    else
                    {
                        ph_Save.Visible = true;
                    }

                    //欄位是否鎖定 
                    lst_ShipComp.Enabled = _isLock;
                    lst_FreightWay.Enabled = _isLock;
                    lst_ShipWay.Enabled = _isLock;
                    tb_ShipDate.Enabled = _isLock;
                    tb_ShipNo.Enabled = _isLock;
                    tb_ShipWho.Enabled = _isLock;
                    tb_ShipTel.Enabled = _isLock;
                    tb_ShipAddr1.Enabled = _isLock;
                    tb_ShipAddr2.Enabled = _isLock;
                    tb_BoxCnt.Enabled = _isLock;
                    tb_Freight.Enabled = _isLock;
                }
                */
                #endregion


                #region >> 管理者判斷 <<

                if (isAdmin)
                {
                    //刪除鈕
                    ph_Del.Visible = !string.IsNullOrWhiteSpace(_DataID);
                    //儲存鈕
                    ph_Save.Visible = true;
                }

                #endregion


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
        //執行查詢
        Response.Redirect(filterUrl(0), false);
    }


    /// <summary>
    /// [按鈕] - 匯出
    /// </summary>
    protected void lbtn_Excel_Click(object sender, EventArgs e)
    {

        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        Dictionary<string, string> sort = new Dictionary<string, string>();
        DataTable DT = new DataTable();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<

        //[取得/檢查參數] - Date (需轉為ERP格式)
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate.ToDateString("yyyyMMdd"));
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate.ToDateString("yyyyMMdd"));
        }

        //[取得/檢查參數] - ShipDate
        if (!string.IsNullOrWhiteSpace(Req_sDate_Ship))
        {
            search.Add("ShipsDate", Req_sDate_Ship);
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate_Ship))
        {
            search.Add("ShipeDate", Req_eDate_Ship);
        }

        //[取得/檢查參數] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);
        }

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrWhiteSpace(Req_Cust))
        {
            search.Add("Cust", Req_Cust);
        }

        //[取得/檢查參數] - Way
        if (!string.IsNullOrWhiteSpace(Req_Way))
        {
            if (!Req_Way.Equals("ALL"))
            {
                search.Add("Way", Req_Way);
            }
        }

        //[取得/檢查參數] - FW
        if (!string.IsNullOrWhiteSpace(Req_FreightWay))
        {
            if (!Req_FreightWay.Equals("ALL"))
            {
                search.Add("fw", Req_FreightWay);
            }
        }

        //[取得/檢查參數] - ShipComp
        if (!string.IsNullOrWhiteSpace(Req_ShipComp))
        {
            search.Add("ShipComp", Req_ShipComp);
        }

        #endregion

        //----- 方法:取得資料 -----
        var _rowData = _data.GetAllShipData(search, Req_DataType.ToUpper(), out ErrMsg);

        if (_rowData.Count() == 0)
        {
            CustomExtension.AlertMsg("目前條件查不到資料.", "");
            return;
        }

        //object to datatable
        DataTable myDT = CustomExtension.LINQToDataTable(_rowData);

        #region ** 填入指定欄位 **

        Dictionary<string, string> _col = new Dictionary<string, string>();
        _col.Add("Erp_SO_Date", "銷貨日期");
        _col.Add("UserCheck1", "資材確認");
        _col.Add("ShipDate", "發貨日期");
        _col.Add("CustName", "客戶");
        _col.Add("Erp_SO_FullID", "銷貨單號");
        _col.Add("TotalPrice", "銷貨金額");
        _col.Add("CfmCode", "銷貨單確認");
        _col.Add("ShipCompName", "貨運公司");
        _col.Add("ShipNo", "物流單號");
        _col.Add("SendTypeName", "運費方式");
        _col.Add("ShipWayName", "物流途徑");
        _col.Add("BoxCnt", "件數");
        _col.Add("Freight", "運費金額");
        _col.Add("ShipWho", "收件人");
        _col.Add("ShipTel", "收件電話");
        _col.Add("ShipAddr1", "收件地址1");
        _col.Add("ShipAddr2", "收件地址2");
        _col.Add("CfmWhoName", "銷售員");
        _col.Add("Remark", "備註");
        _col.Add("Check_Time1", "確認時間");


        //將指定的欄位,轉成陣列
        string[] selectedColumns = _col.Keys.ToArray();

        //資料複製到新的Table(內容為指定的欄位資料)
        DataTable newDT = new DataView(myDT).ToTable(true, selectedColumns);


        #endregion


        #region ** 重新命名欄位,顯示為中文 **

        foreach (var item in _col)
        {
            string _id = item.Key;
            string _name = item.Value;

            newDT.Columns[_id].ColumnName = _name;

        }
        #endregion

        //匯出Excel
        CustomExtension.ExportExcel(
            newDT
            , "ExcelData-CHN-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);

    }


    protected void lbtn_ShipExcel_Click(object sender, EventArgs e)
    {
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        Dictionary<string, string> sort = new Dictionary<string, string>();
        DataTable DT = new DataTable();

        //----- 原始資料:條件篩選 -----
        #region >> 條件篩選 <<

        //[取得/檢查參數] - Date (需轉為ERP格式)
        if (!string.IsNullOrWhiteSpace(Req_sDate))
        {
            search.Add("sDate", Req_sDate.ToDateString("yyyyMMdd"));
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate))
        {
            search.Add("eDate", Req_eDate.ToDateString("yyyyMMdd"));
        }

        //[取得/檢查參數] - ShipDate
        if (!string.IsNullOrWhiteSpace(Req_sDate_Ship))
        {
            search.Add("ShipsDate", Req_sDate_Ship);
        }
        if (!string.IsNullOrWhiteSpace(Req_eDate_Ship))
        {
            search.Add("ShipeDate", Req_eDate_Ship);
        }

        //[取得/檢查參數] - Keyword
        if (!string.IsNullOrWhiteSpace(Req_Keyword))
        {
            search.Add("Keyword", Req_Keyword);
        }

        //[取得/檢查參數] - Cust
        if (!string.IsNullOrWhiteSpace(Req_Cust))
        {
            search.Add("Cust", Req_Cust);
        }

        //[取得/檢查參數] - Way
        if (!string.IsNullOrWhiteSpace(Req_Way))
        {
            if (!Req_Way.Equals("ALL"))
            {
                search.Add("Way", Req_Way);
            }
        }

        //[取得/檢查參數] - FW
        if (!string.IsNullOrWhiteSpace(Req_FreightWay))
        {
            if (!Req_FreightWay.Equals("ALL"))
            {
                search.Add("fw", Req_FreightWay);
            }
        }

        //[取得/檢查參數] - ShipComp
        if (!string.IsNullOrWhiteSpace(Req_ShipComp))
        {
            search.Add("ShipComp", Req_ShipComp);
        }

        #endregion

        //----- 方法:取得資料 -----
        var _rowData = _data.GetAllShipData(search, Req_DataType.ToUpper(), out ErrMsg)
             .Select(fld => new
             {
                 A = fld.EmptyCol,
                 B = fld.Erp_SO_FullID,
                 C = fld.EmptyCol,
                 D = fld.ShipWho,
                 E = fld.Check_Time1,
                 F = fld.ShipTel,
                 G = fld.ShipAddr1,
                 H = "大件快递3.60",
                 I = "月结",
                 J = "送货上楼",
                 K = fld.EmptyCol,
                 L = fld.EmptyCol,
                 M = fld.EmptyCol,
                 N = fld.EmptyCol,
                 O = "五金工具",
                 P = fld.EmptyCol,
                 Q = fld.EmptyCol,
                 R = fld.EmptyCol,
                 S = fld.EmptyCol,
                 T = fld.EmptyCol,
                 U = fld.EmptyCol,
                 V = fld.EmptyCol,
                 W = fld.EmptyCol,
                 X = fld.EmptyCol,
                 Y = fld.EmptyCol,
                 Z = fld.EmptyCol,
                 AA = fld.EmptyCol,
                 AB = fld.EmptyCol,
                 AC = fld.EmptyCol,
                 AD = fld.EmptyCol,
                 AE = fld.EmptyCol,
                 AF = fld.EmptyCol,
                 AG = fld.EmptyCol,
                 AH = fld.EmptyCol,
                 AI = fld.EmptyCol,
                 AJ = fld.EmptyCol
             });

        if (_rowData.Count() == 0)
        {
            CustomExtension.AlertMsg("目前條件查不到資料.", "");
            return;
        }

        //object to datatable
        DataTable myDT = CustomExtension.LINQToDataTable(_rowData);

        if (myDT.Rows.Count > 0)
        {
            //重新命名欄位標頭
            myDT.Columns["A"].ColumnName = "序號";
            myDT.Columns["B"].ColumnName = "平台订单号";
            myDT.Columns["C"].ColumnName = "收件公司名称";
            myDT.Columns["D"].ColumnName = "收件人姓名";
            myDT.Columns["E"].ColumnName = "收件人电话";
            myDT.Columns["F"].ColumnName = "收件人手机";
            myDT.Columns["G"].ColumnName = "收货详细地址";
            myDT.Columns["H"].ColumnName = "运输性质";
            myDT.Columns["I"].ColumnName = "运费付款方式";
            myDT.Columns["J"].ColumnName = "送货方式";
            myDT.Columns["K"].ColumnName = "厚度(0-10]cm件数";
            myDT.Columns["L"].ColumnName = "厚度(10-20]cm件数";
            myDT.Columns["M"].ColumnName = "厚度(20-25]cm件数";
            myDT.Columns["N"].ColumnName = "厚度25cm以上件数";
            myDT.Columns["O"].ColumnName = "货物名称";
            myDT.Columns["P"].ColumnName = "货物件数";
            myDT.Columns["Q"].ColumnName = "货物包装";
            myDT.Columns["R"].ColumnName = "货物重量";
            myDT.Columns["S"].ColumnName = "货物体积";
            myDT.Columns["T"].ColumnName = "其他费用 ";
            myDT.Columns["U"].ColumnName = "保价金额";
            myDT.Columns["V"].ColumnName = "代收退款方式";
            myDT.Columns["W"].ColumnName = "代收金额";
            myDT.Columns["X"].ColumnName = "开户姓名";
            myDT.Columns["Y"].ColumnName = "代收账号";
            myDT.Columns["Z"].ColumnName = "签收单";
            myDT.Columns["AA"].ColumnName = "签收单返单要求-签字";
            myDT.Columns["AB"].ColumnName = "签收单返单要求-盖章";
            myDT.Columns["AC"].ColumnName = "签收单返单要求-身份证号";
            myDT.Columns["AD"].ColumnName = "签收单返单要求-身份证复印件";
            myDT.Columns["AE"].ColumnName = "签收单返单要求-仓库收货回执单";
            myDT.Columns["AF"].ColumnName = "预约派送日期";
            myDT.Columns["AG"].ColumnName = "预约派送时间段";
            myDT.Columns["AH"].ColumnName = "拆包装";
            myDT.Columns["AI"].ColumnName = "预售单";
            myDT.Columns["AJ"].ColumnName = "备注信息";
        }

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "ShipExcelData-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }
    #endregion


    #region -- 附加功能 --
    /// <summary>
    /// 取得下拉清單-資料來源
    /// </summary>
    private IQueryable<ClassItem> GetClassMenu(string type)
    {
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

        try
        {
            //取得資料
            var data = _data.GetRefClass(Req_Lang, type, out ErrMsg);

            return data;

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
    /// 設定選單
    /// </summary>
    /// <param name="_menu">下拉選單Object</param>
    /// <param name="_source">選單資料來源</param>
    /// <param name="_inputVal">輸入值</param>
    /// <param name="_rootText">根目錄文字</param>
    void SetClassMenu(DropDownList _menu, IQueryable<ClassItem> _source, string _inputVal, string _rootText)
    {
        //設定繫結
        _menu.DataSource = _source;
        _menu.DataTextField = "Label";
        _menu.DataValueField = "ID";
        _menu.DataBind();

        //新增root item
        _menu.Items.Insert(0, new ListItem(_rootText, ""));

        //勾選已設定值
        _menu.SelectedIndex = _menu.Items.IndexOf(_menu.Items.FindByValue(_inputVal));
    }


    /// <summary>
    /// 含查詢條件的完整網址
    /// </summary>
    /// <returns></returns>
    public string filterUrl(int pageIdx)
    {
        //Params
        string _sDate = this.filter_sDate.Text;
        string _eDate = this.filter_eDate.Text;
        string _sDate_Ship = this.filter_sDate_Ship.Text;
        string _eDate_Ship = this.filter_eDate_Ship.Text;
        string _Keyword = this.filter_Keyword.Text;
        string _Cust = this.filter_Cust.Text;
        string _Way = this.filter_ShipWay.SelectedValue;
        string _FWay = this.filter_FreightWay.SelectedValue;
        string _ShipComp = this.filter_ShipComp.SelectedValue;

        //url string
        StringBuilder url = new StringBuilder();

        //固定條件:Page/TOP選單
        url.Append("{0}?dt={1}&page={2}".FormatThis(FuncPath(), Req_DataType
            , pageIdx == 0 ? 1 : pageIdx));

        //[查詢條件] - Date
        if (!string.IsNullOrWhiteSpace(_sDate))
        {
            url.Append("&sDate=" + Server.UrlEncode(_sDate));
        }
        if (!string.IsNullOrWhiteSpace(_eDate))
        {
            url.Append("&eDate=" + Server.UrlEncode(_eDate));
        }

        //[查詢條件] - ShipDate
        if (!string.IsNullOrWhiteSpace(_sDate_Ship))
        {
            url.Append("&sDate_Ship=" + Server.UrlEncode(_sDate_Ship));
        }
        if (!string.IsNullOrWhiteSpace(_eDate_Ship))
        {
            url.Append("&eDate_Ship=" + Server.UrlEncode(_eDate_Ship));
        }

        //[查詢條件] - Keyword
        if (!string.IsNullOrWhiteSpace(_Keyword))
        {
            url.Append("&k=" + Server.UrlEncode(_Keyword));
        }

        //[查詢條件] - Cust
        if (!string.IsNullOrWhiteSpace(_Cust))
        {
            url.Append("&Cust=" + Server.UrlEncode(_Cust));
        }

        //[查詢條件] - 物流途徑
        if (!string.IsNullOrWhiteSpace(_Way))
        {
            url.Append("&Way=" + Server.UrlEncode(_Way));
        }

        //[取得/檢查參數] - 貨運公司
        if (!string.IsNullOrWhiteSpace(_ShipComp))
        {
            url.Append("&ShipComp=" + Server.UrlEncode(_ShipComp));
        }

        //[查詢條件] - 運費方式
        if (!string.IsNullOrWhiteSpace(_FWay))
        {
            url.Append("&fw=" + Server.UrlEncode(_FWay));
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
        return "{0}{1}/{2}/ShipFreight_CHN".FormatThis(
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
    /// 資料判別:A=電商工具/B=電商玩具/C=經銷商工具/D=經銷商玩具
    /// </summary>
    public string Req_DataType
    {
        get
        {
            string data = Request.QueryString["dt"] == null ? "A" : Request.QueryString["dt"].ToString();
            return data;
        }
        set
        {
            this._Req_DataType = value;
        }
    }
    private string _Req_DataType;


    /// <summary>
    /// 取得傳遞參數 - sDate
    /// 預設7日內
    /// </summary>
    public string Req_sDate
    {
        get
        {
            String _data = Request.QueryString["sDate"];
            string dt = DateTime.Now.AddDays(-7).ToString().ToDateString("yyyy/MM/dd");
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
            string dt = DateTime.Now.ToString().ToDateString("yyyy/MM/dd");
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : dt;
        }
        set
        {
            this._Req_eDate = value;
        }
    }
    private string _Req_eDate;


    /// <summary>
    /// 取得傳遞參數 - sDate_Ship
    /// </summary>
    public string Req_sDate_Ship
    {
        get
        {
            String _data = Request.QueryString["sDate_Ship"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_sDate_Ship = value;
        }
    }
    private string _Req_sDate_Ship;


    /// <summary>
    /// 取得傳遞參數 - eDate_Ship
    /// </summary>
    public string Req_eDate_Ship
    {
        get
        {
            String _data = Request.QueryString["eDate_Ship"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "10", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_eDate_Ship = value;
        }
    }
    private string _Req_eDate_Ship;


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
    /// 取得傳遞參數 - Cust
    /// </summary>
    public string Req_Cust
    {
        get
        {
            String _data = Request.QueryString["Cust"];
            return (CustomExtension.String_資料長度Byte(_data, "1", "20", out ErrMsg)) ? _data.Trim() : "";
        }
        set
        {
            this._Req_Cust = value;
        }
    }
    private string _Req_Cust;


    /// <summary>
    /// 取得傳遞參數 - Way
    /// </summary>
    public string Req_Way
    {
        get
        {
            String _data = Request.QueryString["Way"];
            return _data;
        }
        set
        {
            this._Req_Way = value;
        }
    }
    private string _Req_Way;


    /// <summary>
    /// 取得傳遞參數 - 貨運公司
    /// </summary>
    public string Req_ShipComp
    {
        get
        {
            String _data = Request.QueryString["ShipComp"];
            return _data;
        }
        set
        {
            this._Req_ShipComp = value;
        }
    }
    private string _Req_ShipComp;


    /// <summary>
    /// 取得傳遞參數 - 運費方式
    /// </summary>
    public string Req_FreightWay
    {
        get
        {
            String _data = Request.QueryString["fw"];
            return _data;
        }
        set
        {
            this._Req_FreightWay = value;
        }
    }
    private string _Req_FreightWay;

    #endregion


}
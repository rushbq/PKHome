using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ShipFreight_CN.Controllers;
using ShipFreight_CN.Models;
using PKLib_Method.Methods;

public partial class myShipping_ShipComp : SecurityCheck
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


                //[參數判斷] - 編號
                if (!string.IsNullOrWhiteSpace(Req_DataID))
                {
                    //-- 載入指定資料 --
                    LookupData();
                }

                //-- 載入List資料 --
                LookupData_Detail();
            }
        }
        catch (Exception)
        {

            throw;
        }
    }


    #region -- 資料顯示:基本資料 --

    /// <summary>
    /// 取得資料
    /// </summary>
    private void LookupData()
    {
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("ID", Req_DataID);

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipComp(search, out ErrMsg).FirstOrDefault();

        //----- 資料整理:繫結 ----- 
        if (query == null)
        {
            CustomExtension.AlertMsg("無法取得資料", "");
            return;
        }

        //填入資料
        this.lt_DataID.Text = query.ID.ToString();
        this.hf_DataID.Value = query.ID.ToString();
        this.tb_Sort.Text = query.Sort.ToString();
        this.tb_DisplayName.Text = query.Label;
        this.cb_Display.Checked = query.Display.Equals("Y") ? true : false;
        this.btn_Save.Text = "資料修改";

    }

    #endregion


    /// <summary>
    /// 資料新增
    /// </summary>
    private void Add_Data()
    {
        //宣告
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

        //----- 設定:資料欄位 -----
        var data = new ShipComp
        {
            Label = this.tb_DisplayName.Text,
            Display = this.cb_Display.Checked ? "Y" : "N",
            Sort = Convert.ToInt16(this.tb_Sort.Text)
        };

        //----- 方法:新增資料 -----
        Int32 myID = _data.CreateShipComp(data, out ErrMsg);
        if (myID.Equals(0))
        {
            CustomExtension.AlertMsg("新增失敗", "");
            return;
        }
        else
        {
            //導向本頁
            Response.Redirect(thisPage);
        }
    }


    /// <summary>
    /// 資料修改
    /// </summary>
    private void Edit_Data()
    {
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

        //----- 設定:資料欄位 -----
        var data = new ShipComp
        {
            ID = Convert.ToInt32(this.hf_DataID.Value),
            Label = this.tb_DisplayName.Text,
            Display = this.cb_Display.Checked ? "Y" : "N",
            Sort = Convert.ToInt16(this.tb_Sort.Text)
        };

        //----- 方法:更新資料 -----
        if (!_data.UpdateShipComp(data, out ErrMsg))
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


    //SAVE-基本資料
    protected void btn_Save_Click(object sender, EventArgs e)
    {
        string errTxt = "";

        if (string.IsNullOrWhiteSpace(this.tb_DisplayName.Text))
        {
            errTxt += "貨運名稱空白\\n";
        }
        if (string.IsNullOrWhiteSpace(this.tb_Sort.Text))
        {
            errTxt += "排序空白\\n";
        }

        //alert
        if (!string.IsNullOrEmpty(errTxt))
        {
            CustomExtension.AlertMsg(errTxt, "");
            return;
        }

        /* 執行新增/更新 */
        if (string.IsNullOrEmpty(this.hf_DataID.Value))
        {
            Add_Data();
        }
        else
        {
            Edit_Data();
        }
    }


    private void LookupData_Detail()
    {
        //----- 宣告:資料參數 -----
        ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();

        //----- 原始資料:條件篩選 -----
        search.Add("Show", "All");

        //----- 原始資料:取得所有資料 -----
        var query = _data.GetShipComp(search, out ErrMsg);


        //----- 資料整理:繫結 ----- 
        this.lvDetailList.DataSource = query;
        this.lvDetailList.DataBind();
    }


    protected void lvDetailList_ItemCommand(object sender, ListViewCommandEventArgs e)
    {
        if (e.Item.ItemType == ListViewItemType.DataItem)
        {
            //取得Key值
            string dataID = ((HiddenField)e.Item.FindControl("hf_DataID")).Value;

            switch (e.CommandName.ToUpper())
            {
                case "DOCLOSE":
                    //----- 宣告:資料參數 -----
                    ShipFreight_CN_Repository _data = new ShipFreight_CN_Repository();

                    //----- 方法:刪除資料 -----
                    if (false == _data.DeleteShipComp(dataID, out ErrMsg))
                    {
                        _data = null;

                        CustomExtension.AlertMsg("刪除失敗", "");

                        return;
                    }
                    else
                    {
                        //導向本頁
                        Response.Redirect(thisPage);
                    }

                    break;
            }
        }
    }


    #region -- 傳遞參數 --

    /// <summary>
    /// 取得傳遞參數 - 資料編號
    /// </summary>
    private string _Req_DataID;
    public string Req_DataID
    {
        get
        {
            String DataID = Request["id"] == null ? "" : Request["id"].ToString();

            return DataID;
        }
        set
        {
            this._Req_DataID = value;
        }
    }

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
    /// 本頁網址
    /// </summary>
    private string _thisPage;
    public string thisPage
    {
        get
        {
            return "{0}/myShipping_CHN/ShipComp.aspx?dt={1}&back={2}".FormatThis(
                fn_Param.WebUrl
                , Req_DataType
                , Server.UrlEncode(prevPage));
        }
        set
        {
            this._thisPage = value;
        }
    }

    /// <summary>
    /// 上一頁網址
    /// </summary>
    private string _prevPage;
    public string prevPage
    {
        get
        {
            return Request.QueryString["back"].ToString();
        }
        set
        {
            this._prevPage = value;
        }
    }

    #endregion

}
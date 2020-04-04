using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

public partial class _Test_Test_Excel : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
       
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        string ErrMsg = "";

        //----- 宣告:資料參數 -----
        Menu4000Repository _data = new Menu4000Repository();
        Dictionary<string, string> search = new Dictionary<string, string>();
        DataTable DT = new DataTable();

        //----- 原始資料:條件篩選 -----
        search.Add("sDate", "2018/10/31");
        search.Add("eDate", "2018/10/31");

        //----- 方法:取得資料 -----
        var query = _data.GetDelayShipStat(search, out ErrMsg)
            .Select(fld => new
            {
                ModelNo = fld.ModelNo,
                NewQty = fld.NewQty
            });

        //將IQueryable轉成DataTable
        DataTable myDT = CustomExtension.LINQToDataTable(query);

        if (myDT.Rows.Count > 0)
        {
            //重新命名欄位標頭
            myDT.Columns["ModelNo"].ColumnName = "欄一";
            myDT.Columns["NewQty"].ColumnName = "欄二";
        }

        //release
        query = null;

        //匯出Excel
        CustomExtension.ExportExcel(
            myDT
            , "DataOutput-{0}.xlsx".FormatThis(DateTime.Now.ToShortDateString().ToDateString("yyyyMMdd"))
            , false);
    }
}
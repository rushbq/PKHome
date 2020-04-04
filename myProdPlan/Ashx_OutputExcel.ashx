<%@ WebHandler Language="C#" Class="Ashx_OutputExcel" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Menu1000Data.Controllers;
using PKLib_Method.Methods;

/// <summary>
/// 輸出至EXCEL
/// 因客制化複雜，故使用網頁輸出模式
/// </summary>
/// <remarks>
/// 產品計劃
/// </remarks>
public class Ashx_OutputExcel : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //System.Threading.Thread.Sleep(2000);

        try
        {

            StringBuilder html = new StringBuilder();

            //[接收參數]
            string _Year = context.Request["Year"];
            string _ClassID = context.Request["ClassID"];
            string _MenuLv1 = context.Request["MenuLv1"];
            string _MenuLv2 = context.Request["MenuLv2"];

            /* Check */
            if (string.IsNullOrWhiteSpace(_Year))
            {
                html.Append(("<h5>參數錯誤...請重新查詢!</h5>"));
                return;
            }

            //----- 宣告:資料參數 -----
            Menu1000Repository _data = new Menu1000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();

            //設定篩選參數
            //[取得/檢查參數] - Class ID
            if (!string.IsNullOrWhiteSpace(_ClassID) && !(_ClassID.Equals("-1")))
            {
                search.Add("Class_ID", _ClassID);
            }

            //[取得/檢查參數] - Menu Lv1
            if (!string.IsNullOrWhiteSpace(_MenuLv1) && !(_MenuLv1.Equals("-1")))
            {
                search.Add("Menu_Lv1", _MenuLv1);
            }

            //[取得/檢查參數] - Menu Lv2
            if (!string.IsNullOrWhiteSpace(_MenuLv2) && !(_MenuLv2.Equals("-1")))
            {
                search.Add("Menu_Lv2", _MenuLv2);
            }


            //----- 方法:取得資料 -----
            var data = _data.GetProdPlanData(search, Convert.ToInt16(_Year), out ErrMsg);
            if (data.Count() == 0)
            {
                html.Append(("<h5>目前條件查無資料...請重新查詢!</h5>" + ErrMsg));
            }
            else
            {
                /*
                  Html Table
                */
                //header
                html.Append("<table border=\"1\" class=\"grid\">");
                html.Append("<thead>");
                html.Append("<tr>");
                html.Append(" <th>銷售類別</th>");
                html.Append(" <th>一階類別</th>");
                html.Append(" <th>二階類別</th>");
                html.Append(" <th>三階類別</th>");
                html.Append(" <th>款式</th>");
                html.Append(" <th>品號</th>");
                html.Append(" <th>品名</th>");
                html.Append(" <th>年銷售量</th>");
                html.Append(" <th>銷售金額</th>");
                html.Append(" <th>平均銷售單價</th>");
                html.Append(" <th>平均單位成本</th>");
                html.Append(" <th>產品圖</th>");
                //html.Append(" <th>產品特性及說明</th>");
                html.Append(" <th>主要出貨地</th>");
                html.Append(" <th>主要供應商</th>");
                html.Append(" <th>預估完成</th>");
                html.Append(" <th>貨號</th>");
                html.Append(" <th>備註</th>");
                html.Append("</tr>");
                html.Append("</thead>");
                html.Append("<tbody>");

                foreach (var item in data)
                {
                    //*** 填入Html ***
                    html.Append("<tr style=\"{0}\">".FormatThis(item.FromData.Equals("1_NEW") ? "background-color:#fff59d" : ""));

                    html.Append("<td>{0}</td>".FormatThis(item.Class_Name));
                    html.Append("<td>{0}</td>".FormatThis(item.MenuNameLv1));
                    html.Append("<td>{0}</td>".FormatThis(item.MenuNameLv2));
                    html.Append("<td>{0}</td>".FormatThis(item.MenuNameLv3));
                    html.Append("<td>{0}</td>".FormatThis(item.StyleName));
                    html.Append("<td>{0}</td>".FormatThis(item.ModelNo));
                    html.Append("<td>{0}</td>".FormatThis(item.Model_Name));
                    //年銷售量
                    html.Append("<td>{0}</td>".FormatThis(item.SalesNum));
                    //銷售金額
                    html.Append("<td>{0}</td>".FormatThis(Math.Round(item.SalesAmount, 2)));
                    //平均銷售單價
                    html.Append("<td>{0}</td>".FormatThis(Math.Round(item.avgSalesAmount, 2)));
                    //平均單位成本
                    html.Append("<td>{0}</td>".FormatThis(Math.Round(item.avgPaperCost, 2)));
                    //產品圖
                    html.Append("<td width=\"110\" {1}>{0}</td>".FormatThis(
                        GetImgUrl(item.FromData, item.ModelNo, item.ListPic)
                        , !string.IsNullOrWhiteSpace(item.ListPic) ? "style=\"height:115px !important;\"" : ""));

                    /*html.Append("<td>{0}</td>".FormatThis(item.ProdDesc + " " + item.ProdFeature)); //產品特性/說明 (此為Html Code不可顯示)*/

                    //主要出貨地
                    html.Append("<td>{0}</td>".FormatThis(item.Ship_From));
                    //主要供應商
                    html.Append("<td>{0}</td>".FormatThis(item.SupName));
                    //預估完成
                    html.Append("<td>{0}</td>".FormatThis(item.TargetMonth));
                    //貨號
                    html.Append("<td>{0}</td>".FormatThis(item.ItemNo));
                    html.Append("<td>{0}</td>".FormatThis(item.Remark));

                    html.Append("</tr>");
                }

                html.Append("</tbody>");
                html.Append("</table>");
            }


            _data = null;

            /*
            output Excel
            */
            HttpResponse resp = System.Web.HttpContext.Current.Response;
            resp.Charset = "utf-8";
            resp.Clear();
            string filename = "Product_{0}".FormatThis(DateTime.Now.ToString("yyyyMMddHHmm"));
            resp.AppendHeader("Content-Disposition", "attachment;filename=" + filename + ".xls");
            resp.ContentEncoding = System.Text.Encoding.UTF8;
            resp.ContentType = "application/ms-excel";

            resp.Write("<html>");
            string style = "<meta http-equiv=\"content-type\" content=\"application/ms-excel; charset=utf-8\"/>" + "<style> .grid{color: #222222; text-align:center; } .grid thead th{font: 10pt \"Microsoft JhengHei\", \"Microsoft YaHei\"; color: #ffffff; font-weight: bold; background-color: #1565c0; height:30px; text-align:center;} .grid td{font: 10pt \"Microsoft JhengHei\", \"Microsoft YaHei\"; height:25px;} .red-text{color:#f44336} </style>";
            resp.Write(style);
            resp.Write("<body>");
            resp.Write(html.ToString());
            resp.Write("</body></html>");

            resp.Flush();
            //resp.End(); //--不可用,會有thread錯誤
            HttpContext.Current.ApplicationInstance.CompleteRequest();


        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Get Image
    /// </summary>
    /// <param name="_source"></param>
    /// <param name="_modelNo"></param>
    /// <param name="_ID">file name</param>
    /// <returns></returns>
    private string GetImgUrl(string _source, string _modelNo, string _ID)
    {
        if (string.IsNullOrWhiteSpace(_ID))
        {
            return "";
        }

        //Img url
        string _url = fn_Param.RefUrl;
        if (_source.ToUpper().Equals("0_SYS"))
        {
            //帶產品中心圖片
            _url += "ProductPic/" + _modelNo + "/1/" + _ID;
        }
        else
        {
            //新品-自行上傳的圖片
            _url += "PKHome/ProductPlan/" + _ID;
        }

        return "<img src=\"{0}\" width=\"80\" height=\"80\">".FormatThis(_url);
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
<%@ WebHandler Language="C#" Class="Ashx_OutputExcel" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

/// <summary>
/// 輸出至EXCEL
/// 因客制化複雜，故使用網頁輸出模式
/// </summary>
/// <remarks>
/// 到貨狀況
/// </remarks>
public class Ashx_OutputExcel : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg;

        //System.Threading.Thread.Sleep(2000);

        try
        {
            //[接收參數]
            string _CompID = context.Request["CompID"];
            string _OpcsNo = context.Request["OpcsNo"];
            string _Cust = context.Request["Cust"];
            string _sDate = context.Request["sDate"];
            string _eDate = context.Request["eDate"];
            string _ProdProperty = context.Request["ProdProperty"];
            string _MakeStatus = context.Request["MakeStatus"];
            string _ShipStatus = context.Request["ShipStatus"];
            string _GetInStatus = context.Request["GetInStatus"];
            string _StockStatus = context.Request["StockStatus"];
            string _PurStatus = context.Request["PurStatus"];
            string _Dept = context.Request["Dept"];
            string _Fastmenu = context.Request["Fastmenu"];
            string _menuID = context.Request["menuID"];


            /* Check */
            if (string.IsNullOrEmpty(_CompID))
            {
                context.Response.Write("<h5>參數錯誤...請重新查詢!</h5>");
                return;
            }

            if (CheckReqNull(_OpcsNo) && CheckReqNull(_Cust) && CheckReqNull(_sDate) && CheckReqNull(_eDate)
                    && CheckReqNull(_ProdProperty) && CheckReqNull(_MakeStatus) && CheckReqNull(_ShipStatus)
                    && CheckReqNull(_GetInStatus) && CheckReqNull(_StockStatus) && CheckReqNull(_PurStatus)
                    && CheckReqNull(_Dept))
            {
                context.Response.Write("<h5>篩選條件太少,會造成伺服器負擔過重...請重新加入條件!</h5>");
                return;
            }

            //----- 宣告:資料參數 -----
            Menu4000Repository _data = new Menu4000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();
            DataTable DT = new DataTable();
            StringBuilder html = new StringBuilder();

            //設定篩選參數
            search.Add("OpcsNo", _OpcsNo);
            search.Add("Cust", _Cust);
            search.Add("sDate", _sDate);
            search.Add("eDate", _eDate);
            search.Add("ProdProperty", _ProdProperty);
            search.Add("MakeStatus", _MakeStatus);
            search.Add("ShipStatus", _ShipStatus);
            search.Add("GetInStatus", _GetInStatus);
            search.Add("StockStatus", _StockStatus);
            search.Add("PurStatus", _PurStatus);
            search.Add("Dept", _Dept);
            search.Add("Fastmenu", _Fastmenu);
            search.Add("menuID", _menuID);


            //----- 方法:取得資料 -----
            using (DT = _data.GetOpcsStatus(_CompID, search, out ErrMsg))
            {
                if (DT.Rows.Count == 0)
                {
                    context.Response.Write("<h5>目前條件查無資料...請重新查詢!</h5>" + ErrMsg);
                }
                else
                {
                    /*
                      Html Table
                    */

                    //header
                    html.Append("<table border=\"1\" class=\"table\">");
                    html.Append("<tr>");
                    html.Append(" <th rowspan=\"2\">序號</th>");
                    //資材
                    if (_menuID.Equals("190"))
                    {
                        html.Append(" <th rowspan=\"2\">訂單號</th>");
                    }
                    html.Append(" <th rowspan=\"2\">品號</th>");
                    html.Append(" <th rowspan=\"2\">品名</th>");
                    html.Append(" <th rowspan=\"2\">訂單未出數量</th>");

                    /* #全部未出數量# */
                    switch (_CompID)
                    {
                        case "SH":
                            html.Append(" <th rowspan=\"2\">12倉全部未出數量</th>");
                            html.Append(" <th rowspan=\"2\">14倉全部未出數量</th>");
                            html.Append(" <th rowspan=\"2\">A01倉全部未出數量</th>");
                            break;

                        default:
                            html.Append(" <th rowspan=\"2\">01倉全部未出數量</th>");
                            html.Append(" <th rowspan=\"2\">20倉全部未出數量</th>");
                            html.Append(" <th rowspan=\"2\">22倉全部未出數量</th>");
                            break;
                    }

                    /* #庫存# */
                    switch (_CompID)
                    {
                        case "SH":
                            html.Append(" <th rowspan=\"2\">12倉庫存</th>");
                            html.Append(" <th rowspan=\"2\">14倉庫存</th>");
                            html.Append(" <th rowspan=\"2\">A01倉庫存</th>");
                            break;

                        default:
                            html.Append(" <th rowspan=\"2\">01倉庫存</th>");
                            html.Append(" <th rowspan=\"2\">20倉庫存</th>");
                            html.Append(" <th rowspan=\"2\">22倉庫存</th>");
                            break;
                    }

                    /* #不足量# */
                    switch (_CompID)
                    {
                        case "SH":
                            html.Append(" <th rowspan=\"2\">12倉不足量</th>");
                            html.Append(" <th rowspan=\"2\">14倉不足量</th>");
                            html.Append(" <th rowspan=\"2\">A01倉不足量</th>");
                            break;

                        default:
                            html.Append(" <th rowspan=\"2\">01倉不足量</th>");
                            html.Append(" <th rowspan=\"2\">20倉不足量</th>");
                            html.Append(" <th rowspan=\"2\">22倉不足量</th>");
                            break;
                    }

                    /* #生產待入庫# */
                    switch (_CompID)
                    {
                        case "SH":
                            html.Append(" <th rowspan=\"2\">12倉待入庫</th>");
                            html.Append(" <th rowspan=\"2\">14倉待入庫</th>");
                            html.Append(" <th rowspan=\"2\">A01倉待入庫</th>");
                            break;

                        default:
                            html.Append(" <th rowspan=\"2\">01倉待入庫</th>");
                            html.Append(" <th rowspan=\"2\">20倉待入庫</th>");
                            html.Append(" <th rowspan=\"2\">22倉待入庫</th>");
                            break;
                    }

                    /* #預計進# */
                    switch (_CompID)
                    {
                        case "SH":
                            html.Append(" <th rowspan=\"2\">12倉預計進</th>");
                            html.Append(" <th rowspan=\"2\">14倉預計進</th>");
                            html.Append(" <th rowspan=\"2\">A01倉預計進</th>");
                            break;

                        default:
                            html.Append(" <th rowspan=\"2\">01倉預計進</th>");
                            html.Append(" <th rowspan=\"2\">20倉預計進</th>");
                            html.Append(" <th rowspan=\"2\">22倉預計進</th>");
                            break;
                    }

                    /* #計畫進# */
                    switch (_CompID)
                    {
                        case "SH":
                            html.Append(" <th rowspan=\"2\">12倉計畫進</th>");
                            html.Append(" <th rowspan=\"2\">14倉計畫進</th>");
                            html.Append(" <th rowspan=\"2\">A01倉計畫進</th>");
                            break;

                        default:
                            html.Append(" <th rowspan=\"2\">01倉計畫進</th>");
                            html.Append(" <th rowspan=\"2\">20倉計畫進</th>");
                            html.Append(" <th rowspan=\"2\">22倉計畫進</th>");
                            break;
                    }

                    html.Append(" <th rowspan=\"2\">品號屬性</th>");
                    html.Append(" <th rowspan=\"2\">預交日</th>");
                    html.Append(" <th rowspan=\"2\">安全存量</th>");
                    html.Append(" <th rowspan=\"2\">儲位</th>");
                    html.Append(" <th rowspan=\"2\">主供應商</th>");
                    html.Append(" <th colspan=\"6\">採購/進貨</th>");
                    html.Append(" <th colspan=\"5\">生產/入庫</th>");
                    //只有資材190顯示
                    if (_menuID.Equals("190"))
                    {
                        html.Append("<th rowspan=\"2\">資材理貨</th>");
                        html.Append("<th rowspan=\"2\">箱號/包裝</th>");
                    }
                    html.Append("<th rowspan=\"2\">客戶品號</th>");
                    html.Append("<th rowspan=\"2\">產品特別注意事項</th>");
                    html.Append("</tr>");
                    html.Append("<tr>");
                    //採購/進貨
                    html.Append(" <th>廠商</th>");
                    html.Append(" <th>採購單號</th>");
                    html.Append(" <th>採購預交日</th>");
                    html.Append(" <th>採購數量</th>");
                    html.Append(" <th>進貨數量</th>");
                    html.Append(" <th>未進貨數量</th>");
                    //生產/入庫
                    html.Append("<th>產品圖號</th>");
                    html.Append("<th>製令單號</th>");
                    html.Append("<th>實際完工日</th>");
                    html.Append("<th>狀態</th>");
                    html.Append("<th>入庫量</th>");
                    html.Append("</tr>");

                    string newline = "<br>";
                    for (int row = 0; row < DT.Rows.Count; row++)
                    {
                        //宣告欄位變數
                        int rowNum = Convert.ToInt32(DT.Rows[row]["RowNumber"]);
                        string modelNo = DT.Rows[row]["ModelNo"].ToString();
                        string _Order_FID = DT.Rows[row]["Order_FID"].ToString();
                        string _Order_SID = DT.Rows[row]["Order_SID"].ToString();
                        int _unShip_OrderQty = Convert.ToInt32(DT.Rows[row]["unShip_OrderQty"]);
                        int _stockQtyMain = Convert.ToInt32(DT.Rows[row]["StockQty_Main"]);
                        int _safeQty_Main = Convert.ToInt32(DT.Rows[row]["SafeQty_Main"]);
                        int _purQty = Convert.ToInt32(DT.Rows[row]["PurQty"]);

                        /*分倉資料*/
                        int _StkQty01 = Convert.ToInt32(DT.Rows[row]["StkQty01"]);
                        int _StkQty20 = Convert.ToInt32(DT.Rows[row]["StkQty20"]);
                        int _StkQty22 = Convert.ToInt32(DT.Rows[row]["StkQty22"]);
                        int _StkQty12 = Convert.ToInt32(DT.Rows[row]["StkQty12"]);
                        int _StkQty14 = Convert.ToInt32(DT.Rows[row]["StkQty14"]);
                        int _StkQtyA01 = Convert.ToInt32(DT.Rows[row]["StkQtyA01"]);
                        int _PreInQty01 = Convert.ToInt32(DT.Rows[row]["PreInQty01"]);
                        int _PreInQty20 = Convert.ToInt32(DT.Rows[row]["PreInQty20"]);
                        int _PreInQty22 = Convert.ToInt32(DT.Rows[row]["PreInQty22"]);
                        int _PreInQty12 = Convert.ToInt32(DT.Rows[row]["PreInQty12"]);
                        int _PreInQty14 = Convert.ToInt32(DT.Rows[row]["PreInQty14"]);
                        int _PreInQtyA01 = Convert.ToInt32(DT.Rows[row]["PreInQtyA01"]);
                        int _PlanInQty01 = Convert.ToInt32(DT.Rows[row]["PlanInQty01"]);
                        int _PlanInQty20 = Convert.ToInt32(DT.Rows[row]["PlanInQty20"]);
                        int _PlanInQty22 = Convert.ToInt32(DT.Rows[row]["PlanInQty22"]);
                        int _PlanInQty12 = Convert.ToInt32(DT.Rows[row]["PlanInQty12"]);
                        int _PlanInQty14 = Convert.ToInt32(DT.Rows[row]["PlanInQty14"]);
                        int _PlanInQtyA01 = Convert.ToInt32(DT.Rows[row]["PlanInQtyA01"]);
                        int _unStockQty01 = Convert.ToInt32(DT.Rows[row]["unStockQty01"]);
                        int _unStockQty20 = Convert.ToInt32(DT.Rows[row]["unStockQty20"]);
                        int _unStockQty22 = Convert.ToInt32(DT.Rows[row]["unStockQty22"]);
                        int _unStockQty12 = Convert.ToInt32(DT.Rows[row]["unStockQty12"]);
                        int _unStockQty14 = Convert.ToInt32(DT.Rows[row]["unStockQty14"]);
                        int _unStockQtyA01 = Convert.ToInt32(DT.Rows[row]["unStockQtyA01"]);
                        int _unOutQty01 = Convert.ToInt32(DT.Rows[row]["unOutQty01"]);
                        int _unOutQty20 = Convert.ToInt32(DT.Rows[row]["unOutQty20"]);
                        int _unOutQty22 = Convert.ToInt32(DT.Rows[row]["unOutQty22"]);
                        int _unOutQty12 = Convert.ToInt32(DT.Rows[row]["unOutQty12"]);
                        int _unOutQty14 = Convert.ToInt32(DT.Rows[row]["unOutQty14"]);
                        int _unOutQtyA01 = Convert.ToInt32(DT.Rows[row]["unOutQtyA01"]);
                        int _ShortQty01 = Convert.ToInt32(DT.Rows[row]["ShortQty01"]);
                        int _ShortQty20 = Convert.ToInt32(DT.Rows[row]["ShortQty20"]);
                        int _ShortQty22 = Convert.ToInt32(DT.Rows[row]["ShortQty22"]);
                        int _ShortQty12 = Convert.ToInt32(DT.Rows[row]["ShortQty12"]);
                        int _ShortQty14 = Convert.ToInt32(DT.Rows[row]["ShortQty14"]);
                        int _ShortQtyA01 = Convert.ToInt32(DT.Rows[row]["ShortQtyA01"]);


                        //判斷是否為各單的第一筆,填入OPCS資訊
                        //資材190:不顯示
                        if (!_menuID.Equals("190"))
                        {
                            if (rowNum.Equals(1))
                            {
                                html.Append("<tr>");
                                html.Append("<td colspan=\"{0}\">".FormatThis(_CompID.Equals("TW") ? 29 : 28));
                                html.Append(" <b>OPCS No：</b>{0}-{1}&nbsp;&nbsp;".FormatThis(_Order_FID, _Order_SID));
                                html.Append(" <b>客戶：</b>({0})&nbsp;{1}&nbsp;&nbsp;".FormatThis(DT.Rows[row]["CustID"], DT.Rows[row]["CustName"]));
                                html.Append(" <b>(<span class=\"red-text\">{0}</span>)</b>{1}".FormatThis(GetShipStatusName(DT.Rows[row]["ShipStatus"].ToString()), newline));
                                //html.Append(" <b>幣別：</b>{0}&nbsp;&nbsp;".FormatThis(DT.Rows[row]["Currency"]));
                                //html.Append(" <b>交易條件：</b>{0}&nbsp;&nbsp;".FormatThis(DT.Rows[row]["TradeConditional"]));
                                //html.Append(" <b>付款方式：</b>{0}".FormatThis(DT.Rows[row]["PaidConditional"] + newline));
                                html.Append(" <b>客戶注意事項：</b>{0}".FormatThis(DT.Rows[row]["OrderRemark"].ToString().Replace("\n", newline)));
                                html.Append("</td>");
                                html.Append("</tr>");
                            }
                        }


                        //*** 填入Html ***
                        //定義收合用class
                        html.Append("<tr>");

                        //訂單序號
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["OrderSno"]));

                        //資材-訂單號
                        if (_menuID.Equals("190"))
                        {
                            html.Append("<td>{0}{1}</td>".FormatThis(_Order_FID, _Order_SID));
                        }

                        //品號
                        html.Append("<td>{0}</td>".FormatThis(
                            DT.Rows[row]["ModelNo"]));

                        //品名
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["ModelName"]));

                        //訂單數量(未出)
                        html.Append("<td>{0}</td>".FormatThis(_unShip_OrderQty));


                        /* #全部未出數量# */
                        switch (_CompID)
                        {
                            case "SH":
                                //SH:12倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _unOutQty12
                                    , _unOutQty12 > _StkQty12 ? "red-text" : "green-text"));

                                //SH:14倉
                                html.Append("<td>;<span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _unOutQty14
                                    , _unOutQty14 > _StkQty14 ? "red-text" : "green-text"));

                                //SH:A01倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _unOutQtyA01
                                    , _unOutQtyA01 > _StkQtyA01 ? "red-text" : "green-text"));

                                break;

                            default:
                                //TW:01倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _unOutQty01
                                    , _unOutQty01 > _StkQty01 ? "red-text" : "green-text"));

                                //TW:20倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _unOutQty20
                                    , _unOutQty20 > _StkQty20 ? "red-text" : "green-text"));

                                //TW:22倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _unOutQty22
                                    , _unOutQty22 > _StkQty22 ? "red-text" : "green-text"));

                                break;
                        }


                        /* #庫存# */
                        switch (_CompID)
                        {
                            case "SH":
                                //SH:12倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_StkQty12));

                                //SH:14倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_StkQty14));

                                //SH:A01倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_StkQtyA01));

                                break;

                            default:
                                //TW:01倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_StkQty01));

                                //TW:20倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_StkQty20));

                                //TW:22倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_StkQty22));

                                break;
                        }


                        /* #不足量# */
                        switch (_CompID)
                        {
                            case "SH":
                                //SH:12倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _ShortQty12
                                    , _ShortQty12 < 0 ? "red-text" : "green-text"));

                                //SH:14倉
                                html.Append("<td>;<span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _ShortQty14
                                    , _ShortQty14 < 0 ? "red-text" : "green-text"));

                                //SH:A01倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _ShortQtyA01
                                    , _ShortQtyA01 < 0 ? "red-text" : "green-text"));

                                break;

                            default:
                                //TW:01倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _ShortQty01
                                    , _ShortQty01 < 0 ? "red-text" : "green-text"));

                                //TW:20倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _ShortQty20
                                    , _ShortQty20 < 0 ? "red-text" : "green-text"));

                                //TW:22倉
                                html.Append("<td><span class=\"{1}\">{0}</span></td>".FormatThis(
                                    _ShortQty22
                                    , _ShortQty22 < 0 ? "red-text" : "green-text"));

                                break;
                        }


                        /* #生產待入庫# */
                        switch (_CompID)
                        {
                            case "SH":
                                //SH:12倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_unStockQty12));

                                //SH:14倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_unStockQty14));

                                //SH:A01倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_unStockQtyA01));

                                break;

                            default:
                                //TW:01倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_unStockQty01));

                                //TW:20倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_unStockQty20));

                                //TW:22倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_unStockQty22));

                                break;
                        }


                        /* #預計進# */
                        switch (_CompID)
                        {
                            case "SH":
                                //SH:12倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PreInQty12));

                                //SH:14倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PreInQty14));

                                //SH:A01倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PreInQtyA01));

                                break;

                            default:
                                //TW:01倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PreInQty01));

                                //TW:20倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PreInQty20));

                                //TW:22倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PreInQty22));

                                break;
                        }


                        /* #計劃進# */
                        switch (_CompID)
                        {
                            case "SH":
                                //SH:12倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PlanInQty12));

                                //SH:14倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PlanInQty14));

                                //SH:A01倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PlanInQtyA01));

                                break;

                            default:
                                //TW:01倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PlanInQty01));

                                //TW:20倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PlanInQty20));

                                //TW:22倉
                                html.Append("<td><span>{0}</span></td>".FormatThis(_PlanInQty22));

                                break;
                        }


                        //品號屬性
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["ProdProperty"]));
                        //預交日
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["OrderPreDate"]));

                        //安全存量
                        html.Append("<td class=\"{1}\">{0}</td>".FormatThis(
                            _safeQty_Main
                            , _safeQty_Main - _unShip_OrderQty > 0 ? "yellow" : ""
                            ));

                        //儲位  
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["StockPos"]));

                        //主供應商
                        html.Append("<td>{0}{1}</td>".FormatThis(
                            DT.Rows[row]["Main_SupplierName"]
                            , "-" + DT.Rows[row]["Main_SupplierID"]));

                        //-- 採購/進貨(col*6) --
                        //廠商
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["PurSupplier"]));
                        //採購單號
                        html.Append("<td>{0}-{1}</td>".FormatThis(DT.Rows[row]["PUR_FID"], DT.Rows[row]["PUR_SID"]));
                        //採購預交日
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["PurPreDate"]));

                        //採購數量
                        html.Append("<td class=\"{1}\">{0}</td>".FormatThis(
                            _purQty
                            , _purQty < _unShip_OrderQty ? "yellow" : ""
                            ));


                        //進貨數量
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["GetInQty"]));
                        //未進貨數量
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["unGetInQty"]));

                        //-- 生產/入庫(col*5) --
                        //產品圖號
                        string _prodImgDesc = DT.Rows[row]["ProdImgDesc"].ToString();
                        string _spDesc = "";
                        if (!string.IsNullOrEmpty(_prodImgDesc))
                        {
                            //拆解以_為分隔的資料
                            if (_prodImgDesc.IndexOf('_') > -1)
                            {
                                string[] strAry = _prodImgDesc.Split('_');
                                //只顯示00的資料
                                if (strAry[0].Equals("00"))
                                {
                                    _spDesc = strAry[1];
                                }
                            }
                        }

                        html.Append("<td>{0}</td>".FormatThis(_spDesc));

                        //製令單號
                        string _makeFid = DT.Rows[row]["Make_FID"].ToString();
                        string _makeSid = DT.Rows[row]["Make_SID"].ToString();
                        html.Append("<td>{0}-{1}</td>".FormatThis(
                            _makeFid
                            , _makeSid
                            ));

                        //實際完工日
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["FinishDate"]));

                        //製令狀態
                        html.Append("<td>{0}</td>".FormatThis(
                            GetMakeName(DT.Rows[row]["MakeStatus"].ToString())
                            , DT.Rows[row]["PurConfirm"].ToString().Equals("V") ? "<span class=\"red-text\">(本單作廢)</span>" : ""));

                        //入庫量  
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["MakeStockQty"]));


                        //只有資材190顯示
                        if (_menuID.Equals("190"))
                        {
                            //資材理貨 
                            string _stockValue = DT.Rows[row]["StockValue"].ToString();
                            string _showArea = _stockValue;
                            html.Append("<td>{0}</td>".FormatThis(
                                string.IsNullOrEmpty(_stockValue) || (_stockValue.Equals("N")) ? "" : _showArea + "區"
                                ));

                            //箱號
                            string _boxValue = DT.Rows[row]["BoxValue"].ToString();
                            html.Append("<td>{0} ~ {1}<br/>{2}</td>".FormatThis(DT.Rows[row]["BoxNoStart"], DT.Rows[row]["BoxNoEnd"], _boxValue));
                        }


                        //客戶品號
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["CustModel"]));

                        //產品特別注意事項
                        html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["ProdRemark"]));
                        html.Append("</tr>");
                    }

                    html.Append("</table>");

                }
            }

            _data = null;

            /*
            output Excel
            */
            HttpResponse resp = System.Web.HttpContext.Current.Response;
            resp.Charset = "utf-8";
            resp.Clear();
            string filename = "OPCS({0})_{1}".FormatThis(_CompID, DateTime.Now.ToString("yyyyMMddHHmm"));
            resp.AppendHeader("Content-Disposition", "attachment;filename=" + filename + ".xls");
            resp.ContentEncoding = System.Text.Encoding.UTF8;

            resp.ContentType = "application/ms-excel";
            string style = "<meta http-equiv=\"content-type\" content=\"application/ms-excel; charset=utf-8\"/>" + "<style> .table{color: #222222; text-align:center; }.table th{font: 10pt \"Microsoft JhengHei\", \"Microsoft YaHei\"; color: #ffffff; font-weight: bold; background-color: #1565c0; height:30px; text-align:center;}.table td{font: 10pt \"Microsoft JhengHei\", \"Microsoft YaHei\"; height:25px;} .red-text{color:#f44336} .yellow{background-color:#fff176 }</style>";

            resp.Write("<html>");
            resp.Write(style);
            resp.Write("<body>");
            resp.Write(html.ToString());
            resp.Write("</body></html>");
            resp.Flush();
            //resp.End(); //--不可用,會有thread錯誤
            HttpContext.Current.ApplicationInstance.CompleteRequest();


        }
        catch (Exception ex)
        {
            context.Response.Write("<h5>資料查詢時發生錯誤!!若持續看到此訊息,請聯絡系統管理人員.</h5>" + ex.Message.ToString());
        }
    }

    private bool CheckReqNull(string reqVal)
    {
        return string.IsNullOrEmpty(reqVal);
    }

    /// <summary>
    /// 製令完工狀態
    /// </summary>
    /// <param name="st"></param>
    /// <returns></returns>
    private string GetMakeName(string st)
    {
        switch (st)
        {
            //1:未生產, 2:已發料, 3:生產中, Y:已完工, y:指定完工
            case "1":
                return "未生產";

            case "2":
                return "已發料";

            case "3":
                return "生產中";

            case "Y":
                return "已完工";

            case "y":
                return "指定完工";

            default:
                return "";
        }
    }

    /// <summary>
    /// 出貨狀態
    /// </summary>
    /// <param name="st"></param>
    /// <returns></returns>
    private string GetShipStatusName(string st)
    {
        switch (st)
        {
            case "Y":
                return "<a class=\"ui blue tag label\">已出貨</a>";

            default:
                return "<a class=\"ui red tag label\">未出貨</a>";
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
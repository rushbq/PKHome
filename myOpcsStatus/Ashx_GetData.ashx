<%@ WebHandler Language="C#" Class="Ashx_GetData" %>

using System;
using System.Web;
using System.Data;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Menu4000Data.Controllers;
using PKLib_Method.Methods;

/// <summary>
/// 取得OPCS所有資料
/// </summary>
public class Ashx_GetData : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string ErrMsg = "";

        //System.Threading.Thread.Sleep(2000);

        try
        {
            context.Response.ContentType = "text/html";

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
                context.Response.Write("#fail#<h5>參數錯誤...請重新查詢!</h5>");
                return;
            }

            if (CheckReqNull(_OpcsNo) && CheckReqNull(_Cust) && CheckReqNull(_sDate) && CheckReqNull(_eDate)
                    && CheckReqNull(_ProdProperty) && CheckReqNull(_MakeStatus) && CheckReqNull(_ShipStatus)
                    && CheckReqNull(_GetInStatus) && CheckReqNull(_StockStatus) && CheckReqNull(_PurStatus)
                    && CheckReqNull(_Dept))
            {
                context.Response.Write("#fail#<h5>篩選條件太少,會造成伺服器負擔過重...請重新加入條件!</h5>");
                return;
            }

            //----- 宣告:資料參數 -----
            Menu4000Repository _data = new Menu4000Repository();
            Dictionary<string, string> search = new Dictionary<string, string>();
            DataTable DT = new DataTable();

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


            //----- 客製:不同部門要顯示對應的欄位 -----
            //所有欄位
            List<Menu4000Repository.OpcsColumn> allCol = _data.SetOpcsTableHeader();

            //要顯示的欄位(對應不同部門)
            List<Menu4000Repository.OpcsDept> deptRel = _data.SetOpcsDept(_CompID, _menuID);


            //----- 方法:取得資料 -----
            using (DT = _data.GetOpcsStatus(_CompID, search, out ErrMsg))
            {
                if (DT.Rows.Count == 0)
                {
                    context.Response.Write("#fail#<h5>目前條件查無資料...請重新查詢!</h5>" + ErrMsg);
                }
                else
                {
                    /*
                      Html Table
                    */
                    StringBuilder html = new StringBuilder();
                    int colCnt = GetColumnCnt(_menuID);

                    for (int row = 0; row < DT.Rows.Count; row++)
                    {
                        //宣告欄位變數
                        int rowNum = Convert.ToInt32(DT.Rows[row]["RowNumber"]);
                        string modelNo = DT.Rows[row]["ModelNo"].ToString();
                        string _OrderSno = DT.Rows[row]["OrderSno"].ToString();
                        string _Order_FID = DT.Rows[row]["Order_FID"].ToString();
                        string _Order_SID = DT.Rows[row]["Order_SID"].ToString();
                        int _unShip_OrderQty = Convert.ToInt32(DT.Rows[row]["unShip_OrderQty"]);
                        int _totalQty = Convert.ToInt32(DT.Rows[row]["TotalQty"]);
                        int _stockQtyMain = Convert.ToInt32(DT.Rows[row]["StockQty_Main"]);
                        int _stockQty11 = Convert.ToInt32(DT.Rows[row]["StockQty_11"]);
                        int _stockQty14 = Convert.ToInt32(DT.Rows[row]["StockQty_14"]);
                        int _shortQty = Convert.ToInt32(DT.Rows[row]["ShortQty"]);
                        int _unStockQty = Convert.ToInt32(DT.Rows[row]["unStockQty"]);
                        int _preInQty = Convert.ToInt32(DT.Rows[row]["PreInQty"]);
                        int _safeQty_Main = Convert.ToInt32(DT.Rows[row]["SafeQty_Main"]);
                        int _purQty = Convert.ToInt32(DT.Rows[row]["PurQty"]);
                        int _getInQty = Convert.ToInt32(DT.Rows[row]["GetInQty"]);
                        int _unGetInQty = Convert.ToInt32(DT.Rows[row]["unGetInQty"]);
                        string _modelName = DT.Rows[row]["ModelName"].ToString();
                        string _orderRemark = DT.Rows[row]["OrderRemark"].ToString();
                        ErrMsg = "{0}-{1},error on 前一筆或下一筆".FormatThis(_Order_FID + _Order_SID, _OrderSno);

                        //判斷是否為各單的第一筆,填入OPCS資訊
                        if (rowNum.Equals(1))
                        {
                            string rowModalID = _Order_FID + _Order_SID;

                            html.Append("<tr>");
                            html.Append("<td colspan=\"{0}\">".FormatThis(colCnt));
                            html.Append("    <table class=\"ui brown table groupData\">");
                            html.Append("        <tr>");
                            //OpcsNo
                            html.Append("            <td class=\"right aligned\" style=\"width:10%\"><strong>OPCS No</strong></td>");
                            html.Append("            <td class=\"red-text text-darken-2 ui header\" style=\"width:20%\"><b>{0}-{1}</b></td>".FormatThis(_Order_FID, _Order_SID));
                            //客戶
                            html.Append("            <td class=\"right aligned\" style=\"width:10%\"><strong>客戶</strong></td>");
                            html.Append("            <td class=\"green-text text-darken-2 ui header\" style=\"width:20%\"><b>({0})&nbsp;{1}</b></td>".FormatThis(DT.Rows[row]["CustID"], DT.Rows[row]["CustName"]));
                            //客戶注意事項
                            html.Append("            <td class=\"center aligned\" style=\"width:20%\">{0}{1}</td>".FormatThis(
                                string.IsNullOrEmpty(_orderRemark) ? "" : "<a href=\"#!\" class=\"detailOpcsRemk ui green label\" data-id=\"opcsRemk-{0}\">客戶注意事項</a>".FormatThis(rowModalID)
                                , GetShipStatusName(DT.Rows[row]["ShipStatus"].ToString(), rowModalID)
                                ));

                            //資材理貨(整批), 只有資材190顯示
                            if (_menuID.Equals("190"))
                            {
                                string btnA = "<button type=\"button\" class=\"ui olive mini button setAllShip\" data-target=\"A_{0}\">全A區</button>".FormatThis(
                                    _Order_FID + _Order_SID);
                                string btnB = "<button type=\"button\" class=\"ui grey mini button setAllShip\" data-target=\"B_{0}\">全B區</button>".FormatThis(
                                    _Order_FID + _Order_SID);
                                html.Append("<td class=\"center aligned\" style=\"width:10%\">{0}</td>".FormatThis(
                                    btnA + btnB
                                    ));
                            }

                            //特別注意事項
                            html.Append("            <td class=\"right aligned\" style=\"width:10%\"><a href=\"#!\" class=\"detailShown\" data-target=\"{0}{1}\"><i class=\"material-icons\">unfold_more</i></a></td>"
                                .FormatThis(_Order_FID, _Order_SID));
                            html.Append("        </tr>");
                            html.Append("    </table>");

                            //<!-- Modal Start -->
                            html.Append("<div id=\"opcsRemk-{0}\" class=\"ui fullscreen modal\">".FormatThis(rowModalID));
                            html.Append("<i class=\"close icon\"></i>");
                            html.Append("<div class=\"header\">客戶注意事項</div>");
                            html.Append("<div class=\"scrolling content\">{0}</div>".FormatThis(_orderRemark.Replace("\n", "<br/>")));
                            html.Append("</div>");
                            //<!--  Modal End -->

                            html.Append("</td>");

                            //因datatables的元件限制,必須填入相對數量的欄位
                            for (int col = 0; col < (colCnt - 1); col++)
                            {
                                html.Append("<td style=\"display:none;\">&nbsp;</td>");
                            }

                            html.Append("</tr>");



                        }

                        //*** 填入Html ***
                        //定義收合用class
                        html.Append("<tr class=\"{0}{1}\">".FormatThis(_Order_FID, _Order_SID));

                        //[1] - 訂單序號
                        if (CheckColShow(1, allCol, deptRel))
                        {
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(_OrderSno));
                        }

                        //[2] - 品號品名
                        if (CheckColShow(2, allCol, deptRel))
                        {
                            html.Append("<td data-tooltip=\"{3}\" data-inverted=\"\" data-position=\"top left\"><a href=\"{1}\" target=\"_blank\" class=\"green-text\"><strong>{0}</strong></a><br/><span>{2}</span></td>".FormatThis(
                                DT.Rows[row]["ModelNo"]
                                , GetEfUrl(_CompID) + "opcsstatusGrid2.asp?s_MB001=" + HttpUtility.UrlEncode(modelNo)
                                , _modelName.Left(9)
                                , HttpUtility.HtmlEncode(_modelName)
                            ));
                        }


                        //[3] - 訂單未出數量
                        if (CheckColShow(3, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned\">{0}</td>".FormatThis(_unShip_OrderQty));
                        }


                        //[4] - 全部未出數量
                        if (CheckColShow(4, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned\"><a href=\"{1}\" target=\"_blank\" class=\"{2}\">{0}</a></td>".FormatThis(
                                _totalQty
                                , GetEfUrl(_CompID) + "opcsstatusGrid.asp?s_TD004=" + HttpUtility.UrlEncode(modelNo)
                                , _totalQty > (_stockQtyMain + _stockQty11) ? "red-text" : "green-text"
                            ));
                        }


                        //[5] - 庫存 Start ------
                        if (CheckColShow(5, allCol, deptRel))
                        {
                            html.Append("<td>");
                            html.Append("<div class=\"grey-text text-darken-2\">{2}：&nbsp;<a href=\"{1}\" target=\"_blank\" class=\"green-text\">{0}</a></div>".FormatThis(
                                      _stockQtyMain
                                      , GetEfUrl(_CompID) + "INVMCGrid3.asp?MC001=" + HttpUtility.UrlEncode(modelNo)
                                      , stockMainHeader(_CompID)));

                            //判斷公司別(注意與表頭的欄位對應)
                            switch (_CompID)
                            {
                                case "SH":
                                    //SH:14倉庫存
                                    html.Append("<div class=\"grey-text text-darken-2\">14：&nbsp;<a href=\"{1}\" target=\"_blank\" class=\"green-text\">{0}</a></div>".FormatThis(
                                        _stockQty14
                                        , GetEfUrl(_CompID) + "INVMCGrid3.asp?MC001=" + HttpUtility.UrlEncode(modelNo)));

                                    break;

                                default:
                                    //TW:11倉庫存
                                    html.Append("<div class=\"grey-text text-darken-2\">11：&nbsp;<a href=\"{1}\" target=\"_blank\" class=\"green-text\">{0}</a></div>".FormatThis(
                                        _stockQty11
                                        , GetEfUrl(_CompID) + "INVMCGrid3.asp?MC001=" + HttpUtility.UrlEncode(modelNo)));
                                    //TW:20倉庫存
                                    html.Append("<div class=\"grey-text text-darken-2\">20：&nbsp;<a href=\"{1}\" target=\"_blank\" class=\"green-text\">{0}</a></div>".FormatThis(
                                       DT.Rows[row]["StockQty_20"]
                                       , GetEfUrl(_CompID) + "INVMCGrid3.asp?MC001=" + HttpUtility.UrlEncode(modelNo)));

                                    break;
                            }
                            html.Append("</td>");
                        }

                        //庫存 End ------


                        //[6] - 不足量
                        if (CheckColShow(6, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned {1}\">{0}</td>".FormatThis(
                            _shortQty
                            , _shortQty < 0 ? "red-text" : "green-text"
                            ));
                        }


                        //[7] - 待入庫
                        if (CheckColShow(7, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned\">{0}</td>".FormatThis(_unStockQty));
                        }


                        //[8] - 預計進
                        if (CheckColShow(8, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned {2}\" title=\"{3}\"><a href=\"{1}\" target=\"_blank\" class=\"green-text\">{0}</a></td>".FormatThis(
                                _preInQty
                                , GetEfUrl(_CompID) + "opcsstatus_in_detail.asp?TD004=" + HttpUtility.UrlEncode(modelNo)
                                , (_shortQty + _unStockQty + _preInQty) < 0 ? "yellow-bg lighten-2" : ""
                                , (_shortQty + _unStockQty + _preInQty) < 0 ? HttpUtility.HtmlEncode("(不足量 + 待入庫 + 預計進) < 0") : ""
                                ));
                        }


                        //[9] - 計劃進
                        if (CheckColShow(9, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned\">{0}</td>".FormatThis(DT.Rows[row]["PlanInQty"]));
                        }


                        //[10] - 品號屬性
                        if (CheckColShow(10, allCol, deptRel))
                        {
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["ProdProperty"]));
                        }


                        //[11] - 預交日
                        if (CheckColShow(11, allCol, deptRel))
                        {
                            html.Append("<td class=\"center aligned\">{0}</td>".FormatThis(DT.Rows[row]["OrderPreDate"].ToString().ToDateString("yyyy<br>MM/dd")));
                        }


                        //[12] - 安全存量
                        if (CheckColShow(12, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned {1}\" title=\"{2}\">{0}</td>".FormatThis(
                                _safeQty_Main
                                , _safeQty_Main - _totalQty > 0 ? "yellow-bg lighten-2" : ""
                                , _safeQty_Main - _totalQty > 0 ? HttpUtility.HtmlEncode("(安全存量 - 全部未出數量) > 0") : ""
                                ));
                        }


                        //[13] - 儲位
                        if (CheckColShow(13, allCol, deptRel))
                        {
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["StockPos"]));
                        }


                        //[14] - 主供應商
                        if (CheckColShow(14, allCol, deptRel))
                        {
                            html.Append("<td><small>{0}<br/></small></td>".FormatThis(
                                DT.Rows[row]["Main_SupplierName"]
                                , DT.Rows[row]["Main_SupplierID"]));
                        }


                        //-- 採購/進貨(col*6) --
                        string _purFid = DT.Rows[row]["PUR_FID"].ToString();
                        string _purSid = DT.Rows[row]["PUR_SID"].ToString();

                        //[15] - 廠商
                        if (CheckColShow(15, allCol, deptRel))
                        {
                            html.Append("<td class=\"{1}\"><small>{0}</small></td>".FormatThis(
                                DT.Rows[row]["PurSupplier"]
                                , DT.Rows[row]["PurSupplierID"].ToString().Equals(DT.Rows[row]["Main_SupplierID"].ToString()) ? "" : "red-text"
                                ));
                        }


                        //[16] - 採購單號/採購預交日
                        if (CheckColShow(16, allCol, deptRel))
                        {
                            html.Append("<td>");
                            if (!string.IsNullOrEmpty(_purFid))
                            {
                                html.Append("<div class=\"purple-text text-darken-2\">{0}-{1}</div>".FormatThis(_purFid, _purSid));
                                html.Append("<div class=\"center aligned\">{0}</div>".FormatThis(DT.Rows[row]["PurPreDate"].ToString().ToDateString("yyyy/MM/dd")));
                            }
                            html.Append("</td>");
                        }


                        //[17] - 採購數量
                        if (CheckColShow(17, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned {1}\">{0}</td>".FormatThis(
                                    _purQty.Equals(0) ? "" : _purQty.ToString()
                                    , !_purQty.Equals(_unShip_OrderQty) ? "yellow-bg lighten-2" : ""
                                    ));
                        }


                        //[18] - 進貨數量
                        if (CheckColShow(18, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned\">{0}</td>".FormatThis(_getInQty.Equals(0) ? "" : _getInQty.ToString()));
                        }


                        //[19] - 未進貨數量
                        if (CheckColShow(19, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned\">{0}</td>".FormatThis(_unGetInQty.Equals(0) ? "" : _unGetInQty.ToString()));
                        }


                        //[20] - 產品圖號
                        if (CheckColShow(20, allCol, deptRel))
                        {
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
                            html.Append("<td class=\"center aligned\"><small>{0}</small></td>".FormatThis(_spDesc));
                        }


                        //-- 生產/入庫(col*4) --
                        //[21] - 製令單號
                        if (CheckColShow(21, allCol, deptRel))
                        {
                            string _makeFid = DT.Rows[row]["Make_FID"].ToString();
                            string _makeSid = DT.Rows[row]["Make_SID"].ToString();
                            string _makeSdate = DateTime.Now.AddDays(-365).ToString().ToDateString("yyyyMMdd");
                            string _makeEdate = DateTime.Now.AddDays(365).ToString().ToDateString("yyyyMMdd");

                            html.Append("<td><a href=\"{1}\" target=\"_blank\" class=\"green-text\">{0}</a></td>".FormatThis(
                                string.IsNullOrEmpty(_makeFid) ? "" : _makeFid + "-" + _makeSid
                                , GetEfUrl(_CompID) + "PLINETIMERecord2.asp?s_MOC_KD={0}&s_MOC_NO={1}&s_ITEM_NO={2}&s_TA009_1={3}&s_TA009_2={4}".FormatThis(
                                    _makeFid, _makeSid, HttpUtility.UrlEncode(modelNo)
                                    , _makeSdate, _makeEdate)
                                ));
                        }

                        //[22] - 實際完工日
                        if (CheckColShow(22, allCol, deptRel))
                        {
                            html.Append("<td>{0}</td>".FormatThis(DT.Rows[row]["FinishDate"].ToString().ToDateString("yyyy<br>MM/dd")));
                        }


                        //[23] - 製令狀態
                        if (CheckColShow(23, allCol, deptRel))
                        {
                            html.Append("<td>{0}{1}</td>".FormatThis(
                               GetMakeName(DT.Rows[row]["MakeStatus"].ToString())
                               , DT.Rows[row]["PurConfirm"].ToString().Equals("V") ? "<span class=\"red-text\">(本單作廢)</span>" : ""));
                        }


                        //[24] - 入庫量
                        if (CheckColShow(24, allCol, deptRel))
                        {
                            html.Append("<td class=\"right aligned\">{0}</td>".FormatThis(DT.Rows[row]["MakeStockQty"]));
                        }


                        //[25] - 資材理貨 Section Start
                        /*
                          data-id -> 由單別+單號+序號組成的編號
                          data-action -> 要執行的動作
                          data-warning -> 提示文字
                        */
                        if (CheckColShow(25, allCol, deptRel))
                        {
                            string _stockValue = DT.Rows[row]["StockValue"].ToString();
                            string _showArea = _stockValue;
                            string thisID = _Order_FID + _Order_SID + _OrderSno;

                            string btnA = "<button type=\"button\" class=\"ui olive mini button btnShip A_{1}\" data-id=\"{0}\" data-action=\"A\">A區</button>".FormatThis(
                                 thisID
                                 , _Order_FID + _Order_SID
                                );
                            string btnB = "<button type=\"button\" class=\"ui grey mini button btnShip B_{1}\" data-id=\"{0}\" data-action=\"B\" style=\"margin-top:2px;\">B區</button>".FormatThis(
                                thisID
                                 , _Order_FID + _Order_SID
                               );

                            string btnCancel = "<button type=\"button\" class=\"ui mini button btnShip\" data-id=\"{0}\" data-action=\"Cancel\" style=\"margin-top:2px;\">清空</button>".FormatThis(
                                thisID
                               );

                            //顯示文字 / 按鈕
                            html.Append("<td class=\"center aligned\"><div class=\"doShipment\">{0}{1}{2}</div></td>".FormatThis(
                                btnA + btnB
                                , string.IsNullOrWhiteSpace(_stockValue) || (_stockValue.Equals("N")) ? "" : btnCancel
                                , string.IsNullOrWhiteSpace(_stockValue) || (_stockValue.Equals("N"))
                                    ? ""
                                    : "<br/><b class=\"yellow-bg result\">" + _showArea + " 區</b>"
                                ));
                        }

                        //資材出貨 Section End


                        //[28] - 箱號
                        if (CheckColShow(28, allCol, deptRel))
                        {
                            string _boxValue = DT.Rows[row]["BoxValue"].ToString();
                            string thisID = _Order_FID + _Order_SID + _OrderSno;
                            string _boxInput = "<div class=\"ui action small input\"><input id=\"{1}\" type=\"text\" value=\"{0}\" placeholder=\"包裝資料\" maxlength=\"20\" style=\"width:60px;\"><button type=\"button\" class=\"ui small icon button btnBox\" data-target=\"{1}\"><i class=\"saveIcon save icon\"></i></button></div>"
                                .FormatThis(
                                 _boxValue
                                 , thisID
                                );

                            html.Append("<td class=\"center aligned\"><div class=\"doBoxing\"><span>{0} ~ {1}</span>{2}</div></td>".FormatThis(
                                DT.Rows[row]["BoxNoStart"], DT.Rows[row]["BoxNoEnd"]
                                , _boxInput
                                ));
                        }

                        //[26] - 客戶品號
                        if (CheckColShow(26, allCol, deptRel))
                        {
                            html.Append("<td><small>{0}</small></td>".FormatThis(DT.Rows[row]["CustModel"]));
                        }


                        //[27] - 產品特別注意事項
                        if (CheckColShow(27, allCol, deptRel))
                        {
                            string remark = DT.Rows[row]["ProdRemark"].ToString();
                            html.Append("<td>{0}</td>".FormatThis(
                                string.IsNullOrEmpty(remark) ? "" :
                                    "<a data-tooltip=\"{0}\" data-inverted=\"\" data-position=\"left center\"><i class=\"material-icons\">more_vert</i></a>".FormatThis(remark)));
                        }

                        //row end
                        html.Append("</tr>");
                    }


                    //output HTML
                    context.Response.Write(html.ToString());
                }
            }

            _data = null;

        }
        catch (Exception ex)
        {
            context.Response.Write("#fail#<h5>資料查詢時發生錯誤!!若持續看到此訊息,請聯絡系統管理人員.</h5>" + ex.Message.ToString() + ErrMsg);
        }

    }

    /// <summary>
    /// Check null
    /// </summary>
    /// <param name="reqVal"></param>
    /// <returns></returns>
    private bool CheckReqNull(string reqVal)
    {
        return string.IsNullOrEmpty(reqVal);
    }

    /// <summary>
    /// 判斷欄位是否要顯示
    /// </summary>
    /// <param name="colID">目的colID</param>
    /// <param name="allCol"></param>
    /// <param name="deptRel"></param>
    /// <returns></returns>
    private bool CheckColShow(int colID, List<Menu4000Repository.OpcsColumn> allCol, List<Menu4000Repository.OpcsDept> deptRel)
    {
        //整理
        var query = allCol
            .Where(fld => deptRel.Select(f => f.colID).Contains(fld.colID))
            .Where(fld => fld.colID.Equals(colID));

        return query.Count() > 0;
    }

    /// <summary>
    /// 回傳要合併的欄位數
    /// </summary>
    /// <param name="deptID"></param>
    /// <returns></returns>
    private int GetColumnCnt(string deptID)
    {
        switch (deptID)
        {
            case "150":
                //生產部
                return 24;

            case "151":
                //採購部
                return 20;

            case "190":
                //資材部
                return 22;

            case "999":
                return 19;

            default:
                return 28;
        }
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
    /// <param name="opcsNo"></param>
    /// <returns></returns>
    private string GetShipStatusName(string st, string opcsNo)
    {
        switch (st)
        {
            case "Y":
                return "<a class=\"ui blue label\">已出貨</a>";

            default:
                return "<a class=\"ui red label detailOpcsFlow\" data-id=\"{0}\">未出貨</a>".FormatThis(opcsNo);
        }
    }


    /// <summary>
    /// 舊版連結
    /// </summary>
    /// <param name="comp"></param>
    /// <returns></returns>
    private string GetEfUrl(string comp)
    {
        switch (comp)
        {
            case "SH":
                return "http://ef.prokits.com.tw/employee/shanghai/opcsstatus_v2/";

            default:
                return "http://ef.prokits.com.tw/employee/";
        }

    }

    /// <summary>
    /// 主倉庫存表頭
    /// </summary>
    /// <param name="comp"></param>
    /// <returns></returns>
    private string stockMainHeader(string comp)
    {
        switch (comp)
        {
            case "SH":
                return "12";

            default:
                return "01";
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
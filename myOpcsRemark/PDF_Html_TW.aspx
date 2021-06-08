<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PDF_Html_TW.aspx.cs" Inherits="myOpcsRemark_PDF_Html_TW" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>OPCS變更單表格下載</title>

    <asp:PlaceHolder ID="ph_pubCss" runat="server">
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/semantic.min.css" rel="stylesheet" />
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/_override.css?v=20201020" rel="stylesheet" />
    </asp:PlaceHolder>
    <style>
        .ui .table {
            border: 1px solid grey;
        }

            .ui .table tr > th {
                border: 1px solid grey;
            }

            .ui .table tr > td {
                border: 1px solid grey;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="ui vertical segment">
            <!-- Content Start -->
            <div>
                <h2 class="ui header center aligned">
                    <asp:Literal ID="lt_CompName" runat="server"></asp:Literal></h2>
                <h3 class="ui header center aligned">訂單變更單 -
                        <asp:Literal ID="lt_OrderTypeName" runat="server"></asp:Literal></h3>
                <div style="padding: 2px; text-align: left;">
                    製表日期：<%=DateTime.Today.ToShortDateString() %>
                </div>
                <div>
                    <table border="0" style="width: 100%; font-size: 16px; border-top: 1px solid #b8b5b5; border-left: 1px solid #b8b5b5; border-right: 1px solid #b8b5b5;">
                        <tr>
                            <td style="width: 90px; text-align: right;">訂單單別：</td>
                            <td>
                                <asp:Literal ID="lt_SO_FID" runat="server"></asp:Literal>
                                &nbsp;&nbsp;&nbsp;
                                <asp:Literal ID="lt_SO_TypeName" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">客戶代號：</td>
                            <td>
                                <asp:Literal ID="TE007" runat="server"></asp:Literal>
                                &nbsp;&nbsp;&nbsp;
                                <asp:Literal ID="TE007Name" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">客戶單號：</td>
                            <td>
                                <asp:Literal ID="TE015" runat="server"></asp:Literal>
                            </td>
                            <td>=>&nbsp;
                                <asp:Literal ID="TE115" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 90px; text-align: right;">訂單單號：</td>
                            <td>
                                <asp:Literal ID="lt_SO_SID" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">出貨廠別：</td>
                            <td>
                                <asp:Literal ID="TE010" runat="server"></asp:Literal>
                                &nbsp;&nbsp;&nbsp;
                                <asp:Literal ID="TE010Name" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">價格條件：</td>
                            <td>
                                <asp:Literal ID="TE016" runat="server"></asp:Literal>
                            </td>
                            <td>=>&nbsp;
                                <asp:Literal ID="TE116" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 90px; text-align: right;">變更版次：</td>
                            <td>
                                <asp:Literal ID="TE003" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">變更原因：</td>
                            <td>
                                <asp:Literal ID="TE006" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">課稅別：</td>
                            <td>
                                <asp:Literal ID="TE018" runat="server"></asp:Literal>
                            </td>
                            <td>=>&nbsp;
                                <asp:Literal ID="TE118" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 90px; text-align: right;">單據日期：</td>
                            <td>
                                <asp:Literal ID="TE004" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">交易幣別：</td>
                            <td>
                                <asp:Literal ID="TE011" runat="server"></asp:Literal>
                                &nbsp;=>&nbsp;
                                <asp:Literal ID="TE111" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">部門：</td>
                            <td>
                                <asp:Literal ID="TE008Name" runat="server"></asp:Literal>
                            </td>
                            <td>=>&nbsp;
                                <asp:Literal ID="TE108Name" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 90px; text-align: right;">整張結案：</td>
                            <td>
                                <asp:Literal ID="TE005" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">匯率：</td>
                            <td>
                                <asp:Literal ID="TE012" runat="server"></asp:Literal>
                                &nbsp;=>&nbsp;
                                <asp:Literal ID="TE112" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">業務人員：</td>
                            <td>
                                <asp:Literal ID="TE009Name" runat="server"></asp:Literal>
                            </td>
                            <td>=>&nbsp;
                                <asp:Literal ID="TE109Name" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 90px; text-align: right;">確認碼：</td>
                            <td>
                                <asp:Literal ID="TE029" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">付款條件：</td>
                            <td colspan="2">
                                <asp:Literal ID="TE017" runat="server"></asp:Literal>
                                &nbsp;=>&nbsp;
                                <asp:Literal ID="TE117" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 90px; text-align: right;">確認者：</td>
                            <td>
                                <asp:Literal ID="lt_CfmWho" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 90px; text-align: right; font-size: 14px;">新送貨地址：</td>
                            <td colspan="4">
                                <div>
                                    <asp:Literal ID="TE013" runat="server"></asp:Literal>
                                </div>
                                <div>
                                    <asp:Literal ID="TE014" runat="server"></asp:Literal>
                                </div>
                            </td>
                            <td style="width: 90px; text-align: right;">營業稅率：</td>
                            <td>
                                <asp:Literal ID="TE040" runat="server"></asp:Literal>
                                %
                                &nbsp;=>&nbsp;
                                <asp:Literal ID="TE136" runat="server"></asp:Literal>
                                %
                            </td>
                        </tr>
                        <tr>
                            <td style="width: 90px; text-align: right; font-size: 14px;">原送貨地址：</td>
                            <td colspan="4">
                                <div>
                                    <asp:Literal ID="TE113" runat="server"></asp:Literal>
                                </div>
                                <div>
                                    <asp:Literal ID="TE114" runat="server"></asp:Literal>
                                </div>
                            </td>
                            <td style="width: 90px; text-align: right;">材積單位：</td>
                            <td>
                                <asp:Literal ID="TE049" runat="server"></asp:Literal>
                                &nbsp;=>&nbsp;
                                <asp:Literal ID="TE143" runat="server"></asp:Literal>
                            </td>
                        </tr>
                    </table>
                </div>
                <div>
                    <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items">
                        <LayoutTemplate>
                            <table class="ui large celled very compact table">
                                <thead>
                                    <tr>
                                        <th class="center aligned" style="width: 9%;">
                                            <div>變更序號</div>
                                            <div>訂單序號</div>
                                        </th>
                                        <th style="width: 30%;">
                                            <div>品號</div>
                                            <div>品名</div>
                                            <div>規格</div>
                                            <div>客戶品號</div>
                                        </th>
                                        <th class="right aligned" style="width: 8%;">
                                            <div>訂購數量</div>
                                            <div>贈品量</div>
                                            <div>單位</div>
                                            <div>小單位</div>
                                        </th>
                                        <th style="width: 5%;">已出<br />
                                            數量</th>
                                        <th style="width: 10%;">
                                            <div class="right aligned">訂單單價</div>
                                            <div class="right aligned">折扣率</div>
                                            <div class="right aligned">訂單金額</div>
                                            <div class="left aligned">包裝方式</div>
                                        </th>
                                        <th style="width: 10%;">
                                            <div>預交日</div>
                                            <div>交貨庫別</div>
                                            <div class="right aligned">毛重(Kg)</div>
                                            <div class="right aligned">材積</div>
                                        </th>
                                        <th class="center aligned" style="width: 8%;">儲位</th>
                                        <th style="width: 20%;">
                                            <div>專案代號</div>
                                            <div>指定結案</div>
                                            <div>變更原因</div>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:PlaceHolder ID="ph_Items" runat="server" />
                                </tbody>
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr class="top aligned">
                                <td style="<%#Get_StyleLine(Eval("Lv").ToString())%>">
                                    <div style="text-align: left;"><%#Eval("TF004") %></div>
                                    <div style="text-align: right;"><%#Eval("StatMark") %></div>
                                </td>
                                <td style="<%#Get_StyleLine(Eval("Lv").ToString())%>">
                                    <!--品號-->
                                    <div><%#Eval("TF005").ToString().Trim() %></div>
                                    <div><%#Eval("TF006") %></div>
                                    <div><%#Eval("TF007") %></div>
                                    <div><%#Eval("TF016") %></div>
                                </td>
                                <td style="text-align: right; <%#Get_StyleLine(Eval("Lv").ToString())%>">
                                    <!--訂購數量-->
                                    <div><%#Math.Round(Convert.ToDecimal(Eval("TF009")), 0) %></div>
                                    <div><%#Math.Round(Convert.ToDecimal(Eval("TF020")), 0) %></div>
                                    <div><%#Eval("TF010") %></div>
                                    <div><%#Eval("TF012") %></div>
                                </td>
                                <td style="text-align: center; <%#Get_StyleLine(Eval("Lv").ToString())%>">
                                    <!--已出數量-->
                                    <%#Math.Round(Convert.ToDecimal(Eval("TF123")), 0) %>
                                </td>
                                <td style="text-align: right; <%#Get_StyleLine(Eval("Lv").ToString())%>">
                                    <!--訂單單價-->
                                    <div><%#Math.Round(Convert.ToDecimal(Eval("TF013")), 2) %></div>
                                    <div><%#Math.Round(Convert.ToDecimal(Eval("TF021"))*100, 0) %>%</div>
                                    <!--訂單金額-->
                                    <div><%#Math.Round(Convert.ToDecimal(Eval("TF014")), 2) %></div>
                                    <div><%#Eval("TF023") %></div>
                                </td>
                                <td style="<%#Get_StyleLine(Eval("Lv").ToString())%>">
                                    <!--預交日-->
                                    <div style="text-align: left;"><%#Eval("TF015").ToString().ToDateString_ERP("/") %></div>
                                    <div style="text-align: left;"><%#Eval("TF008") %></div>
                                    <div style="text-align: right;"><%#Math.Round(Convert.ToDecimal(Eval("TF024")), 2) %></div>
                                    <div style="text-align: right;"><%#Eval("TF025") %></div>
                                </td>
                                <td style="text-align: center; <%#Get_StyleLine(Eval("Lv").ToString())%>">
                                    <!--儲位-->
                                    <%#Eval("NewStkPos") %>
                                </td>
                                <td style="<%#Get_StyleLine(Eval("Lv").ToString())%>">
                                    <div><%#Eval("TF022") %></div>
                                    <div><%#Eval("TF017") %></div>
                                    <div><%#Eval("TF018") %></div>
                                </td>
                            </tr>
                        </ItemTemplate>
                        <EmptyDataTemplate>
                            <div class="ui placeholder segment">
                                <div class="ui icon header">
                                    <i class="wheelchair icon"></i>
                                    ERP查無資料,請重新確認.
                                </div>
                            </div>
                        </EmptyDataTemplate>
                    </asp:ListView>
                </div>
                <asp:Panel ID="pl_MicTxt" runat="server">
                    <table border="0" style="width: 100%">
                        <tr>
                            <td colspan="6" style="height: 10px;">&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="text-align: center;" colspan="3">正嘜</td>
                            <td style="text-align: center;" colspan="3">側嘜</td>
                        </tr>
                        <tr>
                            <td style="width: 10%;"></td>
                            <td style="width: 30%;">
                                <asp:Literal ID="lt_MicTxt1" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 10%;"></td>

                            <td style="width: 10%;"></td>
                            <td style="width: 30%;">
                                <asp:Literal ID="lt_MicTxt2" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 10%;"></td>
                        </tr>
                        <tr>
                            <td colspan="6" style="height: 10px;">&nbsp;</td>
                        </tr>
                        <tr>
                            <td style="text-align: center;" colspan="3">變更前正嘜</td>
                            <td style="text-align: center;" colspan="3">變更前側嘜</td>
                        </tr>
                        <tr>
                            <td style="width: 10%;"></td>
                            <td style="width: 30%;">
                                <asp:Literal ID="lt_OldMicTxt1" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 10%;"></td>

                            <td style="width: 10%;"></td>
                            <td style="width: 30%;">
                                <asp:Literal ID="lt_OldMicTxt2" runat="server"></asp:Literal>
                            </td>
                            <td style="width: 10%;"></td>
                        </tr>
                    </table>
                </asp:Panel>
            </div>
            <!-- Content End -->
        </div>
    </form>
</body>
</html>

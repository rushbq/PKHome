<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PDF_Html_TW.aspx.cs" Inherits="myARdata_PDF_Html_TW" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>對帳單</title>

    <asp:PlaceHolder ID="ph_pubCss" runat="server">
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/semantic.min.css" rel="stylesheet" />
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/_override.css?v=20191020" rel="stylesheet" />
    </asp:PlaceHolder>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div class="ui vertical segment">
                <h2 class="ui header center aligned">寶工實業股份有限公司</h2>
                <h3 class="ui header center aligned">應收帳款對帳單</h3>

                <div>
                    <table border="0" style="width: 100%">
                        <tr>
                            <td style="width: 50px;">地址：
                            </td>
                            <td>
                                <asp:Literal ID="lt_ZipCode" runat="server"></asp:Literal>
                            </td>
                            <td style="text-align: center">To.會計
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td colspan="2">
                                <asp:Literal ID="lt_Addr" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td colspan="2">
                                <asp:Literal ID="lt_AddrRemark" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td>台端：</td>
                            <td colspan="2">
                                <asp:Label ID="lb_Cust" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>FAX：</td>
                            <td colspan="2">
                                <asp:Literal ID="lt_Fax" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td>TEL：</td>
                            <td colspan="2">
                                <asp:Literal ID="lt_Tel" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="3" style="height: 30px"></td>
                        </tr>
                        <tr>
                            <td colspan="2">製表日期:
                                <asp:Literal ID="lt_today" runat="server"></asp:Literal>
                            </td>
                            <td style="text-align: center">期間:
                                <asp:Literal ID="lt_sDate" runat="server"></asp:Literal>
                                至
                                <asp:Literal ID="lt_eDate" runat="server"></asp:Literal>
                            </td>
                        </tr>
                    </table>
                </div>
                <div>
                    <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items">
                        <LayoutTemplate>
                            <table class="ui celled striped table">
                                <thead>
                                    <tr>
                                        <th>結帳日期</th>
                                        <th>發票號碼</th>
                                        <th>憑證號碼</th>
                                        <th>付款條件</th>
                                        <th>預計收款日</th>
                                        <th>幣別</th>
                                        <th>原幣應收帳款</th>
                                        <th>原幣未收帳款</th>
                                        <th>備註</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:PlaceHolder ID="ph_Items" runat="server" />
                                </tbody>
                            </table>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <%#Eval("ArDate").ToString().ToDateString_ERP("/") %>
                                </td>
                                <td>
                                    <%#Eval("BillNo") %>
                                </td>
                                <td>
                                    <%#Eval("AT_Fid") %>-<%#Eval("AT_Sid") %>-<%#Eval("AT_Tid") %>
                                </td>
                                <td>
                                    <%#Eval("TermName") %>
                                </td>
                                <td>
                                    <%#Eval("PreGetDay").ToString().ToDateString_ERP("/") %>
                                </td>
                                <td>
                                    <%#Eval("Currency") %>
                                </td>
                                <td class="right aligned">
                                    <%#(Convert.ToDouble(Eval("Price")) + Convert.ToDouble(Eval("TaxPrice"))).ToString().ToMoneyString() %>
                                </td>
                                <td class="right aligned">
                                    <%#(Convert.ToDouble(Eval("Price")) + Convert.ToDouble(Eval("TaxPrice")) - Convert.ToDouble(Eval("GetPrice"))).ToString().ToMoneyString() %>
                                </td>
                                <td>
                                    <%#Eval("OrderRemark") %>
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
                <hr />
                <div>
                    <table border="0" style="width: 100%">
                        <tr>
                            <td style="text-align: right; width: 100px;">前期未收款</td>
                            <td style="text-align: right; width: 100px;">
                                <asp:Label ID="lb_PrePrice" runat="server"></asp:Label>
                            </td>
                            <td style="text-align: right; width: 60px;">
                                <asp:Literal ID="lt_PreCnt" runat="server"></asp:Literal>
                                筆
                            </td>
                            <td></td>
                            <td style="text-align: right; width: 100px;">本幣未稅金額</td>
                            <td style="text-align: right; width: 100px;">
                                <asp:Label ID="lb_TotalPrice_NoTax" runat="server"></asp:Label>
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td style="text-align: right; width: 100px;">本期應收</td>
                            <td style="text-align: right; width: 100px;">
                                <asp:Label ID="lb_TotalPrice" runat="server"></asp:Label>
                            </td>
                            <td style="text-align: right; width: 60px;">
                                <asp:Literal ID="lt_Cnt" runat="server"></asp:Literal>
                                筆
                            </td>
                            <td></td>
                            <td style="text-align: right; width: 100px;">本幣稅額</td>
                            <td style="text-align: right; width: 100px;">
                                <asp:Label ID="lb_TotalTaxPrice" runat="server"></asp:Label>
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td style="text-align: right; width: 100px;">總應收</td>
                            <td style="text-align: right; width: 100px;">
                                <asp:Label ID="lb_AllPrice" runat="server"></asp:Label>
                            </td>
                            <td style="text-align: right; width: 60px;">
                                <asp:Literal ID="lt_TotalCnt" runat="server"></asp:Literal>
                                筆

                            </td>
                            <td></td>
                            <td style="text-align: right; width: 100px;"></td>
                            <td style="text-align: right; width: 100px;"></td>
                            <td></td>
                        </tr>
                    </table>
                </div>

                <hr />
            </div>
        </div>
    </form>
</body>
</html>

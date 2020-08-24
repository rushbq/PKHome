<%@ Page Language="C#" AutoEventWireup="true" CodeFile="PDF_Html_SH.aspx.cs" Inherits="myARdata_PDF_Html_SH" %>

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
                <h2 class="ui header center aligned">上海宝工工具有限公司</h2>
                <h3 class="ui header center aligned">应收帐款对帐单</h3>

                <div>
                    <table border="0" style="width: 100%">
                        <tr>
                            <td>To.会计
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td style="width: 90px;">地址：
                            </td>
                            <td>
                                <asp:Literal ID="lt_ZipCode" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td>
                                <asp:Literal ID="lt_Addr" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td></td>
                            <td>
                                <asp:Literal ID="lt_AddrRemark" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td>名称：</td>
                            <td>
                                <asp:Label ID="lb_Cust" runat="server"></asp:Label>
                            </td>
                        </tr>
                        <tr>
                            <td>FAX：</td>
                            <td>
                                <asp:Literal ID="lt_Fax" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td>TEL：</td>
                            <td>
                                <asp:Literal ID="lt_Tel" runat="server"></asp:Literal>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="height: 30px"></td>
                        </tr>
                        <tr>
                            <td>制表日期:
                                <asp:Literal ID="lt_today" runat="server"></asp:Literal>
                            </td>
                            <td style="text-align: center">期间:
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
                                        <th>结帐日期</th>
                                        <th>发票号码</th>
                                        <th>凭证号码</th>
                                        <th>付款条件</th>
                                        <th>预计收款日</th>
                                        <th>币别</th>
                                        <th>原币应收帐款</th>
                                        <th>原币未收帐款</th>
                                        <th>备注</th>
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
                                    <%#Eval("InvoiceNo") %>
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
                            <td style="text-align: right; width: 100px;">预收款</td>
                            <td style="text-align: right; width: 100px;">
                                <asp:Label ID="lb_PreGetPrice" runat="server"></asp:Label>
                            </td>
                            <td style="text-align: right; width: 60px;"></td>
                            <td></td>
                            <td style="text-align: right; width: 100px;"></td>
                            <td style="text-align: right; width: 100px;"></td>
                            <td></td>
                        </tr>
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
                            <td style="text-align: right; width: 100px;"></td>
                            <td style="text-align: right; width: 100px;"></td>
                            <td></td>
                        </tr>
                        <tr>
                            <td style="text-align: right; width: 100px;">本期应收</td>
                            <td style="text-align: right; width: 100px;">
                                <asp:Label ID="lb_TotalPrice" runat="server"></asp:Label>
                            </td>
                            <td style="text-align: right; width: 60px;">
                                <asp:Literal ID="lt_Cnt" runat="server"></asp:Literal>
                                筆
                            </td>
                            <td></td>
                            <td style="text-align: right; width: 100px;"></td>
                            <td style="text-align: right; width: 100px;"></td>
                            <td></td>
                        </tr>
                        <tr>
                            <td style="text-align: right; width: 100px;">总应收</td>
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
                <div>
                    <p>请贵公司与收款日前将总应收款汇入我司指定帐户。 </p>
                    <p>帐户资料:上海宝工工具有限公司 91310115755706677X 中国银行康桥支行</p>
                    <p>上海宝工工具有限公司 TEL:021-68183050</p>
                </div>
                <hr />
            </div>
        </div>
    </form>
</body>
</html>

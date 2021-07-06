<%@ Page Language="C#" AutoEventWireup="true" CodeFile="OPCS_form.aspx.cs" Inherits="myOpcsRemark_OPCS_form" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>OPCS訂單</title>
</head>
<body>
    <form id="form1" runat="server">
        <div width="780">
            <div style="text-align: center;">
                <div style="font-size: 12pt;">
                    <b>
                        <asp:Literal ID="lt_CompName" runat="server"></asp:Literal></b>
                </div>
                <div>&nbsp;</div>
                <div style="font-size: 10pt;">
                    <b>
                        <asp:Literal ID="lt_OrderTypeName" runat="server"></asp:Literal></b>
                </div>
            </div>
            <table width="100%" border="1" style="border-collapse: collapse;">
                <!-- 表頭 -->
                <thead>
                    <tr>
                        <td width="240" height="40" colspan="4">訂單日期：
                            <asp:Literal ID="lt_OrderDate" runat="server"></asp:Literal>
                        </td>
                        <td width="380" colspan="7">單據日期：
                            <asp:Literal ID="lt_CheckDate" runat="server"></asp:Literal>
                        </td>
                        <td width="278" colspan="3">訂單編號：
                            <asp:Literal ID="lt_SO_FID" runat="server"></asp:Literal>-<asp:Literal ID="lt_SO_SID" runat="server"></asp:Literal>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4" height="40">客戶名稱：
                            <asp:Literal ID="lt_CustName" runat="server"></asp:Literal>
                        </td>
                        <td colspan="7">客戶編號：
                            <asp:Literal ID="lt_CustID" runat="server"></asp:Literal>
                        </td>
                        <td colspan="3">客戶單號：
                            <asp:Literal ID="lt_CustPO" runat="server"></asp:Literal>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4" height="40">預定日期：
                            <asp:Literal ID="lt_PreDate" runat="server"></asp:Literal>
                        </td>
                        <td colspan="7">海運:〇&nbsp;&nbsp;
                            空運:〇&nbsp;&nbsp;
                            快遞:〇&nbsp;&nbsp;
                            貨運:〇&nbsp;&nbsp;
                        </td>
                        <td colspan="3">業務人員：
                            <asp:Literal ID="lt_SalesWho" runat="server"></asp:Literal>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="4" height="40">交易條件：
                            <asp:Literal ID="lt_TradeTerm" runat="server"></asp:Literal>
                        </td>
                        <td colspan="7" style="font-size: 9pt;">付款方式/幣別：
                            <asp:Literal ID="lt_PayTerm" runat="server"></asp:Literal>&nbsp;/&nbsp;                            
                            <asp:Literal ID="lt_TradeCurrency" runat="server"></asp:Literal>
                        </td>
                        <td colspan="3">確認者：
                            <asp:Literal ID="lt_CfmWho" runat="server"></asp:Literal>
                        </td>
                    </tr>
                </thead>
                <!-- TW 表身 -->
                <asp:ListView ID="lvDataList_TW" runat="server" ItemPlaceholderID="ph_Items" Visible="false">
                    <LayoutTemplate>
                        <tr style="font-size: 9pt;">
                            <td width="40" style="text-align: center;">序號</td>
                            <td width="110">產品編號</td>
                            <td width="50" style="text-align: center">訂購<br />
                                數量</td>
                            <td width="40" style="text-align: center">贈品<br />
                                量</td>

                            <td width="50" style="text-align: center">現有<br />
                                庫存</td>
                            <td width="40" style="text-align: center">庫別</td>
                            <td width="70">廠商<br />
                                簡稱</td>
                            <td width="55" style="font-size: 7pt;">採購單<br />
                                編號</td>
                            <td width="55" style="font-size: 7pt;">採購單<br />
                                預交日</td>
                            <td width="55" style="text-align: center">儲位</td>
                            <td width="55" style="text-align: center">箱號</td>

                            <td width="80">客戶品號</td>
                            <td width="88">條碼編號</td>
                            <td width="110" style="font-size: 7pt;">客戶產品特別<br />
                                注意事項</td>
                        </tr>
                        <tbody>
                            <asp:PlaceHolder ID="ph_Items" runat="server" />
                        </tbody>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr style="font-size: 7pt;">
                            <td height="35" style="text-align: center;">
                                <%#Eval("OrderSid") %>
                            </td>
                            <td style="font-size: 8pt;">
                                <%#Eval("ModelNo") %>
                            </td>
                            <td style="text-align: right; font-size: 9pt;">
                                <!--訂購數量-->
                                <%#Eval("OrderQty") %>
                            </td>
                            <td style="text-align: right; font-size: 9pt;">
                                <!--贈品量-->
                                <%#Eval("GiftQty") %>
                            </td>
                            <td style="text-align: right; font-size: 9pt;">
                                <!--現有庫存-->
                                <%#Eval("StockQty") %>
                            </td>
                            <td style="text-align: center; mso-number-format: \@">
                                <!--庫別-->
                                <%#Eval("StockType") %>
                            </td>
                            <td>
                                <!--廠商簡稱-->
                                <%#Eval("SupName") %>
                            </td>
                            <td>
                                <!--採購單編號-->
                            </td>
                            <td>
                                <!--採購單預交日期-->
                            </td>
                            <td style="text-align: center; font-size: 8pt;">
                                <!--儲位-->
                                <%#Eval("StockPos") %>
                            </td>
                            <td style="text-align: center; font-size: 8pt; mso-number-format: \@">
                                <!--箱號-->
                                <%#Eval("BoxNo") %>
                            </td>
                            <td>
                                <!--客戶品號-->
                                <%#Eval("CustModelNo") %>
                            </td>
                            <td style="mso-number-format: \@">
                                <!--條碼編號-->
                                <%#Eval("Barcode") %>
                            </td>
                            <td>
                                <!--客戶產品特別注意事項-->
                                <%#Eval("ProdRemark") %>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:ListView>

                <!-- SH 表身 -->
                <asp:ListView ID="lvDataList_SH" runat="server" ItemPlaceholderID="ph_Items" Visible="false">
                    <LayoutTemplate>
                        <tr style="font-size: 9pt;">
                            <td width="40" style="text-align: center;">序號</td>
                            <td width="110">產品編號</td>
                            <td width="50" style="text-align: center">訂購<br />
                                數量</td>
                            <td width="40" style="text-align: center">贈品<br />
                                量</td>

                            <td width="50" style="text-align: center">現有<br />
                                庫存</td>
                            <td width="40" style="text-align: center">已訂<br />
                                未交</td>
                            <td width="40" style="text-align: center">採購<br />
                                未進</td>
                            <td width="70">廠商<br />
                                簡稱</td>
                            <td width="55" style="font-size: 7pt;">採購單<br />
                                編號</td>
                            <td width="55" style="text-align: center">屬性</td>
                            <td width="55" style="text-align: center">箱號</td>

                            <td width="80">客戶品號</td>
                            <td width="88">條碼編號</td>
                            <td width="110" style="font-size: 7pt;">客戶產品特別<br />
                                注意事項</td>
                        </tr>
                        <tbody>
                            <asp:PlaceHolder ID="ph_Items" runat="server" />
                        </tbody>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr style="font-size: 7pt;">
                            <td height="35" style="text-align: center;">
                                <%#Eval("OrderSid") %>
                            </td>
                            <td style="font-size: 8pt;">
                                <%#Eval("ModelNo") %>
                            </td>
                            <td style="text-align: right; font-size: 9pt;">
                                <!--訂購數量-->
                                <%#Eval("OrderQty") %>
                            </td>
                            <td style="text-align: right; font-size: 9pt;">
                                <!--贈品量-->
                                <%#Eval("GiftQty") %>
                            </td>
                            <td style="text-align: right; font-size: 9pt;">
                                <!--現有庫存-->
                                <%#Eval("StockQty") %>
                            </td>
                            <td style="text-align: right; font-size: 9pt;">
                                <!--已訂未交(SH)-->
                                <%#Eval("unGiveQty") %>
                            </td>
                            <td style="text-align: right; font-size: 9pt;">
                                <!--採購未進(SH)-->
                                <%#Eval("unPurQty") %>
                            </td>
                            <td>
                                <!--廠商簡稱-->
                                <%#Eval("SupName") %>
                            </td>
                            <td>
                                <!--採購單編號-->
                            </td>
                            <td style="text-align: center; font-size: 8pt;">
                                <!--屬性(SH)-->
                                <%#Eval("Prop") %>
                            </td>
                            <td style="text-align: center; mso-number-format: \@; font-size: 8pt;">
                                <!--箱號-->
                                <%#Eval("BoxNo") %>
                            </td>
                            <td>
                                <!--客戶品號-->
                                <%#Eval("CustModelNo") %>
                            </td>
                            <td style="mso-number-format: \@">
                                <!--條碼編號-->
                                <%#Eval("Barcode") %>
                            </td>
                            <td>
                                <!--客戶產品特別注意事項-->
                                <%#Eval("ProdRemark") %>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:ListView>
            </table>
            <div style="width: 100%">
                <p>備註：</p>
                <p>
                    <asp:Literal ID="lt_OrderRemark" runat="server"></asp:Literal>
                </p>
                <p style="color: red">＊備註事項請特別注意</p>
                <p></p>
                <table width="100%">
                    <tr>
                        <td width="50%" colspan="7">
                            <asp:Literal ID="lt_MicTxt1" runat="server"></asp:Literal>
                            <div style="text-align: center">
                                <asp:Literal ID="lt_MicPic1" runat="server"></asp:Literal>
                            </div>
                        </td>
                        <td width="5%">&nbsp;</td>
                        <td width="45%" colspan="6">
                            <asp:Literal ID="lt_MicTxt2" runat="server"></asp:Literal>
                            <div style="text-align: center">
                                <asp:Literal ID="lt_MicPic2" runat="server"></asp:Literal>
                            </div>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </form>
</body>
</html>

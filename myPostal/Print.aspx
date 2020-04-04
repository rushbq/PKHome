<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Print.aspx.cs" Inherits="myPostal_Print" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>郵寄信件登記表</title>
    <asp:PlaceHolder ID="ph_pubCss" runat="server">
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/semantic.min.css" rel="stylesheet" />
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/_override.css?v=20191020" rel="stylesheet" />
    </asp:PlaceHolder>
    <style>
        .wrapper {
            width: 800px;
            margin: 0 auto;
        }

        thead > tr > th {
            text-align: center !important;
        }
    </style>
    <style>
        @media print {
            .NoPrint {
                display: none;
                visibility: hidden;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="NoPrint">
            <div class="ui attached segment">
                <div class="ui small form container">
                    <div class="three fields">
                        <div class="field">
                            <label>日期</label>
                            <div class="ui left icon input datepicker">
                                <asp:TextBox ID="filter_Date" runat="server" autocomplete="off"></asp:TextBox>
                                <i class="calendar alternate outline icon"></i>
                            </div>
                        </div>
                        <div class="field">
                            <label>郵式</label>
                            <asp:DropDownList ID="filter_PostType" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                        <div class="field">
                            <label>&nbsp;</label>
                            <asp:LinkButton ID="lbtn_Search" runat="server" CssClass="ui blue button" OnClick="lbtn_Search_Click"><i class="search icon"></i>查詢</asp:LinkButton>
                            <button type="button" class="ui green button" onclick="window.print()">開始列印</button>
                        </div>
                    </div>

                </div>
            </div>
        </div>
        <div style="height: 30px;"></div>

        <asp:PlaceHolder ID="ph_NoData" runat="server">
            <div class="ui placeholder segment">
                <div class="ui icon header">
                    <i class="search icon"></i>
                    請先篩選條件並查詢.
                </div>
            </div>
        </asp:PlaceHolder>
        <div class="wrapper">
            <asp:PlaceHolder ID="ph_Data" runat="server" Visible="false">
                <h3 style="text-align: center; padding-bottom: 10px;" class="ui header">寶工實業股份有限公司<br />
                    郵寄信件登記表 (<asp:Literal ID="lt_TypeName" runat="server"></asp:Literal>)
                </h3>
                <div class="ui grid">
                    <div class="four wide column">
                        <h4>日期：<asp:Literal ID="lt_headerDate" runat="server"></asp:Literal></h4>
                    </div>
                    <div class="eight wide column center aligned">
                        <h4>地址：新北市新店區民權路130巷7號5樓</h4>
                    </div>
                    <div class="four wide column right aligned">
                        <h4>電話：(02)2218-3233</h4>
                    </div>
                </div>

                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound">
                    <LayoutTemplate>
                        <table id="tableList" class="ui celled table" style="width: 100%;">
                            <thead>
                                <tr>
                                    <th class="collapsing">序號</th>
                                    <th>號碼</th>
                                    <th>姓名</th>
                                    <th>收件人地址</th>
                                    <th class="collapsing">重量</th>
                                    <th class="collapsing">郵資</th>
                                    <th>備註</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                            </tbody>
                            <tfoot>
                                <tr>
                                    <th colspan="5"></th>
                                    <th class="center aligned">
                                        <strong>
                                            <asp:Literal ID="lt_Total" runat="server"></asp:Literal></strong>
                                    </th>
                                    <th></th>
                                </tr>
                            </tfoot>
                        </table>
                    </LayoutTemplate>
                    <ItemTemplate>
                        <tr>
                            <td class="center aligned">
                                <asp:Literal ID="lt_Idx" runat="server"></asp:Literal>
                            </td>
                            <td class="center aligned">
                                <%#Eval("PostNo") %>
                            </td>
                            <td><%#Eval("ToWho") %></td>
                            <td><%#Eval("ToAddr") %></td>
                            <td class="center aligned">
                                <%#Eval("PackageWeight") %>
                            </td>
                            <td class="center aligned">
                                <%#Eval("PostPrice") %>
                            </td>
                            <td>&nbsp;</td>
                        </tr>
                    </ItemTemplate>
                    <EmptyDataTemplate>
                        <div class="ui placeholder segment">
                            <div class="ui icon header">
                                <i class="search icon"></i>
                                目前條件查無資料.
                            </div>
                        </div>
                    </EmptyDataTemplate>
                </asp:ListView>
            </asp:PlaceHolder>

        </div>

        <script src="<%=fn_Param.CDNUrl %>plugin/jQuery/jquery-3.3.1.min.js"></script>
        <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-2.4.0/semantic.min.js"></script>
        <script>
            $(function () {
                //init dropdown list
                $('select').dropdown();

            });
        </script>
        <%-- 日期選擇器 Start --%>
        <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
        <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
        <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js"></script>
        <script>
            $(function () {
                //載入datepicker
                $('.datepicker').calendar(calendarOpts_Range);
            });
        </script>
        <%-- 日期選擇器 End --%>
    </form>
</body>
</html>

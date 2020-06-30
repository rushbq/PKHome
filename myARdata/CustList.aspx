<%@ Page Title="對帳客戶清單" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="CustList.aspx.cs" Inherits="myARdata_CustList" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<%@ Register Src="Ascx_StepMenu.ascx" TagName="Ascx_Menu" TagPrefix="ucMenu" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">業務行銷</div>
                    <i class="right angle icon divider"></i>
                    <div class="section">應收帳款對帳</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        對帳客戶清單 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui attached segment grey-bg lighten-5">
            <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                <div class="ui negative message">
                    <div class="header">
                        Oops...發生了一點小問題
                    </div>
                    <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                </div>
            </asp:PlaceHolder>
            <!-- Steps menu -->
            <ucMenu:Ascx_Menu ID="Ascx_Menu1" runat="server" nowIndex="1" />
            <!-- 資料區 Start -->
            <div id="formData" class="ui small form attached green segment">
                <!-- 基本資料 S -->
                <div class="two fields">
                    <div class="field">
                        <label>資料庫</label>
                        <asp:Label ID="lb_DBS" runat="server" CssClass="ui blue basic large label"></asp:Label>
                    </div>
                    <div class="field">
                        <label>單據日查詢區間</label>
                        <asp:Label ID="lb_sDate" runat="server" CssClass="ui basic large label"></asp:Label>
                        ~<asp:Label ID="lb_eDate" runat="server" CssClass="ui basic large label"></asp:Label>
                        <asp:HiddenField ID="hf_sDate" runat="server" />
                        <asp:HiddenField ID="hf_eDate" runat="server" />
                    </div>
                </div>
                <!-- 基本資料 E -->
                <!-- 列表 S -->
                <div class="fields">
                    <div class="sixteen wide field">
                        <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand">
                            <LayoutTemplate>
                                <table id="listTable" class="ui celled striped table" style="width: 100%">
                                    <thead>
                                        <tr>
                                            <th>客戶代號</th>
                                            <th>客戶名稱</th>
                                            <th class="no-sort no-search">付款條件</th>
                                            <th class="no-sort center aligned">直接新增
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                                    </tbody>
                                </table>
                            </LayoutTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td class="center aligned">
                                        <%#Eval("CustID") %>
                                    </td>
                                    <td>
                                        <%#Eval("CustName") %>
                                    </td>
                                    <td>
                                        <%#Eval("TermName") %>
                                    </td>
                                    <td class="center aligned">
                                        <asp:LinkButton ID="lbtn_Add" runat="server" CssClass="ui blue icon button" ValidationGroup="List" CommandName="doInsert" OnClientClick="return confirm('確定新增?')"><i class="plus icon"></i></asp:LinkButton>
                                        <asp:HiddenField ID="hf_CustID" runat="server" Value='<%#Eval("CustID") %>' />
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
                </div>
                <!-- 列表 E -->
                <div class="ui grid">
                    <div class="six wide column">
                        <a href="<%=Page_SearchUrl %>" class="ui button"><i class="undo icon"></i>返回列表</a>
                    </div>
                    <div class="ten wide column right aligned">
                    </div>
                </div>
            </div>
            <!-- 資料區 End -->
        </div>
    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <%-- DataTable Start --%>
    <link href="https://cdn.datatables.net/1.10.13/css/jquery.dataTables.min.css" rel="stylesheet" />
    <script src="https://cdn.datatables.net/1.10.11/js/jquery.dataTables.min.js"></script>
    <script>
        $(function () {
            /*
             [使用DataTable]
             注意:標題欄須與內容欄數量相等
           */
            var table = $('#listTable').DataTable({
                "searching": true,  //搜尋
                "ordering": true,   //排序
                "paging": true,     //分頁
                "info": true,      //筆數資訊
                "pageLength": 5,   //每頁筆數
                "language": {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                //讓不排序的欄位在初始化時不出現排序圖
                "order": [],
                //欄位定義:css=no-sort不排序; css=no-search不搜尋
                "columnDefs": [
                    { "targets": 'no-sort', "orderable": false, },
                    { "targets": 'no-search', "searchable": false }
                ],
                //捲軸設定
                "scrollY": '60vh',
                "scrollCollapse": true,
                "scrollX": false

            });

        });
    </script>
    <%-- DataTable End --%>
</asp:Content>


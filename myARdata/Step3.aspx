<%@ Page Title="發送對帳單Step3" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Step3.aspx.cs" Inherits="myARdata_ImportStep3" %>

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
                        發送對帳單 - 
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
            <ucMenu:Ascx_Menu ID="Ascx_Menu1" runat="server" nowIndex="3" />
            <!-- 資料區 Start -->
            <div id="formData" class="ui small form attached green segment">
                <!-- 基本資料 S -->
                <div class="three fields">
                    <div class="field">
                        <label>追蹤碼</label>
                        <asp:Label ID="lb_TraceID" runat="server" CssClass="ui red basic large label"></asp:Label>
                        <asp:HiddenField ID="hf_TraceID" runat="server" />
                    </div>
                    <div class="field">
                        <label>客戶</label>
                        <asp:Label ID="lb_Cust" runat="server" CssClass="ui green basic large label"></asp:Label>
                    </div>
                    <div class="field">
                        <label>單據日查詢區間</label>
                        <asp:Label ID="lb_sDate" runat="server" CssClass="ui basic large label"></asp:Label>
                        ~<asp:Label ID="lb_eDate" runat="server" CssClass="ui basic large label"></asp:Label>
                    </div>
                </div>
                <div class="four fields">
                    <div class="field">
                        <label>前期未收款</label>
                        <asp:Label ID="lb_unGetPrice" runat="server" CssClass="ui basic large label"></asp:Label>
                        &nbsp;(<asp:Literal ID="lt_unGetCnt" runat="server"></asp:Literal>
                        筆)
                    </div>
                    <div class="field">
                        <label>本幣未稅金額</label>
                        <asp:Label ID="lb_TotalPrice_NoTax" runat="server" CssClass="ui basic large label"></asp:Label>
                    </div>
                    <div class="field">
                        <label>本期應收</label>
                        <asp:Label ID="lb_TotalPrice" runat="server" CssClass="ui basic large label"></asp:Label>
                        &nbsp;(<asp:Literal ID="lt_Cnt" runat="server"></asp:Literal>
                        筆)
                    </div>
                    <div class="field">
                        <label>本幣稅額</label>
                        <asp:Label ID="lb_TotalTaxPrice" runat="server" CssClass="ui basic large label"></asp:Label>
                    </div>
                </div>
                <!-- 基本資料 E -->
                <!-- 列表 S -->
                <div class="fields">
                    <div class="sixteen wide field">
                        <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items">
                            <LayoutTemplate>
                                <table id="listTable" class="ui celled striped table" style="width: 100%">
                                    <thead>
                                        <tr>
                                            <th>結帳日期</th>
                                            <th>發票號碼</th>
                                            <th>憑證號碼</th>
                                            <th class="no-sort">付款條件</th>
                                            <th>預計收款日</th>
                                            <th class="no-sort">幣別</th>
                                            <th class="no-sort">原幣應收帳款</th>
                                            <th class="no-sort">原幣未收帳款</th>
                                            <th class="no-sort">備註</th>
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
                                        <%#Eval("ArDate") %>
                                    </td>
                                    <td>
                                        <%#Eval("BillNo") %>&nbsp;<%#Eval("InvoiceNo") %>
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
                </div>

                <!-- 列表 E -->
                <!-- 按鈕 S -->
                <div class="ui grid">
                    <div class="six wide column">
                        <a href="<%=FuncPath() %>/Step2/<%=Req_DataID %>" class="ui grey button"><i class="undo icon"></i>回上一步</a>
                        <a href="<%=Page_SearchUrl %>" class="ui button"><i class="undo icon"></i>返回列表</a>
                    </div>
                    <div class="five wide column center aligned">
                        <a href="<%=fn_Param.RefUrl %>PKHome/ARData/<%=hf_TraceID.Value %>/<%=hf_CustID.Value %>.pdf" target="_blank" class="ui orange button"><i class="file pdf outline icon"></i>檢查PDF</a>
                    </div>
                    <div class="five wide column right aligned">
                        <button id="doNext" type="button" class="ui green button">下一步<i class="chevron right icon"></i></button>
                        <asp:Button ID="btn_Next" runat="server" Text="next" OnClick="btn_Next_Click" Style="display: none;" />
                    </div>
                    <asp:HiddenField ID="hf_DataID" runat="server" />
                    <asp:HiddenField ID="hf_CustID" runat="server" />
                    <asp:HiddenField ID="hf_CustName" runat="server" />
                </div>
                <!-- 按鈕 E -->

                <!-- 發信名單 S -->
                <div class="ui grid">
                    <div class="sixteen wide column">
                        <h4 class="ui horizontal divider header">
                            <i class="mail icon"></i>
                            發信名單
                        </h4>
                        <asp:ListView ID="lvAddressBook" runat="server" ItemPlaceholderID="ph_Items" GroupPlaceholderID="ph_Group" GroupItemCount="4">
                            <LayoutTemplate>
                                <table class="ui celled striped table">
                                    <tbody>
                                        <asp:PlaceHolder ID="ph_Group" runat="server" />
                                    </tbody>
                                </table>
                            </LayoutTemplate>
                            <GroupTemplate>
                                <tr>
                                    <asp:PlaceHolder ID="ph_Items" runat="server" />
                                </tr>
                            </GroupTemplate>
                            <ItemTemplate>
                                <td>
                                    <%#Eval("Email") %>
                                </td>
                            </ItemTemplate>
                            <EmptyDataTemplate>
                                <div class="ui placeholder segment">
                                    <div class="ui icon header">
                                        <i class="wheelchair icon"></i>
                                        沒有Email收件人,請前往客戶基本資料設定.
                                    </div>
                                </div>
                            </EmptyDataTemplate>
                        </asp:ListView>
                    </div>
                </div>

                <!-- 發信名單 E -->
            </div>
            <!-- 資料區 End -->
        </div>
    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //Save Click
            $("#doNext").click(function () {
                if (!confirm("確認要發送?")) {
                    return false;
                }

                //trigger
                $("#formData").addClass("loading");
                $("#MainContent_btn_Next").trigger("click");
            });

        });
    </script>

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
                "searching": false,  //搜尋
                "ordering": true,   //排序
                "paging": false,     //分頁
                "info": true,      //筆數資訊
                "language": {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                //讓不排序的欄位在初始化時不出現排序圖
                "order": [],
                //自訂欄位
                "columnDefs": [{
                    "targets": 'no-sort',
                    "orderable": false,
                }],
                //捲軸設定
                "scrollY": '50vh',
                "scrollCollapse": true,
                "scrollX": false

            });

        });

    </script>
    <%-- DataTable End --%>
</asp:Content>


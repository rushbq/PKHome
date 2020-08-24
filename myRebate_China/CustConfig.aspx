<%@ Page Title="客戶返利統計-China" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="CustConfig.aspx.cs" Inherits="myRebate_China_CustConfig" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<%@ Import Namespace="Resources" %>
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
                    <div class="section">客戶返利統計-China</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        子母公司維護
                    </div>
                </div>
            </div>
            <div class="right menu">
                <a class="item" href="<%=Page_SearchUrl %>">
                    <i class="undo icon"></i>
                    <span class="mobile hidden">返回</span>
                </a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <!-- Search Start -->
        <div class="ui orange attached segment">
            <div class="ui small form">
                <div class="fields">
                    <div class="four wide field">
                        <label>子公司&nbsp;<small>(ex:C755281-01)</small></label>
                        <div class="ui fluid search ac-Cust">
                            <div class="ui input">
                                <asp:TextBox ID="tb_Cust" runat="server" CssClass="prompt" placeholder="輸入客戶代號或名稱關鍵字"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <div class="four wide field">
                        <label>母公司&nbsp;<small>(ex:C755281)</small></label>
                        <div class="ui fluid search ac-Cust">
                            <div class="ui input">
                                <asp:TextBox ID="tb_ParCust" runat="server" CssClass="prompt" placeholder="輸入客戶代號或名稱關鍵字"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <div class="eight wide field">
                        <label>&nbsp;</label>
                        <asp:Button ID="btn_Insert" runat="server" Text="新增" CssClass="ui green small button" OnClick="btn_Insert_Click" />
                    </div>
                </div>
            </div>
        </div>
        <!-- Search End -->

        <!-- List Content Start -->
        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div class="ui green attached segment">
                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand">
                    <LayoutTemplate>
                        <table class="ui celled selectable compact table">
                            <thead>
                                <tr>
                                    <th class="center aligned">ID</th>
                                    <th>子公司</th>
                                    <th>母公司</th>
                                    <th></th>
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
                                <%#Eval("SeqNo") %>
                            </td>
                            <td class="blue-text text-darken-2">
                                <strong><%#Eval("CustName") %> (<%#Eval("CustID") %>)</strong>
                            </td>
                            <td class="green-text text-darken-2">
                                <strong><%#Eval("ParentCustName") %> (<%#Eval("ParentCustID") %>)</strong>
                            </td>
                            <td class="collapsing">
                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>

                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("SeqNo") %>' />
                            </td>
                        </tr>
                    </ItemTemplate>
                    <EmptyDataTemplate>
                        <div class="ui placeholder segment">
                            <div class="ui icon header">
                                <i class="search icon"></i>
                                尚未新增名單
                            </div>
                        </div>
                    </EmptyDataTemplate>
                </asp:ListView>
            </div>
        </asp:PlaceHolder>
        <!-- List Content End -->
    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <%-- Search UI Start --%>
    <script>
        /* 客戶 (一般查詢) */
        $('.ac-Cust').search({
            minCharacters: 1,
            fields: {
                results: 'results',
                title: 'ID',
                description: 'Label'
            },
            searchFields: [
                'title',
                'description'
            ]
            , apiSettings: {
                url: '<%=fn_Param.WebUrl%>Ajax_Data/GetData_Customer.ashx?q={query}'
            }

        });
    </script>
    <%-- Search UI End --%>

</asp:Content>


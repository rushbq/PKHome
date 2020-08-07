<%@ Page Title="收件人維護" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="InformConfig.aspx.cs" Inherits="myPostal_InformConfig" %>

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
                    <div class="section">快遞貨運登記</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">收件人維護
                    </h5>
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
                        <label>公司</label>
                        <asp:TextBox ID="tb_ToComp" runat="server" MaxLength="50" placeholder="公司"></asp:TextBox>
                    </div>
                    <div class="four wide field">
                        <label>收件者</label>
                        <asp:TextBox ID="tb_ToWho" runat="server" MaxLength="40" placeholder="收件者"></asp:TextBox>
                    </div>
                    <div class="four wide field">
                        <label>電話</label>
                        <asp:TextBox ID="tb_ToTel" runat="server" MaxLength="20" placeholder="電話"></asp:TextBox>
                    </div>
                    <div class="four wide field">
                    </div>
                </div>
                <div class="fields">
                    <div class="twelve wide field">
                        <label>收件地址</label>
                        <asp:TextBox ID="tb_ToAddr" runat="server" MaxLength="80" placeholder="收件地址"></asp:TextBox>
                    </div>
                    <div class="four wide field">
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
                        <table class="ui celled selectable compact small table">
                            <thead>
                                <tr>
                                    <th>公司</th>
                                    <th>收件人</th>
                                    <th>電話</th>
                                    <th>收件地址</th>
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
                            <td>
                                <strong><%#Eval("ToComp") %></strong>
                            </td>
                            <td>
                                <strong><%#Eval("ToWho") %></strong>
                            </td>
                            <td>
                                <%#Eval("ToTel") %>
                            </td>
                            <td>
                                <%#Eval("ToAddr") %>
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
                                尚未新增收件人名單
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
    <script>
        $(function () {


        });
    </script>

</asp:Content>


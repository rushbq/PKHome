<%@ Page Title="貨運公司維護" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="ShipComp.aspx.cs" Inherits="myShipping_ShipComp" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
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
                    <div class="active section red-text text-darken-2">
                        貨運公司維護 - 
                        <asp:Literal ID="lt_CorpName" runat="server"></asp:Literal>
                    </div>
                </div>
            </div>
            <div class="right menu">
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui attached segment grey-bg lighten-5">
            <!-- Data Start -->
            <div class="ui segments">
                <div class="ui teal segment">
                    <h5 class="ui header">貨運公司</h5>
                </div>
                <div class="ui small form segment">
                    <div class="ui internally celled grid">
                        <div class="row">
                            <div class="ten wide column">
                                <asp:ListView ID="lvDetailList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDetailList_ItemCommand">
                                    <LayoutTemplate>
                                        <table class="ui celled selectable compact table">
                                            <thead>
                                                <tr>
                                                    <th class="center aligned">編號</th>
                                                    <th>貨運名稱</th>
                                                    <th class="center aligned">顯示</th>
                                                    <th class="center aligned">排序</th>
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
                                            <td class="center aligned"><%#Eval("ID") %></td>
                                            <td><strong><%#Eval("Label") %></strong></td>
                                            <td class="center aligned"><%#Eval("Display") %></td>
                                            <td class="center aligned"><%#Eval("Sort") %></td>
                                            <td class="center aligned collapsing">
                                                <a class="ui small teal basic icon button" href="<%=fn_Param.WebUrl%>myShipping_CHN/ShipComp.aspx?dt=<%=Req_DataType %>&id=<%#Eval("ID") %>&back=<%=Server.UrlEncode(prevPage) %>">
                                                    <i class="pencil icon"></i>
                                                </a>
                                                <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                                <asp:HiddenField ID="hf_DataID" runat="server" Value='<%#Eval("ID") %>' />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <div class="ui placeholder segment">
                                            <div class="ui header">
                                                無資料
                                            </div>
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                            </div>
                            <div class="six wide column">
                                <div class="two fields">
                                    <div class="field">
                                        <label>系統編號</label>
                                        <div class="ui green basic large label">
                                            <asp:Literal ID="lt_DataID" runat="server">自動產生</asp:Literal>
                                        </div>
                                    </div>
                                    <div class="field">
                                        <label>顯示排序<small>&nbsp;(數字大的在後面)</small></label>
                                        <asp:TextBox ID="tb_Sort" runat="server" MaxLength="3" placeholder="排序" autocomplete="off" type="number" min="10" max="900">900</asp:TextBox>
                                    </div>
                                </div>

                                <div class="required field">
                                    <label>貨運名稱</label>
                                    <asp:TextBox ID="tb_DisplayName" runat="server" MaxLength="40" placeholder="輸入貨運名稱" autocomplete="off"></asp:TextBox>
                                </div>
                                <div class="field">
                                    <div class="ui toggle checkbox">
                                        <asp:CheckBox ID="cb_Display" runat="server" Checked="true" />
                                        <label>是否顯示在下拉選單中</label>
                                    </div>
                                </div>
                                <div class="ui divider"></div>
                                <div class="ui two column grid">
                                    <div class="column">
                                        <a href="<%=prevPage %>" class="ui small button"><i class="undo icon"></i>返回發貨維護</a>
                                    </div>
                                    <div class="column right aligned">
                                        <asp:Button ID="btn_Save" runat="server" Text="新增資料" CssClass="ui green small button" OnClick="btn_Save_Click" />
                                        <asp:HiddenField ID="hf_DataID" runat="server" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <!-- Data End -->
        </div>
    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //init dropdown list
            $('select').dropdown();

            //[觸發][SAVE鈕]
            $("#doSave").click(function () {
                $("#MainContent_btn_Save").trigger("click");
            });
        });
    </script>
</asp:Content>


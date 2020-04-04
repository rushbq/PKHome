<%@ Page Title="批量回覆" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="BatchReply.aspx.cs" Inherits="myCustComplaint_BatchReply" %>

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
                    <div class="section"><%:resPublic.nav_3000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="section"><%:resPublic.fun_客訴管理 %></div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">
                        <asp:Literal ID="lt_TypeName" runat="server" Text="功能名稱"></asp:Literal>
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <%--<a class="item" id="tips">
                    <i class="question circle icon"></i>
                </a>
                <a class="anchor" id="top"></a>--%>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui grid">
            <div class="row">
                <!-- Left Body Content Start -->
                <div id="myStickyBody" class="thirteen wide column">
                    <div class="ui attached segment grey-bg lighten-5">
                        <!-- Section-基本資料 Start -->
                        <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                            <div class="ui negative message">
                                <div class="header">
                                    <%:resPublic.error_oops %>
                                </div>
                                <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                            </div>
                        </asp:PlaceHolder>
                        <div class="ui segments">
                            <div class="ui green segment">
                                <h5 class="ui header"><a class="anchor" id="baseData"></a>回覆清單</h5>
                            </div>
                            <div class="ui small form segment">
                                <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemDataBound="lvDataList_ItemDataBound">
                                    <LayoutTemplate>
                                        <table class="ui celled selectable compact small table">
                                            <thead>
                                                <tr>
                                                    <th>客訴編號</th>
                                                    <th>品號</th>
                                                    <th>數量</th>
                                                    <th>客戶類別</th>
                                                    <th>客訴內容</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <asp:PlaceHolder ID="ph_Items" runat="server" />
                                            </tbody>
                                        </table>
                                    </LayoutTemplate>
                                    <ItemTemplate>
                                        <tr>
                                            <td class="collapsing">
                                                <h5 class="blue-text text-darken-2"><%#Eval("CC_UID") %></h5>
                                            </td>
                                            <td class="collapsing">
                                                <strong><%#Eval("ModelNo") %></strong>
                                            </td>
                                            <td class="center aligned">
                                                <strong><%#Eval("Qty") %></strong>
                                            </td>
                                            <td>
                                                <div><%#Eval("CustTypeName") %></div>
                                                <div class="grey-text text-darken-2"><%#Eval("RefCustName") %></div>
                                                <div class="grey-text text-darken-2"><%#Eval("RefMallName") %></div>
                                            </td>
                                            <td>
                                                <asp:Literal ID="lt_Content" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:ListView>
                            </div>
                            <div class="ui bottom attached warning message">
                                <ul>
                                    <li>此清單記錄於Cookie, 請勿將瀏覽器的Cookie功能關閉.</li>
                                    <li>清單中的項目若未處理, 於「隔日」會自動消失.</li>
                                </ul>
                            </div>
                        </div>
                        <!-- Section-基本資料 End -->

                        <!-- Section-流程資料 Start -->
                        <div class="ui segments">
                            <!-- Flow401_業務確認 Start -->
                            <div class="ui red segment">
                                <h5 class="ui header">
                                    <a class="anchor" id="flow401"></a>業務確認&nbsp;
                                    <small class="grey-text">(F401)</small>
                                </h5>
                            </div>
                            <div class="ui small form segment">
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理方式</label>
                                                <asp:DropDownList ID="ddl_Flow401_Type" runat="server">
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>維修費用</label>
                                                <asp:TextBox ID="tb_FixTotalPrice" runat="server" type="number" min="0" step="any">0</asp:TextBox>
                                            </div>
                                            <div class="field">
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>客訴銷單號</label>
                                                <asp:TextBox ID="tb_ERP_No1" runat="server" MaxLength="20"></asp:TextBox>
                                            </div>
                                            <div class="field">
                                                <label>借出單號</label>
                                                <asp:TextBox ID="tb_ERP_No2" runat="server" MaxLength="20"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>維修費訂單</label>
                                                <asp:TextBox ID="tb_ERP_No5" runat="server" MaxLength="20"></asp:TextBox>
                                            </div>
                                            <div class="field">
                                                <label>維修費銷貨單</label>
                                                <asp:TextBox ID="tb_ERP_No6" runat="server" MaxLength="20"></asp:TextBox>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理說明</label>
                                                <asp:TextBox ID="tb_Flow401_Desc" runat="server" MaxLength="2000" TextMode="MultiLine" Rows="5"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>歸還單號</label>
                                                <asp:TextBox ID="tb_ERP_No3" runat="server" MaxLength="20"></asp:TextBox>
                                            </div>
                                            <div class="field">
                                                <label>銷退單號</label>
                                                <asp:TextBox ID="tb_ERP_No4" runat="server" MaxLength="20"></asp:TextBox>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field grey-text text-darken-2">
                                                <label>填寫人員</label>
                                                <asp:Literal ID="lt_Flow401_Who" runat="server"></asp:Literal>
                                            </div>
                                            <div class="nine wide field grey-text text-darken-2">
                                                <label>填寫時間</label>
                                                <asp:Literal ID="lt_Flow401_Time" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- Flow401_業務確認 End -->
                        </div>
                        <!-- Section-流程資料 End -->
                    </div>

                </div>
                <!-- Left Body Content End -->

                <!-- Right Navi Menu Start -->
                <div class="three wide column">
                    <div class="ui sticky">
                        <div class="ui vertical text menu">

                            <div class="header item">功能按鈕</div>
                            <div class="item">
                                <a href="<%:Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
                            </div>
                            <div class="item">
                                <button id="doInvoke" type="button" class="ui orange small button"><i class="gavel icon"></i>派送下關</button>
                                <asp:Button ID="btn_doInvoke" runat="server" Text="Invoke" OnClick="btn_doInvoke_Click" Style="display: none;" />
                            </div>
                        </div>
                    </div>
                </div>
                <!-- Right Navi Menu End -->
            </div>
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

            //[觸發][Save按鈕]
            $("#doInvoke").click(function () {
                if (confirm("確定派送至下一關?")) {

                    $(this).addClass("loading disabled");

                    $("#MainContent_btn_doInvoke").trigger("click");
                }

            });

        });
    </script>

    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>javascript/sticky.js"></script>
</asp:Content>


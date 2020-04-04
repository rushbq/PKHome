<%@ Page Title="開案中客訴-查看" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="SettingView.aspx.cs" Inherits="myCustComplaint_SettingEdit" %>

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
                <a class="anchor" id="top"></a>
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
                                <h5 class="ui header"><a class="anchor" id="baseData"></a>基本資料</h5>
                            </div>
                            <div class="ui small form segment">
                                <div class="fields">
                                    <div class="four wide field">
                                        <label>追蹤碼</label>
                                        <div class="ui red basic large label">
                                            <asp:Literal ID="lt_TraceID" runat="server">資料建立中</asp:Literal>
                                        </div>
                                    </div>
                                    <div class="eight wide field">
                                        <label>目前狀態</label>
                                        <asp:PlaceHolder ID="ph_CS" runat="server">
                                            <div class="ui grey basic label">客服單位未填寫</div>
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder ID="ph_Freight" runat="server">
                                            <div class="ui grey basic label">收貨單位未填寫</div>
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder ID="ph_ProdCnt" runat="server">
                                            <div class="ui red basic label">商品資料未填寫</div>
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder ID="ph_Status" runat="server">
                                            <div class="ui orange basic label">待確認開案</div>
                                        </asp:PlaceHolder>
                                    </div>
                                    <div class="four wide field">
                                        <label>建立日期</label>
                                        <div class="ui basic large label">
                                            <asp:Literal ID="lt_CreateDate" runat="server">資料建立中</asp:Literal>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- Section-基本資料 End -->

                        <!-- Section-流程資料 Start -->
                        <div class="ui segments">
                            <!-- Flow_客服資料 Start -->
                            <div class="ui brown segment">
                                <h5 class="ui header"><a class="anchor" id="flow101"></a>客服資料&nbsp;
                                    <small class="grey-text">(F101)</small></h5>
                            </div>
                            <div id="form101" class="ui small form segment">
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>計劃處理方式</label>
                                                <asp:Label ID="lb_PlanType" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>客戶類別</label>
                                                <asp:Label ID="lb_CustType" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="ten wide field">
                                                <label>客戶(經銷商)</label>
                                                <asp:Label ID="lb_CustName" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="six wide field">
                                                <label>&nbsp;</label>
                                                <asp:Label ID="lb_ERP_ID" runat="server"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="ten wide field">
                                                <label>商城</label>
                                                <asp:Label ID="lb_Mall" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="six wide field">
                                                <label>&nbsp;</label>
                                                <asp:Label ID="lb_Platform_ID" runat="server"></asp:Label>
                                            </div>
                                        </div>

                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>備註</label>
                                                <asp:Label ID="lb_CustInput" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>是否有發票退回</label>
                                                <asp:Label ID="lb_InvoiceIsBack" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field">
                                                <label>聯絡人</label>
                                                <asp:Label ID="lb_BuyerName" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="nine wide field">
                                                <label>聯絡電話</label>
                                                <asp:Label ID="lb_BuyerPhone" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>聯絡地址</label>
                                                <asp:Label ID="lb_BuyerAddr" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>

                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field grey-text text-darken-2">
                                                <label>填寫人員</label>
                                                <asp:Literal ID="lt_CS_Who" runat="server"></asp:Literal>
                                            </div>
                                            <div class="nine wide field grey-text text-darken-2">
                                                <label>填寫時間</label>
                                                <asp:Literal ID="lt_CS_Time" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- Flow_客服資料 End -->
                            <!-- Flow_收貨資料 Start -->
                            <div class="ui brown segment">
                                <h5 class="ui header"><a class="anchor" id="flow102"></a>收貨資料&nbsp;
                                    <small class="grey-text">(F102)</small></h5>
                            </div>
                            <div id="form102" class="ui small form segment">
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>選擇收貨方式</label>
                                                <asp:Label ID="lb_FreightType" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="eight wide field">
                                                <label>若有發票,請填發票號碼</label>
                                                <asp:Label ID="lb_InvoiceNumber" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="eight wide field">
                                                <label>發票金額</label>
                                                <asp:Label ID="lb_InvoicePrice" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>快遞單號</label>
                                                <asp:Label ID="lb_FreightInput" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="seven wide field">
                                                <label>收貨時間</label>
                                                <asp:Label ID="lb_FreightGetDate" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="nine wide field">
                                                <label>快遞公司</label>
                                                <asp:Label ID="lb_ShipComp" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field">
                                                <label>客戶姓名</label>
                                                <asp:Label ID="lb_ShipWho" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="nine wide field">
                                                <label>客戶電話</label>
                                                <asp:Label ID="lb_ShipTel" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>客戶地址</label>
                                                <asp:Label ID="lb_ShipAddr" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field grey-text text-darken-2">
                                                <label>填寫人員</label>
                                                <asp:Literal ID="lt_Freight_Who" runat="server"></asp:Literal>
                                            </div>
                                            <div class="nine wide field grey-text text-darken-2">
                                                <label>填寫時間</label>
                                                <asp:Literal ID="lt_Freight_Time" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>                                
                            </div>
                            <!-- Flow_收貨資料 End -->
                        </div>
                        <!-- Section-流程資料 End -->

                        <!-- Section-收貨圖片 Start -->
                        <div class="ui segments">
                            <div class="ui blue segment">
                                <h5 class="ui header"><a class="anchor" id="attfiles"></a>收貨圖片</h5>
                            </div>
                            <div class="ui segment">
                                <asp:ListView ID="lv_Attachment" runat="server" ItemPlaceholderID="ph_Items">
                                    <LayoutTemplate>
                                        <table class="ui celled striped small table">
                                            <thead>
                                                <tr>
                                                    <th>原始檔名</th>
                                                    <th>上傳者</th>
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
                                                <a href="<%#fn_Param.RefUrl %><%#UploadFolder %><%#Eval("FilePath") %>" target="_blank"><%#Eval("AttachFile_Org") %></a>
                                            </td>
                                            <td class="collapsing grey-text text-darken-2"><%#Eval("Create_Name") %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:ListView>
                            </div>
                        </div>
                        <!-- Section-收貨圖片 End -->

                        <!-- Section-商品資料 Start -->
                        <div class="ui segments">
                            <div class="ui blue segment">
                                <h5 class="ui header"><a class="anchor" id="prodData"></a>商品資料</h5>
                            </div>
                            <div class="ui segment">
                                <asp:ListView ID="lv_Detail" runat="server" ItemPlaceholderID="ph_Items">
                                    <LayoutTemplate>
                                        <table class="ui celled selectable compact small table">
                                            <thead>
                                                <tr>
                                                    <th class="grey-bg lighten-3">品號</th>
                                                    <th class="grey-bg lighten-3 center aligned">數量</th>
                                                    <th class="grey-bg lighten-3 center aligned">是否拆單</th>
                                                    <th class="grey-bg lighten-3 center aligned">保固內</th>
                                                    <th class="grey-bg lighten-3">客訴內容</th>
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
                                                <strong class="blue-text text-darken-3"><%#Eval("ModelNo") %></strong>
                                            </td>
                                            <td class="center aligned"><%#Eval("Qty") %></td>
                                            <td class="center aligned"><%#Eval("IsSplit") %></td>
                                            <td class="center aligned"><%#Eval("IsWarranty") %></td>
                                            <td><%#Eval("Remark") %></td>
                                        </tr>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <div class="ui placeholder segment">
                                            <div class="ui icon header">
                                                <i class="coffee icon"></i>
                                                資料尚未填寫...
                                            </div>
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                            </div>
                        </div>
                        <!-- Section-商品資料 End -->

                        <!-- Section-維護資訊 Start -->
                        <div class="ui segments">
                            <div class="ui grey segment">
                                <h5 class="ui header"><a class="anchor" id="infoData"></a>維護資訊</h5>
                            </div>
                            <div class="ui segment">
                                <table class="ui celled small four column table">
                                    <thead>
                                        <tr>
                                            <th colspan="2" class="center aligned">建立</th>
                                            <th colspan="2" class="center aligned">最後更新</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr class="center aligned">
                                            <td>
                                                <asp:Literal ID="info_Creater" runat="server">資料建立中...</asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="info_CreateTime" runat="server">資料建立中...</asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="info_Updater" runat="server"></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="info_UpdateTime" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                        <!-- Section-維護資訊 End -->
                    </div>

                </div>
                <!-- Left Body Content End -->

                <!-- Right Navi Menu Start -->
                <div class="three wide column">
                    <div class="ui sticky">
                        <div id="fastjump" class="ui secondary vertical pointing fluid text menu">
                            <div class="header item">快速跳轉<i class="dropdown icon"></i></div>
                            <a href="#baseData" class="item">基本資料</a>
                            <a href="#flow101" class="item">客服資料</a>
                            <a href="#flow102" class="item">收貨資料</a>
                            <a href="#attfiles" class="item">收貨圖片</a>
                            <a href="#prodData" class="item">商品資料</a>
                            <a href="#top" class="item"><i class="angle double up icon"></i>到頂端</a>
                        </div>

                        <div class="ui vertical text menu">
                            <div class="header item">功能按鈕</div>
                            <div class="item">
                                <a href="<%:Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i>返回列表</a>
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
          

        });
    </script>

    <%-- 快速選單 --%>
    <script src="<%=fn_Param.WebUrl %>javascript/sticky.js"></script>
</asp:Content>


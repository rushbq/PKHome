<%@ Page Title="客訴內容" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="View.aspx.cs" Inherits="myCustComplaint_Edit" %>

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
                                        <label>客訴編號</label>
                                        <div class="ui blue basic large label">
                                            <asp:Literal ID="lt_CCUID" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="three wide field">
                                        <label>流程狀態</label>
                                        <div class="ui green basic large label">
                                            <asp:Literal ID="lt_Status" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="six wide field">
                                        <label>客戶類別</label>
                                        <div class="ui orange basic large label">
                                            <asp:Literal ID="lt_CustTypeName" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="three wide field">
                                        <label>計畫處理方式</label>
                                        <div class="ui basic large label">
                                            <asp:Literal ID="lt_PlanTypeName" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                </div>
                                <div class="fields">
                                    <div class="four wide field">
                                        <label>品號</label>
                                        <div class="ui basic large label">
                                            <asp:Literal ID="lt_ModelNo" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="twelve wide field">
                                        <label>客訴內容</label>
                                        <div class="ui basic large fluid label">
                                            <asp:Literal ID="lt_Remark" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                </div>
                                <div class="fields">
                                    <div class="four wide field">
                                        <label>數量</label>
                                        <div class="ui basic large label">
                                            <asp:Literal ID="lt_Qty" runat="server"></asp:Literal>
                                        </div>
                                    </div>
                                    <div class="twelve wide field">
                                        <label>客訴判斷說明</label>
                                        <div class="ui basic large fluid label">
                                            <asp:Literal ID="lt_Remark_Check" runat="server"><small class="grey-text text-darken-1">(一線人員填寫正確的客訴內容)</small></asp:Literal>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <!-- Section-基本資料 End -->

                        <!-- Section-流程資料 Start -->
                        <div class="ui segments">
                            <!-- Flow201_一線判斷 Start -->
                            <div class="ui red segment">
                                <h5 class="ui header">
                                    <a class="anchor" id="flow201"></a>一線判斷&nbsp;
                                    <small class="grey-text">(F201)</small>&nbsp;                                  
                                </h5>
                            </div>
                            <div class="ui small form segment">
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理方式</label>
                                                <asp:DropDownList ID="ddl_Flow201_Type" runat="server" Enabled="false">
                                                </asp:DropDownList>
                                            </div>
                                        </div>

                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理說明</label>
                                                <asp:Label ID="lb_Flow201_Desc" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field grey-text text-darken-2">
                                                <label>填寫人員</label>
                                                <asp:Literal ID="lt_Flow201_Who" runat="server"></asp:Literal>
                                            </div>
                                            <div class="nine wide field grey-text text-darken-2">
                                                <label>填寫時間</label>
                                                <asp:Literal ID="lt_Flow201_Time" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- Flow201_一線判斷 End -->

                            <!-- Flow301_二線維修 Start -->
                            <div class="ui red segment">
                                <h5 class="ui header">
                                    <a class="anchor" id="flow301"></a>二線維修&nbsp;
                                    <small class="grey-text">(F301)</small>
                                </h5>
                            </div>
                            <div class="ui small form segment">
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理方式</label>
                                                <asp:DropDownList ID="ddl_Flow301_Type" runat="server" Enabled="false">
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="six wide field">
                                                <label>預計維修完成日</label>
                                                <asp:Label ID="lb_FixWishDate" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="six wide field">
                                                <label>維修完成日</label>
                                                <asp:Label ID="lb_FixOkDate" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="four wide field">
                                                <label>報價金額</label>
                                                <asp:Label ID="lb_FixPrice" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理說明</label>
                                                <asp:Label ID="lb_Flow301_Desc" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field grey-text text-darken-2">
                                                <label>填寫人員</label>
                                                <asp:Literal ID="lt_Flow301_Who" runat="server"></asp:Literal>
                                            </div>
                                            <div class="nine wide field grey-text text-darken-2">
                                                <label>填寫時間</label>
                                                <asp:Literal ID="lt_Flow301_Time" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- Flow301_二線維修 End -->

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
                                                <asp:DropDownList ID="ddl_Flow401_Type" runat="server" Enabled="false">
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>維修費用</label>
                                                <asp:Label ID="lb_FixTotalPrice" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="field">
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>客訴銷單號</label>
                                                <asp:Label ID="lb_ERP_No1" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="field">
                                                <label>借出單號</label>
                                                <asp:Label ID="lb_ERP_No2" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>維修費訂單</label>
                                                <asp:Label ID="lb_ERP_No5" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="field">
                                                <label>維修費銷貨單</label>
                                                <asp:Label ID="lb_ERP_No6" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理說明</label>
                                                <asp:Label ID="lb_Flow401_Desc" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>歸還單號</label>
                                                <asp:Label ID="lb_ERP_No3" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="field">
                                                <label>銷退單號</label>
                                                <asp:Label ID="lb_ERP_No4" runat="server" CssClass="ui fluid basic label"></asp:Label>
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

                            <!-- Flow501_資材寄貨 Start -->
                            <div class="ui red segment">
                                <h5 class="ui header">
                                    <a class="anchor" id="flow501"></a>資材寄貨&nbsp;
                                    <small class="grey-text">(F501)</small>
                                </h5>
                            </div>
                            <div class="ui small form segment">
                                <div class="fields">
                                    <!-- Left Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理方式</label>
                                                <asp:DropDownList ID="ddl_Flow501_Type" runat="server" Enabled="false">
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>物流快遞公司</label>
                                                <asp:Label ID="lb_ShipCorp" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="two fields">
                                            <div class="field">
                                                <label>物流快遞單號</label>
                                                <asp:Label ID="lb_ShipNo" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                            <div class="field">
                                                <label>寄(取)貨日</label>
                                                <asp:Label ID="lb_ShipDate" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                    </div>
                                    <!-- Right Block -->
                                    <div class="eight wide field">
                                        <div class="fields">
                                            <div class="sixteen wide field">
                                                <label>處理說明</label>
                                                <asp:Label ID="lb_Flow501_Desc" runat="server" CssClass="ui fluid basic label"></asp:Label>
                                            </div>
                                        </div>
                                        <div class="fields">
                                            <div class="seven wide field grey-text text-darken-2">
                                                <label>填寫人員</label>
                                                <asp:Literal ID="lt_Flow501_Who" runat="server"></asp:Literal>
                                            </div>
                                            <div class="nine wide field grey-text text-darken-2">
                                                <label>填寫時間</label>
                                                <asp:Literal ID="lt_Flow501_Time" runat="server"></asp:Literal>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- Flow501_資材寄貨 End -->
                        </div>
                        <!-- Section-流程資料 End -->

                        <!-- Section-檔案資訊 Start -->
                        <div class="ui segments">
                            <div class="ui blue segment">
                                <h5 class="ui header"><a class="anchor" id="attfiles"></a>檔案附件</h5>
                            </div>
                            <div class="ui segment">
                                <asp:ListView ID="lv_Attachment" runat="server" ItemPlaceholderID="ph_Items">
                                    <LayoutTemplate>
                                        <table class="ui celled striped small table">
                                            <thead>
                                                <tr>
                                                    <th>流程關卡</th>
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
                                            <td><%#Eval("FlowName") %> (<%#Eval("FlowID") %>)</td>
                                            <td>
                                                <a href="<%#fn_Param.RefUrl %><%#UploadFolder %><%=Req_DataID %>/<%#Eval("AttachFile") %>" target="_blank"><%#Eval("AttachFile_Org") %></a>
                                            </td>
                                            <td class="collapsing grey-text text-darken-2"><%#Eval("Create_Name") %></td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:ListView>
                            </div>
                        </div>
                        <!-- Section-檔案資訊 End -->

                        <!-- Section-客服&收貨 Start -->
                        <div class="ui segments">
                            <!-- Flow_客服資料 Start -->
                            <div class="ui yellow segment">
                                <h5 class="ui header"><a class="anchor" id="flow101"></a>客服資料</h5>
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
                            <div class="ui yellow segment">
                                <h5 class="ui header"><a class="anchor" id="flow102"></a>收貨資料</h5>
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
                                                <label>備註</label>
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
                                <asp:ListView ID="lv_Photos" runat="server" ItemPlaceholderID="ph_Items" GroupItemCount="2" GroupPlaceholderID="ph_Group">
                                    <LayoutTemplate>
                                        <table class="ui celled striped small table">
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
                                            <a href="<%#fn_Param.RefUrl %><%#UploadFolder %><%#Eval("FilePath") %>" target="_blank"><%#Eval("AttachFile_Org") %></a>
                                        </td>
                                    </ItemTemplate>
                                    <EmptyItemTemplate>
                                        <td>&nbsp;</td>
                                    </EmptyItemTemplate>
                                </asp:ListView>
                            </div>
                            <!-- Flow_收貨資料 End -->
                        </div>
                        <!-- Section-客服&收貨 End -->

                        <!-- Section-流程紀錄 Start -->
                        <div class="ui segments">
                            <div class="ui orange segment">
                                <h5 class="ui header"><a class="anchor" id="timeline"></a>流程紀錄</h5>
                            </div>
                            <div class="ui segment">
                                <asp:ListView ID="lv_Timeline" runat="server" ItemPlaceholderID="ph_Items">
                                    <LayoutTemplate>
                                        <div class="ui small feed timeline">
                                            <asp:PlaceHolder ID="ph_Items" runat="server" />
                                        </div>
                                    </LayoutTemplate>
                                    <ItemTemplate>
                                        <div class="event">
                                            <div class="label">
                                                <img src="<%:fn_Param.CDNUrl %>images/common/img-user.jpg" alt="user" />
                                            </div>
                                            <div class="content">
                                                <div class="summary grey-text text-darken-2">
                                                    <span class="orange-text text-darken-4"><%#Eval("LogSubject")%></span>&nbsp;<%#Eval("Create_Name")%>
                                                    <div class="date">
                                                        <strong><%#Eval("Create_Time")%></strong>
                                                    </div>
                                                </div>
                                                <div class="extra text">
                                                    <%#Eval("LogDesc")%>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:ListView>
                            </div>
                        </div>
                        <!-- Section-流程紀錄 End -->

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
                            <a href="#flow201" class="item red-text text-darken-3">一線判斷</a>
                            <a href="#flow301" class="item red-text text-darken-3">二線維修</a>
                            <a href="#flow401" class="item red-text text-darken-3">業務確認</a>
                            <a href="#flow501" class="item red-text text-darken-3">資材寄貨</a>
                            <a href="#attfiles" class="item">檔案附件</a>
                            <a href="#flow101" class="item">客服資料</a>
                            <a href="#flow102" class="item">收貨資料</a>
                            <a href="#timeline" class="item orange-text text-darken-2">流程紀錄</a>
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


<%@ Page Title="製物工單明細" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="View.aspx.cs" Inherits="myMarketingHelp_View" %>

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
                    <div class="section"><%:resPublic.nav_2000 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="section"><%:resPublic.fun_製物工單 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        <asp:Literal ID="lt_CorpName" runat="server" Text="公司別名稱"></asp:Literal>
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
        <div class="ui segment grey-bg lighten-5">
            <!-- 需求資料 Start -->
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
                    <h5 class="ui header"><%:GetLocalResourceObject("txt_需求資料") %>
                    </h5>
                </div>
                <div class="ui form attached segment">
                    <!-- 基本資料 Start -->
                    <div class="fields">
                        <!-- Left Block -->
                        <div class="eight wide field">
                            <div class="three fields">
                                <div class="field">
                                    <label><%:GetLocalResourceObject("txt_追蹤編號") %></label>
                                    <div class="ui red basic large label">
                                        <asp:Literal ID="lt_TraceID" runat="server" Text="系統自動產生"></asp:Literal>
                                    </div>
                                </div>
                                <div class="field">
                                    <label><%:GetLocalResourceObject("txt_處理狀態") %></label>
                                    <div class="ui blue basic large label">
                                        <asp:Literal ID="lt_ReqStatus" runat="server" Text="資料建立中"></asp:Literal>
                                    </div>
                                </div>
                                <div class="field">
                                    <label><%:GetLocalResourceObject("txt_登記日期") %></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_CreateDate" runat="server" Text="資料建立中"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="sixteen wide field" style="z-index: 1;">
                                    <div class="ui piled segment">
                                        <h4 class="ui header">
                                            <asp:Literal ID="lt_ReqSubject" runat="server"></asp:Literal></h4>
                                        <p class="grey-text text-darken-3" style="line-height: 1.6em;">
                                            <asp:Literal ID="lt_ReqContent" runat="server"></asp:Literal>
                                        </p>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Right Block -->
                        <div class="eight wide field">
                            <div class="fields">
                                <div class="six wide field">
                                    <label><%:GetLocalResourceObject("txt_緊急度") %></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_EmgStatus" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div class="six wide field">
                                    <label><%:GetLocalResourceObject("txt_希望完成日") %></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_WishDate" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div class="four wide field">
                                    <label><%:GetLocalResourceObject("txt_預計完成日") %>&nbsp;<span class="grey-text text-darken-1"><%:GetLocalResourceObject("tip1") %></span></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_EstDate" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="six wide field">
                                    <label><%:GetLocalResourceObject("txt_需求類別") %></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_ReqClass" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div class="six wide field">
                                    <label><%:GetLocalResourceObject("txt_需求資源") %></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_ReqRes" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div class="four wide field">
                                    <label><%:GetLocalResourceObject("txt_需求數量") %></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_ReqQty" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                            <div class="fields">
                                <div class="sixteen wide field">
                                    <label><%:GetLocalResourceObject("txt_需求者") %></label>
                                    <div class="ui basic label">
                                        <asp:Literal ID="lt_ReqWho" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- 基本資料 End -->
                    <!-- 附件&轉寄 Start -->
                    <div class="fields">
                        <div class="sixteen wide field">
                            <div class="ui secondary pointing menu">
                                <a class="item active" data-tab="tab-name1">
                                    <i class="paperclip icon"></i><%:GetLocalResourceObject("txt_附件清單") %>
                                </a>
                                <a class="item" data-tab="tab-name2">
                                    <i class="envelope outline icon"></i><%:GetLocalResourceObject("txt_轉寄通知") %>
                                </a>
                            </div>
                            <!-- 附件清單 tab Start -->
                            <div class="ui active tab" data-tab="tab-name1">
                                <asp:ListView ID="lv_Attachment" runat="server" ItemPlaceholderID="ph_Items" GroupPlaceholderID="ph_Group" GroupItemCount="2">
                                    <LayoutTemplate>
                                        <table class="ui celled table">
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
                                        <td style="width: 50%">
                                            <a href="<%#fn_Param.RefUrl %><%#UploadFolder %><%=Req_DataID %>/<%#Eval("AttachFile") %>" target="_blank"><%#Eval("AttachFile_Org") %></a>
                                        </td>
                                    </ItemTemplate>
                                    <EmptyItemTemplate>
                                        <td>&nbsp;</td>
                                    </EmptyItemTemplate>
                                    <EmptyDataTemplate>
                                        <div class="ui message">
                                            <div class="content">
                                                <div class="header">
                                                    <%:GetLocalResourceObject("txt_附件未上傳") %>
                                                </div>
                                            </div>
                                        </div>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                            </div>
                            <!-- 附件清單 tab End -->

                            <!-- 轉寄通知 tab Start -->
                            <div class="ui tab" data-tab="tab-name2">
                                <div class="ui grid">
                                    <div class="row">
                                        <div class="sixteen wide column">
                                            <asp:ListView ID="lv_Inform" runat="server" ItemPlaceholderID="ph_Items">
                                                <LayoutTemplate>
                                                    <asp:PlaceHolder ID="ph_Items" runat="server" />
                                                </LayoutTemplate>
                                                <ItemTemplate>
                                                    <div class="ui basic label">
                                                        <%#Eval("CC_Who") %>
                                                    </div>
                                                </ItemTemplate>
                                                <EmptyDataTemplate>
                                                    <p><%:GetLocalResourceObject("txt_無另外通知") %></p>
                                                </EmptyDataTemplate>
                                            </asp:ListView>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <!-- 轉寄通知 tab End -->
                        </div>
                    </div>
                    <!-- 附件&轉寄 End -->

                    <div class="ui hidden divider"></div>
                    <!-- 功能按鈕 Start -->
                    <div class="ui grid">
                        <div class="row">
                            <div class="eight wide column">
                                <a href="<%=Page_SearchUrl %>" class="ui small button"><i class="undo icon"></i><%:GetLocalResourceObject("txt_返回列表") %></a>
                            </div>
                            <div class="eight wide column right aligned">
                                <asp:PlaceHolder ID="ph_doJob" runat="server">
                                    <a href="<%=FuncPath() %>/Edit/<%=Req_DataID%>" class="ui small blue button"><i class="ambulance icon"></i><%:GetLocalResourceObject("txt_前往處理") %></a>
                                </asp:PlaceHolder>
                            </div>
                        </div>
                    </div>
                    <!-- 功能按鈕 End -->
                </div>
                <div class="ui bottom attached info small message">
                    <%=GetLocalResourceObject("tipList1") %>
                </div>
            </div>
            <!-- 需求資料 End -->

            <!-- 處理進度 Start -->
            <div id="replyComments" class="ui segments">
                <div class="ui blue segment">
                    <h5 class="ui header"><%:GetLocalResourceObject("txt_處理進度") %>
                    </h5>
                </div>
                <div class="ui form segment">
                    <div class="two fields">
                        <div class="field">
                            <label><%:GetLocalResourceObject("txt_工作時數") %></label>
                            <div class="ui basic orange large label">
                                <asp:Literal ID="lt_FinishHours" runat="server">案件處理中</asp:Literal>
                            </div>
                        </div>
                        <div class="field">
                            <label><%:GetLocalResourceObject("txt_結案日期") %></label>
                            <div class="ui basic green large label">
                                <asp:Literal ID="lt_FinishDate" runat="server">案件處理中</asp:Literal>
                            </div>
                        </div>
                    </div>
                    <div class="fields">
                        <div class="sixteen wide field">
                            <label><%:GetLocalResourceObject("txt_處理人員") %></label>
                            <asp:ListView ID="lv_Assigned" runat="server" ItemPlaceholderID="ph_Items">
                                <LayoutTemplate>
                                    <asp:PlaceHolder ID="ph_Items" runat="server" />
                                </LayoutTemplate>
                                <ItemTemplate>
                                    <div class="ui basic label">
                                        <%#Eval("Who") %>
                                    </div>
                                </ItemTemplate>
                                <EmptyDataTemplate>
                                    <div class="ui placeholder segment">
                                        <div class="ui icon header">
                                            <i class="coffee icon"></i>
                                            <%:GetLocalResourceObject("txt_未指派") %>
                                        </div>
                                    </div>
                                </EmptyDataTemplate>
                            </asp:ListView>
                        </div>
                    </div>
                    <div id="processList" class="field">
                        <label><%:GetLocalResourceObject("txt_進度說明") %></label>
                        <div class="ui segment">
                            <div class="ui minimal comments">
                                <asp:ListView ID="lv_Reply" runat="server" ItemPlaceholderID="ph_Items">
                                    <LayoutTemplate>
                                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                                    </LayoutTemplate>
                                    <ItemTemplate>
                                        <div class="comment">
                                            <div class="content">
                                                <a><%#Eval("Create_Name") %></a>
                                                <div class="metadata">
                                                    <span class="date"><%#Eval("Create_Time") %></span>
                                                </div>
                                                <div class="text">
                                                    <%#Eval("Reply_Content").ToString().Replace("\r","<br/>") %>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                    <EmptyDataTemplate>
                                        <div class="grey-text text-darken-3">案件正在處理中，若有最新進度將會此在說明。</div>
                                    </EmptyDataTemplate>
                                </asp:ListView>
                            </div>
                        </div>

                    </div>
                </div>
            </div>

            <!-- 處理進度 Start -->
        </div>

    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //tab menu
            $('.menu .item').tab();

        });
    </script>

</asp:Content>


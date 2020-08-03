<%@ Page Title="發送對帳單結束" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Step4.aspx.cs" Inherits="myARdata_ImportStep4" %>

<%@ Register Src="Ascx_StepMenu.ascx" TagName="Ascx_Menu" TagPrefix="ucMenu" %>
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
                    <div class="section">應收帳款對帳</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        對帳單發送完成
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="ui attached segment grey-bg lighten-5">
            <!-- Steps menu -->
            <ucMenu:Ascx_Menu ID="Ascx_Menu1" runat="server" nowIndex="4" />
            <!-- 基本資料 Start -->
            <div class="ui form attached green segment">
                <div class="three fields">
                    <div class="field">
                        <label>追蹤碼</label>
                        <asp:Label ID="lb_TraceID" runat="server" CssClass="ui red basic large label"></asp:Label>
                    </div>
                    <div class="field">
                        <label>客戶</label>
                        <asp:Label ID="lb_Cust" runat="server" CssClass="ui green basic large label"></asp:Label>
                    </div>
                    <div class="field">
                        <label>單據日區間</label>
                        <asp:Label ID="lb_sDate" runat="server" CssClass="ui basic large label"></asp:Label>
                        ~<asp:Label ID="lb_eDate" runat="server" CssClass="ui basic large label"></asp:Label>
                    </div>
                </div>
                <div class="field">
                    <div class="ui placeholder segment">
                        <div class="ui icon header">
                            <i class="info icon"></i>
                            發送完成，接下來你可以...
                        </div>
                        <div class="inline">
                            <a href="<%=FuncPath() %>/CustList/" class="ui orange button"><i class="list icon"></i>待收款客戶</a>

                            <a class="ui button" href="<%=FuncPath() %>">回列表頁</a>
                            <a class="ui blue button" href="<%=FuncPath() %>/Step1">新增下一筆</a>
                        </div>
                    </div>
                </div>
            </div>
            <!-- 基本資料 End -->
        </div>
    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
</asp:Content>


﻿<%@ Page Title="發貨-物流單轉入Step1" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="ImportStep1.aspx.cs" Inherits="myShipping_ImportStep1" %>

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
                    <div class="section">發貨/運費維護統計</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        發貨-物流單轉入 - 
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
            <!-- 基本資料 Start -->
            <div class="ui small form attached green segment">
                <div class="fields">
                    <div class="sixteen wide required field">
                        <label>
                            追蹤碼</label>
                        <asp:Label ID="lb_TraceID" runat="server" CssClass="ui red basic large label"></asp:Label>
                    </div>
                </div>
                <div class="fields">
                    <div class="sixteen wide required field">
                        <label>
                            上傳檔案</label>
                        <asp:FileUpload ID="fu_File" runat="server" AllowMultiple="false" accept=".xlsx" />
                    </div>
                </div>
                <div class="ui message">
                    <div class="header">注意事項</div>
                    <ul class="list">
                        <li class="red-text text-darken-1"><strong>副檔名限制：.xlsx</strong></li>
                        <li>請注意Excel格式及欄位順序，若任意更改欄位位置，系統會抓不到正確的資料。</li>
                        <li>Excel不要留空白列。</li>
                        <li>若要變更格式，請先與資訊部討論。</li>
                        <li><a href="<%=fn_Param.RefUrl %>PKHome/ShipImport/Sample.xlsx">範本下載</a></li>
                    </ul>
                </div>

                <div class="ui grid">
                    <div class="six wide column">
                        <a href="<%=Page_SearchUrl %>" class="ui button"><i class="undo icon"></i>返回列表</a>
                    </div>
                    <div class="ten wide column right aligned">
                        <asp:LinkButton ID="lbtn_Next" runat="server" CssClass="ui green button" OnClick="lbtn_Next_Click">下一步<i class="chevron right icon"></i></asp:LinkButton>
                    </div>

                    <asp:HiddenField ID="hf_TraceID" runat="server" />
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


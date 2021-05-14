<%@ Page Title="訂單變更單下載頁" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="OPCS_PdfDW.aspx.cs" Inherits="myOpcsRemark_OPCS_PdfDW" %>

<%@ Import Namespace="Resources" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="myContentBody">
        <div class="ui grid">
            <div class="column">
                <div class="ui attached segment grey-bg lighten-5">
                    <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
                        <div class="ui negative message">
                            <div class="header">
                                <%:resPublic.error_oops %>
                            </div>
                            <strong>參數傳遞錯誤，請確認來源路徑是否正確。</strong>
                        </div>
                    </asp:PlaceHolder>
                    <div class="ui placeholder segment">
                        <div class="ui icon header">
                            <i class="notched circle loading icon"></i>
                            PDF下載中，請稍候...
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
</asp:Content>


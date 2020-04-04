<%@ Page Title="發貨-物流單轉入Step3" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="ImportStep3.aspx.cs" Inherits="myShipping_ImportStep3" %>

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
                    <div class="section">發貨/運費維護統計</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        發貨-物流單轉入
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
            <ucMenu:Ascx_Menu ID="Ascx_Menu1" runat="server" nowIndex="3" />
            <!-- 基本資料 Start -->
            <div class="ui small form attached green segment">
                <div class="fields">
                    <div class="eight wide field">
                        <label>
                            追蹤碼</label>
                        <asp:Label ID="lb_TraceID" runat="server" CssClass="ui red basic large label"></asp:Label>
                    </div>
                    <div class="eight wide field">
                        <label>銷貨單-單據日期</label>
                        <asp:Label ID="lb_Dates" runat="server" CssClass="ui brown basic large label"></asp:Label>
                    </div>
                </div>
                <div class="field">
                    <div class="ui placeholder segment">
                        <div class="ui two column stackable center aligned grid">
                            <div class="ui vertical divider">Or</div>
                            <div class="middle aligned row">
                                <div class="column">
                                    <div class="ui icon header">
                                        <i class="pencil icon"></i>
                                        修改資料，請按「上一步」
                                    </div>
                                </div>
                                <div class="column">
                                    <div class="ui icon header">
                                        <i class="thumbs up icon"></i>
                                        <h2>即將完成，就差一步了!</h2>
                                        <div>確定轉入，請按「下一步」</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>

                <div class="ui grid">
                    <div class="six wide column">
                        <a href="<%=FuncPath() %>/Step2/<%=Req_DataID %>" class="ui button"><i class="undo icon"></i>上一步</a>
                    </div>
                    <div class="ten wide column right aligned">
                        <asp:LinkButton ID="lbtn_Next" runat="server" CssClass="ui green button" OnClick="lbtn_Next_Click">下一步<i class="chevron right icon"></i></asp:LinkButton>
                    </div>
                    <asp:HiddenField ID="hf_sDate" runat="server" />
                    <asp:HiddenField ID="hf_eDate" runat="server" />
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


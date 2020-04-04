<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="ErrorPage.aspx.cs" Inherits="myPage_ErrorPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="container">
        <div class="row">
            <div class="s12">
                <div class="card-panel red white-text">
                    <h5>Oops...</h5>
                    <p>
                        <%=Req_Msg %>
                    </p>
                    <p class="right-align">
                        <a href="<%=fn_Param.WebUrl %>" class="waves-effect waves-light btn"><i class="material-icons left">home</i>點我回首頁</a>
                    </p>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
</asp:Content>


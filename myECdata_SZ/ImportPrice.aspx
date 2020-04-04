<%@ Page Title="頁面價匯入" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="ImportPrice.aspx.cs" Inherits="myECdata_SZ_ImportPrice" %>

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
                    <div class="section"><%:resPublic.fun_深圳電商平台業績 %></div>
                    <i class="right angle icon divider"></i>
                    <div class="active section">
                        價格匯入
                    </div>
                </div>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->

    <!-- 內容 Start -->
    <div class="myContentBody">
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

            <!-- Section-基本資料 End -->

            <!-- Section-資料填寫 Start -->
            <div class="ui segments">
                <div class="ui green segment">
                    <h5 class="ui header">工具</h5>
                </div>
                <div class="ui small form segment">
                    <div class="fields">
                        <div class="six wide field">
                            <label>選擇平台</label>
                            <asp:DropDownList ID="ddl_Mall1" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                        <div class="ten wide field">
                            <label>選擇Excel檔&nbsp;<a href="<%=fn_Param.CDNUrl %>PKHome/eCommerceData/Import_Pricelist.xlsx" target="_blank">(範本下載)</a></label>
                            <div class="two fields">
                                <div class="field">
                                    <asp:FileUpload ID="fu_File1" runat="server" AllowMultiple="false" accept=".xlsx" />
                                </div>
                                <div class="field">
                                    <asp:Button ID="btn_Save1" runat="server" Text="開始上傳" OnClick="btn_Save1_Click" CssClass="ui green small button" />

                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="ui orange segment">
                    <h5 class="ui header">科學玩具</h5>
                </div>
                <div class="ui small form segment">
                    <div class="fields">
                        <div class="six wide field">
                            <label>選擇平台</label>
                            <asp:DropDownList ID="ddl_Mall2" runat="server" CssClass="fluid">
                            </asp:DropDownList>
                        </div>
                        <div class="ten wide field">
                            <label>選擇Excel檔&nbsp;<a href="<%=fn_Param.CDNUrl %>PKHome/eCommerceData/Import_Pricelist.xlsx" target="_blank">(範本下載)</a></label>
                            <div class="two fields">
                                <div class="field">
                                    <asp:FileUpload ID="fu_File2" runat="server" AllowMultiple="false" accept=".xlsx" />
                                </div>
                                <div class="field">
                                    <asp:Button ID="btn_Save2" runat="server" Text="開始上傳" OnClick="btn_Save2_Click" CssClass="ui green small button" />

                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <!-- Section-資料填寫 End -->

        </div>

    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //init
            $('select').dropdown();

            //[觸發][SAVE鈕]
            $("#doSave").click(function () {
                $("#MainContent_btn_Save").trigger("click");
            });

        });
    </script>

</asp:Content>


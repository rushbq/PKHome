<%@ Page Title="發貨-物流單轉入Step2" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="ImportStep2.aspx.cs" Inherits="myShipping_ImportStep2" %>

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
            <ucMenu:Ascx_Menu ID="Ascx_Menu1" runat="server" nowIndex="2" />
            <!-- 基本資料 Start -->
            <div class="ui small form attached green segment">
                <div class="fields">
                    <div class="sixteen wide field">
                        <label>
                            追蹤碼</label>
                        <asp:Label ID="lb_TraceID" runat="server" CssClass="ui red basic large label"></asp:Label>
                    </div>
                </div>
                <div class="fields">
                    <div class="six wide required field">
                        <label>銷貨單-單據日期</label>
                        <div class="two fields">
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_sDate" runat="server" placeholder="開始日" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                            <div class="field">
                                <div class="ui left icon input datepicker">
                                    <asp:TextBox ID="filter_eDate" runat="server" placeholder="結束日" autocomplete="off"></asp:TextBox>
                                    <i class="calendar alternate outline icon"></i>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="ten wide required field">
                        <label>選擇工作表</label>
                        <asp:DropDownList ID="ddl_Sheet" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddl_Sheet_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="fields">
                    <div class="sixteen wide field">
                        <label>資料預覽</label>
                        <div>
                            <table id="listTable" class="ui celled table" style="width: 100%">
                                <asp:Literal ID="lt_tbBody" runat="server"><thead><tr><th>工作表未選擇</th></tr></thead></asp:Literal>
                            </table>
                        </div>
                    </div>
                </div>

                <div class="ui grid">
                    <div class="six wide column">
                        <asp:LinkButton ID="lbtn_ReNew" runat="server" CssClass="ui button" OnClick="lbtn_ReNew_Click" OnClientClick="return confirm('是否確定?\n1.追蹤編號會重新產生\n2.目前匯入檔案會刪除')"><i class="undo icon"></i>重新上傳</asp:LinkButton>
                    </div>
                    <div class="ten wide column right aligned">
                        <asp:LinkButton ID="lbtn_Next" runat="server" CssClass="ui green button" OnClick="lbtn_Next_Click">下一步<i class="chevron right icon"></i></asp:LinkButton>
                    </div>

                    <asp:HiddenField ID="hf_FullFileName" runat="server" />
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
    <%-- DataTable Start --%>
    <link href="https://cdn.datatables.net/1.10.13/css/jquery.dataTables.min.css" rel="stylesheet" />
    <script src="https://cdn.datatables.net/1.10.11/js/jquery.dataTables.min.js"></script>
    <script>
        $(function () {
            //使用DataTable
            $('#listTable').DataTable({
                "searching": false,  //搜尋
                "ordering": false,   //排序
                "paging": true,     //分頁
                "info": true,      //頁數資訊
                "language": {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                "pageLength": 20,   //顯示筆數預設值     
                //捲軸設定
                "scrollY": '50vh',
                "scrollCollapse": true,
                "scrollX": true
            });


        });
    </script>

    <style>
        #listTable td {
            word-break: keep-all;
            word-wrap: break-word;
        }
    </style>
    <%-- DataTable End --%>
    <%-- 日期選擇器 Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.css" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/calendar.min.js"></script>
    <script src="<%=fn_Param.CDNUrl %>plugin/Semantic-UI-Calendar0.0.8/options.js"></script>
    <script>
        $(function () {
            //載入datepicker
            $('.datepicker').calendar(calendarOpts_Range);
        });
    </script>
    <%-- 日期選擇器 End --%>
</asp:Content>


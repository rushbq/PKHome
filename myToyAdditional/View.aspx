<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="View.aspx.cs" Inherits="myToyAdditional_View" %>

<asp:Content ID="myCss" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="myBody" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- Sub Header Start -->
    <div class="grey lighten-3">
        <div class="container">
            <div class="row">
                <div class="col s12 m12 l12">
                    <h5 class="breadcrumbs-title">編輯資料<asp:Literal ID="lt_topInfo" runat="server"></asp:Literal></h5>
                    <ol class="breadcrumb">
                        <li><a>科學玩具補件登記簿</a></li>
                        <li><a href="<%=Page_SearchUrl %>">資料列表</a></li>
                        <li class="active">檢視資料</li>
                    </ol>
                </div>
            </div>
        </div>
    </div>
    <!-- Sub Header End -->
    <!-- Body Content Start -->
    <div class="container">
        <asp:PlaceHolder ID="ph_ErrMessage" runat="server" Visible="false">
            <div class="card-panel red darken-1 white-text">
                <h4><i class="material-icons right">error_outline</i>糟糕了!!...發生了一點小問題</h4>
                <p>若持續看到此訊息, 請回報 <strong class="flow-text">詳細操作狀況</strong>。</p>
                <p>
                    <asp:Literal ID="lt_ShowMsg" runat="server"></asp:Literal>
                </p>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder ID="ph_Data" runat="server">
            <div class="row">
                <div class="col s12 m9 l10">
                    <!-- // 接洽窗口 // -->
                    <div id="CreateInfo" class="scrollspy">
                        <ul class="collection with-header">
                            <li class="collection-header grey">
                                <h5 class="white-text">接洽窗口填寫</h5>
                            </li>
                            <li class="collection-item">
                                <div class="section">
                                    <div class="row">
                                        <div class="col s6">
                                            <label>系統編號</label>
                                            <div class="red-text text-darken-2 center-align">
                                                <b>
                                                    <asp:Literal ID="lt_DataID" runat="server">資料建立中</asp:Literal></b>
                                            </div>
                                        </div>
                                        <div class="col s6">
                                            <label>來源</label>
                                            <p>
                                                <asp:Literal ID="lt_CustTypeName" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col s6">
                                            <label>聯繫人</label>
                                            <p>
                                                <asp:Literal ID="lt_CustName" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                        <div class="col s6">
                                            <label>聯繫電話</label>
                                            <p>
                                                <asp:Literal ID="lt_CustTel" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                    </div>

                                    <div class="row">
                                        <div class="col s12">
                                            <label>配送地址</label>
                                            <p>
                                                <asp:Literal ID="lt_CustAddr" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                    </div>
                                    <div class="row">
                                        <div class="col s9">
                                            <label>品號</label>
                                            <p>
                                                <asp:Literal ID="lt_Prod" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                        <div class="col s3">
                                            <label>數量</label>
                                            <p>
                                                <asp:Literal ID="lt_Qty" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                    </div>
                                    <div class="section row">
                                        <div class="col s12">
                                            <label>Step 1.客戶反映狀況描述</label>
                                            <p>
                                                <asp:Literal ID="lt_Remark1" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                    </div>
                                    <div class="section row">
                                        <div class="col s12">
                                            <label>Step 2.解決方式回覆</label>
                                            <p>
                                                <asp:Literal ID="lt_Remark2" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                    </div>

                                    <div class="section row">
                                        <div class="col s12">
                                            <label>Step 3.需補備品明細</label>
                                            <p>
                                                <asp:Literal ID="lt_Remark3" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </li>
                        </ul>
                    </div>


                    <!-- // 備貨單位 // -->
                    <div id="ShipInfo" class="scrollspy">
                        <ul class="collection with-header">
                            <li class="collection-header grey">
                                <h5 class="white-text">備貨單位填寫</h5>
                            </li>
                            <li class="collection-item">
                                <div class="section">
                                    <div class="row">
                                        <div class="col s3">
                                            <label>寄出日</label>
                                            <p>
                                                <asp:Literal ID="lt_ShipDate" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                        <div class="col s6">
                                            <label>貨運及單號</label>
                                            <p>
                                                <asp:Literal ID="lt_ShipNo" runat="server"></asp:Literal>
                                            </p>
                                        </div>

                                        <div class="col s3">
                                            <label>
                                                運費
                                            </label>
                                            <p>
                                                <asp:Literal ID="lt_Freight" runat="server"></asp:Literal>
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </li>
                        </ul>
                    </div>

                    <!-- // 維護資訊 // -->
                    <div>
                        <ul class="collection with-header">
                            <li class="collection-header grey">
                                <h5 class="white-text">維護資訊</h5>
                            </li>
                            <li class="collection-item">
                                <table class="bordered striped responsive-table">
                                    <tbody>
                                        <tr>
                                            <th>建立者
                                            </th>
                                            <td>
                                                <asp:Literal ID="lt_Creater" runat="server"></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="lt_CreateTime" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>最後更新
                                            </th>
                                            <td>
                                                <asp:Literal ID="lt_Updater" runat="server"></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="lt_UpdateTime" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                        <tr>
                                            <th>出貨者
                                            </th>
                                            <td>
                                                <asp:Literal ID="lt_Shipper" runat="server"></asp:Literal>
                                            </td>
                                            <td>
                                                <asp:Literal ID="lt_ShipTime" runat="server"></asp:Literal>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </li>
                        </ul>
                    </div>
                </div>
                <div class="col hide-on-small-only m3 l2">
                    <!-- // 快速導覽按鈕 // -->
                    <div class="table-Nav">
                        <ul class="table-of-contents">
                            <li><a href="#CreateInfo">接洽窗口填寫</a></li>
                            <li><a href="#ShipInfo">備貨單位填寫</a></li>
                            <li></li>
                            <li><a href="<%=Page_SearchUrl %>"><i class="material-icons left">list</i>回列表</a></li>
                        </ul>
                    </div>
                </div>
            </div>

        </asp:PlaceHolder>
    </div>
    <!-- Body Content End -->
</asp:Content>
<asp:Content ID="myBottom" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="myScript" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script src="<%=fn_Param.CDNUrl %>plugin/Materialize/v0.97.8/lib/pickadate/translation/zh_TW.js"></script>
    <script>
        (function ($) {
            $(function () {
                //scrollSpy
                $('.scrollspy').scrollSpy();

                //pushpin
                $('.table-Nav').pushpin({
                    top: 97
                });

            }); // end of document ready
        })(jQuery); // end of jQuery name space
    </script>

</asp:Content>


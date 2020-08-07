<%@ Page Title="快遞貨運登記" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="View.aspx.cs" Inherits="myDelivery_View" %>

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
                    <div class="section">快遞貨運登記</div>
                    <i class="right angle icon divider"></i>
                    <div class="active section red-text text-darken-2">
                        登記內容
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


            <div id="formData" class="ui small form green segment">
                <!-- 基本資料 Start -->
                <div class="fields">
                    <div class="five wide field">
                        <label>登記單號</label>
                        <asp:Label ID="lb_TraceID" runat="server" CssClass="ui red basic large label">系統自動產生</asp:Label>
                    </div>
                    <div class="eleven wide field">
                        <label>類別</label>
                        <div class="ui blue basic large label">
                            <asp:Literal ID="lt_ShipType" runat="server"></asp:Literal>
                        </div>
                    </div>
                </div>
                <div class="fields">
                    <div class="five wide required field">
                        <label>寄送方式</label>
                        <div class="ui basic large label">
                            <asp:Literal ID="lt_ShipWay" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div class="five wide required field">
                        <label>運費付款方式</label>
                        <div class="ui basic large label">
                            <asp:Literal ID="lt_PayWay" runat="server"></asp:Literal>
                        </div>
                    </div>
                    <div class="six wide required field">
                        <label>登記人員</label>
                        <div class="ui basic large label">
                            <asp:Literal ID="lt_ShipWho" runat="server"></asp:Literal>
                        </div>


                    </div>
                </div>
                <!-- 基本資料 End -->
                <!-- 收件者資料 Start -->
                <h4 class="ui horizontal divider header pink-text text-darken-2">
                    <i class="shipping fast icon"></i>
                    收件者資料
                </h4>
                <div class="two fields">
                    <!-- left section -->
                    <div class="field">
                        <div class="fields">
                            <div class="eight wide field">
                                <label>指定寄件日</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_SendDate" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="eight wide field">
                                <label>公司名稱</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_SendComp" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="eight wide field">
                                <label>物流單號</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_ShipNo" runat="server"></asp:Literal>&nbsp;
                                </div>
                            </div>
                            <div class="eight wide field">
                                <label>運費</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_ShipPay" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- right section -->
                    <div class="field">
                        <div class="fields">
                            <div class="eight wide field">
                                <label>收件者</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_SendWho" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="eight wide field">
                                <label>收件電話</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_SendAddr" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="sixteen wide field">
                                <label>收件地址</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_SendTel" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- 收件者資料 End -->

                <!-- 內容物資料 Start -->
                <h4 class="ui horizontal divider header green-text text-darken-3">
                    <i class="archive icon"></i>
                    內容物資料
                </h4>
                <div class="two fields">
                    <!-- left section -->
                    <div class="field">
                        <div class="fields">
                            <div class="eight wide field">
                                <label>總寄件箱數</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_Box" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="eight wide field">
                                <label>對象分類</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_TargetClass" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="eight wide field">
                                <label>內容分類1</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_BoxClass1" runat="server"></asp:Literal>
                                </div>
                            </div>
                            <div class="eight wide field">
                                <label>內容分類2</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_BoxClass2" runat="server"></asp:Literal>
                                </div>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="sixteen wide field">
                                <label>說明</label>
                                <div class="ui basic large fluid label">
                                    <asp:Literal ID="lt_Remark1" runat="server"></asp:Literal>&nbsp;
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- right section -->
                    <div class="field">
                        <div class="fields">
                            <div class="eight wide field">
                                <label>採購單號(單別-單號)</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_PurNo" runat="server"></asp:Literal>&nbsp;
                                </div>
                            </div>
                            <div class="eight wide field">
                                <label>銷貨單號(單別-單號)</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_SaleNo" runat="server"></asp:Literal>&nbsp;
                                </div>
                            </div>
                        </div>
                        <div class="fields">
                            <div class="eight wide field">
                                <label>INVOICE</label>
                                <div class="ui basic large label">
                                    <asp:Literal ID="lt_InvoiceNo" runat="server"></asp:Literal>&nbsp;
                                </div>
                            </div>
                            <div class="eight wide field">
                            </div>
                        </div>
                        <div class="fields">
                            <div class="sixteen wide field">
                                <label>備註</label>
                                <div class="ui basic large fluid label">
                                    <asp:Literal ID="lt_Remark2" runat="server"></asp:Literal>&nbsp;
                                </div>
                            </div>
                        </div>
                    </div>

                </div>
                <!-- 內容物資料 End -->
            </div>

            <!-- button -->
            <div class="ui grid">
                <div class="three wide column">
                    <a href="<%=Page_SearchUrl %>" class="ui button"><i class="undo icon"></i>返回列表</a>
                </div>
                <div class="four wide column">
                </div>
                <div class="nine wide column right aligned">
                </div>
            </div>

        </div>
    </div>
    <!-- 內容 End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
</asp:Content>


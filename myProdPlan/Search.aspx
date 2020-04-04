<%@ Page Title="產品類別+新品" Language="C#" MasterPageFile="~/Site_S_UI.master" AutoEventWireup="true" CodeFile="Search.aspx.cs" Inherits="myShipmentData_Search_TW" %>

<%@ Import Namespace="PKLib_Method.Methods" %>
<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <!-- 工具列 Start -->
    <div class="myContentHeader">
        <div class="ui small menu toolbar">
            <div class="item">
                <div class="ui small breadcrumb">
                    <div class="section">決策支援</div>
                    <i class="right angle icon divider"></i>
                    <h5 class="active section red-text text-darken-2">產品類別+新品
                    </h5>
                </div>
            </div>
            <div class="right menu">
                <a id="doExport" class="item"><i class="file excel icon"></i>匯出</a>
            </div>
        </div>
    </div>
    <!-- 工具列 End -->
    <!-- 內容 Start -->
    <div class="myContentBody">
        <div class="mySideForm">
            <%--
                注意此處的架構,sidebar, pusher有相依性,不可隨意變更
                https://semantic-ui.com/modules/sidebar.html#page-structure
            --%>
            <!-- 側邊表單 Start -->
            <div class="ui right very wide sidebar segment">
                <!-- 新品表單 -->
                <div id="showForm-Prod" class="ui segments">
                    <div class="ui green segment">
                        <h5 class="ui header">新品資料</h5>
                    </div>
                    <div id="formBase" class="ui small form segment">
                        <div class="inline fields">
                            <div class="three wide field">
                                <label>銷售類別</label>
                            </div>
                            <div class="thirteen wide field">
                                <span id="form_ClassName" class="ui basic label"></span>
                                <asp:HiddenField ID="hf_ClassID" runat="server" />
                            </div>
                        </div>
                        <div class="inline fields">
                            <div class="three wide field">
                                <label>一階類別</label>
                            </div>
                            <div class="five wide field">
                                <span id="form_MenuLv1Name" class="ui basic label"></span>
                                <asp:HiddenField ID="hf_MenuLv1ID" runat="server" />
                            </div>
                            <div class="three wide field">
                                <label>二階類別</label>
                            </div>
                            <div class="five wide field">
                                <span id="form_MenuLv2Name" class="ui basic label"></span>
                                <asp:HiddenField ID="hf_MenuLv2ID" runat="server" />
                            </div>
                        </div>
                        <div class="inline fields">
                            <div class="two wide field">
                                <label>品號</label>
                            </div>
                            <div class="fourteen wide field">
                                <asp:TextBox ID="form_ModelNo" runat="server" CssClass="fluid" MaxLength="40"></asp:TextBox>
                            </div>
                        </div>
                        <div class="inline fields">
                            <div class="two wide field">
                                <label>品名</label>
                            </div>
                            <div class="fourteen wide field">
                                <asp:TextBox ID="form_ModelName" runat="server" CssClass="fluid" MaxLength="150"></asp:TextBox>
                            </div>
                        </div>
                        <div class="field">
                            <label>
                                產品圖&nbsp;<span class="grey-text text-darken-1">(<%=FileExtLimit.Replace("|", ", ") %>)</span>&nbsp;
                                <a href="#viewpic" id="view_File" target="_blank" style="display: none;">(查看)</a>
                            </label>
                            <asp:FileUpload ID="fu_Attachment" runat="server" AllowMultiple="false" />
                            <asp:HiddenField ID="hf_OldFile" runat="server" />
                        </div>
                        <div class="inline fields">
                            <div class="two wide field">
                                <label>出貨地</label>
                            </div>
                            <div class="six wide field">
                                <asp:TextBox ID="form_ShipFrom" runat="server" MaxLength="4"></asp:TextBox>
                            </div>
                            <div class="two wide field">
                                <label>供應商</label>
                            </div>
                            <div class="six wide field">
                                <asp:TextBox ID="form_Supplier" runat="server" CssClass="fluid" MaxLength="50"></asp:TextBox>
                            </div>
                        </div>
                        <div class="inline fields">
                            <div class="three wide field">
                                <label>預估完成</label>
                            </div>
                            <div class="thirteen wide field">
                                <asp:TextBox ID="form_TargetMonth" runat="server" CssClass="fluid" MaxLength="10"></asp:TextBox>
                            </div>
                        </div>
                        <div class="inline fields">
                            <div class="two wide field">
                                <label>備註</label>
                            </div>
                            <div class="fourteen wide field">
                                <asp:TextBox ID="form_Remark" runat="server" CssClass="fluid" MaxLength="200" placeholder="最多200字"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <div class="ui segment">
                        <div class="ui two column grid">
                            <div class="column">
                                <a class="ui small cancel button">取消</a>
                            </div>
                            <div class="column right aligned">
                                <button id="doSaveProd" type="button" class="ui small positive right labeled icon button" onclick="return confirm('確認存檔?')">存檔<i class="checkmark icon"></i></button>
                                <asp:Button ID="btn_SaveProd" runat="server" Text="SaveProd" OnClick="btn_SaveProd_Click" Style="display: none;" />
                                <asp:HiddenField ID="hf_DataID" runat="server" />
                                <asp:HiddenField ID="hf_Type" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>

                <!-- 備註表單 -->
                <div id="showForm-Remark" class="ui segments">
                    <div class="ui green segment">
                        <h5 class="ui header"><span class="titleModel green-text text-darken-4"></span>&nbsp; 填寫備註</h5>
                    </div>
                    <div id="formRemark" class="ui small form segment">
                        <div class="field">
                            <asp:TextBox ID="formSysRemark" runat="server" CssClass="fluid" MaxLength="200" placeholder="最多200字" TextMode="MultiLine" Rows="5"></asp:TextBox>
                        </div>
                    </div>
                    <div class="ui segment">
                        <div class="ui two column grid">
                            <div class="column">
                                <a class="ui small cancel button">取消</a>
                            </div>
                            <div class="column right aligned">
                                <button id="doSaveRemark" type="button" class="ui small positive right labeled icon button" onclick="return confirm('確認存檔?')">存檔<i class="checkmark icon"></i></button>
                                <asp:Button ID="btn_SaveRemark" runat="server" Text="SaveProd" OnClick="btn_SaveRemark_Click" Style="display: none;" />
                                <asp:HiddenField ID="hf_Year" runat="server" />
                                <asp:HiddenField ID="hf_ModelNo" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <!-- 側邊表單 End -->

            <!-- 主要顯示內容 Start -->
            <div class="pusher">
                <!-- Search Start -->
                <div class="ui orange attached segment">
                    <div class="ui small form">
                        <div class="fields">
                            <div class="three wide field">
                                <label>年份</label>
                                <asp:DropDownList ID="ddl_Year" runat="server" CssClass="fluid" AutoPostBack="true" OnSelectedIndexChanged="ddl_Year_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div class="three wide field">
                                <label>銷售類別</label>
                                <asp:DropDownList ID="ddl_Class" runat="server" CssClass="fluid" AutoPostBack="true" OnSelectedIndexChanged="ddl_Class_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div class="three wide field">
                                <label>一階分類</label>
                                <asp:DropDownList ID="ddl_Lv1" runat="server" CssClass="fluid" AutoPostBack="true" OnSelectedIndexChanged="ddl_Lv1_SelectedIndexChanged">
                                </asp:DropDownList>
                            </div>
                            <div class="four wide field">
                                <label>二階分類</label>
                                <asp:DropDownList ID="ddl_Lv2" runat="server" CssClass="fluid">
                                    <asp:ListItem Value="">請先選擇一階分類</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="ui two column grid">
                        <div class="column">
                            <a href="<%=FuncPath() %>" class="ui small button"><i class="refresh icon"></i>重置條件</a>
                        </div>
                        <div class="column right aligned">
                            <button type="button" id="doSearch" class="ui blue small button"><i class="search icon"></i>查詢</button>
                            <asp:Button ID="btn_Search" runat="server" Text="Button" OnClick="btn_Search_Click" Style="display: none" />
                        </div>
                    </div>

                </div>
                <!-- Search End -->

                <asp:PlaceHolder ID="ph_Tip" runat="server">
                    <div class="ui placeholder segment">
                        <div class="ui icon header">
                            <i class="search icon"></i>
                            請選擇一項條件<small>(銷售類別 / 一階分類)</small>，開始查詢。
                        </div>
                    </div>
                </asp:PlaceHolder>

                <!-- Empty Content Start -->
                <asp:PlaceHolder ID="ph_EmptyData" runat="server" Visible="false">
                    <div class="ui placeholder segment">
                        <div class="ui icon header">
                            <i class="search icon"></i>
                            目前條件查無資料，請重新查詢。
                        </div>
                    </div>
                </asp:PlaceHolder>
                <!-- Empty Content End -->

                <!-- List Content Start -->
                <asp:PlaceHolder ID="ph_Data" runat="server">
                    <div id="formData" class="ui small form">
                        <asp:ListView ID="lvDataList" runat="server" ItemPlaceholderID="ph_Items" OnItemCommand="lvDataList_ItemCommand" OnItemDataBound="lvDataList_ItemDataBound">
                            <LayoutTemplate>
                                <table id="tableList" class="ui celled compact small table nowrap">
                                    <thead>
                                        <tr>
                                            <th class="grey-bg lighten-3 center aligned">銷售類別</th>
                                            <th class="grey-bg lighten-3 center aligned">一階類別</th>
                                            <th class="grey-bg lighten-3 center aligned">二階類別</th>
                                            <th class="grey-bg lighten-3 center aligned">三階類別</th>
                                            <th class="grey-bg lighten-3 center aligned">款式</th>
                                            <th class="grey-bg lighten-3 center aligned">品號</th>
                                            <th class="grey-bg lighten-3">品名</th>
                                            <th class="grey-bg lighten-3 right aligned">年銷售量</th>
                                            <th class="grey-bg lighten-3 right aligned">銷售金額</th>
                                            <th class="grey-bg lighten-3 right aligned">平均<br />
                                                銷售單價</th>
                                            <th class="grey-bg lighten-3 right aligned">平均<br />
                                                單位成本</th>
                                            <th class="grey-bg lighten-3 center aligned">產品圖</th>
                                            <th class="grey-bg lighten-3">產品特性<br />
                                                及說明</th>
                                            <th class="grey-bg lighten-3 center aligned">主要<br />
                                                出貨地</th>
                                            <th class="grey-bg lighten-3 center aligned">主要供應商</th>
                                            <th class="grey-bg lighten-3">預估完成</th>
                                            <th class="grey-bg lighten-3">貨號</th>
                                            <th class="grey-bg lighten-3">備註</th>
                                            <th class="grey-bg lighten-3">&nbsp;</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:PlaceHolder ID="ph_Items" runat="server" />
                                    </tbody>
                                </table>
                            </LayoutTemplate>
                            <ItemTemplate>
                                <tr class="<%#statusColor(Eval("FromData").ToString()) %>">
                                    <td class="center aligned">
                                        <strong><%#Eval("Class_Name") %></strong>
                                    </td>
                                    <td class="center aligned grey-text text-darken-4">
                                        <%#Eval("MenuNameLv1") %>
                                    </td>
                                    <td class="center aligned grey-text text-darken-3">
                                        <%#Eval("MenuNameLv2") %>
                                    </td>
                                    <td class="center aligned grey-text text-darken-2">
                                        <%#Eval("MenuNameLv3") %>
                                    </td>
                                    <td class="center aligned grey-text text-darken-2">
                                        <%#Eval("StyleName") %>
                                    </td>
                                    <td class="center aligned">
                                        <b class="green-text text-darken-2"><%#Eval("ModelNo") %></b>
                                    </td>
                                    <td>
                                        <%#Eval("Model_Name") %>
                                    </td>
                                    <td class="right aligned">
                                        <!--年銷售量-->
                                        <b>
                                            <%#Eval("SalesNum") %>
                                        </b>
                                    </td>
                                    <td class="right aligned">
                                        <!--銷售金額-->
                                        <b>
                                            <%#Eval("SalesAmount").ToString().ToMoneyString() %>
                                        </b>
                                    </td>
                                    <td class="right aligned">
                                        <!--平均銷售單價-->
                                        <b>
                                            <%#Eval("avgSalesAmount").ToString().ToMoneyString() %>
                                        </b>
                                    </td>
                                    <td class="right aligned">
                                        <!--平均單位成本-->
                                        <b>
                                            <%#Eval("avgPaperCost").ToString().ToMoneyString() %>
                                        </b>
                                    </td>
                                    <td class="center aligned">
                                        <!--產品圖-->
                                        <asp:PlaceHolder ID="ph_ProdImg" runat="server">
                                            <a class="ui mini basic icon button btn-OpenImg" data-id="<%#Eval("ListPic") %>" data-title="<%#Eval("ModelNo") %>" data-src="<%#Eval("FromData") %>" title="看圖片">
                                                <i class="image icon"></i>
                                            </a>
                                        </asp:PlaceHolder>
                                    </td>
                                    <td class="center aligned">
                                        <!--產品特性/說明-->
                                        <asp:PlaceHolder ID="ph_Desc" runat="server">
                                            <a class="ui mini basic icon button btn-OpenDesc" data-id="<%#Eval("ModelNo") %>" title="看資料">
                                                <i class="folder open icon"></i>
                                            </a>
                                        </asp:PlaceHolder>
                                    </td>
                                    <td class="center aligned">
                                        <!--主要出貨地-->
                                        <%#Eval("Ship_From") %>
                                    </td>
                                    <td class="center aligned">
                                        <!--主要供應商-->
                                        <%#Eval("SupName") %>
                                    </td>
                                    <td>
                                        <!--預估完成-->
                                        <%#Eval("TargetMonth") %>
                                    </td>
                                    <td>
                                        <!--貨號-->
                                        <strong class="blue-text text-darken-4"><%#Eval("ItemNo") %></strong>
                                    </td>
                                    <td>
                                        <%#Eval("Remark") %>                             
                                    </td>
                                    <td>
                                        <asp:PlaceHolder ID="ph_SysData" runat="server">
                                            <a class="ui small teal basic icon button openRemark" data-id="<%#Eval("ModelNo") %>" title="寫備註">
                                                <i class="sticky note icon"></i>
                                            </a>
                                            <a class="ui small purple basic icon button openNewProd" data-id="<%#Eval("ModelNo") %>" data-target="add" title="依此類加入新品">
                                                <i class="plus icon"></i>
                                            </a>
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder ID="ph_NewData" runat="server">
                                            <a class="ui small teal basic icon button openNewProd" data-id="<%#Eval("DataID") %>" data-target="edit" title="編輯新品">
                                                <i class="pencil icon"></i>
                                            </a>
                                            <asp:LinkButton ID="lbtn_Close" runat="server" CssClass="ui small orange basic icon button" ValidationGroup="List" CommandName="doClose" OnClientClick="return confirm('確定刪除?')"><i class="trash alternate icon"></i></asp:LinkButton>
                                        </asp:PlaceHolder>

                                        <asp:HiddenField ID="hf_ListDataID" runat="server" Value='<%#Eval("DataID") %>' />
                                        <asp:HiddenField ID="hf_ListOldFile" runat="server" Value='<%#Eval("ListPic") %>' />

                                    </td>
                                </tr>
                            </ItemTemplate>
                            <EmptyDataTemplate>
                                <div class="ui placeholder segment">
                                    <div class="ui icon header">
                                        <i class="search icon"></i>
                                        目前條件查無資料
                                    </div>
                                </div>
                            </EmptyDataTemplate>
                        </asp:ListView>
                    </div>
                </asp:PlaceHolder>
                <!-- List Content End -->
            </div>
            <!-- 主要顯示內容 End -->
        </div>
    </div>
    <!-- 內容 End -->

    <!-- Product Img Modal Start -->
    <div id="prodImg" class="ui small modal">
        <div class="header">
            <span id="prodImgTitle" class="green-text text-darken-4"></span>
            &nbsp;產品圖
        </div>
        <div class="image content">
        </div>
        <div class="actions">
            <div class="ui close button">
                Close
            </div>
        </div>
    </div>
    <!-- Product Img Modal End -->
    <!-- Product Desc Modal Start -->
    <div id="prodDesc" class="ui longer modal">
        <div class="header">
            <span id="prodDescTitle" class="green-text text-darken-4"></span>
            &nbsp;產品特性/說明
        </div>
        <div class="scrolling content">
        </div>
        <div class="actions">
            <div class="ui close button">
                Close
            </div>
        </div>
    </div>
    <!-- Product Desc Modal End -->
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <script>
        $(function () {
            //Click:Search
            $("#doSearch").click(function () {
                //觸發查詢按鈕
                $("#MainContent_btn_Search").trigger("click");
            });

            //init dropdown list
            $('select').dropdown();

        });
    </script>
    <%-- SideBar Form 備註處理 --%>
    <script>
        $(function () {
            //存檔按鈕-備註
            $("#doSaveRemark").click(function () {
                $("#formRemark").addClass("loading");
                $("#MainContent_btn_SaveRemark").trigger("click");
            });


            //SideBar指定區塊
            $(".openRemark").click(function () {
                $('.myContentBody .ui.sidebar')
               .sidebar({
                   context: $('.myContentBody .mySideForm')
               })
               .sidebar('attach events', '.myContentBody .cancel.button', 'hide')
               .sidebar('toggle');

                //表單顯示
                $("#showForm-Prod").hide();
                $("#showForm-Remark").show();

                //取得備註欄
                var _id = $(this).attr("data-id");
                $("#showForm-Remark .titleModel").text(_id);
                getSysData(_id);
            });


            //[Function] 取得指定品號資料
            function getSysData(_modelNo) {
                //----- Ajax 處理 Start -----

                //[Ajax return], 取得資料(年份, 品號, 新品ID)
                var _data = getProdData("<%=Req_Year%>", _modelNo, "");

                //[Ajax done]
                _data.done(function (callback) {
                    $.map(callback, function (item) {
                        //填寫表單
                        $("#MainContent_formSysRemark").val(item.Remark);
                        $("#MainContent_hf_Year").val("<%=Req_Year%>");
                        $("#MainContent_hf_ModelNo").val(item.ModelNo);

                    });
                });

                //[Ajax fail]
                _data.fail(function (jqXHR, textStatus) {
                    event.preventDefault();
                    alert('取資料時發生錯誤 (getSysData)');
                });

                //----- Ajax 處理 End -----
            }

            //[Function] Ajax 主體
            function getProdData(_year, _modelNo, _dataID) {
                var request = $.ajax({
                    url: '<%=fn_Param.WebUrl%>' + "myProdPlan/Ashx_GetData.ashx",
                    method: "POST",
                    data: {
                        ModelNo: _modelNo,
                        Year: _year,
                        DataID: _dataID
                    },
                    dataType: "json"
                });

                    return request;
                }

        });
    </script>

    <%-- SideBar Form 新品資料處理 --%>
    <script>
        $(function () {
            //存檔按鈕
            $("#doSaveProd").click(function () {
                $("#formBase").addClass("loading");
                $("#MainContent_btn_SaveProd").trigger("click");
            });


            //SideBar指定區塊
            $(".openNewProd").click(function () {
                $('.myContentBody .ui.sidebar')
               .sidebar({
                   context: $('.myContentBody .mySideForm')
               })
               .sidebar('attach events', '.myContentBody .cancel.button', 'hide')
               .sidebar('toggle');

                //表單顯示
                $("#showForm-Prod").show();
                $("#showForm-Remark").hide();

                //判斷新增或修改
                var _id = $(this).attr("data-id");
                var _target = $(this).attr("data-target");
                if (_target == "add") {
                    //Add form
                    getModelData(_id, "", _target);
                } else {
                    //Edit form
                    getModelData("", _id, _target);
                }
            });


            //[Function] 取得指定品號資料
            function getModelData(_modelNo, _dataID, _type) {
                //----- Ajax 處理 Start -----

                //[Ajax return], 取得資料(年份, 品號, 新品ID)
                var _data = getProdData("<%=Req_Year%>", _modelNo, _dataID);

                //[Ajax done]
                _data.done(function (callback) {
                    $.map(callback, function (item) {
                        //填寫表單
                        $("#form_ClassName").text(item.Class_Name);
                        $("#form_MenuLv1Name").text(item.MenuNameLv1);
                        $("#form_MenuLv2Name").text(item.MenuNameLv2);
                        $("#MainContent_hf_ClassID").val(item.Class_ID);
                        $("#MainContent_hf_MenuLv1ID").val(item.Menu_Lv1);
                        $("#MainContent_hf_MenuLv2ID").val(item.Menu_Lv2);
                        $("#MainContent_form_ShipFrom").val(item.Ship_From);
                        $("#MainContent_form_Supplier").val(item.SupName);

                        if (_type == "edit") {
                            $("#MainContent_form_ModelNo").val(item.ModelNo);
                            $("#MainContent_form_ModelName").val(item.Model_Name);
                            $("#MainContent_form_TargetMonth").val(item.TargetMonth);
                            $("#MainContent_form_Remark").val(item.Remark);
                            $("#MainContent_hf_DataID").val(item.DataID);

                            //圖片
                            var _pic = item.ListPic;
                            $("#MainContent_hf_OldFile").val(_pic);
                            if (_pic != "") {
                                var eleFile = $("#view_File");
                                eleFile.show();
                                eleFile.prop("href", "<%=fn_Param.RefUrl+ UploadFolder%>" + _pic);
                            }
                        }

                        $("#MainContent_hf_Type").val(_type);

                    });
                });

                //[Ajax fail]
                _data.fail(function (jqXHR, textStatus) {
                    event.preventDefault();
                    alert('取資料時發生錯誤 (getModelData)');
                });

                //----- Ajax 處理 End -----
            }

            //[Function] Ajax 主體
            function getProdData(_year, _modelNo, _dataID) {
                var request = $.ajax({
                    url: '<%=fn_Param.WebUrl%>' + "myProdPlan/Ashx_GetData.ashx",
                    method: "POST",
                    data: {
                        ModelNo: _modelNo,
                        Year: _year,
                        DataID: _dataID
                    },
                    dataType: "json"
                });

                    return request;
                }

        });
    </script>

    <%-- Modal-載入產品圖 --%>
    <script>
        $(function () {
            $(".btn-OpenImg").on("click", function () {
                var id = $(this).attr("data-id");   //檔名
                var title = $(this).attr("data-title"); //品號
                var src = $(this).attr("data-src"); //來源

                $("#prodImgTitle").text(title);

                //load html
                var url = '<%=fn_Param.WebUrl%>' + "myProdPlan/Ashx_GetProdImg.ashx?src=" + src + "&model=" + title + "&id=" + id;
                var datablock = $("#prodImg .content");
                datablock.empty();
                datablock.load(url);

                //show modal
                $('#prodImg')
                    .modal({
                        selector: {
                            close: '.close, .actions .button'
                        }
                    })
                    .modal('show');
            });
        });
    </script>

    <%-- Modal-載入產品說明 --%>
    <script>
        $(function () {
            $(".btn-OpenDesc").on("click", function () {
                var id = $(this).attr("data-id");   //品號

                $("#prodDescTitle").text(id);

                //load html
                var url = '<%=fn_Param.WebUrl%>' + "myProdPlan/Ashx_GetProdDesc.ashx?id=" + encodeURIComponent(id);
                var datablock = $("#prodDesc .content");
                datablock.empty();
                datablock.load(url);

                //show modal
                $('#prodDesc')
                    .modal({
                        selector: {
                            close: '.close, .actions .button'
                        }
                    })
                    .modal('show');
            });
        });
    </script>
    <%-- DataTables Start --%>
    <link href="<%=fn_Param.CDNUrl %>plugin/dataTables-1.10.20/datatables.min.css?v=1.1" rel="stylesheet" />
    <script src="<%=fn_Param.CDNUrl %>plugin/dataTables-1.10.20/datatables.min.js?v=1.1"></script>
    <script>
        $(function () {
            //使用DataTable
            var table = $('#tableList').DataTable({
                fixedHeader: true,
                searching: false,  //搜尋
                ordering: false,   //排序
                paging: false,     //分頁
                info: true,      //頁數資訊
                //pageLength: 20,   //顯示筆數預設值
                language: {
                    //自訂筆數顯示選單
                    "lengthMenu": ''
                },
                //捲軸設定
                "scrollY": '60vh',
                "scrollCollapse": true,
                "scrollX": true,
                //fixedColumns: {
                //    leftColumns: 6, /* 左方N欄凍結 */
                //    heightMatch: 'semiauto' /* [高度計算] semiauto:計算一次後暫存; auto:每次計算,較慢但準確度高; none:不計算 */
                //}
            });

            //點擊時變更背景色
            $('#tableList tbody').on('click', 'tr', function () {
                var bgcolor = 'orange-bg lighten-2';
                var targetBg = 'tr.orange-bg.lighten-2';

                table.$(targetBg).removeClass(bgcolor); //移除其他列背景
                $(this).addClass(bgcolor); //此列新增背景
            });
        });
    </script>
    <%-- DataTables End --%>

    <script>
        $(function () {
            //匯出Excel
            $("#doExport").on("click", function () {
                /* 取得資料 - 各欄位 */
                var _ClassID = $("#MainContent_ddl_Class").val();
                var _Lv1 = $("#MainContent_ddl_Lv1").val();
                var _Lv2 = $("#MainContent_ddl_Lv2").val();

                //導向下載頁,帶入參數
                var url = "<%=fn_Param.WebUrl%>myProdPlan/Ashx_OutputExcel.ashx?Year=<%=Req_Year%>&ClassID=" + _ClassID + "&MenuLv1=" + _Lv1 + "&MenuLv2=" + _Lv2;
                console.log(url)
                window.open(url);
            });
        });
    </script>
</asp:Content>


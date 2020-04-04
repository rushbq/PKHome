<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Tree.aspx.cs" Inherits="_Test_Tree" %>

<asp:Content ID="Content1" ContentPlaceHolderID="CssContent" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <h2>Tree</h2>
    <div id='jqxTree'>
    </div>
    <hr />
    <button type="button" id="getid" class="btn teal">GET ID</button>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="BottomContent" runat="Server">
</asp:Content>
<asp:Content ID="Content4" ContentPlaceHolderID="ScriptContent" runat="Server">
    <link rel="stylesheet" href="<%=fn_Param.WebUrl %>plugins/jqwidgets/styles/jqx.base.css" type="text/css" />
    <script type="text/javascript" src="<%=fn_Param.WebUrl %>plugins/jqwidgets/jqxcore.js"></script>
    <script type="text/javascript" src="<%=fn_Param.WebUrl %>plugins/jqwidgets/jqxdata.js"></script>
    <script type="text/javascript" src="<%=fn_Param.WebUrl %>plugins/jqwidgets/jqxbuttons.js"></script>
    <script type="text/javascript" src="<%=fn_Param.WebUrl %>plugins/jqwidgets/jqxscrollbar.js"></script>
    <script type="text/javascript" src="<%=fn_Param.WebUrl %>plugins/jqwidgets/jqxpanel.js"></script>
    <script type="text/javascript" src="<%=fn_Param.WebUrl %>plugins/jqwidgets/jqxtree.js"></script>
    <script type="text/javascript" src="<%=fn_Param.WebUrl %>plugins/jqwidgets/jqxcheckbox.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            var data = [
               {
                   "id": "2",
                   "parentid": "1",
                   "text": "Hot Chocolate",
                   "value": "$2.3"
               }, {
                   "id": "3",
                   "parentid": "1",
                   "text": "Peppermint Hot Chocolate",
                   "value": "$2.3"
               }, {
                   "id": "4",
                   "parentid": "1",
                   "text": "Salted Caramel Hot Chocolate",
                   "value": "$2.3"
               }, {
                   "id": "5",
                   "parentid": "1",
                   "text": "White Hot Chocolate",
                   "value": "$2.3"
               }, {
                   "id": "1",
                   "text": "Chocolate Beverage",
                   "parentid": "-1",
                   "value": "$2.3"
               }, {
                   "id": "6",
                   "text": "Espresso Beverage",
                   "parentid": "-1",
                   "value": "$2.3"
               }, {
                   "id": "7",
                   "parentid": "6",
                   "text": "Caffe Americano",
                   "value": "$2.3"
               }, {
                   "id": "8",
                   "text": "Caffe Latte",
                   "parentid": "6",
                   "value": "$2.3"
               }, {
                   "id": "9",
                   "text": "Caffe Mocha",
                   "parentid": "6",
                   "value": "$2.3"
               }, {
                   "id": "10",
                   "text": "Cappuccino",
                   "parentid": "6",
                   "value": "$2.3"
               }, {
                   "id": "11",
                   "text": "Pumpkin Spice Latte",
                   "parentid": "6",
                   "value": "$2.3"
               }, {
                   "id": "12",
                   "text": "Frappuccino",
                   "parentid": "-1"
               }, {
                   "id": "13",
                   "text": "Caffe Vanilla Frappuccino",
                   "parentid": "12",
                   "value": "$2.3"
               }, {
                   "id": "15",
                   "text": "450 calories",
                   "parentid": "13",
                   "value": "$2.3"
               }, {
                   "id": "16",
                   "text": "16g fat",
                   "parentid": "13",
                   "value": "$2.3"
               }, {
                   "id": "17",
                   "text": "13g protein",
                   "parentid": "13",
                   "value": "$2.3"
               }, {
                   "id": "14",
                   "text": "Caffe Vanilla Frappuccino Light",
                   "parentid": "12",
                   "value": "$2.3"
               }]
            // prepare the data
            var source =
            {
                datatype: "json",
                datafields: [
                    { name: 'id' },
                    { name: 'parentid' },
                    { name: 'text' },
                    { name: 'value' }
                ],
                id: 'id',
                localdata: data
            };


            // create data adapter.
            var dataAdapter = new $.jqx.dataAdapter(source);
            // perform Data Binding.
            dataAdapter.dataBind();
            // get the tree items. The first parameter is the item's id. The second parameter is the parent item's id. The 'items' parameter represents 
            // the sub items collection name. Each jqxTree item has a 'label' property, but in the JSON data, we have a 'text' field. The last parameter 
            // specifies the mapping between the 'text' and 'label' fields.  
            var records = dataAdapter.getRecordsHierarchy('id', 'parentid', 'items', [{ name: 'text', map: 'label' }]);
            $('#jqxTree').jqxTree({ source: records, hasThreeStates: true, checkboxes: true, width: '300px' });


            //// create jqxTree
            //$('#jqxTree').jqxTree({ height: '400px', hasThreeStates: true, checkboxes: true, width: '330px' });
            //$('#jqxTree').css('visibility', 'visible');
            //$('#jqxCheckBox').jqxCheckBox({ width: '200px', height: '25px', checked: true });
            //$('#jqxCheckBox').on('change', function (event) {
            //    var checked = event.args.checked;
            //    $('#jqxTree').jqxTree({ hasThreeStates: checked });
            //});
            //$("#jqxTree").jqxTree('selectItem', $("#home")[0]);


            $("#getid").on("click", function () {
                var str = "";
                var items = $('#jqxTree').jqxTree('getCheckedItems');
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    str += item.id + ",";
                }

                console.log(str);
            });
        });
    </script>
</asp:Content>


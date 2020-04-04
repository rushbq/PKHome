[ref]
https://github.com/zTree/zTree_v3
作者網站已掛.只剩git


[json欄位格式]
{
 id,
 parentID,
 label,
 selected,
 open
}


說明:
 插件欄位已經過修改,




-----------------------------------------------------------
[使用範例]

  <link rel="stylesheet" href="<%=fn_Param.WebUrl %>plugins/zTree/css/zTreeStyle.css" />
        <script src="<%=fn_Param.WebUrl %>plugins/zTree/jquery.ztree.core-3.5.min.js"></script>
        <script src="<%=fn_Param.WebUrl %>plugins/zTree/jquery.ztree.excheck-3.5.min.js"></script>
        <script>
            //--- zTree 設定 Start ---
            var setting = {
                view: {
                    dblClickExpand: false
                },
                callback: {
                    onClick: MMonClick
                },
                check: {
                    enable: true
                },
                data: {
                    simpleData: {
                        enable: true
                    }
                }
            };

            //Event - onClick
            function MMonClick(e, treeId, treeNode) {
                var zTree = $.fn.zTree.getZTreeObj("authList");
                zTree.expandNode(treeNode);
            }
            //--- zTree 設定 End ---
        </script>
        <script>
            $(function () {
                /*
                    取得權限List
                    ref:http://api.jquery.com/jQuery.post/
                */
                var jqxhr = $.post("<%=fn_Param.WebUrl%>Ajax_Data/GetAuthList.ashx", {
                    id: '<%=Param_thisID %>',
                    db: '<%=Param_dbID %>'
                })
                  .done(function (data) {
                      //載入選單
                      $.fn.zTree.init($("#authList"), setting, data)
                  })
                  .fail(function () {
                      alert("權限選單載入失敗");
                  });


                /*
                    取得已勾選的項目ID
                */
                $("#setAuth").on("click", function () {
                    var myTreeName = "authList";
                    var valAry = [];

                    //宣告tree物件
                    var treeObj = $.fn.zTree.getZTreeObj(myTreeName);

                    //取得節點array
                    var nodes = treeObj.getCheckedNodes(true);

                    //將id丟入陣列
                    for (var row = 0; row < nodes.length; row++) {
                        valAry.push(nodes[row].id);
                    }

                    //將陣列組成以','分隔的字串，並填入欄位
                    $("#MainContent_tb_Values").val(valAry.join(","));

                    //觸發設定 click
                    $("#MainContent_btn_Setting").trigger("click");

                });
            });
        </script>
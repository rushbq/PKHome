<%@ Application Language="C#" %>
<%@ Import Namespace="System.Web.Routing" %>


<script RunAt="server">

    void Application_Start(object sender, EventArgs e)
    {
        // 在應用程式啟動時執行的程式碼
        // 載入Routing設定
        RegisterRoutes(RouteTable.Routes);

    }

    void Application_End(object sender, EventArgs e)
    {
        //  在應用程式關閉時執行的程式碼

    }

    void Application_Error(object sender, EventArgs e)
    {
        // 在發生未處理的錯誤時執行的程式碼

    }

    protected void Application_BeginRequest(Object sender, EventArgs e)
    {
        #region -- 語系判斷 --

        System.Globalization.CultureInfo currentInfo;

        //[判斷參數] - 判斷Cookie是否存在
        HttpCookie cLang = Request.Cookies["PKHome_Lang"];
        if ((cLang != null))
        {
            //依Cookie選擇，變換語言別
            switch (cLang.Value.ToString().ToUpper())
            {
                //case "EN-US":
                //    System.Globalization.CultureInfo currentInfo = new System.Globalization.CultureInfo("en-US");
                //    System.Threading.Thread.CurrentThread.CurrentCulture = currentInfo;
                //    System.Threading.Thread.CurrentThread.CurrentUICulture = currentInfo;
                //    break;

                case "ZH-CN":
                    currentInfo = new System.Globalization.CultureInfo("zh-CN");
                    System.Threading.Thread.CurrentThread.CurrentCulture = currentInfo;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = currentInfo;
                    break;

                default:
                    currentInfo = new System.Globalization.CultureInfo("zh-TW");
                    System.Threading.Thread.CurrentThread.CurrentCulture = currentInfo;
                    System.Threading.Thread.CurrentThread.CurrentUICulture = currentInfo;
                    break;
            }

        }
        else
        {
            //Cookie不存在, 新增預設語系(依瀏覽器預設)
            string defCName = System.Globalization.CultureInfo.CurrentCulture.Name;

            //判斷瀏覽器預設的語系, 除了繁中簡中，其他國家語系都帶英文
            switch (defCName.ToUpper())
            {
                case "ZH-TW":
                case "ZH-CN":
                    break;

                default:
                    defCName = "en-US";
                    break;
            }

            Response.Cookies.Add(new HttpCookie("PKHome_Lang", defCName));
            Response.Cookies["PKHome_Lang"].Expires = DateTime.Now.AddYears(1);
            currentInfo = new System.Globalization.CultureInfo(defCName);
            System.Threading.Thread.CurrentThread.CurrentCulture = currentInfo;
            System.Threading.Thread.CurrentThread.CurrentUICulture = currentInfo;

        }

        #endregion

    }

    /// <summary>
    /// Routing設定
    /// </summary>
    /// <param name="routes">URL路徑</param>
    public static void RegisterRoutes(RouteCollection routes)
    {
        #region -- 定義不處理UrlRouting的規則 --
        routes.Ignore("{*allaspx}", new { allaspx = @".*\.aspx(/.*)?" });
        routes.Ignore("{*allcss}", new { allcss = @".*\.css(/.*)?" });
        routes.Ignore("{*alljpg}", new { alljpg = @".*\.jpg(/.*)?" });
        routes.Ignore("{*alljs}", new { alljs = @".*\.js(/.*)?" });
        routes.Add(new Route("{resource}.css/{*pathInfo}", new StopRoutingHandler()));
        routes.Add(new Route("{resource}.js/{*pathInfo}", new StopRoutingHandler()));
        #endregion

        //[首頁]
        routes.MapPageRoute("HomeRoute", "{lang}", "~/default.aspx", false,
             new RouteValueDictionary {
                    { "lang", "auto" }});


        //--- 權限管理 --- 
        routes.MapPageRoute("AuthUser", "{lang}/AuthUser/{dbid}/{id}", "~/mySystem/AuthByUser.aspx", false,
          new RouteValueDictionary {
                { "lang", "auto" }
                , { "dbid", "new" }
                , { "id", "new" }});
        routes.MapPageRoute("AuthFunc", "{lang}/AuthFunc/{dbid}", "~/mySystem/AuthByFunc.aspx", false,
          new RouteValueDictionary {
                { "lang", "auto" }
                , { "dbid", "new" }});
        routes.MapPageRoute("AuthCopy", "{lang}/AuthCopy", "~/mySystem/AuthCopy.aspx", false,
          new RouteValueDictionary {
                { "lang", "auto" }});
        routes.MapPageRoute("AuthSearch", "{lang}/AuthSearch/{dbid}/{id}", "~/mySystem/AuthSearch.aspx", false,
          new RouteValueDictionary {
                { "lang", "auto" }
                , { "dbid", "new" }
                , { "id", "new" }});

        routes.MapPageRoute("DeptSearch", "{lang}/Depts", "~/myDepts/Search.aspx", false,
          new RouteValueDictionary {
                { "lang", "auto" }});
        routes.MapPageRoute("DeptEdit", "{lang}/Depts/Edit/{id}", "~/myDepts/Edit.aspx", false,
          new RouteValueDictionary {
                { "lang", "auto" }
                , { "id", "new" }});

        routes.MapPageRoute("UsersSearch", "{lang}/Users", "~/myUsers/Search.aspx", false,
          new RouteValueDictionary {
                { "lang", "auto" }});

        //--- 決策支援(1000) --- 
        //ProdPlan
        routes.MapPageRoute("ProdPlanSearch", "{lang}/{rootID}/ProductPlan", "~/myProdPlan/Search.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "1000" }});


        //--- 行政管理(2000) --- 
        //製物工單
        routes.MapPageRoute("MKHelpSearch", "{lang}/{rootID}/MarketingHelp/{CompID}", "~/myMarketingHelp/Search.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "2000" }
                , { "CompID", "TW" }});
        routes.MapPageRoute("MKHelpEdit", "{lang}/{rootID}/MarketingHelp/{CompID}/Edit/{id}", "~/myMarketingHelp/Edit.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "2000" }
                , { "CompID", "TW" }
                , { "id", "new" }});
        routes.MapPageRoute("MKHelpView", "{lang}/{rootID}/MarketingHelp/{CompID}/View/{id}", "~/myMarketingHelp/View.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "2000" }
                , { "CompID", "TW" }
                , { "id", "new" }});
        routes.MapPageRoute("MKHelpChart", "{lang}/{rootID}/MarketingHelp/{CompID}/Chart", "~/myMarketingHelp/Chart.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "2000" }
                , { "CompID", "TW" }});

        //郵件登記
        routes.MapPageRoute("PostalAdmin", "{lang}/{rootID}/PostalAdmin", "~/myPostal/Search.aspx", false,
        new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "2000" }});
        routes.MapPageRoute("PostalUser", "{lang}/{rootID}/Postal", "~/myPostal/SearchClient.aspx", false,
        new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "2000" }});
        routes.MapPageRoute("PostalAddr", "{lang}/{rootID}/Postal/Address", "~/myPostal/InformConfig.aspx", false,
        new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "2000" }});

        //--- 業務行銷(3000) --- 
        //科玩補件登記
        routes.MapPageRoute("TAsearch", "{lang}/{rootID}/ToyAdditional/{CompID}", "~/myToyAdditional/Search.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});
        routes.MapPageRoute("TAedit", "{lang}/{rootID}/ToyAdditional/{CompID}/Edit/{id}", "~/myToyAdditional/Edit.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }
                , { "id", "new" }});
        routes.MapPageRoute("TAview", "{lang}/{rootID}/ToyAdditional/{CompID}/View/{id}", "~/myToyAdditional/View.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }
                , { "id", "new" }});

        //發貨
        routes.MapPageRoute("SFsearch", "{lang}/{rootID}/ShipFreight/{CompID}", "~/myShipping/Search.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("SFedit", "{lang}/{rootID}/ShipFreight/{CompID}/Edit/{id}", "~/myShipping/Edit.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }
                , { "id", "new" }});
        routes.MapPageRoute("SFSendsearch", "{lang}/{rootID}/ShipFreightSend/{CompID}", "~/myShipping/SendList.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("SFStatY", "{lang}/{rootID}/ShipFreightStat_Y/{CompID}", "~/myShipping/StatFreight.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("SFStatW", "{lang}/{rootID}/ShipFreightStat_W/{CompID}", "~/myShipping/StatWeek.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("SFImpList", "{lang}/{rootID}/ShipImport/{CompID}", "~/myShipping/ImportList.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("SFImpS1", "{lang}/{rootID}/ShipImport/{CompID}/Step1", "~/myShipping/ImportStep1.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("SFImpS2", "{lang}/{rootID}/ShipImport/{CompID}/Step2/{id}", "~/myShipping/ImportStep2.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("SFImpS3", "{lang}/{rootID}/ShipImport/{CompID}/Step3/{id}", "~/myShipping/ImportStep3.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("SFImpS4", "{lang}/{rootID}/ShipImport/{CompID}/Step4/{id}", "~/myShipping/ImportStep4.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});


        //銷貨單庫存狀況
        routes.MapPageRoute("SalesOrderStockStat", "{lang}/{rootID}/SalesOrderStockStat/{CompID}", "~/mySalesOrderStock/Search.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});

        //訂單庫存狀況
        routes.MapPageRoute("OrderStockStat", "{lang}/{rootID}/OrderStockStat/{CompID}", "~/myOrderingStock/Search.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});
        routes.MapPageRoute("ProdStockStat", "{lang}/{rootID}/ProdStockStat", "~/myOrderingStock/SearchByProd.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});

        //客戶返利
        routes.MapPageRoute("RebateSearch", "{lang}/{rootID}/CustRebate/{CompID}", "~/myRebate/Search.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }});
        routes.MapPageRoute("RebateEdit", "{lang}/{rootID}/CustRebate/{CompID}/Edit/{id}", "~/myRebate/Edit.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "SZ" }
                , { "id", "new" }});
        routes.MapPageRoute("RebateCHNSearch", "{lang}/{rootID}/RebateChina", "~/myRebate_China/Search.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});
        routes.MapPageRoute("RebateCHNEdit", "{lang}/{rootID}/RebateChina/Edit/{id}", "~/myRebate_China/Edit.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "id", "new" }});
        routes.MapPageRoute("RebateCHNCust", "{lang}/{rootID}/RebateChina/CustRel", "~/myRebate_China/CustConfig.aspx", false,
        new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});


        //客訴管理
        routes.MapPageRoute("CCSearch", "{lang}/{rootID}/CustComplaint/{typeID}", "~/myCustComplaint/Search.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "10" }});
        routes.MapPageRoute("CCEdit", "{lang}/{rootID}/CustComplaint/{typeID}/Edit/{id}", "~/myCustComplaint/Edit.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "10" }
                , { "id", "new" }});
        routes.MapPageRoute("CCView", "{lang}/{rootID}/CustComplaint/{typeID}/View/{id}", "~/myCustComplaint/View.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "10" }
                , { "id", "0" }});
        routes.MapPageRoute("CCsetSearch", "{lang}/{rootID}/CustComplaint/{typeID}/Set", "~/myCustComplaint/SettingSearch.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "10" }});
        routes.MapPageRoute("CCsetEdit", "{lang}/{rootID}/CustComplaint/{typeID}/SetEdit/{id}", "~/myCustComplaint/SettingEdit.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "10" }
                , { "id", "new" }});
        routes.MapPageRoute("CCsetView", "{lang}/{rootID}/CustComplaint/{typeID}/SetView/{id}", "~/myCustComplaint/SettingView.aspx", false,
           new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "10" }
                , { "id", "0" }});
        routes.MapPageRoute("CCInformCfg", "{lang}/{rootID}/CustComplaint/{typeID}/Inform", "~/myCustComplaint/InformConfig.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "10" }});
        routes.MapPageRoute("CCReply", "{lang}/{rootID}/CustComplaint/{typeID}/BatchReply", "~/myCustComplaint/BatchReply.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "10" }});

        //電商平台業績-深圳
        routes.MapPageRoute("SZ_ECDsearchY", "{lang}/{rootID}/eCommerceData/{typeID}/Year", "~/myECdata_SZ/SearchByYear.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }});
        routes.MapPageRoute("SZ_ECDsearchM", "{lang}/{rootID}/eCommerceData/{typeID}/Month", "~/myECdata_SZ/SearchByMonth.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }});
        routes.MapPageRoute("SZ_ECDsearchD", "{lang}/{rootID}/eCommerceData/{typeID}/Date", "~/myECdata_SZ/SearchByDate.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }});

        routes.MapPageRoute("SZ_ECDeditY", "{lang}/{rootID}/eCommerceData/{typeID}/Year/Edit/{id}", "~/myECdata_SZ/EditByYear.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }
                , { "id", "new" }});
        routes.MapPageRoute("SZ_ECDeditM", "{lang}/{rootID}/eCommerceData/{typeID}/Month/Edit/{id}", "~/myECdata_SZ/EditByMonth.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }
                , { "id", "new" }});
        routes.MapPageRoute("SZ_ECDeditD", "{lang}/{rootID}/eCommerceData/{typeID}/Date/Edit/{id}", "~/myECdata_SZ/EditByDay.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }
                , { "id", "new" }});

        //電商平台業績-上海
        routes.MapPageRoute("SH_ECDsearchY", "{lang}/{rootID}/ecData/{typeID}/Year", "~/myECdata_SH/SearchByYear.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }});
        routes.MapPageRoute("SH_ECDsearchM", "{lang}/{rootID}/ecData/{typeID}/Month", "~/myECdata_SH/SearchByMonth.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }});
        routes.MapPageRoute("SH_ECDsearchD", "{lang}/{rootID}/ecData/{typeID}/Date", "~/myECdata_SH/SearchByDate.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }});

        routes.MapPageRoute("SH_ECDeditY", "{lang}/{rootID}/ecData/{typeID}/Year/Edit/{id}", "~/myECdata_SH/EditByYear.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }
                , { "id", "new" }});
        routes.MapPageRoute("SH_ECDeditM", "{lang}/{rootID}/ecData/{typeID}/Month/Edit/{id}", "~/myECdata_SH/EditByMonth.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }
                , { "id", "new" }});
        routes.MapPageRoute("SH_ECDeditD", "{lang}/{rootID}/ecData/{typeID}/Date/Edit/{id}", "~/myECdata_SH/EditByDay.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "typeID", "1" }
                , { "id", "new" }});



        //出貨明細表
        routes.MapPageRoute("ShipData_TW", "{lang}/{rootID}/ShipData-TW", "~/myShipmentData/Search_TW.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});
        routes.MapPageRoute("ShipData_SH", "{lang}/{rootID}/ShipData-SH", "~/myShipmentData/Search_SH.aspx", false,
          new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});
        routes.MapPageRoute("CustomsData_TW", "{lang}/{rootID}/CustomsData-TW", "~/myCustomsData/Search_TW.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});
        routes.MapPageRoute("ShipLocalData_TW", "{lang}/{rootID}/ShipLocalData-TW", "~/myShipmentData/Search_Local_TW.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});


        //應收帳款對帳
        routes.MapPageRoute("ARInformList", "{lang}/{rootID}/ARInform/{CompID}", "~/myARdata/Search.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});
        routes.MapPageRoute("ARInformS1", "{lang}/{rootID}/ARInform/{CompID}/Step1", "~/myARdata/Step1.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});
        routes.MapPageRoute("ARInformS2", "{lang}/{rootID}/ARInform/{CompID}/Step2/{id}", "~/myARdata/Step2.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});
        routes.MapPageRoute("ARInformS3", "{lang}/{rootID}/ARInform/{CompID}/Step3/{id}", "~/myARdata/Step3.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});
        routes.MapPageRoute("ARInformS4", "{lang}/{rootID}/ARInform/{CompID}/Step4/{id}", "~/myARdata/Step4.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});
        routes.MapPageRoute("ARInformCust", "{lang}/{rootID}/ARInform/{CompID}/CustList", "~/myARdata/CustList.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "CompID", "TW" }});
        //快遞貨運登記
        routes.MapPageRoute("DeliverySearch", "{lang}/{rootID}/Delivery", "~/myDelivery/Search.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});
        routes.MapPageRoute("DeliveryEdit", "{lang}/{rootID}/Delivery/Edit/{id}", "~/myDelivery/Edit.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "id", "new" }});
        routes.MapPageRoute("DeliveryView", "{lang}/{rootID}/Delivery/View/{id}", "~/myDelivery/View.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }
                , { "id", "new" }});
        routes.MapPageRoute("DeliveryAddr", "{lang}/{rootID}/Delivery/Address", "~/myDelivery/InformConfig.aspx", false,
        new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "3000" }});



        //--- 生產採購(4000) --- 
        //到貨狀況
        routes.MapPageRoute("OpcsSearch", "{lang}/{rootID}/OpcsStatus/{CompID}", "~/myOpcsStatus/Search.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "4000" }
                , { "CompID", "TW" }});

        //延遲分析
        routes.MapPageRoute("DelayShipStat", "{lang}/{rootID}/DelayShipStat", "~/myDelayShipStat/Search.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "4000" }});


        //外廠包材庫存盤點
        routes.MapPageRoute("SICSearch", "{lang}/{rootID}/SupInvCheck", "~/mySupInvCheck/Search.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "4000" }});
        //routes.MapPageRoute("SICEdit", "{lang}/{rootID}/SupInvCheck/Edit/{id}", "~/mySupInvCheck/Edit.aspx", false,
        //    new RouteValueDictionary {
        //        { "lang", "auto" }
        //        , { "rootID", "4000" }
        //        , { "id", "new" }});
        routes.MapPageRoute("SICView", "{lang}/{rootID}/SupInvCheck/View/{id}/{sup}", "~/mySupInvCheck/View.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "4000" }
                , { "id", "0" }});
        routes.MapPageRoute("SICsetSearch", "{lang}/{rootID}/SupInvCheck/Set", "~/mySupInvCheck/SettingSearch.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "4000" }});
        routes.MapPageRoute("SICsetEdit", "{lang}/{rootID}/SupInvCheck/SetEdit/{id}", "~/mySupInvCheck/SettingEdit.aspx", false,
            new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "4000" }
                , { "id", "new" }});
        routes.MapPageRoute("SICsetView", "{lang}/{rootID}/SupInvCheck/SetView/{id}", "~/mySupInvCheck/SettingView.aspx", false,
           new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "4000" }
                , { "id", "0" }});

        //BOM篩選        
        routes.MapPageRoute("BOMFilter", "{lang}/{rootID}/BOMFilter/{CompID}", "~/myBOMfilter/Search.aspx", false,
         new RouteValueDictionary {
                { "lang", "auto" }
                , { "rootID", "4000" }
                , { "CompID", "TW" }});


        //Error
        routes.MapPageRoute("ErrPage", "Error/{msg}", "~/myPage/ErrorPage.aspx", false);

    }


</script>

﻿using System;
using System.Collections.Generic;
using System.Text;
using PKLib_Method.Methods;

public partial class myShipping_Ascx_StepMenu : System.Web.UI.UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //填入選項
        List<TabMenu> listTab = new List<TabMenu>();
        listTab.Add(new TabMenu(1, "上傳運費Excel", "注意上傳格式"));
        listTab.Add(new TabMenu(2, "確認上傳資料", "選擇銷貨單日期區間"));
        listTab.Add(new TabMenu(3, "物流單 & 運費資料轉入", "ERP銷貨單備註分析後轉入至平台"));
        listTab.Add(new TabMenu(4, "完成", ""));

        //產生Html
        StringBuilder sbTab = new StringBuilder();

        foreach (var item in listTab)
        {
            string css = "";
            int listIdx = item.TabIndex;

            if (listIdx < nowIndex)
            {
                css = "completed";
            }
            if (listIdx.Equals(nowIndex))
            {
                css = "active";
            }
            if (listIdx > nowIndex)
            {
                css = "disabled";
            }
            sbTab.Append("<div class=\"{0} step\">".FormatThis(css));
            sbTab.Append(" <div class=\"content\">");
            sbTab.Append("   <div class=\"title\">{0}</div>".FormatThis(item.TabName));
            sbTab.Append("   <div class=\"description\">{0}</div>".FormatThis(item.TabDesc));
            sbTab.Append(" </div>");
            sbTab.Append("</div>");
        }


        //output
        this.lt_Menu.Text = sbTab.ToString();
    }

    /// <summary>
    /// [參數] - 目前選項
    /// </summary>
    public int nowIndex
    {
        get;
        set;
    }
    private int _nowIndex;

    /// <summary>
    /// Tab選單
    /// </summary>
    public class TabMenu
    {
        /// <summary>
        /// [參數] - Tab位置
        /// </summary>
        private int _TabIndex;
        public int TabIndex
        {
            get { return this._TabIndex; }
            set { this._TabIndex = value; }
        }

        /// <summary>
        /// [參數] - Tab名稱
        /// </summary>
        private string _TabName;
        public string TabName
        {
            get { return this._TabName; }
            set { this._TabName = value; }
        }


        /// <summary>
        /// [參數] - Tab 描述
        /// </summary>
        private string _TabDesc;
        public string TabDesc
        {
            get { return this._TabDesc; }
            set { this._TabDesc = value; }


        }
        /// <summary>
        /// 設定參數值
        /// </summary>
        /// <param name="TabIndex">Tab位置</param>
        /// <param name="TabName">Tab名稱</param>
        /// <param name="TabDesc">描述</param>
        public TabMenu(int TabIndex, string TabName, string TabDesc)
        {
            _TabIndex = TabIndex;
            _TabName = TabName;
            _TabDesc = TabDesc;
        }
    }
}
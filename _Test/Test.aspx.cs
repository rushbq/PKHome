using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PKLib_Method.Methods;

public partial class _Test_Test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        /*
          Set New Cookie
        */
        /*
        HttpCookie setCookie = new HttpCookie("PKHome_CCPInfo");
        HttpCookie readCookie = Request.Cookies["PKHome_CCPInfo"];
        setCookie.HttpOnly = true;

        if (readCookie != null && readCookie.Values.Count > 0)
        {
            foreach (string item in readCookie.Values)
            {
                //insert old cookie
                setCookie.Values.Add(item, readCookie.Values[item]);
            }
        }

        //設定多值, New cookie
        setCookie.Values.Add("30190814001", "ab8e5a8d-8d4b-452d-b714-123e27cbff26");    //CC_UID, Data Guid

        //設定到期日(1 Day)
        setCookie.Expires = DateTime.Now.AddDays(1);

        //寫到用戶端
        Response.Cookies.Add(setCookie);
        */

        /*
          Remove Cookie
        */
        //HttpCookie removeCookie = Request.Cookies["PKHome_CCPInfo"];
        //removeCookie.Values.Remove("10190708001");
        //Response.Cookies.Add(removeCookie);
        ////當集合內無值時,清掉整個cookie
        //if (removeCookie.Values.Count == 0)
        //{
        //    //set expire
        //    setCookie.Expires = DateTime.Now.AddDays(-1);
        //    setCookie.Values.Clear();
        //    //reset
        //    Response.Cookies.Set(setCookie);
        //}


        /*
          Clear Cookie
        */
        //HttpCookie clearCookie = new HttpCookie("PKHome_CCPInfo");
        ////set expire
        //clearCookie.Expires = DateTime.Now.AddDays(-1);
        //clearCookie.Values.Clear();
        ////reset
        //Response.Cookies.Set(clearCookie);

        /*
          Read Cookie
         */
        //HttpCookie readCookie = Request.Cookies["PKHome_CCPInfo"];
        //if (readCookie != null && readCookie.Values.Count > 0)
        //{
        //    foreach (string item in readCookie.Values)
        //    {
        //        //value
        //        Response.Write(readCookie.Values[item] + "<br>");
        //    }

        //    ArrayList ary = new ArrayList();
        //    foreach (string item in readCookie.Values)
        //    {
        //        //name
        //        ary.Add(item);
        //        //Response.Write(item + "<br>");
        //    }
        //    Response.Write(string.Join(",", ary.ToArray()));
        //}

        string email = "jannyei@jannyei.com.tw;test@jannyei.com.tw;";
        string[] aryMail = Regex.Split(email, ";");
        foreach(var item in aryMail)
        {
            if (!string.IsNullOrWhiteSpace(item))
            {
                Response.Write(item + "_");
            }
        }

    }
}
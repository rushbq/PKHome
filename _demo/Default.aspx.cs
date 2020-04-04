using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _demo_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Dictionary<string, int> dic = new Dictionary<string, int>();

        dic.Add("1", 20);
        dic.Add("2", 30);

        dic["1"] = dic["1"] + 2;

        //Response.Write(dic["1"]);

        var query = dic
            .OrderBy(o => o.Key)
            .Select(fld => new
                {
                    x = fld.Value
                }
            );
        foreach(var item in query)
        {
            Response.Write(item.x +"<br>");
        }
        //Hashtable sumRow = new Hashtable();
        //sumRow.Add(1, 0);
        //sumRow.Add(2, 0);

        //sumRow[1] = 100;
        //sumRow[1] = Convert.ToInt32(sumRow[1]) + 2;
        //Response.Write(sumRow[1]);
        //string newVal = DateTime.Now.ToString();
        //string val = getCookie("HomeList_Shipping", newVal, 1);

        //Response.Write(val);


        //Get New Cookie
        //var respCookie = Request.Cookies[CkNameKey];
        ////Response.Cookies.Add(new HttpCookie("PKHome_Lang", defCName));
        ////Response.Cookies["PKHome_Lang"].Expires = DateTime.Now.AddYears(1);
        //TextBox1.Text = respCookie.Value;
    }

    /// <summary>
    /// 設定&取得Cookies
    /// </summary>
    /// <param name="ckName">名稱</param>
    /// <param name="ckValue">傳入值</param>
    /// <param name="expireHours">小時</param>
    /// <returns></returns>
    /// <example>
    /// string val = getCookie("HomeList_Shipping", "hello", 1);
    /// </example>
    string getCookie(string ckName, string ckValue, int expireHours)
    {
        //取得目前cookie
        var requestCookie = Request.Cookies[ckName];

        //判斷cookie是否存在
        if (requestCookie != null)
        {
            //cookie存在, 判斷內容與新設定值是否相同
            if (!requestCookie.Value.Equals(ckValue))
            {
                //Reset Cookie
                resetCookie(ckName, ckValue, expireHours);
            }
        }
        else
        {
            //Reset Cookie
            resetCookie(ckName, ckValue, expireHours);
        }

        //Get New Cookie
        var respCookie = Request.Cookies[ckName];


        return respCookie.Value;
    }

    private void resetCookie(string ckName, string ckValue, int expireHours)
    {
        // 產生新的值並儲存到 cookie
        var responseCookie = new HttpCookie(ckName)
        {
            HttpOnly = true,
            Value = ckValue,
            Expires = DateTime.Now.AddHours(expireHours)
        };

        //Update
        Response.Cookies.Set(responseCookie);
    }
}
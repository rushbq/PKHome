<%@ WebHandler Language="C#" Class="Ashx_CookieSet" %>

using System;
using System.Web;
using System.Collections;
using PKLib_Method.Methods;

public class Ashx_CookieSet : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        //Delay 1 sec.
        //System.Threading.Thread.Sleep(1000);

        //ContentType
        context.Response.ContentType = "text/html";

        //[接收參數]
        string _guid = context.Request["guid"];
        string _ccpid = context.Request["ccpid"];
        string _type = context.Request["setType"];
        string _typeID = context.Request["typeID"]; //TW(工/科)/SZ(工/科)
        string _cookieName = "PKHome_CCPInfo";

        //宣告
        HttpCookie setCookie = new HttpCookie(_cookieName);
        HttpCookie reqCookie = context.Request.Cookies[_cookieName];
        setCookie.HttpOnly = true;

        try
        {
            switch (_type)
            {
                case "readChecked":
                    //已勾選項目
                    if (reqCookie == null && reqCookie.Values.Count == 0)
                    {
                        context.Response.Write("");
                    }
                    else
                    {
                        ArrayList ary = new ArrayList();
                        foreach (string item in reqCookie.Values)
                        {
                            ary.Add(item);
                        }
                        context.Response.Write(string.Join(",", ary.ToArray()));
                    }

                    break;


                case "readCount":
                    //已勾選數量
                    int row = 0;
                    if (reqCookie != null)
                    {
                        foreach (string item in reqCookie.Values)
                        {
                            if (item.Left(2).Equals(_typeID))
                            {
                                row++;
                            }
                        }
                    }

                    //response
                    context.Response.Write(row);

                    break;


                case "set":
                    //新增COOKIE
                    if (reqCookie != null && reqCookie.Values.Count > 0)
                    {
                        foreach (string item in reqCookie.Values)
                        {
                            //insert old cookie
                            setCookie.Values.Add(item, reqCookie.Values[item]);
                        }
                    }

                    //設定值, New cookie
                    setCookie.Values.Add(_ccpid, _guid);    //CC_UID, Data Guid

                    //設定到期日(1 Day)
                    setCookie.Expires = DateTime.Now.AddDays(1);

                    //寫到用戶端
                    context.Response.Cookies.Add(setCookie);

                    //response
                    context.Response.Write("200");

                    break;


                case "remove":
                    //移除COOKIE
                    reqCookie.Values.Remove(_ccpid);
                    context.Response.Cookies.Add(reqCookie);

                    //當集合內無值時,清掉整個cookie
                    if (reqCookie.Values.Count == 0)
                    {
                        //set expire
                        setCookie.Expires = DateTime.Now.AddDays(-1);
                        setCookie.Values.Clear();
                        //reset
                        context.Response.Cookies.Set(setCookie);
                    }

                    //response
                    context.Response.Write("200");

                    break;


                case "clear":
                    //set expire
                    setCookie.Expires = DateTime.Now.AddDays(-1);
                    setCookie.Values.Clear();
                    //reset
                    context.Response.Cookies.Set(setCookie);

                    //response
                    context.Response.Write("200");

                    break;

                default:
                    break;
            }

        }
        catch (Exception ex)
        {
            context.Response.Write("error:" + ex.Message.ToString());
        }
        finally
        {
            setCookie = null;
            reqCookie = null;
        }

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}
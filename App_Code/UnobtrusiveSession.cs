using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Web;

/// <summary>
/// UnobtrusiveSession 使用 MemoryCache 保存資料
/// </summary>
/// <remarks>
/// 以 Cookie 為憑存取實際儲存於 MemoryCache 的資料，Cache 保存政策則比照 Session 設為 20 分鐘不存取自動清除
/// ** 必須加入參考 System.Runtime.Caching
/// </remarks>

public static class UnobtrusiveSession
{
    static HttpContext CurrContext
    {
        get
        {
            if (HttpContext.Current == null)
                throw new ApplicationException("HttpContext.Current is null");
            return HttpContext.Current;
        }
    }
    const string COOKIE_KEY = "UnobtrusiveSessionId";
    public static string SessionId
    {
        get
        {
            var cookie = CurrContext.Request.Cookies[COOKIE_KEY];
            if (cookie != null) return cookie.Value;
            //set session id cookie
            var sessId = Guid.NewGuid().ToString();
            CurrContext.Response.SetCookie(new HttpCookie(COOKIE_KEY, sessId));
            return sessId;
        }
    }
    public static SessionObject Session
    {
        get
        {
            var cache = MemoryCache.Default;
            var sessId = SessionId;
            if (!cache.Contains(sessId))
            {
                cache.Add(sessId, new SessionObject(sessId), new CacheItemPolicy()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(20)
                });
            }
            return (SessionObject)cache[sessId];
        }
    }

    public class SessionObject
    {
        public string SessionId;
        Dictionary<string, object> items =
            new Dictionary<string, object>();
        public SessionObject(string sessId)
        {
            SessionId = sessId;
        }
        public object this[string key]
        {
            get
            {
                lock (items)
                {
                    if (items.ContainsKey(key)) return items[key];
                    return null;
                }
            }
            set
            {
                lock (items)
                {
                    items[key] = value;
                }
            }
        }

    }
}
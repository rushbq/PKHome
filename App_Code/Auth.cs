using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace AuthData.Models
{
    public class Auth
    {
        public string MenuID { get; set; }
        public string ParentID { get; set; }
        public string MenuName { get; set; }
        public bool ItemChecked { get; set; }
        public int Lv { get; set; }
        public string Url { get; set; }
        public string Target { get; set; }
    }

    public class DBInfo
    {
        public int UID { get; set; }
        public string DBName { get; set; }
        public string DBDesc { get; set; }
    }

    public class AuthRel
    {
        public string MenuID { get; set; }
        public string UserID { get; set; }
    }
}

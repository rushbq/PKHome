using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using PKLib_Method.Methods;

/// <summary>
/// AD 資訊
/// </summary>
public class ADService
{
    /// <summary>
    /// 處理訊息
    /// </summary>
    private static string _ProcMessage;
    public static string ProcMessage
    {
        get;
        private set;
    }

    #region "LDAP查詢"

    public class LookupLDAP
    {
        #region "參數設定"
        /// <summary>
        /// GUID - objectGUID
        /// </summary>
        private string _GUID;
        public string GUID
        {
            get { return this._GUID; }
            set { this._GUID = value; }
        }
        /// <summary>
        /// SID - objectSid
        /// </summary>
        private string _SID;
        public string SID
        {
            get { return this._SID; }
            set { this._SID = value; }
        }
        /// <summary>
        /// 顯示名稱 - cn
        /// </summary>
        private string _DisplayName;
        public string DisplayName
        {
            get { return this._DisplayName; }
            set { this._DisplayName = value; }
        }
        /// <summary>
        /// 帳戶名稱 - saMAccountname
        /// </summary>
        private string _AccountName;
        public string AccountName
        {
            get { return this._AccountName; }
            set { this._AccountName = value; }
        }
        /// <summary>
        /// 部門 - department
        /// </summary>
        private string _Department;
        public string Department
        {
            get { return this._Department; }
            set { this._Department = value; }
        }
        /// <summary>
        /// Email
        /// </summary>
        private string _Email;
        public string Email
        {
            get { return this._Email; }
            set { this._Email = value; }
        }

        /// <summary>
        /// 帳戶類型([2+512] -已停用, 512-預設類型)
        /// </summary>
        private int _userAccountControl;
        public int userAccountControl
        {
            get { return this._userAccountControl; }
            set { this._userAccountControl = value; }
        }

        /// <summary>
        /// LDAP Path
        /// </summary>
        private string _LDAPPath;
        public string LDAPPath
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// 設定參數值 - 不含部門
        /// </summary>
        /// <param name="GUID">AD GUID</param>
        /// <param name="SID">AD SID</param>
        /// <param name="DisplayName">AD DisplayName</param>
        /// <param name="AccountName">AD saMaccountName</param>
        public LookupLDAP(string GUID, string SID, string DisplayName, string AccountName, string Email)
        {
            this._GUID = GUID;
            this._SID = SID;
            this._DisplayName = DisplayName;
            this._AccountName = AccountName;
            this._Email = Email;
        }
        /// <summary>
        /// 設定參數值 - 含部門
        /// </summary>
        /// <param name="GUID">AD GUID</param>
        /// <param name="SID">AD SID</param>
        /// <param name="DisplayName">AD DisplayName</param>
        /// <param name="AccountName">AD saMaccountName</param>
        /// <param name="Department">AD department</param>
        /// <param name="userAccountControl">AD userAccountControl</param>
        public LookupLDAP(string GUID, string SID, string DisplayName, string AccountName, string Department, int userAccountControl, string Email)
        {
            this._GUID = GUID;
            this._SID = SID;
            this._DisplayName = DisplayName;
            this._AccountName = AccountName;
            this._Department = Department;
            this._userAccountControl = userAccountControl;
            this._Email = Email;
        }
    }

    /// <summary>
    /// 列出所有群組
    /// </summary>
    /// <param name="LPath">LDAP路徑</param>
    /// <returns>List</returns>
    public static List<LookupLDAP> ListGroups(string LPath)
    {
        try
        {
            //[判斷參數] - LDAPPath
            if (string.IsNullOrEmpty(LPath))
            {
                ProcMessage = "LDAP 路徑空白";
                return null;
            }

            /* 取得所有群組 */
            DirectoryEntry de = new DirectoryEntry();
            //LDAP路徑 
            de.Path = LPath; //DC路徑 
            //驗証類型
            de.AuthenticationType = AuthenticationTypes.Secure; //驗証類型

            //顯示欄位 (顯示名稱, 登入帳號, GUID, SID, Email)
            string[] loadProps = new string[] { "cn", "samaccountname", "objectGUID", "objectSid" };

            //宣告DirectorySearcher, 對AD執行查詢
            using (DirectorySearcher deSearch = new DirectorySearcher(de, "(objectClass=group)", loadProps))
            {
                //分頁筆數(預設上限為1000)
                deSearch.PageSize = 1000;

                List<LookupLDAP> ILdap = new List<LookupLDAP>();

                //SafeFindAll
                var results = SafeFindAll(deSearch);
                foreach (SearchResult result in results)
                {
                    //取得GUID/SID
                    using (DirectoryEntry entry = result.GetDirectoryEntry())
                    {
                        //SID
                        byte[] byteArray = (byte[])result.Properties["objectSid"][0];
                        SecurityIdentifier mySID = new SecurityIdentifier(byteArray, 0);

                        ILdap.Add(new LookupLDAP(
                            entry.Guid.ToString("B")
                            , mySID.ToString()
                            , result.Properties["cn"][0].ToString()
                            , result.Properties["samaccountname"][0].ToString()
                            , ""
                           ));
                    }
                }

                return ILdap;
            }
        }
        catch (Exception ex)
        {
            ProcMessage = ex.Message.ToString();
            return null;
        }
    }

    /// <summary>
    /// 列出所有使用者
    /// </summary>
    /// <param name="LPath">LDAP路徑</param>
    /// <returns>List</returns>
    public static List<LookupLDAP> ListUsers(string LPath)
    {
        try
        {
            //[判斷參數] - LDAPPath
            if (string.IsNullOrEmpty(LPath))
            {
                ProcMessage = "LDAP 路徑空白";
                return null;
            }

            /* 取得所有群組 */
            DirectoryEntry de = new DirectoryEntry();
            //LDAP路徑 
            de.Path = LPath; //DC路徑 
            //驗証類型
            de.AuthenticationType = AuthenticationTypes.Secure; //驗証類型

            //顯示欄位 (顯示名稱, 登入帳號, 完整路徑, 部門)
            string[] loadProps = new string[] { "cn", "samaccountname", "objectGUID", "objectSid", "department", "userAccountControl", "mail" };

            //宣告DirectorySearcher, 對AD執行查詢
            using (DirectorySearcher deSearch = new DirectorySearcher(de, "(&(objectClass=user)(objectCategory=Person))", loadProps))
            {
                //分頁筆數(預設上限為1000)
                deSearch.PageSize = 1000;

                List<LookupLDAP> ILdap = new List<LookupLDAP>();

                //SafeFindAll
                var results = SafeFindAll(deSearch);
                foreach (SearchResult result in results)
                {
                    using (DirectoryEntry entry = result.GetDirectoryEntry())
                    {
                        //SID
                        byte[] byteArray = (byte[])result.Properties["objectSid"][0];
                        SecurityIdentifier mySID = new SecurityIdentifier(byteArray, 0);

                        /*
                         * 取得部門代號
                         * 判斷是否有分隔符號:「-」
                         * 若無「-」，則取左 3 碼
                         */
                        //部門-判斷&拆解, 
                        string tagChar = "-";
                        string deptResult = "";
                        try
                        {
                            deptResult = result.Properties["department"][0].ToString();
                        }
                        catch (Exception)
                        {
                            deptResult = "";
                        }
                        if (deptResult.IndexOf(tagChar) != -1)
                        {
                            //取得第0列的值
                            string[] strAry = Regex.Split(deptResult, @"\" + tagChar + "{1}");
                            deptResult = strAry[0];
                        }
                        else
                        {
                            deptResult = deptResult.Left(3);
                        }

                        //取得帳號類型([2+512]-已停用, 512-預設類型)
                        int accCtrl = (int)entry.Properties["userAccountControl"].Value;
                        //取得Email
                        string mail = (string)entry.Properties["mail"].Value;

                        ILdap.Add(new LookupLDAP(
                            entry.Guid.ToString("B")
                            , mySID.ToString()
                            , result.Properties["cn"][0].ToString()
                            , result.Properties["samaccountname"][0].ToString()
                            , deptResult
                            , accCtrl
                            , mail
                            ));
                    }
                }

                return ILdap;
            }
        }
        catch (Exception ex)
        {
            ProcMessage = ex.Message.ToString();
            return null;
        }
    }


    /// <summary>
    /// 列出所有使用者
    /// </summary>
    /// <param name="LPath">LDAP路徑</param>
    /// <param name="GroupGuid"></param>
    /// <returns>List</returns>
    public static List<LookupLDAP> ListUsers(string LPath, string GroupGuid)
    {
        try
        {
            //[判斷參數] - LDAPPath
            if (string.IsNullOrEmpty(LPath))
            {
                ProcMessage = "LDAP 路徑空白";
                return null;
            }

            /* 取得所有群組 */
            DirectoryEntry de = new DirectoryEntry();
            //LDAP路徑 
            de.Path = LPath; //DC路徑 
            //驗証類型
            de.AuthenticationType = AuthenticationTypes.Secure; //驗証類型

            //顯示欄位 (顯示名稱, 登入帳號, 完整路徑, 部門)
            string[] loadProps = new string[] { "cn", "samaccountname", "objectGUID", "objectSid", "userAccountControl", "mail" };

            //宣告DirectorySearcher, 對AD執行查詢
            using (DirectorySearcher deSearch = new DirectorySearcher(de, "(&(objectClass=user)(objectCategory=Person))", loadProps))
            {
                //分頁筆數(預設上限為1000)
                deSearch.PageSize = 1000;
                deSearch.Filter = "(memberOf=CN=" + GroupGuid + ",OU=MIS_Management,DC=prokits,DC=com,DC=tw)";

                List<LookupLDAP> ILdap = new List<LookupLDAP>();

                //SafeFindAll
                var results = SafeFindAll(deSearch);
                foreach (SearchResult result in results)
                {
                    using (DirectoryEntry entry = result.GetDirectoryEntry())
                    {
                        //SID
                        byte[] byteArray = (byte[])result.Properties["objectSid"][0];
                        SecurityIdentifier mySID = new SecurityIdentifier(byteArray, 0);

                        
                        //取得帳號類型([2+512]-已停用, 512-預設類型)
                        int accCtrl = (int)entry.Properties["userAccountControl"].Value;
                        //取得Email
                        string mail = (string)entry.Properties["mail"].Value;

                        ILdap.Add(new LookupLDAP(
                            entry.Guid.ToString("B")
                            , mySID.ToString()
                            , result.Properties["cn"][0].ToString()
                            , result.Properties["samaccountname"][0].ToString()
                            , ""
                            , accCtrl
                            , mail
                            ));
                    }
                }

                return ILdap;
            }
        }
        catch (Exception ex)
        {
            ProcMessage = ex.Message.ToString();
            return null;
        }
    }



    //釋放 SearchResultCollection
    private static IEnumerable<SearchResult> SafeFindAll(DirectorySearcher searcher)
    {
        using (SearchResultCollection results = searcher.FindAll())
        {
            foreach (SearchResult result in results)
            {
                yield return result;
            }
        } // SearchResultCollection will be disposed here
    }
    #endregion

    /// <summary>
    /// 取得群組/使用者的 GUID - 使用SAMAccountname
    /// </summary>
    /// <param name="pSAMAccountname">Account Name</param>
    /// <returns>Hex Octet in String</returns>
    public static String getGUIDFromName(String pSAMAccountname)
    {
        String result = null;
        DirectoryEntry mEntry = new DirectoryEntry();
        using (DirectorySearcher mySearcher = new DirectorySearcher())
        {
            mySearcher.SearchRoot = mEntry;
            mySearcher.Filter = "(SAMAccountName=" + pSAMAccountname + ")";

            SearchResult groupResult = mySearcher.FindOne();
            if (groupResult != null)
            {
                DirectoryEntry groupEntry = groupResult.GetDirectoryEntry();

                //Get objectGUID as bytearray
                Guid objectGuid = new Guid((byte[])groupEntry.Properties["objectGUID"].Value);
                byte[] arrbyte = objectGuid.ToByteArray();

                //Use this Format: {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}
                result = objectGuid.ToString("B");
            }
            return result;
        }
    }

    /// <summary>
    /// 取得 SID - 使用SAMAccountname
    /// </summary>
    /// <param name="pSAMAccountname">Account Name</param>
    /// <returns>SID</returns>
    public static String getSIDFromName(String pSAMAccountname)
    {
        String result = null;
        DirectoryEntry mEntry = new DirectoryEntry();
        using (DirectorySearcher mySearcher = new DirectorySearcher())
        {
            mySearcher.SearchRoot = mEntry;
            mySearcher.Filter = "(SAMAccountName=" + pSAMAccountname + ")";
            //mySearcher.ClientTimeout = TimeSpan.FromSeconds(5);

            SearchResult groupResult = mySearcher.FindOne();
            if (groupResult != null)
            {
                DirectoryEntry groupEntry = groupResult.GetDirectoryEntry();
                byte[] byteArray = (byte[])groupEntry.Properties["objectSid"][0];
                SecurityIdentifier mySID = new SecurityIdentifier(byteArray, 0);

                result = mySID.ToString();
            }
            return result;
        }
    }

    /// <summary>
    /// 取得 AccountName - 使用GUID
    /// </summary>
    /// <param name="pGUID">GUID in String of Format: {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx}, xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx</param>
    /// <returns>Account Name</returns>
    public static String getNameFromGUID(String pGUID)
    {
        String result = null;
        DirectoryEntry mEntry = new DirectoryEntry();
        using (DirectorySearcher mySearcher = new DirectorySearcher())
        {
            //Change Format to OctetString
            Guid objectGUID = new Guid(pGUID);
            String strGUID = GUID2OctectString(objectGUID);

            mySearcher.SearchRoot = mEntry;
            mySearcher.Filter = "(objectGUID=" + strGUID + ")";

            SearchResult groupResult = mySearcher.FindOne();
            if (groupResult != null)
            {
                DirectoryEntry groupEntry = groupResult.GetDirectoryEntry();
                result = groupEntry.Properties["SAMAccountName"][0].ToString();
            }
            return result;
        }
    }

    /// <summary>
    /// 取得部門 - 使用GUID
    /// </summary>
    /// <param name="pGUID">GUID</param>
    /// <returns>部門代號</returns>
    /// <remarks>
    /// AD欄位名稱:department
    /// AD欄位格式:101-專案部
    /// 處理:
    ///  1) 以「-」為分隔符號
    ///  2) 回傳Array[0]的值 -> 部門代號
    ///  3) 部門代號對應Table:PKSYS.User_Dept
    /// </remarks>
    public static String getDepartmentFromGUID(String pGUID)
    {
        String result = null;
        DirectoryEntry mEntry = new DirectoryEntry();
        using (DirectorySearcher mySearcher = new DirectorySearcher())
        {
            //Change Format to OctetString
            Guid objectGUID = new Guid(pGUID);
            String strGUID = GUID2OctectString(objectGUID);

            mySearcher.SearchRoot = mEntry;
            mySearcher.Filter = "(objectGUID=" + strGUID + ")";

            SearchResult groupResult = mySearcher.FindOne();
            if (groupResult != null)
            {
                DirectoryEntry groupEntry = groupResult.GetDirectoryEntry();
                result = groupEntry.Properties["department"][0].ToString();
                //判斷是否有分隔符號
                string tagChar = "-";
                if (result.IndexOf(tagChar) != -1)
                {
                    //取得第0列的值
                    string[] strAry = Regex.Split(result, @"\" + tagChar + "{1}");
                    result = strAry[0];
                }
            }
            return result;
        }
    }

    #region private utility functions

    /// <summary>
    /// Change GUID to OctetString
    /// </summary>
    /// <param name="pGUID">GUID</param>
    /// <returns></returns>
    private static String GUID2OctectString(Guid pGUID)
    {
        String strGUID = "";
        foreach (byte b in pGUID.ToByteArray())
        {
            strGUID += @"\" + b.ToString("x1");
        }
        return strGUID;
    }

    private enum ADObjectType
    {
        User,
        Group,
        Else
    }

    /// <summary>
    /// Get Object-Class of the Entry
    /// </summary>
    /// <param name="pEntry">Directory Entry</param>
    /// <returns></returns>
    private static ADObjectType getObjectTypeFromEntry(DirectoryEntry pEntry)
    {
        ADObjectType result = ADObjectType.Else;
        foreach (object Property in pEntry.Properties["objectClass"])
        {
            String objectClass = Property.ToString();
            switch (objectClass)
            {
                case "group":
                    result = ADObjectType.Group;
                    break;
                case "user":
                    result = ADObjectType.User;
                    break;
            }
        }
        return result;
    }

    /// <summary>
    /// Get all Parents  GUID in Format of {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx} from this Directory Entry
    /// </summary>
    /// <param name="pEntry">Directory Entry</param>
    /// <returns>ArrayList of GUID</returns>
    private static ArrayList getAllParentsGUIDFromPath(String pPath)
    {
        ArrayList result = new ArrayList();
        //Add itself to ArrayList
        DirectoryEntry myEntry = new DirectoryEntry(pPath);
        result.Add(myEntry.Guid.ToString("B"));

        foreach (object obj in myEntry.Properties["memberOf"])
        {
            //Use Recursiv to find parent groups
            ArrayList tempResult = getAllParentsGUIDFromPath("LDAP://" + obj.ToString());
            foreach (object tempObj in tempResult)
            {
                result.Add(tempObj);
            }
        }

        return result;
    }

    /// <summary>
    /// Get all Children GUID in Format of {xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx} from this Directory Entry
    /// </summary>
    /// <param name="pEntry">Directory Entry</param>
    /// <returns>ArrayList of GUID</returns>
    private static ArrayList getAllChildrenGUIDFromPath(String pPath)
    {
        ArrayList result = new ArrayList();
        //Add itself to ArrayList
        DirectoryEntry myEntry = new DirectoryEntry(pPath);
        result.Add(myEntry.Guid.ToString("B"));

        foreach (object obj in myEntry.Properties["member"])
        {
            //Use Recursiv to find parent groups
            ArrayList tempResult = getAllChildrenGUIDFromPath("LDAP://" + obj.ToString());
            foreach (object tempObj in tempResult)
            {
                //Check if Object is already in list
                Boolean isInList = false;
                foreach (String tempString in result)
                {
                    if (tempString == tempObj.ToString())
                    {
                        isInList = true;
                    }
                }

                if (!isInList)
                    result.Add(tempObj);
            }
        }

        return result;
    }

    #endregion

    /// <summary>
    /// 取得GUID - 使用SID
    /// </summary>
    /// <param name="pSID">SID</param>
    /// <returns>GUID or null</returns>
    public static String getGUIDFromSID(String pSID)
    {
        String result = null;

        DirectoryEntry mEntry = new DirectoryEntry();
        DirectorySearcher mySearcher = new DirectorySearcher();
        mySearcher.SearchRoot = mEntry;
        mySearcher.Filter = "(objectSid=" + pSID + ")";

        SearchResult groupResult = mySearcher.FindOne();
        if (groupResult != null)
        {
            DirectoryEntry groupEntry = groupResult.GetDirectoryEntry();
            result = groupEntry.Guid.ToString("B");
        }

        return result;
    }

    public static ArrayList getGroupGUIDFromGUID(Guid pGUID)
    {
        ArrayList result = new ArrayList();

        //Search for User
        DirectoryEntry mEntry = new DirectoryEntry();
        DirectorySearcher mySearcher = new DirectorySearcher();
        mySearcher.SearchRoot = mEntry;
        mySearcher.Filter = "(objectGUID=" + GUID2OctectString(pGUID) + ")";

        SearchResult objectResult = mySearcher.FindOne();
        if (objectResult != null)
        {
            DirectoryEntry objectEntry = objectResult.GetDirectoryEntry();
            String EntryPath = objectEntry.Path;
            objectEntry.Close();
            result = getAllParentsGUIDFromPath(EntryPath);
        }

        return result;
    }

    /// <summary>
    /// 取得使用者/群組屬性[0: SID, 1:name, 2:sAMAccountName, 3.Guid, 4:ObjectType]
    /// </summary>
    /// <param name="pGUID">GUID</param>
    /// <returns>陣列</returns>
    public static StringCollection getAttributesFromGUID(String pGUID)
    {
        try
        {
            StringCollection result = new StringCollection();
            DirectoryEntry mEntry = new DirectoryEntry();
            DirectorySearcher mySearcher = new DirectorySearcher();

            //Change Format to OctetString
            Guid objectGUID = new Guid(pGUID);
            String strGUID = GUID2OctectString(objectGUID);

            mySearcher.SearchRoot = mEntry;
            mySearcher.Filter = "(objectGUID=" + strGUID + ")";
            mySearcher.PropertiesToLoad.Add("name");
            mySearcher.PropertiesToLoad.Add("sAMAccountName");

            SearchResult groupResult = mySearcher.FindOne();
            if (groupResult != null)
            {
                DirectoryEntry groupEntry = groupResult.GetDirectoryEntry();

                //SID
                byte[] byteArray = (byte[])groupEntry.Properties["objectSid"][0];
                SecurityIdentifier mySID = new SecurityIdentifier(byteArray, 0);
                result.Add(mySID.ToString());

                //name, Account
                try
                {
                    result.Add(groupEntry.Properties["name"][0].ToString());
                    result.Add(groupEntry.Properties["sAMAccountName"][0].ToString());
                }
                catch (Exception)
                {
                    throw;
                }

                //Guid
                result.Add(pGUID);
                //ObjectType
                result.Add(getObjectTypeFromEntry(groupEntry).ToString());
            }
            else
            {
                return null;    //not found
            }

            return result;
        }
        catch (Exception)
        {
            throw new Exception("無法取得與AD的連線或AD資訊錯誤");
        }

    }

    /// <summary>
    /// 取得使用者/群組屬性[0: SID, 1:name, 2:sAMAccountName, 3.Guid, 4:ObjectType]
    /// </summary>
    /// <param name="pSID">SID</param>
    /// <returns></returns>
    public static StringCollection getAttributesFromSID(String pSID)
    {
        StringCollection result = null;
        String strGUID = getGUIDFromSID(pSID);
        if (strGUID != null)
        {
            result = getAttributesFromGUID(strGUID);
        }
        return result;
    }

}

#region ---登入驗證(輸入帳密)---

public class LdapAuthentication
{
    private string _path;
    private string _paramSid;

    public LdapAuthentication(string path)
    {
        _path = path;
    }

    /// <summary>
    /// 驗證使用者輸入的AD帳號、密碼
    /// </summary>
    /// <param name="domain">網域名稱</param>
    /// <param name="username">使用者名稱</param>
    /// <param name="pwd">使用者密碼</param>
    /// <returns>boolean</returns>
    /// <see cref="http://msdn.microsoft.com/zh-tw/library/ms180890(v=vs.90).aspx"/>
    public bool IsAuthenticated(string domain, string username, string pwd)
    {
        string domainAndUsername = domain + @"\" + username;
        DirectoryEntry entry = new DirectoryEntry(_path, domainAndUsername, pwd);

        try
        {
            object obj = entry.NativeObject;

            //建立搜尋AD的物件
            DirectorySearcher search = new DirectorySearcher(entry);
            //設定條件, 帳號判斷
            search.Filter = "(SAMAccountName=" + username + ")";
            //擷取屬性
            search.PropertiesToLoad.Add("objectSid");
            //傳回找到的第一個項目
            SearchResult result = search.FindOne();

            if (null == result)
            {
                return false;
            }

            //取得SID
            DirectoryEntry dEntry = result.GetDirectoryEntry();
            byte[] byteArray = (byte[])dEntry.Properties["objectSid"][0];
            SecurityIdentifier mySID = new SecurityIdentifier(byteArray, 0);
            _paramSid = mySID.ToString();

        }
        catch (Exception)
        {
            return false;
            //throw new Exception(ex.Message);
        }

        return true;
    }

    //取得使用者SID
    public string GetSID
    {
        get { return _paramSid; }
        set { _paramSid = value; }
    }


}
#endregion

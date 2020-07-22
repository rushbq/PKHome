using System;
using AuthData.Controllers;

/// <summary>
/// 判斷使用者是否有使用權限
/// </summary>
public class fn_CheckAuth
{
    public static bool Check(string userID, string menuID)
    {
        AuthRepository _data = new AuthRepository();

        try
        {
            bool hasAuth = _data.Check_Auth(userID, menuID);

            return hasAuth;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _data = null;
        }


    }
}
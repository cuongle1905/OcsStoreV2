using Microsoft.AspNetCore.Http;

namespace OcsStore
{
    public class Session
    {
        public static string Token(HttpRequest request)
        {
            return request.HttpContext.Session.GetString("Token");
        }

        public static void SetToken(HttpRequest request, string token)
        {
            request.HttpContext.Session.SetString("Token", token);
        }

        public static int AdminType(HttpRequest request)
        {
            return request.HttpContext.Session.GetInt32("AdminType") ?? -1;
        }

        public static int EmployeeId(HttpRequest request)
        {
            return request.HttpContext.Session.GetInt32("EmployeeId") ?? -1;
        }

        public static void SetEmployeeId(HttpRequest request, int id)
        {
            request.HttpContext.Session.SetInt32("EmployeeId", id);
        }

        public static string EmployeeName(HttpRequest request)
        {
            return request.HttpContext.Session.GetString("EmployeeName");
        }

        public static void SetEmployeeName(HttpRequest request, string name)
        {
            request.HttpContext.Session.SetString("EmployeeName", name);
        }

        public static string DepartmentName(HttpRequest request)
        {
            return request.HttpContext.Session.GetString("DepartmentName");
        }

        public static void SetDepartmentName(HttpRequest request, string name)
        {
            request.HttpContext.Session.SetString("DepartmentName", name);
        }

        public static short UserId(HttpRequest request)
        {
            return (short)(request.HttpContext.Session.GetInt32("UserId") ?? -1);
        }

        public static void SetUserId(HttpRequest request, int userId)
        {
            request.HttpContext.Session.SetInt32("UserId", userId);
        }

        public static string Username(HttpRequest request)
        {
            return request.HttpContext.Session.GetString("Username");
        }

        public static void SetUsername(HttpRequest request, string username)
        {
            request.HttpContext.Session.SetString("Username", username);
        }

        public static bool IsSuperAdmin(HttpRequest request)
        {
            return (request.HttpContext.Session.GetInt32("IsSuperAdmin") ?? 0) == 1;
        }

        public static void SetIsSuperAdmin(HttpRequest request, bool isSuperAdmin)
        {
            request.HttpContext.Session.SetInt32("IsSuperAdmin", isSuperAdmin ? 1 : 0);
        }

        public static void Logout(HttpRequest request)
        {
            request.HttpContext.Session.Remove("UserId");
            request.HttpContext.Session.Remove("Token");
        }

    }
}

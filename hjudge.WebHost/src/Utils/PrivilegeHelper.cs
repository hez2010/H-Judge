namespace hjudge.WebHost.Utils
{
    public class PrivilegeHelper
    {
        public static bool IsTeacher(int? privilege) => privilege >= 1 && privilege <= 3;
        public static bool IsAdmin(int? privilege) => privilege == 1;
    }
}

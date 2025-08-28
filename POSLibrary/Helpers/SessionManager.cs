namespace POSLibrary.Helpers
{
    public static class SessionManager
    {
        public static Entities.User CurrentUser { get; private set; }

        public static void SetUser(Entities.User user)
        {
            CurrentUser = user;
        }

        public static void Clear()
        {
            CurrentUser = null;
        }

        public static bool IsLoggedIn()
        {
            return CurrentUser != null;
        }

        public static Entities.User GetUser()
        {
            return CurrentUser;
        }
    }
}

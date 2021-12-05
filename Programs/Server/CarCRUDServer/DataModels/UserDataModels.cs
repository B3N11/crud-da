namespace CarCRUD.DataModels
{
    public class UserData
    {
        public string id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string type { get; set; }
        public bool active { get; set; }
        public UserRequest request { get; set; }
    }

    public class UserRequest
    {
        public bool accountRemove { get; set; }
        public bool brandAttach { get; set; }
    }

    public enum UserStatus
    {
        PendingAuthentication,
        Authenticated,
        LoggedIn,
        Connected,
        Disconnected,
        Dropped
    }
}

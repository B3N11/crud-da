namespace CarCRUD.DataModels
{
    public enum UserStatus
    {
        PendingAuthentication,
        Authenticated,
        LoggedIn,
        Connected,
        Disconnected,
        Dropped
    }

    public enum UserType
    {
        User,
        Admin
    }
}

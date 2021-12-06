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

    public enum NetMessageType
    {
        KeyAuthentication,
        LoginRequest,
        ReqistrationRequest,
        LoginResponse,
        AdminRegistrationRequest
    }

    public enum LoginAttemptResult
    {
        //Login
        InvalidUsername,     //Invalid login data
        InvalidPassword,     //Invalid login data
        LogginAttemptsMax,      //Client reached maximum login attempts
        AccountLocked,          //The desired account is inactive

        //Registration
        InvalidPasswordFormat,      //Password does not meet requirements
        NoPasswordMatch,        //Two passwords dont match
        UsernameExists,         //Username already exists

        //General
        Success,    //Successful login
        Failure     //Fail in code
    }
}

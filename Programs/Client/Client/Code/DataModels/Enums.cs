namespace CarCRUD.DataModels
{
    public enum UserStatus
    {
        PendingAuthentication,
        Authenticated,
        LoggedIn,
        LoggedOut,
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
        //Connection
        KeyAuthenticationRequest,
        KeyAuthenticationResponse,
        LoginRequest,
        RegistrationRequest,
        LoginResponse,
        AdminRegistrationRequest,
        AdminRegistrationResponse,
        Logout,

        //Messaging
        UserRequest,
        UserRequestResponse,
        BrandCreate,
        BrandCreateResponse,
        FavouriteCarCreateRequest,
        FavouriteCarCreateResponse,
        RequestAnswerRequest,
        RequestAnswerResponse,
        UserActivityResetRequest,
        UserActivityResetResponse,
        CarDeleteRequest,
        CarDeleteResponse
    }

    public enum LoginAttemptResult
    {
        //Login
        InvalidUsername,     //Invalid login data
        InvalidPassword,     //Invalid login data
        LogginAttemptsMax,      //Client reached maximum login attempts
        AccountLocked,          //The desired account is inactive
        AlreadyLoggedIn,

        //Registration
        InvalidPasswordFormat,      //Password does not meet requirements
        NoPasswordMatch,        //Two passwords dont match
        UsernameExists,         //Username already exists

        //General
        Success,    //Successful login
        Failure     //Fail in code
    }

    public enum UserRequestType
    {
        AccountDelete,
        BrandAttach
    }

    public enum UserRequestResult
    {
        AccountDeleteAlreadyRequested,
        CarPropertyAlreadyRequestedOrExists,
        Success,
        Fail
    }

    public enum GeneralResult
    {
        InvalidFormat,
        AlreadyExists,
        Success,
        Fail
    }
}
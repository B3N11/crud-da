﻿namespace CarCRUD.DataModels
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
        ReqistrationRequest,
        LoginResponse,
        AdminRegistrationRequest,
        AdminRegistrationResponse,
        Logout,

        //Requests
        AccountDeleteRequest,
        AccountDeleteResponse,
        CarBrandAddRequest,
        CarBrandAddResponse
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

    public enum UserRequestResult
    {
        AccountDeleteAlreadyRequested,
        CarPropertyAlreadyRequestedOrExists,
        Success,
        Fail
    }
}
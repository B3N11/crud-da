using System.Collections.Generic;

namespace CarCRUD.DataModels
{
    public class NetMessage
    {
        public NetMessageType type;
    }

    public class SimpleMessage : NetMessage { public bool result { get; set; } }

    public class KeyAuthenticationRequestMessage : NetMessage
    {
        public string key { get; set; }

        public KeyAuthenticationRequestMessage() => type = NetMessageType.KeyAuthenticationRequest;
    }

    public class KeyAuthenticationResponseMessage : SimpleMessage { public KeyAuthenticationResponseMessage() => type = NetMessageType.KeyAuthenticationResponse; }

    #region Login & Registration
    public class LoginRequestMessage : NetMessage
    {
        public string username { get; set; }
        public string password { get; set; }

        public LoginRequestMessage() => type = NetMessageType.LoginRequest;
    }

    public class LoginResponseMessage : NetMessage
    {
        public LoginAttemptResult result { get; set; }
        public int loginTryLeft { get; set; }
        public UserData user { get; set; }
        public GeneralResponseData userResponseData { get; set; }
        public AdminResponseData adminResponseData { get; set; }

        public LoginResponseMessage() => type = NetMessageType.LoginResponse;
    }

    public class LogoutMessage : NetMessage { public LogoutMessage() => type = NetMessageType.Logout; }

    public class GeneralResponseData
    {
        public List<CarBrand> carBrands { get; set; }
        public List<CarType> carTypes { get; set; }

        public List<FavouriteCar> favourites { get; set; }
    }

    public class AdminResponseData
    {
        public List<UserData> users { get; set; }
        public List<UserRequest> requests { get; set; }
    }

    public class RegistrationRequestMessage : NetMessage
    {
        public string username { get; set; }
        public string passwordFirst { get; set; }
        public string passwordSecond { get; set; }
        public string fullname { get; set; }

        public RegistrationRequestMessage() => type = NetMessageType.RegistrationRequest;
    }

    public class RegistrationResponseMessage : LoginResponseMessage { public RegistrationResponseMessage() => type = NetMessageType.LoginResponse; }

    public class AdminRegistrationRequestMessage : RegistrationRequestMessage
    {
        public UserType userType { get; set; }

        public AdminRegistrationRequestMessage() => type = NetMessageType.AdminRegistrationRequest;
    }

    public class AdminRegistrationResponseMessage : RegistrationResponseMessage { public AdminRegistrationResponseMessage() => type = NetMessageType.AdminRegistrationResponse; }
    #endregion

    #region Data Messaging
    /// <summary>
    /// User can send a request (account delete or new brand creation request)
    /// </summary>
    public class UserRequestMesssage : NetMessage
    {
        public UserRequestType requestType { get; set; }
        public string brandAttach { get; set; }
        public bool accountDelete { get; set; }
        public UserRequestMesssage() => type = NetMessageType.UserRequest;
    }

    public class UserRequestResponseMessage : SimpleMessage { public UserRequestResponseMessage() => type = NetMessageType.UserRequestResponse; }

    /// <summary>
    /// New brand creation (ADMIN)
    /// </summary>
    public class BrandCreateRequestMessage : NetMessage
    {
        public string name { get; set; }

        public BrandCreateRequestMessage() => type = NetMessageType.BrandCreate;
    }

    public class BrandCreateResponseMessage : NetMessage
    {
        public bool result { get; set; }
        public CarBrand carBrand { get; set; }
        public BrandCreateResponseMessage() => type = NetMessageType.BrandCreateResponse;
    }

    /// <summary>
    /// New car creation (USER)
    /// </summary>
    public class FavouriteCarCreateRequestMessage : NetMessage
    {
        public int brandID { get; set; }
        public string carType { get; set; }
        public string color { get; set; }
        public int year { get; set; }
        public string fuel { get; set; }

        public FavouriteCarCreateRequestMessage() => type = NetMessageType.FavouriteCarCreateRequest;
    }

    public class FavouriteCarCreateResponseMessage : NetMessage
    {
        public bool result { get; set; }
        public FavouriteCar favouriteCar { get; set; }

        public FavouriteCarCreateResponseMessage() => type = NetMessageType.FavouriteCarCreateResponse;
    }

    /// <summary>
    /// Answering a request of a user (ADMIN)
    /// </summary>
    public class RequestAnswerRequestMessage : NetMessage
    {
        /// <summary>
        /// If true, request is acepted. If false, request is dismissed.
        /// </summary>
        public bool accept { get; set; }
        public int requestID { get; set; }

        public RequestAnswerRequestMessage() => type = NetMessageType.RequestAnswerRequest;
    }

    public class RequestAnswerResponseMessage : NetMessage
    {
        public bool result { get; set; }
        public int requestID { get; set; }
        public RequestAnswerResponseMessage() => type = NetMessageType.RequestAnswerResponse;
    }

    public class UserActivityResetRequestMessage : NetMessage
    {
        public int userID { get; set; }
        public UserActivityResetRequestMessage() => type = NetMessageType.UserActivityResetRequest;
    }

    public class UserActivityResetResponseMessage : NetMessage
    {
        public bool result { get; set; }
        public int userID { get; set; }
        public UserActivityResetResponseMessage() => type = NetMessageType.UserActivityResetResponse;
    }
    #endregion
}
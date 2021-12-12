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
        public List<FavouriteCar> favourites { get; set; }
        public GeneralResponseData userResponseData { get; set; }
        public AdminResponseData adminResponseData { get; set; }

        public LoginResponseMessage() => type = NetMessageType.LoginResponse;
    }

    public class LogoutMessage : NetMessage { public LogoutMessage() => type = NetMessageType.Logout; }

    public class GeneralResponseData
    {
        public List<CarBrand> carBrands { get; set; }
        public List<CarType> carTypes { get; set; }        
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
    public class UserRequestMesssage : NetMessage
    {
        public UserRequestType requestType { get; set; }
        public string brandAttach { get; set; }
        public bool accountDelete { get; set; }
        public UserRequestMesssage() => type = NetMessageType.UserRequest;
    }

    public class UserRequestResponseMessage : SimpleMessage { public UserRequestResponseMessage() => type = NetMessageType.UserRequestResponse; }

    public class BrandCreateMessage : NetMessage
    {
        public string name { get; set; }

        public BrandCreateMessage() => type = NetMessageType.BrandCreate;
    }

    public class BrandCreateResponseMessage : SimpleMessage { public BrandCreateResponseMessage() => type = NetMessageType.BrandCreateResponse; }

    public class FavouriteCarCreateRequestMessage : NetMessage
    {
        public int brandID { get; set; }
        public string carType { get; set; }
        public string color { get; set; }
        public int year { get; set; }
        public string fuel { get; set; }

        public FavouriteCarCreateRequestMessage() => type = NetMessageType.FavouriteCarCreateRequest;
    }

    public class FavouriteCarCreateResponseMessage : SimpleMessage { public FavouriteCarCreateResponseMessage() => type = NetMessageType.FavouriteCarCreateResponse; }

    public class UserDeleteRequestMessage : NetMessage
    {
        public string username { get; set; }

        public UserDeleteRequestMessage() => type = NetMessageType.UserDeleteRequest;
    }

    public class UserDeleteResponseMessage : SimpleMessage { public UserDeleteResponseMessage() => type = NetMessageType.UserDeleteResponse; }
    #endregion
}
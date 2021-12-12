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
        public List<CarFavourite> favourites { get; set; }
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

    public class AdminRegistrationRequestMessage : RegistrationRequestMessage
    {
        public UserType userType { get; set; }

        public AdminRegistrationRequestMessage() => type = NetMessageType.AdminRegistrationRequest;
    }

    public class RegistrationResponseMessage : LoginResponseMessage { public RegistrationResponseMessage() => type = NetMessageType.LoginResponse; }

    public class UserRequestMesssage : NetMessage
    {
        public UserRequestType requestType { get; set; }
        public string brandAttach { get; set; }
        public bool accountDelete { get; set; }
        public UserRequestMesssage() => type = NetMessageType.UserRequest;
    }

    public class UserRequestResponse : SimpleMessage { public UserRequestResponse() => type = NetMessageType.UserRequestResponse; }
}
namespace CarCRUD.DataModels
{
    public class NetMessage
    {
        public NetMessageType type;
    }

    public class SimpleResultMessage : NetMessage { public bool result { get; set; } }

    public class KeyAuthenticationMessage : NetMessage
    {
        public string key { get; set; }

        public KeyAuthenticationMessage() => type = NetMessageType.KeyAuthentication; 
    }

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

        public LoginResponseMessage() => type = NetMessageType.LoginResponse; 
    }

    public class RegistrationRequestMessage : NetMessage
    {
        public string username { get; set; }
        public string passwordFirst { get; set; }
        public string passwordSecond { get; set; }
        public string fullname { get; set; }

        public RegistrationRequestMessage() => type = NetMessageType.AdminRegistrationRequest;
    }

    public class AdminRegistrationRequestMessage : RegistrationRequestMessage
    {
        public UserType userType { get; set; }

        public AdminRegistrationRequestMessage() => type = NetMessageType.AdminRegistrationRequest;
    }

    public class RegistrationResponseMessage : LoginResponseMessage { public RegistrationResponseMessage() => type = NetMessageType.LoginResponse; }

    public class AccountDeleteRequestMessage : NetMessage { public AccountDeleteRequestMessage() => type = NetMessageType.AccountDeleteRequest; }

    public class AccountDeleteResponseMessage : SimpleResultMessage { public AccountDeleteResponseMessage() => type = NetMessageType.AccountDeleteResponse; }

    public class CarBrandAddRequestMessage : NetMessage
    {
        public string brandName { get; set; }

        public CarBrandAddRequestMessage() => type = NetMessageType.CarBrandAddRequest;
    }

    public class CarBrandAddResponseMessage : NetMessage
    {
        public UserRequestResult result { get; set; }

        public CarBrandAddResponseMessage() => type = NetMessageType.CarBrandAddResponse;
    }
}

namespace CarCRUD.DataModels
{
    public class NetMessage
    {
        public NetMessageType type;
    }

    public class KeyAuthenticationMessage : NetMessage
    {
        public string key { get; set; }
    }

    public class LoginRequestMessage : NetMessage
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class LoginResponseMessage : NetMessage
    {
        public LoginAttemptResult result { get; set; }
        public UserType userType { get; set; }
        public int logginTryLeft { get; set; }
    }

    public class RegistrationRequestMessage : NetMessage
    {
        public string username { get; set; }
        public string passwordFirst { get; set; }
        public string passwordSecond { get; set; }
        public string fullname { get; set; }
    }

    public class AdminRegistrationRequestMessage : RegistrationRequestMessage
    {
        public UserType userType { get; set; }
    }

    public class RegistrationResponseMessage : LoginResponseMessage { }

    public enum NetMessageType
    {
        KeyAuthentication,
        LoginRequest,
        ReqistrationRequest
    }
}
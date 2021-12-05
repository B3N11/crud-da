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

    public class LoginResponse : NetMessage
    {
        public bool success { get; set; }
        public UserType userType { get; set; }
    }

    public class RegistrationRequest : NetMessage
    {
        public string username { get; set; }
        public string password { get; set; }
        public string fullname { get; set; }
    }

    public class RegistrationResponse : LoginResponse { }

    public enum NetMessageType
    {
        KeyAuthentication,
        LoginRequest,
        ReqistrationRequest
    }
}

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

    public enum NetMessageType
    {
        KeyAuthentication,
        LoginRequest,
        ReqistrationRequest
    }
}

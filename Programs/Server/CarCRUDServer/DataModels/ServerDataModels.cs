namespace CarCRUD.DataModels
{
    public class ServerSettings
    {
        public string key { get; set; }
        public int clientAuthTimeOut { get; set; }
        public bool loggingEnabled { get; set; }
        public int port { get; set; }
    }
}

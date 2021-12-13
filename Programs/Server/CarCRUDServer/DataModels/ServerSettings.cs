namespace CarCRUD.DataModels
{
    /// <summary>
    /// Settings that define the behaviour of the server. Set them here before building the project.
    /// 
    /// Key: The key used to authenticate clients. Every connecting application needs to send an authentication message with this key to access the server.
    /// 
    /// ClientAuthTimeOut: The time the server waits before dropping a connection, if the client fails to send authentication message. (Milliseconds)
    /// 
    /// LoggingEnabled: Enable server to log activities to the console window.
    /// 
    /// Port: The port the server communicates on an listens to incoming connections and messages. (Value should be between (very minimum)1024 and 65535(very maximum) for safety measures!)
    /// </summary>
    public static class ServerSettings
    {
        private static string key = "4bC_1z3";

        private static int clientAuthTimeOut = 2000;

        private static bool loggingEnabled = true;

        private static int port = 1989;


        public static string Key { get { return key; } }
        public static int ClientAuthTimeOut { get { return clientAuthTimeOut; } }
        public static bool LoggingEnabled { get { return loggingEnabled; } }
        public static int Port { get { return port; } }
    }
}
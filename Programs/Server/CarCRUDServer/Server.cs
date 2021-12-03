using CarCRUD.Networking;
using System;

namespace CarCRUD
{
    class Server
    {
        #region Properties
        //Network
        private static int port = 1989;

        //General
        private static bool accepting = false;
        private static NetClientController ncc;

        //Client Authentication
        private static string key = "4bC_1z3";
        /// <summary>
        /// Number of milliseconds after the server drops clients failing auth
        /// </summary>
        public static int clientAuthTimeOut = 2000;     // 100 <= value <= 10000

        //Custom
        public static bool loggingEnabled = true;
        #endregion

        #region General
        public static void Start(ServerSettings _settings)
        {
            SetSettings(_settings);
        }

        public static void Shutdown()
        {

        }

        public static void SetSettings(ServerSettings _settings)
        {
            key = _settings.key;
            clientAuthTimeOut = MathB.Clamp(_settings.clientAuthTimeOut, 100, 10000);
            loggingEnabled = _settings.loggingEnabled;
            port = _settings.port;

            ncc = new NetClientController(Guid.NewGuid().ToString(), port);
        }
        #endregion

        #region Connection Handle
        //Starts listening to incoming TCP connect requests
        public static void AcceptClients()
        {
            if (ncc == null || accepting) return;

            //Subscribe to every NetClient connection with the UserController's method
            ncc.AcceptClientsAsync(UserController.NetClientConnectedHandle);
            accepting = true;
        }

        //Stops listening connection
        public static void StopClientAccept()
        {
            if (ncc == null || !accepting) return;

            //Forces client acception to stop
            ncc.StopAcceptingClients();
            accepting = false;
        }
        #endregion

        #region Security
        public static bool CheckKey(string _key)
        {
            return _key == key;
        }
        #endregion
    }

    class ServerSettings
    {
        public string key;
        public int clientAuthTimeOut;
        public bool loggingEnabled;
        public int port;
    }
}
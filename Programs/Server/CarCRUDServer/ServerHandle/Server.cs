using System;
using System.IO;
using CarCRUD.Networking;
using CarCRUD.User;
using CarCRUD.DataModels;
using CarCRUD.Tools;
using CarCRUD.DataBase;

namespace CarCRUD.ServerHandle
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
        public static long maxUploadFileSize = 50000;
        #endregion

        #region General
        public static void Start(ServerSettings _settings, bool _startListening = true)
        {
            ApplySettings(_settings);
            SetApplicationParameters();

            if (_startListening) AcceptClients();
        }

        public static void Shutdown()
        {

        }


        #endregion

        #region Settings
        /// <summary>
        /// Applies the given settings to server. Be aware! Drops all connections!
        /// </summary>
        /// <param name="_settings"></param>
        public static void ApplySettings(ServerSettings _settings)
        {
            if (_settings == null) return;

            key = _settings.key;
            clientAuthTimeOut = Math.Clamp(_settings.clientAuthTimeOut, 100, 10000);
            loggingEnabled = _settings.loggingEnabled;
            port = _settings.port;

            ncc = new NetClientController(Guid.NewGuid().ToString(), port);
        }

        private static void SetApplicationParameters()
        {
            LoginValidator.SetTools(DBController.GetUserByUsernameAsync, GeneralManager.HashData);
            DBController.SetTools(new Context());
        }

        /// <summary>
        /// Reads content of a file and tries to create Settings instance from it.
        /// </summary>
        /// <param name="_path"></param>
        /// <returns></returns>
        public static ServerSettings GetSettingsFromFile(string _path)
        {
            //Check call validity
            if (_path == null) return null;

            //Read file
            string[] file = File.ReadAllLines(_path);
            if (file.Length <= 0) return null;
            string fileContent = file[0];

            //Deserialize settings json and return it
            ServerSettings settings = GeneralManager.Deserialize<ServerSettings>(fileContent);
            return settings;
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
}
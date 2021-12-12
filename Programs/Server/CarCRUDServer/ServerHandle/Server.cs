using System;
using System.Collections.Generic;
using System.IO;
using CarCRUD.Networking;
using CarCRUD.User;
using CarCRUD.DataModels;
using CarCRUD.Tools;
using CarCRUD.DataBase;
using System.Threading.Tasks;

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
        public static long maxUploadFileSize = 5000000;
        #endregion

        #region General
        public static void Start(ServerSettings _settings, bool _startListening = true)
        {
            ApplySettings(_settings);
            SetApplicationParameters();

            if (_startListening) AcceptClients();
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

        #region Admin Creation
        /// <summary>
        /// Begins server setup process if necessary. Returns false if nothing changed, true if database has been created.
        /// </summary>
        public static async Task<bool> ServerSetup()
        {
            //Checking db
            Console.Write("Checking database...");
            bool dbExisted = await DBController.Configure();
            Console.WriteLine(dbExisted ? "Created" : "Exists");

            //Check Admins
            Console.Write("Checking admins...");
            List<UserData> admins = await DBController.GetUsersByTypeAsync(UserType.Admin);
            Console.WriteLine("Success");

            int adminCount = admins == null ? 0 : admins.Count;
            Console.WriteLine($"Admins found...{adminCount}");
            if (adminCount > 0) return true;            

            Console.WriteLine("Beginning admin creation process...");
            await CreateAdmin();
            return false;
        }

        public static async Task<bool> CreateAdmin()
        {
            //Username
            string username = null;
            string password = null;
            string passwordConfirm = null;
            string fullname = null;

            while ((passwordConfirm != password) || password == null)
            {
                fullname = GeneralManager.GetInput("Your full name:");
                if (string.IsNullOrEmpty(fullname)) continue;

                username = GeneralManager.GetInput("Username:");
                if (string.IsNullOrEmpty(username)) continue;

                password = GeneralManager.GetInput("Password:");
                if (string.IsNullOrEmpty(password)) continue;

                //Check password format
                LoginAttemptResult pswMsg = LoginValidator.CheckPasswordFormat(password);
                if(pswMsg != LoginAttemptResult.Success)
                {
                    Console.WriteLine($"Error message: {pswMsg}. Password must contain: UPPER & LOWER CASE chars, numbers, special chars.");
                    continue;
                }

                passwordConfirm = GeneralManager.GetInput("Password again:", password, 2);
                
                if(passwordConfirm != password)
                    Console.WriteLine("Account creation failed...Retry");
            }

            Console.Write("Creating user in database...");
            UserData user = new UserData();
            user.active = true;
            user.type = UserType.Admin;
            user.fullname = GeneralManager.Encrypt(fullname, true);
            user.username = GeneralManager.HashData(username);
            user.password = GeneralManager.HashData(password);

            bool result = await DBController.CreateUserAsync(user);

            string resultString = result ? "Success" : "Failure";
            Console.WriteLine($"{resultString}");
            return true;
        }
        #endregion
    }
}
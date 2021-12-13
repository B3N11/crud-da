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
        //General
        private static bool accepting = false;
        private static NetClientController ncc;
        #endregion

        #region General
        public static void Start(bool _startListening = true)
        {
            SetApplicationParameters();

            ncc = new NetClientController(Guid.NewGuid().ToString(), ServerSettings.Port);
            if (_startListening) AcceptClients();
        }
        #endregion

        #region Settings
        private static void SetApplicationParameters()
        {
            LoginValidator.SetTools(DBController.GetUserAsync, GeneralManager.HashData);
            DBController.SetTools(new Context());
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
            return _key == ServerSettings.Key;
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
            List<UserData> admins = await DBController.GetUsersAsync(UserType.Admin);
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
using CarCRUD.Users;
using CarCRUD.ViewModels;
using System.Collections.Generic;

namespace CarCRUD
{
    class Client
    {
        //Network
        public static string ip = "127.0.0.1";
        public static string key = "4bC_1z3";

        //Authentication
        public static int port = 1989;

        //ViewModels
        public static IConnectionHandler mainVM;
        public static List<IConnectionHandler> connectionHandler = new List<IConnectionHandler>();

        public static async void Start(IConnectionHandler _main)
        {
            if (_main == null) return;

            if (mainVM == null)
                mainVM = AssignViewModels(_main);
            CreateUser(true);
            ConnectToServer();
        }

        #region ViewModels
        public static IConnectionHandler AssignViewModels(IConnectionHandler _handlerToAdd = null)
        {
            if (_handlerToAdd == null) return null;
            
            //Login VM Setup
            connectionHandler.Add(_handlerToAdd);
            UserController.OnClientConnectionResultedEvent += _handlerToAdd.ClientConnectionResulted;
            UserController.OnClientDisconnectedEvent += _handlerToAdd.ClientDisconnected;
            UserController.OnClientConnectingEvent += _handlerToAdd.ClientConnecting;

            return _handlerToAdd;
        }
        #endregion

        #region Connection
        public static void CreateUser(bool force)
        {
            UserController.CreateUser(force);
        }

        public static void ConnectToServer()
        {
            UserController.Connect();
        }

        public static void LogOut()
        {

        }
        #endregion
    }
}

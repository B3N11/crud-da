using System;
using CarCRUD.Users;

namespace CarCRUD
{
    class Client
    {
        //Network
        public static string ip = "127.0.0.1";
        public static string key = "4bC_1z3";

        //Authentication
        public static int port = 1989;

        public static void Start()
        {
            CreateUser();
            ConnectToServer();
        }

        public static void CreateUser()
        {
            UserController.CreateUser();
        }

        public static void ConnectToServer()
        {
            UserController.Connect();
        }

        public static void LogOut()
        {

        }
    }
}

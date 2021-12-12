using System;
using System.Net;
using CarCRUD.Users;

namespace CarCRUD.Core
{
    class Client
    {
        public static ClientData Data { get; private set; }

        public static async void Start()
        {
            if (Data == null)
                throw new Exception("No data has been specified for Client.");

            CreateUser(true);
            ConnectToServer();
        }

        #region Connection
        
        public static void CreateUser(bool _force)
        {
            UserController.CreateUser(Data.port, _force);
        }

        public static void ConnectToServer()
        {
            UserController.Connect(Data.ip);
        }
        #endregion

        #region Settings
        public static void CreateSettings()
        {
            Console.WriteLine("Specify a");
        }

        public static bool ApplySettings(ClientData _data)
        {
            if (_data == null) return false;

            if (CheckSettings(_data) != ClientSettingsResult.Success)
                return false;

            Data = _data;
            return true;
        }

        private static ClientSettingsResult CheckSettings(ClientData _data)
        {
            if (_data == null) return ClientSettingsResult.Fail;

            try { IPAddress.Parse(_data.ip); }
            catch { return ClientSettingsResult.InvalidIPAddress; }

            if (_data.port < 1024 || _data.port > 65535)
                return ClientSettingsResult.InvalidPortNumber;

            return ClientSettingsResult.Success;
        }
        #endregion
    }

    public class ClientData
    {
        public string ip { get; set; }
        public string key { get; set; }
        public int port { get; set; }
    }

    enum ClientSettingsResult
    {
        InvalidIPAddress,
        InvalidPortNumber,
        Success,
        Fail
    }
}
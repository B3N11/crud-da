using System;
using CarCRUD.Networking;
using CarCRUD.DataModels;
using CarCRUD.Tools;
using System.Text;
using System.Collections.Generic;

namespace CarCRUD.Users
{
    class UserController
    {
        public static User user;

        public delegate void ClientDisconnected();
        public delegate void ClientConnectionResulted(bool _result);
        public delegate void ClientConnecting();
        public delegate void LoginResponse(LoginAttemptResult _result, int _loginLeft);

        public static ClientDisconnected OnClientDisconnectedEvent;
        public static ClientConnectionResulted OnClientConnectionResultedEvent;
        public static ClientConnecting OnClientConnectingEvent;
        public static LoginResponse OnLoginResultedEvent;

        #region Client Handle
        //Connects to server
        public static void Connect()
        {
            if (!CheckClientConnection()) return;

            user.netClient.ConnectAsync(Client.ip);

            OnClientConnectingEvent?.Invoke();
        }

        /// <summary>
        /// Handles an established NetClient connection as new user.
        /// </summary>
        /// <param name="_object"></param>
        public static void NetClientConnectedHandle(object _object, ConnectionResultEventArgs crea)
        {
            //Create a NetClient from _object (which suppose to be NetClient instance)
            NetClient client = GeneralManager.CastNetClient(_object);
            if (client == null) return;

            //Failed connection attempt
            if (crea.result == Result.Fail)
            {
                user.status = UserStatus.Disconnected;
                OnClientConnectionResultedEvent?.Invoke(false);
            }

            //Successful connection
            else
            {
                SendAuthentication();
                user.status = UserStatus.PendingAuthentication;
                OnClientConnectionResultedEvent?.Invoke(true);
            }
        }

        public static void NetClientDisconnectedHandle(object _object)
        {
            //Create a NetClient from _object (which suppose to be NetClient instance)
            NetClient client = GeneralManager.CastNetClient(_object);
            if (client == null) return;

            //Set status and raise event
            user.status = UserStatus.Disconnected;
            OnClientDisconnectedEvent?.Invoke();

            Console.WriteLine("Disconnected");
        }

        /// <summary>
        /// Starts the client authentication process and handles the result. If the specified user's client does not respond with the server's unique key in time, the server closes the connection.
        /// </summary>
        /// <param name="_user"></param>
        public static void SendAuthentication()
        {
            if (!CheckClientConnection()) return;

            //Create Message
            KeyAuthenticationRequestMessage message = new KeyAuthenticationRequestMessage();
            message.key = Client.key;

            //Send
            Send(message, false);
        }
        #endregion

        #region Messaging
        public static void MessageReceivedHandle(object _sender, string _message)
        {
            //Check call validity
            if (_sender == null || string.IsNullOrEmpty(_message)) return;

            //Check user validity
            User _user = null;
            try { _user = _sender as User; } catch { return; }

            //Decrypt message from received data
            string decryptedMessage = GeneralManager.Encrypt(_message, false);

            //Get Message object and its type
            NetMessage message = GeneralManager.GetMessage(decryptedMessage);

            //Let message be handled based on its type
            HandleMessage(message);
        }

        public static void Send<T>(T _object, bool _waitforResponse = true)
        {
            //Check connection
            if (_object == null || !CheckClientConnection()) return;

            //Encrypt Data
            string message = GeneralManager.Serialize(_object);
            message = GeneralManager.Encrypt(message, true);

            //Send
            byte[] data = Encoding.UTF8.GetBytes(message);
            user.Send(data);
            //If needed, dont let anything happen until the response arrived
            user.canRequest = !_waitforResponse;
        }
        #endregion

        #region User Handling
        /// <summary>
        /// Creates a new user instance. Forcing the process will lose connection if there is any.
        /// </summary>
        /// <param name="_force"></param>
        public static void CreateUser(bool _force = false)
        {
            if (CheckClientConnection() && !_force) return;

            user = new User(Guid.NewGuid().ToString());
            user.netClient = new NetClient(Guid.NewGuid().ToString(), Client.port);
            user.netClient.OnMessageReceivedEvent += user.MessageReceived;
            user.OnMessageReceivedEvent += MessageReceivedHandle;
            user.netClient.OnConnectionResultedEvent += NetClientConnectedHandle;
            user.netClient.OnClientDisconnectedEvent += NetClientDisconnectedHandle;
            user.canRequest = true;
        }

        public static void SetUserData(UserData _data, GeneralResponseData _responseData, List<CarFavourite> _favourites, AdminResponseData _adminResponseData = null)
        {
            user.userData = _data;
            user.userResponseData = _responseData;
            user.adminResponseData = _adminResponseData;
            user.favourites = _favourites;
        }

        public static void Logout(bool breakConnection = false)
        {
            if (!CheckClientConnection()) return;

            SetUserData(null, null, null, null);
            UserActionHandler.RequestLogout();

            //Recreates user if disconnectint from server was requested
            if(breakConnection)
                CreateUser(breakConnection);
        }
        #endregion

        #region Action Handling
        /// <summary>
        /// Handles a NetMessage instance based on their type. The method assumes the _message has been cast.
        /// </summary>
        /// <param name="_message"></param>
        private static void HandleMessage(NetMessage _message)
        {
            //Check call validity
            if (_message == null || !CheckClientConnection()) return;            

            switch (_message.type)
            {
                case NetMessageType.LoginResponse:       //Login Reques Message
                    ResponseHandler.LoginResponseHandle(_message as LoginResponseMessage); break;
            }

            //Enable new request
            user.canRequest = true;
        }
        #endregion

        #region Others
        /// <summary>
        /// Check the user and client instance. If they are not null, true will be returned
        /// </summary>
        /// <param name="connected"></param>
        /// <returns></returns>
        public static bool CheckClientConnection()
        {
            bool result = user == null ? false : (user.netClient == null) ? false : true;

            return result; 
        }
        #endregion
    }
}
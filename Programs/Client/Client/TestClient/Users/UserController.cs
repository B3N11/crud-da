using System;
using CarCRUD.Networking;
using CarCRUD.DataModels;
using CarCRUD.Tools;
using System.Text;

namespace CarCRUD.Users
{
    class UserController
    {
        private static User user;

        public delegate void ClientDisconnected();
        public delegate void ClientConnected();

        public static ClientDisconnected OnClientDisconnectedEvent;
        public static ClientConnected OnClientConnectedEvent;

        #region Client Handle
        //Connects to server
        public static void Connect()
        {
            if (CheckClientConnection()) return;

            user.netClient.ConnectAsync(Client.ip);
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
                user.status = UserStatus.Disconnected;

            //Successful connection
            else
            {
                SendAuthentication();
                user.status = UserStatus.PendingAuthentication;
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
            KeyAuthenticationMessage message = new KeyAuthenticationMessage();
            message.key = Client.key;

            //Send
            Send(message);
        }
        #endregion

        #region Messaging
        public static void MessageReceivedHandle(object _sender, string _message)
        {
            //Check call validity
            if (_sender == null || string.IsNullOrEmpty(_message)) return;

            //Check user validity
            User user = null;
            try { user = _sender as User; } catch { return; }

            //Decrypt message from received data
            string decryptedMessage = GeneralManager.Encrypt(_message, false);

            //Get Message object and its type
            NetMessage message = GeneralManager.GetMessage(decryptedMessage);

            //Let message be handled based on its type
            HandleMessage(message, user);
        }

        public static void Send<T>(T _object)
        {
            //Check connection
            if (_object == null || user == null) return;

            //Encrypt Data
            string message = GeneralManager.Serialize(_object);
            message = GeneralManager.Encrypt(message, true);

            //Send
            byte[] data = Encoding.UTF8.GetBytes(message);
            user.Send(data);
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
        }

        public static void SetUserData(UserData _data)
        {
            if (_data == null) return;

            user.userData = _data;
        }
        #endregion

        #region Action Handling
        /// <summary>
        /// Handles a NetMessage instance based on their type. The method assumes the _message has been cast.
        /// </summary>
        /// <param name="_message"></param>
        private static void HandleMessage(NetMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;            

            switch (_message.type)
            {
                case NetMessageType.LoginResponse:       //Login Reques Message
                    ResponseHandler.LoginResponseHandle(_message as LoginResponseMessage); break;
            }
        }
        #endregion

        #region Others
        /// <summary>
        /// Check the user and client instance. If they are not null, then client.connected will be returned
        /// </summary>
        /// <param name="connected"></param>
        /// <returns></returns>
        private static bool CheckClientConnection()
        {
            bool result = user == null ? false : (user.netClient == null) ? false : user.netClient.connected;

            return result; 
        }
        #endregion
    }
}
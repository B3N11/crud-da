using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarCRUD.Networking;
using System.Text;

namespace CarCRUD
{
    class UserController
    {
        private static User user;

        #region Client Handle
        /// <summary>
        /// Handles an established NetClient connection as new user.
        /// </summary>
        /// <param name="_object"></param>
        public static void NetClientConnectedHandle(object _object, ConnectionResultEventArgs crea)
        {
            //Create a NetClient from _object (which suppose to be NetClient instance)
            NetClient client = GeneralManager.CastNetClient(_object);
            if (client == null) return;

            Console.WriteLine("Connected");
            SendAuthentication();
        }

        public static void NetClientDisconnectedHandle(object _object)
        {
            //Create a NetClient from _object (which suppose to be NetClient instance)
            NetClient client = GeneralManager.CastNetClient(_object);
            if (client == null) return;

            Console.WriteLine("Disconnected");
        }

        #region Authentication
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
            user.Send(message);
        }
        #endregion

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
            user.netClient.OnConnectionResultedEvent += NetClientConnectedHandle;
            user.netClient.OnClientDisconnected += NetClientDisconnectedHandle;
        }

        public static void Connect()
        {
            if (CheckClientConnection()) return;

            user.netClient.ConnectAsync(Client.ip);
        }
        #endregion

        #region Action Handling
        /// <summary>
        /// Handles a NetMessage instance based on their type. The method assumes the _message has been cast.
        /// </summary>
        /// <param name="_message"></param>
        public static void HandleMessage(NetMessage _message, string _userID)
        {
            switch (_message.type)
            {
                
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
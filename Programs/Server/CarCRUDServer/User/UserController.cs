using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarCRUD.DataModels;
using CarCRUD.Networking;
using CarCRUD.Tools;
using CarCRUD.ServerHandle;

namespace CarCRUD.User
{
    class UserController
    {
        public static List<User> users = new List<User>();

        #region Client Handle
        /// <summary>
        /// Handles an established NetClient connection as new user.
        /// </summary>
        /// <param name="_object"></param>
        public static void NetClientConnectedHandle(object _object)
        {
            //Create a NetClient from _object (which suppose to be NetClient instance)
            NetClient client = GeneralManager.CastNetClient(_object);
            if (client == null) return;

            //Create user instance for the newly connected client
            User newUser = new User(Guid.NewGuid().ToString());
            newUser.netClient = client;
            newUser.netClient.OnClientDisconnected += NetClientDisconnectedHandle;
            newUser.netClient.OnMessageReceivedEvent += newUser.MessageReceived;
            newUser.OnMessageReceivedEvent += MessageReceivedHandle;
            newUser.status = UserStatus.Connected;

            users.Add(newUser);

            if(Server.loggingEnabled) Logger.LogConnectionState(newUser);

            //Start the authentication of the client
            ClientAuthenticationAsync(newUser);
        }

        public static void NetClientDisconnectedHandle(object _object)
        {
            //Create a NetClient from _object (which suppose to be NetClient instance)
            NetClient client = GeneralManager.CastNetClient(_object);
            if (client == null) return;

            User user = null;
            try { user = users.First(u => u.netClient.id == client.id); } catch { }
            if (user == null) return;

            //Drop user
            DropUser(user);
            user.status = UserStatus.Disconnected;
            if (Server.loggingEnabled) Logger.LogConnectionState(user);
        }

        #region Authentication

        /// <summary>
        /// Starts the client authentication process and handles the result. If the specified user's client does not respond with the server's unique key in time, the server closes the connection.
        /// </summary>
        /// <param name="_user"></param>
        public static async void ClientAuthenticationAsync(User _user)
        {
            //Update user status
            _user.status = UserStatus.PendingAuthentication;
            if (Server.loggingEnabled) Logger.LogState(_user);

            //Create CTS for the tasks and start coundown
            CancellationTokenSource cts = new CancellationTokenSource();
            GeneralManager.CountdownAsync(Server.clientAuthTimeOut, 10, cts);

            //Start auth checking
            bool result = await Task.Run(() => CheckClientAuthentication(cts.Token, _user));

            //Safe-cancel token for remining background processes
            if (cts.Token.CanBeCanceled)
                cts.Cancel();

            //Authentication has been made, return
            if (result) return;

            //Drop user uppon auth fail
            DropUser(_user);

            if (Server.loggingEnabled) Logger.LogState(_user);
        }

        private static async Task<bool> CheckClientAuthentication(CancellationToken token, User _user)
        {
            //Waits until client is authenticated OR time limit expires
            while (!token.IsCancellationRequested)
            {
                if (_user.status == UserStatus.Authenticated) return true;
                if (_user.status == UserStatus.Dropped) return false;
            }

            return false;
        }
        #endregion

        #endregion

        #region User Handling
        //Releases a User instance and disconnects its client.
        public static void DropUser(User _user)
        {
            _user.status = UserStatus.Dropped;
            try { _user.netClient.StopClient(); } catch { }     //Stop client (disconnect)
            try { users.Remove(_user); } catch { }      //Release instance for GC
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

            //Resub for event
            user.OnMessageReceivedEvent += MessageReceivedHandle;

            //Resub for event
            if(user.netClient != null)
                user.netClient.OnMessageReceivedEvent += user.MessageReceived;

            //Decrypt message from received data
            string decryptedMessage = GeneralManager.Decrypt(_message);

            //Get Message object and its type
            NetMessage message = GeneralManager.Deserialize<NetMessage>(decryptedMessage);
            message = GeneralManager.GetMessage(decryptedMessage);

            //Let message be handled based on its type
            HandleMessage(message, user.userID);
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
                case NetMessageType.KeyAuthentication:
                    UserActionHandler.CheckArrivedKeyAuth(_message as KeyAuthenticationMessage, _userID); break;
            }
        }
        #endregion
    }
}
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarCRUD.DataModels;
using CarCRUD.Networking;
using CarCRUD.Tools;
using CarCRUD.ServerHandle;
using System.Text;

namespace CarCRUD.User
{
    /// <summary>
    /// UserController is a link between network communication and server internal logic.
    /// 
    /// Main tasks: Handle client connections (connect, drop, authenticate)
    ///             Handle communication (send, receive)
    ///             Handle messages/requests from remote connections
    /// </summary>
    class UserController
    {
        /// <summary>
        /// List of online clients (not just logged in users).
        /// </summary>
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
            newUser.netClient.OnClientDisconnectedEvent += NetClientDisconnectedHandle;
            newUser.netClient.OnMessageReceivedEvent += newUser.MessageReceived;
            newUser.OnMessageReceivedEvent += MessageReceivedHandle;
            newUser.status = UserStatus.Connected;

            users.Add(newUser);

            if(ServerSettings.LoggingEnabled) Logger.LogConnectionState(newUser);

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
            if (ServerSettings.LoggingEnabled) Logger.LogConnectionState(user);
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
            if (ServerSettings.LoggingEnabled) Logger.LogState(_user);

            //Create CTS for the tasks and start coundown
            CancellationTokenSource cts = new CancellationTokenSource();
            GeneralManager.CountdownAsync(ServerSettings.ClientAuthTimeOut, 10, cts);

            //Start auth checking
            bool result = await Task.Run(() => CheckClientAuthentication(cts.Token, _user));

            //Safe-cancel token for remining background processes
            if (cts.Token.CanBeCanceled)
                cts.Cancel();

            //Authentication has been made, return
            if (result) return;

            //Drop user uppon auth fail
            DropUser(_user);

            if (ServerSettings.LoggingEnabled) Logger.LogState(_user);
        }

        private static async Task<bool> CheckClientAuthentication(CancellationToken token, User _user)
        {
            //Waits until client is authenticated OR time limit expires
            while (!token.IsCancellationRequested)
            {
                if (_user.status == UserStatus.Authenticated) return true;
                if (_user.status == UserStatus.Dropped) return false;

                await Task.Delay(10);
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

        /// <summary>
        /// Checks if user is logged in.
        /// </summary>
        /// <param name="_username"></param>
        /// <returns></returns>
        public static bool IsUserLoggedIn(string _username)
        {
            bool result = false;

            try { result = users.Exists(u => u.userData?.username == _username); }
            catch { }

            return result;
        }
        #endregion

        #region Messaging
        /// <summary>
        /// Handles an arrived message.
        /// </summary>
        /// <param name="_sender"></param>
        /// <param name="_message"></param>
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

            if (ServerSettings.LoggingEnabled) Logger.LogMessage(user, message);

            //Let message be handled based on its type
            HandleMessage(message, user);
        }

        public static void Send<T>(T _object, User _user)
        {
            //Check connection
            if (_object == null || _user == null) return;

            //Encrypt Data
            string message = GeneralManager.Serialize(_object);
            message = GeneralManager.Encrypt(message, true);

            if (ServerSettings.LoggingEnabled) Logger.LogResponse(_user, _object as NetMessage);

            //Send
            byte[] data = Encoding.UTF8.GetBytes(message);
            _user.Send(data);            
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
            //Check if message can be accepted from client
            if (_message.type != NetMessageType.KeyAuthenticationRequest && _user.status == UserStatus.PendingAuthentication) return;

            switch (_message.type)
            {
                case NetMessageType.KeyAuthenticationRequest:      //Key Auth Message
                    UserActionHandler.CheckAuthenticationKey(_message as KeyAuthenticationRequestMessage, _user); break;

                case NetMessageType.LoginRequest:       //Login Request Message
                    UserActionHandler.LoginRequestHandleAsync(_message as LoginRequestMessage, _user); break;

                case NetMessageType.RegistrationRequest:        //Registration request
                    UserActionHandler.RegistrationHandle(_message as RegistrationRequestMessage, _user); break;

                case NetMessageType.Logout:     //Logout request
                    UserActionHandler.LogoutHandle(_user); break;

                case NetMessageType.UserRequest:    //Some kind of request from user (Account delete/new car brand)
                    UserActionHandler.UserRequestHandleAsync(_message as UserRequestMesssage, _user); break;

                case NetMessageType.AdminRegistrationRequest:       //Admin wants to create a new user
                    UserActionHandler.AdminRegistrationHandle(_message as AdminRegistrationRequestMessage, _user); break;

                case NetMessageType.BrandCreate:        //Admin wants to create a new car brand
                    UserActionHandler.BrandCreateHandleAsync(_message as BrandCreateRequestMessage, _user); break;

                case NetMessageType.FavouriteCarCreateRequest:      //User wants to create new Favourite Car
                    UserActionHandler.FavouriteCarCreateHandleAsync(_message as FavouriteCarCreateRequestMessage, _user); break;

                case NetMessageType.RequestAnswerRequest:
                    UserActionHandler.RequestAnswerRequestHandleAsync(_message as RequestAnswerRequestMessage, _user); break;

                case NetMessageType.UserActivityResetRequest:
                    UserActionHandler.UserActivityResetHandleAsync(_message as UserActivityResetRequestMessage, _user); break;

                case NetMessageType.CarDeleteRequest:
                    UserActionHandler.CarDeleteRequestHandleAsync(_message as CarDeleteRequestMessage, _user); break;
            }
        }
        #endregion
    }
}
using System.Text;
using CarCRUD.Networking;

namespace CarCRUD
{
    //Represents a user
    class User
    {
        #region Properties
        //General User
        public string userID;
        public UserStatus status;
        public UserData userData;

        //Networking
        public NetClient netClient;

        //Unique ID required to create a user
        public User(string _id) { userID = _id; }
        #endregion

        #region General
        #endregion

        #region Communication
        /// <summary>
        /// Callback for handling message from NetClient
        /// </summary>
        /// <param name="_object"></param>
        /// <param name="data"></param>
        public void MessageReceived(object _object, ref byte[] data)
        {
            //Check client validity
            NetClient client = GeneralManager.CastNetClient(_object);
            if (client == null || data == null || data.Length == 0) return;

            //Resub for event
            netClient.OnMessageReceivedEvent += MessageReceived;

            //Decrypt message from received data
            string messageString = Encoding.UTF8.GetString(data);
            messageString = GeneralManager.Decrypt(messageString);

            //Get Message object and its type
            NetMessage message = GeneralManager.Deserialize<NetMessage>(messageString);
            message = NetMessage.GetMessage(messageString);

            //Let message be handled based on its type
            UserController.HandleMessage(message, userID);
        }

        public void Send<T>(T _object)
        {
            //Check connection
            if (_object == null || netClient == null || !netClient.connected) return;

            //Encrypt Data
            string message = GeneralManager.Serialize(_object);
            message = GeneralManager.Encrypt(message);

            //Send
            byte[] data = Encoding.UTF8.GetBytes(message);
            netClient.SendAsync(data);
        }
        #endregion

        #region Logging
        public string GetID()
        {
            return userID;
        }

        public string GetState()
        {
            return status.ToString();
        }

        public string GetEndPoint()
        {
            return netClient != null ? netClient.endPoint.ToString() : null;
        }
        #endregion
    }

    class UserData
    {
        public string username { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string type { get; set; }
        public bool active { get; set; }
        public UserRequest request {get;set;}
    }

    class UserRequest
    {
        public bool accountRemove { get; set; }
        public bool brandAttach { get; set; }
    }

    enum UserStatus
    {
        PendingAuthentication,
        Authenticated,
        LoggedIn,
        Connected,
        Disconnected,
        Dropped
    }
}
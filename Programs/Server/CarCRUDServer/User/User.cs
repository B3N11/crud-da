using System.Text;
using CarCRUD.Networking;
using CarCRUD.DataModels;
using CarCRUD.Tools;

namespace CarCRUD.User
{
    //Represents a user
    public class User : IEndpointLoggable
    {
        #region Properties
        //General User
        public string userID = string.Empty;
        public UserStatus status;
        public UserData userData;

        //Networking
        public NetClient netClient;

        //Login
        public string lastUsername = string.Empty;
        public int loginAttempts = 0;

        //Unique ID required to create a user
        public User(string _id) { userID = _id; }

        //Events & delegates
        public delegate void MessageReceivedData(object _sender, string _message);
        public event MessageReceivedData OnMessageReceivedEvent;
        #endregion

        #region Communication
        /// <summary>
        /// Callback for handling message from NetClient
        /// </summary>
        /// <param name="_object"></param>
        /// <param name="data"></param>
        public void MessageReceived(object _object, ref byte[] data)
        {
            //Check call validity
            if (_object == null || data == null || data.Length == 0) return;

            //Resub for event
            if(netClient != null)
                netClient.OnMessageReceivedEvent += MessageReceived;

            //Decrypt message from received data
            string message = Encoding.UTF8.GetString(data);

            if (OnMessageReceivedEvent != null)
                OnMessageReceivedEvent(this, message);
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
}
using System.Text;
using CarCRUD.Networking;
using CarCRUD.DataModels;
using CarCRUD.Tools;

namespace CarCRUD.User
{
    /// <summary>
    /// Represents a connection with a potential user.
    /// </summary>
    public class User : IEndpointLoggable
    {
        #region Properties
        //General User
        public string userID = string.Empty;
        public UserStatus status;
        public UserData userData;

        //Networking
        public NetClient netClient;

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

            //Decrypt message from received data
            string message = Encoding.UTF8.GetString(data);

            if (OnMessageReceivedEvent != null)
                OnMessageReceivedEvent(this, message);
        }

        public void Send(byte[] _data)
        {
            //Check connection
            if (_data == null || _data.Length == 0 || netClient == null || !netClient.connected) return;

            netClient.SendAsync(_data);
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
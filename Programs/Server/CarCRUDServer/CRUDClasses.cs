using System;
using System.Collections.Generic;
using CarCRUD.Networking;

namespace CarCRUD
{
    //Represents a user
    class User : Loggable
    {
        #region Properties
        //General User
        public UserStatus status;
        public UserData userData;

        //Networking
        public NetClient client;
        #endregion

        #region Logging
        public string GetID()
        {
            return userData != null ? userData.id : null;
        }

        public string GetState()
        {
            return status.ToString();
        }
        #endregion
    }

    class UserData
    {
        public string id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
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
        PendingAuth,
        LoggedIn
    }
}
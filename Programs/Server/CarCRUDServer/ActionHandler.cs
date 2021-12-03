using System;
using CarCRUD.Networking;

namespace CarCRUD
{
    /// <summary>
    /// Handles the actions of user requests.
    /// </summary>
    class ActionHandler
    {
        /// <summary>
        /// Handles a NetMessage instance based on their type. The method assumes the _message has been cast.
        /// </summary>
        /// <param name="_message"></param>
        public static void HandleMessage(NetMessage _message, string _userID)
        {
            switch (_message.type)
            {
                case NetMessageType.KeyAuthentication:
                    UserController.CheckArrivedKeyAuth(_message as KeyAuthenticationMessage, _userID); break;
            }
        }
    }
}

using System.Linq;
using CarCRUD.DataModels;
using CarCRUD.ServerHandle;
using CarCRUD.Tools;

namespace CarCRUD.User
{
    public class UserActionHandler
    {
        public static void CheckArrivedKeyAuth(KeyAuthenticationMessage _message, string _userID)
        {
            if (_message == null) return;

            //Check key match
            bool result = Server.CheckKey(_message.key);
            User user = UserController.users.First(u => u.userID == _userID);

            //Authentication was successfull
            if (result) user.status = UserStatus.Authenticated;

            //Authentication failed
            else UserController.DropUser(user);

            //Log state if enabled
            if (Server.loggingEnabled) Logger.LogState(user);
        }
    }
}

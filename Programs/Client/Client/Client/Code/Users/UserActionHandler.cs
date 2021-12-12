using CarCRUD.DataModels;

namespace CarCRUD.Users
{
    public class UserActionHandler
    {
        #region Connection & Login
        public static bool RequestLogin(string _username, string _password)
        {
            //Check call validity
            if (!UserController.CheckClientConnection()) return false;
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password)) return false;

            LoginRequestMessage message = new LoginRequestMessage();
            message.type = NetMessageType.LoginRequest;
            message.username = _username;
            message.password = _password;

            UserController.Send(message);
            return true;
        }

        /// <summary>
        /// Sends registration request to server.
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="_passwordFirst"></param>
        /// <param name="_passwordSecond"></param>
        /// <param name="_fullname"></param>
        /// <returns></returns>
        public static bool RequestRegistration(string _username, string _passwordFirst, string _passwordSecond, string _fullname)
        {
            if (!UserController.CheckClientConnection()) return false;
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_passwordFirst) || string.IsNullOrEmpty(_passwordSecond) || string.IsNullOrEmpty(_fullname))
                return false;

            RegistrationRequestMessage message = new RegistrationRequestMessage();
            message.type = NetMessageType.RegistrationRequest;
            message.username = _username;
            message.passwordFirst = _passwordFirst;
            message.passwordSecond = _passwordSecond;
            message.fullname = _fullname;

            UserController.Send(message);
            return true;
        }

        public static bool RequestAdminRegistration(string _username, string _passwordFirst, string _passwordSecond, string _fullname)
        {
            if (!UserController.CheckClientConnection()) return false;
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_passwordFirst) || string.IsNullOrEmpty(_passwordSecond) || string.IsNullOrEmpty(_fullname))
                return false;

            AdminRegistrationRequestMessage message = new AdminRegistrationRequestMessage();
            message.username = _username;
            message.passwordFirst = _passwordFirst;
            message.passwordSecond = _passwordSecond;
            message.fullname = _fullname;
            message.userType = UserType.Admin;

            UserController.Send(message);
            return true;
        }

        /// <summary>
        /// Sends logout indicator message
        /// </summary>
        public static void RequestLogout()
        {
            if (!UserController.CheckClientConnection()) return;
            LogoutMessage message = new LogoutMessage();

            UserController.Send(message, false);
        }
        #endregion

        #region Requests
        public static void AccountDeleteRequest()
        {
            if (!UserController.CheckClientConnection()) return;

            UserRequestMesssage request = new UserRequestMesssage();
            request.requestType = UserRequestType.AccountDelete;

            //Send
            UserController.Send(request);
        }
        #endregion
    }
}

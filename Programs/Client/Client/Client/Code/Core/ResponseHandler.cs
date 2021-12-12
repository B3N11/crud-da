using CarCRUD.DataModels;
using CarCRUD.Users;

namespace CarCRUD
{
    class ResponseHandler
    {
        /// <summary>
        /// Handles incoming login response. Additional callback can be specified.
        /// </summary>
        /// <param name="_message"></param>
        /// <param name="_additionalLoginHandle"></param>
        public static void LoginResponseHandle(LoginResponseMessage _message)
        {
            if (_message == null) return;            

            //Set User data
            if (_message.result == LoginAttemptResult.Success && _message.type != NetMessageType.AdminRegistrationResponse)
                UserController.SetUserData(_message.user, _message.userResponseData, _message.favourites, _message.adminResponseData);

            if (_message.result == LoginAttemptResult.Success && _message.type == NetMessageType.AdminRegistrationResponse)
                UserController.user.adminResponseData.users.Add(_message.user);

            //Raise event
            UserController.OnLoginResultedEvent?.Invoke(_message.result, _message.loginTryLeft);
        }
    }
}
using System;
using CarCRUD.DataModels;

namespace CarCRUD
{
    class ResponseHandler
    {
        public delegate void HandleLoginResult(LoginResponseMessage _message);

        /// <summary>
        /// Handles incoming login response. Additional callback can be specified.
        /// </summary>
        /// <param name="_message"></param>
        /// <param name="_additionalLoginHandle"></param>
        public static void LoginResponseHandle(LoginResponseMessage _message, HandleLoginResult _additionalLoginHandle = null)
        {
            if (_message == null) return;

            Console.WriteLine($"Login response: {_message.result} with login attempts left: {_message.loginTryLeft}. Other data: {_message.user?.fullname}");

            _additionalLoginHandle?.Invoke(_message);
        }
    }
}

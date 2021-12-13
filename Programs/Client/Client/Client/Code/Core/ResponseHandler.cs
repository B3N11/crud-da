using CarCRUD.DataModels;
using CarCRUD.Users;
using System;
using System.Linq;
using System.Windows;

namespace CarCRUD
{
    class ResponseHandler
    {
        #region Login
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
                UserController.SetUserData(_message.user, _message.userResponseData, _message.adminResponseData);

            if (_message.result == LoginAttemptResult.Success && _message.type == NetMessageType.AdminRegistrationResponse)
                UserController.user.adminResponseData.users.Add(_message.user);

            //Raise event
            UserController.OnLoginResultedEvent?.Invoke(_message.result, _message.loginTryLeft);
        }
        #endregion

        #region General
        public static void FavouriteCarCreationResponseHandle(FavouriteCarCreateResponseMessage _message)
        {
            if (_message == null) return;

            if (_message.result)
                UserController.user.generalResponseData.favourites.Add(_message.favouriteCar);

            string result = _message.result ? "Car added successfully!" : "Car creation failed!";
            MessageBox.Show(result);
        }

        public static void UserRequestResponseHandle(UserRequestResponseMessage _message)
        {
            if (_message == null) return;

            string result = _message.result ? "Request uccessfully!" : "Request failed!";
            MessageBox.Show(result);
        }
        #endregion

        #region Admin
        /// <summary>
        /// Handles response for a CarBrand creation
        /// </summary>
        /// <param name="_message"></param>
        public static void BrandCreationResponseHandle(BrandCreateResponseMessage _message)
        {
            if (_message == null) return;

            //If success, add new CarBrand to list
            if (_message.result && _message.carBrand != null)
                UserController.user.generalResponseData.carBrands.Add(_message.carBrand);

            string result = _message.result ? "Car brand added successfully!" : "Car brand creation failed!";
            MessageBox.Show(result);
        }

        public static void RequestAnswerResponseHandle(RequestAnswerResponseMessage _message)
        {
            if (_message == null) return;

            if(_message.result)
                try { UserController.user.adminResponseData.requests.RemoveAll(r => r.ID == _message.requestID); }
                catch { }

            string result = _message.result ? "Operation successful!" : "Operation failed!";
            MessageBox.Show(result);
        }

        public static void UserActivityResetResponseHandle(UserActivityResetResponseMessage _message)
        {
            if (_message == null)
                return;

            if(_message.result)
                try { UserController.user.adminResponseData.users.First(u => u.ID == _message.userID).active = true; }
                catch { }

            string result = _message.result ? "Activity reset successful!" : "Activity reset failed!";
            MessageBox.Show(result);
        }
        #endregion
    }
}
using System;
using System.Threading.Tasks;
using CarCRUD.DataBase;
using CarCRUD.DataModels;
using CarCRUD.ServerHandle;
using CarCRUD.Tools;

namespace CarCRUD.User
{
    public class UserActionHandler
    {
        #region Connection And Login
        public static void CheckAuthenticationKey(KeyAuthenticationMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;

            //Check key match
            bool result = Server.CheckKey(_message.key);

            //Authentication was successfull
            if (result) _user.status = UserStatus.Authenticated;

            //Authentication failed
            else UserController.DropUser(_user);

            //Log state if enabled
            if (Server.loggingEnabled) Logger.LogState(_user);
        }

        public static async void LoginRequestHandleAsync(LoginRequestMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;

            //Check credentials and create response message
            LoginValidationResult result = await LoginValidator.ValidateLoginAsync(_message);
            LoginResponseMessage response = new LoginResponseMessage();
            response.type = NetMessageType.LoginResponse;
            response.result = result.result;

            //Set invalid password incrementer
            if(result.result == LoginAttemptResult.InvalidPassword)
            {
                await SetLoginTryAsync(result.userData.username);
                response.loginTryLeft = (5 - result.userData.passwordAttempts);
            }

            //Uppon successfull login
            if (result.result == LoginAttemptResult.Success)
            {
                //Reset login attempts
                await SetLoginTryAsync(result.userData.username, true);

                //Link UserData to User client
                _user.userData = result.userData;

                //Send back user information                
                UserData sendUser = GeneralManager.EncodeUser(_user.userData, false);
                sendUser.password = "N/A";      //Dont send password even to admin
                response.user = sendUser;
            }

            //Send response
            UserController.Send(response, _user);
        }

        public static async void RegistrationHandle(RegistrationRequestMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;

            LoginValidationResult result = await LoginValidator.ValidateRegistrationAsync(_message);
            RegistrationResponseMessage response = new RegistrationResponseMessage();
            response.type = NetMessageType.LoginResponse;
            response.result = result.result;

            if(result.result == LoginAttemptResult.Success)
            {
                //Create new user
                UserData user = await CreateUser(_message);
                _user.userData = user;

                //Send back user information
                UserData sendUser = GeneralManager.EncodeUser(user, false);
                sendUser.password = "N/A";      //Dont send password even to admin
                response.user = sendUser;
            }

            //Send response
            UserController.Send(response, _user);
        }

        public static async void AdminCreateUserHandle(RegistrationRequestMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;

            //If user is not admin
            if (_user.userData?.type != UserType.Admin) return;

            LoginValidationResult result = await LoginValidator.ValidateRegistrationAsync(_message);
            RegistrationResponseMessage response = new RegistrationResponseMessage();
            response.type = NetMessageType.LoginResponse;
            response.result = result.result;
        }
        #endregion

        #region User Requests
        public static async void AccountDeleteRequestHandle(User _user)
        {
            //Check call validity and if requested before
            if (_user == null || _user.userData == null || _user.userData.accountDeleteRequested) return;

            bool result = await SetDeleteRequest(_user.userData);

            AccountDeleteResponseMessage response = new AccountDeleteResponseMessage();
            response.result = result;

            UserController.Send(response, _user);
        }

        private static async Task<bool> SetDeleteRequest(UserData _user)
        {
            if (_user == null) return false;

            _user.accountDeleteRequested = true;

            bool result = await DBController.SetUserData(_user, _user.ID);
            return result;
        }

        public static async void CarBrandRequestHandle(CarBrandAddRequestMessage _message, User _user)
        {
            if (_message == null || _user == null) return;

            //For better matching results, store dictionary elements w/ upper chars
            string brand = _message.brandName.ToUpper();
            brand = GeneralManager.Encrypt(brand, true);

            //Try creating new brand request
            UserRequestResult result = await DBController.CreateCarBrandRequestAsync(brand, _user.userData ?? null);

            //Create response
            CarBrandAddResponseMessage response = new CarBrandAddResponseMessage();
            response.result = result;

            //Send
            UserController.Send(response, _user);
        }
        #endregion

        #region Other
        #region Modify User
        /// <summary>
        /// Increases or resets login try incrementer of a user.
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="reset"></param>
        private static async Task<bool> SetLoginTryAsync(string _username, bool reset = false)
        {
            //Check call validitiy
            if (string.IsNullOrEmpty(_username)) return false;

            await Task.Run(() => SetLoginTry(_username, reset));
            return true;
        }

        private static async Task<bool> SetLoginTry(string _username, bool reset = false)
        {
            //Check call validitiy
            if (string.IsNullOrEmpty(_username)) return false;

            //Get user
            UserData user = null;
            try { user = await DBController.GetUserByUsernameAsync(_username); } catch { return false; }

            //Set tries. If reset, set it to zero
            if (!reset) user.passwordAttempts = Math.Clamp(++user.passwordAttempts, 0, 5);
            else user.passwordAttempts = 0;

            //If password attempts reached 5
            if (user.passwordAttempts == 5)
                user.active = false;

            //Save
            await DBController.SetUserData(user, user.ID);
            return true;
        }
        #endregion

        #region Create / Delete User
        private static async Task<UserData> CreateUser(RegistrationRequestMessage _message)
        {
            //Check call validation
            if (_message == null) return null;

            //Instantiate new user
            //Store data hashed or with custom encryption to prevent SQLi
            UserData newUser = new UserData();
            newUser.username = GeneralManager.HashData(_message.username);
            newUser.password = GeneralManager.HashData(_message.passwordFirst);
            newUser.fullname = GeneralManager.Encrypt(_message.fullname, true);
            newUser.active = true;
            newUser.accountDeleteRequested = false;
            newUser.passwordAttempts = 0;

            //Set User type
            if(_message.type == NetMessageType.AdminRegistrationRequest)
            {
                AdminRegistrationRequestMessage adminMessage = _message as AdminRegistrationRequestMessage;
                newUser.type = adminMessage.userType;
            }
            else newUser.type = UserType.User;

            //Save into db
            await DBController.CreateUser(newUser);
            return newUser;
        }
        #endregion
        #endregion
    }
}

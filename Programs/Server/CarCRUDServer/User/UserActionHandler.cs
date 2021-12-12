using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void CheckAuthenticationKey(KeyAuthenticationRequestMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;

            //Check key match
            bool result = Server.CheckKey(_message.key);

            //Send response
            KeyAuthenticationResponseMessage response = new KeyAuthenticationResponseMessage();
            response.result = result;
            UserController.Send(response, _user);

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
            response.result = result.result;

            //Set invalid password incrementer
            if(result.result == LoginAttemptResult.InvalidPassword)
                await SetLoginTryAsync(result.userData.username);

            //Uppon successfull login
            if (result.result == LoginAttemptResult.Success)
            {
                //Reset login attempts
                await SetLoginTryAsync(result.userData.username, true);

                //Link UserData to User client
                _user.userData = result.userData;

                //Set response data
                response = await SetupLoginResponse(_user, result.result);
                response.result = result.result;
            }

            //Send response
            UserController.Send(response, _user);
        }

        private static async Task<LoginResponseMessage> SetupLoginResponse( User _user, LoginAttemptResult _result)
        {
            if (_user == null) return null;

            LoginResponseMessage response = new LoginResponseMessage();

            //Set user response information            
            UserData sendUser = GeneralManager.EncodeUser(_user.userData, false);

            //Set user specific response data
            if (_user.userData.type != UserType.Admin) //Dont send password if not admin requested
                sendUser.password = "N/A";
            response.user = sendUser;
            response.favourites = await DBController.GetFavouritesAsync(_user.userData.ID);

            //Set response data
            response.userResponseData = await GetGeneralResponseDataAsync();
            if (_user.userData.type == UserType.Admin)
                response.adminResponseData = await GetAdminResponseDataAsync();

            return response;
        }

        private static async Task<GeneralResponseData> GetGeneralResponseDataAsync()
        {
            GeneralResponseData result = new GeneralResponseData();

            result.carBrands = await DBController.GetCarBrandsAsync();
            result.carTypes = await DBController.GetCarTypesAsync("*");

            return result;
        }

        private static async Task<AdminResponseData> GetAdminResponseDataAsync()
        {
            AdminResponseData result = new AdminResponseData();

            result.users = await DBController.GetAllUserAsync();
            result.requests = await DBController.GetRequestsByUsernameAsync("*");

            return result;
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

        public static void LogoutHandle(User _user)
        {
            if (_user == null) return;

            _user.userData = null;
            _user.status = UserStatus.LoggedOut;

            if (Server.loggingEnabled) Logger.LogState(_user);
        }
        #endregion

        #region User Requests
        public static async void UserRequestHandle(UserRequestMesssage _message, User _user)
        {
            if (_message == null || _user == null) return;

            bool result = false;
            if (_message.requestType == UserRequestType.AccountDelete)
                result = await AccountDeleteRequestHandle( _user.userData.username);
            if (_message.requestType == UserRequestType.BrandAttach)
                result = await BrandAttachRequestHandle(_message.brandAttach, _user.userData.username);

            UserRequestResponse response = new UserRequestResponse();
            response.result = result;

            UserController.Send(response, _user);
        }

        private static async Task<bool> AccountDeleteRequestHandle(string _username)
        {
            //Check call validity and if requested before
            if (_username == null) return false;

            List<UserRequest> request = await DBController.GetRequestsByUsernameAsync(_username);

            //If no request is found
            if (request == null || request.Count < 0)
                await DBController.CreateAccountDeleteRequestAsync(_username);

            //If no account delete request is found
            else if(!request.Any(r => r.type == UserRequestType.AccountDelete))
                await DBController.CreateAccountDeleteRequestAsync(_username);

            return true;
        }

        private static async Task<bool> BrandAttachRequestHandle(string _brand, string _username)
        {
            if (_brand == null || _username == null) return false;

            List<UserRequest> request = await DBController.GetRequestsByUsernameAsync("*");

            //If no request is found
            if (request == null || request.Count < 0)
                await DBController.CreateBrandAttachRequestAsync(_brand, _username);

            //If no request with this brand is found
            else if (!request.Any(r => r.brandAttach == _brand))
                await DBController.CreateBrandAttachRequestAsync(_brand, _username);

            return true;
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
            await DBController.SetUserDataAsync(user, user.ID);
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
            newUser.passwordAttempts = 0;

            //Set User type
            if(_message.type == NetMessageType.AdminRegistrationRequest)
            {
                AdminRegistrationRequestMessage adminMessage = _message as AdminRegistrationRequestMessage;
                newUser.type = adminMessage.userType;
            }
            else newUser.type = UserType.User;

            //Save into db
            await DBController.CreateUserAsync(newUser);
            return newUser;
        }
        #endregion
        #endregion
    }
}

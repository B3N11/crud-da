using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarCRUD.DataBase;
using CarCRUD.DataModels;
using CarCRUD.Networking;
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

        #region Login
        public static async void LoginRequestHandleAsync(LoginRequestMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;

            //Check credentials and create response message
            LoginValidationResult result = await LoginValidator.ValidateLoginAsync(_message);
            LoginResponseMessage response = new LoginResponseMessage();
            response.result = result.result;

            //Set invalid password incrementer
            int? triesLeft = 0;
            if(result.result == LoginAttemptResult.InvalidPassword)     //Get tries set to the user
                 triesLeft = await SetLoginTryAsync(result.userData?.username);
            response.loginTryLeft = (5 - triesLeft) ?? default(int);

            //Uppon successfull login
            if (result.result == LoginAttemptResult.Success)
            {
                //Reset login attempts
                await SetLoginTryAsync(result.userData.username, true);

                //Link UserData to User client
                _user.userData = result.userData;

                //Set response data
                response = await SetupLoginResponse(_user, result.result);                
            }

            //Send response
            UserController.Send(response, _user);
        }
        #endregion

        #region LoginResponse
        private static async Task<LoginResponseMessage> SetupLoginResponse(User _user, LoginAttemptResult _result)
        {
            if (_user == null) return null;

            LoginResponseMessage response = new LoginResponseMessage();
            response.result = _result;

            //Set user response information            
            UserData sendUser = GeneralManager.EncryptUser(_user.userData, false);

            //Set user specific response data
            if (_user.userData.type != UserType.Admin) //Dont send password if not admin requested
                sendUser.password = "N/A";
            response.user = sendUser;

            //Get favourites
            List<FavouriteCar> favourites = await DBController.GetFavouritesAsync(_user.userData.ID);
            //Decrypt all
            response.favourites.ForEach(c => c = GeneralManager.EncryptFavouriteCar(c, false));

            //Set response data
            response.userResponseData = await GetGeneralResponseDataAsync();
            if (_user.userData.type == UserType.Admin)
                response.adminResponseData = await GetAdminResponseDataAsync();

            return response;
        }

        private static async Task<GeneralResponseData> GetGeneralResponseDataAsync()
        {
            GeneralResponseData result = new GeneralResponseData();

            //Get data
            List<CarBrand> brands = await DBController.GetCarBrandsAsync();
            List<CarType> types = await DBController.GetCarTypesAsync("*");            

            //Decrypt all data
            result.carBrands.ForEach(b => b = GeneralManager.EncryptCarBrand(b, false));
            result.carTypes.ForEach(t => t = GeneralManager.EncryptCarType(t, false));

            return result;
        }

        private static async Task<AdminResponseData> GetAdminResponseDataAsync()
        {
            AdminResponseData result = new AdminResponseData();

            //Get data
            result.users = await DBController.GetAllUserAsync();
            result.requests = await DBController.GetRequestsAsync("*");

            //Decode all data
            result.users.ForEach(u => u = GeneralManager.EncryptUser(u, false));
            result.requests.ForEach(r => r = GeneralManager.EncryptRequest(r, false));

            return result;
        }
        #endregion

        #region Registration
        public static async void RegistrationHandle(RegistrationRequestMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;

            LoginValidationResult result = await LoginValidator.ValidateRegistrationAsync(_message);
            RegistrationResponseMessage response = new RegistrationResponseMessage();
            response.result = result.result;

            if(result.result == LoginAttemptResult.Success)
            {
                //Create new user
                UserData user = await CreateUser(_message);
                _user.userData = user;

                //Send back user information
                UserData sendUser = GeneralManager.EncryptUser(user, false);
                sendUser.password = "N/A";      //Dont send password even to admin
                response.user = sendUser;
            }

            //Send response
            UserController.Send(response, _user);
        }

        public static async void AdminRegistrationHandle(AdminRegistrationRequestMessage _message, User _user)
        {
            if (_message == null || _user == null || _user.userData?.type != UserType.Admin) return;

            LoginValidationResult result = await LoginValidator.ValidateRegistrationAsync(_message);
            AdminRegistrationResponseMessage response = new AdminRegistrationResponseMessage();
            response.result = result.result;
                        
            //If success, create user
            if(result.result == LoginAttemptResult.Success)
            {
                UserData user = await CreateUser(_message);
                response.user = GeneralManager.EncryptUser(user, false);
                if (user == null) response.result = LoginAttemptResult.Failure;
            }

            //Send Response
            UserController.Send(response, _user);
        }
        #endregion

        #region Logout
        public static void LogoutHandle(User _user)
        {
            if (_user == null) return;

            _user.userData = null;
            _user.status = UserStatus.LoggedOut;

            if (Server.loggingEnabled) Logger.LogState(_user);
        }
        #endregion
        #endregion

        #region User Requests
        public static async void UserRequestHandle(UserRequestMesssage _message, User _user)
        {
            if (_message == null || _user == null) return;


            bool result = false;
            string hashedBrand = GeneralManager.Encrypt(_message.brandAttach, true);

            if (_message.requestType == UserRequestType.AccountDelete)
                result = await AccountDeleteRequestHandle( _user.userData.username);

            if (_message.requestType == UserRequestType.BrandAttach)
                result = await BrandAttachRequestHandle(hashedBrand, _user.userData.username);

            UserRequestResponseMessage response = new UserRequestResponseMessage();
            response.result = result;

            UserController.Send(response, _user);
        }

        private static async Task<bool> AccountDeleteRequestHandle(string _hashedUsername)
        {
            //Check call validity and if requested before
            if (_hashedUsername == null) return false;

            List<UserRequest> request = await DBController.GetRequestsAsync(_hashedUsername);

            //If no request is found
            if (request == null || request.Count < 0)
                await DBController.CreateAccountDeleteRequestAsync(_hashedUsername);

            //If no account delete request is found
            else if(!request.Any(r => r.type == UserRequestType.AccountDelete))
                await DBController.CreateAccountDeleteRequestAsync(_hashedUsername);

            return true;
        }

        private static async Task<bool> BrandAttachRequestHandle(string _brand, string _username)
        {
            if (_brand == null || _username == null) return false;

            //Get requests
            string hashedUsername = GeneralManager.HashData(_username);
            string hashedBrand = GeneralManager.Encrypt(_brand, true);
            List<UserRequest> request = await DBController.GetRequestsAsync(hashedUsername);

            //If no request is found
            if (request == null || request.Count < 0)
                await DBController.CreateBrandAttachRequestAsync(hashedBrand, hashedUsername);

            //If no request with this brand is found
            else if (!request.Any(r => r.brandAttach == hashedBrand))
                await DBController.CreateBrandAttachRequestAsync(hashedBrand, hashedUsername);

            return true;
        }
        #endregion

        #region Other

        #region Modify User
        /// <summary>
        /// Increases or resets login try incrementer of a user.
        /// </summary>
        /// <param name="_hashedUsername"></param>
        /// <param name="reset"></param>
        private static async Task<int?> SetLoginTryAsync(string _hashedUsername, bool reset = false)
        {
            //Check call validitiy
            if (string.IsNullOrEmpty(_hashedUsername)) return null;

            int? result = await Task.Run(() => SetLoginTry(_hashedUsername, reset));
            return result;
        }

        private static async Task<int?> SetLoginTry(string _hashedUsername, bool reset = false)
        {
            //Check call validitiy
            if (string.IsNullOrEmpty(_hashedUsername)) return null;

            //Get user
            UserData user = null;
            try { user = await DBController.GetUserAsync(_hashedUsername); } catch { return null; }

            //Set tries. If reset, set it to zero
            if (!reset) user.passwordAttempts = Math.Clamp(++user.passwordAttempts, 0, 5);
            else user.passwordAttempts = 0;

            //If password attempts reached 5
            if (user.passwordAttempts == 5)
                user.active = false;

            //Save
            await DBController.SetUserDataAsync(user, user.ID);
            return user.passwordAttempts;
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
            if(_message.type == NetMessageType.AdminRegistrationRequest)    //If admin, set user type
                newUser.type = (_message as AdminRegistrationRequestMessage).userType;
            else newUser.type = UserType.User;

            //Save into db
            await DBController.CreateUserAsync(newUser);
            return newUser;
        }

        public static async void UserDeleteHandleAsync(UserDeleteRequestMessage _message, User _user)
        {
            if (_message == null || _user == null || _user.userData?.type != UserType.Admin) return;

            string hashedUsername = GeneralManager.HashData(_message.username);
            UserDeleteResponseMessage response = new UserDeleteResponseMessage();
            response.result = await Task.Run(() => DeleteUser(hashedUsername));

            UserController.Send(response, _user);
        }

        private static async Task<bool> DeleteUser(string _hashedUsername)
        {
            if (string.IsNullOrEmpty(_hashedUsername))
                return false;

            //Get user
            UserData user = await DBController.GetUserAsync(_hashedUsername);
            if (user == null) return false;

            //Check if user requested delete
            List<UserRequest> requests = await DBController.GetRequestsAsync(_hashedUsername);
            if (!requests.Any(r => r.type == UserRequestType.AccountDelete))
                return false;

            bool result = await DBController.DeleteUser(_hashedUsername);
            return result;
        }
        #endregion
        #endregion

        #region Car Handle
        public static async void BrandCreateHandleAsync(BrandCreateMessage _message, User _user)
        {
            if (_message == null || _user == null || _user.userData?.type != UserType.Admin) return;

            string hashedBrand = GeneralManager.Encrypt(_message.name, true);
            List<CarBrand> brands = await DBController.GetCarBrandsAsync();

            //If already exists
            if (brands != null && brands.Any(b => b.name == hashedBrand))
                return;

            bool result = await DBController.CreateBrandAsync(hashedBrand);

            //Send response
            BrandCreateResponseMessage response = new BrandCreateResponseMessage();
            response.result = result;

            UserController.Send(response, _user);
        }

        public static async void FavouriteCarCreateHandleAsync(FavouriteCarCreateRequestMessage _message, User _user)
        {
            if (_message == null || _user == null) return;

            FavouriteCarCreateResponseMessage response = new FavouriteCarCreateResponseMessage();
            response.result = await FavouriteCarCreate(_message, _user.userData);

            UserController.Send(response, _user);
        }

        private static async Task<bool> FavouriteCarCreate(FavouriteCarCreateRequestMessage _message, UserData _user)
        {
            if (_message == null) return false;

            //Get type of car
            string hashedType = GeneralManager.Encrypt(_message.carType, true);
            CarType type;
            List<CarType> types = await DBController.GetCarTypesAsync(_message.brandID);

            //If there is none, create one
            if (types == null || !types.Any(t => t.name == hashedType))
                type = await DBController.CreateCarTypeAsync(hashedType, _message.brandID);
            //If there is one
            else type = types.First(t => t.name == hashedType);

            FavouriteCar car = new FavouriteCar();
            car.carTypeData = type;
            car.userData = _user;
            car.color = GeneralManager.Encrypt(_message.color, true);
            car.year = _message.year;
            car.fuel = GeneralManager.Encrypt(_message.fuel, true);

            bool result = await DBController.CreateFavouriteCarAsync(car);
            return result;
        }
        #endregion
    }
}
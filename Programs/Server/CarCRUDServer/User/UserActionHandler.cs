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
            if (ServerSettings.LoggingEnabled) Logger.LogState(_user);            
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

            //Set response data
            response.userResponseData = await GetGeneralResponseDataAsync(_user.userData.ID);
            if (_user.userData.type == UserType.Admin)
                response.adminResponseData = await GetAdminResponseDataAsync();

            return response;
        }

        private static async Task<GeneralResponseData> GetGeneralResponseDataAsync(int _userID)
        {
            GeneralResponseData result = new GeneralResponseData();

            //Get data
            List<CarBrand> carBrands = await DBController.GetCarBrandsAsync();
            List<CarType> carTypes = await DBController.GetCarTypesAsync("*", false);
            List<FavouriteCar> favourites = await DBController.GetFavouritesAsync(_userID, true);

            //Decrypt all data
            result.carBrands = new List<CarBrand>();
            result.carTypes = new List<CarType>();
            result.favourites = new List<FavouriteCar>();
            carBrands.ForEach(b =>
            {
                CarBrand decrypted = GeneralManager.EncryptCarBrand(b, false);
                result.carBrands.Add(decrypted);
            });
            carTypes.ForEach(t =>
            {
                CarType decrypted = GeneralManager.EncryptCarType(t, false, true);
                result.carTypes.Add(decrypted);
            });
            favourites.ForEach(f =>
            {
                FavouriteCar decrypted = GeneralManager.EncryptFavouriteCar(f, false, true);
                //Remove unecessary component
                decrypted.userData = null;
                result.favourites.Add(decrypted);
            });

            return result;
        }

        private static async Task<AdminResponseData> GetAdminResponseDataAsync()
        {
            AdminResponseData result = new AdminResponseData();

            //Get data
            List<UserData> users = await DBController.GetAllUserAsync();
            List<UserRequest> requests = await DBController.GetRequestsAsync("*");

            //Decode all data
            result.users = new List<UserData>();
            result.requests = new List<UserRequest>();
            users.ForEach(u =>
            {
                UserData decrypted = GeneralManager.EncryptUser(u, false);
                result.users.Add(decrypted);
            });
            requests.ForEach(r =>
            {
                UserRequest decrypted = GeneralManager.EncryptRequest(r, false);
                result.requests.Add(decrypted);
            });

            return result;
        }
        #endregion

        #region Registration
        public static async void RegistrationHandle(RegistrationRequestMessage _message, User _user)
        {
            //Check call validity
            if (_message == null || _user == null) return;

            LoginValidationResult result = await LoginValidator.ValidateRegistrationAsync(_message);
            LoginResponseMessage response = new LoginResponseMessage();
            response.result = result.result;

            if(result.result == LoginAttemptResult.Success)
            {
                //Create new user
                UserData user = await CreateUser(_message);
                _user.userData = user;

                response = await SetupLoginResponse(_user, result.result);
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

            if (ServerSettings.LoggingEnabled) Logger.LogState(_user);
        }
        #endregion
        #endregion

        #region User Requests
        /// <summary>
        /// Handles an incoming user request for account deletion or new brand
        /// </summary>
        /// <param name="_message"></param>
        /// <param name="_user"></param>
        public static async void UserRequestHandleAsync(UserRequestMesssage _message, User _user)
        {
            if (_message == null || _user == null) return;


            bool result = false;
            //Encrypt brand name
            string hashedBrand = GeneralManager.Encrypt(_message.brandAttach, true);

            //Handle if delete request
            if (_message.requestType == UserRequestType.AccountDelete)
                result = await AccountDeleteRequestHandle( _user.userData.username);

            //Handle if new brand request
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

        /// <summary>
        /// Handles a request answer (terminates or dismisses a request)
        /// </summary>
        /// <param name="_message"></param>
        /// <param name="_user"></param>
        public static async void RequestAnswerRequestHandleAsync(RequestAnswerRequestMessage _message, User _user)
        {
            if (_message == null || _user == null || _user.userData?.type != UserType.Admin)
                return;

            //Do operation
            RequestAnswerResponseMessage response = new RequestAnswerResponseMessage();
            response.result = await Task.Run(() => RequestAnswerRequestHandle(_message, _user.userData.ID));
            response.requestID = _message.requestID;

            //Send response
            UserController.Send(response, _user);
        }

        private static async Task<bool> RequestAnswerRequestHandle(RequestAnswerRequestMessage _message, int _userID)
        {
            if (_message == null)
                return false;

            UserRequest request = null;
            try { request = await DBController.GetRequestAsync(_message.requestID, false); } catch { }
            if (request == null) return false;

            //If dismissed
            if (!_message.accept)
            {
                bool result = await DBController.DeleteRequest(request);
                return result;
            }

            //If Account Delete request
            if (request.type == UserRequestType.AccountDelete)
            {
                //One cannot delete themselves
                if (request.userData.ID == _userID)
                    return false;

                bool result = await DBController.DeleteUser(request.userData.username);
                return result;
            }

            //If new brand request
            if(request.type == UserRequestType.BrandAttach && request.brandAttach != null)
            {
                CarBrand result = await DBController.CreateBrandAsync(request.brandAttach);

                if (request == null) return false;
                return true;
            }

            return false;
        }
        #endregion


        #region Modify User
        /// <summary>
        /// Creates new user
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
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
            if (_message.type == NetMessageType.AdminRegistrationRequest)    //If admin, set user type
                newUser.type = (_message as AdminRegistrationRequestMessage).userType;
            else newUser.type = UserType.User;

            //Save into db
            await DBController.CreateUserAsync(newUser);
            return newUser;
        }

        /// <summary>
        /// Resets a user's activity
        /// </summary>
        /// <param name="_message"></param>
        /// <param name="_user"></param>
        public static async void UserActivityResetHandleAsync(UserActivityResetRequestMessage _message, User _user)
        {
            if (_message == null || _user == null || _user.userData.type != UserType.Admin)
                return;

            UserActivityResetResponseMessage response = new UserActivityResetResponseMessage();
            response.result = await Task.Run(() => UserActivityResetHandle(_message));
            response.userID = _message.userID;

            UserController.Send(response, _user);
        }

        private static async Task<bool> UserActivityResetHandle(UserActivityResetRequestMessage _message)
        {
            if (_message == null)
                return false;

            UserData user = null;
            try { user = await DBController.GetUserAsync(_message.userID); } catch { }
            if (user == null) return false;

            //If already active
            if (user.active) return false;

            //Reset
            user.active = true;
            user.passwordAttempts = 0;

            bool result = await DBController.SetUserDataAsync(user, user.ID);
            return result;
        }

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

        #region Car Handle
        public static async void BrandCreateHandleAsync(BrandCreateRequestMessage _message, User _user)
        {
            if (_message == null || _user == null || _user.userData?.type != UserType.Admin) return;

            string hashedBrand = GeneralManager.Encrypt(_message.name, true);
            if (hashedBrand == null) return;

            CarBrand result = await DBController.CreateBrandAsync(hashedBrand);

            //Send response
            BrandCreateResponseMessage response = new BrandCreateResponseMessage();
            response.result = result != null ? true : false;
            response.carBrand = GeneralManager.EncryptCarBrand(result, false);

            UserController.Send(response, _user);
        }

        /// <summary>
        /// Creates a new FavouriteCar for a user;
        /// </summary>
        /// <param name="_message"></param>
        /// <param name="_user"></param>
        public static async void FavouriteCarCreateHandleAsync(FavouriteCarCreateRequestMessage _message, User _user)
        {
            if (_message == null || _user == null) return;

            FavouriteCarCreateResponseMessage response = new FavouriteCarCreateResponseMessage();
            response.favouriteCar = await FavouriteCarCreate(_message, _user.userData);
            response.result = response.favouriteCar == null ? false : true;

            //Remove unecessary data
            if (response.favouriteCar != null)
                response.favouriteCar.userData = null;

            UserController.Send(response, _user);
        }

        private static async Task<FavouriteCar> FavouriteCarCreate(FavouriteCarCreateRequestMessage _message, UserData _user)
        {
            if (_message == null) return null;

            //Get type of car
            string hashedType = GeneralManager.Encrypt(_message.carType, true);

            //Check brand existance
            CarBrand brand = await DBController.GetCarBrandAsync(_message.brandID);
            if (brand == null) return null;

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
            return GeneralManager.EncryptFavouriteCar(car, false, true);
        }
        #endregion
    }
}
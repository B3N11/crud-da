using System;
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
                await DBController.SetLoginTryAsync(result.userData.username);
                response.loginTryLeft = (5 - result.userData.passwordAttempts);
            }

            //Uppon successfull login
            if (result.result == LoginAttemptResult.Success)
            {
                //Reset login attempts
                await DBController.SetLoginTryAsync(result.userData.username, true);

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

            UserRequest request = new UserRequest();
            request.accountRemove = false;
            request.brandAttach = string.Empty;
            newUser.request = request;

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
    }
}

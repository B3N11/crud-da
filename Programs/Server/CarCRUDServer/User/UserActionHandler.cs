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
            response.result = result.result;

            //Set invalid password incrementer
            if(result.result == LoginAttemptResult.InvalidPassword)
                await DBController.SetLoginTryAsync(GeneralManager.HashData(_message.username));

            //Uppon successfull login
            if (result.result == LoginAttemptResult.Success)
            {
                //Link UserData to User client
                _user.userData = result.userData;
                response.userType = result.userData.type;
                //Reset login attempts
                await DBController.SetLoginTryAsync(GeneralManager.HashData(_message.username), true);
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
            response.result = result.result;

            if(result.result == LoginAttemptResult.Success)
            {
                UserData user = await CreateUser(_message);
                response.userType = user.type;
                _user.userData = user;
            }

            //Send response
            UserController.Send(response, _user);
        }

        private static async Task<UserData> CreateUser(RegistrationRequestMessage _message)
        {
            //Check call validation
            if (_message == null) return null;

            //Instantiate new user
            //Store data hashed or in base64 to prevent SQLi
            UserData newUser = new UserData();
            newUser.username = GeneralManager.HashData(_message.username);
            newUser.password = GeneralManager.HashData(_message.passwordFirst);
            newUser.fullname = GeneralManager.Base64(_message.fullname, true);
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

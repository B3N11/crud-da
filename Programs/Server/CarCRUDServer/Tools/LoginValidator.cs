using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CarCRUD.DataModels;

namespace CarCRUD.ServerHandle
{
    class LoginValidator
    {
        /// <summary>
        /// Controller stores the functions to call for each check
        /// </summary>
        public delegate Task<UserData> UsernameCheck(string _username);
        public delegate string DataHasher(string _data, bool _base64);

        private static UsernameCheck CheckUsername;
        private static DataHasher HashData;

        private static bool initialized = false;

        public static void SetTools(UsernameCheck _checkUsername, DataHasher _hashData)
        {
            if (_checkUsername == null) return;

            CheckUsername = _checkUsername;
            HashData = _hashData;
            initialized = true;
        }

        /// <summary>
        /// Uses the given CheckCredentials function to validate login data
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
        public static async Task<LoginValidationResult> ValidateLoginAsync(LoginRequestMessage _message)
        {
            //Check call validity and controller
            if (_message == null || !initialized)
                return new LoginValidationResult(LoginAttemptResult.Failure);

            //Hash Data
            string hashedUsername = HashData(_message.username, true);
            string hashedPassword = HashData(_message.password, true);

            //Check Credentials
            UserData userData = await CheckUsername(hashedUsername);
            if (userData == null) return new LoginValidationResult(LoginAttemptResult.InvalidUsername);
            if (!userData.active) return new LoginValidationResult(LoginAttemptResult.AccountLocked);
            if (hashedPassword != userData.password) return new LoginValidationResult(LoginAttemptResult.InvalidPassword, userData);

            return new LoginValidationResult(LoginAttemptResult.Success, userData);
        }

        /// <summary>
        /// Checks if the registration can be completed with the given data
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
        public static async Task<LoginValidationResult> ValidateRegistrationAsync(RegistrationRequestMessage _message)
        {
            //Check call validity
            if (_message == null || !initialized) return null;

            //Check username existance
            string hashedUsername = HashData(_message.username, true);
            UserData userData = await CheckUsername(hashedUsername);
            if (userData != null) return new LoginValidationResult(LoginAttemptResult.UsernameExists);

            //Check password match
            if (!(_message.passwordFirst == _message.passwordSecond))
                return new LoginValidationResult(LoginAttemptResult.NoPasswordMatch);

            //Check password format
            LoginAttemptResult formatValidity = await Task.Run(() => CheckPasswordFormat(_message.passwordFirst));
            if (formatValidity == LoginAttemptResult.InvalidPasswordFormat)
                return new LoginValidationResult(LoginAttemptResult.InvalidPasswordFormat);

            //If registration can be completed
            return new LoginValidationResult(LoginAttemptResult.Success);
        }

        private static LoginAttemptResult CheckPasswordFormat(string _password)
        {
            //Special characters supported for passwords based on (https://docs.oracle.com/cd/E11223_01/doc.910/e11197/app_special_char.htm#MCMAD416)
            List<char> special = new char[]{ '@', '%', '+', '\\', '/', '\'', '!', '#', '$', '^', '?', ':', ',', '(', ')', '{', '}', '[', ']', '~', '-', '_'}.ToList();

            //Check for special character
            bool containsSpecial = _password.Any(c => special.Any(s => s == c));
            //Check for length
            bool requiredLength = _password.Length >= 8;
            //Check for number, upper and lower case chars
            bool upperChar = _password.Any(char.IsUpper);
            bool lowerChar = _password.Any(char.IsLower);
            bool digitChar = _password.Any(char.IsDigit);

            //Check format
            if (!requiredLength || !containsSpecial || !upperChar || !lowerChar || !digitChar)
                return LoginAttemptResult.InvalidPasswordFormat;

            return LoginAttemptResult.Success;
        }
    }

    class LoginValidationResult
    {
        public UserData userData { get; set; }
        public LoginAttemptResult result { get; set; }

        public LoginValidationResult(LoginAttemptResult _result, UserData _userData = null)
        {
            result = _result;
            userData = _userData;
        }
    }
}
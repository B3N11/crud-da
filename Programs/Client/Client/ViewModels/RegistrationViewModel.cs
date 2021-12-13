using Caliburn.Micro;
using CarCRUD.DataModels;
using CarCRUD.Users;
using System.Windows;

namespace CarCRUD.ViewModels
{
    class RegistrationViewModel : Screen
    {
        #region Properties
        private MainViewModel main;        
        private HomeViewModel home;
        private string username = string.Empty;
        private string passwordFirst = string.Empty;
        private string passwordSecond = string.Empty;
        private string fullName = string.Empty;
        private string loginResponse = string.Empty;

        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                NotifyOfPropertyChange(() => username);
            }
        }
        public string PasswordFirst
        {
            get { return passwordFirst; }
            set
            {
                passwordFirst = value;
                NotifyOfPropertyChange(() => passwordFirst);
            }
        }
        public string PasswordSecond
        {
            get { return passwordSecond; }
            set
            {
                passwordSecond = value;
                NotifyOfPropertyChange(() => passwordSecond);
            }
        }
        public string FullName
        {
            get { return fullName; }
            set
            {
                fullName = value;
                NotifyOfPropertyChange(() => fullName);
            }
        }
        public string LoginResponse
        {
            get { return loginResponse; }
            set
            {
                loginResponse = value;
                NotifyOfPropertyChange(() => loginResponse);
            }
        }
        #endregion

        public RegistrationViewModel(MainViewModel _main, HomeViewModel _home)
        {
            main = _main;
            home = _home;
            //Sub to event
            UserController.OnLoginResultedEvent += ResponseHandle;
        }

        public void ResponseHandle(LoginAttemptResult _message, int _loginsLeft = 0)
        {
            if (_message == LoginAttemptResult.Success)
                main.SetControl(new ActionViewModel(main), true);

            else LoginResultDisplay(_message);
        }

        public void LoginResultDisplay(LoginAttemptResult _result, int _loginsLeft = 0)
        {
            switch (_result)
            {
                case LoginAttemptResult.InvalidPasswordFormat:
                    LoginResponse = "Your character must contain upper case letter, lower case letter, special character and number.";
                    break;

                case LoginAttemptResult.UsernameExists:
                    LoginResponse = "This username already exists.";
                    break;

                case LoginAttemptResult.NoPasswordMatch:
                    LoginResponse = "The two passwords must match.";
                    break;
            }
        }

        #region Button Events
        public void Register()
        {
            //Avoid multiple requests
            if (!UserController.user.canRequest)
            {
                MessageBox.Show("We haven't finished processing your previous request. Please wait!");
                return;
            }

            //Clear response text and send request
            LoginResponse = string.Empty;
            bool result = UserActionHandler.RequestRegistration(username, passwordFirst, passwordSecond, fullName);

            if (result) return;
            //Warn about empty fields
            LoginResponse = "Don't leave the fields empty!";
        }

        public void Cancel()
        {
            //Avoid multiple requests
            if (!UserController.user.canRequest)
            {
                MessageBox.Show("We haven't finished processing your previous request. Please wait!");
                return;
            }

            main.SetControl(home, false);
        }
        #endregion
    }
}
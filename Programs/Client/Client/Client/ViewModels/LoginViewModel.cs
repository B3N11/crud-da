using Caliburn.Micro;
using CarCRUD.Users;
using CarCRUD.DataModels;
using System.Windows;

namespace CarCRUD.ViewModels
{
    class LoginViewModel : Screen
    {
        #region Properties
        private MainViewModel main;
        private HomeViewModel home;
        private string username = string.Empty;
        private string password = string.Empty;
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
        public string Password
        {
            get { return password; }
            set
            {
                password = value;
                NotifyOfPropertyChange(() => password);
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
        public bool RequestEnabled
        {
            get
            {
                return UserController.user.canRequest;
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.canRequest);
            }
        }
        #endregion

        public LoginViewModel(MainViewModel _main, HomeViewModel _home)
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

            else LoginResultDisplay(_message, _loginsLeft);
        }

        public void LoginResultDisplay(LoginAttemptResult _result, int _loginsLeft = 0)
        {
            switch (_result)
            {
                case LoginAttemptResult.InvalidUsername:
                    LoginResponse = $"No user found with username {Username}";
                    break;

                case LoginAttemptResult.InvalidPassword:
                    LoginResponse = $"Invalid password. You have {_loginsLeft} try left before your account gets locked.";
                    break;

                case LoginAttemptResult.LogginAttemptsMax:
                    LoginResponse = "You have reached the maximum enabled number of login attempts. Your has beend account locked. Please, contact an admin for further information!";
                    break;

                case LoginAttemptResult.AccountLocked:
                    LoginResponse = "Your account is locked. Please, contact an admin for further information!";
                    break;

                case LoginAttemptResult.AlreadyLoggedIn:
                    LoginResponse = "Looks like someone is already logged in with this account. Please, contact an admin for further information!";
                    break;
            }
        }

        #region Button Events
        public void Login()
        {
            //Avoid multiple requests
            if (!RequestEnabled)
            {
                MessageBox.Show("We haven't finished processing your previous request. Please wait!");
                return;
            }

            LoginResponse = string.Empty;
            bool result = UserActionHandler.RequestLogin(username, password);

            if (result) return;
            LoginResponse = "Don't leave the fields empty!";
        }

        public void Cancel()
        {
            //Avoid multiple requests
            if (!RequestEnabled)
            {
                MessageBox.Show("We haven't finished processing your previous request. Please wait!");
                return;
            }

            main.SetControl(home, false);
        }
        #endregion
    }
}

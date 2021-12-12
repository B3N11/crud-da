using Caliburn.Micro;
using CarCRUD.DataModels;
using CarCRUD.Users;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CarCRUD.ViewModels
{
    class UsereditViewModel : Screen
    {
        #region Properties
        private MainViewModel main;
        private UsersViewModel users;
        private ActionViewModel action;

        private UserData user;

        private string loginResponse = string.Empty;

        private string username = string.Empty;
        private string passwordFirst = string.Empty;
        private string passwordSecond = string.Empty;
        private string fullname = string.Empty;
        private UserType userType;

        public UsereditViewModel(MainViewModel _main, UsersViewModel _users, ActionViewModel _action, UserData _user = null)
        {
            main = _main;
            users = _users;
            action = _action;
            user = _user;
            //Sub to event
            UserController.OnLoginResultedEvent += ResponseHandle;
        }

        public UserData User
        {
            get
            {
                return user;
            }
            set
            {
                user = value;
                NotifyOfPropertyChange(() => user);
            }
        }
        public string LoginResponse
        {
            get
            {
                return loginResponse;
            }
            set
            {
                loginResponse = value;
                NotifyOfPropertyChange(() => loginResponse);
            }
        }
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
        public string Fullname
        {
            get { return fullname; }
            set
            {
                fullname = value;
                NotifyOfPropertyChange(() => fullname);
            }
        }
        public UserType UserType
        {
            get { return userType; }
            set
            {
                userType = value;
                NotifyOfPropertyChange(() => userType);
            }
        }
        #endregion

        #region Display
        public void ResponseHandle(LoginAttemptResult _message, int loginTryLeft = 0)
        {
            if (_message == LoginAttemptResult.Success)
                action.SetControl(users, true);

            else LoginResultDisplay(_message);
        }

        public void LoginResultDisplay(LoginAttemptResult _result)
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
        #endregion

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
            bool result = UserActionHandler.RequestAdminRegistration(username, passwordFirst, passwordSecond, fullname);

            if (result) return;
            //Warn about empty fields
            LoginResponse = "Don't leave the fields empty!";
        }

        public void Cancel()
        {
            action.SetControl(users, true);
        }
        #endregion
    }
}

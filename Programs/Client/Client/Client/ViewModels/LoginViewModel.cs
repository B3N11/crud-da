using Caliburn.Micro;
using CarCRUD.Users;

namespace CarCRUD.ViewModels
{
    class LoginViewModel : Screen
    {
        #region Properties
        private MainViewModel main;
        private string username = string.Empty;
        private string password = string.Empty;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        #endregion

        public LoginViewModel(MainViewModel _main)
        {
            main = _main;
        }

        #region Button Events
        public void Login()
        {
            UserActionHandler.RequestLogin(username, password);
        }

        public void Cancel()
        {
            main.SetControl(new HomeViewModel(main), true);
        }
        #endregion
    }
}

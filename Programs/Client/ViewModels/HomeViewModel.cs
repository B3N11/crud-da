using Caliburn.Micro;
using CarCRUD.Users;

namespace CarCRUD.ViewModels
{
    class HomeViewModel : Screen
    {
        private MainViewModel main;
        private LoginViewModel login;
        private RegistrationViewModel register;

        public HomeViewModel(MainViewModel _main)
        {
            main = _main;
        }

        #region Button Events
        public void Login()
        {
            if (login == null) login = new LoginViewModel(main, this);
            main.SetControl(login, false);
        }

        public void Registration()
        {
            if (register == null) register = new RegistrationViewModel(main, this);
            main.SetControl(register, false);
        }
        #endregion
    }
}
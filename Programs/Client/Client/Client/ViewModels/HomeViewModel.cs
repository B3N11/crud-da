using Caliburn.Micro;

namespace CarCRUD.ViewModels
{
    class HomeViewModel : Screen
    {
        private MainViewModel main;

        public HomeViewModel(MainViewModel _main)
        {
            main = _main;
        }

        #region Button Events
        public void Login()
        {
            main.SetControl(new LoginViewModel(main), true);
        }
        public void Registration()
        {
            main.SetControl(new RegistrationViewModel(main), true);
        }
        #endregion
    }
}
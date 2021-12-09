using Caliburn.Micro;

namespace CarCRUD.ViewModels
{
    class RegistrationViewModel : Screen
    {
        private MainViewModel main;

        public RegistrationViewModel(MainViewModel _main)
        {
            main = _main;
        }
    }
}

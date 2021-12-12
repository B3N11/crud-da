using Caliburn.Micro;

namespace CarCRUD.ViewModels
{
    class AdminactionViewModel : ActionViewModel
    {
        #region Properties
        private UsersViewModel users;

        public AdminactionViewModel(MainViewModel _main) : base(_main)
        {
        }
        #endregion

        #region Button Events
        public void OpenUsers()
        {
            if (users == null) users = new UsersViewModel(main);
            SetControl(users, false);
        }
        #endregion
    }
}
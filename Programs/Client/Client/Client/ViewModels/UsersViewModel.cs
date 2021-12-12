using System.Collections.Generic;
using Caliburn.Micro;
using CarCRUD.Users;
using CarCRUD.DataModels;

namespace CarCRUD.ViewModels
{
    class UsersViewModel : Screen
    {
        private MainViewModel main;

        public List<UserData> Users
        {
            get
            {
                return UserController.user.adminResponseData.users;
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.adminResponseData.users);
            }
        }

        public UsersViewModel(MainViewModel _main)
        {
            main = _main;
        }
    }
}

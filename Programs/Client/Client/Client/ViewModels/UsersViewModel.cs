using System.Collections.Generic;
using Caliburn.Micro;
using CarCRUD.Users;
using CarCRUD.DataModels;
using System.Windows;

namespace CarCRUD.ViewModels
{
    class UsersViewModel : Screen
    {
        #region Properties
        private MainViewModel main;
        private AdminactionViewModel action;
        private UserData selectedUser;

        public UserData SelectedUser
        {
            get
            {
                return selectedUser;
            }
            set
            {
                selectedUser = value;
                NotifyOfPropertyChange(() => selectedUser);
            }
        }

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

        public UsersViewModel(MainViewModel _main, AdminactionViewModel _action)
        {
            main = _main;
            action = _action;
        }
        #endregion

        #region Button Events
        public void NewUser()
        {
            action.SetControl(new UsereditViewModel(main, this, action), false);
        }
        public void ActivateUser()
        {
            if (selectedUser == null)
                return;

            if (selectedUser.active)
            {
                MessageBox.Show($"User already active.");
                return;
            }


        }
        #endregion
    }
}
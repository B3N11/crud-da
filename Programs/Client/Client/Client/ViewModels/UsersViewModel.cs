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

        public UsersViewModel(AdminactionViewModel _action)
        {
            action = _action;
        }
        #endregion

        #region Button Events
        public void NewUser()
        {
            action.SetControl(new UsereditViewModel(action), false);
        }
        public void ActivateUser()
        {
            if (SelectedUser == null)
                return;

            UserActionHandler.UserActivityResetRequest(SelectedUser.ID);
        }
        #endregion
    }
}
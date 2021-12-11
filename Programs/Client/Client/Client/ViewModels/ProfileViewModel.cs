using Caliburn.Micro;
using CarCRUD.Users;
using System.Windows;

namespace CarCRUD.ViewModels
{
    class ProfileViewModel : Screen
    {
        #region Properties
        private MainViewModel main;
        private string requestBut = string.Empty;

        public string FullName
        {
            get
            {
                return $"Name: {UserController.user?.userData?.fullname ?? "N/A"}";
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.userData.fullname ?? "N/A");
            }
        }
        public string UserType
        {
            get
            {
                return $"Account type: {UserController.user?.userData?.type.ToString() ?? "N/A"}";
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.userData.type.ToString() ?? "N/A");
            }
        }
        public string AcDelRequestButtonTitle
        {
            get
            {
                return UserController.user.userData.accountDeleteRequested == false ? "Request Account Deletion" : "Cancel Account Deletion";
            }
            set
            {
                requestBut = UserController.user.userData.accountDeleteRequested == false ? "Request Account Deletion" : "Cancel Account Deletion";
                NotifyOfPropertyChange(() => requestBut);
            }
        }
        #endregion

        public ProfileViewModel(MainViewModel _main)
        {
            main = _main;
        }

        #region Button Events
        public void Logout()
        {
            //Avoid multiple requests
            if (!UserController.user.canRequest)
            {
                MessageBox.Show("We haven't finished processing your previous request. Please wait!");
                return;
            }

            UserController.Logout();
            main.SetControl(new HomeViewModel(main), true);
        }

        public void RequestAccountDeletion()
        {
            //Avoid multiple requests
            if (!UserController.user.canRequest)
            {
                MessageBox.Show("We haven't finished processing your previous request. Please wait!");
                return;
            }

            //Request/Cancel Account deletion
            UserActionHandler.AccountDeleteRequest(!UserController.user.userData.accountDeleteRequested);
        }
        #endregion
    }
}

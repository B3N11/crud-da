using Caliburn.Micro;
using CarCRUD.Users;
using System;

namespace CarCRUD.ViewModels
{
    class ActionViewModel : Conductor<object>.Collection.OneActive
    {
        #region Properties
        protected MainViewModel main;

        public string FullName
        {
            get
            {
                return UserController.user?.userData?.fullname;
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
                return UserController.user?.userData?.type.ToString();
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.userData.type.ToString() ?? "N/A");
            }
        }
        public string Time
        {
            get
            {
                string time = DateTime.Now.ToString("MMMM dd    HH:mm");
                return time;
            }
            set
            {
                NotifyOfPropertyChange(() => DateTime.Now.ToString("MMMM dd    HH:mm"));
            }
        }
        #endregion

        public ActionViewModel(MainViewModel _main)
        {
            main = _main;
        }

        #region Content
        public virtual async void SetControl<T>(T _control, bool closeLast)
        {
            await ChangeActiveItemAsync(_control, closeLast);
        }
        #endregion

        #region Button Events
        public virtual void OpenCars()
        {
            SetControl(new CarsViewModel(this), true);
        }

        public virtual void OpenProfile()
        {
            SetControl(new ProfileViewModel(main), true);
        }
        #endregion
    }
}
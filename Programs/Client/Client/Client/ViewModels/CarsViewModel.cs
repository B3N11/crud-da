using Caliburn.Micro;
using CarCRUD.DataModels;
using CarCRUD.Users;
using System.Collections.Generic;

namespace CarCRUD.ViewModels
{
    class CarsViewModel : Screen
    {
        #region Properties
        private MainViewModel main;
        private ActionViewModel action;

        private List<CarFavourite> cars = new List<CarFavourite>();

        public List<CarFavourite> Cars
        {
            get
            {
                return cars;
            }
            set
            {
                cars = UserController.user.responseData.favourites;
                NotifyOfPropertyChange(() => cars);
            }
        }
        #endregion
        public CarsViewModel(MainViewModel _main, ActionViewModel _action)
        {
            main = _main;
            action = _action;
        }

        #region Button Events
        public void NewCar()
        {

        }
        #endregion
    }
}

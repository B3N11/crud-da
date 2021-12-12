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
        private FavouriteCar selectedCar;

        public FavouriteCar SelectedCar
        {
            get
            {
                return selectedCar;
            }
            set
            {
                selectedCar = value;
                NotifyOfPropertyChange(() => selectedCar);
            }
        }
        public List<FavouriteCar> Cars
        {
            get
            {
                return UserController.user.favourites;
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.favourites);
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

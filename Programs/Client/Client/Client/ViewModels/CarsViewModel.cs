using Caliburn.Micro;
using CarCRUD.DataModels;
using CarCRUD.Users;
using System.Collections.Generic;

namespace CarCRUD.ViewModels
{
    class CarsViewModel : Screen
    {
        #region Properties
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
                return UserController.user.generalResponseData.favourites;
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.generalResponseData.favourites);
            }
        }
        #endregion
        public CarsViewModel(ActionViewModel _action)
        {
            action = _action;
        }

        #region Button Events
        public void NewCar()
        {
            action.SetControl(new CareditViewModel(action), true);
        }
        #endregion
    }
}

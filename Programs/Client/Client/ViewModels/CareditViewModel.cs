using Caliburn.Micro;
using CarCRUD.DataModels;
using CarCRUD.Users;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace CarCRUD.ViewModels
{
    class CareditViewModel : Screen
    {        
        #region Properties
        private ActionViewModel action;

        private CarBrand selectedBrand;
        private CarType type;
        private string carTypeName = string.Empty;
        private string color = string.Empty;
        private string year = string.Empty;
        private string fuel = string.Empty;

        public CareditViewModel(ActionViewModel _action)
        {
            action = _action;
        }

        public List<CarBrand> CarBrands
        {
            get
            {
                return UserController.user.generalResponseData.carBrands;
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.generalResponseData.carBrands);
            }
        }
        public List<CarType> CarTypes
        {
            get
            {
                List<CarType> types = null;
                if (SelectedBrand == null) return null;
                try { types = UserController.user.generalResponseData.carTypes.Where(t => t.brandData.ID == selectedBrand.ID).ToList(); }
                catch { }
                return types;
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.generalResponseData.carTypes);
                NotifyOfPropertyChange(() => SelectedBrand);
            }
        }
        public CarBrand SelectedBrand
        {
            get
            {
                return selectedBrand;
            }
            set
            {
                selectedBrand = value;
                NotifyOfPropertyChange(() => selectedBrand);
            }
        }
        public CarType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                NotifyOfPropertyChange(() => type);
            }
        }
        public string CarTypeName
        {
            get
            {
                return carTypeName;
            }
            set
            {
                carTypeName = value;
                NotifyOfPropertyChange(() => carTypeName);
            }
        }
        public string Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
                NotifyOfPropertyChange(() => color);
            }
        }
        public string Year
        {
            get
            {
                return year;
            }
            set
            {
                year = value;
                NotifyOfPropertyChange(() => year);
            }
        }
        public string Fuel
        {
            get
            {
                return fuel;
            }
            set
            {
                fuel = value;
                NotifyOfPropertyChange(() => fuel);
            }
        }
        #endregion

        #region Button Events
        public void Create()
        {
            int yearNumber = 0;
            try { yearNumber = int.Parse(year); }
            catch
            {
                MessageBox.Show("Invalid data!");
                return;
            }

            if(SelectedBrand == null)
            {
                MessageBox.Show("Invalid data!");
                return;
            }

            bool result = UserActionHandler.FavouriteCarCreateRequest(SelectedBrand.ID, Type, CarTypeName, yearNumber, Color, Fuel); ;
            if (!result) MessageBox.Show("Invalid data!");
        }

        public void Cancel()
        {
            action.SetControl(new CarsViewModel(action), true);
        }
        #endregion
    }
}

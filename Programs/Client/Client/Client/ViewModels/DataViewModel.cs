using Caliburn.Micro;
using CarCRUD.Users;

namespace CarCRUD.ViewModels
{
    class DataViewModel : Screen
    {
        #region Properties
        private string brandName = string.Empty;

        public string BrandName
        {
            get
            {
                return brandName;
            }
            set
            {
                brandName = value;
                NotifyOfPropertyChange(() => brandName);
            }
        }
        #endregion

        #region Button Events
        public void Submit()
        {
            if (string.IsNullOrEmpty(BrandName))
                return;

            UserActionHandler.BrancCreateRequest(BrandName);
        }
        #endregion
    }
}

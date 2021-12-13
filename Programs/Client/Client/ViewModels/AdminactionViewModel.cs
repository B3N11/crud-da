namespace CarCRUD.ViewModels
{
    class AdminactionViewModel : ActionViewModel
    {
        #region Properties

        public AdminactionViewModel(MainViewModel _main) : base(_main)
        {
        }
        #endregion

        #region Button Events
        public void OpenUsers()
        {
            SetControl(new UsersViewModel(this), true);
        }

        public void OpenData()
        {
            SetControl(new DataViewModel(), true);
        }

        public void OpenRequests()
        {
            SetControl(new RequestsViewModel(), true);
        }
        #endregion
    }
}
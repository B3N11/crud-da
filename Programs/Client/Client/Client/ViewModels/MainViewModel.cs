using Caliburn.Micro;
using System.Windows;

namespace Client.ViewModels
{
    public class MainViewModel : Conductor<object>
    {
        protected override void OnViewLoaded(object view)
        {
            SetControl(new LoginViewModel());
        }

        public void SetControl<T>(T _control)
        {
            ChangeActiveItemAsync(_control, true);
        }

        #region App Window State
        public void CloseApp()
        {
            Application.Current.Shutdown();
        }

        public void MinimizeApp()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        #endregion
    }
}
using Caliburn.Micro;
using System.Windows;
using System.Threading.Tasks;

namespace CarCRUD.ViewModels
{
    public class MainViewModel : Conductor<object>.Collection.OneActive, IConnectionHandler
    {
        #region Properties
        //Window
        private IWindowManager windowManager;

        //Data
        private string connectionState = "Connecting...";
        public string ConnectionState
        {
            get { return connectionState; }
            set
            {
                connectionState = value;
                NotifyOfPropertyChange(() => connectionState);
            }
        }
        #endregion

        public MainViewModel()
        {
            windowManager = new WindowManager();
        }

        protected override async void OnViewLoaded(object view)
        {
            SetControl(new HomeViewModel(this), true);
            Client.Start(this);
        }

        public async void SetControl<T>(T _control, bool closeLast)
        {
            await ChangeActiveItemAsync(_control, closeLast);
        }

        #region App Window
        public void CloseApp()
        {
            Application.Current.Shutdown();
        }

        public void MinimizeApp()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        public async void ShowWindow(Screen _window)
        {
            await windowManager.ShowWindowAsync(_window);
        }
        #endregion

        #region Event Handlers
        public void ClientConnectionResulted(bool result)
        {
            if (!result) ClientDisconnected();
            else ConnectionState = "Connected";
        }

        public void ClientDisconnected()
        {
            ConnectionState = "Not Connected";
            var dvm = new DisconnectedViewModel(this);
            ShowWindow(dvm);
            SetControl(new HomeViewModel(this), true);
        }        

        public void ClientConnecting()
        {
            ConnectionState = "Connecting...";
        }
        #endregion
    }
}
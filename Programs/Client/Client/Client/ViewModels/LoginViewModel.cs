using Caliburn.Micro;

namespace Client.ViewModels
{
    class LoginViewModel : Screen
    {
        private string connectionState = "Connecting...";

        public string ConnectionState
        {
            get { return connectionState; }
            set { connectionState = value; }
        }
    }
}
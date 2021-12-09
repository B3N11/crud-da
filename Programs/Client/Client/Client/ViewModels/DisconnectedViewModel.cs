using Caliburn.Micro;
using System.Windows;

namespace CarCRUD.ViewModels
{
    public class DisconnectedViewModel : Screen
    {
        public MainViewModel main;

        //Set main instance
        public DisconnectedViewModel(MainViewModel _main) => main = _main;

        public void Reconnect()
        {
            Client.Start(main);
            TryCloseAsync();
        }
    }
}
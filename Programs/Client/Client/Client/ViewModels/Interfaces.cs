namespace CarCRUD.ViewModels
{
    public interface IConnectionHandler
    {
        public void ClientConnectionResulted(bool result);
        public void ClientDisconnected();
        public void ClientConnecting();
    }
}

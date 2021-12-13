using System;
using CarCRUD.ServerHandle;
using CarCRUD.DataModels;
using System.Threading.Tasks;

namespace CarCRUD
{
    class CRUDMain
    {
        static void Main(string[] args)
        {
            CRUDMain main = new CRUDMain();
            main.Start();

            //Dont close program
            Task.Delay(-1).GetAwaiter().GetResult();
        }

        private async void Start()
        {
            Console.Write("Starting server...");
            Server.Start(false);
            Console.WriteLine("Success");

            //Setup server if needed
            await Server.ServerSetup();

            //Accept incoming connections
            Console.WriteLine("Listening...");
            Server.AcceptClients();            
        }
    }
}
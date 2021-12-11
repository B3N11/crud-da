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
            Console.Write("Creating server...");

            /*
             * CUSTOM SERVER SETUP
             * Configure server settings here!
             * 
             * port: The port the server will listen to for incoming connections.
             * key: The key to authenticate the connection clients.
             * clientAuthTimeOut: The time in milliseconds after the server drops unauthenticated connections.
             * 
             */
            ServerSettings settings = new ServerSettings();
            settings.key = "4bC_1z3";
            settings.port = 1989;
            settings.clientAuthTimeOut = 5000;
            settings.loggingEnabled = true;
            /*
             * 
             * CUSTOM SETUP DONE!
             */
            Server.Start(settings, false);
            Console.WriteLine("Success");

            //Setup server if needed
            await Server.ServerSetup();

            //Accept incoming connections
            Console.WriteLine("Listening...");
            Server.AcceptClients();            
        }
    }
}
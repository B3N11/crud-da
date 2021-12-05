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
            Console.Write("Creating server...");
            ServerSettings settings = new ServerSettings();
            settings.key = "4bC_1z3";
            settings.port = 1989;
            settings.clientAuthTimeOut = 5000;
            settings.loggingEnabled = true;
            Console.WriteLine("Success");

            Console.Write("Starting server...");
            Server.Start(settings);
            Console.WriteLine("Success");

            Console.WriteLine("Accepting clients...");
            Server.AcceptClients();

            //Dont close program
            Task.Delay(-1).GetAwaiter().GetResult();
        }
    }
}

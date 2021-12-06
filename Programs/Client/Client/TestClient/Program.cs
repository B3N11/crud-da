using System;
using System.Threading.Tasks;
using CarCRUD.Users;

namespace CarCRUD
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();

            Console.WriteLine("username:");
            string username = Console.ReadLine();
            Console.WriteLine("password:");
            string password = Console.ReadLine();
            Console.WriteLine("fullname:");
            string fullname = Console.ReadLine();

            UserActionHandler.RequestRegistration(username, password, password, fullname);
            Console.WriteLine("Login requested");

            Task.Delay(-1).GetAwaiter().GetResult();
        }

        private void Start()
        {
            Console.WriteLine("Connecting");
            Client.Start();
        }
    }
}

using System;
using System.Threading.Tasks;

namespace CarCRUD
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.Start();

            Task.Delay(-1).GetAwaiter().GetResult();
        }

        private void Start()
        {
            Console.WriteLine("Connecting");
            Client.Start();
        }
    }
}

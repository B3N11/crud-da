using System.Threading.Tasks;
using CarCRUD.Core;
using CarCRUD.Tools;

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
            Displayer.Welcome();
        }
    }
}

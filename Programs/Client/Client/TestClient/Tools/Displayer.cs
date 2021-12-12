using System;

namespace CarCRUD.Tools
{
    class Displayer
    {
        public static void Welcome()
        {
            string text = string.Empty;
            text += "Welcome to CarCRUD!";
            text += "\n";
            text += DateTime.Now.ToString("MMMM dd    HH:mm");
            text += "\n\n";

            Console.WriteLine(text);
        }

        public static void Disconnected()
        {
            string text = string.Empty;
            text += "DISCONNECTED!";
            text += "\n";
            text += "This could have happened because you have used the wrong authentication key for this server\n or a network error" +
                "on one side of the connection.\n";
            text += "Please try again!";
            text += "\n\n";
        }
    }
}

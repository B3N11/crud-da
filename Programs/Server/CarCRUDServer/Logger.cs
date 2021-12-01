using System;
using System.Collections.Generic;

namespace CarCRUD
{
    class Logger
    {
        //Displays current state of a loggable object
        public static void LogState(Loggable _object)
        {
            if (_object == null) return;

            Console.WriteLine($"{DateTime.Now} STATUS UPDATE: Status of {_object.GetID()} is now {_object.GetState()}");
        }
    }

    interface Loggable
    {        
        public string GetID();
        public string GetState();
    }
}

using CarCRUD.DataModels;
using System;
using System.Collections.Generic;

namespace CarCRUD.Tools
{
    class Logger
    {
        //Displays current state of a loggable object
        public static void LogState(ILoggable _object)
        {
            if (_object == null) return;

            Console.WriteLine($"{DateTime.Now} STATUS UPDATE: Status of {_object.GetID()} is now {_object.GetState()}.");
        }

        public static void LogMessage(ILoggable _object, NetMessage _netMessage)
        {
            if (_object == null || _netMessage == null) return;

            Console.WriteLine($"{DateTime.Now} MESSAGE ARRIVED: {_object.GetID()} sent message with type of {_netMessage.type}.");
        }

        public static void LogResponse(ILoggable _object, NetMessage _netMessage)
        {
            if (_object == null || _netMessage == null) return;

            Console.WriteLine($"{DateTime.Now} RESPONSE SENT: Responded to {_object.GetID()} with {_netMessage.type}.");
        }

        public static void LogConnectionState(IEndpointLoggable _object)
        {
            if (_object == null) return;

            Console.WriteLine($"{DateTime.Now} CONNECTION UPDATE: {_object.GetID()} with endpoint {_object.GetEndPoint()} {_object.GetState()}.");
        }
    }

    interface ILoggable
    {        
        public string GetID();
        public string GetState();
    }

    interface IEndpointLoggable : ILoggable
    {
        public string GetEndPoint();
    }
}
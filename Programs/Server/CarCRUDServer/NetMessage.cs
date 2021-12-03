using System;
using CarCRUD;

namespace CarCRUD
{
    namespace Networking
    {
        class NetMessage
        {
            public NetMessageType type;

            /// <summary>
            /// Returns a NetMessage instance from a string based on their type.
            /// </summary>
            /// <param name="_object"></param>
            /// <returns></returns>
            public static NetMessage GetMessage(string _object)
            {
                if (_object == null) return null;

                NetMessage cast = GeneralManager.Deserialize<NetMessage>(_object);

                switch (cast.type)
                {
                    case NetMessageType.KeyAuthentication:
                        return GeneralManager.Deserialize<KeyAuthenticationMessage>(_object);
                }

                return null;
            }
        }

        class KeyAuthenticationMessage : NetMessage
        {
            public string key;
        }

        enum NetMessageType
        {
            KeyAuthentication,
            LoginRequest,
            ReqistrationRequest
        }
    }
}

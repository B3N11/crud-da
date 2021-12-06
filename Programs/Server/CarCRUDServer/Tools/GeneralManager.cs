using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using CarCRUD.Networking;
using CarCRUD.DataModels;
using Newtonsoft.Json;

namespace CarCRUD.Tools
{
    /// <summary>
    /// Class for different non-program specific tasks
    /// </summary>
    class GeneralManager
    {
        #region Properties
        public static int encryptionKey = 7;
        #endregion

        #region Countdown
        /// <summary>
        /// Starts and handles the result of an async countdown
        /// </summary>
        /// <param name="_milliseconds"></param>
        /// <param name="_iterations"></param>
        /// <param name="_cts"></param>
        public static async void CountdownAsync(int _milliseconds, int _iterations, CancellationTokenSource _cts)
        {
            bool result = await Task.Run(() => Countdown(_milliseconds, _iterations, _cts.Token));

            if (!result) return;

            if (_cts.Token.CanBeCanceled)
                _cts.Cancel();
        }

        private static async Task<bool> Countdown(int _milliseconds, int _iterations, CancellationToken token)
        {
            int timePerIteration = _milliseconds / _iterations;
            int tick = _iterations;

            while (!token.IsCancellationRequested && tick > 0)
            {
                tick--;
                await Task.Delay(timePerIteration);
            }

            if (token.IsCancellationRequested) return false;
            return true;
        }
        #endregion

        #region Casting & Serializing
        public static NetClient CastNetClient(object _object)
        {
            NetClient result = null;
            try { result = _object as NetClient; } catch { }

            return result;
        }

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
        #endregion

        #region Serializing
        public static string Serialize<T>(T _object)
        {
            if (_object == null) return null;

            string result = null;
            try { result = JsonConvert.SerializeObject(_object); } catch { }

            return result;
        }

        public static T Deserialize<T>(string _data)
        {
            if (_data == null) return default(T);

            T result = default(T);
            try { result = JsonConvert.DeserializeObject<T>(_data); } catch { }

            return result;
        }
        #endregion

        #region Encryption
        public static string Encrypt(string _data)
        {
            return CeasarEncrypt.Encrypt(_data, true, encryptionKey);
        }

        public static string Decrypt(string _data)
        {
            return CeasarEncrypt.Encrypt(_data, false, encryptionKey);
        }
        #endregion

        #region Hashing
        public static string HashData(string _data)
        {
            if (string.IsNullOrEmpty(_data)) return null;

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(_data));
            string result = Encoding.UTF8.GetString(hash);

            return result;
        }
        #endregion
    }
}

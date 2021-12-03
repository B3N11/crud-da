using System.Threading;
using System.Threading.Tasks;
using CarCRUD.Networking;
using Newtonsoft.Json;

namespace CarCRUD
{
    /// <summary>
    /// Class for different non-program specific tasks
    /// </summary>
    class GeneralManager
    {
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

        public static string Serialize<T>(T _object)
        {
            if (_object == null) return null;

            string result = null;
            try { JsonConvert.SerializeObject(_object); } catch { }

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
    }
}

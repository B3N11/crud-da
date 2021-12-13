using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using CarCRUD.Networking;
using CarCRUD.DataModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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

        #region Data Management
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

            NetMessage result = Deserialize<NetMessage>(_object);

            switch (result.type)
            {
                case NetMessageType.KeyAuthenticationRequest:
                    return Deserialize<KeyAuthenticationRequestMessage>(_object);

                case NetMessageType.RegistrationRequest:
                    return Deserialize<RegistrationRequestMessage>(_object);

                case NetMessageType.LoginRequest:
                    return Deserialize<LoginRequestMessage>(_object);

                case NetMessageType.AdminRegistrationRequest:
                    return Deserialize<AdminRegistrationRequestMessage>(_object);

                case NetMessageType.AdminRegistrationResponse:
                    return Deserialize<AdminRegistrationResponseMessage>(_object);

                case NetMessageType.LoginResponse:
                    return Deserialize<LoginResponseMessage>(_object);

                case NetMessageType.UserRequest:
                    return Deserialize<UserRequestMesssage>(_object);

                case NetMessageType.UserRequestResponse:
                    return Deserialize<UserRequestResponseMessage>(_object);

                case NetMessageType.BrandCreateResponse:
                    return Deserialize<BrandCreateResponseMessage>(_object);

                case NetMessageType.FavouriteCarCreateResponse:
                    return Deserialize<FavouriteCarCreateResponseMessage>(_object);

                case NetMessageType.RequestAnswerResponse:
                    return Deserialize<RequestAnswerResponseMessage>(_object);

                case NetMessageType.UserActivityResetResponse:
                    return Deserialize<UserActivityResetResponseMessage>(_object);

                case NetMessageType.CarDeleteResponse:
                    return Deserialize<CarDeleteResponseMessage>(_object);
            }

            return result;
        }

        /// <summary>
        /// Clears(deep copies) all the elements if a list and returns them as a cleared list. Paramters depend on the IDeepCopyable type you want to get.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_list"></param>
        /// <returns>Returns null if T does not inherit from IDeepCopyable interface.</returns>
        public static List<T> DeepCopyList<T>(List<T> _list, object[] _parameters)
        {
            //If T doesnt iherit from IDeepCopyable
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(typeof(IDeepCopyable<T>).Ge‌​tTypeInfo()))
                return null;

            List<T> result = new List<T>();
            foreach (IDeepCopyable<T> item in _list)
            {
                T cleared = item.DeepCopy(_parameters);
                result.Add(cleared);
            }

            return result;
        }

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

        public static async Task<bool> CreateFullFavouritesAsync(List<FavouriteCar> _favourites, GeneralResponseData _responseData)
        {
            if (_favourites == null || _responseData == null)
                return false;

            bool result = await Task.Run(() => CreateFullFavourites(_favourites, _responseData));

            return result;
        }

        private static async Task<bool> CreateFullFavourites(List<FavouriteCar> _favourites, GeneralResponseData _responseData)
        {
            if (_favourites == null || _responseData == null)
                return false;

            foreach(FavouriteCar current in _favourites)
            {
                CarType type = _responseData.carTypes.First(t => t.ID == current.cartype);
                type.brandData = _responseData.carBrands.First(b => b.ID == type.brand);
                current.carTypeData = type;
            }

            return true;
        }
        #endregion

        #region Hashing
        /// <summary>
        /// Performs Bitpush and Base64 convert on a string
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public static string Encrypt(string _data, bool _encrypt)
        {
            return CeasarEncrypt.Encrypt(_data, _encrypt, encryptionKey);
        }

        /// <summary>
        /// Returnes SHA256 hash of _data;
        /// </summary>
        /// <param name="_data"></param>
        /// <returns></returns>
        public static string HashData(string _data, bool base64 = true)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_data)) return null;

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(_data));
            string result = Base64(hash, true);

            return result;
        }

        /// <summary>
        /// Encodes/decodes _data to/from Base64.
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="encode"></param>
        /// <returns></returns>
        public static string Base64(string _data, bool encode)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_data)) return null;

            string result = string.Empty;

            if (encode) result = Convert.ToBase64String(Encoding.UTF8.GetBytes(_data));
            else result = Encoding.UTF8.GetString(Convert.FromBase64String(_data));

            return result;
        }

        public static string Base64(byte[] _data, bool encode)
        {
            //Check call validity
            if (_data == null) return null;

            string result = string.Empty;

            if (encode) result = Convert.ToBase64String(_data);
            else result = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(_data)));

            return result;
        }

        /// <summary>
        /// Encodes/decodes the data of a user.
        /// </summary>
        /// <param name="_user"></param>
        /// <param name="_encode"></param>
        /// <returns></returns>
        public static UserData EncodeUser(UserData _user, bool _encode)
        {
            if (_user == null) return null;

            UserData result = new UserData();
            result.username = _encode ? HashData(_user.username) : _user.username;
            result.password = _encode ? HashData(_user.password) : _user.password;
            result.fullname = Encrypt(_user.fullname, _encode);
            result.active = _user.active;
            result.passwordAttempts = _user.passwordAttempts;
            result.ID = _user.ID;
            result.type = _user.type;

            return result;
        }
        #endregion

        #region Input Handling
        public static string GetInput(string _message = null, string _condition = null, int _tries = 1)
        {
            string result = null;
            int tries = _tries;
            while (true)
            {
                //If out of tries
                if (tries <= 0) return result;

                //Display message before every input
                if (_message != null)
                    Console.WriteLine(_message);

                //Decrease tries
                tries--;

                //Get Input
                result = Console.ReadLine();

                //If result is empty
                if (string.IsNullOrWhiteSpace(result) || string.IsNullOrEmpty(result))
                    continue;

                //If result needs to match something
                if (_condition != null && result != _condition)
                    continue;

                return result;
            }
        }
        #endregion
    }
}

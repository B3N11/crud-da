using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarCRUD.DataModels
{
    public class UserData : IDeepCopyable<UserData>
    {
        public int ID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string fullname { get; set; }
        public int passwordAttempts { get; set; }
        public UserType type { get; set; }
        public bool active { get; set; }

        public UserData DeepCopy(object[] _params)
        {
            UserData result = (UserData)MemberwiseClone();
            return result;
        }
    }

    [Serializable]
    public class UserRequest : IDeepCopyable<UserRequest>
    {
        public int ID { get; set; }
        public UserRequestType type { get; set; }
        public string brandAttach { get; set; }

        [NotMapped]
        public int user { get; set; }

        [Required]
        public UserData userData { get; set; }

        /// <summary>
        /// Creates a copy of a user without effecting database entry. Required parameters: UserData
        /// </summary>
        /// <param name="_userData"></param>
        /// <returns></returns>
        public UserRequest DeepCopy(object[] _parameters)
        {
            UserRequest result = (UserRequest)MemberwiseClone();
            result.userData = GetFromParameter<UserData>(_parameters[0]);
            result.user = userData.ID;
            return result;
        }

        private T GetFromParameter<T>(object _param)
        {
            T result;
            try { result = (T)_param; }
            catch { result = default(T); }

            return result;
        }
    }

    public class CarBrand : IDeepCopyable<CarBrand>
    {
        public int ID { get; set; }
        public string name { get; set; }

        /// <summary>
        /// Creates a copy of a user without effecting database entry. Required parameters: CarBrand
        /// </summary>
        /// <param name="_userData"></param>
        /// <returns></returns>
        public CarBrand DeepCopy(object[] _parameters)
        {
            CarBrand result = (CarBrand)MemberwiseClone();
            return result;
        }
    }

    public class CarType : IDeepCopyable<CarType>
    {
        public int ID { get; set; }

        [NotMapped]
        public int brand { get; set; }

        [Required]
        public CarBrand brandData { get; set; }

        public string name { get; set; }

        /// <summary>
        /// Creates a copy of a user without effecting database entry. Required parameters: CarBrand
        /// </summary>
        /// <param name="_userData"></param>
        /// <returns></returns>
        public CarType DeepCopy(object[] _parameters)
        {
            CarType result = (CarType)MemberwiseClone();
            result.brandData = GetFromParameter<CarBrand>(_parameters[0]);
            result.brand = brandData.ID;
            return result;
        }

        private T GetFromParameter<T>(object _param)
        {
            T result;
            try { result = (T)_param; }
            catch { result = default(T); }

            return result;
        }
    }

    public class FavouriteCar : IDeepCopyable<FavouriteCar>
    {
        public int ID { get; set; }

        [NotMapped]
        public int cartype { get; set; }

        [Required]
        public CarType carTypeData { get; set; }

        [NotMapped]
        public int user { get; set; }

        [Required]
        public UserData userData { get; set; }

        public int year { get; set; }
        public string color { get; set; }
        public string fuel { get; set; }

        /// <summary>
        /// Creates a copy of a user without effecting database entry. It required parameters: CarType, UserData
        /// </summary>
        /// <returns></returns>
        public FavouriteCar DeepCopy(object[] _parameters)
        {
            FavouriteCar result = (FavouriteCar)MemberwiseClone();
            result.carTypeData = GetFromParameter<CarType>(_parameters[0]);
            result.cartype = carTypeData.ID;

            result.userData = GetFromParameter<UserData>(_parameters[1]);
            result.user = userData.ID;
            return result;
        }

        private T GetFromParameter<T>(object _param)
        {
            T result;
            try { result = (T)_param; }
            catch { result = default(T); }

            return result;
        }
    }

    public class CarImage : IDeepCopyable<CarImage>
    {
        public int ID { get; set; }

        [NotMapped]
        public int favouriteCar { get; set; }

        [Required]
        public FavouriteCar favouriteCarData { get; set; }
        public byte[] image { get; set; }

        /// <summary>
        /// Creates a copy of a user without effecting database entry. Required parameters: FavouriteCar
        /// </summary>
        /// <returns></returns>
        public CarImage DeepCopy(object[] _params)
        {
            CarImage result = (CarImage)MemberwiseClone();

            result.favouriteCarData = GetFromParameter<FavouriteCar>(_params[0]);

            result.favouriteCar = favouriteCarData.ID;
            return result;
        }

        private T GetFromParameter<T>(object _param)
        {
            T result;
            try { result = (T)_param; }
            catch { result = default(T); }

            return result;
        }
    }

    public interface IDeepCopyable<T>
    {
        public T DeepCopy(object[] _params);
    }
}

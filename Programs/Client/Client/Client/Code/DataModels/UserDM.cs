using System.Collections.Generic;

namespace CarCRUD.DataModels
{
    public class UserData
    {
        public int ID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string fullname { get; set; }
        public int passwordAttempts { get; set; }
        public UserType type { get; set; }
        public bool active { get; set; }
    }

    public class UserRequest
    {
        public int ID { get; set; }
        public UserRequestType type { get; set; }
        public string brandAttach { get; set; }

        public int user { get; set; }
        public UserData userData { get; set; }
    }

    public class CarBrand
    {
        public int ID { get; set; }
        public string name { get; set; }
    }

    public class CarType
    {
        public int ID { get; set; }
        public int brand { get; set; }
        public CarBrand brandData { get; set; }
        public string name { get; set; }
    }

    public class CarFavourite
    {
        public int ID { get; set; }
        public int cartype { get; set; }
        public CarType carTypeData { get; set; }
        public int user { get; set; }
        public UserData userData { get; set; }
        public int year { get; set; }
        public string color { get; set; }
        public string fuel { get; set; }
    }

    public class CarImage
    {
        public int ID { get; set; }
        public int favouriteCar { get; set; }
        public CarFavourite favouriteCarData { get; set; }
        public byte[] image { get; set; }
    }
}

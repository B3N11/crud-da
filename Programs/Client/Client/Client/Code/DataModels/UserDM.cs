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
        public bool accountDeleteRequested { get; set; }
    }

    public class UserBrandRequest
    {
        public int ID { get; set; }
        public string brand { get; set; }
        public int userID { get; set; }
        public UserData user { get; set; }
    }

    public class CarBrand
    {
        public int ID { get; set; }
        public string name { get; set; }
    }

    public class CarType
    {
        public int ID { get; set; }
        public int brandID { get; set; }
        public CarBrand brand { get; set; }
        public string name { get; set; }
    }

    public class CarFavourite
    {
        public int ID { get; set; }
        public int cartypeID { get; set; }
        public CarType carType { get; set; }
        public int userID { get; set; }
        public UserData userData { get; set; }
        public int year { get; set; }
        public string color { get; set; }
        public string fuel { get; set; }
    }

    public class CarImage
    {
        public int ID { get; set; }
        public int favouriteCarID { get; set; }
        public CarFavourite favouriteCar { get; set; }
        public byte[] image { get; set; }
    }
}

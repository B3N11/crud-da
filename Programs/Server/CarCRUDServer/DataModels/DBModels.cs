namespace CarCRUD.DataModels
{
    public class UserData
    {
        public string ID { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string fullname { get; set; }
        public UserType type { get; set; }
        public bool active { get; set; }
        public int requestID { get; set; }
    }

    public class UserRequest
    {
        public int ID { get; set; }
        public bool accountRemove { get; set; }
        public bool brandAttach { get; set; }
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
        public string name { get; set; }
    }

    public class CarFavourite
    {
        public int ID { get; set; }
        public int carTypeID { get; set; }
        public int userID { get; set; }
        public int year { get; set; }
        public string color { get; set; }
        public string fuel { get; set; }
    }

    public class CarImage
    {
        public int ID { get; set; }
        public int favCarID { get; set; }
        public byte[] image { get; set; }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarCRUD.DataModels;
using Microsoft.EntityFrameworkCore;

namespace CarCRUD.DataBase
{
    /// <summary>
    /// Handles database interactions. Every
    /// </summary>
    class DBController
    {
        #region Properties
        private static Context database;

        private static bool initialized = false;
        #endregion

        #region DB Creation
        /// <summary>
        /// Must be called first to access database.
        /// </summary>
        /// <param name="_context"></param>
        public static void SetTools(Context _context)
        {
            database = _context;
            initialized = true;
        }

        /// <summary>
        /// Creates database if it doesnt exist.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> Configure()
        {
            bool result = await database.Database.EnsureCreatedAsync();
            return result;
        }
        #endregion

        #region User Query
        /// <summary>
        /// Returns UserData based on username. Asyncronous operation, await for result!
        /// </summary>
        /// <param name="_username"></param>
        /// <returns></returns>
        public static async Task<UserData> GetUserAsync(string _username)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_username) || !initialized) return null;

            UserData result = await Task.Run(() => GetUser(_username));
            return result;
        }

        private static UserData GetUser(string _username)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_username) || !initialized) return null;

            UserData result = null;
            try { result = database.Users.First(u => u.username == _username); } catch { }

            return result;
        }

        /// <summary>
        /// Returns UserData based on user ID. Asyncronous operation, await for result!
        /// </summary>
        /// <param name="_ID"></param>
        /// <returns></returns>
        public static async Task<UserData> GetUserAsync(int _ID)
        {
            UserData user = null;
            user = await Task.Run(() => GetUser(_ID));

            return user;
        }

        private static UserData GetUser(int ID)
        {
            UserData user = null;
            try { user = database.Users.First(u => u.ID == ID); }
            catch { }

            return user;
        }

        /// <summary>
        /// Returnes all users based on type
        /// </summary>
        /// <param name="_userType"></param>
        /// <returns></returns>
        public static async Task<List<UserData>> GetUsersAsync(UserType _userType)
        {
            List<UserData>  result = await Task.Run(() => GetUsers(_userType));

            return result;
        }

        private static List<UserData> GetUsers(UserType _userType)
        {
            List<UserData> result = null;

            try { result = database.Users.Where(u => u.type == _userType).ToList(); }
            catch { }

            return result;
        }

        /// <summary>
        /// Returns all user in database.
        /// </summary>
        /// <returns></returns>
        public static async Task<List<UserData>> GetAllUserAsync()
        {
            List<UserData> result = null;
            result = await Task.Run(() => GetAllUser());

            return result;
        }

        private static List<UserData> GetAllUser()
        {
            List<UserData> result = null;
            try { result = database.Users.Where(u => true).ToList(); }
            catch { }

            return result;
        }
        #endregion

        #region Car Data

        #region Car Brands
        /// <summary>
        /// Returns all the carbrands in the database
        /// </summary>
        /// <returns></returns>
        public static async Task<List<CarBrand>> GetCarBrandsAsync()
        {
            List<CarBrand> result = null;
            result = await Task.Run(() => GetCarBrands());

            return result;
        }

        private static List<CarBrand> GetCarBrands()
        {
            List<CarBrand> result = null;
            try { result = database.CarBrands.Where(c => true).ToList(); }
            catch { }

            return result;
        }

        public static async Task<CarBrand> GetCarBrandAsync(int _ID)
        {
            CarBrand brand = await Task.Run(() => GetCarBrand(_ID));

            return brand;
        }

        private static CarBrand GetCarBrand(int _ID)
        {
            CarBrand brand = null;
            try { brand = database.CarBrands.First(b => b.ID == _ID); }
            catch { }

            return brand;
        }

        public static async Task<bool> CreateBrandAsync(string _brand)
        {
            if (string.IsNullOrEmpty(_brand)) return false;

            CarBrand brand = new CarBrand();
            brand.name = _brand;

            database.CarBrands.Add(brand);

            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }
        #endregion

        #region Car Types
        /// <summary>
        /// Returns all cartypes belonging to a brand. Set parameter to * to get all the types!
        /// </summary>
        /// <param name="_carBrand"></param>
        /// <returns></returns>
        public static async Task<List<CarType>> GetCarTypesAsync(string _carBrand)
        {
            if (string.IsNullOrEmpty(_carBrand)) return null;

            List<CarType> result = null;
            result = await Task.Run(() => GetCarTypes(_carBrand));

            return result;
        }

        private static async Task<List<CarType>> GetCarTypes(string _carBrand)
        {
            List<CarType> result = null;

            //Get all types
            if (_carBrand == "*")
            {
                try { result = await Task.Run(() => database.CarType.Where(c => true).ToList()); }
                catch { }
            }
            //Get types of a brand
            else
            {
                try { result = await Task.Run(() => database.CarType.Where(c => c.brandData.name == _carBrand).ToList()); }
                catch { }
            }

            return result;
        }

        public static async Task<List<CarType>> GetCarTypesAsync(int _brandID)
        {
            List<CarType> result = null;
            result = await Task.Run(() => GetCarTypes(_brandID));

            return result;
        }

        private static async Task<List<CarType>> GetCarTypes(int _brandID)
        {
            List<CarType> result = null;

            //Get all types
            try { result = await Task.Run(() => database.CarType.Where(c => c.brandData.ID == _brandID).ToList()); }
            catch { }

            return result;
        }

        public static async Task<CarType> CreateCarTypeAsync(string _name, int _brandID)
        {
            if (string.IsNullOrEmpty(_name))
                return null;

            CarType result = await Task.Run(() => CreateCarType(_name, _brandID));

            return result;
        }

        private static async Task<CarType> CreateCarType(string _name, int _brandID)
        {
            CarType type = new CarType();
            type.name = _name;
            type.brandData = await GetCarBrandAsync(_brandID);
            if (type.brandData == null) return null;

            database.CarType.Add(type);

            try { await database.SaveChangesAsync(); }
            catch { return null; }
            return type;
        }
        #endregion

        #region Favourite Cars
        /// <summary>
        /// Returns all the cars of a user.
        /// </summary>
        /// <param name="_userID"></param>
        /// <returns></returns>
        public static async Task<List<FavouriteCar>> GetFavouritesAsync(int _userID)
        {
            List<FavouriteCar> result = null;
            result = await Task.Run(() => GetFavourites(_userID));

            return result;
        }

        private static List<FavouriteCar> GetFavourites(int _userID)
        {
            List<FavouriteCar> result = null;
            try { result = database.FavouriteCars.Where(c => c.userData.ID == _userID).ToList(); }
            catch { }

            return result;
        }

        public static async Task<List<FavouriteCar>> GetFavouritesAsync(string _username)
        {
            if (_username == null) return null;

            List<FavouriteCar> result = null;
            result = await Task.Run(() => GetFavourites(_username));

            return result;
        }

        private static async Task<List<FavouriteCar>> GetFavourites(string _username)
        {
            List<FavouriteCar> result = null;
            try { result = database.FavouriteCars.Where(c => c.userData.username == _username).ToList(); }
            catch { }

            return result;
        }

        public static async Task<bool> CreateFavouriteCarAsync(FavouriteCar _favouriteCar)
        {
            if (_favouriteCar == null) return false;

            database.FavouriteCars.Add(_favouriteCar);
            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }
        #endregion

        #endregion

        #region User Request Data
        /// <summary>
        /// Get all requests based on username.
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="_withoutAdditionalData"></param>
        /// <returns></returns>
        public static async Task<List<UserRequest>> GetRequestsAsync(string _username)
        {
            if (string.IsNullOrEmpty(_username)) return null;

            List<UserRequest> result = await Task.Run(() => GetRequests(_username));

            return result;
        }

        private static List<UserRequest> GetRequests(string _username)
        {
            List<UserRequest> result = null;

            if(_username == "*")
                try { result = database.UserRequests.Include(r => r.userData).ToList(); } catch { }
            else try { result = database.UserRequests.Include(r => r.userData).Where(r => r.userData.username == _username).ToList(); } catch { }

            return result;
        }

        /// <summary>
        /// Creates brand attach request for user.
        /// </summary>
        /// <param name="_brand"></param>
        /// <param name="_username"></param>
        /// <returns></returns>
        public static async Task<bool> CreateBrandAttachRequestAsync(string _brand, string _username)
        {
            if (string.IsNullOrEmpty(_brand) || string.IsNullOrEmpty(_username))
                return false;

            UserRequest request = new UserRequest();
            request.type = UserRequestType.BrandAttach;
            request.brandAttach = _brand;

            UserData user = await GetUserAsync(_username);
            if (user == null) return false;
            request.userData = user;

            database.UserRequests.Add(request);
            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Creates account request for user.
        /// </summary>
        /// <param name="_username"></param>
        /// <returns></returns>
        public static async Task<bool> CreateAccountDeleteRequestAsync(string _username)
        {
            if (string.IsNullOrEmpty(_username))
                return false;

            UserRequest request = new UserRequest();
            request.type = UserRequestType.AccountDelete;

            UserData user = await GetUserAsync(_username);
            if (user == null) return false;
            request.userData = user;

            database.UserRequests.Add(request);
            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }
        #endregion

        #region Modifiy User Data
        /// <summary>
        /// Creates a new user in the Users table and a new request in UserRequests table.
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static async Task<bool> CreateUserAsync(UserData _user)
        {
            //Check call validtiy
            if (_user == null || !initialized) return false;

            bool result = await Task.Run(() => CreateUser(_user));

            return result;
        }

        private static async Task<bool> CreateUser(UserData _user)
        {
            if (_user == null) return false;

            try { database.Users.Add(_user); }
            catch { return false; }

            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }

        public static async Task<bool> SetUserDataAsync(UserData _userData, int _ID)
        {
            if (_userData == null) return false;

            UserData user = await GetUserAsync(_ID);
            if (user == null) return false;

            user = _userData;
            user.ID = _ID;

            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Deletes a user and every associated data from the database.
        /// </summary>
        /// <param name="_username"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteUser(string _username)
        {
            if (string.IsNullOrEmpty(_username)) return false;

            UserData user = await GetUserAsync(_username);
            if (user == null) return false;

            //Delete Favourite Cars
            List<FavouriteCar> cars = await GetFavouritesAsync(_username);
            if(cars != null) cars.ForEach(c => database.FavouriteCars.Remove(c));

            //Delete Requests
            List<UserRequest> requests = await GetRequestsAsync(_username);
            if (requests != null) requests.ForEach(r => database.UserRequests.Remove(r));

            //Delete user
            database.Users.Remove(user);

            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }
        #endregion
    }
}
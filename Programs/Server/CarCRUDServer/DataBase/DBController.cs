using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarCRUD.DataModels;
using Microsoft.EntityFrameworkCore;

namespace CarCRUD.DataBase
{
    /// <summary>
    /// DBController is the only module that interacts with database directly with the help of EntityFramework.
    /// 
    /// Every text based data stored in the database is either hashed or encrypted using a custom encrypt tool (See: CeasarEncrypter.cs)
    /// 
    /// DBController assumes, that every data passed to its methods are in proper format, thus it won't modify it. (Data needs to be encrypted/hashed even fro query.)
    /// 
    /// Most of the public functions are async due to the longer database operations. They are usually just a wrapper around their sync pair.
    /// </summary>
    class DBController
    {
        #region Variables
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
        /// <param name="_hashedUsername"></param>
        /// <returns></returns>
        public static async Task<UserData> GetUserAsync(string _hashedUsername)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_hashedUsername) || !initialized) return null;

            UserData result = await Task.Run(() => GetUser(_hashedUsername));
            return result;
        }

        private static UserData GetUser(string _hashedUsername)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_hashedUsername) || !initialized) return null;

            UserData result = null;
            try { result = database.Users.First(u => u.username == _hashedUsername); } catch { }

            return result;
        }

        /// <summary>
        /// Returns UserData based on user ID. Asyncronous operation, await for result!
        /// </summary>
        /// <param name="_ID"></param>
        /// <returns></returns>
        public static async Task<UserData> GetUserAsync(int _ID)
        {
            if (!initialized) return null;

            UserData user = null;
            user = await Task.Run(() => GetUser(_ID));

            return user;
        }

        private static UserData GetUser(int ID)
        {
            if (!initialized) return null;

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
            if (!initialized) return null;

            List<UserData> result = await Task.Run(() => GetUsers(_userType));

            return result;
        }

        private static List<UserData> GetUsers(UserType _userType)
        {
            if (!initialized) return null;

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
            if (!initialized) return null;

            List<UserData> result = null;
            result = await Task.Run(() => GetAllUser());

            return result;
        }

        private static List<UserData> GetAllUser()
        {
            if (!initialized) return null;

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
            if (!initialized) return null;

            List<CarBrand> result = null;
            result = await Task.Run(() => GetCarBrands());

            return result;
        }

        private static List<CarBrand> GetCarBrands()
        {
            if (!initialized) return null;

            List<CarBrand> result = null;
            try { result = database.CarBrands.Where(c => true).ToList(); }
            catch { }

            return result;
        }

        /// <summary>
        /// Returns a CarBrand based on its ID.
        /// </summary>
        /// <param name="_ID"></param>
        /// <returns></returns>
        public static async Task<CarBrand> GetCarBrandAsync(int _ID)
        {
            if (!initialized) return null;

            CarBrand brand = await Task.Run(() => GetCarBrand(_ID));

            return brand;
        }

        private static CarBrand GetCarBrand(int _ID)
        {
            if (!initialized) return null;

            CarBrand brand = null;
            try { brand = database.CarBrands.First(b => b.ID == _ID); }
            catch { }

            return brand;
        }

        /// <summary>
        /// Creates a CarBrand in database.
        /// </summary>
        /// <param name="_encryptedBrandName"></param>
        /// <returns></returns>
        public static async Task<CarBrand> CreateBrandAsync(string _encryptedBrandName)
        {
            if (string.IsNullOrEmpty(_encryptedBrandName) || !initialized)
                return null;

            bool exists = database.CarBrands.Any(b => b.name == _encryptedBrandName);
            if (exists) return null;

            CarBrand newBrand = new CarBrand();
            newBrand.name = _encryptedBrandName;

            database.CarBrands.Add(newBrand);

            try { await database.SaveChangesAsync(); }
            catch { return null; }
            return newBrand;
        }
        #endregion

        #region Car Types
        /// <summary>
        /// Returns all cartypes belonging to a brand. Set parameter to * to get all the types!
        /// </summary>
        /// <param name="_encryptedBrandName"></param>
        /// <returns></returns>
        public static async Task<List<CarType>> GetCarTypesAsync(string _encryptedBrandName, bool _withoutAdditionalData = true)
        {
            if (string.IsNullOrEmpty(_encryptedBrandName) || !initialized) return null;

            List<CarType> result = null;
            result = await Task.Run(() => GetCarTypes(_encryptedBrandName, _withoutAdditionalData));

            return result;
        }

        private static List<CarType> GetCarTypes(string _encryptedBrandName, bool _withoutAdditionalData = true)
        {
            if (string.IsNullOrEmpty(_encryptedBrandName) || !initialized) return null;

            List<CarType> result = null;

            //Get all types
            if (_encryptedBrandName == "*")
            {
                if(_withoutAdditionalData)
                    try { result = database.CarType.Where(t => true).ToList(); } catch { }

                else try { result = database.CarType.Include(t => t.brandData).Where(t => true).ToList(); } catch { }
            }
            //Get types of a brand
            else
            {
                if(_withoutAdditionalData)
                    try { result = database.CarType.Where(c => c.brandData.name == _encryptedBrandName).ToList(); } catch { }
                else try { result = database.CarType.Include(t => t.brandData).Where(c => c.brandData.name == _encryptedBrandName).ToList(); } catch { }
            }

            return result;
        }

        /// <summary>
        /// Returns CarTypes of a brand.
        /// </summary>
        /// <param name="_brandID"></param>
        /// <returns></returns>
        public static async Task<List<CarType>> GetCarTypesAsync(int _brandID, bool _withoutAdditionalData = true)
        {
            if (!initialized) return null;

            List<CarType> result = null;
            result = await Task.Run(() => GetCarTypes(_brandID, _withoutAdditionalData));

            return result;
        }

        private static List<CarType> GetCarTypes(int _brandID, bool _withoutAdditionalData = true)
        {
            if (!initialized) return null;

            List<CarType> result = null;

            //Get all types
            if(_withoutAdditionalData)
                try { result = database.CarType.Where(c => c.brandData.ID == _brandID).ToList(); } catch { }
            else try { result = database.CarType.Include(c => c.brandData).Where(c => c.brandData.ID == _brandID).ToList(); } catch { }

            return result;
        }

        /// <summary>
        /// Create CarType in database linked to a CarBrand
        /// </summary>
        /// <param name="_encryptedTypeName"></param>
        /// <param name="_brandID"></param>
        /// <returns></returns>
        public static async Task<CarType> CreateCarTypeAsync(string _encryptedTypeName, int _brandID)
        {
            if (string.IsNullOrEmpty(_encryptedTypeName) || !initialized)
                return null;

            CarType result = await Task.Run(() => CreateCarType(_encryptedTypeName, _brandID));

            return result;
        }

        private static async Task<CarType> CreateCarType(string _encryptedTypeName, int _brandID)
        {
            if (string.IsNullOrEmpty(_encryptedTypeName) || !initialized)
                return null;

            CarType type = new CarType();
            type.name = _encryptedTypeName;
            type.brandData = await GetCarBrandAsync(_brandID);

            //Check if brand exists
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
        public static async Task<List<FavouriteCar>> GetFavouritesAsync(int _userID, bool _withoutAdditionalData = true)
        {
            if (!initialized) return null;

            List<FavouriteCar> result = null;
            result = await Task.Run(() => GetFavourites(_userID, _withoutAdditionalData));

            return result;
        }

        private static List<FavouriteCar> GetFavourites(int _userID, bool _withoutAdditionalData = true)
        {
            if (!initialized) return null;

            List<FavouriteCar> result = null;

            if(_withoutAdditionalData)
                try { result = database.FavouriteCars.Where(c => c.userData.ID == _userID).ToList(); } catch { }
            
            else try { result = database.FavouriteCars.Include(c => c.userData).Include(c => c.carTypeData.brandData).Where(c => c.userData.ID == _userID).ToList(); } catch { }

            return result;
        }

        public static async Task<List<FavouriteCar>> GetFavouritesAsync(string _hashedUsername, bool _withoutAdditionalData = true)
        {
            if (_hashedUsername == null || ! initialized) return null;

            List<FavouriteCar> result = null;
            result = await Task.Run(() => GetFavourites(_hashedUsername, _withoutAdditionalData));

            return result;
        }

        private static List<FavouriteCar> GetFavourites(string _hashedUsername, bool _withoutAdditionalData = true)
        {
            if (_hashedUsername == null || !initialized) return null;

            List<FavouriteCar> result = null;
            if(_withoutAdditionalData)
                try { result = database.FavouriteCars.Where(c => c.userData.username == _hashedUsername).ToList(); } catch { }
            else try { result = database.FavouriteCars.Include(c => c.userData).Include(c => c.carTypeData).Where(c => c.userData.username == _hashedUsername).ToList(); } catch { }

            return result;
        }

        /// <summary>
        /// Creates a new FavouriteCar in database.
        /// </summary>
        /// <param name="_favouriteCar"></param>
        /// <returns></returns>
        public static async Task<bool> CreateFavouriteCarAsync(FavouriteCar _favouriteCar)
        {
            if (_favouriteCar == null || !initialized) return false;

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
        /// <param name="_hashedUsername"></param>
        /// <param name="_withoutAdditionalData"></param>
        /// <returns></returns>
        public static async Task<List<UserRequest>> GetRequestsAsync(string _hashedUsername, bool _withoutAdditionalData = true)
        {
            if (string.IsNullOrEmpty(_hashedUsername) || !initialized) return null;

            List<UserRequest> result = await Task.Run(() => GetRequests(_hashedUsername, _withoutAdditionalData));

            return result;
        }

        private static List<UserRequest> GetRequests(string _hashedUsername, bool _withoutAdditionalData = true)
        {
            List<UserRequest> result = null;

            if(_hashedUsername == "*")
            {
                if(_withoutAdditionalData)
                    try { result = database.UserRequests.Where(r => true).ToList(); } catch { }
                else try { result = database.UserRequests.Include(r => r.userData).Where(r => true).ToList(); } catch { }
            }
            else
            {
                if (_withoutAdditionalData)
                    try { result = database.UserRequests.Where(r => r.userData.username == _hashedUsername).ToList(); } catch { }
                else try { result = database.UserRequests.Include(r => r.userData).Where(r => r.userData.username == _hashedUsername).ToList(); } catch { }
            }

            return result;
        }

        public static async Task<UserRequest> GetRequestAsync(int _requestID, bool _withoutAdditionalData = true)
        {
            if (!initialized) return null;

            UserRequest result = await Task.Run(() => GetRequest(_requestID, _withoutAdditionalData));

            return result;
        }

        private static UserRequest GetRequest(int _requestID, bool _withoutAdditionalData = true)
        {
            if (!initialized) return null;

            UserRequest result = null;

            if (_withoutAdditionalData)
                try { result = database.UserRequests.First(r => r.ID == _requestID); } catch { }
            else try { result = database.UserRequests.Include(r => r.userData).First(r => r.ID == _requestID); } catch { }

            return result;
        }

        /// <summary>
        /// Deletes request from database
        /// </summary>
        /// <param name="_requestID"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteRequest(int _requestID)
        {
            if (!initialized)
                return false;

            UserRequest request = null;
            try { request = GetRequest(_requestID); } catch { }
            if (request == null) return false;

            database.UserRequests.Remove(request);

            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Deletes request from database
        /// </summary>
        /// <param name="_requestID"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteRequest(UserRequest _request)
        {
            if (_request == null || !initialized)
                return false;

            try { database.UserRequests.Remove(_request); }
            catch { return false; }

            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Creates brand attach request for user.
        /// </summary>
        /// <param name="_brand"></param>
        /// <param name="_username"></param>
        /// <returns></returns>
        public static async Task<bool> CreateBrandAttachRequestAsync(string _brand, string _username)
        {
            if (string.IsNullOrEmpty(_brand) || string.IsNullOrEmpty(_username) || !initialized)
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
            if (string.IsNullOrEmpty(_username) || !initialized)
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
            if (_user == null || !initialized) return false;

            try { database.Users.Add(_user); }
            catch { return false; }

            try { await database.SaveChangesAsync(); }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Replaces a users data with new UserData. The ID needs to be the one's you wish to replace.
        /// </summary>
        /// <param name="_userData"></param>
        /// <param name="_ID"></param>
        /// <returns></returns>
        public static async Task<bool> SetUserDataAsync(UserData _userData, int _ID)
        {
            if (_userData == null || !initialized) return false;

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
            if (string.IsNullOrEmpty(_username) || !initialized) return false;

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
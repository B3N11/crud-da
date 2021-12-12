using System;
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
        /// Returns UserData based on _username. Asyncronous operation, await for result!
        /// </summary>
        /// <param name="_username"></param>
        /// <returns></returns>
        public static async Task<UserData> GetUserByUsernameAsync(string _username)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_username) || !initialized) return null;

            UserData result = await Task.Run(() => GetUserByUsername(_username));
            return result;
        }

        private static UserData GetUserByUsername(string _username)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_username) || !initialized) return null;

            UserData result = null;
            try { result = database.Users.First(u => u.username == _username); } catch { }

            return result;
        }

        /// <summary>
        /// Returnes all users based on type
        /// </summary>
        /// <param name="_userType"></param>
        /// <returns></returns>
        public static async Task<List<UserData>> GetUsersByTypeAsync(UserType _userType)
        {
            List<UserData>  result = await Task.Run(() => GetUserByType(_userType));

            return result;
        }

        private static List<UserData> GetUserByType(UserType _userType)
        {
            List<UserData> result = null;

            try { result = database.Users.Where(u => u.type == _userType).ToList(); }
            catch { }

            return result;
        }

        /// <summary>
        /// Returns all user in database
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

        public static async Task<UserData> GetUserByIDAsync(int _ID)
        {
            UserData user = null;
            user = await Task.Run(() => GetUserByID(_ID));

            return user;
        }

        private static async Task<UserData> GetUserByID(int ID)
        {
            UserData user = null;
            try { user = database.Users.First(u => u.ID == ID); }
            catch { }

            return user;
        }
        #endregion

        #region Car Query

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
        #endregion

        #region Favourite Cars
        /// <summary>
        /// Returns all the cars of a user.
        /// </summary>
        /// <param name="_userID"></param>
        /// <returns></returns>
        public static async Task<List<CarFavourite>> GetFavouritesAsync(int _userID)
        {
            if (_userID < 1) return null;

            List<CarFavourite> result = null;
            result = await Task.Run(() => GetFavourites(_userID));

            return result;
        }

        private static async Task<List<CarFavourite>> GetFavourites(int _userID)
        {
            List<CarFavourite> result = null;
            try { result = database.FavouriteCars.Where(c => c.userData.ID == _userID).ToList(); }
            catch { }

            return result;
        }
        #endregion

        #endregion

        #region Request Query
        /// <summary>
        /// Get all requests based on username.
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="_withoutAdditionalData"></param>
        /// <returns></returns>
        public static async Task<List<UserRequest>> GetRequestsByUsernameAsync(string _username)
        {
            List<UserRequest> result = null;
            result = await Task.Run(() => GerRequestsByUsername(_username));

            return result;
        }

        private static List<UserRequest> GerRequestsByUsername(string _username)
        {
            List<UserRequest> result = null;

            if(_username == "*")
                try { result = database.UserRequests.Include(r => r.userData).ToList(); } catch { }
            else try { result = database.UserRequests.Include(r => r.userData).Where(r => r.userData.username == _username).ToList(); } catch { }

            return result;
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
            try { database.Users.Add(_user); }
            catch { return false; }

            await database.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> SetUserDataAsync(UserData _userData, int _ID)
        {
            if (_userData == null) return false;

            UserData user = null;
            user = await GetUserByIDAsync(_ID);
            if (user == null) return false;

            user = _userData;
            user.ID = _ID;

            await database.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> CreateAccountDeleteRequestAsync(string _username)
        {
            UserRequest request = new UserRequest();
            request.type = UserRequestType.AccountDelete;
            request.userData = await GetUserByUsernameAsync(_username);

            database.UserRequests.Add(request);
            await database.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> CreateBrandAttachRequestAsync(string _brand, string _username)
        {
            UserRequest request = new UserRequest();
            request.type = UserRequestType.BrandAttach;
            request.brandAttach = _brand;
            request.userData = await GetUserByUsernameAsync(_username);

            database.UserRequests.Add(request);
            await database.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
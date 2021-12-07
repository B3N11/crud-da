using System;
using System.Linq;
using System.Threading.Tasks;
using CarCRUD.DataModels;

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

        public static void SetTools(Context _context)
        {
            database = _context;
            initialized = true;
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
        #endregion

        #region Modifiy User Data
        /// <summary>
        /// Creates a new user in the Users table and a new request in UserRequests table.
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static async Task<bool> CreateUser(UserData _user)
        {
            //Check call validtiy
            if (_user == null || !initialized) return false;

            await Task.Run(() => database.Users.Add(_user));

            await database.SaveChangesAsync();
            return true;
        }

        public static async Task<bool> SetUserData(UserData _userData, int ID)
        {
            if (_userData == null) return false;

            UserData user = null;
            try { user = await Task.Run(() => database.Users.First(u => u.ID == ID)); } catch { return false; }
            user = _userData;
            user.ID = ID;

            await database.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Create Data
        public static async Task<UserRequestResult> CreateCarBrandRequestAsync(string _message, UserData _user)
        {
            if (_message == null) return UserRequestResult.Fail;

            string carBrand = null;

            //Check in requests
            try { carBrand = await Task.Run(() => database.UserRequests.First(u => u.brand == _message).brand); }
            //Check in already existing brands
            catch { try { carBrand = await Task.Run(() => database.CarBrands.First(u => u.name == _message).name); } catch { } }

            //If there was a match
            if (carBrand != null) return UserRequestResult.CarPropertyAlreadyRequestedOrExists;

            UserBrandRequest request = new UserBrandRequest();
            request.brand = _message;
            request.user = _user;

            database.UserRequests.Add(request);
            await database.SaveChangesAsync();
            return UserRequestResult.Success;
        }
        #endregion
    }
}

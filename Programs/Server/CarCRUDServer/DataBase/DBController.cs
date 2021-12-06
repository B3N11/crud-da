using System;
using System.Linq;
using System.Threading.Tasks;
using CarCRUD.DataModels;
using CarCRUD.Tools;

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

            await Task.Run(() => {
                database.Users.Add(_user);
                database.UserRequests.Add(_user.request);
                });

            await database.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Increases or resets login try incrementer of a user.
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="reset"></param>
        public static async Task<bool> SetLoginTryAsync(string _username, bool reset = false)
        {
            //Check call validitiy
            if (string.IsNullOrEmpty(_username) || !initialized) return false;

            await Task.Run(() => SetLoginTry(_username, reset));
            return true;
        }

        private static async Task<bool> SetLoginTry(string _username, bool reset = false)
        {
            //Check call validitiy
            if (string.IsNullOrEmpty(_username) || !initialized) return false;

            //Get user
            UserData user = null;
            try { user = database.Users.First(u => u.username == _username); } catch { return false; }

            //Set tries. If reset, set it to zero
            if (!reset)
                user.passwordAttempts = Math.Clamp(++user.passwordAttempts, 0, 5);
            else user.passwordAttempts = 0;

            //If password attempts reached 5
            if (user.passwordAttempts == 5)
            {
                await LockUserAsync(user);
                return true;
            }

            //Save
            await database.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Sets the user's account to inactive
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static async Task<bool> LockUserAsync(string _user)
        {
            //Check call validitiy
            if (_user == null || !initialized) return false;

            bool result = await Task.Run(() => LockUser(_user));
            return result;
        }

        /// <summary>
        /// Sets the user's account to inactive
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public static async Task<bool> LockUserAsync(UserData _user)
        {
            //Check call validitiy
            if (_user == null || !initialized) return false;

            bool result = await Task.Run(() => LockUser(_user));
            return result;
        }

        private static bool LockUser(string _username)
        {
            //Check call validitiy
            if (string.IsNullOrEmpty(_username) || !initialized) return false;

            UserData user = null;
            try { user = database.Users.First(u => u.username == _username); } catch { return false; }
            user.active = false;

            database.SaveChangesAsync();
            return true;
        }

        private static async Task<bool> LockUser(UserData _user)
        {
            //Check call validitiy
            if (_user == null || !initialized) return false;

            //Lock
            _user.active = false;

            //Save
            await database.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}

﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarCRUD.DataModels;
using CarCRUD.Tools;

namespace CarCRUD.Users
{
    public class UserActionHandler
    {
        #region Connection & Login
        public static bool RequestLogin(string _username, string _password)
        {
            //Check call validity
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password)) return false;

            LoginRequestMessage message = new LoginRequestMessage();
            message.type = NetMessageType.LoginRequest;
            message.username = _username;
            message.password = _password;

            UserController.Send(message);
            return true;
        }

        /// <summary>
        /// Sends registration request to server.
        /// </summary>
        /// <param name="_username"></param>
        /// <param name="_passwordFirst"></param>
        /// <param name="_passwordSecond"></param>
        /// <param name="_fullname"></param>
        /// <returns></returns>
        public static bool RequestRegistration(string _username, string _passwordFirst, string _passwordSecond, string _fullname)
        {
            if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_passwordFirst) || string.IsNullOrEmpty(_passwordSecond) || string.IsNullOrEmpty(_fullname))
                return false;

            RegistrationRequestMessage message = new RegistrationRequestMessage();
            message.type = NetMessageType.ReqistrationRequest;
            message.username = _username;
            message.passwordFirst = _passwordFirst;
            message.passwordSecond = _passwordSecond;
            message.fullname = _fullname;

            UserController.Send(message);
            return true;
        }

        /// <summary>
        /// Sends logout indicator message
        /// </summary>
        public static void Logout()
        {
            NetMessage message = new NetMessage();
            message.type = NetMessageType.Logout;

            UserController.Send(message);
        }
        #endregion

        #region Requests
        public static void AccountDeleteRequest()
        {
            UserController.Send(new AccountDeleteRequestMessage());
        }

        public static void CarBrandRequest(string _brand)
        {
            if (string.IsNullOrEmpty(_brand)) return;

            CarBrandAddRequestMessage message = new CarBrandAddRequestMessage();
            message.brandName = _brand;

            UserController.Send(message);
        }
        #endregion
    }
}
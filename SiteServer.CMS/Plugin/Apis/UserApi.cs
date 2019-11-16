﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiteServer.CMS.Context;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Model;
using SiteServer.CMS.Provider;
using SiteServer.Plugin;

namespace SiteServer.CMS.Plugin.Apis
{
    public class UserApi : IUserApi
    {
        private UserApi() { }

        private static UserApi _instance;
        public static UserApi Instance => _instance ?? (_instance = new UserApi());

        public IUser NewInstance()
        {
            return new User();
        }

        public async Task<IUser> GetUserByUserIdAsync(int userId)
        {
            return await UserManager.GetUserByUserIdAsync(userId);
        }

        public async Task<IUser> GetUserByUserNameAsync(string userName)
        {
            return await UserManager.GetUserByUserNameAsync(userName);
        }

        public async Task<IUser> GetUserByEmailAsync(string email)
        {
            return await UserManager.GetUserByEmailAsync(email);
        }

        public async Task<IUser> GetUserByMobileAsync(string mobile)
        {
            return await UserManager.GetUserByMobileAsync(mobile);
        }

        public async Task<IUser> GetUserByAccountAsync(string account)
        {
            return await UserManager.GetUserByAccountAsync(account);
        }

        public async Task<bool> IsUserNameExistsAsync(string userName)
        {
            return await DataProvider.UserDao.IsUserNameExistsAsync(userName);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await DataProvider.UserDao.IsEmailExistsAsync(email);
        }

        public async Task<bool> IsMobileExistsAsync(string mobile)
        {
            return await DataProvider.UserDao.IsMobileExistsAsync(mobile);
        }

        public async Task<(bool Valid, string ErrorMessage)> InsertAsync(IUser user, string password)
        {
            var valid = await DataProvider.UserDao.InsertAsync(user as User, password, PageUtils.GetIpAddress());
            return (valid.UserId > 0, valid.ErrorMessage);
        }

        public async Task<(bool Valid, string UserName, string ErrorMessage)> ValidateAsync(string account, string password)
        {
            var valid = await DataProvider.UserDao.ValidateAsync(account, password, false);
            return (valid.User != null, valid.UserName, valid.ErrorMessage);
        }

        public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(string userName, string password)
        {
            var valid = await DataProvider.UserDao.ChangePasswordAsync(userName, password);
            return (valid.IsValid, valid.ErrorMessage);
        }

        public async Task UpdateAsync(IUser userInfo)
        {
            await DataProvider.UserDao.UpdateAsync(userInfo as User);
        }

        public async Task<(bool Valid, string ErrorMessage)> IsPasswordCorrectAsync(string password)
        {
            return await UserDao.IsPasswordCorrectAsync(password);
        }

        public async Task AddLogAsync(string userName, string action, string summary)
        {
            await LogUtils.AddUserLogAsync(userName, action, summary);
        }

        public async Task<IEnumerable<ILog>> GetLogsAsync(string userName, int totalNum, string action = "")
        {
            return await DataProvider.UserLogDao.ListAsync(userName, totalNum, action);
        }

        public string GetAccessToken(int userId, string userName, TimeSpan expiresAt)
        {
            return AuthenticatedRequest.GetAccessToken(userId, userName, expiresAt);
        }

        public string GetAccessToken(int userId, string userName, DateTime expiresAt)
        {
            return AuthenticatedRequest.GetAccessToken(userId, userName, expiresAt);
        }

        public IAccessToken ParseAccessToken(string accessToken)
        {
            return AuthenticatedRequest.ParseAccessToken(accessToken);
        }
    }
}

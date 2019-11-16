﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datory;
using SiteServer.Utils;
using SiteServer.CMS.Model;
using SiteServer.CMS.Plugin.Impl;

namespace SiteServer.CMS.Provider
{
    public class SitePermissionsDao : IRepository
    {
        private readonly Repository<SitePermissions> _repository;

        public SitePermissionsDao()
        {
            _repository = new Repository<SitePermissions>(new Database(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString));
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        public async Task InsertAsync(SitePermissions permissions)
        {
            await _repository.InsertAsync(permissions);
        }

        public async Task DeleteAsync(string roleName)
        {
            await _repository.DeleteAsync(Q.Where(nameof(SitePermissions.RoleName), roleName));
        }

        public async Task<List<SitePermissions>> GetSystemPermissionsListAsync(string roleName)
        {
            var permissionsList = await _repository.GetAllAsync(Q.Where(nameof(SitePermissions.RoleName), roleName));

            return permissionsList.ToList();
        }

        public async Task<SitePermissions> GetSystemPermissionsAsync(string roleName, int siteId)
        {
            return await _repository.GetAsync(Q
                .Where(nameof(SitePermissions.RoleName), roleName)
                .Where(nameof(SitePermissions.SiteId), siteId)
            );
        }

        public async Task<Dictionary<int, List<string>>> GetWebsitePermissionSortedListAsync(IEnumerable<string> roles)
        {
            var sortedList = new Dictionary<int, List<string>>();
            if (roles == null) return sortedList;

            foreach (var roleName in roles)
            {
                var systemPermissionsList = await GetSystemPermissionsListAsync(roleName);
                foreach (var systemPermissions in systemPermissionsList)
                {
                    var list = new List<string>();
                    foreach (var websitePermission in systemPermissions.WebsitePermissionList)
                    {
                        if (!list.Contains(websitePermission)) list.Add(websitePermission);
                    }
                    sortedList[systemPermissions.SiteId] = list;
                }
            }

            return sortedList;
        }

        public async Task<Dictionary<string, List<string>>> GetChannelPermissionSortedListAsync(IList<string> roles)
        {
            var dict = new Dictionary<string, List<string>>();
            if (roles == null) return dict;

            foreach (var roleName in roles)
            {
                var systemPermissionsList = await GetSystemPermissionsListAsync(roleName);
                foreach (var systemPermissions in systemPermissionsList)
                {
                    foreach (var channelId in systemPermissions.ChannelIdList)
                    {
                        var key = PermissionsImpl.GetChannelPermissionDictKey(systemPermissions.SiteId, channelId);

                        if (!dict.TryGetValue(key, out var list))
                        {
                            list = new List<string>();
                            dict[key] = list;
                        }

                        foreach (var channelPermission in systemPermissions.ChannelPermissionList)
                        {
                            if (!list.Contains(channelPermission)) list.Add(channelPermission);
                        }
                    }
                }
            }

            return dict;
        }

        public async Task<List<string>> GetChannelPermissionListIgnoreChannelIdAsync(IList<string> roles)
        {
            var list = new List<string>();
            if (roles == null) return list;

            foreach (var roleName in roles)
            {
                var systemPermissionsList = await GetSystemPermissionsListAsync(roleName);
                foreach (var systemPermissions in systemPermissionsList)
                {
                    foreach (var channelPermission in systemPermissions.ChannelPermissionList)
                    {
                        if (!list.Contains(channelPermission))
                        {
                            list.Add(channelPermission);
                        }
                    }
                }
            }

            return list;
        }
    }
}

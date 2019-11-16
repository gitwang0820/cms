﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiteServer.CMS.Model;
using SiteServer.CMS.Plugin.Impl;
using SiteServer.Plugin;
using SiteServer.Utils;

namespace SiteServer.CMS.Plugin
{
    public static class PluginContentManager
    {
        public static async Task<List<IMetadata>> GetContentModelPluginsAsync()
        {
            var list = new List<IMetadata>();

            foreach (var service in await PluginManager.GetServicesAsync())
            {
                if (PluginContentTableManager.IsContentTable(service))
                {
                    list.Add(service.Metadata);
                }
            }

            return list;
        }

        public static async Task<List<string>> GetContentTableNameListAsync()
        {
            var list = new List<string>();

            foreach (var service in await PluginManager.GetServicesAsync())
            {
                if (PluginContentTableManager.IsContentTable(service))
                {
                    if (!StringUtils.ContainsIgnoreCase(list, service.ContentTableName))
                    {
                        list.Add(service.ContentTableName);
                    }
                }
            }

            return list;
        }

        public static async Task<List<IMetadata>> GetAllContentRelatedPluginsAsync(bool includeContentTable)
        {
            var list = new List<IMetadata>();

            foreach (var service in await PluginManager.GetServicesAsync())
            {
                var isContentModel = PluginContentTableManager.IsContentTable(service);

                if (!includeContentTable && isContentModel) continue;

                if (isContentModel)
                {
                    list.Add(service.Metadata);
                }
                else if (service.ContentMenuFuncs != null)
                {
                    list.Add(service.Metadata);
                }
                else if (service.ContentColumns != null && service.ContentColumns.Count > 0)
                {
                    list.Add(service.Metadata);
                }
            }

            return list;
        }

        public static async Task<List<ServiceImpl>> GetContentPluginsAsync(Channel channel, bool includeContentTable)
        {
            var list = new List<ServiceImpl>();
            var pluginIds = new List<string>(channel.ContentRelatedPluginIdList);
            if (!string.IsNullOrEmpty(channel.ContentModelPluginId))
            {
                pluginIds.Add(channel.ContentModelPluginId);
            }

            foreach (var service in await PluginManager.GetServicesAsync())
            {
                if (!pluginIds.Contains(service.PluginId)) continue;

                if (!includeContentTable && PluginContentTableManager.IsContentTable(service)) continue;

                list.Add(service);
            }

            return list;
        }

        public static List<string> GetContentPluginIds(Channel channel)
        {
            if (channel.ContentRelatedPluginIds.Any() &&
                string.IsNullOrEmpty(channel.ContentModelPluginId))
            {
                return null;
            }

            var pluginIds = new List<string>(channel.ContentRelatedPluginIdList);
            if (!string.IsNullOrEmpty(channel.ContentModelPluginId))
            {
                pluginIds.Add(channel.ContentModelPluginId);
            }

            return pluginIds;
        }

        public static async Task<Dictionary<string, Dictionary<string, Func<IContentContext, string>>>> GetContentColumnsAsync(List<string> pluginIds)
        {
            var dict = new Dictionary<string, Dictionary<string, Func<IContentContext, string>>>();
            if (pluginIds == null || pluginIds.Count == 0) return dict;

            foreach (var service in await PluginManager.GetServicesAsync())
            {
                if (!pluginIds.Contains(service.PluginId) || service.ContentColumns == null || service.ContentColumns.Count == 0) continue;

                dict[service.PluginId] = service.ContentColumns;
            }

            return dict;
        }
    }
}

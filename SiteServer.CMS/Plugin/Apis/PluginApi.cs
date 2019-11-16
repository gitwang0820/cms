﻿using System.Threading.Tasks;
using SiteServer.CMS.Api;
using SiteServer.CMS.Context;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.Plugin;
using SiteServer.Utils;

namespace SiteServer.CMS.Plugin.Apis
{
    public class PluginApi : IPluginApi
    {
        private PluginApi() { }

        private static PluginApi _instance;
        public static PluginApi Instance => _instance ?? (_instance = new PluginApi());

        public async Task<string> GetPluginUrlAsync(string pluginId, string relatedUrl = "")
        {
            if (PageUtils.IsProtocolUrl(relatedUrl)) return relatedUrl;

            if (StringUtils.StartsWith(relatedUrl, "~/"))
            {
                return PageUtils.GetRootUrl(relatedUrl.Substring(1));
            }

            if (StringUtils.StartsWith(relatedUrl, "@/"))
            {
                return PageUtils.GetAdminUrl(relatedUrl.Substring(1));
            }

            var config = await ConfigManager.GetInstanceAsync();

            return PageUtility.GetSiteFilesUrl(config.ApiUrl, PageUtils.Combine(DirectoryUtils.SiteFiles.Plugins, pluginId, relatedUrl));
        }

        public async Task<string> GetPluginApiUrlAsync(string pluginId)
        {
            return await ApiManager.GetApiUrlAsync($"plugins/{pluginId}");
        }

        public string GetPluginPath(string pluginId, string relatedPath = "")
        {
            var path = PathUtils.Combine(WebUtils.GetPluginPath(pluginId), relatedPath);
            DirectoryUtils.CreateDirectoryIfNotExists(path);
            return path;
        }

        public async Task<T> GetPluginAsync<T>() where T : PluginBase
        {
            var pluginInfo = await PluginManager.GetPluginInfoAsync<T>();
            return pluginInfo?.Plugin as T;
        }
    }
}

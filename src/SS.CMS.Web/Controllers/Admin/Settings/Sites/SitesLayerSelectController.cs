﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SS.CMS.Abstractions;

namespace SS.CMS.Web.Controllers.Admin.Settings.Sites
{
    [Route(Constants.ApiRoute)]
    public partial class SitesLayerSelectController : ControllerBase
    {
        public const string Route = "settings/sitesLayerSelect";

        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly IPluginManager _pluginManager;
        private readonly ISiteRepository _siteRepository;

        public SitesLayerSelectController(IAuthManager authManager, IPathManager pathManager, IPluginManager pluginManager, ISiteRepository siteRepository)
        {
            _authManager = authManager;
            _pathManager = pathManager;
            _pluginManager = pluginManager;
            _siteRepository = siteRepository;
        }

        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get()
        {
            
            if (!await _authManager.IsAdminAuthenticatedAsync() ||
                !await _authManager.HasSystemPermissionsAsync(Constants.AppPermissions.SettingsSites))
            {
                return Unauthorized();
            }

            var rootSiteId = await _siteRepository.GetIdByIsRootAsync();

            var sites = await _siteRepository.GetSitesWithChildrenAsync(0, async x => new
            {
                SiteUrl = await _pathManager.GetSiteUrlAsync(x, false)
            });

            var tableNames = await _siteRepository.GetSiteTableNamesAsync(_pluginManager);

            return new GetResult
            {
                Sites = sites,
                RootSiteId = rootSiteId,
                TableNames = tableNames
            };
        }
    }
}

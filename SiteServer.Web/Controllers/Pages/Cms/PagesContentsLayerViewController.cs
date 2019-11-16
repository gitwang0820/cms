﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using NSwag.Annotations;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.DataCache.Content;
using SiteServer.CMS.Model;

namespace SiteServer.API.Controllers.Pages.Cms
{
    [OpenApiIgnore]
    [RoutePrefix("pages/cms/contentsLayerView")]
    public class PagesContentsLayerViewController : ApiController
    {
        private const string Route = "";

        [HttpGet, Route(Route)]
        public async Task<IHttpActionResult> Get()
        {
            try
            {
                var request = await AuthenticatedRequest.GetRequestAsync();

                var siteId = request.GetQueryInt("siteId");
                var channelId = request.GetQueryInt("channelId");
                var contentId = request.GetQueryInt("contentId");

                if (!request.IsAdminLoggin ||
                    !await request.AdminPermissionsImpl.HasChannelPermissionsAsync(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentView))
                {
                    return Unauthorized();
                }

                var site = await SiteManager.GetSiteAsync(siteId);
                if (site == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = await ChannelManager.GetChannelAsync(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var contentInfo = await ContentManager.GetContentInfoAsync(site, channelInfo, contentId);
                if (contentInfo == null) return BadRequest("无法确定对应的内容");

                contentInfo.Set(ContentAttribute.CheckState, CheckManager.GetCheckState(site, contentInfo));

                var channelName = await ChannelManager.GetChannelNameNavigationAsync(siteId, channelId);

                var attributes = await ChannelManager.GetContentsColumnsAsync(site, channelInfo, true);

                return Ok(new
                {
                    Value = contentInfo,
                    ChannelName = channelName,
                    Attributes = attributes
                });
            }
            catch (Exception ex)
            {
                await LogUtils.AddErrorLogAsync(ex);
                return InternalServerError(ex);
            }
        }
    }
}

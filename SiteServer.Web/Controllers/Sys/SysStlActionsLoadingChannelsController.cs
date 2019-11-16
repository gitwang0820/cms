﻿using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NSwag.Annotations;
using SiteServer.CMS.Api.Sys.Stl;
using SiteServer.CMS.Context.Enumerations;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.StlParser.StlElement;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Sys
{
    [OpenApiIgnore]
    public class SysStlActionsLoadingChannelsController : ApiController
    {
        [HttpPost, Route(ApiRouteActionsLoadingChannels.Route)]
        public async Task Main()
        {
            var builder = new StringBuilder();

            try
            {
                var form = HttpContext.Current.Request.Form;
                var siteId = TranslateUtils.ToInt(form["siteId"]);
                var parentId = TranslateUtils.ToInt(form["parentId"]);
                var target = form["target"];
                var isShowTreeLine = TranslateUtils.ToBool(form["isShowTreeLine"]);
                var isShowContentNum = TranslateUtils.ToBool(form["isShowContentNum"]);
                var currentFormatString = form["currentFormatString"];
                var topChannelId = TranslateUtils.ToInt(form["topChannelId"]);
                var topParentsCount = TranslateUtils.ToInt(form["topParentsCount"]);
                var currentChannelId = TranslateUtils.ToInt(form["currentChannelId"]);

                var site = await SiteManager.GetSiteAsync(siteId);
                var channelIdList = await ChannelManager.GetChannelIdListAsync(await ChannelManager.GetChannelAsync(siteId, parentId == 0 ? siteId : parentId), EScopeType.Children, string.Empty, string.Empty, string.Empty);

                foreach (var channelId in channelIdList)
                {
                    var nodeInfo = await ChannelManager.GetChannelAsync(siteId, channelId);

                    builder.Append(await StlTree.GetChannelRowHtmlAsync(site, nodeInfo, target, isShowTreeLine, isShowContentNum, WebConfigUtils.DecryptStringBySecretKey(currentFormatString), topChannelId, topParentsCount, currentChannelId, false));
                }
            }
            catch
            {
                // ignored
            }

            HttpContext.Current.Response.Write(builder);
            HttpContext.Current.Response.End();
        }
    }
}

﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SiteServer.CMS.Context.Enumerations;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Model;
using SiteServer.CMS.Plugin.Impl;
using SiteServer.CMS.StlParser.Model;
using SiteServer.Plugin;
using SiteServer.Utils;

namespace SiteServer.CMS.Api.V1
{
    public class StlRequest
    {
        private AuthenticatedRequest Request { get; set; }

        public bool IsApiAuthorized { get; private set; }

        public Site Site { get; private set; }

        public PageInfo PageInfo { get; private set; }

        public ContextInfo ContextInfo { get; private set; }

        public async Task LoadAsync(AuthenticatedRequest request, bool isApiAuthorized)
        {
            //Request = new AuthenticatedRequest();
            //IsApiAuthorized = Request.IsApiAuthenticated && AccessTokenManager.IsScope(Request.ApiToken, AccessTokenManager.ScopeStl);

            Request = request;
            IsApiAuthorized = isApiAuthorized;

            if (!IsApiAuthorized) return;

            var siteId = Request.GetQueryInt("siteId");
            var siteDir = Request.GetQueryString("siteDir");

            var channelId = Request.GetQueryInt("channelId");
            var contentId = Request.GetQueryInt("contentId");

            if (siteId > 0)
            {
                Site = await SiteManager.GetSiteAsync(siteId);
            }
            else if (!string.IsNullOrEmpty(siteDir))
            {
                Site = await SiteManager.GetSiteByDirectoryAsync(siteDir);
            }
            else
            {
                Site = await SiteManager.GetSiteByIsRootAsync();
                if (Site == null)
                {
                    var siteList = await SiteManager.GetSiteListAsync();
                    if (siteList != null && siteList.Count > 0)
                    {
                        Site = siteList[0];
                    }
                }
            }

            if (Site == null) return;

            if (channelId == 0)
            {
                channelId = Site.Id;
            }

            var templateInfo = new Template
            {
                Id = 0,
                SiteId = Site.Id,
                TemplateName = string.Empty,
                Type = TemplateType.IndexPageTemplate,
                RelatedFileName = string.Empty,
                CreatedFileFullName = string.Empty,
                CreatedFileExtName = string.Empty,
                CharsetType = ECharset.utf_8,
                Default = true
            };

            PageInfo = await PageInfo.GetPageInfoAsync(channelId, contentId, Site, templateInfo, new Dictionary<string, object>());

            PageInfo.UniqueId = 1000;
            PageInfo.User = Request.User;

            var attributes = TranslateUtils.NewIgnoreCaseNameValueCollection();
            foreach (var key in Request.QueryString.AllKeys)
            {
                attributes[key] = Request.QueryString[key];
            }

            ContextInfo = new ContextInfo(PageInfo)
            {
                IsStlEntity = true,
                Attributes = attributes,
                InnerHtml = string.Empty
            };
        }
    }
}

﻿using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NSwag.Annotations;
using SiteServer.CMS.Api.Sys.Stl;
using SiteServer.CMS.Context;
using SiteServer.CMS.Context.Enumerations;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.DataCache.Content;
using SiteServer.CMS.Model;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Sys
{
    [OpenApiIgnore]
    public class SysStlActionsDownloadController : ApiController
    {
        [HttpGet]
        [Route(ApiRouteActionsDownload.Route)]
        public async Task Main()
        {
            try
            {
                var request = await AuthenticatedRequest.GetRequestAsync();

                if (!string.IsNullOrEmpty(request.GetQueryString("siteId")) && !string.IsNullOrEmpty(request.GetQueryString("fileUrl")) && string.IsNullOrEmpty(request.GetQueryString("contentId")))
                {
                    var siteId = request.GetQueryInt("siteId");
                    var fileUrl = WebConfigUtils.DecryptStringBySecretKey(request.GetQueryString("fileUrl"));

                    if (PageUtils.IsProtocolUrl(fileUrl))
                    {
                        PageUtils.Redirect(fileUrl);
                        return;
                    }

                    var site = await SiteManager.GetSiteAsync(siteId);
                    var filePath = PathUtility.MapPath(site, fileUrl);
                    var fileType = EFileSystemTypeUtils.GetEnumType(PathUtils.GetExtension(filePath));
                    if (EFileSystemTypeUtils.IsDownload(fileType))
                    {
                        if (FileUtils.IsFileExists(filePath))
                        {
                            PageUtils.Download(HttpContext.Current.Response, filePath);
                            return;
                        }
                    }
                    else
                    {
                        PageUtils.Redirect(PageUtility.ParseNavigationUrl(site, fileUrl, false));
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(request.GetQueryString("filePath")))
                {
                    var filePath = WebConfigUtils.DecryptStringBySecretKey(request.GetQueryString("filePath"));
                    var fileType = EFileSystemTypeUtils.GetEnumType(PathUtils.GetExtension(filePath));
                    if (EFileSystemTypeUtils.IsDownload(fileType))
                    {
                        if (FileUtils.IsFileExists(filePath))
                        {
                            PageUtils.Download(HttpContext.Current.Response, filePath);
                            return;
                        }
                    }
                    else
                    {
                        var fileUrl = PageUtils.GetRootUrlByPhysicalPath(filePath);
                        PageUtils.Redirect(PageUtils.ParseNavigationUrl(fileUrl));
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(request.GetQueryString("siteId")) && !string.IsNullOrEmpty(request.GetQueryString("channelId")) && !string.IsNullOrEmpty(request.GetQueryString("contentId")) && !string.IsNullOrEmpty(request.GetQueryString("fileUrl")))
                {
                    var siteId = request.GetQueryInt("siteId");
                    var channelId = request.GetQueryInt("channelId");
                    var contentId = request.GetQueryInt("contentId");
                    var fileUrl = WebConfigUtils.DecryptStringBySecretKey(request.GetQueryString("fileUrl"));
                    var site = await SiteManager.GetSiteAsync(siteId);
                    var channelInfo = await ChannelManager.GetChannelAsync(siteId, channelId);
                    var contentInfo = await ContentManager.GetContentInfoAsync(site, channelInfo, contentId);

                    DataProvider.ContentDao.AddDownloads(await ChannelManager.GetTableNameAsync(site, channelInfo), channelId, contentId);

                    if (!string.IsNullOrEmpty(contentInfo?.Get<string>(ContentAttribute.FileUrl)))
                    {
                        if (PageUtils.IsProtocolUrl(fileUrl))
                        {
                            PageUtils.Redirect(fileUrl);
                            return;
                        }

                        var filePath = PathUtility.MapPath(site, fileUrl, true);
                        var fileType = EFileSystemTypeUtils.GetEnumType(PathUtils.GetExtension(filePath));
                        if (EFileSystemTypeUtils.IsDownload(fileType))
                        {
                            if (FileUtils.IsFileExists(filePath))
                            {
                                PageUtils.Download(HttpContext.Current.Response, filePath);
                                return;
                            }
                        }
                        else
                        {
                            PageUtils.Redirect(PageUtility.ParseNavigationUrl(site, fileUrl, false));
                            return;
                        }
                    }
                }
            }
            catch
            {
                // ignored
            }

            HttpContext.Current.Response.Write("下载失败，不存在此文件！");
        }
    }
}

﻿using System;
using System.Threading.Tasks;
using System.Web.Http;
using NSwag.Annotations;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Pages.Cms
{
    [OpenApiIgnore]
    [RoutePrefix("pages/cms/special")]
    public class PagesSpecialController : ApiController
    {
        private const string Route = "";
        private const string RouteDownload = "actions/download";

        [HttpGet, Route(Route)]
        public async Task<IHttpActionResult> List()
        {
            try
            {
                var request = await AuthenticatedRequest.GetRequestAsync();

                var siteId = request.GetQueryInt("siteId");

                if (!request.IsAdminLoggin ||
                    !await request.AdminPermissionsImpl.HasSitePermissionsAsync(siteId,
                        ConfigManager.WebSitePermissions.Template))
                {
                    return Unauthorized();
                }

                var site = await SiteManager.GetSiteAsync(siteId);
                var specialInfoList = await DataProvider.SpecialDao.GetSpecialListAsync(siteId);

                return Ok(new
                {
                    Value = specialInfoList,
                    SiteUrl = PageUtility.GetSiteUrl(site, true)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpDelete, Route(Route)]
        public async Task<IHttpActionResult> Delete()
        {
            try
            {
                var request = await AuthenticatedRequest.GetRequestAsync();
                var siteId = request.GetPostInt("siteId");
                var specialId = request.GetPostInt("specialId");

                if (!request.IsAdminLoggin ||
                    !await request.AdminPermissionsImpl.HasSitePermissionsAsync(siteId,
                        ConfigManager.WebSitePermissions.Template))
                {
                    return Unauthorized();
                }

                var site = await SiteManager.GetSiteAsync(siteId);
                var specialInfo = await SpecialManager.DeleteSpecialAsync(site, specialId);

                await request.AddSiteLogAsync(siteId,
                    "删除专题",
                    $"专题名称:{specialInfo.Title}");

                var specialInfoList = await DataProvider.SpecialDao.GetSpecialListAsync(siteId);

                return Ok(new
                {
                    Value = specialInfoList
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(RouteDownload)]
        public async Task<IHttpActionResult> Download()
        {
            try
            {
                var request = await AuthenticatedRequest.GetRequestAsync();

                var siteId = request.GetPostInt("siteId");
                var specialId = request.GetPostInt("specialId");

                if (!request.IsAdminLoggin ||
                    !await request.AdminPermissionsImpl.HasSitePermissionsAsync(siteId,
                        ConfigManager.WebSitePermissions.Template))
                {
                    return Unauthorized();
                }

                var site = await SiteManager.GetSiteAsync(siteId);
                var specialInfo = await SpecialManager.GetSpecialAsync(siteId, specialId);

                var directoryPath = SpecialManager.GetSpecialDirectoryPath(site, specialInfo.Url);
                var srcDirectoryPath = SpecialManager.GetSpecialSrcDirectoryPath(directoryPath);
                var zipFilePath = SpecialManager.GetSpecialZipFilePath(specialInfo.Title, directoryPath);

                FileUtils.DeleteFileIfExists(zipFilePath);
                ZipUtils.CreateZip(zipFilePath, srcDirectoryPath);
                var url = SpecialManager.GetSpecialZipFileUrl(site, specialInfo);

                return Ok(new
                {
                    Value = url
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}

﻿using System.Threading.Tasks;
using System.Web.Http;
using NSwag.Annotations;
using SiteServer.CMS.Core;

namespace SiteServer.API.Controllers.Sys
{
    [OpenApiIgnore]
    public class SysErrorController : ApiController
    {
        private const string Route = "sys/errors/{id}";

        [HttpGet, Route(Route)]
        public async Task<IHttpActionResult> Main(int id)
        {
            return Ok(new
            {
                LogInfo = await DataProvider.ErrorLogDao.GetErrorLogAsync(id),
                Version = SystemManager.ProductVersion
            });
        }
    }
}

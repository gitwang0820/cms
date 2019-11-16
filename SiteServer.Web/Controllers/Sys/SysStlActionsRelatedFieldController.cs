﻿using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using NSwag.Annotations;
using SiteServer.CMS.Api.Sys.Stl;
using SiteServer.CMS.Core;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Sys
{
    [OpenApiIgnore]
    public class SysStlActionsRelatedFieldController : ApiController
    {
        [HttpPost, Route(ApiRouteActionsRelatedField.Route)]
        public async Task Main(int siteId)
        {
            var request = await AuthenticatedRequest.GetRequestAsync();

            var callback = request.GetQueryString("callback");
            var relatedFieldId = request.GetQueryInt("relatedFieldId");
            var parentId = request.GetQueryInt("parentId");
            var jsonString = await GetRelatedFieldAsync(relatedFieldId, parentId);
            var call = callback + "(" + jsonString + ")";

            HttpContext.Current.Response.Write(call);
            HttpContext.Current.Response.End();
        }

        private async Task<string> GetRelatedFieldAsync(int relatedFieldId, int parentId)
        {
            var jsonString = new StringBuilder();

            jsonString.Append("[");

            var list = await DataProvider.RelatedFieldItemDao.GetRelatedFieldItemInfoListAsync(relatedFieldId, parentId);
            if (list.Any())
            {
                foreach (var itemInfo in list)
                {
                    jsonString.AppendFormat(@"{{""id"":""{0}"",""name"":""{1}"",""value"":""{2}""}},", itemInfo.Id, StringUtils.ToJsString(itemInfo.ItemName), StringUtils.ToJsString(itemInfo.ItemValue));
                }
                jsonString.Length -= 1;
            }

            jsonString.Append("]");
            return jsonString.ToString();
        }
    }
}

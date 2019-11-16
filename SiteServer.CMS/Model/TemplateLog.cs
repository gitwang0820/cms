using System;
using Datory;

namespace SiteServer.CMS.Model
{
    [DataTable("siteserver_TemplateLog")]
    public class TemplateLog : Entity
    {
        [DataColumn]
        public int TemplateId { get; set; }

        [DataColumn]
        public int SiteId { get; set; }

        [DataColumn]
        public DateTime AddDate { get; set; }

        [DataColumn]
        public string AddUserName { get; set; }

        [DataColumn]
        public int ContentLength { get; set; }

        [DataColumn(Text = true)]
        public string TemplateContent { get; set; }
    }
}

using Datory;

namespace SiteServer.CMS.Model
{
    [DataTable("siteserver_RelatedField")]
    public class RelatedField : Entity
	{
        [DataColumn]
	    public string Title { get; set; }

        [DataColumn]
        public int SiteId { get; set; }

        [DataColumn]
        public int TotalLevel { get; set; }

        [DataColumn]
        public string Prefixes { get; set; }

        [DataColumn]
        public string Suffixes { get; set; }
	}
}

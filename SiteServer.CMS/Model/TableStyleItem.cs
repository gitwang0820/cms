using Datory;
using SiteServer.Utils;

namespace SiteServer.CMS.Model
{
    [DataTable("siteserver_TableStyleItem")]
    public class TableStyleItem : Entity
    {
        [DataColumn]
        public int TableStyleId { get; set; }

        [DataColumn]
        public string ItemTitle { get; set; }

        [DataColumn]
        public string ItemValue { get; set; }

        [DataColumn]
        public string IsSelected { get; set; }

        public bool Selected
        {
            get => TranslateUtils.ToBool(IsSelected);
            set => IsSelected = value.ToString();
        }
    }
}
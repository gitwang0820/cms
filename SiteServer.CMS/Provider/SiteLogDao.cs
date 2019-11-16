using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Datory;
using SiteServer.CMS.Data;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Model;
using SiteServer.Utils;

namespace SiteServer.CMS.Provider
{
    public class SiteLogDao : IRepository
    {
        private readonly Repository<SiteLog> _repository;

        public SiteLogDao()
        {
            _repository = new Repository<SiteLog>(new Database(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString));
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;

        public async Task InsertAsync(SiteLog log)
        {
            await _repository.InsertAsync(log);
        }

        public async Task DeleteIfThresholdAsync()
        {
            var config = await ConfigManager.GetInstanceAsync();
            if (!config.IsTimeThreshold) return;

            var days = config.TimeThreshold;
            if (days <= 0) return;

            await _repository.DeleteAsync(Q
                .Where(nameof(SiteLog.AddDate), "<", DateTime.Now.AddDays(-days))
            );
        }

        public async Task DeleteAsync(List<int> idList)
        {
            if (idList == null || idList.Count <= 0) return;

            await _repository.DeleteAsync(Q
                .WhereIn(nameof(ErrorLog.Id), idList)
            );
        }

        public async Task DeleteAllAsync()
        {
            await _repository.DeleteAsync();
        }

        public string GetSelectCommend()
        {
            return "SELECT Id, SiteId, ChannelId, ContentId, UserName, IpAddress, AddDate, Action, Summary FROM siteserver_SiteLog";
        }

        public string GetSelectCommend(int siteId, string logType, string userName, string keyword, string dateFrom, string dateTo)
        {
            if (siteId == 0 && (string.IsNullOrEmpty(logType) || StringUtils.EqualsIgnoreCase(logType, "All")) && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(keyword) && string.IsNullOrEmpty(dateFrom) && string.IsNullOrEmpty(dateTo))
            {
                return GetSelectCommend();
            }

            var whereString = new StringBuilder("WHERE ");

            var isWhere = false;

            if (siteId > 0)
            {
                isWhere = true;
                whereString.AppendFormat("(SiteId = {0})", siteId);
            }

            if (!string.IsNullOrEmpty(logType) && !StringUtils.EqualsIgnoreCase(logType, "All"))
            {
                if (isWhere)
                {
                    whereString.Append(" AND ");
                }
                isWhere = true;

                if (StringUtils.EqualsIgnoreCase(logType, "Channel"))
                {
                    whereString.Append("(ChannelId > 0 AND ContentId = 0)");
                }
                else if (StringUtils.EqualsIgnoreCase(logType, "Body"))
                {
                    whereString.Append("(ChannelId > 0 AND ContentId > 0)");
                }
            }

            if (!string.IsNullOrEmpty(userName))
            {
                if (isWhere)
                {
                    whereString.Append(" AND ");
                }
                isWhere = true;
                whereString.AppendFormat("(UserName = '{0}')", userName);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                if (isWhere)
                {
                    whereString.Append(" AND ");
                }
                isWhere = true;
                whereString.AppendFormat("(Action LIKE '%{0}%' OR Summary LIKE '%{0}%')", AttackUtils.FilterSql(keyword));
            }

            if (!string.IsNullOrEmpty(dateFrom))
            {
                if (isWhere)
                {
                    whereString.Append(" AND ");
                }
                isWhere = true;
                whereString.Append($"(AddDate >= {SqlUtils.GetComparableDate(TranslateUtils.ToDateTime(dateFrom))})");
            }
            if (!string.IsNullOrEmpty(dateTo))
            {
                if (isWhere)
                {
                    whereString.Append(" AND ");
                }
                whereString.Append($"(AddDate <= {SqlUtils.GetComparableDate(TranslateUtils.ToDateTime(dateTo))})");
            }

            return "SELECT Id, SiteId, ChannelId, ContentId, UserName, IpAddress, AddDate, Action, Summary FROM siteserver_SiteLog " + whereString;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Datory;
using SiteServer.CMS.Context.Enumerations;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.Data;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Model;
using SiteServer.Plugin;

namespace SiteServer.CMS.Provider
{
    public class TemplateDao : DataProviderBase, IRepository
    {
        private readonly Repository<Template> _repository;

        public TemplateDao()
        {
            _repository = new Repository<Template>(new Database(WebConfigUtils.DatabaseType, WebConfigUtils.ConnectionString));
        }

        public IDatabase Database => _repository.Database;

        public string TableName => _repository.TableName;

        public List<TableColumn> TableColumns => _repository.TableColumns;


        private const string SqlSelectAllTemplateByType = "SELECT Id, SiteId, TemplateName, TemplateType, RelatedFileName, CreatedFileFullName, CreatedFileExtName, Charset, Default FROM siteserver_Template WHERE SiteId = @SiteId AND TemplateType = @TemplateType ORDER BY RelatedFileName";

        private const string SqlSelectAllTemplateBySiteId = "SELECT Id, SiteId, TemplateName, TemplateType, RelatedFileName, CreatedFileFullName, CreatedFileExtName, Charset, Default FROM siteserver_Template WHERE SiteId = @SiteId ORDER BY TemplateType, RelatedFileName";

        private const string ParmSiteId = "@SiteId";
        private const string ParmTemplateType = "@TemplateType";

        public async Task<int> InsertAsync(Template template, string templateContent, string administratorName)
        {
            if (template.Default)
            {
                await SetAllTemplateDefaultToFalseAsync(template.SiteId, template.Type);
            }

            template.Id = await _repository.InsertAsync(template);

            var site = await SiteManager.GetSiteAsync(template.SiteId);
            await TemplateManager.WriteContentToTemplateFileAsync(site, template, templateContent, administratorName);

            TemplateManager.RemoveCache(template.SiteId);

            return template.Id;
        }

        public async Task UpdateAsync(Site site, Template template, string templateContent, string administratorName)
        {
            if (template.Default)
            {
                await SetAllTemplateDefaultToFalseAsync(site.Id, template.Type);
            }

            await _repository.UpdateAsync(template);

            await TemplateManager.WriteContentToTemplateFileAsync(site, template, templateContent, administratorName);

            TemplateManager.RemoveCache(template.SiteId);
        }

        private async Task SetAllTemplateDefaultToFalseAsync(int siteId, TemplateType templateType)
        {
            await _repository.UpdateAsync(Q
                .Set(nameof(Template.IsDefault), false.ToString())
                .Where(nameof(Template.SiteId), siteId)
                .Where(nameof(Template.TemplateType), templateType.Value)
            );
        }

        public async Task SetDefaultAsync(int siteId, int templateId)
        {
            var template = await TemplateManager.GetTemplateAsync(siteId, templateId);
            await SetAllTemplateDefaultToFalseAsync(template.SiteId, template.Type);

            await _repository.UpdateAsync(Q
                .Set(nameof(Template.IsDefault), true.ToString())
                .Where(nameof(Template.Id), templateId)
            );

            TemplateManager.RemoveCache(siteId);
        }

        public async Task DeleteAsync(int siteId, int id)
        {
            var site = await SiteManager.GetSiteAsync(siteId);
            var template = await TemplateManager.GetTemplateAsync(siteId, id);
            var filePath = TemplateManager.GetTemplateFilePath(site, template);

            await _repository.DeleteAsync(id);
            FileUtils.DeleteFileIfExists(filePath);

            TemplateManager.RemoveCache(siteId);
        }

        public async Task<string> GetImportTemplateNameAsync(int siteId, string templateName)
        {
            string importTemplateName;
            if (templateName.IndexOf("_", StringComparison.Ordinal) != -1)
            {
                var templateNameCount = 0;
                var lastTemplateName = templateName.Substring(templateName.LastIndexOf("_", StringComparison.Ordinal) + 1);
                var firstTemplateName = templateName.Substring(0, templateName.Length - lastTemplateName.Length);
                try
                {
                    templateNameCount = int.Parse(lastTemplateName);
                }
                catch
                {
                    // ignored
                }
                templateNameCount++;
                importTemplateName = firstTemplateName + templateNameCount;
            }
            else
            {
                importTemplateName = templateName + "_1";
            }

            var exists = await _repository.ExistsAsync(Q
                .Where(nameof(Template.SiteId), siteId)
                .Where(nameof(Template.TemplateName), importTemplateName)
            );
            if (exists)
            {
                importTemplateName = await GetImportTemplateNameAsync(siteId, importTemplateName);
            }

            return importTemplateName;
        }

        public async Task<Dictionary<TemplateType, int>> GetCountDictionaryAsync(int siteId)
        {
            var dictionary = new Dictionary<TemplateType, int>();

            var dataList = await _repository.GetAllAsync<(string Type, int Count)>(Q
                .Select(nameof(Template.TemplateType))
                .SelectRaw("COUNT(*) as Count")
                .Where(nameof(Template.SiteId), siteId)
                .GroupBy(nameof(Template.TemplateType)));

            foreach (var (type, count) in dataList)
            {
                var templateType = TemplateTypeUtils.GetEnumType(type);

                if (dictionary.ContainsKey(templateType))
                {
                    dictionary[templateType] += count;
                }
                else
                {
                    dictionary[templateType] = count;
                }
            }

            return dictionary;
        }

        public IDataReader GetDataSourceByType(int siteId, TemplateType type)
        {
            var parms = new IDataParameter[]
			{
				GetParameter(ParmSiteId, DataType.Integer, siteId),
				GetParameter(ParmTemplateType, DataType.VarChar, 50, type.Value)
			};

            var enumerable = ExecuteReader(SqlSelectAllTemplateByType, parms);
            return enumerable;
        }

        public IDataReader GetDataSource(int siteId, string searchText, string templateTypeString)
        {
            if (string.IsNullOrEmpty(searchText) && string.IsNullOrEmpty(templateTypeString))
            {
                var parms = new IDataParameter[]
				{
					GetParameter(ParmSiteId, DataType.Integer, siteId)
				};

                var enumerable = ExecuteReader(SqlSelectAllTemplateBySiteId, parms);
                return enumerable;
            }
            if (!string.IsNullOrEmpty(searchText))
            {
                var whereString = (string.IsNullOrEmpty(templateTypeString)) ? string.Empty :
                    $"AND TemplateType = '{templateTypeString}' ";
                searchText = AttackUtils.FilterSql(searchText);
                whereString +=
                    $"AND (TemplateName LIKE '%{searchText}%' OR RelatedFileName LIKE '%{searchText}%' OR CreatedFileFullName LIKE '%{searchText}%' OR CreatedFileExtName LIKE '%{searchText}%')";
                string sqlString =
                    $"SELECT Id, SiteId, TemplateName, TemplateType, RelatedFileName, CreatedFileFullName, CreatedFileExtName, Charset, Default FROM siteserver_Template WHERE SiteId = {siteId} {whereString} ORDER BY TemplateType, RelatedFileName";

                var enumerable = ExecuteReader(sqlString);
                return enumerable;
            }

            return GetDataSourceByType(siteId, TemplateTypeUtils.GetEnumType(templateTypeString));
        }

        public async Task<List<int>> GetIdListByTypeAsync(int siteId, TemplateType templateType)
        {
            var list = await GetTemplateListByTypeAsync(siteId, templateType);
            return list.Select(x => x.Id).ToList();
        }

        public async Task<List<Template>> GetTemplateListByTypeAsync(int siteId, TemplateType templateType)
        {
            var templateEntityList = await _repository.GetAllAsync(Q
                .Where(nameof(Template.SiteId), siteId)
                .Where(nameof(Template.TemplateType), templateType.Value)
                .OrderBy(nameof(Template.RelatedFileName))
            );

            return templateEntityList.ToList();
        }

        public async Task<List<Template>> GetTemplateListBySiteIdAsync(int siteId)
        {
            var templateEntityList = await _repository.GetAllAsync(Q
                .Where(nameof(Template.SiteId), siteId)
                .OrderBy(nameof(Template.TemplateType), nameof(Template.RelatedFileName))
            );

            return templateEntityList.ToList();
        }

        public async Task<IEnumerable<string>> GetTemplateNameListAsync(int siteId, TemplateType templateType)
        {
            return await _repository.GetAllAsync<string>(Q
                .Select(nameof(Template.TemplateName))
                .Where(nameof(Template.SiteId), siteId)
                .Where(nameof(Template.TemplateType), templateType.Value)
            );
        }

        public async Task<IEnumerable<string>> GetRelatedFileNameListAsync(int siteId, TemplateType templateType)
        {
            return await _repository.GetAllAsync<string>(Q
                .Select(nameof(Template.RelatedFileName))
                .Where(nameof(Template.SiteId), siteId)
                .Where(nameof(Template.TemplateType), templateType.Value)
            );
        }

        public async Task CreateDefaultTemplateAsync(int siteId, string administratorName)
        {
            var site = await SiteManager.GetSiteAsync(siteId);

            var templateList = new List<Template>();
            var charset = ECharsetUtils.GetEnumType(site.Charset);

            var template = new Template
            {
                Id = 0,
                SiteId = site.Id,
                TemplateName = "系统首页模板",
                Type = TemplateType.IndexPageTemplate,
                RelatedFileName = "T_系统首页模板.html",
                CreatedFileFullName = "@/index.html",
                CreatedFileExtName = ".html",
                CharsetType = charset,
                Default = true
            };
            templateList.Add(template);

            template = new Template
            {
                Id = 0,
                SiteId = site.Id,
                TemplateName = "系统栏目模板",
                Type = TemplateType.ChannelTemplate,
                RelatedFileName = "T_系统栏目模板.html",
                CreatedFileFullName = "index.html",
                CreatedFileExtName = ".html",
                CharsetType = charset,
                Default = true
            };
            templateList.Add(template);

            template = new Template
            {
                Id = 0,
                SiteId = site.Id,
                TemplateName = "系统内容模板",
                Type = TemplateType.ContentTemplate,
                RelatedFileName = "T_系统内容模板.html",
                CreatedFileFullName = "index.html",
                CreatedFileExtName = ".html",
                CharsetType = charset,
                Default = true
            };
            templateList.Add(template);

            foreach (var theTemplate in templateList)
            {
                await InsertAsync(theTemplate, theTemplate.Content, administratorName);
            }
        }

        public async Task<Dictionary<int, Template>> GetTemplateDictionaryBySiteIdAsync(int siteId)
        {
            var dictionary = new Dictionary<int, Template>();

            var list = await _repository.GetAllAsync(Q
                .Where(nameof(Template.SiteId), siteId)
                .OrderBy(nameof(Template.TemplateType), nameof(Template.RelatedFileName))
            );

            foreach (var template in list)
            {
                dictionary[template.Id] = template;
            }

            return dictionary;
        }


        public async Task<Template> GetTemplateByUrlTypeAsync(int siteId, TemplateType templateType, string createdFileFullName)
        {
            return await _repository.GetAsync(Q
                .Where(nameof(Template.SiteId), siteId)
                .Where(nameof(Template.TemplateType), templateType.Value)
                .Where(nameof(Template.CreatedFileFullName), createdFileFullName)
            );
        }

        public async Task<Template> GetAsync(int templateId)
        {
            return await _repository.GetAsync(templateId);
        }
    }
}

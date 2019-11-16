﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SiteServer.CMS.Context;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache.Stl;
using SiteServer.CMS.Model;
using SiteServer.CMS.StlParser.Model;
using SiteServer.CMS.StlParser.Utility;

namespace SiteServer.CMS.StlParser.StlElement
{
    [StlElement(Title = "标签", Description = "通过 stl:tags 标签在模板中显示内容标签")]
    public class StlTags
	{
        private StlTags() { }
        public const string ElementName = "stl:tags";

        [StlAttribute(Title = "标签级别")]
        private const string TagLevel = nameof(TagLevel);

        [StlAttribute(Title = "显示标签数目")]
        private const string TotalNum = nameof(TotalNum);

        [StlAttribute(Title = "是否按引用次数排序")]
        private const string IsOrderByCount = nameof(IsOrderByCount);

        [StlAttribute(Title = "主题样式")]
        private const string Theme = nameof(Theme);

        [StlAttribute(Title = "所处上下文")]
        private const string Context = nameof(Context);


        public static async Task<object> ParseAsync(PageInfo pageInfo, ContextInfo contextInfo)
		{
		    var tagLevel = 1;
            var totalNum = 0;
            var isOrderByCount = false;
            var theme = "default";
            var isInnerHtml = !string.IsNullOrEmpty(contextInfo.InnerHtml);

		    foreach (var name in contextInfo.Attributes.AllKeys)
		    {
		        var value = contextInfo.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, TagLevel))
                {
                    tagLevel = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, TotalNum))
                {
                    totalNum = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, IsOrderByCount))
                {
                    isOrderByCount = TranslateUtils.ToBool(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, Theme))
                {
                    theme = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, Context))
                {
                    contextInfo.ContextType = EContextTypeUtils.GetEnumType(value);
                }
            }

            return await ParseImplAsync(isInnerHtml, pageInfo, contextInfo, tagLevel, totalNum, isOrderByCount, theme);
		}

        private static async Task<string> ParseImplAsync(bool isInnerHtml, PageInfo pageInfo, ContextInfo contextInfo, int tagLevel, int totalNum, bool isOrderByCount, string theme)
        {
            var innerHtml = string.Empty;
            if (isInnerHtml)
            {
                innerHtml = StringUtils.StripTags(contextInfo.OuterHtml, ElementName);
            }

            var tagsBuilder = new StringBuilder();
            if (!isInnerHtml)
            {
                tagsBuilder.Append($@"
<link rel=""stylesheet"" href=""{SiteFilesAssets.Tags.GetStyleUrl(pageInfo.ApiUrl, theme)}"" type=""text/css"" />
");
                tagsBuilder.Append(@"<ul class=""tagCloud"">");
            }

            if (contextInfo.ContextType == EContextType.Undefined)
            {
                contextInfo.ContextType = contextInfo.ContentId != 0 ? EContextType.Content : EContextType.Channel;
            }
            var contentId = 0;
            if (contextInfo.ContextType == EContextType.Content)
            {
                contentId = contextInfo.ContentId;
            }

            var tagInfoList = await StlTagCache.GetTagListAsync(pageInfo.SiteId, contentId, isOrderByCount, totalNum);
            tagInfoList = ContentTagUtils.GetTagInfoList(tagInfoList, totalNum, tagLevel);
            var contentInfo = await contextInfo.GetContentAsync();
            if (contextInfo.ContextType == EContextType.Content && contentInfo != null)
            {
                var tagInfoList2 = new List<ContentTag>();
                var tagNameList = TranslateUtils.StringCollectionToStringList(contentInfo.Tags.Trim().Replace(" ", ","));
                foreach (var tagName in tagNameList)
                {
                    if (!string.IsNullOrEmpty(tagName))
                    {
                        var isAdd = false;
                        foreach (var tagInfo in tagInfoList)
                        {
                            if (tagInfo.Tag == tagName)
                            {
                                isAdd = true;
                                tagInfoList2.Add(tagInfo);
                                break;
                            }
                        }
                        if (!isAdd)
                        {
                            var tagInfo = new ContentTag
                            {
                                Id = 0,
                                SiteId = pageInfo.SiteId,
                                ContentIds = new List<int> { contentId },
                                Tag = tagName,
                                UseNum = 1
                            };
                            tagInfoList2.Add(tagInfo);
                        }
                    }
                }
                tagInfoList = tagInfoList2;
            }

            foreach (var tagInfo in tagInfoList)
            {
                if (isInnerHtml)
                {
                    var tagHtml = innerHtml;
                    tagHtml = StringUtils.ReplaceIgnoreCase(tagHtml, "{Tag.Name}", tagInfo.Tag);
                    tagHtml = StringUtils.ReplaceIgnoreCase(tagHtml, "{Tag.Count}", tagInfo.UseNum.ToString());
                    tagHtml = StringUtils.ReplaceIgnoreCase(tagHtml, "{Tag.Level}", tagInfo.Level.ToString());
                    var innerBuilder = new StringBuilder(tagHtml);
                    await StlParserManager.ParseInnerContentAsync(innerBuilder, pageInfo, contextInfo);
                    tagsBuilder.Append(innerBuilder);
                }
                else
                {
                    var url = PageUtility.ParseNavigationUrl(pageInfo.Site,
                        $"@/utils/tags.html?tagName={PageUtils.UrlEncode(tagInfo.Tag)}", pageInfo.IsLocal);
                    tagsBuilder.Append($@"
<li class=""tag_popularity_{tagInfo.Level}""><a target=""_blank"" href=""{url}"">{tagInfo.Tag}</a></li>
");
                }
            }

            if (!isInnerHtml)
            {
                tagsBuilder.Append("</ul>");
            }
            return tagsBuilder.ToString();
        }
	}
}

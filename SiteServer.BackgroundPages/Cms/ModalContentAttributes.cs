﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using SiteServer.CMS.Context;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.DataCache.Content;
using SiteServer.CMS.Model;

namespace SiteServer.BackgroundPages.Cms
{
	public class ModalContentAttributes : BasePageCms
    {
        protected CheckBox CbIsRecommend;
        protected CheckBox CbIsHot;
        protected CheckBox CbIsColor;
        protected CheckBox CbIsTop;
        protected HtmlInputHidden HihType;
        protected TextBox TbHits;
        protected TextBox TbDownloads;

        private Channel _channel;
        private List<int> _idList;

        public static string GetOpenWindowString(int siteId, int channelId)
        {
            return LayerUtils.GetOpenScriptWithCheckBoxValue("设置内容属性", PageUtils.GetCmsUrl(siteId, nameof(ModalContentAttributes), new NameValueCollection
            {
                {"channelId", channelId.ToString()}
            }), "contentIdCollection", "请选择需要设置属性的内容！", 450, 350);
        }

        public static string GetOpenWindowStringWithCheckBoxValue(int siteId, int channelId)
        {
            return LayerUtils.GetOpenScriptWithCheckBoxValue("设置内容属性", PageUtils.GetCmsUrl(siteId, nameof(ModalContentAttributes), new NameValueCollection
            {
                {"channelId", channelId.ToString()}
            }), "contentIdCollection", "请选择需要设置属性的内容！", 450, 350);
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("siteId", "channelId");

            var channelId = AuthRequest.GetQueryInt("channelId");
            _channel = ChannelManager.GetChannelAsync(SiteId, channelId).GetAwaiter().GetResult();
            _idList = TranslateUtils.StringCollectionToIntList(AuthRequest.GetQueryString("contentIdCollection"));
		}

        public override void Submit_OnClick(object sender, EventArgs e)
        {
			var isChanged = false;

            switch (HihType.Value)
            {
                case "1":
                    if (CbIsRecommend.Checked || CbIsHot.Checked || CbIsColor.Checked || CbIsTop.Checked)
                    {
                        foreach (var contentId in _idList)
                        {
                            var contentInfo = ContentManager.GetContentInfoAsync(Site, _channel, contentId).GetAwaiter().GetResult();
                            if (contentInfo != null)
                            {
                                if (CbIsRecommend.Checked)
                                {
                                    contentInfo.Recommend = true;
                                }
                                if (CbIsHot.Checked)
                                {
                                    contentInfo.Hot = true;
                                }
                                if (CbIsColor.Checked)
                                {
                                    contentInfo.Color = true;
                                }
                                if (CbIsTop.Checked)
                                {
                                    contentInfo.Top = true;
                                }
                                DataProvider.ContentDao.UpdateAsync(Site, _channel, contentInfo).GetAwaiter().GetResult();
                            }
                        }

                        AuthRequest.AddSiteLogAsync(SiteId, "设置内容属性").GetAwaiter().GetResult();

                        isChanged = true;
                    }

                    break;

                case "2":
                    if (CbIsRecommend.Checked || CbIsHot.Checked || CbIsColor.Checked || CbIsTop.Checked)
                    {
                        foreach (var contentId in _idList)
                        {
                            var contentInfo = ContentManager.GetContentInfoAsync(Site, _channel, contentId).GetAwaiter().GetResult();
                            if (contentInfo != null)
                            {
                                if (CbIsRecommend.Checked)
                                {
                                    contentInfo.Recommend = false;
                                }
                                if (CbIsHot.Checked)
                                {
                                    contentInfo.Hot = false;
                                }
                                if (CbIsColor.Checked)
                                {
                                    contentInfo.Color = false;
                                }
                                if (CbIsTop.Checked)
                                {
                                    contentInfo.Top = false;
                                }
                                DataProvider.ContentDao.UpdateAsync(Site, _channel, contentInfo).GetAwaiter().GetResult();
                            }
                        }

                        AuthRequest.AddSiteLogAsync(SiteId, "取消内容属性").GetAwaiter().GetResult();

                        isChanged = true;
                    }

                    break;

                case "3":
                    var hits = TranslateUtils.ToInt(TbHits.Text);

                    foreach (var contentId in _idList)
                    {
                        var contentInfo = ContentManager.GetContentInfoAsync(Site, _channel, contentId).GetAwaiter().GetResult();
                        if (contentInfo != null)
                        {
                            contentInfo.Hits = hits;
                            DataProvider.ContentDao.UpdateAsync(Site, _channel, contentInfo).GetAwaiter().GetResult();
                        }
                    }

                    AuthRequest.AddSiteLogAsync(SiteId, "设置内容点击量").GetAwaiter().GetResult();

                    isChanged = true;
                    break;

                case "4":
                    var downloads = TranslateUtils.ToInt(TbDownloads.Text);

                    foreach (var contentId in _idList)
                    {
                        var contentInfo = ContentManager.GetContentInfoAsync(Site, _channel, contentId).GetAwaiter().GetResult();
                        if (contentInfo != null)
                        {
                            contentInfo.Downloads = downloads;
                            DataProvider.ContentDao.UpdateAsync(Site, _channel, contentInfo).GetAwaiter().GetResult();
                        }
                    }

                    AuthRequest.AddSiteLogAsync(SiteId, "设置内容下载量").GetAwaiter().GetResult();

                    isChanged = true;
                    break;
            }

            if (isChanged)
			{
                LayerUtils.Close(Page);
			}
		}

	}
}

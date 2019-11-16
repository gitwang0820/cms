﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.CMS.Context;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Create;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Enumerations;
using SiteServer.CMS.Model;

namespace SiteServer.BackgroundPages.Cms
{
    public class ModalContentTaxis : BasePageCms
    {
        protected DropDownList DdlTaxisType;
        protected TextBox TbTaxisNum;

        private int _channelId;
        private string _returnUrl;
        private List<int> _contentIdList;
        private string _tableName;

        public static string GetOpenWindowString(int siteId, int channelId, string returnUrl)
        {
            return LayerUtils.GetOpenScriptWithCheckBoxValue("内容排序", PageUtils.GetCmsUrl(siteId, nameof(ModalContentTaxis), new NameValueCollection
            {
                {"channelId", channelId.ToString()},
                {"ReturnUrl", StringUtils.ValueToUrl(returnUrl)}
            }), "contentIdCollection", "请选择需要排序的内容！", 400, 280);
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            PageUtils.CheckRequestParameter("siteId", "channelId", "ReturnUrl", "contentIdCollection");

            _channelId = AuthRequest.GetQueryInt("channelId");
            _returnUrl = StringUtils.ValueFromUrl(AuthRequest.GetQueryString("ReturnUrl"));
            _contentIdList = TranslateUtils.StringCollectionToIntList(AuthRequest.GetQueryString("contentIdCollection"));
            _tableName = ChannelManager.GetTableNameAsync(Site, _channelId).GetAwaiter().GetResult();

            if (IsPostBack) return;

            DdlTaxisType.Items.Add(new ListItem("上升", "Up"));
            DdlTaxisType.Items.Add(new ListItem("下降", "Down"));
            ControlUtils.SelectSingleItem(DdlTaxisType, "Up");
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            var isUp = DdlTaxisType.SelectedValue == "Up";
            var taxisNum = TranslateUtils.ToInt(TbTaxisNum.Text);

            var nodeInfo = ChannelManager.GetChannelAsync(SiteId, _channelId).GetAwaiter().GetResult();
            if (ETaxisTypeUtils.Equals(nodeInfo.DefaultTaxisType, ETaxisType.OrderByTaxis))
            {
                isUp = !isUp;
            }

            if (isUp == false)
            {
                _contentIdList.Reverse();
            }

            foreach (var contentId in _contentIdList)
            {
                var tuple = DataProvider.ContentDao.GetValue(_tableName, contentId, ContentAttribute.IsTop);
                if (tuple == null) continue;

                var isTop = TranslateUtils.ToBool(tuple.Item2);
                for (var i = 1; i <= taxisNum; i++)
                {
                    if (isUp)
                    {
                        if (DataProvider.ContentDao.SetTaxisToUp(_tableName, _channelId, contentId, isTop) == false)
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (DataProvider.ContentDao.SetTaxisToDown(_tableName, _channelId, contentId, isTop) == false)
                        {
                            break;
                        }
                    }
                }
            }

            CreateManager.TriggerContentChangedEventAsync(SiteId, _channelId).GetAwaiter().GetResult();
            AuthRequest.AddSiteLogAsync(SiteId, _channelId, 0, "对内容排序", string.Empty).GetAwaiter().GetResult();

            LayerUtils.CloseAndRedirect(Page, _returnUrl);
        }

    }
}

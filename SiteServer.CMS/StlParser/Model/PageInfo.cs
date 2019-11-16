﻿using System.Collections;
using System.Collections.Generic;
using SiteServer.CMS.Model;
using System.Text;
using System.Threading.Tasks;
using SiteServer.CMS.Context;
using SiteServer.CMS.DataCache;

namespace SiteServer.CMS.StlParser.Model
{
    public class PageInfo
    {
        private PageInfo() { }

        public SortedDictionary<string, string> HeadCodes { get; private set; }

        public SortedDictionary<string, string> BodyCodes { get; private set; }

        public SortedDictionary<string, string> FootCodes { get; private set; }

        public Site Site { get; private set; }

        public Dictionary<string, string> Parameters { get; set; }

        public string ApiUrl { get; private set; }

        public Template Template { get; private set; }

        public User User { get; set; }

        public int SiteId { get; private set; }

        public int PageChannelId { get; private set; }

        public int PageContentId { get; private set; }

        public bool IsLocal { get; set; }

        public Stack<ChannelItemInfo> ChannelItems { get; private set; }

        public Stack<ContentItemInfo> ContentItems { get; private set; }

        public Stack SqlItems { get; private set; }

        public Stack SiteItems { get; private set; }

        public Stack EachItems { get; private set; }

        public Dictionary<string, object> PluginItems { get; private set; }

        private int _uniqueId;

        public int UniqueId
        {
            get => _uniqueId++;
            set => _uniqueId = value;
        }

        public async Task<PageInfo> CloneAsync()
        {
            return await PageInfo.GetPageInfoAsync(PageChannelId, PageContentId, Site, Template, PluginItems);
        }

        public static async Task<PageInfo> GetPageInfoAsync(int pageChannelId, int pageContentId, Site site, Template template, Dictionary<string, object> pluginItems)
        {
            var pageInfo = new PageInfo();

            var config = await ConfigManager.GetInstanceAsync();

            pageInfo.Template = template;
            pageInfo.SiteId = site.Id;
            pageInfo.PageChannelId = pageChannelId;
            pageInfo.PageContentId = pageContentId;
            pageInfo.IsLocal = false;
            pageInfo.HeadCodes = new SortedDictionary<string, string>();
            pageInfo.BodyCodes = new SortedDictionary<string, string>();
            pageInfo.FootCodes = new SortedDictionary<string, string>();
            pageInfo.Site = site;
            pageInfo.User = null;
            pageInfo._uniqueId = 1;
            pageInfo.ApiUrl = config.ApiUrl;

            pageInfo.ChannelItems = new Stack<ChannelItemInfo>(5);
            pageInfo.ContentItems = new Stack<ContentItemInfo>(5);
            pageInfo.SqlItems = new Stack(5);
            pageInfo.SiteItems = new Stack(5);
            pageInfo.EachItems = new Stack(5);

            pageInfo.PluginItems = pluginItems;

            return pageInfo;
        }

        public void ChangeSite(Site site, int pageChannelId, int pageContentId, ContextInfo contextInfo)
        {
            SiteId = site.Id;
            Site = site;
            PageChannelId = pageChannelId;
            PageContentId = pageContentId;

            contextInfo.Site = site;
            contextInfo.ChannelId = pageChannelId;
            contextInfo.ContentId = pageContentId;
        }

        public void AddPageBodyCodeIfNotExists(string pageJsName)
        {
            if (!BodyCodes.ContainsKey(pageJsName))
            {
                BodyCodes.Add(pageJsName, GetJsCode(pageJsName));
            }
        }

        public void AddPageAfterHtmlCodeIfNotExists(string pageJsName)
        {
            if (!FootCodes.ContainsKey(pageJsName))
            {
                FootCodes.Add(pageJsName, GetJsCode(pageJsName));
            }
        }

        public void AddPageHeadCodeIfNotExists(string pageJsName)
        {
            if (!HeadCodes.ContainsKey(pageJsName))
            {
                HeadCodes.Add(pageJsName, GetJsCode(pageJsName));
            }
        }

        /// <summary>
        /// 将一个页面的js复制给本页面，提供给分页时使用
        /// add by sessionliang at 20151209
        /// </summary>
        /// <param name="lastPageInfo"></param>
        public void AddLastPageScript(PageInfo lastPageInfo)
        {
            foreach (var key in lastPageInfo.BodyCodes.Keys)
            {
                if (!BodyCodes.ContainsKey(key))
                {
                    BodyCodes.Add(key, lastPageInfo.BodyCodes[key]);
                }
            }
            foreach (var key in lastPageInfo.FootCodes.Keys)
            {
                if (!FootCodes.ContainsKey(key))
                {
                    FootCodes.Add(key, lastPageInfo.FootCodes[key]);
                }
            }
            foreach (var key in lastPageInfo.HeadCodes.Keys)
            {
                if (!HeadCodes.ContainsKey(key))
                {
                    HeadCodes.Add(key, lastPageInfo.HeadCodes[key]);
                }
            }
        }

        /// <summary>
        /// 将一个页面的js从本页面去除，提供给分页时使用
        ///  add by sessionliang at 20151209
        /// </summary>
        /// <param name="lastPageInfo"></param>
        public void ClearLastPageScript(PageInfo lastPageInfo)
        {
            foreach (var key in lastPageInfo.BodyCodes.Keys)
            {
                BodyCodes.Remove(key);
            }
            foreach (var key in lastPageInfo.FootCodes.Keys)
            {
                FootCodes.Remove(key);
            }
            foreach (var key in lastPageInfo.HeadCodes.Keys)
            {
                HeadCodes.Remove(key);
            }
        }

        /// <summary>
        /// 清理本页面的js
        /// </summary>
        public void ClearLastPageScript()
        {
            HeadCodes.Clear();
            BodyCodes.Clear();
            FootCodes.Clear();
        }

        public class Const
        {
            public const string Jquery = "Jquery";
            public const string Vue = "Vue";
            public const string JsCookie = "JsCookie";
            public const string StlClient = "StlClient";

            public const string BAjaxUpload = "B_AjaxUpload";              //AjaxUpload
            public const string BQueryString = "B_QueryString";            //QueryString
            public const string BjQueryForm = "B_JQueryForm";              //JQueryForm
            public const string BShowLoading = "B_ShowLoading";              //ShowLoading
            public const string BjTemplates = "B_JTemplates";              //JTemplates
            public const string BValidate = "B_Validate";                  //Validate
            public const string BBootstrap = "B_Bootstrap";                  //Bootstrap

            public const string JsAcSwfObject = "Js_Ac_SWFObject";                //SWFObject

            public const string JsAcJwPlayer6 = "Js_Ac_JWPlayer6";                //JWPlayer6
            public const string JsAcFlowPlayer = "Js_Ac_FlowPlayer";              //flowPlayer
            public const string JsAcMediaElement = "Js_Ac_MediaElement";          //mediaelement
            public const string JsAcAudioJs = "Js_Ac_AudioJs";                    //audio.js
            public const string JsAcVideoJs = "Js_Ac_VideoJs";                    //video.js

            public const string JsAeStlZoom = "Js_Ae_StlZoom";                    //文字缩放
            public const string JsAfStlPrinter = "Js_Af_StlPrinter";              //打印
            public const string JsAgStlTreeNotAjax = "Js_Ag_StlTreeNotAjax";                    //树状导航
            public const string JsAgStlTreeAjax = "Js_Ag_StlTreeAjax";                    //树状导航
            public const string JsAhTranslate = "Js_Ah_Translate";                //繁体/简体转换

            public const string JsPageOpenWindow = "Js_Page_OpenWindow";
            public const string JsUserScript = "Js_User_Script";
            public const string JsInnerCalendar = "Js_Inner_Calendar";

            public const string JsStaticAdFloating = "Js_Static_AdFloating";      //漂浮广告
        }

        private string GetJsCode(string pageJsName)
        {
            var retVal = string.Empty;

            if (pageJsName == Const.Jquery)
            {
                if (Site.IsCreateWithJQuery)
                {
                    retVal =
                        $@"<script src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.Components.Jquery)}"" type=""text/javascript""></script>";
                }
            }
            else if (pageJsName == Const.Vue)
            {
                retVal =
                    $@"<script src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.Components.Vue)}"" type=""text/javascript""></script>";
            }
            else if (pageJsName == Const.JsCookie)
            {
                retVal =
                    $@"<script src=""{ SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.Components.JsCookie)}"" type=""text/javascript""></script>";
            }
            else if (pageJsName == Const.StlClient)
            {
                retVal =
                    $@"<script src=""{ SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.Components.Lodash)}"" type=""text/javascript""></script><script src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.Components.StlClient)}"" type=""text/javascript""></script>";
            }
            else if (pageJsName == Const.BAjaxUpload)
            {
                retVal =
                    $@"<script src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.AjaxUpload.Js)}"" type=""text/javascript""></script>";
            }
            else if (pageJsName == Const.BQueryString)
            {
                retVal =
                    $@"<script src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.QueryString.Js)}"" type=""text/javascript""></script>";
            }
            else if (pageJsName == Const.BjQueryForm)
            {
                retVal =
                    $@"<script src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.JQueryForm.Js)}"" type=""text/javascript""></script>";
            }
            else if (pageJsName == Const.BShowLoading)
            {
                retVal =
                    $@"<link href=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.ShowLoading.Css)}"" rel=""stylesheet"" media=""screen"" /><script type=""text/javascript"" charset=""{SiteFilesAssets
                        .JQuery.ShowLoading.Charset}"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.ShowLoading.Js)}""></script>";
            }
            else if (pageJsName == Const.BjTemplates)
            {
                retVal =
                    $@"<script type=""text/javascript"" charset=""{SiteFilesAssets.JQuery.JTemplates.Charset}"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.JTemplates.Js)}""></script>";
            }
            else if (pageJsName == Const.BValidate)
            {
                retVal =
                    $@"<script type=""text/javascript"" charset=""{SiteFilesAssets.JQuery.ValidateJs.Charset}"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.ValidateJs.Js)}""></script>";
            }
            else if (pageJsName == Const.BBootstrap)
            {
                var cssUrl = SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.Bootstrap.Css);
                var jsUrl = SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JQuery.Bootstrap.Js);
                retVal = $@"
<link rel=""stylesheet"" type=""text/css"" href=""{cssUrl}"">
<script language=""javascript"" src=""{jsUrl}""></script>
";
            }
            else if (pageJsName == Const.JsAcSwfObject)
            {
                retVal =
                    $@"<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.SwfObject.Js)}""></script>";
            }
            else if (pageJsName == Const.JsAcJwPlayer6)
            {
                retVal =
                    $@"<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.JwPlayer6.Js)}""></script><script type=""text/javascript"">jwplayer.key=""ABCDEFGHIJKLMOPQ"";</script>";
            }
            else if (pageJsName == Const.JsAcFlowPlayer)
            {
                retVal =
                    $@"<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.FlowPlayer.Js)}""></script>";
            }
            else if (pageJsName == Const.JsAcMediaElement)
            {
                retVal =
                    $@"<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.MediaElement.Js)}""></script><link rel=""stylesheet"" href=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.MediaElement.Css)}"" />";
            }
            else if (pageJsName == Const.JsAcAudioJs)
            {
                retVal =
                    $@"<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.AudioJs.Js)}""></script>
<script type='text/javascript'>
audiojs.events.ready(function() {{
    audiojs.createAll();
}});
</script>
";
            }
            else if (pageJsName == Const.JsAcVideoJs)
            {
                retVal = $@"
<link href=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.VideoJs.Css)}"" rel=""stylesheet"">
<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.VideoJs.Js)}""></script>
";
            }
            else if (pageJsName == Const.JsPageOpenWindow)
            {
                retVal = @"
<div id=""stl_wnd_board"" style=""position:absolute;top:100px;left:0px;width:100%;z-index:65531;height:100%;display:none"" align=""center"">
    <div id=""stl_wnd_div"" style=""display:none; width:400px; height:330px; padding:0; margin:0px;"" align=""center"">
    <iframe id=""stl_wnd_frame"" frameborder=""0"" scrolling=""auto"" width=""100%"" height=""100%"" src=""""></iframe>
    </div>
</div>
<script>
function stlCloseWindow()
{document.getElementById('stl_wnd_div').style.display='none';document.getElementById('stl_wnd_board').style.display='none';}
function stlOpenWindow(pageUrl,width,height)
{var stl_wnd=document.getElementById('stl_wnd_div');var stl_board=document.getElementById('stl_wnd_board');var wnd_frame=document.getElementById('stl_wnd_frame');if(stl_wnd){stl_wnd.style.width=width+'px';stl_wnd.style.height=height+'px';stl_board.style.display='block';stl_board.style.top=(100+document.documentElement.scrollTop)+'px';stl_wnd.style.visible='hidden';stl_wnd.style.display='block';var url;if(pageUrl.indexOf('?')==-1){url=pageUrl+'?_r='+Math.random();}else{url=pageUrl+'&_r='+Math.random();}
wnd_frame.src=url;}}
</script>
";
            }
            else if (pageJsName == Const.JsUserScript)
            {
                retVal = $@"
<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.Stl.JsPageScript)}""></script>
<script type=""text/javascript"">stlInit('{SiteFilesAssets.GetUrl(ApiUrl, string.Empty)}', '{Site.Id}', {Site.WebUrl.TrimEnd('/')}');</script>
<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.Stl.JsUserScript)}""></script>";
            }
            else if (pageJsName == Const.JsInnerCalendar)
            {
                retVal = $@"<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.DatePicker.Js)}""></script>";
            }
            else if (pageJsName == Const.JsStaticAdFloating)
            {
                retVal =
                    $@"<script type=""text/javascript"" src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.Static.JsStaticAdFloating)}""></script>";
            }
            else if (pageJsName == Const.JsAhTranslate)
            {
                retVal =
                    $@"<script src=""{SiteFilesAssets.GetUrl(ApiUrl, SiteFilesAssets.TwCn.Js)}"" charset=""{SiteFilesAssets.TwCn.Charset}"" type=""text/javascript""></script>";
            }
            return retVal;
        }

        public string HeadCodesHtml
        {
            get
            {
                var builder = new StringBuilder();

                //builder.Append(
                //$@"<script>var $pageInfo = {{siteId : {SiteId}, channelId : {PageChannelId}, contentId : {PageContentId}, siteUrl : ""{Site.WebUrl.TrimEnd('/')}"", rootUrl : ""{PageUtils.GetRootUrl(string.Empty).TrimEnd('/')}"", apiUrl : ""{ApiUrl.TrimEnd('/')}""}};</script>").AppendLine();

                foreach (var key in HeadCodes.Keys)
                {
                    builder.Append(HeadCodes[key]);
                }
                return builder.ToString();
            }
        }

        public string BodyCodesHtml
        {
            get
            {
                var builder = new StringBuilder();
                foreach (var key in BodyCodes.Keys)
                {
                    builder.Append(BodyCodes[key]);
                }
                return builder.ToString();
            }   
        }

        public string FootCodesHtml
        {
            get
            {
                var builder = new StringBuilder();
                foreach (var key in FootCodes.Keys)
                {
                    builder.Append(FootCodes[key]);
                }
                return builder.ToString();
            }
        }
    }
}

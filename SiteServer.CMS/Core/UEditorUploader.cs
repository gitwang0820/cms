﻿using System;
using System.Web;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using SiteServer.CMS.Context.Enumerations;
using SiteServer.CMS.Model;
using SiteServer.Utils;

namespace SiteServer.CMS.Core
{
    public class UEditorUploader
    {
        Site site = null;
        EUploadType uploadType = EUploadType.Image;

        string state = "SUCCESS";
        string URL = null;
        string currentType = null;
        string originalName = null;
        HttpPostedFile uploadFile = null;

        public UEditorUploader(Site site, EUploadType uploadType)
        {
            this.site = site;
            this.uploadType = uploadType;
        }

        public async Task<Hashtable> upFileAsync(HttpContext cxt)
        {
            try
            {
                uploadFile = cxt.Request.Files[0];
                originalName = uploadFile.FileName;
                currentType = PathUtils.GetExtension(originalName);

                var localDirectoryPath = PathUtility.GetUploadDirectoryPath(site, uploadType);
                var localFileName = PathUtility.GetUploadFileName(site, originalName);
                var localFilePath = PathUtils.Combine(localDirectoryPath, localFileName);

                //格式验证
                if (!PathUtility.IsUploadExtenstionAllowed(uploadType, site, currentType))
                {
                    state = "不允许的文件类型";
                }
                //大小验证
                if (!PathUtility.IsUploadSizeAllowed(uploadType, site, uploadFile.ContentLength))
                {
                    state = "文件大小超出网站限制";
                }
                //保存图片
                if (state == "SUCCESS")
                {
                    uploadFile.SaveAs(localFilePath);
                    URL = await PageUtility.GetSiteUrlByPhysicalPathAsync(site, localFilePath, true);
                    //URL = pathbase + filename;
                    if (uploadType == EUploadType.Image)
                        //添加水印
                        FileUtility.AddWaterMark(site, localFilePath);
                }
            }
            catch (Exception e)
            {
                state = e.Message;
                URL = "";
            }
            return getUploadInfo();
        }

        public async Task<Hashtable> upScrawlAsync(HttpContext cxt, string base64Data)
        {
            FileStream fs = null;
            try
            {
                var fileExtension = ".png";
                var localDirectoryPath = PathUtility.GetUploadDirectoryPath(site, fileExtension);
                var fileName = Guid.NewGuid() + fileExtension;
                var localFilePath = PathUtils.Combine(localDirectoryPath, fileName);
                fs = File.Create(localFilePath);
                var bytes = Convert.FromBase64String(base64Data);
                fs.Write(bytes, 0, bytes.Length);

                URL = await PageUtility.GetSiteUrlByPhysicalPathAsync(site, localFilePath, true);
            }
            catch
            {
                state = "未知错误";
                URL = "";
            }
            finally
            {
                fs.Close();
            }
            return getUploadInfo();
        }

        public string getOtherInfo(HttpContext cxt, string field)
        {
            string info = null;
            if (cxt.Request.Form[field] != null && !String.IsNullOrEmpty(cxt.Request.Form[field]))
            {
                info = field == "fileName" ? cxt.Request.Form[field].Split(',')[1] : cxt.Request.Form[field];
            }
            return info;
        }

        private Hashtable getUploadInfo()
        {
            var infoList = new Hashtable();

            infoList.Add("state", state);
            infoList.Add("url", URL);

            if (currentType != null)
                infoList.Add("currentType", currentType);
            if (originalName != null)
                infoList.Add("originalName", originalName);
            return infoList;
        }
    }
}

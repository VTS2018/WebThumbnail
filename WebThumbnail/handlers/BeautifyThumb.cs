using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using VTS.Common;
using WebThumbnail.Service;

namespace WebThumbnail.Handlers
{
    public class BeautifyThumb : IHttpHandler
    {
        #region ProcessRequest===============
        public void ProcessRequest(HttpContext Context)
        {
            Context.Response.Clear();
            Context.Response.ClearHeaders();
            Context.Response.ClearContent();
            Context.Response.ContentType = "image/jpeg";

            //接收参数
            string url = VTSRequest.GetRawUrl();
            //图片名字
            string md5 = MD5Encrypt.DataToMD5(url);

            //////////////开始分文件夹/////////////////
            string folderName = md5.Substring(0, 2);
            string imageDir = Context.Server.MapPath("/cache/cache_images/" + folderName);
            if (!System.IO.Directory.Exists(imageDir))
            {
                System.IO.Directory.CreateDirectory(imageDir);
            }
            //////////////结束分文件夹/////////////////

            //物理地址
            string imageSavePath = string.Concat(imageDir, "\\", md5, ".jpg"); //Context.Server.MapPath("/cache/cache_images/" + md5 + ".jpg");

            //分析参数  http://beautify.afuli.mobi 
            //示例参数：/https/storage.googleapis.com/140x80/cut/forward/beautify/Pics/1007/005/7A8A25209579C10A943A13E4C27AF54/14.jpg
            string[] urlarrs = url.Replace("http://", "").TrimStart('/').Split('/');

            //urlarrs[0]=thumb.afuli.mobi
            //urlarrs[0]=https

            //urlarrs[1]=storage.googleapis.com
            //urlarrs[2]=140x80

            //urlarrs[3]=cut
            //urlarrs[4]=forward
            //urlarrs[5]=beautify

            //得到原始URL
            string imgUrl = string.Empty;

            //得到相对URL：/Pics/1007/005/7A8A25209579C10A943A13E4C27AF54/14.jpg
            string imgRelUrl = string.Empty;

            int beautify = url.IndexOf("beautify");

            if (beautify != -1)
            {
                //表示找到标识符beautify
                beautify += "beautify".Length;
                imgRelUrl = url.Substring(beautify);
                imgUrl = string.Concat(urlarrs[0], "://", urlarrs[1], imgRelUrl);

                //Context.Response.Write(imgUrl + "<br />");
            }

            //最终参数
            int w = Convert.ToInt32(urlarrs[2].Substring(0, urlarrs[2].IndexOf('x')));
            int h = Convert.ToInt32(urlarrs[2].Substring(urlarrs[2].IndexOf('x') + 1));
            string model = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(urlarrs[3]);
            //int zc = VTSRequest.GetQueryInt("zc");
            string src = imgUrl;

            //Context.Response.Write(w + "<br />");
            //Context.Response.Write(h + "<br />");
            //Context.Response.Write(imgUrl + "<br />");
            /***************************************************************************************************************************************************************/

            byte[] b = null;
            if (File.Exists(imageSavePath))
            {
                #region 缓存策略
                //正确显示图片 才设置图片缓存策略
                //配置成一个星期 168小时
                Context.Response.Cache.SetExpires(DateTime.Now.AddHours(168));
                //下面的代码示例演示如何设置 Cache-Control: max-age 标题，为 0 小时，30 分钟和 0 秒。
                TimeSpan ts = new TimeSpan(168, 0, 0);
                Context.Response.Cache.SetMaxAge(ts);

                //设置Etag
                //Context.Response.Cache.SetETag(md5.ToLower());
                #endregion

                #region 响应输出
                b = VTSCommon.GetPictureData(imageSavePath);
                Context.Response.OutputStream.Write(b, 0, b.Length);
                #endregion
            }
            else
            {
                #region 修改类型
                Context.Response.ContentType = "image/gif";
                #endregion

                #region 清除缓存
                Context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Context.Response.Cache.SetNoStore();
                #endregion

                #region 后台生成
                ImagePackage image = new ImagePackage();
                image.Src = src;
                image.SavePath = imageSavePath;
                image.Width = w;
                image.Height = h;
                image.Model = model;

                WaitCallback callBack = new WaitCallback(GeneratePicture);
                ThreadPool.QueueUserWorkItem(callBack, image);
                #endregion

                #region 响应输出
                //随机响应
                //imageSavePath = Context.Server.MapPath(string.Concat("/common/images/random/tb", ImageRandom.GetRandomInt().ToString(), ".jpg"));
                imageSavePath = Context.Server.MapPath("/common/images/process/process.gif");
                b = VTSCommon.GetPictureData(imageSavePath);
                Context.Response.OutputStream.Write(b, 0, b.Length);
                #endregion
            }
            Context.ApplicationInstance.CompleteRequest();
        }
        #endregion

        #region 生成缩略图===================
        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="obj"></param>
        public void GeneratePicture(object obj)
        {
            try
            {
                if (obj == null) { return; }
                ImagePackage data = (ImagePackage)obj;
                //Thumbnail.MakeRemoteThumbnailImage(data.Src, data.SavePath, data.Width, data.Height, "Cut");
                Thumbnail.MakeRemoteThumbnailImage(data.Src, data.SavePath, data.Width, data.Height, data.Model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region IsReusable===================
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        #endregion
    }
}
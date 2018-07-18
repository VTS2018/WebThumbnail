using System;
using System.Web;
using System.IO;
using System.Threading;

using VTS.Common;
using WebThumbnail.Service;

namespace WebThumbnail.Web
{
    #region 同步方式
    /// <summary>
    /// Timthumb 的摘要说明
    /// </summary>
    public class Timthumb : IHttpHandler
    {
        #region 处理请求=====================
        public void ProcessRequest(HttpContext Context)
        {
            #region Note
            //<img src="${siteurl}Timthumb.ashx?src=${item.Images.Bigimg}&amp;h=95&amp;w=142&amp;zc=1" width="142" height="95" alt="${item.title}" />
            #endregion

            Context.Response.Clear();
            Context.Response.ClearHeaders();
            Context.Response.ClearContent();
            Context.Response.ContentType = "image/jpeg";

            //宽度
            int w = VTSRequest.GetQueryInt("w");
            //高度
            int h = VTSRequest.GetQueryInt("h");
            //预留
            int zc = VTSRequest.GetQueryInt("zc");
            //图片地址
            string src = VTSRequest.GetQueryString("src");

            //原始 URL
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

            //VTS.Log.LogOut.Info(string.Format("ManagedThreadId:{0} src:{1},url:{2}", System.Threading.Thread.CurrentThread.ManagedThreadId, src, url));
            /*******************************************************************************************************************************************************/
            byte[] b = null;

            if (File.Exists(imageSavePath))
            {
                #region 图片已存在

                #region 缓存策略
                //正确显示图片 才设置图片缓存策略
                //配置成一个星期 168小时
                Context.Response.Cache.SetExpires(DateTime.Now.AddHours(168));
                //下面的代码示例演示如何设置 Cache-Control: max-age 标题，为 0 小时，30 分钟和 0 秒。
                TimeSpan ts = new TimeSpan(168, 0, 0);
                Context.Response.Cache.SetMaxAge(ts);
                #endregion

                b = VTSCommon.GetPictureData(imageSavePath);
                Context.Response.OutputStream.Write(b, 0, b.Length);
                #endregion
            }
            else
            {
                #region 图片不存在
                ImagePackage image = new ImagePackage();
                image.Src = src;
                image.SavePath = imageSavePath;
                image.Width = w;
                image.Height = h;

                WaitCallback callBack = new WaitCallback(GeneratePicture);
                ThreadPool.QueueUserWorkItem(callBack, image);

                imageSavePath = Context.Server.MapPath(string.Concat("/common/images/random/tb", ImageRandom.GetRandomInt().ToString(), ".jpg"));
                b = VTSCommon.GetPictureData(imageSavePath);

                //清除缓存头策略
                Context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Context.Response.Cache.SetNoStore();
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
                Thumbnail.MakeRemoteThumbnailImage(data.Src, data.SavePath, data.Width, data.Height, "Cut");
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
    #endregion

    #region 异步方式
    /*
    /// <summary>
    /// Timthumb 的摘要说明
    /// </summary>
    public class Timthumb : IHttpAsyncHandler
    {
        public Timthumb() { }

        public void ProcessRequest(HttpContext context)
        {
            throw new InvalidOperationException();
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            //context.Response.Write("<p>Begin IsThreadPoolThread is " + Thread.CurrentThread.IsThreadPoolThread + "</p>\r\n");
            AsynchOperation asynch = new AsynchOperation(cb, context, extraData);
            asynch.StartAsyncWork();
            return asynch;
        }

        public void EndProcessRequest(IAsyncResult result)
        {

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AsynchOperation : IAsyncResult
    {
        private bool _completed;
        private Object _state;
        private AsyncCallback _callback;
        private HttpContext _context;

        bool IAsyncResult.IsCompleted { get { return _completed; } }
        object IAsyncResult.AsyncState { get { return _state; } }
        WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
        bool IAsyncResult.CompletedSynchronously { get { return false; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="context"></param>
        /// <param name="state"></param>
        public AsynchOperation(AsyncCallback callback, HttpContext context, Object state)
        {
            _callback = callback;
            _context = context;
            _state = state;
            _completed = false;
        }

        public void StartAsyncWork()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(StartAsyncTask), null);
        }

        private void StartAsyncTask(Object workItemState)
        {
            //Thread.Sleep(3000);
            //_context.Response.Write("<p>Completion IsThreadPoolThread is " + Thread.CurrentThread.IsThreadPoolThread + "</p>\r\n");
            //_context.Response.Write("Hello World from Async Handler!");

            HttpContext context = _context;
            context.Response.ContentType = "image/jpeg";

            string src = VTSRequest.GetQueryString(context, "src");
            int w = VTSRequest.GetQueryInt(context, "w");
            int h = VTSRequest.GetQueryInt(context, "h");
            int zc = VTSRequest.GetQueryInt(context, "zc");
            string url = VTSRequest.GetRawUrl(context);

            string md5 = MD5Encrypt.DataToMD5(url);
            string imageSavePath = context.Server.MapPath("/cache/cache_images/" + md5 + ".jpg");
            if (!File.Exists(imageSavePath))
            {
                Thumbnail.MakeRemoteThumbnailImage(src, imageSavePath, w, h, "Cut");
            }
            byte[] b = VTSCommon.GetPictureData(imageSavePath);
            context.Response.OutputStream.Write(b, 0, b.Length);
            _completed = true;
            _callback(this);
        }
    }
    */
    #endregion
}
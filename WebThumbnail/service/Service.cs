using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebThumbnail.Service
{
    #region Image Random
    public class ImageRandom
    {
        #region 随机变量
        /// <summary>
        /// 随机变量 RM
        /// </summary>
        static Random rm = new Random();
        #endregion

        #region 生成随机
        /// <summary>
        /// 说明：Y 生成一个随机数 用来索引特色图片
        /// </summary>
        /// <returns></returns>
        public static int GetRandomInt()
        {
            return rm.Next(1, 21);
        }
        #endregion
    }
    #endregion

    #region ImagePackage
    /// <summary>
    /// 处理缩略图
    /// </summary>
    public class ImagePackage
    {
        /// <summary>
        /// 图片地址
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// 保存的路径
        /// </summary>
        public string SavePath { get; set; }

        /// <summary>
        /// 宽度
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 高度
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// model
        /// </summary>
        public string Model { get; set; }
    }
    #endregion
}
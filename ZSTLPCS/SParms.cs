using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZSTJPCS
{
  public  static class SParms
    {
        /// <summary>登录的用户名
        /// 
        /// </summary>
        public static string User { get; set; }

        public static bool RFIDEnable { get; set; } = true;

        public static bool DebugMode { get; set; } = false;


        #region 常量字段和类型

        /// <summary>程序初始化、未从数据库查询到数据等非正常情况时控件的文字
        /// 
        /// </summary>
        public const string NothingStr = "--";

        /// <summary>代表默认情况的颜色 - 深蓝
        /// 
        /// </summary>
        public static  Color DefultColor = Color.Lime;

        /// <summary>代表正确的情况的颜色 - 亮绿
        /// 
        /// </summary>
        public static readonly Color GoodColor = Color.Lime;

        /// <summary>代表错误的情况的颜色 - 红色
        /// 
        /// </summary>
        public static readonly Color ErrColor = Color.Red;

        /// <summary>代表一般的情况的颜色 - 白色
        /// 
        /// </summary>
        public static readonly Color NomalColor = Color.White;

        /// <summary>代表警示的情况的颜色 - 黄色
        /// 
        /// </summary>
        public static readonly Color AlarmColor = Color.Yellow;

        #endregion
    }
}

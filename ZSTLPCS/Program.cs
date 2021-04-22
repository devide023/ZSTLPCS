using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QLib;

namespace ZSTJPCS
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //AtlasConnecter atls = new AtlasConnecter(AtlasConnecter.AtlasControlerType.Multiple);
            //atls.SelectPro("02S");

            //A3_axis = 0x44;
            //A3_proNum = 11;
            //write(0, PRO);

            //var asd = AtlasConnecter.RefuseCommand.asd;
            //string allstr = "";
            //foreach (var a in asd)
            //{
            //    var s = a.Value.Trim();
            //    allstr += "{" + a.Key.ToString().PadLeft(2, '0') + ",\"" + s + "\" },\r\n";
            //}

            #region 程序只启动一次

            if (!QLib.QL_OneApplication.Init())
            {
                MessageBox.Show("程序不能打开两次");
                return;
            }

            #endregion
            #region 异常捕获

            QLib.QL_ExceptionCatch.Init();
            QLib.QEvent.ApplicationErrorEvent.Wrong_ApplicationError += (s) => { AppMessage.Add(s, AppMessage.MsgType.应用程序错误); MessageBox.Show(s); };

            #endregion
            Application.Run(new Login());
        }





        //public static bool write(int addr, int value)
        //{
        //    byte[] array = new byte[] { 67, 80, 0, 0, 0, 6, 1, 6, 0, 0, 0, 1 };
        //    array[8] = (byte)(addr % 65536 >> 8);
        //    array[9] = (byte)(addr % 65536 % 256);
        //    array[10] = (byte)(value % 65536 >> 8);
        //    array[11] = (byte)(value % 65536 % 256);
        //    return true;
        //}

        ///// <summary>轴定义
        ///// 
        ///// </summary>
        //public static int A3_axis = 0;
        ///// <summary>程序号
        ///// 
        ///// </summary>
        //public static int A3_proNum = 0;
        ///// <summary>轴<8+程序号
        ///// 
        ///// </summary>
        //public static int PRO { get { return (A3_axis << 8) + A3_proNum; } }
    }
}

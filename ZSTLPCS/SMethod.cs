using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Deployment.Application;

namespace ZSTJPCS
{
    public static class SMethod
    {
        public static readonly System.Drawing.Color MyBackColor = System.Drawing.Color.FromArgb(17, 51, 88);
        public static readonly System.Drawing.Color MyForeColor = System.Drawing.Color.White;

        /// <summary>使本系统中的窗体具有统一的前后景色
        /// 
        /// </summary>
        /// <param name="f"></param>
        public static void InitBackForeColor(Form f)
        {
            InitBackForeColor_Control(f);
        }

        static void InitBackForeColor_MenuStrip(ToolStripItem tsi)
        {
            if (tsi is ToolStripMenuItem tsmi)
            {
                tsi.BackColor = MyBackColor;
                tsi.ForeColor = MyForeColor;
                for (int i = 0; i < tsmi.DropDownItems.Count; i++)
                {
                    var ms_temp = tsmi.DropDownItems[i];
                    InitBackForeColor_MenuStrip(ms_temp);
                }
            }
        }

        static void InitBackForeColor_Control(Control c)
        {
            foreach (Control c_temp in c.Controls)
            {
                c_temp.BackColor = MyBackColor;
                c_temp.ForeColor = MyForeColor;
                InitBackForeColor_Control(c_temp);

                if (c_temp is MenuStrip ms)
                {
                    for (int i = 0; i < ms.Items.Count; i++)
                    {
                        var tsi = ms.Items[i];
                        InitBackForeColor_MenuStrip(tsi);
                    }
                }
                if (c_temp is DataGridView dgv)
                {
                    dgv.DefaultCellStyle.BackColor = MyBackColor;
                }
            }
        }
    }
}

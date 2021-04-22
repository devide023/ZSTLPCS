using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZSTJPCS
{
    public partial class ToolAlarm : Form
    {
        public ToolAlarm()
        {
            InitializeComponent();
        }

        /// <summary>弹出一个窗口并在倒计时若干毫秒后关闭
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="millsecond">默认为0则不关闭</param>
        public ToolAlarm(string msg, int millsecond = 0)
        {
            this.InitializeComponent();
            this.Text = msg;
            this.button1.Text = msg;

            //倒计时处理
            if (millsecond != 0)
            {
                CloseTime = DateTime.Now.AddMilliseconds(millsecond);
                //TotalMilliseconds = millsecond;
                timer1.Enabled = true;
                button1.Enabled = false;
                timer1_Tick(null, null);
            }
        }

        /// <summary>倒计时中预计的关闭时间
        /// 
        /// </summary>
        DateTime CloseTime;

        private void 确认换型_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        /// <summary>弹出一个窗口并在倒计时若干毫秒后关闭
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="millsecond"></param>
        public static void ShowDialog(string msg, int millsecond = 0)
        {
            ToolAlarm ta = new ToolAlarm(msg, millsecond);
            ta.ShowDialog();
        }

        /// <summary>自动关闭窗口倒计时
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            var seconds = (DateTime.Now - CloseTime).TotalSeconds;
            if (seconds >= 0)
            {
                this.Close();
            }
            else
            {
                this.Text = "自动关闭在" + Math.Round(Math.Abs(seconds)) + "秒后";
            }
        }
    }
}

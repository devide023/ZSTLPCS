using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QLib;

namespace ZSTJPCS.UI
{
    /// <summary>一个用来表示通讯状态的加强型Label,来用上下两部分的背景颜色分别表示通讯资源是否连接 与 通讯对象是否正常
    /// 
    /// </summary>
    public partial class UCButton_CommcationStateShow : Button
    {
        public UCButton_CommcationStateShow()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.FillRectangle(IsConnected_ ? OKColor : ErrColor, 0, 0, this.Width, this.Height / 2);
            g.FillRectangle(IsCommcation_ && IsConnected_ ? OKColor : ErrColor, 0, this.Height / 2, this.Width, this.Height / 2);
            g.DrawString(this.Text, this.Font, new SolidBrush(ForeColor), new PointF(this.Width / 2, this.Height / 2), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            //g.DrawString(this.Text, this.Font, new SolidBrush(ForeColor), pevent.ClipRectangle.GetCenter(), ContentAlignment.MiddleCenter);
            //base.OnPaintBackground(pevent);
            //base.OnPaint(pevent);
        }

        //protected override void OnPaintBackground(PaintEventArgs pevent)
        //{
            //base.OnPaintBackground(pevent);
            //var g = pevent.Graphics;
            //g.FillRectangle(IsConnected_ ? OKColor : ErrColor, 0, 0, this.Width, this.Height / 2);
            //g.FillRectangle(IsCommcation_ ? OKColor : ErrColor, 0, this.Height / 2, this.Width, this.Height / 2);
        //}


        bool IsConnected_ = false;
        bool IsCommcation_ = false;

        /// <summary>通讯资源是否打开或建立连接
        /// 
        /// </summary>
        public bool IsConnected { get { return IsConnected_; } set { IsConnected_ = value; this.Invalidate(); } }

        /// <summary>通讯对象是否正常通讯
        /// 
        /// </summary>
        public bool IsCommcation { get { return IsCommcation_; } set { IsCommcation_ = value; this.Invalidate(); } }

        public Brush OKColor { get; set; } = Brushes.Lime;

        public Brush ErrColor { get; set; } = Brushes.Red;
    }
}

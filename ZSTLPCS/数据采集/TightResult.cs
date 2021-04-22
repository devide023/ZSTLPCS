using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZSTJPCS
{
    /// <summary>从设备返回的一次拧紧结果
    /// 
    /// </summary>
    public class TightResult
    {
        public TightResult(bool as_ok,decimal as_nl,decimal as_angle)
        {
            TorqueStatus = as_ok;
            Torque = as_nl;
            Angle = as_angle;
            Time = DateTime.Now;
        }

        /// <summary>本次拧紧是否合格
        /// 
        /// </summary>
        public bool TorqueStatus { get; set; }

        /// <summary>扭力
        /// 
        /// </summary>
        public decimal Torque { get; set; }

        /// <summary>角度
        /// 
        /// </summary>
        public decimal Angle { get; set; }

        /// <summary>时间
        /// 
        /// </summary>
        public DateTime Time { get; set; }
    }
}
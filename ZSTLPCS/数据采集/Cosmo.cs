using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using QLib;

namespace ZSTJPCS
{
    public class Cosmo : DataCollecter
    {
        #region 构造函数和初始化

        public Cosmo(string as_ComPort, int BaudRate)
        {
            sp.PortName = as_ComPort;
            sp.BaudRate = BaudRate;
            //sp.DiscardNull = false;
        }

        public Cosmo(string as_ComConfigStr)
        {
            QL_Commcation_COM.ComConfigStr.GetComConfig(as_ComConfigStr, sp);
        }

        public bool Init(out string err)
        {
            bool re = Open(out err);
            sp.DataReceived += Sp_DataReceived;
            new System.Threading.Thread(() =>
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(5000);
                    if (!sp.IsOpen)
                    {
                        Open(out _);
                    }
                }
            })
            { IsBackground = true }.Start();
            return re;
        }

        #endregion

        #region 私有字段和属性

        private System.IO.Ports.SerialPort sp = new System.IO.Ports.SerialPort();

        /// <summary>干检仪停止检测
        /// 
        /// </summary>
        private const string StartGJ = "STT\r\n";

        /// <summary>干检仪开始检测
        /// 
        /// </summary>
        private const string StopGJ = "STP\r\n";

        string ReceDataCache = "";

        #endregion

        #region 私有方法

        /// <summary>尝试打开串口
        /// 
        /// </summary>
        /// <param name="err"></param>
        /// <returns></returns>
        bool Open(out string err)
        {
            try
            {
                sp.Open();
            }
            catch (Exception exc)
            {
                err = exc.Message;
                return false;
            }
            err = "";
            return true;
        }

        private void Sp_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //if (e.EventType == System.IO.Ports.SerialData.Eof)
            //{
            LastCommcationTime = DateTime.Now;
            byte[] b = new byte[sp.BytesToRead];
            sp.Read(b, 0, b.Length);
            AppMessage.Add("接收干检数据", AppMessage.MsgType.干检, false, AppMessage.ImportantEnum.Normal, b.ToStandardString(false, b.Length));
            ReceDataCache += System.Text.Encoding.ASCII.GetString(b);
            if (ReceDataCache.Length > 10000)//防止异常情况下的内存爆炸
            {
                ReceDataCache = ReceDataCache.Substring(0, ReceDataCache.Length);
            }
            var ms = System.Text.RegularExpressions.Regex.Matches(ReceDataCache, "[#].+?[\r]");
            if (ms.Count == 0)
            {
                return;
            }
            ReceDataCache = "";
            var str = ms[ms.Count - 1].Value;
            if (str.Length < 50 || str.Length > 75)
            {
                return;//向干检仪发送启动或停止指令的时候对方可能会做简短的应答
            }
            GJResult re = GJResult.Create(str, out string err);
            if (err == "")
            {
                OnGetData_Event(re);
                //AddReceData(re);
            }
            else
            {
                OnMsg_Event(err + " | " + b.ToStandardString(false, b.Length));
            }
            //}
        }



        #endregion

        #region 继承的字段、属性、方法

        public override bool DC_IsConnected { get => sp == null ? false : sp.IsOpen; }

        public override bool DC_IsCommcation { get => true; }

        public override void DC_Channel(string channel)
        {
            if (sp.IsOpen)
            {
                if (string.IsNullOrEmpty(channel))//不讲道理,因为数据库里工艺人员大部分是0程序号的就没有维护
                {
                    channel = "0";
                }
                if (channel.IsInt())
                {
                    int channel_int = channel.ToInt();
                    if (channel_int >= 0 && channel_int <= 15)
                    {
                        sp.Write("WCHN " + channel_int.ToString().PadLeft(2, '0') + "\r\n");
                    }
                }
            }
        }

        public override void DC_Start()
        {
            if (sp.IsOpen)
            {
                sp.Write(StartGJ);
            }
        }

        public override void DC_Start(string channel)
        {
            if (sp.IsOpen)
            {
                if (string.IsNullOrEmpty(channel))//不讲道理,因为数据库里工艺人员大部分是0程序号的就没有维护
                {
                    channel = "0";
                }
                if (channel.IsInt())
                {
                    int channel_int = channel.ToInt();
                    if (channel_int >= 0 && channel_int <= 15)
                    {
                        string cmd_temp = "#00 00 " + channel_int.ToString().PadLeft(2, '0') + " STT:";
                        byte re1_temp = QLib.QL_ADDCheck.CheckADD(System.Text.Encoding.ASCII.GetBytes(cmd_temp));
                        byte re2_temp = (byte)(~re1_temp+1);
                        cmd_temp += Convert.ToString(re2_temp, 16).ToUpper().PadLeft(2, '0') + "\r\n"; 
                        sp.Write(cmd_temp);
                    }
                }
            }
        }

        public override void DC_Lock(bool as_Enable)
        {
            return;
        }

        public override void DC_ByPass()
        {
            return;
        }

        public override void DC_Stop()
        {
            if (sp.IsOpen)
            {
                //sp.Write(StopGJ, 0, StopGJ.Length);
                sp.Write(StopGJ);
            }
        }

        public override void DC_Close()
        {

        }

        #endregion

        /// <summary>一次干检结果数据
        /// 
        /// </summary>
        public class GJResult
        {
            GJResult()
            {
            }

            /// <summary>单次检测结果是否合格
            /// 
            /// </summary>
            public bool OK { get; set; }

            /// <summary>单次检测结果详细描述
            /// 
            /// </summary>
            public string OKRe { get; set; }

            /// <summary>泄漏值
            /// 
            /// </summary>
            public double XLZ { get; set; }

            public double DP { get; set; }

            /// <summary>泄漏值上限
            /// 
            /// </summary>
            public double Hi { get; set; }

            /// <summary>泄漏值下限
            /// 
            /// </summary>
            public double Lo { get; set; }

            public static GJResult Create(string str, out string err)
            {
                GJResult re = new GJResult();
                //if (str.Length < 50 || str.Length > 75)//数据接收没有完成
                //{
                //    err = "设备数据长度不正确:" + str.Length;
                //    return null;
                //}
                int int_temp = 0;
                //int int_temp = str.IndexOf("#");
                //if (int_temp == -1)
                //{
                //    err = "设备数据不以#开始无法解析";
                //    return null;
                //}
                switch (str.Substring(int_temp + 7, 1))
                {
                    case "1":
                        re.OKRe = "Lo NG";
                        re.OK = false;
                        break;
                    case "2":
                        re.OKRe = "GOOD";
                        re.OK = true;
                        break;
                    case "4":
                        re.OKRe = "Hi NG";
                        re.OK = false;
                        break;
                    case "9":
                        re.OKRe = "LL NG";
                        re.OK = false;
                        break;
                    case "C":
                        re.OKRe = "HH NG";
                        re.OK = false;
                        break;
                    case "D":
                        re.OKRe = "Hi NG";
                        re.OK = false;
                        break;
                    default:
                        err = "设备数据异常无法正常解析";
                        break;
                }
                re.XLZ = str.Substring(int_temp + 9, 8).ToDouble();
                re.Hi = str.Substring(int_temp + 18, 8).ToDouble();
                re.Lo = str.Substring(int_temp + 27, 8).ToDouble();
                re.DP = str.Substring(int_temp + 36, 8).ToDouble();
                err = "";
                return re;
            }
        }
    }
}
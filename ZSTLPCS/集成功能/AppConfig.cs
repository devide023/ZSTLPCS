//#define MyDebugMode

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
//using QLib4;
using QLib;
using System.Configuration;
using System.Net;

namespace ZSTJPCS
{
    /// <summary>应用程序的数据库配置项
    /// 
    /// </summary>
    public static class AppConfig
    {
        /// <summary>本函数的调用应当在AppConfig数据库属性被注入后
        /// 
        /// </summary>
        /// <param name="as_PCIP">当前使用的有效IP地址</param>
        /// <returns></returns>
        public static void Init()
        {
            System.Threading.Thread th = new System.Threading.Thread(() =>
            {
                while (true)
                {
                    GetDynmaicConfigImm();
                    System.Threading.Thread.Sleep(60000);
                }
            })
            { IsBackground = true };
            th.Start();
        }

        /// <summary>立即从数据库获取动态日志等配置信息
        /// 
        /// </summary>
        /// <returns></returns>
        public static void GetDynmaicConfigImm()
        {
            //定时读取LogEnable
            var logenable = SQL.GetLogEnable(out string err);
            if (err == "")
            {
                LOGENABLE = logenable;
            }
            //写入程序最后运行时间
            SQL.SetApplicationLastRunTime(/*DateTime.Now,*/ out _);
        }

        #region 从数据库中读取的配置属性

        static int scx;

        /// <summary>对应到value.line
        /// 
        /// </summary>
        public static int SCX { get { return scx; } set { scx = value; RFIDSTARTINDEX = (value == 1 || value == 3) ? 0 : 1; } }

        /// <summary>对应到value.gongwei,比如Gang1
        /// 
        /// </summary>
        public static string GONGWEI { get; set; }

        /// <summary>RFID触发方式,0=自动触发模式,1=按钮触发模式,2=扫描绑定模式,3=A1A3线飞轮岗位模式,4=装夹岗位工作模式[永远不自动读取RFID,除非代码控制]
        /// 【从2020-06-02开始,原来1=按钮触发[包括扫描枪触发]被分解为1=按钮触发,2=扫描绑定模式】
        /// </summary>
        public static int RFIDRECIVETYPE { get; set; }

        /// <summary>当RFIDReciveType=0或4时,扫描RFID的频率,单位毫秒
        /// 
        /// </summary>
        public static int RFIDAUTOSCANINTERVAL { get; set; }

        /// <summary>是否开启机号缓存功能,0=不缓存,1=缓存
        /// 
        /// </summary>
        public static bool ISCACHE { get; set; }

        /// <summary>RFID工位类型,0=一般岗位[默认配置],1=A1A3线飞轮岗位[根据IP地址自动识别服务端和客户端]
        /// 
        /// </summary>
        [Obsolete]
        public static int GONGWEITYPE { get; set; }

        /// <summary>飞轮岗位服务端IP地址
        /// 
        /// </summary>
        public static IPEndPoint FEILUN_SERVERIP { get; set; }

        /// <summary>飞轮岗位客户端IP地址
        /// 
        /// </summary>
        public static IPEndPoint FEILUN_CLIENTIP { get; set; }

        /// <summary>线体主控PLC的IP地址,由于历史原因,不要太在意其名称
        /// 
        /// </summary>
        public static string FEILUN_PLCIP { get; set; }

        /// <summary>线体主控PLC的通讯端口号,由于历史原因,不要太在意其名称
        /// 
        /// </summary>
        public static int FEILUN_PLCPORT { get; set; }

        /// <summary>A1A3线专门用于从PLC接受员工脚踏板信号的PLC连接,由于历史原因,不要太在意其名称
        /// 
        /// </summary>
        public static IPEndPoint FeiLun_PLCIP2 { get; set; }

        /// <summary>是否存储日志记录到数据库
        /// 
        /// </summary>
        public static bool LOGENABLE { get; set; }

        /// <summary>RFID的尝试读取次数,默认为2
        /// 
        /// </summary>
        public static int RFIDREADTIMES { get; set; }

        /// <summary>程序语言模式,0=中文  1=双语(西班牙语和中文)
        /// 
        /// </summary>
        public static int LANG { get; set; }

        /// <summary>串口端口号&波特率等配置信息,对应于value.duankou,比如:9600,N,8,1
        /// 
        /// </summary>
        public static string ComPort { get; set; }

        /// <summary>工位名称,对应于value.stationName
        /// 
        /// </summary>
        public static string GWName { get; set; }

        /// <summary>当前本机有效的IP地址
        /// 
        /// </summary>
        public static string PCIP { get; set; }

        /// <summary>保存的工序号,是数据库里某些表的Work_Flow字段
        /// 
        /// </summary>
        public static int GX { get; set; }

        /// <summary>是否使用明匠板卡,对应于value.banka
        /// 
        /// </summary>
        [Obsolete]
        public static bool isBanKa { get; set; }

        /// <summary>使用的拧紧枪控制器类型,0=串口版阿特拉斯PF4000,1=网络版阿特拉斯PF4000,2=明匠板卡-匹配英格索兰,3=明匠板卡-匹配博世,5=阿特拉斯多轴拧紧
        /// 
        /// </summary>
        public static int TIGHTENCONTROLERTYPE { get; set; }

        /// <summary>拧紧枪控制器类型为网络通讯时的目标IP地址,如:使用网络版通讯的阿特拉斯通讯驱动时 阿特拉斯控制器的IP地址
        /// 
        /// </summary>
        public static IPEndPoint TIGHTENCONTROLERIP { get; set; }

        /// <summary>拧紧枪控制器使用指定的本地IP地址强制关联通讯,主要解决A3线飞轮岗位 英格索兰拧紧枪和线体主控PLC的IP在同一网段的 不同网卡不同网段问题 兰浩
        /// 
        /// </summary>
        public static IPEndPoint TIGHTENCONTROLERBINDINGIP { get; set; }

        /// <summary>程序允许运行的最低版本
        /// 
        /// </summary>
        public static string ALLOWLOWVERSION { get; set; }

        #endregion
        #region 衍生属性

        /// <summary>RFID读取开始地址，1和3线为0，2和4线为1
        /// 
        /// </summary>
        public static int RFIDSTARTINDEX = 0;

        public enum GX_EnumType
        {
            未定义,
            连杆 = 21,
            缸头 = 22,
            气门 = 23,
            干检 = 24,
            飞轮 = 25,
            点火器 = 26,
            化油器 = 28,
            绑定 = 99,
        };

        public static GX_EnumType GX_Enum { get; set; }

        public enum GXType_EnumType
        {
            未定义,
            扭力,
            干检,
            人工,
            绑定,
        }

        public static GXType_EnumType GXType_Enum { get; set; }

        /// <summary>获取衍生属性
        /// 
        /// </summary>
        public static void GetOtherProp()
        {
            //switch (GX)
            //{
            //    case 1://连杆
            //        GX = 21;
            //        break;
            //    case 2://缸头
            //        GX = 22;
            //        break;
            //    case 3://飞轮
            //        GX = 25;
            //        break;
            //    case 4://化油器
            //        GX = 28;
            //        break;
            //    case 5://干检
            //        GX = 24;
            //        break;
            //    case 12://气门
            //        GX = 23;
            //        break;
            //    case 13://点火器
            //        GX = 26;
            //        break;
            //    default:
            //        GX = 0;
            //        break;
            //}
            //switch (GX)
            //{
            //    case 1://连杆
            //    case 2://缸头
            //    case 3://飞轮
            //    case 4://化油器
            //    case 5://干检
            //    case 12://气门
            //    case 13://点火器
            //        GX_Enum = GX.ToEnum<GX_EnumType>();
            //        break;
            //    default:
            //        GX_Enum = GX_EnumType.未定义;
            //        break;
            //}

            GX_Enum = GX.ToEnum<GX_EnumType>();
            GXType_EnumType GXType_EnumType_Temp;
            switch (GX_Enum)
            {
                case GX_EnumType.未定义:
                default:
                    GXType_EnumType_Temp = GXType_EnumType.未定义;
                    break;
                case GX_EnumType.连杆:
                case GX_EnumType.化油器:
                case GX_EnumType.缸头:
                case GX_EnumType.飞轮:
                    GXType_EnumType_Temp = GXType_EnumType.扭力;
                    break;
                case GX_EnumType.气门:
                case GX_EnumType.点火器:
                    GXType_EnumType_Temp = GXType_EnumType.人工;
                    break;
                case GX_EnumType.干检:
                    GXType_EnumType_Temp = GXType_EnumType.干检;
                    break;
                case GX_EnumType.绑定:
                    GXType_EnumType_Temp = GXType_EnumType.绑定;
                    break;
            }
            GXType_Enum = GXType_EnumType_Temp;
        }

        #endregion
        #region 静态属性

#if MyDebugMode
                
        public static readonly IPEndPoint RFIDIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3001);
    
#else

        public static readonly IPEndPoint RFIDIP = new IPEndPoint(IPAddress.Parse("192.168.2.10"), 3001);

#endif

        #endregion
    }
}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using QLib;

//namespace ZSTJPCS
//{
//    /// <summary>博世枪使用的板卡通讯驱动
//    /// 
//    /// </summary>
//    [Obsolete("目前暂未使用博世控制器",true)]
//    public class MJBanKa1 : DataCollecter
//    {
//        System.IO.Ports.SerialPort sp = new System.IO.Ports.SerialPort();

//        public void DealCommcation(string str)
//        {
//            if (str.Length < 8 || str.Length > 10)//数据接收没有完成 返回
//            {
//                return;
//            }
//            if (str.IndexOf("H") > -1)//心跳包 返回
//            {
//                LastCommcationTime = DateTime.Now;
//                return;
//            }

//            str = str.Replace("\n", "").Replace(" ", " ").Replace("\t", "").Replace("\r", "");
//            switch (str)
//            {
//                #region 常规通信区域
//                case "*000ERR#":
//                    //tb_log.Text = "请继续您的操作....";
//                    break;

//                case "*000RET#":
//                    OnMsg_Event("拧紧枪复位成功");
//                    break;

//                case "*00LOCK#":
//                    OnMsg_Event("拧紧枪锁定");
//                    //if (value.gongxu != 3)
//                    //    tb_log.Text = "请您 扫条码..."; // "拧紧枪 锁定状态";
//                    break;

//                case "*UNLOCK#":
//                    OnMsg_Event("拧紧枪解锁");
//                    //if (value.gongxu != 3)
//                    //    tb_log.Text = "请您 拧螺丝..."; //"拧紧枪 解锁状态";
//                    break;

//                case "HHHHHHHH":
//                    //tb_log.Text = "收到板卡心跳包" + DateTime.Now.ToString("ss"); ;
//                    break;

//                case "*OUTERR#":
//                    OnMsg_Event("板卡输出错误,请联系维护人员");
//                    break;


//                case "*000P00#":
//                case "*000P01#":
//                case "*000P02#":
//                case "*000P03#":
//                case "*000P04#":
//                case "*000P05#":
//                case "*000P06#":
//                case "*000P07#":
//                case "*000P08#":
//                case "*000P09#":
//                case "*000P10#":
//                case "*000P11#":
//                case "*000P12#":
//                case "*000P13#":
//                case "*000P14#":
//                case "*000P15#":
//                    OnMsg_Event("切换" + str.Substring(5, 2) + "号程序成功");
//                    break;
//                #endregion
//                default:
//                    sp.Write("*0000OK#");//收到数据回应
//                    string str_temp = str;
//                    str_temp = str_temp.Replace("*", " ").Replace("Y", " ").Replace("N", " ").Replace("#", " ");
//                    if (str_temp.IsDecimal())
//                    {
//                        decimal nl = str_temp.ToDecimal();
//                        OnGetData_Event(new object[] { nl });
//                    }
//                    //value.dqnj = double.Parse(value.str_temp);
//                    //deal_pro_resualt_banka(str, value.gongxu); //处理数据结果
//                    break;
//            }
//        }
//    }
//}

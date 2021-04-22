using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Deployment.Application;

namespace ZSTJPCS
{
    /// <summary>用于控制ClickOnce应用程序更新的类
    /// 
    /// </summary>
    public static class ApplicationUpdate
    {
        /// <summary>DBVersion使用该关键词会永远保持更新
        /// 
        /// </summary>
        const string DBVersionKeyWord_New = "NEW";
        /// <summary>DBVersion使用该关键词会永远不会更新
        /// 
        /// </summary>
        const string DBVersionKeyWord_Old = "OLD";

        /// <summary>CheckForUpdate函数的返回结果,是=有可用的更新,否=没有可用的更新
        /// 
        /// </summary>
        public static bool CheckForUpdate_Result = false;

        public static string GetCurrentVersion()
        {
            return ApplicationDeployment.IsNetworkDeployed ? ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString() : "";
        }

        /// <summary>初始化程序自动更新模块,主要是调用CheckForUpdate函数,由于该函数耗时比较长,在程序初始化后立刻异步调用该线程完成是否更新的检测任务
        /// 
        /// </summary>
        public static void Init()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)//不是网络部署程序
            {
                return;
            }
            var ad = ApplicationDeployment.CurrentDeployment;
            CheckForUpdate_Result = ad.CheckForUpdate(false);//这里必须要false,否则会弹出窗口请用户选择是否更新程序
        }

        /// <summary>使用自有逻辑判断是否需要更新程序
        /// 
        /// </summary>
        /// <param name="DBVersion"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        public static bool CheckForUpdate(string DBVersion)
        {
            if (!ApplicationDeployment.IsNetworkDeployed)//不是网络部署程序
            {
                return false;
            }
            var ad = ApplicationDeployment.CurrentDeployment;
            if (!CheckForUpdate_Result)//不需要更新,已经是最新版了
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(DBVersion))
            {
                ToolAlarm.ShowDialog("允许运行的最低版本:" + DBVersion + "不正确,请提醒管理人员处理", 3000);
                return false;
            }
            if (DBVersion.ToUpper() == DBVersionKeyWord_New.ToUpper())
            {
                return true;
            }
            if (DBVersion.ToUpper() == DBVersionKeyWord_Old.ToUpper())
            {
                return false;
            }
            Version DBVersion_Temp;
            try
            {
                DBVersion_Temp = new Version(DBVersion);
            }
            catch
            {
                ToolAlarm.ShowDialog("允许运行的最低版本:" + DBVersion + "不正确,请提醒管理人员处理", 3000);
                return false;
            }
            return ad.CurrentVersion < DBVersion_Temp;
        }

        /// <summary>开始进行同步更新并提示相关信息
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool Update()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                return false;
            }
            var ad = ApplicationDeployment.CurrentDeployment;
            ToolAlarm.ShowDialog("该程序版本已经过期,即将更新到最新版!", 2000);
            try
            {
                ad.Update();
                ToolAlarm.ShowDialog("程序已经更新完成,请重新启动应用程序", 2000);
            }
            catch (Exception exc)
            {
                MessageBox.Show("无法更新程序,请检查网络或稍后再尝试:" + exc.Message.ToString());
                return false;
            }
            Application.Restart();
            return true;
        }

        #region 未使用的

        ///// <summary>检查并尝试更新程序
        ///// 
        ///// </summary>
        //private static bool InstallUpdateSyncWithInfo(string DBVersion,out string err)
        //{
        //    if (!ApplicationDeployment.IsNetworkDeployed)
        //    {
        //        err= "当前不是ClickOnce客户端,更新程序被中止";
        //        return false;
        //    }
        //    if (string.IsNullOrWhiteSpace(DBVersion))
        //    {
        //        err = "您允许保持的最低版本为空,更新程序被中止";
        //        return false;
        //    }
        //    var ad = ApplicationDeployment.CurrentDeployment;

        //    UpdateCheckInfo info = null;
        //    try
        //    {
        //        info = ad.CheckForDetailedUpdate();
        //    }
        //    catch (DeploymentDownloadException dde)
        //    {
        //        err = "在向客户端计算机下载 ClickOnce 清单或部署文件时发生错误:\r\n" + dde.Message;
        //        return false;
        //    }
        //    catch (InvalidDeploymentException ide)
        //    {
        //        err = "ClickOnce 未能读取部署或应用程序清单:\r\n" + ide.Message;
        //        return false;
        //    }
        //    catch (InvalidOperationException ioe)
        //    {
        //        err = "应用程序无法更新:\r\n" + ioe.Message;
        //        return false;
        //    }
        //    if (!info.UpdateAvailable)
        //    {
        //        err = "检测到没有新的更新";
        //        return false;
        //    }
        //    Version DBVersion_Temp = null;
        //    try
        //    {
        //        DBVersion_Temp = new Version(DBVersion);
        //    }
        //    catch
        //    {
        //        ToolAlarm.ShowDialog("数据库中维护的版本号" + DBVersion + "不正确,请提醒管理人员处理", 3000);
        //        return false;
        //    }
        //    if (ad.CurrentVersion >= DBVersion_Temp)
        //    {
        //        return;
        //    }
        //    ToolAlarm.ShowDialog("该程序版本已经过期,将更新到最新版!", 2000);
        //    try
        //    {
        //        ad.Update();
        //        ToolAlarm.ShowDialog("程序已经更新完成,即将重新启动!", 2000);
        //        Application.Restart();
        //    }
        //    catch (DeploymentDownloadException dde)
        //    {
        //        MessageBox.Show("无法更新程序,请检查网络或稍后再尝试:" + dde);
        //        return;
        //    }
        //}

        /// <summary>检查更新程序-微软默认
        /// 
        /// </summary>
        [Obsolete("只提供参考的微软默认方法", true)]
        private static void InstallUpdateSyncWithInfo()
        {
            if (!ApplicationDeployment.IsNetworkDeployed)
            {
                return;
            }
            var ad = ApplicationDeployment.CurrentDeployment;
            UpdateCheckInfo info = null;
            try
            {
                info = ad.CheckForDetailedUpdate();
            }
            catch (DeploymentDownloadException dde)
            {
                MessageBox.Show("在向客户端计算机下载 ClickOnce 清单或部署文件时发生错误:\r\n" + dde.Message);
                return;
            }
            catch (InvalidDeploymentException ide)
            {
                MessageBox.Show("ClickOnce 未能读取部署或应用程序清单:\r\n" + ide.Message);
                return;
            }
            catch (InvalidOperationException ioe)
            {
                MessageBox.Show("应用程序无法更新:\r\n" + ioe.Message);
                return;
            }
            if (!info.UpdateAvailable)
            {
                return;
            }
            bool doUpdate = true;
            if (!info.IsUpdateRequired)
            {
                DialogResult dr = MessageBox.Show("你需要更新到最新版本" + info.MinimumRequiredVersion.ToString() + "吗?", "版本更新提示", MessageBoxButtons.OKCancel);
                if (!(DialogResult.OK == dr))
                {
                    doUpdate = false;
                }
            }
            else
            {
                // Display a message that the app MUST reboot. Display the minimum required version.
                MessageBox.Show("This application has detected a mandatory update from your current " +
                    "version to version " + info.MinimumRequiredVersion.ToString() +
                    ". The application will now install the update and restart.",
                    "Update Available", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            if (doUpdate)
            {
                try
                {
                    ad.Update();
                    MessageBox.Show("The application has been upgraded, and will now restart.");
                    Application.Restart();
                }
                catch (DeploymentDownloadException dde)
                {
                    MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde);
                    return;
                }
            }
        }

        #endregion
    }
}
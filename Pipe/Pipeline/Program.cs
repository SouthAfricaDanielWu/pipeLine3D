using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NorthStar.Common;
using System.IO;
using log4net.Config;
using NorthStar.Pipeline.Common;
using Microsoft.Win32;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： Program
    ** 描述： 程序入口
    ** 作者： PLWhite
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SetAutoBootStatu(false);  
            #region 软件崩溃
            //处理未捕获的异常
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程异常
           // Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //处理非UI线程异常
           // AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            #endregion
            // 初始化系统变量
            InitAppEnv();
            //Application.Run(new FrmProgressBar("this",ProgressStyle.Single));
              Application.Run(new FrmMain());
        }

        private static void InitAppEnv()
        {
            // 日志
            FileInfo logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"Config\log4net.config");
            XmlConfigurator.ConfigureAndWatch(logCfg);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Utilities.Log(typeof(Program)).Error(sender.ToString() + "\t " + e.ExceptionObject.ToString());
        }
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Utilities.Log(typeof(Program)).Error(sender.ToString() + "\t " + e.Exception.StackTrace);
        }
        ///// <summary>  
        ///// 在注册表中添加、删除开机自启动键值  
        ///// </summary>  
        public static int SetAutoBootStatu(bool isAutoBoot)
        {
            try
            {
                string execPath = Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                if (isAutoBoot)
                {
                    rk2.SetValue("MyExec", execPath);
                    Console.WriteLine(string.Format("[注册表操作]添加注册表键值：path = {0}, key = {1}, value = {2} 成功", rk2.Name, "TuniuAutoboot", execPath));
                }
                else
                {
                    rk2.DeleteValue("MyExec", false);
                    Console.WriteLine(string.Format("[注册表操作]删除注册表键值：path = {0}, key = {1} 成功", rk2.Name, "TuniuAutoboot"));
                }
                rk2.Close();
                rk.Close();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[注册表操作]向注册表写开机启动信息失败, Exception: {0}", ex.Message));
                return -1;
            }
        }  
    }
}

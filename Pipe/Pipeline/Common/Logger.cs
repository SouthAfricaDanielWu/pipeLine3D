using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using log4net.Config;
using log4net;

namespace NorthStar.Common.Utility
{
    /********************************************************************************

    ** 类名称： Logger
    ** 描述：
    ** 作者： dlfan
    ** 创建时间： 2017/7/25 10:52:53
    ** 最后修改人： dlfan
    ** 最后修改时间：2017/7/27 10:52:53
    ** 版权所有 (C) :北斗星测绘地理信息研发中心

    *********************************************************************************/
    public class Logger
    {
        // 
       // static string fileName = System.Windows.Forms.Application.StartupPath + @"\日志.txt";
        private static ILog mLog = null;

        private static void init()
        {
            FileInfo logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"Config\log4net.config");
            XmlConfigurator.ConfigureAndWatch(logCfg);
            mLog = LogManager.GetLogger("myLog");
        }
  
        public static ILog Log
        {
            get{
                if (mLog == null)
                {
                    init();
                }
                return mLog;
        
            }
        }

    }
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using log4net.Config;
using log4net;

namespace NorthStar.Common.Log
{
    /********************************************************************************

    ** 类名称： Logger
    ** 描述：
    ** 作者： dlfan
    ** 创建时间： 2017/7/25 10:52:53
    ** 最后修改人： dlfan
    ** 最后修改时间：2017/7/27 10:52:53
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心

    *********************************************************************************/
    public class Logger
    {
        // 
       // static string fileName = System.Windows.Forms.Application.StartupPath + @"\日志.txt";
        private static ILog mLog = null;

        /// <summary>
        /// 获取目标类型为名的日志类
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ILog Log(Type type)
        {
            return LogManager.GetLogger(type);
        }
        public static ILog Log(string name)
        {
            return LogManager.GetLogger(name);
        }
  
        /// <summary>
        /// 获取AOP日志类
        /// </summary>
        public static ILog AopLog
        {
            get{
                if (mLog == null)
                {
                    mLog = LogManager.GetLogger("myLog");
                }
                return mLog;
        
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using NorthStar.Common.Log;

namespace NorthStar.Common
{
    /********************************************************************************

    ** 类名称： Utilities
    ** 描述： 工具类
    ** 作者： dlfan
    ** 创建时间： 2017/7/27 15:48:47
    ** 最后修改人：（无）
    ** 最后修改时间：2017/7/27 15:48:47
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心

    *********************************************************************************/
    public class Utilities
    {
        #region 日志
        public static ILog Log(string name)
        {
           return Logger.Log(name);
        }
        public static ILog Log(Type type)
        {
           return Logger.Log(type);
        }
        #endregion
    }
}

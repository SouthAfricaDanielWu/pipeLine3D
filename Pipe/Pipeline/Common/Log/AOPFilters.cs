﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NorthStar.Common.Log
{
    /********************************************************************************

    ** 类名称： AOPFilters
    ** 描述：
    ** 作者： dlfan
    ** 创建时间： 2017/7/25 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/7/25 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心

    *********************************************************************************/
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class LoggerAttribute : Attribute
    {
        // 输出级别
        public LoggerAttribute(string level)
        {
            this.mLevel = level;
        }

        private string mLevel;
        public string Level
        {
            get { return mLevel; }
            set { mLevel = value; }
        }
        
    }
    /// <summary>
    /// 对类过滤
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class    PointCutAttribute : Attribute
    {
        // 输出级别
        public PointCutAttribute(string matchingText,bool isInherit)
        {
            this.mMatcher = matchingText;
        }

        private string mMatcher;
        private bool isInherit;
        public string MatcherText
        {
            get { return mMatcher; }
            set { mMatcher = value; }
        }

        public bool Inherit
        {
            get { return isInherit; }
            set { isInherit = value; }
        }
    } 
}

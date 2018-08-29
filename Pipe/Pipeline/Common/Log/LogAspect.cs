using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NorthStar.Common.Log
{
    /********************************************************************************
   
    ** 类名称： LoggerSink
    ** 描述：
    ** 作者： dlfan
    ** 创建时间： 2017/7/25 10:52:53
    ** 最后修改人：（无）
    ** 最后修改时间：2017/7/25 10:52:53
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    
    *********************************************************************************/
    public class LoggerSink : IMessageSink
    {
        IMessageSink mNextSink;
        private String mTypeAndName;

        public LoggerSink(IMessageSink nextSink)
        {
            mNextSink = nextSink;
        }

        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            return null;
        }

        public IMessageSink NextSink
        {
            get { return mNextSink; }
        }

        //对目标对象的方法调用时，消息流转到LoggerSink中，在SyncProcessMessage方法对消息进行处理，实现拦截
        //然后在传递到一下个sink，一直传到目标对象
        public IMessage SyncProcessMessage(IMessage msg)
        {
            Preprocess(msg);
            IMessage returnMethod = mNextSink.SyncProcessMessage(msg);
            PostProcess(msg, returnMethod);
            return returnMethod;
            }

        #region // 方法调用前执行
        private void Preprocess(IMessage msg)
        {
            // 仅仅处理方法调用
            if (!(msg is IMethodMessage)) return;

            IMethodMessage call = msg as IMethodMessage;
            
            Type type = Type.GetType(call.TypeName);

            mTypeAndName = type.Name + "." + call.MethodName;
            if (!FilterMethodName(msg)) return;
            string logText = "PreProcessing: " + mTypeAndName + "( ";

            // 遍历参数
            for (int i = 0; i < call.ArgCount; ++i)
            {
                if (i > 0) logText += ", ";
                logText += call.GetArgName(i) + " = " + call.GetArg(i);
            }
            logText += " )";
            Logger.AopLog.Info(logText);
        }

        private void PostProcess(IMessage msg, IMessage msgReturn)
        {
            // 处理返回方法调用时的接口
            if (!(msg is IMethodMessage) ||
                !(msgReturn is IMethodReturnMessage)) return;
            if (!FilterMethodName(msg)) return;
            IMethodReturnMessage retMsg = (IMethodReturnMessage)msgReturn;
            string logText = "PostProcessing: ";
            Exception e = retMsg.Exception;
            if (e != null)
            {
                logText += "Exception was thrown: " + e;
                Logger.AopLog.Error(logText);
                return;
            }

            // 遍历输出参数
            logText += mTypeAndName + "(";
            if (retMsg.OutArgCount > 0)
            {
                logText += "out parameters[";
                for (int i = 0; i < retMsg.OutArgCount; ++i)
                {
                    if (i > 0) logText += ", ";
                    logText += retMsg.GetOutArgName(i) + " = " +
                                  retMsg.GetOutArg(i);
                }
                logText += "]";
            }
            if (retMsg.ReturnValue.GetType() != typeof(void))
                logText += " returned [" + retMsg.ReturnValue + "]";
            logText += " )";
            Logger.AopLog.Info(logText);
        }

        private bool FilterMethodName(IMessage msg)
       {
            IMethodMessage call = msg as IMethodMessage;
            // 过滤构造函数
            if (call.MethodName.Contains("ctor")) return false;

            if (FilterMethod(msg)) return true;

            if (FilterClass(msg)) return true;
             
            return false;
        }

        private string[] MatcherStrGroup(string matherText)
        {
            Regex reg = new Regex(@"([\S]+)[ ]+([\S]+)[ ]+([\S]+)\(([^\)]*)");
            Match m = reg.Match(matherText);
            string[] results = new string[4];
            if (m.Success) { for (int j = 1; j < m.Groups.Count; j++) { results[j - 1] = m.Groups[j].Value; } };
            return results;
        }

        // 检测方法是否包含特性
        private bool FilterClass(IMessage msg)
        {
            IMethodMessage call = msg as IMethodMessage;
            Type type = Type.GetType(call.TypeName);
            object[] attrs = type.GetCustomAttributes(typeof(PointCutAttribute), true);
            foreach (object attr in attrs)
            {
                if (attr is PointCutAttribute)
                {
                    // 获取类过滤名称
                    PointCutAttribute logmather = attr as PointCutAttribute;
                    string matherText = logmather.MatcherText;

                    // 获取方法签名
                    string signature = "protect ";
                    if(call.MethodBase.IsPrivate)  signature = "private ";
                    if(call.MethodBase.IsPublic) signature = "public ";

                    signature += call.MethodBase.ToString();

                    string[] matherList = MatcherStrGroup(matherText);
                    string[] wattingMatchList = MatcherStrGroup(signature);
                   
                    if (Mathered(matherList, wattingMatchList)) return true;

                    // 如果Inherit为false,则不搜索后续特性，直接退出
                    if(!logmather.Inherit) break;
                    
                 }
            }

            return false;
        }
        
        // 比较两个数组是否存在匹配
        private bool Mathered(string[] matherList, string[] wattingMatchList)
        {
            if (matherList.Length != wattingMatchList.Length) return false;

            for (int i = 0; i < matherList.Length; i++)
            {
                if (!Mathered(matherList[i], wattingMatchList[i])) return false;
            }
            return true;
        }

        private bool Mathered(string pattern, string waittingMatcher)
        {
            // 1、通用字符比较
            if (pattern.Equals("*") || pattern.Equals(waittingMatcher, StringComparison.OrdinalIgnoreCase)) return true;

            // 2、 正则表达式比较
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(waittingMatcher)) return true;

            return false;
        }

        // 检测方法是否包含特性
        private bool FilterMethod(IMessage msg)
        {
            IMethodMessage call = msg as IMethodMessage;
            // 检测方法是否包含特性
            Type type = Type.GetType(call.TypeName);
            MethodInfo methodInfo = type.GetMethod(call.MethodName);
            if (methodInfo == null) return false;
            object[] attrs = methodInfo.GetCustomAttributes(typeof(LoggerAttribute), false);
            foreach (object attr in attrs)
            {
                if (attr is LoggerAttribute)
                {
                    return true;
                }
            }  

            return false;
        }

        #endregion Helpers
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class LoggableAttribute : ContextAttribute
    {
        public LoggableAttribute()
            : base("Logger")
        { }

        public override void GetPropertiesForNewContext(System.Runtime.Remoting.Activation.IConstructionCallMessage ctorMsg)
        {
            LogProperty logProperty = new LogProperty();
            ctorMsg.ContextProperties.Add(logProperty);
        }
        public override bool IsNewContextOK(Context newCtx)
        {
            return false;
        }
    }

    public class LogProperty : IContextProperty, IContributeServerContextSink
    {
        //判断一个context是否已经有LogProperty，如果有就用这个context，如果没有则新建一个
        public bool IsNewContextOK(Context newCtx)
        {
            IContextProperty property = newCtx.GetProperty(this.Name) as LogProperty;
            if (property == null)
                return false;
            else return true;
        }

        public string Name
        {
            get { return "LogProperty"; }
        }

        //系统会自动调用该方法。通过调用该方法系统会将我们自定义的Sink插入到对象调用的消息传递链中，这样就可以
        //在自定义的Sink中拦截方法调用
        public IMessageSink GetServerContextSink(IMessageSink nextSink)
        {
            LoggerSink loggerSink = new LoggerSink(nextSink);
            return loggerSink;
        }

        public void Freeze(Context newContext)
        {

        }
    }
}

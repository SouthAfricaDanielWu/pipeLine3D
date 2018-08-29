using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;
using System.Reflection;

namespace NorthStar.Common.Utility
{
    /********************************************************************************
   
    ** 类名称： LoggerSink
    ** 描述：
    ** 作者： dlfan
    ** 创建时间： 2017/7/25 10:52:53
    ** 最后修改人：（无）
    ** 最后修改时间：2017/7/25 10:52:53
    ** 版权所有 (C) :北斗星测绘地理信息研发中心
    
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
            Logger.Log.Info(logText);
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
                Logger.Log.Error(logText);
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
            Logger.Log.Info(logText);
        }

        private bool FilterMethodName(IMessage msg)
        {
            IMethodMessage call = msg as IMethodMessage;
            // 过滤构造函数
            if (call.MethodName.Contains("ctor")) return false;

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

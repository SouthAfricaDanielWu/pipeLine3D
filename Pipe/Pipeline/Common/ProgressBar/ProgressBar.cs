using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NorthStar.Common;
using System.Windows.Forms;

namespace NorthStar.Pipeline.Common
{
    /********************************************************************************

    ** 类名称： ProgressBar
    ** 描述：
    ** 作者： dlfan
    ** 创建时间： 2017/8/6 16:18:50
    ** 最后修改人：（无）
    ** 最后修改时间：2017/8/6 16:18:50
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心

    *********************************************************************************/
    public class ProgressBar
    {       
        Thread thread;
        FrmProgressBar progressbarFrm;
        public ProgressBar(string title, ProgressStyle style)
        {
            progressbarFrm = new FrmProgressBar(title, style);
            thread = new Thread(ShowProgress);
        }

        public void ReportProgress(string content, int value)
        {
            progressbarFrm.ReportMainProgress(content, value);
        }
        public void Show()
        {
            thread.Start();
        }
        public void MonitorProgressRunning(string content, int offsetValue, int seconds)
        {
            progressbarFrm.MonitorProgressRunning(content, offsetValue, seconds);
        }
        public void Close()
        {
            progressbarFrm.CloseMe();         
        }
        private void ShowProgress()
        {
            progressbarFrm.TopMost = true;
            progressbarFrm.ShowDialog();
        }

    }
}

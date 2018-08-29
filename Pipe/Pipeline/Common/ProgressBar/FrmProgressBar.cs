using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using NorthStar.Common;

namespace NorthStar.Pipeline.Common
{
    public partial class FrmProgressBar : Form
    {

        // 进度条风格
        ProgressStyle style;

        // 委托，改变进度
        delegate void ChangeProgessHandler(string content,int value);
        ChangeProgessHandler handler;

        // 是否取消，只有"取消"按钮有效，该值才有效
        bool cancelPressed = false;

        // 范围值
        int minValue = 0;
        int maxValue = 100;

        // 是否关闭
        bool isClosed = false;

        System.Timers.Timer timer1;
        public FrmProgressBar()
        {
            InitializeComponent();
            pBarMain.Minimum = 0;
            pBarMain.Maximum = 100;
            timer1 = new System.Timers.Timer();
        }

        /// <summary>
        /// 进度条的默认范围为0-100
        /// </summary>
        /// <param name="title"></param>
        /// <param name="style"></param>
        public FrmProgressBar(string title, ProgressStyle style):this()
        {
            this.Text = title;
            this.style = style;

            ReLayoutFrm(style);
            handler = ChangeMainProgess;
        }

        /// <summary>
        /// 重新调整布局
        /// </summary>
        /// <param name="style"></param>
        private void ReLayoutFrm(ProgressStyle style)
        {
            switch (style)
            {
                case ProgressStyle.SingleCancel:
                    SingleProgessFrm();
                    break;
                case ProgressStyle.SingleDetail:
                    SingleDetailProgressFrm();
                    break;
                case ProgressStyle.SingleNone:
                    SingleNoneProgressFrm();
                    break;
                case ProgressStyle.DoubleNone:
                    DoubleNoneProgressFrm();
                    break;
                case ProgressStyle.DoubleCancel:
                    DoubleCancelProgressFrm();
                    break;
                case ProgressStyle.DoubleDetail:
                    DoubleDetailProgressFrm();
                    break;
            }
        }

        private void DoubleDetailProgressFrm()
        {
            throw new NotImplementedException();
        }

        private void DoubleCancelProgressFrm()
        {
            throw new NotImplementedException();
        }

        private void DoubleNoneProgressFrm()
        {
            throw new NotImplementedException();
        }

        private void SingleNoneProgressFrm()
        {
            this.Size = new Size(this.Size.Width, labSplitLine.Location.Y+40);
            this.Refresh();
        }

        private void SingleDetailProgressFrm()
        {
            // 不动
        }

        private void SingleProgessFrm()
        {
            int detailYLoc = txtDetailContent.Location.Y;
            int btnYLoc = btnCancel.Location.Y;
            int offsetY = btnYLoc - detailYLoc;
            txtDetailContent.Visible = false;
            btnCancel.Location = new Point(btnCancel.Location.X, detailYLoc);
            this.Size = new Size(this.Size.Width, this.Size.Height - offsetY);
            this.Refresh();
        }

        private void ChangeMainProgess(string content,int value)
        {
            if (isClosed) return;
            if (this.InvokeRequired)
            {
                this.Invoke(handler, new object[] { content, value });
            }
            else
            {
                // 关闭窗口
                
                this.labContent.Text = content + value+"%";
                this.pBarMain.Value = value;
            }
        }

        private void ChangeSubProgess(string content, int value)
        {
            if (isClosed) return;
            if (this.InvokeRequired)
            {
                this.Invoke(handler, new object[] { content, value });
            }
            else
            {
                // 关闭窗口
                this.labSubContent.Text = content;
                this.pBarSub.Value = value;

            }
        }

        private bool CheckClose()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((EventHandler) delegate{
                    this.Close();
                });
                return true;

            }
            else
            {
                this.Close();
            }
            return true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            cancelPressed = true;
        }
        private void ShowMe()
        {
            this.Show();
        }

        #region 开放接口
        // 更新进度条
        public void ReportMainProgress(string content, int value)
        {
            ChangeMainProgess(content, value);
        }

        // 更新进度条
        public void ReportSubProgress(string content, int value)
        {
            if (((int)style) / 2 != 0) return;
            ChangeMainProgess(content, value);
        }
        // 是否按下取消按钮
        public bool IsCancelPressed()
        {
            if (!style.Equals(ProgressStyle.SingleNone) && cancelPressed) return true;
            return false;
        }

        public void SetMainProgressRange(int min, int max)
        {
            pBarMain.Minimum = min;
            pBarMain.Maximum = max;

            minValue = min;
            maxValue = max;
        }

        public void CloseMe()
        {
            isClosed = true;
            if (this.InvokeRequired)
            {
                this.Invoke((EventHandler)delegate
                {
                    this.Close();
                });
            }
            else
            {
                this.Close();
            }

        }

        /// <summary>
        /// 模拟进度条走动
        /// 应用于一些未知总时长耗时操作
        /// </summary>
        /// <param name="content">主标题内容</param>
        /// <param name="value">进度条走动位置</param>
        /// <param name="seconds">模拟时间总长</param>

        int totalMillseconds=0;   // 总计用时
        int limitValue = 0;         // 进度条走动最终位置
        int limitMillseconds = 0;
        int simpleValue;
        string monitorContent;

        private void StartNextTimer()
        {
            int times = limitMillseconds / 500;
            simpleValue = limitValue / times;
            int nextTime = GetRandom(500);
            if (nextTime < 100) nextTime = 100;
            timer1.Interval = nextTime;
            timer1.Elapsed -= new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Enabled = true;
            timer1.Start();
        }

        void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Stop();
            if (isClosed) return;
            totalMillseconds += (int)timer1.Interval;
            int value = GetRandom(simpleValue);

            value += pBarMain.Value;
            // 最后一次
            if (totalMillseconds >= limitMillseconds || value > limitValue)
            {
                value = limitValue;

                // 重置数据
                limitValue = 0;
                limitMillseconds = 0;
            }
            else
            {
                ChangeMainProgess(monitorContent, value);
                // 启动下一次timer
                StartNextTimer();
            }
        }

        public void MonitorProgressRunning(string content, int value, int seconds)
        {

             limitMillseconds += seconds * 1000;
             limitValue += value;
             monitorContent = content;
            // 开始计时
             StartNextTimer();
        }


        // 500毫秒采样一次
        private int GetRandom(int value)
        {
            Random rand = new Random();
            return rand.Next(value);
        }

        #endregion 

        private void FrmProgressBar_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;
        }

    }

    public enum ProgressStyle
    {
        SingleNone   = 1,   // 只有一个进度条
        SingleCancel = 3,   // 一个进度条和一个取消按钮
        SingleDetail = 5,   // 显示进度条、一个文本框、一个取消按钮
        DoubleNone   = 2,   // 两个进度条
        DoubleCancel = 4,   // 两个进度条和一个取消按钮
        DoubleDetail = 6    // 两个进度条、一个文本框、一个取消按钮
    }
}

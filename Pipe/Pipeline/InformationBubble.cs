using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： informationBubble
    ** 描述： 气泡信息设置
    ** 作者： GJF
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    public partial class InformationBubble : UserControl
    {
        public InformationBubble()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 设置或获取模型的名字
        /// </summary>
        public Label Description
        {
            get
            {
                return m_labelDescription;
            }
            set
            {
                m_labelDescription = value;
            }
        }
    }
}

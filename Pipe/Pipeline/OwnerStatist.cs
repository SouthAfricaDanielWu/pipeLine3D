using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using SuperMap.Data;
using SuperMap.Realspace;
using SuperMap.UI;
using SQL;
using System.Diagnostics;
using System.Collections;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： OwnerStatist
    ** 描述： 权属统计
    ** 作者： FKW
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    public partial class OwnerStatist : Form
    {
        private SceneControl mSceneControl;
        private int mIndex = 4;
        private CaliberStatist mCaliberStatist;
        private LengthStatistics mLengthStatist;
        private MaterialStatist mMaterialStatist;
        private ArrayList mPipeStatist = new ArrayList(); //存放管线类型的文本
        private ArrayList mOwnerStatist = new ArrayList(); //存放长度分段的文本
        public ArrayList GetPipeStatist
        {
            get { return mPipeStatist; }
        }
        public ArrayList GetOwnerStatist
        {
            get { return mOwnerStatist; }
        }
        public OwnerStatist(SceneControl m_SceneControl)
        {
            mSceneControl = m_SceneControl;
            InitializeComponent();
        }

        #region 管线内容
        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                mPipeStatist.Add(this.checkBox1.Text);
            }
            else
            {
                mPipeStatist.Remove(this.checkBox1.Text);
            }
        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                mPipeStatist.Add(this.checkBox2.Text);
            }
            else
            {
                mPipeStatist.Remove(this.checkBox2.Text);
            }
        }
        private void checkBox3_Click(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                mPipeStatist.Add(this.checkBox3.Text);
            }
            else
            {
                mPipeStatist.Remove(this.checkBox3.Text);
            }
        }
        #endregion

        private void butStatist_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.checkedListBox1.CheckedItems.Count; i++)
            {
                mOwnerStatist.Add(this.checkedListBox1.CheckedItems[i].ToString());
            }
            PipeStatist mPipeStatist = new PipeStatist(mLengthStatist, mSceneControl, mCaliberStatist, mIndex, mMaterialStatist,this);
            mPipeStatist.StartPosition = FormStartPosition.CenterScreen;
            this.Visible = false;
            mPipeStatist.ShowDialog();
        }

        private void butClosing_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }



    }
}

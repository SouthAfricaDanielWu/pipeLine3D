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

    ** 类名称： LengthStatistics
    ** 描述： 长度统计
    ** 作者： FKW
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    public partial class LengthStatistics : Form
    {
        private SceneControl mSceneControl;
        private int mIndex = 1;
        private CaliberStatist mCaliberStatist;
        private MaterialStatist mMaterialStatist;
        private OwnerStatist mOwnerStatist;
        private ArrayList mPipeStatist= new ArrayList(); //存放管线类型的文本
        private ArrayList mLengthStatist = new ArrayList(); //存放长度分段的文本
        public ArrayList GetPipeStatist
        {
            get { return mPipeStatist; }
        }
        public ArrayList GetLengthStatist
        {
            get { return mLengthStatist; }
        }

        public LengthStatistics(SceneControl m_scene)
        {
            mSceneControl = m_scene;
            InitializeComponent();
        }

        #region 统计管线内容
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
            for (int i = 0; i < this.checkedListBox1.CheckedItems.Count;i++ )
            {
                mLengthStatist.Add(this.checkedListBox1.CheckedItems[i].ToString());
            }
            PipeStatist mPipeStatist = new PipeStatist(this, mSceneControl, mCaliberStatist, mIndex, mMaterialStatist, mOwnerStatist);
            mPipeStatist.StartPosition = FormStartPosition.CenterScreen;
            this.Visible = false;
            mPipeStatist.ShowDialog();
        }

        private void butClosing_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void LengthStatistics_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < this.checkedListBox1.Items.Count; i++)
            {
                this.checkedListBox1.SetItemChecked(i, true);
            }
        }

    }
}

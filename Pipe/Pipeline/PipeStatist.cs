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
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： PipeStatist
    ** 描述： 管线统计
    ** 作者： FKW
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    public partial class PipeStatist : Form
    {
        private LengthStatistics mLengthStatis;
        private CaliberStatist mCaliberStatist;
        private MaterialStatist mMaterialStatist;
        private OwnerStatist mOwnerStatist;
        private SceneControl mSceneControl;
        private String mDataSourceName;
        private int mIndex; //存放统计索引，判断是哪个类型统计     
        private ArrayList mMaterialCount = new ArrayList(); //存放材质数量
        private ArrayList mOwnerCount = new ArrayList(); //存放权属信息数量
        private int[] mCount = new int[3]; //存放先CLICK的管线 每段长度的个数
        private int[] mCount1 = new int[3]; //存放后CLICK的管线 每段长度的个数
        private int[] mCount2 = new int[3]; //存放后CLICK的管线 每段长度的个数
        public PipeStatist(LengthStatistics m_LengthStatis,SceneControl m_SceneControl,CaliberStatist m_CaliberStatist,int m_Index,MaterialStatist m_MaterialStatist,OwnerStatist m_OwnerStatist)
        {          
            mIndex = m_Index;
            this.mOwnerStatist = m_OwnerStatist;
            this.mMaterialStatist = m_MaterialStatist;
            this.mCaliberStatist = m_CaliberStatist;
            this.mLengthStatis = m_LengthStatis;
            this.mSceneControl = m_SceneControl;
            //获取数据源的名字
            Datasources m_datasources = mSceneControl.Scene.Workspace.Datasources;
            mDataSourceName = m_datasources[0].Alias;
            InitializeComponent();
            switch (mIndex)
            {
                case 1: Array.Clear(mCount, 0, mCount.Length);
                        Array.Clear(mCount1, 0, mCount1.Length);
                        Array.Clear(mCount2, 0, mCount2.Length);
                        LengBar();                
                    break;
                case 2: Array.Clear(mCount, 0, mCount.Length);
                        Array.Clear(mCount1, 0, mCount1.Length);
                        Array.Clear(mCount2, 0, mCount2.Length);
                        CaliberBar();
                    break;
                case 3: MaterialBar();
                    break;
                case 4: OwnerBar();
                    break;
            }         
        }
        #region 给长度柱形图赋值
        /// <summary>
        /// 给长度柱形图赋值
        /// </summary>
        public void LengBar()
        {
            try
            {
                Layer3Ds mLayer3D = mSceneControl.Scene.Layers;    
                Layer3DDataset mLayer = null;
                Recordset mRecord = null;
                DatasetVector mData = null;
                String str = "";
                String str1 = "";
                for (int i = 0; i < mLengthStatis.GetPipeStatist.Count; i++)
                {
                    mLayer = mLayer3D[mLengthStatis.GetPipeStatist[i].ToString() + "@" + mDataSourceName] as Layer3DDataset;
                    mData = mLayer.Dataset as DatasetVector;
                    mRecord = mData.GetRecordset(false, CursorType.Static);
                    mRecord.MoveFirst();
                    if (i == 0)
                    {
                        while (!mRecord.IsEOF)
                        {
                            for (int k = 0; k < mRecord.FieldCount; k++)
                            {
                                str = mRecord.GetFieldInfos()[k].Name;
                                if (str == "Length")
                                {
                                    str1 = mRecord.GetFieldValue(k).ToString();
                                    if (System.Convert.ToDouble(str1) > 0 && System.Convert.ToDouble(str1) <= 5)
                                    {
                                        mCount[0]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 5 && System.Convert.ToDouble(str1) <= 10)
                                    {
                                        mCount[1]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 10)
                                    {
                                        mCount[2]++;
                                        break;
                                    }
                                }
                            }
                            mRecord.MoveNext();
                        }
                    }
                    else if (i == 1)
                    {
                        while (!mRecord.IsEOF)
                        {
                            for (int k = 0; k < mRecord.FieldCount; k++)
                            {
                                str = mRecord.GetFieldInfos()[k].Name;
                                if (str == "Length")
                                {
                                    str1 = mRecord.GetFieldValue(k).ToString();
                                    if (System.Convert.ToDouble(str1) > 0 && System.Convert.ToDouble(str1) <= 5)
                                    {
                                        mCount1[0]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 5 && System.Convert.ToDouble(str1) <= 10)
                                    {
                                        mCount1[1]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 10)
                                    {
                                        mCount1[2]++;
                                        break;
                                    }
                                }
                            }
                            mRecord.MoveNext();
                        }
                    }
                    else if (i == 2)
                    {
                        while (!mRecord.IsEOF)
                        {
                            for (int k = 0; k < mRecord.FieldCount; k++)
                            {
                                str = mRecord.GetFieldInfos()[k].Name;
                                if (str == "Length")
                                {
                                    str1 = mRecord.GetFieldValue(k).ToString();
                                    if (System.Convert.ToDouble(str1) > 0 && System.Convert.ToDouble(str1) <= 5)
                                    {
                                        mCount2[0]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 5 && System.Convert.ToDouble(str1) <= 10)
                                    {
                                        mCount2[1]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 10)
                                    {
                                        mCount2[2]++;
                                        break;
                                    }
                                }
                            }
                            mRecord.MoveNext();
                        }
                    }
                }
                LengthCreate();
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 生成柱形图
        /// </summary>
        public void LengthCreate()
        {
            int height = 330, width = 500;
            Bitmap image = new Bitmap(width, height);
            //创建Graphics类对象
            Graphics g = Graphics.FromImage(image);
            try
            {
                g.Clear(Color.White);
                Font font = new Font("Arial", 8, FontStyle.Regular);
                Font font1 = new Font("宋体", 15, FontStyle.Bold);
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.BlueViolet, 1.2f, true);
                g.FillRectangle(Brushes.WhiteSmoke, 0, 0, width, height);

                g.DrawString("地下管线分段长度统计图", font1, brush, new PointF(138, 8));
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Blue), 0, 0, image.Width - 1, image.Height - 1);
                Pen mypen = new Pen(brush, 1);
                //绘制线条
                //绘制横向线条
                int x = 90;
                for (int z = 0; z <= 18; z++)
                {
                    g.DrawLine(mypen, x, 40, x, 240);
                    x = x + 20;
                }
                Pen mypen1 = new Pen(Color.Blue, 2);
                x = 70;
                g.DrawLine(mypen1, x, 40, x, 240);

                //绘制纵向线条
                int y = 60;
                for (int z = 0; z < 9; z++)
                {
                    g.DrawLine(mypen, 70, y, 450, y);
                    y = y + 20;
                }
                g.DrawLine(mypen1, 70, y, 450, y);
                //X轴的内容
                x = 79;
                for (int n = 0; n < mLengthStatis.GetPipeStatist.Count; n++)
                {
                    g.DrawString(this.mLengthStatis.GetPipeStatist[n].ToString().Substring(0, this.mLengthStatis.GetPipeStatist[n].ToString().Length), font, Brushes.Blue, x, 248); //设置文字内容及输出位置
                    x = x + 115;
                }
                //Y轴的内容
                String[] m = { "", "72", "64", "56", "48", "40", "32", "24", "16", "8", "0" };
                y = 30;
                for (int b = 0; b < 10; b++)
                {
                    g.DrawString(m[b].ToString(), font, Brushes.Blue, 40, y); //设置文字内容及输出位置
                    y = y + 20;
                }
                g.DrawString("数量", font, Brushes.Blue, 40, 32);
                //填充柱形图的内容
                SolidBrush mybrush = new SolidBrush(Color.Red);
                SolidBrush mybrush1 = new SolidBrush(Color.Blue);
                SolidBrush mybrush2 = new SolidBrush(Color.Yellow);
                SolidBrush mybrush3 = new SolidBrush(Color.Green);
                SolidBrush mybrush4 = new SolidBrush(Color.Purple);
                SolidBrush mybrush5 = new SolidBrush(Color.Pink);
                SolidBrush mybrush6 = new SolidBrush(Color.PaleGreen);
                SolidBrush mybrush7 = new SolidBrush(Color.SeaGreen);
                SolidBrush mybrush8 = new SolidBrush(Color.RoyalBlue);
                SolidBrush[] mybrushs = new SolidBrush[9] { mybrush, mybrush1, mybrush2, mybrush3, mybrush4, mybrush5, mybrush6, mybrush7, mybrush8 };
                Font font2 = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
                x = 85;
                for (int q = 0; q <mLengthStatis.GetPipeStatist.Count; q++)
                {
                    if(q==0)
                    {
                        for (int p = 0; p < mCount.Length; p++)
                        {
                            g.FillRectangle(mybrushs[p], x, Convert.ToInt32(240 - Convert.ToDouble(mCount[p]) / Convert.ToDouble(8) * 20), 20, Convert.ToInt32(Convert.ToDouble(mCount[p]) / Convert.ToDouble(8) * 20));
                            g.DrawString(mCount[p].ToString(), font2, Brushes.Red, x, Convert.ToInt32(240 - Convert.ToDouble(mCount[p]) / Convert.ToDouble(8) * 20) - 15);
                            x = x + 20;
                        }
                    }
                    else if(q==1)
                    {
                        for (int p = 0; p < mCount1.Length; p++)
                        {
                            g.FillRectangle(mybrushs[p], x, Convert.ToInt32(240 - Convert.ToDouble(mCount1[p]) / Convert.ToDouble(8) * 20), 20, Convert.ToInt32(Convert.ToDouble(mCount1[p]) / Convert.ToDouble(8) * 20));
                            g.DrawString(mCount1[p].ToString(), font2, Brushes.Red, x, Convert.ToInt32(240 - Convert.ToDouble(mCount1[p]) / Convert.ToDouble(8) * 20) - 15);
                            x = x + 20;
                        }
                    }
                    else if (q == 2)
                    {
                        for (int p = 0; p < mCount2.Length; p++)
                        {
                            g.FillRectangle(mybrushs[p], x, Convert.ToInt32(240 - Convert.ToDouble(mCount2[p]) / Convert.ToDouble(8) * 20), 20, Convert.ToInt32(Convert.ToDouble(mCount2[p]) / Convert.ToDouble(8) * 20));
                            g.DrawString(mCount2[p].ToString(), font2, Brushes.Red, x, Convert.ToInt32(240 - Convert.ToDouble(mCount2[p]) / Convert.ToDouble(8) * 20) - 15);
                            x = x + 20;
                        }
                    }     
                    x = x + 45;
                }
                Font font3 = new System.Drawing.Font("宋体", 8, FontStyle.Regular);
                g.DrawRectangle(new Pen(Brushes.Blue), 140, 269, 250, 30); //绘制范围框
                x = 150;
                y = 275;
                for (int b = 0; b < mLengthStatis.GetLengthStatist.Count; b++)
                {
                    g.FillRectangle(mybrushs[b], x, y, 20, 10); //绘制小矩形
                    g.DrawString(mLengthStatis.GetLengthStatist[b].ToString() , font3, Brushes.Red, x + 23, y);
                    if (b == 2)
                    {
                        x = 150;
                        y = 290;
                    }
                    else
                    {
                        if (b == 4)
                        {
                            x = 150;
                            y = 305;
                        }
                        else
                        {
                            x = x + 80;
                        }
                    }
                }
                //把柱形图显示到pictureBox上
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                this.pictureBox1.Image = new Bitmap(image);
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 给管径柱形图赋值
        /// <summary>
        /// 管径统计图
        /// </summary>
        public void CaliberBar()
        {
            try
            {
                Layer3Ds mLayer3D = mSceneControl.Scene.Layers;
                Layer3DDataset mLayer = null;
                Recordset mRecord = null;
                DatasetVector mData = null;
                String str = "";
                String str1 = "";
                for (int i = 0; i < mCaliberStatist.GetPipeStatist.Count; i++)
                {
                    mLayer = mLayer3D[mCaliberStatist.GetPipeStatist[i].ToString() + "@" + mDataSourceName] as Layer3DDataset;
                    mData = mLayer.Dataset as DatasetVector;
                    mRecord = mData.GetRecordset(false, CursorType.Static);
                    mRecord.MoveFirst();
                    if (i == 0)
                    {
                        while (!mRecord.IsEOF)
                        {
                            for (int k = 0; k < mRecord.FieldCount; k++)
                            {
                                str = mRecord.GetFieldInfos()[k].Name;
                                if (str == "PipeDiameter")
                                {
                                    str1 = mRecord.GetFieldValue(k).ToString();
                                    if (System.Convert.ToDouble(str1) > 10 && System.Convert.ToDouble(str1) <= 20)
                                    {
                                        mCount[0]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 20 && System.Convert.ToDouble(str1) <= 30)
                                    {
                                        mCount[1]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 30)
                                    {
                                        mCount[2]++;
                                        break;
                                    }
                                }
                            }
                            mRecord.MoveNext();
                        }
                    }
                    else if (i == 1)
                    {
                        while (!mRecord.IsEOF)
                        {
                            for (int k = 0; k < mRecord.FieldCount; k++)
                            {
                                str = mRecord.GetFieldInfos()[k].Name;
                                if (str == "PipeDiameter")
                                {
                                    str1 = mRecord.GetFieldValue(k).ToString();
                                    if (System.Convert.ToDouble(str1) > 10 && System.Convert.ToDouble(str1) <= 20)
                                    {
                                        mCount1[0]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 20 && System.Convert.ToDouble(str1) <= 30)
                                    {
                                        mCount1[1]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 30)
                                    {
                                        mCount1[2]++;
                                        break;
                                    }
                                }
                            }
                            mRecord.MoveNext();
                        }
                    }
                    else if (i == 2)
                    {
                        while (!mRecord.IsEOF)
                        {
                            for (int k = 0; k < mRecord.FieldCount; k++)
                            {
                                str = mRecord.GetFieldInfos()[k].Name;
                                if (str == "PipeDiameter")
                                {
                                    str1 = mRecord.GetFieldValue(k).ToString();
                                    if (System.Convert.ToDouble(str1) > 10 && System.Convert.ToDouble(str1) <= 20)
                                    {
                                        mCount2[0]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 20 && System.Convert.ToDouble(str1) <= 30)
                                    {
                                        mCount2[1]++;
                                        break;
                                    }
                                    else if (System.Convert.ToDouble(str1) > 30)
                                    {
                                        mCount2[2]++;
                                        break;
                                    }
                                }
                            }
                            mRecord.MoveNext();
                        }
                    }
                }
                CaliberCreate();
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
         public void CaliberCreate()
        {
            int height = 330, width = 500;
            Bitmap image = new Bitmap(width, height);
            //创建Graphics类对象
            Graphics g = Graphics.FromImage(image);
            try
            {
                g.Clear(Color.White);
                Font font = new Font("Arial", 8, FontStyle.Regular);
                Font font1 = new Font("宋体", 15, FontStyle.Bold);
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.BlueViolet, 1.2f, true);
                g.FillRectangle(Brushes.WhiteSmoke, 0, 0, width, height);

                g.DrawString("地下管线分段口径统计图", font1, brush, new PointF(138, 8));
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Blue), 0, 0, image.Width - 1, image.Height - 1);
                Pen mypen = new Pen(brush, 1);
                //绘制线条
                //绘制横向线条
                int x = 90;
                for (int z = 0; z <= 18; z++)
                {
                    g.DrawLine(mypen, x, 40, x, 240);
                    x = x + 20;
                }
                Pen mypen1 = new Pen(Color.Blue, 2);
                x = 70;
                g.DrawLine(mypen1, x, 40, x, 240);

                //绘制纵向线条
                int y = 60;
                for (int z = 0; z < 9; z++)
                {
                    g.DrawLine(mypen, 70, y, 450, y);
                    y = y + 20;
                }
                g.DrawLine(mypen1, 70, y, 450, y);
                //X轴的内容
                x = 79;
                for (int n = 0; n < mCaliberStatist.GetPipeStatist.Count; n++)
                {
                    g.DrawString(this.mCaliberStatist.GetPipeStatist[n].ToString().Substring(0, this.mCaliberStatist.GetPipeStatist[n].ToString().Length), font, Brushes.Blue, x, 248); //设置文字内容及输出位置
                    x = x + 115;
                }
                //Y轴的内容
                String[] m = { "", "72", "64", "56", "48", "40", "32", "24", "16", "8", "0" };
                y = 30;
                for (int b = 0; b < 10; b++)
                {
                    g.DrawString(m[b].ToString(), font, Brushes.Blue, 40, y); //设置文字内容及输出位置
                    y = y + 20;
                }
                g.DrawString("数量", font, Brushes.Blue, 40, 32);
                //填充柱形图的内容
                SolidBrush mybrush = new SolidBrush(Color.Red);
                SolidBrush mybrush1 = new SolidBrush(Color.Blue);
                SolidBrush mybrush2 = new SolidBrush(Color.Yellow);
                SolidBrush mybrush3 = new SolidBrush(Color.Green);
                SolidBrush mybrush4 = new SolidBrush(Color.Purple);
                SolidBrush mybrush5 = new SolidBrush(Color.Pink);
                SolidBrush mybrush6 = new SolidBrush(Color.PaleGreen);
                SolidBrush mybrush7 = new SolidBrush(Color.SeaGreen);
                SolidBrush mybrush8 = new SolidBrush(Color.RoyalBlue);
                SolidBrush[] mybrushs = new SolidBrush[9] { mybrush, mybrush1, mybrush2, mybrush3, mybrush4, mybrush5, mybrush6, mybrush7, mybrush8 };
                Font font2 = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
                x = 85;
                for (int q = 0; q < mCaliberStatist.GetPipeStatist.Count; q++)
                {
                    if (q == 0)
                    {
                        for (int p = 0; p < mCount.Length; p++)
                        {
                            g.FillRectangle(mybrushs[p], x, Convert.ToInt32(240 - Convert.ToDouble(mCount[p]) / Convert.ToDouble(8) * 20), 20, Convert.ToInt32(Convert.ToDouble(mCount[p]) / Convert.ToDouble(8) * 20));
                            g.DrawString(mCount[p].ToString(), font2, Brushes.Red, x, Convert.ToInt32(240 - Convert.ToDouble(mCount[p]) / Convert.ToDouble(8) * 20) - 15);
                            x = x + 20;
                        }
                    }
                    else if (q == 1)
                    {
                        for (int p = 0; p < mCount1.Length; p++)
                        {
                            g.FillRectangle(mybrushs[p], x, Convert.ToInt32(240 - Convert.ToDouble(mCount1[p]) / Convert.ToDouble(8) * 20), 20, Convert.ToInt32(Convert.ToDouble(mCount1[p]) / Convert.ToDouble(8) * 20));
                            g.DrawString(mCount1[p].ToString(), font2, Brushes.Red, x, Convert.ToInt32(240 - Convert.ToDouble(mCount1[p]) / Convert.ToDouble(8) * 20) - 15);
                            x = x + 20;
                        }
                    }
                    else if (q == 2)
                    {
                        for (int p = 0; p < mCount1.Length; p++)
                        {
                            g.FillRectangle(mybrushs[p], x, Convert.ToInt32(240 - Convert.ToDouble(mCount2[p]) / Convert.ToDouble(8) * 20), 20, Convert.ToInt32(Convert.ToDouble(mCount2[p]) / Convert.ToDouble(8) * 20));
                            g.DrawString(mCount2[p].ToString(), font2, Brushes.Red, x, Convert.ToInt32(240 - Convert.ToDouble(mCount2[p]) / Convert.ToDouble(8) * 20) - 15);
                            x = x + 20;
                        }
                    }
                    x = x + 45;
                }
                Font font3 = new System.Drawing.Font("宋体", 8, FontStyle.Regular);
                g.DrawRectangle(new Pen(Brushes.Blue), 140, 269, 250, 30); //绘制范围框
                x = 150;
                y = 275;
                for (int b = 0; b < mCaliberStatist.GetCaliberStatist.Count; b++)
                {
                    g.FillRectangle(mybrushs[b], x, y, 20, 10); //绘制小矩形
                    g.DrawString(mCaliberStatist.GetCaliberStatist[b].ToString(), font3, Brushes.Red, x + 23, y);
                    if (b == 2)
                    {
                        x = 150;
                        y = 290;
                    }
                    else
                    {
                        if (b == 4)
                        {
                            x = 150;
                            y = 305;
                        }
                        else
                        {
                            x = x + 80;
                        }
                    }
                }
                //把柱形图显示到pictureBox上
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                this.pictureBox1.Image = new Bitmap(image);
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 给材质柱形图赋值
        /// <summary>
        /// 材质柱形图
        /// </summary>
         public void MaterialBar()
         {
             try
             {
                 Layer3Ds mLayer3D = mSceneControl.Scene.Layers;
                 Layer3DDataset mLayer = null;
                 Recordset mRecord = null;
                 DatasetVector mData = null;
                 String str = "";
                 String str1 = "";              
                 for (int i = 0; i < mMaterialStatist.GetPipeStatist.Count; i++)
                 {
                     mLayer = mLayer3D[mMaterialStatist.GetPipeStatist[i].ToString() + "@"+mDataSourceName] as Layer3DDataset;
                     mData = mLayer.Dataset as DatasetVector;     
                     for (int k = 0; k < mMaterialStatist.GetMaterialStatist.Count;k++ )
                     {
                         mRecord = mData.Query("Material='" + mMaterialStatist.GetMaterialStatist[k].ToString() + "'",CursorType.Dynamic);
                         mMaterialCount.Add(mRecord.RecordCount.ToString());
                     }    
                 }
                 MaterialCreate();
             }
             catch (System.Exception ex)
             {
                 Trace.WriteLine(ex.Message);
             }
         }
         public void MaterialCreate()
         {
             int h = 0; //存放材质数组的索引
             int height = 330, width = 500;
             Bitmap image = new Bitmap(width, height);
             //创建Graphics类对象
             Graphics g = Graphics.FromImage(image);
             try
             {
                 g.Clear(Color.White);
                 Font font = new Font("Arial", 8, FontStyle.Regular);
                 Font font1 = new Font("宋体", 15, FontStyle.Bold);
                 LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.BlueViolet, 1.2f, true);
                 g.FillRectangle(Brushes.WhiteSmoke, 0, 0, width, height);

                 g.DrawString("地下管线各类材质统计图", font1, brush, new PointF(138, 8));
                 //画图片的边框线
                 g.DrawRectangle(new Pen(Color.Blue), 0, 0, image.Width - 1, image.Height - 1);
                 Pen mypen = new Pen(brush, 1);
                 //绘制线条
                 //绘制横向线条
                 int x = 90;
                 for (int z = 0; z <= 18; z++)
                 {
                     g.DrawLine(mypen, x, 40, x, 240);
                     x = x + 20;
                 }
                 Pen mypen1 = new Pen(Color.Blue, 2);
                 x = 70;
                 g.DrawLine(mypen1, x, 40, x, 240);

                 //绘制纵向线条
                 int y = 60;
                 for (int z = 0; z < 9; z++)
                 {
                     g.DrawLine(mypen, 70, y, 450, y);
                     y = y + 20;
                 }
                 g.DrawLine(mypen1, 70, y, 450, y);
                 //X轴的内容
                 x = 79;
                 for (int n = 0; n < mMaterialStatist.GetPipeStatist.Count; n++)
                 {
                     g.DrawString(this.mMaterialStatist.GetPipeStatist[n].ToString().Substring(0, this.mMaterialStatist.GetPipeStatist[n].ToString().Length), font, Brushes.Blue, x, 248); //设置文字内容及输出位置
                     x = x + 115;
                 }
                 //Y轴的内容
                 String[] m = { "", "72", "64", "56", "48", "40", "32", "24", "16", "8", "0" };
                 y = 30;
                 for (int b = 0; b < 10; b++)
                 {
                     g.DrawString(m[b].ToString(), font, Brushes.Blue, 40, y); //设置文字内容及输出位置
                     y = y + 20;
                 }
                 g.DrawString("数量", font, Brushes.Blue, 40, 32);
                 //填充柱形图的内容
                 SolidBrush mybrush = new SolidBrush(Color.Red);
                 SolidBrush mybrush1 = new SolidBrush(Color.Blue);
                 SolidBrush mybrush2 = new SolidBrush(Color.Yellow);
                 SolidBrush mybrush3 = new SolidBrush(Color.Green);
                 SolidBrush mybrush4 = new SolidBrush(Color.Purple);
                 SolidBrush mybrush5 = new SolidBrush(Color.Pink);
                 SolidBrush mybrush6 = new SolidBrush(Color.PaleGreen);
                 SolidBrush mybrush7 = new SolidBrush(Color.SeaGreen);
                 SolidBrush mybrush8 = new SolidBrush(Color.RoyalBlue);
                 SolidBrush[] mybrushs = new SolidBrush[9] { mybrush, mybrush1, mybrush2, mybrush3, mybrush4, mybrush5, mybrush6, mybrush7, mybrush8 };
                 Font font2 = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
                 x = 85;
                 for (int q = 0; q < mMaterialStatist.GetPipeStatist.Count; q++)
                 {
                    for (int p = 0; p < mMaterialStatist.GetMaterialStatist.Count; p++)
                    {
                      g.FillRectangle(mybrushs[p], x, Convert.ToInt32(240 - Convert.ToDouble(mMaterialCount[h]) / Convert.ToDouble(8) * 20), 20, Convert.ToInt32(Convert.ToDouble(mMaterialCount[h]) / Convert.ToDouble(8) * 20));
                      g.DrawString(mMaterialCount[h].ToString(), font2, Brushes.Red, x, Convert.ToInt32(240 - Convert.ToDouble(mMaterialCount[h]) / Convert.ToDouble(8) * 20) - 15);
                      x = x + 20;
                      h = h + 1;
                    }                        
                    x = x + 35;
                 }
                 Font font3 = new System.Drawing.Font("宋体", 8, FontStyle.Regular);
                 g.DrawRectangle(new Pen(Brushes.Blue), 140, 269, 250, 40); //绘制范围框
                 x = 150;
                 y = 275;
                 for (int b = 0; b < mMaterialStatist.GetMaterialStatist.Count; b++)
                 {
                     g.FillRectangle(mybrushs[b], x, y, 20, 10); //绘制小矩形
                     g.DrawString(mMaterialStatist.GetMaterialStatist[b].ToString(), font3, Brushes.Red, x + 23, y);
                     if (b == 2)
                     {
                         x = 150;
                         y = 290;
                     }
                     else
                     {
                         if (b == 4)
                         {
                             x = 150;
                             y = 305;
                         }
                         else
                         {
                             x = x + 80;
                         }
                     }
                 }
                 //把柱形图显示到pictureBox上
                 System.IO.MemoryStream ms = new System.IO.MemoryStream();
                 image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                 this.pictureBox1.Image = new Bitmap(image);
             }
             catch (System.Exception ex)
             {
                 Trace.WriteLine(ex.Message);
             }
         }
        #endregion

        #region 给权属信息柱形图赋值
         public void OwnerBar()
         {
             try
             {
                 Layer3Ds mLayer3D = mSceneControl.Scene.Layers;
                 Layer3DDataset mLayer = null;
                 Recordset mRecord = null;
                 DatasetVector mData = null;
                 String str = "";
                 String str1 = "";
                 for (int i = 0; i < mOwnerStatist.GetPipeStatist.Count; i++)
                 {
                     mLayer = mLayer3D[mOwnerStatist.GetPipeStatist[i].ToString() + "@" + mDataSourceName] as Layer3DDataset;
                     mData = mLayer.Dataset as DatasetVector;
                     for (int k = 0; k < mOwnerStatist.GetOwnerStatist.Count; k++)
                     {
                         mRecord = mData.Query("OwnerShip='" + mOwnerStatist.GetOwnerStatist[k].ToString() + "'", CursorType.Dynamic);
                         mOwnerCount.Add(mRecord.RecordCount.ToString());
                     }
                 }
                 OwnerCreate();
             }
             catch (System.Exception ex)
             {
                 Trace.WriteLine(ex.Message);
             }
         }
         public void OwnerCreate()
         {
             int h = 0; //存放权属信息数组的索引
             int height = 330, width = 500;
             Bitmap image = new Bitmap(width, height);
             //创建Graphics类对象
             Graphics g = Graphics.FromImage(image);
             try
             {
                 g.Clear(Color.White);
                 Font font = new Font("Arial", 8, FontStyle.Regular);
                 Font font1 = new Font("宋体", 15, FontStyle.Bold);
                 LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.BlueViolet, 1.2f, true);
                 g.FillRectangle(Brushes.WhiteSmoke, 0, 0, width, height);

                 g.DrawString("地下管线各个权属统计图", font1, brush, new PointF(138, 8));
                 //画图片的边框线
                 g.DrawRectangle(new Pen(Color.Blue), 0, 0, image.Width - 1, image.Height - 1);
                 Pen mypen = new Pen(brush, 1);
                 //绘制线条
                 //绘制横向线条
                 int x = 90;
                 for (int z = 0; z <= 18; z++)
                 {
                     g.DrawLine(mypen, x, 40, x, 240);
                     x = x + 20;
                 }
                 Pen mypen1 = new Pen(Color.Blue, 2);
                 x = 70;
                 g.DrawLine(mypen1, x, 40, x, 240);

                 //绘制纵向线条
                 int y = 60;
                 for (int z = 0; z < 9; z++)
                 {
                     g.DrawLine(mypen, 70, y, 450, y);
                     y = y + 20;
                 }
                 g.DrawLine(mypen1, 70, y, 450, y);
                 //X轴的内容
                 x = 79;
                 for (int n = 0; n < mOwnerStatist.GetPipeStatist.Count; n++)
                 {
                     g.DrawString(this.mOwnerStatist.GetPipeStatist[n].ToString().Substring(0, this.mOwnerStatist.GetPipeStatist[n].ToString().Length), font, Brushes.Blue, x, 248); //设置文字内容及输出位置
                     x = x + 115;
                 }
                 //Y轴的内容
                 String[] m = { "", "72", "64", "56", "48", "40", "32", "24", "16", "8", "0" };
                 y = 30;
                 for (int b = 0; b < 10; b++)
                 {
                     g.DrawString(m[b].ToString(), font, Brushes.Blue, 40, y); //设置文字内容及输出位置
                     y = y + 20;
                 }
                 g.DrawString("数量", font, Brushes.Blue, 40, 32);
                 //填充柱形图的内容
                 SolidBrush mybrush = new SolidBrush(Color.Red);
                 SolidBrush mybrush1 = new SolidBrush(Color.Blue);
                 SolidBrush mybrush2 = new SolidBrush(Color.Yellow);
                 SolidBrush mybrush3 = new SolidBrush(Color.Green);
                 SolidBrush mybrush4 = new SolidBrush(Color.Purple);
                 SolidBrush mybrush5 = new SolidBrush(Color.Pink);
                 SolidBrush mybrush6 = new SolidBrush(Color.PaleGreen);
                 SolidBrush mybrush7 = new SolidBrush(Color.SeaGreen);
                 SolidBrush mybrush8 = new SolidBrush(Color.RoyalBlue);
                 SolidBrush[] mybrushs = new SolidBrush[9] { mybrush, mybrush1, mybrush2, mybrush3, mybrush4, mybrush5, mybrush6, mybrush7, mybrush8 };
                 Font font2 = new System.Drawing.Font("Arial", 8, FontStyle.Bold);
                 x = 85;
                 for (int q = 0; q < mOwnerStatist.GetPipeStatist.Count; q++)
                 {
                     for (int p = 0; p < mOwnerStatist.GetOwnerStatist.Count; p++)
                     {
                         g.FillRectangle(mybrushs[p], x, Convert.ToInt32(240 - Convert.ToDouble(mOwnerCount[h]) / Convert.ToDouble(8) * 20), 20, Convert.ToInt32(Convert.ToDouble(mOwnerCount[h]) / Convert.ToDouble(8) * 20));
                         g.DrawString(mOwnerCount[h].ToString(), font2, Brushes.Red, x, Convert.ToInt32(240 - Convert.ToDouble(mOwnerCount[h]) / Convert.ToDouble(8) * 20) - 15);
                         x = x + 20;
                         h = h + 1;
                     }
                     x = x + 65;
                 }
                 Font font3 = new System.Drawing.Font("宋体", 8, FontStyle.Regular);
                 g.DrawRectangle(new Pen(Brushes.Blue), 140, 269, 250, 40); //绘制范围框
                 x = 150;
                 y = 275;
                 for (int b = 0; b < mOwnerStatist.GetOwnerStatist.Count; b++)
                 {
                     g.FillRectangle(mybrushs[b], x, y, 20, 10); //绘制小矩形
                     g.DrawString(mOwnerStatist.GetOwnerStatist[b].ToString(), font3, Brushes.Red, x + 23, y);
                     if (b == 2)
                     {
                         x = 150;
                         y = 290;
                     }
                     else
                     {
                         if (b == 4)
                         {
                             x = 150;
                             y = 305;
                         }
                         else
                         {
                             x = x + 80;
                         }
                     }
                 }
                 //把柱形图显示到pictureBox上
                 System.IO.MemoryStream ms = new System.IO.MemoryStream();
                 image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                 this.pictureBox1.Image = new Bitmap(image);
             }
             catch (System.Exception ex)
             {
                 Trace.WriteLine(ex.Message);
             }
         }
        #endregion
    }
}

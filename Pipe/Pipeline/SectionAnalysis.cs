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
    ** 类名称： SectionAnalysis
    ** 描述： 断面分析等操作（暂时不使用）
    ** 作者： PLWhite
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    public partial class SectionAnalysis : Form
    {
        private SceneControl mSceneControl;
        private UseData mUseData;
        private ArrayList mPipeType = new ArrayList(); //存放rad的管线
        private Rectangle2D mRec2D;  

        public SectionAnalysis(SceneControl m_SceneControl,UseData m_UseData)
        {
            mSceneControl = m_SceneControl;
            mUseData = m_UseData;
            InitializeComponent();
        }

        #region 选择图层
        private void radbtn1_CheckedChanged(object sender, EventArgs e)
        {
            if (radbtn1.Checked)
            {
                mPipeType.Add(this.radbtn1.Text);
            }
            else
            {
                mPipeType.Remove(this.radbtn1.Text);
            }
        }
        private void radbtn2_CheckedChanged(object sender, EventArgs e)
        {
            if (radbtn2.Checked)
            {
                mPipeType.Add(this.radbtn2.Text);
            }
            else
            {
                mPipeType.Remove(this.radbtn2.Text);
            }
        }
        private void radbtn3_CheckedChanged(object sender, EventArgs e)
        {
            if (radbtn3.Checked)
            {
                mPipeType.Add(this.radbtn3.Text);
            }
            else
            {
                mPipeType.Remove(this.radbtn3.Text);
            }
        }     
        #endregion

        #region 横断面分析
        private void btnCrossSec_Click(object sender, EventArgs e)
        {
            mSceneControl.Action = Action3D.MeasureDistance;
            //注册事件
            mSceneControl.Tracking -= new Tracking3DEventHandler(TrackingSectionEvent);
            mSceneControl.Tracking += new Tracking3DEventHandler(TrackingSectionEvent);
            mSceneControl.Tracked -= new Tracked3DEventHandler(TrackedSectionEvent);
            mSceneControl.Tracked += new Tracked3DEventHandler(TrackedSectionEvent);
        }
        private void TrackedSectionEvent(object sender, Tracked3DEventArgs e)
        {
            try
            {
                //绘制量算线对象
                GeoLine3D geoLine3D = e.Geometry.Clone() as GeoLine3D;
                mRec2D = geoLine3D.Bounds;
                //设置数据集容量，避免空间查询出现过多对象
                  mUseData.OutWaterNetWork.Tolerance.NodeSnap = 0.0001;
                  mUseData.SupplyWaterNetWork.Tolerance.NodeSnap = 0.0001;
                  mUseData.ElectricNetWork.Tolerance.NodeSnap = 0.0001;

                GeoStyle3D mGeoStyle3D = new GeoStyle3D();
                // mGeoStyle3D.MarkerColor = Color.FromArgb(255, 0, 255);
                mGeoStyle3D.LineColor = Color.FromArgb(0x00, 0x99, 0x00);
                mGeoStyle3D.LineWidth = 3;
                geoLine3D.Style3D = mGeoStyle3D.Clone();
                geoLine3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoLine3D, "GeoLine3D");
                mSceneControl.Action = Action3D.Pan2;

                CrossSecLine();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void TrackingSectionEvent(object sender, Tracking3DEventArgs e)
        {
            try
            {
                Point location = mSceneControl.PointToClient(Cursor.Position);

                location.Offset(30, 30);
                if (location.X > mSceneControl.Bounds.Width / 3 * 2)
                {
                    location.X = mSceneControl.Bounds.Width / 3 * 2;
                }
                if (location.Y > mSceneControl.Bounds.Height)
                {
                    location.Y = location.Y - 60;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 找出横断面线与管线相交的断点
        /// </summary>
        public void CrossSecLine()
        {
            try
            {
                Layer3Ds m_layer = mSceneControl.Scene.Layers;
                foreach (Layer3D mlayer in m_layer)
                {
                    if (mlayer.Selection != null)
                    {
                        mlayer.Selection.Clear();
                    }
                }

                QueryParameter para = new QueryParameter();
                para.HasGeometry = true;
                para.SpatialQueryMode = SpatialQueryMode.Intersect;
                para.SpatialQueryObject = mRec2D;
                Recordset recordset = null;
                Layer3DDataset layer = null;

                switch (mPipeType[0].ToString())
                {
                    case "排水管网": 
                        recordset = mUseData.OutWaterNetWork.Query(para);
                        layer = mUseData.OutWaterLines;                     
                        break;
                    case "给水管网": 
                        recordset = mUseData.SupplyWaterNetWork.Query(para);
                        layer = mUseData.SupplyWaterLines;
                        break;
                    case "电力管网": 
                        recordset = mUseData.ElectricNetWork.Query(para);
                        layer = mUseData.ElectricLayer;
                        break;
                }

                List<Int32> ids = new List<int>(recordset.RecordCount);
                while (!recordset.IsEOF)
                {
                    ids.Add(recordset.GetID());
                    recordset.MoveNext();
                }
                layer.Selection.AddRange(ids.ToArray());
                layer.Selection.UpdateData();
                layer.Selection.Style.LineColor = Color.GreenYellow;
                mSceneControl.Scene.Refresh();
            }
            catch (Exception ex)
            {
                Trace.Write(ex.Message);
            }        
        }
        #endregion
  
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SuperMap.Data;
using SuperMap.Mapping;
using SuperMap.UI;
using SuperMap.Realspace;
using SuperMap.Realspace.NetworkAnalyst;
using System.Diagnostics;
using System.IO;
using NorthStar.Common.Log;
using SuperMap.Analyst.NetworkAnalyst;
using SuperMap.Realspace.SpatialAnalyst;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： RodeExectionAnalysis
    ** 描述： 道路扩建分析等操作
    ** 作者： GJF
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    /// <summary>
    /// 道路扩建与范围拆迁
    /// </summary>
    public class RodeExectionAnalysis
    {
        private SceneControl mSceneControl;
        private UseData mUseData;
        private Datasource mDatasource;
        private DatasetVector mBufferDataset;
        private DatasetVector mRegionDatasets;
        private Recordset mRecordset;
        private Geometry3D mGeo;
        private Layer3D mLayerBuffer;
        private double mRadius;

        public RodeExectionAnalysis(SceneControl sceneControl, UseData UseData)
        {
            mSceneControl = sceneControl;
            mUseData = UseData;
            Initialize();
        }
        private void Initialize()
        {

        }
        #region 道路扩建
        /// <summary>
        /// 创建缓冲区
        /// </summary>
        public void CreateBuffer()
        {
            try
            {

                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要扩建的道路线对象");
                    return;
                }

                Recordset mLineRecordset = selection[0].ToRecordset();
                object RodeType = mLineRecordset.GetFieldValue("Name");
                if (RodeType.ToString().Trim() == "大路")
                {
                    mRadius += 3.5;
                }
                else if (RodeType.ToString().Trim() == "小路")
                {
                    mRadius += 1.85;
                }
                else
                {
                    MessageBox.Show("请选择需要扩建的道路线对象");
                    Layer3Ds mlayer = mSceneControl.Scene.Layers;
                    foreach (Layer3D layer in mlayer)
                    {
                        if (layer.Selection != null)
                        {
                            layer.Selection.Clear();
                        }
                    }
                    return;
                }

                //按照模板创建模型数据集
                mDatasource = mUseData.DataSource;
                Datasource datasource = mDatasource;
                DatasetVector mRegionDatasets = datasource.Datasets["New_Model"] as DatasetVector;
                String bufferName = "bufferModel";
                if (datasource.Datasets.Contains(bufferName))
                {
                    datasource.Datasets.Delete(bufferName);
                }
                mBufferDataset = (DatasetVector)datasource.Datasets.CreateFromTemplate(bufferName, mRegionDatasets);
                mBufferDataset.PrjCoordSys = mLineRecordset.Dataset.PrjCoordSys;
                mRecordset = mBufferDataset.GetRecordset(false, CursorType.Dynamic);
                while (!mLineRecordset.IsEOF)
                {
                    mGeo = mLineRecordset.GetGeometry() as Geometry3D;
                    BufferAnalyst3DParameter bufferAnalyst3DParameter = new BufferAnalyst3DParameter();
                    bufferAnalyst3DParameter.EndType = SuperMap.Realspace.SpatialAnalyst.BufferEndType.Round;
                    bufferAnalyst3DParameter.BufferDistance = Convert.ToDouble(mRadius);
                    bufferAnalyst3DParameter.BufferQuality = 20;
                    Geometry3D geo3D = Geometrist3D.CreateBuffer(mGeo, bufferAnalyst3DParameter, mLineRecordset.Dataset.PrjCoordSys);
                    GeoStyle3D geoStyle3D = new GeoStyle3D();
                    mRecordset.AddNew(geo3D);
                    mLineRecordset.MoveNext();
                }
                mRecordset.Update();
                //设置数据集容量，避免空间查询出现过多对象
                mRecordset.Dataset.Tolerance.NodeSnap = 0.0002;

                Layer3DSettingVector layer3DSetting = new Layer3DSettingVector();
                GeoStyle3D style = new GeoStyle3D();
                style.FillForeColor = Color.FromArgb(170, 242, 242, 15);
                style.FillBackColor = Color.GreenYellow;
                style.AltitudeMode = AltitudeMode.RelativeToGround;
                style.FillMode = FillMode3D.Fill;
                layer3DSetting.Style = style;

                mLayerBuffer = mSceneControl.Scene.Layers.Add(mRecordset.Dataset, layer3DSetting, true);
                mLayerBuffer.UpdateData();

                if (mSceneControl.Scene.Layers.Contains("道路断线_3D@127.0.0.1_UnderPipeLine#1"))
                {
                    mSceneControl.Scene.Layers["道路断线_3D@127.0.0.1_UnderPipeLine#1"].Selection.Clear();
                }

                mSceneControl.Scene.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// 设置缓存区的半径
        /// </summary>
        public void SetRadius(double Radius)
        {
            try
            {
                mRadius = Radius;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        public void DeleteBuffer()
        {
            try
            {
                mRecordset.DeleteAll();

                if (mLayerBuffer != null)
                {
                    mSceneControl.Scene.Layers.Remove(mLayerBuffer.Name);
                    mLayerBuffer = null;
                }
                mSceneControl.Scene.Refresh();

                if (mBufferDataset != null)
                {
                    mDatasource.Datasets.Delete(mBufferDataset.Name);
                    mBufferDataset = null;
                }             
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// 删除缓冲区
        /// </summary>
        public void DeleteWhole()
        {
            try
            {
                mRecordset.DeleteAll();
                
                if (mLayerBuffer != null)
                {
                    mSceneControl.Scene.Layers.Remove(mLayerBuffer.Name);
                    mLayerBuffer = null;
                }
                mSceneControl.Scene.Refresh();

                if (mBufferDataset != null)
                {
                    mDatasource.Datasets.Delete(mBufferDataset.Name);
                    mBufferDataset = null;
                }
                Layer3Ds mlayer = mSceneControl.Scene.Layers;
                foreach (Layer3D layer in mlayer)
                {
                    if (layer.Selection != null)
                    {
                        layer.Selection.Clear();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }


        #endregion

        #region 范围拆迁
        double mRemoveRadius;
        public void CircleEvent(double radius)
        {
            mSceneControl.MouseDoubleClick -= new MouseEventHandler(mSceneControlRemoveRangeMouseDown);
            mSceneControl.MouseDoubleClick += new MouseEventHandler(mSceneControlRemoveRangeMouseDown);
            mRemoveRadius = radius;
        }
        GeoPoint3D mRemovePoint3D;
        private void mSceneControlRemoveRangeMouseDown(object sender, MouseEventArgs e)
        {
            Point3D mPoint3D = new Point3D();
            mPoint3D = mSceneControl.Scene.PixelToGlobe(e.Location, PixelToGlobeMode.TerrainAndModel);
            mPoint3D.Z = 0.5;
            DrawCirle(mPoint3D);
            mSceneControl.Action = Action3D.Pan2;
            mRemovePoint3D = new GeoPoint3D(mPoint3D);
            DisplayRemovePoint();

        }
        private Rectangle2D mRec;
        private void DrawCirle(Point3D mPoint3D)
        {
            try
            {
                GeoCircle3D m_circle3d = new GeoCircle3D(mPoint3D, mRemoveRadius);
                GeoModel3D m_model = m_circle3d.ConvertToGeoModel3D(true);
                mRec = m_model.Bounds;
                //设置数据集容量，避免空间查询出现过多对象
                mUseData.OutWaterNetWork.Tolerance.NodeSnap = 0.0001;
                mUseData.SupplyWaterNetWork.Tolerance.NodeSnap = 0.0001;
                GeoStyle3D style = new GeoStyle3D();
                style.FillForeColor = Color.FromArgb(150, 153, 207, 25);
                style.AltitudeMode = AltitudeMode.RelativeToGround;
                style.FillMode = FillMode3D.Fill;
                m_model.Style3D = style;
                mSceneControl.Scene.TrackingLayer.IsEditable = true;
                mSceneControl.Scene.TrackingLayer.IsVisible = true;
                mSceneControl.Scene.TrackingLayer.Add(m_model, "圆");
                CloseMethod();
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void CloseMethod()
        {
            mSceneControl.MouseDoubleClick -= new MouseEventHandler(mSceneControlRemoveRangeMouseDown);
        }




        private String mRemovePointTag;
        private String mMarkRemove;
        /// <summary>
        /// 在三维场景中显示起始点
        /// </summary>
        public void DisplayRemovePoint()
        {
            try
            {
                GeoStyle3D pointStyle = new GeoStyle3D();
                pointStyle.MarkerColor = Color.FromArgb(255, 51, 255, 0);
                pointStyle.MarkerSize = 10.0;
                pointStyle.AltitudeMode = AltitudeMode.Absolute;

                mRemovePoint3D.Style3D = pointStyle;

                mRemovePointTag = "startPoint";
                mSceneControl.Scene.TrackingLayer.Add(mRemovePoint3D, mRemovePointTag);

                mMarkRemove = "PlaceMarkStart";
                GeoPlacemark markStart = new GeoPlacemark("拆迁点", mRemovePoint3D);
                markStart.NameStyle.ForeColor = Color.FromArgb(255, 51, 255, 0);
                markStart.Style3D.MarkerFile = @"..\..\Resource\blupin.png";
                mSceneControl.Scene.TrackingLayer.Add(markStart, mMarkRemove);

                mSceneControl.Scene.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        public void ClearRemoveRange()
        {
            try
            {
                mSceneControl.Scene.TrackingLayer.Clear();
                Layer3Ds m_layer = mSceneControl.Scene.Layers;
                foreach (Layer3D layer in m_layer)
                {
                    if (layer.Selection != null)
                    {
                        layer.Selection.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }



        public void BufferQuery()
        {
            try
            {
                Datasource datasource = mUseData.DataSource;
                DatasetVector mRegionDatasets = datasource.Datasets["New_Model"] as DatasetVector;
                mRegionDatasets.PrjCoordSys = datasource.Datasets[0].PrjCoordSys;
                String bufferName = "bufferModel";
                if (datasource.Datasets.Contains(bufferName))
                {
                    datasource.Datasets.Delete(bufferName);
                }
                mBufferDataset = (DatasetVector)datasource.Datasets.CreateFromTemplate(bufferName, mRegionDatasets);


                #region 设置矢量面初始样式

                GeoStyle3D style3D = new GeoStyle3D();

                style3D.AltitudeMode = AltitudeMode.ClampToGround;
                style3D.FillForeColor = Color.White;
                style3D.FillMode = FillMode3D.LineAndFill;

                Layer3DSettingVector layer3DSetting = new Layer3DSettingVector();
                layer3DSetting.Style = style3D;
                Layer3DDataset m_layerRegion = mSceneControl.Scene.Layers.Add(mRegionDatasets, layer3DSetting, true);
                #endregion




                QueryParameter para = new QueryParameter();
                para.SpatialQueryMode = SpatialQueryMode.Intersect;
                para.SpatialQueryObject = mBufferDataset;

                Recordset recordset = mRegionDatasets.Query(para);

                List<Int32> ids = new List<int>(recordset.RecordCount);

                while (!recordset.IsEOF)
                {
                    ids.Add(recordset.GetID());
                    recordset.MoveNext();
                }
                m_layerRegion.Selection.AddRange(ids.ToArray());
                m_layerRegion.Selection.UpdateData();

                m_layerRegion.Selection.Style.FillForeColor = Color.FromArgb(180, 100, 100, 255);
                mSceneControl.Scene.Refresh();

                recordset.Dispose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }










        #endregion

    }
}

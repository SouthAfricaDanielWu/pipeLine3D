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

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： Query
    ** 描述： 空间查询等操作
    ** 作者： FKW
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）11
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    class Query : NorthStar.Common.Base
    {
        private SceneControl mSceneControl;
        private Point3D mTempPoint;
        private Point3D mFlyPoint3D;
        private Point3D mPoint3D;
        private Point3Ds mPoint3Ds;
        private Rectangle2D m_rec;
        private UseData mUseData;
        private DataGridView mDataGridView;
        private GeoStyle3D mGeoStyle3DTemp;
        private GeoStyle3D mGeoStyle3D;
        private static String mMessageTrackingTag = "MeasureDistanceTracking";

        public SceneControl SceneControl
        {
            get { return mSceneControl; }
        }

        public Query(SceneControl bd_sceneControl, UseData bd_usedata, DataGridView bd_DataGridView)
        {
            this.mSceneControl = bd_sceneControl;
            this.mUseData = bd_usedata;
            this.mDataGridView = bd_DataGridView;
            mPoint3Ds = new Point3Ds();
        }

        #region 绘制矩形查询区域
        /// <summary>
        /// 矩形空间查询区域事件
        /// </summary>
        public void AreaQueryEvent()
        {
            mSceneControl.Action = SuperMap.UI.Action3D.MeasureArea;
            mSceneControl.Tracking += new Tracking3DEventHandler(bd_sceneControl_Tracking);
            mSceneControl.Tracked += new Tracked3DEventHandler(bd_sceneControl_Tracked);
        }
        public void bd_sceneControl_Tracking(object sender, Tracking3DEventArgs e)
        {
            OutputMeasureArea(e);
        }
        public void bd_sceneControl_Tracked(object sender, Tracked3DEventArgs e)
        {
            Output(e);
        }
        public void OutputMeasureArea(SuperMap.UI.Tracking3DEventArgs e1)
        {
            try
            {
                Point location = mSceneControl.PointToClient(Cursor.Position);
                mTempPoint = new Point3D(e1.X, e1.Y, e1.Z);
                mPoint3Ds.Add(mTempPoint);
                GeoRegion3D geoRegion3D = null;

                if (mPoint3Ds.Count >= 3)
                {
                    geoRegion3D = new GeoRegion3D(mPoint3Ds);
                    mGeoStyle3DTemp = new GeoStyle3D();
                    mGeoStyle3DTemp.MarkerColor = Color.FromArgb(255, 0, 0);
                    mGeoStyle3DTemp.LineColor = Color.FromArgb(0, 255, 0);
                    mGeoStyle3DTemp.LineWidth = 1;
                    mGeoStyle3DTemp.FillForeColor = Color.FromArgb(180, Color.Violet);
                    mGeoStyle3DTemp.AltitudeMode = AltitudeMode.RelativeToGround;
                    geoRegion3D.Style3D = mGeoStyle3DTemp.Clone();

                    location.Offset(30, 30);
                    if (location.X > mSceneControl.Bounds.Width / 4 * 3)
                    {
                        location.X = mSceneControl.Bounds.Width / 4 * 3;
                    }
                    if (location.Y > mSceneControl.Bounds.Height)
                    {
                        location.Y = location.Y - 60;
                    }

                    int index = mSceneControl.Scene.TrackingLayer.IndexOf(mMessageTrackingTag);
                    if (index >= 0)
                    {
                        mSceneControl.Scene.TrackingLayer.Remove(index);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

        }
        int RegionID;
        public void Output(SuperMap.UI.Tracked3DEventArgs e1)
        {
            try
            {
                //绘制量算面对象
                GeoRegion3D geoRegion3D = e1.Geometry as GeoRegion3D;
                //得到面对象的外接矩形，作为查询区域
                m_rec = geoRegion3D.Bounds;
                //设置数据集容量，避免空间查询出现过多对象
                mUseData.OutWaterNetWork.Tolerance.NodeSnap = 0.0001;
                mUseData.SupplyWaterNetWork.Tolerance.NodeSnap = 0.0001;
                mGeoStyle3D = new GeoStyle3D();
                mGeoStyle3D.MarkerColor = Color.FromArgb(255, 0, 255);
                mGeoStyle3D.LineColor = Color.FromArgb(255, 255, 0);
                mGeoStyle3D.LineWidth = 1;
                mGeoStyle3D.FillForeColor = Color.FromArgb(100, 250, 250, 50);
               // mGeoStyle3D.AltitudeMode = AltitudeMode.ClampToGround;
                geoRegion3D.Style3D = mGeoStyle3D.Clone();
                geoRegion3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                geoRegion3D.Style3D.FillForeColor = Color.FromArgb(50, 255, 128, 64);
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoRegion3D, "geoRegion3D");
                RegionID=trackinglayer.IndexOf("geoRegion3D");
                mSceneControl.Action = Action3D.Pan2;
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 空间相交查询
        public void InsertQuery()
        {
            try
            {          
                this.mDataGridView.Rows.Clear();
                this.mDataGridView.Columns.Clear();
                Layer3Ds m_layer = mSceneControl.Scene.Layers;
                foreach (Layer3D mlayer in m_layer)
                {
                    if(mlayer.Selection!=null)
                    {
                       mlayer.Selection.Clear();
                    }               
                }  
 
                QueryParameter para = new QueryParameter();
                para.HasGeometry = true;
                para.SpatialQueryMode = SpatialQueryMode.Intersect;
                para.SpatialQueryObject = m_rec;
                Recordset recordset=null;
                Layer3DDataset layer = null;
                for(int i=0;i<3;i++)
                {
                    if(i==0)
                    {
                        recordset = mUseData.OutWaterNetWork.Query(para);
                        layer = mUseData.OutWaterLines;
                    }
                    else if(i==1)
                    {
                        recordset = mUseData.SupplyWaterNetWork.Query(para);
                        layer = mUseData.SupplyWaterLines;
                    }
                    else if (i == 2)
                    {
                        recordset = mUseData.ElectricNetWork.Query(para);
                        layer = mUseData.ElectricLayer;
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

                    //在DataGrid中显示表
                    recordset.MoveFirst();
                    FieldInfos fieldinfos = recordset.GetFieldInfos();
                    foreach (FieldInfo fieldInfo in fieldinfos)
                    {
                        if (i >= 1)
                        {
                            break;
                        }
                        if (!fieldInfo.IsSystemField)
                        {
                            if (fieldInfo.Name == "SmPPoint" || fieldInfo.Name == "SmNPoint")
                            {
                                continue;
                            }
                            string mCaption = fieldInfo.Caption;
                            this.mDataGridView.Columns.Add(mCaption, mCaption);
                        }
                    }
                    //初始化行
                    DataGridViewRow dataGridViewRow;
                    recordset.MoveFirst();

                    //根据选中的个数将对象的信息添加到列表中
                    while (!recordset.IsEOF)
                    {
                        dataGridViewRow = new DataGridViewRow();
                        for (int a = 0; a < recordset.FieldCount; a++)
                        {
                            if (!recordset.GetFieldInfos()[a].IsSystemField)
                            {
                                if (recordset.GetFieldInfos()[a].Name == "SmPPoint" || recordset.GetFieldInfos()[a].Name == "SmNPoint")
                                {
                                    continue;
                                }
                                //定义并获取字段值
                                object filevalue = recordset.GetFieldValue(a);

                                //添加到相应的位置
                                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                                if (filevalue != null)
                                {
                                    cell.ValueType = filevalue.GetType();
                                    cell.Value = filevalue;
                                }
                                dataGridViewRow.Cells.Add(cell);
                            }
                        }
                        this.mDataGridView.Rows.Add(dataGridViewRow);
                        recordset.MoveNext();
                    }
                    this.mDataGridView.Update();
                    recordset.Dispose();
                }
                mSceneControl.Action = Action3D.Pan2;
                mSceneControl.Scene.Refresh();                
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 空间相离查询
        public void SeparQuery()
        {
            try
            {
                this.mDataGridView.Rows.Clear();
                this.mDataGridView.Columns.Clear();
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
                para.SpatialQueryMode = SpatialQueryMode.Disjoint;
                para.SpatialQueryObject = m_rec;
                Recordset recordset = null;
                Layer3DDataset layer = null;
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0)
                    {
                        recordset = mUseData.OutWaterNetWork.Query(para);
                        layer = mUseData.OutWaterLines;
                    }
                    else if (i == 1)
                    {
                        recordset = mUseData.SupplyWaterNetWork.Query(para);
                        layer = mUseData.SupplyWaterLines;
                    }
                    else if (i == 2)
                    {
                        recordset = mUseData.ElectricNetWork.Query(para);
                        layer = mUseData.ElectricLayer;
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

                    //在DataGrid中显示表
                    recordset.MoveFirst();
                    FieldInfos fieldinfos = recordset.GetFieldInfos();
                    foreach (FieldInfo fieldInfo in fieldinfos)
                    {
                        if (i >= 1)
                        {
                            break;
                        }
                        if (!fieldInfo.IsSystemField)
                        {
                            if (fieldInfo.Name == "SmPPoint" || fieldInfo.Name == "SmNPoint")
                            {
                                continue;
                            }
                            string mCaption = fieldInfo.Caption;
                            this.mDataGridView.Columns.Add(mCaption, mCaption);
                        }
                    }
                    //初始化行
                    DataGridViewRow dataGridViewRow;
                    recordset.MoveFirst();

                    //根据选中的个数将对象的信息添加到列表中
                    while (!recordset.IsEOF)
                    {
                        dataGridViewRow = new DataGridViewRow();
                        for (int a = 0; a < recordset.FieldCount; a++)
                        {
                            if (!recordset.GetFieldInfos()[a].IsSystemField)
                            {
                                if (recordset.GetFieldInfos()[a].Name == "SmPPoint" || recordset.GetFieldInfos()[a].Name == "SmNPoint")
                                {
                                    continue;
                                }
                                //定义并获取字段值
                                object filevalue = recordset.GetFieldValue(a);

                                //添加到相应的位置
                                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                                if (filevalue != null)
                                {
                                    cell.ValueType = filevalue.GetType();
                                    cell.Value = filevalue;
                                }
                                dataGridViewRow.Cells.Add(cell);
                            }
                        }
                        this.mDataGridView.Rows.Add(dataGridViewRow);
                        recordset.MoveNext();
                    }
                    this.mDataGridView.Update();
                    recordset.Dispose();
                }
                mSceneControl.Action = Action3D.Pan2;
                mSceneControl.Scene.Refresh();               
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 空间包含查询
        public void ContainQuery()
        {
            try
            {
                this.mDataGridView.Rows.Clear();
                this.mDataGridView.Columns.Clear();
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
                para.SpatialQueryMode = SpatialQueryMode.Contain;
                para.SpatialQueryObject = m_rec;
                Recordset recordset = null;
                Layer3DDataset layer = null;
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0)
                    {
                        recordset = mUseData.OutWaterNetWork.Query(para);
                        layer = mUseData.OutWaterLines;
                    }
                    else if (i == 1)
                    {
                        recordset = mUseData.SupplyWaterNetWork.Query(para);
                        layer = mUseData.SupplyWaterLines;
                    }
                    else if (i == 2)
                    {
                        recordset = mUseData.ElectricNetWork.Query(para);
                        layer = mUseData.ElectricLayer;
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

                    //在DataGrid中显示表
                    recordset.MoveFirst();
                    FieldInfos fieldinfos = recordset.GetFieldInfos();
                    foreach (FieldInfo fieldInfo in fieldinfos)
                    {
                        if (i >= 1)
                        {
                            break;
                        }
                        if (!fieldInfo.IsSystemField)
                        {
                            if (fieldInfo.Name == "SmPPoint" || fieldInfo.Name == "SmNPoint")
                            {
                                continue;
                            }
                            string mCaption = fieldInfo.Caption;
                            this.mDataGridView.Columns.Add(mCaption, mCaption);
                        }
                    }
                    //初始化行
                    DataGridViewRow dataGridViewRow;
                    recordset.MoveFirst();

                    //根据选中的个数将对象的信息添加到列表中
                    while (!recordset.IsEOF)
                    {
                        dataGridViewRow = new DataGridViewRow();
                        for (int a = 0; a < recordset.FieldCount; a++)
                        {
                            if (!recordset.GetFieldInfos()[a].IsSystemField)
                            {
                                if (recordset.GetFieldInfos()[a].Name == "SmPPoint" || recordset.GetFieldInfos()[a].Name == "SmNPoint")
                                {
                                    continue;
                                }
                                //定义并获取字段值
                                object filevalue = recordset.GetFieldValue(a);

                                //添加到相应的位置
                                DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                                if (filevalue != null)
                                {
                                    cell.ValueType = filevalue.GetType();
                                    cell.Value = filevalue;
                                }
                                dataGridViewRow.Cells.Add(cell);
                            }
                        }
                        this.mDataGridView.Rows.Add(dataGridViewRow);
                        recordset.MoveNext();
                    }
                    this.mDataGridView.Update();
                    recordset.Dispose();
                }
                mSceneControl.Action = Action3D.Pan2;
                mSceneControl.Scene.Refresh();               
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 绘制圆形查询区域
        /// <summary>
        /// 圆形区域空间查询
        /// </summary>
        public void CircleEvent()
        {
            mSceneControl.MouseDown += new MouseEventHandler(mSceneControlMouseDown);
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlObjectSelected);
        }
        private void mSceneControlMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                mPoint3D = new Point3D();
                mPoint3D = mSceneControl.Scene.PixelToGlobe(e.Location, PixelToGlobeMode.TerrainAndModel);
                mPoint3D.Z = -2;
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
            }
        }
        private void mSceneControlObjectSelected(object sender, ObjectSelectedEventArgs e)
        {
            // 无对象被选中
            if (e.Count == 0)
            {
                MessageBox.Show("未选择对象!");
            }
            //有对象选中
            else if (e.Count > 0)
            {           
                DrawCirle(mPoint3D);
                mSceneControl.Action = Action3D.Pan2;
            }
        }

        int CircleID;
        public void DrawCirle(Point3D bd_point3D)
        {
          try
           {   
            GeoCircle3D m_circle3d = new GeoCircle3D(bd_point3D, 20);
            GeoModel3D m_model = m_circle3d.ConvertToGeoModel3D(true);
            m_rec = m_model.Bounds;
            //设置数据集容量，避免空间查询出现过多对象
            mUseData.OutWaterNetWork.Tolerance.NodeSnap = 0.0001;
            mUseData.SupplyWaterNetWork.Tolerance.NodeSnap = 0.0001;
            GeoStyle3D style = new GeoStyle3D();
            style.FillForeColor = Color.FromArgb(100, 255, 128, 64);
            style.AltitudeMode = AltitudeMode.RelativeToGround;
            style.FillMode = FillMode3D.Fill;
            m_model.Style3D = style;
            TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
            trackinglayer.IsEditable = true;
            trackinglayer.IsVisible = true;
            trackinglayer.Add(m_model, "圆");
            CircleID=trackinglayer.IndexOf("圆");
           }
          catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 寻找目标点，进行飞行
        /// </summary>
        public void TargetPoint()
        {
            try
            {
                if (mFlyPoint3D.X == 0 && mFlyPoint3D.Y == 0 && mFlyPoint3D.Z == 0)
                {
                    Layer3Ds bd_3DLayers = mSceneControl.Scene.Layers;
                    Layer3DDataset bd_3DLayer = bd_3DLayers[0] as Layer3DDataset;
                    double bd_3DLayerHeight = bd_3DLayer.Bounds.Height;
                    double bd_3DLayerWidth = bd_3DLayer.Bounds.Width;
                    int PointX = (int)(bd_3DLayer.Bounds.Left + bd_3DLayerWidth / 2);
                    int PointY = (int)(bd_3DLayer.Bounds.Bottom + bd_3DLayerHeight / 2);
                    Point FlyPoint = new Point(PointX, PointY);
                    mFlyPoint3D = mSceneControl.Scene.PixelToGlobe(FlyPoint);
                    FlyTo(200, mFlyPoint3D.X, mFlyPoint3D.Y, 0);
                }
                else
                {
                    FlyTo(200, mFlyPoint3D.X, mFlyPoint3D.Y, 0);
                }
            }
            catch (Exception)
            {
                                
            }       
        }
        /// <summary>
        /// 管线定位(相机高度，经度，纬度，俯角)
        /// </summary>
        /// <param name="Latitude"> </param>
        /// <param name="Longitude"></param>
        public void FlyTo(double Altitude, double Longitude, double Latitude, double Tilt)
        {
            Camera camera = mSceneControl.Scene.Camera;
            camera.Altitude = Altitude;
            camera.Longitude = Longitude;
            camera.Latitude = Latitude;
            camera.Tilt = Tilt;
            mSceneControl.Scene.Fly(camera, 5);
        }
        #endregion

        #region 清除图层
        public void ClearTrack()
        {
            try
            {
                mSceneControl.Bubbles.Clear();
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.Clear();
                m_rec=Rectangle2D.Empty;
                mSceneControl.Scene.Refresh();
                this.mDataGridView.Rows.Clear();
                this.mDataGridView.Columns.Clear();
                mSceneControl.Tracking -= new Tracking3DEventHandler(bd_sceneControl_Tracking);
                mSceneControl.Tracked -= new Tracked3DEventHandler(bd_sceneControl_Tracked);
                mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelected);
                Layer3Ds m_layer = mSceneControl.Scene.Layers;
                foreach (Layer3D layer in m_layer)
                {
                    if(layer.Selection!=null)
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
    }
}

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

    ** 类名称： SpaceAnalysis
    ** 描述： 空间分析操作
    ** 作者： GJF
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/8/14 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/

    /// <summary>
    /// 空间分析功能类
    /// </summary>
    public class SpaceAnalysis : NorthStar.Common.Base
    {

        #region 私有字段
        private SceneControl mSceneControl;
        private List<String> mNetworkDatasets;
        private UseData mUseData;
        //爆管分析
        private List<String> mPoint3DFields;
        private List<String> mLine3DFields;
        private DatasetVector mPipeNet;
        private GroupBox mGroupBoxBoom;
        private GroupBox mGroupBoxBoomInfluence;
        private FacilityAnalystSetting3D mFacilityAnalystSetting;
        private FacilityAnalyst3D mFacilityAnalyst;
        private Layer3DDataset mLayerNetLine;
        private Layer3DDataset mLayerNetNode;
        private Int32[] mFacilitiesID;
        private GeoLine3D mSelectedLine;
        private Int32[] mResultFacilities;
        private Int32[] mResultEdges;
        private double PipeDeepth;
        private int mPipeBoomId;
        string mBoonPipeType = null;
        private Point3D mClickPoint;
        private Int32 mPointType = 0;
        public InformationBubble mInformationBubble;
        private DataGridView mDataGridViewResult;
        private DataGridView mDataGridViewEdge;
        private TextBox mTextBoxPipeLineID;
        // 用于存储添加到三维场景中的网络数据集图层
        String mStrLayer3DNodeNetwork = "RodeNetWork2D_Node@guilinligong";
        String mStrLayer3DLineNetwork = "RodeNetWork2D@guilinligong";
        // 用于记录分析结果的线段
        String mStrTrackingTagNetworkResult = "resultLine";
        private TransportationAnalyst mAnalyst;
        private TransportationAnalystResult mAnalystResult;
        private TransportationAnalystParameter mAnalystParameter;
        //爆管分析
        //开关阀分析
        private List<Int32> mSourcePoints;
        private List<Int32> mSinkPoints;
        private bool mControlObjectFlag = false;
        public bool ControlObjectFlag
        { get { return mControlObjectFlag; } }
        private Int32 mControlID;
        private String mPipeType;
        double mControlDeepth = 0;
        public string ControlPipeType
        { get { return mPipeType; } }
        public object mAnalongitude;
        public object Analongitude
        { get { return mAnalongitude; } }

        public object mAnalatitude;
        public object Analatitude
        { get { return Analatitude; } }

        public int mInfluencePipe = 0;
        public int InfluencePipe
        { get { return mInfluencePipe; } }


        //开关阀分析
        //开挖分析
        double mExcavationDeepth;
        bool mFlag = false;
        public GeoStyle3D mGeoStyle3D;
        private double mInerX;
        private double mInerY;
        public double mTotalArea;
        public double TotalArea
        {
            get { return this.mTotalArea; }
        }
        private TextBox mTextBoxTotalArea;
        private TextBox mTextBoxTotalsqure;
        private readonly String mSquareMeter = "平方米";
        private readonly String mArea = "挖方面积：";
        private Int32 mIndex = -1;
        #endregion

       public SpaceAnalysis(
                         SceneControl sceneControl,
                         TextBox textBoxTotalArea,
                         TextBox textBoxTotalsqure,
                         DataGridView dataGridView,
                         DataGridView dataGridViewEdge,
                         TextBox textBoxPipeLineID,
                         InformationBubble informationBubble,
                         UseData UseData, GroupBox GroupBoxAccident,
                         GroupBox GroupBoxInfulence
                         )
        {
            mSceneControl = sceneControl;
            mTextBoxTotalArea = textBoxTotalArea;
            mTextBoxTotalsqure = textBoxTotalsqure;
            mGroupBoxBoom = GroupBoxAccident;
            mGroupBoxBoomInfluence = GroupBoxInfulence;
            mDataGridViewResult = dataGridView;
            mDataGridViewEdge = dataGridViewEdge;
            mTextBoxPipeLineID = textBoxPipeLineID;
            mInformationBubble = informationBubble;
            mUseData = UseData;
            Initialize();//打开需要的工作空间
        }

       private void Initialize()
       {

           mPoint3DFields = new List<String>();
           mLine3DFields = new List<String>();
           mFacilityAnalyst = new FacilityAnalyst3D();
           mFacilityAnalystSetting = new FacilityAnalystSetting3D();
           mSourcePoints = new List<Int32>();
           mSinkPoints = new List<Int32>();
           mNetworkDatasets = new List<String>();

           mAnalyst = new TransportationAnalyst();
           mAnalystParameter = new TransportationAnalystParameter();

           mSceneControl.BubbleInitialize += new BubbleInitializeEventHandler(mSceneControlBubbleInitialize);
           mSceneControl.BubbleClose += new BubbleCloseEventHandler(mSceneControlBubbleClose);
           mSceneControl.BubbleResize += new BubbleResizeEventHandler(mSceneControlBubbleResize);
       }

       #region/气泡事件
       void mSceneControlBubbleInitialize(object sender, BubbleEventArgs e)
       {
           System.Drawing.Point point = new Point(e.Bubble.ClientLeft, e.Bubble.ClientTop);
           mInformationBubble.Location = point;
           mInformationBubble.Visible = true;
       }

       void mSceneControlBubbleClose(object sender, BubbleEventArgs e)
       {
           mInformationBubble.Visible = false;
       }

       void mSceneControlBubbleResize(object sender, BubbleEventArgs e)
       {
           System.Drawing.Point point = new Point(e.Bubble.ClientLeft, e.Bubble.ClientTop);
           mInformationBubble.Location = point;
           mInformationBubble.Visible = true;
       }
       #endregion


       #region 开挖分析

       // 构造填挖方区域
       [Logger("debug")]
       public void AddExcavationRegion(string TextDeepth)
       {
           mExcavationDeepth = (-1) * Convert.ToDouble(TextDeepth);
           mSceneControl.Action = Action3D.MeasureArea;
           mSceneControl.Tracked -= new Tracked3DEventHandler(mSceneControl_Tracked);
           mSceneControl.Tracking -= new Tracking3DEventHandler(mSceneControl_Tracking);
           mSceneControl.Tracking += new Tracking3DEventHandler(mSceneControl_Tracking);
           mSceneControl.Tracked += new Tracked3DEventHandler(mSceneControl_Tracked);
           mFlag = true;
       }

       /// <summary>
       /// 寻找目标点，进行飞行
       /// </summary>
       public void TargetPoint()
       {
           Layer3Ds bd_3DLayers = mSceneControl.Scene.Layers;
           Layer3DDataset bd_3DLayer = bd_3DLayers[0] as Layer3DDataset;
           double bd_3DLayerHeight = bd_3DLayer.Bounds.Height;
           double bd_3DLayerWidth = bd_3DLayer.Bounds.Width;
           int PointX = (int)(bd_3DLayer.Bounds.Left + bd_3DLayerWidth / 2);
           int PointY = (int)(bd_3DLayer.Bounds.Bottom + bd_3DLayerHeight / 2);
           Point FlyPoint = new Point(PointX, PointY);
           Point3D FlyPoint3D = mSceneControl.Scene.PixelToGlobe(FlyPoint);
           FlyTo(200, FlyPoint3D.X, FlyPoint3D.Y, 0);
       }


       // 清除填挖方区域
       public void RemoveExcavationRegion()
       {
           if (mSceneControl.Scene.GlobalImage.ExcavationRegionCount > 0)
           {
               mSceneControl.Tracked -= new Tracked3DEventHandler(mSceneControl_Tracked);
               mSceneControl.Tracking -= new Tracking3DEventHandler(mSceneControl_Tracking);
               mSceneControl.Scene.GlobalImage.ClearExcavationRegions();
               mSceneControl.Scene.TrackingLayer.Clear();            
           }
       }

       // 绘制多边形时实时显示挖方面积
       void mSceneControl_Tracking(object sender, Tracking3DEventArgs e)
       {
           try
           {
               Point3D loction = new Point3D(0, 0, 0);
               Geometry3D geometry = e.Geometry; // 获取当前正在绘制的三维几何对象
               Point3D point3D = new Point3D(e.X, e.Y, e.CurrentHeight);
               point3D.Z = mSceneControl.Scene.GetAltitude(e.X, e.Y);
               Point point = mSceneControl.Scene.GlobeToPixel(point3D);
               Int32 index = mSceneControl.Scene.TrackingLayer.IndexOf("area");
               String text = String.Empty;

               if (mSceneControl.Action == Action3D.MeasureArea && geometry != null)
               {
                   if (index != -1)
                   {
                       mSceneControl.Scene.TrackingLayer.Remove(index);
                   }
                   text = String.Format("{0}{1}{2}", mArea, e.TotalArea, mSquareMeter);
                   mTextBoxTotalArea.Text = e.TotalArea.ToString().Substring(0, 8);
                   mTextBoxTotalsqure.Text = ((e.TotalArea) * (-1) * mExcavationDeepth).ToString("0.00");
                   loction = geometry.InnerPoint3D;
                   GeoText3D geoText = new GeoText3D(new TextPart3D(text, loction));
                   mSceneControl.Scene.TrackingLayer.Add(geoText, "area");
               }
               else
               {
                   return;
               }
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       // 绘制结束时将所绘制的区域添加到挖方区域中
       void mSceneControl_Tracked(object sender, Tracked3DEventArgs e)
       {
           try
           {
               mIndex++;
               Int32 index = mSceneControl.Scene.TrackingLayer.IndexOf("area");
               if (index != -1)
               {
                   mSceneControl.Scene.TrackingLayer.Remove(index);
               }

               string TextureSidePath = null;
               string TextureBottomPath = null;
               TextureSidePath = Application.StartupPath + "\\Resources\\excavationregion_side.jpg";
               TextureBottomPath = Application.StartupPath + "\\Resources\\excavationregion_top.jpg";

               //TextureSidePath = Path.GetFullPath(@"..\..\Resources\Side.JPG");
               //TextureBottomPath = Path.GetFullPath(@"..\..\Resources\Top.JPG");

               GeoRegion3D geoRegion3D = e.Geometry as GeoRegion3D;
               if (mFlag == true)
               {
                   mGeoStyle3D = new GeoStyle3D();
                   mGeoStyle3D.BottomAltitude = 0;
                   mGeoStyle3D.ExtendedHeight = mExcavationDeepth;
                   mGeoStyle3D.SideTextureFiles = new String[] { TextureSidePath };
                   mGeoStyle3D.TilingU = 1;
                   mGeoStyle3D.TilingV = 1;
                   mGeoStyle3D.TopTextureFile = TextureBottomPath;
                   mGeoStyle3D.TopTilingU = 1;
                   mGeoStyle3D.TopTilingV = 1;
                   mGeoStyle3D.TextureRepeatMode = TextureRepeatMode.RepeatTimes;
               }
               if (mGeoStyle3D != null)
               {
                   geoRegion3D.Style3D = mGeoStyle3D;
                   mSceneControl.Scene.GlobalImage.AddExcavationRegion(geoRegion3D, "ExcavationRegion" + mIndex);

                   Camera camera = new Camera();
                   camera.Longitude = geoRegion3D.InnerPoint3D.X;
                   camera.Latitude = geoRegion3D.InnerPoint3D.Y;
                   camera.AltitudeMode = AltitudeMode.Absolute;
                   camera.Altitude = 100;
                   camera.Tilt = 0;
                   mSceneControl.Scene.Fly(camera);
                   mInerX = geoRegion3D.InnerPoint3D.X;
                   mInerY = geoRegion3D.InnerPoint3D.Y;
               }
               mSceneControl.Action = Action3D.Pan2;
               mSceneControl.Scene.GlobalImage.Transparency = 0;


           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       public void ShowTarget()
       {
           FlyTo(200, mInerX, mInerY, 0);
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

       #region 爆管分析
       /// <summary>
       /// 选择爆管管线
       /// </summary>
       public void SelectPipeLine()
       {
           mSceneControl.Scene.TrackingLayer.Clear();
           mTextBoxPipeLineID.Text = "";
           mSceneControl.MouseDown -= new MouseEventHandler(mSceneControlMouseDown);
           mSceneControl.MouseDown += new MouseEventHandler(mSceneControlMouseDown);
           mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedControlAnalysis);
           mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedPipeBoom);
           mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlObjectSelectedPipeBoom);
       }
       public void DeletPipeLine()
       {
           mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedControlAnalysis);
           mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedPipeBoom);
           mSceneControl.MouseDown -= new MouseEventHandler(mSceneControlMouseDown);
       }
       private void FillDataGridTextBox(Dictionary<Int32, Int32> arcErrorInfos, Dictionary<Int32, Int32> nodeErrorInfos)
       {
           Int32 rowIndex = 0;
           try
           {
               foreach (KeyValuePair<Int32, Int32> kv in nodeErrorInfos)
               {
                   rowIndex = mDataGridViewResult.Rows.Add();
                   Int32 nodeID = kv.Key;
                   Int32 nodeErrorID = kv.Value;
                   mDataGridViewResult.Rows[rowIndex].Cells[0].Value = nodeID;
                   mDataGridViewResult.Rows[rowIndex].Cells[1].Value = "管点";
                   mDataGridViewResult.Rows[rowIndex].Cells[2].Value = GetErrorInfo(nodeErrorID);
               }
               foreach (KeyValuePair<Int32, Int32> kv in arcErrorInfos)
               {
                   rowIndex = mDataGridViewResult.Rows.Add();
                   Int32 arcID = kv.Key;
                   Int32 arcErrorID = kv.Value;
                   mDataGridViewResult.Rows[rowIndex].Cells[0].Value = arcID;
                   mDataGridViewResult.Rows[rowIndex].Cells[1].Value = "管段";
                   mDataGridViewResult.Rows[rowIndex].Cells[2].Value = GetErrorInfo(arcErrorID);
               }
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }
       }
       private String GetErrorInfo(Int32 type)
       {
           String error = String.Empty;
           switch (type)
           {
               case 1:
                   error = "结点ID重复";
                   break;
               case 2:
                   error = "弧段ID重复";
                   break;
               case 3:
                   error = "弧段没有对应结点";
                   break;
               case 4:
                   error = "空间位置不匹配";
                   break;
               case 5:
                   error = "复杂线对象";
                   break;
           }
           return error;
       }
       Point3D mControlPoint;
       private DatasetVector mBufferDataset;
       private Recordset mLineRecordset;
       Recordset mRecordset;
       private Layer3D mLayerBuffer;
       /// <summary>
       /// 爆管分析
       /// </summary>
       public void PipeAnalyst()
       {

           //三维网络信息设置
           FacilityAnalystSetting3D facilityAnalystSetting = new FacilityAnalystSetting3D();
           facilityAnalystSetting.NetworkDataset = mPipeNet;
           // facilityAnalystSetting.DirectionField = "direction";
           facilityAnalystSetting.EdgeIDField = "SMEDGEID";
           facilityAnalystSetting.NodeIDField = "SMNODEID";
           facilityAnalystSetting.FNodeIDField = "SMFNODE";
           facilityAnalystSetting.TNodeIDField = "SMTNODE";
           facilityAnalystSetting.Tolerance = 0.001;


           //权字段信息设置
           WeightFieldInfo3D weightFieldInfo = new WeightFieldInfo3D();
           weightFieldInfo.Name = "Length";
           weightFieldInfo.FTWeightField = "SMLENGTH";
           weightFieldInfo.TFWeightField = "SMLENGTH";
           WeightFieldInfos3D weightFieldInfos = new WeightFieldInfos3D();
           weightFieldInfos.Add(weightFieldInfo);
           facilityAnalystSetting.WeightFieldInfos = weightFieldInfos;


           FacilityAnalyst3D mFacilityAnalyst = new FacilityAnalyst3D();
           mFacilityAnalyst.AnalystSetting = facilityAnalystSetting;


           FacilityAnalystCheckResult3D result = mFacilityAnalyst.Check();
           Dictionary<Int32, Int32> arcErrorInfos = result.ArcErrorInfos;
           Dictionary<Int32, Int32> nodeErrorInfos = result.NodeErrorInfos;

           FillDataGridTextBox(arcErrorInfos, nodeErrorInfos);
           mLayerNetLine.IsSelectable = true;
           mLayerNetNode.IsSelectable = true;
           Boolean isLoad = mFacilityAnalyst.Load();
           Recordset recordset = null;
           try
           {
               recordset = mPipeNet.ChildDataset.Query("SymbolID =330122", CursorType.Static);

               Int32[] sourceNodeIDs = new Int32[recordset.RecordCount];
               recordset.MoveFirst();
               for (int i = 0; i < recordset.RecordCount; i++)
               {
                   sourceNodeIDs[i] = recordset.GetInt32("SmID");
                   recordset.MoveNext();
               }
               // 上游最近设施点
               FacilityAnalystResult3D resultFindCriticalFacilities = mFacilityAnalyst.FindCriticalFacilitiesUpFromEdge(sourceNodeIDs, mPipeBoomId, true);
               {
                   if (resultFindCriticalFacilities != null)
                   {
                       mResultFacilities = resultFindCriticalFacilities.Nodes;
                       mFacilitiesID = new Int32[mResultFacilities.Length];
                       FillDataGridViewResult();
                       GeoStyle3D style = new GeoStyle3D();
                       style.FillForeColor = Color.Blue;
                       mLayerNetNode.Selection.AddRange(mFacilitiesID);
                       mLayerNetNode.Selection.Style = style;
                       mLayerNetNode.Selection.UpdateData();

                       int ResID = mResultFacilities[0];
                       Recordset recordsetRes = mPipeNet.ChildDataset.Query("SmID =" + ResID, CursorType.Static);
                       double mPx = Convert.ToDouble(recordsetRes.GetFieldValue("SmX"));
                       double mPy = Convert.ToDouble(recordsetRes.GetFieldValue("SmY"));
                       mControlPoint = new Point3D();
                       mControlPoint.X = mPx;
                       mControlPoint.Y = mPy;
                   }
                   else
                   {
                       mResultFacilities = new Int32[1] { 0 };
                       MessageBox.Show("该管线暂时无法找到阀门信息");
                   }
               }


               // 下游追踪 
               FacilityAnalystResult3D resultTraceDown = mFacilityAnalyst.TraceDownFromEdge(mPipeBoomId, "Length", true);
               if (resultTraceDown != null)
               {
                   mResultEdges = new Int32[resultTraceDown.Edges.Length];
                   mResultEdges = resultTraceDown.Edges;
                   FillDataGridViewEdge();
                   GeoStyle3D style = new GeoStyle3D();
                   mLayerNetLine.Selection.AddRange(mResultEdges);
                   style.LineColor = Color.Blue;
                   mLayerNetLine.Selection.Style = style;
                   mLayerNetLine.Selection.UpdateData();
               }
               else
               { mResultEdges = new Int32[1] { 0 }; MessageBox.Show("该管线无下游信息"); }
           }
           catch (System.Exception e)
           {
               Trace.WriteLine(e.Message);
           }
           finally
           {
               if (recordset != null)
               {
                   recordset.Close();
                   recordset.Dispose();
               }
           }
       }

       public void BoomCreateBuffer()
       {
           try
           {
               #region 缓冲区创建
               // mDatasource = mWorkspace.Datasources["guilinligong"];
               Selection3D[] selection3d = mSceneControl.Scene.FindSelection(true);
               Recordset mLineRecordset = selection3d[0].ToRecordset();
               Datasource datasource = mUseData.DataSource;
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
                   Geometry3D mGeo = mLineRecordset.GetGeometry() as Geometry3D;
                   BufferAnalyst3DParameter bufferAnalyst3DParameter = new BufferAnalyst3DParameter();
                   bufferAnalyst3DParameter.EndType = SuperMap.Realspace.SpatialAnalyst.BufferEndType.Round;
                   bufferAnalyst3DParameter.BufferDistance = 10;
                   bufferAnalyst3DParameter.BufferQuality = 20;
                   Geometry3D geo3D = Geometrist3D.CreateBuffer(mGeo, bufferAnalyst3DParameter, mLineRecordset.Dataset.PrjCoordSys);
                   GeoStyle3D geoStyle3D = new GeoStyle3D();
                   mRecordset.AddNew(geo3D);
                   mRecordset.MoveNext();
                   mLineRecordset.MoveNext();
                   int a = mRecordset.RecordCount;
                   int b = mLineRecordset.RecordCount;
               }

               mRecordset.Update();
               mRecordset.Dataset.Tolerance.NodeSnap = 0.0002;
               Layer3DSettingVector layer3DSetting = new Layer3DSettingVector();
               GeoStyle3D style2 = new GeoStyle3D();
               //style2.FillForeColor = Color.FromArgb(0x3B, 0xFF, 0x80, 0x40);
               style2.FillForeColor = Color.FromArgb(127, 233, 74, 22);
               style2.FillBackColor = Color.GreenYellow;
               style2.AltitudeMode = AltitudeMode.RelativeToGround;
               style2.FillMode = FillMode3D.Fill;
               layer3DSetting.Style = style2;

               mLayerBuffer = mSceneControl.Scene.Layers.Add(mRecordset.Dataset, layer3DSetting, true);
               mLayerBuffer.UpdateData();
               mSceneControl.Scene.Refresh();

               #endregion
           }
           catch (Exception ex)
           {
               MessageBox.Show("信息已清除，无法分析影响区域");
               Trace.WriteLine(ex);
           }


       }


       public void ClearBoomBuffer()
       {
           try
           {
               if (mLayerBuffer != null)
               {
                   mSceneControl.Scene.Layers.Remove(mLayerBuffer.Name);
                   mLayerBuffer = null;
               }
               mSceneControl.Scene.Refresh();

               if (mBufferDataset != null)
               {

                   mUseData.DataSource.Datasets.Delete(mBufferDataset.Name);
                   mBufferDataset = null;
               }
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }

       }

       private void FillDataGridViewResult()
       {
           try
           {
               DataGridViewTextBoxColumn column1 = new DataGridViewTextBoxColumn();
               column1.HeaderText = "应关闭阀门";
               column1.Width = 95;

               DataGridViewTextBoxColumn column2 = new DataGridViewTextBoxColumn();
               column2.HeaderText = "埋深";
               column2.Width = 55;

               DataGridViewTextBoxColumn column3 = new DataGridViewTextBoxColumn();
               column3.HeaderText = "经度";
               column3.Width = 69;

               DataGridViewTextBoxColumn column4 = new DataGridViewTextBoxColumn();
               column4.HeaderText = "纬度";
               column4.Width = 69;

               mDataGridViewResult.Columns.Add(column1);
               mDataGridViewResult.Columns.Add(column2);
               mDataGridViewResult.Columns.Add(column3);
               mDataGridViewResult.Columns.Add(column4);

               Int32 nIndex = 0;
               for (int i = 0; i < mResultFacilities.Length; i++)
               {

                   nIndex = mDataGridViewResult.Rows.Add();
                   String expression = "SmID = " + mResultFacilities[i];
                   Recordset node = mPipeNet.ChildDataset.Query(expression, CursorType.Static);
                   mFacilitiesID[i] = node.GetInt32("SmID");
                   mDataGridViewResult.Rows[nIndex].Cells[0].Value = mFacilitiesID[i];
                   mDataGridViewResult.Rows[nIndex].Cells[1].Value = (Double)node.GetFieldValue("BottomAltitude") + (Double)node.GetFieldValue("SMZ");
                   mDataGridViewResult.Rows[nIndex].Cells[2].Value = String.Format("{0:F6}", node.GetFieldValue("SMX"));
                   mDataGridViewResult.Rows[nIndex].Cells[3].Value = String.Format("{0:F6}", node.GetFieldValue("SMY"));
               }
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }


       }

       private void FillDataGridViewEdge()
       {
           try
           {
               DataGridViewTextBoxColumn column1 = new DataGridViewTextBoxColumn();
               column1.HeaderText = "受影响管段";
               column1.Width = 85;

               DataGridViewTextBoxColumn column2 = new DataGridViewTextBoxColumn();
               column2.HeaderText = "长度";
               column2.Width = 60;

               DataGridViewTextBoxColumn column3 = new DataGridViewTextBoxColumn();
               column3.HeaderText = "起始结点";
               column3.Width = 75;

               DataGridViewTextBoxColumn column4 = new DataGridViewTextBoxColumn();
               column4.HeaderText = "终止结点";
               column4.Width = 75;

               mDataGridViewEdge.Columns.Add(column1);
               mDataGridViewEdge.Columns.Add(column2);
               mDataGridViewEdge.Columns.Add(column3);
               mDataGridViewEdge.Columns.Add(column4);


               Int32 nIndex = 0;

               for (int i = 0; i < mResultEdges.Length; i++)
               {

                   nIndex = mDataGridViewEdge.Rows.Add();

                   String expression = "SmID = " + mResultEdges[i];
                   Recordset edge = mPipeNet.Query(expression, CursorType.Static);
                   mDataGridViewEdge.Rows[nIndex].Cells[0].Value = mResultEdges[i];
                   mDataGridViewEdge.Rows[nIndex].Cells[1].Value = String.Format("{0:F6}", edge.GetFieldValue("SMLENGTH"));
                   mDataGridViewEdge.Rows[nIndex].Cells[2].Value = (Int32)edge.GetFieldValue("SMFNODE");
                   mDataGridViewEdge.Rows[nIndex].Cells[3].Value = (Int32)edge.GetFieldValue("SMTNODE");
               }
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }

       }

       public void FlyToValve(Int32 row)
       {
           Recordset selected = null;
           try
           {
               Int32 id = Convert.ToInt32(mDataGridViewResult[0, row].Value);
               selected = mPipeNet.ChildDataset.Query("SMID = " + id, CursorType.Static);
               double Lon = Convert.ToDouble(selected.GetFieldValue("SmX"));
               double Lat = Convert.ToDouble(selected.GetFieldValue("SmY"));
               Geometry3D geometry = selected.GetGeometry() as GeoPoint3D;
               mSceneControl.Scene.EnsureVisible(geometry.Bounds, 10);
               DisplayBubble(geometry, id);
               Camera camera = new Camera();
               camera.Longitude = Convert.ToDouble(Lon);
               camera.Latitude = Convert.ToDouble(Lat);
               camera.AltitudeMode = AltitudeMode.RelativeToGround;
               camera.Tilt = 0;
               camera.Altitude = 25;
               mSceneControl.Scene.Fly(camera, 10);
           }
           catch (System.Exception e)
           {
               Trace.WriteLine(e.Message);
           }
           finally
           {
               if (selected != null)
               {
                   selected.Close();
                   selected.Dispose();
               }
           }

       }

       public void FlyToPipe(Int32 row)
       {
           Recordset selected = null;
           try
           {
               Int32 id = Convert.ToInt32(mDataGridViewEdge[0, row].Value);
               selected = mPipeNet.Query("SMID = " + id, CursorType.Static);

               Geometry3D geometry = selected.GetGeometry() as GeoLine3D;
               mSceneControl.Scene.EnsureVisible(geometry.Bounds, 10);

               DisplayBubble(geometry, id);


           }
           catch (System.Exception e)
           {
               Trace.WriteLine(e.Message);
           }
           finally
           {
               if (selected != null)
               {
                   selected.Close();
                   selected.Dispose();
               }
           }

       }

       /// <summary>
       /// 根据在所点击的表格显示气泡
       /// </summary>
       /// <param name="geometry"></param>
       /// <param name="id"></param>
       private void DisplayBubble(Geometry3D geometry, Int32 id)
       {
           try
           {
               mSceneControl.Bubbles.Clear();
               Bubble bubble = new Bubble();
               mInformationBubble.Visible = true;
               if (geometry.Type == GeometryType.GeoPoint3D)
               {
                   mInformationBubble.Description.Text = string.Format("应关闭的阀门： ID = " + id + "\n阀门类型为： " + mBoonPipeType);
               }

               else
               {
                   mInformationBubble.Description.Text = string.Format("受影响管段： ID = " + id + "\n管线类型为： " + mBoonPipeType);
               }
               bubble.Pointer = new Point3D(geometry.InnerPoint3D.X, geometry.InnerPoint3D.Y, PipeDeepth);
               bubble.ClientWidth = mInformationBubble.Width;
               bubble.ClientHeight = mInformationBubble.Height;

               mInformationBubble.Location = new Point(bubble.ClientLeft, bubble.ClientTop);
               mSceneControl.Bubbles.Add(bubble);
               mSceneControl.Scene.Refresh();
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }
       }
       /// <summary>
       /// 清除分析结果及表格
       /// </summary>
       public void Clear()
       {
           try
           {
               mSceneControl.Bubbles.Clear();
               mLayerNetLine.Selection.Clear();
               mLayerNetNode.Selection.Clear();
               mLayerNetLine.Selection.Remove(mSelectedLine.ID);
               Layer3Ds m_layer = mSceneControl.Scene.Layers;
               foreach (Layer3D layer in m_layer)
               {
                   if (layer.Selection != null)
                   {
                       layer.Selection.Clear();
                   }
               }
               mSceneControl.Scene.TrackingLayer.Clear();
               mTextBoxPipeLineID.Text = "";
               ClearDataGridView();
               mPipeNet = null;
               mResultEdges = null;
               mLine3D = null;
               mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedPipeBoom);
               mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedControlAnalysis);
               mSceneControl.MouseDown -= new MouseEventHandler(mSceneControlMouseDown);
               mSceneControl.MouseDoubleClick -= mSceneControlBoomStartMouseDoubleClick;
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }

       }
       /// <summary>
       /// 清除结果表格
       /// </summary>
       public void ClearDataGridView()
       {
           try
           {
               Int32 count = mDataGridViewResult.Columns.Count;
               for (int i = 0; i < count; i++)
               {

                   mDataGridViewResult.Columns.RemoveAt(0);

               }

               Int32 egde = mDataGridViewEdge.Columns.Count;
               for (int j = 0; j < egde; j++)
               {
                   mDataGridViewEdge.Columns.RemoveAt(0);


               }
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }

       }
       Point3D mBoomClickPoint;
       /// <summary>
       /// 在场景窗口按下鼠标事件
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       void mSceneControlMouseDown(object sender, MouseEventArgs e)
       {
           try
           {
               if (e.Button == MouseButtons.Left)
               {
                   mClickPoint = new Point3D();
                   // 鼠标左键点击点为爆管点   
                   mClickPoint = mSceneControl.Scene.PixelToGlobe(e.Location, PixelToGlobeMode.TerrainAndModel);
                   Point mTestPoin = new Point();
                   mTestPoin.X = e.X;
                   mTestPoin.Y = e.Y;
                   Point3D clickPoint3D = mSceneControl.Scene.PixelToGlobe(mTestPoin, SuperMap.Realspace.PixelToGlobeMode.TerrainAndModel);
                   mBoomClickPoint.X = clickPoint3D.X;
                   mBoomClickPoint.Y = clickPoint3D.Y;
                   mBoomClickPoint.Z = clickPoint3D.Z;
               }
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       int mPipeBoom;//粒子特效索引
       /// <summary>
       /// 在爆管点添加喷泉粒子，模拟水管爆破的效果
       /// </summary>
       /// <param name="pt3D"></param>
       private void AddFountainParticle(Point3D pt3D)
       {
           try
           {
               GeoPoint3D geopoint3d = new GeoPoint3D(pt3D);
               GeoParticle geoparticle = new GeoParticle(ParticleType.Fountain, geopoint3d);
               GeoStyle3D explodeStyle = new GeoStyle3D();
               explodeStyle.AltitudeMode = AltitudeMode.RelativeToGround;
               geoparticle.Style3D = explodeStyle;
               string path;
               if (System.Environment.Is64BitProcess == true)
               {
                   path = Path.GetFullPath(@"..\..\..\Resources\FountainParticle.par");
                   geoparticle.FromXML(@"..\..\..\Resources\FountainParticle.par");
               }
               else if (System.Environment.Is64BitProcess == false)
               {
                   path = Path.GetFullPath(@"..\..\Resources\FountainParticle.par");
                   geoparticle.FromXML(@"..\..\Resources\FountainParticle.par");
               }
               mSceneControl.Scene.TrackingLayer.Add(geoparticle, "ExplodeParticle");
               mPipeBoom = mSceneControl.Scene.TrackingLayer.IndexOf("ExplodeParticle");
               mSceneControl.MouseDown -= new MouseEventHandler(mSceneControlMouseDown);

           }
           catch (System.Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       /// <summary>
       /// 对象选择事件
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       void mSceneControlObjectSelectedPipeBoom(object sender, ObjectSelectedEventArgs e)
       {
           Recordset recordset = null;
           Selection3D[] selection3d = null;
           try
           {
               // 无对象被选中
               if (e.Count == 0)
               {
                   MessageBox.Show("未选择对象!");
               }
               //有对象选中
               if (e.Count > 0)
               {
                   mSceneControl.Action = Action3D.Pan2;
                   selection3d = mSceneControl.Scene.FindSelection(true);
                   recordset = selection3d[0].ToRecordset();
                   Geometry geo = recordset.GetGeometry();
                   GeometryType recordsetType = geo.Type;
                   if (recordsetType == GeometryType.GeoPoint3D)
                   {
                       MessageBox.Show("请选择三维管线");
                   }
                   else if (recordsetType == GeometryType.GeoLine3D)
                   {
                       mPointType = 2;

                       if (recordset.GetFieldValue("PipeType").ToString() != null)
                       {
                           mBoonPipeType = recordset.GetFieldValue("PipeType").ToString();
                           switch (mBoonPipeType.Trim())
                           {
                               case "给水": mLayerNetLine = mUseData.SupplyWaterLayerNetLine;
                                   mLayerNetNode = mUseData.SupplyWaterLayerNetNode;
                                   mPipeNet = mUseData.SupplyWaterNetWork;
                                   PipeDeepth = -2;
                                   break;
                               case "排水": mLayerNetLine = mUseData.OutWaterLayerNetLine;
                                   mLayerNetNode = mUseData.OutWaterLayerNetNode;
                                   mPipeNet = mUseData.OutWaterNetWork;
                                   PipeDeepth = -3;
                                   break;
                               default:
                                   MessageBox.Show("请选择其它类型管线");
                                   break;
                           }
                           if (mPipeNet != null)
                           {
                               mPipeBoomId = Convert.ToInt32(recordset.GetFieldValue("SmID"));
                               mSelectedLine = recordset.GetGeometry() as GeoLine3D;
                               mTextBoxPipeLineID.Text = mSelectedLine.ID.ToString();
                               mClickPoint.Z = PipeDeepth;
                               AddFountainParticle(mClickPoint);
                               mGroupBoxBoom.Enabled = true;
                               mGroupBoxBoomInfluence.Enabled = true;
                               PipeAnalyst();
                           }
                       }

                       else { MessageBox.Show("请选择管线对象"); }
                   }
               }
           }

           catch (System.Exception ex)
           {
               Trace.Write(ex.Message);
           }
           finally
           {
               if (recordset != null)
               {
                   recordset.Close();
                   recordset.Dispose();
               }
           }
       }

       private void PiPePoint(Geometry3D GeoPoint, Point3D ClickPoint)
       {
           int x1 = Convert.ToInt32(GeoPoint.InnerPoint.X);
           int y1 = Convert.ToInt32(GeoPoint.InnerPoint.Y);
           Point p = mSceneControl.Scene.GlobeToPixel(ClickPoint);
           int x2 = p.X;
           int y2 = p.Y;
           Point3D FlyPoint3D = mSceneControl.Scene.PixelToGlobe(new Point((x1 + x2) / 2, (y1 + y2) / 2));
           FlyTo(100, FlyPoint3D.X, FlyPoint3D.Y, 45);
       }
       #region 事故处理
       //用于保存飞行路线
       public GeoLine3D mLine3D;
       public void FindStartPoint()
       {
           try
           {
               if (mLine3D != null && mLine3D.PartCount > 0)
               {
                   mLine3D.SetEmpty();
               }
               else if (mLine3D == null)
               {
                   mLine3D = new GeoLine3D();
               }
               mSceneControl.Action = Action3D.Select;
               mSceneControl.MouseDown -= new MouseEventHandler(mSceneControlMouseDown);
               mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedControlAnalysis);
               mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedPipeBoom);
               mSceneControl.MouseDoubleClick -= mSceneControlBoomStartMouseDoubleClick;
               mSceneControl.MouseDoubleClick += mSceneControlBoomStartMouseDoubleClick;

           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }
       }
       private DatasetVector mRodeNetWork;
       private void mSceneControlBoomStartMouseDoubleClick(object sender, MouseEventArgs e)
       {

           try
           {
               mRodeNetWork = mUseData.RodeNetwork;
               Point mClickBoomPoint = new Point();
               mClickBoomPoint.X = e.X;
               mClickBoomPoint.Y = e.Y;
               if (e.Button == MouseButtons.Left)
               {
                   Point3D clickPoint3D = mSceneControl.Scene.PixelToGlobe(mClickBoomPoint, SuperMap.Realspace.PixelToGlobeMode.TerrainAndModel);
                   Point3D antualPoint3D = Point3D.Empty;
                   if (clickPoint3D != Point3D.Empty && CheckPoint(clickPoint3D))
                   {
                       SetStartPoint(clickPoint3D);
                       DisplayStartPoint();
                   }
               }
               mSceneControl.Action = Action3D.Pan;
               if (mResultEdges != null)
               {
                   GeoStyle3D style = new GeoStyle3D();
                   mLayerNetLine.Selection.AddRange(mResultEdges);
                   mLayerNetLine.Selection.Add(mPipeBoomId);
                   style.LineColor = Color.Blue;
                   mLayerNetLine.Selection.Style = style;
                   mLayerNetLine.Selection.UpdateData();
               }
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       /// <summary>
       /// 检查点
       /// </summary>
       /// <param name="point"></param>
       /// <returns></returns>
       public Boolean CheckPoint(Point3D point)
       {
           Boolean result = false;
           try
           {
               Double longitudeMin = mRodeNetWork.Bounds.Left;
               Double longitudeMax = mRodeNetWork.Bounds.Right;
               Double latitudeMin = mRodeNetWork.Bounds.Bottom;
               Double latitudeMax = mRodeNetWork.Bounds.Top;

               Double longitude = point.X;
               Double latitude = point.Y;

               result = (longitude >= longitudeMin && longitude <= longitudeMax) && (latitude >= latitudeMin && latitude <= latitudeMax);
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
           return result;
       }
       // 用于存储分析设置的起始以及起始点在跟踪层上的标签
       private String mStartPointTag;
       private GeoPoint3D mStartPoint;
       // 用于存储起始地标在跟踪层上的标签
       private String mMarkStart;
       /// <summary>
       /// 设置起始点
       /// </summary>
       public void SetStartPoint(Point3D point3D)
       {
           try
           {
               ClearStartPoint();
               mSceneControl.Scene.Refresh();
               mStartPoint = new GeoPoint3D(point3D);
               mStartPoint.Z = 2;
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       /// <summary>
       /// 释放起始点
       /// </summary>
       public void ClearStartPoint()
       {
           try
           {
               mStartPoint.X = 0;
               mStartPoint.Y = 0;
               mStartPoint.Z = 0;

               Int32 index = mSceneControl.Scene.TrackingLayer.IndexOf(mStartPointTag);
               if (index >= 0)
               {
                   mSceneControl.Scene.TrackingLayer.Remove(index);
                   mSceneControl.Scene.Refresh();
               }

               mStartPointTag = String.Empty;

               index = mSceneControl.Scene.TrackingLayer.IndexOf(mMarkStart);
               if (index >= 0)
               {
                   mSceneControl.Scene.TrackingLayer.Remove(index);
                   mSceneControl.Scene.Refresh();
               }

               mMarkStart = String.Empty;
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       /// <summary>
       /// 在三维场景中显示起始点
       /// </summary>
       public void DisplayStartPoint()
       {
           try
           {
               GeoStyle3D pointStyle = new GeoStyle3D();
               pointStyle.MarkerColor = Color.FromArgb(255, 51, 255, 0);
               pointStyle.MarkerSize = 10.0;
               pointStyle.AltitudeMode = AltitudeMode.Absolute;

               mStartPoint.Style3D = pointStyle;

               mStartPointTag = "startPoint";
               mSceneControl.Scene.TrackingLayer.Add(mStartPoint, mStartPointTag);

               mMarkStart = "PlaceMarkStart";
               GeoPlacemark markStart = new GeoPlacemark("起始点", mStartPoint);
               markStart.NameStyle.ForeColor = Color.FromArgb(255, 51, 255, 0);
               if (System.Environment.Is64BitProcess)
               {
                   markStart.Style3D.MarkerFile = @"..\..\..\Resource\blupin.png";
               }
               else
               {
                   markStart.Style3D.MarkerFile = @"..\..\Resource\blupin.png";
               }
               mSceneControl.Scene.TrackingLayer.Add(markStart, mMarkStart);

               mSceneControl.Scene.Refresh();
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       int ControlOrSolveFlag;
       /// <summary>
       /// 设置网络分析参数
       /// </summary>
       public void SetParameter(int Flag)
       {
           try
           {
               ControlOrSolveFlag = Flag;
               if (ControlOrSolveFlag == 1)//阀门控制点
               {
                   Point2D startPoint = new Point2D(mStartPoint.X, mStartPoint.Y);
                   Point2D endPoint = new Point2D(mControlPoint.X, mControlPoint.Y);
                   Point2Ds points = new Point2Ds(new Point2D[] { startPoint, endPoint });
                   mAnalystParameter.IsRoutesReturn = true;
                   mAnalystParameter.WeightName = "Smlength";
                   mAnalystParameter.IsEdgesReturn = true;
                   mAnalystParameter.Points = points;
               }
               else if (ControlOrSolveFlag == 2)//保管点
               {
                   Point2D startPoint = new Point2D(mStartPoint.X, mStartPoint.Y);
                   Point2D endPoint = new Point2D(mBoomClickPoint.X, mBoomClickPoint.Y);
                   Point2Ds points = new Point2Ds(new Point2D[] { startPoint, endPoint });
                   mAnalystParameter.IsRoutesReturn = true;
                   mAnalystParameter.WeightName = "Smlength";
                   mAnalystParameter.IsEdgesReturn = true;
                   mAnalystParameter.Points = points;
               }

               BeginNetworkAnalyst();

           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       /// <summary>
       /// 加载网络分析模型
       /// </summary>
       public void LoadModel()
       {
           try
           {

               if (!mSceneControl.Scene.Layers.Contains(mStrLayer3DNodeNetwork))
               {
                   //Layer3DDataset layerNetNode = mSceneControl.Scene.Layers.Add(mRodeNetWork.ChildDataset, new Layer3DSettingVector(), true);
                   //Layer3DDataset layerNetLine = mSceneControl.Scene.Layers.Add(mRodeNetWork, new Layer3DSettingVector(), true);
                   //layerNetLine.IsSelectable = false;
                   //layerNetNode.IsSelectable = false;
               }
               else
               {
                   mSceneControl.Scene.Layers[mStrLayer3DNodeNetwork].IsVisible = true;
                   mSceneControl.Scene.Layers[mStrLayer3DLineNetwork].IsVisible = true;
               }
               WeightFieldInfo weightInfo = new WeightFieldInfo();
               weightInfo.Name = "SmLength";
               weightInfo.FTWeightField = "SmLength";
               weightInfo.TFWeightField = "SmLength";

               TransportationAnalystSetting analystSetting = new TransportationAnalystSetting();
               analystSetting.NetworkDataset = mRodeNetWork;
               analystSetting.EdgeIDField = "SmEdgeID";
               //analystSetting.NodeIDField = "SmNodeID";
               analystSetting.FNodeIDField = "SmFNode";
               analystSetting.TNodeIDField = "SmTNode";
               analystSetting.WeightFieldInfos.Add(weightInfo);
               mAnalyst.AnalystSetting = analystSetting;
               DatasetType ty1 = analystSetting.NetworkDataset.Type;
               mAnalyst.Load();

           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       /// <summary>
       /// 进行网络分析分析
       /// </summary>
       public void BeginNetworkAnalyst()
       {
           try
           {
               mAnalystResult = mAnalyst.FindTSPPath(mAnalystParameter, false);

               GeoLine line = mAnalystResult.Routes[0].ConvertToLine();
               if (mLine3D == null)
               {
                   mLine3D = new GeoLine3D();
               }
               mLine3D.SetEmpty();
               for (Int32 i = 0; i < line.PartCount; i++)
               {
                   mLine3D.AddPart(line[i].ToPoint3Ds());
               }
               mLine3D[0].Insert(0, new Point3D(mStartPoint.X, mStartPoint.Y, 0));
               if (ControlOrSolveFlag == 1)
               {
                   mLine3D[0].Add(new Point3D(mControlPoint.X, mControlPoint.Y, 0));
               }
               else if (ControlOrSolveFlag == 2)
               {
                   mLine3D[0].Add(new Point3D(mBoomClickPoint.X, mBoomClickPoint.Y, 0));
               }

               DisplayFlyRoute();
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }
       /// <summary>
       /// 显示分析路线
       /// </summary>
       public void DisplayFlyRoute()
       {
           try
           {
               GeoStyle3D style3D = new GeoStyle3D();
               style3D.AltitudeMode = AltitudeMode.RelativeToGround;
               style3D.BottomAltitude = 0.5;
               style3D.LineColor = Color.Yellow;
               style3D.LineWidth = 5;
               mLine3D.Style3D = style3D;
               if (mSceneControl.Scene.TrackingLayer.IndexOf(mStrTrackingTagNetworkResult) >= 0)
               {
                   mSceneControl.Scene.TrackingLayer.Remove(mSceneControl.Scene.TrackingLayer.IndexOf(mStrTrackingTagNetworkResult));
               }
               Int32 lineIndex = mSceneControl.Scene.TrackingLayer.Add(mLine3D, mStrTrackingTagNetworkResult);
           }
           catch (Exception ex)
           {
               Trace.WriteLine(ex.Message);
           }
       }


       #endregion

       #region 影响区域




       #endregion


       #endregion

       #region 开关阀分析
       /// <summary>
       /// 开关阀分析--选择线对象
       /// </summary>
       public void SelectLine()
       {
           mSceneControl.Action = Action3D.Select;
           mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedPipeBoom);
           mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedControlAnalysis);
           mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlObjectSelectedControlAnalysis);
       }

       /// <summary>
       /// 对象选择
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
       void mSceneControlObjectSelectedControlAnalysis(object sender, ObjectSelectedEventArgs e)
       {
           Recordset recordset = null;
           Selection3D[] selection3d = null;
           try
           {
               // 无对象被选中
               if (e.Count == 0)
               {
                   MessageBox.Show("未选择对象!");
               }
               //有对象选中
               if (e.Count > 0)
               {
                   selection3d = mSceneControl.Scene.FindSelection(true);
                   recordset = selection3d[0].ToRecordset();
                   Geometry geo = recordset.GetGeometry();
                   GeometryType recordsetType = geo.Type;

                   if (recordsetType == GeometryType.GeoPoint3D)
                   {
                       MessageBox.Show("请选择三维管线");
                   }
                   else if (recordsetType == GeometryType.GeoLine3D)
                   {
                       mPipeType = recordset.GetFieldValue("PipeType").ToString();
                       switch (mPipeType.Trim())
                       {
                           case "给水": mLayerNetLine = mUseData.SupplyWaterLayerNetLine;
                               mLayerNetNode = mUseData.SupplyWaterLayerNetNode;
                               mPipeNet = mUseData.SupplyWaterNetWork;
                               mControlDeepth = -2;
                               break;
                           case "排水": mLayerNetLine = mUseData.OutWaterLayerNetLine;
                               mLayerNetNode = mUseData.OutWaterLayerNetNode;
                               mPipeNet = mUseData.OutWaterNetWork;
                               mControlDeepth = -3.5;
                               break;
                           default:
                               MessageBox.Show("请选择其它类型管线");
                               break;
                       }
                       if (mPipeNet != null)
                       {
                           GeoStyle3D style = new GeoStyle3D();
                           style.FillForeColor = Color.Red;
                           mSelectedLine = recordset.GetGeometry() as GeoLine3D;
                           mControlID = Convert.ToInt32(recordset.GetFieldValue("SmID"));
                           mControlObjectFlag = true;
                       }
                   }
               }
           }
           catch (System.Exception ex)
           {
               Trace.Write(ex.Message);
           }
           finally
           {
               if (recordset != null)
               {
                   recordset.Close();
                   recordset.Dispose();
               }
           }
       }
       /// <summary>
       /// 开关阀分析---分析线对象
       /// </summary>
       public void AnalysisLine()
       {

           //三维网络信息设置
           FacilityAnalystSetting3D facilityAnalystSetting = new FacilityAnalystSetting3D();
           facilityAnalystSetting.NetworkDataset = mPipeNet;
           // facilityAnalystSetting.DirectionField = "direction";
           facilityAnalystSetting.EdgeIDField = "SMEDGEID";
           facilityAnalystSetting.NodeIDField = "SMNODEID";
           facilityAnalystSetting.FNodeIDField = "SMFNODE";
           facilityAnalystSetting.TNodeIDField = "SMTNODE";
           facilityAnalystSetting.Tolerance = 0.001;


           //权字段信息设置
           WeightFieldInfo3D weightFieldInfo = new WeightFieldInfo3D();
           weightFieldInfo.Name = "Length";
           weightFieldInfo.FTWeightField = "SMLENGTH";
           weightFieldInfo.TFWeightField = "SMLENGTH";
           WeightFieldInfos3D weightFieldInfos = new WeightFieldInfos3D();
           weightFieldInfos.Add(weightFieldInfo);
           facilityAnalystSetting.WeightFieldInfos = weightFieldInfos;


           FacilityAnalyst3D mFacilityAnalyst = new FacilityAnalyst3D();
           mFacilityAnalyst.AnalystSetting = facilityAnalystSetting;
           Boolean isLoad = mFacilityAnalyst.Load();
           Recordset recordset = null;
           try
           {
               recordset = mPipeNet.ChildDataset.Query("SymbolID =330122 ", CursorType.Static);
               Int32[] sourceNodeIDs = new Int32[recordset.RecordCount];
               recordset.MoveFirst();
               for (int i = 0; i < recordset.RecordCount; i++)
               {
                   sourceNodeIDs[i] = recordset.GetInt32("SmID");
                   recordset.MoveNext();
               }
               // 上游最近设施点
               FacilityAnalystResult3D resultFindCriticalFacilities = mFacilityAnalyst.FindCriticalFacilitiesUpFromEdge(sourceNodeIDs, mControlID, true);
               mResultFacilities = resultFindCriticalFacilities.Nodes;
               String expression = "SmID = " + mResultFacilities[0];
               Recordset node = mPipeNet.ChildDataset.Query(expression, CursorType.Static);
               mInfluencePipe = mResultFacilities[0];
               mAnalongitude = node.GetFieldValue("SmX");
               mAnalatitude = node.GetFieldValue("SmY");

           }
           catch (System.Exception e)
           {
               Trace.WriteLine(e.Message);
           }
           finally
           {
               if (recordset != null)
               {
                   recordset.Close();
                   recordset.Dispose();
               }
           }
       }
       /// <summary>
       /// 开关阀分析---清除对象
       /// </summary>
       public void ControlClear()
       {
           try
           {
               mLayerNetLine.Selection.Clear();
               mLayerNetNode.Selection.Clear();
               mLayerNetLine.Selection.Remove(mControlID);
               mSceneControl.Bubbles.Clear();
               mSceneControl.Scene.TrackingLayer.Clear();
               Layer3Ds m_layer = mSceneControl.Scene.Layers;
               foreach (Layer3D layer in m_layer)
               {
                   if (layer.Selection != null)
                   {
                       layer.Selection.Clear();
                   }
               }          
               mControlObjectFlag = false;
               mInfluencePipe = 0;
               mAnalatitude = null;
               mAnalongitude = null;
               mPipeNet = null;
               mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedControlAnalysis);
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }

       }
       /// <summary>
       ///  开关阀分析---飞到控制点
       /// </summary>
       public void FlyToControlPipe(string PipeType)
       {
           try
           {
               Recordset selected = null;
               selected = mPipeNet.Query("SMID = " + mInfluencePipe, CursorType.Static);
               Geometry3D geometry = selected.GetGeometry() as GeoLine3D;

               mSceneControl.Bubbles.Clear();
               Bubble bubble = new Bubble();
               mInformationBubble.Visible = true;

               mInformationBubble.Description.Text = "阀门ID:  " + mInfluencePipe + "\n经度: " + mAnalongitude + "\n纬度: " + mAnalatitude + "\n所属类型: " + PipeType;
               bubble.Pointer = new Point3D(Convert.ToDouble(mAnalongitude), Convert.ToDouble(mAnalatitude), mControlDeepth);
               bubble.ClientWidth = mInformationBubble.Width;
               bubble.ClientHeight = mInformationBubble.Height;

               mInformationBubble.Location = new Point(bubble.ClientLeft, bubble.ClientTop);
               mSceneControl.Bubbles.Add(bubble);
               mSceneControl.Scene.Refresh();


               Camera camera = new Camera();
               camera.Longitude = Convert.ToDouble(mAnalongitude);
               camera.Latitude = Convert.ToDouble(mAnalatitude);
               camera.AltitudeMode = AltitudeMode.RelativeToGround;
               camera.Tilt = 0;
               camera.Altitude = 5;
               mSceneControl.Scene.Fly(camera, 10);

               mSceneControl.Action = Action3D.Pan2;
           }
           catch (Exception ex)
           {

               Trace.WriteLine(ex);
           }

       }


       #endregion







    }

}

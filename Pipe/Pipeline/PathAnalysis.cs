using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using SuperMap.Data;
using SuperMap.Realspace;
using SuperMap.UI;
using SuperMap.Analyst.NetworkAnalyst;
using System.IO;
using System.Windows.Forms;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： PathAnalysis
    ** 描述： 路径分析
    ** 作者： GJF
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    /// <summary>
    /// 路径分析类
    /// </summary>
    public class PathAnalysis
    {
        #region 私有字段
        private Workspace mWorkspace;
        private SceneControl mSceneControl;
        private DatasetVector mRodeNetWork;
        private UseData mUseData;
        // 用于存储分析所用的网络数据集
        private TransportationAnalyst mAnalyst;
        private TransportationAnalystResult mAnalystResult;
        private TransportationAnalystParameter mAnalystParameter;
        // 用于存储分析设置的起始以及起始点在跟踪层上的标签
        private String mStartPointTag;
        private GeoPoint3D mStartPoint;
        private String mEndPointTag;
        private GeoPoint3D mEndPoint;
        // 用于存储分析结果以及分析结果在跟踪层上的标签
        private GeoLine mGeoLine;
        private String mGeoLineTag;
        // 用于存储起始地标在跟踪层上的标签
        private String mMarkStart;
        private String mMarkEnd;
        // 用于记录分析结果的线段
        String mStrTrackingTagNetworkResult = "resultLine";
        //用于保存飞行路线
        public GeoLine3D mLine3D;
        private Point mClickPoint;
        // 用于存储添加到三维场景中的网络数据集图层
        String mStrLayer3DNodeNetwork = "RodeNetWork2D_Node@guilinligong";
        String mStrLayer3DLineNetwork = "RodeNetWork2D@guilinligong";
        private int StartOrEnd;
        //存储道路名称
        string mTextBoxRodeName = null;
        public string TextBoxRodeName
        { get { return mTextBoxRodeName; } set { mTextBoxRodeName = value; } }
        //存储道路距离
        int mRodeTotalLength = 0;
        public int RodeTotalLength
        {
            get { return mRodeTotalLength; }
            set { mRodeTotalLength = value; }
        }
        #endregion

        public PathAnalysis(Workspace workspace, SceneControl sceneControl, UseData usedata)
        {

            mWorkspace = workspace;
            mSceneControl = sceneControl;
            mSceneControl.Scene.Workspace = workspace;
            mUseData = usedata;
            Initialize();


        }
        private void Initialize()
        {
            mAnalyst = new TransportationAnalyst();
            mAnalystParameter = new TransportationAnalystParameter();

            mStartPointTag = String.Empty;
            mStartPoint = new GeoPoint3D();

            mEndPointTag = String.Empty;
            mEndPoint = new GeoPoint3D();

            mStartPointTag = String.Empty;
            mStartPoint = new GeoPoint3D();

            mGeoLineTag = String.Empty;
            mGeoLine = new GeoLine();

            mSceneControl.Action = Action3D.Pan;
            mSceneControl.Scene.Refresh();
        }

        public void SetPoint(int flag)
        {
            try
            {
                StartOrEnd = flag;
                if (mLine3D != null && mLine3D.PartCount > 0)
                {
                    mLine3D.SetEmpty();
                }
                else if (mLine3D == null)
                {
                    mLine3D = new GeoLine3D();
                }
                mSceneControl.Action = Action3D.Select;
                mSceneControl.MouseDoubleClick -= PathAnalysisMouseClick;
                mSceneControl.MouseDoubleClick += PathAnalysisMouseClick;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void PathAnalysisMouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                mRodeNetWork = mUseData.RodeNetwork;
                mClickPoint.X = e.X;
                mClickPoint.Y = e.Y;
                if (e.Button == MouseButtons.Left && mSceneControl.Action == SuperMap.UI.Action3D.Select)
                {
                    Point3D clickPoint3D = mSceneControl.Scene.PixelToGlobe(mClickPoint, SuperMap.Realspace.PixelToGlobeMode.TerrainAndModel);
                    Point3D antualPoint3D = Point3D.Empty;
                    if (StartOrEnd == 1)
                    {
                        if (clickPoint3D != Point3D.Empty && CheckPoint(clickPoint3D))
                        {
                            SetStartPoint(clickPoint3D);
                            DisplayStartPoint();
                        }
                    }
                    else if (StartOrEnd == 2)
                    {
                        if (clickPoint3D != Point3D.Empty && CheckPoint(clickPoint3D))
                        {
                            SetEndPoint(clickPoint3D);
                            DisplayEndPoint();
                        }
                    }
                }
                mSceneControl.Action = Action3D.Pan;
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
                mStartPoint.Z = 1;
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
        /// <summary>
        /// 设置终止点
        /// </summary>
        public void SetEndPoint(Point3D point3D)
        {
            try
            {
                ClearEndPoint();
                mSceneControl.Scene.Refresh();
                mEndPoint = new GeoPoint3D(point3D);
                mEndPoint.Z = 1;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        private void ClearEndPoint()
        {
            try
            {
                mEndPoint.X = 0;
                mEndPoint.Y = 0;
                mEndPoint.Z = 0;

                Int32 index = mSceneControl.Scene.TrackingLayer.IndexOf(mEndPointTag);
                if (index >= 0)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    mSceneControl.Scene.Refresh();
                }

                mEndPointTag = String.Empty;

                index = mSceneControl.Scene.TrackingLayer.IndexOf(mMarkEnd);
                if (index >= 0)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    mSceneControl.Scene.Refresh();
                }

                mMarkEnd = String.Empty;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 在三维场景中显示终止点
        /// </summary>
        public void DisplayEndPoint()
        {
            try
            {
                GeoStyle3D pointStyle = new GeoStyle3D();
                pointStyle.MarkerColor = Color.FromArgb(255, 51, 255, 0);
                pointStyle.MarkerSize = 10.0;
                pointStyle.AltitudeMode = AltitudeMode.Absolute;

                mEndPoint.Style3D = pointStyle;

                mEndPointTag = "endPoint";
                mSceneControl.Scene.TrackingLayer.Add(mEndPoint, mEndPointTag);

                mMarkEnd = "PlaceMarkEnd";
                GeoPlacemark markEnd = new GeoPlacemark("终止点", mEndPoint);
                markEnd.NameStyle.ForeColor = Color.FromArgb(255, 51, 255, 0);
                if (System.Environment.Is64BitProcess)
                {
                    markEnd.Style3D.MarkerFile = @"..\..\..\Resource\redpin";
                }
                else
                {
                    markEnd.Style3D.MarkerFile = @"..\..\Resource\redpin";
                }
                mSceneControl.Scene.TrackingLayer.Add(markEnd, mMarkEnd);

                mSceneControl.Scene.Refresh();
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
        /// 设置网络分析参数
        /// </summary>
        public void SetParameter()
        {
            try
            {
                Point2D startPoint = new Point2D(mStartPoint.X, mStartPoint.Y);
                Point2D endPoint = new Point2D(mEndPoint.X, mEndPoint.Y);
                Point2Ds points = new Point2Ds(new Point2D[] { startPoint, endPoint });

                mAnalystParameter.IsRoutesReturn = true;
                mAnalystParameter.WeightName = "Smlength";
                mAnalystParameter.IsEdgesReturn = true;

                mAnalystParameter.Points = points;
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
                if (mAnalystResult!=null)
                {
                    int[][] EdgeId = new int[mAnalystResult.Edges.Length][];
                    //拿到交错数组中第一行元素ID 
                    int[] IDedge = new int[mAnalystResult.Edges[0].Length];
                    for (int i = 0; i < IDedge.Length; i++)
                    {
                        IDedge[i] = mAnalystResult.Edges[0][i];
                    }
                    //拿到弧段ID后查询路名
                    string[] RodeName = new string[IDedge.Length];
                    Recordset recordset = null;
                    for (int i = 0; i < IDedge.Length; i++)
                    {
                        recordset = mRodeNetWork.Query("SmID =" + IDedge[i], CursorType.Static);
                        RodeName[i] = recordset.GetFieldValue("RodeName").ToString().Trim();
                        mRodeTotalLength += Convert.ToInt32(recordset.GetFieldValue("SmLength"));
                    }
                    //编辑路名显示形式
                    string mStartRode = "起点→";
                    string mNewRode = null;
                    for (int i = 0; i < RodeName.Length; i++)
                    {
                        if (i == 0)
                        {
                            mNewRode = mStartRode.Insert(mStartRode.Length, RodeName[i] + "→");

                        }
                        else
                        {

                            mNewRode = mNewRode.Insert(mNewRode.Length, RodeName[i] + "→");
                        }
                    }
                    mTextBoxRodeName = mNewRode + "终点";
                }
                GeoLine line = mAnalystResult.Routes[0].ConvertToLine();
                if (mLine3D == null)
                {
                    mLine3D = new GeoLine3D();
                }

                for (Int32 i = 0; i < line.PartCount; i++)
                {
                    mLine3D.AddPart(line[i].ToPoint3Ds());
                }
                mLine3D[0].Insert(0, new Point3D(mStartPoint.X, mStartPoint.Y, 0));
                mLine3D[0].Add(new Point3D(mEndPoint.X, mEndPoint.Y, 0));
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


        public void ClearPath()
        {
            try
            {
                if ( mLine3D != null)
                {
                    mLine3D = null;
                }
                mEndPoint.X = 0;
                mEndPoint.Y = 0;
                mEndPoint.Z = 0;
                mStartPoint.X = 0;
                mStartPoint.Y = 0;
                mStartPoint.Z = 0;
                if (RodeTotalLength!=0)
                {
                    RodeTotalLength = 0;
                }
                if (mTextBoxRodeName!=null)
                {
                    mTextBoxRodeName = null;
                }
                mSceneControl.MouseDoubleClick -= PathAnalysisMouseClick;
                mSceneControl.Scene.TrackingLayer.Clear();
               
            }
            catch (Exception ex)
            {
                
                Trace.WriteLine(ex);
            }

        }
    }
}

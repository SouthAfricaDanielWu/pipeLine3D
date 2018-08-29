using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SuperMap.UI;
using SuperMap.Realspace;
using SuperMap.Data;

namespace NorthStar.Pipeline
{
    public class MeasureAnalysis
    {
        #region 定义变量
        private FrmMain mFormMain;
        private SceneControl mSceneControl;
        private Label mLabel;


        //定义存储字符串的变量

        private Point3Ds mPoint3Ds;
        private Double mCurLength = 0.0;
        private Double mCurAltitude = 0.0;
        private Double mCurArea = 0.0;
        private GeoText3D mCurrentGeoText3DMessage;
        private String mStrResult;
        private Point3D mTempPoint;
        private int mIndex;

        private static String MeasureDistanceTagTemp = "MeasureDistanceTemp";
        private static String MeasureAreaTagTemp = "MeasureAreaTemp";
        private static String MessageTag = "MeasureDistancePart";
        private static String MessageTrackingTag = "MeasureDistanceTracking";
        private static String MeasureDistanceTag = "MeasureDistance";
        private static String MeasureAreaTag = "MeasureArea";
        private static String MeasureAltitudeTag = "MeasureAltitude";
        private readonly String mSquareMeter = "平方米";
        private readonly String mArea = "量测面积：";
        private GeoStyle3D mGeoStyle3DTemp;
        private GeoStyle3D mGeoStyle3D;

        #endregion

        public MeasureAnalysis(SceneControl sceneControl,FrmMain formMain,int flag)
        {
            try
            {
                mSceneControl = sceneControl;
                mFormMain = formMain;
                mIndex = flag;
                mLabel = new Label();
                mLabel.AutoSize = true;
                mLabel.BackColor = Color.White;
                mLabel.Visible = false;
                mSceneControl.Parent.Controls.Add(mLabel);
                mSceneControl.Parent.Controls.SetChildIndex(mLabel, 0);
                Initialize();               
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }   
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            try 
            {
                mSceneControl.Action = Action3D.Pan;

                //注册事件
                mSceneControl.Tracking += new Tracking3DEventHandler(TrackingHandler);
                mSceneControl.Tracked += new Tracked3DEventHandler(TrackedHandler);
                mSceneControl.MouseUp += SceneControlMouseUp;


                mPoint3Ds = new Point3Ds();
                mGeoStyle3D = new GeoStyle3D();

                mGeoStyle3D.MarkerColor = Color.FromArgb(255, 0, 255);
                mGeoStyle3D.LineColor = Color.FromArgb(255, 255, 0);
                mGeoStyle3D.LineWidth = 2;
                mGeoStyle3D.FillForeColor = Color.FromArgb(180, Color.Violet);
                mGeoStyle3D.AltitudeMode = AltitudeMode.RelativeToUnderground;
                
                //临时样式
                mGeoStyle3DTemp = new GeoStyle3D();
                mGeoStyle3DTemp.MarkerColor = Color.FromArgb(255, 0, 0);
                mGeoStyle3DTemp.LineColor = Color.FromArgb(0, 255, 0);
                mGeoStyle3DTemp.LineWidth = 2;
                mGeoStyle3DTemp.FillForeColor = Color.FromArgb(180, Color.Violet);
                mGeoStyle3DTemp.AltitudeMode = AltitudeMode.RelativeToUnderground;

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

        }

        #region 量测模块

        /// <summary>
        /// 创建三维文本信息
        /// </summary>
        /// <returns></returns>
        private GeoText3D CreateText3DMessage()
        {
            TextPart3D textPart3D = new TextPart3D();
            textPart3D.AnchorPoint = new Point3D(0, 0, 0);
            textPart3D.Text = String.Empty;

            TextStyle style = new TextStyle();
            style.ForeColor = Color.White;
            style.IsSizeFixed = true;
            style.FontHeight = 4;
            GeoText3D text3D = new GeoText3D(textPart3D, style);
            text3D.Style3D = new GeoStyle3D();
            text3D.Style3D.AltitudeMode = AltitudeMode.RelativeToUnderground;
            return text3D;
        }




        /// <summary>
        /// 结束测量
        /// </summary>
        private void EndOneMeasure()
        {
            try
            {
                mCurLength = 0.0;
                mCurAltitude = 0.0;
                mCurArea = 0.0;
                mIndex = 0;
                mPoint3Ds.Clear();
                mTempPoint = Point3D.Empty;
                mStrResult = String.Empty;
                mSceneControl.Action = Action3D.Pan2;
                if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
                {
                    mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = false;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// 鼠标动作Up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SceneControlMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                EndOneMeasure();
            }

            if (e.Button == MouseButtons.Left)
            {
                if (mPoint3Ds.Count == 0)
                {
                    //ClearMeasureResult();
                }

                if (mTempPoint != Point3D.Empty && mPoint3Ds.Count > 1)
                {
                    mPoint3Ds.Remove(mPoint3Ds.Count - 1);
                    mTempPoint = Point3D.Empty;
                }

                Point location = mSceneControl.PointToClient(Cursor.Position);
                Point3D point3D = new Point3D();

                if (mSceneControl.Action == Action3D.MeasureDistance&&mIndex==8
                    || mSceneControl.Action == Action3D.MeasureAltitude &&mIndex==9
                    || mSceneControl.Action == Action3D.MeasureHorizontalDistance)
                {
                    point3D = mSceneControl.Scene.PixelToGlobe(new Point(e.X, e.Y), SuperMap.Realspace.PixelToGlobeMode.TerrainAndModel);
                }
                else
                {
                    point3D = mSceneControl.Scene.PixelToGlobe(new Point(e.X, e.Y));
                }

                if (!Double.IsNaN(point3D.X) && !Double.IsNaN(point3D.Y) && !Double.IsNaN(point3D.Z))
                {
                    mPoint3Ds.Add(point3D);


                    if (mSceneControl.Action == Action3D.MeasureHorizontalDistance 
                        || mSceneControl.Action == Action3D.MeasureDistance&&mIndex==8)
                    {
                        //添加部分段长度
                        if (!Toolkit.IsZero(mCurLength))
                        {
                            if (mPoint3Ds.Count >= 2)
                            {
                                mPoint3Ds.RemoveRange(0, mPoint3Ds.Count - 2);
                                String distanceUnit = "米";
                                GeoLine3D geoLine3D = new GeoLine3D(mPoint3Ds);
                                TextPart3D textPart3D = new TextPart3D();
                                textPart3D.AnchorPoint = new Point3D(0, 0, 0);
                                Double tempCurLength = mCurLength;
                                textPart3D.Text = String.Format("{0:F2}{1}", tempCurLength, " " + distanceUnit);
                                if (mPoint3Ds[0].X > 0 && mPoint3Ds[1].X > 0 ||
                                    mPoint3Ds[0].X < 0 && mPoint3Ds[1].X < 0)
                                {
                                    textPart3D.X = geoLine3D.InnerPoint3D.X;
                                }
                                else
                                {
                                    textPart3D.X = 180;
                                }

                                textPart3D.Y = geoLine3D.InnerPoint3D.Y;
                                textPart3D.Z = geoLine3D.InnerPoint3D.Z;

                                GeoText3D text3d = mCurrentGeoText3DMessage == null ? GetGeoText3DMessage() : mCurrentGeoText3DMessage;
                                text3d.AddPart(textPart3D);
                               
                                //设置文字样式
                                text3d.TextStyle.FontHeight = 6;
                                text3d.TextStyle.Alignment = TextAlignment.BottomLeft;
                                text3d.TextStyle.BackColor = Color.Black;
                                text3d.TextStyle.Outline = true;
                                text3d.Style3D.AltitudeMode = AltitudeMode.Absolute;

                                int index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTag);
                                if (index >= 0)
                                {
                                    mSceneControl.Scene.TrackingLayer.Remove(index);
                                }
                                //ClearTextMessageTag();
                                mSceneControl.Scene.TrackingLayer.Add(text3d, MessageTag);
                            }
                        }

                        //测地形距离
                        else if (mSceneControl.Action == Action3D.MeasureTerrainDistance)
                        {
                            //添加部分段长度
                            if (!Toolkit.IsZero(mCurLength))
                            {
                                if (mPoint3Ds.Count >= 2)
                                {
                                    mPoint3Ds.RemoveRange(0, mPoint3Ds.Count - 2);

                                    GeoLine3D geoLine3D = new GeoLine3D(mPoint3Ds);
                                    String distanceUnit = "米";
                                    TextPart3D textPart3D = new TextPart3D();
                                    textPart3D.AnchorPoint = new Point3D(0, 0, 0);
                                    Double tempCurLength = mCurLength;
                                    textPart3D.Text = String.Format("{0:F2}{1}", tempCurLength, " " + distanceUnit);
                                    if (mPoint3Ds[0].X > 0 && mPoint3Ds[1].X > 0 ||
                                        mPoint3Ds[0].X < 0 && mPoint3Ds[1].X < 0)
                                    {
                                        textPart3D.X = geoLine3D.InnerPoint.X;
                                        textPart3D.Y = geoLine3D.InnerPoint.Y;
                                    }
                                    else
                                    {
                                        textPart3D.X = 180;
                                        textPart3D.Y = geoLine3D.InnerPoint.Y;
                                    }
                                    textPart3D.Z = 0;
                                    GeoText3D text3d = GetGeoText3DMessage();
                                    text3d.AddPart(textPart3D);
                                    text3d.TextStyle.FontHeight = 6;
                                    text3d.TextStyle.Alignment = TextAlignment.BottomLeft;
                                    text3d.TextStyle.BackColor = Color.Black;
                                    text3d.TextStyle.Outline = true;
                                    text3d.Style3D.AltitudeMode = AltitudeMode.ClampToGround;//贴地高程模式
                                    ClearTextMessageTag();
                                    mSceneControl.Scene.TrackingLayer.Add(text3d, MessageTag);
                                }
                            }
                        }
                        else if (mSceneControl.Action == Action3D.MeasureArea&&mIndex==10)
                        {
                            
                        }
                        else if (mSceneControl.Action == Action3D.MeasureAltitude&&mIndex==9)
                        {
                            //清除量算高度信息
                            if (!(Toolkit.IsZero(mCurAltitude)))
                            {
                                EndOneMeasure();
                            }
                        }

                    }
                }
            }
        }
        /// <summary>
        /// 清除测量结果
        /// </summary>
        private void ClearMeasureResult()
        {
            try
            {
                int index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureAreaTag);
                while (index != -1)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureAreaTag);
                }
                index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureAreaTagTemp);
                while (index != -1)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureAreaTagTemp);
                }
                index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureDistanceTag);
                while (index != -1)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureDistanceTag);
                }
                index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureDistanceTagTemp);
                while (index != -1)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureDistanceTagTemp);
                }
                index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureAltitudeTag);
                while (index != -1)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureAltitudeTag);
                }
                index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureAreaTagTemp);
                while (index != -1)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    index = mSceneControl.Scene.TrackingLayer.IndexOf(MeasureAreaTagTemp);
                }

                index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTag);
                while (index >= 0)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                    index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTag);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        /// <summary>
        /// 清除文本
        /// </summary>
        private void ClearTextMessageTag()
        {
            int index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTag);
            while (index >= 0)
            {
                mSceneControl.Scene.TrackingLayer.Remove(index);
                index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTag);
            }
            mCurrentGeoText3DMessage = null;
        }

        private GeoText3D GetGeoText3DMessage()
        {
            GeoText3D text = CreateText3DMessage();
            mSceneControl.Scene.TrackingLayer.Add(text, MessageTag);
            mCurrentGeoText3DMessage = text;
            return text;
        }
        /// <summary>
        ///设置三维样式
        /// </summary>
        /// <param name="geometry"></param>
        private void SetGeometry3DStyle(Geometry3D geometry)
        {
            try
            {
                GeoStyle3D style = new GeoStyle3D();

                if (mSceneControl.Action == Action3D.MeasureAltitude 
                    || mSceneControl.Action == Action3D.MeasureDistance 
                    || mSceneControl.Action == Action3D.MeasureHorizontalDistance)
                {
                    style.AltitudeMode = AltitudeMode.RelativeToUnderground;
                }
                else
                {
                    style.AltitudeMode = AltitudeMode.ClampToGround;
                }

                style.MarkerSize = 4;
                style.MarkerColor = Color.FromArgb(255, 0, 255);

                //设置线样式
                style.LineColor = Color.Yellow;
                style.LineWidth = 2;
                style.FillMode = FillMode3D.LineAndFill;
                style.FillForeColor = Color.LightSeaGreen;
                
                geometry.Style3D = style;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 设置结果文本样式
        /// </summary>
        /// <param name="text"></param>
        private void SetResultTextStyle(GeoText3D text)
        {
            try
            {
                TextStyle textStyle = new TextStyle();
                textStyle.ForeColor = Color.White;
                textStyle.Outline = true;
                textStyle.BackColor = Color.Black;
                textStyle.FontHeight = 10;
                text.TextStyle = textStyle;
                GeoStyle3D style = new GeoStyle3D();
            
                style.AltitudeMode = AltitudeMode.RelativeToUnderground;

                text.Style3D = style;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 事件跟踪
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrackingHandler(object sender, Tracking3DEventArgs e)
        {
            try
            {
                OutputMeasureResult(e);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TrackedHandler(object sender, Tracked3DEventArgs e)
        {
            try
            {
                // 清空临时结果
                mPoint3Ds.Clear();
                mCurLength = 0.0;

                Geometry3D geometry = e.Geometry;
                Point3D textLocation = new Point3D(0, 0, 0);

                String text = String.Empty;
                if (mSceneControl.Action == Action3D.MeasureDistance &&mIndex==8
                    || mSceneControl.Action == Action3D.MeasureTerrainDistance)
                {
                    //绘制量算线对象
                    GeoLine3D geoLine3D = e.Geometry.Clone() as GeoLine3D;
                    geoLine3D.Style3D = mGeoStyle3D.Clone();
                    if (mSceneControl.Action == Action3D.MeasureDistance && mIndex == 8)
                    {
                        geoLine3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                    }
                    else
                    {
                        geoLine3D.Style3D.AltitudeMode = AltitudeMode.ClampToGround;
                    }
                    mSceneControl.Scene.TrackingLayer.Add(geoLine3D, MeasureDistanceTag);

                    //绘制量算点对象
                    Point3D point3D = Point3D.Empty;
                    for (Int32 i = 0; i < geoLine3D.PartCount; i++)
                    {
                        for (Int32 j = 0; j < geoLine3D[i].Count; j++)
                        {
                            GeoPoint3D geoPoint3D = new GeoPoint3D(geoLine3D[i][j]);
                            geoPoint3D.Style3D = mGeoStyle3D.Clone();
                            if (mSceneControl.Action == Action3D.MeasureDistance && mIndex == 8)
                            {
                                geoPoint3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                            }
                            else if (mSceneControl.Action == Action3D.MeasureTerrainDistance)
                            {
                                geoPoint3D.Style3D.AltitudeMode = AltitudeMode.ClampToGround;
                            }
                            mSceneControl.Scene.TrackingLayer.Add(geoPoint3D, MeasureDistanceTag);
                            point3D = geoLine3D[i][j];
                        }
                    }

                    //量算结果
                    int index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTrackingTag);
                    if (index >= 0)
                    {
                        mSceneControl.Scene.TrackingLayer.Remove(index);
                    }

                    // 添加结果文字
                    if (mSceneControl.Action == Action3D.MeasureDistance && mIndex == 8)
                    {
                        text = String.Format("{0}{1}{2}", "空间距离：", Math.Round(Convert.ToDecimal(e.Length), 2), "米");
                    }
                    else
                    {
                        text = String.Format("{0}{1}{2}", "依地距离：", Math.Round(Convert.ToDecimal(e.Length), 2), "米");
                    }

                    textLocation = geoLine3D[0][geoLine3D[0].Count - 1];
                    GeoText3D geoText = new GeoText3D(new TextPart3D(text, textLocation));
                    SetResultTextStyle(geoText);
                    mSceneControl.Scene.TrackingLayer.Add(geoText, "MeasureDistance");

                    //计算首尾点高度差
                    Point3Ds point3Ds = geoLine3D[0];
                    double height = point3Ds[point3Ds.Count - 1].Z - point3Ds[0].Z;
                    String message = string.Format("首尾点高度差为:{0}米。", height.ToString("##.00"));

                }

                else if (mSceneControl.Action == Action3D.MeasureArea && mIndex == 10
                    || mSceneControl.Action == Action3D.MeasureTerrainArea )
                {
                    //绘制量算面对象
                    GeoRegion3D geoRegion3D = e.Geometry as GeoRegion3D;
                    //绘制量算面对象
                    geoRegion3D.Style3D = mGeoStyle3D.Clone();
                    if (mSceneControl.Action == Action3D.MeasureArea)
                    {
                        geoRegion3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                    }
                    else
                    {
                        geoRegion3D.Style3D.AltitudeMode = AltitudeMode.ClampToGround;
                    }
                    geoRegion3D.Style3D.FillForeColor = Color.FromArgb(120, 250, 250, 50);
                    ClearTextMessageTag();
                    int index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTrackingTag);
                    if (index >= 0)
                    {
                        mSceneControl.Scene.TrackingLayer.Remove(index);
                    }
                       
                    if(mIndex==10)
                        mSceneControl.Scene.TrackingLayer.Add(geoRegion3D, "geoRegion3D");
       

                    //绘制量算点对象
                    for (Int32 i = 0; i < geoRegion3D.PartCount; i++)
                    {
                        for (Int32 j = 0; j < geoRegion3D[i].Count; j++)
                        {
                            GeoPoint3D geoPoint3D = new GeoPoint3D(geoRegion3D[i][j]);
                            geoPoint3D.Style3D = mGeoStyle3D.Clone();
                            if (mSceneControl.Action == Action3D.MeasureArea && mIndex == 10)
                            {
                                geoPoint3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                            }
                            else if (mSceneControl.Action == Action3D.MeasureTerrainArea)
                            {
                                geoPoint3D.Style3D.AltitudeMode = AltitudeMode.ClampToGround;
                            }
                            mSceneControl.Scene.TrackingLayer.Add(geoPoint3D, MeasureAreaTag + i.ToString() + j.ToString());
                        }
                    }

                    ClearTextMessageTag();

                    //量算结果
                    if (mSceneControl.Action == Action3D.MeasureArea && mIndex == 10)
                    {
                        mStrResult = String.Format("{0}{1}{2}", "空间面积：", Math.Round(Convert.ToDecimal(e.Area), 2), "平方米");
                    }
                    else
                    {
                        mStrResult = String.Format("{0}{1}{2}", "依地面积：", Math.Round(Convert.ToDecimal(e.Area), 2), "平方米");
                    }
                    GeoText3D text3d = GetGeoText3DMessage();
                    text3d[0].Text = mStrResult;
                    text3d[0].X = geoRegion3D.InnerPoint3D.X;
                    text3d[0].Y = geoRegion3D.InnerPoint3D.Y;
                    text3d[0].Z = geoRegion3D.InnerPoint3D.Z;

                    GeoText3D geoText = new GeoText3D(text3d);
                   
                    // 添加结果文字
                    SetResultTextStyle(geoText);
                    mSceneControl.Scene.TrackingLayer.Add(geoText, "MeasureArea");

                }

                else if (mSceneControl.Action == Action3D.MeasureAltitude && mIndex == 9)
                {
                    //绘制量算线对象
                    GeoLine3D geoLine3D = e.Geometry as GeoLine3D;
                    geoLine3D.Style3D = mGeoStyle3D.Clone();
                    geoLine3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                    mSceneControl.Scene.TrackingLayer.Add(geoLine3D, "Altitude");
                    // 添加结果文字
                    text = String.Format("{0}{1}{2}", "高度：", Math.Round(Convert.ToDecimal(e.Height), 2), "米");
                    textLocation = geoLine3D[0][geoLine3D[0].Count - 1];
                    GeoText3D geoText = new GeoText3D(new TextPart3D(text, textLocation));
                    SetResultTextStyle(geoText);
                    mSceneControl.Scene.TrackingLayer.Add(geoText, "Altitude");

                    //输出首尾点高度
                    Point3Ds point3Ds = geoLine3D[0];
                    double startHeight = point3Ds[0].Z;
                    double endHeight = point3Ds[1].Z;
                    string message = string.Format("首点高度为:{0}米。", startHeight.ToString("##.00"));

                    //bd_formMain.ClearOutputMessage();
                    //bd_formMain.OutputMessage(message);
                    message = string.Format("尾点高度为:{0}米。", endHeight.ToString("##.00"));
                    //bd_formMain.OutputMessage(message);

                    //绘制量算点对象
                    for (Int32 i = 0; i < geoLine3D.PartCount; i++)
                    {
                        for (Int32 j = 0; j < geoLine3D[i].Count; j++)
                        {
                            GeoPoint3D geoPoint3D = new GeoPoint3D(geoLine3D[i][j]);
                            geoPoint3D.Style3D = mGeoStyle3D.Clone();
                            geoPoint3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                            mSceneControl.Scene.TrackingLayer.Add(geoPoint3D, MeasureAltitudeTag + j.ToString());
                        }
                    }
                    ClearTextMessageTag();
                    if (!(Toolkit.IsZero(mCurAltitude)))
                    {
                        EndOneMeasure();
                    }
                }

                else if (mSceneControl.Action == Action3D.MeasureHorizontalDistance)
                {
                    // 结果线
                    GeoLine3D resLine3D = e.Geometry as GeoLine3D;
                    resLine3D.Style3D = mGeoStyle3D.Clone();
                    resLine3D.Style3D.AltitudeMode = AltitudeMode.Absolute;


                    // 结果点
                    Point3Ds resPoints = resLine3D[0];
                    for (Int32 i = 0; i < resPoints.Count; i++)
                    {
                        GeoPoint3D geoPoint = new GeoPoint3D(resPoints[i]);
                        SetGeometry3DStyle(geoPoint);
                        mSceneControl.Scene.TrackingLayer.Add(geoPoint, "Geometry" + i.ToString());
                    }

                    EndOneMeasure();
                    mSceneControl.Scene.TrackingLayer.Add(resLine3D, "Geometry");

                    // 添加结果文字
                    text = String.Format("{0}{1}{2}", "总长度： ", Math.Round(Convert.ToDecimal(e.Length), 2), "米");
                    GeoLine3D line = (geometry as GeoLine3D);
                    textLocation = line[0][line[0].Count - 2];

                    //ClearTextMessageTag();
                    int index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTrackingTag);
                    if (index >= 0)
                    {
                        mSceneControl.Scene.TrackingLayer.Remove(index);
                    }
                    GeoText3D geoText = new GeoText3D(new TextPart3D(text, textLocation));
                    SetResultTextStyle(geoText);

                    mSceneControl.Scene.TrackingLayer.Add(geoText, "MeasureResult");

                    //bd_formMain.ClearOutputMessage();
                    //bd_formMain.OutputMessage(text);

                    //计算首尾点高度差
                    Point3Ds point3Ds = resLine3D[0];
                    double height = point3Ds[point3Ds.Count - 1].Z - point3Ds[0].Z;
                    string message = string.Format("首尾点高度差为:{0}米。", height.ToString("##.00"));
                    //bd_formMain.OutputMessage(message);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                mLabel.Visible = false;
            }
        }

        public void BeginMeasureDistance()
        {
            mSceneControl.Action = Action3D.MeasureDistance;
        }

        /// <summary>
        /// 开始面积量算
        /// </summary>
        public void BeginMeasureArea()
        {
            mSceneControl.Action = Action3D.MeasureArea;
        }

        /// <summary>
        /// 开始高度量算
        /// </summary>
        public void BeginMeasureAltitude()
        {
            mSceneControl.Action = Action3D.MeasureAltitude;
        }

        /// <summary>
        /// 清空量算结果
        /// </summary>
        public void ClearResult()
        {
            try
            {
                mSceneControl.Scene.TrackingLayer.Clear();              
                mSceneControl.Action = Action3D.Pan2;
                mCurrentGeoText3DMessage = null;
                mIndex = 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 平移
        /// </summary>
        public void PanAction()
        {
            mSceneControl.Action = Action3D.Pan;
        }

        private void OutputMeasureResult(SuperMap.UI.Tracking3DEventArgs e)
        {
            if (mSceneControl.Action == Action3D.MeasureDistance &&mIndex==8
                || mSceneControl.Action == Action3D.MeasureTerrainDistance
                || mSceneControl.Action == Action3D.MeasureHorizontalDistance)
            {
                if (e.CurrentLength > 0)
                {
                    mCurLength = e.CurrentLength;
                    Double tempCurLength = mCurLength;
                    Double tempTotalLength = e.TotalLength;
                    mStrResult = String.Format("    ", tempCurLength, " " + "米", tempTotalLength, " " + "米");
                    OutputMeasureDistance(e);
                }
            }
            if (mSceneControl.Action == Action3D.MeasureArea&&mIndex==10
                || mSceneControl.Action == Action3D.MeasureTerrainArea)
            {
                Double area = e.TotalArea;
                mStrResult = String.Format("", area, " " + "米");

                if (!Toolkit.IsZero(e.TotalArea))
                {
                    OutputMeasureArea(e);
                }
            }
            if (mSceneControl.Action == Action3D.MeasureAltitude&&mIndex==9)
            {
                Double currentHeight = e.CurrentHeight;
                mStrResult = String.Format("    ", currentHeight, " " + "米");
                OutputMeasureAltitude(e);
            }
        }

        private void OutputMeasureDistance(SuperMap.UI.Tracking3DEventArgs e)
        {
            try
            {
                Point location = mSceneControl.PointToClient(Cursor.Position);

                if (mTempPoint != Point3D.Empty && mPoint3Ds.Count > 1)
                {
                    mPoint3Ds.Remove(mPoint3Ds.Count - 1);
                }
                mTempPoint = new Point3D(e.X, e.Y, e.Z);
                mPoint3Ds.Add(mTempPoint);

                location.Offset(30, 30);
                if (location.X > mSceneControl.Bounds.Width / 3 * 2)
                {
                    location.X = mSceneControl.Bounds.Width / 3 * 2;
                }
                if (location.Y > mSceneControl.Bounds.Height)
                {
                    location.Y = location.Y - 60;
                }

                TextPart3D textPart3D = new TextPart3D();
                textPart3D.AnchorPoint = new Point3D(0, 0, 0);
                textPart3D.Text = String.Empty;

                TextStyle style = new TextStyle();
                style.ForeColor = Color.White;
                style.IsSizeFixed = true;
                style.FontHeight = 6;
                style.Alignment = TextAlignment.BottomLeft;
                style.BackColor = Color.Black;
                style.Outline = true;
                GeoText3D text3d = new GeoText3D(textPart3D, style);
                text3d.Style3D = new GeoStyle3D();
                text3d.Style3D.AltitudeMode = AltitudeMode.Absolute;

                text3d[0].Text = e.CurrentLength.ToString("##.00") + "米";
                Point3D lastPoint = Point3D.Empty;
                if (e.Geometry != null)
                {
                    Point3Ds points = (e.Geometry as GeoLine3D)[0];
                    lastPoint = points[points.Count - 1];
                }
                else
                {
                    lastPoint = mPoint3Ds[0];
                }
                text3d[0].X = (lastPoint.X + e.X) / 2;
                text3d[0].Y = (lastPoint.Y + e.Y) / 2;

                int index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTrackingTag);
                if (index >= 0)
                {
                    mSceneControl.Scene.TrackingLayer.Remove(index);
                }
                mSceneControl.Scene.TrackingLayer.Add(text3d, MessageTrackingTag);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        /// <summary>
        /// 测量面积
        /// </summary>
        /// <param name="e"></param>
        private void OutputMeasureArea(SuperMap.UI.Tracking3DEventArgs e)
        {
            try
            {
                Point location = mSceneControl.PointToClient(Cursor.Position);

                if (mTempPoint != Point3D.Empty)
                {
                    mPoint3Ds.Remove(mPoint3Ds.Count - 1);
                }
                mTempPoint = new Point3D(e.X, e.Y, e.Z);
                mPoint3Ds.Add(mTempPoint);

                GeoRegion3D geoRegion3D = null;
                if (mPoint3Ds.Count >= 3)
                {
                    geoRegion3D = new GeoRegion3D(mPoint3Ds);
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

                    TextPart3D textPart3D = new TextPart3D();
                    textPart3D.AnchorPoint = new Point3D(0, 0, 0);
                    textPart3D.Text = String.Empty;

                    TextStyle style = new TextStyle();
                    style.ForeColor = Color.White;
                    style.IsSizeFixed = true;
                    style.FontHeight = 6;
                    style.Alignment = TextAlignment.BottomLeft;
                    style.BackColor = Color.Black;
                    style.Outline = true;
                    GeoText3D text3d = new GeoText3D(textPart3D, style);
                    text3d.Style3D = new GeoStyle3D();
                    text3d.Style3D.AltitudeMode = AltitudeMode.Absolute;

                    text3d[0].Text = e.TotalArea.ToString("##.00") + "平方米";

                    if (e.Geometry != null)
                    {
                        text3d[0].X = e.Geometry.InnerPoint.X;
                        text3d[0].Y = e.Geometry.InnerPoint.Y;
                    }
                    else
                    {
                        text3d[0].X = geoRegion3D.InnerPoint.X;
                        text3d[0].Y = geoRegion3D.InnerPoint.Y;
                    }

                    int index = mSceneControl.Scene.TrackingLayer.IndexOf(MessageTrackingTag);
                    if (index >= 0)
                    {
                        mSceneControl.Scene.TrackingLayer.Remove(index);
                    }
                    mSceneControl.Scene.TrackingLayer.Add(text3d, MessageTrackingTag);
                    mCurArea = e.TotalArea;               
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void OutputMeasureAltitude(SuperMap.UI.Tracking3DEventArgs e)
        {
            try
            {
                Point location = mSceneControl.PointToClient(Cursor.Position);
                location.Offset(30, 30);

                if (location.X > mSceneControl.Bounds.Width / 5 * 4)
                {
                    location.X = mSceneControl.Bounds.Width / 5 * 4;
                }
                if (location.Y > mSceneControl.Bounds.Height)
                {
                    location.Y = location.Y - 60;
                }

                GeoText3D text3d = GetGeoText3DMessage();

                text3d.TextStyle.FontHeight = 6;
                text3d.TextStyle.Alignment = TextAlignment.BottomLeft;
                text3d.TextStyle.BackColor = Color.Black;
                text3d.TextStyle.Outline = true;
                text3d.Style3D.AltitudeMode = AltitudeMode.RelativeToGround;

                text3d[0].Text = e.CurrentHeight.ToString("##.00") + "米";
                text3d[0].X = e.X;
                text3d[0].Y = e.Y;
                text3d[0].Z = e.Z + e.CurrentHeight;

                Console.WriteLine(text3d[0].Z);

                text3d.Style3D.AltitudeMode = AltitudeMode.Absolute;

                ClearTextMessageTag();
                mSceneControl.Scene.TrackingLayer.Add(text3d, MessageTag);
                mCurAltitude = e.CurrentHeight;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        #endregion
    }
}

 

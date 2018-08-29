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
    /********************************************************************************

    ** 类名称： PureDistance
    ** 描述： 净距分析
    ** 作者： PLWhite
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    public class PureDistance
     {

        #region 变量申明
      
        private SceneControl mSceneControl;//场景对象
        private FrmMain mFormMain;//窗体对象
        private UseData mUseData;//数据对象      
        private Point3D[] mPoint3D_0;//三维线段1中点
        private Point3D[] mPoint3D_1;//三维线段2中点
        private Point3Ds mPoint3Ds;//分析点数组
        private Point3Ds mTextPoint3Ds;//文本控制点
        private GeoLine3D mGeoLine3D;//三维线对象
        private List<GeoLine3D >TempGeoLine=new List<GeoLine3D>();//三维线对象数组

        private int mFlag;//标识
        private int mIndex;//索引
        private double mAltitude;//埋深
        private double[] mDiameter;//直径数组 
        private double[] mResult;//结果数组       
        private double mHDistance;//水平净距值
        private double mVDistance;//竖直净距值
        private double mCDistance;//碰撞距离
        private Recordset TempRecordset;//记录集对象

        private GeoStyle3D mGeoStyle3D;
        private GeoText3D mCurrentGeoText3DMessage;
        private static String MessageTag ="DistanceText";  
    
        #endregion
        /// <summary>
        /// 构造净距分析函数
        /// </summary>
        /// <param name="sceneControl"></param>
        /// <param name="formMain"></param>
        /// <param name="useData"></param>
        /// <param name="flag"></param>
        /// <param name="m_index"></param>
        public PureDistance(SceneControl sceneControl,FrmMain formMain,UseData useData,int flag,int m_index)
        {
            mSceneControl= sceneControl;
            mFormMain =formMain;
            mUseData= useData;
            mFlag = flag;
            mIndex = m_index;
            Initialize();
            if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
            {
                mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = false;
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize()
        {
            //设置鼠标抓手
            mSceneControl.Action = Action3D.Pan2;
            //清空跟踪图层
            mSceneControl.Scene.TrackingLayer.Clear();
            //注册鼠标点击/对象选择事件        
            mSceneControl.MouseDown += new MouseEventHandler(mSceneControl_MouseDown);
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlSelected);
            //设置符号样式
            mTextPoint3Ds = new Point3Ds();
            mGeoStyle3D = new GeoStyle3D();          
            mGeoStyle3D.MarkerColor = Color.FromArgb(255, 0, 255);
            mGeoStyle3D.LineColor = Color.FromArgb(255, 255, 0);
            mGeoStyle3D.LineWidth = 2;
            mGeoStyle3D.FillForeColor = Color.FromArgb(180, Color.Violet);
            mGeoStyle3D.AltitudeMode = AltitudeMode.RelativeToUnderground;
        }
        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mSceneControl_MouseDown(object sender, MouseEventArgs e)
        {            
            //右击
            if (e.Button == MouseButtons.Right)
            {
                switch (mIndex)
                {
                    case 8:
                        mFlag = 0;
                        mIndex = 0;
                        TempGeoLine.Clear();
                        mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlSelected);
                        mSceneControl.Action = Action3D.Pan2;   
                        break;
                    case 9: 
                        mFlag = 0;
                        mIndex = 0;
                        TempGeoLine.Clear();
                        mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlSelected);
                        mSceneControl.Action = Action3D.Pan2;                        
                        break;
                    case 10:
                        mFlag = 0;
                        mIndex = 0;
                        mAltitude = 0;                       
                        TempGeoLine.Clear();
                        mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlSelected);
                        mSceneControl.Action = Action3D.Pan2;
                        break;
                    case 11:
                        mFlag = 0;
                        mIndex = 0;
                        TempRecordset.Dispose();
                        TempGeoLine.Clear();
                        mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlSelected);
                        mSceneControl.Action = Action3D.Pan2;
                        break;
                }             
            }           
        }

        void mSceneControlSelected(object sender, ObjectSelectedEventArgs e)
        {
            Recordset recordset = null;
            Selection3D[] selection3d = null;
            mGeoLine3D = new GeoLine3D();
            //水平及竖直净距
            if (mSceneControl.Action == Action3D.Select && mFlag == 1 
                ||mSceneControl.Action == Action3D.Select && mFlag ==2)
            {
                if (e.Count > 0)
                {
                    selection3d = mSceneControl.Scene.FindSelection(true);
                    recordset = selection3d[0].ToRecordset();
                    if (recordset.GetGeometry().Type != mGeoLine3D.Type)
                    {
                        MessageBox.Show("请选择三维线对象!");
                    }
                    else
                    {
                        mGeoLine3D = recordset.GetGeometry() as GeoLine3D;
                        if (TempGeoLine.Count > 0 && TempGeoLine[0].Bounds == mGeoLine3D.Bounds)
                        {
                            MessageBox.Show("请不要选择同一条三维线对象!");
                        }
                        else
                        {
                            mTextPoint3Ds.Add(mGeoLine3D.InnerPoint3D);
                            TempGeoLine.Add(mGeoLine3D);
                        }
                    }
                    if (TempGeoLine.Count == 2)
                    {
                        this.SetPoint(TempGeoLine);
                    }
                }
            }
            //覆土分析
            if (mSceneControl.Action == Action3D.Select && mFlag == 3)
            {
                string str = "";
                object obj;
                selection3d = mSceneControl.Scene.FindSelection(true);
                recordset = selection3d[0].ToRecordset();
                if (e.Count > 0)
                {
                    selection3d = mSceneControl.Scene.FindSelection(true);
                    recordset = selection3d[0].ToRecordset();
                    if (recordset.GetGeometry().Type != mGeoLine3D.Type)
                    {
                        MessageBox.Show("请选择三维线对象!");
                    }
                    else
                    {
                        mGeoLine3D = recordset.GetGeometry() as GeoLine3D;
                        mTextPoint3Ds.Add(mGeoLine3D.InnerPoint3D);
                        Point3D cPoint3D = new Point3D(mGeoLine3D.InnerPoint3D.X, mGeoLine3D.InnerPoint3D.Y, 0);
                        mTextPoint3Ds.Add(cPoint3D);
                        for(int i = 0; i < recordset.FieldCount; i++)
                        {
                            str = recordset.GetFieldInfos()[i].Name;
                            if (str == "BottomAltitude")
                            {
                                obj = recordset.GetFieldValue(i);
                                mAltitude = Convert.ToDouble(obj);
                            }
                            if (str == "PipeDiameter")
                            {
                                obj = recordset.GetFieldValue(i);
                                mDiameter =new double[] {Convert.ToDouble(obj)};
                            }
                        }
                    }
                    GetCoverRusult();
                }
            }
            //碰撞检测
            if(mSceneControl.Action == Action3D.Select && mFlag == 4)
            {
                if (e.Count >0)
                {
                    string str = "";
                    object obj;                   
                    selection3d = mSceneControl.Scene.FindSelection(true);
                    recordset = selection3d[0].ToRecordset();
                    if (recordset.GetGeometry().Type != mGeoLine3D.Type)
                    {
                        MessageBox.Show("请选择三维线对象!");
                    }
                    else
                    {
                        mGeoLine3D = recordset.GetGeometry() as GeoLine3D;
                        if (TempGeoLine.Count > 0 && TempGeoLine[0].Bounds == mGeoLine3D.Bounds)
                        {
                            MessageBox.Show("请不要选择同一条三维线对象!");
                        }
                        else if (TempGeoLine.Count >0 && recordset.Dataset.Name == TempRecordset.Dataset.Name)
                        {
                            MessageBox.Show("请不要选择同一数据集线对象!");
                        }
                        else
                        {
                            mTextPoint3Ds.Add(mGeoLine3D.InnerPoint3D);
                            TempGeoLine.Add(mGeoLine3D);
                            TempRecordset = recordset;
                            for (int i = 0; i < recordset.FieldCount; i++)
                            {
                                str = recordset.GetFieldInfos()[i].Name;
                                if (str == "PipeDiameter")
                                {
                                    obj = recordset.GetFieldValue(i);
                                    if (TempGeoLine.Count == 2)
                                    {
                                        mDiameter[1] = Convert.ToDouble(obj);
                                    }
                                    else
                                    {
                                        mDiameter = new double[2];
                                        mDiameter[0] = Convert.ToDouble(obj);
                                    }                                        
                                }
                            }
                        }
                    }
                    if (TempGeoLine.Count == 2)
                    {
                        this.SetPoint(TempGeoLine);                        
                    }
                }
            }
        }
               
        /// <summary>
        /// 设置管段起始终止点为分析点
        /// </summary>
        /// <param name="Geoline3Ds"></param>
        void SetPoint(List<GeoLine3D> Geoline3Ds)
        {       
            for (int i=0; i < Geoline3Ds.Count;i++ )
            {
                mGeoLine3D = Geoline3Ds[i];
                mPoint3Ds=mGeoLine3D[0];
                mPoint3Ds = coordinateTrans3D(mPoint3Ds);
                if (i == 0)
                {
                    mPoint3D_0 = mPoint3Ds.ToArray();
                }              
                else
                {
                    mPoint3D_1 = mPoint3Ds.ToArray(); 
                }
            }
            mResult = ClearDistanceAlgorithm(mPoint3D_0[0], mPoint3D_0[1], mPoint3D_1[0], mPoint3D_1[1]);
            switch (mFlag)
            {
                case 1:
                    GetHResult();
                    break;
                case 2: 
                    GetVResult();
                    break;
                case 4:
                    GetColResult();
                    break;
            }             
        }

        /// <summary>
        /// 测量高差
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private bool CheckAlutitude(Point3D p1, Point3D p2)
        {
            double z1 = p1.Z;
            double z2 = p2.Z;
            if (Math.Abs(z1- z2)<0.01)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 检查碰撞
        /// </summary>
        /// <param name="Distance"></param>
        /// <returns></returns>
        private bool CheckCollsion(double Distance)
        {
            double distance = Distance;
            if (mDiameter[0]/2 + mDiameter[1]/2 > Distance*1000)
            {
                return true;
            }
            else
                return false;            
        }

        /// <summary>
        /// 地理坐标向投影坐标转换
        /// </summary>
        /// <param name="point3Ds"></param>
        /// <returns></returns>
        private Point3Ds coordinateTrans3D(Point3Ds point3Ds)
        {
            Point2Ds point2Ds = new Point2Ds();
            foreach (Point3D p3d in point3Ds)
            {
                Point2D point2D = new Point2D(p3d.X, p3d.Y);
                point2Ds.Add(point2D);
            }
            //Assume Wgs1984WorldMercator as target project coordinate system
            PrjCoordSys prjTarget = new PrjCoordSys(PrjCoordSysType.Wgs1984Utm50N);
            bool b = CoordSysTranslator.Forward(point2Ds, prjTarget);

            Point3Ds result = new Point3Ds();
            for (int i = 0; i < point2Ds.Count; i++)
            {
                Point3D point3D = new Point3D(point2Ds[i].X, point2Ds[i].Y, point3Ds[i].Z);
                result.Add(point3D);
            }
            return result;
        }

        /// <summary>
        /// 两线段间空间距离算法
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public bool IsEqual(double d1, double d2)
        {
            if (Math.Abs(d1 - d2) < 1e-7)
                return true;
            return false;
        }

        private double[] ClearDistanceAlgorithm(Point3D p1, Point3D p2, Point3D p3, Point3D p4)
        {
            double x1 = p1.X;
            double y1 = p1.Y;
            double z1 = p1.Z;
            double x2 = p2.X;
            double y2 = p2.Y;
            double z2 = p2.Z;
            double x3 = p3.X;
            double y3 = p3.Y;
            double z3 = p3.Z;
            double x4 = p4.X;
            double y4 = p4.Y;
            double z4 = p4.Z;

            // 解析几何通用解法，可以求出点的位置，判断点是否在线段上
            // 算法描述：设两条无限长度直线s、t,起点为s0、t0，方向向量为u、v
            // 最短直线两点：在s1上为s0+sc*u，在t上的为t0+tc*v
            // 记向量w为(s0+sc*u)-(t0+tc*v),记向量w0=s0-t0
            // 记a=u*u，b=u*v，c=v*v，d=u*w0，e=v*w0——(a)；
            // 由于u*w=、v*w=0，将w=-tc*v+w0+sc*u带入前两式得：
            // (u*u)*sc - (u*v)*tc = -u*w0  (公式2)
            // (v*u)*sc - (v*v)*tc = -v*w0  (公式3)
            // 再将前式(a)带入可得sc=(be-cd)/(ac-b2)、tc=(ae-bd)/(ac-b2)——（b）
            // 注意到ac-b2=|u|2|v|2-(|u||v|cosq)2=(|u||v|sinq)2不小于0
            // 所以可以根据公式（b）判断sc、tc符号和sc、tc与1的关系即可分辨最近点是否在线段内
            // 当ac-b2=0时，(公式2)(公式3)独立，表示两条直线平行。可令sc=0单独解出tc
            // 最终距离d（L1、L2）=|（P0-Q0)+[(be-cd)*u-(ae-bd)v]/(ac-b2)|
            double ux = x2 - x1;
            double uy = y2 - y1;
            double uz = z2 - z1;

            double vx = x4 - x3;
            double vy = y4 - y3;
            double vz = z4 - z3;

            double wx = x1 - x3;
            double wy = y1 - y3;
            double wz = z1 - z3;

            double a = (ux * ux + uy * uy + uz * uz); //u*u
            double b = (ux * vx + uy * vy + uz * vz); //u*v
            double c = (vx * vx + vy * vy + vz * vz); //v*v
            double d = (ux * wx + uy * wy + uz * wz); //u*w 
            double e = (vx * wx + vy * wy + vz * wz); //v*w
            double dt = a * c - b * b;

            double sd = dt;
            double td = dt;

            double sn = 0.0;//sn = be-cd
            double tn = 0.0;//tn = ae-bd

            if (IsEqual(dt, 0.0))
            {
                //两直线平行
                sn = 0.0;    //在s上指定取s0
                sd = 1.00;   //防止计算时除0错误

                tn = e;      //按(公式3)求tc
                td = c;
            }
            else
            {
                sn = (b * e - c * d);
                tn = (a * e - b * d);
                if (sn < 0.0)
                {
                    //最近点在s起点以外，同平行条件
                    sn = 0.0;
                    tn = e;
                    td = c;
                }
                else if (sn > sd)
                {
                    //最近点在s终点以外(即sc>1,则取sc=1)
                    sn = sd;
                    tn = e + b; //按(公式3)计算
                    td = c;
                }
            }
            if (tn < 0.0)
            {
                //最近点在t起点以外
                tn = 0.0;
                if (-d < 0.0) //按(公式2)计算，如果等号右边小于0，则sc也小于零，取sc=0
                    sn = 0.0;
                else if (-d > a) //按(公式2)计算，如果sc大于1，取sc=1
                    sn = sd;
                else
                {
                    sn = -d;
                    sd = a;
                }
            }
            else if (tn > td)
            {
                tn = td;
                if ((-d + b) < 0.0)
                    sn = 0.0;
                else if ((-d + b) > a)
                    sn = sd;
                else
                {
                    sn = (-d + b);
                    sd = a;
                }
            }

            double sc = 0.0;
            double tc = 0.0;

            if (IsEqual(sn, 0.0))
                sc = 0.0;
            else
                sc = sn / sd;

            if (IsEqual(tn, 0.0))
                tc = 0.0;
            else
                tc = tn / td;

            double dx = wx + (sc * ux) - (tc * vx);
            double dy = wy + (sc * uy) - (tc * vy);
            double dz = wz + (sc * uz) - (tc * vz);

            double distance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double alutitude = Math.Abs(z1 - z3);
            double[] result = { distance,alutitude, sc, tc };
            return result;
        }
       
        /// <summary>
        /// 两管线对象水平净距
        /// </summary>
        private void GetHResult()
        {
            if (CheckAlutitude(TempGeoLine[0].InnerPoint3D, TempGeoLine[1].InnerPoint3D) == true)
            {
                mHDistance = mResult[0];
            }
            else
            {
                mHDistance = Math.Sqrt(Math.Abs(mResult[0] * mResult[0] - mResult[1] * mResult[1]));
            }
            mVDistance = mResult[1];
            if (Math.Abs(mHDistance - 0) < 0.001)
                ShowV();
            else
                ShowH();
        }

        /// <summary>
        /// 两管线对象竖直净距
        /// </summary>
        private void GetVResult()
        {
            mVDistance = mResult[1];
            ShowV();
        }

        /// <summary>
        /// 覆土深度计算
        /// </summary>
        /// <returns></returns>
        private void GetCoverRusult()
        {
            mCDistance = (mAltitude * 1000 - mDiameter[0] / 2)/1000;
            ShowC();
        }
        /// <summary>
        ///碰撞距离计算
        /// </summary>
        private void GetColResult()
        {
            if (CheckAlutitude(TempGeoLine[0].InnerPoint3D, TempGeoLine[1].InnerPoint3D) == true)
            {
                mHDistance = mResult[0];
            }
            else
            {
                mHDistance = Math.Sqrt(Math.Abs(mResult[0] * mResult[0] - mResult[1] * mResult[1]));
            }
            mVDistance = mResult[1];
            if (Math.Abs(mHDistance - 0) < 0.001)
            {
                if (!CheckCollsion(mVDistance))
                {
                    MessageBox.Show("经检测所选管线无碰撞");
                }
                else
                    MessageBox.Show("经检测所选管线存在碰撞点");
            }
            else
            {
                if (!CheckCollsion(mHDistance))
                {
                    MessageBox.Show("经检测所选管线无碰撞");
                }
                else
                    MessageBox.Show("经检测所选管线存在碰撞点");
            }                
        }

        /// <summary>
        /// 获取文本信息
        /// </summary>
        /// <returns></returns>
        private GeoText3D GetGeoText3DMessage()
        {
            GeoText3D text = CreateText3DMessage();
            mSceneControl.Scene.TrackingLayer.Add(text, MessageTag);
            mCurrentGeoText3DMessage = text;
            return text;
        }
        
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
        /// 设置结果文本样式
        /// </summary>
        /// <param name="text"></param>
        private void SetResultTextStyle(GeoText3D text)
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

        /// <summary>
        /// 获取3D要素风格
        /// </summary>
        /// <param name="geometry"></param>
        private void SetGeometry3DStyle(Geometry3D geometry)
        {
            GeoStyle3D style = new GeoStyle3D();
            style.MarkerSize = 4;
            style.MarkerColor = Color.FromArgb(255, 0, 255);
            style.AltitudeMode = AltitudeMode.RelativeToUnderground;
            //设置线样式
            style.LineColor = Color.Yellow;
            style.LineWidth = 3;
            style.FillMode = FillMode3D.LineAndFill;
            style.FillForeColor = Color.LightSeaGreen;
            geometry.Style3D = style;
        }
        /// <summary>
        /// 显示水平净距分析结果
        /// </summary>
        private void ShowH()
        {
            String text = String.Empty;
            Point3D textLocation = new Point3D(0, 0, 0);
            GeoLine3D geoLine = new GeoLine3D(mTextPoint3Ds);
            SetGeometry3DStyle(geoLine);
            mSceneControl.Scene.TrackingLayer.Add(geoLine, "水平净距");
            text = String.Format("{0}{1}{2}", "水平净距：", Math.Round(Convert.ToDouble(mHDistance), 4), "米");
            textLocation = geoLine.InnerPoint3D;
            GeoText3D geoText = new GeoText3D(new TextPart3D(text, textLocation));
            SetResultTextStyle(geoText);
            mSceneControl.Scene.TrackingLayer.Add(geoText, "水平");
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlSelected);
        }
        /// <summary>
        /// 显示垂距分析结果
        /// </summary>
        private void ShowV()
        {
            String text = String.Empty;
            Point3D textLocation = new Point3D(0, 0, 0);
            GeoLine3D geoLine = new GeoLine3D(mTextPoint3Ds);
            SetGeometry3DStyle(geoLine);
            mSceneControl.Scene.TrackingLayer.Add(geoLine, "竖直净距");
            text = String.Format("{0}{1}{2}", "竖直净距：", Math.Round(Convert.ToDouble(mVDistance), 4), "米");
            textLocation = geoLine.InnerPoint3D;
            GeoText3D geoText = new GeoText3D(new TextPart3D(text, textLocation));
            SetResultTextStyle(geoText);
            mSceneControl.Scene.TrackingLayer.Add(geoText, "竖直");
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlSelected);
        }
        /// <summary>
        /// 显示覆土分析结果
        /// </summary>
        private void ShowC()
        {
            String text = String.Empty;
            Point3D textLocation = new Point3D(0, 0, 0);
            GeoLine3D geoLine = new GeoLine3D(mTextPoint3Ds);
            SetGeometry3DStyle(geoLine);
            mSceneControl.Scene.TrackingLayer.Add(geoLine, "覆土深度");
            text = String.Format("{0}{1}{2}", "覆土深度：", Math.Round(Convert.ToDouble(mCDistance), 2), "米");
            textLocation = geoLine.InnerPoint3D;
            GeoText3D geoText = new GeoText3D(new TextPart3D(text, textLocation));
            SetResultTextStyle(geoText);
            mSceneControl.Scene.TrackingLayer.Add(geoText, "覆土");
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlSelected);
        }

        public void BenginSelect()
        {
            mSceneControl.Action = Action3D.Select;
        }
    }
}

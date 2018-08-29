using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperMap.Data;
using SuperMap.UI;
using SuperMap.Mapping;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： ManualEdit
    ** 描述： 图层编辑
    ** 作者： GJF
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    /// <summary>
    /// 图层编辑类
    /// </summary>
    public class ManualEdit
    {
        private Workspace mWorkspace;
        private MapControl mMapControl;


        private Layer mPointLayer;
        private Layer mLineLayer;
        private Layer mRegionLayer;
        private Layer mTextLayer;

        public string mFilePath;
        private LayerSettingVector mDefaultSetting;
        private LayerSettingVector mBackgroudSetting;



        public ManualEdit(Workspace workspace, MapControl mapControl)
        {

            mWorkspace = workspace;
            mMapControl = mapControl;


            Initialize();//打开需要的工作空间文件及事件注册
        }
        public void GetPath(string path)
        {
            this.mFilePath = path;
        }
        /// <summary>
        /// 打开需要的工作空间文件及事件注册
        /// </summary>
        private void Initialize()
        {
            try
            {

                //打开工作空间及地图
                WorkspaceConnectionInfo conInfo = new WorkspaceConnectionInfo(mFilePath);
                mWorkspace.Open(conInfo);
                //创建数据集
                CreateDatasets();
                //禁用鼠标等待
                mMapControl.IsWaitCursorEnabled = false;//当前窗口的等待光标是否有效
                mMapControl.TrackMode = TrackMode.Edit;//TrackMode.Edit在图层中创建一个新对象
                //该枚举定义了绘制方式类型常量。用来定义地图控件中绘制对象时，是在图层中创建一个新对象还是在内存中创建一个新对象，或者是在CAD图层中绘制地图几何对象（GeoMap）。 

                //获得默认的对象风格
                mDefaultSetting = new LayerSettingVector();//构造矢量图层设置新实例
                GeoStyle defaultStyle = new GeoStyle();//构造 几何风格 对象
                defaultStyle.LineColor = Color.Green;//颜色
                defaultStyle.LineWidth = 0.3;//宽度
                mDefaultSetting.Style = defaultStyle;//设置矢量图层的风格 为 defaultStyle 
                //初始化背景图片显示时对象的风格
                mBackgroudSetting = new LayerSettingVector();//
                GeoStyle backgroudStyle = new GeoStyle();
                backgroudStyle.FillForeColor = Color.PaleGreen; ;
                backgroudStyle.LineColor = Color.Red;
                backgroudStyle.LineWidth = 0.3;
                mBackgroudSetting.Style = backgroudStyle;

                mMapControl.Tracked += new TrackedEventHandler(m_mapControl_Tracked);//在地图窗口中绘制几何对象 结束后发生

                DrawPoint();//绘制点
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 创建数据集
        /// </summary>
        private void CreateDatasets()
        {
            try
            {
                //创建点、线、面、文本数据集
                Datasource datasource = mWorkspace.Datasources[0];
                //数据源集合对象存储工作空间下的数据源的逻辑信息， 如数据源数据的连接信息，位置等，并不存储实际的数据源数据。
                //实际的数据源数据存储在关系型数据库或UDB文件中。 工作空间的数据源集合对象主要用来管理该工作空间中的数据源，
                //包括打开、创建、关闭等操作。

                datasource.Datasets.Delete("point");//若 有点集合 则 删除...因为不重复创建
                DatasetVectorInfo pointLayerInfo = new DatasetVectorInfo("point", DatasetType.Point);//数据集类型--点
                //根据指定的参数来构造一个 DatasetVectorInfo 的新对象（数据集名称，数据集类型），返回一个DatasetVectorInfo类型
                //DatasetVectorInfo矢量数据集信息类。包括了矢量数据集的信息，如矢量数据集的名称，数据集的类型，编码方式，是否选用文件缓存等。
                //文件缓存只针对图库索引而言。

                DatasetVector pointDataset = datasource.Datasets.Create(pointLayerInfo);//根据矢量数据集信息 构造矢量数据集
                //DatasetVector描述矢量数据集，并提供相应的管理和操作。对矢量数据集的操作主要包括数据查询、修改、删除、建立索引等。
                //思路： ① 利用工作空间构造数据源集合对象datasource ②根据指定参数构造数据集信息对象 ③ 根据数据源集合对象 利用矢量数据集信息 构造矢量数据集
                //同上
                datasource.Datasets.Delete("line");
                DatasetVectorInfo lineLayerInfo = new DatasetVectorInfo("line", DatasetType.Line);//数据集类型--线
                DatasetVector lineDataset = datasource.Datasets.Create(lineLayerInfo);
                //同上
                datasource.Datasets.Delete("region");
                DatasetVectorInfo regionLayerInfo = new DatasetVectorInfo("region", DatasetType.Region);//数据集类型--多边形
                DatasetVector regionDataset = datasource.Datasets.Create(regionLayerInfo);
                //同上
                datasource.Datasets.Delete("text");
                DatasetVectorInfo textLayerInfo = new DatasetVectorInfo("text", DatasetType.Text);//数据集类型--文本
                DatasetVector textDataset = datasource.Datasets.Create(textLayerInfo);
                //将点、线、面、文本数据集加入到地图中
                mPointLayer = mMapControl.Map.Layers.Add(pointDataset, true);//把点数据集添加到点图层
                mLineLayer = mMapControl.Map.Layers.Add(lineDataset, true);//把线数据集添加到线图层
                mRegionLayer = mMapControl.Map.Layers.Add(regionDataset, true);//把多边形数据集添加到多边形图层
                mTextLayer = mMapControl.Map.Layers.Add(textDataset, true);//把文本数据集添加到文本图层
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 在输出窗口输出消息
        /// </summary>
        /// <param name="printMessage"></param>



        //-------------------------------------------------------------------------------------
        //-------------------------绘制 点 线 面 文本--------------------------------------------------
        //-------------------------------------------------------------------------------------
        /// <summary>
        /// 绘制点
        /// </summary>
        public void DrawPoint()
        {
            try
            {

                mPointLayer.IsEditable = true;

                mLineLayer.IsEditable = false;

                mRegionLayer.IsEditable = false;

                mTextLayer.IsEditable = false;
                mMapControl.Action = SuperMap.UI.Action.CreatePoint;//画点
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// 绘制线
        /// </summary>
        public void DrawLine()
        {
            try
            {
                mPointLayer.IsEditable = false;
                mLineLayer.IsEditable = true;
                mRegionLayer.IsEditable = false;
                mTextLayer.IsEditable = false;
                mMapControl.Action = SuperMap.UI.Action.CreateLine;//划线
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 绘制面
        /// </summary>
        public void DrawRegion()
        {
            try
            {
                mPointLayer.IsEditable = false;
                mLineLayer.IsEditable = false;
                mRegionLayer.IsEditable = true;
                mTextLayer.IsEditable = false;
                mMapControl.Action = SuperMap.UI.Action.CreatePolygon; ;//画多边形
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 绘制文本
        /// </summary>
        public void DrawText()
        {
            try
            {
                mPointLayer.IsEditable = false;//点图层 不可见
                mLineLayer.IsEditable = false;//线图层 不可见
                mRegionLayer.IsEditable = false;//面图层 不可见
                mTextLayer.IsEditable = true;//文本图层可见
                mMapControl.Action = SuperMap.UI.Action.CreateText;//添加注记
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }





        //---------------------------------------------------------------------------------
        //-------------------------注册事件------------------------------------------------
        //----------------------------------------------------------------------------------


        /// <summary>
        /// 改变MapControl的Action
        /// </summary>
        /// <param name="action"></param>
        public void SetAction(SuperMap.UI.Action action)
        {
            mMapControl.Action = action;
        }


        /// <summary>
        //跟踪层绘制事件，用于添加文本对象---绘制完几何对象后，判断 是否要继续绘制文本 或者 绘制沿线文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_mapControl_Tracked(Object sender, TrackedEventArgs e)
        {
            try
            {
                //绘制文本
                if (mMapControl.Action.Equals(SuperMap.UI.Action.CreateText))//若 地图操作状态为添加注记
                {
                    GeoText geoText = e.Geometry as GeoText;//获取刚刚绘制完成的几何图形 转为 GeoText文本类
                    geoText.TextStyle.ForeColor = Color.Green;//设置文本风格--前景色 
                    geoText.TextStyle.FontHeight = 32;//文本字体的高度为32
                    if (mTextLayer.IsEditable)//若 文本类图层 可编辑
                    {
                        String text = Microsoft.VisualBasic.Interaction.InputBox("请输入文本", "绘制文本", "", mMapControl.Size.Width / 2 - 150, mMapControl.Size.Height / 2);
                        // public static string InputBox(string Prompt, string Title = "", string DefaultResponse = "", int XPos = -1, int YPos = -1);
                        geoText[0].Text = text;//文本内容赋值
                    }
                }
                //绘制沿线文本
                if (mMapControl.Action == SuperMap.UI.Action.CreateAlongLineText)//若 地图操作状态为添加沿线标注
                {
                    GeoCompound geoCompound = e.Geometry as GeoCompound;//转为 复合几何对象类
                    if (mTextLayer.IsEditable)//若 文本类图层 可编辑
                    {
                        String text = Microsoft.VisualBasic.Interaction.InputBox("请输入沿线文本", "绘制沿线文本", "", mMapControl.Size.Width / 2 - 150, mMapControl.Size.Height / 2);
                        GeoText geoText = geoCompound[0] as GeoText;
                        geoText.TextStyle.ForeColor = Color.Green;
                        geoText.TextStyle.FontHeight = 32;
                        geoText[0].Text = text;
                    }
                }

                mMapControl.Map.Refresh();//重新绘制当前地图
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }



    }
}

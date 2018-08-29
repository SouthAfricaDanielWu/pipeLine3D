using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SuperMap.Analyst.NetworkAnalyst;
using SuperMap.Data;
using SuperMap.Mapping;
using SuperMap.UI;
using SuperMap.Realspace;
using SuperMap.Realspace.NetworkAnalyst;
using System.Diagnostics;
using System.IO;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： PipeNetworkAnalysis
    ** 描述： 连通分析等操作
    ** 作者： GJF
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    /// <summary>
    /// 官网分析类
    /// </summary>
  public  class PipeNetworkAnalysis
    {

        #region 私有字段
        private SceneControl mSceneControl;
        private UseData mUseData;
        public InformationBubble mInformationBubble;

        //*****************流通判断字段*************************
        int mFlag;//流通判断----确定数组的索引--通过索引赋值给mPipeConId1 mPipeConId2

        private int mPipeConId1;//流通判断----将数组接收的ID，传给窗体
        private int mPipeConId2;//流通判断----将数组接收的ID，传给窗体
        public int PipeConId1//流通判断----为了向窗体传属性
        { get { return mPipeConId1; } }
        public int PipeConId2//流通判断----为了向窗体属性
        { get { return mPipeConId2; } }

        private string mConLayerName;//流通判断----当前图层
        public string ConLayerName
        { get { return mConLayerName; } }
        public GeometryType recordsetTypeConId1;//流通判断----第一个管线的记录集类型（点 or 线）
        public GeometryType recordsetTypeConId2;
        public string mPipeTypeConId1;//流通判断----第一个管线的类型（给 or 排）
        public string mPipeTypeConId2;//流通判断----第二个管线的类型
        private double mPipeAltitude; //流通判断----管点高程
        //流通判断----看第一个是不是被选中，以判断 当前选择是第一个点还是第二个点
        bool mIsPipe1Selected = false;
        public bool IsPipe1SelectedConnect
        { get { return mIsPipe1Selected; } set { mIsPipe1Selected = value; } }


        private DatasetVector mConNetWorkName;//流通判断----当前网络数据集

        //流通判断----判断是否连通，0为不连通，1为连通
        public int mConnectFlag = 0;
        /// <summary>
        /// 流通判断----判断是否连通
        /// </summary>
        public int ConnectFlag
        {
            get { return mConnectFlag; }
            set { mConnectFlag = value; }
        }

        Int32[] mConnectUpEdges;//流通判断----上游弧段数组
        Int32[] mConnectDownEdges;//流通判断----下游弧段数组
        Int32[] mConnectUpNodes;//流通判断----上游结点数组
        Int32[] mConnectDownNodes;//;//流通判断----下游结点数组
        //****************************************************************

        //流向分析**********************************
        //当前图层名称
        private string mLayerName;
        public string LayerName
        { get { return mLayerName; } }
        //流向分析--------判断类型（点 or 线）
        private string mSeachFlag;
        public string SeachFlag
        {
            get { return mSeachFlag; }
        }
        //流通分析--------判断类型（点 or 线）
        private String mConnectSeachFlag;


        private double mSinkFromNodeCost;//汇结点代价
        public double SinkFromNodeCost
        { get { return mSinkFromNodeCost; } }

        private double mSourceFromNodeCost;//源结点代价
        public double SourceFromNodeCost
        { get { return mSourceFromNodeCost; } }

        public int mSinkNode;//汇结点ID
        public int SinkNode
        { get { return mSinkNode; } }

        public int mSourceNode;//源结点ID
        public int SourceNode
        { get { return mSourceNode; } }

        Int32[] mSourceEdges;//源弧段数组
        Int32[] mSinkEdges;//汇弧段数组

        Int32[] mTraceUpEdges;//上游弧段数组
        Int32[] mTraceDownEdges;//下游弧段数组

        private int mPipeId1;//选择管线点ID-1
        public int PipeId1//当前分析的ID
        { get { return mPipeId1; } }

        FacilityAnalyst3D mfacilityAnalyst;
        private DatasetVector network=null;


        #endregion




        /// <summary>
        /// PipeNetworkAnalysis构造函数
        /// </summary>
        public PipeNetworkAnalysis(SceneControl sceneControl, UseData UseData, InformationBubble informationBubble)
        {
            mSceneControl = sceneControl;
            mUseData = UseData;
            mInformationBubble = informationBubble;
            Initialize();
        }


        /// <summary>
        /// 对象初始化
        /// </summary>
        private void Initialize()
        {

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


        #region 流向分析
        /// <summary>
        /// 获取选择点ID
        /// </summary>
        public void SelectPoint()
        {
            mSceneControl.Action = Action3D.Select;
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedCon);
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedFlow);
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlObjectSelectedFlow);
        }

        void mSceneControlObjectSelectedFlow(object sender, ObjectSelectedEventArgs e)
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
                    selection3d[0].Style.LineColor = Color.Blue;
                    recordset = selection3d[0].ToRecordset();
                    string[] StrName = new string[recordset.FieldCount];
                    bool PipeFlag = false;
                    string PipeType = null;
                    //判断是否为管线或管点对象 ，若不是 则提示选择管点或管线对象
                    for (int i = 0; i < recordset.FieldCount; i++)
                    {
                        StrName[i] = recordset.GetFieldInfos()[i].Name;
                        if (StrName[i] == "PipeType")
                        {
                            PipeFlag = true;
                            break;
                        }
                    }
                    if (PipeFlag == true)
                    {
                        PipeType = recordset.GetFieldValue("PipeType").ToString();
                        switch (PipeType.Trim())
                        {
                            case "给水":
                                network = mUseData.SupplyWaterNetWork;
                                mLayerName = "给水管网@127.0.0.1_UnderPipeLine";
                                break;
                            case "排水":
                                network = mUseData.OutWaterNetWork;
                                mLayerName = "排水管网@127.0.0.1_UnderPipeLine";
                                break;
                            default:
                                MessageBox.Show("请选择其他类型管线对象");
                                break;
                        }
                        if (network!=null)
                        {
                            mPipeId1 = Convert.ToInt32(recordset.GetFieldValue("SmID"));
                            Geometry geo = recordset.GetGeometry();
                            GeometryType recordsetType = geo.Type;
                            if (recordsetType == GeometryType.GeoLine3D)
                            {
                                MessageBox.Show("当前选择管线ID为" + mPipeId1.ToString().Trim());
                                mSeachFlag = "Line";
                                mConnectSeachFlag = "Line";
                            }
                            else if (recordsetType == GeometryType.GeoPoint3D)
                            {
                                MessageBox.Show("当前选择管点ID为" + mPipeId1.ToString().Trim());
                                mConnectSeachFlag = "Node";
                                mSeachFlag = "Node";
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("请选择管点管线对象");
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
        /// 寻找源/汇/上游/下游
        /// </summary>
        /// <param name="SeachFlag"></param>
        public void FindSourceAndSink(string SeachFlag)
        {

            try
            {
                //三维网络信息设置
                FacilityAnalystSetting3D facilityAnalystSetting = new FacilityAnalystSetting3D();
                facilityAnalystSetting.NetworkDataset = network;
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


                mfacilityAnalyst = new FacilityAnalyst3D();
                mfacilityAnalyst.AnalystSetting = facilityAnalystSetting;

                bool isLoad = mfacilityAnalyst.Load();//是否加载模型
                if (isLoad)
                {
                    switch (SeachFlag)
                    {
                        case "Line":
                            #region 根据弧段查找
                            //根据弧段查找汇
                            FacilityAnalystResult3D resultSinkFromEdges = mfacilityAnalyst.FindSinkFromEdge(mPipeId1, "Length", true);
                            //汇弧段ID数组
                            {
                                if (resultSinkFromEdges != null)
                                {
                                    mSinkFromNodeCost = resultSinkFromEdges.Cost;
                                    mSinkEdges = new Int32[resultSinkFromEdges.Edges.Length];
                                    for (int i = 0; i < resultSinkFromEdges.Edges.Length; i++)
                                    {

                                        mSinkEdges[i] = resultSinkFromEdges.Edges[i];
                                    }
                                    //汇结点ID
                                    for (int i = 0; i < resultSinkFromEdges.Nodes.Length; i++)
                                    {
                                        if (i == resultSinkFromEdges.Nodes.Length - 1)
                                        {
                                            mSinkNode = resultSinkFromEdges.Nodes[i];
                                        }
                                    }

                                }
                                else { mSinkEdges = new Int32[1]; mSinkFromNodeCost = 0; }
                            }

                            //根据弧段查找源
                            FacilityAnalystResult3D resultSourceFromEdges = mfacilityAnalyst.FindSourceFromEdge(mPipeId1, "Length", true);


                            //源弧段ID数组
                            {
                                if (resultSourceFromEdges != null)
                                {
                                    mSourceFromNodeCost = resultSourceFromEdges.Cost;
                                    mSourceEdges = new Int32[resultSourceFromEdges.Edges.Length];
                                    for (int i = 0; i < resultSourceFromEdges.Edges.Length; i++)
                                    {

                                        mSourceEdges[i] = resultSourceFromEdges.Edges[i];
                                    }
                                    //源节点ID
                                    for (int i = 0; i < resultSourceFromEdges.Nodes.Length; i++)
                                    {
                                        if (i == resultSourceFromEdges.Nodes.Length - 1)
                                        {
                                            mSourceNode = resultSourceFromEdges.Nodes[i];
                                        }
                                    }
                                }
                                else { mSourceEdges = new Int32[1]; mSourceFromNodeCost = 0; }
                            }




                            //根据弧段查上游
                            FacilityAnalystResult3D TraceUpFromEdges = mfacilityAnalyst.TraceUpFromEdge(mPipeId1, "Length", true);
                            {
                                if (TraceUpFromEdges != null)
                                {
                                    mTraceUpEdges = new Int32[TraceUpFromEdges.Edges.Length];
                                    for (int i = 0; i < TraceUpFromEdges.Edges.Length; i++)
                                    {
                                        mTraceUpEdges[i] = TraceUpFromEdges.Edges[i];
                                    }
                                }
                                else { mTraceUpEdges = new Int32[1]; }
                            }

                            //根据弧段查下游
                            FacilityAnalystResult3D TraceDownFromEdges = mfacilityAnalyst.TraceDownFromEdge(mPipeId1, "Length", true);
                            {
                                if (TraceDownFromEdges != null)
                                {
                                    mTraceDownEdges = new Int32[TraceDownFromEdges.Edges.Length];
                                    for (int i = 0; i < TraceDownFromEdges.Edges.Length; i++)
                                    {
                                        mTraceDownEdges[i] = TraceDownFromEdges.Edges[i];

                                    }
                                }
                                else { mTraceDownEdges = new Int32[1]; }
                            }

                            #endregion
                            break;

                        case "Node":
                            #region 根据结点查找
                            //根据结点查找汇
                            FacilityAnalystResult3D resultSinkFromNode = mfacilityAnalyst.FindSinkFromNode(mPipeId1, "Length", true);


                            //汇弧段ID数组
                            {
                                if (resultSinkFromNode != null)
                                {
                                    mSinkFromNodeCost = resultSinkFromNode.Cost;
                                    mSinkEdges = new Int32[resultSinkFromNode.Edges.Length];
                                    for (int i = 0; i < resultSinkFromNode.Edges.Length; i++)
                                    {

                                        mSinkEdges[i] = resultSinkFromNode.Edges[i];
                                    }
                                }
                                else { mSinkEdges = new Int32[1]; mSinkFromNodeCost = 0; }
                            }

                            //汇结点数组
                            {
                                if (resultSinkFromNode != null)
                                {
                                    Int32[] SinkNodes = new Int32[resultSinkFromNode.Nodes.Length];
                                    for (int i = 0; i < resultSinkFromNode.Nodes.Length; i++)
                                    {
                                        SinkNodes[i] = resultSinkFromNode.Nodes[i];
                                        if (i == resultSinkFromNode.Nodes.Length - 1)
                                        {
                                            mSinkNode = resultSinkFromNode.Nodes[i];
                                        }
                                    }
                                }
                                else { Int32[] SinkNodes = new Int32[1]; }
                            }


                            //根据结点查找源
                            FacilityAnalystResult3D resultSourceFromNode = mfacilityAnalyst.FindSourceFromNode(mPipeId1, "Length", true);


                            //源弧段ID数组
                            {
                                if (resultSourceFromNode != null)
                                {
                                    mSourceFromNodeCost = resultSourceFromNode.Cost;
                                    mSourceEdges = new Int32[resultSourceFromNode.Edges.Length];
                                    for (int i = 0; i < resultSourceFromNode.Edges.Length; i++)
                                    {

                                        mSourceEdges[i] = resultSourceFromNode.Edges[i];
                                    }
                                }
                                else { mSourceEdges = new Int32[1]; mSourceFromNodeCost = 0; }
                            }

                            //源节点ID数组
                            if (resultSourceFromNode != null)
                            {
                                Int32[] SourceNodes = new Int32[resultSourceFromNode.Nodes.Length];
                                for (int i = 0; i < resultSourceFromNode.Nodes.Length; i++)
                                {

                                    SourceNodes[i] = resultSourceFromNode.Nodes[i];
                                    if (i == resultSourceFromNode.Nodes.Length - 1)
                                    {
                                        mSourceNode = resultSourceFromNode.Nodes[i];
                                    }
                                }
                            }
                            else { Int32[] SourceNodes = new Int32[1]; }



                            //根据结点查上游
                            FacilityAnalystResult3D TraceUpFromNode = mfacilityAnalyst.TraceUpFromNode(mPipeId1, "Length", true);
                            {
                                if (TraceUpFromNode != null)
                                {
                                    mTraceUpEdges = new Int32[TraceUpFromNode.Edges.Length];
                                    for (int i = 0; i < TraceUpFromNode.Edges.Length; i++)
                                    {
                                        mTraceUpEdges[i] = TraceUpFromNode.Edges[i];
                                    }
                                }
                                else { mTraceUpEdges = new Int32[1]; }
                            }

                            //根据结点查下游
                            FacilityAnalystResult3D TraceDownFromNode = mfacilityAnalyst.TraceDownFromNode(mPipeId1, "Length", true);
                            if (TraceDownFromNode != null)
                            {
                                mTraceDownEdges = new Int32[TraceDownFromNode.Edges.Length];
                                for (int i = 0; i < TraceDownFromNode.Edges.Length; i++)
                                {
                                    mTraceDownEdges[i] = TraceDownFromNode.Edges[i];

                                }
                            }
                            else { mTraceDownEdges = new Int32[1]; }

                            #endregion
                            break;
                        default:
                            break;
                    }



                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);

            }
        }
        /// <summary>
        /// 显示源
        /// </summary>
        public void ShowSourceEdges(string Layername)
        {
            mSceneControl.Scene.Layers[Layername].Selection.Clear();
            if (mSourceEdges[0] != 0)
            {
                mSceneControl.Scene.Layers[Layername].Selection.AddRange(mSourceEdges);
                mSceneControl.Scene.Layers[Layername].Selection.Style.LineColor = Color.Green;
            }
            mSceneControl.Scene.Layers[Layername].Selection.UpdateData();
            mSceneControl.Scene.Refresh();
        }
        /// <summary>
        /// 显示汇
        /// </summary>
        public void ShowSinkEdges(string Layername)
        {
            mSceneControl.Scene.Layers[Layername].Selection.Clear();
            if (mSinkEdges[0] != 0)
            {
                mSceneControl.Scene.Layers[Layername].Selection.AddRange(mSinkEdges);
                mSceneControl.Scene.Layers[Layername].Selection.Style.LineColor = Color.Green;
                mSceneControl.Scene.Layers[Layername].Selection.UpdateData();
            }
            mSceneControl.Scene.Refresh();
        }
        /// <summary>
        /// 全线显示
        /// </summary>
        public void ShowAllEdges(string Layername)
        {
            mSceneControl.Scene.Layers[Layername].Selection.Clear();
            if (mSeachFlag == "Line" && mPipeId1!=0)
            {
                mSceneControl.Scene.Layers[Layername].Selection.Add(mPipeId1);
            }

            if (mTraceUpEdges[0] != 0)
            {
                mSceneControl.Scene.Layers[Layername].Selection.AddRange(mTraceUpEdges);
            }

            if (mTraceDownEdges[0] != 0)
            {
                mSceneControl.Scene.Layers[Layername].Selection.AddRange(mTraceDownEdges);
            }
            mSceneControl.Scene.Layers[Layername].Selection.Style.LineColor = Color.Green;
            mSceneControl.Scene.Layers[Layername].Selection.UpdateData();
            mSceneControl.Scene.Refresh();
        }
        bool mClearFlag = false;
        /// <summary>
        /// 全线清楚
        /// </summary>
        public void FlowClearEdges(string Layername)
        {
            try
            {
                mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedCon);
                mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedFlow);
                mPipeId1 = 0;
                mClearFlag = true;
                if (mSourceEdges[0] > 0)
                {
                    Array.Clear(mSourceEdges, 0, mSourceEdges.Length);
                }
                if (mSinkEdges[0] != 0)
                {
                    Array.Clear(mSinkEdges, 0, mSinkEdges.Length);
                }
                if (mTraceUpEdges[0] != 0)
                {
                    Array.Clear(mTraceUpEdges, 0, mTraceUpEdges.Length);
                }
                if (mTraceDownEdges[0] != 0)
                {
                    Array.Clear(mTraceDownEdges, 0, mTraceDownEdges.Length);
                }

                Selection3D[] m_select = mSceneControl.Scene.FindSelection(false);
                if (m_select != null)
                {
                    for (int i = 0; i < m_select.Length; i++)
                    {
                        m_select[i].Clear();
                    }
                }   

                mSceneControl.Scene.Layers[Layername].Selection.Clear();
                mSceneControl.Scene.Layers[Layername].Selection.UpdateData();
                mSceneControl.Scene.Refresh();
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }
        /// <summary>
        /// 上游追溯
        /// </summary>
        public void TraceUp(string Layername)
        {
            mSceneControl.Scene.Layers[Layername].Selection.Clear();
            if (mTraceUpEdges[0] != 0)
            {
                mSceneControl.Scene.Layers[Layername].Selection.AddRange(mTraceUpEdges);
                mSceneControl.Scene.Layers[Layername].Selection.Style.LineColor = Color.Orange;
            }
            mSceneControl.Scene.Layers[Layername].Selection.UpdateData();
            mSceneControl.Scene.Refresh();
        }
        /// <summary>
        /// 下游追溯
        /// </summary>
        public void TraceDown(string Layername)
        {
            mSceneControl.Scene.Layers[Layername].Selection.Clear();
            if (mTraceDownEdges[0] != 0)
            {
                mSceneControl.Scene.Layers[Layername].Selection.AddRange(mTraceDownEdges);
                mSceneControl.Scene.Layers[Layername].Selection.Style.LineColor = Color.Orange;
            }
            mSceneControl.Scene.Layers[Layername].Selection.UpdateData();
            mSceneControl.Scene.Refresh();
        }
        #endregion


        #region 流通分析

        /// <summary>
        /// 流向显示，实质是显示上下游
        /// </summary>
        /// <param name="LayerName">当前图层名称</param>
        public void ConnectAnalysis(string LayerName)
        {
            try
            {
                FindSourceAndSink(mConnectSeachFlag);
                mSceneControl.Scene.Layers[LayerName].Selection.Clear();
                if (mTraceUpEdges[0] != 0)
                {
                    mSceneControl.Scene.Layers[LayerName].Selection.AddRange(mTraceUpEdges);
                }
                if (mTraceDownEdges[0] != 0)
                {
                    mSceneControl.Scene.Layers[LayerName].Selection.AddRange(mTraceDownEdges);
                }
                if (mConnectSeachFlag == "Line")
                {
                    mSceneControl.Scene.Layers[LayerName].Selection.Add(mPipeId1);
                }
                mSceneControl.Scene.Layers[LayerName].Selection.Style.LineColor = Color.Yellow;
                mSceneControl.Scene.Layers[LayerName].Selection.UpdateData();
                mSceneControl.Scene.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        /// <summary>
        /// 选择点对象
        /// </summary>
        /// <param name="flag">控制第几个点</param>
        public void SelectPoint2(int flag)
        {
            if (flag == 1 || flag == 2)
            {
                mFlag = flag;
            }
            else
            {
                mFlag = 2;
            }
            //flag 为控制第几个点 前窗体控制第一个点，若点完第一个点 顺利点第二个 则赋值为2
            //若不顺利 无论点多少次 第一个点不变 第二个符合条件的点赋值为2

            mSceneControl.Action = Action3D.Select;
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedFlow);
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedCon);
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlObjectSelectedCon);
        }
        public int ConFlag
        { get { return mFlag; } }
        public DatasetVector ConNetWorkName
        { get { return mConNetWorkName; } }

        void mSceneControlObjectSelectedCon(object sender, ObjectSelectedEventArgs e)
        {
            mIsPipe1Selected = true;
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
                    switch (mFlag)//判断是第几个被选中的点 第一个点用于获得图层 网络数据集等，第二个点要判断是否与一 一致
                    {

                        #region case1

                        case 1: mPipeTypeConId1 = recordset.GetFieldValue("PipeType").ToString().Trim();
                            switch (mPipeTypeConId1)
                            {
                                case "给水":
                                    mConNetWorkName = mUseData.SupplyWaterNetWork;
                                    mConLayerName = "给水管网@127.0.0.1_UnderPipeLine";
                                    mPipeAltitude = -2;
                                    break;
                                case "排水":
                                    mConNetWorkName = mUseData.OutWaterNetWork;
                                    mConLayerName = "排水管网@127.0.0.1_UnderPipeLine";
                                    mPipeAltitude = -3.5;
                                    break;
                                default:
                                    MessageBox.Show("请选择其他类型管线");
                                    mFlag = 0;
                                    break;
                            }
                            if (mConNetWorkName!=null)
                            {
                                mPipeConId1 = Convert.ToInt32(recordset.GetFieldValue("SmID"));
                                Geometry geo1 = recordset.GetGeometry();
                                recordsetTypeConId1 = geo1.Type;
                                if (recordsetTypeConId1 == GeometryType.GeoLine3D)
                                {
                                    MessageBox.Show("当前选择管线ID为" + mPipeConId1.ToString());
                                }
                                else if (recordsetTypeConId1 == GeometryType.GeoPoint3D)
                                {
                                    MessageBox.Show("当前选择管点ID为" + mPipeConId1.ToString());
                                }
                                else
                                {
                                    MessageBox.Show("请选择管点或管线对象");
                                }
                            }
                            break;
                        #endregion

                        #region case2

                        case 2:
                            //判断图层-----------------------------------
                            mPipeTypeConId2 = recordset.GetFieldValue("PipeType").ToString().Trim();
                            if (mPipeTypeConId2!= mPipeTypeConId1 || mPipeTypeConId2 == null)
                            {
                                MessageBox.Show("请选择" + mPipeTypeConId1 + "管线对象");
                                break;
                            }
                            //----------------------------------------------------

                            //判断类型--------------------------------------
                            Geometry geo2 = recordset.GetGeometry();
                            recordsetTypeConId2 = geo2.Type;
                            if (recordsetTypeConId2 != recordsetTypeConId1)
                            {
                                MessageBox.Show("请选择当前图层" + recordsetTypeConId1.ToString() + "对象");
                                break;
                            }
                            mPipeConId2 = Convert.ToInt32(recordset.GetFieldValue("SmID"));
                            if (recordsetTypeConId2 == GeometryType.GeoLine3D)
                            {
                                MessageBox.Show("当前选择管线ID为" + mPipeConId2.ToString());
                            }
                            else if (recordsetTypeConId2 == GeometryType.GeoPoint3D)
                            {
                                MessageBox.Show("当前选择管点ID为" + mPipeConId2.ToString());
                            }
                            //------------------------------------------------
                            break;
                        #endregion

                        default:
                            break;
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
        /// 连通判断
        /// </summary>
        /// <param name="PipeConIdFlag">需要判断的ID</param>
        public void ConenectJudge(int PipeConIdFlag)
        {
            #region 找出上下游弧段 管点，并赋值给数组
            DatasetVector network = mConNetWorkName;
            try
            {
                //设置分析环境
                FacilityAnalystSetting3D facilityAnalystSetting = new FacilityAnalystSetting3D();
                facilityAnalystSetting.NetworkDataset = network;
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


                FacilityAnalyst3D mfacilityAnalyst = new FacilityAnalyst3D();
                mfacilityAnalyst.AnalystSetting = facilityAnalystSetting;

                bool isLoad = mfacilityAnalyst.Load();//是否加载模型
                if (isLoad)
                {
                    if (recordsetTypeConId1 == GeometryType.GeoPoint3D)//以点查找上下游弧段
                    {
                        //根据点查上游
                        FacilityAnalystResult3D TraceUpFromNode = mfacilityAnalyst.TraceUpFromNode(PipeConIdFlag, "Length", true);
                        {
                            //判断是否为空 若为空则实例化，避免在删除时候出错
                            if (TraceUpFromNode != null)
                            {
                                mConnectUpEdges = new Int32[TraceUpFromNode.Edges.Length];
                                for (int i = 0; i < TraceUpFromNode.Edges.Length; i++)
                                {
                                    mConnectUpEdges[i] = TraceUpFromNode.Edges[i];
                                }
                            }
                            else
                            { mConnectUpEdges = new Int32[1]; }
                        }


                        {
                            if (TraceUpFromNode != null)
                            {
                                mConnectUpNodes = new Int32[TraceUpFromNode.Nodes.Length];
                                for (int i = 0; i < TraceUpFromNode.Nodes.Length; i++)
                                {
                                    mConnectUpNodes[i] = TraceUpFromNode.Nodes[i];
                                }

                            }
                            else
                            { mConnectUpNodes = new Int32[1]; }
                        }
                        //根据结点查下游
                        FacilityAnalystResult3D TraceDownFromNode = mfacilityAnalyst.TraceDownFromNode(PipeConIdFlag, "Length", true);
                        {
                            if (TraceDownFromNode != null)
                            {
                                mConnectDownEdges = new Int32[TraceDownFromNode.Edges.Length];
                                for (int i = 0; i < TraceDownFromNode.Edges.Length; i++)
                                {
                                    mConnectDownEdges[i] = TraceDownFromNode.Edges[i];
                                }
                            }
                            else
                            { mConnectDownEdges = new Int32[1]; }
                        }

                        {
                            if (TraceDownFromNode != null)
                            {
                                mConnectDownNodes = new Int32[TraceDownFromNode.Nodes.Length];
                                for (int i = 0; i < TraceDownFromNode.Nodes.Length; i++)
                                {
                                    mConnectDownNodes[i] = TraceDownFromNode.Nodes[i];
                                }
                            }
                            else { mConnectDownNodes = new Int32[1]; }
                        }
                    }
                    else if (recordsetTypeConId1 == GeometryType.GeoLine3D)//以弧段查找弧段
                    {
                        //以弧段查找上游
                        FacilityAnalystResult3D TraceUpFromEdge = mfacilityAnalyst.TraceUpFromEdge(PipeConIdFlag, "Length", true);
                        {
                            if (TraceUpFromEdge != null)
                            {
                                mConnectUpEdges = new Int32[TraceUpFromEdge.Edges.Length];
                                for (int i = 0; i < TraceUpFromEdge.Edges.Length; i++)
                                {
                                    mConnectUpEdges[i] = TraceUpFromEdge.Edges[i];
                                }
                            }
                            else { mConnectUpEdges = new Int32[1]; }
                        }
                        {
                            if (TraceUpFromEdge != null)
                            {
                                mConnectUpNodes = new Int32[TraceUpFromEdge.Nodes.Length];
                                for (int i = 0; i < TraceUpFromEdge.Nodes.Length; i++)
                                {
                                    mConnectUpNodes[i] = TraceUpFromEdge.Nodes[i];
                                }
                            }
                            else { mConnectUpNodes = new Int32[1]; }
                        }
                        //根据弧段查下游
                        FacilityAnalystResult3D TraceDownFromEdge = mfacilityAnalyst.TraceDownFromEdge(PipeConIdFlag, "Length", true);
                        {
                            if (TraceDownFromEdge != null)
                            {
                                mConnectDownEdges = new Int32[TraceDownFromEdge.Edges.Length];
                                for (int i = 0; i < TraceDownFromEdge.Edges.Length; i++)
                                {
                                    mConnectDownEdges[i] = TraceDownFromEdge.Edges[i];

                                }

                            }
                            else { mConnectDownEdges = new Int32[1]; }
                        }
                        {
                            if (TraceDownFromEdge != null)
                            {
                                mConnectDownNodes = new Int32[TraceDownFromEdge.Nodes.Length];
                                for (int i = 0; i < TraceDownFromEdge.Nodes.Length; i++)
                                {
                                    mConnectDownNodes[i] = TraceDownFromEdge.Nodes[i];
                                }
                            }
                            else { mConnectDownNodes = new Int32[1]; }
                        }
                    }

                }

            #endregion

                #region 遍历上下游数组，寻找是否连通
                //若为线，遍历上下游弧段 若为点，遍历上下游管点 

                if (recordsetTypeConId1 == GeometryType.GeoLine3D)
                {
                    //第一次遍历时,进入前mConnectFlag=0，进入后若mConnectFlag=0，则进入下一个判断，若=1，说明连通 不在进行判断
                    //遍历上游弧段
                    for (int i = 0; i < mConnectUpEdges.Length; i++)
                    {
                        if (mConnectUpEdges[i] == mPipeConId2)
                        {
                            mConnectFlag = 1;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (mConnectFlag != 1)
                    {
                        //遍历下游弧段
                        for (int i = 0; i < mConnectDownEdges.Length; i++)
                        {
                            if (mConnectDownEdges[i] == mPipeConId2)
                            {
                                mConnectFlag = 1;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }
                if (recordsetTypeConId1 == GeometryType.GeoPoint3D)
                {
                    if (mConnectFlag != 1)
                    {
                        //遍历上游管点
                        for (int i = 0; i < mConnectUpNodes.Length; i++)
                        {
                            if (mConnectUpNodes[i] == mPipeConId2)
                            {
                                mConnectFlag = 1;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    if (mConnectFlag != 1)
                    {
                        //遍历下游管点
                        for (int i = 0; i < mConnectDownNodes.Length; i++)
                        {
                            if (mConnectDownNodes[i] == mPipeConId2)
                            {
                                mConnectFlag = 1;
                                break;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                }

                #endregion
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        /// <summary>
        /// 飞向管线ID
        /// </summary>
        /// <param name="flag">判断是第几个管线或管点对象</param>
        /// <param name="ID">获取该ID值</param>
        public void FlyToConnectID(int flag, int ID)
        {
            try
            {
                Recordset selected = null;
                DatasetVector network = mConNetWorkName;

                mSceneControl.Bubbles.Clear();
                Bubble bubble = new Bubble();
                mInformationBubble.Visible = true;

                //定义经纬度，为方便飞行定位
                Double mFlylongitude = 0;
                Double mFlylatitude = 0;
                //区分类型 按点飞 还是 按线飞
                if (recordsetTypeConId1 == GeometryType.GeoLine3D)
                {
                    String expression = "SmID = " + ID;
                    selected = network.Query(expression, CursorType.Static);
                    Geometry3D geometry = selected.GetGeometry() as Geometry3D;
                    mInformationBubble.Description.Text = "管线ID-" + flag + ":" + ID + "的位置如下"; ;
                    bubble.Pointer = new Point3D(geometry.InnerPoint3D.X, geometry.InnerPoint3D.Y, mPipeAltitude);
                    bubble.ClientWidth = mInformationBubble.Width;
                    bubble.ClientHeight = mInformationBubble.Height;
                    mInformationBubble.Location = new Point(bubble.ClientLeft, bubble.ClientTop);
                    mSceneControl.Bubbles.Add(bubble);
                    mFlylongitude = geometry.InnerPoint3D.X;
                    mFlylatitude = geometry.InnerPoint3D.Y;

                }
                else if (recordsetTypeConId1 == GeometryType.GeoPoint3D)
                {
                    String expression = "SmID = " + ID;
                    selected = network.ChildDataset.Query(expression, CursorType.Static);
                    object mAnalongitude = selected.GetFieldValue("SmX");
                    object mAnalatitude = selected.GetFieldValue("SmY");

                    mInformationBubble.Description.Text = "管点ID-" + flag + ":" + ID + "的位置如下";
                    bubble.Pointer = new Point3D(Convert.ToDouble(mAnalongitude), Convert.ToDouble(mAnalatitude), mPipeAltitude);
                    bubble.ClientWidth = mInformationBubble.Width;
                    bubble.ClientHeight = mInformationBubble.Height;
                    mInformationBubble.Location = new Point(bubble.ClientLeft, bubble.ClientTop);
                    mSceneControl.Bubbles.Add(bubble);
                    mFlylongitude = Convert.ToDouble(mAnalongitude);
                    mFlylatitude = Convert.ToDouble(mAnalatitude);
                }
                Camera camera = new Camera();
                camera.Longitude = Convert.ToDouble(mFlylongitude);
                camera.Latitude = Convert.ToDouble(mFlylatitude);
                camera.AltitudeMode = AltitudeMode.RelativeToGround;
                camera.Tilt = 0;
                camera.Altitude = 25;
                mSceneControl.Scene.Fly(camera, 10);
                mSceneControl.Scene.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }

        }
        /// <summary>
        /// 展示管线弧段
        /// </summary>
        /// <param name="LayerName">图层名称</param>
        public void ConnectShow(string LayerName)
        {
            //展示连通的上下游弧段，当为线时候 把自身显示，判断是否有上下游
            try
            {
                mSceneControl.Scene.Layers[LayerName].Selection.Clear();
                if (recordsetTypeConId1 == GeometryType.GeoLine3D && mPipeConId1!=0)
                {
                    mSceneControl.Scene.Layers[LayerName].Selection.Add(mPipeConId1);
                }
                if (mConnectUpEdges[0] != 0)
                {
                    mSceneControl.Scene.Layers[LayerName].Selection.AddRange(mConnectUpEdges);
                }
                if (mConnectDownEdges[0] != 0)
                {
                    mSceneControl.Scene.Layers[LayerName].Selection.AddRange(mConnectDownEdges);
                }

                mSceneControl.Scene.Layers[LayerName].Selection.Style.LineColor = Color.Yellow;
                mSceneControl.Scene.Layers[LayerName].Selection.UpdateData();
                mSceneControl.Scene.Refresh();
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }
        /// <summary>
        /// 全线清除掉
        /// </summary>
        /// <param name="Layername">图层的名称</param>
        public void ConnectClearEdges(string Layername)
        {
            try
            {
                /* 判断是否有上游下游 即判断数组是否有值，有值清空数组 无值跳过
                 * 按图层名字清空图层选择集 并注销掉两个MouseDown事件
                 */
                if (mConnectUpEdges[0] != 0)
                {

                    Array.Clear(mConnectUpEdges, 0, mConnectUpEdges.Length);
                }
                mPipeConId1 = 0;
                mPipeConId2 = 0;
                if (mConnectDownEdges[0] != 0)
                {

                    Array.Clear(mConnectDownEdges, 0, mConnectDownEdges.Length);
                }

                if (mConnectUpNodes[0] != 0)
                {

                    Array.Clear(mConnectUpNodes, 0, mConnectUpNodes.Length);
                }
                if (mConnectDownNodes[0] != 0)
                {

                    Array.Clear(mConnectDownNodes, 0, mConnectDownNodes.Length);
                }
                Layer3Ds m_layer = mSceneControl.Scene.Layers;
                foreach (Layer3D layer in m_layer)
                {
                    if (layer.Selection != null)
                    {
                        layer.Selection.Clear();
                    }
                }          
                mSceneControl.Scene.Layers[Layername].Selection.Clear();
                mSceneControl.Scene.Layers[Layername].Selection.UpdateData();
                mSceneControl.Scene.Refresh();
                mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedFlow);
                mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObjectSelectedCon);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        #endregion








    }
}

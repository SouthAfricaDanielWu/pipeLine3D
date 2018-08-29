using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Configuration;
using System.IO;

using SuperMap.UI;
using SuperMap.Data;
using SuperMap.Mapping;
using SuperMap.Realspace;
using SuperMap.Realspace.NetworkAnalyst;
using SuperMap.Realspace.SpatialAnalyst;
using NorthStar.Common;
using log4net;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： UseData
    ** 描述： 数据管理模块
    ** 作者： PLWhite
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：PLWhite
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心

    *********************************************************************************/

    /// <summary>
    /// 数据体构造函数
    /// </summary>
    public class UseData
    {
        #region 申明变量

        private WorkspaceControl mWorkspaceControl;

        private SceneControl mSceneControl;

        private MapControl mMapControl;

        private LayersControl mLayersControl;

        private Workspace mWorkspace;

        private Datasource mDatasource;
        public Datasource DataSource
        { get { return mDatasource; } }

        private List<String> mAllSceneNames;
        private Theme3DCustom mTheme3D;
        private Layer3DSettingVector mSettingLine;
        private KeyEventHandler mLayer3DsTreeDoKeyDelete;
        private KeyEventHandler mLayersTreeDoKeyDelete;
        private Layer3DSettingGrid mLayer3DSettingGrid;
        private Layer3DSettingImage mLayer3DSettingImage;
        private Layer3DSettingVector mLayer3DSettingVector;
        private AttributeQuery mQuery;
        private int mViewIndex;
        // private int m_viewIndex;
        private Boolean mHasCreated = false;
        // 缓冲区左右半径----------缓冲区分析//
        private Object mRadius;
        private DatasetVector mRegionData;
        private DatasetVector mRegion;
        // 缓冲区结果数据集和对应的场景图层----------缓冲区分析//
        private DatasetVector mBufferDataset;
        private DatasetVector mRegionDatasets;
        private Layer3D mLayerBuffer;
        private Layer3D mLayerRegion;
        // 分析结果记录集----------缓冲区分析//
        private Recordset mLineRecordset;
        Recordset mRecordset;
        Recordset mRegionRecordset;
        Geometry3D mGeo;
        private Geometry mQuerygeo;
        private GeometryType mType;
        private DataGridView mDatagrid;
        public InformationBubble mInformationBubble;


        private Common.ProgressBar pbar;
        public DatasetVector RodeNetwork
        {
            get { return mRodeNetwork; }
        }
        public DatasetVector PipeNet
        { get { return mPipeNet; } }
        public Layer3DDataset LayerNetNode
        { get { return mLayerNetNode; } }
        public Layer3DDataset LayerNetLine
        {
            get { return mLayerNetLine; }
        }
        //public FacilityAnalystSetting3D FacilityAnalystSetting
        //{ get { return m_facilityAnalystSetting; } }
        DatasetVector mRodeNetwork;

        public Geometry QueryGeo
        {
            get { return mQuerygeo; }
        }
        public GeometryType Type
        {
            get { return mType; }
        }

        //三维点矢量数据集
        private DatasetVector mDatasetPoint3D;
        //三维线矢量数据集
        private DatasetVector mDatasetLine3D;

        //三维点名
        private List<String> mNamesPoint3D;

        public List<String> NamesOfPoint3D
        {
            get { return mNamesPoint3D; }
        }

        //三维线名
        private List<String> mNamesLine3D;

        public List<String> NamesOfLine3D
        {
            get { return mNamesLine3D; }
        }

        //网络数据集
        private List<String> mNetworkDatasets;

        public List<String> NetworkDatasets
        {
            get { return mNetworkDatasets; }
        }

        //获取三维矢量点数据集内的属性信息
        private List<String> mPoint3DFields;

        public List<String> Point3DFields
        {
            get { return GetDatasetPoint3DFields(); }
            set { mPoint3DFields = value; }
        }

        //获取三维矢量线数据集内的属性信息
        private List<String> mLine3DFields;

        public List<String> Line3DFields
        {
            get { return GetDatasetLine3DFields(); }
            set { mLine3DFields = value; }
        }
        // 设施网络分析
        private DatasetVector mPipeNet;

        /// <summary>
        /// 系统字段
        /// </summary>
        private String[] m_point3DSysField = { "SMID", "SMX", "SMY", "SMZ", "SMUSERID", "SMGEOMETRYSIZE" };
        private String[] m_Line3DSysField = { "SMID", "SMLENGTH", "SMSDRIW", "SMSDRIN", "SMSDRIE", "SMSDRIS", "SMUSERID", "SMTOPOERROR", "SMGEOMETRYSIZE" };

        private FacilityAnalystSetting3D mFacilityAnalystSetting;

        public FacilityAnalystSetting3D FacilityAnalystSetting
        {
            get { return mFacilityAnalystSetting; }
        }

        private FacilityAnalyst3D mFacilityAnalyst;

        private Layer3DDataset mLayerNetLine;

        private Layer3DDataset mLayerNetNode;

        public int ViewIndex
        {
            set { mViewIndex = value; }
        }

        Rectangle2D mRec;
        private Point3D[] mTempPoint;
        private Point3Ds mPoint3Ds;
        public double mAltitude;

        #endregion
        public UseData(WorkspaceControl workspaceControl,
                        SceneControl sceneControl,
                        MapControl mapControl,
                        LayersControl layersControl,
                        DataGridView datagridview,
                        InformationBubble informationBubb
                        )
        {
            try
            {
                mWorkspaceControl = workspaceControl;
                mWorkspace = mWorkspaceControl.WorkspaceTree.Workspace;
                mSceneControl = sceneControl;
                mSceneControl.Scene.Workspace = mWorkspace;
                mMapControl = mapControl;
                mLayersControl = layersControl;
                mDatagrid = datagridview;
                mInformationBubble = informationBubb;
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
                mWorkspace.Closing += new WorkspaceClosingEventHandler(WorkspaceClosing);//关闭工作空间
                mWorkspace.Opened += new WorkspaceOpenedEventHandler(WorkspaceOpened);//打开工作空间
                mWorkspace.Datasources.Opened += new DatasourceOpenedEventHandler(DatasourcesOpened);//打开数据源
                mWorkspace.Datasources.Closing += new DatasourceClosingEventHandler(DatasourcesClosing);//关闭数据源

                //工作树对象
                mWorkspaceControl.WorkspaceTree.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(WorkspaceTreeDoubleClick);
                mWorkspaceControl.WorkspaceTree.AllowDefaultAction = true;
                mWorkspaceControl.WorkspaceTree.BeforeNodeContextMenuStripShow += new BeforeNodeContextMenuStripShowEventHandler(WorkspaceTreeBeforeMenuShow);
               
                //MapControl控件
                mMapControl.Map.Workspace = mWorkspace;
                mMapControl.AllowDrop = true;
                mMapControl.DoubleClick += new EventHandler(MapControlDoubleClick);

                // LayersTree保存Delete按键默认操作并自定义操作
                mLayersControl.Map = mMapControl.Map;
                mLayersTreeDoKeyDelete = mLayersControl.LayersTree.Interactions[InteractionType.KeyDelete] as KeyEventHandler;
                mLayersControl.LayersTree.Interactions[InteractionType.KeyDelete] = new KeyEventHandler(LayersTreeDoKeyDelete);
                mLayersControl.LayersTree.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(LayersTreeDoubleClick);
                mLayersControl.LayersTree.BeforeNodeContextMenuStripShow += new BeforeNodeContextMenuStripShowEventHandler(LayersTreeBeforeShow);

                mSceneControl.Scene.Workspace = mWorkspace;//三维地图场景工作空间 关联--三维
                mSceneControl.AllowDrop = false;

                // Layer3DsTree保存Delete按键默认操作并自定义操作--三维
                mLayersControl.Scene = mSceneControl.Scene;
                mLayer3DsTreeDoKeyDelete = mLayersControl.Layer3DsTree.Interactions[InteractionType.KeyDelete] as KeyEventHandler;
                mLayersControl.Layer3DsTree.Interactions[InteractionType.KeyDelete] = new KeyEventHandler(Layer3DsTreeDoKeyDelete);
                mLayersControl.Layer3DsTree.BeforeNodeContextMenuStripShow += new BeforeNodeContextMenuStripShowEventHandler(Layer3DsTreeBeforeShow);

                //创建三维图层设置参数--三维
                mLayer3DSettingGrid = new Layer3DSettingGrid();
                mLayer3DSettingImage = new Layer3DSettingImage(); ;
                mLayer3DSettingVector = new Layer3DSettingVector();
                mLayer3DSettingVector.Style.LineColor = Color.GreenYellow;
                mLayer3DSettingVector.Style.LineWidth = 0.2;

                mPoint3DFields = new List<String>();
                mLine3DFields = new List<String>();
                mNetworkDatasets = new List<String>();
                mFacilityAnalyst = new FacilityAnalyst3D();
                mFacilityAnalystSetting = new FacilityAnalystSetting3D();
                mFacilityAnalystSetting.EdgeIDField = "SMEDGEID";
                mFacilityAnalystSetting.NodeIDField = "SMNODEID";
                mFacilityAnalystSetting.FNodeIDField = "SMFNODE";//起始点
                mFacilityAnalystSetting.TNodeIDField = "SMTNODE";//终点

                WeightFieldInfo3D weightFieldInfo = new WeightFieldInfo3D();
                weightFieldInfo.Name = "Length";
                weightFieldInfo.FTWeightField = "SMLENGTH";
                weightFieldInfo.TFWeightField = "SMLENGTH";
                WeightFieldInfos3D weightFieldInfos = new WeightFieldInfos3D();
                weightFieldInfos.Add(weightFieldInfo);
                mFacilityAnalystSetting.WeightFieldInfos = weightFieldInfos;

                //匹配管点符号
                mTheme3D = new Theme3DCustom();
                mTheme3D.AltitudeModeExpression = "AltitudeMode";
                mTheme3D.MarkerSymbolIDExpression = "SymbolID";
                mTheme3D.Marker3DScaleXExpression = "ScaleX";
                mTheme3D.Marker3DScaleYExpression = "ScaleY";
                mTheme3D.Marker3DScaleZExpression = "ScaleZ";
                mTheme3D.Marker3DRotateXExpression = "RotationX";
                mTheme3D.Marker3DRotateYExpression = "RotationY";
                mTheme3D.Marker3DRotateZExpression = "RotationZ";

                mSettingLine = new Layer3DSettingVector();
                mSettingLine.Style.AltitudeMode = AltitudeMode.RelativeToUnderground;
                mSettingLine.Style.LineWidth = 0.3;

                mPoint3DFields = new List<String>();
                mLine3DFields = new List<String>();
                mNetworkDatasets = new List<String>();
                mDatasource = mWorkspace.Datasources[0];
             //   string name = mWorkspace.Datasources[0].Alias;
                mRodeNetwork = mDatasource.Datasets["RodeNetWork2D"] as DatasetVector;
                mSceneControl.Scene.Workspace = mWorkspace;

                // add by dlfan
                // 不激活鼠标点击事件,数据加载不出
                mSceneControl.DoMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 300, 200, 0));
                mSceneControl.Scene.Close();
                mSceneControl.Scene.Open(mWorkspace.Scenes[0]);
                MessageBox.Show("工作空间加载成功");
                
                //m_sceneControl.Scene.Close();
                //m_sceneControl.Scene.Open("桂林理工大学地上建筑");
                //m_sceneControl.Scene.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                MessageBox.Show("工作空间加载失败！");
            }
        }
        #region 初始化操作
        /// <summary>
        /// 数据库方式打开工作空间
        /// </summary>
        public void OpenWorkSpace()
        {
            try
            {                    
                #region 文件型工作空间打开方式
                //OpenFileDialog fileDialog = new OpenFileDialog();
                //fileDialog.Multiselect = true;
                //fileDialog.Title = "请选择文件";
                //fileDialog.Filter = "工作空间文件(*.smwu)|*.smwu";
                //fileDialog.FilterIndex = 0;
                //if (fileDialog.ShowDialog() == DialogResult.OK)
                //{
                //    DialogResult saveResult = DialogResult.No;
                //if (mWorkspace.IsModified)//若有改动
                //{
                //    saveResult = MessageBox.Show("当前工作空间需要关闭，是否保存？", "保存工作空间", MessageBoxButtons.YesNoCancel);
                //    //提示是否保存
                //    if (saveResult == DialogResult.Yes)//若保存
                //    {
                //        mWorkspace.Save();
                //        mWorkspace.Close();
                //    }
                //    else if (saveResult == DialogResult.No)
                //    {
                //        mWorkspace.Close();
                //    }
                //}
                //pbar = new Common.ProgressBar("加载工作空间...", Common.ProgressStyle.SingleNone);
                //pbar.Show();
                //pbar.ReportProgress("开始加载...", 2);
                //pbar.MonitorProgressRunning("加载数据...", 70, 4);
                // WorkspaceConnectionInfo conInfo = new WorkspaceConnectionInfo(fileDialog.FileName);
                //conInfo.Type = WorkspaceType.SMWU;
                // mWorkspace.Open(conInfo);
                // String m_ConInfo = ConfigurationManager.ConnectionStrings["sql"].ConnectionString;
                #endregion
                //OpenFileDialog fileDialog = new OpenFileDialog();
                //fileDialog.Multiselect = true;
                //fileDialog.Title = "请选择文件";
                //fileDialog.Filter = "数据库信息文件(*.txt)|*.txt";
                //fileDialog.FilterIndex = 1;

                //WorkspaceConnectionInfo conInfo = new WorkspaceConnectionInfo();
                //if (fileDialog.ShowDialog() == DialogResult.OK)
                //{
                //    if (fileDialog.FilterIndex == 1)
                //    {
                //        string[] ServerInfo = ReadFile(fileDialog.FileName);
                WorkspaceConnectionInfo conInfo = new WorkspaceConnectionInfo();
                conInfo.Type = WorkspaceType.SQL;
                conInfo.Driver = "SQL Server";
                //        for (int i = 0; i < ServerInfo.Length; i++)
                //        {
                //            if (ServerInfo[i] == "服务器地址")
                //            {                        
                //                conInfo.Server = ServerInfo[i + 1];
                //                i += 1;
                //            }
                //            if (ServerInfo[i] == "数据库名称")
                //            {                               
                //                conInfo.Database = ServerInfo[i + 1];
                //                i += 1;
                //            }
                //            if (ServerInfo[i] == "服务器用户名")
                //            {                               
                //                conInfo.User = ServerInfo[i + 1];
                //                i += 1;
                //            }
                //            if (ServerInfo[i] == "服务器密码")
                //            {                        
                //                conInfo.Password = ServerInfo[i + 1];
                //                i += 1;
                //            }
                //            if (ServerInfo[i] == "工作空间名称")
                //            {                           
                //                conInfo.Name = ServerInfo[i + 1];
                //                i += 1;
                //            }                           
                //        }                       
                //    }
                    //if (fileDialog.FilterIndex == 2)
                    //{                
                    //    conInfo.Type = WorkspaceType.SMWU;
                    //    // mWorkspace.Open(conInfo);
                    //}                   
              //  }                             
                conInfo.Name = "桂林理工地上建模";
                conInfo.Server = "127.0.0.1";
                conInfo.Database = "UnderPipeLine";
                conInfo.User = "sa";
                conInfo.Password = "456123";
                mWorkspace.Open(conInfo);
                //pbar.ReportProgress("数据加载完毕.", 90);
                Initialize();               
                //pbar.ReportProgress("打开工作空间完成", 100);
                //pbar.Close();                  
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private string[] ReadFile(string FileName)
        {
            StreamReader rd = new StreamReader(FileName, Encoding.Default);          
            string[] ss;
            List<string> strList = new List<string>();
            while (!rd.EndOfStream)
            {
                ss = rd.ReadLine().Trim().Split('：');                
                for (int i = 0; i < ss.Length; i++)
                {
                    strList.Add(ss[i]);
                }
            }
            ss = strList.ToArray();            
            return ss;
        }

        /// <summary>
        /// 打开工作空间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkspaceOpened(object sender, WorkspaceOpenedEventArgs e)
        {
            foreach (Datasource datasource in e.Workspace.Datasources)
            {
                datasource.Datasets.Deleting += new DatasetDeletingEventHandler(DatasetsDeleting);
            }
        }
        /// <summary>
        /// 关闭工作空间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkspaceClosing(object sender, WorkspaceClosingEventArgs e)
        {
            try
            {
                mSceneControl.Scene.Close();
                mMapControl.Map.Close();

                mSceneControl.Scene.Refresh();
                mMapControl.Map.Refresh();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 打开数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatasourcesOpened(object sender, DatasourceOpenedEventArgs e)
        {
            e.Datasource.Datasets.Deleting += new DatasetDeletingEventHandler(DatasetsDeleting);
        }
        /// <summary>
        /// 关闭数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatasourcesClosing(object sender, DatasourceClosingEventArgs e)
        {
            e.Datasource.Datasets.Deleting -= new DatasetDeletingEventHandler(DatasetsDeleting);

            if (mMapControl.Map != null)
            {
                for (Int32 i = mMapControl.Map.Layers.Count - 1; i >= 0; i--)
                {
                    if (mMapControl.Map.Layers[i].Dataset.Datasource == e.Datasource)
                    {
                        mMapControl.Map.Layers.Remove(i);
                    }
                }
                mMapControl.Map.Refresh();
            }
            if (mSceneControl.Scene != null)///----三维
            {
                for (Int32 i = mSceneControl.Scene.Layers.Count - 1; i >= 0; i--)
                {
                    String layerDataName = mSceneControl.Scene.Layers[i].DataName;

                    String[] split = layerDataName.Split(new Char[] { '@' });
                    String relatedDatasetName = split[0];
                    if (e.Datasource.Datasets.Contains(relatedDatasetName))
                    {
                        mSceneControl.Scene.Layers.Remove(layerDataName);
                    }
                }
                mSceneControl.Scene.Refresh();
            }
        }
        /// <summary>
        /// 删除数据集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatasetsDeleting(object sender, DatasetDeletingEventArgs e)
        {
            if (mMapControl.Map != null)
            {
                for (Int32 i = mMapControl.Map.Layers.Count - 1; i >= 0; i--)//i 当前图层总数-1，
                {
                    if (mMapControl.Map.Layers[i].Dataset.Name == e.DatasetName)
                    {
                        mMapControl.Map.Layers.Remove(i);
                    }
                }
                mMapControl.Map.Refresh();
            }
            if (mSceneControl.Scene != null)//-----三维
            {
                for (Int32 i = mSceneControl.Scene.Layers.Count - 1; i >= 0; i--)
                {
                    String layerDataName = mSceneControl.Scene.Layers[i].DataName;

                    String[] split = layerDataName.Split(new Char[] { '@' });
                    String relatedDatasetName = split[0];
                    if (relatedDatasetName.Equals(e.DatasetName))
                    {
                        mSceneControl.Scene.Layers.Remove(i);
                    }
                }
                mSceneControl.Scene.Refresh();
            }
        }
        /// <summary>
        /// 右键前事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkspaceTreeBeforeMenuShow(object sender, BeforeNodeContextMenuStripShowEventArgs e)
        {
            try
            {
                ToolStripMenuItem toolStripMenuItemAddData = new ToolStripMenuItem();
                toolStripMenuItemAddData.Click += new EventHandler(toolStripAddData);
                ToolStripMenuItem toolStripMenuItemAddData1 = new ToolStripMenuItem();
                toolStripMenuItemAddData1.Click += new EventHandler(toolStripAddData1);

                if (mViewIndex == 0)//当前视图类型 0为map 其它为 场景
                {
                    toolStripMenuItemAddData.Text = "添加到地图";
                    toolStripMenuItemAddData1.Text = "浏览属性表";
                }
                else
                {
                    toolStripMenuItemAddData.Text = "添加到场景";
                    toolStripMenuItemAddData1.Text = "浏览属性表";
                }
                ToolStripMenuItem toolStripMenuItemOpenMap = new ToolStripMenuItem("打开地图");//菜单项目上显示打开地图
                toolStripMenuItemOpenMap.Click += new EventHandler(toolStripAddData);//去打开东西

                ContextMenuStrip contextMenuStripWorkspaceTree = new ContextMenuStrip();//快捷菜单
                WorkspaceTreeNodeBase treeNode = e.Node as WorkspaceTreeNodeBase;//触发该事件的节点 转为工作空间树节点基类
                if ((treeNode.NodeType & WorkspaceTreeNodeDataType.Dataset) != WorkspaceTreeNodeDataType.Unknown)
                {
                    //若该节点类型以及工作空间树节点的数据集节点类型 ！=未知
                    contextMenuStripWorkspaceTree.Items.AddRange(new ToolStripItem[] { toolStripMenuItemAddData, toolStripMenuItemAddData1 });
                }
                else if (treeNode.NodeType == WorkspaceTreeNodeDataType.MapName)
                {
                    contextMenuStripWorkspaceTree.Items.AddRange(new ToolStripItem[] { toolStripMenuItemOpenMap, toolStripMenuItemAddData1 });
                }
                mWorkspaceControl.WorkspaceTree.NodeContextMenuStrips[treeNode.NodeType] = contextMenuStripWorkspaceTree;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 向地图或场景中添加数据的响应事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripAddData(object sender, EventArgs e)
        {
            AddData();
        }
        /// <summary>
        /// 查看属性表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripAddData1(object sender, EventArgs e)
        {
            WorkspaceTreeNodeBase node = mWorkspaceControl.WorkspaceTree.SelectedNode as WorkspaceTreeNodeBase;//把树节点设置为基类
            WorkspaceTreeNodeDataType type = node.NodeType;//获取节点的类型 存在type里
            if ((type & WorkspaceTreeNodeDataType.Dataset) != WorkspaceTreeNodeDataType.Unknown)//若节点类型并且数据集节点类型！=未知类型
            {
                type = WorkspaceTreeNodeDataType.Dataset;//type 就为数据集 节点类型
            }
            Dataset dataset1 = node.GetData() as Dataset;

            mQuery = new AttributeQuery(dataset1);
            mQuery.Show();
        }
        /// <summary>
        /// 工作空间空间节点双击添加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WorkspaceTreeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            AddData();
        }
        /// <summary>
        /// 添加数据
        /// </summary>
        private void AddData()
        {

            try
            {
                //工作控件树节点的基类
                WorkspaceTreeNodeBase node = mWorkspaceControl.WorkspaceTree.SelectedNode as WorkspaceTreeNodeBase;//把树节点设置为基类
                WorkspaceTreeNodeDataType type = node.NodeType;//获取节点的类型 存在type里
                if ((type & WorkspaceTreeNodeDataType.Dataset) != WorkspaceTreeNodeDataType.Unknown)//若节点类型并且数据集节点类型！=未知类型
                {
                    type = WorkspaceTreeNodeDataType.Dataset;//type 就为数据集 节点类型
                }
                switch (type)
                {
                    case WorkspaceTreeNodeDataType.Dataset://数据集节点类型
                        {
                            Dataset dataset = node.GetData() as Dataset;//获取节点所属的数据 转为 数据集类型

                            if (mLayersControl.Map != null)//若图层管理器地图 不为null
                            {
                                mLayersControl.Map.Layers.Add(dataset, true);//图层管理器关联地图的图层集合
                                //用于把一个数据集添加到此图层集合作为一个普通图层显示，即创建一个普通图层
                                mLayersControl.Map.Refresh();//刷新当前地图
                            }
                            else if (mLayersControl.Scene != null)//若图层管理器场景不为NULL 
                            {
                                if (dataset.Type == DatasetType.Grid)//若数据集类型为栅格数据集
                                {
                                    mLayersControl.Scene.Layers.Add(dataset, mLayer3DSettingGrid, true);
                                    // 向三维图层集合中添加栅格数据集类型的三维图层。
                                }
                                else if (dataset.Type == DatasetType.Image)//影响数据集
                                {
                                    mLayersControl.Scene.Layers.Add(dataset, mLayer3DSettingImage, true);
                                    // 向三维图层集合中添加影像数据集类型的三维图层。
                                }
                                else
                                {
                                    mLayersControl.Scene.Layers.Add(dataset, mLayer3DSettingVector, true);
                                    // 向三维图层集合中添加矢量数据集类型的三维图层。
                                }
                                mSceneControl.Scene.Refresh();
                            }
                        }
                        break;
                    case WorkspaceTreeNodeDataType.MapName://工作树控件节点类型为  地图节点类型
                        {
                            String mapName = node.GetData() as String;

                            if (mLayersControl.Map != null)
                            {
                                mMapControl.Map.Open(mapName);
                                mMapControl.Map.Refresh();
                            }
                            else if (mLayersControl.Scene != null && !mLayersControl.Scene.Layers.Contains(mapName))
                            {
                                mLayersControl.Scene.Layers.Add(mapName, Layer3DType.Map, true, mapName);
                                mSceneControl.Scene.Refresh();
                            }
                        }
                        break;
                    case WorkspaceTreeNodeDataType.SceneName:
                        {
                            String sceneName = node.GetData() as String;
                            if (mLayersControl.Scene != null)
                            {
                                mLayersControl.Scene.Open(sceneName);
                                mSceneControl.Scene.Refresh();
                            }
                        }
                        break;
                    case WorkspaceTreeNodeDataType.SymbolMarker:
                        {
                            SymbolLibraryDialog.ShowDialog(mWorkspace.Resources, SymbolType.Marker);
                        }
                        break;
                    case WorkspaceTreeNodeDataType.SymbolLine:
                        {
                            SymbolLibraryDialog.ShowDialog(mWorkspace.Resources, SymbolType.Line);
                        }
                        break;
                    case WorkspaceTreeNodeDataType.SymbolFill:
                        {
                            SymbolLibraryDialog.ShowDialog(mWorkspace.Resources, SymbolType.Fill);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 地图控件双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MapControlDoubleClick(object sender, EventArgs e)
        {
            try
            {
                Layer layer = mMapControl.ActiveEditableLayer;

                // 如果图层是文本图层并且图层选择集不为空，则可以弹出文本风格对话框
                if (layer != null && layer.Dataset.Type == DatasetType.Text && layer.Selection.Count != 0)
                {
                    Recordset recordset = layer.Selection.ToRecordset();
                    GeoText text = recordset.GetGeometry() as GeoText;

                    TextStyleDialog dialog = new TextStyleDialog();
                    dialog.GeoText = text;
                    dialog.MapObject = mMapControl.Map;

                    //GeoText textChange = TextStyleDialog.ShowDialog(text, false, false);
                    //点击确定对文本风格进行设置并更新显示
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        recordset.Edit();
                        recordset.SetGeometry(dialog.GeoText);
                        recordset.Update();

                        mMapControl.Map.Refresh();
                    }
                    recordset.Dispose();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// LayersTree自定义Delete按键操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayersTreeDoKeyDelete(object sender, KeyEventArgs e)
        {
            DeleteLayer();
        }
        public void DeleteLayer()
        {
            try
            {
                TreeNode[] selected = mLayersControl.LayersTree.SelectedNodes;
                if (selected.Length > 0)
                {
                    Object data = (selected[0] as TreeNodeBase).GetData();
                    if (data is ThemeUniqueItem)
                    {
                        // 删除单值专题图子项
                        LayersTreeNodeBase layerNode = selected[0].Parent as LayersTreeNodeBase;
                        Layer layer = layerNode.GetData() as Layer;
                        ThemeUnique themeUnique = layer.Theme as ThemeUnique;
                        foreach (TreeNode node in selected)
                        {
                            if (themeUnique.Remove(node.Index))
                            {
                                node.Remove();
                            }
                        }
                        mLayersControl.LayersTree.SelectedNodes = new TreeNode[] { layerNode };
                        mLayersControl.Map.Refresh();
                    }
                    else if (data is ThemeGridUniqueItem)
                    {
                        // 删除栅格单值专题图子项
                        LayersTreeNodeBase layerNode = selected[0].Parent as LayersTreeNodeBase;
                        Layer layer = layerNode.GetData() as Layer;
                        ThemeGridUnique themeGridUnique = layer.Theme as ThemeGridUnique;
                        foreach (TreeNode node in selected)
                        {
                            if (themeGridUnique.Remove(node.Index))
                            {
                                node.Remove();
                            }
                        }
                        mLayersControl.LayersTree.SelectedNodes = new TreeNode[] { layerNode };
                        mLayersControl.Map.Refresh();
                    }
                    else if (data is ThemeGraphItem)
                    {
                        // 删除统计专题图子项
                        LayersTreeNodeBase layerNode = selected[0].Parent as LayersTreeNodeBase;
                        Layer layer = layerNode.GetData() as Layer;
                        ThemeGraph themeGraph = layer.Theme as ThemeGraph;
                        foreach (TreeNode node in selected)
                        {
                            if (themeGraph.Remove(node.Index))
                            {
                                node.Remove();
                            }
                        }
                        mLayersControl.LayersTree.SelectedNodes = new TreeNode[] { layerNode };
                        mLayersControl.Map.Refresh();
                    }
                    else
                    {
                        // 执行Layer3DsTree的默认操作
                        mLayersTreeDoKeyDelete(mLayersControl.LayersTree, new KeyEventArgs(Keys.Delete));
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 图层树双击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayersTreeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            SetLayerStyle();
        }
        /// <summary>
        /// 风格设置
        /// </summary>
        private void SetLayerStyle()
        {
            try
            {
                LayersTreeNodeBase node = mLayersControl.LayersTree.SelectedNode as LayersTreeNodeBase;
                Layer layer = node.GetData() as Layer;
                if (layer != null && layer.Theme == null)
                {
                    DatasetType type = layer.Dataset.Type;
                    //根据图层类型显示对应的资源管理器控件
                    switch (type)
                    {
                        case DatasetType.Point:
                        case DatasetType.Line:
                        case DatasetType.Region:
                            {
                                LayerSettingVector setting = layer.AdditionalSetting as LayerSettingVector;
                                GeoStyle style = SymbolDialog.ShowDialog(mWorkspace.Resources, setting.Style, GetSymbolType(type));
                                if (style != null)
                                {
                                    setting.Style = style;

                                    mMapControl.Map.Refresh();
                                    mLayersControl.LayersTree.RefreshNode(node);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        ///  根据数据集类型获取符号库的类型
        /// </summary>
        /// <param name="datasetType"></param>
        /// <returns></returns>
        private SymbolType GetSymbolType(DatasetType datasetType)
        {
            SymbolType result = SymbolType.Marker;

            switch (datasetType)
            {
                case DatasetType.Line:
                    {
                        result = SymbolType.Line;
                    }
                    break;
                case DatasetType.Point:
                    {
                        result = SymbolType.Marker;
                    }
                    break;
                case DatasetType.Region:
                    {
                        result = SymbolType.Fill;
                    }
                    break;
                default:
                    break;
            }
            return result;
        }


        /// <summary>
        /// 图层树显示前事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LayersTreeBeforeShow(object sender, BeforeNodeContextMenuStripShowEventArgs e)
        {
            try
            {
                TreeNodeBase treeNode = e.Node as TreeNodeBase;
                Layer layer = treeNode.GetData() as Layer;
                if (layer != null)
                {
                    ToolStripMenuItem toolStripMenuItemLayersTreeDelete = new ToolStripMenuItem("移除");
                    toolStripMenuItemLayersTreeDelete.Click += new EventHandler(toolStripLayersTreeDelete);
                    ToolStripMenuItem toolStripMenuItemRename = new ToolStripMenuItem("重命名");
                    toolStripMenuItemRename.Click += new EventHandler(toolStripLayersRename);
                    ToolStripMenuItem toolStripMenuItemStyle = new ToolStripMenuItem("风格设置");
                    toolStripMenuItemStyle.Click += new EventHandler(toolStripStyle);
                    ToolStripMenuItem toolStripMenuItemCanEdit = new ToolStripMenuItem("可编辑");
                    toolStripMenuItemCanEdit.Tag = TreeIconTypes.Editable;
                    if (layer.IsEditable)
                    {
                        toolStripMenuItemCanEdit.CheckState = CheckState.Checked;
                    }
                    ToolStripMenuItem toolStripMenuItemCanSnap = new ToolStripMenuItem("可捕捉");
                    toolStripMenuItemCanSnap.Tag = TreeIconTypes.Snapable;
                    if (layer.IsSnapable)
                    {
                        toolStripMenuItemCanSnap.CheckState = CheckState.Checked;
                    }
                    ToolStripMenuItem toolStripMenuItemCanSee = new ToolStripMenuItem("可显示");
                    toolStripMenuItemCanSee.Tag = TreeIconTypes.Visible;
                    if (layer.IsVisible)
                    {
                        toolStripMenuItemCanSee.CheckState = CheckState.Checked;
                    }
                    else
                    {
                        toolStripMenuItemCanEdit.Enabled = false;
                        toolStripMenuItemCanSnap.Enabled = false;
                    }

                    ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                    contextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(contextMenuStrip_ItemClicked);
                    if (layer.Theme == null)
                    {
                        switch (layer.Dataset.Type)
                        {
                            case DatasetType.Point:
                            case DatasetType.Line:
                            case DatasetType.Region:
                                {
                                    contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemCanSee, toolStripMenuItemCanEdit, toolStripMenuItemCanSnap, toolStripMenuItemStyle, toolStripMenuItemLayersTreeDelete, toolStripMenuItemRename });
                                }
                                break;
                            case DatasetType.Text:
                                {
                                    contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemCanSee, toolStripMenuItemCanEdit, toolStripMenuItemLayersTreeDelete, toolStripMenuItemRename });
                                }
                                break;
                            default:
                                {
                                    contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemCanSee, toolStripMenuItemLayersTreeDelete, toolStripMenuItemRename });
                                }
                                break;
                        }
                    }
                    else if (layer.Theme != null)
                    {
                        switch (layer.Theme.Type)
                        {
                            case ThemeType.Unique:
                                {
                                    contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemCanSee, toolStripMenuItemLayersTreeDelete, toolStripMenuItemRename });
                                }
                                break;
                            case ThemeType.Label:
                                {
                                    contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemCanSee, toolStripMenuItemLayersTreeDelete });

                                }
                                break;
                            default:
                                {
                                    contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemCanSee });
                                }
                                break;
                        }
                    }
                    mLayersControl.LayersTree.NodeContextMenuStrips[LayersTreeNodeDataType.Layer] = contextMenuStrip;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 右键菜单子项单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            try
            {
                TreeNodeBase treeNode = mLayersControl.LayersTree.SelectedNode as TreeNodeBase;
                Layer layer = treeNode.GetData() as Layer;
                TreeIconTypes type = (TreeIconTypes)e.ClickedItem.Tag;
                switch (type)
                {
                    case TreeIconTypes.Editable:
                        {
                            Boolean isItemChecked = (((ToolStripMenuItem)e.ClickedItem).CheckState == CheckState.Checked);

                            ((ToolStripMenuItem)e.ClickedItem).CheckState = isItemChecked ? CheckState.Checked : CheckState.Unchecked;
                            layer.IsEditable = !isItemChecked;
                            mMapControl.Map.Refresh();
                        }
                        break;
                    case TreeIconTypes.Visible:
                        {
                            Boolean isItemChecked = (((ToolStripMenuItem)e.ClickedItem).CheckState == CheckState.Checked);

                            ((ToolStripMenuItem)e.ClickedItem).CheckState = isItemChecked ? CheckState.Checked : CheckState.Unchecked;
                            layer.IsVisible = !isItemChecked;
                            mMapControl.Map.Refresh();
                        }
                        break;
                    case TreeIconTypes.Snapable:
                        {
                            Boolean isItemChecked = (((ToolStripMenuItem)e.ClickedItem).CheckState == CheckState.Checked);

                            ((ToolStripMenuItem)e.ClickedItem).CheckState = isItemChecked ? CheckState.Checked : CheckState.Unchecked;
                            layer.IsSnapable = !isItemChecked;
                            mMapControl.Map.Refresh();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 二维图层树移除图层、专题图子项单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripLayersTreeDelete(object sender, EventArgs e)
        {
            DeleteLayer();
        }
        /// <summary>
        /// 风格设置单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripStyle(object sender, EventArgs e)
        {
            SetLayerStyle();
        }
        /// <summary>
        /// 二维图层重命名单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripLayersRename(object sender, EventArgs e)
        {
            LayersRename();
        }
        /// <summary>
        /// 二维图层重命名单击事件
        /// </summary>
        public void LayersRename()
        {
            try
            {
                TreeNode node = mLayersControl.LayersTree.SelectedNode;
                mLayersControl.LayersTree.LabelEdit = true;
                node.BeginEdit();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Layer3DsTree自定义Delete按键操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Layer3DsTreeDoKeyDelete(object sender, KeyEventArgs e)
        {
            DeleteLayer3D();
        }
        /// <summary>
        /// 删除三维图层树中的图层
        /// </summary>
        private void DeleteLayer3D()
        {
            try
            {
                TreeNode[] selected = mLayersControl.Layer3DsTree.SelectedNodes;
                if (selected.Length > 0)
                {
                    Object data = (selected[0] as TreeNodeBase).GetData();
                    if (data is Theme3DUniqueItem)
                    {
                        // 删除三维单值专题图子项
                        Layer3DsTreeNodeBase layerNode = selected[0].Parent as Layer3DsTreeNodeBase;
                        Layer3DDataset layer = layerNode.GetData() as Layer3DDataset;
                        Theme3DUnique theme3DUnique = layer.Theme as Theme3DUnique;
                        foreach (TreeNode node in selected)
                        {
                            if (theme3DUnique.Remove(node.Index))
                            {
                                node.Remove();
                            }
                        }
                        layer.UpdateData();
                        mLayersControl.Scene.Refresh();
                    }
                    else
                    {
                        // 执行Layer3DsTree的默认操作
                        mLayer3DsTreeDoKeyDelete(mLayersControl.Layer3DsTree, new KeyEventArgs(Keys.Delete));
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 三维图层树右键菜单弹出前事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Layer3DsTreeBeforeShow(object sender, BeforeNodeContextMenuStripShowEventArgs e)
        {
            try
            {
                TreeNodeBase treeNode = e.Node as TreeNodeBase;
                Layer3D layer3D = treeNode.GetData() as Layer3D;
                if (layer3D != null)
                {
                    ToolStripMenuItem toolStripMenuItemLayersTreeDelete = new ToolStripMenuItem("移除");
                    toolStripMenuItemLayersTreeDelete.Click += new EventHandler(toolStripLayer3DsTreeDelete);
                    ToolStripMenuItem toolStripMenuItemRename = new ToolStripMenuItem("重命名");
                    toolStripMenuItemRename.Click += new EventHandler(toolStripLayer3DsRename);

                    ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                    contextMenuStrip.Items.AddRange(new ToolStripItem[] { toolStripMenuItemLayersTreeDelete, toolStripMenuItemRename });
                    mLayersControl.Layer3DsTree.NodeContextMenuStrips[Layer3DsTreeNodeDataType.Layer3D] = contextMenuStrip;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 三维图层树移除图层、专题图子项单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripLayer3DsTreeDelete(object sender, EventArgs e)
        {
            DeleteLayer3D();
        }
        /// <summary>
        /// 三维图层重命名单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripLayer3DsRename(object sender, EventArgs e)
        {
            try
            {
                TreeNode node = mLayersControl.Layer3DsTree.SelectedNode;
                mLayersControl.Layer3DsTree.LabelEdit = true;
                node.BeginEdit();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        #endregion

        /// <summary>
        /// 打开管线图层
        /// </summary>   
        public void OpenPipe()
        {
            mSceneControl.Scene.Layers.Clear();
            mPoint3Ds = new Point3Ds();
            mDatasetLine3D = mDatasource.Datasets["PipeLine3D"] as DatasetVector;
            mDatasetPoint3D = mDatasource.Datasets["PipePoint3D"] as DatasetVector;
            try
            {
                mSceneControl.Scene.Layers.Clear();
                mSceneControl.Scene.Underground.IsVisible = true;
                mSceneControl.Scene.GlobalImage.Transparency = 50;
                Layer3Ds layer3ds = mSceneControl.Scene.Layers;

                DatasetVector pipePoint = mWorkspace.Datasources["Pipe3D"].Datasets["PipePoint3D"] as DatasetVector;

                Layer3DSettingVector settingPoint = new Layer3DSettingVector();
                settingPoint.Style.AltitudeMode = AltitudeMode.RelativeToUnderground;
                settingPoint.BottomAltitudeField = "BottomAltitude";
                DatasetVector pipeLine = mWorkspace.Datasources["Pipe3D"].Datasets["PipeLine3D"] as DatasetVector;

                Layer3DSettingVector setting = new Layer3DSettingVector();
                setting.Style.AltitudeMode = AltitudeMode.RelativeToUnderground;
                setting.BottomAltitudeField = "BottomAltitude";
                setting.Style.LineSymbolID = 962048;
                setting.Style.LineColor = Color.Yellow;
                setting.Style.LineWidth = 0.3;

                Layer3DDataset layerPipePoint = this.mSceneControl.Scene.Layers.Add(pipePoint, settingPoint, true, "pipePoint");
                Layer3DDataset layerPipeLine = this.mSceneControl.Scene.Layers.Add(pipeLine, setting, true, "pipeLine");

                mSceneControl.Scene.EnsureVisible(layerPipeLine.Bounds, 2);

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void DrawRegion(Point3Ds bd_point3D)
        {
            GeoRegion3D mRegion = new GeoRegion3D(bd_point3D);
            //按照模板创建三维面数据集
            Datasource datasource = mDatasource;
            mRegionDatasets = datasource.Datasets["New_Region3D"] as DatasetVector;
            String RegionName = "Region3DModel";
            if (datasource.Datasets.Contains(RegionName))
            {
                datasource.Datasets.Delete(RegionName);
            }
            DatasetVector mRegionDataset = (DatasetVector)datasource.Datasets.CreateFromTemplate(RegionName, mRegionDatasets);
            mRegionDataset.PrjCoordSys = PipeNetWork[0].PrjCoordSys;
            Recordset mRecordset = mRegionDataset.GetRecordset(false, CursorType.Dynamic);
            mRecordset.AddNew(mRegion);
            mRecordset.Update();

            Layer3DSettingVector layer3DSetting = new Layer3DSettingVector();
            GeoStyle3D style = new GeoStyle3D();
            style.FillForeColor = Color.FromArgb(40, 0, 0, 0);
            style.AltitudeMode = AltitudeMode.RelativeUnderGround;
            style.FillMode = FillMode3D.Fill;
            layer3DSetting.Style = style;

            Layer3DDataset mLayerRegion = mSceneControl.Scene.Layers.Add(mRecordset.Dataset, layer3DSetting, true);
            mLayerRegion.UpdateData();
            mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsSelectable = false;
            //mSceneControl.Scene.Layers["Region3DModel@guilinligong"].IsSelectable = false;
            // m_sceneControl.Scene.Layers["Region3DModel@guilinligong"].IsVisible = false;
            mSceneControl.Scene.Refresh();
        }

        #region 新构建管网

        Layer3DDataset[] LayerNetLines = new Layer3DDataset[15];
        Layer3DDataset[] LayerNetNodes = new Layer3DDataset[15];
        DatasetVector[] DatasetLine3D = new DatasetVector[15];
        DatasetVector[] DatasetPoint3D = new DatasetVector[15];
       public DatasetVector[] PipeNetWork = new DatasetVector[15];

        /// <summary>
        /// 属性,构建后 方便其它类获取该网络数据集---1 为给水管 2 为电力管
        /// </summary>
        public DatasetVector SupplyWaterNetWork
        { get { return PipeNetWork[1]; } }
        public DatasetVector ElectricNetWork
        {
            get { return PipeNetWork[2]; }
        }
        public Layer3DDataset SupplyWaterLines
        {
            get { return LayerNetLines[1]; }
        }
        public Layer3DDataset ElectricLayer
        {
            get { return LayerNetLines[2]; }
        }
        public Layer3DDataset SupplyWaterLayerNetLine
        { get { return LayerNetLines[1]; } }
        public Layer3DDataset SupplyWaterLayerNetNode
        { get { return LayerNetNodes[1]; } }

        /// <summary>
        /// 属性,构建后 方便其它类获取该网络数据集---0 为排水管
        /// </summary>
        public Layer3DDataset OutWaterLines
        {
            get { return LayerNetLines[0]; }
        }
        public DatasetVector OutWaterNetWork
        { get { return PipeNetWork[0]; } }

        public Layer3DDataset OutWaterLayerNetLine
        { get { return LayerNetLines[0]; } }

        public Layer3DDataset OutWaterLayerNetNode
        { get { return LayerNetNodes[0]; } }




        /// <summary>
        /// 构建管网
        /// </summary>
        /// <param name="DatasetsPointName">三维管点数据名称</param>
        /// <param name="DatasetsLineName">三维管线数据名称</param>
        /// <param name="NetWorkName">网络数据集名称</param>
        /// <param name="MathFlag">索引标识</param>
        /// <param name="SympolID">线符号ID</param>
        /// <param name="LineColor">线符号颜色</param>
        /// <param name="ShowProgress">是否显示进度条</param>
        public void BuildNet2(string DatasetsPointName, string DatasetsLineName, string NetWorkName, int MathFlag, int SympolID, Color LineColor, bool showProgress = true)
        {

            // MathFlag 0:排水管  1:给水管  2:电力网

            DatasetLine3D[MathFlag] = mDatasource.Datasets[DatasetsLineName] as DatasetVector;
            DatasetPoint3D[MathFlag] = mDatasource.Datasets[DatasetsPointName] as DatasetVector;
            try
            {
                if (DatasetLine3D[MathFlag] == null)
                {
                    MessageBox.Show("必须指定三维线数据集!");
                    //return false;
                }

                String networkName = NetWorkName;
                if (mDatasource.Datasets.Contains(NetWorkName))
                {
                    mDatasource.Datasets.Delete(NetWorkName);
                }

                String[] lineFieldNames = GetDatasetLine3DFields2(MathFlag).ToArray();

                String[] pointFieldNames = GetDatasetPoint3DFields2(MathFlag).ToArray();


                if (showProgress)
                {
                    pbar = new Common.ProgressBar("构建地下管网...", Common.ProgressStyle.SingleNone);
                    NetworkBuilder3D.Stepped += new SteppedEventHandler(NetworkBuilder3D_Stepped);
                    pbar.Show();
                    pbar.ReportProgress("创建" + NetWorkName + "...", 1);
                }
                PipeNetWork[MathFlag] = NetworkBuilder3D.BuildNetwork(DatasetLine3D[MathFlag], DatasetPoint3D[MathFlag], lineFieldNames, pointFieldNames, mDatasource, networkName, NetworkSplitMode3D.LineSplitByPoint, 0.001);

                if (PipeNetWork[MathFlag] != null)
                {
                    mFacilityAnalystSetting.NetworkDataset = PipeNetWork[MathFlag];
                }
                // 检查网络数据集
                if (CheckNetworkDataset())
                {
                    AddLayers2(MathFlag, PipeNetWork[MathFlag], SympolID, LineColor);
                    //return true;
                }

                else
                {
                    //return false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.Message == "“SuperMap.Realspace.NetworkAnalyst.NetworkBuilder3D”的类型初始值设定项引发异常。")
                {
                    MessageBox.Show("请配置SuperMap Objects三维网络分析许可，否则无法进行三维网络分析。");
                }
                //return false;
            }
            finally
            {
                if (pbar != null)
                {
                    // 关闭进度条
                    pbar.Close();
                    pbar = null;
                }
            }
        }

        void NetworkBuilder3D_Stepped(object sender, SteppedEventArgs e)
        {
            pbar.ReportProgress(e.Message, e.Percent);
        }
        /// <summary>
        /// 添加到图层信息
        /// </summary>
        /// <param name="mathFlag">索引</param>
        /// <param name="networkDataset">网络数据集</param>
        /// <param name="SympolID">线符号ID</param>
        /// <param name="LineColor">线颜色</param>
        private void AddLayers2(int mathFlag, DatasetVector networkDataset, int SympolID, Color LineColor)
        {
            LayerNetNodes[mathFlag] = mSceneControl.Scene.Layers.Add(networkDataset.ChildDataset, mTheme3D, true);
            mSettingLine.Style.LineSymbolID = SympolID;
            mSettingLine.Style.LineColor = LineColor;
            LayerNetLines[mathFlag] = mSceneControl.Scene.Layers.Add(networkDataset, mSettingLine, true);
            mSceneControl.Scene.Refresh();
        }

        /// <summary>
        /// 获取点数据集内字段信息
        /// </summary>
        /// <returns></returns>
        private List<String> GetDatasetPoint3DFields2(int flag)
        {
            mPoint3DFields.Clear();

            for (int i = 0; i < DatasetPoint3D[flag].FieldCount; i++)
            {
                if (!isSysField(DatasetPoint3D[flag].FieldInfos[i].Name, DatasetType.Point3D))
                    mPoint3DFields.Add(DatasetPoint3D[flag].FieldInfos[i].Name);
            }
            return mPoint3DFields;
        }

        /// <summary>
        ///获取线数据集内字段信息 
        /// </summary>
        /// <returns></returns>
        private List<String> GetDatasetLine3DFields2(int flag)
        {
            mLine3DFields.Clear();

            for (int i = 0; i < DatasetLine3D[flag].FieldCount; i++)
            {
                if (!isSysField(DatasetLine3D[flag].FieldInfos[i].Name, DatasetType.Line3D))
                    mLine3DFields.Add(DatasetLine3D[flag].FieldInfos[i].Name);
            }
            return mLine3DFields;
        }

        #endregion


        #region 旧构建管网

        public void BuildNet()
        {
            try
            {
                mSceneControl.Scene.Layers.Clear();

                String networkName = "水管网";
                if (mDatasource.Datasets.Contains(networkName))
                {
                    mDatasource.Datasets.Delete(networkName);
                }

                if (mDatasetLine3D == null)
                {
                    MessageBox.Show("必须指定三维线数据集!");
                    //return false;
                }
                else
                {
                    String[] lineFieldNames = GetDatasetLine3DFields().ToArray();
                    String[] pointFieldNames = GetDatasetPoint3DFields().ToArray();

                    mPipeNet = NetworkBuilder3D.BuildNetwork(mDatasetLine3D, mDatasetPoint3D, lineFieldNames, pointFieldNames, mDatasource, networkName, NetworkSplitMode3D.LineSplitByPoint, 0.001);

                    if (mPipeNet != null)
                    {
                        mFacilityAnalystSetting.NetworkDataset = mPipeNet;

                        mFacilityAnalystSetting.EdgeIDField = "SMEDGEID";
                        mFacilityAnalystSetting.NodeIDField = "SMNODEID";
                        mFacilityAnalystSetting.FNodeIDField = "SMFNODE";
                        mFacilityAnalystSetting.TNodeIDField = "SMTNODE";

                        WeightFieldInfo3D weightFieldInfo = new WeightFieldInfo3D();
                        weightFieldInfo.Name = "Length";
                        weightFieldInfo.FTWeightField = "SMLENGTH";
                        weightFieldInfo.TFWeightField = "SMLENGTH";
                        WeightFieldInfos3D weightFieldInfos = new WeightFieldInfos3D();
                        weightFieldInfos.Add(weightFieldInfo);
                        mFacilityAnalystSetting.WeightFieldInfos = weightFieldInfos;

                        MessageBox.Show("三维网络数据集构建成功！");

                    }
                    // 检查网络数据集
                    if (CheckNetworkDataset())
                    {
                        AddLayers(mPipeNet);
                        //return true;
                    }

                    else
                    {
                        //return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                if (ex.Message == "“SuperMap.Realspace.NetworkAnalyst.NetworkBuilder3D”的类型初始值设定项引发异常。")
                {
                    MessageBox.Show("请配置SuperMap Objects三维网络分析许可，否则无法进行三维网络分析。");
                }
                //return false;
            }
        }

        private void AddLayers(DatasetVector networkDataset)
        {
            DatasetVector dataSet_Region = mDatasource.Datasets["region"] as DatasetVector;

            int[] id = { 1 };
            Recordset recordset = dataSet_Region.Query(id, CursorType.Static);
            GeoRegion georegion = recordset.GetGeometry() as GeoRegion;
            int i = mSceneControl.Scene.GlobalImage.AddExcavationRegion(georegion, "地表局部透明");

            Theme3DCustom theme3D = new Theme3DCustom();
            theme3D.AltitudeModeExpression = "AltitudeMode";
            theme3D.MarkerSymbolIDExpression = "SymbolID";
            theme3D.Marker3DScaleXExpression = "ScaleX";
            theme3D.Marker3DScaleYExpression = "ScaleY";
            theme3D.Marker3DScaleZExpression = "ScaleZ";
            theme3D.Marker3DRotateXExpression = "RotationX";
            theme3D.Marker3DRotateYExpression = "RotationY";
            theme3D.Marker3DRotateZExpression = "RotationZ";

            mLayerNetNode = mSceneControl.Scene.Layers.Add(networkDataset.ChildDataset, theme3D, true);

            Layer3DSettingVector settingLine = new Layer3DSettingVector();
            settingLine.Style.AltitudeMode = AltitudeMode.RelativeToUnderground;

            settingLine.Style.LineSymbolID = 964526;
            settingLine.Style.LineColor = Color.White;
            settingLine.Style.LineWidth = 0.3;

            mLayerNetLine = mSceneControl.Scene.Layers.Add(networkDataset, settingLine, true);

            Camera camera = new Camera(116.391105342212, 39.9912318395699, -3.499);//(116.391105342212, 39.9912318395699, -3.499)116.387747, 39.991392, 1.193054;
            camera.Heading = 1.822038;
            camera.Tilt = 65.297303;
            mSceneControl.Scene.Fly(camera, 2);
        }

        /// <summary>
        /// 获取点数据集内字段信息
        /// </summary>
        /// <returns></returns>
        private List<String> GetDatasetPoint3DFields()
        {
            mPoint3DFields.Clear();

            for (int i = 0; i < mDatasetPoint3D.FieldCount; i++)
            {
                if (!isSysField(mDatasetPoint3D.FieldInfos[i].Name, DatasetType.Point3D))
                    mPoint3DFields.Add(mDatasetPoint3D.FieldInfos[i].Name);
            }
            return mPoint3DFields;
        }

        /// <summary>
        ///获取线数据集内字段信息 
        /// </summary>
        /// <returns></returns>
        private List<String> GetDatasetLine3DFields()
        {
            mLine3DFields.Clear();

            for (int i = 0; i < mDatasetLine3D.FieldCount; i++)
            {
                if (!isSysField(mDatasetLine3D.FieldInfos[i].Name, DatasetType.Line3D))
                    mLine3DFields.Add(mDatasetLine3D.FieldInfos[i].Name);
            }
            return mLine3DFields;
        }



        #endregion


        public Boolean CheckNetworkDataset()
        {

            mFacilityAnalyst.AnalystSetting = mFacilityAnalystSetting;
            FacilityAnalystCheckResult3D result = mFacilityAnalyst.Check();

            Dictionary<Int32, Int32> arcErrorInfos = result.ArcErrorInfos;
            Dictionary<Int32, Int32> nodeErrorInfos = result.NodeErrorInfos;

            if (arcErrorInfos.Count == 0 && nodeErrorInfos.Count == 0)
            {
                return true;
            }
            else
            {
                MessageBox.Show("所构建的网络存在错误，无法进行分析！");
                return false;
            }
        }
        /// <summary>
        /// 检测是否为系统字段
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Boolean isSysField(String fieldName, DatasetType type)
        {
            bool result = false;

            try
            {
                if (type == DatasetType.Point3D)
                {
                    for (int i = 0; i < m_point3DSysField.Length; i++)
                    {
                        if (m_point3DSysField[i] == fieldName)
                        {
                            result = true;
                        }
                    }
                }
                else if (type == DatasetType.Line3D)
                {
                    for (int i = 0; i < m_Line3DSysField.Length; i++)
                    {
                        if (m_Line3DSysField[i] == fieldName)
                        {
                            result = true;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return result;
            }
        }


        #region 缓冲区分析
        /// <summary>
        /// 创建缓冲区
        /// </summary>
        public void CreateBuffer()
        {
            try
            {
                if (mHasCreated)
                {
                    mHasCreated = false;
                }
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要创建缓冲区的空间对象");
                    return;
                }
                mLineRecordset = selection[0].ToRecordset();
                //按照模板创建模型数据集
                Datasource datasource = mDatasource;
                mRegionDatasets = datasource.Datasets["New_Model"] as DatasetVector;
                String bufferName = "bufferModel";
                if (datasource.Datasets.Contains(bufferName))
                {
                    datasource.Datasets.Delete(bufferName);
                }
                mBufferDataset = (DatasetVector)datasource.Datasets.CreateFromTemplate(bufferName, mRegionDatasets);
                mBufferDataset.PrjCoordSys = mLineRecordset.Dataset.PrjCoordSys;
                mRecordset = mBufferDataset.GetRecordset(false, CursorType.Dynamic);
                //遍历选择的管线，如果有多条，则生成多个缓冲区
                while (!mLineRecordset.IsEOF)
                {
                    mGeo = mLineRecordset.GetGeometry() as Geometry3D;
                    //设置缓冲参数
                    BufferAnalyst3DParameter bufferAnalyst3DParameter = new BufferAnalyst3DParameter();
                    bufferAnalyst3DParameter.EndType = SuperMap.Realspace.SpatialAnalyst.BufferEndType.Round;
                    bufferAnalyst3DParameter.BufferDistance = Convert.ToDouble(mRadius);
                    bufferAnalyst3DParameter.BufferQuality = 20;
                    //生成缓冲区
                    Geometry3D geo3D = Geometrist3D.CreateBuffer(mGeo, bufferAnalyst3DParameter, mLineRecordset.Dataset.PrjCoordSys);
                    GeoStyle3D geoStyle3D = new GeoStyle3D();
                    mRecordset.AddNew(geo3D);
                    mLineRecordset.MoveNext();
                }
                mRecordset.Update();
                //设置数据集容量，避免空间查询出现过多对象
                mRecordset.Dataset.Tolerance.NodeSnap = 0.0002;
                PipeNetWork[0].Tolerance.NodeSnap = 0.0002;
                PipeNetWork[1].Tolerance.NodeSnap = 0.0002;
                //获取缓冲区记录集的外接矩形
                mRec = mRecordset.Bounds;
                //把缓冲区作为三维图层显示出来
                Layer3DSettingVector layer3DSetting = new Layer3DSettingVector();
                GeoStyle3D style = new GeoStyle3D();
                style.FillForeColor = Color.FromArgb(0x3B, 0xFF, 0x80, 0x40);
                style.FillBackColor = Color.GreenYellow;
                style.AltitudeMode = AltitudeMode.RelativeToGround;
                style.FillMode = FillMode3D.Fill;
                layer3DSetting.Style = style;

                mLayerBuffer = mSceneControl.Scene.Layers.Add(mRecordset.Dataset, layer3DSetting, true);
                mLayerBuffer.UpdateData();
                mSceneControl.Scene.Refresh();
                mHasCreated = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 设置缓存区的半径
        /// </summary>
        public void SetRadius(String text, Boolean isField)
        {
            try
            {
                if (String.Empty == text)
                {
                    mRadius = null;
                    return;
                }

                if (isField)
                {
                    mRadius = text;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 对生成的缓冲区进行空间查询
        /// </summary>
        public void BufferQuery()
        {
            try
            {
                if (mRecordset != null)
                {
                    this.mDatagrid.Rows.Clear();
                    this.mDatagrid.Columns.Clear();
                    Layer3Ds m_layer = mSceneControl.Scene.Layers;
                    foreach (Layer3D mlayer in m_layer)
                    {
                        if (mlayer.Selection != null)
                        {
                            mlayer.Selection.Clear();
                        }
                    }
                    //查询参数设置
                    QueryParameter para = new QueryParameter();
                    para.HasGeometry = true;
                    para.SpatialQueryMode = SpatialQueryMode.Contain;
                    para.SpatialQueryObject = mRec;
                    for (int i = 0; i < 3; i++)
                    {
                        //得到查询结果，进行高亮显示
                        Recordset recordset = PipeNetWork[i].Query(para);
                        List<Int32> ids = new List<int>(recordset.RecordCount);
                        while (!recordset.IsEOF)
                        {
                            ids.Add(recordset.GetID());
                            recordset.MoveNext();
                        }
                        LayerNetLines[i].Selection.AddRange(ids.ToArray());
                        LayerNetLines[i].Selection.UpdateData();
                        LayerNetLines[i].Selection.Style.LineColor = Color.GreenYellow;
                        mSceneControl.Scene.Refresh();

                        //在DataGriw显示属性表
                        recordset.MoveFirst();
                        mQuerygeo = recordset.GetGeometry();
                        mType = mQuerygeo.Type;
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
                                this.mDatagrid.Columns.Add(mCaption, mCaption);
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
                            this.mDatagrid.Rows.Add(dataGridViewRow);
                            recordset.MoveNext();
                        }
                        this.mDatagrid.Update();
                        recordset.Dispose();
                    }
                    mSceneControl.Action = Action3D.Pan2;
                    mSceneControl.Scene.Refresh();
                }
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 空间查询结果气泡显示
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="id"></param>
        public void DisplayBubbleQuery(Geometry3D geometry, Int32 id, String str)
        {
            mSceneControl.Bubbles.Clear();
            Bubble bubble = new Bubble();
            mInformationBubble.Visible = true;
            mInformationBubble.Description.Text = str + ":ID = " + id;

            bubble.Pointer = new Point3D(geometry.InnerPoint3D.X, geometry.InnerPoint3D.Y, geometry.InnerPoint3D.Z);
            bubble.ClientWidth = mInformationBubble.Width;
            bubble.ClientHeight = mInformationBubble.Height;

            mInformationBubble.Location = new Point(bubble.ClientLeft, bubble.ClientTop);
            mSceneControl.Bubbles.Add(bubble);
            mSceneControl.Scene.Refresh();
        }
        /// <summary>
        /// 判断点击表格的数据集是排水还是给水,进行气泡显示
        /// </summary>
        public void FlyToLine(Int32 row)
        {
            Recordset selected = null;
            try
            {
                Int32 id = Convert.ToInt32(mDatagrid[0, row].Value);
                string str = Convert.ToString(mDatagrid[1, row].Value);
                if (str == "排水管线")
                {
                    selected = PipeNetWork[0].Query("SMID = " + id, CursorType.Static);
                    Geometry3D geometry = selected.GetGeometry() as Geometry3D;
                    mSceneControl.Scene.EnsureVisible(geometry.Bounds, 10);
                    DisplayBubbleQuery(geometry, id, str);
                }
                else if (str == "给水管线")
                {
                    selected = PipeNetWork[1].Query("SMID = " + id, CursorType.Static);
                    Geometry3D geometry = selected.GetGeometry() as Geometry3D;
                    mSceneControl.Scene.EnsureVisible(geometry.Bounds, 10);
                    DisplayBubbleQuery(geometry, id, str);
                }
                else if (str == "电力管线")
                {
                    selected = PipeNetWork[2].Query("SMID = " + id, CursorType.Static);
                    Geometry3D geometry = selected.GetGeometry() as Geometry3D;
                    mSceneControl.Scene.EnsureVisible(geometry.Bounds, 10);
                    DisplayBubbleQuery(geometry, id, str);
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
        public void DeleteBuffer()
        {
            try
            {
                this.mDatagrid.Rows.Clear();
                this.mDatagrid.Columns.Clear();
                mSceneControl.Bubbles.Clear();

                for (int i = 0; i < 3; i++)
                {
                    LayerNetLines[i].Selection.Clear();
                }

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

        public void FlyTo(double Altitude, double Longitude, double Latitude, double Tilt)
        {
            Camera camera = mSceneControl.Scene.Camera;
            camera.Altitude = Altitude;
            camera.Longitude = Longitude;
            camera.Latitude = Latitude;
            camera.Tilt = Tilt;
            mSceneControl.Scene.Fly(camera, 3666);
        }
        #endregion

        #region 记载管线时构建面
        public void CreateRegion()
        {
            try
            {
                string str = "";
                object obj;
                mPoint3Ds = new Point3Ds();

                Recordset recordset = DatasetLine3D[0].GetRecordset(false, CursorType.Static);
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "BottomAltitude")
                    {
                        obj = recordset.GetFieldValue(i);
                        mAltitude = Convert.ToDouble(obj);
                    }
                }
                //创建地下空间对象
                mTempPoint = new Point3D[4];
                mTempPoint[0] = new Point3D(LayerNetLines[0].Bounds.Left, LayerNetLines[0].Bounds.Top, mAltitude);
                mTempPoint[1] = new Point3D(LayerNetLines[0].Bounds.Right, LayerNetLines[0].Bounds.Top, mAltitude);
                mTempPoint[2] = new Point3D(LayerNetLines[0].Bounds.Right, LayerNetLines[0].Bounds.Bottom, mAltitude);
                mTempPoint[3] = new Point3D(LayerNetLines[0].Bounds.Left, LayerNetLines[0].Bounds.Bottom, mAltitude);

                for (int i = 0; i < 4; i++)
                {
                    int id = mPoint3Ds.Add(mTempPoint[i]);
                }
                this.DrawRegion(mPoint3Ds);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }
        #endregion

        #region 修改网络数据集的别名与管线数据集相同
        /// <summary>
        /// 修改网络数据集的别名与管线数据集相同
        /// </summary>
        /// <param name="index"></param>
        public void ChangeCaption(int index)
        {
            //修改线网络数据集的别名
            for (int i = 0; i < PipeNetWork[index].FieldCount; i++)
            {
                for (int j = 0; j < DatasetLine3D[index].FieldCount; j++)
                {
                    if (PipeNetWork[index].FieldInfos[i].Name == DatasetLine3D[index].FieldInfos[j].Name)
                    {
                        PipeNetWork[index].FieldInfos[i].Caption = DatasetLine3D[index].FieldInfos[j].Caption;
                        break;
                    }
                }
            }
            //修改点网络数据集的别名
            for (int i = 0; i < PipeNetWork[index].ChildDataset.FieldCount; i++)
            {
                for (int j = 0; j < DatasetPoint3D[index].FieldCount; j++)
                {
                    if (PipeNetWork[index].ChildDataset.FieldInfos[i].Name == DatasetPoint3D[index].FieldInfos[j].Name)
                    {
                        PipeNetWork[index].ChildDataset.FieldInfos[i].Caption = DatasetPoint3D[index].FieldInfos[j].Caption;
                        break;
                    }
                }
            }
        }
        #endregion
    }
}

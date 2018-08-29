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
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System.Threading;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： FrmMain
    ** 描述： 程序主界面
    ** 作者： PLWhite
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：PLWhite
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    public partial class FrmMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region 变量声明
        int mOpencount = 0;
        private SceneControl mSceneControl;
        private Workspace mWorkspace;
        private SpaceAnalysis mSpaceAnalys;
        private WorkspaceControl mWorkspaceControl;
        private LayersControl mLayersControl;
        private MapControl mMapControl;
        private MapNavigationControl mMapNavigationControl;
        private ManualEdit mManualEdit;
        private InformationBubble mInformationBubble;
        private RodeExectionAnalysis mRodeExectionAnalysis;
        private UseData mData;
        private QuickQuery mQuickQuery;
        private SQLQuery mSQLQuery;
        private Query mQuery;
        private MeasureAnalysis mAna;
        private PureDistance mPureDis;
        private PipeNetworkAnalysis mPipeNetworkAnalysis;
        private PathAnalysis mPathAnalysis;
        private InformationLabel mInforLabel;
        private String mComboBoxValue;
        private int mIndex;
        #endregion

        public FrmMain()
        {
            //开启反走样
            SuperMap.Data.Environment.IsSceneAntialias = true;
            SuperMap.Data.Environment.SceneAntialiasValue = 8;
            InitializeComponent();

            Initialize();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //defaultLookAndFeel.LookAndFeel.SkinName = config.AppSettings.Settings["SkinName"].Value;
            PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            PnlCheckR.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            PnlBufferQuery.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            PnlSpaceQuery.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            PnlQueryRusult.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Hidden;
            btnBNet.Enabled = false;
        }
        private void Initialize()
        {
            mWorkspace = new Workspace();

            mMapControl = new MapControl();//构造一个新的地图控件
            mMapControl.Action = SuperMap.UI.Action.Select;
            mMapControl.Bounds = new Rectangle(2, 28, 542, 517);//x,y,width,high 控件（包括其非工作区元素）相对于其父控件的大小和位置
            mMapControl.Dock = DockStyle.Fill;//指针的停靠方式为边缘
            mMapControl.Map.Workspace = mWorkspace;//绑定地图控件的工作空间
            TabControl_1.TabPages[0].Controls.Add(mMapControl);

            mSceneControl = new SceneControl();//三维地图控件
            mSceneControl.Action = Action3D.Pan2;
            mSceneControl.Dock = DockStyle.Fill;
            mSceneControl.Bounds = new Rectangle(2, 28, 542, 517);
            TabControl_1.TabPages[1].Controls.Add(mSceneControl);

            mInformationBubble = new InformationBubble();
            mInformationBubble.Visible = false;
            this.mSceneControl.Controls.Add(mInformationBubble);

            mMapNavigationControl = new MapNavigationControl();//地图导航控件
            mMapNavigationControl.MapControl = mMapControl;//与bd_mapControl相关联
            mMapNavigationControl.Anchor = AnchorStyles.Top | AnchorStyles.Right;//控件锚定到上边缘和右边缘
            mMapNavigationControl.Location = new Point(mMapControl.Width - 90, 20);//左上角相对坐标

            mWorkspaceControl = new WorkspaceControl(mWorkspace);// 工作空间管理器。 
            mWorkspaceControl.Dock = DockStyle.Fill;
            LayoutsplitContainer.Panel1.Controls.Add(mWorkspaceControl);

            mLayersControl = new LayersControl();//图层管理器，用来展现和管理其所关联的地图或者场景中的图层。
            mLayersControl.Dock = DockStyle.Fill;
            LayoutsplitContainer.Panel2.Controls.Add(mLayersControl);

            InitializeBrowseTools();//初始化二维浏览工具
            InitializeBrowseTools3D();//初始化三维浏览工具
            InitComBox();  //初始化标注里的ComBoBoxEdit的Item

            mData = new UseData(mWorkspaceControl, mSceneControl, mMapControl, mLayersControl, dataGridView1, mInformationBubble);
            mQuery = new Query(mSceneControl, mData, dataGridView1);
            mSpaceAnalys = new SpaceAnalysis(mSceneControl, bd_textBoxTotalArea, bd_textBoxTotalsqure, bd_dataGridViewResult, bd_dataGridViewEdge, bd_textBoxPipeLineID, mInformationBubble, mData, mGroupBoxAccident, mGroupBoxInfluence);
            mManualEdit = new ManualEdit(mWorkspace, mMapControl);
            mPipeNetworkAnalysis = new PipeNetworkAnalysis(mSceneControl, mData, mInformationBubble);
            mPathAnalysis = new PathAnalysis(mWorkspace, mSceneControl, mData);
            mQuickQuery = new QuickQuery(mSceneControl, dataGridAttitu);
            mInforLabel = new InformationLabel(mSceneControl, mData);
            mRodeExectionAnalysis = new RodeExectionAnalysis(mSceneControl, mData);
            mData.FlyTo(10000000, 116.46, 39.92, 15);
        }

        #region 二维空间初始化
        /// <summary>
        /// 二维控件初始化
        /// </summary>
        private void InitializeBrowseTools()
        {
            try
            {
                ToolStrip toolStripBrowseTools = new ToolStrip();
                toolStripBrowseTools.GripStyle = ToolStripGripStyle.Hidden;

                //....................地图浏览控件..........................
                ToolStripButton toolStripButtonSelect = new ToolStripButton();
                ToolStripButton toolStripButtonPan = new ToolStripButton();
                ToolStripButton toolStripButtonZoomIn = new ToolStripButton();
                ToolStripButton toolStripButtonZoomOut = new ToolStripButton();
                ToolStripButton toolStripButtonZoomFree = new ToolStripButton();
                ToolStripButton toolStripButtonViewEntire = new ToolStripButton();
                ToolStripButton toolStripButtonRefresh = new ToolStripButton();


                // comboBoxDrawSelect.Items.AddRange(new string[]{ "点", "线", "面", "文本" 
                //   });
                //comboBoxDrawSelect.SelectedIndex = 0;

                //添加到toolStripBrowseTools中
                toolStripBrowseTools.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                toolStripButtonSelect,
                toolStripButtonPan,
                toolStripButtonZoomIn,
                toolStripButtonZoomOut,
                toolStripButtonZoomFree,
                toolStripButtonViewEntire,
                toolStripButtonRefresh,
                });

                //------------------地图浏览控件事件注册--------------------------//
                toolStripButtonSelect.ToolTipText = "选择";
                toolStripButtonSelect.Image = Properties.Resources.MapSelect;
                toolStripButtonSelect.Click += new EventHandler(toolStripButtonSelect_Click);

                toolStripButtonPan.ToolTipText = "漫游";
                toolStripButtonPan.Image = Properties.Resources.MapPan;
                toolStripButtonPan.Click += new EventHandler(toolStripButtonPan_Click);


                toolStripButtonZoomIn.ToolTipText = "放大";
                toolStripButtonZoomIn.Image = Properties.Resources.MapZoomIn;
                toolStripButtonZoomIn.Click += new EventHandler(toolStripButtonZoomIn_Click);

                toolStripButtonZoomOut.ToolTipText = "缩小";
                toolStripButtonZoomOut.Image = Properties.Resources.MapZoomOut;
                toolStripButtonZoomOut.Click += new EventHandler(toolStripButtonZoomOut_Click);

                toolStripButtonZoomFree.ToolTipText = "自由缩放";
                toolStripButtonZoomFree.Image = Properties.Resources.MapZoomFree;
                toolStripButtonZoomFree.Click += new EventHandler(toolStripButtonZoomFree_Click);

                toolStripButtonViewEntire.ToolTipText = "全幅显示";
                toolStripButtonViewEntire.Image = Properties.Resources.MapEntire;
                toolStripButtonViewEntire.Click += new EventHandler(toolStripButtonViewEntire_Click);

                toolStripButtonRefresh.ToolTipText = "刷新";
                toolStripButtonRefresh.Image = Properties.Resources.MapRefresh;
                toolStripButtonRefresh.Click += new EventHandler(toolStripButtonRefresh_Click);

                toolStripButtonRefresh.ToolTipText = "关闭";
                toolStripButtonRefresh.Image = Properties.Resources.MapClose;
                toolStripButtonRefresh.Click += new EventHandler(toolStripButtonClose_Click);

                TabControl_1.TabPages[0].Controls.Add(toolStripBrowseTools);//把toolStripBrowseTools加入到第一个选项卡集合中

                toolStripBrowseTools.SendToBack();//把控件发送到Z顺序后
                toolStripBrowseTools.Dock = DockStyle.Top;//停靠顶端
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 二维控件上的编辑事件
        /// <summary>
        /// 选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonSelect_Click(object sender, EventArgs e)
        {
            mMapControl.Action = SuperMap.UI.Action.Select;
        }

        /// <summary>
        /// 漫游事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonPan_Click(object sender, EventArgs e)
        {
            mMapControl.Action = SuperMap.UI.Action.Pan;
        }

        /// <summary>
        /// 放大
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonZoomIn_Click(object sender, EventArgs e)
        {
            mMapControl.Action = SuperMap.UI.Action.ZoomIn;
        }
        /// <summary>
        /// 缩小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
        {
            mMapControl.Action = SuperMap.UI.Action.ZoomOut;
        }
        /// <summary>
        /// 自由缩放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonZoomFree_Click(object sender, EventArgs e)
        {
            mMapControl.Action = SuperMap.UI.Action.ZoomFree;
        }
        /// <summary>
        /// 全幅显示地图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonViewEntire_Click(object sender, EventArgs e)
        {
            mMapControl.Map.ViewEntire();

        }
        /// <summary>
        /// 刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            mMapControl.Map.Refresh();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            mMapControl.Map.Close();
        }
        #endregion

        #region 三维控件初始化
        /// <summary>
        /// 三维控件初始化
        /// </summary>
        private void InitializeBrowseTools3D()
        {
            ToolStrip toolStripBrowseTools3D = new ToolStrip();
            toolStripBrowseTools3D.GripStyle = ToolStripGripStyle.Hidden;

            ToolStripButton toolStripButtonSelect3D = new ToolStripButton();
            ToolStripButton toolStripButtonPan3D = new ToolStripButton();
            ToolStripButton toolStripButtonRefresh3D = new ToolStripButton();
            ToolStripButton toolStripButtonViewEntire3D = new ToolStripButton();

            toolStripBrowseTools3D.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                toolStripButtonSelect3D,
                toolStripButtonPan3D,
                toolStripButtonViewEntire3D,
                toolStripButtonRefresh3D});

            toolStripButtonSelect3D.ToolTipText = "选择";
            toolStripButtonSelect3D.Image = Properties.Resources.SceneSelect;
            toolStripButtonSelect3D.Click += new EventHandler(bd_toolStripViewTools3D_Click);

            toolStripButtonPan3D.ToolTipText = "选择漫游";
            toolStripButtonPan3D.Image = Properties.Resources.ScenePan;
            toolStripButtonPan3D.Click += new EventHandler(toolStripButtonPan3D_Click);

            toolStripButtonViewEntire3D.ToolTipText = "全球";
            toolStripButtonViewEntire3D.Image = Properties.Resources.SceneViewEntire;
            toolStripButtonViewEntire3D.Click += new EventHandler(toolStripButtonViewEntire3D_Click);

            toolStripButtonRefresh3D.ToolTipText = "刷新";
            toolStripButtonRefresh3D.Image = Properties.Resources.SceneRefresh;
            toolStripButtonRefresh3D.Click += new EventHandler(toolStripButtonRefresh3D_Click);

            toolStripButtonRefresh3D.ToolTipText = "关闭";
            toolStripButtonRefresh3D.Image = Properties.Resources.MapClose;
            toolStripButtonRefresh3D.Click += new EventHandler(toolStripButtonClose3D_Click);

            TabControl_1.TabPages[1].Controls.Add(toolStripBrowseTools3D);
            toolStripBrowseTools3D.SendToBack();
            toolStripBrowseTools3D.Dock = DockStyle.Top;
        }
        #endregion

        #region 初始化标注里面的COMBOX控件
        public void InitComBox()
        {
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)this.barEditLabel.Edit).Items.Add("ID");
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)this.barEditLabel.Edit).Items.Add("管径");
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)this.barEditLabel.Edit).Items.Add("埋深");
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)this.barEditLabel.Edit).Items.Add("坐标");
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)this.barEditLabel.Edit).Items.Add("材质");
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)this.barEditLabel.Edit).Items.Add("权属");
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)this.barEditLabel.Edit).Items.Add("名称");
            ((DevExpress.XtraEditors.Repository.RepositoryItemComboBox)this.barEditLabel.Edit).Items.Add("流向");
        }
        /// <summary>
        /// 得到ComBox里面的值
        /// </summary>
        public void GetComBoxValue()
        {
            try
            {
                mComboBoxValue = this.barEditLabel.EditValue.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region 三维控件编辑事件
        /// <summary>
        /// 选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bd_toolStripViewTools3D_Click(object sender, EventArgs e)
        {
            mSceneControl.Action = Action3D.Select;
        }
        /// <summary>
        /// 漫游事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonPan3D_Click(object sender, EventArgs e)
        {
            mSceneControl.Action = Action3D.Pan2;
        }
        /// <summary>
        /// 全球事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonViewEntire3D_Click(object sender, EventArgs e)
        {
            mSceneControl.Scene.ViewEntire();

        }
        /// <summary>
        /// 刷新事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonRefresh3D_Click(object sender, EventArgs e)
        {
            mSceneControl.Scene.Refresh();
        }
        /// <summary>
        /// 关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButtonClose3D_Click(object sender, EventArgs e)
        {
            //bd_sceneControl.Scene.Close();
            try
            {
                mSceneControl.Scene.TrackingLayer.Clear();
                mSceneControl.Action = Action3D.Pan2;
                Layer3Ds m_layer = mSceneControl.Scene.Layers;
                foreach (Layer3D layer in m_layer)
                {
                    if (layer.Selection != null)
                    {
                        layer.Selection.Clear();
                    }
                }
                if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
                {
                    mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// 打开工作空间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLWSpace_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (mSceneControl.Controls.Count == mOpencount)
            {                
                //this.mData = new UseData(mWorkspaceControl, mSceneControl, mMapControl, mLayersControl, dataGridView1, mInformationBubble);               
                this.mData.OpenWorkSpace();
                this.mSceneControl.Scene.Refresh();
                mOpencount += 2;
            }
            else
            {
                this.mSceneControl.Action = Action3D.Pan2;
                this.mWorkspace.Close();
                this.mData.OpenWorkSpace();
                //this.mSceneControl.Scene.ViewEntire();
                this.mSceneControl.Scene.Refresh();
                if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
                {
                    mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = false;
                }
            }
            btnBNet.Enabled = true;
            //mData.OpenPipe();
            //mData.BuildNet();
        }
        /// <summary>
        /// 打开图层管理窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLManger_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏
            if (PanelControlValve.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlExcavationAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRemoveRange.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRodeExection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlConnectAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlFlowAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlPathAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlBoomAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            if(PnlLayoutControl.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;

        }
        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mMapNavigationControl != null)
            {
                mMapNavigationControl.Dispose();
            }
            if (mSceneControl != null)
            {
                mInformationBubble.Dispose();
                mSceneControl.Dispose();
            }
            if (mMapControl != null)
            {
                mMapControl.Dispose();
            }
            mWorkspaceControl.Dispose();
            mLayersControl.Dispose();
            mWorkspace.Dispose();
        }

        #region  空间量测
        private void btnHDistance_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int mFlag = 8;
            mAna = new MeasureAnalysis(this.mSceneControl, this,mFlag);
            mAna.BeginMeasureDistance();
            if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
            {
                mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = true;
            }
        }

        private void btnVDistance_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int mFlag=9;
            mAna = new MeasureAnalysis(this.mSceneControl, this,mFlag);
            mAna.BeginMeasureAltitude();
            if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
            {
                mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = true;
            }
        }
        private void btnArea_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int mFlag = 10;
            mAna = new MeasureAnalysis(mSceneControl, this,mFlag);
            mAna.BeginMeasureArea();
            if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
            {
                mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = true;
            }
        }
        #endregion

       

        private void TabControl_1_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            if (TabControl_1.SelectedTabPageIndex == 0)
            {
                mLayersControl.Map = mMapControl.Map;
                mData.ViewIndex = 0;
            }
            else
            {
                mLayersControl.Scene = mSceneControl.Scene;
                mData.ViewIndex = 1;
            }
        }

        #region 点 线 面 文本编辑方式
        /// <summary>
        /// 点按钮编辑方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barpoint_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            buttonPoint.Enabled = true;

            buttonLine.Enabled = false;
            button1PolyLine.Enabled = false;
            buttonFreeLine.Enabled = false;
            buttonCurve.Enabled = false;
            buttonParallel.Enabled = false;

            buttonPolygon.Enabled = false;
            buttonRectangle.Enabled = false;
            buttonRoundRectangle.Enabled = false;
            button3pCircle.Enabled = false;
            buttonParallelogram.Enabled = false;

            buttonText.Enabled = false;
            buttonLineText.Enabled = false;

            buttonVertexAdd.Enabled = false;
            buttonVertexEdit.Enabled = false;
        }

        /// <summary>
        /// 线按钮编辑方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barCheckItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            buttonPoint.Enabled = false;
            //线 true
            buttonLine.Enabled = true;
            button1PolyLine.Enabled = true;
            buttonFreeLine.Enabled = true;
            buttonCurve.Enabled = true;
            buttonParallel.Enabled = true;
            //画多边形  true
            buttonPolygon.Enabled = true;
            buttonRectangle.Enabled = true;
            buttonRoundRectangle.Enabled = true;
            button3pCircle.Enabled = true;
            buttonParallelogram.Enabled = true;
            //文本 FALSE
            buttonText.Enabled = false;
            buttonLineText.Enabled = false;
            //添加节点 true
            buttonVertexAdd.Enabled = true;
            buttonVertexEdit.Enabled = true;

            mManualEdit.DrawLine();//调方法 画线
        }

        /// <summary>
        /// 多边形按钮编辑方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barCheckItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            buttonPoint.Enabled = false;

            buttonLine.Enabled = false;
            button1PolyLine.Enabled = false;
            buttonFreeLine.Enabled = false;
            buttonCurve.Enabled = false;
            buttonParallel.Enabled = false;
            //线
            buttonPolygon.Enabled = true;
            buttonRectangle.Enabled = true;
            buttonRoundRectangle.Enabled = true;
            button3pCircle.Enabled = true;
            buttonParallelogram.Enabled = true;

            buttonText.Enabled = false;
            buttonLineText.Enabled = false;
            //节点
            buttonVertexAdd.Enabled = true;
            buttonVertexEdit.Enabled = true;

            mManualEdit.DrawRegion();//画多边形
        }

        /// <summary>
        /// 文本按钮编辑方式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barCheckItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            buttonPoint.Enabled = false;

            buttonLine.Enabled = false;
            button1PolyLine.Enabled = false;
            buttonFreeLine.Enabled = false;
            buttonCurve.Enabled = false;
            buttonParallel.Enabled = false;

            buttonPolygon.Enabled = false;
            buttonRectangle.Enabled = false;
            buttonRoundRectangle.Enabled = false;
            button3pCircle.Enabled = false;
            buttonParallelogram.Enabled = false;
            //文本可见
            buttonText.Enabled = true;
            buttonLineText.Enabled = true;

            buttonVertexAdd.Enabled = false;
            buttonVertexEdit.Enabled = false;
            mManualEdit.DrawText();//画文本
        }
        #endregion

        #region 编辑事件按钮
        private void buttonPoint_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreatePoint);
        }
        private void buttonLine_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateLine);
        }

        private void button1PolyLine_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreatePolyline);
        }

        private void buttonFreeLine_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateFreePolyline);
        }

        private void buttonCurve_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateCurve);
        }

        private void buttonParallel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateParallel);
        }

        private void buttonPolygon_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreatePolygon);
        }

        private void buttonRectangle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateRectangle);
        }

        private void buttonRoundRectangle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateRoundRectangle);
        }

        private void buttonParallelograbd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateCircle3P);
        }

        private void button3pCircle_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateParallelogram);
        }

        private void buttonText_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateText);
        }

        private void buttonLineText_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.CreateAlongLineText);
        }

        private void buttonVertexAdd_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.VertexAdd);//添加节点
        }


        private void buttonVertexEdit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mManualEdit.SetAction(SuperMap.UI.Action.VertexEdit);//编辑节点
            mMapControl.Map.Refresh();
        }
        #endregion

        private void buttonParallelogram_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        #region  开挖分析
        private void bd_buttonAddExcavationRegion_Click(object sender, EventArgs e)
        {
            try
            {
                if (bd_textBoxDeep.Text != "")
                {
                    mSpaceAnalys.RemoveExcavationRegion();
                    mSpaceAnalys.AddExcavationRegion(bd_textBoxDeep.Text);
                    if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
                    {
                        mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = false;
                    }
                    bd_buttonShowRegion.Enabled = true;
                }
                else
                {
                    MessageBox.Show("请输入底部高程");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void bd_buttonRemoveExcavationRegion_Click(object sender, EventArgs e)
        {
            try
            {
                mSpaceAnalys.RemoveExcavationRegion();
                bd_textBoxDeep.Text = "";
                bd_textBoxTotalArea.Text = "";
                bd_textBoxTotalsqure.Text = "";
                if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
                {
                    mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = true;
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void bd_buttonShowRegion_Click(object sender, EventArgs e)
        {
            try
            {
                if (bd_textBoxDeep.Text != "")
                {
                    mSpaceAnalys.ShowTarget();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void btnDig_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏
            if (PanelControlValve.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;           
            else if (pnlRemoveRange.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRodeExection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlConnectAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlFlowAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlPathAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlBoomAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlLayoutControl.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            if (PnlExcavationAnalysis.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
        }
        #endregion

        #region 打开缓冲查询窗口
        private void btnCArea_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mSceneControl.Action = Action3D.Select;
            PnlBufferQuery.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            PnlQueryRusult.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }
        #endregion

        #region 创建缓冲区
        private void butCreate_Click(object sender, EventArgs e)
        {
            mData.CreateBuffer();
        }
        #endregion

        #region 设置缓冲区半径
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Boolean isField = false;
            if (textBox1.Text.Length != 0)
            {
                isField = true;
            }
            mData.SetRadius(textBox1.Text, isField);
        }
        #endregion

        #region 生成的缓冲区进行查询
        private void butQuery_Click(object sender, EventArgs e)
        {
            mData.BufferQuery();
        }
        #endregion

        #region 清除缓冲区
        private void butClear_Click(object sender, EventArgs e)
        {
            this.textBox1.Clear();
            mData.DeleteBuffer();
        }
        #endregion

        #region 打开空间查询窗口
        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            PnlSpaceQuery.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            PnlQueryRusult.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
        }
        #endregion

        #region 爆管分析
        int mBoomSelectFlag = 0;
        private void bd_buttonSelectLine_Click(object sender, EventArgs e)
        {
            try
            {
                if (mBoomSelectFlag % 2 == 0)
                {
                    mSceneControl.Action = Action3D.Select;
                    mSpaceAnalys.SelectPipeLine();

                    if (mSceneControl.Scene.Layers.Contains("道路断线_3D@127.0.0.1_UnderPipeLine#1"))
                    {
                        mSceneControl.Scene.Layers["道路断线_3D@127.0.0.1_UnderPipeLine#1"].IsSelectable = false;
                    }
                    mBoomSelectFlag += 1;
                    bd_buttonSelectLine.Text = "取消操作";

                }
                else
                {
                    mSpaceAnalys.DeletPipeLine();
                    mSceneControl.Action = Action3D.Pan2;
                    bd_buttonSelectLine.Text = "选取管线";
                    mBoomSelectFlag += 1;
                }

            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void bd_buttonClearSelectLine_Click(object sender, EventArgs e)
        {
            try
            {
                mSpaceAnalys.Clear();
                mSpaceAnalys.ClearBoomBuffer();
                bd_buttonSelectLine.Enabled = true;
                mBtnControlBoom.Enabled = false;
                mBtnAccident.Enabled = false;
                mGroupBoxAccident.Enabled = false;
                mGroupBoxInfluence.Enabled = false;
                mBoomSelectFlag = 0;
                bd_buttonSelectLine.Text = "选取管线";
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void bd_dataGridViewResult_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                mSpaceAnalys.FlyToValve(e.RowIndex);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void bd_dataGridViewEdge_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            try
            {
                mSpaceAnalys.FlyToPipe(e.RowIndex);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void btnBPoint_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏            
            if (PanelControlValve.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlExcavationAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility =DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRemoveRange.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRodeExection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlConnectAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlFlowAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlPathAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlLayoutControl.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            if (PnlBoomAnalysis.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
        }

        private void mBtnControlBoom_Click(object sender, EventArgs e)
        {
            try
            {
                mSpaceAnalys.LoadModel();
                mSpaceAnalys.SetParameter(1);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void mBtnSetstrat_Click(object sender, EventArgs e)
        {
            try
            {
                mSpaceAnalys.FindStartPoint();
                mBtnControlBoom.Enabled = true;
                mBtnAccident.Enabled = true;
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void mBtnAccident_Click(object sender, EventArgs e)
        {
            try
            {
                mSpaceAnalys.LoadModel();
                mSpaceAnalys.SetParameter(2);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }



        private void mBtnAreaAnalysisBoom_Click(object sender, EventArgs e)
        {
            try
            {
                mSpaceAnalys.BoomCreateBuffer();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void mBtnAreaClearBoom_Click(object sender, EventArgs e)
        {
            try
            {
                mSpaceAnalys.ClearBoomBuffer();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        #endregion

        private void btnHSection_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            // mSectionAnalys = new SectionAnalysis(mSceneControl, this,mData);
            //  mSectionAnalys.BeginSection();          
        }

        #region 阀门控制
        private void mButtonSelectValve_Click(object sender, EventArgs e)
        {
            try
            {
                mSpaceAnalys.SelectLine();

            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mButtonAnalysisValva_Click(object sender, EventArgs e)
        {
            try
            {
                if (mSpaceAnalys.ControlObjectFlag == true)
                {
                    mSpaceAnalys.AnalysisLine();
                    mTextBoxControl.Text = mSpaceAnalys.InfluencePipe.ToString();
                    mTextBoxLon.Text = mSpaceAnalys.mAnalongitude.ToString();
                    mTextBoxLat.Text = mSpaceAnalys.mAnalatitude.ToString();
                }
                else
                {
                    MessageBox.Show("请选取分析管线");
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void mButtonlocateValva_Click(object sender, EventArgs e)
        {
            try
            {
                if (mSpaceAnalys.ControlObjectFlag == true)
                {
                    mSpaceAnalys.FlyToControlPipe(mSpaceAnalys.ControlPipeType);
                }
                else
                {
                    MessageBox.Show("请选取分析管线");
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void mButtonClearValva_Click(object sender, EventArgs e)
        {
            try
            {
                if (mSpaceAnalys.ControlObjectFlag == true)
                {
                    mSpaceAnalys.ControlClear();
                    mTextBoxControl.Text = "";
                    mTextBoxLon.Text = "";
                    mTextBoxLat.Text = "";
                }
                else
                {
                    MessageBox.Show("无信息可清除");
                }

            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void btnValve_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏        
            if (PnlExcavationAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRemoveRange.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRodeExection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlConnectAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlFlowAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlPathAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlBoomAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlLayoutControl.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            if (PanelControlValve.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            }         
        }
        #endregion

        #region 显示全幅区域、清除图层和属性表
        private void butShow_Click(object sender, EventArgs e)
        {
            mQuery.TargetPoint();
        }

        private void but_Clearing_Click(object sender, EventArgs e)
        {
            mQuery.ClearTrack();
        }
        #endregion

        #region 矩形空间查询区域事件
        private void butEllipse_Click(object sender, EventArgs e)
        {
            mQuery.AreaQueryEvent();
        }
        #endregion

        #region 圆形区域空间查询
        private void button1_Click(object sender, EventArgs e)
        {
            mSceneControl.Action = Action3D.Select;
            mQuery.CircleEvent();
        }
        #endregion

        #region 矩形区域空间相交、相离查询、包含查询
        private void button10_Click(object sender, EventArgs e)
        {
            if (radIntersect.Checked)
            {
                mQuery.InsertQuery();
            }
            else if (radContain.Checked)
            {
                mQuery.ContainQuery();
            }
            else if (radSepar.Checked)
            {
                mQuery.SeparQuery();
            }
        }
        #endregion

        #region 流向分析

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏
            if (PanelControlValve.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlExcavationAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRemoveRange.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRodeExection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlConnectAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;         
            else if (pnlPathAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlBoomAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlLayoutControl.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            if (PnlFlowAnalysis.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
        }

        private void mBtnFlowSelect_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.SelectPoint();
                mBtnFlow = 0;
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }
        int mBtnFlow;
        private void mBtnFlowAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                if (mBtnFlow == 0)
                {
                    mPipeNetworkAnalysis.FindSourceAndSink(mPipeNetworkAnalysis.SeachFlag);
                    mtextBoxSinkID.Text = mPipeNetworkAnalysis.SinkNode.ToString();
                    mtextBoxSinkCost.Text = mPipeNetworkAnalysis.SinkFromNodeCost.ToString("0.0000");
                    mtextBoxSourceID.Text = mPipeNetworkAnalysis.SourceNode.ToString();
                    mtextBoxSourceCost.Text = mPipeNetworkAnalysis.SourceFromNodeCost.ToString("0.0000");
                    mSceneControl.Action = Action3D.Pan;
                }
                mBtnFlow = 1;
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnFlowShowSource_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.ShowSourceEdges(mPipeNetworkAnalysis.LayerName);
                mSceneControl.Action = Action3D.Pan2;
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnFlowshowSink_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.ShowSinkEdges(mPipeNetworkAnalysis.LayerName);
                mSceneControl.Action = Action3D.Pan2;
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnFlowTraceUp_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.TraceUp(mPipeNetworkAnalysis.LayerName);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnFlowTraceDown_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.TraceDown(mPipeNetworkAnalysis.LayerName);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnFlowShowAll_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.ShowAllEdges(mPipeNetworkAnalysis.LayerName);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnFlowClear_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.FlowClearEdges(mPipeNetworkAnalysis.LayerName);
                mtextBoxSinkID.Text = "";
                mtextBoxSinkCost.Text = "";
                mtextBoxSourceID.Text = "";
                mtextBoxSourceCost.Text = "";
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }
        #endregion

        #region  流通分析
        int flag = 0;//ID号
        int BtnConnect;//控制按钮
        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏
            if (PanelControlValve.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlExcavationAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRemoveRange.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRodeExection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;            
            else if (PnlFlowAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlPathAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlBoomAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlLayoutControl.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            try
            {
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
                if (PnlConnectAnalysis.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
                    PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;

                mGroupBoxJudge.Enabled = false;
                mGroupBoxShow.Enabled = false;
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnConnectSelect_Click(object sender, EventArgs e)
        {
            try
            {
                BtnConnect = 0;
                flag += 1;
                if (mPipeNetworkAnalysis.ConNetWorkName == null)
                {
                    flag = 1;
                }
                if (flag == 1)
                {
                    MessageBox.Show("请选择第1个对象");
                    mPipeNetworkAnalysis.SelectPoint2(flag);

                }
                else if (mPipeNetworkAnalysis.IsPipe1SelectedConnect == false)
                {
                    MessageBox.Show("请选择第1个对象");
                    mPipeNetworkAnalysis.SelectPoint2(flag);
                    flag = 0;
                    mPipeNetworkAnalysis.IsPipe1SelectedConnect = true;

                }
                else
                {
                    MessageBox.Show("请选择第2个对象");
                    mPipeNetworkAnalysis.SelectPoint2(flag);

                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnConnectAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                if (BtnConnect == 0)
                {
                    while (mPipeNetworkAnalysis.PipeConId1 == 0)
                    {
                        MessageBox.Show("请选择第一个对象");
                        break;
                    }
                    while (mPipeNetworkAnalysis.PipeConId1 != 0 && mPipeNetworkAnalysis.PipeConId2 == 0)
                    {
                        MessageBox.Show("请选择第二个对象");
                        break;
                    }
                    if (mPipeNetworkAnalysis.PipeConId1 != 0 && mPipeNetworkAnalysis.PipeConId2 != 0)
                    {
                        mTextBoxID1.Text = mPipeNetworkAnalysis.PipeConId1.ToString();
                        mTextBoxID2.Text = mPipeNetworkAnalysis.PipeConId2.ToString();
                        mPipeNetworkAnalysis.ConenectJudge(mPipeNetworkAnalysis.PipeConId1);
                        if (mPipeNetworkAnalysis.ConnectFlag == 1)
                        {
                            mTextBoxResult.Text = "相互连通";
                        }
                        else if (mPipeNetworkAnalysis.ConnectFlag == 0)
                        {
                            mTextBoxResult.Text = "不连通";
                        }
                        mPipeNetworkAnalysis.ConnectShow(mPipeNetworkAnalysis.ConLayerName);
                        BtnConnect = 1;
                    }
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }


        }

        private void mBtnLocateID1_Click(object sender, EventArgs e)
        {
            try
            {
                if (mPipeNetworkAnalysis.PipeConId1 != 0)
                {
                    mPipeNetworkAnalysis.FlyToConnectID(1, mPipeNetworkAnalysis.PipeConId1);
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnLocateID2_Click(object sender, EventArgs e)
        {
            try
            {
                if (mPipeNetworkAnalysis.PipeConId2 != 0)
                {
                    mPipeNetworkAnalysis.FlyToConnectID(2, mPipeNetworkAnalysis.PipeConId2);

                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnConnectClear1_Click(object sender, EventArgs e)
        {
            try
            {
                mTextBoxID1.Text = "";
                mTextBoxID2.Text = "";
                mTextBoxResult.Text = "";
                mPipeNetworkAnalysis.ConnectFlag = 0;
                mPipeNetworkAnalysis.ConnectClearEdges(mPipeNetworkAnalysis.ConLayerName);

                mSceneControl.Bubbles.Clear();
                flag = 0;

            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnConnectSelect2_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.SelectPoint();
                BtnConFlag = true;
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }
        bool BtnConFlag = false;
        private void mBtnConnectShow_Click(object sender, EventArgs e)
        {
            try
            {
                if (BtnConFlag == true)
                {
                    mPipeNetworkAnalysis.ConnectAnalysis(mPipeNetworkAnalysis.LayerName);
                    mTextBoxID.Text = mPipeNetworkAnalysis.PipeId1.ToString();
                    BtnConFlag = false;
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mBtnConnectClear2_Click(object sender, EventArgs e)
        {
            try
            {
                mPipeNetworkAnalysis.FlowClearEdges(mPipeNetworkAnalysis.LayerName);
                mTextBoxID.Text = "";
                BtnConFlag = false;
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        private void mComboBoxAnalysis_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (mComboBoxAnalysis.SelectedIndex == 0)
                {
                    mGroupBoxJudge.Enabled = true;
                    mGroupBoxShow.Enabled = false;
                }
                else if (mComboBoxAnalysis.SelectedIndex == 1)
                {
                    mGroupBoxShow.Enabled = true;
                    mGroupBoxJudge.Enabled = false;
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }

        }

        #endregion

        #region 地形透明和地下浏览
        private void btnLView_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (this.panel2.Visible == true)
            {
                this.panel2.Visible = false;
                return;
            }
            this.panel2.Visible = true;
        }
        private void trackTran_Scroll(object sender, EventArgs e)
        {
            mSceneControl.Scene.GlobalImage.Transparency = trackTran.Value;
        }
        private void btnVUnder_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            Layer3Ds mLayer = mSceneControl.Scene.Layers;
            foreach (Layer3D layer in mLayer)
            {
                string str = layer.Name;
                if (str == "水池@127.0.0.1_UnderPipeLine" || str == "水池@guilinligong"
                    || str == "地上建筑模型@guilinligong" || str == "地上建筑模型"
                    || str == "SCHOOLIMAGE@127.0.0.1_UnderPipeLine" || str == "SCHOOLIMAGE@guilinligong" 
                    || str == "道路断线_3D@127.0.0.1_UnderPipeLine#1" ||str=="道路断线_3D@guilinligong"
                    || str == "水面@127.0.0.1_UnderPipeLine" || str == "水面@guilinligong"
                    || str == "绿树点@127.0.0.1_UnderPipeLine" || str == "绿树点@guilinligong"
                    || str == "绿地@127.0.0.1_UnderPipeLine" || str == "绿地@guilinligong")
                {
                    if (layer.IsVisible == true)
                    {
                        layer.IsVisible = false;
                    }
                    else
                    {
                        layer.IsVisible = true;
                    }
                }
            }
        }
        #endregion

        #region 构建管网
        private void barButtonItem5_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                mData.BuildNet2("排水管点_3D", "排水管线_3D", "排水管网", 0, 965565, Color.White);
                mData.ChangeCaption(0);
                mData.BuildNet2("给水管点_3D", "给水管线_3D", "给水管网", 1, 965564, Color.Red);
                mData.ChangeCaption(1);
                mData.BuildNet2("电力管点_3D", "电力管线_3D", "电力管网", 2, 965569, Color.Gray);
                mData.ChangeCaption(2);                      
                mData.CreateRegion();
                MessageBox.Show("三维网络数据集构建成功！");
                if (mSceneControl.Scene.Layers.Contains("Region3DModel@127.0.0.1_UnderPipeLine"))
                {
                    mSceneControl.Scene.Layers["Region3DModel@127.0.0.1_UnderPipeLine"].IsVisible = false;
                }
                btnBNet.Enabled = false;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
        #endregion

        #region 点击查询表格发生的事件
        private void dataGridView1_CellClick_1(object sender, DataGridViewCellEventArgs e)
        {
            mData.FlyToLine(e.RowIndex);
        }
        #endregion

        #region SQL查询
        private void btnCSpace_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            PnlQueryRusult.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            mSQLQuery = new SQLQuery(mWorkspace, mSceneControl, mData, dataGridView1);
            mSQLQuery.ShowDialog();
        }
        #endregion

        #region 属性查询
        private void btnCProperty_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mIndex = 1;
            if (PnlCheckR.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
            {
                PnlCheckR.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            }    
            mQuickQuery.AttMouseEvent(mIndex);
        }
        #endregion

        #region 权属查询
        private void btnCOwner_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mIndex = 2;
            mSceneControl.Action = Action3D.Select;
            mQuickQuery.OwnerEvent(mIndex);
        }
        #endregion

        #region 单位查询
        private void btnCDepartment_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mIndex = 3;
            mSceneControl.Action = Action3D.Select;
            mQuickQuery.UnitsEvent(mIndex);
        }
        #endregion

        #region 材质查询
        private void btnCMaterial_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mIndex = 4;
            mSceneControl.Action = Action3D.Select;
            mQuickQuery.MaterEvent(mIndex);
        }
        #endregion

        #region 测区查询
        private void btnCSurveyArea_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mIndex = 5;
            mSceneControl.Action = Action3D.Select;
            mQuickQuery.MeasureEvent(mIndex);
        }
        #endregion

        #region 管径查询
        private void btnCPipeW_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mIndex = 6;
            mSceneControl.Action = Action3D.Select;
            mQuickQuery.DiametEvent(mIndex);
        }
        #endregion

        #region 附属物查询
        private void btnCEquipment_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mIndex = 7;
            mSceneControl.Action = Action3D.Select;
            mQuickQuery.AppendEvent(mIndex);
        }
        #endregion

        #region 净距分析
        private void btnAbsHDistance_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int Pflag = 1;
            int index = 8;
            mPureDis = new PureDistance(mSceneControl, this, mData, Pflag, index);
            mPureDis.BenginSelect();
        }

        private void btnAbsVDistance_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int Pflag = 2;
            int index = 9;
            mPureDis = new PureDistance(mSceneControl, this, mData, Pflag, index);
            mPureDis.BenginSelect();
        }

        private void btnCover_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int Pflag = 3;
            int index = 10;
            mPureDis = new PureDistance(mSceneControl, this, mData, Pflag, index);
            mPureDis.BenginSelect();
        }
        private void btnCollision_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            int Pflag = 4;
            int index = 11;
            mPureDis = new PureDistance(mSceneControl, this, mData, Pflag, index);
            mPureDis.BenginSelect();
        }

        #endregion

        #region 统计功能
        private void btnSpaceCount_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            LengthStatistics mLengthStatis = new LengthStatistics(mSceneControl);
            mLengthStatis.ShowDialog();
        }
        private void btnSizeCount_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            CaliberStatist mCaliberStatis = new CaliberStatist(mSceneControl);
            mCaliberStatis.ShowDialog();
        }
        private void btnMertiralCount_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            MaterialStatist mMaterialStatist = new MaterialStatist(mSceneControl);
            mMaterialStatist.ShowDialog();
        }
        private void btnOwnCount_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            OwnerStatist mOwnerStatist = new OwnerStatist(mSceneControl);
            mOwnerStatist.ShowDialog();
        }
        #endregion

        #region 路径分析
        private void mBtnPathSelectStart_Click(object sender, EventArgs e)
        {
            try
            {
                PathFlag = true;
                if (mSceneControl.Scene.Layers.Contains("道路断线_3D@127.0.0.1_UnderPipeLine#1"))
                {
                    mSceneControl.Scene.Layers["道路断线_3D@127.0.0.1_UnderPipeLine#1"].IsSelectable = false;
                }
                mPathAnalysis.SetPoint(1);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void mBtnPathSelectEnd_Click(object sender, EventArgs e)
        {
            try
            {
                mPathAnalysis.SetPoint(2);
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void mBtnPathAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                if (PathFlag == true)
                {
                    mPathAnalysis.LoadModel();
                    mPathAnalysis.SetParameter();
                    mPathAnalysis.BeginNetworkAnalyst();
                    mSceneControl.Action = Action3D.Pan;
                    mTextPathAnalysis.Text = "当前路线为:           " + mPathAnalysis.TextBoxRodeName;
                    mTextRodeLength.Text = "当前总路程为:" + mPathAnalysis.RodeTotalLength.ToString() + "米";
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }
        bool PathFlag = false;
        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                mPathAnalysis.ClearPath();
                mTextPathAnalysis.Text = "";
                mTextRodeLength.Text = "";
                PathFlag = false;

            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏
            if (PanelControlValve.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlExcavationAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRemoveRange.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRodeExection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlConnectAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlFlowAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;            
            else if (PnlBoomAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlLayoutControl.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            if (pnlPathAnalysis.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
        }
        #endregion


        #region 信息标注
        private void benLabel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            GetComBoxValue();
            mSceneControl.Action = Action3D.Select;
            mInforLabel.registevent(mComboBoxValue);
        }
        private void btnClearLabel_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mInforLabel.ClearEvent();
        }
        private void barButtonItem10_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            mInforLabel.PullFlatEvent();
        }
        #endregion

        #region 范围拆迁
        private void barButtonItem7_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏
            if (PanelControlValve.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlExcavationAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;         
            else if (pnlRodeExection.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlConnectAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlFlowAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlPathAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlBoomAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlLayoutControl.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            if (pnlRemoveRange.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
        }

        private void mBtnRemovePoint_Click(object sender, EventArgs e)
        {
            if (mTextBoxRemoveRange.Text == "" || Convert.ToDouble(mTextBoxRemoveRange.Text) <= 0)
            {
                MessageBox.Show("请输入大于0的拆迁半径");
            }
            else
            {
                mSceneControl.Action = Action3D.Select;
                mRodeExectionAnalysis.CircleEvent(Convert.ToDouble(mTextBoxRemoveRange.Text));

            }
        }

        private void mBtnRemoveReset_Click(object sender, EventArgs e)
        {
            try
            {
                mRodeExectionAnalysis.ClearRemoveRange();
                mTextBoxRemoveRange.Text = "";
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }


        #endregion

        #region 道路扩建

        private void barButtonItem8_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            #region 判断其他侧边栏
            if (PanelControlValve.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PanelControlValve.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlExcavationAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlExcavationAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlRemoveRange.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRemoveRange.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;           
            else if (PnlConnectAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlConnectAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlFlowAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlFlowAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (pnlPathAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlPathAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlBoomAnalysis.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlBoomAnalysis.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            else if (PnlLayoutControl.Visibility == DevExpress.XtraBars.Docking.DockVisibility.Visible)
                PnlLayoutControl.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            #endregion
            pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.Visible;
            if (pnlRodeExection.Visibility != DevExpress.XtraBars.Docking.DockVisibility.Visible)
                pnlRodeExection.Visibility = DevExpress.XtraBars.Docking.DockVisibility.AutoHide;
            
            if (mSceneControl.Scene.Layers.Contains("道路断线_3D@127.0.0.1_UnderPipeLine#1"))
            {
                mSceneControl.Scene.Layers["道路断线_3D@127.0.0.1_UnderPipeLine#1"].IsSelectable = true;
            }
        }

        private void mBtnExectionAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                mRodeExectionAnalysis.DeleteBuffer();
                if (mTextBoxRodeExection.Text == "" || Convert.ToDouble(mTextBoxRodeExection.Text) <= 0)
                {
                    MessageBox.Show("请输入大于0的扩建半径");
                }
                else
                {
                    mRodeExectionAnalysis.SetRadius(Convert.ToDouble(mTextBoxRodeExection.Text));
                    mRodeExectionAnalysis.CreateBuffer();
                    mSceneControl.Action = Action3D.Pan2;
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }

        private void mBtnExectionClear_Click(object sender, EventArgs e)
        {
            try
            {
                mRodeExectionAnalysis.DeleteWhole();
                mTextBoxRodeExection.Text = "";
                if (mSceneControl.Scene.Layers.Contains("道路断线_3D@127.0.0.1_UnderPipeLine#1"))
                {
                    mSceneControl.Scene.Layers["道路断线_3D@127.0.0.1_UnderPipeLine#1"].IsSelectable = false;
                }
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex);
            }
        }



        #endregion

        #region 断面分析
        private void barButtonItem9_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            SectionAnalysis mSectionAnaly = new SectionAnalysis(mSceneControl, mData);
            mSectionAnaly.StartPosition = FormStartPosition.CenterScreen;
            mSectionAnaly.Show();
        }
        #endregion
    }
}

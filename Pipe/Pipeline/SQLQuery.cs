using System;
using System.Collections;
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
using NorthStar.Pipeline;

namespace SQL
{
    /********************************************************************************

    ** 类名称： SQLQuery
    ** 描述： 进行SQL查询
    ** 作者： FKW
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/8/14 11:38:16
    ** 版权所有 (C) :北斗星测绘地理信息研发中心
    *********************************************************************************/
    public partial class SQLQuery : Form
    {
        private String mDataSourceName;
        private TreeNode mNode;
        private TreeNode mTreeNode;
        private Workspace mWorkspace=null;
        private SceneControl mScenecontrol = null;
        private DatasetVector mData;
        private DatasetVector mData1;
        private static bool mBFilter = false;
        private static bool mBFields = true;
        private string mOpera = string.Empty;
        public static Recordset mRec = null;
        private List<int> mSelIDs = new List<int> { };
        private string mLayername = String.Empty;
        private UseData mUseData;
        private DataGridView mDataGridView;
       // private Selection3D selection_R;
       // private Selection3D selection_L;
       // private Selection3D selection_P;
        GeoStyle3D mGeostyle_R = new GeoStyle3D();
        GeoStyle3D mGeostyle_L = new GeoStyle3D();
        GeoStyle3D mGeostyle_P = new GeoStyle3D();

        /// <summary>
        /// 查询出对象的ID号
        /// </summary>
        public List<int> SelIDs
        {
            get { return mSelIDs; }
        }

        /// <summary>
        /// 选中的图层名
        /// </summary>
        public string SelLyName
        {
            get { return mLayername; }
        }

        public SQLQuery(Workspace workspace,SceneControl scenecontrol,UseData useData,DataGridView datagridview)
        {
            InitializeComponent();
            mUseData = useData;
            mDataGridView = datagridview;
            mScenecontrol = scenecontrol;
            //layer3DsTree1.Scene = scenecontrol.Scene;
            mWorkspace = workspace;
            //layer3DsTree1.SimpleMode = true;
            //移除屏幕图层和地形图层节点
          //  layer3DsTree1.TerrainLayersNode.Remove();
          //  layer3DsTree1.ScreenLayer3DNode.Remove();
          //  layer3DsTree1.Icons = TreeIconTypes.None | TreeIconTypes.TypeIcon;
          //  layer3DsTree1.MultiSelect = false;
            GetTreeNode();
            Datasources m_datasources=mScenecontrol.Scene.Workspace.Datasources;
            mDataSourceName = m_datasources[0].Alias;
        }

        #region 获取场景图层并添加到treeview节点中
        public void GetTreeNode()
        {
            Layer3Ds mLayer=mScenecontrol.Scene.Layers;
            foreach (Layer3D m_layer in mLayer)
            {
                String m_layname = m_layer.Name;
                if (m_layname.Contains("管网"))
                {
                    mTreeNode = new TreeNode(m_layname);
                    treeView1.Nodes.Add(mTreeNode);
                }
            }
            treeView1.ExpandAll();   
        }
        #endregion

        /// <summary>
        /// 填充LISTView
        /// </summary>
        #region 填充ListView
        private void InitializeLvFields()
        {
            try
            {
                //获取进行分析的数据集
                 // Layer3DsTreeNodeBase treeNode_3d = treeView1.SelectedNode as Layer3DsTreeNodeBase;
                //   String mNodeText= this.treeView1.SelectedNode.Text;         
              //if (mNodeText != null)
              //  {
                    //object obj = treeView1.SelectedNode.Tag;
                    //Layer3D layer3d_sel = obj as Layer3D;

                    //if (layer3d_sel != null)
                    //{
                        mLayername = mNode.Text;
                        string m_name = mNode.Text.Split(new char[] { '@' })[1];
                        if (m_name != mDataSourceName)
                        {
                            m_name = mDataSourceName;
                        }
                        string m_name_data = mNode.Text.Split(new char[] { '@' })[0];
                        string str1 = "_Node";

                        if (m_name_data.Contains(str1))
                        {
                            m_name_data = m_name_data.Replace(str1, "");
                            mData1 = (DatasetVector)mWorkspace.Datasources[m_name].Datasets[m_name_data];
                            mData1 = mData1.ChildDataset;
                        }
                        else
                        {
                            mData1 = (DatasetVector)mWorkspace.Datasources[m_name].Datasets[m_name_data];
                        }
                        if (mData1 != null)
                        {
                            mData = mData1;

                            this.LV_fieldinfo.Items.Clear();

                            this.LV_fieldinfo.BeginUpdate();
                            this.LV_fieldinfo.Items.Add(
                                new ListViewItem(new string[] { "*", "AllValues" }));
                            foreach (FieldInfo filedInfo in mData1.FieldInfos)
                            {
                                this.LV_fieldinfo.Items.Add(
                                    new ListViewItem(new string[] { filedInfo.Name, EnsToChsType(filedInfo.Type) }));
                            }
                            this.LV_fieldinfo.EndUpdate();
                        }
                    }
                //}
            //}
            catch (System.Exception ex)
            {
                Console.WriteLine("error:" + ex.Message + ex.StackTrace);
            }
        }
        #endregion


        #region 判断焦点位置，控制编辑
        private void TextBoxClick(Object sender, EventArgs e)
        {
            try
            {
                Control control = sender as TextBox;

                if (control != null)
                {
                    switch (control.Name)
                    {
                        case "txtQueryFields":
                            mBFields = true;
                            mBFilter = false;
                            break;
                        case "txtQueryFilter":
                            mBFilter = true;
                            mBFields = false;
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {

                Console.WriteLine("error:" + ex.Message + ex.StackTrace);
            }
        }
        #endregion

        /// <summary>
        /// 转换字段类型成文本显示
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        #region 转换字段类型
        private static string EnsToChsType(FieldType fieldType)
        {
            String fieldTypeName = String.Empty;
            try
            {
                switch (fieldType)
                {
                    case FieldType.Boolean:
                        fieldTypeName = "布尔型";
                        break;
                    case FieldType.Int32:
                        fieldTypeName = "32位整型";
                        break;
                    case FieldType.Text:
                        fieldTypeName = "文本型";
                        break;
                    case FieldType.Double:
                        fieldTypeName = "浮点型";
                        break;
                    case FieldType.Single:
                        fieldTypeName = "单精度";
                        break;
                    case FieldType.Char:
                        fieldTypeName = "字符型";
                        break;
                    case FieldType.Int16:
                        fieldTypeName = "16位整型";
                        break;
                    default:
                        fieldTypeName = fieldType.ToString();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("error:" + ex.Message);
            }
            return fieldTypeName;
        }
        #endregion

        #region 添加所有值
        private void Button_allval_Click(object sender, EventArgs e)
        {
            try
            {
                if (LV_fieldinfo.SelectedItems.Count == 1)
                {
                    LB_allval.Items.Clear();

                    string selFieldname = LV_fieldinfo.SelectedItems[0].Text;
                    if (selFieldname == "*")
                    {
                        return;
                    }
                    Recordset recordset = mData.GetRecordset(false, CursorType.Dynamic);
                    recordset.MoveFirst();
                    while (!recordset.IsEOF)
                    {
                        object allvalue = recordset.GetFieldValue(selFieldname);
                        if (allvalue == null)
                        {
                            return;
                        }
                        if (LB_allval.Items.Contains("\'" + allvalue + "\'"))
                        {
                            recordset.MoveNext();
                            continue;
                        }
                        else
                        {
                            LB_allval.Items.Add("\'"+allvalue+"\'");
                        }
                        recordset.MoveNext();
                    }
                    recordset.Dispose();
                }
                if (LV_fieldinfo.SelectedItems.Count > 1)
                {
                    MessageBox.Show("只能选择一个字段");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("error:" + ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// 填充运算符号
        /// </summary>
        #region 填充运算符号
        private void FillOperators()
        {
            ArrayList list = new ArrayList();
            list.Add(">");
            list.Add("<");
            list.Add("=");
            list.Add(">=");
            list.Add("<=");
            list.Add("<>");
            foreach (string s in list)
            {
                CBB_Operats.Text = s;
                CBB_Operats.Items.Add(s);
            }
        }
        #endregion


        /// <summary>
        /// 填充查询语句
        /// </summary>
        #region 填充查询语句
        private void FillQuereyExpress()
        {
            try
            {
                if (treeView1.SelectedNode != null)
                {
                    txtQueryExpression.Clear();

                    if (mData != null)
                    {
                        string seldtv = mData.Name;
                       // txtQueryExpression.AppendText("Select     form  " + "  " + seldtv);
                        txtQueryExpression.AppendText("Select form  " + "  " + seldtv);
                        //查询的属性
                        if (txtQueryFields.Text != null)
                        {
                            txtQueryExpression.Clear();

                            string Fields = txtQueryFields.Text;
                           // txtQueryExpression.AppendText("Select   " + Fields + "  " + "from  " + seldtv);
                            txtQueryExpression.AppendText("Select " + Fields + "  " + "from  " + seldtv);
                            //查询条件
                            if (txtQueryFilter.Text != null)
                            {
                                txtQueryExpression.Clear();

                                string Filter = txtQueryFilter.Text;
                             //   txtQueryExpression.AppendText("Select   " + Fields + "  " + "from  " + seldtv + "  " + "Where" + "  " + Filter);
                                txtQueryExpression.AppendText("Select " + Fields + "  " + "from  " + seldtv + "  " + "Where" + "  " + Filter);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("error:" + ex.Message);
            }
        }
        #endregion


        #region 选择运算符
        private void CBB_Operats_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                mOpera = CBB_Operats.Text;

                if (mBFilter)
                {
                    String selectFieldName = this.LV_fieldinfo.SelectedItems[0].Text;
                    if (this.txtQueryFilter.Text == "")
                    {
                        this.txtQueryFilter.AppendText(selectFieldName);
                    }
                    else
                    {
                        this.txtQueryFilter.Clear();
                        this.txtQueryFilter.AppendText(selectFieldName + mOpera);
                    }
                }

                FillQuereyExpress();
            }
            catch (Exception ex)
            {
                MessageBox.Show("error:" + ex.Message);
            }
        }
        #endregion


        #region 设置查询值
        private void LB_allval_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                object value = LB_allval.SelectedItem;
                String selectFieldName = this.LV_fieldinfo.SelectedItems[0].Text;

                if (this.txtQueryFilter.Text == "")
                {
                    this.txtQueryFilter.AppendText(selectFieldName);
                }
                else
                {
                    this.txtQueryFilter.Clear();
                    this.txtQueryFilter.AppendText(selectFieldName + mOpera + value);
                }

                FillQuereyExpress();
            }
            catch (Exception ex)
            {
                MessageBox.Show("error:" + ex.Message);
            }
        }
        #endregion


        #region 进行查询
        private void Button_Query_Click(object sender, EventArgs e)
        {
            try
            {
                 this.mDataGridView.Rows.Clear();
                 this.mDataGridView.Columns.Clear();
                //每次查询都要先获取图层上的选择集并清除
                Selection3D[] selection1 = mScenecontrol.Scene.FindSelection(true);
                if (selection1 != null)
                {
                    for (int i = 0; i < selection1.Length; i++)
                    {
                        selection1[i].Clear();
                    }
                }
                 //进行分析的数据集
                 DatasetVector dtvQuery = mData;

                 //设置分析参数
                 QueryParameter parameter = new QueryParameter();
                 parameter.HasGeometry = false;
                 parameter.AttributeFilter = txtQueryFilter.Text;
                 string[] resultfields=new string[dtvQuery.GetRecordset(false, CursorType.Static).GetFieldInfos().Count];
                    if (txtQueryFields.Text == "*")
                    {
                        Recordset recordset = dtvQuery.GetRecordset(false, CursorType.Static);
                        FieldInfos fieldInfos = recordset.GetFieldInfos();
                        for (int i = 0; i < fieldInfos.Count; i++)
                        {
                            resultfields[i] = fieldInfos[i].Name;
                        }
                        recordset.Dispose();
                    }
                    else if(txtQueryFields.Text.Length==0)
                    {
                        MessageBox.Show("查询字段不能为空!");
                        return;
                    } 
                    else
                    {
                        resultfields = txtQueryFields.Text.Split(new char[] { ',' });
                    }      
                   parameter.ResultFields = resultfields;
                   parameter.CursorType = CursorType.Static;
                   parameter.HasGeometry = true;
                    
                   //进行查询显示
                   mRec = dtvQuery.Query(parameter);

                   if(mRec.RecordCount != 0)
                    {
                        mRec.MoveFirst();
                       Geometry geo = mRec.GetGeometry();
                       GeometryType typ = geo.Type;

                        //进行显示,把查询结果放入到选择集中，设置查询结果的风格，在场景中高亮出来
                        int layerindex = mScenecontrol.Scene.Layers.IndexOf(mLayername);
                        Layer3DDataset layer3d = mScenecontrol.Scene.Layers[layerindex] as Layer3DDataset;
                        List<Int32> ids = new List<int>(mRec.RecordCount);
                       // Selection3D selection = layer3d.Selection;
                        while (!mRec.IsEOF)
                        {
                            ids.Add(mRec.GetID());
                            mRec.MoveNext();
                        }
                        layer3d.Selection.AddRange(ids.ToArray());
                        layer3d.Selection.Style.LineColor = Color.GreenYellow;
                        layer3d.Selection.Style.FillForeColor = Color.GreenYellow;
                        layer3d.Selection.Style.FillMode = FillMode3D.Line;
                        layer3d.Selection.UpdateData();
                        mScenecontrol.Scene.Refresh();
                    }
                    else
                    {
                        MessageBox.Show("没有查询结果！");
                        return;
                    }
                    this.Close();
                     //在DataGriw显示属性表，以表格的形式显示
                       mRec.MoveFirst();
                       FieldInfos fieldinfos = mRec.GetFieldInfos();
                        
                        foreach (FieldInfo fieldInfo in fieldinfos)
                        {
                            if(!fieldInfo.IsSystemField)
                            {
                                if (fieldInfo.Name == "SmPPoint" || fieldInfo.Name == "SmNPoint")
                                {
                                    continue;
                                }
                               // string name = fieldInfo.Name;
                                string mCaption = fieldInfo.Caption;
                                this.mDataGridView.Columns.Add(mCaption, mCaption);
                            }
                        }
                        //初始化行
                        DataGridViewRow dataGridViewRow;
                        mRec.MoveFirst();

                        //根据选中的个数将对象的信息添加到列表中
                        while (!mRec.IsEOF)
                        {
                            dataGridViewRow = new DataGridViewRow();
                            for (int a = 0; a < mRec.FieldCount; a++)
                            {
                                if (!mRec.GetFieldInfos()[a].IsSystemField)
                                {
                                    if (mRec.GetFieldInfos()[a].Name == "SmPPoint" || mRec.GetFieldInfos()[a].Name == "SmNPoint")
                                    {
                                        continue;
                                    }
                                    //定义并获取字段值
                                    object filevalue = mRec.GetFieldValue(a);

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
                            mRec.MoveNext();                          
                        }
                    this.mDataGridView.Update();
                    mScenecontrol.Action = Action3D.Pan2;
                    mScenecontrol.Scene.Refresh();
                    mRec.Dispose();
                }
            catch (Exception ex)
            {
                
            }
        }
        #endregion

        #region 清除查询
        private void Button_Clear_Click(object sender, EventArgs e)
        {
            mDataGridView.Rows.Clear();
            mDataGridView.Columns.Clear();
            mScenecontrol.Bubbles.Clear();
            txtQueryFilter.Clear();
            txtQueryFields.Clear();
            txtQueryExpression.Clear();
            LB_allval.Items.Clear();
            LV_fieldinfo.SelectedItems.Clear();
            Selection3D[] m_select = mScenecontrol.Scene.FindSelection(false);
            if (m_select != null)
            {
                for (int i = 0; i < m_select.Length; i++)
                {
                    m_select[i].Clear();
                }
            }
        }
        #endregion


         #region 关闭
        private void Button_Close_Click(object sender, EventArgs e)
        {
            txtQueryFilter.Clear();
            txtQueryFields.Clear();
            txtQueryExpression.Clear();
            //m_rec.Dispose();
            this.Close();
        }
        #endregion

        //private void layer3DsTree1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{            
        //    //填充ListView
        //    InitializeLvFields();
        //    //填充查询语句
        //    FillQuereyExpress();
        //}

        private void LV_fieldinfo_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (LV_fieldinfo.SelectedItems.Count>0)
            {
                string selFieldname = LV_fieldinfo.SelectedItems[0].Text;
                if (selFieldname != "*")
                {
                    Button_allval.Enabled = true;
                }
            }
        }

        private void txtQueryFilter_TextChanged(object sender, EventArgs e)
        {
            //填充查询表达式
            FillQuereyExpress();
        }

        #region 初始化窗体
        private void SpaceQuery_Load(object sender, EventArgs e)
        {
            try
            {
                //初始化运算符号
                FillOperators();
                //注册textbox点击事件，设置焦点位置
                txtQueryFilter.Click += TextBoxClick;
                txtQueryFields.Click += TextBoxClick;
                //设置所有值窗口不可用
                Button_allval.Enabled=false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("error:" + ex.Message);
            }
        }
        #endregion


        #region 根据焦点位置，填充查询字段，查询条件textbox
        private void LV_fieldinfo_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mBFields)
                {
                    String selectFieldName = this.LV_fieldinfo.SelectedItems[0].Text;
                    if (this.txtQueryFields.Text == "")
                    {
                        this.txtQueryFields.AppendText(selectFieldName);
                    }
                    else
                    {
                        if (!this.txtQueryFields.Text.Contains(selectFieldName))
                        {
                            this.txtQueryFields.AppendText("," + selectFieldName);
                        }
                    }
                    this.txtQueryFields.TabIndex = 1;
                    this.txtQueryFields.Focus();
                }
                else if (mBFilter)
                {
                    String selectFieldName = this.LV_fieldinfo.SelectedItems[0].Text;
                    if (selectFieldName != "*")
                    {
                        if (this.txtQueryFilter.Text == "")
                        {
                            this.txtQueryFilter.AppendText(selectFieldName);
                        }
                        else
                        {
                            this.txtQueryFilter.Clear();
                            this.txtQueryFilter.AppendText(selectFieldName + mOpera);
                        }
                    }
                    else
                    {
                        return;
                    }
                    this.txtQueryFilter.TabIndex = 1;
                    this.txtQueryFilter.Focus();
                }

                //填充查询表达式
                FillQuereyExpress();
            }
        }
        #endregion

        #region 设置选择集的样式
        private void SetStyle(DatasetType type, Selection3D selection)
        {
            switch (type)
            {
                case DatasetType.Point3D: 
                     mGeostyle_P.IsMarkerSizeFixed = false;
                     mGeostyle_P.MarkerSize = 100; 
                     mGeostyle_P.MarkerColor = Color.GreenYellow;
                     selection.Style = mGeostyle_P;
                     break;
                case DatasetType.Line3D:
                    mGeostyle_L.FillForeColor = Color.GreenYellow;
                    mGeostyle_L.FillMode = FillMode3D.Line;
                    mGeostyle_L.LineColor = Color.GreenYellow;
                    selection.Style = mGeostyle_L;
                    break;
            }
        }
        #endregion

        #region 鼠标点击节点时
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
             mNode= e.Node;
            //填充ListView
            InitializeLvFields();
            //填充查询语句
            FillQuereyExpress();
        }
        #endregion
    }
}

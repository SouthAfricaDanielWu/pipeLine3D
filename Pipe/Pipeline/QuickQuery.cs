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

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： QuickQuery
    ** 描述： 进行管径、材质等快速查询
    ** 作者： FKW
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：PLWhite
    ** 最后修改时间：2017/11/23 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    class QuickQuery
    {
        private SceneControl mSceneControl;
        private DataGridView mDataGridAtt;
        private int mIndex;
        public QuickQuery(SceneControl m_SceneControl,DataGridView m_DataGridAtt)
        {
            mSceneControl = m_SceneControl;
            mDataGridAtt = m_DataGridAtt;
            mSceneControl.MouseDown += new MouseEventHandler(mSceneMouseQuery);
        }

        #region 鼠标右键事件
        private void mSceneMouseQuery(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Right)
                {
                    switch (mIndex)
                    {
                        case 1: 
                            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);
                            mSceneControl.Action = Action3D.Pan2;
                            mIndex = 0;
                            break;
                        case 2: 
                            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);
                            mSceneControl.Action = Action3D.Pan2;
                            mIndex = 0;
                            break;
                        case 3: 
                            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);
                            mSceneControl.Action = Action3D.Pan2;
                            mIndex = 0;
                            break;
                        case 4: 
                            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);
                            mSceneControl.Action = Action3D.Pan2;
                            mIndex = 0;
                            break;
                        case 5: 
                            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);
                            mSceneControl.Action = Action3D.Pan2;
                            mIndex = 0;
                            break;
                        case 6: 
                            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);
                            mSceneControl.Action = Action3D.Pan2;
                            mIndex = 0;
                            break;
                        case 7: 
                            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);
                            mSceneControl.Action = Action3D.Pan2;
                            mIndex = 0;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                
            
            }
        }
        #endregion

        #region 属性查询 mIndex==1
        /// <summary>
        /// 属性查询事件 mIndex==1
        /// </summary>
        public void AttMouseEvent(int index)
        {
                mIndex = index;
                mSceneControl.Action = Action3D.Select;
                mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);  
                mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlQuery);
        }
        private void mSceneControlQuery(object sender, ObjectSelectedEventArgs e)
        {
            // 无对象被选中
            if (e.Count == 0)
            {
                MessageBox.Show("未选择对象!");
            }
            //有对象选中

            else if (e.Count > 0 && mIndex == 1)
            {
                AttQuery(mDataGridAtt);
            }
            else if (e.Count > 0 && mIndex == 2)
            {
                OwnerQuery();
            }
            else if (e.Count > 0 && mIndex == 3)
            {
                UnitsQuery();
            }
            else if (e.Count > 0 && mIndex == 4)
            {
                MaterQuery();
            }
            else if (e.Count > 0 && mIndex == 5)
            {
                MeasureQuery();
            }
            else if (e.Count > 0 && mIndex == 6)
            {
                DiametQuery();
            }
            else if (e.Count > 0 && mIndex == 7)
            {
                AppendQuery();
            }
        }
        public void AttQuery(DataGridView bd_DataGridAttit)
        {
            try
            {
                DataGridView mDataGridAttit = bd_DataGridAttit;
                mDataGridAttit.Columns.Clear();
                mDataGridAttit.Rows.Clear();
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                // 判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要查询属性的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();
                DatasetVector dataset = recordset.Dataset;
                mDataGridAttit.Columns.Add("", "字段");
                mDataGridAttit.Columns.Add("", "字段值");
                mDataGridAttit.Columns.Add("", "类型");
                mDataGridAttit.Columns.Add("", "长度");
                mDataGridAttit.Columns.Add("", "缺省值");

                string str = "";
                string str_1 = "";
                string str1 = "";
                string str2 = "";
                object obj;
                string str3 = "";
                string str4 = "";
                for (int i = 0; i < dataset.FieldCount; i++)
                {
                    if (!recordset.GetFieldInfos()[i].IsSystemField)
                    {
                        if (recordset.GetFieldInfos()[i].Name == "SmPPoint" || recordset.GetFieldInfos()[i].Name == "SmNPoint")
                        {
                            continue;
                        }
                            str = recordset.GetFieldInfos()[i].Name;
                            str_1 = recordset.GetFieldInfos()[i].Caption;
                            obj = recordset.GetFieldValue(i);
                            if (obj != null)
                            {
                                str1 = obj.ToString();
                            }
                            else
                            {
                                str1 = null;
                            }
                            str2 = recordset.GetFieldInfos()[i].Type.ToString();
                            str3 = recordset.GetFieldInfos()[i].MaxLength.ToString();
                            str4 = recordset.GetFieldInfos()[i].DefaultValue;

                            mDataGridAttit.Rows.Add(new[] { str_1, str1, str2, str3, str4 });                      
                    }            
                }
                mDataGridAttit.Update();
                recordset.Dispose();
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 权属查询 mIndex==2
        /// <summary>
        /// 权属查询 mIndex==2
        /// </summary>
        public void OwnerEvent(int index)
        {
           mIndex = index;
           mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);  
           mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlQuery);  
        }
        //private void mSceneControlQuery2(object sender, ObjectSelectedEventArgs e)
        //{
        //        // 无对象被选中
        //        if (e.Count == 0 && mIndex == 2)
        //        {
        //            MessageBox.Show("未选择对象!");
        //        }
        //        //有对象选中
        //        else if (e.Count > 0 && mIndex == 2)
        //        {
        //            OwnerQuery();
        //        }            
        //}
        public void OwnerQuery()
        {
                mSceneControl.Action = Action3D.Select;
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要查询权属信息的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();

                string str = "";
                string str1 = "";
                object obj;
                bool bol = false;
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "OwnerShip")
                    {
                        bol = true;
                        obj = recordset.GetFieldValue(i);
                        str1 = "权属信息" + "：" + obj.ToString() + "\n";
                        MessageBox.Show(str1, "权属信息查询结果");
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有权属信息属性！");
                }
                recordset.Dispose();
        }
        #endregion

        #region 单位查询 mIndex==3
        /// <summary>
        /// 单位查询 mIndex==3
        /// </summary>
        public void UnitsEvent(int index)
        {
            mIndex = index;
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);  
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlQuery);
        }
        //private void mSceneControlQuery3(object sender, ObjectSelectedEventArgs e)
        //{
        //    // 无对象被选中
        //    if (e.Count == 0 && mIndex == 3)
        //    {
        //        MessageBox.Show("未选择对象!");
        //    }
        //    //有对象选中
        //    else if (e.Count > 0 && mIndex == 3)
        //    {
        //        UnitsQuery();
        //    }
        //}
        public void UnitsQuery()
        {
                mSceneControl.Action = Action3D.Select;
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要查询单位信息的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();

                string str = "";
                string str1 = "";
                object obj;
                bool bol = false;
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "Units")
                    {
                        bol = true;
                        obj = recordset.GetFieldValue(i);
                        str1 = "单位" + "：" + obj.ToString() + "\n";
                        MessageBox.Show(str1, "单位查询结果");
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有单位信息属性！");
                }
                recordset.Dispose();
        }
        #endregion

        #region 材质查询 mIndex==4
        /// <summary>
        /// 材质查询 mIndex==4
        /// </summary>
        /// <param name="index"></param>
        public void MaterEvent(int index)
        {
          mIndex = index;
          mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);  
          mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlQuery);  
        }
       //private void mSceneControlQuery1(object sender, ObjectSelectedEventArgs e)
       // {
       //     // 无对象被选中
       //     if (e.Count == 0 && mIndex == 4)
       //     {
       //         MessageBox.Show("未选择对象!");
       //     }
       //     //有对象选中
       //     else if (e.Count > 0&&mIndex==4)
       //     {
       //         MaterQuery();
       //     }
       // }
        /// <summary>
        /// 材质查询
        /// </summary>
        public void MaterQuery()
        {
                mSceneControl.Action = Action3D.Select;
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要查询材质的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();

                string str = "";
                string str1 = "";
                object obj;
                bool bol = false;
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "Material")
                    {
                        bol = true;
                        obj = recordset.GetFieldValue(i);
                        str1 = "材质" + "：" + obj.ToString() + "\n";
                        MessageBox.Show(str1, "材质查询结果");
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if(bol==false)
                {
                    MessageBox.Show("该对象没有材质属性！");
                }         
                recordset.Dispose();    
        }
        #endregion

        #region 测区查询 mIndex==5
        /// <summary>
        /// 测区查询 mIndex==5
        /// </summary>
        public void MeasureEvent(int index)
        {
            mIndex = index;
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);  
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlQuery);
        }
        //private void mSceneControlQuery4(object sender, ObjectSelectedEventArgs e)
        //{
        //    // 无对象被选中
        //    if (e.Count == 0 && mIndex == 5)
        //    {
        //        MessageBox.Show("未选择对象!");
        //    }
        //    //有对象选中
        //    else if (e.Count > 0 && mIndex == 5)
        //    {
        //        MeasureQuery();
        //    }
        //}
        public void MeasureQuery()
        {
                mSceneControl.Action = Action3D.Select;
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要查询测区的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();

                string str = "";
                string str1 = "";
                object obj;
                bool bol = false;
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "MeasureArea")
                    {
                        bol = true;
                        obj = recordset.GetFieldValue(i);
                        str1 = "测区" + "：" + obj.ToString() + "\n";
                        MessageBox.Show(str1, "测区查询结果");
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有测区属性！");
                }
                recordset.Dispose();
        }
        #endregion

        #region 管径查询 mIndex==6
        /// <summary>
        /// 管径查询 mIndex==6
        /// </summary>
       public void DiametEvent(int index)
        {
            mIndex = index;
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);  
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlQuery); 
        }
       //private void mSceneControlQuery5(object sender, ObjectSelectedEventArgs e)
       //{
       //    // 无对象被选中
       //    if (e.Count == 0&& mIndex==6)
       //    {
       //        MessageBox.Show("未选择对象!");
       //    }
       //    //有对象选中
       //    else if (e.Count > 0 && mIndex == 6)
       //    {
       //        DiametQuery();
       //    }
       //}
        public void DiametQuery()
        {
                mSceneControl.Action = Action3D.Select;
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要查询管径的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();

                string str = "";
                string str1 = "";
                object obj;
                bool bol = false;
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "PipeDiameter")
                    {
                        bol = true;
                        obj = recordset.GetFieldValue(i);
                        str1 = "管径" + "：" + obj.ToString() +"mm"+ "\n";
                        MessageBox.Show(str1, "管径查询结果");
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有管径属性！");
                }
                recordset.Dispose();
        }
        #endregion

        #region 附属物查询 mIndex==7
        /// <summary>
        /// 附属物查询 mIndex==7
        /// </summary>
        public void AppendEvent(int index)
        {
            mIndex = index;
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlQuery);  
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlQuery); 
        }
        //private void mSceneControlQuery6(object sender, ObjectSelectedEventArgs e)
        //{
        //    // 无对象被选中
        //    if (e.Count == 0 && mIndex == 7)
        //    {
        //        MessageBox.Show("未选择对象!");
        //    }
        //    //有对象选中
        //    else if (e.Count > 0 && mIndex == 7)
        //    {
        //        AppendQuery();
        //    }
        //}
        public void AppendQuery()
        {
                mSceneControl.Action = Action3D.Select;
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要查询附属物的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();

                string str1 = "";

                string PipeType = recordset.GetFieldValue("PipeType").ToString();
                if (PipeType== "附属物")
                {
                        object obj1 = recordset.GetFieldValue("Name");
                        str1 = "该附属物为" + "：" + obj1.ToString() + "\n";
                        MessageBox.Show(str1, "附属物查询结果");
                }
                else
                {
                        MessageBox.Show("该对象不是附属物！");
                }                                                           
                recordset.Dispose();
        }        
    }
}
        #endregion
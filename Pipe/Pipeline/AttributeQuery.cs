using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SuperMap.Data;
using SuperMap.Mapping;
using SuperMap.UI;
using SuperMap.Realspace;
using SuperMap.Realspace.NetworkAnalyst;
using System.Diagnostics;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： AttributeQuery
    ** 描述： 属性查询
    ** 作者： FKW
    ** 创建时间： 2017/7/10 11:38:16
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 11:38:16
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
    *********************************************************************************/
    public partial class AttributeQuery : Form
    {
        private Dataset mData;
        public Dataset Data
        {
            get { return mData; }
        }

        private DatasetVector mDataVec;
        public DatasetVector DataVec
        {
            get { return mDataVec; }
        }

        public AttributeQuery(Dataset data1)
        {
            this.mData = data1;
            InitializeComponent();
            Query();
        }

        public void Query()
        {
            try
            {
             mDataVec = mData as DatasetVector;
            Recordset rec = mDataVec.GetRecordset(false, CursorType.Dynamic);
            this.dataGridView1.Columns.Clear();
            this.dataGridView1.Rows.Clear();

            for (int i = 0; i < rec.FieldCount; i++)
            {
                //定义并获得字段名称
                String fieldName = rec.GetFieldInfos()[i].Name;
                //将得到的字段名称添加到dataGridView列中
                this.dataGridView1.Columns.Add(fieldName, fieldName);
            }

            //初始化row
            DataGridViewRow row = null;

            //根据选中记录的个数，将选中对象的信息添加到dataGridView中显示
            while (!rec.IsEOF)
            {
                row = new DataGridViewRow();
                for (int i = 0; i < rec.FieldCount; i++)
                {
                    //定义并获得字段值
                    Object fieldValue = rec.GetFieldValue(i);

                    //将字段值添加到dataGridView中对应的位置
                    DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
                    if (fieldValue != null)
                    {
                        cell.ValueType = fieldValue.GetType();
                        cell.Value = fieldValue;
                    }
                    row.Cells.Add(cell);
                }

                this.dataGridView1.Rows.Add(row);
                rec.MoveNext();
            }
            this.dataGridView1.Update();
            rec.Dispose();
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        private void AttributeQuery_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Recordset rec = mDataVec.GetRecordset(false, CursorType.Dynamic);
                //rec.MoveFirst();
                //while(!rec.IsEOF)
                //{
                //   for(int i=0;i<rec.RecordCount;i++)
                //   {
                //      for(int j=0;j<rec.FieldCount;j++)
                //      {
                //          if (!rec.Dataset.FieldInfos[j].IsSystemField)
                //          {
                //              String m_cellvalue = this.dataGridView1.Rows[i].Cells[j].Value.ToString();
                //              rec.SetFieldValue(j, m_cellvalue);
                //          }                       
                //    }
                //   rec.MoveNext();
                //   }
                //}
                //rec.Update();
                rec.Dispose();
               // rec.Dispose();//释放记录集
            }
            catch (Exception)
            {               
                throw;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Recordset rec = mDataVec.GetRecordset(false, CursorType.Dynamic);
                int Rowindex = e.RowIndex;//获取修改记录的行索引
                int Colindex = e.ColumnIndex;//获取修改记录的列索引

                rec.MoveTo(Rowindex);//记录集移到当前索引
                object changevalue = dataGridView1.Rows[Rowindex].Cells[Colindex].Value;//获取修改之后的cell值
                if (!rec.Dataset.FieldInfos[Colindex].IsSystemField)
                {
                   rec.Edit();//编辑记录集中该行记录
                   rec.SetFieldValue(Colindex, changevalue);
                   rec.Update();
                }       
            }
            catch (Exception)
            {               
                throw;
            }
        }
    }
}

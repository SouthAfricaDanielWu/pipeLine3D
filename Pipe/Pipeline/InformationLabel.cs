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
using System.Drawing.Drawing2D;

namespace NorthStar.Pipeline
{
    /********************************************************************************

    ** 类名称： InformationLabel
    ** 描述： 信息标注类，包括管径、距离、埋深等
    ** 作者： fkw
    ** 创建时间： 2017/9/11 15:55:46
    ** 最后修改人：（无）
    ** 最后修改时间：2017/11/22 15:55:46
    ** 版权所有 (C) :桂林理工大学测绘地理信息研发中心
	
    *********************************************************************************/
    class InformationLabel
    {
        private SceneControl mSceneControl;
        private String mComBoBoxValue;
        private Point3D mPoint3D;
        private Point3D mAnchorPoint3D;
        private Rectangle2D mRec2D;
        private UseData mUseData;

        public InformationLabel(SceneControl m_SceneControl, UseData m_UseData)
        {
            mSceneControl = m_SceneControl;
            mUseData = m_UseData;
        }

        #region 扯旗分析事件
        public void PullFlatEvent()
        {
            mSceneControl.Action = Action3D.MeasureDistance;
            //注册事件
            mSceneControl.Tracking -= new Tracking3DEventHandler(TrackingPullFlatEvent);
            mSceneControl.Tracking += new Tracking3DEventHandler(TrackingPullFlatEvent);
            mSceneControl.Tracked -= new Tracked3DEventHandler(TrackedPullFlatEvent);
            mSceneControl.Tracked += new Tracked3DEventHandler(TrackedPullFlatEvent);       
        }

  
        private void TrackedPullFlatEvent(object sender, Tracked3DEventArgs e)
        {
            try
            {
                //绘制量算线对象
                GeoLine3D geoLine3D = e.Geometry.Clone() as GeoLine3D;
                mRec2D = geoLine3D.Bounds;
                mAnchorPoint3D = geoLine3D.InnerPoint3D;
                //设置数据集容量，避免空间查询出现过多对象
                mUseData.OutWaterNetWork.Tolerance.NodeSnap = 0.0001;
                mUseData.SupplyWaterNetWork.Tolerance.NodeSnap = 0.0001;
                mUseData.ElectricNetWork.Tolerance.NodeSnap = 0.0001;

                GeoStyle3D mGeoStyle3D = new GeoStyle3D();
                // mGeoStyle3D.MarkerColor = Color.FromArgb(255, 0, 255);
                mGeoStyle3D.LineColor = Color.FromArgb(0x00, 0x99, 0x00);
                mGeoStyle3D.LineWidth = 3;
                geoLine3D.Style3D = mGeoStyle3D.Clone();
                geoLine3D.Style3D.AltitudeMode = AltitudeMode.Absolute;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoLine3D, "GeoLine3D");
                mSceneControl.Action = Action3D.Pan2;

                LineInsertQuery();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        private void TrackingPullFlatEvent(object sender, Tracking3DEventArgs e)
        {
            try
            {
                Point location = mSceneControl.PointToClient(Cursor.Position);

                location.Offset(30, 30);
                if (location.X > mSceneControl.Bounds.Width / 3 * 2)
                {
                    location.X = mSceneControl.Bounds.Width / 3 * 2;
                }
                if (location.Y > mSceneControl.Bounds.Height)
                {
                    location.Y = location.Y - 60;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        public void LineInsertQuery()
        {
            Layer3Ds m_layer = mSceneControl.Scene.Layers;
            foreach (Layer3D mlayer in m_layer)
            {
                if (mlayer.Selection != null)
                {
                    mlayer.Selection.Clear();
                }
            }

            QueryParameter para = new QueryParameter();
            para.HasGeometry = true;
            para.SpatialQueryMode = SpatialQueryMode.Intersect;
            para.SpatialQueryObject = mRec2D;

            Recordset recordset = null;
            Layer3DDataset layer = null;
            String m_textID = String.Empty;
            String m_textName = String.Empty;
            String m_textDepth = String.Empty;
            String m_textDiameter = String.Empty;
            String m_InfomationText = String.Empty;

            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                {
                    recordset = mUseData.OutWaterNetWork.Query(para);
                    layer = mUseData.OutWaterLines;
                }
                else if (i == 1)
                {
                    recordset = mUseData.SupplyWaterNetWork.Query(para);
                    layer = mUseData.SupplyWaterLines;
                }
                else if (i == 2)
                {
                    recordset = mUseData.ElectricNetWork.Query(para);
                    layer = mUseData.ElectricLayer;
                }


                List<Int32> ids = new List<int>(recordset.RecordCount);
                while (!recordset.IsEOF)
                {
                    m_textID = System.Convert.ToString(recordset.GetFieldValue("SmID"));
                    m_textName = System.Convert.ToString(recordset.GetFieldValue("Name"));
                    m_textDepth = System.Convert.ToString(recordset.GetFieldValue("BottomAltitude"));
                    m_textDiameter = System.Convert.ToString(recordset.GetFieldValue("PipeDiameter"));
                    m_InfomationText += m_textID + "      " + m_textName + "    " + m_textDepth + "            " + m_textDiameter + "\n";
                    ids.Add(recordset.GetID());
                    recordset.MoveNext();
                }
                layer.Selection.AddRange(ids.ToArray());
                layer.Selection.UpdateData();
                layer.Selection.Style.LineColor = Color.GreenYellow;
                mSceneControl.Scene.Refresh();
            }

            //进行文本标注
            String m_text = "编号" + "  " + "管线类别" + "  " + "埋设深度" + "  " + "管径" + "\n";
            m_text += m_InfomationText;
            TextPart3D mText1 = new TextPart3D();
            mText1.Text = m_text;
            mText1.AnchorPoint = mAnchorPoint3D;
            TextStyle mTextStyle1 = new TextStyle();
            mTextStyle1.FontName = "微软雅黑";
            mTextStyle1.ForeColor = Color.Red;
            mTextStyle1.FontHeight = 7;
            mTextStyle1.IsSizeFixed = false;
            mTextStyle1.Alignment = TextAlignment.MiddleCenter;
            GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
            GeoStyle3D geostyle_1 = new GeoStyle3D();
            geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
            geoText_1.Style3D = geostyle_1;
            TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
            trackinglayer.IsEditable = true;
            trackinglayer.IsVisible = true;
            trackinglayer.Add(geoText_1, "PullFlatLabel");      
        }

        #endregion

        #region 信息标注事件
        public void registevent(String m_ComBoxValue)
        {
            mComBoBoxValue = m_ComBoxValue;
            mSceneControl.MouseDown -= new MouseEventHandler(mSceneControlMouse);
            mSceneControl.MouseDown += new MouseEventHandler(mSceneControlMouse);
            mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObject);
            mSceneControl.ObjectSelected += new ObjectSelectedEventHandler(mSceneControlObject);
        }

        private void mSceneControlMouse(object sender, MouseEventArgs e)
        {
            try
            {
                mPoint3D = new Point3D();
                mPoint3D = mSceneControl.Scene.PixelToGlobe(e.Location, PixelToGlobeMode.TerrainAndModel);
            }
            catch (System.Exception ex)
            {
                Trace.Write(ex.Message);
            }
        }
        private void mSceneControlObject(object sender, ObjectSelectedEventArgs e)
        {
            try
            {
                // 无对象被选中
                if (e.Count == 0)
                {
                    return;
                }
                //有对象选中
                else if (e.Count > 0)
                {
                    switch (mComBoBoxValue)
                    {
                        case "管径": DiametLabel();
                            break;
                        case "埋深": PlaceDepthLabel();
                            break;
                        case "坐标": coordLabel();
                            break;
                        case "ID": IdLabel();
                            break;
                        case "材质": MaterialLabel();
                            break;
                        case "权属": OwnerLabel();
                            break;
                        case "名称": NameLabel();
                            break;
                        case "流向": FlowLabel();
                            break;
                    }
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region 管径标注
        /// <summary>
        /// 管径标注
        /// </summary>
        public void DiametLabel()
        {
            try
            {
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要标注管径的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();
                //获取管径字段的值
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
                        str1 = "管径" + "：" + obj.ToString() + "mm" + "\n";
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

                TextPart3D mText1 = new TextPart3D();
                mText1.Text = str1;
                mText1.AnchorPoint = mPoint3D;
                TextStyle mTextStyle1 = new TextStyle();
                mTextStyle1.FontName = "微软雅黑";
                mTextStyle1.ForeColor = Color.Red;
                mTextStyle1.FontHeight = 7;
                mTextStyle1.IsSizeFixed = false;
                mTextStyle1.Alignment = TextAlignment.MiddleCenter;
                GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
                GeoStyle3D geostyle_1 = new GeoStyle3D();
                geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
                geoText_1.Style3D = geostyle_1;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoText_1, "DiametLabel");
                mSceneControl.Scene.Refresh();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region 埋深标注
        public void PlaceDepthLabel()
        {
            try
            {
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要标注埋深的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();
                //获取埋深字段的值
                string str = "";
                string str1 = "";
                object obj;
                bool bol = false;
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "BottomAltitude")
                    {
                        bol = true;
                        obj = recordset.GetFieldValue(i);
                        str1 = "埋深" + "：" + obj.ToString() + "m" + "\n";
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有埋深属性！");
                }
                recordset.Dispose();

                TextPart3D mText1 = new TextPart3D();
                mText1.Text = str1;
                mText1.AnchorPoint = mPoint3D;
                TextStyle mTextStyle1 = new TextStyle();
                mTextStyle1.FontName = "微软雅黑";
                mTextStyle1.ForeColor = Color.Red;
                mTextStyle1.FontHeight = 7;
                mTextStyle1.IsSizeFixed = false;
                mTextStyle1.Alignment = TextAlignment.MiddleCenter;
                GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
                GeoStyle3D geostyle_1 = new GeoStyle3D();
                geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
                geoText_1.Style3D = geostyle_1;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoText_1, "PlaceDepthLabel");
                mSceneControl.Scene.Refresh();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region 清除
        public void ClearEvent()
        {
            try
            {
                mSceneControl.Tracking -= new Tracking3DEventHandler(TrackingPullFlatEvent);
                mSceneControl.Tracked -= new Tracked3DEventHandler(TrackedPullFlatEvent);
                mSceneControl.MouseDown -= new MouseEventHandler(mSceneControlMouse);
                mSceneControl.ObjectSelected -= new ObjectSelectedEventHandler(mSceneControlObject);
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
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
        #endregion

        #region 坐标标注
        public void coordLabel()
        {
            try
            {
                double x_label = mPoint3D.X;
                double y_label = mPoint3D.Y;
                double z_label = mPoint3D.Z;
                String coord = "X: " + x_label + "\n\n" + "Y: " + y_label + "\n\n" + "Z: " + z_label;

                TextPart3D mText1 = new TextPart3D();
                mText1.Text = coord;
                mText1.AnchorPoint = mPoint3D;
                TextStyle mTextStyle1 = new TextStyle();
                mTextStyle1.FontName = "微软雅黑";
                mTextStyle1.ForeColor = Color.Red;
                mTextStyle1.FontHeight = 7;
                mTextStyle1.IsSizeFixed = false;
                mTextStyle1.Alignment = TextAlignment.MiddleCenter;
                GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
                GeoStyle3D geostyle_1 = new GeoStyle3D();
                geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
                geoText_1.Style3D = geostyle_1;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoText_1, "Coord");
                mSceneControl.Scene.Refresh();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region ID标注
        /// <summary>
        /// 标注ID
        /// </summary>
        public void IdLabel()
        {
            try
            {
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要标注ID的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();
                //获取ID字段的值
                string str = "";
                string str1 = "";
                object obj;
                bool bol = false;
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "SmID")
                    {
                        bol = true;
                        obj = recordset.GetFieldValue(i);
                        str1 = "ID: " + obj.ToString() + "\n";
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有ID属性！");
                }
                recordset.Dispose();

                TextPart3D mText1 = new TextPart3D();
                mText1.Text = str1;
                mText1.AnchorPoint = mPoint3D;
                TextStyle mTextStyle1 = new TextStyle();
                mTextStyle1.FontName = "微软雅黑";
                mTextStyle1.ForeColor = Color.Red;
                mTextStyle1.FontHeight = 7;
                mTextStyle1.IsSizeFixed = false;
                mTextStyle1.Alignment = TextAlignment.MiddleCenter;
                GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
                GeoStyle3D geostyle_1 = new GeoStyle3D();
                geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
                geoText_1.Style3D = geostyle_1;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoText_1, "IDLabel");
                mSceneControl.Scene.Refresh();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region 材质标注
        /// <summary>
        /// 标注材质
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MaterialLabel()
        {
            try
            {
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要标注材质的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();
                //获取材质字段的值
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
                        str1 = "材质: " + obj.ToString() + "\n";
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有材质属性！");
                }
                recordset.Dispose();

                TextPart3D mText1 = new TextPart3D();
                mText1.Text = str1;
                mText1.AnchorPoint = mPoint3D;
                TextStyle mTextStyle1 = new TextStyle();
                mTextStyle1.FontName = "微软雅黑";
                mTextStyle1.ForeColor = Color.Red;
                mTextStyle1.FontHeight = 7;
                mTextStyle1.IsSizeFixed = false;
                mTextStyle1.Alignment = TextAlignment.MiddleCenter;
                GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
                GeoStyle3D geostyle_1 = new GeoStyle3D();
                geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
                geoText_1.Style3D = geostyle_1;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoText_1, "MaterialLabel");
                mSceneControl.Scene.Refresh();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region 权属标注
        /// <summary>
        /// 标注权属
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OwnerLabel()
        {
            try
            {
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要标注权属的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();
                //获取权属字段的值
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
                        str1 = "权属: " + obj.ToString() + "\n";
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有权属属性！");
                }
                recordset.Dispose();

                TextPart3D mText1 = new TextPart3D();
                mText1.Text = str1;
                mText1.AnchorPoint = mPoint3D;
                TextStyle mTextStyle1 = new TextStyle();
                mTextStyle1.FontName = "微软雅黑";
                mTextStyle1.ForeColor = Color.Red;
                mTextStyle1.FontHeight = 7;
                mTextStyle1.IsSizeFixed = false;
                mTextStyle1.Alignment = TextAlignment.MiddleCenter;
                GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
                GeoStyle3D geostyle_1 = new GeoStyle3D();
                geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
                geoText_1.Style3D = geostyle_1;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoText_1, "OwnerLabel");
                mSceneControl.Scene.Refresh();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region 名称标注
        /// <summary>
        /// 标注名称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NameLabel()
        {
            try
            {
                Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
                //判断选择集是否为空
                if (selection == null || selection.Length == 0)
                {
                    MessageBox.Show("请选择要标注名称的空间对象");
                    return;
                }
                //将选择集转换为记录
                Recordset recordset = selection[0].ToRecordset();
                //获取名称字段的值
                string str = "";
                string str1 = "";
                object obj;
                bool bol = false;
                for (int i = 0; i < recordset.FieldCount; i++)
                {
                    str = recordset.GetFieldInfos()[i].Name;
                    if (str == "Name")
                    {
                        bol = true;
                        obj = recordset.GetFieldValue(i);
                        str1 = "名称: " + obj.ToString() + "\n";
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (bol == false)
                {
                    MessageBox.Show("该对象没有名称属性！");
                }
                recordset.Dispose();

                TextPart3D mText1 = new TextPart3D();
                mText1.Text = str1;
                mText1.AnchorPoint = mPoint3D;
                TextStyle mTextStyle1 = new TextStyle();
                mTextStyle1.FontName = "微软雅黑";
                mTextStyle1.ForeColor = Color.Red;
                mTextStyle1.FontHeight = 7;
                mTextStyle1.IsSizeFixed = false;
                mTextStyle1.Alignment = TextAlignment.MiddleCenter;
                GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
                GeoStyle3D geostyle_1 = new GeoStyle3D();
                geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
                geoText_1.Style3D = geostyle_1;
                TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
                trackinglayer.IsEditable = true;
                trackinglayer.IsVisible = true;
                trackinglayer.Add(geoText_1, "NameLabel");
                mSceneControl.Scene.Refresh();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region 流向标注
        /// <summary>
        /// 标注流向
        /// </summary>
        public void FlowLabel()
        {
            Selection3D[] selection = mSceneControl.Scene.FindSelection(true);
            //判断选择集是否为空
            if (selection == null || selection.Length == 0)
            {
                MessageBox.Show("请选择要标注流向的空间对象");
                return;
            }
            //将选择集转换为记录
            Recordset recordset = selection[0].ToRecordset();
            GeoLine3D m_line = recordset.GetGeometry() as GeoLine3D;
            Point3D m_Point3D1 = m_line.InnerPoint3D;

            double x1 = System.Convert.ToDouble(recordset.GetFieldValue("SmSdriE"));
            double y1 = System.Convert.ToDouble(recordset.GetFieldValue("SmSdriS"));
            double z = System.Convert.ToDouble(recordset.GetFieldValue("BottomAltitude"));

            Point3D m_Point3D2 = new Point3D(x1, y1, z);
            Point3Ds m_Point3Ds = new Point3Ds(new Point3D[] { m_Point3D1, m_Point3D2 });
            m_Point3Ds = coordinateTrans3D(m_Point3Ds);
            GeoLine3D m_Line3D = new GeoLine3D(m_Point3Ds);
            // GeoModel3D m_model=m_Line3D.ConvertToGeoModel3D(false);
            //设置线样式
            GeoStyle3D m_Style3D = new GeoStyle3D();
            m_Style3D.IsMarker3D = true;
            m_Style3D.AltitudeMode = AltitudeMode.Absolute;
            m_Style3D.FillForeColor = Color.FromArgb(0x64, 0x40, 0xFF, 0x5F);
            m_Style3D.FillMode = FillMode3D.Fill;
            m_Style3D.LineColor = Color.Lime;
            m_Style3D.LineWidth = 3;
            m_Line3D.Style3D = m_Style3D.Clone();

            TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
            trackinglayer.IsEditable = true;
            trackinglayer.IsVisible = true;
            trackinglayer.Add(m_Line3D, "FlowLine");
            //  mSceneControl.Scene.Refresh(); 


            //double x2 = System.Convert.ToDouble(mPoint3D.X);
            //double y2 = System.Convert.ToDouble(mPoint3D.Y);
            //Point3D m_Point3D1 = new Point3D(x1, y1, 0); 
            //Point3D m_Point3D2 = new Point3D(x1, y1, 0);
            //  Point3Ds m_Point3Ds = new Point3Ds(new Point3D[] { m_Point3D1, m_Point3D2 });
            //m_Point3Ds = coordinateTrans3D(m_Point3Ds);
            //m_Point3D1 = m_Point3Ds[0];
            //m_Point3D2 = m_Point3Ds[1];
            //x1 = m_Point3D1.X;
            //y1 = m_Point3D1.Y;
            //x2 = m_Point3D2.X;
            //y2 = m_Point3D2.Y;

            //double m_Roation = Math.Atan(Math.Abs((y1 - y2) / (x1 - x2)));
            //m_Roation=m_Roation * 180 / Math.PI;
            // String m_text = string.Empty;
            // String m_Start = System.Convert.ToString(recordset.GetFieldValue("SmFNode"));
            // m_Start="";
            // String m_End = System.Convert.ToString(recordset.GetFieldValue("SmTNode"));
            // m_End="";
            // m_text = m_Start + "→→→→→→→→→" + m_End;


            // TextPart3D mText1 = new TextPart3D();
            // mText1.Text = m_text;
            // mText1.AnchorPoint = m_Point3D1;       
            // TextStyle mTextStyle1 = new TextStyle();
            // mTextStyle1.FontName = "微软雅黑";
            // mTextStyle1.ForeColor = Color.Red;
            // mTextStyle1.FontHeight = 7;

            // mTextStyle1.IsSizeFixed = false;
            // mTextStyle1.Alignment = TextAlignment.BaselineCenter;
            //// mTextStyle1.Rotation = m_Roation;
            // GeoText3D geoText_1 = new GeoText3D(mText1, mTextStyle1);
            // GeoStyle3D geostyle_1 = new GeoStyle3D();
            // geostyle_1.AltitudeMode = AltitudeMode.RelativeToGround;
            // geoText_1.Style3D = geostyle_1;
            // TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
            // trackinglayer.IsEditable = true;
            // trackinglayer.IsVisible = true;
            // trackinglayer.Add(geoText_1, "FlowLa");
            // mSceneControl.Scene.Refresh();

            // //得到所选对象的起点和终点坐标
            // double x1=System.Convert.ToDouble(recordset.GetFieldValue("SmSdriW"));
            // double y1=System.Convert.ToDouble(recordset.GetFieldValue("SmSdriN"));
            // double x2 = System.Convert.ToDouble(recordset.GetFieldValue("SmSdriE"));
            // double y2 = System.Convert.ToDouble(recordset.GetFieldValue("SmSdriS"));
            // double z = System.Convert.ToDouble(recordset.GetFieldValue("BottomAltitude"));

            // Point3D m_Point3D1 = new Point3D(x1, y1, z); 
            // Point3D m_Point3D2 = new Point3D(x2, y2, z);
            // Point3Ds m_Point3Ds = new Point3Ds(new Point3D[]{mPoint3D,m_Point3D2});
            // m_Point3Ds=coordinateTrans3D(m_Point3Ds);
            // //根据坐标画线
            // GeoLine3D m_Line3D = new GeoLine3D(m_Point3Ds);
            // //设置线样式
            // GeoStyle3D m_Style3D = new GeoStyle3D();
            //// m_Style3D.IsMarker3D = true;
            // m_Style3D.AltitudeMode = AltitudeMode.RelativeToGround;
            // m_Style3D.LineColor = Color.Lime;
            // m_Style3D.LineWidth = 3;        
            // m_Line3D.Style3D = m_Style3D;

            // TrackingLayer3D trackinglayer = mSceneControl.Scene.TrackingLayer;
            // trackinglayer.IsEditable = true;
            // trackinglayer.IsVisible = true;
            // trackinglayer.Add(m_Line3D, "FlowLine");
            // mSceneControl.Scene.Refresh();            
        }
        #endregion

        #region 地理坐标向投影坐标转换
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
        #endregion

        public MouseEventHandler mSceneControlMouseRight { get; set; }
    }
}

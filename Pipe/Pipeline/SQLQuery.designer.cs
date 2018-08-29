namespace SQL
{
    partial class SQLQuery
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SQLQuery));
            this.Button_allval = new System.Windows.Forms.Button();
            this.LB_allval = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.Button_Clear = new System.Windows.Forms.Button();
            this.Button_Close = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtQueryExpression = new System.Windows.Forms.TextBox();
            this.Button_Query = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtQueryFilter = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.LV_fieldinfo = new System.Windows.Forms.ListView();
            this.colFieldName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFieldType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label4 = new System.Windows.Forms.Label();
            this.CBB_Operats = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtQueryFields = new System.Windows.Forms.TextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // Button_allval
            // 
            this.Button_allval.Location = new System.Drawing.Point(310, 48);
            this.Button_allval.Margin = new System.Windows.Forms.Padding(2);
            this.Button_allval.Name = "Button_allval";
            this.Button_allval.Size = new System.Drawing.Size(56, 24);
            this.Button_allval.TabIndex = 39;
            this.Button_allval.Text = "所有值";
            this.Button_allval.UseVisualStyleBackColor = true;
            this.Button_allval.Click += new System.EventHandler(this.Button_allval_Click);
            // 
            // LB_allval
            // 
            this.LB_allval.FormattingEnabled = true;
            this.LB_allval.ItemHeight = 12;
            this.LB_allval.Location = new System.Drawing.Point(310, 76);
            this.LB_allval.Margin = new System.Windows.Forms.Padding(2);
            this.LB_allval.Name = "LB_allval";
            this.LB_allval.Size = new System.Drawing.Size(201, 76);
            this.LB_allval.TabIndex = 37;
            this.LB_allval.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LB_allval_MouseDoubleClick);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(258, 53);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 35;
            this.label7.Text = "字段值：";
            // 
            // Button_Clear
            // 
            this.Button_Clear.Location = new System.Drawing.Point(381, 338);
            this.Button_Clear.Margin = new System.Windows.Forms.Padding(2);
            this.Button_Clear.Name = "Button_Clear";
            this.Button_Clear.Size = new System.Drawing.Size(60, 24);
            this.Button_Clear.TabIndex = 32;
            this.Button_Clear.Text = "清除";
            this.Button_Clear.UseVisualStyleBackColor = true;
            this.Button_Clear.Click += new System.EventHandler(this.Button_Clear_Click);
            // 
            // Button_Close
            // 
            this.Button_Close.Location = new System.Drawing.Point(453, 338);
            this.Button_Close.Margin = new System.Windows.Forms.Padding(2);
            this.Button_Close.Name = "Button_Close";
            this.Button_Close.Size = new System.Drawing.Size(57, 24);
            this.Button_Close.TabIndex = 31;
            this.Button_Close.Text = "关闭";
            this.Button_Close.UseVisualStyleBackColor = true;
            this.Button_Close.Click += new System.EventHandler(this.Button_Close_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(247, 285);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 29;
            this.label6.Text = "查询语句：";
            // 
            // txtQueryExpression
            // 
            this.txtQueryExpression.Location = new System.Drawing.Point(310, 282);
            this.txtQueryExpression.Margin = new System.Windows.Forms.Padding(2);
            this.txtQueryExpression.Multiline = true;
            this.txtQueryExpression.Name = "txtQueryExpression";
            this.txtQueryExpression.ReadOnly = true;
            this.txtQueryExpression.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtQueryExpression.Size = new System.Drawing.Size(201, 51);
            this.txtQueryExpression.TabIndex = 36;
            // 
            // Button_Query
            // 
            this.Button_Query.Location = new System.Drawing.Point(309, 338);
            this.Button_Query.Margin = new System.Windows.Forms.Padding(2);
            this.Button_Query.Name = "Button_Query";
            this.Button_Query.Size = new System.Drawing.Size(60, 24);
            this.Button_Query.TabIndex = 30;
            this.Button_Query.Text = "查询";
            this.Button_Query.UseVisualStyleBackColor = true;
            this.Button_Query.Click += new System.EventHandler(this.Button_Query_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 190);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 28;
            this.label5.Text = "字段信息：";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 27;
            this.label3.Text = "查询数据集：";
            // 
            // txtQueryFilter
            // 
            this.txtQueryFilter.Location = new System.Drawing.Point(309, 223);
            this.txtQueryFilter.Margin = new System.Windows.Forms.Padding(2);
            this.txtQueryFilter.Multiline = true;
            this.txtQueryFilter.Name = "txtQueryFilter";
            this.txtQueryFilter.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtQueryFilter.Size = new System.Drawing.Size(201, 51);
            this.txtQueryFilter.TabIndex = 38;
            this.txtQueryFilter.TextChanged += new System.EventHandler(this.txtQueryFilter_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(247, 230);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 25;
            this.label2.Text = "查询条件：";
            // 
            // LV_fieldinfo
            // 
            this.LV_fieldinfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFieldName,
            this.colFieldType});
            this.LV_fieldinfo.FullRowSelect = true;
            this.LV_fieldinfo.GridLines = true;
            this.LV_fieldinfo.Location = new System.Drawing.Point(9, 207);
            this.LV_fieldinfo.Margin = new System.Windows.Forms.Padding(2);
            this.LV_fieldinfo.MultiSelect = false;
            this.LV_fieldinfo.Name = "LV_fieldinfo";
            this.LV_fieldinfo.Size = new System.Drawing.Size(224, 155);
            this.LV_fieldinfo.TabIndex = 34;
            this.LV_fieldinfo.UseCompatibleStateImageBehavior = false;
            this.LV_fieldinfo.View = System.Windows.Forms.View.Details;
            this.LV_fieldinfo.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.LV_fieldinfo_ItemSelectionChanged);
            this.LV_fieldinfo.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.LV_fieldinfo_MouseDoubleClick_1);
            // 
            // colFieldName
            // 
            this.colFieldName.Text = "字段名称";
            this.colFieldName.Width = 91;
            // 
            // colFieldType
            // 
            this.colFieldType.Text = "字段类型";
            this.colFieldType.Width = 209;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(247, 26);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 26;
            this.label4.Text = "运算符号：";
            // 
            // CBB_Operats
            // 
            this.CBB_Operats.FormattingEnabled = true;
            this.CBB_Operats.Location = new System.Drawing.Point(310, 22);
            this.CBB_Operats.Margin = new System.Windows.Forms.Padding(2);
            this.CBB_Operats.Name = "CBB_Operats";
            this.CBB_Operats.Size = new System.Drawing.Size(195, 20);
            this.CBB_Operats.TabIndex = 41;
            this.CBB_Operats.SelectedIndexChanged += new System.EventHandler(this.CBB_Operats_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(247, 174);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 24;
            this.label1.Text = "查询字段：";
            // 
            // txtQueryFields
            // 
            this.txtQueryFields.Location = new System.Drawing.Point(310, 165);
            this.txtQueryFields.Margin = new System.Windows.Forms.Padding(2);
            this.txtQueryFields.Multiline = true;
            this.txtQueryFields.Name = "txtQueryFields";
            this.txtQueryFields.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtQueryFields.Size = new System.Drawing.Size(201, 51);
            this.txtQueryFields.TabIndex = 40;
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(9, 23);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(224, 163);
            this.treeView1.TabIndex = 42;
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            // 
            // SQLQuery
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(521, 373);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.CBB_Operats);
            this.Controls.Add(this.Button_allval);
            this.Controls.Add(this.LB_allval);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtQueryFields);
            this.Controls.Add(this.Button_Clear);
            this.Controls.Add(this.Button_Close);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtQueryExpression);
            this.Controls.Add(this.Button_Query);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtQueryFilter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.LV_fieldinfo);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SQLQuery";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQL查询";
            this.Load += new System.EventHandler(this.SpaceQuery_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Button_allval;
        private System.Windows.Forms.ListBox LB_allval;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button Button_Clear;
        private System.Windows.Forms.Button Button_Close;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtQueryExpression;
        private System.Windows.Forms.Button Button_Query;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtQueryFilter;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView LV_fieldinfo;
        private System.Windows.Forms.ColumnHeader colFieldName;
        private System.Windows.Forms.ColumnHeader colFieldType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox CBB_Operats;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtQueryFields;
        private System.Windows.Forms.TreeView treeView1;
    }
}
namespace NorthStar.Pipeline
{
    partial class SectionAnalysis
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radbtn3 = new System.Windows.Forms.RadioButton();
            this.radbtn2 = new System.Windows.Forms.RadioButton();
            this.radbtn1 = new System.Windows.Forms.RadioButton();
            this.btnCrossSec = new System.Windows.Forms.Button();
            this.btnLongSec = new System.Windows.Forms.Button();
            this.btnSectionClear = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radbtn3);
            this.groupBox1.Controls.Add(this.radbtn2);
            this.groupBox1.Controls.Add(this.radbtn1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(113, 93);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "选择分析图层：";
            // 
            // radbtn3
            // 
            this.radbtn3.AutoSize = true;
            this.radbtn3.Location = new System.Drawing.Point(7, 68);
            this.radbtn3.Name = "radbtn3";
            this.radbtn3.Size = new System.Drawing.Size(71, 16);
            this.radbtn3.TabIndex = 2;
            this.radbtn3.TabStop = true;
            this.radbtn3.Text = "电力管网";
            this.radbtn3.UseVisualStyleBackColor = true;
            this.radbtn3.CheckedChanged += new System.EventHandler(this.radbtn3_CheckedChanged);
            // 
            // radbtn2
            // 
            this.radbtn2.AutoSize = true;
            this.radbtn2.Location = new System.Drawing.Point(7, 45);
            this.radbtn2.Name = "radbtn2";
            this.radbtn2.Size = new System.Drawing.Size(71, 16);
            this.radbtn2.TabIndex = 1;
            this.radbtn2.TabStop = true;
            this.radbtn2.Text = "给水管网";
            this.radbtn2.UseVisualStyleBackColor = true;
            this.radbtn2.CheckedChanged += new System.EventHandler(this.radbtn2_CheckedChanged);
            // 
            // radbtn1
            // 
            this.radbtn1.AutoSize = true;
            this.radbtn1.Location = new System.Drawing.Point(7, 21);
            this.radbtn1.Name = "radbtn1";
            this.radbtn1.Size = new System.Drawing.Size(71, 16);
            this.radbtn1.TabIndex = 0;
            this.radbtn1.TabStop = true;
            this.radbtn1.Text = "排水管网";
            this.radbtn1.UseVisualStyleBackColor = true;
            this.radbtn1.CheckedChanged += new System.EventHandler(this.radbtn1_CheckedChanged);
            // 
            // btnCrossSec
            // 
            this.btnCrossSec.Location = new System.Drawing.Point(15, 120);
            this.btnCrossSec.Name = "btnCrossSec";
            this.btnCrossSec.Size = new System.Drawing.Size(75, 23);
            this.btnCrossSec.TabIndex = 1;
            this.btnCrossSec.Text = "横断面分析";
            this.btnCrossSec.UseVisualStyleBackColor = true;
            this.btnCrossSec.Click += new System.EventHandler(this.btnCrossSec_Click);
            // 
            // btnLongSec
            // 
            this.btnLongSec.Location = new System.Drawing.Point(109, 120);
            this.btnLongSec.Name = "btnLongSec";
            this.btnLongSec.Size = new System.Drawing.Size(75, 23);
            this.btnLongSec.TabIndex = 2;
            this.btnLongSec.Text = "纵断面分析";
            this.btnLongSec.UseVisualStyleBackColor = true;
            // 
            // btnSectionClear
            // 
            this.btnSectionClear.Location = new System.Drawing.Point(56, 151);
            this.btnSectionClear.Name = "btnSectionClear";
            this.btnSectionClear.Size = new System.Drawing.Size(75, 23);
            this.btnSectionClear.TabIndex = 3;
            this.btnSectionClear.Text = "清除";
            this.btnSectionClear.UseVisualStyleBackColor = true;
            // 
            // SectionAnalysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(207, 189);
            this.Controls.Add(this.btnSectionClear);
            this.Controls.Add(this.btnLongSec);
            this.Controls.Add(this.btnCrossSec);
            this.Controls.Add(this.groupBox1);
            this.Name = "SectionAnalysis";
            this.Text = "断面分析";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCrossSec;
        private System.Windows.Forms.Button btnLongSec;
        private System.Windows.Forms.Button btnSectionClear;
        private System.Windows.Forms.RadioButton radbtn3;
        private System.Windows.Forms.RadioButton radbtn2;
        private System.Windows.Forms.RadioButton radbtn1;
    }
}
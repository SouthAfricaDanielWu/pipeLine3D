namespace NorthStar.Pipeline.Common
{
    partial class FrmProgressBar
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
            this.pBarMain = new System.Windows.Forms.ProgressBar();
            this.labContent = new System.Windows.Forms.Label();
            this.txtDetailContent = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.labSubContent = new System.Windows.Forms.Label();
            this.pBarSub = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.labSplitLine = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pBarMain
            // 
            this.pBarMain.Location = new System.Drawing.Point(29, 48);
            this.pBarMain.Name = "pBarMain";
            this.pBarMain.Size = new System.Drawing.Size(944, 67);
            this.pBarMain.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pBarMain.TabIndex = 0;
            this.pBarMain.Value = 50;
            // 
            // labContent
            // 
            this.labContent.AutoSize = true;
            this.labContent.Location = new System.Drawing.Point(25, 10);
            this.labContent.Name = "labContent";
            this.labContent.Size = new System.Drawing.Size(76, 21);
            this.labContent.TabIndex = 1;
            this.labContent.Text = "label1";
            // 
            // txtDetailContent
            // 
            this.txtDetailContent.Location = new System.Drawing.Point(29, 279);
            this.txtDetailContent.Multiline = true;
            this.txtDetailContent.Name = "txtDetailContent";
            this.txtDetailContent.Size = new System.Drawing.Size(955, 303);
            this.txtDetailContent.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.labSubContent);
            this.panel1.Controls.Add(this.pBarSub);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.txtDetailContent);
            this.panel1.Controls.Add(this.labSplitLine);
            this.panel1.Controls.Add(this.pBarMain);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(996, 651);
            this.panel1.TabIndex = 3;
            // 
            // labSubContent
            // 
            this.labSubContent.AutoSize = true;
            this.labSubContent.Location = new System.Drawing.Point(28, 136);
            this.labSubContent.Name = "labSubContent";
            this.labSubContent.Size = new System.Drawing.Size(76, 21);
            this.labSubContent.TabIndex = 4;
            this.labSubContent.Text = "label1";
            // 
            // pBarSub
            // 
            this.pBarSub.Location = new System.Drawing.Point(29, 160);
            this.pBarSub.Name = "pBarSub";
            this.pBarSub.Size = new System.Drawing.Size(944, 67);
            this.pBarSub.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pBarSub.TabIndex = 5;
            this.pBarSub.Value = 50;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 255);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(956, 21);
            this.label2.TabIndex = 4;
            this.label2.Text = "---------------------------------------------------------------------------------" +
                "-----";
            this.label2.Visible = false;
            // 
            // labSplitLine
            // 
            this.labSplitLine.AutoSize = true;
            this.labSplitLine.Location = new System.Drawing.Point(28, 136);
            this.labSplitLine.Name = "labSplitLine";
            this.labSplitLine.Size = new System.Drawing.Size(956, 21);
            this.labSplitLine.TabIndex = 3;
            this.labSplitLine.Text = "---------------------------------------------------------------------------------" +
                "-----";
            this.labSplitLine.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(430, 82);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 21);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(812, 588);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(172, 50);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FrmProgressBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 651);
            this.Controls.Add(this.labContent);
            this.Controls.Add(this.panel1);
            this.Name = "FrmProgressBar";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " ";
            this.Load += new System.EventHandler(this.FrmProgressBar_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pBarMain;
        private System.Windows.Forms.Label labContent;
        private System.Windows.Forms.TextBox txtDetailContent;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labSplitLine;
        private System.Windows.Forms.Label labSubContent;
        private System.Windows.Forms.ProgressBar pBarSub;
        private System.Windows.Forms.Label label2;

    }
}
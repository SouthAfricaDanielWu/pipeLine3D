namespace NorthStar.Pipeline
{
    partial class InformationBubble
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.m_labelDescription = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // m_labelDescription
            // 
            this.m_labelDescription.AutoSize = true;
            this.m_labelDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_labelDescription.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.m_labelDescription.Location = new System.Drawing.Point(0, 0);
            this.m_labelDescription.Name = "m_labelDescription";
            this.m_labelDescription.Size = new System.Drawing.Size(130, 24);
            this.m_labelDescription.TabIndex = 0;
            this.m_labelDescription.Text = "受影响管段";
            // 
            // InformationBubble
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_labelDescription);
            this.Name = "InformationBubble";
            this.Size = new System.Drawing.Size(261, 118);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_labelDescription;
    }
}

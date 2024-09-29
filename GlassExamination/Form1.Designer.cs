namespace GlassExamination
{
    partial class GlassExamination
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GlassExamination));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.iconToolStripButton1 = new FontAwesome.Sharp.IconToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.txtZoomPoint = new System.Windows.Forms.Label();
            this.txtMarkers = new System.Windows.Forms.Label();
            this.lstGlass = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(156, 108);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(150, 83);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // iconToolStripButton1
            // 
            this.iconToolStripButton1.IconChar = FontAwesome.Sharp.IconChar.None;
            this.iconToolStripButton1.IconColor = System.Drawing.Color.Black;
            this.iconToolStripButton1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconToolStripButton1.Name = "iconToolStripButton1";
            this.iconToolStripButton1.Size = new System.Drawing.Size(23, 23);
            this.iconToolStripButton1.Text = "iconToolStripButton1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(1200, 31);
            this.toolStrip1.TabIndex = 1;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // txtZoomPoint
            // 
            this.txtZoomPoint.AutoSize = true;
            this.txtZoomPoint.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtZoomPoint.Location = new System.Drawing.Point(234, 0);
            this.txtZoomPoint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.txtZoomPoint.Name = "txtZoomPoint";
            this.txtZoomPoint.Size = new System.Drawing.Size(0, 25);
            this.txtZoomPoint.TabIndex = 5;
            // 
            // txtMarkers
            // 
            this.txtMarkers.AutoSize = true;
            this.txtMarkers.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtMarkers.Location = new System.Drawing.Point(0, 717);
            this.txtMarkers.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.txtMarkers.Name = "txtMarkers";
            this.txtMarkers.Size = new System.Drawing.Size(0, 25);
            this.txtMarkers.TabIndex = 6;
            // 
            // lstGlass
            // 
            this.lstGlass.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lstGlass.FormattingEnabled = true;
            this.lstGlass.ItemHeight = 25;
            this.lstGlass.Location = new System.Drawing.Point(0, 48);
            this.lstGlass.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstGlass.Name = "lstGlass";
            this.lstGlass.Size = new System.Drawing.Size(178, 129);
            this.lstGlass.TabIndex = 7;
            // 
            // GlassExamination
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 750);
            this.Controls.Add(this.lstGlass);
            this.Controls.Add(this.txtMarkers);
            this.Controls.Add(this.txtZoomPoint);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.pictureBox1);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "GlassExamination";
            this.Text = "玻璃瑕疵檢測";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private FontAwesome.Sharp.IconToolStripButton iconToolStripButton1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Label txtZoomPoint;
        private System.Windows.Forms.Label txtMarkers;
        private System.Windows.Forms.ListBox lstGlass;
    }
}


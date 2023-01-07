namespace Client
{
    partial class MainView
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.VideoBox = new Cyotek.Windows.Forms.ImageBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.VideoBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(927, 445);
            this.panel1.TabIndex = 0;
            // 
            // VideoBox
            // 
            this.VideoBox.AllowDoubleClick = true;
            this.VideoBox.AllowUnfocusedMouseWheel = true;
            this.VideoBox.AllowZoom = false;
            this.VideoBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.VideoBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.VideoBox.Font = new System.Drawing.Font("Arial Black", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.VideoBox.GridColor = System.Drawing.SystemColors.ControlLight;
            this.VideoBox.GridColorAlternate = System.Drawing.SystemColors.ControlLight;
            this.VideoBox.GridDisplayMode = Cyotek.Windows.Forms.ImageBoxGridDisplayMode.Image;
            this.VideoBox.ImageBorderColor = System.Drawing.SystemColors.ControlLight;
            this.VideoBox.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            this.VideoBox.Location = new System.Drawing.Point(0, 0);
            this.VideoBox.Name = "VideoBox";
            this.VideoBox.ShortcutsEnabled = false;
            this.VideoBox.Size = new System.Drawing.Size(927, 445);
            this.VideoBox.TabIndex = 1;
            this.VideoBox.Text = "Waiting for connection..";
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(927, 445);
            this.Controls.Add(this.panel1);
            this.Name = "MainView";
            this.Text = "MainView";
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private Cyotek.Windows.Forms.ImageBox VideoBox;
    }
}


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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainView));
            this.panel1 = new System.Windows.Forms.Panel();
            this.VideoBox = new Cyotek.Windows.Forms.ImageBox();
            this.QualitySetter = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.autoQualityControl = new System.Windows.Forms.ToolStripMenuItem();
            this.q_100p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_90p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_80p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_70p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_60p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_50p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_40p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_30p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_20p = new System.Windows.Forms.ToolStripMenuItem();
            this.q_10p = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            this.QualitySetter.SuspendLayout();
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
            this.VideoBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.VideoBox.ContextMenuStrip = this.QualitySetter;
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
            // QualitySetter
            // 
            this.QualitySetter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoQualityControl,
            this.q_100p,
            this.q_90p,
            this.q_80p,
            this.q_70p,
            this.q_60p,
            this.q_50p,
            this.q_40p,
            this.q_30p,
            this.q_20p,
            this.q_10p});
            this.QualitySetter.Name = "QualitySetter";
            this.QualitySetter.ShowCheckMargin = true;
            this.QualitySetter.ShowImageMargin = false;
            this.QualitySetter.ShowItemToolTips = false;
            this.QualitySetter.Size = new System.Drawing.Size(185, 268);
            this.QualitySetter.Text = "Set new quality";
            // 
            // autoQualityControl
            // 
            this.autoQualityControl.Checked = true;
            this.autoQualityControl.CheckOnClick = true;
            this.autoQualityControl.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoQualityControl.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.autoQualityControl.Name = "autoQualityControl";
            this.autoQualityControl.Size = new System.Drawing.Size(184, 22);
            this.autoQualityControl.Text = "Auto Quality Control";
            this.autoQualityControl.Click += new System.EventHandler(this.AutoQualityControl_Click);
            // 
            // q_100p
            // 
            this.q_100p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_100p.Enabled = false;
            this.q_100p.Name = "q_100p";
            this.q_100p.Size = new System.Drawing.Size(184, 22);
            this.q_100p.Text = "Quality: 100%";
            this.q_100p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_90p
            // 
            this.q_90p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_90p.Enabled = false;
            this.q_90p.Name = "q_90p";
            this.q_90p.Size = new System.Drawing.Size(184, 22);
            this.q_90p.Text = "Quality: 90%";
            this.q_90p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_80p
            // 
            this.q_80p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_80p.Enabled = false;
            this.q_80p.Name = "q_80p";
            this.q_80p.Size = new System.Drawing.Size(184, 22);
            this.q_80p.Text = "Quality: 80%";
            this.q_80p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_70p
            // 
            this.q_70p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_70p.Enabled = false;
            this.q_70p.Name = "q_70p";
            this.q_70p.Size = new System.Drawing.Size(184, 22);
            this.q_70p.Text = "Quality: 70%";
            this.q_70p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_60p
            // 
            this.q_60p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_60p.Enabled = false;
            this.q_60p.Name = "q_60p";
            this.q_60p.Size = new System.Drawing.Size(184, 22);
            this.q_60p.Text = "Quality: 60%";
            this.q_60p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_50p
            // 
            this.q_50p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_50p.Enabled = false;
            this.q_50p.Name = "q_50p";
            this.q_50p.Size = new System.Drawing.Size(184, 22);
            this.q_50p.Text = "Quality: 50%";
            this.q_50p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_40p
            // 
            this.q_40p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_40p.Enabled = false;
            this.q_40p.Name = "q_40p";
            this.q_40p.Size = new System.Drawing.Size(184, 22);
            this.q_40p.Text = "Quality: 40%";
            this.q_40p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_30p
            // 
            this.q_30p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_30p.Enabled = false;
            this.q_30p.Name = "q_30p";
            this.q_30p.Size = new System.Drawing.Size(184, 22);
            this.q_30p.Text = "Quality: 30%";
            this.q_30p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_20p
            // 
            this.q_20p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_20p.Enabled = false;
            this.q_20p.Name = "q_20p";
            this.q_20p.Size = new System.Drawing.Size(184, 22);
            this.q_20p.Text = "Quality: 20%";
            this.q_20p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // q_10p
            // 
            this.q_10p.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.q_10p.Enabled = false;
            this.q_10p.Name = "q_10p";
            this.q_10p.Size = new System.Drawing.Size(184, 22);
            this.q_10p.Text = "Quality: 10%";
            this.q_10p.Click += new System.EventHandler(this.QualityChange_button_Click);
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(927, 445);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainView";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainView_FormClosing);
            this.panel1.ResumeLayout(false);
            this.QualitySetter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private Cyotek.Windows.Forms.ImageBox VideoBox;
        private System.Windows.Forms.ContextMenuStrip QualitySetter;
        private System.Windows.Forms.ToolStripMenuItem q_100p;
        private System.Windows.Forms.ToolStripMenuItem q_90p;
        private System.Windows.Forms.ToolStripMenuItem q_80p;
        private System.Windows.Forms.ToolStripMenuItem q_70p;
        private System.Windows.Forms.ToolStripMenuItem q_60p;
        private System.Windows.Forms.ToolStripMenuItem q_50p;
        private System.Windows.Forms.ToolStripMenuItem q_40p;
        private System.Windows.Forms.ToolStripMenuItem q_30p;
        private System.Windows.Forms.ToolStripMenuItem q_20p;
        private System.Windows.Forms.ToolStripMenuItem q_10p;
        private System.Windows.Forms.ToolStripMenuItem autoQualityControl;
    }
}


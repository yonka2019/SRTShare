namespace Client
{
    partial class MainMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainMenu));
            this.MainLabel = new System.Windows.Forms.Label();
            this.StartButton = new System.Windows.Forms.Button();
            this.SettingsButton = new System.Windows.Forms.Button();
            this.separateLabel = new System.Windows.Forms.Label();
            this.CreatorsLabel = new System.Windows.Forms.Label();
            this.ProjectLogo = new System.Windows.Forms.PictureBox();
            this.MainTP = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ProjectLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // MainLabel
            // 
            this.MainLabel.AutoSize = true;
            this.MainLabel.Font = new System.Drawing.Font("Corbel", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MainLabel.ForeColor = System.Drawing.Color.RoyalBlue;
            this.MainLabel.Location = new System.Drawing.Point(47, 9);
            this.MainLabel.Name = "MainLabel";
            this.MainLabel.Size = new System.Drawing.Size(194, 49);
            this.MainLabel.TabIndex = 0;
            this.MainLabel.Text = "SRTShare";
            this.MainTP.SetToolTip(this.MainLabel, "Magshimim Final Project");
            // 
            // StartButton
            // 
            this.StartButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.StartButton.Location = new System.Drawing.Point(33, 95);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(223, 49);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start Conversation";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // SettingsButton
            // 
            this.SettingsButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SettingsButton.Location = new System.Drawing.Point(86, 150);
            this.SettingsButton.Name = "SettingsButton";
            this.SettingsButton.Size = new System.Drawing.Size(125, 30);
            this.SettingsButton.TabIndex = 2;
            this.SettingsButton.Text = "Settings";
            this.SettingsButton.UseVisualStyleBackColor = true;
            this.SettingsButton.Click += new System.EventHandler(this.SettingsButton_Click);
            // 
            // separateLabel
            // 
            this.separateLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separateLabel.Location = new System.Drawing.Point(-8, 213);
            this.separateLabel.Name = "separateLabel";
            this.separateLabel.Size = new System.Drawing.Size(305, 2);
            this.separateLabel.TabIndex = 4;
            // 
            // CreatorsLabel
            // 
            this.CreatorsLabel.AutoSize = true;
            this.CreatorsLabel.Font = new System.Drawing.Font("Microsoft YaHei UI", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CreatorsLabel.Location = new System.Drawing.Point(78, 234);
            this.CreatorsLabel.Name = "CreatorsLabel";
            this.CreatorsLabel.Size = new System.Drawing.Size(133, 25);
            this.CreatorsLabel.TabIndex = 3;
            this.CreatorsLabel.Text = "Yonka && Eyal";
            this.MainTP.SetToolTip(this.CreatorsLabel, "Project creators");
            // 
            // ProjectLogo
            // 
            this.ProjectLogo.BackColor = System.Drawing.Color.Transparent;
            this.ProjectLogo.Image = global::Client.Properties.Resources.SRTShare_logo;
            this.ProjectLogo.Location = new System.Drawing.Point(12, 229);
            this.ProjectLogo.Name = "ProjectLogo";
            this.ProjectLogo.Size = new System.Drawing.Size(35, 35);
            this.ProjectLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.ProjectLogo.TabIndex = 6;
            this.ProjectLogo.TabStop = false;
            this.MainTP.SetToolTip(this.ProjectLogo, "Project logo");
            // 
            // MainMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 276);
            this.Controls.Add(this.ProjectLogo);
            this.Controls.Add(this.CreatorsLabel);
            this.Controls.Add(this.separateLabel);
            this.Controls.Add(this.SettingsButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.MainLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Welcome!";
            ((System.ComponentModel.ISupportInitialize)(this.ProjectLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label MainLabel;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button SettingsButton;
        private System.Windows.Forms.Label separateLabel;
        private System.Windows.Forms.Label CreatorsLabel;
        private System.Windows.Forms.PictureBox ProjectLogo;
        private System.Windows.Forms.ToolTip MainTP;
    }
}
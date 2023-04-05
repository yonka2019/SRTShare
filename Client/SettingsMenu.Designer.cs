namespace Client
{
    partial class SettingsMenu
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsMenu));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.serverIpLabel = new System.Windows.Forms.Label();
            this.serverIP = new MaterialSkin.Controls.MaterialTextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.serverIpLabel);
            this.groupBox1.Controls.Add(this.serverIP);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(659, 367);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Settings";
            // 
            // serverIpLabel
            // 
            this.serverIpLabel.AutoSize = true;
            this.serverIpLabel.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.serverIpLabel.Location = new System.Drawing.Point(266, 58);
            this.serverIpLabel.Name = "serverIpLabel";
            this.serverIpLabel.Size = new System.Drawing.Size(127, 23);
            this.serverIpLabel.TabIndex = 2;
            this.serverIpLabel.Text = "Server IP:PORT";
            // 
            // serverIP
            // 
            this.serverIP.AnimateReadOnly = false;
            this.serverIP.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.serverIP.Depth = 0;
            this.serverIP.Font = new System.Drawing.Font("Roboto", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.serverIP.LeadingIcon = null;
            this.serverIP.Location = new System.Drawing.Point(236, 19);
            this.serverIP.MaxLength = 50;
            this.serverIP.MouseState = MaterialSkin.MouseState.OUT;
            this.serverIP.Multiline = false;
            this.serverIP.Name = "serverIP";
            this.serverIP.Size = new System.Drawing.Size(187, 36);
            this.serverIP.TabIndex = 1;
            this.serverIP.Text = "";
            this.serverIP.TrailingIcon = null;
            this.serverIP.UseTallSize = false;
            // 
            // ServerSettingsMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 391);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ServerSettingsMenu";
            this.Text = "ServerSettingsMenu";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label serverIpLabel;
        private MaterialSkin.Controls.MaterialTextBox serverIP;
    }
}
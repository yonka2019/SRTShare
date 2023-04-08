namespace Client
{
    partial class Settings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            this.ServerIpPortTB = new MaterialSkin.Controls.MaterialTextBox2();
            this.SettingsGP = new System.Windows.Forms.GroupBox();
            this.SaveExitButton = new System.Windows.Forms.Button();
            this.audioTransCB = new System.Windows.Forms.CheckBox();
            this.retrModeCB = new System.Windows.Forms.CheckBox();
            this.autoQualityControlCB = new System.Windows.Forms.CheckBox();
            this.AutoQualityControlGP = new System.Windows.Forms.GroupBox();
            this.DecreaseQualityNum = new System.Windows.Forms.NumericUpDown();
            this.DecreaseQualityLabel = new System.Windows.Forms.Label();
            this.DataLossRequiredNum = new System.Windows.Forms.NumericUpDown();
            this.DataLossRequiredLabel = new System.Windows.Forms.Label();
            this.IntialPSNNum = new System.Windows.Forms.NumericUpDown();
            this.InitPSNLabel = new System.Windows.Forms.Label();
            this.EncryptionLabel = new System.Windows.Forms.Label();
            this.EncryptionCBox = new System.Windows.Forms.ComboBox();
            this.separateLabel = new System.Windows.Forms.Label();
            this.ServerIpPortLabel = new System.Windows.Forms.Label();
            this.SettingsGP.SuspendLayout();
            this.AutoQualityControlGP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DecreaseQualityNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataLossRequiredNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.IntialPSNNum)).BeginInit();
            this.SuspendLayout();
            // 
            // ServerIpPortTB
            // 
            this.ServerIpPortTB.AnimateReadOnly = false;
            this.ServerIpPortTB.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ServerIpPortTB.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ServerIpPortTB.Depth = 0;
            this.ServerIpPortTB.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.ServerIpPortTB.HideSelection = true;
            this.ServerIpPortTB.LeadingIcon = null;
            this.ServerIpPortTB.Location = new System.Drawing.Point(174, 19);
            this.ServerIpPortTB.MaxLength = 32767;
            this.ServerIpPortTB.MouseState = MaterialSkin.MouseState.OUT;
            this.ServerIpPortTB.Name = "ServerIpPortTB";
            this.ServerIpPortTB.PasswordChar = '\0';
            this.ServerIpPortTB.PrefixSuffixText = null;
            this.ServerIpPortTB.ReadOnly = false;
            this.ServerIpPortTB.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.ServerIpPortTB.SelectedText = "";
            this.ServerIpPortTB.SelectionLength = 0;
            this.ServerIpPortTB.SelectionStart = 0;
            this.ServerIpPortTB.ShortcutsEnabled = false;
            this.ServerIpPortTB.ShowAssistiveText = true;
            this.ServerIpPortTB.Size = new System.Drawing.Size(250, 52);
            this.ServerIpPortTB.TabIndex = 2;
            this.ServerIpPortTB.TabStop = false;
            this.ServerIpPortTB.Text = "192.168.1.29:1397";
            this.ServerIpPortTB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.ServerIpPortTB.TrailingIcon = null;
            this.ServerIpPortTB.UseSystemPasswordChar = false;
            this.ServerIpPortTB.UseTallSize = false;
            this.ServerIpPortTB.TextChanged += new System.EventHandler(this.ServerIpPortTB_TextChanged);
            // 
            // SettingsGP
            // 
            this.SettingsGP.Controls.Add(this.SaveExitButton);
            this.SettingsGP.Controls.Add(this.audioTransCB);
            this.SettingsGP.Controls.Add(this.retrModeCB);
            this.SettingsGP.Controls.Add(this.autoQualityControlCB);
            this.SettingsGP.Controls.Add(this.AutoQualityControlGP);
            this.SettingsGP.Controls.Add(this.IntialPSNNum);
            this.SettingsGP.Controls.Add(this.InitPSNLabel);
            this.SettingsGP.Controls.Add(this.EncryptionLabel);
            this.SettingsGP.Controls.Add(this.EncryptionCBox);
            this.SettingsGP.Controls.Add(this.separateLabel);
            this.SettingsGP.Controls.Add(this.ServerIpPortLabel);
            this.SettingsGP.Controls.Add(this.ServerIpPortTB);
            this.SettingsGP.Location = new System.Drawing.Point(12, 12);
            this.SettingsGP.Name = "SettingsGP";
            this.SettingsGP.Size = new System.Drawing.Size(599, 334);
            this.SettingsGP.TabIndex = 1;
            this.SettingsGP.TabStop = false;
            this.SettingsGP.Text = "Conversation Settings";
            // 
            // SaveExitButton
            // 
            this.SaveExitButton.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F);
            this.SaveExitButton.Location = new System.Drawing.Point(408, 272);
            this.SaveExitButton.Name = "SaveExitButton";
            this.SaveExitButton.Size = new System.Drawing.Size(151, 48);
            this.SaveExitButton.TabIndex = 1;
            this.SaveExitButton.Text = "Save and Exit";
            this.SaveExitButton.UseVisualStyleBackColor = true;
            this.SaveExitButton.Click += new System.EventHandler(this.SaveExitButton_Click);
            // 
            // audioTransCB
            // 
            this.audioTransCB.AutoSize = true;
            this.audioTransCB.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.audioTransCB.Checked = true;
            this.audioTransCB.CheckState = System.Windows.Forms.CheckState.Checked;
            this.audioTransCB.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.audioTransCB.Location = new System.Drawing.Point(10, 254);
            this.audioTransCB.Name = "audioTransCB";
            this.audioTransCB.Size = new System.Drawing.Size(186, 24);
            this.audioTransCB.TabIndex = 6;
            this.audioTransCB.Text = "• Audio Transmission";
            this.audioTransCB.UseVisualStyleBackColor = true;
            // 
            // retrModeCB
            // 
            this.retrModeCB.AutoSize = true;
            this.retrModeCB.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.retrModeCB.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.retrModeCB.Location = new System.Drawing.Point(10, 296);
            this.retrModeCB.Name = "retrModeCB";
            this.retrModeCB.Size = new System.Drawing.Size(201, 24);
            this.retrModeCB.TabIndex = 7;
            this.retrModeCB.Text = "• Retransmission Mode";
            this.retrModeCB.UseVisualStyleBackColor = true;
            // 
            // autoQualityControlCB
            // 
            this.autoQualityControlCB.AutoSize = true;
            this.autoQualityControlCB.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.autoQualityControlCB.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.autoQualityControlCB.Location = new System.Drawing.Point(10, 212);
            this.autoQualityControlCB.Name = "autoQualityControlCB";
            this.autoQualityControlCB.Size = new System.Drawing.Size(195, 24);
            this.autoQualityControlCB.TabIndex = 5;
            this.autoQualityControlCB.Text = "• Auto Quality Control";
            this.autoQualityControlCB.UseVisualStyleBackColor = true;
            // 
            // AutoQualityControlGP
            // 
            this.AutoQualityControlGP.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.AutoQualityControlGP.Controls.Add(this.DecreaseQualityNum);
            this.AutoQualityControlGP.Controls.Add(this.DecreaseQualityLabel);
            this.AutoQualityControlGP.Controls.Add(this.DataLossRequiredNum);
            this.AutoQualityControlGP.Controls.Add(this.DataLossRequiredLabel);
            this.AutoQualityControlGP.Location = new System.Drawing.Point(374, 127);
            this.AutoQualityControlGP.Name = "AutoQualityControlGP";
            this.AutoQualityControlGP.Size = new System.Drawing.Size(225, 124);
            this.AutoQualityControlGP.TabIndex = 10;
            this.AutoQualityControlGP.TabStop = false;
            this.AutoQualityControlGP.Text = "Auto Quality Control";
            // 
            // DecreaseQualityNum
            // 
            this.DecreaseQualityNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.DecreaseQualityNum.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.DecreaseQualityNum.Location = new System.Drawing.Point(154, 80);
            this.DecreaseQualityNum.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.DecreaseQualityNum.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.DecreaseQualityNum.Name = "DecreaseQualityNum";
            this.DecreaseQualityNum.Size = new System.Drawing.Size(52, 21);
            this.DecreaseQualityNum.TabIndex = 9;
            this.DecreaseQualityNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DecreaseQualityNum.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // DecreaseQualityLabel
            // 
            this.DecreaseQualityLabel.AutoSize = true;
            this.DecreaseQualityLabel.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.DecreaseQualityLabel.Location = new System.Drawing.Point(6, 81);
            this.DecreaseQualityLabel.Name = "DecreaseQualityLabel";
            this.DecreaseQualityLabel.Size = new System.Drawing.Size(142, 18);
            this.DecreaseQualityLabel.TabIndex = 16;
            this.DecreaseQualityLabel.Text = "Decrease quality by:";
            // 
            // DataLossRequiredNum
            // 
            this.DataLossRequiredNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.DataLossRequiredNum.Location = new System.Drawing.Point(161, 39);
            this.DataLossRequiredNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.DataLossRequiredNum.Name = "DataLossRequiredNum";
            this.DataLossRequiredNum.Size = new System.Drawing.Size(52, 21);
            this.DataLossRequiredNum.TabIndex = 8;
            this.DataLossRequiredNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DataLossRequiredNum.Value = new decimal(new int[] {
            55,
            0,
            0,
            0});
            // 
            // DataLossRequiredLabel
            // 
            this.DataLossRequiredLabel.AutoSize = true;
            this.DataLossRequiredLabel.Font = new System.Drawing.Font("Microsoft JhengHei UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.DataLossRequiredLabel.Location = new System.Drawing.Point(6, 40);
            this.DataLossRequiredLabel.Name = "DataLossRequiredLabel";
            this.DataLossRequiredLabel.Size = new System.Drawing.Size(149, 18);
            this.DataLossRequiredLabel.TabIndex = 10;
            this.DataLossRequiredLabel.Text = "Data % loss required:";
            // 
            // IntialPSNNum
            // 
            this.IntialPSNNum.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.IntialPSNNum.Location = new System.Drawing.Point(274, 170);
            this.IntialPSNNum.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.IntialPSNNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.IntialPSNNum.Name = "IntialPSNNum";
            this.IntialPSNNum.Size = new System.Drawing.Size(77, 23);
            this.IntialPSNNum.TabIndex = 4;
            this.IntialPSNNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.IntialPSNNum.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // InitPSNLabel
            // 
            this.InitPSNLabel.AutoSize = true;
            this.InitPSNLabel.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.InitPSNLabel.Location = new System.Drawing.Point(6, 171);
            this.InitPSNLabel.Name = "InitPSNLabel";
            this.InitPSNLabel.Size = new System.Drawing.Size(262, 20);
            this.InitPSNLabel.TabIndex = 8;
            this.InitPSNLabel.Text = "• Intial Packet Sequence Number:";
            // 
            // EncryptionLabel
            // 
            this.EncryptionLabel.AutoSize = true;
            this.EncryptionLabel.Font = new System.Drawing.Font("Microsoft JhengHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.EncryptionLabel.Location = new System.Drawing.Point(6, 129);
            this.EncryptionLabel.Name = "EncryptionLabel";
            this.EncryptionLabel.Size = new System.Drawing.Size(105, 20);
            this.EncryptionLabel.TabIndex = 7;
            this.EncryptionLabel.Text = "• Encryption:";
            // 
            // EncryptionCBox
            // 
            this.EncryptionCBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.EncryptionCBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.EncryptionCBox.FormattingEnabled = true;
            this.EncryptionCBox.Location = new System.Drawing.Point(117, 127);
            this.EncryptionCBox.Name = "EncryptionCBox";
            this.EncryptionCBox.Size = new System.Drawing.Size(112, 24);
            this.EncryptionCBox.TabIndex = 3;
            // 
            // separateLabel
            // 
            this.separateLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.separateLabel.Location = new System.Drawing.Point(0, 96);
            this.separateLabel.Name = "separateLabel";
            this.separateLabel.Size = new System.Drawing.Size(599, 2);
            this.separateLabel.TabIndex = 5;
            // 
            // ServerIpPortLabel
            // 
            this.ServerIpPortLabel.AutoSize = true;
            this.ServerIpPortLabel.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ServerIpPortLabel.Location = new System.Drawing.Point(165, 71);
            this.ServerIpPortLabel.Name = "ServerIpPortLabel";
            this.ServerIpPortLabel.Size = new System.Drawing.Size(268, 23);
            this.ServerIpPortLabel.TabIndex = 1;
            this.ServerIpPortLabel.Text = "Server IP:PORT / Hostname:PORT";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 358);
            this.Controls.Add(this.SettingsGP);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Settings";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.SettingsGP.ResumeLayout(false);
            this.SettingsGP.PerformLayout();
            this.AutoQualityControlGP.ResumeLayout(false);
            this.AutoQualityControlGP.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DecreaseQualityNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataLossRequiredNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.IntialPSNNum)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private MaterialSkin.Controls.MaterialTextBox2 ServerIpPortTB;
        private System.Windows.Forms.GroupBox SettingsGP;
        private System.Windows.Forms.Label ServerIpPortLabel;
        private System.Windows.Forms.Label separateLabel;
        private System.Windows.Forms.ComboBox EncryptionCBox;
        private System.Windows.Forms.Label EncryptionLabel;
        private System.Windows.Forms.Label InitPSNLabel;
        private System.Windows.Forms.NumericUpDown IntialPSNNum;
        private System.Windows.Forms.GroupBox AutoQualityControlGP;
        private System.Windows.Forms.CheckBox autoQualityControlCB;
        private System.Windows.Forms.CheckBox audioTransCB;
        private System.Windows.Forms.CheckBox retrModeCB;
        private System.Windows.Forms.Label DataLossRequiredLabel;
        private System.Windows.Forms.NumericUpDown DataLossRequiredNum;
        private System.Windows.Forms.NumericUpDown DecreaseQualityNum;
        private System.Windows.Forms.Label DecreaseQualityLabel;
        private System.Windows.Forms.Button SaveExitButton;
    }
}
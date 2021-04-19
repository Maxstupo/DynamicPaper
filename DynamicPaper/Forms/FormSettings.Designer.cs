namespace Maxstupo.DynamicPaper.Forms {

    partial class FormSettings {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            if (disposing)
                OnDispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.btnOkay = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbRestorePlaying = new System.Windows.Forms.CheckBox();
            this.cbRestorePlaylists = new System.Windows.Forms.CheckBox();
            this.cbCheckForUpdates = new System.Windows.Forms.CheckBox();
            this.cbStartMinimized = new System.Windows.Forms.CheckBox();
            this.cbStartWithWindows = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbMinimizeToTray = new System.Windows.Forms.CheckBox();
            this.cbCloseToTray = new System.Windows.Forms.CheckBox();
            this.linkRestoreDefaults = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOkay
            // 
            this.btnOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOkay.Location = new System.Drawing.Point(297, 257);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(75, 23);
            this.btnOkay.TabIndex = 0;
            this.btnOkay.Text = "&OK";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(378, 257);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cbRestorePlaying);
            this.groupBox1.Controls.Add(this.cbRestorePlaylists);
            this.groupBox1.Controls.Add(this.cbCheckForUpdates);
            this.groupBox1.Controls.Add(this.cbStartMinimized);
            this.groupBox1.Controls.Add(this.cbStartWithWindows);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(441, 137);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Startup";
            // 
            // cbRestorePlaying
            // 
            this.cbRestorePlaying.AutoSize = true;
            this.cbRestorePlaying.Location = new System.Drawing.Point(6, 113);
            this.cbRestorePlaying.Name = "cbRestorePlaying";
            this.cbRestorePlaying.Size = new System.Drawing.Size(154, 17);
            this.cbRestorePlaying.TabIndex = 5;
            this.cbRestorePlaying.Text = "Restore currently playing";
            this.cbRestorePlaying.UseVisualStyleBackColor = true;
            // 
            // cbRestorePlaylists
            // 
            this.cbRestorePlaylists.AutoSize = true;
            this.cbRestorePlaylists.Location = new System.Drawing.Point(6, 90);
            this.cbRestorePlaylists.Name = "cbRestorePlaylists";
            this.cbRestorePlaylists.Size = new System.Drawing.Size(134, 17);
            this.cbRestorePlaylists.TabIndex = 4;
            this.cbRestorePlaylists.Text = "Restore playlist items";
            this.cbRestorePlaylists.UseVisualStyleBackColor = true;
            // 
            // cbCheckForUpdates
            // 
            this.cbCheckForUpdates.AutoSize = true;
            this.cbCheckForUpdates.Enabled = false;
            this.cbCheckForUpdates.Location = new System.Drawing.Point(6, 67);
            this.cbCheckForUpdates.Name = "cbCheckForUpdates";
            this.cbCheckForUpdates.Size = new System.Drawing.Size(181, 17);
            this.cbCheckForUpdates.TabIndex = 2;
            this.cbCheckForUpdates.Text = "Check for application updates";
            this.cbCheckForUpdates.UseVisualStyleBackColor = true;
            // 
            // cbStartMinimized
            // 
            this.cbStartMinimized.AutoSize = true;
            this.cbStartMinimized.Location = new System.Drawing.Point(6, 44);
            this.cbStartMinimized.Name = "cbStartMinimized";
            this.cbStartMinimized.Size = new System.Drawing.Size(105, 17);
            this.cbStartMinimized.TabIndex = 3;
            this.cbStartMinimized.Text = "Start minimized";
            this.cbStartMinimized.UseVisualStyleBackColor = true;
            // 
            // cbStartWithWindows
            // 
            this.cbStartWithWindows.AutoSize = true;
            this.cbStartWithWindows.Location = new System.Drawing.Point(6, 21);
            this.cbStartWithWindows.Name = "cbStartWithWindows";
            this.cbStartWithWindows.Size = new System.Drawing.Size(199, 17);
            this.cbStartWithWindows.TabIndex = 2;
            this.cbStartWithWindows.Text = "Start automatically with Windows";
            this.cbStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.cbMinimizeToTray);
            this.groupBox2.Controls.Add(this.cbCloseToTray);
            this.groupBox2.Location = new System.Drawing.Point(12, 155);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(441, 75);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "System Tray";
            // 
            // cbMinimizeToTray
            // 
            this.cbMinimizeToTray.AutoSize = true;
            this.cbMinimizeToTray.Location = new System.Drawing.Point(6, 44);
            this.cbMinimizeToTray.Name = "cbMinimizeToTray";
            this.cbMinimizeToTray.Size = new System.Drawing.Size(108, 17);
            this.cbMinimizeToTray.TabIndex = 1;
            this.cbMinimizeToTray.Text = "Minimize to tray";
            this.cbMinimizeToTray.UseVisualStyleBackColor = true;
            // 
            // cbCloseToTray
            // 
            this.cbCloseToTray.AutoSize = true;
            this.cbCloseToTray.Location = new System.Drawing.Point(6, 21);
            this.cbCloseToTray.Name = "cbCloseToTray";
            this.cbCloseToTray.Size = new System.Drawing.Size(90, 17);
            this.cbCloseToTray.TabIndex = 0;
            this.cbCloseToTray.Text = "Close to tray";
            this.cbCloseToTray.UseVisualStyleBackColor = true;
            // 
            // linkRestoreDefaults
            // 
            this.linkRestoreDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.linkRestoreDefaults.AutoSize = true;
            this.linkRestoreDefaults.Location = new System.Drawing.Point(22, 262);
            this.linkRestoreDefaults.Name = "linkRestoreDefaults";
            this.linkRestoreDefaults.Size = new System.Drawing.Size(130, 13);
            this.linkRestoreDefaults.TabIndex = 4;
            this.linkRestoreDefaults.TabStop = true;
            this.linkRestoreDefaults.Text = "Restore default settings";
            this.linkRestoreDefaults.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkRestoreDefaults_LinkClicked);
            // 
            // FormSettings
            // 
            this.AcceptButton = this.btnOkay;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(465, 292);
            this.Controls.Add(this.linkRestoreDefaults);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOkay);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(337, 272);
            this.Name = "FormSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOkay;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.LinkLabel linkRestoreDefaults;
        private System.Windows.Forms.CheckBox cbRestorePlaying;
        private System.Windows.Forms.CheckBox cbRestorePlaylists;
        private System.Windows.Forms.CheckBox cbCheckForUpdates;
        private System.Windows.Forms.CheckBox cbStartMinimized;
        private System.Windows.Forms.CheckBox cbStartWithWindows;
        private System.Windows.Forms.CheckBox cbMinimizeToTray;
        private System.Windows.Forms.CheckBox cbCloseToTray;
    }
}
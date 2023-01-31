namespace XLAutoDeploy.Updates
{
    partial class UpdateNotificationView
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
            this.pnlTop = new System.Windows.Forms.Panel();
            this.txtBxUpdateMessage = new System.Windows.Forms.TextBox();
            this.lblNewVersionAvailable = new System.Windows.Forms.Label();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.btnSkip = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.lblPublisher = new System.Windows.Forms.Label();
            this.lblAddIn = new System.Windows.Forms.Label();
            this.lblInstalledVersion = new System.Windows.Forms.Label();
            this.txtBxPublisher = new System.Windows.Forms.TextBox();
            this.txtBxAddIn = new System.Windows.Forms.TextBox();
            this.txtBxInstalledVersion = new System.Windows.Forms.TextBox();
            this.txtBxNewVersion = new System.Windows.Forms.TextBox();
            this.lblNewVersion = new System.Windows.Forms.Label();
            this.pnlTop.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlTop.Controls.Add(this.txtBxUpdateMessage);
            this.pnlTop.Controls.Add(this.lblNewVersionAvailable);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(521, 73);
            this.pnlTop.TabIndex = 0;
            // 
            // txtBxUpdateMessage
            // 
            this.txtBxUpdateMessage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtBxUpdateMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtBxUpdateMessage.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtBxUpdateMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxUpdateMessage.Location = new System.Drawing.Point(0, 31);
            this.txtBxUpdateMessage.Multiline = true;
            this.txtBxUpdateMessage.Name = "txtBxUpdateMessage";
            this.txtBxUpdateMessage.Size = new System.Drawing.Size(521, 42);
            this.txtBxUpdateMessage.TabIndex = 1;
            this.txtBxUpdateMessage.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblNewVersionAvailable
            // 
            this.lblNewVersionAvailable.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblNewVersionAvailable.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNewVersionAvailable.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewVersionAvailable.Location = new System.Drawing.Point(0, 0);
            this.lblNewVersionAvailable.Name = "lblNewVersionAvailable";
            this.lblNewVersionAvailable.Size = new System.Drawing.Size(521, 28);
            this.lblNewVersionAvailable.TabIndex = 0;
            this.lblNewVersionAvailable.Text = "New Version is Available";
            this.lblNewVersionAvailable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlBottom.Controls.Add(this.btnSkip);
            this.pnlBottom.Controls.Add(this.btnOk);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 204);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(521, 46);
            this.pnlBottom.TabIndex = 1;
            // 
            // btnSkip
            // 
            this.btnSkip.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnSkip.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSkip.Location = new System.Drawing.Point(437, 10);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Size = new System.Drawing.Size(75, 25);
            this.btnSkip.TabIndex = 1;
            this.btnSkip.Text = "Skip";
            this.btnSkip.UseVisualStyleBackColor = false;
            this.btnSkip.Click += new System.EventHandler(this.SkipBtnClick);
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.Location = new System.Drawing.Point(356, 10);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 25);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "OK";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.OkBtnClick);
            // 
            // lblPublisher
            // 
            this.lblPublisher.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPublisher.Location = new System.Drawing.Point(25, 91);
            this.lblPublisher.Name = "lblPublisher";
            this.lblPublisher.Size = new System.Drawing.Size(115, 21);
            this.lblPublisher.TabIndex = 2;
            this.lblPublisher.Text = "Publisher";
            this.lblPublisher.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAddIn
            // 
            this.lblAddIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAddIn.Location = new System.Drawing.Point(25, 117);
            this.lblAddIn.Name = "lblAddIn";
            this.lblAddIn.Size = new System.Drawing.Size(115, 21);
            this.lblAddIn.TabIndex = 3;
            this.lblAddIn.Text = "Add-in";
            this.lblAddIn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInstalledVersion
            // 
            this.lblInstalledVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstalledVersion.Location = new System.Drawing.Point(25, 143);
            this.lblInstalledVersion.Name = "lblInstalledVersion";
            this.lblInstalledVersion.Size = new System.Drawing.Size(115, 21);
            this.lblInstalledVersion.TabIndex = 4;
            this.lblInstalledVersion.Text = "Installed Version";
            this.lblInstalledVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBxPublisher
            // 
            this.txtBxPublisher.Location = new System.Drawing.Point(146, 92);
            this.txtBxPublisher.Name = "txtBxPublisher";
            this.txtBxPublisher.Size = new System.Drawing.Size(190, 20);
            this.txtBxPublisher.TabIndex = 5;
            // 
            // txtBxAddIn
            // 
            this.txtBxAddIn.Location = new System.Drawing.Point(146, 118);
            this.txtBxAddIn.Name = "txtBxAddIn";
            this.txtBxAddIn.Size = new System.Drawing.Size(190, 20);
            this.txtBxAddIn.TabIndex = 6;
            // 
            // txtBxInstalledVersion
            // 
            this.txtBxInstalledVersion.Location = new System.Drawing.Point(146, 143);
            this.txtBxInstalledVersion.Name = "txtBxInstalledVersion";
            this.txtBxInstalledVersion.Size = new System.Drawing.Size(119, 20);
            this.txtBxInstalledVersion.TabIndex = 7;
            // 
            // txtBxNewVersion
            // 
            this.txtBxNewVersion.Location = new System.Drawing.Point(146, 169);
            this.txtBxNewVersion.Name = "txtBxNewVersion";
            this.txtBxNewVersion.Size = new System.Drawing.Size(119, 20);
            this.txtBxNewVersion.TabIndex = 9;
            // 
            // lblNewVersion
            // 
            this.lblNewVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewVersion.Location = new System.Drawing.Point(25, 169);
            this.lblNewVersion.Name = "lblNewVersion";
            this.lblNewVersion.Size = new System.Drawing.Size(115, 21);
            this.lblNewVersion.TabIndex = 8;
            this.lblNewVersion.Text = "New Version";
            this.lblNewVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // UpdateNotificationView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(521, 250);
            this.ControlBox = false;
            this.Controls.Add(this.txtBxNewVersion);
            this.Controls.Add(this.lblNewVersion);
            this.Controls.Add(this.txtBxInstalledVersion);
            this.Controls.Add(this.txtBxAddIn);
            this.Controls.Add(this.txtBxPublisher);
            this.Controls.Add(this.lblInstalledVersion);
            this.Controls.Add(this.lblAddIn);
            this.Controls.Add(this.lblPublisher);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateNotificationView";
            this.ShowIcon = false;
            this.Text = "Update Available";
            this.TopMost = true;
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblNewVersionAvailable;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblPublisher;
        private System.Windows.Forms.Label lblAddIn;
        private System.Windows.Forms.Label lblInstalledVersion;
        private System.Windows.Forms.TextBox txtBxPublisher;
        private System.Windows.Forms.TextBox txtBxAddIn;
        private System.Windows.Forms.TextBox txtBxInstalledVersion;
        private System.Windows.Forms.Button btnSkip;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.TextBox txtBxNewVersion;
        private System.Windows.Forms.Label lblNewVersion;
        private System.Windows.Forms.TextBox txtBxUpdateMessage;
    }
}
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
            if (disposing && (components is not null))
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
            this.lblUpdateMessage = new System.Windows.Forms.Label();
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
            this.pnlTop.Controls.Add(this.lblUpdateMessage);
            this.pnlTop.Controls.Add(this.lblNewVersionAvailable);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(521, 61);
            this.pnlTop.TabIndex = 0;
            // 
            // lblUpdateMessage
            // 
            this.lblUpdateMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdateMessage.Location = new System.Drawing.Point(0, 28);
            this.lblUpdateMessage.Name = "lblUpdateMessage";
            this.lblUpdateMessage.Size = new System.Drawing.Size(521, 30);
            this.lblUpdateMessage.TabIndex = 1;
            this.lblUpdateMessage.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblNewVersionAvailable
            // 
            this.lblNewVersionAvailable.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblNewVersionAvailable.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblNewVersionAvailable.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewVersionAvailable.Location = new System.Drawing.Point(0, 0);
            this.lblNewVersionAvailable.Name = "lblNewVersionAvailable";
            this.lblNewVersionAvailable.Size = new System.Drawing.Size(521, 28);
            this.lblNewVersionAvailable.TabIndex = 0;
            this.lblNewVersionAvailable.Text = "A New Version is Available";
            this.lblNewVersionAvailable.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlBottom
            // 
            this.pnlBottom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlBottom.Controls.Add(this.btnSkip);
            this.pnlBottom.Controls.Add(this.btnOk);
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Location = new System.Drawing.Point(0, 192);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(521, 46);
            this.pnlBottom.TabIndex = 1;
            // 
            // btnSkip
            // 
            this.btnSkip.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnSkip.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.btnOk.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
            this.lblPublisher.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPublisher.Location = new System.Drawing.Point(25, 79);
            this.lblPublisher.Name = "lblPublisher";
            this.lblPublisher.Size = new System.Drawing.Size(115, 21);
            this.lblPublisher.TabIndex = 2;
            this.lblPublisher.Text = "Publisher";
            this.lblPublisher.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAddIn
            // 
            this.lblAddIn.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAddIn.Location = new System.Drawing.Point(25, 105);
            this.lblAddIn.Name = "lblAddIn";
            this.lblAddIn.Size = new System.Drawing.Size(115, 21);
            this.lblAddIn.TabIndex = 3;
            this.lblAddIn.Text = "Add-in";
            this.lblAddIn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblInstalledVersion
            // 
            this.lblInstalledVersion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInstalledVersion.Location = new System.Drawing.Point(25, 131);
            this.lblInstalledVersion.Name = "lblInstalledVersion";
            this.lblInstalledVersion.Size = new System.Drawing.Size(115, 21);
            this.lblInstalledVersion.TabIndex = 4;
            this.lblInstalledVersion.Text = "Installed Version";
            this.lblInstalledVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtBxPublisher
            // 
            this.txtBxPublisher.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxPublisher.Location = new System.Drawing.Point(146, 80);
            this.txtBxPublisher.Name = "txtBxPublisher";
            this.txtBxPublisher.ReadOnly = true;
            this.txtBxPublisher.Size = new System.Drawing.Size(190, 23);
            this.txtBxPublisher.TabIndex = 5;
            // 
            // txtBxAddIn
            // 
            this.txtBxAddIn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxAddIn.Location = new System.Drawing.Point(146, 106);
            this.txtBxAddIn.Name = "txtBxAddIn";
            this.txtBxAddIn.ReadOnly = true;
            this.txtBxAddIn.Size = new System.Drawing.Size(190, 23);
            this.txtBxAddIn.TabIndex = 6;
            // 
            // txtBxInstalledVersion
            // 
            this.txtBxInstalledVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxInstalledVersion.Location = new System.Drawing.Point(146, 132);
            this.txtBxInstalledVersion.Name = "txtBxInstalledVersion";
            this.txtBxInstalledVersion.ReadOnly = true;
            this.txtBxInstalledVersion.Size = new System.Drawing.Size(119, 23);
            this.txtBxInstalledVersion.TabIndex = 7;
            // 
            // txtBxNewVersion
            // 
            this.txtBxNewVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBxNewVersion.Location = new System.Drawing.Point(146, 158);
            this.txtBxNewVersion.Name = "txtBxNewVersion";
            this.txtBxNewVersion.ReadOnly = true;
            this.txtBxNewVersion.Size = new System.Drawing.Size(119, 23);
            this.txtBxNewVersion.TabIndex = 9;
            // 
            // lblNewVersion
            // 
            this.lblNewVersion.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNewVersion.Location = new System.Drawing.Point(25, 157);
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
            this.ClientSize = new System.Drawing.Size(521, 238);
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
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.pnlTop.ResumeLayout(false);
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
        private System.Windows.Forms.Label lblUpdateMessage;
    }
}
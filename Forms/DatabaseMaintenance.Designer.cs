namespace ncgmpToolbar.Forms
{
    partial class DatabaseMaintenance
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
            this.grpMain = new System.Windows.Forms.GroupBox();
            this.chkDataSources = new System.Windows.Forms.CheckBox();
            this.chkGlossaryUpdate = new System.Windows.Forms.CheckBox();
            this.pnlProgress = new System.Windows.Forms.Panel();
            this.prbProgress = new System.Windows.Forms.ProgressBar();
            this.pnlButtons = new System.Windows.Forms.Panel();
            this.btnContinue = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkScheduleTasks = new System.Windows.Forms.CheckBox();
            this.chkExportDatabase = new System.Windows.Forms.CheckBox();
            this.chkUpdateDomains = new System.Windows.Forms.CheckBox();
            this.grpMain.SuspendLayout();
            this.pnlProgress.SuspendLayout();
            this.pnlButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpMain
            // 
            this.grpMain.Controls.Add(this.chkDataSources);
            this.grpMain.Controls.Add(this.chkGlossaryUpdate);
            this.grpMain.Controls.Add(this.pnlProgress);
            this.grpMain.Controls.Add(this.pnlButtons);
            this.grpMain.Controls.Add(this.chkScheduleTasks);
            this.grpMain.Controls.Add(this.chkExportDatabase);
            this.grpMain.Controls.Add(this.chkUpdateDomains);
            this.grpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMain.Location = new System.Drawing.Point(0, 0);
            this.grpMain.Name = "grpMain";
            this.grpMain.Size = new System.Drawing.Size(322, 230);
            this.grpMain.TabIndex = 0;
            this.grpMain.TabStop = false;
            this.grpMain.Text = "Which actions do you want to perform?";
            // 
            // chkDataSources
            // 
            this.chkDataSources.AutoSize = true;
            this.chkDataSources.Location = new System.Drawing.Point(12, 98);
            this.chkDataSources.Name = "chkDataSources";
            this.chkDataSources.Size = new System.Drawing.Size(122, 17);
            this.chkDataSources.TabIndex = 9;
            this.chkDataSources.Text = "Check DataSources";
            this.chkDataSources.UseVisualStyleBackColor = true;
            this.chkDataSources.CheckedChanged += new System.EventHandler(this.chkDataSources_CheckedChanged);
            // 
            // chkGlossaryUpdate
            // 
            this.chkGlossaryUpdate.AutoSize = true;
            this.chkGlossaryUpdate.Enabled = false;
            this.chkGlossaryUpdate.Location = new System.Drawing.Point(12, 74);
            this.chkGlossaryUpdate.Name = "chkGlossaryUpdate";
            this.chkGlossaryUpdate.Size = new System.Drawing.Size(153, 17);
            this.chkGlossaryUpdate.TabIndex = 8;
            this.chkGlossaryUpdate.Text = "Update Database Glossary";
            this.chkGlossaryUpdate.UseVisualStyleBackColor = true;
            // 
            // pnlProgress
            // 
            this.pnlProgress.Controls.Add(this.prbProgress);
            this.pnlProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlProgress.Location = new System.Drawing.Point(3, 147);
            this.pnlProgress.Name = "pnlProgress";
            this.pnlProgress.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.pnlProgress.Size = new System.Drawing.Size(316, 27);
            this.pnlProgress.TabIndex = 7;
            // 
            // prbProgress
            // 
            this.prbProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.prbProgress.Location = new System.Drawing.Point(10, 0);
            this.prbProgress.Name = "prbProgress";
            this.prbProgress.Size = new System.Drawing.Size(296, 27);
            this.prbProgress.TabIndex = 0;
            this.prbProgress.Visible = false;
            // 
            // pnlButtons
            // 
            this.pnlButtons.Controls.Add(this.btnContinue);
            this.pnlButtons.Controls.Add(this.btnCancel);
            this.pnlButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtons.Location = new System.Drawing.Point(3, 174);
            this.pnlButtons.Name = "pnlButtons";
            this.pnlButtons.Size = new System.Drawing.Size(316, 53);
            this.pnlButtons.TabIndex = 6;
            // 
            // btnContinue
            // 
            this.btnContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnContinue.Enabled = false;
            this.btnContinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnContinue.Location = new System.Drawing.Point(217, 10);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(90, 34);
            this.btnContinue.TabIndex = 3;
            this.btnContinue.Text = "Continue";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(9, 10);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 34);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkScheduleTasks
            // 
            this.chkScheduleTasks.AutoSize = true;
            this.chkScheduleTasks.Enabled = false;
            this.chkScheduleTasks.Location = new System.Drawing.Point(13, 121);
            this.chkScheduleTasks.Name = "chkScheduleTasks";
            this.chkScheduleTasks.Size = new System.Drawing.Size(208, 17);
            this.chkScheduleTasks.TabIndex = 5;
            this.chkScheduleTasks.Text = "Schedule Regular Maintenance Tasks";
            this.chkScheduleTasks.UseVisualStyleBackColor = true;
            // 
            // chkExportDatabase
            // 
            this.chkExportDatabase.AutoSize = true;
            this.chkExportDatabase.Location = new System.Drawing.Point(12, 51);
            this.chkExportDatabase.Name = "chkExportDatabase";
            this.chkExportDatabase.Size = new System.Drawing.Size(123, 17);
            this.chkExportDatabase.TabIndex = 4;
            this.chkExportDatabase.Text = "Export Geodatabase";
            this.chkExportDatabase.UseVisualStyleBackColor = true;
            this.chkExportDatabase.CheckedChanged += new System.EventHandler(this.chkExportDatabase_CheckedChanged);
            // 
            // chkUpdateDomains
            // 
            this.chkUpdateDomains.AutoSize = true;
            this.chkUpdateDomains.Location = new System.Drawing.Point(12, 28);
            this.chkUpdateDomains.Name = "chkUpdateDomains";
            this.chkUpdateDomains.Size = new System.Drawing.Size(172, 17);
            this.chkUpdateDomains.TabIndex = 3;
            this.chkUpdateDomains.Text = "Update Geodatabase Domains";
            this.chkUpdateDomains.UseVisualStyleBackColor = true;
            this.chkUpdateDomains.CheckedChanged += new System.EventHandler(this.chkUpdateDomains_CheckedChanged);
            // 
            // DatabaseMaintenance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 230);
            this.Controls.Add(this.grpMain);
            this.Name = "DatabaseMaintenance";
            this.ShowIcon = false;
            this.Text = "Database Maintenance Routines";
            this.grpMain.ResumeLayout(false);
            this.grpMain.PerformLayout();
            this.pnlProgress.ResumeLayout(false);
            this.pnlButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpMain;
        private System.Windows.Forms.CheckBox chkScheduleTasks;
        private System.Windows.Forms.CheckBox chkExportDatabase;
        private System.Windows.Forms.CheckBox chkUpdateDomains;
        private System.Windows.Forms.Panel pnlProgress;
        private System.Windows.Forms.ProgressBar prbProgress;
        private System.Windows.Forms.Panel pnlButtons;
        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkGlossaryUpdate;
        private System.Windows.Forms.CheckBox chkDataSources;
    }
}
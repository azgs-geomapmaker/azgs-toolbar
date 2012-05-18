namespace ncgmpToolbar.Forms
{
    partial class azgsSqlDatabaseChooser
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
            this.panelButtons = new System.Windows.Forms.Panel();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.splitter = new System.Windows.Forms.SplitContainer();
            this.groupProjectPicker = new System.Windows.Forms.GroupBox();
            this.listProjects = new System.Windows.Forms.ListBox();
            this.groupVersion = new System.Windows.Forms.GroupBox();
            this.listVersions = new System.Windows.Forms.ListBox();
            this.panelButtons.SuspendLayout();
            this.splitter.Panel1.SuspendLayout();
            this.splitter.Panel2.SuspendLayout();
            this.splitter.SuspendLayout();
            this.groupProjectPicker.SuspendLayout();
            this.groupVersion.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.buttonContinue);
            this.panelButtons.Controls.Add(this.buttonCancel);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 402);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(407, 45);
            this.panelButtons.TabIndex = 0;
            // 
            // buttonContinue
            // 
            this.buttonContinue.Enabled = false;
            this.buttonContinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonContinue.Location = new System.Drawing.Point(285, 3);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(110, 39);
            this.buttonContinue.TabIndex = 2;
            this.buttonContinue.Text = "Continue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(12, 3);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(110, 39);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // splitter
            // 
            this.splitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitter.Location = new System.Drawing.Point(0, 0);
            this.splitter.Name = "splitter";
            // 
            // splitter.Panel1
            // 
            this.splitter.Panel1.Controls.Add(this.groupProjectPicker);
            // 
            // splitter.Panel2
            // 
            this.splitter.Panel2.Controls.Add(this.groupVersion);
            this.splitter.Size = new System.Drawing.Size(407, 402);
            this.splitter.SplitterDistance = 247;
            this.splitter.TabIndex = 2;
            // 
            // groupProjectPicker
            // 
            this.groupProjectPicker.Controls.Add(this.listProjects);
            this.groupProjectPicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupProjectPicker.Location = new System.Drawing.Point(0, 0);
            this.groupProjectPicker.Name = "groupProjectPicker";
            this.groupProjectPicker.Size = new System.Drawing.Size(247, 402);
            this.groupProjectPicker.TabIndex = 2;
            this.groupProjectPicker.TabStop = false;
            this.groupProjectPicker.Text = "Choose your Project";
            // 
            // listProjects
            // 
            this.listProjects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listProjects.FormattingEnabled = true;
            this.listProjects.Location = new System.Drawing.Point(3, 16);
            this.listProjects.Name = "listProjects";
            this.listProjects.Size = new System.Drawing.Size(241, 381);
            this.listProjects.TabIndex = 0;
            this.listProjects.SelectedIndexChanged += new System.EventHandler(this.listProjects_SelectedIndexChanged);
            // 
            // groupVersion
            // 
            this.groupVersion.Controls.Add(this.listVersions);
            this.groupVersion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupVersion.Location = new System.Drawing.Point(0, 0);
            this.groupVersion.Name = "groupVersion";
            this.groupVersion.Size = new System.Drawing.Size(156, 402);
            this.groupVersion.TabIndex = 0;
            this.groupVersion.TabStop = false;
            this.groupVersion.Text = "Choose your Version";
            // 
            // listVersions
            // 
            this.listVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listVersions.FormattingEnabled = true;
            this.listVersions.Location = new System.Drawing.Point(3, 16);
            this.listVersions.Name = "listVersions";
            this.listVersions.Size = new System.Drawing.Size(150, 381);
            this.listVersions.TabIndex = 0;
            this.listVersions.SelectedIndexChanged += new System.EventHandler(this.listVersions_SelectedIndexChanged);
            // 
            // azgsSqlDatabaseChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(407, 447);
            this.Controls.Add(this.splitter);
            this.Controls.Add(this.panelButtons);
            this.Name = "azgsSqlDatabaseChooser";
            this.Text = "Choose an SDE Database";
            this.panelButtons.ResumeLayout(false);
            this.splitter.Panel1.ResumeLayout(false);
            this.splitter.Panel2.ResumeLayout(false);
            this.splitter.ResumeLayout(false);
            this.groupProjectPicker.ResumeLayout(false);
            this.groupVersion.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.SplitContainer splitter;
        private System.Windows.Forms.GroupBox groupProjectPicker;
        private System.Windows.Forms.ListBox listProjects;
        private System.Windows.Forms.GroupBox groupVersion;
        private System.Windows.Forms.ListBox listVersions;
    }
}
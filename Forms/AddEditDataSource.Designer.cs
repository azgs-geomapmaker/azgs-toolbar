namespace ncgmpToolbar.Forms
{
    partial class AddEditDataSource
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddEditDataSource));
            this.tlsMenu = new System.Windows.Forms.ToolStrip();
            this.tlbtnNewRecord = new System.Windows.Forms.ToolStripButton();
            this.tlbtnEditExisting = new System.Windows.Forms.ToolStripButton();
            this.grpSource = new System.Windows.Forms.GroupBox();
            this.txtSource = new System.Windows.Forms.TextBox();
            this.pnlSaveCancel = new System.Windows.Forms.Panel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpNotes = new System.Windows.Forms.GroupBox();
            this.txtNotes = new System.Windows.Forms.TextBox();
            this.tlsMenu.SuspendLayout();
            this.grpSource.SuspendLayout();
            this.pnlSaveCancel.SuspendLayout();
            this.grpNotes.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlsMenu
            // 
            this.tlsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tlbtnNewRecord,
            this.tlbtnEditExisting});
            this.tlsMenu.Location = new System.Drawing.Point(0, 0);
            this.tlsMenu.Name = "tlsMenu";
            this.tlsMenu.Size = new System.Drawing.Size(358, 25);
            this.tlsMenu.TabIndex = 0;
            this.tlsMenu.Text = "toolStrip1";
            // 
            // tlbtnNewRecord
            // 
            this.tlbtnNewRecord.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tlbtnNewRecord.Image = ((System.Drawing.Image)(resources.GetObject("tlbtnNewRecord.Image")));
            this.tlbtnNewRecord.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbtnNewRecord.Name = "tlbtnNewRecord";
            this.tlbtnNewRecord.Size = new System.Drawing.Size(124, 22);
            this.tlbtnNewRecord.Text = "Add a new Data Source";
            this.tlbtnNewRecord.Click += new System.EventHandler(this.tlbtnNewRecord_Click);
            // 
            // tlbtnEditExisting
            // 
            this.tlbtnEditExisting.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tlbtnEditExisting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tlbtnEditExisting.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.tlbtnEditExisting.Image = ((System.Drawing.Image)(resources.GetObject("tlbtnEditExisting.Image")));
            this.tlbtnEditExisting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tlbtnEditExisting.Name = "tlbtnEditExisting";
            this.tlbtnEditExisting.Size = new System.Drawing.Size(155, 22);
            this.tlbtnEditExisting.Text = "Edit selected Data Source";
            this.tlbtnEditExisting.Click += new System.EventHandler(this.tlbtnEditExisting_Click);
            // 
            // grpSource
            // 
            this.grpSource.Controls.Add(this.txtSource);
            this.grpSource.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpSource.Location = new System.Drawing.Point(0, 25);
            this.grpSource.Name = "grpSource";
            this.grpSource.Size = new System.Drawing.Size(358, 41);
            this.grpSource.TabIndex = 0;
            this.grpSource.TabStop = false;
            this.grpSource.Text = "Source";
            // 
            // txtSource
            // 
            this.txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSource.Location = new System.Drawing.Point(3, 16);
            this.txtSource.Name = "txtSource";
            this.txtSource.Size = new System.Drawing.Size(352, 20);
            this.txtSource.TabIndex = 1;
            // 
            // pnlSaveCancel
            // 
            this.pnlSaveCancel.Controls.Add(this.btnSave);
            this.pnlSaveCancel.Controls.Add(this.btnCancel);
            this.pnlSaveCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlSaveCancel.Location = new System.Drawing.Point(0, 236);
            this.pnlSaveCancel.Name = "pnlSaveCancel";
            this.pnlSaveCancel.Size = new System.Drawing.Size(358, 39);
            this.pnlSaveCancel.TabIndex = 2;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Enabled = false;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.Location = new System.Drawing.Point(241, 6);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(105, 28);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(12, 6);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(105, 28);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.TabStop = false;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // grpNotes
            // 
            this.grpNotes.Controls.Add(this.txtNotes);
            this.grpNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpNotes.Location = new System.Drawing.Point(0, 66);
            this.grpNotes.Name = "grpNotes";
            this.grpNotes.Size = new System.Drawing.Size(358, 170);
            this.grpNotes.TabIndex = 0;
            this.grpNotes.TabStop = false;
            this.grpNotes.Text = "Notes";
            // 
            // txtNotes
            // 
            this.txtNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNotes.Location = new System.Drawing.Point(3, 16);
            this.txtNotes.Multiline = true;
            this.txtNotes.Name = "txtNotes";
            this.txtNotes.Size = new System.Drawing.Size(352, 151);
            this.txtNotes.TabIndex = 2;
            this.txtNotes.TextChanged += new System.EventHandler(this.txtNotes_TextChanged);
            // 
            // AddEditDataSource
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 275);
            this.Controls.Add(this.grpNotes);
            this.Controls.Add(this.pnlSaveCancel);
            this.Controls.Add(this.grpSource);
            this.Controls.Add(this.tlsMenu);
            this.Name = "AddEditDataSource";
            this.ShowIcon = false;
            this.Text = "Manage Data Sources";
            this.tlsMenu.ResumeLayout(false);
            this.tlsMenu.PerformLayout();
            this.grpSource.ResumeLayout(false);
            this.grpSource.PerformLayout();
            this.pnlSaveCancel.ResumeLayout(false);
            this.grpNotes.ResumeLayout(false);
            this.grpNotes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tlsMenu;
        private System.Windows.Forms.ToolStripButton tlbtnNewRecord;
        private System.Windows.Forms.ToolStripButton tlbtnEditExisting;
        private System.Windows.Forms.GroupBox grpSource;
        private System.Windows.Forms.TextBox txtSource;
        private System.Windows.Forms.Panel pnlSaveCancel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpNotes;
        private System.Windows.Forms.TextBox txtNotes;
    }
}
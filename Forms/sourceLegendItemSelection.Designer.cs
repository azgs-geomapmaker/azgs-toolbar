namespace ncgmpToolbar.Forms
{
    partial class sourceLegendItemSelection
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
            this.pnlControls = new System.Windows.Forms.Panel();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpSourceLegend = new System.Windows.Forms.GroupBox();
            this.lstSourceLegend = new System.Windows.Forms.ListBox();
            this.pnlControls.SuspendLayout();
            this.grpSourceLegend.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlControls
            // 
            this.pnlControls.Controls.Add(this.btnCopy);
            this.pnlControls.Controls.Add(this.btnCancel);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlControls.Location = new System.Drawing.Point(0, 324);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(397, 49);
            this.pnlControls.TabIndex = 0;
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopy.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopy.Location = new System.Drawing.Point(286, 9);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(99, 29);
            this.btnCopy.TabIndex = 1;
            this.btnCopy.Text = "Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(12, 9);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(99, 29);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // grpSourceLegend
            // 
            this.grpSourceLegend.Controls.Add(this.lstSourceLegend);
            this.grpSourceLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSourceLegend.Location = new System.Drawing.Point(0, 0);
            this.grpSourceLegend.Name = "grpSourceLegend";
            this.grpSourceLegend.Size = new System.Drawing.Size(397, 324);
            this.grpSourceLegend.TabIndex = 1;
            this.grpSourceLegend.TabStop = false;
            // 
            // lstSourceLegend
            // 
            this.lstSourceLegend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSourceLegend.FormattingEnabled = true;
            this.lstSourceLegend.Location = new System.Drawing.Point(3, 16);
            this.lstSourceLegend.Name = "lstSourceLegend";
            this.lstSourceLegend.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstSourceLegend.Size = new System.Drawing.Size(391, 305);
            this.lstSourceLegend.TabIndex = 0;
            // 
            // sourceLegendItemSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 373);
            this.Controls.Add(this.grpSourceLegend);
            this.Controls.Add(this.pnlControls);
            this.Name = "sourceLegendItemSelection";
            this.Text = "Copy Legend Items";
            this.pnlControls.ResumeLayout(false);
            this.grpSourceLegend.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox grpSourceLegend;
        private System.Windows.Forms.ListBox lstSourceLegend;
    }
}
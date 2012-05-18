namespace ncgmpToolbar.Forms
{
    partial class dropChooser
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpSibling = new System.Windows.Forms.GroupBox();
            this.grpChild = new System.Windows.Forms.GroupBox();
            this.picSibling = new System.Windows.Forms.PictureBox();
            this.picChild = new System.Windows.Forms.PictureBox();
            this.chkBelow = new System.Windows.Forms.CheckBox();
            this.chkAbove = new System.Windows.Forms.CheckBox();
            this.btnDropSibling = new System.Windows.Forms.Button();
            this.btnDropChild = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.grpSibling.SuspendLayout();
            this.grpChild.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picSibling)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChild)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(423, 13);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Please specify how the dragged unit/heading is related to the drop target";
            // 
            // grpSibling
            // 
            this.grpSibling.Controls.Add(this.btnDropSibling);
            this.grpSibling.Controls.Add(this.chkAbove);
            this.grpSibling.Controls.Add(this.chkBelow);
            this.grpSibling.Controls.Add(this.picSibling);
            this.grpSibling.Location = new System.Drawing.Point(12, 25);
            this.grpSibling.Name = "grpSibling";
            this.grpSibling.Size = new System.Drawing.Size(189, 213);
            this.grpSibling.TabIndex = 1;
            this.grpSibling.TabStop = false;
            this.grpSibling.Text = "Add the Unit/Heading as a sibling";
            // 
            // grpChild
            // 
            this.grpChild.Controls.Add(this.btnDropChild);
            this.grpChild.Controls.Add(this.picChild);
            this.grpChild.Location = new System.Drawing.Point(247, 25);
            this.grpChild.Name = "grpChild";
            this.grpChild.Size = new System.Drawing.Size(189, 213);
            this.grpChild.TabIndex = 2;
            this.grpChild.TabStop = false;
            this.grpChild.Text = "Add the Unit/Heading as a child";
            // 
            // picSibling
            // 
            this.picSibling.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picSibling.Image = global::ncgmpToolbar.Properties.Resources.SibDrop;
            this.picSibling.Location = new System.Drawing.Point(8, 19);
            this.picSibling.Name = "picSibling";
            this.picSibling.Size = new System.Drawing.Size(173, 98);
            this.picSibling.TabIndex = 0;
            this.picSibling.TabStop = false;
            // 
            // picChild
            // 
            this.picChild.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picChild.Image = global::ncgmpToolbar.Properties.Resources.ChildDrop;
            this.picChild.Location = new System.Drawing.Point(7, 19);
            this.picChild.Name = "picChild";
            this.picChild.Size = new System.Drawing.Size(176, 100);
            this.picChild.TabIndex = 0;
            this.picChild.TabStop = false;
            // 
            // chkBelow
            // 
            this.chkBelow.AutoSize = true;
            this.chkBelow.Location = new System.Drawing.Point(22, 123);
            this.chkBelow.Name = "chkBelow";
            this.chkBelow.Size = new System.Drawing.Size(144, 17);
            this.chkBelow.TabIndex = 1;
            this.chkBelow.Text = "Insert Below Drop Target";
            this.chkBelow.UseVisualStyleBackColor = true;
            this.chkBelow.CheckedChanged += new System.EventHandler(this.chkBelow_CheckedChanged);
            // 
            // chkAbove
            // 
            this.chkAbove.AutoSize = true;
            this.chkAbove.Location = new System.Drawing.Point(21, 146);
            this.chkAbove.Name = "chkAbove";
            this.chkAbove.Size = new System.Drawing.Size(146, 17);
            this.chkAbove.TabIndex = 2;
            this.chkAbove.Text = "Insert Above Drop Target";
            this.chkAbove.UseVisualStyleBackColor = true;
            this.chkAbove.CheckedChanged += new System.EventHandler(this.chkAbove_CheckedChanged);
            // 
            // btnDropSibling
            // 
            this.btnDropSibling.Location = new System.Drawing.Point(20, 172);
            this.btnDropSibling.Name = "btnDropSibling";
            this.btnDropSibling.Size = new System.Drawing.Size(148, 28);
            this.btnDropSibling.TabIndex = 3;
            this.btnDropSibling.Text = "Drop as Sibling";
            this.btnDropSibling.UseVisualStyleBackColor = true;
            this.btnDropSibling.Click += new System.EventHandler(this.btnDropSibling_Click);
            // 
            // btnDropChild
            // 
            this.btnDropChild.Location = new System.Drawing.Point(21, 172);
            this.btnDropChild.Name = "btnDropChild";
            this.btnDropChild.Size = new System.Drawing.Size(148, 28);
            this.btnDropChild.TabIndex = 1;
            this.btnDropChild.Text = "Drop as Child";
            this.btnDropChild.UseVisualStyleBackColor = true;
            this.btnDropChild.Click += new System.EventHandler(this.btnDropChild_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(149, 253);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(148, 28);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // dropChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(440, 289);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.grpChild);
            this.Controls.Add(this.grpSibling);
            this.Controls.Add(this.lblTitle);
            this.Name = "dropChooser";
            this.ShowIcon = false;
            this.Text = "Choose a Drop CreateFeature";
            this.grpSibling.ResumeLayout(false);
            this.grpSibling.PerformLayout();
            this.grpChild.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picSibling)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChild)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpSibling;
        private System.Windows.Forms.GroupBox grpChild;
        private System.Windows.Forms.Button btnDropSibling;
        private System.Windows.Forms.CheckBox chkAbove;
        private System.Windows.Forms.CheckBox chkBelow;
        private System.Windows.Forms.PictureBox picSibling;
        private System.Windows.Forms.Button btnDropChild;
        private System.Windows.Forms.PictureBox picChild;
        private System.Windows.Forms.Button btnCancel;
    }
}
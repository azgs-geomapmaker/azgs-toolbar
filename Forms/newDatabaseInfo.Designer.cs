namespace ncgmpToolbar.Forms
{
    partial class newDatabaseInfo
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
            this.groupInfo = new System.Windows.Forms.GroupBox();
            this.panelEnterInfo = new System.Windows.Forms.Panel();
            this.groupAbbr = new System.Windows.Forms.GroupBox();
            this.textAbbr = new System.Windows.Forms.TextBox();
            this.groupProjectTitle = new System.Windows.Forms.GroupBox();
            this.textTitle = new System.Windows.Forms.TextBox();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.groupInfo.SuspendLayout();
            this.panelEnterInfo.SuspendLayout();
            this.groupAbbr.SuspendLayout();
            this.groupProjectTitle.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupInfo
            // 
            this.groupInfo.Controls.Add(this.panelEnterInfo);
            this.groupInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupInfo.Location = new System.Drawing.Point(0, 0);
            this.groupInfo.Name = "groupInfo";
            this.groupInfo.Size = new System.Drawing.Size(290, 226);
            this.groupInfo.TabIndex = 0;
            this.groupInfo.TabStop = false;
            this.groupInfo.Text = "Please Fill In This Information About Your New Database";
            // 
            // panelEnterInfo
            // 
            this.panelEnterInfo.Controls.Add(this.groupAbbr);
            this.panelEnterInfo.Controls.Add(this.groupProjectTitle);
            this.panelEnterInfo.Controls.Add(this.buttonContinue);
            this.panelEnterInfo.Location = new System.Drawing.Point(34, 41);
            this.panelEnterInfo.Name = "panelEnterInfo";
            this.panelEnterInfo.Size = new System.Drawing.Size(222, 153);
            this.panelEnterInfo.TabIndex = 4;
            // 
            // groupAbbr
            // 
            this.groupAbbr.Controls.Add(this.textAbbr);
            this.groupAbbr.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupAbbr.Location = new System.Drawing.Point(0, 66);
            this.groupAbbr.Name = "groupAbbr";
            this.groupAbbr.Size = new System.Drawing.Size(222, 43);
            this.groupAbbr.TabIndex = 11;
            this.groupAbbr.TabStop = false;
            this.groupAbbr.Text = "Three-Letter Project Abbreviation";
            // 
            // textAbbr
            // 
            this.textAbbr.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.textAbbr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textAbbr.Location = new System.Drawing.Point(3, 16);
            this.textAbbr.MaxLength = 3;
            this.textAbbr.Name = "textAbbr";
            this.textAbbr.Size = new System.Drawing.Size(216, 20);
            this.textAbbr.TabIndex = 1;
            this.textAbbr.TextChanged += new System.EventHandler(this.textAbbr_TextChanged);
            // 
            // groupProjectTitle
            // 
            this.groupProjectTitle.Controls.Add(this.textTitle);
            this.groupProjectTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupProjectTitle.Location = new System.Drawing.Point(0, 0);
            this.groupProjectTitle.Name = "groupProjectTitle";
            this.groupProjectTitle.Size = new System.Drawing.Size(222, 66);
            this.groupProjectTitle.TabIndex = 10;
            this.groupProjectTitle.TabStop = false;
            this.groupProjectTitle.Text = "Project Name";
            // 
            // textTitle
            // 
            this.textTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textTitle.Location = new System.Drawing.Point(3, 16);
            this.textTitle.Multiline = true;
            this.textTitle.Name = "textTitle";
            this.textTitle.Size = new System.Drawing.Size(216, 47);
            this.textTitle.TabIndex = 0;
            // 
            // buttonContinue
            // 
            this.buttonContinue.Enabled = false;
            this.buttonContinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonContinue.Location = new System.Drawing.Point(48, 115);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(124, 33);
            this.buttonContinue.TabIndex = 2;
            this.buttonContinue.Text = "Continue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // newDatabaseInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 226);
            this.Controls.Add(this.groupInfo);
            this.Name = "newDatabaseInfo";
            this.Text = "New Database";
            this.groupInfo.ResumeLayout(false);
            this.panelEnterInfo.ResumeLayout(false);
            this.groupAbbr.ResumeLayout(false);
            this.groupAbbr.PerformLayout();
            this.groupProjectTitle.ResumeLayout(false);
            this.groupProjectTitle.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupInfo;
        private System.Windows.Forms.Panel panelEnterInfo;
        private System.Windows.Forms.GroupBox groupProjectTitle;
        private System.Windows.Forms.TextBox textTitle;
        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.GroupBox groupAbbr;
        private System.Windows.Forms.TextBox textAbbr;
    }
}
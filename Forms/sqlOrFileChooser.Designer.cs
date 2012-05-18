namespace ncgmpToolbar.Forms
{
    partial class sqlOrFileChooser
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
            this.buttonContinue = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.checkWriteReg = new System.Windows.Forms.CheckBox();
            this.radioFile = new System.Windows.Forms.RadioButton();
            this.radioSQL = new System.Windows.Forms.RadioButton();
            this.grpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpMain
            // 
            this.grpMain.Controls.Add(this.buttonContinue);
            this.grpMain.Controls.Add(this.buttonCancel);
            this.grpMain.Controls.Add(this.checkWriteReg);
            this.grpMain.Controls.Add(this.radioFile);
            this.grpMain.Controls.Add(this.radioSQL);
            this.grpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpMain.Location = new System.Drawing.Point(0, 0);
            this.grpMain.Name = "grpMain";
            this.grpMain.Size = new System.Drawing.Size(528, 139);
            this.grpMain.TabIndex = 0;
            this.grpMain.TabStop = false;
            this.grpMain.Text = "Please choose the appropriate option";
            // 
            // buttonContinue
            // 
            this.buttonContinue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonContinue.Location = new System.Drawing.Point(404, 99);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(101, 28);
            this.buttonContinue.TabIndex = 3;
            this.buttonContinue.Text = "Continue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(12, 99);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(101, 28);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // checkWriteReg
            // 
            this.checkWriteReg.AutoSize = true;
            this.checkWriteReg.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkWriteReg.Location = new System.Drawing.Point(123, 65);
            this.checkWriteReg.Name = "checkWriteReg";
            this.checkWriteReg.Size = new System.Drawing.Size(281, 17);
            this.checkWriteReg.TabIndex = 2;
            this.checkWriteReg.Text = "Use my current selection and never ask me this again.";
            this.checkWriteReg.UseVisualStyleBackColor = true;
            // 
            // radioFile
            // 
            this.radioFile.AutoSize = true;
            this.radioFile.Location = new System.Drawing.Point(22, 42);
            this.radioFile.Name = "radioFile";
            this.radioFile.Size = new System.Drawing.Size(298, 17);
            this.radioFile.TabIndex = 1;
            this.radioFile.TabStop = true;
            this.radioFile.Text = "I would like to connect to a File or Personal Geodatabase.";
            this.radioFile.UseVisualStyleBackColor = true;
            this.radioFile.CheckedChanged += new System.EventHandler(this.radioFile_CheckedChanged);
            // 
            // radioSQL
            // 
            this.radioSQL.AutoSize = true;
            this.radioSQL.Location = new System.Drawing.Point(22, 19);
            this.radioSQL.Name = "radioSQL";
            this.radioSQL.Size = new System.Drawing.Size(483, 17);
            this.radioSQL.TabIndex = 0;
            this.radioSQL.TabStop = true;
            this.radioSQL.Text = "I am an AZGS Employee in the Tucson office and would like to connect to an ArcSDE" +
                " Database.";
            this.radioSQL.UseVisualStyleBackColor = true;
            this.radioSQL.CheckedChanged += new System.EventHandler(this.radioSQL_CheckedChanged);
            // 
            // sqlOrFileChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(528, 139);
            this.Controls.Add(this.grpMain);
            this.Name = "sqlOrFileChooser";
            this.Text = "Connect to a Database";
            this.grpMain.ResumeLayout(false);
            this.grpMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpMain;
        private System.Windows.Forms.RadioButton radioSQL;
        private System.Windows.Forms.RadioButton radioFile;
        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkWriteReg;

        
    }
}
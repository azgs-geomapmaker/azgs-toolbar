using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ncgmpToolbar.Forms
{
    public partial class newDatabaseInfo : Form
    {
        public string projTitle;
        public string projAbbr;

        public newDatabaseInfo()
        {
            InitializeComponent();
        }

        private void textAbbr_TextChanged(object sender, EventArgs e)
        {
            // Check to see that both fields are populated, if so enable the continue button
            if (this.textTitle.TextLength > 0)
            {
                if (this.textAbbr.TextLength == 3)
                {
                    this.buttonContinue.Enabled = true;
                }
                else
                {
                    this.buttonContinue.Enabled = false;
                }
            }
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            this.projAbbr = this.textAbbr.Text;
            this.projTitle = this.textTitle.Text;
            this.Hide();
        }
    }
}

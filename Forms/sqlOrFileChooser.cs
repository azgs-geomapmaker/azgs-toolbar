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
    public partial class sqlOrFileChooser : Form
    {
        public string m_getSqlDatabase = "True";
        public Boolean m_canceled = false;
        public Boolean m_writeReg = false;

        public sqlOrFileChooser()
        {
            InitializeComponent();
            radioSQL.Checked = true;
        }

        private void radioSQL_CheckedChanged(object sender, EventArgs e)
        {
            // Turn on/off the other radio button accordingly
            if (radioSQL.Checked == true)
            {
                if (radioFile.Checked == true)
                {
                    radioFile.Checked = false;
                }
            }
            else
            {
                if (radioFile.Checked == false)
                {
                    radioFile.Checked = true;
                }
            }
        }

        private void radioFile_CheckedChanged(object sender, EventArgs e)
        {
            // Turn on/off the other radio button accordingly
            if (radioFile.Checked == true)
            {
                if (radioSQL.Checked == true)
                {
                    radioSQL.Checked = false;
                }
            }
            else
            {
                if (radioSQL.Checked == false)
                {
                    radioSQL.Checked = true;
                }
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            //Hide the form, set the canceled variable
            m_canceled = true;
            this.Hide();
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            //Set public variables appropriately, hide the form
            if (radioFile.Checked == true)
            {
                m_getSqlDatabase = "False";
            }

            if (checkWriteReg.Checked == true)
            {
                m_writeReg = true;
            }

            this.Hide();
        }
    }
}

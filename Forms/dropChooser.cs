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
    public partial class dropChooser : Form
    {
        public dropChooser()
        {
            InitializeComponent();

            m_canceled = false;
            m_dropBelow = true;
            m_dropAsChild = true;
            chkBelow.Checked = true;
        }

        private bool m_dropAsChild;
        public bool DropAsChild
        {
            get { return m_dropAsChild; }
        }

        private bool m_dropBelow;
        public bool DropBelow
        {
            get { return m_dropBelow; }
        }

        private bool m_canceled;
        public bool Canceled
        {
            get { return m_canceled; }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            m_canceled = true;
            this.Hide();
        }

        private void btnDropSibling_Click(object sender, EventArgs e)
        {
            m_dropAsChild = false;
            m_dropBelow = chkBelow.Checked;
            m_canceled = false;
            this.Hide();
        }

        private void btnDropChild_Click(object sender, EventArgs e)
        {
            m_dropAsChild = true;
            m_canceled = false;
            this.Hide();
        }

        private void chkBelow_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBelow.Checked == true) { chkAbove.Checked = false; }
        }

        private void chkAbove_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAbove.Checked == true) { chkBelow.Checked = false; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Editor;
using ncgmpToolbar.Utilities.DatabaseMaintenance;

namespace ncgmpToolbar.Forms
{
    public partial class DatabaseMaintenance : Form
    {
        private IWorkspace m_theWorkspace;

        public DatabaseMaintenance(IWorkspace theWorkspace)
        {
            m_theWorkspace = theWorkspace;
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            // Turn off edit session
            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditWorkspace.Equals(m_theWorkspace) && (theEditor.EditState == esriEditState.esriStateEditing)) { theEditor.StopEditing(true); }

            // Do whatever has been checked
            if (chkUpdateDomains.Checked == true) { domainUpdater.UpdateDomains(m_theWorkspace); }
            if (chkExportDatabase.Checked == true) { databaseExporter.ExportDatabase(m_theWorkspace); }
            if (chkDataSources.Checked == true) { dataSourceChecker.CheckForMissingDataSources(m_theWorkspace); }
            // Close the form
            this.Close();
        }

        private void chkUpdateDomains_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUpdateDomains.Checked == true) { btnContinue.Enabled = true; }
            else
            {
                if ((chkDataSources.Checked == false) && (chkExportDatabase.Checked == false) && (chkScheduleTasks.Checked == false)) { btnContinue.Enabled = false; }
            }
        }

        private void chkExportDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (chkExportDatabase.Checked == true) { btnContinue.Enabled = true; }
            else
            {
                if ((chkDataSources.Checked == false) && (chkUpdateDomains.Checked == false) && (chkScheduleTasks.Checked == false)) { btnContinue.Enabled = false; }
            }
        }

        private void chkDataSources_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDataSources.Checked == true) { btnContinue.Enabled = true; }
            else
            {
                if ((chkUpdateDomains.Checked == false) && (chkScheduleTasks.Checked == false) && (chkExportDatabase.Checked == false)) { btnContinue.Enabled = false; }
            }
        }
    }
}

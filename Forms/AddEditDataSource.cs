using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ncgmpToolbar.Utilities;
using ncgmpToolbar.Utilities.DataAccess;
using ESRI.ArcGIS.Geodatabase;

namespace ncgmpToolbar.Forms
{
    public partial class AddEditDataSource : Form
    {
        private string m_selectedDataSourceID;
        public string selectedDataSourceID
        {
            get { return m_selectedDataSourceID; }
        }

        private bool m_canceled = true;
        public bool WasCanceled
        {
            get { return m_canceled; }
        }

        private DataSourcesAccess.Datasource m_workingDataSource;
        private bool m_inUpdateMode;
        private DataSourcesAccess m_dataSourceAccess;
        private IWorkspace m_theWorkspace;
        private Font m_boldFont;
        private Font m_notBoldFont;
        
        public AddEditDataSource(IWorkspace theWorkspace, string selectedDataSourceID = null)
        {
            InitializeComponent();
            
            // Set Class-level variables
            m_theWorkspace = theWorkspace;
            m_dataSourceAccess = new DataSourcesAccess(theWorkspace);

            // Check to see if a Data Source is selected. If not, adjust form for new DataSource creation
            m_selectedDataSourceID = selectedDataSourceID;

            m_boldFont = (Font)tlbtnEditExisting.Font.Clone();
            m_notBoldFont = new Font(m_boldFont, FontStyle.Regular);

            if (m_selectedDataSourceID == null)
            {
                // Set the fonts appropriately in the form. Nothing to populate.
                tlbtnEditExisting.Font = m_notBoldFont;
                tlbtnNewRecord.Font = m_boldFont;
                tlbtnEditExisting.Enabled = false;
                tlbtnNewRecord.Enabled = false;
                btnSave.Enabled = false;

                m_inUpdateMode = false;
            }
            else
            {
                // Fonts default appropriately. Load Source and Notes from the selected record.
                //m_currentDataSource = new DataSourceRecord(m_theWorkspace, m_selectedDataSourceID);
                m_dataSourceAccess.AddDataSources("DataSources_ID = '" + m_selectedDataSourceID + "'");
                DataSourcesAccess.Datasource thisDataSource = m_dataSourceAccess.DataSourceCollection[m_selectedDataSourceID];

                txtSource.Text = thisDataSource.Source;
                txtNotes.Text = thisDataSource.Notes;

                // Enable the save button
                btnSave.Enabled = true;

                m_inUpdateMode = true;
            }
        }

        private void tlbtnNewRecord_Click(object sender, EventArgs e)
        {
            // Setup the form for a new record
            tlbtnNewRecord.Font = m_boldFont;
            tlbtnEditExisting.Font = m_notBoldFont;
            txtNotes.Text = null;
            txtSource.Text = null;
            m_inUpdateMode = false;
            btnSave.Enabled = false;
        }

        private void tlbtnEditExisting_Click(object sender, EventArgs e)
        {
            // Setup the form for an existing record
            tlbtnNewRecord.Font = m_notBoldFont;
            tlbtnEditExisting.Font = m_boldFont;
            btnSave.Enabled = true;

            // Get the DataSource in order to populate the fields
            DataSourcesAccess.Datasource thisDataSource = m_dataSourceAccess.DataSourceCollection[m_selectedDataSourceID];
            txtSource.Text = thisDataSource.Source;
            txtNotes.Text = thisDataSource.Notes;

            m_inUpdateMode = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Set the Canceled flag, close the form
            m_canceled = true;
            this.Hide();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Determine if we'll be inserting or updating
            switch (m_inUpdateMode)
            {
                case true:
                    // Perform an update on the selected DataSource - First populate a DataSource structure
                    DataSourcesAccess.Datasource thisDataSource = m_dataSourceAccess.DataSourceCollection[m_selectedDataSourceID];
                    thisDataSource.Source = txtSource.Text;
                    thisDataSource.Notes = txtNotes.Text;

                    // Then perform the update and save
                    m_dataSourceAccess.UpdateDataSource(thisDataSource);
                    m_dataSourceAccess.SaveDataSources();
                    break;

                case false:
                    // Perform an insert. This is done directly through a method on the DataSourceAccess object
                    m_dataSourceAccess.NewDataSource(txtSource.Text, txtNotes.Text);
                    m_dataSourceAccess.SaveDataSources();
                    break;
            }

            // Set the Canceled flag, hide the form
            m_canceled = false;
            this.Hide();
        }

        private void txtNotes_TextChanged(object sender, EventArgs e)
        {
            // If this m_identifier is changed, check the content of the other m_identifier. If both are populated then enable the save button.
            if (txtNotes.Text.Length > 0)
            {
                if (txtSource.Text.Length > 0) { btnSave.Enabled = true; }
                else { btnSave.Enabled = false; }
            }
            else {btnSave.Enabled = false; }
        }


    }
}

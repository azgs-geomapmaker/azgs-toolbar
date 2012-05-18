using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoDatabaseUI;
using ncgmpToolbar.Utilities;

namespace ncgmpToolbar.Forms
{
    public partial class azgsSqlDatabaseChooser : Form
    {
        public string databaseName = null;
        public string versionName = null;

        public azgsSqlDatabaseChooser()
        {
            InitializeComponent();

            // Populate the listbox

            // Build a DataTable to bind to the listbox control
            DataTable projectTable = new DataTable();

            DataColumn projName = new DataColumn();
            projName.ColumnName = "ProjectName";
            projName.DataType = typeof(string);

            DataColumn dbName = new DataColumn();
            dbName.ColumnName = "DatabaseName";
            dbName.DataType = typeof(string);

            projectTable.Columns.Add(projName);
            projectTable.Columns.Add(dbName);

            // Populate the DataTable - Right now this is pinging a DB on malachite
            IPropertySet connectionProperties = new PropertySetClass();
            connectionProperties.SetProperty("SERVER", "malachite\\azgsgeodatabases");
            connectionProperties.SetProperty("INSTANCE", "sde:sqlserver:malachite\\azgsgeodatabases");
            connectionProperties.SetProperty("DATABASE", "AzgsIndex");
            connectionProperties.SetProperty("AUTHENTICATION_MODE", "OSA");
            connectionProperties.SetProperty("VERSION", "dbo.Default");

            try
            {
                IWorkspaceFactory wsFact = new SdeWorkspaceFactoryClass();
                IWorkspace theWs = wsFact.Open(connectionProperties, 0);
            
                // Open the table in the repository database
                ITable ProjectListingsTable = commonFunctions.OpenTable(theWs, "ProjectDatabases");

                // Get all the records into a sorted cursor
                ITableSort projSorter = new TableSortClass();
                projSorter.Table = ProjectListingsTable;
                projSorter.QueryFilter = null;
                projSorter.Fields = "ProjectName";
                projSorter.set_Ascending("ProjectName", true);
                projSorter.Sort(null);
                ICursor projCur = projSorter.Rows; //ProjectListingsTable.Search(null, false);

                // Loop through the cursor and add records to the DataTable
                IRow projRow = projCur.NextRow();
                while (projRow != null)
            {
                projectTable.Rows.Add((String)projRow.get_Value(ProjectListingsTable.FindField("ProjectName")), (String)projRow.get_Value(ProjectListingsTable.FindField("DatabaseName")));
                projRow = projCur.NextRow();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(projCur);    
             
            // Bind the DataTable to the control
            this.listProjects.DataSource = projectTable;            
            this.listProjects.DisplayMember = "ProjectName";
            this.listProjects.ValueMember = "DatabaseName";
            this.listProjects.SelectedItem = null;
            this.listVersions.DataSource = null;
            }
            catch (Exception ex)
            { MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace); return; }

        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            // Grab the DatabaseName from the listbox
            databaseName = (string)this.listProjects.SelectedValue;

            // Grab the VersionName from the other listbox
            versionName = (string)this.listVersions.SelectedValue;

            this.Hide();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            //Hide the form, null variables will be checked
            this.Hide();
        }

        private void listProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Need to clear the versions when selected
            this.listVersions.DataSource = null;

            string selectedProject = null;
            try
            {
                selectedProject = (string)this.listProjects.SelectedValue;
            }
            catch { return; }

            if (selectedProject == null) { return; }

            // Prompt the user to select a version. Open the database
            IPropertySet connectionProperties = new PropertySetClass();
            connectionProperties.SetProperty("SERVER", "malachite\\azgsgeodatabases");
            connectionProperties.SetProperty("INSTANCE", "sde:sqlserver:malachite\\azgsgeodatabases");
            connectionProperties.SetProperty("DATABASE", selectedProject);
            connectionProperties.SetProperty("AUTHENTICATION_MODE", "OSA");
            connectionProperties.SetProperty("VERSION", "dbo.Default");

            IWorkspaceFactory wsFact = new SdeWorkspaceFactoryClass();
            IVersionedWorkspace vWs = (IVersionedWorkspace)wsFact.Open(connectionProperties, 0);

            // Build a DataTable to bind to the listbox control
            DataTable verTable = new DataTable();

            DataColumn verName = new DataColumn();
            verName.ColumnName = "VersionName";
            verName.DataType = typeof(string);

            verTable.Columns.Add(verName);

            IEnumVersionInfo theseVersions = vWs.Versions;
            IVersionInfo aVersion = theseVersions.Next();
            while (aVersion != null)
            {
                string thisVersionName = (string)aVersion.VersionName;
                string[] Split = thisVersionName.Split(new char[] { '.' });
                verTable.Rows.Add(Split[1]);
                aVersion = theseVersions.Next();
            }

            this.listVersions.DataSource = verTable;
            this.listVersions.DisplayMember = "VersionName";
            this.listVersions.ValueMember = "VersionName";
        }

        private void listVersions_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selectedProject = (string)this.listProjects.SelectedValue;
                string selectedVersion = (string)this.listProjects.SelectedValue;
            }
            catch
            {
                this.buttonContinue.Enabled = false;
                return;
            }

            this.buttonContinue.Enabled = true;
        }
    }
}

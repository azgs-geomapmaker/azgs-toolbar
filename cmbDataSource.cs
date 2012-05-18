using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ncgmpToolbar.Utilities;

namespace ncgmpToolbar
{
    public class cmbDataSource : ESRI.ArcGIS.Desktop.AddIns.ComboBox
    {
        private IEditor m_Editor = ArcMap.Editor;
        private IWorkspace m_EditWorkspace;
        private bool m_DatabaseIsValid = false;

        private IEditEvents_Event Events
        {
            get { return ArcMap.Editor as IEditEvents_Event; }
        }

        public cmbDataSource()
        {
            // Set up listeners for edit events. This allows the combobox to populate itself when an edit session starts.
            Events.OnStartEditing += delegate() { OnStartEditing(); };
            Events.OnStopEditing += delegate(bool save) { OnStopEditing(save); };
        }

        protected override void OnUpdate()
        {
            // Should only be enabled if we are editing a valid NCGMP database
            bool pleaseEnabled = ArcMap.Editor.EditState == esriEditState.esriStateEditing;
            if (pleaseEnabled == true) { Enabled = ncgmpEditorExtension.g_EditWorkspaceIsValid; } else { Enabled = false; }
        }

        void OnStartEditing()
        {
            // Check that the workspace being edited is NCGMP-valid.
            m_EditWorkspace = m_Editor.EditWorkspace;
            m_DatabaseIsValid = ncgmpChecks.IsWorkspaceMinNCGMPCompliant(m_EditWorkspace);

            if (m_DatabaseIsValid == true)
            {
                PopulateComboboxContents();
            }
        }

        protected override void OnSelChange(int cookie)
        {            
            try
            {
                // The GetItem function gets me access to the object added to the combobox along with the caption.
                //  Apparently you access the object through the Tag property.
                globalVariables.currentDataSource = GetItem(cookie).Tag as string;
            }
            catch
            {
                globalVariables.currentDataSource = null;                
            }

            // This has to sit here at the end of the overridden function
            base.OnSelChange(cookie);
        }

        void OnStopEditing(bool save)
        {
            // Empty the Combobox
            Clear();
        }

        public void PopulateComboboxContents()
        {
            // Clear the combobox first
            Clear();

            // Get a reference to the Data Sources Table
            ITable dataSourcesTable = commonFunctions.OpenTable(m_EditWorkspace, "DataSources");

            // Get all the Data Sources into a cursor, sorted alphabetically
            ITableSort dsSorter = new TableSortClass();
            dsSorter.Table = dataSourcesTable;
            dsSorter.QueryFilter = null;
            dsSorter.Fields = "Source";
            dsSorter.set_Ascending("Source", true);
            // Seem to have trouble sorting. I suspect it has to do with this being a >255 char m_identifier? Should Source be > 255??
            dsSorter.Sort(null);
            ICursor sortedSources = dsSorter.Rows;
            //ICursor sortedSources = dataSourcesTable.Search(null, false);

            // Iterate through the returned, sorted rows. Add each one to the DataTable
            IRow aSource = sortedSources.NextRow();
            while (aSource != null)
            {
                Add(aSource.get_Value(dataSourcesTable.FindField("Source")).ToString(), aSource.get_Value(dataSourcesTable.FindField("DataSources_ID")).ToString());
                aSource = sortedSources.NextRow();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(sortedSources);
        }
    }

}

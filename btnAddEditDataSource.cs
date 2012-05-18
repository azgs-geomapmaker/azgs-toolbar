using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ncgmpToolbar.Forms;
using ncgmpToolbar.Utilities;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Editor;


namespace ncgmpToolbar
{
    public class btnAddEditDataSource : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public btnAddEditDataSource()
        {
        }

        protected override void OnClick()
        {
            AddEditDataSource datasourceForm = new AddEditDataSource(ArcMap.Editor.EditWorkspace, globalVariables.currentDataSource);
            datasourceForm.ShowDialog();

            // Find out if we were canceled
            if (datasourceForm.WasCanceled == false)
            {
                // Adjustments were made. Repopulate the Combobox!
                var dataSourceCombobox = AddIn.FromID <cmbDataSource> (ThisAddIn.IDs.cmbDataSource);
                dataSourceCombobox.PopulateComboboxContents();                
            }
        }

        protected override void OnUpdate()
        {
            // Should only be enabled if we are editing a valid NCGMP database
            bool pleaseEnabled = ArcMap.Editor.EditState == esriEditState.esriStateEditing;
            if (pleaseEnabled == true) { Enabled = ncgmpEditorExtension.g_EditWorkspaceIsValid; } else { Enabled = false; }
        }
    }
}

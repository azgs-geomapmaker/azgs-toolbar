using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ncgmpToolbar.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Editor;


namespace ncgmpToolbar
{
    public class btnDatabaseMaintenance : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public btnDatabaseMaintenance()
        {
        }

        protected override void OnClick()
        {
            // Get a Workspace reference, somehow
            // For now, gets it from the edit session
            IWorkspace theWorkspace = ArcMap.Editor.EditWorkspace;
            DatabaseMaintenance maintenanceForm = new DatabaseMaintenance(theWorkspace);
            maintenanceForm.ShowDialog();
        }

        protected override void OnUpdate()
        {
            // Should only be available when NOT editing. However, until I think of how to get the workspace reference above,
            //  it works inside an edit session, shuts down the edit session and then performs maintenance.
            bool pleaseEnabled = ArcMap.Editor.EditState == esriEditState.esriStateEditing;
            if (pleaseEnabled == true) { Enabled = ncgmpEditorExtension.g_EditWorkspaceIsValid; } else { Enabled = false; }
        }
    }
}

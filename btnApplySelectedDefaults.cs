using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Editor;
using ncgmpToolbar.Forms;

namespace ncgmpToolbar
{
    public class btnApplySelectedDefaults : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public btnApplySelectedDefaults()
        {
        }

        protected override void OnClick()
        {
            updateAttributes updateForm = new updateAttributes(ArcMap.Editor.EditWorkspace);
            updateForm.ShowDialog();
            updateForm.Close();
        }

        protected override void OnUpdate()
        {
            // Should only be enabled if we are editing a valid NCGMP database
            bool pleaseEnabled = ArcMap.Editor.EditState == esriEditState.esriStateEditing;
            if (pleaseEnabled == true) { Enabled = ncgmpEditorExtension.g_EditWorkspaceIsValid; } else { Enabled = false; }
        }
    }
}

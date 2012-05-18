using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Editor;
using System.Windows.Forms;

namespace ncgmpToolbar
{
    public class btnShowMapUnitLegendEditor : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public btnShowMapUnitLegendEditor()
        {
        }

        protected override void OnClick()
        {
            UID theUid = new UIDClass();
            theUid.Value = ThisAddIn.IDs.dwnMapUnitLegendEditor;
            
            IDockableWindow mapUnitForm = ArcMap.DockableWindowManager.GetDockableWindow(theUid);
            if (mapUnitForm.IsVisible() == false) { mapUnitForm.Show(true); }
        }

        protected override void OnUpdate()
        {
            // Should only be enabled if we are editing a valid NCGMP database
            bool pleaseEnabled = ArcMap.Editor.EditState == esriEditState.esriStateEditing;
            if (pleaseEnabled == true) { Enabled = ncgmpEditorExtension.g_EditWorkspaceIsValid; } else { Enabled = false; }
        }
    }
}

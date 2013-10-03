using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ncgmpToolbar.Utilities;
using ncgmpToolbar.Utilities.DataAccess;
using ncgmpToolbar.Forms;

namespace ncgmpToolbar
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains user interfaces that
    /// make up the dockable window.
    /// </summary>
    ///     

    public partial class dwnMapUnitLegendEditor : UserControl
    {
        IWorkspace m_theWorkspace;
        bool m_ThisIsAnUpdate = true;
        string m_theOldMapUnitName;

        // Some logic here which tries to adjust depending on the edit state/ncgmp validity
        public dwnMapUnitLegendEditor(object hook)
        {
            InitializeComponent();
            this.Hook = hook;

            // Check for edit state
            // -------------------------------------------------------------------
            // -------------------------------------------------------------------
            // Note: This seems to be getting called at wierd times, like the first time you save some edits
            // -------------------------------------------------------------------
            // -------------------------------------------------------------------
            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateEditing)
            {
                m_theWorkspace = theEditor.EditWorkspace;
                if (ncgmpChecks.IsWorkspaceMinNCGMPCompliant(m_theWorkspace) == true) 
                {
                    sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
                    this.tlslblLegendName.Text = SysInfoTable.ProjName;
                    PopulateMainLegendTree(); 
                }
                else
                {
                    this.tlslblLegendName.Text = "Not Editing.";
                    ClearMainLegendTree();
                }
            }
            else
            {
                this.tlslblLegendName.Text = "Not Editing.";
                ClearMainLegendTree();
            }

            if (trvLegendItems.SelectedNode != null)
            {
                initLithListBox(txtMapUnitAbbreviation.Text);
            }

            initEmptyAgeEventTab();
        }

        /// <summary>
        /// Host object of the dockable window
        /// </summary>
        private object Hook
        {
            get;
            set;
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private dwnMapUnitLegendEditor m_windowUI;

            public AddinImpl()
            {         
                
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new dwnMapUnitLegendEditor(this.Hook);
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }
        }

    #region "Copying from existing legends"

        private void tlsbtnShowCopyPanel_Click(object sender, EventArgs e)
        {
            #region "Open the Database to Copy From"

        findDatabase:
            // Check the registry
            string regValue = commonFunctions.ReadReg("Software\\NCGMPTools", "getSqlDatabase");

            // Find a Database
            IWorkspaceFactory wsFact = null;
            IWorkspace openedWorkspace = null;

            // Browse for a file, personal or SDE geodatabase
            IGxObjectFilter objectFilter = new GxFilterWorkspaces();
            IGxObject openedObject = commonFunctions.OpenArcFile(objectFilter, "Please select an NCGMP database");
            if (openedObject == null) { return; }

            // Check to see if it is a File, Personal or SDE database, create appropriate workspace factory
            string pathToOpen = null;

            switch (openedObject.Category)
            {
                case "Personal Geodatabase":
                    wsFact = new AccessWorkspaceFactoryClass();
                    pathToOpen = openedObject.FullName;
                    break;
                case "File Geodatabase":
                    wsFact = new FileGDBWorkspaceFactoryClass();
                    pathToOpen = openedObject.FullName;
                    break;
                case "Spatial Database Connection":
                    wsFact = new SdeWorkspaceFactoryClass();
                    IGxRemoteDatabaseFolder remoteDatabaseFolder = (IGxRemoteDatabaseFolder)openedObject.Parent;
                    pathToOpen = remoteDatabaseFolder.Path + openedObject.Name;
                    break;
                default:
                    break;
            }
            // This doesn't seem to be working for SDE databases.
            openedWorkspace = wsFact.OpenFromFile(pathToOpen, 0);


            // Check to see if the database is valid NCGMP
            bool isValid = ncgmpChecks.IsWorkspaceMinNCGMPCompliant(openedWorkspace);
            if (isValid == false)
            {
                MessageBox.Show("The selected database is not a valid NCGMP database.", "NCGMP Toolbar");
                goto findDatabase;
            }
            else
            {
                isValid = ncgmpChecks.IsSysInfoPresent(openedWorkspace);
                if (isValid == false)
                {
                    MessageBox.Show("In order to use these tools, the NCGMP database must contain a SysInfo table.", "NCGMP Toolbar");
                    goto findDatabase;
                }
            }
            #endregion
            
            // Show the copy form
            sourceLegendItemSelection sourceForm = new sourceLegendItemSelection(openedWorkspace);
            sourceForm.ShowDialog();

            // Bail if they canceled
            if (sourceForm.Canceled == true) { return; }

            // Get the Ids from the form, then close it
            if (sourceForm.idsToCopy.Count == 0) { sourceForm.Close(); return; }
            List<string> idsToCopy = sourceForm.idsToCopy;
            sourceForm.Close();

            // Build the Query to get the records to copy
            string sqlWhereClause = "DescriptionOfMapUnits_ID = '";
            foreach (string idValue in idsToCopy) { sqlWhereClause += idValue + "' OR DescriptionOfMapUnits_ID = '"; }

            // Get the records
            if (sqlWhereClause == "DescriptionOfMapUnits_ID = '") { return; }
            DescriptionOfMapUnitsAccess sourceDmu = new DescriptionOfMapUnitsAccess(openedWorkspace);
            sourceDmu.AddDescriptionOfMapUnits(sqlWhereClause.Remove(sqlWhereClause.Length - 32));

            // Get the next new Hierarchy Key 
            string newHierarchy = GetNewHierarchyKey();
            int newValue = int.Parse(newHierarchy.Substring(newHierarchy.Length - 4));

            // Loop through the source records, add them to the target legend after adjusting the Hierarchy
            DescriptionOfMapUnitsAccess targetDmu = new DescriptionOfMapUnitsAccess(m_theWorkspace);
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> sourceEntry in sourceDmu.DescriptionOfMapUnitsDictionary)
            {
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit sourceDmuEntry = sourceEntry.Value;
                string thisHierachyKey = newValue.ToString().PadLeft(4, '0');

                targetDmu.NewDescriptionOfMapUnit(sourceDmuEntry.MapUnit, sourceDmuEntry.Name, sourceDmuEntry.FullName, sourceDmuEntry.Label, 
                    sourceDmuEntry.Age, sourceDmuEntry.Description, thisHierachyKey, 
                    sourceDmuEntry.ParagraphStyle, sourceDmuEntry.AreaFillRGB, sourceDmuEntry.AreaFillPatternDescription, 
                    commonFunctions.GetCurrentDataSourceID(), sourceDmuEntry.GeneralLithologyTerm, sourceDmuEntry.GeneralLithologyConfidence);

                newValue++;
            }

            // Save the target Dmu
            targetDmu.SaveDescriptionOfMapUnits();

            // Refresh the tree
            ClearMapUnitInput();
            PopulateMainLegendTree();
        }

        private void tlsbtnCloseCopy_Click(object sender, EventArgs e) {}
     
    #endregion

    #region "Main Legend Treeview"

        public void PopulateMainLegendTree()
        {
            // Checks to see if form knows what workspace to edit.
            // *** THIS IS A PROBLEM. UNIT LEGEND EDITOR GETS WIRED TO ONE DATABASE ***
            if (m_theWorkspace == null) { m_theWorkspace = ArcMap.Editor.EditWorkspace; }
            if (m_theWorkspace == null) { return; }

            // Clear the tree first
            trvLegendItems.Nodes.Clear();

            // Get Sorted DmuEntries
            var sortedDmuEntries = GetDmuSortedByHierarchy();

            // Build ID lookup Dictionary
            var hierarchyDmuDictionary = BuildIDLookupFromHierarchy(sortedDmuEntries);
            
            // Setup variables that will be needed during legend population
            System.Drawing.Font BoldFont = new System.Drawing.Font("FGDCGeoAge", 8, FontStyle.Bold, GraphicsUnit.Point);
            TreeNode thisNode;
            TreeNode[] nodeArray;

            // Loop through each Dictionary Entry, add as a node to the Treeview
            trvLegendItems.BeginUpdate();

            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> aDictionaryEntry in sortedDmuEntries)
            {                
                // Grab the DMU object itself
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit aDescription = aDictionaryEntry.Value;

                // The node's label will be different if it is a heading or not
                string nodeLabel;
                bool isHeading = aDescription.ParagraphStyle.Contains("Heading");

                if (isHeading == true) { nodeLabel = aDescription.Name; }
                else { nodeLabel = aDescription.Label + " - " + aDescription.Name; }

                // HierarchyKey will tell me if this is a top-level node or not.
                string thisHierarchyKey = aDescription.HierarchyKey;
                if (thisHierarchyKey.Length <= 4) 
                {
                    // Add this node to the top level
                    trvLegendItems.Nodes.Add(aDescription.DescriptionOfMapUnits_ID, nodeLabel);
                }
                else
                {
                    // Lookup the parent's ID given it's hierarchy key. Parent's Hierarchy Key can be determined by stripping characters off the child
                    string parentHierarchyKey = thisHierarchyKey.Remove(thisHierarchyKey.Length - 5);
                    string parentID = hierarchyDmuDictionary[parentHierarchyKey];    
                
                    // Find the parent node in the tree, add this node as a child to it
                    nodeArray = trvLegendItems.Nodes.Find(parentID, true);
                    nodeArray[0].Nodes.Add(aDescription.DescriptionOfMapUnits_ID, nodeLabel);
                }              

                // Change the Font if it is a heading
                if (isHeading == true)
                {
                    thisNode = trvLegendItems.Nodes.Find(aDescription.DescriptionOfMapUnits_ID, true)[0];
                    thisNode.NodeFont = BoldFont;
                }     
            }

            // Expand all nodes - this calls a self-iterating function that expands all children
            foreach (TreeNode aNode in trvLegendItems.Nodes)
            {
                if (aNode.IsExpanded == false) { aNode.Expand(); }
                ExpandAllNodes(aNode.Nodes);
            }

            trvLegendItems.EndUpdate();

            // Update the renderer
            commonFunctions.UpdateMapUnitPolysRenderer(m_theWorkspace);

            // Update FeatureTemplates
            commonFunctions.UpdateMapUnitPolysFeatureTemplates(m_theWorkspace);
        }

        private void ExpandAllNodes(TreeNodeCollection NodeToExpand)
        {
            foreach (TreeNode aNode in NodeToExpand)
            {
                if (aNode.IsExpanded == false) { aNode.Expand(); }
                ExpandAllNodes(aNode.Nodes);
            }
        }

        private void ClearMainLegendTree()
        {
            trvLegendItems.Nodes.Clear();
        }

        private void trvLegendItems_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // -----------------------------------------------------------------------------
            // -----------------------------------------------------------------------------
            //  Populate the input controls based on the selected node
            // -----------------------------------------------------------------------------
            // -----------------------------------------------------------------------------

            // Make sure that something is selected
            if (e.Node == null) { return; }
            

            // Get the selected DMU entry
            DescriptionOfMapUnitsAccess dmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);
            dmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + e.Node.Name + "'");
            DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry = dmuAccess.DescriptionOfMapUnitsDictionary[e.Node.Name];

            // Populate controls
            PopulateInputControls(thisDmuEntry);

            // Get the related standard lithology entry
            initLithListBox(thisDmuEntry.MapUnit);

            /// Initialize the age tab
            initAgeTab(thisDmuEntry.MapUnit);
        }

        private void tlsbtnRefreshLegend_Click(object sender, EventArgs e)
        {
            // Clear Inputs and refresh the Legend Tree
            ClearLithologyInput();
            ClearMapUnitInput();
            PopulateMainLegendTree();

            // Update the Legend title
            sysInfo sysInfoForm = new sysInfo(m_theWorkspace);
            tlslblLegendName.Text = sysInfoForm.ProjName;

            initAgeTab(txtMapUnitAbbreviation.Text);
        }

    #region "Drag and Drop Stuff"

        private void trvLegendItems_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {
            // Start the drag-drop routine
            trvLegendItems.DoDragDrop(e.Item, DragDropEffects.All);
        }

        private void trvLegendItems_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Cast the cursor's location into Treeview-coordinates
            System.Drawing.Point dragLocation = (sender as TreeView).PointToClient(new System.Drawing.Point(e.X, e.Y));

            // Find out what the drag is over
            TreeNode hoverNode = trvLegendItems.GetNodeAt(dragLocation);

            // Scroll if we're near the edges
            if (dragLocation.Y <= (trvLegendItems.Font.Height / 2)) { ScrollTree(0); }
            else if (dragLocation.Y >= (trvLegendItems.ClientSize.Height - (trvLegendItems.Font.Height / 2))) { ScrollTree(1); }

            // If dragged item is over another node, highlight it, unhighlight others, and allow for a move
            if (hoverNode != null)
            {
                e.Effect = DragDropEffects.Move;
                ClearHighlights(trvLegendItems.Nodes);
                hoverNode.BackColor = Color.DarkBlue;
                hoverNode.ForeColor = Color.White;
            }
        }

        private void trvLegendItems_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            // Get the dragged node, and the associated DMU entry
            TreeNode draggedNode = e.Data.GetData((new TreeNode()).GetType()) as TreeNode;
            DescriptionOfMapUnitsAccess dmuFinder = new DescriptionOfMapUnitsAccess(m_theWorkspace);
            dmuFinder.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + draggedNode.Name + "'");
            DescriptionOfMapUnitsAccess.DescriptionOfMapUnit draggedDmuEntry = dmuFinder.DescriptionOfMapUnitsDictionary[draggedNode.Name];

            // Cast the cursor's location into Treeview-coordinates
            System.Drawing.Point dragLocation = (sender as TreeView).PointToClient(new System.Drawing.Point(e.X, e.Y));

            // Find out what the drag is over
            TreeNode destinationNode = trvLegendItems.GetNodeAt(dragLocation);
            if (destinationNode == null) { return; }

            // Ask the user how to perform the drop
            dropChooser dropChoiceForm = new dropChooser();
            dropChoiceForm.ShowDialog();

            // Bail if they canceled
            if (dropChoiceForm.Canceled == true) { return; }

            // Before making changes, gather the children of the dragged node
            var draggedChildren = GetSortedChildren(draggedDmuEntry.HierarchyKey);

            // First, remove the dragged node from the hierarchy
            RemoveItemFromHierarchy(draggedDmuEntry.HierarchyKey);

            // Get the dropped item's DMU entry
            dmuFinder.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + destinationNode.Name + "'");
            DescriptionOfMapUnitsAccess.DescriptionOfMapUnit destinationDmuEntry = dmuFinder.DescriptionOfMapUnitsDictionary[destinationNode.Name];                        

            // Next, insert the node in the new location
            string newHierarchyKey = "";
            int newKeyValue = 0;

            // Depending on the user's choice, drop appropriately
            switch (dropChoiceForm.DropAsChild)
            {
                case true:
                    // Child drop, put it at the end of the block
                    newHierarchyKey = GetNewHierarchyKey(destinationDmuEntry.HierarchyKey);
                    break;

                case false:
                    // Sibling drop - above or below?
                    switch (dropChoiceForm.DropBelow)
                    {
                        case true:
                            // Drop below
                            newKeyValue = int.Parse(destinationDmuEntry.HierarchyKey.Substring(destinationDmuEntry.HierarchyKey.Length - 4)) + 1;                            
                            break;

                        case false:
                            // Drop above
                            newKeyValue = int.Parse(destinationDmuEntry.HierarchyKey.Substring(destinationDmuEntry.HierarchyKey.Length - 4));
                            break;
                    }

                    // Build the new Hierarchy Key
                    if (destinationDmuEntry.HierarchyKey.Length == 4) { newHierarchyKey = newKeyValue.ToString().PadLeft(4, '0'); }
                    else { newHierarchyKey = destinationDmuEntry.HierarchyKey.Remove(destinationDmuEntry.HierarchyKey.Length - 5) + "." + newKeyValue.ToString().PadLeft(4, '0'); }
                    break;
            }
                        
            // Take care of rebuilding the hierarchy due to the insert
            //  I need to pass in these children because after making any adjustments to the hierarchy
            //  (RemoveFromHierarchy call above), I cannot rely on hierarchy keys to be truthful as to
            //  who was the initial parent. That is, at this point trying to find the inital children
            //  of the dragged node is impossible.
            InsertItemIntoHierarchy(draggedDmuEntry, newHierarchyKey, draggedChildren);

            // Lastly, clear all highlighted nodes, repopulate the tree
            ClearHighlights(trvLegendItems.Nodes);
            ClearMapUnitInput();
            PopulateMainLegendTree();
        }

        private void ClearHighlights(TreeNodeCollection  theNodeCollection)
        {
            // Cycle through the tree, set colors appropriately - recursive.
            foreach (TreeNode thisNode in theNodeCollection)
            {
                thisNode.BackColor = Color.White;
                thisNode.ForeColor = Color.Black;

                ClearHighlights(thisNode.Nodes);
            }
        }

        private void ScrollTree(int scrollDirection)
        {
            // scrollDirection = 0 means scroll up, 1 means scroll down
            const int WM_SCROLL = 276; // This integer means horizontal scroll please!
            SendMessage(trvLegendItems.Handle, WM_SCROLL, (IntPtr)scrollDirection, IntPtr.Zero);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    #endregion

    #endregion

    #region "Input Controls"

        private void PopulateInputControls(DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry)
        {
            // Determine if the FullName should be amalgamated from Name/Age
            bool buildFullName = false;
            if (thisDmuEntry.FullName == thisDmuEntry.Name + " (" + thisDmuEntry.Age + ")") { buildFullName = true; }

            // Determine if the unit is a heading
            bool isHeading = thisDmuEntry.ParagraphStyle.Contains("Heading");

            // Clear inputs
            ClearMapUnitInput();
            ClearLithologyInput();

            // Enable/Disable appropriate controls
            EnableControls(isHeading, buildFullName);

            // Set update flag
            m_ThisIsAnUpdate = true;
            
            if (isHeading == true)
            {
                // This is a heading
                chkIsHeading.Checked = true;
                txtUnitName.Text = thisDmuEntry.Name;
                txtMapUnitDescription.Text = thisDmuEntry.Description;                
                pnlColor.BackColor = Color.White;
            }
            else
            {
                // This is a MapUnit
                chkIsHeading.Checked = false;
                txtUnitName.Text = thisDmuEntry.Name;                
                txtMapUnitAbbreviation.Text = thisDmuEntry.MapUnit;
                txtMapUnitAge.Text = thisDmuEntry.Age;
                txtMapUnitFullName.Text = thisDmuEntry.FullName;
                txtMapUnitDescription.Text = thisDmuEntry.Description;                

                // This variable will hang onto the old MapUnitName in case it gets changed
                m_theOldMapUnitName = thisDmuEntry.MapUnit;

                // Color the panel from RGB values - this is currently dependant on ; being used as a delimiter
                string rgbString = thisDmuEntry.AreaFillRGB;
                string[] rgbValues = rgbString.Split(';');

                if (rgbValues.Length < 3) 
                { 
                    pnlColor.BackColor = Color.White;
                    colorDialog.Color = Color.White;
                }
                else 
                { 
                    pnlColor.BackColor = Color.FromArgb(int.Parse(rgbValues[0]), int.Parse(rgbValues[1]), int.Parse(rgbValues[2]));
                    colorDialog.Color = pnlColor.BackColor;
                }
            }
        }

        private void EnableControls(bool isHeading, bool buildFullName)
        {
            switch (isHeading)
            {
                case true:
                    txtUnitName.Enabled = true;
                    txtMapUnitAbbreviation.Enabled = false;
                    txtMapUnitAge.Enabled = false;
                    txtMapUnitFullName.Enabled = false;
                    txtMapUnitFullName.ReadOnly = true;                    
                    txtMapUnitDescription.Enabled = true;
                    btnColorChooser.Enabled = false;
                    tlsbtnAssignUnit.Enabled = false;
                    tlsbtnRemoveLegendItem.Enabled = true;
                    btnEditFullName.Enabled = false;
                    tlsbtnCancel.Enabled = true;
                    break;

                case false:
                    txtUnitName.Enabled = true;
                    txtMapUnitAbbreviation.Enabled = true;
                    txtMapUnitAge.Enabled = true;
                    txtMapUnitFullName.Enabled = true;                                   
                    txtMapUnitDescription.Enabled = true;
                    btnColorChooser.Enabled = true;
                    tlsbtnAssignUnit.Enabled = true;
                    tlsbtnRemoveLegendItem.Enabled = true;
                    tlsbtnCancel.Enabled = true;

                    switch (buildFullName)
                    {
                        case true:
                            btnEditFullName.Enabled = true;
                            txtMapUnitFullName.ReadOnly = true;
                            break;
                        case false:
                            btnEditFullName.Enabled = false;
                            txtMapUnitFullName.ReadOnly = false;
                            break;
                    }                         
                    break;
            }
        }    

        private void ClearMapUnitInput()
        {
            // Clear values
            txtUnitName.Text = null;
            txtMapUnitAbbreviation.Text = null;
            txtMapUnitAge.Text = null;
            txtMapUnitFullName.Text = null;
            txtMapUnitDescription.Text = null;
            chkIsHeading.Checked = false;
            pnlColor.BackColor = Color.White;

            //Disable controls
            txtUnitName.Enabled = false;
            txtMapUnitAbbreviation.Enabled = false;
            txtMapUnitAge.Enabled = false;
            txtMapUnitFullName.Enabled = false;
            txtMapUnitFullName.ReadOnly = true;
            btnEditFullName.Enabled = false;
            txtMapUnitDescription.Enabled = false;
            btnColorChooser.Enabled = false;
            tlsbtnAssignUnit.Enabled = false;
            tlsbtnRemoveLegendItem.Enabled = false;
            tlsbtnSaveMapUnit.Enabled = false;
            tlsbtnCancel.Enabled = false;

            // Set Update Flag
            m_ThisIsAnUpdate = false;
            
            // Clear the old MapUnit variable
            m_theOldMapUnitName = null;
        }

        private void btnColorChooser_Click(object sender, EventArgs e)
        {
            // Open color dialog to allow user to pick a color
            this.colorDialog.Color = this.pnlColor.BackColor;
            this.colorDialog.ShowDialog();
            this.pnlColor.BackColor = this.colorDialog.Color;
        }

        private void txtMapUnitAbbreviation_EnabledChanged(object sender, EventArgs e)
        {
            initLithListBox(txtMapUnitAbbreviation.Text);
            initAgeTab(txtMapUnitAbbreviation.Text);

            if (txtMapUnitAbbreviation.Enabled)
            {
                tabInputs.SelectedIndex = 0;
                ((Control)tabAge).Enabled = true;
                ((Control)tabLith).Enabled = true;
            }
            else
            {
                tabInputs.SelectedIndex = 0;
                ((Control)tabAge).Enabled = false;
                ((Control)tabLith).Enabled = false;
            }
        }

        #region "Adjustments to be made when textboxes are changed"

        private void txtUnitName_TextChanged(object sender, EventArgs e)
        {
            EnableSaveButton();
            AdjustFullName();
        }

        private void txtMapUnitAbbreviation_TextChanged(object sender, EventArgs e)
        {
            EnableSaveButton();           
        }

        private void txtMapUnitAge_TextChanged(object sender, EventArgs e)
        {
            EnableSaveButton();
            AdjustFullName();
        }

        private void txtMapUnitDescription_TextChanged(object sender, EventArgs e)
        {
            EnableSaveButton();
        }

        private void EnableSaveButton()
        {
            // If you've populated all the right textboxes, then enable the save button
            switch (chkIsHeading.Checked)
            {
                case true:
                    if (txtUnitName.Text.Length > 0) { tlsbtnSaveMapUnit.Enabled = true; }
                    else { tlsbtnSaveMapUnit.Enabled = false; }
                    break;

                case false:
                    if ((txtUnitName.Text.Length > 0) && (txtMapUnitAbbreviation.Text.Length > 0) && (txtMapUnitAge.Text.Length > 0) && (txtMapUnitDescription.Text.Length > 0))
                    { tlsbtnSaveMapUnit.Enabled = true; }
                    else { tlsbtnSaveMapUnit.Enabled = false; }
                    break;
            }
        }

        private void AdjustFullName()
        {
            if (txtMapUnitFullName.ReadOnly == false) { return; }
            if (txtMapUnitFullName.Enabled == true) { txtMapUnitFullName.Text = txtUnitName.Text + " (" + txtMapUnitAge.Text + ")"; }
            //else { txtMapUnitFullName.Text = null; }
        }

        private void btnEditFullName_Click(object sender, EventArgs e)
        {
            txtMapUnitFullName.ReadOnly = false;
        }

        private void chkIsHeading_CheckedChanged(object sender, EventArgs e)
        {
            EnableControls(chkIsHeading.Checked, false);

            if (chkIsHeading.Checked)
            {
                ((Control)tabAge).Enabled = false;
                ((Control)tabLith).Enabled = false;
            }
            else
            {
                ((Control)tabAge).Enabled = true;
                ((Control)tabLith).Enabled = true;
            }
        }

    #endregion        

    #region "Save/Cancel Functionality"

        private void tlsbtnSaveMapUnit_Click(object sender, EventArgs e)
        {
            saveAge();
            saveLithology();
            saveMapUnit();
            
        }

        private void saveMapUnit()
        {
            // Get attributes from the form
            string thisDmuAge = txtMapUnitAge.Text;
            string thisDmuDefinitionSourceID = commonFunctions.GetCurrentDataSourceID();
            string thisDmuDescription = txtMapUnitDescription.Text;
            string thisDmuFullName = txtMapUnitFullName.Text;
            string thisDmuLabel = txtMapUnitAbbreviation.Text;
            string thisDmuMapUnit = txtMapUnitAbbreviation.Text;
            string thisDmuName = txtUnitName.Text;

            // These attributes are dependant on whether this is a heading or not
            string thisDmuParagraphStyle = "";
            string thisDmuAreaFillRGB = "";
            if (chkIsHeading.Checked == true)
            {
                thisDmuParagraphStyle = "Heading";
                thisDmuAreaFillRGB = "";
            }
            else
            {
                thisDmuParagraphStyle = "Standard";
                thisDmuAreaFillRGB = pnlColor.BackColor.R + ";" + pnlColor.BackColor.G + ";" + pnlColor.BackColor.B;
            }

            // Get the DMU reference that will be used to provide table access
            DescriptionOfMapUnitsAccess dmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);

            // Set the variable to represent the updated Dmu entry in the case of an update. This will be used to update polygons later
            DescriptionOfMapUnitsAccess.DescriptionOfMapUnit dmuEntry = new DescriptionOfMapUnitsAccess.DescriptionOfMapUnit();

            switch (m_ThisIsAnUpdate)
            {
                case true:
                    // Get the DMU entry that should be updated
                    dmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + trvLegendItems.SelectedNode.Name + "'");
                    dmuEntry = dmuAccess.DescriptionOfMapUnitsDictionary[trvLegendItems.SelectedNode.Name];

                    // Add attributes from the form
                    dmuEntry.Age = thisDmuAge;
                    dmuEntry.DescriptionSourceID = thisDmuDefinitionSourceID;
                    dmuEntry.Description = thisDmuDescription;
                    dmuEntry.FullName = thisDmuFullName;
                    dmuEntry.Label = thisDmuLabel;
                    dmuEntry.MapUnit = thisDmuMapUnit;
                    dmuEntry.Name = thisDmuName;
                    dmuEntry.RequiresUpdate = true;

                    // These attributes are dependant on whether this is a heading or not
                    if (chkIsHeading.Checked == true)
                    {
                        dmuEntry.ParagraphStyle = thisDmuParagraphStyle;
                        dmuEntry.AreaFillRGB = thisDmuAreaFillRGB;
                    }
                    else
                    {
                        dmuEntry.ParagraphStyle = thisDmuParagraphStyle;
                        dmuEntry.AreaFillRGB = thisDmuAreaFillRGB;
                    }

                    // Perform the update
                    dmuAccess.UpdateDescriptionOfMapUnit(dmuEntry);

                    break;

                case false:
                    // This is a new entry, get an Hierarchy Key
                    string thisDmuHierarchyKey = GetNewHierarchyKey();

                    // Add the record
                    dmuAccess.NewDescriptionOfMapUnit(thisDmuMapUnit, thisDmuName, thisDmuFullName,
                        thisDmuLabel, thisDmuAge, thisDmuDescription,
                        thisDmuHierarchyKey, thisDmuParagraphStyle, thisDmuAreaFillRGB,
                        "", thisDmuDefinitionSourceID, "", "");

                    break;
            }

            // All done - save
            dmuAccess.SaveDescriptionOfMapUnits();

            // Refresh the tree
            PopulateMainLegendTree();

            // Update polys
            if ((m_ThisIsAnUpdate == true) && (m_theOldMapUnitName != null)) { UpdatePolygons(m_theOldMapUnitName, dmuEntry); }

            // Clear Inputs
            ClearMapUnitInput(); 
        }

        private void tlsbtnCancel_Click(object sender, EventArgs e)
        {
            cancelAge();
            cancelLithology();
            cancelMapUnit();
        }

        private void cancelMapUnit()
        {
            // Don't save or anything, just cancel
            ClearMapUnitInput();
            trvLegendItems.SelectedNode = null;
        }

    #endregion

    #region "Add/Remove Legend Item Buttons"

        private void tlsbtnNewLegendItem_Click(object sender, EventArgs e)
        {
            // Clear the input controls
            ClearMapUnitInput();
            ClearLithologyInput();
            liEvts4ThisUnit.Items.Clear(); /// Clear the age display list for this map unit
            trvLegendItems.SelectedNode = null;

            /// Switch to the map unit tab
            tabInputs.SelectedIndex = 0;

            // Set the Update flag
            m_ThisIsAnUpdate = false;
            
            // Enable controls - in this case I need to explicitly disable the Remove and assign buttons
            EnableControls(false, true);
            tlsbtnRemoveLegendItem.Enabled = false;
            tlsbtnAssignUnit.Enabled = false;
        }

        private void tlsbtnRemoveLegendItem_Click(object sender, EventArgs e)
        {
            // Get DMU Access Class
            DescriptionOfMapUnitsAccess dmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);

            // Find out if the selected node has children
            if (trvLegendItems.SelectedNode.Nodes.Count > 0)
            {
                // This node has children, ensure they want it all gone
                DialogResult result = MessageBox.Show("If you delete this entry, all of its children will be removed as well. Do you want to continue?", "NCGMP Tools", MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) { return; }

                // They want to continue. First delete this one
                dmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + trvLegendItems.SelectedNode.Name + "'");
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisEntry = dmuAccess.DescriptionOfMapUnitsDictionary[trvLegendItems.SelectedNode.Name];

                /// Delete related record in the Extended Attributes table
                deleteExtendedAtrributesRecord(thisEntry.MapUnit);
                /// Delete related lithology records
                deleteLithologyRecord(thisEntry.MapUnit);

                dmuAccess.DeleteDescriptionOfMapUnits(thisEntry);

                // Now Delete all children
                DeleteAllChildren(thisEntry.HierarchyKey);

                // Adjust the Hierarchy
                RemoveItemFromHierarchy(thisEntry.HierarchyKey);
            }
            else
            {
                // This node has no children, simply delete it
                dmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + trvLegendItems.SelectedNode.Name + "'");
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisEntry = dmuAccess.DescriptionOfMapUnitsDictionary[trvLegendItems.SelectedNode.Name];

                /// Delete related record in the Extended Attributes table
                deleteExtendedAtrributesRecord(thisEntry.MapUnit);
                /// Delete related lithology records
                deleteLithologyRecord(thisEntry.MapUnit);

                dmuAccess.DeleteDescriptionOfMapUnits(thisEntry);

                // Remove this item from the Hierarchy - Update its siblings and children
                RemoveItemFromHierarchy(thisEntry.HierarchyKey);
            }

            // Clear Inputs
            ClearMapUnitInput();

            //Re-populate the Treeview
            PopulateMainLegendTree();
        }

        private void DeleteAllChildren(string parentKey)
        {
            // Get DMU Access Class
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);

            // Get the children
            var sortedChildren = GetSortedChildren(parentKey);

            // Loop through them, delete each one
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> anEntry in sortedChildren)
            {
                // Get the DMU entry for this entry
                DmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + anEntry.Value.DescriptionOfMapUnits_ID + "'");
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry = DmuAccess.DescriptionOfMapUnitsDictionary[anEntry.Value.DescriptionOfMapUnits_ID];

                /// Delete related record in the Extended Attributes table
                deleteExtendedAtrributesRecord(thisDmuEntry.MapUnit);
                /// Delete related lithology records
                deleteLithologyRecord(thisDmuEntry.MapUnit);

                // Delete it
                DmuAccess.DeleteDescriptionOfMapUnits(thisDmuEntry);

                // Delete its children
                DeleteAllChildren(thisDmuEntry.HierarchyKey);
            }
        }

    #endregion

    #region "Hierarchy Control"

        private void InsertItemIntoHierarchy(DescriptionOfMapUnitsAccess.DescriptionOfMapUnit theInsertedDmuEntry, string theNewHierarchyKey, 
            IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>> thisItemsChildren)
        {
            // Get the insert location parent Hierarchy Key
            string parentKey;
            if (theNewHierarchyKey.Length == 4) { parentKey = null; }
            else { parentKey = theNewHierarchyKey.Remove(theNewHierarchyKey.Length - 5); }

            int newKeyValue = int.Parse(theNewHierarchyKey.Substring(theNewHierarchyKey.Length - 4));

            // Get the children of the inserted item's new parent
            var sortedChildren = GetSortedChildren(parentKey);

            // Get a DMU Access object to perform an update
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);            

            // I need to have each of the entry's children gathered before I start updating them.
            // What a fucking mess.
            Dictionary<string, IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>>> theChildren = new Dictionary<string, IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>>>();
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> anEntry in sortedChildren)
            {
                theChildren.Add(anEntry.Value.DescriptionOfMapUnits_ID, GetSortedChildren(anEntry.Value.HierarchyKey));
            }

            // Cycle through these children
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> anEntry in sortedChildren)
            {
                // Grab this item's hierarchy info
                string thisKey = anEntry.Value.HierarchyKey;
                int thisKeyValue = int.Parse(thisKey.Substring(thisKey.Length - 4));

                // If the value is greater than or equal to the inserted item's new hierarchy value, increment by one, update children
                if (thisKeyValue >= newKeyValue)
                {
                    string thisNewKey;
                    if (parentKey == null) { thisNewKey = (thisKeyValue + 1).ToString().PadLeft(4, '0'); }
                    else { thisNewKey = parentKey + "." + (thisKeyValue + 1).ToString().PadLeft(4, '0'); }

                    // Get the DMU entry for this entry
                    DmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + anEntry.Value.DescriptionOfMapUnits_ID + "'");
                    DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry = DmuAccess.DescriptionOfMapUnitsDictionary[anEntry.Value.DescriptionOfMapUnits_ID];

                    // Update its key
                    thisDmuEntry.HierarchyKey = thisNewKey;
                    DmuAccess.UpdateDescriptionOfMapUnit(thisDmuEntry);

                    // Update its children's keys, gathered beforehand
                    UpdateChildrenKeys(thisKey, thisNewKey, theChildren[thisDmuEntry.DescriptionOfMapUnits_ID]);
                }
            }

            // Update the Children of the dragged item using the collection of children passed in            
            UpdateChildrenKeys(theInsertedDmuEntry.HierarchyKey, theNewHierarchyKey, thisItemsChildren);

            // Update the dragged item itself
            theInsertedDmuEntry.HierarchyKey = theNewHierarchyKey;
            DmuAccess.UpdateDescriptionOfMapUnit(theInsertedDmuEntry);

            // Save changes
            DmuAccess.SaveDescriptionOfMapUnits();
        }

        private void RemoveItemFromHierarchy(string keyBeingRemoved)
        {
            // Get the parent Hierearchy Key
            string parentKey;
            if (keyBeingRemoved.Length == 4) { parentKey = null; }
            else { parentKey = keyBeingRemoved.Remove(keyBeingRemoved.Length - 5); }

            int removedKeyValue = int.Parse(keyBeingRemoved.Substring(keyBeingRemoved.Length - 4));

            // Get the children of the removed node's parent
            var sortedChildren = GetSortedChildren(parentKey);

            // Get a DMU Access object to perform an update
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);

            // Cycle through the children            
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> anEntry in sortedChildren)
            {
                // Grab the hierarchy information
                string thisKey = anEntry.Value.HierarchyKey;
                int thisKeyValue = int.Parse(thisKey.Substring(thisKey.Length - 4));

                // If the value is greater than the removed key, decrement by one, and update children
                if (thisKeyValue > removedKeyValue)
                {
                    string newKey;
                    if (parentKey == null) { newKey = (thisKeyValue -1).ToString().PadLeft(4, '0'); }
                    else { newKey = parentKey + "." + (thisKeyValue - 1).ToString().PadLeft(4, '0'); }

                    // Get the DMU entry for this entry
                    DmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + anEntry.Value.DescriptionOfMapUnits_ID + "'");
                    DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry = DmuAccess.DescriptionOfMapUnitsDictionary[anEntry.Value.DescriptionOfMapUnits_ID];

                    // Update its key
                    thisDmuEntry.HierarchyKey = newKey;
                    DmuAccess.UpdateDescriptionOfMapUnit(thisDmuEntry);

                    // Update its children's keys
                    UpdateChildrenKeys(thisKey, newKey);
                }
            }

            // Save the changes
            DmuAccess.SaveDescriptionOfMapUnits();
        }

        private void UpdateChildrenKeys(string oldParentKey, string newParentKey, IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>> sortedChildren = null)
        {
            // If a set of children are not passed in to update, then get them based on the oldParentKey that is sent in
            if (sortedChildren == null) { sortedChildren = GetSortedChildren(oldParentKey); }

            // Get a DMU Access object to perform an update
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);

            // Cycle through the children            
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> anEntry in sortedChildren)
            {
                // Grab, adjust the hierarchy information
                string thisKey = anEntry.Value.HierarchyKey;
                string thisKeyValue = thisKey.Substring(thisKey.Length - 4);
                string newKey = newParentKey + "." + thisKeyValue;

                // Get the DMU entry for this entry
                DmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + anEntry.Value.DescriptionOfMapUnits_ID + "'");
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry = DmuAccess.DescriptionOfMapUnitsDictionary[anEntry.Value.DescriptionOfMapUnits_ID];

                // Update its key
                thisDmuEntry.HierarchyKey = newKey;
                DmuAccess.UpdateDescriptionOfMapUnit(thisDmuEntry);

                // Update its children's keys
                UpdateChildrenKeys(thisKey, newKey);
            }

            // Save the changes
            DmuAccess.SaveDescriptionOfMapUnits();
        }

        private string GetNewHierarchyKey(string parentKey = null)
        {
            // Null parentKey will return a new Hierarchy Key at the top level.

            // First find the last current HierarchyKey
            string lastHierarchyKey = GetLastHierarchyKey(parentKey);

            // If there is nothing in the legend yet, lastHierarchyKey will come through null
            int lastKeyValue;
            if (lastHierarchyKey == null) { lastKeyValue = 0; }
            else
            {
                // Pick the last few characters off of it, cast as int
                lastKeyValue = int.Parse(lastHierarchyKey.Substring(lastHierarchyKey.Length - 4));
            }

            // Increment by one and pad it
            string newLastKeyValue = ((lastKeyValue + 1).ToString()).PadLeft(4, '0');

            // Generate the new key
            string newHierarchyKey = "";
            if (parentKey != null) { newHierarchyKey += parentKey + "."; }
            newHierarchyKey += newLastKeyValue;

            return newHierarchyKey;            
        }

        private string GetLastHierarchyKey(string parentKey = null)
        {            
            // Get Sorted DmuEntries - parentKey will limit this list to children of a specific parent
            var sortedDmuEntries = GetSortedChildren(parentKey);

            // If there's nothing in the legend yet, sortedDmuEntries will be blank
            if (sortedDmuEntries.Count() == 0) { return null; }

            // Loop through to find the longest and last hierarchy key
            string lastHierarchyKey = "";
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> anEntry in sortedDmuEntries)
            {
                // If this one is longer than the longest so far, set it and continue
                if (anEntry.Value.HierarchyKey.Length > lastHierarchyKey.Length) { lastHierarchyKey = anEntry.Value.HierarchyKey; continue; }

                // If the length's are equal, compare the values at the end of the strings
                if (anEntry.Value.HierarchyKey.Length == lastHierarchyKey.Length)
                {
                    int thisHierarchy = int.Parse(anEntry.Value.HierarchyKey.Substring(anEntry.Value.HierarchyKey.Length - 4));
                    int bestSoFar = int.Parse(lastHierarchyKey.Substring(lastHierarchyKey.Length - 4));

                    if (thisHierarchy > bestSoFar) { lastHierarchyKey = anEntry.Value.HierarchyKey; }
                }
            }

            return lastHierarchyKey;
        }

        private IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>> GetDmuSortedByHierarchy()
        {
            // Get All DescriptionOfMapUnits.
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);
            DmuAccess.AddDescriptionOfMapUnits();                           

            // Sort using Linq syntax
            var sortedDmuEntries = (
                from entry in DmuAccess.DescriptionOfMapUnitsDictionary
                orderby ((DescriptionOfMapUnitsAccess.DescriptionOfMapUnit)entry.Value).HierarchyKey ascending 
                select entry);

            return sortedDmuEntries;
        }

        private IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>> GetSortedChildren(string parentKey = null)
        {
            // Get the children of a given parent, sorted on HierarchyKey
            // First get all DMU entries
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);
            DmuAccess.AddDescriptionOfMapUnits();

            if (parentKey != null)
            {
                // Sort and filter using Linq syntax
                var sortedDmuEntries = (
                    from entry in DmuAccess.DescriptionOfMapUnitsDictionary
                    where ((entry.Value.HierarchyKey.Length == parentKey.Length + 5) && (entry.Value.HierarchyKey.StartsWith(parentKey)))
                    orderby entry.Value.HierarchyKey ascending
                    select entry);

                return sortedDmuEntries;
            }
            else
            {
                // Just return the root-level stuff
                var sortedDmuEntries = (
                    from entry in DmuAccess.DescriptionOfMapUnitsDictionary
                    where entry.Value.HierarchyKey.Length == 4
                    orderby entry.Value.HierarchyKey ascending
                    select entry);

                return sortedDmuEntries;

            }
        }

        private Dictionary<string, string> BuildIDLookupFromHierarchy(IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>> dmuEntries)
        {
            // Pass these records into a new Dictionary correlating ID to HierarchyKey
            Dictionary<string, string> hierarchyDmuDictionary = new Dictionary<string,string>();
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> aDictionaryEntry in dmuEntries)
            {
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry = (DescriptionOfMapUnitsAccess.DescriptionOfMapUnit)aDictionaryEntry.Value;
                hierarchyDmuDictionary.Add(thisDmuEntry.HierarchyKey, thisDmuEntry.DescriptionOfMapUnits_ID);
            }

            return hierarchyDmuDictionary;
        }

    #endregion

    #region "Attribute Polygons, Maintain Attribution"

        private void tlsbtnAssignUnit_Click(object sender, EventArgs e)
        {
            // Find the selected DMU Entry
            string dmuID = trvLegendItems.SelectedNode.Name;
            if (dmuID == null) { return; }

            DescriptionOfMapUnitsAccess dmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);
            dmuAccess.AddDescriptionOfMapUnits("DescriptionOfMapUnits_ID = '" + dmuID + "'");
            DescriptionOfMapUnitsAccess.DescriptionOfMapUnit dmuEntry = dmuAccess.DescriptionOfMapUnitsDictionary[dmuID];

            // Get selected polygons
            IFeatureLayer mapUnitPolysLayer = commonFunctions.FindFeatureLayer(m_theWorkspace, "MapUnitPolys");

            // Find out if there are selected features
            IFeatureSelection featureSelection = (IFeatureSelection)mapUnitPolysLayer;
            ISelectionSet theSelection = featureSelection.SelectionSet;

            // Bail if nothing was selected
            if (theSelection.Count == 0) { return; }

            // Pass the selected features into a cursor that we can iterate through
            ICursor theCursor;
            theSelection.Search(null, false, out theCursor);
            int IdFld = theCursor.FindField("MapUnitPolys_ID");

            // Build the Where Clause to get these features by looping through the cursor
            string sqlWhereClause = "MapUnitPolys_ID = '";
            IRow theRow = theCursor.NextRow();
            while (theRow != null)
            {
                sqlWhereClause += theRow.get_Value(IdFld) + "' OR MapUnitPolys_ID = '";
                theRow = theCursor.NextRow();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(theCursor);

            // If we didn't add anything to the where clause, bail
            if (sqlWhereClause == "MapUnitPolys_ID = '") { return; }

            // Cleanup the where clause
            sqlWhereClause = sqlWhereClause.Remove(sqlWhereClause.Length - 23);

            // Get the MapUnitPolys
            MapUnitPolysAccess polysAccess = new MapUnitPolysAccess(m_theWorkspace);
            polysAccess.AddMapUnitPolys(sqlWhereClause);

            //---------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------
            // Data Access Issue: I actually have to pass the dictionary into another object.
            //  If I don't, once the first record is updated, the dictionary is changed.
            //  Then the foreach loop fails, because what it is looping through was adjusted.
            //  Not very happy with this.
            //---------------------------------------------------------------------------------
            //---------------------------------------------------------------------------------

            // Sort using Linq syntax
            var sortedPolys = (
                from entry in polysAccess.MapUnitPolysDictionary                
                select entry);
            
                MapUnitPolysAccess secondPolysAccess = new MapUnitPolysAccess(m_theWorkspace);
            try
            {
                // Cycle through the MapUnitPolys and update the MapUnit and Label  attributes
                foreach (KeyValuePair<string, MapUnitPolysAccess.MapUnitPoly> anEntry in sortedPolys)
                {
                    // Get the MapUnitPoly object                    
                    secondPolysAccess.AddMapUnitPolys("MapUnitPolys_ID = '" + anEntry.Value.MapUnitPolys_ID + "'");
                    MapUnitPolysAccess.MapUnitPoly aPoly = secondPolysAccess.MapUnitPolysDictionary[anEntry.Value.MapUnitPolys_ID];

                    // Change the appropriate values
                    aPoly.MapUnit = dmuEntry.MapUnit;
                    aPoly.Label = dmuEntry.Label;

                    // Update the Poly
                    secondPolysAccess.UpdateMapUnitPoly(aPoly);
                }
            }
            catch (Exception err) { MessageBox.Show(err.Message); }

            // Save updates
            secondPolysAccess.SaveMapUnitPolys();

            // Update the active view
            ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, mapUnitPolysLayer, null);

        }

        private void UpdatePolygons(string oldMapUnitName, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit theNewDmuEntry)
        {
            // Find polygons that use the old MapUnit
            MapUnitPolysAccess polysFinder = new MapUnitPolysAccess(m_theWorkspace);
            polysFinder.AddMapUnitPolys("MapUnit = '" + oldMapUnitName + "'");

            try
            {
                // A second poly access class to do the updating
                MapUnitPolysAccess polysUpdater = new MapUnitPolysAccess(m_theWorkspace);

                // Loop through them and update
                foreach (KeyValuePair<string, MapUnitPolysAccess.MapUnitPoly> anEntry in polysFinder.MapUnitPolysDictionary)
                {
                    // Get the Polygon
                    polysUpdater.AddMapUnitPolys("MapUnitPolys_ID = '" + anEntry.Value.MapUnitPolys_ID + "'");
                    MapUnitPolysAccess.MapUnitPoly aPoly = polysUpdater.MapUnitPolysDictionary[anEntry.Value.MapUnitPolys_ID];

                    // Set new values
                    aPoly.MapUnit = theNewDmuEntry.MapUnit;
                    aPoly.Label = theNewDmuEntry.Label;

                    // Update it
                    polysUpdater.UpdateMapUnitPoly(aPoly);
                }

                // Save changes
                polysUpdater.SaveMapUnitPolys();

                // Refresh the Active View
                IFeatureLayer mapUnitPolysLayer = commonFunctions.FindFeatureLayer(m_theWorkspace, "MapUnitPolys");
                ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, mapUnitPolysLayer, null);
            }
            catch (Exception err) { MessageBox.Show(err.Message); }            
        }

    #endregion

    #endregion


    #region "Lithology Controls by Genhan"

        bool m_isLithUpdate = false;

        private Dictionary<string, StandardLithologyAccess.StandardLithology> m_StandardLithologyDeleteDictionary = new Dictionary<string, StandardLithologyAccess.StandardLithology>();

        private Dictionary<string, StandardLithologyAccess.StandardLithology> m_StandardLithologyDictionary = new Dictionary<string, StandardLithologyAccess.StandardLithology>();

        private void ClearLithologyInput()
        {
            liLith.SelectedIndex = -1;
            cboLith.SelectedIndex = -1;
            cboPartType.SelectedIndex = -1;
            cboPropTerm.SelectedIndex = -1;

            m_isLithUpdate = false;
        }

        private void initLithListBox(string mapUnit)
        {
            if (m_theWorkspace == null)
            {
                MessageBox.Show("Please open a working space!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            liLith.Items.Clear();
            ClearLithologyInput();

            m_StandardLithologyDictionary.Clear();
            m_StandardLithologyDeleteDictionary.Clear();

            StandardLithologyAccess lithAccess = new StandardLithologyAccess(m_theWorkspace);
            lithAccess.AddStandardLithology("MapUnit = '" + mapUnit + "'");

            foreach (KeyValuePair<string, StandardLithologyAccess.StandardLithology> aDictionaryEntry in lithAccess.StandardLithologyDictionary)
            {
                string aKey = aDictionaryEntry.Key;
                StandardLithologyAccess.StandardLithology aDictionaryValue = aDictionaryEntry.Value;
                liLith.Items.Add(aDictionaryValue.Lithology);

                m_StandardLithologyDictionary.Add(aKey, aDictionaryValue);
            }
            

        }

        private void btnNewLith_Click(object sender, EventArgs e)
        {
            cboLith.SelectedIndex = -1;
            cboPartType.SelectedIndex = -1;
            cboPropTerm.SelectedIndex = -1;
            liLith.ClearSelected();

            m_isLithUpdate = false;
        }

        private void btnAcceptLith_Click(object sender, EventArgs e)
        {
            if (m_theWorkspace != null && cboLith.SelectedIndex != -1)
            {
                string mapUnit = txtMapUnitAbbreviation.Text;
                string lithology = cboLith.SelectedItem.ToString();
                string partType = cboPartType.SelectedItem.ToString();
                string propTerm = cboPropTerm.SelectedItem.ToString();
                double propValue = getPropValue(propTerm);
                string resourceID = commonFunctions.GetCurrentDataSourceID();

                if (m_isLithUpdate)
                {
                    foreach (KeyValuePair<string, StandardLithologyAccess.StandardLithology> aDictionaryEntry in m_StandardLithologyDictionary)
                    {
                        string aLithology = aDictionaryEntry.Value.Lithology;
                        if (aLithology == lithology)
                        {
                            StandardLithologyAccess.StandardLithology aLithValue = aDictionaryEntry.Value;

                            aLithValue.Lithology = lithology;
                            aLithValue.PartType = partType;
                            aLithValue.ProportionTerm = propTerm;
                            aLithValue.ProportionValue = propValue;
                            aLithValue.RequiresUpdate = true;

                            m_StandardLithologyDictionary.Remove(aLithValue.StandardLithology_ID);
                            m_StandardLithologyDictionary.Add(aLithValue.StandardLithology_ID, aLithValue);
                            break;
                        }
                    }
                }
                else
                {
                    StandardLithologyAccess lithAccess = new StandardLithologyAccess(m_theWorkspace);
                    lithAccess.NewStandardLithology(mapUnit, partType, lithology, propTerm, propValue, null, resourceID);
                    string thisKey = lithAccess.StandardLithologyDictionary.First().Key;
                    StandardLithologyAccess.StandardLithology thisLith = lithAccess.StandardLithologyDictionary.First().Value;

                    m_StandardLithologyDictionary.Add(thisKey, thisLith);
                    liLith.Items.Add(thisLith.Lithology);
                }

                ClearLithologyInput();           
            }

        }

        private void liLith_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_theWorkspace != null && m_StandardLithologyDictionary.Count != 0) 
            {
                if (liLith.SelectedIndex != -1)
                {
                    m_isLithUpdate = true;
                    string lith = liLith.SelectedItem.ToString();

                    foreach (KeyValuePair<string, StandardLithologyAccess.StandardLithology> aDictionaryEntry in m_StandardLithologyDictionary)
                    {
                        string aLithology = aDictionaryEntry.Value.Lithology;

                        if (aLithology == lith)
                        {
                            int lithIndex = cboLith.Items.IndexOf(lith);
                            int pTypeIndex = cboPartType.Items.IndexOf(aDictionaryEntry.Value.PartType);
                            int propIndex = cboPropTerm.Items.IndexOf(aDictionaryEntry.Value.ProportionTerm);

                            cboLith.SelectedIndex = lithIndex;
                            cboPartType.SelectedIndex = pTypeIndex;
                            cboPropTerm.SelectedIndex = propIndex;
                        }
                    }

                }
            }

        }

        private void btnDeleteLith_Click(object sender, EventArgs e)
        {
            if (m_theWorkspace != null)
            {
                // Confirm box to delete the lithology record
                DialogResult result = MessageBox.Show("Are you sure to delete this record?", liLith.SelectedItem.ToString(), MessageBoxButtons.YesNo);
                if (result != DialogResult.Yes) { return; }

                string lithology = liLith.SelectedItem.ToString();

                foreach (KeyValuePair<string, StandardLithologyAccess.StandardLithology> aDictionaryEntry in m_StandardLithologyDictionary)
                {
                    if (aDictionaryEntry.Value.Lithology == lithology)
                    {
                        m_StandardLithologyDictionary.Remove(aDictionaryEntry.Key);
                        liLith.Items.Remove(lithology);

                        m_StandardLithologyDeleteDictionary.Add(aDictionaryEntry.Key, aDictionaryEntry.Value);
                        break;
                    }
                }

                ClearLithologyInput();
            }
        }

        private double getPropValue (string propTerm) 
        {
            switch (propTerm)
            {
                case "trace":
                    return 0.05;
                case "rare":
                    return 2;
                case "minor":
                    return 15;
                case "present":
                    return 35;
                case "variable":
                    return 35;
                case "less than half":
                    return 40;
                case "most abundant":
                    return 52;
                case "more than half":
                    return 60;
                case "major":
                    return 85;
                case "all":
                    return 100;
                default:
                    return 999;
            }

        }

        private void btnSaveLith_Click(object sender, EventArgs e)
        {
            saveAge();
            saveLithology();
            saveMapUnit();
        }

        private void saveLithology()
        {
            if (m_theWorkspace == null)
            {
                MessageBox.Show("Please open a working space!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtMapUnitAbbreviation.Text == "")
            {
                MessageBox.Show("Please select a valid map unit!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            updateLithology4MapUnit();
            string mapUnit = txtMapUnitAbbreviation.Text;          

            StandardLithologyAccess lithAccess = new StandardLithologyAccess(m_theWorkspace);
            lithAccess.AddStandardLithology("MapUnit = '" + mapUnit + "'");
            lithAccess.StandardLithologyDictionary = m_StandardLithologyDictionary;

            lithAccess.SaveStandardLithology();

            foreach (KeyValuePair<string, StandardLithologyAccess.StandardLithology> aDictionaryEntry in m_StandardLithologyDeleteDictionary)
            {
                lithAccess.DeleteStandardLithology(aDictionaryEntry.Value);
            }

            ClearLithologyInput();

        }

        #region "Update the related lithology if when the map unit info changes -- by Genhan"

        private void updateLithology4MapUnit()
        {
            if (m_StandardLithologyDictionary.Count != 0)
            {
                Dictionary<string, StandardLithologyAccess.StandardLithology> newStandardLithologyDictionary = new Dictionary<string, StandardLithologyAccess.StandardLithology>();
                foreach (KeyValuePair<string, StandardLithologyAccess.StandardLithology> aDictionaryEntry in m_StandardLithologyDictionary)
                {
                    string key = aDictionaryEntry.Key;
                    StandardLithologyAccess.StandardLithology aLithology = aDictionaryEntry.Value;
                    aLithology.MapUnit = txtMapUnitAbbreviation.Text;

                    newStandardLithologyDictionary.Add(key, aLithology);
                }

                m_StandardLithologyDictionary = newStandardLithologyDictionary;
            }
        }

        #endregion

        private void btnCancelLith_Click(object sender, EventArgs e)
        {
            cancelAge();
            cancelLithology();
            cancelMapUnit();
        }

        private void cancelLithology()
        {
            string mapUnit = txtMapUnitAbbreviation.Text;
            if (m_theWorkspace != null && mapUnit != "")
            {
                initLithListBox(mapUnit);
            }
        }

        private void deleteLithologyRecord(string mapUnit)
        {
            if (m_theWorkspace == null)
            {
                MessageBox.Show("Please open a working space!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /// Connect with the Lithology table
            StandardLithologyAccess lithAccess = new StandardLithologyAccess(m_theWorkspace);
            /// Search for the lithology record for this map unit
            lithAccess.AddStandardLithology("MapUnit = '" + mapUnit + "'");

            Dictionary<string, StandardLithologyAccess.StandardLithology> thisDictionary = new Dictionary<string, StandardLithologyAccess.StandardLithology>();

            foreach (KeyValuePair<string, StandardLithologyAccess.StandardLithology> aLithEntry in lithAccess.StandardLithologyDictionary)
            {
                thisDictionary.Add(aLithEntry.Key, aLithEntry.Value);
            }

            foreach (KeyValuePair<string, StandardLithologyAccess.StandardLithology> aDelLith in thisDictionary)
            {
                lithAccess.DeleteStandardLithology(aDelLith.Value);
            }
        }

    #endregion

    #region "Age Controls by Genhan"
        private Dictionary<string, GeologicEventsAccess.GeologicEvents> m_GeologicEventsDictionary = new Dictionary<string, GeologicEventsAccess.GeologicEvents>();
        private Dictionary<string, ExtendedAttributesAccess.ExtendedAttributes> m_ExtendedAttributesDictionary = new Dictionary<string, ExtendedAttributesAccess.ExtendedAttributes>();
        //private ExtendedAttributesAccess.ExtendedAttributes m_ExtendedAttributes = new ExtendedAttributesAccess.ExtendedAttributes();
        private string m_initMapUnit;

        private Dictionary<string, string> m_EvtListDictionary = new Dictionary<string, string>();
        private bool isUpdate4AgeEvent = false;

        private void initAgeTab(string mapUnit)
        {
            /// Set the dictionary to search era term, its start time and end time
            initTimeScaleDictionary();
            if (liEvts.Items.Count == 0) { initAgeEventsListbox(); }

            if (mapUnit == null || mapUnit == "") { initEmptyAgeEventTab(); }
            else {
                initEmptyAgeEventTab();
                initAgeEventTab(mapUnit); 
            }
        }

        /// <summary>
        /// Set values for the age event tabs
        /// The values are from Extended Attributes table and Geologic Events table
        /// </summary>
        private void initAgeEventTab(string mapUnit)
        {
            m_initMapUnit = mapUnit;

            if (m_theWorkspace == null)
            {
                MessageBox.Show("Please open a working space!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /// Connect with the Extended Attribute table
            ExtendedAttributesAccess extAttrAccess = new ExtendedAttributesAccess(m_theWorkspace);
            /// Search the extended attributes records for this map unit
            extAttrAccess.AddExtendedAttributes("OwnerID = '" + mapUnit + "'");
            m_ExtendedAttributesDictionary = extAttrAccess.ExtendedAttributesDictionary;
            foreach (KeyValuePair<string, ExtendedAttributesAccess.ExtendedAttributes> anExtendedAttributeEntry in extAttrAccess.ExtendedAttributesDictionary)
            {
                ExtendedAttributesAccess.ExtendedAttributes thisExtendedAttribute = anExtendedAttributeEntry.Value;

                string vLinkId = thisExtendedAttribute.ValueLinkID;
                if (vLinkId != null)
                {
                    /// Connect with Geologic Events table
                    GeologicEventsAccess geoEvtsAccess = new GeologicEventsAccess(m_theWorkspace);
                    geoEvtsAccess.AddGeologicEvents("GeologicEvents_ID = '" + vLinkId + "'");

                    if (geoEvtsAccess.GeologicEventsDictionary.Count != 0)
                    {
                        GeologicEventsAccess.GeologicEvents thisGeologicEvent = geoEvtsAccess.GeologicEventsDictionary.First().Value;
                        liEvts4ThisUnit.Items.Add(thisGeologicEvent.AgeDisplay);
                    }
                }
            }
        }

        /// <summary>
        /// Add values to the Geologic Events Tab
        /// </summary>
        private void initAgeEventsListbox()
        {
            if (m_theWorkspace == null)
            {
                MessageBox.Show("Please open a working space!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                /// Pull out all the records from the Geologic Events table
                GeologicEventsAccess geoEvtsAccess = new GeologicEventsAccess(m_theWorkspace); /// Create a new geologic events access object
                geoEvtsAccess.AddGeologicEvents(); /// Search for all the events                                                

                /// Clear the content of geologic events dictionary and event list dictionary
                m_GeologicEventsDictionary.Clear();
                m_EvtListDictionary.Clear();

                m_GeologicEventsDictionary = geoEvtsAccess.GeologicEventsDictionary; /// Replace the old geologic events dictionary

                /// Add the age displays into the list box
                foreach (KeyValuePair<string, GeologicEventsAccess.GeologicEvents> aGeologicEventEntry in m_GeologicEventsDictionary)
                {
                    string ageDisplay = aGeologicEventEntry.Value.AgeDisplay;
                    liEvts.Items.Add(ageDisplay);
                    m_EvtListDictionary.Add(ageDisplay, aGeologicEventEntry.Key); /// Add new connection between age diaplay and event id
                }

            }
            catch (Exception e) { throw (e); }
        }

        #region "Initialize the empty tabs"
            private void initEmptyAgeEventTab()
            {
                liEvts4ThisUnit.Items.Clear();
                liEvts.SelectedIndex = -1;

                initEmptyEventTab();
            }

            private void initEmptyEventTab()
            {
                txtAgeDisplay.Clear();
                cboEventType.SelectedIndex = 0;
                cboEvt.SelectedIndex = -1;
                txtNotes.Clear();

                initEmptySingleTimeScale();
                initEmptyRangeTimeScale();

                isUpdate4AgeEvent = false;
            }

            private void initEmptySingleTimeScale()
            {
                cboSEra.SelectedIndex = -1;
                txtSOlderAge.Clear();
                txtSYoungerAge.Clear();
            }

            private void initEmptyRangeTimeScale()
            {
                cboROlderEra.SelectedIndex = -1;
                cboRYoungerEra.SelectedIndex = -1;
                txtRYoungerAge.Clear();
                txtROlderAge.Clear();
            }
        #endregion

        #region "Functions in age and event list tab by Genhan"
            private void btnAgeAdd_Click(object sender, EventArgs e)
            {
                tabEvtEditor.SelectedTab = tabAgeEvent;
                initEmptyEventTab();
                isUpdate4AgeEvent = false;
                liEvts.SelectedIndex = -1;
            }

            private void btnAgeChangeAccept_Click(object sender, EventArgs e)
            {
                if (liEvts.SelectedIndex == -1) { return; }
                if (m_theWorkspace == null) { return; }
                if (liEvts4ThisUnit.Items.Contains(liEvts.SelectedItem.ToString())) { return; }

                liEvts4ThisUnit.Items.Add(liEvts.SelectedItem.ToString());
                string ownerId = m_EvtListDictionary[liEvts.SelectedItem.ToString()];

                /// Generate a new Extended Attribute record
                ExtendedAttributesAccess extAttrAccess = new ExtendedAttributesAccess(m_theWorkspace);
                string thisExtAttrId = extAttrAccess.NewExtendedAttributes(m_initMapUnit, null, null, null, ownerId, null, commonFunctions.GetCurrentDataSourceID(), null);
                ExtendedAttributesAccess.ExtendedAttributes thisExtAttr = extAttrAccess.ExtendedAttributesDictionary.First().Value;
                m_ExtendedAttributesDictionary.Add(thisExtAttrId, thisExtAttr);

            }

            private void btnAgeDelete_Click(object sender, EventArgs e)
            {
                if (liEvts.Focused)
                {
                    if (liEvts.SelectedIndex == -1) { return; }

                    string selectedString = liEvts.SelectedItem.ToString();
                    /// Remove the selected item from the event list box
                    liEvts.Items.Remove(liEvts.SelectedItem);

                    /// Remove the selected item from the list box dictionary
                    string valueLinkId = m_EvtListDictionary[selectedString];
                    m_GeologicEventsDictionary.Remove(valueLinkId);
                    m_EvtListDictionary.Remove(selectedString);

                    /// Remove the related extended attribute record
                    foreach (KeyValuePair<string, ExtendedAttributesAccess.ExtendedAttributes> anExtAttrEntry in m_ExtendedAttributesDictionary)
                    {
                        ExtendedAttributesAccess.ExtendedAttributes thisExtAttr = anExtAttrEntry.Value;
                        if (thisExtAttr.ValueLinkID == valueLinkId)
                        {
                            m_ExtendedAttributesDictionary.Remove(anExtAttrEntry.Key);
                            liEvts4ThisUnit.Items.Remove(selectedString);
                            return;
                        }
                    }

                    isUpdate4AgeEvent = false;
                }

                if (liEvts4ThisUnit.Focused)
                {
                    if (liEvts4ThisUnit.SelectedIndex == -1) { return; }

                    string selectedString = liEvts4ThisUnit.SelectedItem.ToString();
                    liEvts4ThisUnit.Items.Remove(selectedString);

                    /// Remove the selected item from this map unit events dictionary
                    foreach (KeyValuePair<string, ExtendedAttributesAccess.ExtendedAttributes> anExtAttrEntry in m_ExtendedAttributesDictionary)
                    {
                        ExtendedAttributesAccess.ExtendedAttributes thisExtAttr = anExtAttrEntry.Value;
                        if (thisExtAttr.ValueLinkID == m_EvtListDictionary[selectedString])
                        {
                            m_ExtendedAttributesDictionary.Remove(anExtAttrEntry.Key);
                            return;
                        }
                    }
                }
            }

            private void liEvts_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (liEvts.SelectedIndex == -1)
                {
                    if (isUpdate4AgeEvent) { return; }
                    initEmptyEventTab();
                }
                else
                {
                    setAgeEventTabContent(m_EvtListDictionary[liEvts.SelectedItem.ToString()]);
                    isUpdate4AgeEvent = true;
                }

            }
            
            /// <summary>
            /// Display the attributtes of the selected item on the event tab
            /// </summary>
            /// <param name="eventId">The selected event item's id</param>
            private void setAgeEventTabContent(string eventId)
            {
                GeologicEventsAccess.GeologicEvents thisGeologicEvents = m_GeologicEventsDictionary[eventId];

                txtAgeDisplay.Text = thisGeologicEvents.AgeDisplay;
                cboEvt.SelectedItem = thisGeologicEvents.Event;
                txtNotes.Text = thisGeologicEvents.Notes;

                /// Identify if this event is single age event or age range event
                if (thisGeologicEvents.AgeOlderTerm == thisGeologicEvents.AgeYoungerTerm)
                {
                    cboEventType.SelectedItem = "Single Age Event";
                    cboSEra.SelectedItem = thisGeologicEvents.AgeOlderTerm;
                    txtSOlderAge.Text = thisGeologicEvents.AgeYoungerValue;
                    txtSYoungerAge.Text = thisGeologicEvents.AgeOlderValue;
                }
                else
                {
                    cboEventType.SelectedItem = "Age Range Event";
                    cboRYoungerEra.SelectedItem = thisGeologicEvents.AgeYoungerTerm;
                    cboROlderEra.SelectedItem = thisGeologicEvents.AgeOlderTerm;
                    txtROlderAge.Text = thisGeologicEvents.AgeYoungerValue;
                    txtRYoungerAge.Text = thisGeologicEvents.AgeOlderValue;
                }

            }
        #endregion
        
        #region "Functions in event details tab by Genhan"
            private struct TimeScale
            {
                public string label;
                public double startTime;
                public double endTime;
            }
            private Dictionary<string, TimeScale> m_TimeScaleDictionary = new Dictionary<string, TimeScale>();  /// Timescale dictionary with era term, start time and end time    
            private void initTimeScaleDictionary()
            {
                if (m_TimeScaleDictionary.Count != 0) { return; }
                string[] labels = {"Holocene Epoch",
                                    "Quaternary Period",
                                    "Cainozoic Era",
                                    "Cenozoic Era",
                                    "Phanerozoic Eon",
                                    "Upper Pleistocene Age",
                                    "Pleistocene Epoch",
                                    "Ionian Age",
                                    "Calabrian Age",
                                    "Gelasian Age",
                                    "Piacenzian Age",
                                    "Pliocene Epoch",
                                    "Neogene Period",
                                    "Zanclean Age",
                                    "Messinian Age",
                                    "Miocene Epoch",
                                    "Tortonian Age",
                                    "Serravallian Age",
                                    "Langhian Age",
                                    "Burdigalian Age",
                                    "Aquitanian Age",
                                    "Chattian Age",
                                    "Oligocene Epoch",
                                    "Palaeogene Period",
                                    "Rupelian Age",
                                    "Priabonian Age",
                                    "Eocene Epoch",
                                    "Bartonian Age",
                                    "Lutetian Age",
                                    "Ypresian Age",
                                    "Thanetian Age",
                                    "Palaeocene Epoch",
                                    "Selandian Age",
                                    "Danian Age",
                                    "Maastrichtian Age",
                                    "Upper Cretaceous Epoch",
                                    "Cretaceous Period",
                                    "Mesozoic Era",
                                    "Campanian Age",
                                    "Santonian Age",
                                    "Coniacian Age",
                                    "Turonian Age",
                                    "Cenomanian Age",
                                    "Albian Age",
                                    "Lower Cretaceous Epoch",
                                    "Aptian Age",
                                    "Barremian Age",
                                    "Hauterivian Age",
                                    "Valanginian Age",
                                    "Berriasian Age",
                                    "Tithonian Age",
                                    "Upper Jurassic Epoch",
                                    "Jurassic Period",
                                    "Kimmeridgian Age",
                                    "Oxfordian Age",
                                    "Callovian Age",
                                    "Middle Jurassic Epoch",
                                    "Bathonian Age",
                                    "Bajocian Age",
                                    "Aalenian Age",
                                    "Toarcian Age",
                                    "Lower Jurassic Epoch",
                                    "Pliensbachian Age",
                                    "Sinemurian Age",
                                    "Hettangian Age",
                                    "Rhaetian Age",
                                    "Upper Triassic Epoch",
                                    "Triassic Period",
                                    "Norian Age",
                                    "Carnian Age",
                                    "Ladinian Age",
                                    "Middle Triassic Epoch",
                                    "Anisian Age",
                                    "Olenekian Age",
                                    "Lower Triassic Epoch",
                                    "Induan Age",
                                    "Changhsingian Age",
                                    "Lopingian Epoch",
                                    "Permian Period",
                                    "Palaeozoic Era",
                                    "Wuchiapingian Age",
                                    "Guadalupian Epoch",
                                    "Wordian Age",
                                    "Roadian Age",
                                    "Capitanian Age",
                                    "Kungurian Age",
                                    "Cisuralian Epoch",
                                    "Artinskian Age",
                                    "Sakmarian Age",
                                    "Asselian Age",
                                    "Gzhelian Age",
                                    "Upper Pennsylvanian Epoch",
                                    "Pennsylvanian Sub-period",
                                    "Upper Mississippian Epoch",
                                    "Carboniferous Period",
                                    "Kasimovian Age",
                                    "Middle Pennsylvanian Epoch",
                                    "Moscovian Age",
                                    "Bashkirian Age",
                                    "Lower Pennsylvanian Epoch",
                                    "Serpukhovian Age",
                                    "Mississippian Sub-period",
                                    "Middle Mississippian Epoch",
                                    "Visean Age",
                                    "Lower Mississippian Epoch",
                                    "Tournaisian Age",
                                    "Famennian Age",
                                    "Upper Devonian Epoch",
                                    "Devonian Period",
                                    "Frasnian Age",
                                    "Givetian Age",
                                    "Middle Devonian Epoch",
                                    "Eifelian Age",
                                    "Emsian Age",
                                    "Lower Devonian Epcoh",
                                    "Pragian Age",
                                    "Lochkovian Age",
                                    "Pridoli Epoch",
                                    "Silurian Period",
                                    "Ludfordian Age",
                                    "Gorstian Age",
                                    "Ludlow Epoch",
                                    "Homerian Age",
                                    "Wenlock Epoch",
                                    "Sheinwoodian Age",
                                    "Telychian Age",
                                    "Llandovery Epoch",
                                    "Aeronian Age",
                                    "Rhuddanian Age",
                                    "Hirnantian Age",
                                    "Upper Ordovician Epoch",
                                    "Ordovician Period",
                                    "Katian Age",
                                    "Sandbian Age",
                                    "Darriwilian Age",
                                    "Middle Ordovician Epoch",
                                    "Dapingian Age",
                                    "Floian Age",
                                    "Lower Ordovician Epoch",
                                    "Tremadocian Age",
                                    "Cambrian Stage 10 Age",
                                    "Furongian Epoch",
                                    "Cambrian Period",
                                    "Cambrian Stage 9 Age",
                                    "Paibian Age",
                                    "Guzhangian Age",
                                    "Cambrian Series 3 Epoch",
                                    "Drumian Age",
                                    "Cambrian Stage 5 Age",
                                    "Cambrian Stage 4 Age",
                                    "Cambrian Series 2 Epoch",
                                    "Cambrian Stage 3 Age",
                                    "Cambrian Stage 2 Age",
                                    "Terreneuvian Epoch",
                                    "Fortunian Age",
                                    "Ediacaran Period",
                                    "Neoproterozoic Era",
                                    "Proterozoic Eon",
                                    "Precambrian Supereon",
                                    "Cryogenian Period",
                                    "Tonian Period",
                                    "Stenian Period",
                                    "Mesoproterozoic Era",
                                    "Ectasian Period",
                                    "Calymmian Period",
                                    "Statherian Period",
                                    "Palaeoproterozoic Era",
                                    "Orosirian Period",
                                    "Rhyacian Period",
                                    "Siderian Period",
                                    "Neoarchaean Era",
                                    "Archaean Eon",
                                    "Mesoarchaean Era",
                                    "Palaeoarchaean Era",
                                    "Eoarchaean Era",
                                    "Hadean Eon"
                                    };
                double[] startTimes = {0.0117,
                                        2.588,
                                        65.5,
                                        65.5,
                                        542,
                                        0.126,
                                        2.588,
                                        0.781,
                                        1.806,
                                        2.588,
                                        3.6,
                                        5.332,
                                        23.02,
                                        5.332,
                                        7.246,
                                        23.02,
                                        11.608,
                                        13.82,
                                        15.97,
                                        20.43,
                                        23.02,
                                        28.4,
                                        33.9,
                                        65.5,
                                        33.9,
                                        37.2,
                                        55.8,
                                        40.4,
                                        48.6,
                                        55.8,
                                        58.7,
                                        65.5,
                                        61.1,
                                        65.5,
                                        70.6,
                                        99.6,
                                        145.5,
                                        251,
                                        83.5,
                                        85.8,
                                        89.3,
                                        93.6,
                                        99.6,
                                        112,
                                        145.5,
                                        125,
                                        130,
                                        133.9,
                                        140.2,
                                        145.5,
                                        150.8,
                                        161.2,
                                        199.6,
                                        155.6,
                                        161.2,
                                        164.7,
                                        175.6,
                                        167.7,
                                        171.6,
                                        175.6,
                                        183,
                                        199.6,
                                        189.6,
                                        196.5,
                                        199.6,
                                        203.6,
                                        228.7,
                                        251,
                                        216.5,
                                        228.7,
                                        237,
                                        245.9,
                                        245.9,
                                        249.5,
                                        251,
                                        251,
                                        253.8,
                                        260.4,
                                        299,
                                        542,
                                        260.4,
                                        270.6,
                                        268,
                                        270.6,
                                        265.8,
                                        275.6,
                                        299,
                                        284.4,
                                        294.6,
                                        299,
                                        303.4,
                                        307.2,
                                        318.1,
                                        328.3,
                                        359.2,
                                        307.2,
                                        311.7,
                                        311.7,
                                        318.1,
                                        318.1,
                                        328.3,
                                        359.2,
                                        345.3,
                                        345.3,
                                        359.2,
                                        359.2,
                                        374.5,
                                        385.3,
                                        416,
                                        385.3,
                                        391.8,
                                        397.5,
                                        397.5,
                                        407,
                                        416,
                                        411.2,
                                        416,
                                        418.7,
                                        443.7,
                                        421.3,
                                        422.9,
                                        418.7,
                                        426.2,
                                        428.2,
                                        428.2,
                                        436,
                                        443.7,
                                        439,
                                        443.7,
                                        445.6,
                                        460.9,
                                        488.3,
                                        455.8,
                                        460.9,
                                        468.1,
                                        471.8,
                                        471.8,
                                        478.6,
                                        488.3,
                                        488.3,
                                        492,
                                        499,
                                        542,
                                        496,
                                        499,
                                        503,
                                        510,
                                        506.5,
                                        510,
                                        517,
                                        521,
                                        521,
                                        528,
                                        542,
                                        542,
                                        635,
                                        1000,
                                        2500,
                                        9999.9999,
                                        850,
                                        1000,
                                        1200,
                                        1600,
                                        1400,
                                        1600,
                                        1800,
                                        2500,
                                        2050,
                                        2300,
                                        2500,
                                        2800,
                                        4000,
                                        3200,
                                        3600,
                                        4000,
                                        9999.9999};
                double[] endTimes = {0,
                                        0,
                                        0,
                                        0,
                                        0,
                                        0.0117,
                                        0.0117,
                                        0.126,
                                        0.781,
                                        1.806,
                                        2.588,
                                        2.588,
                                        2.588,
                                        3.6,
                                        5.332,
                                        5.332,
                                        7.246,
                                        11.608,
                                        13.82,
                                        15.97,
                                        20.43,
                                        23.02,
                                        23.02,
                                        23.02,
                                        28.4,
                                        33.9,
                                        33.9,
                                        37.2,
                                        40.4,
                                        48.6,
                                        55.8,
                                        55.8,
                                        58.7,
                                        61.1,
                                        65.5,
                                        65.5,
                                        65.5,
                                        65.5,
                                        70.6,
                                        83.5,
                                        85.8,
                                        89.3,
                                        93.6,
                                        99.6,
                                        99.6,
                                        112,
                                        125,
                                        130,
                                        133.9,
                                        140.2,
                                        145.5,
                                        145.5,
                                        145.5,
                                        150.8,
                                        155.6,
                                        161.2,
                                        161.2,
                                        164.7,
                                        167.7,
                                        171.6,
                                        175.6,
                                        175.6,
                                        183,
                                        189.6,
                                        196.5,
                                        199.6,
                                        199.6,
                                        199.6,
                                        203.6,
                                        216.5,
                                        228.7,
                                        228.7,
                                        237,
                                        245.9,
                                        245.9,
                                        249.5,
                                        251,
                                        251,
                                        251,
                                        251,
                                        253.8,
                                        260.4,
                                        265.8,
                                        268,
                                        270.6,
                                        270.6,
                                        270.6,
                                        275.6,
                                        284.4,
                                        294.6,
                                        299,
                                        299,
                                        299,
                                        299,
                                        299,
                                        303.4,
                                        307.2,
                                        307.2,
                                        311.7,
                                        311.7,
                                        318.1,
                                        318.1,
                                        328.3,
                                        328.3,
                                        345.3,
                                        345.3,
                                        359.2,
                                        359.2,
                                        359.2,
                                        374.5,
                                        385.3,
                                        385.3,
                                        391.8,
                                        397.5,
                                        397.5,
                                        407,
                                        411.2,
                                        416,
                                        416,
                                        418.7,
                                        421.3,
                                        422.9,
                                        422.9,
                                        422.9,
                                        426.2,
                                        428.2,
                                        428.2,
                                        436,
                                        439,
                                        443.7,
                                        443.7,
                                        443.7,
                                        445.6,
                                        455.8,
                                        460.9,
                                        460.9,
                                        468.1,
                                        471.8,
                                        471.8,
                                        478.6,
                                        488.3,
                                        488.3,
                                        488.3,
                                        492,
                                        496,
                                        499,
                                        499,
                                        503,
                                        506.5,
                                        510,
                                        510,
                                        517,
                                        521,
                                        521,
                                        528,
                                        542,
                                        542,
                                        542,
                                        542,
                                        635,
                                        850,
                                        1000,
                                        1000,
                                        1200,
                                        1400,
                                        1600,
                                        1600,
                                        1800,
                                        2050,
                                        2300,
                                        2500,
                                        2500,
                                        2800,
                                        3200,
                                        3600,
                                        4000};

                for (int i = 0; i < labels.Length; i++)
                {
                    TimeScale aTimeScale = new TimeScale();
                    aTimeScale.label = labels[i];
                    aTimeScale.startTime = startTimes[i];
                    aTimeScale.endTime = endTimes[i];

                    m_TimeScaleDictionary.Add(aTimeScale.label, aTimeScale);
                }
            } 

            /// <summary>
            /// Add/update geologic event in m_GeologicEventsDictionary and events list box
            /// </summary>
            private void btnEvtAccept_Click(object sender, EventArgs e)
            {
                /// <start> Validate the event inputs before save
                if (m_theWorkspace == null) {
                    MessageBox.Show("Please open a working space!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; 
                }
                if (!validateEvtInputs()) { return; } 

                if (txtAgeDisplay.Text == "") {
                    MessageBox.Show("Please input Age Display value!", "Invalid Inputs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                /// <end>


                GeologicEventsAccess thisGeologicEventsAccess = new GeologicEventsAccess(m_theWorkspace);
                string dataSrcID = commonFunctions.GetCurrentDataSourceID();
                string thisKey;
                GeologicEventsAccess.GeologicEvents thisGeologicEvents = new GeologicEventsAccess.GeologicEvents();

                if (isUpdate4AgeEvent)
                {
                    GeologicEventsAccess.GeologicEvents selectedEvent = m_GeologicEventsDictionary[m_EvtListDictionary[liEvts.SelectedItem.ToString()]];
                    
                    /// Remove the old age display text from the age tab
                    liEvts.Items.Remove(selectedEvent.AgeDisplay);

                    /// Remove item from the evet list for this map unit
                    if (liEvts4ThisUnit.Items.Contains(selectedEvent.AgeDisplay))
                    {
                        liEvts4ThisUnit.Items.Remove(selectedEvent.AgeDisplay);
                        string thisGeoEvtID = m_EvtListDictionary[selectedEvent.AgeDisplay];

                        List<string> listIds = new List<string>();
                        foreach (KeyValuePair<string, ExtendedAttributesAccess.ExtendedAttributes> anExtAttrEntry in m_ExtendedAttributesDictionary)
                        {
                            ExtendedAttributesAccess.ExtendedAttributes thisExtAttr = anExtAttrEntry.Value;
                            if (thisExtAttr.ValueLinkID == thisGeoEvtID)
                            {
                                string thisID = anExtAttrEntry.Key;                               
                                listIds.Add(thisID);
                            }
                        }

                        for (int i = 0; i < listIds.Count; i++) { m_ExtendedAttributesDictionary.Remove(listIds[i]); }
                    }

                    /// Remove the old connection between list item and age
                    m_EvtListDictionary.Remove(selectedEvent.AgeDisplay);

                    selectedEvent.AgeDisplay = txtAgeDisplay.Text;
                    selectedEvent.Event = cboEvt.SelectedItem.ToString();
                    selectedEvent.TimeScale = "http://resource.geosciml.org/classifierscheme/ics/ischart/2009";
                    selectedEvent.Notes = txtNotes.Text;
                    selectedEvent.DataSourceID = commonFunctions.GetCurrentDataSourceID();

                    switch (cboEventType.SelectedItem.ToString())
                    {
                        case "Single Age Event":
                            selectedEvent.AgeYoungerTerm = cboSEra.SelectedItem.ToString();
                            selectedEvent.AgeOlderTerm = cboSEra.SelectedItem.ToString();
                            selectedEvent.AgeYoungerValue = txtSOlderAge.Text;
                            selectedEvent.AgeOlderValue = txtSYoungerAge.Text;
                            break;
                        case "Age Range Event":
                            selectedEvent.AgeYoungerTerm = cboRYoungerEra.SelectedItem.ToString();
                            selectedEvent.AgeOlderTerm = cboROlderEra.SelectedItem.ToString();
                            selectedEvent.AgeYoungerValue = txtROlderAge.Text;
                            selectedEvent.AgeOlderValue = txtRYoungerAge.Text;
                            break;
                    }

                    m_GeologicEventsDictionary.Remove(selectedEvent.GeologicEvents_ID);
                    m_GeologicEventsDictionary.Add(selectedEvent.GeologicEvents_ID, selectedEvent);

                    if (m_EvtListDictionary.ContainsKey(txtAgeDisplay.Text))
                    {
                        MessageBox.Show("This age display already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        /// Add text item into event list box
                        liEvts.Items.Add(selectedEvent.AgeDisplay);
                        /// Add the new item into list box dictionary
                        m_EvtListDictionary.Add(selectedEvent.AgeDisplay, selectedEvent.GeologicEvents_ID);
                    }
                }
                else
                {
                    switch (cboEventType.SelectedItem.ToString())
                    {
                        case "Single Age Event":
                            thisGeologicEventsAccess.NewGeologicEvents(cboEvt.SelectedItem.ToString(), txtAgeDisplay.Text, cboSEra.SelectedItem.ToString(), cboSEra.SelectedItem.ToString(),
                                "http://resource.geosciml.org/classifierscheme/ics/ischart/2009", txtSOlderAge.Text, txtSYoungerAge.Text, dataSrcID, txtNotes.Text);
                            thisKey = thisGeologicEventsAccess.GeologicEventsDictionary.First().Key;
                            thisGeologicEvents = thisGeologicEventsAccess.GeologicEventsDictionary.First().Value;
                            /// Add the new event into the dictionary
                            m_GeologicEventsDictionary.Add(thisKey, thisGeologicEvents);
                            break;
                        case "Age Range Event":
                            thisGeologicEventsAccess.NewGeologicEvents(cboEvt.SelectedItem.ToString(), txtAgeDisplay.Text, cboRYoungerEra.SelectedItem.ToString(), cboROlderEra.SelectedItem.ToString(),
                                "http://resource.geosciml.org/classifierscheme/ics/ischart/2009", txtROlderAge.Text, txtRYoungerAge.Text, dataSrcID, txtNotes.Text);
                            thisKey = thisGeologicEventsAccess.GeologicEventsDictionary.First().Key;
                            thisGeologicEvents = thisGeologicEventsAccess.GeologicEventsDictionary.First().Value;
                            /// Add the new event into the dictionary
                            m_GeologicEventsDictionary.Add(thisKey, thisGeologicEvents);
                            break;
                    }

                    if (m_EvtListDictionary.ContainsKey(txtAgeDisplay.Text))
                    {
                        MessageBox.Show("This age display already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        /// Add text item into event list box
                        liEvts.Items.Add(txtAgeDisplay.Text);
                        /// Add the new item into list box dictionary
                        m_EvtListDictionary.Add(txtAgeDisplay.Text, thisGeologicEvents.GeologicEvents_ID);
                    }
                }

                /// Switch into event list tab
                tabEvtEditor.SelectedTab = tabAgeList;
                /// Clear the event editing tab
                initEmptyEventTab();
            }

            private void cboEventType_SelectedIndexChanged(object sender, EventArgs e)
            {
                switch (cboEventType.SelectedIndex)
                {
                    case 0:
                        grpSingleTimeScale.Show();
                        grpRangeTimeScale.Hide();
                        initEmptyRangeTimeScale();
                        break;
                    case 1:
                        grpSingleTimeScale.Hide();
                        grpRangeTimeScale.Show();
                        initEmptySingleTimeScale();
                        break;
                }
            }

            private void cboSEra_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (cboSEra.SelectedIndex == -1) { return; }

                string thisEra = cboSEra.SelectedItem.ToString();
                TimeScale thisTimeScale = m_TimeScaleDictionary[thisEra];
                txtSOlderAge.Text = thisTimeScale.startTime.ToString();
                txtSYoungerAge.Text = thisTimeScale.endTime.ToString();
            }      

            private void cboRYoungerEra_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (cboRYoungerEra.SelectedIndex == -1) { return; }

                string thisYoungerEra = cboRYoungerEra.SelectedItem.ToString();
                TimeScale thisYoungerTimeScale = m_TimeScaleDictionary[thisYoungerEra];
                txtRYoungerAge.Text = thisYoungerTimeScale.endTime.ToString();
            }

            private void cboROlderEra_SelectedIndexChanged(object sender, EventArgs e)
            {
                if (cboROlderEra.SelectedIndex == -1) { return; }

                string thisOlderEra = cboROlderEra.SelectedItem.ToString();
                TimeScale thisOlderTimeScale = m_TimeScaleDictionary[thisOlderEra];
                txtROlderAge.Text = thisOlderTimeScale.startTime.ToString();
            }
            
            /// <summary>
            /// Generate the Age Display string in the event tab
            /// </summary>
            private void btnAgeGen_Click(object sender, EventArgs e)
            {
                if (!validateEvtInputs()) { return; }

                txtAgeDisplay.Text = cboEvt.SelectedItem.ToString() + "-" + cboEventType.SelectedItem.ToString();
                string evtTerm = cboEvt.SelectedItem.ToString();

                switch (cboEventType.SelectedItem.ToString())
                {
                    case "Single Age Event":
                        txtAgeDisplay.Text = evtTerm + "; " 
                            + cboSEra.SelectedItem.ToString() + ","
                            + txtSOlderAge.Text + "Ma - " 
                            + txtSYoungerAge.Text + "Ma";
                        break;
                    case "Age Range Event":
                        txtAgeDisplay.Text = evtTerm + "; "
                            + cboRYoungerEra.SelectedItem.ToString() + "," + txtROlderAge.Text + "Ma - "
                            + cboROlderEra.SelectedItem.ToString() + "," + txtRYoungerAge.Text + "Ma";
                        break;
                }
            }

            /// <summary> 
            /// Validate the inputs
            /// Return bool value to identify if the inputs are valid or not
            /// <summary>
            private bool validateEvtInputs()
            {                
                double youngerAge = 0.0, olderAge = 0.0;
                switch (cboEventType.SelectedItem.ToString())
                {
                    case "Single Age Event":
                        if (txtSYoungerAge.Text == "" || txtSOlderAge.Text == "") { return false; }
                        youngerAge = double.Parse(txtSYoungerAge.Text);
                        olderAge = double.Parse(txtSOlderAge.Text);
                        break;
                    case "Age Range Event":
                        if (txtRYoungerAge.Text == "" || txtROlderAge.Text == "") { return false; }
                        youngerAge = double.Parse(txtRYoungerAge.Text);
                        olderAge = double.Parse(txtROlderAge.Text);
                        break;
                }
                if (youngerAge > olderAge)
                {
                    MessageBox.Show("Max Age cannot be younger than min age!", "Invalid Inputs", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
        #endregion

        private void btnSaveAge_Click(object sender, EventArgs e)
        {
            saveAge();
            saveLithology();
            saveMapUnit();
        }

        private void saveAge()
        {
            if (m_theWorkspace == null)
            {
                MessageBox.Show("Please open a working space!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtMapUnitAbbreviation.Text == "")
            {
                MessageBox.Show("Please select a valid map unit!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /// ---------------------------------------------------------------------------------------------------------
            /// <start> Settings to save geologic events changes --------------------------------------------------------
            /// Connect with the Geologic Events table
            GeologicEventsAccess geoEvtsAccess = new GeologicEventsAccess(m_theWorkspace);
            geoEvtsAccess.AddGeologicEvents();

            Dictionary<string, GeologicEventsAccess.GeologicEvents> deleteGeologicEventsDictionary = new Dictionary<string, GeologicEventsAccess.GeologicEvents>();
            foreach (KeyValuePair<string, GeologicEventsAccess.GeologicEvents> anOldGeologicEventsEntry in geoEvtsAccess.GeologicEventsDictionary)
            {
                /// Identify if the old record still exits in the new dictionary
                /// If not, delete this record
                if (!m_GeologicEventsDictionary.ContainsKey(anOldGeologicEventsEntry.Key))
                { deleteGeologicEventsDictionary.Add(anOldGeologicEventsEntry.Key, anOldGeologicEventsEntry.Value); }
            }
            foreach (KeyValuePair<string, GeologicEventsAccess.GeologicEvents> anDeleteGeologicEventsEntry in deleteGeologicEventsDictionary)
            {
                GeologicEventsAccess.GeologicEvents thisGeoEvt = anDeleteGeologicEventsEntry.Value;
                /// Remove item from the evet list for this map unit
                if (liEvts4ThisUnit.Items.Contains(thisGeoEvt.AgeDisplay))
                {
                    liEvts4ThisUnit.Items.Remove(thisGeoEvt.AgeDisplay);
                    string thisGeoEvtID = m_EvtListDictionary[thisGeoEvt.AgeDisplay];

                    foreach (KeyValuePair<string, ExtendedAttributesAccess.ExtendedAttributes> anExtAttrEntry in m_ExtendedAttributesDictionary)
                    {
                        ExtendedAttributesAccess.ExtendedAttributes thisExtAttr = anExtAttrEntry.Value;
                        if (thisExtAttr.ValueLinkID == thisGeoEvtID)
                        {
                            string thisID = anExtAttrEntry.Key;
                            m_ExtendedAttributesDictionary.Remove(thisID);
                        }
                    }
                }
                geoEvtsAccess.DeleteGeologicEvents(thisGeoEvt);
            }

            geoEvtsAccess.GeologicEventsDictionary = m_GeologicEventsDictionary;

            geoEvtsAccess.SaveGeologicEvents();

            /// Reset variable m_GeologicEventsDictionary and m_EvtListDictionary
            geoEvtsAccess = new GeologicEventsAccess(m_theWorkspace);
            geoEvtsAccess.AddGeologicEvents();
            m_GeologicEventsDictionary = geoEvtsAccess.GeologicEventsDictionary;
            m_EvtListDictionary.Clear();

            foreach (KeyValuePair<string, GeologicEventsAccess.GeologicEvents> aGeologicEventsEntry in geoEvtsAccess.GeologicEventsDictionary)
            {
                m_EvtListDictionary.Add(aGeologicEventsEntry.Value.AgeDisplay, aGeologicEventsEntry.Key);
            }
            
            /// <end> ---------------------------------------------------------------------------------------------------
            /// ---------------------------------------------------------------------------------------------------------

            /// ---------------------------------------------------------------------------------------------------------
            /// <start> Settings to save extended attributes changes ----------------------------------------------------
            /// connect with the Extended Attributes table
            ExtendedAttributesAccess extAttrAccess = new ExtendedAttributesAccess(m_theWorkspace);
            /// Search for the related record in the extended attribute table with the old map unit value
            extAttrAccess.AddExtendedAttributes(("OwnerID = '" + m_initMapUnit + "'"));

            Dictionary<string, ExtendedAttributesAccess.ExtendedAttributes> deleteExtendedAttributesDictionary = new Dictionary<string, ExtendedAttributesAccess.ExtendedAttributes>();
            foreach (KeyValuePair<string, ExtendedAttributesAccess.ExtendedAttributes> anOldExtendedAttributesEntry in extAttrAccess.ExtendedAttributesDictionary)
            {
                /// Identify if the old record still exits in the new dictionary
                /// If not, delete this record
                if (!m_ExtendedAttributesDictionary.ContainsKey(anOldExtendedAttributesEntry.Key))
                { deleteExtendedAttributesDictionary.Add(anOldExtendedAttributesEntry.Key, anOldExtendedAttributesEntry.Value); }
            }

            foreach (KeyValuePair<string, ExtendedAttributesAccess.ExtendedAttributes> anDeleteExtendedAttributesEntry in deleteExtendedAttributesDictionary)
            {
                ExtendedAttributesAccess.ExtendedAttributes thisExtAttr = anDeleteExtendedAttributesEntry.Value;      
                extAttrAccess.DeleteExtendedAttributes(thisExtAttr);
            }

            extAttrAccess.ExtendedAttributesDictionary = m_ExtendedAttributesDictionary;

            extAttrAccess.SaveExtendedAttributes();
            /// <end> ---------------------------------------------------------------------------------------------------
            /// ---------------------------------------------------------------------------------------------------------

            initEmptyAgeEventTab();
        }

        private void btnCancelAge_Click(object sender, EventArgs e)
        {
            cancelAge();
            cancelLithology();
            cancelMapUnit();
        }

        private void cancelAge()
        {
            string mapUnit = txtMapUnitAbbreviation.Text;
            if (m_theWorkspace != null && mapUnit != null)
            {
                liEvts.Items.Clear();
                initAgeTab(mapUnit);
            }
        }

        private void deleteExtendedAtrributesRecord(string mapUnit)
        {
            if (m_theWorkspace == null)
            {
                MessageBox.Show("Please open a working space!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /// Connect with the Extended Attribute table
            ExtendedAttributesAccess extAttrAccess = new ExtendedAttributesAccess(m_theWorkspace);
            /// Search for the extended attributes record for this map unit
            extAttrAccess.AddExtendedAttributes("OwnerID = '" + mapUnit + "'");

            ExtendedAttributesAccess.ExtendedAttributes anExtendedAttributes = extAttrAccess.ExtendedAttributesDictionary.First().Value;
            extAttrAccess.DeleteExtendedAttributes(anExtendedAttributes);           
        }

        #region Change the focus of listboxes

        private void liEvts4ThisUnit_Click(object sender, EventArgs e)
        {
            if (!liEvts.Focused) 
            {
                isUpdate4AgeEvent = false;
                liEvts.SelectedIndex = -1;               
            }
        }

        private void liEvts_Click(object sender, EventArgs e)
        {
            if (!liEvts4ThisUnit.Focused) { liEvts4ThisUnit.SelectedIndex = -1; }
        }

        #endregion
        
    #endregion



    }
}

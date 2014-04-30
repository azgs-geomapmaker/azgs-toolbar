using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ncgmpToolbar.Utilities;
using ncgmpToolbar.Utilities.DataAccess;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;

namespace ncgmpToolbar
{
    /// <summary>
    /// ncgmpEditorExtension class implementing custom ESRI Editor Extension functionalities.
    /// </summary>
    public class ncgmpEditorExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        private IWorkspace m_EditWorkspace;
        private IEditor theEditor;
        private bool m_DatabaseIsValid = false;
        private bool m_DatabaseUsesRepresentation = false;
        private bool m_DatabaseHasStations = false;
        private sysInfo m_SysInfo;
        public static bool g_EditWorkspaceIsValid;

        private bool IsExtensionEnabled = true;

        public ncgmpEditorExtension()
        {
            WireEditorEvents();
        }

        protected override void OnStartup()
        {
            theEditor = ArcMap.Editor;
        }

        protected override void OnShutdown()
        {
        }

        #region Editor Events

        #region Shortcut properties to the various editor event interfaces
        private IEditEvents_Event Events
        {
            get { return ArcMap.Editor as IEditEvents_Event; }
        }
        private IEditEvents2_Event Events2
        {
            get { return ArcMap.Editor as IEditEvents2_Event; }
        }
        private IEditEvents3_Event Events3
        {
            get { return ArcMap.Editor as IEditEvents3_Event; }
        }
        private IEditEvents4_Event Events4
        {
            get { return ArcMap.Editor as IEditEvents4_Event; }
        }
        #endregion

        void WireEditorEvents()
        {
            // This function wires up events with the appropriate functions defined below
            Events.OnStartEditing += delegate() { OnStartEditing(); };
            Events.OnCreateFeature += delegate(ESRI.ArcGIS.Geodatabase.IObject obj) { OnCreateFeature(obj); };
            Events.OnChangeFeature += delegate(ESRI.ArcGIS.Geodatabase.IObject obj) { OnChangeFeature(obj); };
            Events.OnDeleteFeature += delegate(ESRI.ArcGIS.Geodatabase.IObject obj) { OnDeleteFeature(obj); };
            Events.OnStopEditing += delegate(bool save) { OnStopEditing(save); };
        }

        void OnStartEditing()
        {
            // Don't do anything if the extension is disabled
            if (IsExtensionEnabled != true) { return; }

            // Check that the workspace being edited is NCGMP-valid.
            m_EditWorkspace = theEditor.EditWorkspace;
            m_DatabaseIsValid = ncgmpChecks.IsWorkspaceMinNCGMPCompliant(m_EditWorkspace);

            //// Check that the SysInfo table is present.
            //if (m_DatabaseIsValid == true)
            //{
            //    m_DatabaseIsValid = ncgmpChecks.IsSysInfoPresent(m_EditWorkspace);
            //}

            // Open the SysInfo Table, Look for ProjectID values
            if (m_DatabaseIsValid == true)
            {
                m_SysInfo = new sysInfo(m_EditWorkspace);

                // If Project Name has not yet been specified, then open the form to set it.
                if (m_SysInfo.ProjName == null)
                {
                    Forms.newDatabaseInfo NewInfoForm = new Forms.newDatabaseInfo();
                    NewInfoForm.ShowDialog();
                    m_SysInfo.ProjName = NewInfoForm.projTitle;
                    m_SysInfo.ProjAbbr = NewInfoForm.projAbbr;
                }

                // Check other Database-validity things
                m_DatabaseUsesRepresentation = ncgmpChecks.AreRepresentationsUsed(m_EditWorkspace);
                //m_DatabaseHasStations = ncgmpChecks.IsAzgsStationAddinPresent(m_EditWorkspace);                               
            }

            // From http://resources.esri.com/help/9.3/ArcGISDesktop/ArcObjects/esrigeodatabase/IWorkspaceEditControl.htm
            //  "By default on simple classes, insert cursors handed out by the geodatabase will internally bypass the CreateRow
            //   and Store mechanisms when creating and updating objects.  Tools that use insert cursors include Planarize,
            //   Create Features and the Object Loader.  When CreateRow and Store are bypassed, subsequent events are not fired
            //   for clients listening to IEditEvents and IObjectClassEvents."
            //
            // I have to do this because the topology tool uses an Insert cursor, bypassing the OnCreate event!
            IWorkspaceEditControl wsEditControl = (IWorkspaceEditControl)m_EditWorkspace;
            wsEditControl.SetStoreEventsRequired();

            // Set the Global variable that notifies other classes that the database is valid NCGMP
            g_EditWorkspaceIsValid = true;
        }

        void OnCreateFeature(ESRI.ArcGIS.Geodatabase.IObject obj)
        {
            // Don't do anything if the extension is disabled
            if (IsExtensionEnabled != true) { return; }

            // Bail if this is not a valid NCGMP workspace
            if (m_DatabaseIsValid == false) { return; }

            #region "Groundwork"
            // Grab the FeatureClass name from the Row's Table (as an IDataset).
            IRow theRow = obj;
            ITable theTable = theRow.Table;
            IDataset theDS = (IDataset)theTable;
            string TableName = theDS.Name;

            // Parse the table name in order to strip out unneccessary bits of SDE tables
            ISQLSyntax nameParser = (ISQLSyntax)theDS.Workspace;
            string parsedDbName, parsedOwnerName, parsedTableName;
            nameParser.ParseTableName(TableName, out parsedDbName, out parsedOwnerName, out parsedTableName);
            #endregion

            #region "Set New ID"
            // Call the routine to get a new ID
            int id = m_SysInfo.GetNextIdValue(parsedTableName);

            // Set the new ID value on the row itself
            theRow.set_Value(theTable.FindField(parsedTableName + "_ID"), m_SysInfo.ProjAbbr + "." + parsedTableName + "." + id);
            #endregion

            #region "Calculate SymbolRotation"
            if (m_DatabaseUsesRepresentation == true)
            {
                if (parsedTableName == "OrientationPoints")
                {
                    // Get the Azimuth from the feature
                    int Azimuth;
                    bool result = int.TryParse(theRow.get_Value(theTable.FindField("Azimuth")).ToString(), out Azimuth);
                    // Calculate the stupid form of rotation...
                    int Rotation = CalculateSymbolRotation(Azimuth);
                    // Set the SymbolRotation Field
                    theRow.set_Value(theTable.FindField("SymbolRotation"), double.Parse(Rotation.ToString()));
                }
            }
            #endregion

            #region "Set DataSource"
            // Bail if the new object is in fact a Data Source
            if (parsedTableName != "DataSources")
            {
                if (globalVariables.currentDataSource == null)
                {
                    // I can warn the user that they should choose a data source, but I can't keep the feature from being created anyways
                    System.Windows.Forms.MessageBox.Show("You have not selected a valid Data Source for this edit session." + Environment.NewLine + "Your feature was created without a Data Source.", "NCGMP Tools");
                    return;
                }
                else
                {
                    // Set the DataSourceID value
                    if (parsedTableName == "Glossary") { theRow.set_Value(theTable.FindField("DefinitionSourceID"), globalVariables.currentDataSource); }
                    else if (parsedTableName == "DescriptionOfMapUnits") { theRow.set_Value(theTable.FindField("DescriptionSourceID"), globalVariables.currentDataSource); }
                    else { theRow.set_Value(theTable.FindField("DataSourceID"), globalVariables.currentDataSource); }
                }
            }
            #endregion
        }

        void OnChangeFeature(ESRI.ArcGIS.Geodatabase.IObject obj)
        {
            // Don't do anything if the extension is disabled
            if (IsExtensionEnabled != true) { return; }

            // Bail if this is not a valid NCGMP workspace
            if (m_DatabaseIsValid == false) { return; }

            #region "Groundwork"
            // Grab the FeatureClass name from the Row's Table (as an IDataset).
            IRow theRow = obj;
            ITable theTable = theRow.Table;
            IDataset theDS = (IDataset)theTable;
            string TableName = theDS.Name;

            // Parse the table name in order to strip out unneccessary bits of SDE tables
            ISQLSyntax nameParser = (ISQLSyntax)theDS.Workspace;
            string parsedDbName, parsedOwnerName, parsedTableName;
            nameParser.ParseTableName(TableName, out parsedDbName, out parsedOwnerName, out parsedTableName);
            #endregion

            #region "Calculate SymbolRotation"
            if (m_DatabaseUsesRepresentation == true)
            {
                if (parsedTableName == "OrientationPoints")
                {
                    // Get the Azimuth from the feature - this is ugly -- why is this m_identifier a double?
                    int Azimuth = (int)Math.Round((double)theRow.get_Value(theTable.FindField("Azimuth")), 0);
                    // Calculate the stupid form of rotation...
                    int Rotation = CalculateSymbolRotation(Azimuth);
                    // Set the SymbolRotation Field
                    theRow.set_Value(theTable.FindField("SymbolRotation"), double.Parse(Rotation.ToString()));
                }
            }
            #endregion

            // Debugging flag to turn off repositioning of related data when stations are edited:
            bool adjustLocations = false;

            if (adjustLocations == true)
            {
                #region "Adjust Samples/OrientationPoints to Match Stations"
                if (parsedTableName == "Stations")
                {
                    // Cast the obj as a Feature in order to access Geometry information
                    IFeature theStation = (IFeature)obj;
                    IGeometry stationGeom = theStation.ShapeCopy;

                    // Find related Samples
                    IRelationshipClass stationSampleLink = commonFunctions.OpenRelationshipClass(m_EditWorkspace, "StationSampleLink");
                    ESRI.ArcGIS.esriSystem.ISet relatedSamples = stationSampleLink.GetObjectsRelatedToObject(obj);

                    // Loop through the related Samples and set their Geometry to that of the Station
                    relatedSamples.Reset();
                    IFeature aSample = (IFeature)relatedSamples.Next();
                    while (aSample != null)
                    {
                        aSample.Shape = stationGeom;
                        aSample.Store();
                        aSample = (IFeature)relatedSamples.Next();
                    }

                    // Find related OrientationPoints
                    IRelationshipClass stationStructureLink = commonFunctions.OpenRelationshipClass(m_EditWorkspace, "StationOrientationPointsLink");
                    ESRI.ArcGIS.esriSystem.ISet relatedStructures = stationStructureLink.GetObjectsRelatedToObject(obj);

                    // Loop through the related OrientationPoints and set their Geometry to that of the Station
                    relatedStructures.Reset();
                    IFeature aStructure = (IFeature)relatedStructures.Next();
                    while (aStructure != null)
                    {
                        aStructure.Shape = stationGeom;
                        aStructure.Store();
                        aStructure = (IFeature)relatedStructures.Next();
                    }
                }
                #endregion
            }
        }

        void OnDeleteFeature(ESRI.ArcGIS.Geodatabase.IObject obj)
        {
            // Don't do anything if the extension is disabled
            if (IsExtensionEnabled != true) { return; }

            // Bail if this is not a valid NCGMP workspace
            if (m_DatabaseIsValid == false) { return; }

            #region "Groundwork"
            // Grab the FeatureClass name from the Row's Table (as an IDataset).
            IRow theRow = obj;
            ITable theTable = theRow.Table;
            IDataset theDS = (IDataset)theTable;
            string TableName = theDS.Name;

            // Parse the table name in order to strip out unneccessary bits of SDE tables
            ISQLSyntax nameParser = (ISQLSyntax)theDS.Workspace;
            string parsedDbName, parsedOwnerName, parsedTableName;
            nameParser.ParseTableName(TableName, out parsedDbName, out parsedOwnerName, out parsedTableName);
            #endregion

        //    #region "Delete Related Station Data"
        //    if (parsedTableName == "Stations")
        //    {
        //        // Get the related information first, then prompt, then delete if that's what they want.
        //        string stationName = (string)theRow.get_Value(theTable.FindField("FieldID"));
        //        IRelationshipClass stationStructureLink = commonFunctions.OpenRelationshipClass(m_EditWorkspace, "StationOrientationPointsLink");
        //        IRelationshipClass stationSamplesLink = commonFunctions.OpenRelationshipClass(m_EditWorkspace, "StationSampleLink");
        //        IRelationshipClass stationNotesLink = commonFunctions.OpenRelationshipClass(m_EditWorkspace, "StationNotesLink");
        //        IRelationshipClass stationDocsLink = commonFunctions.OpenRelationshipClass(m_EditWorkspace, "StationDocumentLink");

        //        ESRI.ArcGIS.esriSystem.ISet structureSet = stationStructureLink.GetObjectsRelatedToObject(obj);
        //        ESRI.ArcGIS.esriSystem.ISet sampleSet = stationSamplesLink.GetObjectsRelatedToObject(obj);
        //        ESRI.ArcGIS.esriSystem.ISet noteSet = stationNotesLink.GetObjectsRelatedToObject(obj);
        //        ESRI.ArcGIS.esriSystem.ISet docSet = stationDocsLink.GetObjectsRelatedToObject(obj);

        //        string theMessage = ("Deleting Station " + stationName + " will also delete the following:");
        //        theMessage += Environment.NewLine;
        //        theMessage += structureSet.Count.ToString() + " structural observations";
        //        theMessage += Environment.NewLine;
        //        theMessage += sampleSet.Count.ToString() + " sample locations";
        //        theMessage += Environment.NewLine;
        //        theMessage += noteSet.Count.ToString() + " recorded notes";
        //        theMessage += Environment.NewLine;
        //        theMessage += docSet.Count.ToString() + " related document links";
        //        theMessage += Environment.NewLine;
        //        theMessage += Environment.NewLine;
        //        theMessage += "Are you sure you want to do this?";

        //        // Probably would be wise to warn them first...
        //        System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(theMessage, "NCGMP Tools", System.Windows.Forms.MessageBoxButtons.YesNo);
        //        if (result == System.Windows.Forms.DialogResult.Yes)
        //        {
        //            IFeature theFeature = (IFeature)structureSet.Next();
        //            while (theFeature != null)
        //            {
        //                theFeature.Delete();
        //                theFeature = (IFeature)structureSet.Next();
        //            }
        //            theFeature = (IFeature)sampleSet.Next();
        //            while (theFeature != null)
        //            {
        //                theFeature.Delete();
        //                theFeature = (IFeature)sampleSet.Next();
        //            }
        //            theFeature = (IFeature)noteSet.Next();
        //            while (theFeature != null)
        //            {
        //                theFeature.Delete();
        //                theFeature = (IFeature)noteSet.Next();
        //            }
        //            theFeature = (IFeature)docSet.Next();
        //            while (theFeature != null)
        //            {
        //                theFeature.Delete();
        //                theFeature = (IFeature)docSet.Next();
        //            }
        //        }
        //    }
        //    #endregion
        }

        void OnStopEditing(bool save)
        {
            // Don't do anything if the extension is disabled
            if (IsExtensionEnabled != true) { return; }

            // Reset the database-validity flag just in case
            m_DatabaseIsValid = false;
            m_DatabaseUsesRepresentation = false;

            // Destroy the SysInfo reference
            m_SysInfo = null;

            // Kill the NCGMP-validity flag
            g_EditWorkspaceIsValid = false;

            // Close the Map Unit Legend Form
            // Mixing UI into the Extension! Balderdash! I should try and make the UserControl listen to edit events...
            UID theUid = new UIDClass();
            theUid.Value = ThisAddIn.IDs.dwnMapUnitLegendEditor;

            IDockableWindow mapUnitForm = ArcMap.DockableWindowManager.GetDockableWindow(theUid);
            if (mapUnitForm.IsVisible() == true) { mapUnitForm.Show(false); }
            
        }
       
        #endregion

        private int CalculateSymbolRotation(int Azimuth)
        {
            // This is due to the fact that Representations use a LUDICROUS scheme for calculating rotation angles
            return 360 - Azimuth;
        }

        internal bool Enabled { 
            get
            {
                return this.State == ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled;
            }
            set
            {
                if (value == true) { this.State = ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Enabled; }
                else { this.State = ESRI.ArcGIS.Desktop.AddIns.ExtensionState.Disabled; }
            }
        }
    }

}

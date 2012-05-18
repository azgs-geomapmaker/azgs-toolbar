using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geometry;
using ncgmpToolbar.Utilities;
using ncgmpToolbar.Utilities.DataAccess;

namespace ncgmpToolbar.Forms
{
    public partial class updateAttributes : Form
    {
        private IWorkspace m_theWorkspace;
        private bool m_canceled = false;
        public bool Canceled
        {
            get { return m_canceled; }
        }

        public updateAttributes(IWorkspace theWorkspace)
        {
            m_theWorkspace = theWorkspace;
            InitializeComponent();

            chkFeatureTemplate.Enabled = true;
            chkRecalculateIDs.Enabled = true;
        }

        private void chkDataSources_CheckedChanged(object sender, EventArgs e)
        {
            EnableContinueButton();
        }

        private void chkFeatureTemplate_CheckedChanged(object sender, EventArgs e)
        {
            EnableContinueButton();
        }

        private void chkRecalculateIDs_CheckedChanged(object sender, EventArgs e)
        {
            EnableContinueButton();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            m_canceled = true;
            this.Hide();
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> dataAccessClasses = BuildDataAccessFromSelection();

            if (chkDataSources.Checked == true) { UpdateDataSources(dataAccessClasses); }
            if (chkFeatureTemplate.Checked == true) { ApplyFeatureTemplates(); }
            if (chkRecalculateIDs.Checked == true) { RecalculateIdentifiers(dataAccessClasses); }

            // Refresh the ActiveView, close the form
            ArcMap.Document.ActiveView.Refresh();
            this.Close();
        }

        private void EnableContinueButton()
        {
            switch (chkDataSources.Checked)
            {
                case true: btnContinue.Enabled = true; break;

                case false:
                    switch (chkFeatureTemplate.Checked)
                    {
                        case true: btnContinue.Enabled = true; break;

                        case false:
                            switch (chkRecalculateIDs.Checked)
                            {
                                case true: btnContinue.Enabled = true; break;

                                case false: btnContinue.Enabled = false; break;
                            }
                            break;
                    }
                    break;
            }
        }

        private Dictionary<string, object> BuildDataAccessFromSelection()
        {
            // Will be building SQL Where Clauses for each possible selected thing
            string MapUnitPolysSearch = "MapUnitPolys_ID = '";
            string OverlayPolysSearch = "OverlayPolys_ID = '";
            string ContactsAndFaultsSearch = "ContactsAndFaults_ID = '";
            string OtherLinesSearch = "OtherLines_ID = '";
            string StationPointsSearch = "StationPoints_ID = '";
            string SamplePointsSearch = "SamplePoints_ID = '";
            string OrientationDataPointsSearch = "OrientationDataPoints_ID = '";
            string GlossarySearch = "Glossary_ID = '";
            string NotesSearch = "Notes_ID = '";
            string RelatedDocumentsSearch = "RelatedDocuments_ID = '";
            string DataSourcesSearch = "DataSources_ID = '";
            string DescriptionOfMapUnitsSearch = "DescriptionOfMapUnits_ID = '";


            // This object will be populated and returned
            Dictionary<string, object> dataAccessClasses = new Dictionary<string, object>();

            #region Selected FEATURES
            // Get an enumeration of the selected features
            IEnumFeature selectionEnum = ArcMap.Document.FocusMap.FeatureSelection as IEnumFeature;

            // Loop through the features to build queries to find the features
            IRow thisFeature = selectionEnum.Next() as IRow;
            
            while (thisFeature != null)
            {
                // Get the Table Name
                string tableName = (thisFeature.Table as IDataset).Name;

                // Parse the table name in order to strip out unneccessary bits of SDE tables
                ISQLSyntax nameParser = (ISQLSyntax)m_theWorkspace;
                string parsedDbName, parsedOwnerName, parsedTableName;
                nameParser.ParseTableName(tableName, out parsedDbName, out parsedOwnerName, out parsedTableName);
                

                // Build the SQL Where Clause depending on the table...
                switch (parsedTableName)
                {
                    case "MapUnitPolys":
                        MapUnitPolysSearch += thisFeature.get_Value(thisFeature.Table.FindField("MapUnitPolys_ID")) + "' OR MapUnitPolys_ID = '";
                        break;
                    case "OverlayPolys":
                        OverlayPolysSearch += thisFeature.get_Value(thisFeature.Table.FindField("OverlayPolys_ID")) + "' OR OverlayPolys_ID = '";
                        break;
                    case "ContactsAndFaults":
                        ContactsAndFaultsSearch += thisFeature.get_Value(thisFeature.Table.FindField("ContactsAndFaults_ID")) + "' OR ContactsAndFaults_ID = '";
                        break;
                    case "OtherLines":
                        OtherLinesSearch += thisFeature.get_Value(thisFeature.Table.FindField("OtherLines_ID")) + "' OR OtherLines_ID = '";
                        break;
                    case "StationPoints":
                        StationPointsSearch += thisFeature.get_Value(thisFeature.Table.FindField("StationPoints_ID")) + "' OR StationPoints_ID = '";
                        break;
                    case "SamplePoints":
                        SamplePointsSearch += thisFeature.get_Value(thisFeature.Table.FindField("SamplePoints_ID")) + "' OR SamplePoints_ID = '";
                        break;
                    case "OrientationDataPoints":
                        OrientationDataPointsSearch += thisFeature.get_Value(thisFeature.Table.FindField("OrientationDataPoints_ID")) + "' OR OrientationDataPoints_ID = '";
                        break;                    
                }

                // Iterate the enumeration
                thisFeature = selectionEnum.Next();
            }

            #region "Build Dictionary"
            // Clean up the Where Clauses, create the data access classes, and then add it to the dictionary            

            // MapUnitPolys
            if (MapUnitPolysSearch != "MapUnitPolys_ID = '")
            {
                MapUnitPolysSearch = MapUnitPolysSearch.Remove(MapUnitPolysSearch.Length - 23);
                MapUnitPolysAccess MapUnitPolysRecords = new MapUnitPolysAccess(m_theWorkspace);
                MapUnitPolysRecords.AddMapUnitPolys(MapUnitPolysSearch);
                dataAccessClasses.Add("MapUnitPolys", MapUnitPolysRecords);
            }

            // OverlayPolys
            if (OverlayPolysSearch != "OverlayPolys_ID = '")
            {
                OverlayPolysSearch = OverlayPolysSearch.Remove(OverlayPolysSearch.Length - 23);
                OverlayPolysAccess OverlayPolysRecords = new OverlayPolysAccess(m_theWorkspace);
                OverlayPolysRecords.AddOverlayPolys(OverlayPolysSearch);
                dataAccessClasses.Add("OverlayPolys", OverlayPolysRecords);
            }

            // ContactsAndFaults
            if (ContactsAndFaultsSearch != "ContactsAndFaults_ID = '")
            {
                ContactsAndFaultsSearch = ContactsAndFaultsSearch.Remove(ContactsAndFaultsSearch.Length - 28);
                ContactsAndFaultsAccess ContactsAndFaultsRecords = new ContactsAndFaultsAccess(m_theWorkspace);
                ContactsAndFaultsRecords.AddContactsAndFaults(ContactsAndFaultsSearch);
                dataAccessClasses.Add("ContactsAndFaults", ContactsAndFaultsRecords);
            }

            // OtherLines
            if (OtherLinesSearch != "OtherLines_ID = '")
            {
                OtherLinesSearch = OtherLinesSearch.Remove(OtherLinesSearch.Length - 21);
                OtherLinesAccess OtherLinesRecords = new OtherLinesAccess(m_theWorkspace);
                OtherLinesRecords.AddOtherLines(OtherLinesSearch);
                dataAccessClasses.Add("OtherLines", OtherLinesRecords);
            }

            // StationPoints
            if (StationPointsSearch != "StationPoints_ID = '")
            {
                StationPointsSearch = StationPointsSearch.Remove(StationPointsSearch.Length - 24);
                StationPointsAccess StationPointsRecords = new StationPointsAccess(m_theWorkspace);
                StationPointsRecords.AddStationPoints(StationPointsSearch);
                dataAccessClasses.Add("StationPoints", StationPointsRecords);
            }

            // SamplePoints
            if (SamplePointsSearch != "SamplePoints_ID = '")
            {
                SamplePointsSearch = SamplePointsSearch.Remove(SamplePointsSearch.Length - 23);
                SamplePointsAccess SamplePointsRecords = new SamplePointsAccess(m_theWorkspace);
                SamplePointsRecords.AddSamplePoints(SamplePointsSearch);
                dataAccessClasses.Add("SamplePoints", SamplePointsRecords);
            }

            // OrientationDataPoints
            if (OrientationDataPointsSearch != "OrientationDataPoints_ID = '")
            {
                OrientationDataPointsSearch = OrientationDataPointsSearch.Remove(OrientationDataPointsSearch.Length - 32);
                OrientationDataPointsAccess OrientationDataPointsRecords = new OrientationDataPointsAccess(m_theWorkspace);
                OrientationDataPointsRecords.AddOrientationDataPoints(OrientationDataPointsSearch);
                dataAccessClasses.Add("OrientationDataPoints", OrientationDataPointsRecords);
            }

            #endregion

            #endregion

            #region Selected TABLE ROWS
            // Loop through the tables in the map
            IStandaloneTableCollection tableCollection = ArcMap.Document.FocusMap as IStandaloneTableCollection;
            for (int i = 0; i <= tableCollection.StandaloneTableCount - 1; i++)
            {
                // Get one of the tables
                IStandaloneTable thisTable = tableCollection.StandaloneTable[i];
                string tableName = (thisTable.Table as IDataset).Name;

                // Parse the table name in order to strip out unneccessary bits of SDE tables
                ISQLSyntax nameParser = (ISQLSyntax)m_theWorkspace;
                string parsedDbName, parsedOwnerName, parsedTableName;
                nameParser.ParseTableName(tableName, out parsedDbName, out parsedOwnerName, out parsedTableName);

                // Find the selection
                ITableSelection selectedRows = thisTable as ITableSelection;
                ISelectionSet theSelection = selectedRows.SelectionSet;
                
                // Iterate if there are no selected rows
                if (theSelection.Count == 0) { continue; }

                // Loop through selected rows, build the where clauses up.
                ICursor theCursor;
                theSelection.Search(null, false, out theCursor);

                IRow theRow = theCursor.NextRow();
                while (theRow != null)
                {
                    switch (parsedTableName)
                    {
                        case "Glossary":
                            GlossarySearch += theRow.get_Value(thisTable.Table.FindField("Glossary_ID")) + "' OR Glossary_ID = '";
                            break;
                        case "Notes":
                            NotesSearch += theRow.get_Value(thisTable.Table.FindField("Notes_ID")) + "' OR Notes_ID = '";
                            break;
                        case "RelatedDocuments":
                            RelatedDocumentsSearch += theRow.get_Value(thisTable.Table.FindField("RelatedDocuments_ID")) + "' OR RelatedDocuments_ID = '";
                            break;
                        case "DataSources":
                            DataSourcesSearch += theRow.get_Value(thisTable.Table.FindField("DataSources_ID")) + "' OR DataSources_ID = '";
                            break;
                        case "DescriptionOfMapUnits":
                            DescriptionOfMapUnitsSearch += theRow.get_Value(thisTable.Table.FindField("DescriptionOfMapUnits_ID")) + "' OR DescriptionOfMapUnits_ID = '";
                            break;
                    }

                    // Iterate
                    theRow = theCursor.NextRow();
                }
            }

            #region Build Dictionary
            // Clean up the Where Clauses, create the data access classes, and then add it to the dictionary            

            // Glossary
            if (GlossarySearch != "Glossary_ID = '")
            {
                GlossarySearch = GlossarySearch.Remove(GlossarySearch.Length - 19);
                GlossaryAccess GlossaryRecords = new GlossaryAccess(m_theWorkspace);
                GlossaryRecords.AddGlossary(GlossarySearch);
                dataAccessClasses.Add("Glossary", GlossaryRecords);
            }

            // Notes
            if (NotesSearch != "Notes_ID = '")
            {
                NotesSearch = NotesSearch.Remove(NotesSearch.Length - 16);
                NotesAccess NotesRecords = new NotesAccess(m_theWorkspace);
                NotesRecords.AddNotes(NotesSearch);
                dataAccessClasses.Add("Notes", NotesRecords);
            }

            // RelatedDocuments
            if (RelatedDocumentsSearch != "RelatedDocuments_ID = '")
            {
                RelatedDocumentsSearch = RelatedDocumentsSearch.Remove(RelatedDocumentsSearch.Length - 27);
                RelatedDocumentsAccess RelatedDocumentsRecords = new RelatedDocumentsAccess(m_theWorkspace);
                RelatedDocumentsRecords.AddRelatedDocuments(RelatedDocumentsSearch);
                dataAccessClasses.Add("RelatedDocuments", RelatedDocumentsRecords);
            }

            // DataSources
            if (DataSourcesSearch != "DataSources_ID = '")
            {
                DataSourcesSearch = DataSourcesSearch.Remove(DataSourcesSearch.Length - 22);
                DataSourcesAccess DataSourcesRecords = new DataSourcesAccess(m_theWorkspace);
                DataSourcesRecords.AddDataSources(DataSourcesSearch);
                dataAccessClasses.Add("DataSources", DataSourcesRecords);
            }

            // DescriptionOfMapUnits
            if (DescriptionOfMapUnitsSearch != "DescriptionOfMapUnits_ID = '")
            {
                DescriptionOfMapUnitsSearch = DescriptionOfMapUnitsSearch.Remove(DescriptionOfMapUnitsSearch.Length - 32);
                DescriptionOfMapUnitsAccess DescriptionOfMapUnitsRecords = new DescriptionOfMapUnitsAccess(m_theWorkspace);
                DescriptionOfMapUnitsRecords.AddDescriptionOfMapUnits(DescriptionOfMapUnitsSearch);
                dataAccessClasses.Add("DescriptionOfMapUnits", DescriptionOfMapUnitsRecords);
            }

            #endregion

            #endregion

            // Okay! Return the dictionary that has been built
            return dataAccessClasses;

        }

        private void UpdateDataSources(Dictionary<string, object> dataAccessClasses)
        {
            // Get the Data Source to set
            string dataSourceID = commonFunctions.GetCurrentDataSourceID();

            // Bail if there isn't one
            if (dataSourceID == null) { return; }

            // Get DataAccess Classes to perform updates
            MapUnitPolysAccess mapUnitPolysAccess = new MapUnitPolysAccess(m_theWorkspace);
            OverlayPolysAccess OverlayPolysAccess = new OverlayPolysAccess(m_theWorkspace);
            ContactsAndFaultsAccess ContactsAndFaultsAccess = new ContactsAndFaultsAccess(m_theWorkspace);
            OtherLinesAccess OtherLinesAccess = new OtherLinesAccess(m_theWorkspace);
            StationPointsAccess StationPointsAccess = new StationPointsAccess(m_theWorkspace);
            SamplePointsAccess SamplePointsAccess = new SamplePointsAccess(m_theWorkspace);
            OrientationDataPointsAccess OrientationDataPointsAccess = new OrientationDataPointsAccess(m_theWorkspace);
            GlossaryAccess GlossaryAccess = new GlossaryAccess(m_theWorkspace);
            NotesAccess NotesAccess = new NotesAccess(m_theWorkspace);
            RelatedDocumentsAccess RelatedDocumentsAccess = new RelatedDocumentsAccess(m_theWorkspace);
            DescriptionOfMapUnitsAccess DescriptionOfMapUnitsAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);

            // Loop through the dictionary
            foreach (KeyValuePair<string, object> anEntry in dataAccessClasses)
            {
                // What table does this come from?
                switch (anEntry.Key)
                {
                    case "MapUnitPolys":
                        // Loop through the records in the data access object that comes in
                        MapUnitPolysAccess thisMapUnitPolysAccess = (MapUnitPolysAccess)anEntry.Value;
                        foreach (KeyValuePair<string, MapUnitPolysAccess.MapUnitPoly> MapUnitPolysToUpdate in thisMapUnitPolysAccess.MapUnitPolysDictionary)
                        {
                            MapUnitPolysAccess.MapUnitPoly thisMapUnitPoly = (MapUnitPolysAccess.MapUnitPoly)MapUnitPolysToUpdate.Value;
                            thisMapUnitPoly.DataSourceID = dataSourceID;
                            mapUnitPolysAccess.UpdateMapUnitPoly(thisMapUnitPoly);
                        }
                        mapUnitPolysAccess.SaveMapUnitPolys();
                        break;
                    case "OverlayPolys":
                        OverlayPolysAccess thisOverlayPolysAccess = (OverlayPolysAccess)anEntry.Value;
                        foreach (KeyValuePair<string, OverlayPolysAccess.OverlayPoly> OverlayPolysToUpdate in thisOverlayPolysAccess.OverlayPolysDictionary)
                        {
                            OverlayPolysAccess.OverlayPoly thisOverlayPoly = (OverlayPolysAccess.OverlayPoly)OverlayPolysToUpdate.Value;
                            thisOverlayPoly.DataSourceID = dataSourceID;
                            OverlayPolysAccess.UpdateOverlayPoly(thisOverlayPoly);
                        }
                        OverlayPolysAccess.SaveOverlayPolys();
                        break;
                    case "ContactsAndFaults":
                        ContactsAndFaultsAccess thisContactsAndFaultsAccess = (ContactsAndFaultsAccess)anEntry.Value;
                        foreach (KeyValuePair<string, ContactsAndFaultsAccess.ContactsAndFault> ContactsAndFaultsToUpdate in thisContactsAndFaultsAccess.ContactsAndFaultsDictionary)
                        {
                            ContactsAndFaultsAccess.ContactsAndFault thisContactsAndFault = (ContactsAndFaultsAccess.ContactsAndFault)ContactsAndFaultsToUpdate.Value;
                            thisContactsAndFault.DataSourceID = dataSourceID;
                            ContactsAndFaultsAccess.UpdateContactsAndFault(thisContactsAndFault);
                        }
                        ContactsAndFaultsAccess.SaveContactsAndFaults();
                        break;
                    case "OtherLines":
                        OtherLinesAccess thisOtherLinesAccess = (OtherLinesAccess)anEntry.Value;
                        foreach (KeyValuePair<string, OtherLinesAccess.OtherLine> OtherLinesToUpdate in thisOtherLinesAccess.OtherLinesDictionary)
                        {
                            OtherLinesAccess.OtherLine thisOtherLine = (OtherLinesAccess.OtherLine)OtherLinesToUpdate.Value;
                            thisOtherLine.DataSourceID = dataSourceID;
                            OtherLinesAccess.UpdateOtherLine(thisOtherLine);
                        }
                        OtherLinesAccess.SaveOtherLines();
                        break;
                    case "StationPoints":
                        StationPointsAccess thisStationPointsAccess = (StationPointsAccess)anEntry.Value;
                        foreach (KeyValuePair<string, StationPointsAccess.StationPoint> StationPointsToUpdate in thisStationPointsAccess.StationPointsDictionary)
                        {
                            StationPointsAccess.StationPoint thisStationPoint = (StationPointsAccess.StationPoint)StationPointsToUpdate.Value;
                            thisStationPoint.DataSourceID = dataSourceID;
                            StationPointsAccess.UpdateStationPoint(thisStationPoint);
                        }
                        StationPointsAccess.SaveStationPoints();
                        break;
                    case "SamplePoints":
                        SamplePointsAccess thisSamplePointsAccess = (SamplePointsAccess)anEntry.Value;
                        foreach (KeyValuePair<string, SamplePointsAccess.SamplePoint> SamplePointsToUpdate in thisSamplePointsAccess.SamplePointsDictionary)
                        {
                            SamplePointsAccess.SamplePoint thisSamplePoint = (SamplePointsAccess.SamplePoint)SamplePointsToUpdate.Value;
                            thisSamplePoint.DataSourceID = dataSourceID;
                            SamplePointsAccess.UpdateSamplePoint(thisSamplePoint);
                        }
                        SamplePointsAccess.SaveSamplePoints();
                        break;
                    case "OrientationDataPoints":
                        OrientationDataPointsAccess thisOrientationDataPointsAccess = (OrientationDataPointsAccess)anEntry.Value;
                        foreach (KeyValuePair<string, OrientationDataPointsAccess.OrientationDataPoint> OrientationDataPointsToUpdate in thisOrientationDataPointsAccess.OrientationDataPointsDictionary)
                        {
                            OrientationDataPointsAccess.OrientationDataPoint thisOrientationDataPoint = (OrientationDataPointsAccess.OrientationDataPoint)OrientationDataPointsToUpdate.Value;
                            thisOrientationDataPoint.DataSourceID = dataSourceID;
                            OrientationDataPointsAccess.UpdateOrientationDataPoint(thisOrientationDataPoint);
                        }
                        OrientationDataPointsAccess.SaveOrientationDataPoints();
                        break;
                    case "Glossary":
                        GlossaryAccess thisGlossaryAccess = (GlossaryAccess)anEntry.Value;
                        foreach (KeyValuePair<string, GlossaryAccess.Glossary> GlossaryToUpdate in thisGlossaryAccess.GlossaryDictionary)
                        {
                            GlossaryAccess.Glossary thisGlossary = (GlossaryAccess.Glossary)GlossaryToUpdate.Value;
                            thisGlossary.DefinitionSourceID = dataSourceID;
                            GlossaryAccess.UpdateGlossary(thisGlossary);
                        }
                        GlossaryAccess.SaveGlossary();
                        break;
                    case "Notes":
                        NotesAccess thisNotesAccess = (NotesAccess)anEntry.Value;
                        foreach (KeyValuePair<string, NotesAccess.Note> NotesToUpdate in thisNotesAccess.NotesDictionary)
                        {
                            NotesAccess.Note thisNote = (NotesAccess.Note)NotesToUpdate.Value;
                            thisNote.DataSourceID = dataSourceID;
                            NotesAccess.UpdateNote(thisNote);
                        }
                        NotesAccess.SaveNotes();
                        break;
                    case "RelatedDocuments":
                        RelatedDocumentsAccess thisRelatedDocumentsAccess = (RelatedDocumentsAccess)anEntry.Value;
                        foreach (KeyValuePair<string, RelatedDocumentsAccess.RelatedDocument> RelatedDocumentsToUpdate in thisRelatedDocumentsAccess.RelatedDocumentsDictionary)
                        {
                            RelatedDocumentsAccess.RelatedDocument thisRelatedDocument = (RelatedDocumentsAccess.RelatedDocument)RelatedDocumentsToUpdate.Value;
                            thisRelatedDocument.DataSourceID = dataSourceID;
                            RelatedDocumentsAccess.UpdateRelatedDocument(thisRelatedDocument);
                        }
                        RelatedDocumentsAccess.SaveRelatedDocuments();
                        break;
                    case "DescriptionOfMapUnits":
                        DescriptionOfMapUnitsAccess thisDescriptionOfMapUnitsAccess = (DescriptionOfMapUnitsAccess)anEntry.Value;
                        foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> DescriptionOfMapUnitsToUpdate in thisDescriptionOfMapUnitsAccess.DescriptionOfMapUnitsDictionary)
                        {
                            DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDescriptionOfMapUnit = (DescriptionOfMapUnitsAccess.DescriptionOfMapUnit)DescriptionOfMapUnitsToUpdate.Value;
                            thisDescriptionOfMapUnit.DescriptionSourceID = dataSourceID;
                            DescriptionOfMapUnitsAccess.UpdateDescriptionOfMapUnit(thisDescriptionOfMapUnit);
                        }
                        DescriptionOfMapUnitsAccess.SaveDescriptionOfMapUnits();
                        break;   
                }
            }            
        }

        private void ApplyFeatureTemplates()
        {
            // Get a reference to the selected FeatureTemplate
            IEditor3 templateEditor = ArcMap.Editor as IEditor3;
            IEditTemplate theCurrentTemplate = templateEditor.CurrentTemplate;

            // If they didn't select a template, they'll need to
            if (theCurrentTemplate == null) { MessageBox.Show("No FeatureTemplate was selected.", "NCGMP Tools"); return; }

            // Find out what FeatureClass the selected template belongs to, and what its geometry is
            IFeatureClass templateFC = ((IFeatureLayer)theCurrentTemplate.Layer).FeatureClass;
            switch (templateFC.ShapeType)
            {
                case esriGeometryType.esriGeometryPolyline:
                    // Look for feature selected in line FeatureClasses
                    #region ContactsAndFaults
                    IFeatureLayer contactsAndFaultsFL = commonFunctions.FindFeatureLayer(m_theWorkspace, "ContactsAndFaults");
                    if (contactsAndFaultsFL != null)
                    {
                        // Found ContactsAndFaults. Check for a selection
                        IFeatureSelection selectedFeatures = contactsAndFaultsFL as IFeatureSelection;
                        if (selectedFeatures.SelectionSet.Count > 0)
                        {
                            // ContactsAndFaults were selected. Pass the selection into a cursor.
                            ISelectionSet featureSelectionSet = selectedFeatures.SelectionSet;
                            ICursor theCursor;
                            featureSelectionSet.Search(null, false, out theCursor);
                            IFeatureCursor theFeatureCursor = (IFeatureCursor)theCursor;

                            // Some variables to declare before the loops
                            int idFld = theFeatureCursor.FindField("ContactsAndFaults_ID");
                            IFeature theFeature;
                            string thisId;

                            // Construct a dummy feature to get attributes from
                            IFeature dummyFeature = templateFC.CreateFeature();
                            theCurrentTemplate.SetDefaultValues(dummyFeature);
                            string ExistenceConfidence = dummyFeature.get_Value(templateFC.FindField("ExistenceConfidence")).ToString();
                            string IdentityConfidence = dummyFeature.get_Value(templateFC.FindField("IdentityConfidence")).ToString();
                            string Label = dummyFeature.get_Value(templateFC.FindField("Label")).ToString();
                            double LocationConfidenceMeters;
                            bool result = double.TryParse(dummyFeature.get_Value(templateFC.FindField("LocationConfidenceMeters")).ToString(), out LocationConfidenceMeters);
                            string Notes = dummyFeature.get_Value(templateFC.FindField("Notes")).ToString();
                            string Symbol = dummyFeature.get_Value(templateFC.FindField("Symbol")).ToString();
                            string Type = dummyFeature.get_Value(templateFC.FindField("Type")).ToString();
                            int IsConcealed = 0;
                            if (contactsAndFaultsFL.FeatureClass.Equals(templateFC) == true)
                            {
                                result = int.TryParse(dummyFeature.get_Value(templateFC.FindField("IsConcealed")).ToString(), out IsConcealed);
                            }

                            // Destroy the dummy feature - must happen within an edit operation
                            ArcMap.Editor.StartOperation();
                            dummyFeature.Delete();
                            ArcMap.Editor.StopOperation("Remove Dummy Feature");

                            //Is the FeatureTemplate also for ContactsAndFaults?
                            switch (contactsAndFaultsFL.FeatureClass.Equals(templateFC))
                            {
                                case true:
                                    // Update the features to the FeatureTemplate
                                    ContactsAndFaultsAccess ContactsAndFaultsUpdater = new ContactsAndFaultsAccess(m_theWorkspace);                                    

                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the ContactsAndFaults feature to update
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        ContactsAndFaultsUpdater.AddContactsAndFaults("ContactsAndFaults_ID = '" + thisId + "'");
                                        ContactsAndFaultsAccess.ContactsAndFault thisContactsAndFault = ContactsAndFaultsUpdater.ContactsAndFaultsDictionary[thisId];

                                        // Assign values from the FeatureTemplate
                                        thisContactsAndFault.ExistenceConfidence = ExistenceConfidence;
                                        thisContactsAndFault.IdentityConfidence = IdentityConfidence;
                                        thisContactsAndFault.Label = Label;
                                        thisContactsAndFault.LocationConfidenceMeters = LocationConfidenceMeters;
                                        thisContactsAndFault.Notes = Notes;
                                        thisContactsAndFault.Symbol = Symbol;
                                        thisContactsAndFault.Type = Type;
                                        thisContactsAndFault.IsConcealed = IsConcealed;
                                        

                                        // Update the feature
                                        ContactsAndFaultsUpdater.UpdateContactsAndFault(thisContactsAndFault);

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }

                                    // Save changes
                                    ContactsAndFaultsUpdater.SaveContactsAndFaults();
                                    break;
                                case false:
                                    // Copy the features into the templateFC using the selected template as a guide

                                    // Build DataAccess Classes for target FeatureClasses
                                    OtherLinesAccess OtherLinesInserter = new OtherLinesAccess(m_theWorkspace);

                                    // Find the target FeatureClass Name
                                    string tableName = (templateFC as IDataset).Name;

                                    // Parse the table name in order to strip out unneccessary bits of SDE tables
                                    ISQLSyntax nameParser = (ISQLSyntax)m_theWorkspace;
                                    string parsedDbName, parsedOwnerName, parsedTableName;
                                    nameParser.ParseTableName(tableName, out parsedDbName, out parsedOwnerName, out parsedTableName);

                                    // Will need to remove the selected feature from ContactsAndFaults
                                    ContactsAndFaultsAccess ContactsAndFaultsRemover = new ContactsAndFaultsAccess(m_theWorkspace);

                                    // Loop through the selected ContactsAndFaults
                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the ContactsAndFaults feature to remove
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        ContactsAndFaultsRemover.AddContactsAndFaults("ContactsAndFaults_ID = '" + thisId + "'");
                                        ContactsAndFaultsAccess.ContactsAndFault thisContactsAndFault = ContactsAndFaultsRemover.ContactsAndFaultsDictionary[thisId];

                                        // Setup attributes                                        
                                        string DataSourceID = thisContactsAndFault.DataSourceID;
                                        IPolyline Shape = thisContactsAndFault.Shape;

                                        // Create the new feature in the template FeatureClass
                                        switch (parsedTableName)
                                        {
                                            case "OtherLines":
                                                // Insert the new OtherLines
                                                OtherLinesInserter.NewOtherLine(Type, LocationConfidenceMeters, ExistenceConfidence, IdentityConfidence, Label, Notes, DataSourceID, Symbol, Shape);
                                                OtherLinesInserter.SaveOtherLines();

                                                // Clear out the dictionary so lines don't get added more than once.
                                                OtherLinesInserter.ClearOtherLines();

                                                // Delete the old one
                                                ContactsAndFaultsRemover.DeleteContactsAndFaults(thisContactsAndFault);
                                                break;
                                            default:
                                                // This would happen if there's some other polyline featuretemplate selected. Just need to exit gracefully.
                                                break;
                                        }

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }
                                    break;
                            }
                        }
                    }
                    #endregion                    
                    
                    #region OtherLines
                    IFeatureLayer OtherLinesFL = commonFunctions.FindFeatureLayer(m_theWorkspace, "OtherLines");
                    if (OtherLinesFL != null)
                    {
                        // Found OtherLines. Check for a selection
                        IFeatureSelection selectedFeatures = OtherLinesFL as IFeatureSelection;
                        if (selectedFeatures.SelectionSet.Count > 0)
                        {
                            // OtherLines were selected. Pass the selection into a cursor.
                            ISelectionSet featureSelectionSet = selectedFeatures.SelectionSet;
                            ICursor theCursor;
                            featureSelectionSet.Search(null, false, out theCursor);
                            IFeatureCursor theFeatureCursor = (IFeatureCursor)theCursor;

                            // Some variables to declare before the loops
                            int idFld = theFeatureCursor.FindField("OtherLines_ID");
                            IFeature theFeature;
                            string thisId;

                            // Construct a dummy feature to get attributes from
                            IFeature dummyFeature = templateFC.CreateFeature();
                            theCurrentTemplate.SetDefaultValues(dummyFeature);
                            string ExistenceConfidence = dummyFeature.get_Value(templateFC.FindField("ExistenceConfidence")).ToString();
                            string IdentityConfidence = dummyFeature.get_Value(templateFC.FindField("IdentityConfidence")).ToString();
                            string Label = dummyFeature.get_Value(templateFC.FindField("Label")).ToString();
                            double LocationConfidenceMeters;
                            bool result = double.TryParse(dummyFeature.get_Value(templateFC.FindField("LocationConfidenceMeters")).ToString(), out LocationConfidenceMeters);
                            string Notes = dummyFeature.get_Value(templateFC.FindField("Notes")).ToString();
                            string Symbol = dummyFeature.get_Value(templateFC.FindField("Symbol")).ToString();
                            string Type = dummyFeature.get_Value(templateFC.FindField("Type")).ToString();
                            int IsConcealed = 0;
                            if (templateFC.Equals(commonFunctions.OpenFeatureClass(m_theWorkspace, "ContactsAndFaults")) == true)
                            {
                                result = int.TryParse(dummyFeature.get_Value(templateFC.FindField("IsConcealed")).ToString(), out IsConcealed);
                            }

                            // Destroy the dummy feature - must happen within an edit operation
                            ArcMap.Editor.StartOperation();
                            dummyFeature.Delete();
                            ArcMap.Editor.StopOperation("Remove Dummy Feature");

                            //Is the FeatureTemplate also for OtherLines?
                            switch (OtherLinesFL.FeatureClass.Equals(templateFC))
                            {
                                case true:
                                    // Update the features to the FeatureTemplate
                                    OtherLinesAccess OtherLinesUpdater = new OtherLinesAccess(m_theWorkspace);
                                    
                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the OtherLines feature to update
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        OtherLinesUpdater.AddOtherLines("OtherLines_ID = '" + thisId + "'");
                                        OtherLinesAccess.OtherLine thisOtherLine = OtherLinesUpdater.OtherLinesDictionary[thisId];

                                        // Assign values from the FeatureTemplate
                                        thisOtherLine.ExistenceConfidence = ExistenceConfidence;
                                        thisOtherLine.IdentityConfidence = IdentityConfidence;
                                        thisOtherLine.Label = Label;
                                        thisOtherLine.LocationConfidenceMeters = LocationConfidenceMeters;
                                        thisOtherLine.Notes = Notes;
                                        thisOtherLine.Symbol = Symbol;
                                        thisOtherLine.Type = Type;

                                        // Update the feature
                                        OtherLinesUpdater.UpdateOtherLine(thisOtherLine);

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }

                                    // Save changes
                                    OtherLinesUpdater.SaveOtherLines();
                                    break;
                                case false:
                                    // Copy the features into the templateFC using the selected template as a guide

                                    // Build DataAccess Classes for target FeatureClasses
                                    ContactsAndFaultsAccess ContactsAndFaultsInserter = new ContactsAndFaultsAccess(m_theWorkspace);

                                    // Find the target FeatureClass Name
                                    string tableName = (templateFC as IDataset).Name;

                                    // Parse the table name in order to strip out unneccessary bits of SDE tables
                                    ISQLSyntax nameParser = (ISQLSyntax)m_theWorkspace;
                                    string parsedDbName, parsedOwnerName, parsedTableName;
                                    nameParser.ParseTableName(tableName, out parsedDbName, out parsedOwnerName, out parsedTableName);

                                    // Will need to remove the selected feature from OtherLines
                                    OtherLinesAccess OtherLinesRemover = new OtherLinesAccess(m_theWorkspace);

                                    // Loop through the selected OtherLines
                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the OtherLines feature to remove
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        OtherLinesRemover.AddOtherLines("OtherLines_ID = '" + thisId + "'");
                                        OtherLinesAccess.OtherLine thisOtherLine = OtherLinesRemover.OtherLinesDictionary[thisId];

                                        // Setup attributes
                                        string DataSourceID = thisOtherLine.DataSourceID;
                                        IPolyline Shape = thisOtherLine.Shape;

                                        // Create the new feature in the template FeatureClass
                                        switch (parsedTableName)
                                        {
                                            case "ContactsAndFaults":
                                                // Insert the new OtherLines
                                                ContactsAndFaultsInserter.NewContactsAndFault(Type, IsConcealed, LocationConfidenceMeters, ExistenceConfidence, IdentityConfidence, Label, Notes, DataSourceID, Symbol, Shape);
                                                ContactsAndFaultsInserter.SaveContactsAndFaults();

                                                // Clear out the dictionary so lines don't get added more than once.
                                                ContactsAndFaultsInserter.ClearContactsAndFaults();

                                                // Delete the old one
                                                OtherLinesRemover.DeleteOtherLines(thisOtherLine);
                                                break;
                                            default:
                                                // This would happen if there's some other polyline featuretemplate selected. Just need to exit gracefully.
                                                break;
                                        }

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }
                                    break;
                            }
                        }
                    }
                    #endregion

                    break;
                case esriGeometryType.esriGeometryPoint:
                    // Look for features selected in point FeatureClasses
                    //  Note: I don't care about FeatureTemplates in Station or Sample featureclasses.
                    #region OrientationDataPoints
                    IFeatureLayer OrientationDataPointsFL = commonFunctions.FindFeatureLayer(m_theWorkspace, "OrientationDataPoints");
                    if (OrientationDataPointsFL != null)
                    {
                        // Found OrientationDataPoints. Check for a selection
                        IFeatureSelection selectedFeatures = OrientationDataPointsFL as IFeatureSelection;
                        if (selectedFeatures.SelectionSet.Count > 0)
                        {
                            // OrientationDataPoints were selected. Pass the selection into a cursor.
                            ISelectionSet featureSelectionSet = selectedFeatures.SelectionSet;
                            ICursor theCursor;
                            featureSelectionSet.Search(null, false, out theCursor);
                            IFeatureCursor theFeatureCursor = (IFeatureCursor)theCursor;

                            // Some variables to declare before the loops
                            int idFld = theFeatureCursor.FindField("OrientationDataPoints_ID");
                            IFeature theFeature;
                            string thisId;

                            // Construct a dummy feature to get attributes from
                            IFeature dummyFeature = templateFC.CreateFeature();
                            theCurrentTemplate.SetDefaultValues(dummyFeature);
                            string Type = dummyFeature.get_Value(templateFC.FindField("Type")).ToString();
                            string IdentityConfidence = dummyFeature.get_Value(templateFC.FindField("IdentityConfidence")).ToString();
                            string Label = dummyFeature.get_Value(templateFC.FindField("Label")).ToString();
                            int PlotAtScale;
                            bool result = int.TryParse(dummyFeature.get_Value(templateFC.FindField("PlotAtScale")).ToString(), out PlotAtScale);                            
                            double OrientationConfidenceDegrees;
                            result = double.TryParse(dummyFeature.get_Value(templateFC.FindField("OrientationConfidenceDegrees")).ToString(), out OrientationConfidenceDegrees);
                            string Notes = dummyFeature.get_Value(templateFC.FindField("Notes")).ToString();
                            int Symbol;
                            result = int.TryParse(dummyFeature.get_Value(templateFC.FindField("Symbol")).ToString(), out Symbol);

                            // Destroy the dummy feature - must happen within an edit operation
                            ArcMap.Editor.StartOperation();
                            dummyFeature.Delete();
                            ArcMap.Editor.StopOperation("Remove Dummy Feature");

                            //Is the FeatureTemplate also for OrientationDataPoints?
                            switch (OrientationDataPointsFL.FeatureClass.Equals(templateFC))
                            {
                                case true:
                                    // Update the features to the FeatureTemplate
                                    OrientationDataPointsAccess OrientationDataPointsUpdater = new OrientationDataPointsAccess(m_theWorkspace);                                    

                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the OrientationDataPoints feature to update
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        OrientationDataPointsUpdater.AddOrientationDataPoints("OrientationDataPoints_ID = '" + thisId + "'");
                                        OrientationDataPointsAccess.OrientationDataPoint thisOrientationDataPoint = OrientationDataPointsUpdater.OrientationDataPointsDictionary[thisId];

                                        // Assign values from the FeatureTemplate
                                        
                                        thisOrientationDataPoint.Type = Type;
                                        thisOrientationDataPoint.IdentityConfidence = IdentityConfidence;
                                        thisOrientationDataPoint.Label = Label;
                                        thisOrientationDataPoint.PlotAtScale = PlotAtScale;
                                        thisOrientationDataPoint.OrientationConfidenceDegrees = OrientationConfidenceDegrees;
                                        thisOrientationDataPoint.Notes = Notes;
                                        thisOrientationDataPoint.Symbol = Symbol;

                                        // Update the feature
                                        OrientationDataPointsUpdater.UpdateOrientationDataPoint(thisOrientationDataPoint);

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }

                                    // Save changes
                                    OrientationDataPointsUpdater.SaveOrientationDataPoints();
                                    break;
                                case false:
                                    // User needs to select an appropriate feature template
                                    MessageBox.Show("Please select a FeatureTemplate appropriate for the type of features you've selected" + Environment.NewLine + "You've selected some Orientation Data on the map but have not selected a structural FeatureTemplate.");

                                    break;
                            }
                        }
                    }
                    #endregion

                    break;
                case esriGeometryType.esriGeometryPolygon:
                    // Look for features selected in polygon FeatureClasses
                    #region MapUnitPolys
                    IFeatureLayer MapUnitPolysFL = commonFunctions.FindFeatureLayer(m_theWorkspace, "MapUnitPolys");
                    if (MapUnitPolysFL != null)
                    {
                        // Found MapUnitPolys. Check for a selection
                        IFeatureSelection selectedFeatures = MapUnitPolysFL as IFeatureSelection;
                        if (selectedFeatures.SelectionSet.Count > 0)
                        {
                            // MapUnitPolys were selected. Pass the selection into a cursor.
                            ISelectionSet featureSelectionSet = selectedFeatures.SelectionSet;
                            ICursor theCursor;
                            featureSelectionSet.Search(null, false, out theCursor);
                            IFeatureCursor theFeatureCursor = (IFeatureCursor)theCursor;

                            // Some variables to declare before the loops
                            int idFld = theFeatureCursor.FindField("MapUnitPolys_ID");
                            IFeature theFeature;
                            string thisId;

                            // Construct a dummy feature to get attributes from
                            IFeature dummyFeature = templateFC.CreateFeature();
                            theCurrentTemplate.SetDefaultValues(dummyFeature);
                            string MapUnit = dummyFeature.get_Value(templateFC.FindField("MapUnit")).ToString();
                            string IdentityConfidence = dummyFeature.get_Value(templateFC.FindField("IdentityConfidence")).ToString();
                            string Label = dummyFeature.get_Value(templateFC.FindField("Label")).ToString();
                            string Notes = dummyFeature.get_Value(templateFC.FindField("Notes")).ToString();
                            string Symbol = dummyFeature.get_Value(templateFC.FindField("Symbol")).ToString();

                            // Destroy the dummy feature - must happen within an edit operation
                            ArcMap.Editor.StartOperation();
                            dummyFeature.Delete();
                            ArcMap.Editor.StopOperation("Remove Dummy Feature");

                            //Is the FeatureTemplate also for MapUnitPolys?
                            switch (MapUnitPolysFL.FeatureClass.Equals(templateFC))
                            {
                                case true:
                                    // Update the features to the FeatureTemplate
                                    MapUnitPolysAccess MapUnitPolysUpdater = new MapUnitPolysAccess(m_theWorkspace);                                    

                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the MapUnitPolys feature to update
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        MapUnitPolysUpdater.AddMapUnitPolys("MapUnitPolys_ID = '" + thisId + "'");
                                        MapUnitPolysAccess.MapUnitPoly thisMapUnitPoly = MapUnitPolysUpdater.MapUnitPolysDictionary[thisId];

                                        // Assign values from the FeatureTemplate
                                        thisMapUnitPoly.MapUnit = MapUnit;
                                        thisMapUnitPoly.IdentityConfidence = IdentityConfidence;
                                        thisMapUnitPoly.Label = Label;
                                        thisMapUnitPoly.Notes = Notes;
                                        thisMapUnitPoly.Symbol = Symbol;

                                        // Update the feature
                                        MapUnitPolysUpdater.UpdateMapUnitPoly(thisMapUnitPoly);

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }

                                    // Save changes
                                    MapUnitPolysUpdater.SaveMapUnitPolys();
                                    break;
                                case false:
                                    // Copy the features into the templateFC using the selected template as a guide

                                    // Build DataAccess Classes for target FeatureClasses
                                    OverlayPolysAccess OverlayPolysInserter = new OverlayPolysAccess(m_theWorkspace);

                                    // Find the target FeatureClass Name
                                    string tableName = (templateFC as IDataset).Name;

                                    // Parse the table name in order to strip out unneccessary bits of SDE tables
                                    ISQLSyntax nameParser = (ISQLSyntax)m_theWorkspace;
                                    string parsedDbName, parsedOwnerName, parsedTableName;
                                    nameParser.ParseTableName(tableName, out parsedDbName, out parsedOwnerName, out parsedTableName);

                                    // Will need to remove the selected feature from MapUnitPolys
                                    MapUnitPolysAccess MapUnitPolysRemover = new MapUnitPolysAccess(m_theWorkspace);

                                    // Loop through the selected MapUnitPolys
                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the MapUnitPolys feature to remove
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        MapUnitPolysRemover.AddMapUnitPolys("MapUnitPolys_ID = '" + thisId + "'");
                                        MapUnitPolysAccess.MapUnitPoly thisMapUnitPoly = MapUnitPolysRemover.MapUnitPolysDictionary[thisId];

                                        // Setup attributes                                        
                                        string DataSourceID = thisMapUnitPoly.DataSourceID;
                                        IPolygon Shape = thisMapUnitPoly.Shape;

                                        // Create the new feature in the template FeatureClass
                                        switch (parsedTableName)
                                        {
                                            case "OverlayPolys":
                                                // Insert the new OtherLines
                                                OverlayPolysInserter.NewOverlayPoly(MapUnit, IdentityConfidence, Label, Notes, DataSourceID, Symbol, Shape);
                                                OverlayPolysInserter.SaveOverlayPolys();

                                                // Clear out the dictionary so lines don't get added more than once.
                                                OverlayPolysInserter.ClearOverlayPolys();

                                                // Delete the old one
                                                MapUnitPolysRemover.DeleteMapUnitPolys(thisMapUnitPoly);
                                                break;
                                            default:
                                                // This would happen if there's some other polyline featuretemplate selected. Just need to exit gracefully.
                                                break;
                                        }

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region OverlayPolys
                    IFeatureLayer OverlayPolysFL = commonFunctions.FindFeatureLayer(m_theWorkspace, "OverlayPolys");
                    if (OverlayPolysFL != null)
                    {
                        // Found OverlayPolys. Check for a selection
                        IFeatureSelection selectedFeatures = OverlayPolysFL as IFeatureSelection;
                        if (selectedFeatures.SelectionSet.Count > 0)
                        {
                            // OverlayPolys were selected. Pass the selection into a cursor.
                            ISelectionSet featureSelectionSet = selectedFeatures.SelectionSet;
                            ICursor theCursor;
                            featureSelectionSet.Search(null, false, out theCursor);
                            IFeatureCursor theFeatureCursor = (IFeatureCursor)theCursor;

                            // Some variables to declare before the loops
                            int idFld = theFeatureCursor.FindField("OverlayPolys_ID");
                            IFeature theFeature;
                            string thisId;

                            // Construct a dummy feature to get attributes from
                            IFeature dummyFeature = templateFC.CreateFeature();
                            theCurrentTemplate.SetDefaultValues(dummyFeature);
                            string MapUnit = dummyFeature.get_Value(templateFC.FindField("MapUnit")).ToString();
                            string IdentityConfidence = dummyFeature.get_Value(templateFC.FindField("IdentityConfidence")).ToString();
                            string Label = dummyFeature.get_Value(templateFC.FindField("Label")).ToString();
                            string Notes = dummyFeature.get_Value(templateFC.FindField("Notes")).ToString();
                            string Symbol = dummyFeature.get_Value(templateFC.FindField("Symbol")).ToString();

                            // Destroy the dummy feature - must happen within an edit operation
                            ArcMap.Editor.StartOperation();
                            dummyFeature.Delete();
                            ArcMap.Editor.StopOperation("Remove Dummy Feature");

                            //Is the FeatureTemplate also for OverlayPolys?
                            switch (OverlayPolysFL.FeatureClass.Equals(templateFC))
                            {
                                case true:
                                    // Update the features to the FeatureTemplate
                                    OverlayPolysAccess OverlayPolysUpdater = new OverlayPolysAccess(m_theWorkspace);                                    

                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the OverlayPolys feature to update
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        OverlayPolysUpdater.AddOverlayPolys("OverlayPolys_ID = '" + thisId + "'");
                                        OverlayPolysAccess.OverlayPoly thisOverlayPoly = OverlayPolysUpdater.OverlayPolysDictionary[thisId];

                                        // Assign values from the FeatureTemplate
                                        thisOverlayPoly.MapUnit = MapUnit;
                                        thisOverlayPoly.IdentityConfidence = IdentityConfidence;
                                        thisOverlayPoly.Label = Label;
                                        thisOverlayPoly.Notes = Notes;
                                        thisOverlayPoly.Symbol = Symbol;

                                        // Update the feature
                                        OverlayPolysUpdater.UpdateOverlayPoly(thisOverlayPoly);

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }

                                    // Save changes
                                    OverlayPolysUpdater.SaveOverlayPolys();
                                    break;
                                case false:
                                    // Copy the features into the templateFC using the selected template as a guide

                                    // Build DataAccess Classes for target FeatureClasses
                                    MapUnitPolysAccess MapUnitPolysInserter = new MapUnitPolysAccess(m_theWorkspace);

                                    // Find the target FeatureClass Name
                                    string tableName = (templateFC as IDataset).Name;

                                    // Parse the table name in order to strip out unneccessary bits of SDE tables
                                    ISQLSyntax nameParser = (ISQLSyntax)m_theWorkspace;
                                    string parsedDbName, parsedOwnerName, parsedTableName;
                                    nameParser.ParseTableName(tableName, out parsedDbName, out parsedOwnerName, out parsedTableName);

                                    // Will need to remove the selected feature from OverlayPolys
                                    OverlayPolysAccess OverlayPolysRemover = new OverlayPolysAccess(m_theWorkspace);

                                    // Loop through the selected OverlayPolys
                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the OverlayPolys feature to remove
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        OverlayPolysRemover.AddOverlayPolys("OverlayPolys_ID = '" + thisId + "'");
                                        OverlayPolysAccess.OverlayPoly thisOverlayPoly = OverlayPolysRemover.OverlayPolysDictionary[thisId];

                                        // Setup attributes                                        
                                        string DataSourceID = thisOverlayPoly.DataSourceID;
                                        IPolygon Shape = thisOverlayPoly.Shape;

                                        // Create the new feature in the template FeatureClass
                                        switch (parsedTableName)
                                        {
                                            case "MapUnitPolys":
                                                // Insert the new OtherLines
                                                MapUnitPolysInserter.NewMapUnitPoly(MapUnit, IdentityConfidence, Label, Notes, DataSourceID, Symbol, Shape);
                                                MapUnitPolysInserter.SaveMapUnitPolys();

                                                // Clear out the dictionary so lines don't get added more than once.
                                                MapUnitPolysInserter.ClearMapUnitPolys();

                                                // Delete the old one
                                                OverlayPolysRemover.DeleteOverlayPolys(thisOverlayPoly);
                                                break;
                                            default:
                                                // This would happen if there's some other polygon featuretemplate selected. Just need to exit gracefully.
                                                break;
                                        }

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }
                                    break;
                            }
                        }
                    }
                    #endregion

                    break;
            }
        }

        private void RecalculateIdentifiers(Dictionary<string, object> dataAccessClasses)
        {
            // SysInfo reference to calculate new IDs
            sysInfo theSysInfo = new sysInfo(ArcMap.Editor.EditWorkspace);

            // Begin looping through dataAccessClasses
            foreach (KeyValuePair<string, object> anEntry in dataAccessClasses)
            {
                // Parse the table name
                ISQLSyntax nameParser = (ISQLSyntax)m_theWorkspace;
                string parsedDbName, parsedOwnerName, parsedTableName;
                nameParser.ParseTableName(anEntry.Key, out parsedDbName, out parsedOwnerName, out parsedTableName);

                // Build an SQL query for the features/rows passed in
                string sqlWhereClause = parsedTableName + "_ID = '";
                switch(parsedTableName)
                {
                    case "MapUnitPolys":
                        foreach (KeyValuePair<string, MapUnitPolysAccess.MapUnitPoly> aRecord in (anEntry.Value as MapUnitPolysAccess).MapUnitPolysDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "ContactsAndFaults":
                        foreach (KeyValuePair<string, ContactsAndFaultsAccess.ContactsAndFault> aRecord in (anEntry.Value as ContactsAndFaultsAccess).ContactsAndFaultsDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "OtherLines":
                        foreach (KeyValuePair<string, OtherLinesAccess.OtherLine> aRecord in (anEntry.Value as OtherLinesAccess).OtherLinesDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "OverlayPolys":
                        foreach (KeyValuePair<string, OverlayPolysAccess.OverlayPoly> aRecord in (anEntry.Value as OverlayPolysAccess).OverlayPolysDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "OrientationDataPoints":
                        foreach (KeyValuePair<string, OrientationDataPointsAccess.OrientationDataPoint> aRecord in (anEntry.Value as OrientationDataPointsAccess).OrientationDataPointsDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "SamplePoints":
                        foreach (KeyValuePair<string, SamplePointsAccess.SamplePoint> aRecord in (anEntry.Value as SamplePointsAccess).SamplePointsDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "StationPoints":
                        foreach (KeyValuePair<string, StationPointsAccess.StationPoint> aRecord in (anEntry.Value as StationPointsAccess).StationPointsDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "DataSources":
                        foreach (KeyValuePair<string, DataSourcesAccess.Datasource> aRecord in (anEntry.Value as DataSourcesAccess).DataSourceCollection)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "DescriptionOfMapUnits":
                        foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> aRecord in (anEntry.Value as DescriptionOfMapUnitsAccess).DescriptionOfMapUnitsDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "Glossary":
                        foreach (KeyValuePair<string, GlossaryAccess.Glossary> aRecord in (anEntry.Value as GlossaryAccess).GlossaryDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "Notes":
                        foreach (KeyValuePair<string, NotesAccess.Note> aRecord in (anEntry.Value as NotesAccess).NotesDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "RelatedDocuments":
                        foreach (KeyValuePair<string, RelatedDocumentsAccess.RelatedDocument> aRecord in (anEntry.Value as RelatedDocumentsAccess).RelatedDocumentsDictionary)
	                    {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                }
                
                // Cleanup the string
                sqlWhereClause = sqlWhereClause.Remove(sqlWhereClause.Length - (11 + parsedTableName.Length));

                // Get the features
                ITable theTable = commonFunctions.OpenTable(m_theWorkspace, parsedTableName);
                IQueryFilter theQF = new QueryFilterClass();
                theQF.WhereClause = sqlWhereClause;

                // Wrap in edit operation
                ArcMap.Editor.StartOperation();
                try
                {
                    ICursor theCursor = theTable.Update(theQF, false);
                    IRow theRow = theCursor.NextRow();
                    int theIdFld = theTable.FindField(parsedTableName + "_ID");

                    while (theRow != null)
                    {
                        // Update the identifier
                        theRow.set_Value(theIdFld, theSysInfo.ProjAbbr + "." + parsedTableName + "." + theSysInfo.GetNextIdValue(parsedTableName));
                        theCursor.UpdateRow(theRow);

                        //Iterate
                        theRow = theCursor.NextRow();
                    }

                    // Close the edit session
                    ArcMap.Editor.StopOperation("Update Identifiers");
                }
                catch
                {
                    ArcMap.Editor.StopOperation("Failed to update identifiers");
                }
            }            
        }

        private void BulkIdentifierCalc()
        {
            // Get an enumeration of the selected features
            IEnumFeature selectionEnum = ArcMap.Document.FocusMap.FeatureSelection as IEnumFeature;
        }
    }
}

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
            string DataSourcePolysSearch = "DataSourcePolys_ID = '";
            string OtherPolysSearch = "OtherPolys_ID = '";
            string ContactsAndFaultsSearch = "ContactsAndFaults_ID = '";
            string GeologicLinesSearch = "GeologicLines_ID = '";
            string StationsSearch = "Stations_ID = '";
            string GenericSamplesSearch = "GenericSamples_ID = '";
            string OrientationPointsSearch = "OrientationPoints_ID = '";
            string GlossarySearch = "Glossary_ID = '";
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
                    case "DataSourcePolys":
                        DataSourcePolysSearch += thisFeature.get_Value(thisFeature.Table.FindField("DataSourcePolys_ID")) + "' OR DataSourcePolys_ID = '";
                        break;
                    case "OtherPolys":
                        OtherPolysSearch += thisFeature.get_Value(thisFeature.Table.FindField("OtherPolys_ID")) + "' OR OtherPolys_ID = '";
                        break;
                    case "ContactsAndFaults":
                        ContactsAndFaultsSearch += thisFeature.get_Value(thisFeature.Table.FindField("ContactsAndFaults_ID")) + "' OR ContactsAndFaults_ID = '";
                        break;
                    case "GeologicLines":
                        GeologicLinesSearch += thisFeature.get_Value(thisFeature.Table.FindField("GeologicLines_ID")) + "' OR GeologicLines_ID = '";
                        break;
                    case "Stations":
                        StationsSearch += thisFeature.get_Value(thisFeature.Table.FindField("Stations_ID")) + "' OR Stations_ID = '";
                        break;
                    case "GenericSamples":
                        GenericSamplesSearch += thisFeature.get_Value(thisFeature.Table.FindField("GenericSamples_ID")) + "' OR GenericSamples_ID = '";
                        break;
                    case "OrientationPoints":
                        OrientationPointsSearch += thisFeature.get_Value(thisFeature.Table.FindField("OrientationPoints_ID")) + "' OR OrientationPoints_ID = '";
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

            if (DataSourcePolysSearch != "DataSourcePolys_ID = '")
            {
                DataSourcePolysSearch = DataSourcePolysSearch.Remove(DataSourcePolysSearch.Length - 25);
                DataSourcePolysAccess DataSourcePolysRecords = new DataSourcePolysAccess(m_theWorkspace);
                DataSourcePolysRecords.AddDataSourcePolys(DataSourcePolysSearch);
                dataAccessClasses.Add("DataSourcePolys", DataSourcePolysRecords);
            }

            // OtherPolys
            if (OtherPolysSearch != "OtherPolys_ID = '")
            {
                OtherPolysSearch = OtherPolysSearch.Remove(OtherPolysSearch.Length - 23);
                OtherPolysAccess OtherPolysRecords = new OtherPolysAccess(m_theWorkspace);
                OtherPolysRecords.AddOtherPolys(OtherPolysSearch);
                dataAccessClasses.Add("OtherPolys", OtherPolysRecords);
            }

            // ContactsAndFaults
            if (ContactsAndFaultsSearch != "ContactsAndFaults_ID = '")
            {
                ContactsAndFaultsSearch = ContactsAndFaultsSearch.Remove(ContactsAndFaultsSearch.Length - 28);
                ContactsAndFaultsAccess ContactsAndFaultsRecords = new ContactsAndFaultsAccess(m_theWorkspace);
                ContactsAndFaultsRecords.AddContactsAndFaults(ContactsAndFaultsSearch);
                dataAccessClasses.Add("ContactsAndFaults", ContactsAndFaultsRecords);
            }

            // GeologicLines
            if (GeologicLinesSearch != "GeologicLines_ID = '")
            {
                GeologicLinesSearch = GeologicLinesSearch.Remove(GeologicLinesSearch.Length - 24);
                GeologicLinesAccess GeologicLinesRecords = new GeologicLinesAccess(m_theWorkspace);
                GeologicLinesRecords.AddGeologicLines(GeologicLinesSearch);
                dataAccessClasses.Add("GeologicLines", GeologicLinesRecords);
            }

            // Stations
            if (StationsSearch != "Stations_ID = '")
            {
                StationsSearch = StationsSearch.Remove(StationsSearch.Length - 19);
                StationsAccess StationsRecords = new StationsAccess(m_theWorkspace);
                StationsRecords.AddStations(StationsSearch);
                dataAccessClasses.Add("Stations", StationsRecords);
            }

            // GenericSamples
            if (GenericSamplesSearch != "GenericSamples_ID = '")
            {
                GenericSamplesSearch = GenericSamplesSearch.Remove(GenericSamplesSearch.Length - 23);
                GenericSamplesAccess GenericSamplesRecords = new GenericSamplesAccess(m_theWorkspace);
                GenericSamplesRecords.AddGenericSamples(GenericSamplesSearch);
                dataAccessClasses.Add("GenericSamples", GenericSamplesRecords);
            }

            // OrientationPoints
            if (OrientationPointsSearch != "OrientationPoints_ID = '")
            {
                OrientationPointsSearch = OrientationPointsSearch.Remove(OrientationPointsSearch.Length - 32);
                OrientationPointsAccess OrientationPointsRecords = new OrientationPointsAccess(m_theWorkspace);
                OrientationPointsRecords.AddOrientationPoints(OrientationPointsSearch);
                dataAccessClasses.Add("OrientationPoints", OrientationPointsRecords);
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
                string tableName = null;
                if (thisTable.Table == null)
                    tableName = thisTable.Name;
                else
                    tableName = (thisTable.Table as IDataset).Name;

                // Parse the table name in order to strip out unneccessary bits of SDE tables
                ISQLSyntax nameParser = (ISQLSyntax)m_theWorkspace;
                string parsedDbName, parsedOwnerName, parsedTableName;
                nameParser.ParseTableName(tableName, out parsedDbName, out parsedOwnerName, out parsedTableName);

                // Find the selection
                ITableSelection selectedRows = thisTable as ITableSelection;
                ISelectionSet theSelection = selectedRows.SelectionSet;

                // Iterate if there are no selected rows
                if (theSelection == null)
                    continue;
                else
                    if (theSelection.Count == 0)    
                        continue;

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
            DataSourcePolysAccess dataSourcePolysAccess = new DataSourcePolysAccess(m_theWorkspace);
            OtherPolysAccess OtherPolysAccess = new OtherPolysAccess(m_theWorkspace);
            ContactsAndFaultsAccess ContactsAndFaultsAccess = new ContactsAndFaultsAccess(m_theWorkspace);
            GeologicLinesAccess GeologicLinesAccess = new GeologicLinesAccess(m_theWorkspace);
            StationsAccess StationsAccess = new StationsAccess(m_theWorkspace);
            GenericSamplesAccess GenericSamplesAccess = new GenericSamplesAccess(m_theWorkspace);
            OrientationPointsAccess OrientationPointsAccess = new OrientationPointsAccess(m_theWorkspace);
            GlossaryAccess GlossaryAccess = new GlossaryAccess(m_theWorkspace);
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
                    case "DataSourcePolys":
                        // Loop through the records in the data access object that comes in
                        DataSourcePolysAccess thisDataSourcePolysAccess = (DataSourcePolysAccess)anEntry.Value;
                        foreach (KeyValuePair<string, DataSourcePolysAccess.DataSourcePoly> DataSourcePolysToUpdate in thisDataSourcePolysAccess.DataSourcePolysDictionary)
                        {
                            DataSourcePolysAccess.DataSourcePoly thisDataSourcePoly = (DataSourcePolysAccess.DataSourcePoly)DataSourcePolysToUpdate.Value;
                            thisDataSourcePoly.DataSourceID = dataSourceID;
                            dataSourcePolysAccess.UpdateDataSourcePoly(thisDataSourcePoly);
                        }
                        dataSourcePolysAccess.SaveDataSourcePolys();
                        break;
                    case "OtherPolys":
                        OtherPolysAccess thisOtherPolysAccess = (OtherPolysAccess)anEntry.Value;
                        foreach (KeyValuePair<string, OtherPolysAccess.OtherPoly> OtherPolysToUpdate in thisOtherPolysAccess.OtherPolysDictionary)
                        {
                            OtherPolysAccess.OtherPoly thisOtherPoly = (OtherPolysAccess.OtherPoly)OtherPolysToUpdate.Value;
                            thisOtherPoly.DataSourceID = dataSourceID;
                            OtherPolysAccess.UpdateOtherPoly(thisOtherPoly);
                        }
                        OtherPolysAccess.SaveOtherPolys();
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
                    case "GeologicLines":
                        GeologicLinesAccess thisGeologicLinesAccess = (GeologicLinesAccess)anEntry.Value;
                        foreach (KeyValuePair<string, GeologicLinesAccess.GeologicLine> GeologicLinesToUpdate in thisGeologicLinesAccess.GeologicLinesDictionary)
                        {
                            GeologicLinesAccess.GeologicLine thisGeologicLine = (GeologicLinesAccess.GeologicLine)GeologicLinesToUpdate.Value;
                            thisGeologicLine.DataSourceID = dataSourceID;
                            GeologicLinesAccess.UpdateGeologicLine(thisGeologicLine);
                        }
                        GeologicLinesAccess.SaveGeologicLines();
                        break;
                    case "Stations":
                        StationsAccess thisStationsAccess = (StationsAccess)anEntry.Value;
                        foreach (KeyValuePair<string, StationsAccess.Station> StationsToUpdate in thisStationsAccess.StationsDictionary)
                        {
                            StationsAccess.Station thisStation = (StationsAccess.Station)StationsToUpdate.Value;
                            thisStation.DataSourceID = dataSourceID;
                            StationsAccess.UpdateStation(thisStation);
                        }
                        StationsAccess.SaveStations();
                        break;
                    case "GenericSamples":
                        GenericSamplesAccess thisGenericSamplesAccess = (GenericSamplesAccess)anEntry.Value;
                        foreach (KeyValuePair<string, GenericSamplesAccess.GenericSample> GenericSamplesToUpdate in thisGenericSamplesAccess.GenericSamplesDictionary)
                        {
                            GenericSamplesAccess.GenericSample thisGenericSample = (GenericSamplesAccess.GenericSample)GenericSamplesToUpdate.Value;
                            thisGenericSample.DataSourceID = dataSourceID;
                            GenericSamplesAccess.UpdateGenericSample(thisGenericSample);
                        }
                        GenericSamplesAccess.SaveGenericSamples();
                        break;
                    case "OrientationPoints":
                        OrientationPointsAccess thisOrientationPointsAccess = (OrientationPointsAccess)anEntry.Value;
                        foreach (KeyValuePair<string, OrientationPointsAccess.OrientationPoint> OrientationPointsToUpdate in thisOrientationPointsAccess.OrientationPointsDictionary)
                        {
                            OrientationPointsAccess.OrientationPoint thisOrientationPoint = (OrientationPointsAccess.OrientationPoint)OrientationPointsToUpdate.Value;
                            thisOrientationPoint.DataSourceID = dataSourceID;
                            OrientationPointsAccess.UpdateOrientationPoint(thisOrientationPoint);
                        }
                        OrientationPointsAccess.SaveOrientationPoints();
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
                            string Symbol = dummyFeature.get_Value(templateFC.FindField("Symbol")).ToString();
                            string Label = dummyFeature.get_Value(templateFC.FindField("Label")).ToString();
                            double LocationConfidenceMeters;
                            bool result = double.TryParse(dummyFeature.get_Value(templateFC.FindField("LocationConfidenceMeters")).ToString(), out LocationConfidenceMeters);
                            string Notes = dummyFeature.get_Value(templateFC.FindField("Notes")).ToString();
                            string RuleID = dummyFeature.get_Value(templateFC.FindField("RuleID")).ToString();
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
                                        thisContactsAndFault.Symbol = Symbol;
                                        thisContactsAndFault.Label = Label;
                                        thisContactsAndFault.LocationConfidenceMeters = LocationConfidenceMeters;
                                        thisContactsAndFault.Notes = Notes;
                                        thisContactsAndFault.RuleID = RuleID;
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
                                    GeologicLinesAccess GeologicLinesInserter = new GeologicLinesAccess(m_theWorkspace);

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
                                            case "GeologicLines":
                                                // Insert the new GeologicLines
                                                GeologicLinesInserter.NewGeologicLine(Type, LocationConfidenceMeters, ExistenceConfidence, IdentityConfidence, Symbol, Label, Notes, DataSourceID, RuleID, Shape);
                                                GeologicLinesInserter.SaveGeologicLines();

                                                // Clear out the dictionary so lines don't get added more than once.
                                                GeologicLinesInserter.ClearGeologicLines();

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

                    #region GeologicLines
                    IFeatureLayer GeologicLinesFL = commonFunctions.FindFeatureLayer(m_theWorkspace, "GeologicLines");
                    if (GeologicLinesFL != null)
                    {
                        // Found GeologicLines. Check for a selection
                        IFeatureSelection selectedFeatures = GeologicLinesFL as IFeatureSelection;
                        if (selectedFeatures.SelectionSet.Count > 0)
                        {
                            // GeologicLines were selected. Pass the selection into a cursor.
                            ISelectionSet featureSelectionSet = selectedFeatures.SelectionSet;
                            ICursor theCursor;
                            featureSelectionSet.Search(null, false, out theCursor);
                            IFeatureCursor theFeatureCursor = (IFeatureCursor)theCursor;

                            // Some variables to declare before the loops
                            int idFld = theFeatureCursor.FindField("GeologicLines_ID");
                            IFeature theFeature;
                            string thisId;

                            // Construct a dummy feature to get attributes from
                            IFeature dummyFeature = templateFC.CreateFeature();
                            theCurrentTemplate.SetDefaultValues(dummyFeature);
                            string ExistenceConfidence = dummyFeature.get_Value(templateFC.FindField("ExistenceConfidence")).ToString();
                            string IdentityConfidence = dummyFeature.get_Value(templateFC.FindField("IdentityConfidence")).ToString();
                            string Symbol = dummyFeature.get_Value(templateFC.FindField("Symbol")).ToString();
                            string Label = dummyFeature.get_Value(templateFC.FindField("Label")).ToString();
                            double LocationConfidenceMeters;
                            bool result = double.TryParse(dummyFeature.get_Value(templateFC.FindField("LocationConfidenceMeters")).ToString(), out LocationConfidenceMeters);
                            string Notes = dummyFeature.get_Value(templateFC.FindField("Notes")).ToString();
                            string RuleID = dummyFeature.get_Value(templateFC.FindField("RuleID")).ToString();
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

                            //Is the FeatureTemplate also for GeologicLines?
                            switch (GeologicLinesFL.FeatureClass.Equals(templateFC))
                            {
                                case true:
                                    // Update the features to the FeatureTemplate
                                    GeologicLinesAccess GeologicLinesUpdater = new GeologicLinesAccess(m_theWorkspace);

                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the GeologicLines feature to update
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        GeologicLinesUpdater.AddGeologicLines("GeologicLines_ID = '" + thisId + "'");
                                        GeologicLinesAccess.GeologicLine thisGeologicLine = GeologicLinesUpdater.GeologicLinesDictionary[thisId];

                                        // Assign values from the FeatureTemplate
                                        if (ExistenceConfidence != "")
                                            thisGeologicLine.ExistenceConfidence = ExistenceConfidence;
                                        if (IdentityConfidence != "")
                                            thisGeologicLine.IdentityConfidence = IdentityConfidence;
                                        if (Symbol != "")
                                            thisGeologicLine.Symbol = Symbol;
                                        if (Label != "")
                                            thisGeologicLine.Label = Label;
                                        if (LocationConfidenceMeters != 0.0)
                                            thisGeologicLine.LocationConfidenceMeters = LocationConfidenceMeters;
                                        if (Notes != "")
                                            thisGeologicLine.Notes = Notes;
                                        if (RuleID != "")
                                            thisGeologicLine.RuleID = RuleID;
                                        if (Type != "")
                                            thisGeologicLine.Type = Type;

                                        // Update the feature
                                        GeologicLinesUpdater.UpdateGeologicLine(thisGeologicLine);

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }

                                    // Save changes
                                    GeologicLinesUpdater.SaveGeologicLines();
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

                                    // Will need to remove the selected feature from GeologicLines
                                    GeologicLinesAccess GeologicLinesRemover = new GeologicLinesAccess(m_theWorkspace);

                                    // Loop through the selected GeologicLines
                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the GeologicLines feature to remove
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        GeologicLinesRemover.AddGeologicLines("GeologicLines_ID = '" + thisId + "'");
                                        GeologicLinesAccess.GeologicLine thisGeologicLine = GeologicLinesRemover.GeologicLinesDictionary[thisId];

                                        // Setup attributes
                                        string DataSourceID = thisGeologicLine.DataSourceID;
                                        IPolyline Shape = thisGeologicLine.Shape;

                                        // Create the new feature in the template FeatureClass
                                        switch (parsedTableName)
                                        {
                                            case "ContactsAndFaults":
                                                // Insert the new GeologicLines
                                                ContactsAndFaultsInserter.NewContactsAndFault(Type, IsConcealed, LocationConfidenceMeters, ExistenceConfidence, IdentityConfidence, Symbol, Label, Notes, DataSourceID, RuleID, Shape);
                                                ContactsAndFaultsInserter.SaveContactsAndFaults();

                                                // Clear out the dictionary so lines don't get added more than once.
                                                ContactsAndFaultsInserter.ClearContactsAndFaults();

                                                // Delete the old one
                                                GeologicLinesRemover.DeleteGeologicLines(thisGeologicLine);
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
                    #region OrientationPoints
                    IFeatureLayer OrientationPointsFL = commonFunctions.FindFeatureLayer(m_theWorkspace, "OrientationPoints");
                    if (OrientationPointsFL != null)
                    {
                        // Found OrientationPoints. Check for a selection
                        IFeatureSelection selectedFeatures = OrientationPointsFL as IFeatureSelection;
                        if (selectedFeatures.SelectionSet.Count > 0)
                        {
                            // OrientationPoints were selected. Pass the selection into a cursor.
                            ISelectionSet featureSelectionSet = selectedFeatures.SelectionSet;
                            ICursor theCursor;
                            featureSelectionSet.Search(null, false, out theCursor);
                            IFeatureCursor theFeatureCursor = (IFeatureCursor)theCursor;

                            // Some variables to declare before the loops
                            int idFld = theFeatureCursor.FindField("OrientationPoints_ID");
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
                            int RuleID;
                            result = int.TryParse(dummyFeature.get_Value(templateFC.FindField("RuleID")).ToString(), out RuleID);

                            // Destroy the dummy feature - must happen within an edit operation
                            ArcMap.Editor.StartOperation();
                            dummyFeature.Delete();
                            ArcMap.Editor.StopOperation("Remove Dummy Feature");

                            //Is the FeatureTemplate also for OrientationPoints?
                            switch (OrientationPointsFL.FeatureClass.Equals(templateFC))
                            {
                                case true:
                                    // Update the features to the FeatureTemplate
                                    OrientationPointsAccess OrientationPointsUpdater = new OrientationPointsAccess(m_theWorkspace);

                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the OrientationPoints feature to update
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        OrientationPointsUpdater.AddOrientationPoints("OrientationPoints_ID = '" + thisId + "'");
                                        OrientationPointsAccess.OrientationPoint thisOrientationPoint = OrientationPointsUpdater.OrientationPointsDictionary[thisId];

                                        // Assign values from the FeatureTemplate

                                        thisOrientationPoint.Type = Type;
                                        thisOrientationPoint.IdentityConfidence = IdentityConfidence;
                                        thisOrientationPoint.Label = Label;
                                        thisOrientationPoint.PlotAtScale = PlotAtScale;
                                        thisOrientationPoint.OrientationConfidenceDegrees = OrientationConfidenceDegrees;
                                        thisOrientationPoint.Notes = Notes;
                                        thisOrientationPoint.RuleID = RuleID;

                                        // Update the feature
                                        OrientationPointsUpdater.UpdateOrientationPoint(thisOrientationPoint);

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }

                                    // Save changes
                                    OrientationPointsUpdater.SaveOrientationPoints();
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
                                    OtherPolysAccess OtherPolysInserter = new OtherPolysAccess(m_theWorkspace);

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
                                            case "OtherPolys":
                                                // Insert the new GeologicLines
                                                OtherPolysInserter.NewOtherPoly(MapUnit, IdentityConfidence, Label, Notes, DataSourceID, Symbol, Shape);
                                                OtherPolysInserter.SaveOtherPolys();

                                                // Clear out the dictionary so lines don't get added more than once.
                                                OtherPolysInserter.ClearOtherPolys();

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

                    #region OtherPolys
                    IFeatureLayer OtherPolysFL = commonFunctions.FindFeatureLayer(m_theWorkspace, "OtherPolys");
                    if (OtherPolysFL != null)
                    {
                        // Found OtherPolys. Check for a selection
                        IFeatureSelection selectedFeatures = OtherPolysFL as IFeatureSelection;
                        if (selectedFeatures.SelectionSet.Count > 0)
                        {
                            // OtherPolys were selected. Pass the selection into a cursor.
                            ISelectionSet featureSelectionSet = selectedFeatures.SelectionSet;
                            ICursor theCursor;
                            featureSelectionSet.Search(null, false, out theCursor);
                            IFeatureCursor theFeatureCursor = (IFeatureCursor)theCursor;

                            // Some variables to declare before the loops
                            int idFld = theFeatureCursor.FindField("OtherPolys_ID");
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

                            //Is the FeatureTemplate also for OtherPolys?
                            switch (OtherPolysFL.FeatureClass.Equals(templateFC))
                            {
                                case true:
                                    // Update the features to the FeatureTemplate
                                    OtherPolysAccess OtherPolysUpdater = new OtherPolysAccess(m_theWorkspace);

                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the OtherPolys feature to update
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        OtherPolysUpdater.AddOtherPolys("OtherPolys_ID = '" + thisId + "'");
                                        OtherPolysAccess.OtherPoly thisOtherPoly = OtherPolysUpdater.OtherPolysDictionary[thisId];

                                        // Assign values from the FeatureTemplate
                                        thisOtherPoly.MapUnit = MapUnit;
                                        thisOtherPoly.IdentityConfidence = IdentityConfidence;
                                        thisOtherPoly.Label = Label;
                                        thisOtherPoly.Notes = Notes;
                                        thisOtherPoly.Symbol = Symbol;

                                        // Update the feature
                                        OtherPolysUpdater.UpdateOtherPoly(thisOtherPoly);

                                        // Increment the cursor
                                        theFeature = theFeatureCursor.NextFeature();
                                    }

                                    // Save changes
                                    OtherPolysUpdater.SaveOtherPolys();
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

                                    // Will need to remove the selected feature from OtherPolys
                                    OtherPolysAccess OtherPolysRemover = new OtherPolysAccess(m_theWorkspace);

                                    // Loop through the selected OtherPolys
                                    theFeature = theFeatureCursor.NextFeature();
                                    while (theFeature != null)
                                    {
                                        // Find the OtherPolys feature to remove
                                        thisId = theFeature.get_Value(idFld).ToString();
                                        OtherPolysRemover.AddOtherPolys("OtherPolys_ID = '" + thisId + "'");
                                        OtherPolysAccess.OtherPoly thisOtherPoly = OtherPolysRemover.OtherPolysDictionary[thisId];

                                        // Setup attributes                                        
                                        string DataSourceID = thisOtherPoly.DataSourceID;
                                        IPolygon Shape = thisOtherPoly.Shape;

                                        // Create the new feature in the template FeatureClass
                                        switch (parsedTableName)
                                        {
                                            case "MapUnitPolys":
                                                // Insert the new GeologicLines
                                                MapUnitPolysInserter.NewMapUnitPoly(MapUnit, IdentityConfidence, Label, Notes, DataSourceID, Symbol, Shape);
                                                MapUnitPolysInserter.SaveMapUnitPolys();

                                                // Clear out the dictionary so lines don't get added more than once.
                                                MapUnitPolysInserter.ClearMapUnitPolys();

                                                // Delete the old one
                                                OtherPolysRemover.DeleteOtherPolys(thisOtherPoly);
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
                switch (parsedTableName)
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
                    case "GeologicLines":
                        foreach (KeyValuePair<string, GeologicLinesAccess.GeologicLine> aRecord in (anEntry.Value as GeologicLinesAccess).GeologicLinesDictionary)
                        {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "OtherPolys":
                        foreach (KeyValuePair<string, OtherPolysAccess.OtherPoly> aRecord in (anEntry.Value as OtherPolysAccess).OtherPolysDictionary)
                        {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "OrientationPoints":
                        foreach (KeyValuePair<string, OrientationPointsAccess.OrientationPoint> aRecord in (anEntry.Value as OrientationPointsAccess).OrientationPointsDictionary)
                        {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "GenericSamples":
                        foreach (KeyValuePair<string, GenericSamplesAccess.GenericSample> aRecord in (anEntry.Value as GenericSamplesAccess).GenericSamplesDictionary)
                        {
                            sqlWhereClause += aRecord.Key + "' OR " + parsedTableName + "_ID = '";
                        }
                        break;
                    case "Stations":
                        foreach (KeyValuePair<string, StationsAccess.Station> aRecord in (anEntry.Value as StationsAccess).StationsDictionary)
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

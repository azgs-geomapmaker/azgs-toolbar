using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;

namespace ncgmpToolbar.Utilities.DatabaseMaintenance
{
    static class dataSourceChecker
    {
        public static void CheckForMissingDataSources(IWorkspace theWorkspace)
        {
            // This will create a list of unique DataSource entries, and check that each is available in the DataSources table.
            //  Should output a table detailing which DataSources there are, and which are not included in the DataSources table.

            // Make a collection to house all the unique DataSources
            System.Collections.ArrayList uniqueDs = new System.Collections.ArrayList();

            #region Top Level Datasets
            // Get the Dataset names for every dataset in the workspace.
            IEnumDataset workspaceDatasets = theWorkspace.get_Datasets(esriDatasetType.esriDTAny);
            uniqueDs = AddUniques(workspaceDatasets, uniqueDs);
            #endregion

            #region Check for Feature Datasets
            IEnumDataset featureDatasets = theWorkspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);

            IDataset aFeatureDataset = featureDatasets.Next();
            while (aFeatureDataset != null)
            {
                IFeatureDataset thisFeatureDs = (IFeatureDataset)aFeatureDataset;
                IEnumDataset internalDatasets = thisFeatureDs.Subsets;
                uniqueDs = AddUniques(internalDatasets, uniqueDs);   
                aFeatureDataset = featureDatasets.Next();                
            }
            #endregion

            #region Check which are in the DataSources Table
            Dictionary<string, bool> validatedIds = CheckEachEntry(uniqueDs, theWorkspace);
            #endregion

            #region Generate an output
            // Output to Debug window

            //Generate WHERE clause to select from AZGeology
            string whereClause = "SysGUID = '";

            foreach (KeyValuePair<string,bool> anId in validatedIds)
	        {
		        System.Diagnostics.Debug.WriteLine(anId.Key + ": " + ((anId.Value == false) ? "Not Available" : "In DataSources Table"));
                if (anId.Value == false) { whereClause += anId.Key + "' OR SysGUID = '"; }
            }

            if (whereClause == "SysGUID = '")
            {
                System.Diagnostics.Debug.WriteLine("All Data Sources Are Accounted For!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(whereClause.Remove(whereClause.Length - 15));
            }
            #endregion
        }

        private static System.Collections.ArrayList AddUniques(IEnumDataset fromTheseDatasets, System.Collections.ArrayList AppendTo = null)
        {
            if (AppendTo == null) { AppendTo = new System.Collections.ArrayList(); }

            IDataset aDs = fromTheseDatasets.Next();

            while (aDs != null)
            {
                // If this is a FeatureDataset, we need to 
                // Check to see if the dataset is a FeatureClass or a table. This is all we care about
                if ((aDs.Type == esriDatasetType.esriDTFeatureClass) || (aDs.Type == esriDatasetType.esriDTTable))
                {
                    // Cast the DS as an ITable
                    ITable thisTable = aDs as ITable;

                    // Find the Data Source field
                    int fldID = -1;
                    if (thisTable.FindField("DataSourceID") != -1) { fldID = thisTable.FindField("DataSourceID"); }
                    if (thisTable.FindField("DescriptionSourceID") != -1) { fldID = thisTable.FindField("DescriptionSourceID"); }
                    if (thisTable.FindField("DefinitionSourceID") != -1) { fldID = thisTable.FindField("DefinitionSourceID"); }
                    if (fldID == -1) { aDs = fromTheseDatasets.Next(); continue; }
                    IField dsField = thisTable.Fields.get_Field(fldID);

                    // Use the IDataStatistics interface to find unique values
                    IDataStatistics dataStats = new DataStatisticsClass();
                    dataStats.Cursor = thisTable.Search(null, false);
                    dataStats.Field = dsField.Name;
                    System.Collections.IEnumerator uniqueValues = dataStats.UniqueValues;

                    // Setup for iteration
                    uniqueValues.Reset();

                    // Add the unique values to the collection
                    try
                    {
                        uniqueValues.MoveNext();
                        while (uniqueValues.Current != null)
                        {
                            // Only add the value if it isn't already there...
                            if (!AppendTo.Contains(uniqueValues.Current.ToString())) { AppendTo.Add(uniqueValues.Current.ToString()); }
                            uniqueValues.MoveNext();
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                // Iterate to the next dataset
                aDs = fromTheseDatasets.Next();
            }

            return AppendTo;
        }

        private static Dictionary<string, bool> CheckEachEntry(System.Collections.ArrayList arrayOfIds, IWorkspace theWorkspace)
        {
            Dictionary<string, bool> output = new Dictionary<string, bool>();
            ITable DataSourcesTable = commonFunctions.OpenTable(theWorkspace, "DataSources");

            foreach (string anId in arrayOfIds)
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "DataSources_ID = '" + anId + "'";

                if (DataSourcesTable.RowCount(qf) == 0) { output.Add(anId, false); }
                else { output.Add(anId, true); }
            }

            return output;
        }
    }


    
}

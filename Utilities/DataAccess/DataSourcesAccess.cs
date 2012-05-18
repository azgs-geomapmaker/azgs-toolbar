using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Editor;
using ncgmpToolbar.Utilities;

namespace ncgmpToolbar.Utilities.DataAccess
{
    class DataSourcesAccess
    {
        // Class Constructor. Requires a Workspace reference in order to find the DataSources table.
        public DataSourcesAccess(IWorkspace theWorkspace) 
        {
            m_DataSourcesTable = commonFunctions.OpenTable(theWorkspace, "DataSources");
            m_theWorkspace = theWorkspace;
        }

        // The Datasource structure represents a single row in the DataSources table.
        public struct Datasource
        {
            public string DataSources_ID;
            public string Source;
            public string Notes;
            public bool RequiresUpdate;
        }

        // The m_dataSourceDictionary is the internal representation of a collection of Datasource structures
        //  It is also exposed as a public read-only property
        private Dictionary<string, Datasource> m_dataSourceDictionary = new Dictionary<string,Datasource>();
        public Dictionary<string, Datasource> DataSourceCollection 
        {
            get { return m_dataSourceDictionary; }
        }

        // Other Class-level variables for convenience
        ITable m_DataSourcesTable;
        IWorkspace m_theWorkspace;

        // The ClearDataSources method clears the collection
        public void ClearDataSources()
        {
            m_dataSourceDictionary.Clear();
        }

        // The AddDatasources method adds Datasource structures to the collection, based on a query defined by input parameters.
        public void AddDataSources(string SqlWhereClause)
        {
            // Get m_identifier indexes outside the loop for better performance
            int idFld = m_DataSourcesTable.FindField("DataSources_ID");
            int sourceFld = m_DataSourcesTable.FindField("Source");
            int notesFld = m_DataSourcesTable.FindField("Notes");
            
            // Setup the query
            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            // Perform the search, and grab the first returned row
            ICursor theCursor = m_DataSourcesTable.Search(QF,false);
            IRow theRow = theCursor.NextRow();

            // Loop through the returned rows until you're all done.
            while (theRow != null) 
            {
                // Populate a DataSource Structure
                Datasource aDataSource = new Datasource();
                aDataSource.DataSources_ID = theRow.get_Value(idFld).ToString();
                aDataSource.Source = theRow.get_Value(sourceFld).ToString();
                aDataSource.Notes = theRow.get_Value(notesFld).ToString();
                aDataSource.RequiresUpdate = true;

                // Add the Structure to the dictionary
                m_dataSourceDictionary.Add(aDataSource.DataSources_ID, aDataSource);

                // Increment to the next returned DataSource
                theRow = theCursor.NextRow();
            }
        }       

        // The NewDatasource method creates a new, blank Datasource Structure with a new ID.
        //  Other parameters are provided by the function call.
        //  Returns the ID of the new DataSource
        public string NewDataSource(string Source, string Notes)
        {
            // Create a Datasource structure
            Datasource newDataSource = new Datasource();

            // Attribute the Datasource structure
            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newDataSource.DataSources_ID = SysInfoTable.ProjAbbr + ".DataSources." + SysInfoTable.GetNextIdValue("DataSources");
            newDataSource.Source = Source;
            newDataSource.Notes = Notes;
            newDataSource.RequiresUpdate = false;

            // Add the Datasource to the collection
            m_dataSourceDictionary.Add(newDataSource.DataSources_ID, newDataSource);
            return newDataSource.DataSources_ID;
        }

        // The UpdateDataSource method takes a Datasource structure as a parameter. It looks for the particular Datasource in the collection
        //  and replaces it in the collection. 
        public void UpdateDataSource(Datasource theDataSource)
        {
            // Try to remove the Datasource in the collection - If the try fails, then the thing wasn't there in the first place
            try
                {
                    m_dataSourceDictionary.Remove(theDataSource.DataSources_ID);                    
                }
            catch { }

            // Add the new DataSource to the collection. Note: the update is not acutally saved until calling the SaveDataSources function
            theDataSource.RequiresUpdate = true;
            m_dataSourceDictionary.Add(theDataSource.DataSources_ID, theDataSource);            
        }

        // The SaveDataSource method saves changes to everything that is sitting in the collection.
        public void SaveDataSources()
        {
            // Get m_identifier indexes outside the loop for better performance
            int idFld = m_DataSourcesTable.FindField("DataSources_ID");
            int sourceFld = m_DataSourcesTable.FindField("Source");
            int notesFld = m_DataSourcesTable.FindField("Notes");

            // Wrap everything in an edit Operation - Must be in an edit session.
            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                // There will be one query for updates, while inserts will be handled through rowbuffers
                string updateWhereClause = "DataSources_ID = '";
                ICursor insertCursor = m_DataSourcesTable.Insert(true);

                foreach (KeyValuePair<string, Datasource> aDictionaryEntry in m_dataSourceDictionary)
                {
                    Datasource thisDataSource = (Datasource)aDictionaryEntry.Value;
                    switch (thisDataSource.RequiresUpdate)
                    {
                        case true:
                            // Expand the Update WhereClause
                            updateWhereClause += thisDataSource.DataSources_ID + "' OR DataSources_ID = '";
                            break;

                        case false:
                            // Create and populate a RowBuffer
                            IRowBuffer theRowBuffer = m_DataSourcesTable.CreateRowBuffer();
                            theRowBuffer.set_Value(idFld, thisDataSource.DataSources_ID);
                            theRowBuffer.set_Value(sourceFld, (thisDataSource.Source));
                            theRowBuffer.set_Value(notesFld, (thisDataSource.Notes));

                            // Insert the RowBuffer
                            insertCursor.InsertRow(theRowBuffer);
                            break;
                    }
                }

                // Finalize inserts by destroying the insert cursor, start another edit operation for the update
                //insertCursor = null;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("DataSource Management");
                theEditor.StartOperation();

                // Cleanup the Update WhereClause
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 22);

                // Setup the Update Query
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                // Grab the rows to update
                ICursor updateCursor = m_DataSourcesTable.Update(QF, false);
                IRow theRow = updateCursor.NextRow();

                // Loop through the rows to be updated
                while (theRow != null)
                {
                    // Get the ID
                    string theID = theRow.get_Value(idFld).ToString();

                    // Get the updated record from the Dictionary
                    Datasource thisDataSource = m_dataSourceDictionary[theID];

                    // Set appropriate values and perform the update
                    theRow.set_Value(sourceFld, (thisDataSource.Source));
                    theRow.set_Value(notesFld, (thisDataSource.Notes));
                    updateCursor.UpdateRow(theRow);

                    // Increment the row
                    theRow = updateCursor.NextRow();
                }

                // Close the edit operation
                theEditor.StopOperation("DataSource Management");
            }
            catch { theEditor.StopOperation("DataSource Management Failure"); }
        }
    }
}

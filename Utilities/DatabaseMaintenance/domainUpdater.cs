using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ncgmpToolbar.Utilities;

namespace ncgmpToolbar.Utilities.DatabaseMaintenance
{
    static class domainUpdater
    {
        public static void UpdateDomains(IWorkspace theWorkspace)
        {
            // Domains to update:
            // DataSources: d_DataSources
            // Station IDs: d_StationIDs
            
            // Every ncgmp database has a datasources table, right?!
            UpdateCodedValueDomain(theWorkspace, "d_DataSources", "DataSources", "DataSources_ID", "Source");

            // Update StationIDs if the database has that capability
            //if (ncgmpChecks.IsAzgsStationAddinPresent(theWorkspace) == true) { 
            UpdateCodedValueDomain(theWorkspace, "d_StationIDs", "Stations", "Stations_ID", "FieldID"); //}   

            IWorkspace2 theWorkspace2 = (IWorkspace2)theWorkspace;
            if (theWorkspace2.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(theWorkspace, "Landslides")) == true)
            {
                UpdateCodedValueDomain(theWorkspace, "d_LandslideIDs", "Landslides", "LandslideID", "Name");
            }


            if (theWorkspace2.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(theWorkspace, "Costs")) == true)
            {
                UpdateCodedValueDomain(theWorkspace, "d_CostIDs", "Costs", "CostID", "Damage");
            }
        }

        private static void UpdateCodedValueDomain(IWorkspace theWorkspace, string DomainName, string SourceClassName, string CodeFieldName, string ValueFieldName)
        {                    
            // Get reference to the table to read codes and values from
            ITable theTable = commonFunctions.OpenTable(theWorkspace, SourceClassName);

            // Get reference to the domain itself
            IWorkspaceDomains wsDomains = (IWorkspaceDomains)theWorkspace;
            ICodedValueDomain theDomain = (ICodedValueDomain)wsDomains.get_DomainByName(DomainName);

            // Requires exclusive schema lock
            ISchemaLock schemaLock = (ISchemaLock)theDomain;

            try
            {
                // Get an exclusive lock
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);

                // Clear everything out of the domain first
                int codedValues = theDomain.CodeCount;
                for (int i = 0; i <= codedValues - 1; i++) { theDomain.DeleteCode(theDomain.Value[0]); }

                // Sort the table
                ITableSort tableSorter = new TableSortClass();
                tableSorter.Fields = ValueFieldName;
                tableSorter.set_Ascending(ValueFieldName, true);
                tableSorter.Table = theTable;
                tableSorter.Sort(null);

                // Loop through the sorted rows, add to the domain
                int codeFld = theTable.FindField(CodeFieldName);
                int valueFld = theTable.FindField(ValueFieldName);

                ICursor theCursor = tableSorter.Rows;
                IRow theRow = theCursor.NextRow();

                while (theRow != null)
                {
                    theDomain.AddCode(theRow.get_Value(codeFld), theRow.get_Value(valueFld).ToString());
                    theRow = theCursor.NextRow();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(DomainName + " was not updated. This is likely because an exclusive schema lock could not be obtained.", "NCGMP Tools");
            }
            finally
            {
                // Release the exclusive lock, if it was acquired.
                //  This block of code (finally) is called whether or not there is a problem
                schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
            }
        }
    }
}

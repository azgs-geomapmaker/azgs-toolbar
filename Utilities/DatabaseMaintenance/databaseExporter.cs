using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.GeoDatabaseDistributed;

namespace ncgmpToolbar.Utilities.DatabaseMaintenance
{
    static class databaseExporter
    {
        public static void ExportDatabase(IWorkspace theWorkspace)
        {
            // Try and make sure the user understands what they're doing
            MessageBox.Show("You must first browse to a previously generated, empty geodatabase.", "NCGMP Tools");

            // Browse for a file, personal or SDE geodatabase
            IWorkspaceFactory wsFact = null;
            IWorkspace openedWorkspace = null;

            IGxObjectFilter objectFilter = new GxFilterWorkspaces();
            IGxObject openedObject = commonFunctions.OpenArcFile(objectFilter, "Please select an empty database");
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
            openedWorkspace = wsFact.OpenFromFile(pathToOpen, 0);

            // Okay, now export the current database to an XML doc
            string tempFilePath = System.IO.Path.GetTempFileName();
            tempFilePath += ".xml";

            IGdbXmlExport xmlExporter = new GdbExporterClass();
            xmlExporter.ExportWorkspace(theWorkspace, tempFilePath, true, false, false);

            // Import to the new database
            // Use the temp file to perform the import
            IGdbXmlImport xmlImporter = new GdbImporterClass();
            IEnumNameMapping enumNameMapping = null;
            bool conflictsFound = xmlImporter.GenerateNameMapping(tempFilePath, openedWorkspace, out enumNameMapping);

            try
            {
                // Deal with conflicts (lifted wholesale from http://help.arcgis.com/en/sdk/10.0/arcobjects_net/conceptualhelp/index.html#/d/00010000011m000000.htm)
                if (conflictsFound)
                {
                    IName workspaceName = ((IDataset)openedWorkspace).FullName;

                    // Iterate through each name mapping.
                    INameMapping nameMapping = null;
                    enumNameMapping.Reset();
                    while ((nameMapping = enumNameMapping.Next()) != null)
                    {
                        // Resolve the mapping's conflict (if there is one).
                        if (nameMapping.NameConflicts)
                        {
                            nameMapping.TargetName = nameMapping.GetSuggestedName(workspaceName);
                        }
                        // See if the mapping's children have conflicts.
                        IEnumNameMapping childEnumNameMapping = nameMapping.Children;
                        if (childEnumNameMapping != null)
                        {
                            childEnumNameMapping.Reset();
                            // Iterate through each child mapping.
                            INameMapping childNameMapping = null;
                            while ((childNameMapping = childEnumNameMapping.Next()) != null)
                            {
                                if (childNameMapping.NameConflicts)
                                {
                                    childNameMapping.TargetName = nameMapping.GetSuggestedName
                                        (workspaceName);
                                }
                            }
                        }
                    }
                }

                // Perform the import
                xmlImporter.ImportWorkspace(tempFilePath, enumNameMapping, openedWorkspace, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine + Environment.NewLine + "XML Workspace Doc written to: " + tempFilePath);
            }
            

            // Perhaps, strip the SysInfo table


        }
    }
}

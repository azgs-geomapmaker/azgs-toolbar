using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ncgmpToolbar.Utilities;
using ncgmpToolbar.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;

namespace ncgmpToolbar
{
    public class btnOpenNcgmpDatabase : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public btnOpenNcgmpDatabase()
        {
        }

        protected override void OnClick()
        {
            string getSqlDatabase = "False";
        findDatabase:
            // Check the registry
            string regValue = commonFunctions.ReadReg("Software\\NCGMPTools", "getSqlDatabase");
            if (regValue == null)
            {
                // There is no registry entry, open the chooser form
                sqlOrFileChooser chooserForm = new sqlOrFileChooser();
                chooserForm.ShowDialog();

                // After the form is closed, use the selected method and write the registry if applicable
                if (chooserForm.m_canceled == true) { return; }

                getSqlDatabase = chooserForm.m_getSqlDatabase;
                if (chooserForm.m_writeReg == true)
                {
                    commonFunctions.WriteReg("Software\\NCGMPTools", "getSqlDatabase", getSqlDatabase);
                }
            }
            else
            {
                // There is a registry entry, lead the user in the appropriate direction
                if (regValue == "True")
                {
                    getSqlDatabase = "True";
                }
            }
        
            // Find a Database
            IWorkspaceFactory wsFact = null;
            IWorkspace openedWorkspace = null;

            if (getSqlDatabase == "True")
            {
                // Open the form listing AZGS SQL Databases
                azgsSqlDatabaseChooser chooseDbForm = new azgsSqlDatabaseChooser();
                chooseDbForm.ShowDialog();
                if (chooseDbForm.databaseName == null) { return; }
                if (chooseDbForm.versionName == null) { return; }

                wsFact = new SdeWorkspaceFactoryClass();

                // Setup connection properties for the selected database
                IPropertySet connectionProperties = new PropertySetClass();
                connectionProperties.SetProperty("SERVER", "malachite\\azgsgeodatabases");
                connectionProperties.SetProperty("INSTANCE", "sde:sqlserver:malachite\\azgsgeodatabases");
                connectionProperties.SetProperty("DATABASE", chooseDbForm.databaseName);
                connectionProperties.SetProperty("AUTHENTICATION_MODE", "OSA");
                connectionProperties.SetProperty("VERSION", "DBO." + chooseDbForm.versionName);

                openedWorkspace = wsFact.Open(connectionProperties, 0);
            }
            else
            {
                // Browse for a file, personal or SDE geodatabase
                IGxObjectFilter objectFilter = new GxFilterWorkspaces();
                IGxObject openedObject = commonFunctions.OpenArcFile(objectFilter, "Please select an NCGMP database");
                if (openedObject == null) { return;  }

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
                        pathToOpen = remoteDatabaseFolder.Path + "\\" + openedObject.Name;
                        break;
                    default:
                        break;
                }

                try
                {
                    openedWorkspace = wsFact.OpenFromFile(pathToOpen, 0);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            
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

            bool isTopologyUsed = ncgmpChecks.IsTopologyUsed(openedWorkspace);
            bool hasStationTables = ncgmpChecks.IsAzgsStationAddinPresent(openedWorkspace);
            bool hasStandardLithtables = ncgmpChecks.IsStandardLithAddinPresent(openedWorkspace);
            bool hasRepresentations = ncgmpChecks.AreRepresentationsUsed(openedWorkspace);

            // Add FeatureClasses and tables to the ArcMap Project
            AddLayersToMap(openedWorkspace, isTopologyUsed, hasStationTables, hasRepresentations);

            // Populate the Data Sources combobox

        }

        protected override void OnUpdate()
        {
        }

        private void AddLayersToMap(IWorkspace ValidNcgmpDatabase, bool hasTopologyStuff, bool hasStationFunction, bool useRepresentation)
        {
            // Get references to the map for adding layers and tables
            IMxDocument MxDoc = (IMxDocument)ArcMap.Document;
            IMap thisMap = MxDoc.FocusMap;
            IStandaloneTableCollection thisMapTables = (IStandaloneTableCollection)thisMap;

            // Create a group layer
            IGroupLayer GeoMapGroupLayer = new GroupLayerClass();
            GeoMapGroupLayer.Name = "Geologic Map";

            if (hasTopologyStuff == true)
            {
                #region "GeologicMapTopology"
                ITopology geoMapTopo = commonFunctions.OpenTopology(ValidNcgmpDatabase,"GeologicMapTopology");
                ITopologyLayer geoMapTopoTL = new TopologyLayerClass();
                geoMapTopoTL.Topology = geoMapTopo;

                ILayer geoMapTopoL = (ILayer)geoMapTopoTL;
                geoMapTopoL.Name = "Geologic Map Topology";
                geoMapTopoL.Visible = false;

                // Minimizing the legend info in the Table of Contents is not trivial
                ILegendInfo geoMapTopoLegendInfo = (ILegendInfo)geoMapTopoL;
                ILegendGroup geoMapTopoLegendGroup = geoMapTopoLegendInfo.get_LegendGroup(0);
                geoMapTopoLegendGroup.Visible = false;
                geoMapTopoLegendGroup = geoMapTopoLegendInfo.get_LegendGroup(1);
                geoMapTopoLegendGroup.Visible = false;
                geoMapTopoLegendGroup = geoMapTopoLegendInfo.get_LegendGroup(2);
                geoMapTopoLegendGroup.Visible = false;

                GeoMapGroupLayer.Add(geoMapTopoL);

                #endregion
            }                

            if (hasStationFunction == true)
            {
                // Create a Group layer
                IGroupLayer stationGroupLayer = new GroupLayerClass();
                stationGroupLayer.Name = "Station Data";

                #region "StationPoints"
                // Open a FeatureClass, set it to a FeatureLayer
                IFeatureClass stationFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "StationPoints");
                IFeatureLayer stationFL = new FeatureLayerClass();
                stationFL.FeatureClass = stationFC;
                
                // Configure the FeatureLayer
                stationFL.Name = "Stations";
                stationFL.DisplayField = "FieldID";
                
                // Collapse the legend for this layer
                ILegendInfo stationLegendInfo = (ILegendInfo)stationFL;
                ILegendGroup stationLegendGroup = stationLegendInfo.get_LegendGroup(0);
                stationLegendGroup.Visible = false;

                // Finally, add the layer to the group layer
                stationGroupLayer.Add(stationFL);

                #endregion

                // Repeat for all these FeatureClasses
                #region "SamplePoints"
                IFeatureClass sampleFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "SamplePoints");
                IFeatureLayer sampleFL = new FeatureLayerClass();
                sampleFL.FeatureClass = sampleFC;

                sampleFL.Name = "Samples";
                sampleFL.DisplayField = "FieldID";

                ILegendInfo sampleLegendInfo = (ILegendInfo)sampleFL;
                ILegendGroup sampleLegendGroup = sampleLegendInfo.get_LegendGroup(0);
                sampleLegendGroup.Visible = false;

                stationGroupLayer.Add(sampleFL);

                #endregion

                #region "OrientationDataPoints"
                IFeatureClass structureFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "OrientationDataPoints");
                IFeatureLayer structureFL = new FeatureLayerClass();
                structureFL.FeatureClass = structureFC;

                structureFL.Name = "Orientation Data";
                structureFL.DisplayField = "Type";

                // Symbology, if representations are present
                if (useRepresentation == true)
                {
                    // FeatureLayer must be cast as a GeoFeatureLayer in order to access the Renderer
                    IGeoFeatureLayer structureGeoFL = (IGeoFeatureLayer)structureFL;
                    
                    // Create a RepresentationClassRenderer, assign the appropriate RepresentationClass to it
                    IRepresentationRenderer structureRepRend = new RepresentationRendererClass();
                    structureRepRend.RepresentationClass = commonFunctions.GetRepClass(ValidNcgmpDatabase, "r_OrientationDataPoints");

                    // Assign the RepresentationClassRenderer to the GeoFeatureLayer
                    structureGeoFL.Renderer = (IFeatureRenderer)structureRepRend;

                    // Assign generic FeatureTemplates
                    commonFunctions.BuildGenericTemplates(ValidNcgmpDatabase, structureFL as ILayer, "OrientationDataPoints");
                }

                ILegendInfo structureLegendInfo = (ILegendInfo)structureFL;
                ILegendGroup structureLegendGroup = structureLegendInfo.get_LegendGroup(0);
                structureLegendGroup.Visible = false;

                stationGroupLayer.Add(structureFL);
                
                #endregion

                // Add the Group Layer to the main layer
                stationGroupLayer.Expanded = true;
                GeoMapGroupLayer.Add(stationGroupLayer);

                #region "Notes Table"
                ITable notesTable = commonFunctions.OpenTable(ValidNcgmpDatabase, "Notes");
                IStandaloneTable notesStandalone = new StandaloneTableClass();
                notesStandalone.Table = notesTable;

                notesStandalone.Name = "Notes";
                notesStandalone.DisplayField = "Type";

                thisMapTables.AddStandaloneTable(notesStandalone);

                #endregion

                #region "RelatedDocuments Table"
                ITable relatedDocsTable = commonFunctions.OpenTable(ValidNcgmpDatabase, "RelatedDocuments");
                IStandaloneTable relatedDocsStandalone = new StandaloneTableClass();
                relatedDocsStandalone.Table = relatedDocsTable;

                relatedDocsStandalone.Name = "Related Documents";
                relatedDocsStandalone.DisplayField = "Type";

                thisMapTables.AddStandaloneTable(relatedDocsStandalone);

                #endregion
            }

            if (hasTopologyStuff == true)
            {
                #region "OtherLines"
                IFeatureClass otherLinesFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "OtherLines");
                IFeatureLayer otherLinesFL = new FeatureLayerClass();
                otherLinesFL.FeatureClass = otherLinesFC;

                otherLinesFL.Name = "Other Lines";
                otherLinesFL.DisplayField = "Type";

                if (useRepresentation == true)
                {
                    IGeoFeatureLayer otherLinesGeoFL = (IGeoFeatureLayer)otherLinesFL;

                    IRepresentationRenderer otherLinesRepRend = new RepresentationRendererClass();
                    otherLinesRepRend.RepresentationClass = commonFunctions.GetRepClass(ValidNcgmpDatabase, "r_OtherLines");

                    otherLinesGeoFL.Renderer = (IFeatureRenderer)otherLinesRepRend;

                    commonFunctions.BuildGenericTemplates(ValidNcgmpDatabase, otherLinesFL as ILayer, "OtherLines");
                }

                ILegendInfo otherLinesLegendInfo = (ILegendInfo)otherLinesFL;
                ILegendGroup otherLinesLegendGroup = otherLinesLegendInfo.get_LegendGroup(0);
                otherLinesLegendGroup.Visible = false;

                GeoMapGroupLayer.Add(otherLinesFL);

                #endregion                
                     
            }

            #region "ContactsAndFaults"
            IFeatureClass contactsAndFaultsFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "ContactsAndFaults");
            IFeatureLayer contactsAndFaultsFL = new FeatureLayerClass();
            contactsAndFaultsFL.FeatureClass = contactsAndFaultsFC;

            contactsAndFaultsFL.Name = "Contacts and Faults";
            contactsAndFaultsFL.DisplayField = "Type";

            if (useRepresentation == true)
            {
                // Set the layer renderer to use representations
                IGeoFeatureLayer contactsAndFaultsGeoFL = (IGeoFeatureLayer)contactsAndFaultsFL;

                IRepresentationRenderer contactsAndFaultsRepRend = new RepresentationRendererClass();
                contactsAndFaultsRepRend.RepresentationClass = commonFunctions.GetRepClass(ValidNcgmpDatabase, "r_ContactsAndFaults");

                contactsAndFaultsGeoFL.Renderer = (IFeatureRenderer)contactsAndFaultsRepRend;

                commonFunctions.BuildGenericTemplates(ValidNcgmpDatabase, contactsAndFaultsFL as ILayer, "ContactsAndFaults");
            }

            ILegendInfo contactsAndFaultsLegendInfo = (ILegendInfo)contactsAndFaultsFL;
            ILegendGroup contactsAndFaultsLegendGroup = contactsAndFaultsLegendInfo.get_LegendGroup(0);
            contactsAndFaultsLegendGroup.Visible = false;

            GeoMapGroupLayer.Add(contactsAndFaultsFL);

            #endregion

            if (hasTopologyStuff == true)
            {
                #region "OverlayPolys"
                IFeatureClass overlayPolysFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "OverlayPolys");
                IFeatureLayer overlayPolysFL = new FeatureLayerClass();
                overlayPolysFL.FeatureClass = overlayPolysFC;

                overlayPolysFL.Name = "Overlay Polygons";
                overlayPolysFL.DisplayField = "MapUnit";

                ILegendInfo overlayPolysLegendInfo = (ILegendInfo)overlayPolysFL;
                ILegendGroup overlayPolysLegendGroup = overlayPolysLegendInfo.get_LegendGroup(0);
                overlayPolysLegendGroup.Visible = false;

                GeoMapGroupLayer.Add(overlayPolysFL);

                #endregion
            }

            #region "MapUnitPolys"
            IFeatureClass mapUnitPolysFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "MapUnitPolys");
            IFeatureLayer mapUnitPolysFL = new FeatureLayerClass();
            mapUnitPolysFL.FeatureClass = mapUnitPolysFC;

            mapUnitPolysFL.Name = "Distribution of Map Units";
            mapUnitPolysFL.DisplayField = "MapUnit";

            ILegendInfo mapUnitPolysLegendInfo = (ILegendInfo)mapUnitPolysFL;
            ILegendGroup mapUnitPolysLegendGroup = mapUnitPolysLegendInfo.get_LegendGroup(0);
            mapUnitPolysLegendGroup.Visible = false;

            GeoMapGroupLayer.Add(mapUnitPolysFL);
            #endregion

            if (hasTopologyStuff == true)
            {
                #region "DataSourcePolys"
                IFeatureClass dataSourcePolysFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "DataSourcePolys");
                IFeatureLayer dataSourcePolysFL = new FeatureLayerClass();
                dataSourcePolysFL.FeatureClass = dataSourcePolysFC;

                dataSourcePolysFL.Name = "Data Source Polys";
                dataSourcePolysFL.DisplayField = "DataSourceID";

                ILegendInfo dataSourcePolysLegendInfo = (ILegendInfo)dataSourcePolysFL;
                ILegendGroup dataSourcePolysLegendGroup = dataSourcePolysLegendInfo.get_LegendGroup(0);
                dataSourcePolysLegendGroup.Visible = false;

                GeoMapGroupLayer.Add(dataSourcePolysFL);

                #endregion
            }

            // Add the Geologic Map Group Layer to the map
            GeoMapGroupLayer.Expanded = true;
            thisMap.AddLayer(GeoMapGroupLayer);

            // Adjust the MapUnitPolys Renderer
            commonFunctions.UpdateMapUnitPolysRenderer(ValidNcgmpDatabase);

            // Adjust the MapUnitPolys Feature Templates
            commonFunctions.UpdateMapUnitPolysFeatureTemplates(ValidNcgmpDatabase);
        }
    }
}

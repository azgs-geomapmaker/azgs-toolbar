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

        findDatabase:
            // Check the registry       
            // Find a Database
            IWorkspaceFactory wsFact = null;
            IWorkspace openedWorkspace = null;

            // Browse for a file, personal or SDE geodatabase
            IGxObjectFilter objectFilter = new GxFilterWorkspaces();
            IGxObject openedObject = commonFunctions.OpenArcFile(objectFilter, "Please select an NCGMP database");
            if (openedObject == null) { return;  }

            // Check to see if it is a File, Personal or SDE database, create appropriate workspace factory
            string pathToOpen = null;
            IGxRemoteDatabaseFolder remoteDatabaseFolder;

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
                    remoteDatabaseFolder = (IGxRemoteDatabaseFolder)openedObject.Parent;
                    pathToOpen = remoteDatabaseFolder.Path + "\\" + openedObject.Name;
                    break;
                case "Database Connection":
                    wsFact = new SdeWorkspaceFactoryClass();
                    remoteDatabaseFolder = (IGxRemoteDatabaseFolder)openedObject.Parent;
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
            
            // Check to see if the database is valid NCGMP
            bool isValid = ncgmpChecks.IsWorkspaceMinNCGMPCompliant(openedWorkspace);
            if (isValid == false)
            {
                MessageBox.Show("The selected database is not a valid NCGMP database.", "NCGMP Toolbar");
                goto findDatabase;
            }
            //else
            //{
            //    isValid = ncgmpChecks.IsSysInfoPresent(openedWorkspace);
            //    if (isValid == false)
            //    {
            //        MessageBox.Show("In order to use these tools, the NCGMP database must contain a SysInfo table.", "NCGMP Toolbar");
            //        goto findDatabase;
            //    }
            //}

            //bool isTopologyUsed = ncgmpChecks.IsTopologyUsed(openedWorkspace);
            //bool hasStationTables = ncgmpChecks.IsAzgsStationAddinPresent(openedWorkspace);
            //bool hasStandardLithtables = ncgmpChecks.IsStandardLithAddinPresent(openedWorkspace);
            bool hasRepresentations = ncgmpChecks.AreRepresentationsUsed(openedWorkspace);

            // Add FeatureClasses and tables to the ArcMap Project
            AddLayersToMap(openedWorkspace, hasRepresentations); // isTopologyUsed, hasStationTables, 


            // Populate the Data Sources combobox

        }

        protected override void OnUpdate()
        {
        }

        private void AddLayersToMap(IWorkspace ValidNcgmpDatabase, bool useRepresentation) // bool hasTopologyStuff, bool hasStationFunction, 
        {
            // Get references to the map for adding layers and tables
            IMxDocument MxDoc = (IMxDocument)ArcMap.Document;
            IMap thisMap = MxDoc.FocusMap;
            IStandaloneTableCollection thisMapTables = (IStandaloneTableCollection)thisMap;

            // Create a group layer
            IGroupLayer GeoMapGroupLayer = new GroupLayerClass();
            GeoMapGroupLayer.Name = "Geologic Map";

            // Create a group layer
            IGroupLayer StationGroupLayer = new GroupLayerClass();
            StationGroupLayer.Name = "Observation Data";

            //if (hasTopologyStuff == true)
            {
                #region "GeologicMapTopology"
                ITopology geoMapTopo = commonFunctions.OpenTopology(ValidNcgmpDatabase, "GeologicMapTopology");
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
            {
                #region "OrientationPoints"
                IFeatureClass orientationPointsFC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, "OrientationPoints");
                IFeatureLayer orientationPointsFL = new FeatureLayerClass();
                orientationPointsFL.FeatureClass = orientationPointsFC;

                orientationPointsFL.Name = "Orientation Points";
                orientationPointsFL.DisplayField = "Type";

                if (useRepresentation == true)
                {
                    // Set the layer renderer to use representations
                    IGeoFeatureLayer orientationPointsGeoFL = (IGeoFeatureLayer)orientationPointsFL;

                    IRepresentationRenderer orientationPointsRepRend = new RepresentationRendererClass();
                    orientationPointsRepRend.RepresentationClass = commonFunctions.GetRepClass(ValidNcgmpDatabase, "OrientationPoints_Rep");

                    orientationPointsGeoFL.Renderer = (IFeatureRenderer)orientationPointsRepRend;

                    commonFunctions.BuildGenericTemplates(ValidNcgmpDatabase, orientationPointsFL as ILayer, "OrientationPoints");
                }

                ILegendInfo orientationPointsLegendInfo = (ILegendInfo)orientationPointsFL;
                ILegendGroup orientationPointsLegendGroup = orientationPointsLegendInfo.get_LegendGroup(0);
                orientationPointsLegendGroup.Visible = false;

                StationGroupLayer.Add(orientationPointsFL);

                #endregion
                
                #region "ObservationData"
                string[] arr = new string[4]; // Initialize
                arr[0] = "Stations";               // Element 1
                arr[1] = "GenericPoints";               // Element 2
                arr[2] = "GenericSamples";             // Element 3
                arr[3] = "GeochronPoints";              // Element 4

                foreach (string s in arr)
                {
                    IFeatureClass FC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, s);
                    IFeatureLayer FL = new FeatureLayerClass();
                    FL.FeatureClass = FC;

                    FL.Name = s;
                    FL.Visible = false;

                    ILegendInfo LegendInfo = (ILegendInfo)FL;
                    ILegendGroup LegendGroup = LegendInfo.get_LegendGroup(0);
                    LegendGroup.Visible = false;

                    StationGroupLayer.Add(FL);
                }

                #endregion
                
            }

            //add station group layer to map document
            GeoMapGroupLayer.Add(StationGroupLayer);

            { 
                #region "LinesWithoutRepresentations"
                string[] arr = new string[2]; // Initialize
                arr[0] = "CartographicLines";               // Element 1
                arr[1] = "IsoValueLines";               // Element 2

                foreach (string s in arr)
                {
                    IFeatureClass FC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, s);
                    IFeatureLayer FL = new FeatureLayerClass();
                    FL.FeatureClass = FC;

                    FL.Name = s;
                    FL.Visible = false;

                    ILegendInfo LegendInfo = (ILegendInfo)FL;
                    ILegendGroup LegendGroup = LegendInfo.get_LegendGroup(0);
                    LegendGroup.Visible = false;

                    GeoMapGroupLayer.Add(FL);
                }

                #endregion
            }
            {
                #region "LinesWithRepresentations"
                string[] arr = new string[2]; // Initialize
                arr[0] = "GeologicLines";               // Element 1
                arr[1] = "ContactsAndFaults";               // Element 2

                foreach (string s in arr)
                {
                    IFeatureClass FC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, s);
                    IFeatureLayer FL = new FeatureLayerClass();
                    FL.FeatureClass = FC;

                    FL.Name = s;

                    if (useRepresentation == true)
                    {
                        // Set the layer renderer to use representations
                        IGeoFeatureLayer GeoFL = (IGeoFeatureLayer)FL;

                        IRepresentationRenderer RepRend = new RepresentationRendererClass();
                        RepRend.RepresentationClass = commonFunctions.GetRepClass(ValidNcgmpDatabase, s + "_Rep");

                        GeoFL.Renderer = (IFeatureRenderer)RepRend;

                        commonFunctions.BuildGenericTemplates(ValidNcgmpDatabase, FL as ILayer, s);
                    }

                    ILegendInfo LegendInfo = (ILegendInfo)FL;
                    ILegendGroup LegendGroup = LegendInfo.get_LegendGroup(0);
                    LegendGroup.Visible = false;

                    GeoMapGroupLayer.Add(FL);
                }

                #endregion
            }
            {
                #region "Polygons"
                string[] arr = new string[3]; // Initialize
                arr[0] = "DataSourcePolys";               // Element 1
                arr[1] = "MapUnitPolys";               // Element 2
                arr[2] = "OtherPolys";               // Element 3

                foreach (string s in arr)
                {
                    IFeatureClass FC = commonFunctions.OpenFeatureClass(ValidNcgmpDatabase, s);
                    IFeatureLayer FL = new FeatureLayerClass();
                    FL.FeatureClass = FC;

                    FL.Name = s;

                    ILegendInfo LegendInfo = (ILegendInfo)FL;
                    ILegendGroup LegendGroup = LegendInfo.get_LegendGroup(0);
                    LegendGroup.Visible = false;

                    GeoMapGroupLayer.Add(FL);
                }

                #endregion
            }
            {
                #region "Tables"
                string[] arr = new string[7]; // Initialize
                arr[0] = "DataSources";               // Element 1
                arr[1] = "DescriptionOfMapUnits";               // Element 2
                arr[2] = "ExtendedAttributes";             // Element 3
                arr[3] = "GeologicEvents";              // Element 4
                arr[4] = "Glossary";              // Element 5
                arr[5] = "Notes";              // Element 6
                arr[6] = "StandardLithology";              // Element 7
                foreach (string s in arr)
                {
                    ITable Table = commonFunctions.OpenTable(ValidNcgmpDatabase, s);
                    IStandaloneTable Standalone = new StandaloneTableClass();
                    Standalone.Table = Table;

                    Standalone.Name = s;

                    thisMapTables.AddStandaloneTable(Standalone);
                }

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

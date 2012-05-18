using System;
using Microsoft.Win32; 
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Editor;
using ncgmpToolbar.Utilities.DataAccess;

namespace ncgmpToolbar.Utilities
{
    public class commonFunctions
    {
        #region "Registry Manipulation"

        public static void WriteReg(string Path, string Name, string Value)
        {
            RegistryKey theKey = Registry.CurrentUser.CreateSubKey(Path);
            theKey.SetValue(Name, Value);
        }

        public static string ReadReg(string Path, string Name)
        {
            RegistryKey theKey = Registry.CurrentUser.CreateSubKey(Path);
            string theValue = (string)theKey.GetValue(Name);
            if (theValue == null)
            {
                return null;
            }
            else 
            { 
                return theValue; 
            }

        }

        #endregion

        public static IGxObject OpenArcFile(IGxObjectFilter objectFilter, string Caption)
        {
            IGxDialog fileChooser = new GxDialogClass();
            IEnumGxObject chosenFiles = null;

            fileChooser.Title = Caption;
            fileChooser.ButtonCaption = "Select";
            fileChooser.AllowMultiSelect = false;
            fileChooser.ObjectFilter = objectFilter;
            fileChooser.DoModalOpen(0, out chosenFiles);

            chosenFiles.Reset();
            return chosenFiles.Next();
        }

        public static ITable OpenTable(IWorkspace TheWorkspace, string TableName)
        {
             IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)TheWorkspace;
            string theTable = QualifyClassName(TheWorkspace, TableName);

            return featureWorkspace.OpenTable(theTable);
        }

        public static IFeatureClass OpenFeatureClass(IWorkspace TheWorkspace, string FeatureClassName)
        {
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)TheWorkspace;
            string theFC = QualifyClassName(TheWorkspace, FeatureClassName);

            return featureWorkspace.OpenFeatureClass(theFC);
        }

        public static IRelationshipClass OpenRelationshipClass(IWorkspace TheWorkspace, string RelationshipClassName)
        {
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)TheWorkspace;
            return featureWorkspace.OpenRelationshipClass(commonFunctions.QualifyClassName(TheWorkspace, RelationshipClassName));
        }

        public static ITopology OpenTopology(IWorkspace TheWorkspace, string TopologyName)
        {
            ITopologyWorkspace topoWorkspace = (ITopologyWorkspace)TheWorkspace;
            return topoWorkspace.OpenTopology(commonFunctions.QualifyClassName(TheWorkspace, TopologyName));
        }

        public static IRepresentationWorkspaceExtension GetRepExtension(IWorkspace Workspace)
        {
            IWorkspaceExtensionManager ExtManager = (IWorkspaceExtensionManager)Workspace;
            UID theUID = new UIDClass();
            theUID.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            return (IRepresentationWorkspaceExtension)ExtManager.FindExtension(theUID);
        }

        public static IRepresentationClass GetRepClass(IWorkspace Workspace, string RepresentationClassName)
        {
            // Get the RepresentationWorkspaceExtension from the Workspace
            IRepresentationWorkspaceExtension RepWsExt = GetRepExtension(Workspace);

            // Get and return the RepresentationClass
            return RepWsExt.OpenRepresentationClass(commonFunctions.QualifyClassName(Workspace, RepresentationClassName));
        }

        public static string QualifyClassName(IWorkspace theWorkspace, string givenClassName)
        {
            if (theWorkspace.Type == esriWorkspaceType.esriRemoteDatabaseWorkspace)
            {
                string dbName = ((IDataset)theWorkspace).Name;

                // ------- TROUBLE ---------
                // I don't know how to handle situations where the owner is not DBO.
                // ------- TROUBLE ---------
                string owner = "DBO";

                ISQLSyntax Qualifier = (ISQLSyntax)theWorkspace;
                return Qualifier.QualifyTableName(dbName, owner, givenClassName);
            }
            else { return givenClassName; }            
        }

        public static string GetCurrentDataSourceID()
        {
            return globalVariables.currentDataSource;
        }

        public static IFeatureLayer FindFeatureLayer(IWorkspace theWorkspace, string theFeatureClassName)
        {
            // First get a reference to the MapUnitPolys FeatureClass
            IFeatureClass theFC = OpenFeatureClass(theWorkspace, theFeatureClassName);

            // Cycle through ToC, checking for layer using that featureclass
            UID featureLayerUid = new UIDClass();
            featureLayerUid.Value = "{40A9E885-5533-11D0-98BE-00805F7CED21}";

            IEnumLayer theseLayers = ArcMap.Document.FocusMap.get_Layers(featureLayerUid, true);
            ILayer thisFeatureLayer = theseLayers.Next();

            while (thisFeatureLayer != null)
            {
                if (((IFeatureLayer)thisFeatureLayer).FeatureClass.Equals(theFC)) { return (IFeatureLayer)thisFeatureLayer; }
                thisFeatureLayer = theseLayers.Next();
            }

            // No Layer was found
            return null;
        }

        public static void BuildGenericTemplates(IWorkspace theWorkspace, ILayer FeatureClassLayer, string FeatureClassName)
        {
            // Get reference to the template manager, remove current templates
            IEditor3 templateEditor = ArcMap.Editor as IEditor3;
            templateEditor.RemoveAllTemplatesInLayer(FeatureClassLayer);

            // Setup the template array and the factory for making templates
            IArray templateArray = new ArrayClass();
            IEditTemplateFactory templateFactory = new EditTemplateFactoryClass();
            IEditTemplate newTemplate;

            // Generic legend generation depends on what class is being passed in
            //  Note - this routine assumes representations are in use.

            switch (FeatureClassName)
            {
                case "ContactsAndFaults":
                    // Accurate Contact
                    newTemplate = templateFactory.Create("Accurate Contact", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Accurate Contact", false);
                    newTemplate.SetDefaultValue("Symbol", 1, false);
                    templateArray.Add(newTemplate);

                    // Approximate Contact
                    newTemplate = templateFactory.Create("Approximate Contact", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Approximate Contact", false);
                    newTemplate.SetDefaultValue("Symbol", 3, false);
                    templateArray.Add(newTemplate);

                    // Gradational Contact
                    newTemplate = templateFactory.Create("Gradational Contact", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Gradational Contact", false);
                    newTemplate.SetDefaultValue("Symbol", 17, false);
                    templateArray.Add(newTemplate);

                    // Accurate Fault
                    newTemplate = templateFactory.Create("Accurate Fault", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Accurate Fault", false);
                    newTemplate.SetDefaultValue("Symbol", 118, false);
                    templateArray.Add(newTemplate);

                    // Approximate Fault
                    newTemplate = templateFactory.Create("Approximate Fault", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Approximate Fault", false);
                    newTemplate.SetDefaultValue("Symbol", 120, false);
                    templateArray.Add(newTemplate);
                    
                    // Concealed Contact
                    newTemplate = templateFactory.Create("Concealed Contact", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact", false);
                    newTemplate.SetDefaultValue("IsConcealed", 1, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Concealed Contact", false);
                    newTemplate.SetDefaultValue("Symbol", 265, false);
                    templateArray.Add(newTemplate);

                    // Concealed Fault
                    newTemplate = templateFactory.Create("Concealed Fault", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 1, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Concealed Fault", false);
                    newTemplate.SetDefaultValue("Symbol", 273, false);
                    templateArray.Add(newTemplate);
                    break;

                case "OtherLines":
                    // Dike
                    newTemplate = templateFactory.Create("Accurate Dike", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Dike", false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Accurate Dike", false);
                    newTemplate.SetDefaultValue("Symbol", 82, false);
                    templateArray.Add(newTemplate);

                    // Anticline
                    newTemplate = templateFactory.Create("Accurate Anticline", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fold", false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Accurate Anticline", false);
                    newTemplate.SetDefaultValue("Symbol", 121, false);
                    templateArray.Add(newTemplate);

                    // Syncline
                    newTemplate = templateFactory.Create("Accurate Syncline", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fold", false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Label", "Accurate Syncline", false);
                    newTemplate.SetDefaultValue("Symbol", 233, false);
                    templateArray.Add(newTemplate);
                    break;

                case "OrientationDataPoints":
                    break;
            }

            // Add the templates to the editor
            templateEditor.AddTemplates(templateArray);

            // Update the Doc
            ArcMap.Document.UpdateContents();
            

        }

        public static void UpdateMapUnitPolysRenderer(IWorkspace theWorkspace)
        {
            // Get the MapUnitPolys Layer from the ToC
            IFeatureLayer theLayer = commonFunctions.FindFeatureLayer(theWorkspace, "MapUnitPolys");

            // If the layer was not found, don't try and update the renderer!
            if (theLayer == null) { return; }

            // Setup the renderer
            IUniqueValueRenderer theRenderer = new UniqueValueRendererClass();
            theRenderer.FieldCount = 1;
            theRenderer.Field[0] = "MapUnit";            

            // Setup a couple variables
            IColor nullColor = new RgbColorClass();
            nullColor.NullColor = true;

            // Setup a blank line
            ILineSymbol nullLine = new SimpleLineSymbolClass();
            nullLine.Color = nullColor;

            // Setup the "All Other Values" symbol
            ISimpleFillSymbol defaultSymbol = new SimpleFillSymbolClass();
            IColor defaultColor = new RgbColorClass();
            defaultColor.RGB = 255;
            defaultSymbol.Color = defaultColor;
            defaultSymbol.Outline = nullLine;

            // Apply the "All other values" symbol to the renderer
            theRenderer.DefaultSymbol = defaultSymbol as ISymbol;
            theRenderer.UseDefaultSymbol = true;

            string theHeading = "Geologic Map Units";

            // Get the Legend Items
            var sortedDmu = GetDmuSortedByHierarchy(theWorkspace);

            // Loop through the legend items
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> legendEntry in sortedDmu)
            {
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry = legendEntry.Value;

                // If this is a heading, make it so
                if (thisDmuEntry.ParagraphStyle.Contains("Heading") == true)
                {
                    theHeading = thisDmuEntry.Name;
                    continue;
                }

                // Make a Simple Fill Symbol
                ISimpleFillSymbol theSymbol = new SimpleFillSymbolClass();
                //theSymbol.Style = esriSimpleFillStyle.esriSFSSolid;

                // Get the right color
                IColor symbolColor = new RgbColorClass();
                string rgbString = thisDmuEntry.AreaFillRGB;
                string[] rgbValues = rgbString.Split(';');

                // Long integer representations of RGB values are dumb: G*65536 + B*256 + R
                if (rgbValues.Length < 3) { symbolColor.RGB = 16777215; }
                else { symbolColor.RGB = int.Parse(rgbValues[0]) + int.Parse(rgbValues[1]) * 256 + int.Parse(rgbValues[2]) * 65536; }
                theSymbol.Color = symbolColor;

                theSymbol.Outline = nullLine;

                // Add it to the renderer
                theRenderer.AddValue(thisDmuEntry.MapUnit, theHeading, theSymbol as ISymbol);

                // Give it the right label
                theRenderer.Label[thisDmuEntry.MapUnit] = thisDmuEntry.MapUnit + " - " + thisDmuEntry.Name;
            }

            // Apply the renderer
            IGeoFeatureLayer geoFLayer = (IGeoFeatureLayer)theLayer;
            geoFLayer.Renderer = (IFeatureRenderer)theRenderer;

            // Minimizing the legend info in the Table of Contents is not trivial
            ILegendInfo layerLegendInfo = (ILegendInfo)theLayer;
            for (int i = 0; i < layerLegendInfo.LegendGroupCount; i++)
            {
                ILegendGroup layerLegendGroup = layerLegendInfo.get_LegendGroup(i);
                layerLegendGroup.Visible = false;
            }

            // Update the views
            ArcMap.Document.UpdateContents();
            ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, theLayer, null);
        }

        public static void UpdateMapUnitPolysFeatureTemplates(IWorkspace theWorkspace)
        {
            // Get the MapUnitPolys Layer from the ToC
            IFeatureLayer theLayer = commonFunctions.FindFeatureLayer(theWorkspace, "MapUnitPolys");

            // If the layer was not found, don't try and update the templates!
            if (theLayer == null) { return; }

            // Get a reference to the template editor, remove current templates
            IEditor3 templateEditor = (IEditor3)ArcMap.Editor;
            templateEditor.RemoveAllTemplatesInLayer((ILayer)theLayer);

            // Get the DMU entries
            var sortedDmu = GetDmuSortedByHierarchy(theWorkspace);

            // Loop through the DMU, add templates to an array
            IArray templateArray = new ArrayClass();
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> anEntry in sortedDmu)
            {
                // Get this DMU entry
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit dmuEntry = anEntry.Value;

                // Build the template if this is not a heading
                if (dmuEntry.ParagraphStyle.Contains("Heading") == true) { continue; }
                IEditTemplateFactory templateFactory = new EditTemplateFactoryClass();
                IEditTemplate newTemplate = templateFactory.Create(dmuEntry.MapUnit + " - " + dmuEntry.Name, (ILayer)theLayer);

                // Set the Template's default values
                newTemplate.SetDefaultValue("MapUnit", dmuEntry.MapUnit, false);
                newTemplate.SetDefaultValue("Label", dmuEntry.Label, false);
                newTemplate.SetDefaultValue("Notes", dmuEntry.Description, false);
                newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);

                // Add the template to the array
                templateArray.Add(newTemplate);
            }

            // Add one "UNLABELED" template
            IEditTemplateFactory unlabeledTemplateFactory = new EditTemplateFactoryClass();
            IEditTemplate unlabeledTemplate = unlabeledTemplateFactory.Create("UNLABELED", (ILayer)theLayer);
            unlabeledTemplate.SetDefaultValue("MapUnit", "UNLABELED", false);
            unlabeledTemplate.SetDefaultValue("Label", "UNLABELED", false);
            templateArray.Add(unlabeledTemplate);

            // Add the templates to the editor
            templateEditor.AddTemplates(templateArray);

            // Update the doc?
            ArcMap.Document.UpdateContents();
        }

        private static IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>> GetDmuSortedByHierarchy(IWorkspace theWorkspace)
        {
            // Get All DescriptionOfMapUnits.
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(theWorkspace);
            DmuAccess.AddDescriptionOfMapUnits();

            // Sort using Linq syntax
            var sortedDmuEntries = (
                from entry in DmuAccess.DescriptionOfMapUnitsDictionary
                orderby ((DescriptionOfMapUnitsAccess.DescriptionOfMapUnit)entry.Value).HierarchyKey ascending
                select entry);

            return sortedDmuEntries;
        }
    }
}

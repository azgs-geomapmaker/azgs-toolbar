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
                    newTemplate = templateFactory.Create("contact, accurate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 1, false);
                    templateArray.Add(newTemplate);

                    // Approximate Contact
                    newTemplate = templateFactory.Create("contact, approximate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 3, false);
                    templateArray.Add(newTemplate);

                    // Concealed Contact
                    newTemplate = templateFactory.Create("contact, concealed", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact", false);
                    newTemplate.SetDefaultValue("IsConcealed", 1, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 265, false);
                    templateArray.Add(newTemplate);

                    // Gradational Contact
                    newTemplate = templateFactory.Create("contact, gradational", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 17, false);
                    templateArray.Add(newTemplate);

                    // Accurate Fault
                    newTemplate = templateFactory.Create("fault, accurate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 118, false);
                    templateArray.Add(newTemplate);

                    // Approximate Fault
                    newTemplate = templateFactory.Create("fault, approximate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 120, false);
                    templateArray.Add(newTemplate);

                    // Concealed Fault
                    newTemplate = templateFactory.Create("fault, concealed", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 1, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 273, false);
                    templateArray.Add(newTemplate);

                    // Accurate High-angle normal fault
                    newTemplate = templateFactory.Create("high-angle normal fault, accurate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "High-angle normal fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 126, false);
                    templateArray.Add(newTemplate);

                    // Approximate High-angle normal fault
                    newTemplate = templateFactory.Create("high-angle normal fault, approximate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "High-angle normal fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 128, false);
                    templateArray.Add(newTemplate);

                    // Concealed High-angle normal fault
                    newTemplate = templateFactory.Create("high-angle normal fault, concealed", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "High-angle normal fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 1, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 275, false);
                    templateArray.Add(newTemplate);

                    // Accurate Thrust fault
                    newTemplate = templateFactory.Create("thrust fault, accurate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Thrust fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 206, false);
                    templateArray.Add(newTemplate);

                    // Approximate Thrust fault
                    newTemplate = templateFactory.Create("thrust fault, approximate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Thrust fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 208, false);
                    templateArray.Add(newTemplate);

                    // Concealed Thrust fault
                    newTemplate = templateFactory.Create("thrust fault, concealed", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Thrust fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 1, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 295, false);
                    templateArray.Add(newTemplate);

                    // Accurate Right-lateral strike slip fault
                    newTemplate = templateFactory.Create("right-lateral strike slip fault, accurate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Right-lateral strike slip fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 174, false);
                    templateArray.Add(newTemplate);

                    // Approximate Right-lateral strike slip fault
                    newTemplate = templateFactory.Create("right-lateral strike slip fault, approximate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Right-lateral strike slip fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 176, false);
                    templateArray.Add(newTemplate);

                    // Concealed Right-lateral strike slip fault
                    newTemplate = templateFactory.Create("right-lateral strike slip fault, concealed", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Right-lateral strike slip fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 1, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 287, false);
                    templateArray.Add(newTemplate);

                    // Accurate Left-lateral strike slip fault
                    newTemplate = templateFactory.Create("left-lateral strike slip fault, accurate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Left-lateral strike slip fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 182, false);
                    templateArray.Add(newTemplate);

                    // Approximate Left-lateral strike slip fault
                    newTemplate = templateFactory.Create("left-lateral strike slip fault, approximate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Left-lateral strike slip fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 0, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 184, false);
                    templateArray.Add(newTemplate);

                    // Concealed Left-lateral strike slip fault
                    newTemplate = templateFactory.Create("left-lateral strike slip fault, concealed", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Left-lateral strike slip fault", false);
                    newTemplate.SetDefaultValue("IsConcealed", 1, false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 289, false);
                    templateArray.Add(newTemplate);
                    break;

                case "OtherLines":
                    // Pink Dike
                    newTemplate = templateFactory.Create("dike, pink", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Dike", false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 321, false);
                    templateArray.Add(newTemplate);

                    // Blue Dike
                    newTemplate = templateFactory.Create("dike, blue", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Dike", false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 325, false);
                    templateArray.Add(newTemplate);

                    // Purple Dike
                    newTemplate = templateFactory.Create("dike, purple", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Dike", false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 323, false);
                    templateArray.Add(newTemplate);

                    // Anticline
                    newTemplate = templateFactory.Create("anticline, accurate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Anticline", false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 1, false);
                    templateArray.Add(newTemplate);

                    // Syncline
                    newTemplate = templateFactory.Create("syncline, accurate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Syncline", false);
                    newTemplate.SetDefaultValue("ExistenceConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("LocationConfidenceMeters", 0, false);
                    newTemplate.SetDefaultValue("Symbol", 97, false);
                    templateArray.Add(newTemplate);
                    break;

                case "OrientationDataPoints":
                    // bedding, approximate
                    newTemplate = templateFactory.Create("bedding, approximate", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Bedding fabric", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 53, false);
                    templateArray.Add(newTemplate);

                    // bedding, horizontal
                    newTemplate = templateFactory.Create("bedding, horizontal", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Bedding fabric", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 21, false);
                    templateArray.Add(newTemplate);

                    // bedding, inclined
                    newTemplate = templateFactory.Create("bedding, inclined", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Bedding fabric", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 22, false);
                    templateArray.Add(newTemplate);

                    // bedding, transposed
                    newTemplate = templateFactory.Create("bedding, transposed", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Transposed bedding layering", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 142, false);
                    templateArray.Add(newTemplate);

                    // cleavage
                    newTemplate = templateFactory.Create("cleavage", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Cleavage", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 68, false);
                    templateArray.Add(newTemplate);

                    // contact dip
                    newTemplate = templateFactory.Create("contact dip", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Contact dip", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 398, false);
                    templateArray.Add(newTemplate);

                    // fault attitude
                    newTemplate = templateFactory.Create("fault attitude", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fault attitude", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 395, false);
                    templateArray.Add(newTemplate);

                    // fault attitude, no fault trace
                    newTemplate = templateFactory.Create("fault attitude (no fault trace)", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Fault attitude", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 396, false);
                    templateArray.Add(newTemplate);

                    // foliation, eutaxitic
                    newTemplate = templateFactory.Create("foliation, eutaxitic", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Eutaxitic foliation", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 128, false);
                    templateArray.Add(newTemplate);

                    // foliation, generic
                    newTemplate = templateFactory.Create("foliation, generic", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Foliation", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 136, false);
                    templateArray.Add(newTemplate);

                    // foliation, gneissic layering
                    newTemplate = templateFactory.Create("foliation, gneissic layering", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Gneissic layering", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 181, false);
                    templateArray.Add(newTemplate);

                    // foliation, mylonitic
                    newTemplate = templateFactory.Create("foliation, mylonitic", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Generic mylonitic foliation", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 381, false);
                    templateArray.Add(newTemplate);

                    // joint
                    newTemplate = templateFactory.Create("joint", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Joint fabric", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 3, false);
                    templateArray.Add(newTemplate);

                    // lineation, horizontal
                    newTemplate = templateFactory.Create("lineation, horizontal", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Lineation", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 199, false);
                    templateArray.Add(newTemplate);

                    // lineation, generic (arrowhead)
                    newTemplate = templateFactory.Create("lineation, generic (arrowhead)", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Lineation", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 195, false);
                    templateArray.Add(newTemplate);

                    // lineation, generic (no arrowhead)
                    newTemplate = templateFactory.Create("lineation, generic (no arrowhead)", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Lineation", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 384, false);
                    templateArray.Add(newTemplate);

                    // Slickenline, horizontal
                    newTemplate = templateFactory.Create("slickenline, horizontal", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Slickenline", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 213, false);
                    templateArray.Add(newTemplate);

                    // Slickenline, inclined
                    newTemplate = templateFactory.Create("slickenline, inclined", FeatureClassLayer);
                    newTemplate.SetDefaultValue("Type", "Slickenline", false);
                    newTemplate.SetDefaultValue("IdentityConfidence", "Standard Confidence", false);
                    newTemplate.SetDefaultValue("PlotAtScale", 24000, false);
                    newTemplate.SetDefaultValue("Symbol", 211, false);
                    templateArray.Add(newTemplate);
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

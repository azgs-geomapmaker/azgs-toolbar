using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;

namespace ncgmpToolbar.Utilities
{
    public class ncgmpChecks
    {
        public static bool IsWorkspaceMinNCGMPCompliant(IWorkspace Workspace)
        {
            try
            {
                IWorkspace2 theWorkspace = (IWorkspace2)Workspace;
                //Check that requisite datasets are present
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureDataset, commonFunctions.QualifyClassName(Workspace, "GeologicMap")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace,"MapUnitPolys")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace,"ContactsAndFaults")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace,"DescriptionOfMapUnits")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace,"DataSources")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace,"Glossary")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace,"DataSourcePolys")) == false) { return false; }

                //Check that requisite fields are present in each dataset
                ITable checkTable = commonFunctions.OpenTable(Workspace, "MapUnitPolys");
                if (checkTable.FindField("MapUnitPolys_ID") == -1) { return false; }
                if (checkTable.FindField("MapUnit") == -1) { return false; }
                if (checkTable.FindField("IdentityConfidence") == -1) { return false; }
                if (checkTable.FindField("Label") == -1) { return false; }
                if (checkTable.FindField("Symbol") == -1) { return false; }
                if (checkTable.FindField("Notes") == -1) { return false; }
                if (checkTable.FindField("DataSourceID") == -1) { return false; }

                checkTable = commonFunctions.OpenTable(Workspace, "ContactsAndFaults");
                if (checkTable.FindField("ContactsAndFaults_ID") == -1) { return false; }
                if (checkTable.FindField("Type") == -1) { return false; }
                if (checkTable.FindField("LocationConfidenceMeters") == -1) { return false; }
                if (checkTable.FindField("ExistenceConfidence") == -1) { return false; }
                if (checkTable.FindField("IdentityConfidence") == -1) { return false; }
                if (checkTable.FindField("IsConcealed") == -1) { return false; }
                if (checkTable.FindField("Symbol") == -1) { return false; }
                if (checkTable.FindField("Label") == -1) { return false; }
                if (checkTable.FindField("Notes") == -1) { return false; }
                if (checkTable.FindField("DataSourceID") == -1) { return false; }

                checkTable = commonFunctions.OpenTable(Workspace, "DescriptionOfMapUnits");
                if (checkTable.FindField("DescriptionOfMapUnits_ID") == -1) { return false; }
                if (checkTable.FindField("MapUnit") == -1) { return false; }
                if (checkTable.FindField("Label") == -1) { return false; }
                if (checkTable.FindField("Name") == -1) { return false; }
                if (checkTable.FindField("FullName") == -1) { return false; }
                if (checkTable.FindField("Age") == -1) { return false; }
                if (checkTable.FindField("Description") == -1) { return false; }
                if (checkTable.FindField("HierarchyKey") == -1) { return false; }
                if (checkTable.FindField("ParagraphStyle") == -1) { return false; }
                if (checkTable.FindField("AreaFillRGB") == -1) { return false; }
                if (checkTable.FindField("AreaFillPatternDescription") == -1) { return false; }
                if (checkTable.FindField("DescriptionSourceID") == -1) { return false; }
                if (checkTable.FindField("GeneralLithologyTerm") == -1) { return false; }
                if (checkTable.FindField("GeneralLithologyConfidence") == -1) { return false; }

                checkTable = commonFunctions.OpenTable(Workspace, "DataSources");
                if (checkTable.FindField("DataSources_ID") == -1) { return false; }
                if (checkTable.FindField("Source") == -1) { return false; }
                if (checkTable.FindField("Notes") == -1) { return false; }

                checkTable = commonFunctions.OpenTable(Workspace, "Glossary");
                if (checkTable.FindField("Glossary_ID") == -1) { return false; }
                if (checkTable.FindField("Term") == -1) { return false; }
                if (checkTable.FindField("Definition") == -1) { return false; }
                if (checkTable.FindField("DefinitionSourceID") == -1) { return false; }

                checkTable = commonFunctions.OpenTable(Workspace, "DataSourcePolys");
                if (checkTable.FindField("DataSourcePolys_ID") == -1) { return false; }
                if (checkTable.FindField("DataSourceID") == -1) { return false; }
                if (checkTable.FindField("Notes") == -1) { return false; }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, ex.StackTrace);
                return false;
            }
        }

        public static bool IsSysInfoPresent(IWorkspace Workspace)
        {
            IWorkspace2 theWorkspace = (IWorkspace2)Workspace;
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, "SysInfo") == false) { return false; }

            ITable checkTable = commonFunctions.OpenTable(Workspace, "SysInfo");
            if (checkTable.FindField("Sub") == -1) { return false; }
            if (checkTable.FindField("Pred") == -1) { return false; }
            if (checkTable.FindField("Obj") == -1) { return false; }

            return true;
        }

        public static bool IsAzgsStationAddinPresent(IWorkspace Workspace)
        {
            IWorkspace2 theWorkspace = (IWorkspace2)Workspace;
            //Check that requisite datasets are present
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureDataset, commonFunctions.QualifyClassName(Workspace, "StationData")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace, "StationPoints")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace, "SamplePoints")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace, "OrientationDataPoints")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace, "Notes")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace, "RelatedDocuments")) == false) { return false; }
            //if (theWorkspace.get_NameExists(esriDatasetType.esriDTRelationshipClass, commonFunctions.QualifyClassName(Workspace, "StationDocumentLink")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTRelationshipClass, commonFunctions.QualifyClassName(Workspace, "StationSampleLink")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTRelationshipClass, commonFunctions.QualifyClassName(Workspace, "StationOrientationDataPointsLink")) == false) { return false; }
            //if (theWorkspace.get_NameExists(esriDatasetType.esriDTRelationshipClass, commonFunctions.QualifyClassName(Workspace, "StationNotesLink")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTRepresentationClass, commonFunctions.QualifyClassName(Workspace, "r_OrientationDataPoints")) == false) { return false; }

            ITable checkTable = commonFunctions.OpenTable(Workspace, "StationPoints");
            if (checkTable.FindField("StationPoints_ID") == -1) { return false; }
            if (checkTable.FindField("FieldID") == -1) { return false; }
            if (checkTable.FindField("Label") == -1) { return false; }
            if (checkTable.FindField("Symbol") == -1) { return false; }
            if (checkTable.FindField("PlotAtScale") == -1) { return false; }
            if (checkTable.FindField("LocationConfidenceMeters") == -1) { return false; }
            if (checkTable.FindField("Latitude") == -1) { return false; }
            if (checkTable.FindField("Longitude") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            checkTable = commonFunctions.OpenTable(Workspace, "SamplePoints");
            if (checkTable.FindField("SamplePoints_ID") == -1) { return false; }
            if (checkTable.FindField("FieldID") == -1) { return false; }
            if (checkTable.FindField("StationID") == -1) { return false; }
            if (checkTable.FindField("Label") == -1) { return false; }
            if (checkTable.FindField("Symbol") == -1) { return false; }
            if (checkTable.FindField("PlotAtScale") == -1) { return false; }
            if (checkTable.FindField("Notes") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            checkTable = commonFunctions.OpenTable(Workspace, "OrientationDataPoints");
            if (checkTable.FindField("OrientationDataPoints_ID") == -1) { return false; }
            if (checkTable.FindField("StationID") == -1) { return false; }
            if (checkTable.FindField("Type") == -1) { return false; }
            if (checkTable.FindField("IdentityConfidence") == -1) { return false; }
            if (checkTable.FindField("Label") == -1) { return false; }
            if (checkTable.FindField("Symbol") == -1) { return false; }
            if (checkTable.FindField("PlotAtScale") == -1) { return false; }
            if (checkTable.FindField("Azimuth") == -1) { return false; }
            if (checkTable.FindField("Inclination") == -1) { return false; }
            if (checkTable.FindField("OrientationConfidenceDegrees") == -1) { return false; }
            if (checkTable.FindField("Notes") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            checkTable = commonFunctions.OpenTable(Workspace, "Notes");
            if (checkTable.FindField("Notes_ID") == -1) { return false; }
            if (checkTable.FindField("OwnerID") == -1) { return false; }
            if (checkTable.FindField("Type") == -1) { return false; }
            if (checkTable.FindField("Notes") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            checkTable = commonFunctions.OpenTable(Workspace, "RelatedDocuments");
            if (checkTable.FindField("RelatedDocuments_ID") == -1) { return false; }
            if (checkTable.FindField("OwnerID") == -1) { return false; }
            if (checkTable.FindField("Type") == -1) { return false; }
            if (checkTable.FindField("DocumentPath") == -1) { return false; }
            if (checkTable.FindField("DocumentName") == -1) { return false; }
            if (checkTable.FindField("Notes") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            return true;
        }

        public static bool IsStandardLithAddinPresent(IWorkspace Workspace)
        {
            IWorkspace2 theWorkspace = (IWorkspace2)Workspace;
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace, "StandardLithology")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace, "ExtendedAttributes")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace, "GeologicEvents")) == false) { return false; }
            //if (theWorkspace.get_NameExists(esriDatasetType.esriDTRelationshipClass, "DmuStandardLithologyLink") == false) { return false; }

            ITable checkTable = commonFunctions.OpenTable(Workspace, "StandardLithology");
            if (checkTable.FindField("StandardLithology_ID") == -1) { return false; }
            if (checkTable.FindField("MapUnit") == -1) { return false; }
            if (checkTable.FindField("PartType") == -1) { return false; }
            if (checkTable.FindField("Lithology") == -1) { return false; }
            if (checkTable.FindField("ProportionTerm") == -1) { return false; }
            if (checkTable.FindField("ProportionValue") == -1) { return false; }
            if (checkTable.FindField("ScientificConfidence") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            checkTable = commonFunctions.OpenTable(Workspace, "ExtendedAttributes");
            if (checkTable.FindField("ExtendedAttributes_ID") == -1) { return false; }
            if (checkTable.FindField("OwnerTable") == -1) { return false; }
            if (checkTable.FindField("OwnerID") == -1) { return false; }
            if (checkTable.FindField("Property") == -1) { return false; }
            if (checkTable.FindField("PropertyValue") == -1) { return false; }
            if (checkTable.FindField("ValueLinkID") == -1) { return false; }
            if (checkTable.FindField("Qualifier") == -1) { return false; }
            if (checkTable.FindField("Notes") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            checkTable = commonFunctions.OpenTable(Workspace, "GeologicEvents");
            if (checkTable.FindField("GeologicEvents_ID") == -1) { return false; }
            if (checkTable.FindField("Event") == -1) { return false; }
            if (checkTable.FindField("AgeDisplay") == -1) { return false; }
            if (checkTable.FindField("AgeYoungerTerm") == -1) { return false; }
            if (checkTable.FindField("AgeOlderTerm") == -1) { return false; }
            if (checkTable.FindField("TimeScale") == -1) { return false; }
            if (checkTable.FindField("AgeYoungerValue") == -1) { return false; }
            if (checkTable.FindField("AgeOlderValue") == -1) { return false; }
            if (checkTable.FindField("Notes") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            return true;
        }

        public static bool AreRepresentationsUsed(IWorkspace Workspace)
        {
            IWorkspace2 theWorkspace = (IWorkspace2)Workspace;
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTRepresentationClass, commonFunctions.QualifyClassName(Workspace, "r_ContactsAndFaults")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTRepresentationClass, commonFunctions.QualifyClassName(Workspace, "r_OtherLines")) == false) { return false; }
            return true;
        }

        public static bool IsTopologyUsed(IWorkspace Workspace)
        {
            IWorkspace2 theWorkspace = (IWorkspace2)Workspace;
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTTopology, commonFunctions.QualifyClassName(Workspace, "GeologicMapTopology")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace, "OtherLines")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace, "OverlayPolys")) == false) { return false; }

            ITable checkTable = commonFunctions.OpenTable(Workspace, "OtherLines");
            if (checkTable.FindField("OtherLines_ID") == -1) { return false; }
            if (checkTable.FindField("Type") == -1) { return false; }
            if (checkTable.FindField("LocationConfidenceMeters") == -1) { return false; }
            if (checkTable.FindField("ExistenceConfidence") == -1) { return false; }
            if (checkTable.FindField("IdentityConfidence") == -1) { return false; }
            if (checkTable.FindField("Symbol") == -1) { return false; }
            if (checkTable.FindField("Label") == -1) { return false; }
            if (checkTable.FindField("Notes") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            checkTable = commonFunctions.OpenTable(Workspace, "OverlayPolys");
            if (checkTable.FindField("OverlayPolys_ID") == -1) { return false; }
            if (checkTable.FindField("MapUnit") == -1) { return false; }
            if (checkTable.FindField("IdentityConfidence") == -1) { return false; }
            if (checkTable.FindField("Label") == -1) { return false; }
            if (checkTable.FindField("Symbol") == -1) { return false; }
            if (checkTable.FindField("Notes") == -1) { return false; }
            if (checkTable.FindField("DataSourceID") == -1) { return false; }

            return true;

        }
    }
}

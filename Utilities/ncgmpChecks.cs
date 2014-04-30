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
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace, "GeologicLines")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace,"DescriptionOfMapUnits")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace,"DataSources")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace,"Glossary")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTFeatureClass, commonFunctions.QualifyClassName(Workspace,"DataSourcePolys")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace, "StandardLithology")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace, "ExtendedAttributes")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, commonFunctions.QualifyClassName(Workspace, "GeologicEvents")) == false) { return false; }
                if (theWorkspace.get_NameExists(esriDatasetType.esriDTTable, "SysInfo") == false) { return false; }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message, ex.StackTrace);
                return false;
            }
        }
    
        public static bool AreRepresentationsUsed(IWorkspace Workspace)
        {
            IWorkspace2 theWorkspace = (IWorkspace2)Workspace;
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTRepresentationClass, commonFunctions.QualifyClassName(Workspace, "ContactsAndFaults_Rep")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTRepresentationClass, commonFunctions.QualifyClassName(Workspace, "GeologicLines_Rep")) == false) { return false; }
            if (theWorkspace.get_NameExists(esriDatasetType.esriDTRepresentationClass, commonFunctions.QualifyClassName(Workspace, "OrientationPoints_Rep")) == false) { return false; }
            return true;
        }
       
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Editor;
using ncgmpToolbar.Utilities;
using ESRI.ArcGIS.Geometry;

namespace ncgmpToolbar.Utilities.DataAccess
{
    class OrientationPointsAccess
    {
        IFeatureClass m_OrientationPointsFC;
        IWorkspace m_theWorkspace;

        public OrientationPointsAccess(IWorkspace theWorkspace)
        {
            m_OrientationPointsFC = commonFunctions.OpenFeatureClass(theWorkspace, "OrientationPoints");
            m_theWorkspace = theWorkspace;
        }

        public struct OrientationPoint
        {
            public string OrientationPoints_ID;
            public string StationID;
            public string Type;
            public string IdentityConfidence;
            public string Label;
            public int PlotAtScale;
            public double Azimuth;
            public double Inclination;
            public double OrientationConfidenceDegrees;
            public string Notes;
            public string DataSourceID;
            public double SymbolRotation;
            public int RuleID;
            public IPoint Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, OrientationPoint> m_OrientationPointsDictionary = new Dictionary<string,OrientationPoint>();
        public Dictionary<string, OrientationPoint> OrientationPointsDictionary
        {
            get { return m_OrientationPointsDictionary; }
        }

        public void ClearOrientationPoints()
        {
            m_OrientationPointsDictionary.Clear();
        }

        public void AddOrientationPoints(string SqlWhereClause)
        {
            int idFld = m_OrientationPointsFC.FindField("OrientationPoints_ID");
            int stationFld = m_OrientationPointsFC.FindField("StationID");
            int typeFld = m_OrientationPointsFC.FindField("Type");
            int idConfFld = m_OrientationPointsFC.FindField("IdentityConfidence");
            int lblFld = m_OrientationPointsFC.FindField("Label");
            int plotFld = m_OrientationPointsFC.FindField("PlotAtScale");
            int aziFld = m_OrientationPointsFC.FindField("Azimuth");
            int incFld = m_OrientationPointsFC.FindField("Inclination");
            int orConfFld = m_OrientationPointsFC.FindField("OrientationConfidenceDegrees");
            int notesFld = m_OrientationPointsFC.FindField("Notes");
            int dsFld = m_OrientationPointsFC.FindField("DataSourceID");
            int symbRotFld = m_OrientationPointsFC.FindField("SymbolRotation");
            int symFld = m_OrientationPointsFC.FindField("RuleID");
            
            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_OrientationPointsFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                OrientationPoint anOrientationPoint = new OrientationPoint();
                anOrientationPoint.OrientationPoints_ID = theFeature.get_Value(idFld).ToString();
                anOrientationPoint.StationID = theFeature.get_Value(stationFld).ToString();
                anOrientationPoint.Type = theFeature.get_Value(typeFld).ToString();
                anOrientationPoint.IdentityConfidence = theFeature.get_Value(idConfFld).ToString();
                anOrientationPoint.Label = theFeature.get_Value(lblFld).ToString();
                bool result;
                result = int.TryParse(theFeature.get_Value(plotFld).ToString(), out anOrientationPoint.PlotAtScale);
                result = double.TryParse(theFeature.get_Value(aziFld).ToString(), out anOrientationPoint.Azimuth);
                result = double.TryParse(theFeature.get_Value(incFld).ToString(), out anOrientationPoint.Inclination);
                result = double.TryParse(theFeature.get_Value(orConfFld).ToString(), out anOrientationPoint.OrientationConfidenceDegrees);
                anOrientationPoint.Notes = theFeature.get_Value(notesFld).ToString();
                anOrientationPoint.DataSourceID = theFeature.get_Value(dsFld).ToString();
                result = double.TryParse(theFeature.get_Value(symbRotFld).ToString(), out anOrientationPoint.SymbolRotation);
                result = int.TryParse(theFeature.get_Value(symFld).ToString(), out anOrientationPoint.RuleID);
                anOrientationPoint.Shape = (IPoint)theFeature.Shape;
                anOrientationPoint.RequiresUpdate = true;

                m_OrientationPointsDictionary.Add(anOrientationPoint.OrientationPoints_ID, anOrientationPoint);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewOrientationPoint(string StationID, string Type, string IdentityConfidence, 
            string Label, int PlotAtScale, double Azimuth, double Inclination, double OrientationConfidenceDegrees,
            string Notes, string DataSourceID, int SymbolRotation, int RuleID, IPoint Shape)
        {
            OrientationPoint newOrientationPoint = new OrientationPoint();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newOrientationPoint.OrientationPoints_ID = SysInfoTable.ProjAbbr + ".OrientationPoints." + SysInfoTable.GetNextIdValue("OrientationPoints");
            newOrientationPoint.StationID = StationID;
            newOrientationPoint.Type = Type;
            newOrientationPoint.IdentityConfidence = IdentityConfidence;
            newOrientationPoint.Label = Label;
            newOrientationPoint.PlotAtScale = PlotAtScale;
            newOrientationPoint.Azimuth = Azimuth;
            newOrientationPoint.Inclination = Inclination;
            newOrientationPoint.OrientationConfidenceDegrees = OrientationConfidenceDegrees;
            newOrientationPoint.Notes = Notes;
            newOrientationPoint.DataSourceID = DataSourceID;
            newOrientationPoint.SymbolRotation = SymbolRotation;
            newOrientationPoint.RuleID = RuleID;
            newOrientationPoint.Shape = Shape;
            newOrientationPoint.RequiresUpdate = false;

            m_OrientationPointsDictionary.Add(newOrientationPoint.OrientationPoints_ID, newOrientationPoint);
            return newOrientationPoint.OrientationPoints_ID;
        }

        public void UpdateOrientationPoint(OrientationPoint theOrientationPoint)
        {
            try { m_OrientationPointsDictionary.Remove(theOrientationPoint.OrientationPoints_ID); }
            catch { }

            theOrientationPoint.RequiresUpdate = true;
            m_OrientationPointsDictionary.Add(theOrientationPoint.OrientationPoints_ID, theOrientationPoint);
        }

        public void SaveOrientationPoints()
        {
            int idFld = m_OrientationPointsFC.FindField("OrientationPoints_ID");
            int stationFld = m_OrientationPointsFC.FindField("StationID");
            int typeFld = m_OrientationPointsFC.FindField("Type");
            int idConfFld = m_OrientationPointsFC.FindField("IdentityConfidence");
            int lblFld = m_OrientationPointsFC.FindField("Label");
            int plotFld = m_OrientationPointsFC.FindField("PlotAtScale");
            int aziFld = m_OrientationPointsFC.FindField("Azimuth");
            int incFld = m_OrientationPointsFC.FindField("Inclination");
            int orConfFld = m_OrientationPointsFC.FindField("OrientationConfidenceDegrees");
            int notesFld = m_OrientationPointsFC.FindField("Notes");
            int dsFld = m_OrientationPointsFC.FindField("DataSourceID");
            int symbRotFld = m_OrientationPointsFC.FindField("SymbolRotation");
            int symFld = m_OrientationPointsFC.FindField("RuleID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "OrientationPoints_ID = '";
                IFeatureCursor insertCursor = m_OrientationPointsFC.Insert(true);

                foreach (KeyValuePair<string, OrientationPoint> aDictionaryEntry in m_OrientationPointsDictionary)
                {
                    OrientationPoint thisOrientationPoint = (OrientationPoint)aDictionaryEntry.Value;
                    switch (thisOrientationPoint.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisOrientationPoint.OrientationPoints_ID + "' OR OrientationPoints_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_OrientationPointsFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisOrientationPoint.OrientationPoints_ID);
                            theFeatureBuffer.set_Value(stationFld, thisOrientationPoint.StationID);
                            theFeatureBuffer.set_Value(typeFld, thisOrientationPoint.Type);
                            theFeatureBuffer.set_Value(idConfFld, thisOrientationPoint.IdentityConfidence);
                            theFeatureBuffer.set_Value(lblFld, thisOrientationPoint.Label);
                            theFeatureBuffer.set_Value(plotFld, thisOrientationPoint.PlotAtScale);
                            theFeatureBuffer.set_Value(aziFld, thisOrientationPoint.Azimuth);
                            theFeatureBuffer.set_Value(incFld, thisOrientationPoint.Inclination);
                            theFeatureBuffer.set_Value(orConfFld, thisOrientationPoint.OrientationConfidenceDegrees);
                            theFeatureBuffer.set_Value(notesFld, thisOrientationPoint.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisOrientationPoint.DataSourceID);
                            theFeatureBuffer.set_Value(symbRotFld, thisOrientationPoint.SymbolRotation);
                            theFeatureBuffer.set_Value(symFld, thisOrientationPoint.RuleID);
                            theFeatureBuffer.Shape = thisOrientationPoint.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert OrientationPoints");
                theEditor.StartOperation();

                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_OrientationPointsFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    OrientationPoint thisOrientationPoint = m_OrientationPointsDictionary[theID];
                    theFeature.set_Value(stationFld, thisOrientationPoint.StationID);
                    theFeature.set_Value(typeFld, thisOrientationPoint.Type);
                    theFeature.set_Value(idConfFld, thisOrientationPoint.IdentityConfidence);
                    theFeature.set_Value(lblFld, thisOrientationPoint.Label);
                    theFeature.set_Value(plotFld, thisOrientationPoint.PlotAtScale);
                    theFeature.set_Value(aziFld, thisOrientationPoint.Azimuth);
                    theFeature.set_Value(incFld, thisOrientationPoint.Inclination);
                    theFeature.set_Value(orConfFld, thisOrientationPoint.OrientationConfidenceDegrees);
                    theFeature.set_Value(notesFld, thisOrientationPoint.Notes);
                    theFeature.set_Value(dsFld, thisOrientationPoint.DataSourceID);
                    theFeature.set_Value(symbRotFld, thisOrientationPoint.SymbolRotation);
                    theFeature.set_Value(symFld, thisOrientationPoint.RuleID);
                    theFeature.Shape = thisOrientationPoint.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update OrientationPoints");
            }
            catch { theEditor.StopOperation("OrientationPoints Management Failure"); }
        }
    }
}

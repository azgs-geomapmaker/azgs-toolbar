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
    class OrientationDataPointsAccess
    {
        IFeatureClass m_OrientationDataPointsFC;
        IWorkspace m_theWorkspace;

        public OrientationDataPointsAccess(IWorkspace theWorkspace)
        {
            m_OrientationDataPointsFC = commonFunctions.OpenFeatureClass(theWorkspace, "OrientationDataPoints");
            m_theWorkspace = theWorkspace;
        }

        public struct OrientationDataPoint
        {
            public string OrientationDataPoints_ID;
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
            public int Symbol;
            public IPoint Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, OrientationDataPoint> m_OrientationDataPointsDictionary = new Dictionary<string,OrientationDataPoint>();
        public Dictionary<string, OrientationDataPoint> OrientationDataPointsDictionary
        {
            get { return m_OrientationDataPointsDictionary; }
        }

        public void ClearOrientationDataPoints()
        {
            m_OrientationDataPointsDictionary.Clear();
        }

        public void AddOrientationDataPoints(string SqlWhereClause)
        {
            int idFld = m_OrientationDataPointsFC.FindField("OrientationDataPoints_ID");
            int stationFld = m_OrientationDataPointsFC.FindField("StationID");
            int typeFld = m_OrientationDataPointsFC.FindField("Type");
            int idConfFld = m_OrientationDataPointsFC.FindField("IdentityConfidence");
            int lblFld = m_OrientationDataPointsFC.FindField("Label");
            int plotFld = m_OrientationDataPointsFC.FindField("PlotAtScale");
            int aziFld = m_OrientationDataPointsFC.FindField("Azimuth");
            int incFld = m_OrientationDataPointsFC.FindField("Inclination");
            int orConfFld = m_OrientationDataPointsFC.FindField("OrientationConfidenceDegrees");
            int notesFld = m_OrientationDataPointsFC.FindField("Notes");
            int dsFld = m_OrientationDataPointsFC.FindField("DataSourceID");
            int symbRotFld = m_OrientationDataPointsFC.FindField("SymbolRotation");
            int symFld = m_OrientationDataPointsFC.FindField("Symbol");
            
            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_OrientationDataPointsFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                OrientationDataPoint anOrientationDataPoint = new OrientationDataPoint();
                anOrientationDataPoint.OrientationDataPoints_ID = theFeature.get_Value(idFld).ToString();
                anOrientationDataPoint.StationID = theFeature.get_Value(stationFld).ToString();
                anOrientationDataPoint.Type = theFeature.get_Value(typeFld).ToString();
                anOrientationDataPoint.IdentityConfidence = theFeature.get_Value(idConfFld).ToString();
                anOrientationDataPoint.Label = theFeature.get_Value(lblFld).ToString();
                bool result;
                result = int.TryParse(theFeature.get_Value(plotFld).ToString(), out anOrientationDataPoint.PlotAtScale);
                result = double.TryParse(theFeature.get_Value(aziFld).ToString(), out anOrientationDataPoint.Azimuth);
                result = double.TryParse(theFeature.get_Value(incFld).ToString(), out anOrientationDataPoint.Inclination);
                result = double.TryParse(theFeature.get_Value(orConfFld).ToString(), out anOrientationDataPoint.OrientationConfidenceDegrees);
                anOrientationDataPoint.Notes = theFeature.get_Value(notesFld).ToString();
                anOrientationDataPoint.DataSourceID = theFeature.get_Value(dsFld).ToString();
                result = double.TryParse(theFeature.get_Value(symbRotFld).ToString(), out anOrientationDataPoint.SymbolRotation);
                result = int.TryParse(theFeature.get_Value(symFld).ToString(), out anOrientationDataPoint.Symbol);
                anOrientationDataPoint.Shape = (IPoint)theFeature.Shape;
                anOrientationDataPoint.RequiresUpdate = true;

                m_OrientationDataPointsDictionary.Add(anOrientationDataPoint.OrientationDataPoints_ID, anOrientationDataPoint);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewOrientationDataPoint(string StationID, string Type, string IdentityConfidence, 
            string Label, int PlotAtScale, double Azimuth, double Inclination, double OrientationConfidenceDegrees,
            string Notes, string DataSourceID, int SymbolRotation, int Symbol, IPoint Shape)
        {
            OrientationDataPoint newOrientationDataPoint = new OrientationDataPoint();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newOrientationDataPoint.OrientationDataPoints_ID = SysInfoTable.ProjAbbr + ".OrientationDataPoints." + SysInfoTable.GetNextIdValue("OrientationDataPoints");
            newOrientationDataPoint.StationID = StationID;
            newOrientationDataPoint.Type = Type;
            newOrientationDataPoint.IdentityConfidence = IdentityConfidence;
            newOrientationDataPoint.Label = Label;
            newOrientationDataPoint.PlotAtScale = PlotAtScale;
            newOrientationDataPoint.Azimuth = Azimuth;
            newOrientationDataPoint.Inclination = Inclination;
            newOrientationDataPoint.OrientationConfidenceDegrees = OrientationConfidenceDegrees;
            newOrientationDataPoint.Notes = Notes;
            newOrientationDataPoint.DataSourceID = DataSourceID;
            newOrientationDataPoint.SymbolRotation = SymbolRotation;
            newOrientationDataPoint.Symbol = Symbol;
            newOrientationDataPoint.Shape = Shape;
            newOrientationDataPoint.RequiresUpdate = false;

            m_OrientationDataPointsDictionary.Add(newOrientationDataPoint.OrientationDataPoints_ID, newOrientationDataPoint);
            return newOrientationDataPoint.OrientationDataPoints_ID;
        }

        public void UpdateOrientationDataPoint(OrientationDataPoint theOrientationDataPoint)
        {
            try { m_OrientationDataPointsDictionary.Remove(theOrientationDataPoint.OrientationDataPoints_ID); }
            catch { }

            theOrientationDataPoint.RequiresUpdate = true;
            m_OrientationDataPointsDictionary.Add(theOrientationDataPoint.OrientationDataPoints_ID, theOrientationDataPoint);
        }

        public void SaveOrientationDataPoints()
        {
            int idFld = m_OrientationDataPointsFC.FindField("OrientationDataPoints_ID");
            int stationFld = m_OrientationDataPointsFC.FindField("StationID");
            int typeFld = m_OrientationDataPointsFC.FindField("Type");
            int idConfFld = m_OrientationDataPointsFC.FindField("IdentityConfidence");
            int lblFld = m_OrientationDataPointsFC.FindField("Label");
            int plotFld = m_OrientationDataPointsFC.FindField("PlotAtScale");
            int aziFld = m_OrientationDataPointsFC.FindField("Azimuth");
            int incFld = m_OrientationDataPointsFC.FindField("Inclination");
            int orConfFld = m_OrientationDataPointsFC.FindField("OrientationConfidenceDegrees");
            int notesFld = m_OrientationDataPointsFC.FindField("Notes");
            int dsFld = m_OrientationDataPointsFC.FindField("DataSourceID");
            int symbRotFld = m_OrientationDataPointsFC.FindField("SymbolRotation");
            int symFld = m_OrientationDataPointsFC.FindField("Symbol");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "OrientationDataPoints_ID = '";
                IFeatureCursor insertCursor = m_OrientationDataPointsFC.Insert(true);

                foreach (KeyValuePair<string, OrientationDataPoint> aDictionaryEntry in m_OrientationDataPointsDictionary)
                {
                    OrientationDataPoint thisOrientationDataPoint = (OrientationDataPoint)aDictionaryEntry.Value;
                    switch (thisOrientationDataPoint.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisOrientationDataPoint.OrientationDataPoints_ID + "' OR OrientationDataPoints_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_OrientationDataPointsFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisOrientationDataPoint.OrientationDataPoints_ID);
                            theFeatureBuffer.set_Value(stationFld, thisOrientationDataPoint.StationID);
                            theFeatureBuffer.set_Value(typeFld, thisOrientationDataPoint.Type);
                            theFeatureBuffer.set_Value(idConfFld, thisOrientationDataPoint.IdentityConfidence);
                            theFeatureBuffer.set_Value(lblFld, thisOrientationDataPoint.Label);
                            theFeatureBuffer.set_Value(plotFld, thisOrientationDataPoint.PlotAtScale);
                            theFeatureBuffer.set_Value(aziFld, thisOrientationDataPoint.Azimuth);
                            theFeatureBuffer.set_Value(incFld, thisOrientationDataPoint.Inclination);
                            theFeatureBuffer.set_Value(orConfFld, thisOrientationDataPoint.OrientationConfidenceDegrees);
                            theFeatureBuffer.set_Value(notesFld, thisOrientationDataPoint.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisOrientationDataPoint.DataSourceID);
                            theFeatureBuffer.set_Value(symbRotFld, thisOrientationDataPoint.SymbolRotation);
                            theFeatureBuffer.set_Value(symFld, thisOrientationDataPoint.Symbol);
                            theFeatureBuffer.Shape = thisOrientationDataPoint.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert OrientationDataPoints");
                theEditor.StartOperation();

                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_OrientationDataPointsFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    OrientationDataPoint thisOrientationDataPoint = m_OrientationDataPointsDictionary[theID];
                    theFeature.set_Value(stationFld, thisOrientationDataPoint.StationID);
                    theFeature.set_Value(typeFld, thisOrientationDataPoint.Type);
                    theFeature.set_Value(idConfFld, thisOrientationDataPoint.IdentityConfidence);
                    theFeature.set_Value(lblFld, thisOrientationDataPoint.Label);
                    theFeature.set_Value(plotFld, thisOrientationDataPoint.PlotAtScale);
                    theFeature.set_Value(aziFld, thisOrientationDataPoint.Azimuth);
                    theFeature.set_Value(incFld, thisOrientationDataPoint.Inclination);
                    theFeature.set_Value(orConfFld, thisOrientationDataPoint.OrientationConfidenceDegrees);
                    theFeature.set_Value(notesFld, thisOrientationDataPoint.Notes);
                    theFeature.set_Value(dsFld, thisOrientationDataPoint.DataSourceID);
                    theFeature.set_Value(symbRotFld, thisOrientationDataPoint.SymbolRotation);
                    theFeature.set_Value(symFld, thisOrientationDataPoint.Symbol);
                    theFeature.Shape = thisOrientationDataPoint.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update OrientationDataPoints");
            }
            catch { theEditor.StopOperation("OrientationDataPoints Management Failure"); }
        }
    }
}

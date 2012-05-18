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
    class StationPointsAccess
    {
        IFeatureClass m_StationPointsFC;
        IWorkspace m_theWorkspace;

        public StationPointsAccess(IWorkspace theWorkspace)
        {
            m_StationPointsFC = commonFunctions.OpenFeatureClass(theWorkspace, "StationPoints");
            m_theWorkspace = theWorkspace;
        }

        public struct StationPoint
        {
            public string StationPoints_ID;
            public string FieldID;
            public string Label;
            public int PlotAtScale;
            public double LocationConfidenceMeters;
            public double Latitude;
            public double Longitude;
            public string DataSourceID;
            public IPoint Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, StationPoint> m_StationPointsDictionary = new Dictionary<string, StationPoint>();
        public Dictionary<string, StationPoint> StationPointsDictionary
        {
            get { return m_StationPointsDictionary; }
        }

        public void ClearStationPoints()
        {
            m_StationPointsDictionary.Clear();
        }

        public void AddStationPoints(string SqlWhereClause)
        {
            int idFld = m_StationPointsFC.FindField("StationPoints_ID");
            int fieldFld = m_StationPointsFC.FindField("FieldID");
            int lblFld = m_StationPointsFC.FindField("Label");
            int plotFld = m_StationPointsFC.FindField("PlotAtScale");
            int locConfFld = m_StationPointsFC.FindField("LocationConfidenceMeters");
            int latFld = m_StationPointsFC.FindField("Latitude");
            int longFld = m_StationPointsFC.FindField("Longitude");
            int dsFld = m_StationPointsFC.FindField("DataSourceID");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_StationPointsFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                StationPoint anStationPoint = new StationPoint();
                anStationPoint.StationPoints_ID = theFeature.get_Value(idFld).ToString();
                anStationPoint.FieldID = theFeature.get_Value(fieldFld).ToString();
                anStationPoint.Label = theFeature.get_Value(lblFld).ToString();
                anStationPoint.PlotAtScale = int.Parse(theFeature.get_Value(plotFld).ToString());
                anStationPoint.LocationConfidenceMeters = double.Parse(theFeature.get_Value(locConfFld).ToString());
                anStationPoint.Latitude = double.Parse(theFeature.get_Value(latFld).ToString());
                anStationPoint.Longitude = double.Parse(theFeature.get_Value(longFld).ToString());
                anStationPoint.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anStationPoint.Shape = (IPoint)theFeature.Shape;
                anStationPoint.RequiresUpdate = true;

                m_StationPointsDictionary.Add(anStationPoint.StationPoints_ID, anStationPoint);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewStationPoint(string StationID, string Type, 
            string Label, int PlotAtScale, double LocationConfidenceMeters, double Latitude, double Longitude,
            string DataSourceID, IPoint Shape)
        {
            StationPoint newStationPoint = new StationPoint();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newStationPoint.StationPoints_ID = SysInfoTable.ProjAbbr + ".StationPoints." + SysInfoTable.GetNextIdValue("StationPoints");
            newStationPoint.FieldID = StationID;
            newStationPoint.Label = Label;
            newStationPoint.PlotAtScale = PlotAtScale;
            newStationPoint.LocationConfidenceMeters = LocationConfidenceMeters;
            newStationPoint.Latitude = Latitude;
            newStationPoint.Longitude = Longitude;
            newStationPoint.DataSourceID = DataSourceID;
            newStationPoint.Shape = Shape;
            newStationPoint.RequiresUpdate = false;

            m_StationPointsDictionary.Add(newStationPoint.StationPoints_ID, newStationPoint);
            return newStationPoint.StationPoints_ID;
        }

        public void UpdateStationPoint(StationPoint theStationPoint)
        {
            try { m_StationPointsDictionary.Remove(theStationPoint.StationPoints_ID); }
            catch { }

            theStationPoint.RequiresUpdate = true;
            m_StationPointsDictionary.Add(theStationPoint.StationPoints_ID, theStationPoint);
        }

        public void SaveStationPoints()
        {
            int idFld = m_StationPointsFC.FindField("StationPoints_ID");
            int fieldFld = m_StationPointsFC.FindField("FieldID");
            int lblFld = m_StationPointsFC.FindField("Label");
            int plotFld = m_StationPointsFC.FindField("PlotAtScale");
            int locConfFld = m_StationPointsFC.FindField("LocationConfidenceMeters");
            int latFld = m_StationPointsFC.FindField("Latitude");
            int longFld = m_StationPointsFC.FindField("Longitude");
            int dsFld = m_StationPointsFC.FindField("DataSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "StationPoints_ID = '";
                IFeatureCursor insertCursor = m_StationPointsFC.Insert(true);

                foreach (KeyValuePair<string, StationPoint> aDictionaryEntry in m_StationPointsDictionary)
                {
                    StationPoint thisStationPoint = (StationPoint)aDictionaryEntry.Value;
                    switch (thisStationPoint.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisStationPoint.StationPoints_ID + "' OR StationPoints_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_StationPointsFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisStationPoint.StationPoints_ID);
                            theFeatureBuffer.set_Value(fieldFld, thisStationPoint.FieldID);
                            theFeatureBuffer.set_Value(lblFld, thisStationPoint.Label);
                            theFeatureBuffer.set_Value(plotFld, thisStationPoint.PlotAtScale);
                            theFeatureBuffer.set_Value(locConfFld, thisStationPoint.LocationConfidenceMeters);
                            theFeatureBuffer.set_Value(latFld, thisStationPoint.Latitude);
                            theFeatureBuffer.set_Value(longFld, thisStationPoint.Longitude);
                            theFeatureBuffer.set_Value(dsFld, thisStationPoint.DataSourceID);
                            theFeatureBuffer.Shape = thisStationPoint.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert StationPoints");
                theEditor.StartOperation();

                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_StationPointsFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    StationPoint thisStationPoint = m_StationPointsDictionary[theID];
                    theFeature.set_Value(fieldFld, thisStationPoint.FieldID);
                    theFeature.set_Value(lblFld, thisStationPoint.Label);
                    theFeature.set_Value(plotFld, thisStationPoint.PlotAtScale);
                    theFeature.set_Value(locConfFld, thisStationPoint.LocationConfidenceMeters);
                    theFeature.set_Value(latFld, thisStationPoint.Latitude);
                    theFeature.set_Value(longFld, thisStationPoint.Longitude);
                    theFeature.set_Value(dsFld, thisStationPoint.DataSourceID);
                    theFeature.Shape = thisStationPoint.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update StationPoints");
            }
            catch { theEditor.StopOperation("StationPoints Management Failure"); }
        }
    }
}

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
    class StationsAccess
    {
        IFeatureClass m_StationsFC;
        IWorkspace m_theWorkspace;

        public StationsAccess(IWorkspace theWorkspace)
        {
            m_StationsFC = commonFunctions.OpenFeatureClass(theWorkspace, "Stations");
            m_theWorkspace = theWorkspace;
        }

        public struct Station
        {
            public string Stations_ID;
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

        private Dictionary<string, Station> m_StationsDictionary = new Dictionary<string, Station>();
        public Dictionary<string, Station> StationsDictionary
        {
            get { return m_StationsDictionary; }
        }

        public void ClearStations()
        {
            m_StationsDictionary.Clear();
        }

        public void AddStations(string SqlWhereClause)
        {
            int idFld = m_StationsFC.FindField("Stations_ID");
            int fieldFld = m_StationsFC.FindField("FieldID");
            int lblFld = m_StationsFC.FindField("Label");
            int plotFld = m_StationsFC.FindField("PlotAtScale");
            int locConfFld = m_StationsFC.FindField("LocationConfidenceMeters");
            int latFld = m_StationsFC.FindField("Latitude");
            if (latFld == -1)
                latFld = m_StationsFC.FindField("MapY");
            int longFld = m_StationsFC.FindField("Longitude");
            if (longFld == -1)
                longFld = m_StationsFC.FindField("MapX");
            int dsFld = m_StationsFC.FindField("DataSourceID");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_StationsFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                Station anStation = new Station();
                anStation.Stations_ID = theFeature.get_Value(idFld).ToString();
                anStation.FieldID = theFeature.get_Value(fieldFld).ToString();
                anStation.Label = theFeature.get_Value(lblFld).ToString();
                string plotFldStr = theFeature.get_Value(plotFld).ToString();
                anStation.PlotAtScale = int.Parse(string.IsNullOrEmpty(plotFldStr) ? "-9999" : plotFldStr);
                string locConfFldStr = theFeature.get_Value(locConfFld).ToString();
                anStation.LocationConfidenceMeters = double.Parse(string.IsNullOrEmpty(locConfFldStr) ? "-9999" : locConfFldStr);
                string latFldStr = theFeature.get_Value(latFld).ToString();
                anStation.Latitude = double.Parse(string.IsNullOrEmpty(latFldStr) ? "-9999" : latFldStr);
                string longFldStr = theFeature.get_Value(longFld).ToString();
                anStation.Longitude = double.Parse(string.IsNullOrEmpty(longFldStr) ? "-9999" : longFldStr);
                anStation.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anStation.Shape = (IPoint)theFeature.Shape;
                anStation.RequiresUpdate = true;

                m_StationsDictionary.Add(anStation.Stations_ID, anStation);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewStation(string StationID, string Type, 
            string Label, int PlotAtScale, double LocationConfidenceMeters, double Latitude, double Longitude,
            string DataSourceID, IPoint Shape)
        {
            Station newStation = new Station();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newStation.Stations_ID = SysInfoTable.ProjAbbr + ".Stations." + SysInfoTable.GetNextIdValue("Stations");
            newStation.FieldID = StationID;
            newStation.Label = Label;
            newStation.PlotAtScale = PlotAtScale;
            newStation.LocationConfidenceMeters = LocationConfidenceMeters;
            newStation.Latitude = Latitude;
            newStation.Longitude = Longitude;
            newStation.DataSourceID = DataSourceID;
            newStation.Shape = Shape;
            newStation.RequiresUpdate = false;

            m_StationsDictionary.Add(newStation.Stations_ID, newStation);
            return newStation.Stations_ID;
        }

        public void UpdateStation(Station theStation)
        {
            try { m_StationsDictionary.Remove(theStation.Stations_ID); }
            catch { }

            theStation.RequiresUpdate = true;
            m_StationsDictionary.Add(theStation.Stations_ID, theStation);
        }

        public void SaveStations()
        {
            int idFld = m_StationsFC.FindField("Stations_ID");
            int fieldFld = m_StationsFC.FindField("FieldID");
            int lblFld = m_StationsFC.FindField("Label");
            int plotFld = m_StationsFC.FindField("PlotAtScale");
            int locConfFld = m_StationsFC.FindField("LocationConfidenceMeters");
            int latFld = m_StationsFC.FindField("Latitude");
            int longFld = m_StationsFC.FindField("Longitude");
            int dsFld = m_StationsFC.FindField("DataSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "Stations_ID = '";
                IFeatureCursor insertCursor = m_StationsFC.Insert(true);

                foreach (KeyValuePair<string, Station> aDictionaryEntry in m_StationsDictionary)
                {
                    Station thisStation = (Station)aDictionaryEntry.Value;
                    switch (thisStation.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisStation.Stations_ID + "' OR Stations_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_StationsFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisStation.Stations_ID);
                            theFeatureBuffer.set_Value(fieldFld, thisStation.FieldID);
                            theFeatureBuffer.set_Value(lblFld, thisStation.Label);
                            theFeatureBuffer.set_Value(plotFld, thisStation.PlotAtScale);
                            theFeatureBuffer.set_Value(locConfFld, thisStation.LocationConfidenceMeters);
                            theFeatureBuffer.set_Value(latFld, thisStation.Latitude);
                            theFeatureBuffer.set_Value(longFld, thisStation.Longitude);
                            theFeatureBuffer.set_Value(dsFld, thisStation.DataSourceID);
                            theFeatureBuffer.Shape = thisStation.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert Stations");
                theEditor.StartOperation();

                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_StationsFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    Station thisStation = m_StationsDictionary[theID];
                    theFeature.set_Value(fieldFld, thisStation.FieldID);
                    theFeature.set_Value(lblFld, thisStation.Label);
                    theFeature.set_Value(plotFld, thisStation.PlotAtScale);
                    theFeature.set_Value(locConfFld, thisStation.LocationConfidenceMeters);
                    theFeature.set_Value(latFld, thisStation.Latitude);
                    theFeature.set_Value(longFld, thisStation.Longitude);
                    theFeature.set_Value(dsFld, thisStation.DataSourceID);
                    theFeature.Shape = thisStation.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update Stations");
            }
            catch { theEditor.StopOperation("Stations Management Failure"); }
        }
    }
}

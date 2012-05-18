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
    class SamplePointsAccess
    {
        IFeatureClass m_SamplePointsFC;
        IWorkspace m_theWorkspace;

        public SamplePointsAccess(IWorkspace theWorkspace)
        {
            m_SamplePointsFC = commonFunctions.OpenFeatureClass(theWorkspace, "SamplePoints");
            m_theWorkspace = theWorkspace;
        }

        public struct SamplePoint
        {
            public string SamplePoints_ID;
            public string FieldID;
            public string StationID;
            public string Label;
            public int PlotAtScale;
            public double LocationConfidenceMeters;
            public string Notes;
            public string Symbol;
            public string DataSourceID;
            public IPoint Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, SamplePoint> m_SamplePointsDictionary = new Dictionary<string, SamplePoint>();
        public Dictionary<string, SamplePoint> SamplePointsDictionary
        {
            get { return m_SamplePointsDictionary; }
        }

        public void ClearSamplePoints()
        {
            m_SamplePointsDictionary.Clear();
        }

        public void AddSamplePoints(string SqlWhereClause)
        {
            int idFld = m_SamplePointsFC.FindField("SamplePoints_ID");
            int fieldFld = m_SamplePointsFC.FindField("FieldID");
            int staFld = m_SamplePointsFC.FindField("StationID");
            int lblFld = m_SamplePointsFC.FindField("Label");
            int plotFld = m_SamplePointsFC.FindField("PlotAtScale");
            int locConfFld = m_SamplePointsFC.FindField("LocationConfidenceMeters");
            int notesFld = m_SamplePointsFC.FindField("Notes");
            int symbFld = m_SamplePointsFC.FindField("Symbol");
            int dsFld = m_SamplePointsFC.FindField("DataSourceID");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_SamplePointsFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                SamplePoint anSamplePoint = new SamplePoint();
                anSamplePoint.SamplePoints_ID = theFeature.get_Value(idFld).ToString();
                anSamplePoint.FieldID = theFeature.get_Value(fieldFld).ToString();
                anSamplePoint.StationID = theFeature.get_Value(staFld).ToString();
                anSamplePoint.Label = theFeature.get_Value(lblFld).ToString();
                bool result = int.TryParse(theFeature.get_Value(plotFld).ToString(), out anSamplePoint.PlotAtScale);
                result = double.TryParse(theFeature.get_Value(locConfFld).ToString(), out anSamplePoint.LocationConfidenceMeters);
                anSamplePoint.Notes = theFeature.get_Value(notesFld).ToString();
                anSamplePoint.Symbol = theFeature.get_Value(symbFld).ToString();
                anSamplePoint.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anSamplePoint.Shape = (IPoint)theFeature.Shape;
                anSamplePoint.RequiresUpdate = true;

                m_SamplePointsDictionary.Add(anSamplePoint.SamplePoints_ID, anSamplePoint);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewSamplePoint(string StationID, 
            string Label, int PlotAtScale, double LocationConfidenceMeters, 
            string Notes, string DataSourceID, string Symbol, IPoint Shape)
        {
            SamplePoint newSamplePoint = new SamplePoint();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newSamplePoint.SamplePoints_ID = SysInfoTable.ProjAbbr + ".SamplePoints." + SysInfoTable.GetNextIdValue("SamplePoints");
            newSamplePoint.FieldID = StationID;
            newSamplePoint.Label = Label;
            newSamplePoint.PlotAtScale = PlotAtScale;
            newSamplePoint.LocationConfidenceMeters = LocationConfidenceMeters;
            newSamplePoint.Notes = Notes;
            newSamplePoint.Symbol = Symbol;
            newSamplePoint.DataSourceID = DataSourceID;
            newSamplePoint.Shape = Shape;
            newSamplePoint.RequiresUpdate = false;

            m_SamplePointsDictionary.Add(newSamplePoint.SamplePoints_ID, newSamplePoint);
            return newSamplePoint.SamplePoints_ID;
        }

        public void UpdateSamplePoint(SamplePoint theSamplePoint)
        {
            try { m_SamplePointsDictionary.Remove(theSamplePoint.SamplePoints_ID); }
            catch { }

            theSamplePoint.RequiresUpdate = true;
            m_SamplePointsDictionary.Add(theSamplePoint.SamplePoints_ID, theSamplePoint);
        }

        public void SaveSamplePoints()
        {
            int idFld = m_SamplePointsFC.FindField("SamplePoints_ID");
            int fieldFld = m_SamplePointsFC.FindField("FieldID");
            int staFld = m_SamplePointsFC.FindField("StationID");
            int lblFld = m_SamplePointsFC.FindField("Label");
            int plotFld = m_SamplePointsFC.FindField("PlotAtScale");
            int locConfFld = m_SamplePointsFC.FindField("LocationConfidenceMeters");
            int notesFld = m_SamplePointsFC.FindField("Notes");
            int symbFld = m_SamplePointsFC.FindField("Symbol");
            int dsFld = m_SamplePointsFC.FindField("DataSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "SamplePoints_ID = '";
                IFeatureCursor insertCursor = m_SamplePointsFC.Insert(true);

                foreach (KeyValuePair<string, SamplePoint> aDictionaryEntry in m_SamplePointsDictionary)
                {
                    SamplePoint thisSamplePoint = (SamplePoint)aDictionaryEntry.Value;
                    switch (thisSamplePoint.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisSamplePoint.SamplePoints_ID + "' OR SamplePoints_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_SamplePointsFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisSamplePoint.SamplePoints_ID);
                            theFeatureBuffer.set_Value(fieldFld, thisSamplePoint.FieldID);
                            theFeatureBuffer.set_Value(staFld, thisSamplePoint.StationID);
                            theFeatureBuffer.set_Value(lblFld, thisSamplePoint.Label);
                            theFeatureBuffer.set_Value(plotFld, thisSamplePoint.PlotAtScale);
                            theFeatureBuffer.set_Value(locConfFld, thisSamplePoint.LocationConfidenceMeters);
                            theFeatureBuffer.set_Value(notesFld, thisSamplePoint.Notes);
                            theFeatureBuffer.set_Value(symbFld, thisSamplePoint.Symbol);
                            theFeatureBuffer.set_Value(dsFld, thisSamplePoint.DataSourceID);
                            theFeatureBuffer.Shape = thisSamplePoint.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert SamplePoints");
                theEditor.StartOperation();

                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_SamplePointsFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    SamplePoint thisSamplePoint = m_SamplePointsDictionary[theID];
                    theFeature.set_Value(fieldFld, thisSamplePoint.FieldID);
                    theFeature.set_Value(staFld, thisSamplePoint.StationID);
                    theFeature.set_Value(lblFld, thisSamplePoint.Label);
                    theFeature.set_Value(plotFld, thisSamplePoint.PlotAtScale);
                    theFeature.set_Value(locConfFld, thisSamplePoint.LocationConfidenceMeters);
                    theFeature.set_Value(notesFld, thisSamplePoint.Notes);
                    theFeature.set_Value(symbFld, thisSamplePoint.Symbol);
                    theFeature.set_Value(dsFld, thisSamplePoint.DataSourceID);
                    theFeature.Shape = thisSamplePoint.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update SamplePoints");
            }
            catch { theEditor.StopOperation("SamplePoints Management Failure"); }
        }
    }
}

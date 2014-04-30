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
    class GenericSamplesAccess
    {
        IFeatureClass m_GenericSamplesFC;
        IWorkspace m_theWorkspace;

        public GenericSamplesAccess(IWorkspace theWorkspace)
        {
            m_GenericSamplesFC = commonFunctions.OpenFeatureClass(theWorkspace, "GenericSamples");
            m_theWorkspace = theWorkspace;
        }

        public struct GenericSample
        {
            public string GenericSamples_ID;
            public string FieldID;
            public string StationID;
            public string Label;
            public int PlotAtScale;
            public double LocationConfidenceMeters;
            public string Notes;
            public string RuleID;
            public string DataSourceID;
            public IPoint Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, GenericSample> m_GenericSamplesDictionary = new Dictionary<string, GenericSample>();
        public Dictionary<string, GenericSample> GenericSamplesDictionary
        {
            get { return m_GenericSamplesDictionary; }
        }

        public void ClearGenericSamples()
        {
            m_GenericSamplesDictionary.Clear();
        }

        public void AddGenericSamples(string SqlWhereClause)
        {
            int idFld = m_GenericSamplesFC.FindField("GenericSamples_ID");
            int fieldFld = m_GenericSamplesFC.FindField("FieldID");
            int staFld = m_GenericSamplesFC.FindField("StationID");
            int lblFld = m_GenericSamplesFC.FindField("Label");
            int plotFld = m_GenericSamplesFC.FindField("PlotAtScale");
            int locConfFld = m_GenericSamplesFC.FindField("LocationConfidenceMeters");
            int notesFld = m_GenericSamplesFC.FindField("Notes");
            int symbFld = m_GenericSamplesFC.FindField("RuleID");
            int dsFld = m_GenericSamplesFC.FindField("DataSourceID");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_GenericSamplesFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                GenericSample anGenericSample = new GenericSample();
                anGenericSample.GenericSamples_ID = theFeature.get_Value(idFld).ToString();
                anGenericSample.FieldID = theFeature.get_Value(fieldFld).ToString();
                anGenericSample.StationID = theFeature.get_Value(staFld).ToString();
                anGenericSample.Label = theFeature.get_Value(lblFld).ToString();
                bool result = int.TryParse(theFeature.get_Value(plotFld).ToString(), out anGenericSample.PlotAtScale);
                result = double.TryParse(theFeature.get_Value(locConfFld).ToString(), out anGenericSample.LocationConfidenceMeters);
                anGenericSample.Notes = theFeature.get_Value(notesFld).ToString();
                anGenericSample.RuleID = theFeature.get_Value(symbFld).ToString();
                anGenericSample.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anGenericSample.Shape = (IPoint)theFeature.Shape;
                anGenericSample.RequiresUpdate = true;

                m_GenericSamplesDictionary.Add(anGenericSample.GenericSamples_ID, anGenericSample);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewGenericSample(string StationID, 
            string Label, int PlotAtScale, double LocationConfidenceMeters, 
            string Notes, string DataSourceID, string RuleID, IPoint Shape)
        {
            GenericSample newGenericSample = new GenericSample();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newGenericSample.GenericSamples_ID = SysInfoTable.ProjAbbr + ".GenericSamples." + SysInfoTable.GetNextIdValue("GenericSamples");
            newGenericSample.FieldID = StationID;
            newGenericSample.Label = Label;
            newGenericSample.PlotAtScale = PlotAtScale;
            newGenericSample.LocationConfidenceMeters = LocationConfidenceMeters;
            newGenericSample.Notes = Notes;
            newGenericSample.RuleID = RuleID;
            newGenericSample.DataSourceID = DataSourceID;
            newGenericSample.Shape = Shape;
            newGenericSample.RequiresUpdate = false;

            m_GenericSamplesDictionary.Add(newGenericSample.GenericSamples_ID, newGenericSample);
            return newGenericSample.GenericSamples_ID;
        }

        public void UpdateGenericSample(GenericSample theGenericSample)
        {
            try { m_GenericSamplesDictionary.Remove(theGenericSample.GenericSamples_ID); }
            catch { }

            theGenericSample.RequiresUpdate = true;
            m_GenericSamplesDictionary.Add(theGenericSample.GenericSamples_ID, theGenericSample);
        }

        public void SaveGenericSamples()
        {
            int idFld = m_GenericSamplesFC.FindField("GenericSamples_ID");
            int fieldFld = m_GenericSamplesFC.FindField("FieldID");
            int staFld = m_GenericSamplesFC.FindField("StationID");
            int lblFld = m_GenericSamplesFC.FindField("Label");
            int plotFld = m_GenericSamplesFC.FindField("PlotAtScale");
            int locConfFld = m_GenericSamplesFC.FindField("LocationConfidenceMeters");
            int notesFld = m_GenericSamplesFC.FindField("Notes");
            int symbFld = m_GenericSamplesFC.FindField("RuleID");
            int dsFld = m_GenericSamplesFC.FindField("DataSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "GenericSamples_ID = '";
                IFeatureCursor insertCursor = m_GenericSamplesFC.Insert(true);

                foreach (KeyValuePair<string, GenericSample> aDictionaryEntry in m_GenericSamplesDictionary)
                {
                    GenericSample thisGenericSample = (GenericSample)aDictionaryEntry.Value;
                    switch (thisGenericSample.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisGenericSample.GenericSamples_ID + "' OR GenericSamples_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_GenericSamplesFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisGenericSample.GenericSamples_ID);
                            theFeatureBuffer.set_Value(fieldFld, thisGenericSample.FieldID);
                            theFeatureBuffer.set_Value(staFld, thisGenericSample.StationID);
                            theFeatureBuffer.set_Value(lblFld, thisGenericSample.Label);
                            theFeatureBuffer.set_Value(plotFld, thisGenericSample.PlotAtScale);
                            theFeatureBuffer.set_Value(locConfFld, thisGenericSample.LocationConfidenceMeters);
                            theFeatureBuffer.set_Value(notesFld, thisGenericSample.Notes);
                            theFeatureBuffer.set_Value(symbFld, thisGenericSample.RuleID);
                            theFeatureBuffer.set_Value(dsFld, thisGenericSample.DataSourceID);
                            theFeatureBuffer.Shape = thisGenericSample.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert GenericSamples");
                theEditor.StartOperation();

                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_GenericSamplesFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    GenericSample thisGenericSample = m_GenericSamplesDictionary[theID];
                    theFeature.set_Value(fieldFld, thisGenericSample.FieldID);
                    theFeature.set_Value(staFld, thisGenericSample.StationID);
                    theFeature.set_Value(lblFld, thisGenericSample.Label);
                    theFeature.set_Value(plotFld, thisGenericSample.PlotAtScale);
                    theFeature.set_Value(locConfFld, thisGenericSample.LocationConfidenceMeters);
                    theFeature.set_Value(notesFld, thisGenericSample.Notes);
                    theFeature.set_Value(symbFld, thisGenericSample.RuleID);
                    theFeature.set_Value(dsFld, thisGenericSample.DataSourceID);
                    theFeature.Shape = thisGenericSample.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update GenericSamples");
            }
            catch { theEditor.StopOperation("GenericSamples Management Failure"); }
        }
    }
}

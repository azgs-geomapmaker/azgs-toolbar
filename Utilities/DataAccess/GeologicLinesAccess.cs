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
    class GeologicLinesAccess
    {
        IFeatureClass m_GeologicLinesFC;
        IWorkspace m_theWorkspace;

        public GeologicLinesAccess(IWorkspace theWorkspace)
        {
            m_GeologicLinesFC = commonFunctions.OpenFeatureClass(theWorkspace, "GeologicLines");
            m_theWorkspace = theWorkspace;
        }

        public struct GeologicLine
        {
            public string GeologicLines_ID;
            public string Type;
            public double LocationConfidenceMeters;
            public string ExistenceConfidence;
            public string IdentityConfidence;
            public string RuleID;
            public string Symbol;
            public string Label;
            public string Notes;
            public string DataSourceID;
            public IPolyline Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, GeologicLine> m_GeologicLinesDictionary = new Dictionary<string, GeologicLine>();
        public Dictionary<string, GeologicLine> GeologicLinesDictionary
        {
            get { return m_GeologicLinesDictionary; }
        }

        public void ClearGeologicLines()
        {
            m_GeologicLinesDictionary.Clear();
        }

        public void AddGeologicLines(string SqlWhereClause)
        {
            int idFld = m_GeologicLinesFC.FindField("GeologicLines_ID");
            int typeFld = m_GeologicLinesFC.FindField("Type");
            int locConfFld = m_GeologicLinesFC.FindField("LocationConfidenceMeters");
            int exConfFld = m_GeologicLinesFC.FindField("ExistenceConfidence");
            int idConfFld = m_GeologicLinesFC.FindField("IdentityConfidence");
            int symbFld = m_GeologicLinesFC.FindField("Symbol");
            int lblFld = m_GeologicLinesFC.FindField("Label");
            int notesFld = m_GeologicLinesFC.FindField("Notes");
            int dsFld = m_GeologicLinesFC.FindField("DataSourceID");
            int symFld = m_GeologicLinesFC.FindField("RuleID");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_GeologicLinesFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                GeologicLine anGeologicLine = new GeologicLine();
                anGeologicLine.GeologicLines_ID = theFeature.get_Value(idFld).ToString();
                anGeologicLine.Type = theFeature.get_Value(typeFld).ToString();
                string locConfFldStr = theFeature.get_Value(locConfFld).ToString();
                anGeologicLine.LocationConfidenceMeters = double.Parse(string.IsNullOrEmpty(locConfFldStr) ? "-9999" : locConfFldStr);
                anGeologicLine.ExistenceConfidence = theFeature.get_Value(exConfFld).ToString();
                anGeologicLine.IdentityConfidence = theFeature.get_Value(idConfFld).ToString();
                anGeologicLine.Symbol = theFeature.get_Value(symbFld).ToString();
                anGeologicLine.Label = theFeature.get_Value(lblFld).ToString();
                anGeologicLine.Notes = theFeature.get_Value(notesFld).ToString();
                anGeologicLine.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anGeologicLine.RuleID = theFeature.get_Value(symFld).ToString();
                anGeologicLine.Shape = (IPolyline)theFeature.Shape;
                anGeologicLine.RequiresUpdate = true;

                m_GeologicLinesDictionary.Add(anGeologicLine.GeologicLines_ID, anGeologicLine);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewGeologicLine(string Type, double LocationConfidenceMeters, string ExistenceConfidence, string IdentityConfidence,
            string Symbol, string Label, string Notes, string DataSourceID, string RuleID, IPolyline Shape)
        {
            GeologicLine newGeologicLine = new GeologicLine();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newGeologicLine.GeologicLines_ID = SysInfoTable.ProjAbbr + ".GeologicLines." + SysInfoTable.GetNextIdValue("GeologicLines");
            newGeologicLine.Type = Type;
            newGeologicLine.LocationConfidenceMeters = LocationConfidenceMeters;
            newGeologicLine.ExistenceConfidence = ExistenceConfidence;
            newGeologicLine.IdentityConfidence = IdentityConfidence;
            newGeologicLine.Symbol = Symbol;
            newGeologicLine.Label = Label;
            newGeologicLine.Notes = Notes;
            newGeologicLine.DataSourceID = DataSourceID;
            newGeologicLine.RuleID = RuleID;
            newGeologicLine.Shape = Shape;
            newGeologicLine.RequiresUpdate = false;

            m_GeologicLinesDictionary.Add(newGeologicLine.GeologicLines_ID, newGeologicLine);
            return newGeologicLine.GeologicLines_ID;
        }

        public void UpdateGeologicLine(GeologicLine theGeologicLine)
        {
            try { m_GeologicLinesDictionary.Remove(theGeologicLine.GeologicLines_ID); }
            catch { }

            theGeologicLine.RequiresUpdate = true;
            m_GeologicLinesDictionary.Add(theGeologicLine.GeologicLines_ID, theGeologicLine);
        }

        public void SaveGeologicLines()
        {
            int idFld = m_GeologicLinesFC.FindField("GeologicLines_ID");
            int typeFld = m_GeologicLinesFC.FindField("Type");
            int locConfFld = m_GeologicLinesFC.FindField("LocationConfidenceMeters");
            int exConfFld = m_GeologicLinesFC.FindField("ExistenceConfidence");
            int idConfFld = m_GeologicLinesFC.FindField("IdentityConfidence");
            int symbFld = m_GeologicLinesFC.FindField("Symbol");
            int lblFld = m_GeologicLinesFC.FindField("Label");
            int notesFld = m_GeologicLinesFC.FindField("Notes");
            int dsFld = m_GeologicLinesFC.FindField("DataSourceID");
            int symFld = m_GeologicLinesFC.FindField("RuleID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "GeologicLines_ID = '";
                IFeatureCursor insertCursor = m_GeologicLinesFC.Insert(true);

                foreach (KeyValuePair<string, GeologicLine> aDictionaryEntry in m_GeologicLinesDictionary)
                {
                    GeologicLine thisGeologicLine = (GeologicLine)aDictionaryEntry.Value;
                    switch (thisGeologicLine.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisGeologicLine.GeologicLines_ID + "' OR GeologicLines_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_GeologicLinesFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisGeologicLine.GeologicLines_ID);
                            theFeatureBuffer.set_Value(typeFld, thisGeologicLine.Type);
                            theFeatureBuffer.set_Value(locConfFld, thisGeologicLine.LocationConfidenceMeters);
                            theFeatureBuffer.set_Value(exConfFld, thisGeologicLine.ExistenceConfidence);
                            theFeatureBuffer.set_Value(idConfFld, thisGeologicLine.IdentityConfidence);
                            theFeatureBuffer.set_Value(symbFld, thisGeologicLine.Symbol);
                            theFeatureBuffer.set_Value(lblFld, thisGeologicLine.Label);
                            theFeatureBuffer.set_Value(notesFld, thisGeologicLine.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisGeologicLine.DataSourceID);
                            theFeatureBuffer.set_Value(symFld, thisGeologicLine.RuleID);
                            theFeatureBuffer.Shape = thisGeologicLine.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert GeologicLines");

                if (updateWhereClause == "GeologicLines_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 24);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_GeologicLinesFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    GeologicLine thisGeologicLine = m_GeologicLinesDictionary[theID];
                    theFeature.set_Value(typeFld, thisGeologicLine.Type);
                    theFeature.set_Value(locConfFld, thisGeologicLine.LocationConfidenceMeters);
                    theFeature.set_Value(exConfFld, thisGeologicLine.ExistenceConfidence);
                    theFeature.set_Value(idConfFld, thisGeologicLine.IdentityConfidence);
                    theFeature.set_Value(symbFld, thisGeologicLine.Symbol);
                    theFeature.set_Value(lblFld, thisGeologicLine.Label);
                    theFeature.set_Value(notesFld, thisGeologicLine.Notes);
                    theFeature.set_Value(dsFld, thisGeologicLine.DataSourceID);
                    theFeature.set_Value(symFld, thisGeologicLine.RuleID);
                    theFeature.Shape = thisGeologicLine.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update GeologicLines");
            }
            catch { theEditor.StopOperation("GeologicLines Management Failure"); }
        }

        public void DeleteGeologicLines(GeologicLine theGeologicLine)
        {
            try { m_GeologicLinesDictionary.Remove(theGeologicLine.GeologicLines_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "GeologicLines_ID = '" + theGeologicLine.GeologicLines_ID + "'";

                ITable GeologicLinesTable = m_GeologicLinesFC as ITable;
                GeologicLinesTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete GeologicLines");
            }
            catch (Exception e) { theEditor.StopOperation("GeologicLines Management Failure"); }
        }
    }
}

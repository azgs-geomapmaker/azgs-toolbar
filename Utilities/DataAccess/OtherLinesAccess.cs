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
    class OtherLinesAccess
    {
        IFeatureClass m_OtherLinesFC;
        IWorkspace m_theWorkspace;

        public OtherLinesAccess(IWorkspace theWorkspace)
        {
            m_OtherLinesFC = commonFunctions.OpenFeatureClass(theWorkspace, "OtherLines");
            m_theWorkspace = theWorkspace;
        }

        public struct OtherLine
        {
            public string OtherLines_ID;
            public string Type;
            public double LocationConfidenceMeters;
            public string ExistenceConfidence;
            public string IdentityConfidence;
            public string Symbol;
            public string Label;
            public string Notes;
            public string DataSourceID;
            public IPolyline Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, OtherLine> m_OtherLinesDictionary = new Dictionary<string, OtherLine>();
        public Dictionary<string, OtherLine> OtherLinesDictionary
        {
            get { return m_OtherLinesDictionary; }
        }

        public void ClearOtherLines()
        {
            m_OtherLinesDictionary.Clear();
        }

        public void AddOtherLines(string SqlWhereClause)
        {
            int idFld = m_OtherLinesFC.FindField("OtherLines_ID");
            int typeFld = m_OtherLinesFC.FindField("Type");
            int locConfFld = m_OtherLinesFC.FindField("LocationConfidenceMeters");
            int exConfFld = m_OtherLinesFC.FindField("ExistenceConfidence");
            int idConfFld = m_OtherLinesFC.FindField("IdentityConfidence");
            int lblFld = m_OtherLinesFC.FindField("Label");
            int notesFld = m_OtherLinesFC.FindField("Notes");
            int dsFld = m_OtherLinesFC.FindField("DataSourceID");
            int symFld = m_OtherLinesFC.FindField("Symbol");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_OtherLinesFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                OtherLine anOtherLine = new OtherLine();
                anOtherLine.OtherLines_ID = theFeature.get_Value(idFld).ToString();
                anOtherLine.Type = theFeature.get_Value(typeFld).ToString();
                anOtherLine.LocationConfidenceMeters = double.Parse(theFeature.get_Value(locConfFld).ToString());
                anOtherLine.ExistenceConfidence = theFeature.get_Value(exConfFld).ToString();
                anOtherLine.IdentityConfidence = theFeature.get_Value(idConfFld).ToString();
                anOtherLine.Label = theFeature.get_Value(lblFld).ToString();
                anOtherLine.Notes = theFeature.get_Value(notesFld).ToString();
                anOtherLine.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anOtherLine.Symbol = theFeature.get_Value(symFld).ToString();
                anOtherLine.Shape = (IPolyline)theFeature.Shape;
                anOtherLine.RequiresUpdate = true;

                m_OtherLinesDictionary.Add(anOtherLine.OtherLines_ID, anOtherLine);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewOtherLine(string Type, double LocationConfidenceMeters, string ExistenceConfidence, string IdentityConfidence,
            string Label, string Notes, string DataSourceID, string Symbol, IPolyline Shape)
        {
            OtherLine newOtherLine = new OtherLine();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newOtherLine.OtherLines_ID = SysInfoTable.ProjAbbr + ".OtherLines." + SysInfoTable.GetNextIdValue("OtherLines");
            newOtherLine.Type = Type;
            newOtherLine.LocationConfidenceMeters = LocationConfidenceMeters;
            newOtherLine.ExistenceConfidence = ExistenceConfidence;
            newOtherLine.IdentityConfidence = IdentityConfidence;
            newOtherLine.Label = Label;
            newOtherLine.Notes = Notes;
            newOtherLine.DataSourceID = DataSourceID;
            newOtherLine.Symbol = Symbol;
            newOtherLine.Shape = Shape;
            newOtherLine.RequiresUpdate = false;

            m_OtherLinesDictionary.Add(newOtherLine.OtherLines_ID, newOtherLine);
            return newOtherLine.OtherLines_ID;
        }

        public void UpdateOtherLine(OtherLine theOtherLine)
        {
            try { m_OtherLinesDictionary.Remove(theOtherLine.OtherLines_ID); }
            catch { }

            theOtherLine.RequiresUpdate = true;
            m_OtherLinesDictionary.Add(theOtherLine.OtherLines_ID, theOtherLine);
        }

        public void SaveOtherLines()
        {
            int idFld = m_OtherLinesFC.FindField("OtherLines_ID");
            int typeFld = m_OtherLinesFC.FindField("Type");
            int locConfFld = m_OtherLinesFC.FindField("LocationConfidenceMeters");
            int exConfFld = m_OtherLinesFC.FindField("ExistenceConfidence");
            int idConfFld = m_OtherLinesFC.FindField("IdentityConfidence");
            int lblFld = m_OtherLinesFC.FindField("Label");
            int notesFld = m_OtherLinesFC.FindField("Notes");
            int dsFld = m_OtherLinesFC.FindField("DataSourceID");
            int symFld = m_OtherLinesFC.FindField("Symbol");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "OtherLines_ID = '";
                IFeatureCursor insertCursor = m_OtherLinesFC.Insert(true);

                foreach (KeyValuePair<string, OtherLine> aDictionaryEntry in m_OtherLinesDictionary)
                {
                    OtherLine thisOtherLine = (OtherLine)aDictionaryEntry.Value;
                    switch (thisOtherLine.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisOtherLine.OtherLines_ID + "' OR OtherLines_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_OtherLinesFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisOtherLine.OtherLines_ID);
                            theFeatureBuffer.set_Value(typeFld, thisOtherLine.Type);
                            theFeatureBuffer.set_Value(locConfFld, thisOtherLine.LocationConfidenceMeters);
                            theFeatureBuffer.set_Value(exConfFld, thisOtherLine.ExistenceConfidence);
                            theFeatureBuffer.set_Value(idConfFld, thisOtherLine.IdentityConfidence);
                            theFeatureBuffer.set_Value(lblFld, thisOtherLine.Label);
                            theFeatureBuffer.set_Value(notesFld, thisOtherLine.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisOtherLine.DataSourceID);
                            theFeatureBuffer.set_Value(symFld, thisOtherLine.Symbol);
                            theFeatureBuffer.Shape = thisOtherLine.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert OtherLines");

                if (updateWhereClause == "OtherLines_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 21);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_OtherLinesFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    OtherLine thisOtherLine = m_OtherLinesDictionary[theID];
                    theFeature.set_Value(typeFld, thisOtherLine.Type);
                    theFeature.set_Value(locConfFld, thisOtherLine.LocationConfidenceMeters);
                    theFeature.set_Value(exConfFld, thisOtherLine.ExistenceConfidence);
                    theFeature.set_Value(idConfFld, thisOtherLine.IdentityConfidence);
                    theFeature.set_Value(lblFld, thisOtherLine.Label);
                    theFeature.set_Value(notesFld, thisOtherLine.Notes);
                    theFeature.set_Value(dsFld, thisOtherLine.DataSourceID);
                    theFeature.set_Value(symFld, thisOtherLine.Symbol);
                    theFeature.Shape = thisOtherLine.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update OtherLines");
            }
            catch { theEditor.StopOperation("OtherLines Management Failure"); }
        }

        public void DeleteOtherLines(OtherLine theOtherLine)
        {
            try { m_OtherLinesDictionary.Remove(theOtherLine.OtherLines_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "OtherLines_ID = '" + theOtherLine.OtherLines_ID + "'";

                ITable OtherLinesTable = m_OtherLinesFC as ITable;
                OtherLinesTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete OtherLines");
            }
            catch (Exception e) { theEditor.StopOperation("OtherLines Management Failure"); }
        }
    }
}

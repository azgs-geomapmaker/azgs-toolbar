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
    class OtherPolysAccess
    {
        IFeatureClass m_OtherPolysFC;
        IWorkspace m_theWorkspace;

        public OtherPolysAccess(IWorkspace theWorkspace)
        {
            m_OtherPolysFC = commonFunctions.OpenFeatureClass(theWorkspace, "OtherPolys");
            m_theWorkspace = theWorkspace;
        }

        public struct OtherPoly
        {
            public string OtherPolys_ID;
            public string MapUnit;
            public string IdentityConfidence;
            public string Label;
            public string Notes;
            public string DataSourceID;
            public string Symbol;
            public IPolygon Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, OtherPoly> m_OtherPolysDictionary = new Dictionary<string, OtherPoly>();
        public Dictionary<string, OtherPoly> OtherPolysDictionary
        {
            get { return m_OtherPolysDictionary; }
        }

        public void ClearOtherPolys()
        {
            m_OtherPolysDictionary.Clear();
        }

        public void AddOtherPolys(string SqlWhereClause)
        {
            int idFld = m_OtherPolysFC.FindField("OtherPolys_ID");
            int unitFld = m_OtherPolysFC.FindField("MapUnit");
            int idConfFld = m_OtherPolysFC.FindField("IdentityConfidence");
            int lblFld = m_OtherPolysFC.FindField("Label");
            int notesFld = m_OtherPolysFC.FindField("Notes");
            int dsFld = m_OtherPolysFC.FindField("DataSourceID");
            int symFld = m_OtherPolysFC.FindField("Symbol");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_OtherPolysFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                OtherPoly anOtherPoly = new OtherPoly();
                anOtherPoly.OtherPolys_ID = theFeature.get_Value(idFld).ToString();
                anOtherPoly.MapUnit = theFeature.get_Value(unitFld).ToString();
                anOtherPoly.IdentityConfidence = theFeature.get_Value(idConfFld).ToString();
                anOtherPoly.Label = theFeature.get_Value(lblFld).ToString();
                anOtherPoly.Notes = theFeature.get_Value(notesFld).ToString();
                anOtherPoly.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anOtherPoly.Symbol = theFeature.get_Value(symFld).ToString();
                anOtherPoly.Shape = (IPolygon)theFeature.Shape;
                anOtherPoly.RequiresUpdate = true;

                m_OtherPolysDictionary.Add(anOtherPoly.OtherPolys_ID, anOtherPoly);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewOtherPoly(string MapUnit, string IdentityConfidence,
            string Label, string Notes, string DataSourceID, string Symbol, IPolygon Shape)
        {
            OtherPoly newOtherPoly = new OtherPoly();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newOtherPoly.OtherPolys_ID = SysInfoTable.ProjAbbr + ".OtherPolys." + SysInfoTable.GetNextIdValue("OtherPolys");
            newOtherPoly.MapUnit = MapUnit;
            newOtherPoly.IdentityConfidence = IdentityConfidence;
            newOtherPoly.Label = Label;
            newOtherPoly.Notes = Notes;
            newOtherPoly.DataSourceID = DataSourceID;
            newOtherPoly.Symbol = Symbol;
            newOtherPoly.Shape = Shape;
            newOtherPoly.RequiresUpdate = false;

            m_OtherPolysDictionary.Add(newOtherPoly.OtherPolys_ID, newOtherPoly);
            return newOtherPoly.OtherPolys_ID;
        }

        public void UpdateOtherPoly(OtherPoly theOtherPoly)
        {
            try { m_OtherPolysDictionary.Remove(theOtherPoly.OtherPolys_ID); }
            catch { }

            theOtherPoly.RequiresUpdate = true;
            m_OtherPolysDictionary.Add(theOtherPoly.OtherPolys_ID, theOtherPoly);
        }

        public void SaveOtherPolys()
        {
            int idFld = m_OtherPolysFC.FindField("OtherPolys_ID");
            int unitFld = m_OtherPolysFC.FindField("MapUnit");
            int idConfFld = m_OtherPolysFC.FindField("IdentityConfidence");
            int lblFld = m_OtherPolysFC.FindField("Label");
            int notesFld = m_OtherPolysFC.FindField("Notes");
            int dsFld = m_OtherPolysFC.FindField("DataSourceID");
            int symFld = m_OtherPolysFC.FindField("Symbol");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "OtherPolys_ID = '";
                IFeatureCursor insertCursor = m_OtherPolysFC.Insert(true);

                foreach (KeyValuePair<string, OtherPoly> aDictionaryEntry in m_OtherPolysDictionary)
                {
                    OtherPoly thisOtherPoly = (OtherPoly)aDictionaryEntry.Value;
                    switch (thisOtherPoly.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisOtherPoly.OtherPolys_ID + "' OR OtherPolys_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_OtherPolysFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisOtherPoly.OtherPolys_ID);
                            theFeatureBuffer.set_Value(unitFld, thisOtherPoly.MapUnit);
                            theFeatureBuffer.set_Value(idConfFld, thisOtherPoly.IdentityConfidence);
                            theFeatureBuffer.set_Value(lblFld, thisOtherPoly.Label);
                            theFeatureBuffer.set_Value(notesFld, thisOtherPoly.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisOtherPoly.DataSourceID);
                            theFeatureBuffer.set_Value(symFld, thisOtherPoly.Symbol);
                            theFeatureBuffer.Shape = thisOtherPoly.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert OtherPolys");

                if (updateWhereClause == "OtherPolys_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 23);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_OtherPolysFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    OtherPoly thisOtherPoly = m_OtherPolysDictionary[theID];
                    theFeature.set_Value(unitFld, thisOtherPoly.MapUnit);
                    theFeature.set_Value(idConfFld, thisOtherPoly.IdentityConfidence);
                    theFeature.set_Value(lblFld, thisOtherPoly.Label);
                    theFeature.set_Value(notesFld, thisOtherPoly.Notes);
                    theFeature.set_Value(dsFld, thisOtherPoly.DataSourceID);
                    theFeature.set_Value(symFld, thisOtherPoly.Symbol);
                    theFeature.Shape = thisOtherPoly.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update OtherPolys");
            }
            catch { theEditor.StopOperation("OtherPolys Management Failure"); }
        }

        public void DeleteOtherPolys(OtherPoly theOtherPoly)
        {
            try { m_OtherPolysDictionary.Remove(theOtherPoly.OtherPolys_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "OtherPolys_ID = '" + theOtherPoly.OtherPolys_ID + "'";

                ITable OtherPolysTable = m_OtherPolysFC as ITable;
                OtherPolysTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete OtherPolys");
            }
            catch (Exception e) { theEditor.StopOperation("OtherPolys Management Failure"); }
        }
    }
}

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
    class OverlayPolysAccess
    {
        IFeatureClass m_OverlayPolysFC;
        IWorkspace m_theWorkspace;

        public OverlayPolysAccess(IWorkspace theWorkspace)
        {
            m_OverlayPolysFC = commonFunctions.OpenFeatureClass(theWorkspace, "OverlayPolys");
            m_theWorkspace = theWorkspace;
        }

        public struct OverlayPoly
        {
            public string OverlayPolys_ID;
            public string MapUnit;
            public string IdentityConfidence;
            public string Label;
            public string Notes;
            public string DataSourceID;
            public string Symbol;
            public IPolygon Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, OverlayPoly> m_OverlayPolysDictionary = new Dictionary<string, OverlayPoly>();
        public Dictionary<string, OverlayPoly> OverlayPolysDictionary
        {
            get { return m_OverlayPolysDictionary; }
        }

        public void ClearOverlayPolys()
        {
            m_OverlayPolysDictionary.Clear();
        }

        public void AddOverlayPolys(string SqlWhereClause)
        {
            int idFld = m_OverlayPolysFC.FindField("OverlayPolys_ID");
            int unitFld = m_OverlayPolysFC.FindField("MapUnit");
            int idConfFld = m_OverlayPolysFC.FindField("IdentityConfidence");
            int lblFld = m_OverlayPolysFC.FindField("Label");
            int notesFld = m_OverlayPolysFC.FindField("Notes");
            int dsFld = m_OverlayPolysFC.FindField("DataSourceID");
            int symFld = m_OverlayPolysFC.FindField("Symbol");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_OverlayPolysFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                OverlayPoly anOverlayPoly = new OverlayPoly();
                anOverlayPoly.OverlayPolys_ID = theFeature.get_Value(idFld).ToString();
                anOverlayPoly.MapUnit = theFeature.get_Value(unitFld).ToString();
                anOverlayPoly.IdentityConfidence = theFeature.get_Value(idConfFld).ToString();
                anOverlayPoly.Label = theFeature.get_Value(lblFld).ToString();
                anOverlayPoly.Notes = theFeature.get_Value(notesFld).ToString();
                anOverlayPoly.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anOverlayPoly.Symbol = theFeature.get_Value(symFld).ToString();
                anOverlayPoly.Shape = (IPolygon)theFeature.Shape;
                anOverlayPoly.RequiresUpdate = true;

                m_OverlayPolysDictionary.Add(anOverlayPoly.OverlayPolys_ID, anOverlayPoly);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewOverlayPoly(string MapUnit, string IdentityConfidence,
            string Label, string Notes, string DataSourceID, string Symbol, IPolygon Shape)
        {
            OverlayPoly newOverlayPoly = new OverlayPoly();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newOverlayPoly.OverlayPolys_ID = SysInfoTable.ProjAbbr + ".OverlayPolys." + SysInfoTable.GetNextIdValue("OverlayPolys");
            newOverlayPoly.MapUnit = MapUnit;
            newOverlayPoly.IdentityConfidence = IdentityConfidence;
            newOverlayPoly.Label = Label;
            newOverlayPoly.Notes = Notes;
            newOverlayPoly.DataSourceID = DataSourceID;
            newOverlayPoly.Symbol = Symbol;
            newOverlayPoly.Shape = Shape;
            newOverlayPoly.RequiresUpdate = false;

            m_OverlayPolysDictionary.Add(newOverlayPoly.OverlayPolys_ID, newOverlayPoly);
            return newOverlayPoly.OverlayPolys_ID;
        }

        public void UpdateOverlayPoly(OverlayPoly theOverlayPoly)
        {
            try { m_OverlayPolysDictionary.Remove(theOverlayPoly.OverlayPolys_ID); }
            catch { }

            theOverlayPoly.RequiresUpdate = true;
            m_OverlayPolysDictionary.Add(theOverlayPoly.OverlayPolys_ID, theOverlayPoly);
        }

        public void SaveOverlayPolys()
        {
            int idFld = m_OverlayPolysFC.FindField("OverlayPolys_ID");
            int unitFld = m_OverlayPolysFC.FindField("MapUnit");
            int idConfFld = m_OverlayPolysFC.FindField("IdentityConfidence");
            int lblFld = m_OverlayPolysFC.FindField("Label");
            int notesFld = m_OverlayPolysFC.FindField("Notes");
            int dsFld = m_OverlayPolysFC.FindField("DataSourceID");
            int symFld = m_OverlayPolysFC.FindField("Symbol");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "OverlayPolys_ID = '";
                IFeatureCursor insertCursor = m_OverlayPolysFC.Insert(true);

                foreach (KeyValuePair<string, OverlayPoly> aDictionaryEntry in m_OverlayPolysDictionary)
                {
                    OverlayPoly thisOverlayPoly = (OverlayPoly)aDictionaryEntry.Value;
                    switch (thisOverlayPoly.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisOverlayPoly.OverlayPolys_ID + "' OR OverlayPolys_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_OverlayPolysFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisOverlayPoly.OverlayPolys_ID);
                            theFeatureBuffer.set_Value(unitFld, thisOverlayPoly.MapUnit);
                            theFeatureBuffer.set_Value(idConfFld, thisOverlayPoly.IdentityConfidence);
                            theFeatureBuffer.set_Value(lblFld, thisOverlayPoly.Label);
                            theFeatureBuffer.set_Value(notesFld, thisOverlayPoly.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisOverlayPoly.DataSourceID);
                            theFeatureBuffer.set_Value(symFld, thisOverlayPoly.Symbol);
                            theFeatureBuffer.Shape = thisOverlayPoly.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert OverlayPolys");

                if (updateWhereClause == "OverlayPolys_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 23);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_OverlayPolysFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    OverlayPoly thisOverlayPoly = m_OverlayPolysDictionary[theID];
                    theFeature.set_Value(unitFld, thisOverlayPoly.MapUnit);
                    theFeature.set_Value(idConfFld, thisOverlayPoly.IdentityConfidence);
                    theFeature.set_Value(lblFld, thisOverlayPoly.Label);
                    theFeature.set_Value(notesFld, thisOverlayPoly.Notes);
                    theFeature.set_Value(dsFld, thisOverlayPoly.DataSourceID);
                    theFeature.set_Value(symFld, thisOverlayPoly.Symbol);
                    theFeature.Shape = thisOverlayPoly.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update OverlayPolys");
            }
            catch { theEditor.StopOperation("OverlayPolys Management Failure"); }
        }

        public void DeleteOverlayPolys(OverlayPoly theOverlayPoly)
        {
            try { m_OverlayPolysDictionary.Remove(theOverlayPoly.OverlayPolys_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "OverlayPolys_ID = '" + theOverlayPoly.OverlayPolys_ID + "'";

                ITable OverlayPolysTable = m_OverlayPolysFC as ITable;
                OverlayPolysTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete OverlayPolys");
            }
            catch (Exception e) { theEditor.StopOperation("OverlayPolys Management Failure"); }
        }
    }
}

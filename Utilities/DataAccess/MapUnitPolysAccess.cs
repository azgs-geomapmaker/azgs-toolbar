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
    class MapUnitPolysAccess
    {
        IFeatureClass m_MapUnitPolysFC;
        IWorkspace m_theWorkspace;

        public MapUnitPolysAccess(IWorkspace theWorkspace)
        {
            m_MapUnitPolysFC = commonFunctions.OpenFeatureClass(theWorkspace, "MapUnitPolys");
            m_theWorkspace = theWorkspace;
        }

        public MapUnitPolysAccess(IWorkspace theWorkspace, string mupLayerName)
        {
            m_MapUnitPolysFC = commonFunctions.OpenFeatureClass(theWorkspace, mupLayerName);
            m_theWorkspace = theWorkspace;
        }

        public struct MapUnitPoly
        {
            public string MapUnitPolys_ID;
            public string MapUnit;
            public string IdentityConfidence;
            public string Label;
            public string Notes;
            public string DataSourceID;
            public string Symbol;
            public IPolygon Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, MapUnitPoly> m_MapUnitPolysDictionary = new Dictionary<string, MapUnitPoly>();
        public Dictionary<string, MapUnitPoly> MapUnitPolysDictionary
        {
            get { return m_MapUnitPolysDictionary; }
        }

        public void ClearMapUnitPolys()
        {
            m_MapUnitPolysDictionary.Clear();
        }

        public void AddMapUnitPolys(string SqlWhereClause)
        {
            // Figure out the MapUnitPolys_ID field. It could be one of the following.
            string[] mupIdFields = { "MapUnitPolys_ID", "CMUMapUnitPolys_ID", "CSAMapUnitPolys_ID", "CSBMapUnitPolys_ID", "CSCMapUnitPolys_ID", "CSDMapUnitPolys_ID", "CSEMapUnitPolys_ID", "CSFMapUnitPolys_ID" };
            string mupIdField = "";
            int idFld = -1;
            for (int i = 0; i < mupIdFields.Length; i++)
                if (idFld == -1)
                {
                    mupIdField = mupIdFields[i];
                    idFld = m_MapUnitPolysFC.FindField(mupIdField);
                }
            int unitFld = m_MapUnitPolysFC.FindField("MapUnit");
            int idConfFld = m_MapUnitPolysFC.FindField("IdentityConfidence");
            int lblFld = m_MapUnitPolysFC.FindField("Label");
            int notesFld = m_MapUnitPolysFC.FindField("Notes");
            int dsFld = m_MapUnitPolysFC.FindField("DataSourceID");
            int symFld = m_MapUnitPolysFC.FindField("Symbol");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_MapUnitPolysFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                MapUnitPoly anMapUnitPoly = new MapUnitPoly();
                anMapUnitPoly.MapUnitPolys_ID = theFeature.get_Value(idFld).ToString();
                anMapUnitPoly.MapUnit = theFeature.get_Value(unitFld).ToString();
                anMapUnitPoly.IdentityConfidence = theFeature.get_Value(idConfFld).ToString();
                anMapUnitPoly.Label = theFeature.get_Value(lblFld).ToString();
                anMapUnitPoly.Notes = theFeature.get_Value(notesFld).ToString();
                anMapUnitPoly.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anMapUnitPoly.Symbol = theFeature.get_Value(symFld).ToString();
                anMapUnitPoly.Shape = (IPolygon)theFeature.Shape;
                anMapUnitPoly.RequiresUpdate = true;

                m_MapUnitPolysDictionary.Add(anMapUnitPoly.MapUnitPolys_ID, anMapUnitPoly);

                theFeature = theCursor.NextFeature();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(theCursor);
        }

        public string NewMapUnitPoly(string MapUnit, string IdentityConfidence,
            string Label, string Notes, string DataSourceID, string Symbol, IPolygon Shape)
        {
            MapUnitPoly newMapUnitPoly = new MapUnitPoly();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newMapUnitPoly.MapUnitPolys_ID = SysInfoTable.ProjAbbr + ".MapUnitPolys." + SysInfoTable.GetNextIdValue("MapUnitPolys");
            newMapUnitPoly.MapUnit = MapUnit;
            newMapUnitPoly.IdentityConfidence = IdentityConfidence;
            newMapUnitPoly.Label = Label;
            newMapUnitPoly.Notes = Notes;
            newMapUnitPoly.DataSourceID = DataSourceID;
            newMapUnitPoly.Symbol = Symbol;
            newMapUnitPoly.Shape = Shape;
            newMapUnitPoly.RequiresUpdate = false;

            m_MapUnitPolysDictionary.Add(newMapUnitPoly.MapUnitPolys_ID, newMapUnitPoly);
            return newMapUnitPoly.MapUnitPolys_ID;
        }

        public void UpdateMapUnitPoly(MapUnitPoly theMapUnitPoly)
        {
            try { m_MapUnitPolysDictionary.Remove(theMapUnitPoly.MapUnitPolys_ID); }
            catch { }

            theMapUnitPoly.RequiresUpdate = true;
            m_MapUnitPolysDictionary.Add(theMapUnitPoly.MapUnitPolys_ID, theMapUnitPoly);
        }

        public void SaveMapUnitPolys()
        {
            // Figure out the MapUnitPolys_ID field. It could be one of the following.
            string[] mupIdFields = { "MapUnitPolys_ID", "CMUMapUnitPolys_ID", "CSAMapUnitPolys_ID", "CSBMapUnitPolys_ID", "CSCMapUnitPolys_ID", "CSDMapUnitPolys_ID", "CSEMapUnitPolys_ID", "CSFMapUnitPolys_ID" };
            string mupIdField = "";
            int idFld = -1;
            for (int i = 0; i < mupIdFields.Length; i++)
                if (idFld == -1)
                {
                    mupIdField = mupIdFields[i];
                    idFld = m_MapUnitPolysFC.FindField(mupIdField);
                }
            int unitFld = m_MapUnitPolysFC.FindField("MapUnit");
            int idConfFld = m_MapUnitPolysFC.FindField("IdentityConfidence");
            int lblFld = m_MapUnitPolysFC.FindField("Label");
            int notesFld = m_MapUnitPolysFC.FindField("Notes");
            int dsFld = m_MapUnitPolysFC.FindField("DataSourceID");
            int symFld = m_MapUnitPolysFC.FindField("Symbol");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = mupIdField + " = '";
                IFeatureCursor insertCursor = m_MapUnitPolysFC.Insert(true);

                foreach (KeyValuePair<string, MapUnitPoly> aDictionaryEntry in m_MapUnitPolysDictionary)
                {
                    MapUnitPoly thisMapUnitPoly = (MapUnitPoly)aDictionaryEntry.Value;
                    switch (thisMapUnitPoly.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisMapUnitPoly.MapUnitPolys_ID + "' OR " + mupIdField + " = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_MapUnitPolysFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisMapUnitPoly.MapUnitPolys_ID);
                            theFeatureBuffer.set_Value(unitFld, thisMapUnitPoly.MapUnit);
                            theFeatureBuffer.set_Value(idConfFld, thisMapUnitPoly.IdentityConfidence);
                            theFeatureBuffer.set_Value(lblFld, thisMapUnitPoly.Label);
                            theFeatureBuffer.set_Value(notesFld, thisMapUnitPoly.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisMapUnitPoly.DataSourceID);
                            theFeatureBuffer.set_Value(symFld, thisMapUnitPoly.Symbol);
                            theFeatureBuffer.Shape = thisMapUnitPoly.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert MapUnitPolys");

                if (updateWhereClause == mupIdField + " = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - (" OR " + mupIdField + " = '").Length);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_MapUnitPolysFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    MapUnitPoly thisMapUnitPoly = m_MapUnitPolysDictionary[theID];
                    theFeature.set_Value(unitFld, thisMapUnitPoly.MapUnit);
                    theFeature.set_Value(idConfFld, thisMapUnitPoly.IdentityConfidence);
                    theFeature.set_Value(lblFld, thisMapUnitPoly.Label);
                    theFeature.set_Value(notesFld, thisMapUnitPoly.Notes);
                    theFeature.set_Value(dsFld, thisMapUnitPoly.DataSourceID);
                    theFeature.set_Value(symFld, thisMapUnitPoly.Symbol);
                    //theFeature.Shape = thisMapUnitPoly.Shape; Adjusting the shape of the feature triggers annotations to be re-placed. No me gusta.
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(updateCursor);
                theEditor.StopOperation("Update MapUnitPolys");
            }
            catch { theEditor.StopOperation("MapUnitPolys Management Failure"); }
        }

        public void DeleteMapUnitPolys(MapUnitPoly theMapUnitPoly)
        {
            try { m_MapUnitPolysDictionary.Remove(theMapUnitPoly.MapUnitPolys_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "MapUnitPolys_ID = '" + theMapUnitPoly.MapUnitPolys_ID + "'";

                ITable MapUnitPolysTable = m_MapUnitPolysFC as ITable;
                MapUnitPolysTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete MapUnitPolys");
            }
            catch (Exception e) { theEditor.StopOperation("MapUnitPolys Management Failure"); }
        }
    }
}
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
    class DataSourcePolysAccess
    {
        IFeatureClass m_DataSourcePolysFC;
        IWorkspace m_theWorkspace;

        public DataSourcePolysAccess(IWorkspace theWorkspace)
        {
            m_DataSourcePolysFC = commonFunctions.OpenFeatureClass(theWorkspace, "DataSourcePolys");
            m_theWorkspace = theWorkspace;
        }

        public struct DataSourcePoly
        {
            public string DataSourcePolys_ID;
            public string MapUnit;
            public string IdentityConfidence;
            public string Label;
            public string Notes;
            public string DataSourceID;
            public string Symbol;
            public IPolygon Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, DataSourcePoly> m_DataSourcePolysDictionary = new Dictionary<string, DataSourcePoly>();
        public Dictionary<string, DataSourcePoly> DataSourcePolysDictionary
        {
            get { return m_DataSourcePolysDictionary; }
        }

        public void ClearDataSourcePolys()
        {
            m_DataSourcePolysDictionary.Clear();
        }

        public void AddDataSourcePolys(string SqlWhereClause)
        {
            int idFld = m_DataSourcePolysFC.FindField("DataSourcePolys_ID");
            int notesFld = m_DataSourcePolysFC.FindField("Notes");
            int dsFld = m_DataSourcePolysFC.FindField("DataSourceID");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_DataSourcePolysFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                DataSourcePoly anDataSourcePoly = new DataSourcePoly();
                anDataSourcePoly.DataSourcePolys_ID = theFeature.get_Value(idFld).ToString();
                anDataSourcePoly.Notes = theFeature.get_Value(notesFld).ToString();
                anDataSourcePoly.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anDataSourcePoly.Shape = (IPolygon)theFeature.Shape;
                anDataSourcePoly.RequiresUpdate = true;

                m_DataSourcePolysDictionary.Add(anDataSourcePoly.DataSourcePolys_ID, anDataSourcePoly);

                theFeature = theCursor.NextFeature();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(theCursor);
        }

        public string NewDataSourcePoly(string MapUnit, string IdentityConfidence,
            string Label, string Notes, string DataSourceID, string Symbol, IPolygon Shape)
        {
            DataSourcePoly newDataSourcePoly = new DataSourcePoly();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newDataSourcePoly.DataSourcePolys_ID = SysInfoTable.ProjAbbr + ".DataSourcePolys." + SysInfoTable.GetNextIdValue("DataSourcePolys");
            newDataSourcePoly.Notes = Notes;
            newDataSourcePoly.DataSourceID = DataSourceID;
            newDataSourcePoly.Shape = Shape;
            newDataSourcePoly.RequiresUpdate = false;

            m_DataSourcePolysDictionary.Add(newDataSourcePoly.DataSourcePolys_ID, newDataSourcePoly);
            return newDataSourcePoly.DataSourcePolys_ID;
        }

        public void UpdateDataSourcePoly(DataSourcePoly theDataSourcePoly)
        {
            try { m_DataSourcePolysDictionary.Remove(theDataSourcePoly.DataSourcePolys_ID); }
            catch { }

            theDataSourcePoly.RequiresUpdate = true;
            m_DataSourcePolysDictionary.Add(theDataSourcePoly.DataSourcePolys_ID, theDataSourcePoly);
        }

        public void SaveDataSourcePolys()
        {
            int idFld = m_DataSourcePolysFC.FindField("DataSourcePolys_ID");
            int notesFld = m_DataSourcePolysFC.FindField("Notes");
            int dsFld = m_DataSourcePolysFC.FindField("DataSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "DataSourcePolys_ID = '";
                IFeatureCursor insertCursor = m_DataSourcePolysFC.Insert(true);

                foreach (KeyValuePair<string, DataSourcePoly> aDictionaryEntry in m_DataSourcePolysDictionary)
                {
                    DataSourcePoly thisDataSourcePoly = (DataSourcePoly)aDictionaryEntry.Value;
                    switch (thisDataSourcePoly.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisDataSourcePoly.DataSourcePolys_ID + "' OR DataSourcePolys_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_DataSourcePolysFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisDataSourcePoly.DataSourcePolys_ID);
                            theFeatureBuffer.set_Value(notesFld, thisDataSourcePoly.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisDataSourcePoly.DataSourceID);
                            theFeatureBuffer.Shape = thisDataSourcePoly.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert DataSourcePolys");

                if (updateWhereClause == "DataSourcePolys_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 25);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_DataSourcePolysFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    DataSourcePoly thisDataSourcePoly = m_DataSourcePolysDictionary[theID];
                    theFeature.set_Value(notesFld, thisDataSourcePoly.Notes);
                    theFeature.set_Value(dsFld, thisDataSourcePoly.DataSourceID);
                    //theFeature.Shape = thisDataSourcePoly.Shape; Adjusting the shape of the feature triggers annotations to be re-placed. No me gusta.
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(updateCursor);
                theEditor.StopOperation("Update DataSourcePolys");
            }
            catch { theEditor.StopOperation("DataSourcePolys Management Failure"); }
        }

        public void DeleteDataSourcePolys(DataSourcePoly theDataSourcePoly)
        {
            try { m_DataSourcePolysDictionary.Remove(theDataSourcePoly.DataSourcePolys_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "DataSourcePolys_ID = '" + theDataSourcePoly.DataSourcePolys_ID + "'";

                ITable DataSourcePolysTable = m_DataSourcePolysFC as ITable;
                DataSourcePolysTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete DataSourcePolys");
            }
            catch (Exception e) { theEditor.StopOperation("DataSourcePolys Management Failure"); }
        }
    }
}

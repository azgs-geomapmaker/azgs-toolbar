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
    class RelatedDocumentsAccess
    {
        ITable m_RelatedDocumentsTable;
        IWorkspace m_theWorkspace;

        public RelatedDocumentsAccess(IWorkspace theWorkspace)
        {
            m_RelatedDocumentsTable = commonFunctions.OpenTable(theWorkspace, "RelatedDocuments");
            m_theWorkspace = theWorkspace;
        }

        public struct RelatedDocument
        {
            public string RelatedDocuments_ID;
            public string OwnerID;
            public string Type;
            public string DocumentPath;
            public string DocumentName;
            public string Notes;
            public string DataSourceID;
            public bool RequiresUpdate;
        }

        private Dictionary<string, RelatedDocument> m_RelatedDocumentsDictionary = new Dictionary<string, RelatedDocument>();
        public Dictionary<string, RelatedDocument> RelatedDocumentsDictionary
        {
            get { return m_RelatedDocumentsDictionary; }
        }

        public void ClearRelatedDocuments()
        {
            m_RelatedDocumentsDictionary.Clear();
        }

        public void AddRelatedDocuments(string SqlWhereClause = null)
        {
            int idFld = m_RelatedDocumentsTable.FindField("RelatedDocuments_ID");
            int ownerFld = m_RelatedDocumentsTable.FindField("OwnerID");
            int typeFld = m_RelatedDocumentsTable.FindField("Type");
            int pathFld = m_RelatedDocumentsTable.FindField("DocumentPath");
            int nameFld = m_RelatedDocumentsTable.FindField("DocumentName");
            int noteFld = m_RelatedDocumentsTable.FindField("Notes");
            int dsFld = m_RelatedDocumentsTable.FindField("DataSourceID");

            ICursor theCursor;

            if (SqlWhereClause == null) { theCursor = m_RelatedDocumentsTable.Search(null, false); }
            else
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = SqlWhereClause;
                theCursor = m_RelatedDocumentsTable.Search(QF, false);
            }

            IRow theRow = theCursor.NextRow();

            while (theRow != null)
            {
                RelatedDocument anRelatedDocument = new RelatedDocument();
                anRelatedDocument.RelatedDocuments_ID = theRow.get_Value(idFld).ToString();
                anRelatedDocument.OwnerID = theRow.get_Value(ownerFld).ToString();
                anRelatedDocument.Notes = theRow.get_Value(noteFld).ToString();
                anRelatedDocument.Type = theRow.get_Value(typeFld).ToString();
                anRelatedDocument.DocumentPath = theRow.get_Value(pathFld).ToString();
                anRelatedDocument.DocumentName = theRow.get_Value(nameFld).ToString();
                anRelatedDocument.DataSourceID = theRow.get_Value(dsFld).ToString();
                anRelatedDocument.RequiresUpdate = true;

                m_RelatedDocumentsDictionary.Add(anRelatedDocument.RelatedDocuments_ID, anRelatedDocument);

                theRow = theCursor.NextRow();
            }
        }

        public string NewRelatedDocument(string OwnerID, string DocumentPath, string DocumentName, string Notes, string Type, string DataSourceID)
        {
            RelatedDocument newRelatedDocument = new RelatedDocument();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newRelatedDocument.RelatedDocuments_ID = SysInfoTable.ProjAbbr + ".RelatedDocuments." + SysInfoTable.GetNextIdValue("RelatedDocuments");
            newRelatedDocument.OwnerID = OwnerID;
            newRelatedDocument.DocumentName = DocumentName;
            newRelatedDocument.DocumentPath = DocumentPath;
            newRelatedDocument.Notes = Notes;
            newRelatedDocument.Type = Type;
            newRelatedDocument.DataSourceID = DataSourceID;
            newRelatedDocument.RequiresUpdate = false;

            m_RelatedDocumentsDictionary.Add(newRelatedDocument.RelatedDocuments_ID, newRelatedDocument);
            return newRelatedDocument.RelatedDocuments_ID;
        }

        public void UpdateRelatedDocument(RelatedDocument theRelatedDocument)
        {
            try { m_RelatedDocumentsDictionary.Remove(theRelatedDocument.RelatedDocuments_ID); }
            catch { }

            theRelatedDocument.RequiresUpdate = true;
            m_RelatedDocumentsDictionary.Add(theRelatedDocument.RelatedDocuments_ID, theRelatedDocument);
        }

        public void SaveRelatedDocuments()
        {
            int idFld = m_RelatedDocumentsTable.FindField("RelatedDocuments_ID");
            int ownerFld = m_RelatedDocumentsTable.FindField("OwnerID");
            int typeFld = m_RelatedDocumentsTable.FindField("Type");
            int pathFld = m_RelatedDocumentsTable.FindField("DocumentPath");
            int nameFld = m_RelatedDocumentsTable.FindField("DocumentName");
            int noteFld = m_RelatedDocumentsTable.FindField("Notes");
            int dsFld = m_RelatedDocumentsTable.FindField("DataSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "RelatedDocuments_ID = '";
                ICursor insertCursor = m_RelatedDocumentsTable.Insert(true);

                foreach (KeyValuePair<string, RelatedDocument> aDictionaryEntry in m_RelatedDocumentsDictionary)
                {
                    RelatedDocument thisRelatedDocument = (RelatedDocument)aDictionaryEntry.Value;
                    switch (thisRelatedDocument.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisRelatedDocument.RelatedDocuments_ID + "' OR RelatedDocuments_ID = '";
                            break;

                        case false:
                            IRowBuffer theRowBuffer = m_RelatedDocumentsTable.CreateRowBuffer();
                            theRowBuffer.set_Value(idFld, thisRelatedDocument.RelatedDocuments_ID);
                            theRowBuffer.set_Value(ownerFld, thisRelatedDocument.OwnerID);
                            theRowBuffer.set_Value(pathFld, thisRelatedDocument.DocumentPath);
                            theRowBuffer.set_Value(nameFld, thisRelatedDocument.DocumentName);
                            theRowBuffer.set_Value(noteFld, thisRelatedDocument.Notes);
                            theRowBuffer.set_Value(typeFld, thisRelatedDocument.Type);
                            theRowBuffer.set_Value(dsFld, thisRelatedDocument.DataSourceID);

                            insertCursor.InsertRow(theRowBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert RelatedDocuments");

                if (updateWhereClause == "RelatedDocuments_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                ICursor updateCursor = m_RelatedDocumentsTable.Update(QF, false);
                IRow theRow = updateCursor.NextRow();

                while (theRow != null)
                {
                    string theID = theRow.get_Value(idFld).ToString();

                    RelatedDocument thisRelatedDocument = m_RelatedDocumentsDictionary[theID];
                    theRow.set_Value(ownerFld, thisRelatedDocument.OwnerID);
                    theRow.set_Value(noteFld, thisRelatedDocument.Notes);
                    theRow.set_Value(pathFld, thisRelatedDocument.DocumentPath);
                    theRow.set_Value(nameFld, thisRelatedDocument.DocumentName);
                    theRow.set_Value(typeFld, thisRelatedDocument.Type);
                    theRow.set_Value(dsFld, thisRelatedDocument.DataSourceID);
                    updateCursor.UpdateRow(theRow);

                    theRow = updateCursor.NextRow();
                }

                theEditor.StopOperation("Update RelatedDocuments");
            }
            catch { theEditor.StopOperation("RelatedDocuments Management Failure"); }
        }

        public void DeleteRelatedDocuments(RelatedDocument theRelatedDocument)
        {
            try { m_RelatedDocumentsDictionary.Remove(theRelatedDocument.RelatedDocuments_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "RelatedDocuments_ID = '" + theRelatedDocument.RelatedDocuments_ID + "'";

                m_RelatedDocumentsTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete RelatedDocuments");
            }
            catch (Exception e) { theEditor.StopOperation("RelatedDocuments Management Failure"); }
        }
    }
}

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
    class NotesAccess
    {
        ITable m_NotesTable;
        IWorkspace m_theWorkspace;

        public NotesAccess(IWorkspace theWorkspace)
        {
            m_NotesTable = commonFunctions.OpenTable(theWorkspace, "Notes");
            m_theWorkspace = theWorkspace;
        }

        public struct Note
        {
            public string Notes_ID;
            public string OwnerID;
            public string Type;
            public string Notes;
            public string DataSourceID;
            public bool RequiresUpdate;
        }

        private Dictionary<string, Note> m_NotesDictionary = new Dictionary<string, Note>();
        public Dictionary<string, Note> NotesDictionary
        {
            get { return m_NotesDictionary; }
        }

        public void ClearNotes()
        {
            m_NotesDictionary.Clear();
        }

        public void AddNotes(string SqlWhereClause = null)
        {
            int idFld = m_NotesTable.FindField("Notes_ID");
            int ownerFld = m_NotesTable.FindField("OwnerID");
            int typeFld = m_NotesTable.FindField("Type");
            int notesFld = m_NotesTable.FindField("Notes");
            int dsFld = m_NotesTable.FindField("DataSourceID");

            ICursor theCursor;

            if (SqlWhereClause == null) { theCursor = m_NotesTable.Search(null, false); }
            else
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = SqlWhereClause;
                theCursor = m_NotesTable.Search(QF, false);
            }

            IRow theRow = theCursor.NextRow();

            while (theRow != null)
            {
                Note anNote = new Note();
                anNote.Notes_ID = theRow.get_Value(idFld).ToString();
                anNote.OwnerID = theRow.get_Value(ownerFld).ToString();
                anNote.Notes = theRow.get_Value(notesFld).ToString();
                anNote.Type = theRow.get_Value(typeFld).ToString();
                anNote.DataSourceID = theRow.get_Value(dsFld).ToString();
                anNote.RequiresUpdate = true;

                m_NotesDictionary.Add(anNote.Notes_ID, anNote);

                theRow = theCursor.NextRow();
            }
        }

        public string NewNote(string OwnerID, string Notes, string Type, string DataSourceID)
        {
            Note newNote = new Note();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newNote.Notes_ID = SysInfoTable.ProjAbbr + ".Notes." + SysInfoTable.GetNextIdValue("Notes");
            newNote.OwnerID = OwnerID;
            newNote.Notes = Notes;
            newNote.Type = Type;
            newNote.DataSourceID = DataSourceID;
            newNote.RequiresUpdate = false;

            m_NotesDictionary.Add(newNote.Notes_ID, newNote);
            return newNote.Notes_ID;
        }

        public void UpdateNote(Note theNote)
        {
            try { m_NotesDictionary.Remove(theNote.Notes_ID); }
            catch { }

            theNote.RequiresUpdate = true;
            m_NotesDictionary.Add(theNote.Notes_ID, theNote);
        }

        public void SaveNotes()
        {
            int idFld = m_NotesTable.FindField("Notes_ID");
            int ownerFld = m_NotesTable.FindField("OwnerID");
            int typeFld = m_NotesTable.FindField("Type");
            int notesFld = m_NotesTable.FindField("Notes");
            int dsFld = m_NotesTable.FindField("DataSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "Notes_ID = '";
                ICursor insertCursor = m_NotesTable.Insert(true);

                foreach (KeyValuePair<string, Note> aDictionaryEntry in m_NotesDictionary)
                {
                    Note thisNote = (Note)aDictionaryEntry.Value;
                    switch (thisNote.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisNote.Notes_ID + "' OR Notes_ID = '";
                            break;

                        case false:
                            IRowBuffer theRowBuffer = m_NotesTable.CreateRowBuffer();
                            theRowBuffer.set_Value(idFld, thisNote.Notes_ID);
                            theRowBuffer.set_Value(ownerFld, thisNote.OwnerID);
                            theRowBuffer.set_Value(notesFld, thisNote.Notes);
                            theRowBuffer.set_Value(typeFld, thisNote.Type);
                            theRowBuffer.set_Value(dsFld, thisNote.DataSourceID);

                            insertCursor.InsertRow(theRowBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert Notes");

                if (updateWhereClause == "Notes_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                ICursor updateCursor = m_NotesTable.Update(QF, false);
                IRow theRow = updateCursor.NextRow();

                while (theRow != null)
                {
                    string theID = theRow.get_Value(idFld).ToString();

                    Note thisNote = m_NotesDictionary[theID];
                    theRow.set_Value(ownerFld, thisNote.OwnerID);
                    theRow.set_Value(notesFld, thisNote.Notes);
                    theRow.set_Value(typeFld, thisNote.Type);
                    theRow.set_Value(dsFld, thisNote.DataSourceID);
                    updateCursor.UpdateRow(theRow);

                    theRow = updateCursor.NextRow();
                }

                theEditor.StopOperation("Update Notes");
            }
            catch { theEditor.StopOperation("Notes Management Failure"); }
        }

        public void DeleteNotes(Note theNote)
        {
            try { m_NotesDictionary.Remove(theNote.Notes_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "Notes_ID = '" + theNote.Notes_ID + "'";

                m_NotesTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete Notes");
            }
            catch (Exception e) { theEditor.StopOperation("Notes Management Failure"); }
        }
    }
}

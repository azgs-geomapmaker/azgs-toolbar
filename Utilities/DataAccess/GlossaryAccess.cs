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
    class GlossaryAccess
    {
        ITable m_GlossaryTable;
        IWorkspace m_theWorkspace;

        public GlossaryAccess(IWorkspace theWorkspace)
        {
            m_GlossaryTable = commonFunctions.OpenTable(theWorkspace, "Glossary");
            m_theWorkspace = theWorkspace;
        }

        public struct Glossary
        {
            public string Glossary_ID;
            public string Term;
            public string Definition;
            public string DefinitionSourceID;
            public bool RequiresUpdate;
        }

        private Dictionary<string, Glossary> m_GlossaryDictionary = new Dictionary<string, Glossary>();
        public Dictionary<string, Glossary> GlossaryDictionary
        {
            get { return m_GlossaryDictionary; }
        }

        public void ClearGlossary()
        {
            m_GlossaryDictionary.Clear();
        }

        public void AddGlossary(string SqlWhereClause)
        {
            int idFld = m_GlossaryTable.FindField("Glossary_ID");
            int trmFld = m_GlossaryTable.FindField("Term");
            int defFld = m_GlossaryTable.FindField("Definition");
            int dsFld = m_GlossaryTable.FindField("DefinitionSourceID");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            ICursor theCursor = m_GlossaryTable.Search(QF, false);
            IRow theRow = theCursor.NextRow();

            while (theRow != null)
            {
                Glossary anGlossary = new Glossary();
                anGlossary.Glossary_ID = theRow.get_Value(idFld).ToString();
                anGlossary.Term = theRow.get_Value(trmFld).ToString();
                anGlossary.Definition = theRow.get_Value(trmFld).ToString();
                anGlossary.DefinitionSourceID = theRow.get_Value(dsFld).ToString();
                anGlossary.RequiresUpdate = true;

                m_GlossaryDictionary.Add(anGlossary.Glossary_ID, anGlossary);

                theRow = theCursor.NextRow();
            }
        }

        public string NewGlossary(string Term, string Definition, string DefinitionSourceID)
        {
            Glossary newGlossary = new Glossary();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newGlossary.Glossary_ID = SysInfoTable.ProjAbbr + ".Glossary." + SysInfoTable.GetNextIdValue("Glossary");
            newGlossary.Term = Term;
            newGlossary.Definition = Definition;
            newGlossary.DefinitionSourceID = DefinitionSourceID;
            newGlossary.RequiresUpdate = false;

            m_GlossaryDictionary.Add(newGlossary.Glossary_ID, newGlossary);
            return newGlossary.Glossary_ID;
        }

        public void UpdateGlossary(Glossary theGlossary)
        {
            try { m_GlossaryDictionary.Remove(theGlossary.Glossary_ID); }
            catch { }

            theGlossary.RequiresUpdate = true;
            m_GlossaryDictionary.Add(theGlossary.Glossary_ID, theGlossary);
        }

        public void SaveGlossary()
        {
            int idFld = m_GlossaryTable.FindField("Glossary_ID");
            int trmFld = m_GlossaryTable.FindField("Term");
            int defFld = m_GlossaryTable.FindField("Definition");
            int dsFld = m_GlossaryTable.FindField("DefinitionSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "Glossary_ID = '";
                ICursor insertCursor = m_GlossaryTable.Insert(true);

                foreach (KeyValuePair<string, Glossary> aDictionaryEntry in m_GlossaryDictionary)
                {
                    Glossary thisGlossary = (Glossary)aDictionaryEntry.Value;
                    switch (thisGlossary.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisGlossary.Glossary_ID + "' OR Glossary_ID = '";
                            break;

                        case false:
                            IRowBuffer theRowBuffer = m_GlossaryTable.CreateRowBuffer();
                            theRowBuffer.set_Value(idFld, thisGlossary.Glossary_ID);
                            theRowBuffer.set_Value(trmFld, thisGlossary.Term);
                            theRowBuffer.set_Value(defFld, thisGlossary.Definition);
                            theRowBuffer.set_Value(dsFld, thisGlossary.DefinitionSourceID);

                            insertCursor.InsertRow(theRowBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert Glossary");
                theEditor.StartOperation();

                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                ICursor updateCursor = m_GlossaryTable.Update(QF, false);
                IRow theRow = updateCursor.NextRow();

                while (theRow != null)
                {
                    string theID = theRow.get_Value(idFld).ToString();

                    Glossary thisGlossary = m_GlossaryDictionary[theID];
                    theRow.set_Value(trmFld, thisGlossary.Term);
                    theRow.set_Value(defFld, thisGlossary.Definition);
                    theRow.set_Value(dsFld, thisGlossary.DefinitionSourceID);
                    updateCursor.UpdateRow(theRow);

                    theRow = updateCursor.NextRow();
                }

                theEditor.StopOperation("Update Glossary");
            }
            catch { theEditor.StopOperation("Glossary Management Failure"); }
        }
    }
}

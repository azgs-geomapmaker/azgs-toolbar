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
    class StandardLithologyAccess
    {
        ITable m_StandardLithologyTable;
        IWorkspace m_theWorkspace;

        public StandardLithologyAccess(IWorkspace theWorkspace)
        {
            m_StandardLithologyTable = commonFunctions.OpenTable(theWorkspace, "StandardLithology");
            m_theWorkspace = theWorkspace;
        }

        public struct StandardLithology
        {
            public string StandardLithology_ID;
            public string MapUnit;
            public string PartType;
            public string Lithology;
            public string ProportionTerm;
            public double ProportionValue;
            public string ScientificConfidence;
            public string DataSourceID;
            public bool RequiresUpdate;
        }

        private Dictionary<string, StandardLithology> m_StandardLithologyDictionary = new Dictionary<string, StandardLithology>();
        public Dictionary<string, StandardLithology> StandardLithologyDictionary
        {
            get { return m_StandardLithologyDictionary; }
            set { m_StandardLithologyDictionary = value; }
        }

        public void ClearDescriptionOfMapUnits()
        {
            m_StandardLithologyDictionary.Clear();
        }

        public void AddStandardLithology(string SqlWhereClause = null)
        {
            int idFld = m_StandardLithologyTable.FindField("StandardLithology_ID");
            int unitFld = m_StandardLithologyTable.FindField("MapUnit");
            int pTypeFld = m_StandardLithologyTable.FindField("PartType");
            int lithFld = m_StandardLithologyTable.FindField("Lithology");
            int propTermFld = m_StandardLithologyTable.FindField("ProportionTerm");
            int propValueFld = m_StandardLithologyTable.FindField("ProportionValue");
            int sciConfidenceFld = m_StandardLithologyTable.FindField("ScientificConfidence");
            int dataSrcFld = m_StandardLithologyTable.FindField("DataSourceID");

            ICursor theCursor;

            if (SqlWhereClause == null) { theCursor = m_StandardLithologyTable.Search(null, false); }
            else
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = SqlWhereClause;
                theCursor = m_StandardLithologyTable.Search(QF, false);
            }

            IRow theRow = theCursor.NextRow();

            while (theRow != null)
            {
                StandardLithology anStandardLithology = new StandardLithology();
                anStandardLithology.StandardLithology_ID = theRow.get_Value(idFld).ToString();
                anStandardLithology.MapUnit = theRow.get_Value(unitFld).ToString();
                anStandardLithology.PartType = theRow.get_Value(pTypeFld).ToString();
                anStandardLithology.Lithology = theRow.get_Value(lithFld).ToString();
                anStandardLithology.ProportionTerm = theRow.get_Value(propTermFld).ToString();
                anStandardLithology.ProportionValue = double.Parse(theRow.get_Value(propValueFld).ToString());
                anStandardLithology.ScientificConfidence = theRow.get_Value(sciConfidenceFld).ToString();
                anStandardLithology.DataSourceID = theRow.get_Value(dataSrcFld).ToString();
                anStandardLithology.RequiresUpdate = true;

                m_StandardLithologyDictionary.Add(anStandardLithology.StandardLithology_ID, anStandardLithology);

                theRow = theCursor.NextRow();
            }
        }

        public string NewStandardLithology(string MapUnit, string PartType, string Lithology, string ProportionTerm,
            double ProportionValue, string ScientificConfidence, string DataSourceID)
        {
            StandardLithology newStandardLithology = new StandardLithology();
            
            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newStandardLithology.StandardLithology_ID = SysInfoTable.ProjAbbr + ".StandardLithology." + SysInfoTable.GetNextIdValue("StandardLithology");

            newStandardLithology.MapUnit = MapUnit;
            newStandardLithology.PartType = PartType;
            newStandardLithology.Lithology = Lithology;
            newStandardLithology.ProportionTerm = ProportionTerm;
            newStandardLithology.ProportionValue = ProportionValue;
            newStandardLithology.ScientificConfidence = ScientificConfidence;
            newStandardLithology.DataSourceID = DataSourceID;
            newStandardLithology.RequiresUpdate = false;

            m_StandardLithologyDictionary.Add(newStandardLithology.StandardLithology_ID, newStandardLithology);
            return newStandardLithology.StandardLithology_ID;
        }

        public void UpdateStandardLithology(StandardLithology theStandardLithology)
        {
            try { m_StandardLithologyDictionary.Remove(theStandardLithology.StandardLithology_ID); }
            catch { }

            theStandardLithology.RequiresUpdate = true;
            m_StandardLithologyDictionary.Add(theStandardLithology.StandardLithology_ID, theStandardLithology);
        }

        public void SaveStandardLithology()
        {
            int idFld = m_StandardLithologyTable.FindField("StandardLithology_ID");
            int unitFld = m_StandardLithologyTable.FindField("MapUnit");
            int pTypeFld = m_StandardLithologyTable.FindField("PartType");
            int lithFld = m_StandardLithologyTable.FindField("Lithology");
            int propTermFld = m_StandardLithologyTable.FindField("ProportionTerm");
            int propValueFld = m_StandardLithologyTable.FindField("ProportionValue");
            int sciConfidenceFld = m_StandardLithologyTable.FindField("ScientificConfidence");
            int dataSrcFld = m_StandardLithologyTable.FindField("DataSourceID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "StandardLithology_ID = '";
                ICursor insertCursor = m_StandardLithologyTable.Insert(true);

                foreach(KeyValuePair<string, StandardLithology> aDictionaryEntry in m_StandardLithologyDictionary)
                {
                    StandardLithology thisStandardLithology = (StandardLithology)aDictionaryEntry.Value;
                    switch (thisStandardLithology.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisStandardLithology.StandardLithology_ID + "' OR StandardLithology_ID = '";
                            break;

                        case false:
                            IRowBuffer theRowBuffer = m_StandardLithologyTable.CreateRowBuffer();
                            theRowBuffer.set_Value(idFld, thisStandardLithology.StandardLithology_ID);
                            theRowBuffer.set_Value(unitFld, thisStandardLithology.MapUnit);
                            theRowBuffer.set_Value(pTypeFld, thisStandardLithology.PartType);
                            theRowBuffer.set_Value(lithFld, thisStandardLithology.Lithology);
                            theRowBuffer.set_Value(propTermFld, thisStandardLithology.ProportionTerm);
                            theRowBuffer.set_Value(propValueFld, thisStandardLithology.ProportionValue);
                            theRowBuffer.set_Value(sciConfidenceFld, thisStandardLithology.ScientificConfidence);
                            theRowBuffer.set_Value(dataSrcFld, thisStandardLithology.DataSourceID);

                            insertCursor.InsertRow(theRowBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert StandardLithology");

                if (updateWhereClause == "StandardLithology_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 28);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                ICursor updateCursor = m_StandardLithologyTable.Update(QF, false);
                IRow theRow = updateCursor.NextRow();

                while (theRow != null)
                {
                    string theID = theRow.get_Value(idFld).ToString();

                    StandardLithology thisStandardLithology = m_StandardLithologyDictionary[theID];
                    theRow.set_Value(unitFld, thisStandardLithology.MapUnit);
                    theRow.set_Value(pTypeFld, thisStandardLithology.PartType);
                    theRow.set_Value(lithFld, thisStandardLithology.Lithology);
                    theRow.set_Value(propTermFld, thisStandardLithology.ProportionTerm);
                    theRow.set_Value(propValueFld, thisStandardLithology.ProportionValue);
                    theRow.set_Value(sciConfidenceFld, thisStandardLithology.ScientificConfidence);
                    theRow.set_Value(dataSrcFld, thisStandardLithology.DataSourceID);

                    updateCursor.UpdateRow(theRow);

                    theRow = updateCursor.NextRow();
                }

                theEditor.StopOperation("Update StandardLithology");
            }
            catch { theEditor.StopOperation("StandardLithology Management Failure");  }
        }

        public void DeleteStandardLithology(StandardLithology theStandardLithology)
        {
            try { m_StandardLithologyDictionary.Remove(theStandardLithology.StandardLithology_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace);  }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "StandardLithology_ID = '" + theStandardLithology.StandardLithology_ID + "'";

                m_StandardLithologyTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete StandardLithology");
            }
            catch (Exception e) { theEditor.StopOperation("StandardLithology Management Failure");  }
        }
    }
}

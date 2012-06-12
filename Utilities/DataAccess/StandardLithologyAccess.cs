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
            public string ProportionValue;
            public string ScientificConfidence;
            public string DataSourceID;
        }

        private Dictionary<string, StandardLithology> m_StandardLithologyDictionary = new Dictionary<string, StandardLithology>();
        public Dictionary<string, StandardLithology> StandardLithologyDictionary
        {
            get { return m_StandardLithologyDictionary; }
        }

        public void ClearDescriptionOfMapUnits()
        {
            m_StandardLithologyDictionary.Clear();
        }

        public void AddStandardLithologyAccess(string SqlWhereClause = null)
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
                anStandardLithology.ProportionValue = theRow.get_Value(propValueFld).ToString();
                anStandardLithology.ScientificConfidence = theRow.get_Value(sciConfidenceFld).ToString();
                anStandardLithology.DataSourceID = theRow.get_Value(dataSrcFld).ToString();

                m_StandardLithologyDictionary.Add(anStandardLithology.StandardLithology_ID, anStandardLithology);

                theRow = theCursor.NextRow();
            }
        }

        public string NewStandardLithology(string MapUnit, string PartType, string Lithology, string ProportionTerm,
            string ProportionValue, string ScientificConfidence, string DataSourceID)
        {
            StandardLithology newStandardLithology = new StandardLithology();


            return newStandardLithology.StandardLithology_ID;
        }
    }
}

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
    class DescriptionOfMapUnitsAccess
    {
        ITable m_DescriptionOfMapUnitsTable;
        IWorkspace m_theWorkspace;

        public DescriptionOfMapUnitsAccess(IWorkspace theWorkspace)
        {
            m_DescriptionOfMapUnitsTable = commonFunctions.OpenTable(theWorkspace, "DescriptionOfMapUnits");
            m_theWorkspace = theWorkspace;
        }

        public struct DescriptionOfMapUnit
        {
            public string DescriptionOfMapUnits_ID;
            public string MapUnit;
            public string Label;
            public string Name;
            public string FullName;
            public string Age;
            public string Description;
            public string HierarchyKey;
            public string ParagraphStyle;
            public string AreaFillRGB;
            public string DescriptionSourceID;
            public string AreaFillPatternDescription;
            public string GeneralLithology;
            public string GeneralLithologyConfidence;
            public bool RequiresUpdate;
        }

        private Dictionary<string, DescriptionOfMapUnit> m_DescriptionOfMapUnitsDictionary = new Dictionary<string, DescriptionOfMapUnit>();
        public Dictionary<string, DescriptionOfMapUnit> DescriptionOfMapUnitsDictionary
        {
            get { return m_DescriptionOfMapUnitsDictionary; }
        }

        public void ClearDescriptionOfMapUnits()
        {
            m_DescriptionOfMapUnitsDictionary.Clear();
        }

        public void AddDescriptionOfMapUnits(string SqlWhereClause = null)
        {
            int idFld = m_DescriptionOfMapUnitsTable.FindField("DescriptionOfMapUnits_ID");
            int unitFld = m_DescriptionOfMapUnitsTable.FindField("MapUnit");
            int nameFld = m_DescriptionOfMapUnitsTable.FindField("Name");
            int flNameFld = m_DescriptionOfMapUnitsTable.FindField("FullName");
            int lblFld = m_DescriptionOfMapUnitsTable.FindField("Label");
            int ageFld = m_DescriptionOfMapUnitsTable.FindField("Age");
            int descFld = m_DescriptionOfMapUnitsTable.FindField("Description");
            int hierFld = m_DescriptionOfMapUnitsTable.FindField("HierarchyKey");
            int styleFld = m_DescriptionOfMapUnitsTable.FindField("ParagraphStyle");
            int rgbFld = m_DescriptionOfMapUnitsTable.FindField("AreaFillRGB");
            int patFld = m_DescriptionOfMapUnitsTable.FindField("AreaFillPatternDescription");
            int dsFld = m_DescriptionOfMapUnitsTable.FindField("DescriptionSourceID");
            int glFld = m_DescriptionOfMapUnitsTable.FindField("GeneralLithology");
            int glConfFld = m_DescriptionOfMapUnitsTable.FindField("GeneralLithologyConfidence");

            ICursor theCursor;
            
            if (SqlWhereClause == null) { theCursor = m_DescriptionOfMapUnitsTable.Search(null, false); }
            else
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = SqlWhereClause;
                theCursor = m_DescriptionOfMapUnitsTable.Search(QF, false);
            }
            
            IRow theRow = theCursor.NextRow();

            while (theRow != null)
            {
                DescriptionOfMapUnit anDescriptionOfMapUnit = new DescriptionOfMapUnit();
                anDescriptionOfMapUnit.DescriptionOfMapUnits_ID = theRow.get_Value(idFld).ToString();
                anDescriptionOfMapUnit.MapUnit = theRow.get_Value(unitFld).ToString();
                anDescriptionOfMapUnit.Name = theRow.get_Value(nameFld).ToString();
                anDescriptionOfMapUnit.FullName = theRow.get_Value(flNameFld).ToString();
                anDescriptionOfMapUnit.Label = theRow.get_Value(lblFld).ToString();
                anDescriptionOfMapUnit.Age = theRow.get_Value(ageFld).ToString();
                anDescriptionOfMapUnit.Description = theRow.get_Value(descFld).ToString();
                anDescriptionOfMapUnit.HierarchyKey = theRow.get_Value(hierFld).ToString();
                anDescriptionOfMapUnit.ParagraphStyle = theRow.get_Value(styleFld).ToString();
                anDescriptionOfMapUnit.AreaFillRGB = theRow.get_Value(rgbFld).ToString();
                anDescriptionOfMapUnit.AreaFillPatternDescription = theRow.get_Value(patFld).ToString();
                anDescriptionOfMapUnit.DescriptionSourceID = theRow.get_Value(dsFld).ToString();
                anDescriptionOfMapUnit.GeneralLithology = theRow.get_Value(glFld).ToString();
                anDescriptionOfMapUnit.GeneralLithologyConfidence = theRow.get_Value(glConfFld).ToString();
                anDescriptionOfMapUnit.RequiresUpdate = true;

                m_DescriptionOfMapUnitsDictionary.Add(anDescriptionOfMapUnit.DescriptionOfMapUnits_ID, anDescriptionOfMapUnit);

                theRow = theCursor.NextRow();
            }            
        }

        public string NewDescriptionOfMapUnit(string MapUnit, string Name, string FullName,
            string Label, string Age, string Description, string HierarchyKey, string ParagraphStyle,
            string AreaFillRGB, string AreaFillPatternDescription, string DescriptionSourceID,
            string GeneralLithology, string GeneralLithologyConfidence)
        {
            DescriptionOfMapUnit newDescriptionOfMapUnit = new DescriptionOfMapUnit();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newDescriptionOfMapUnit.DescriptionOfMapUnits_ID = SysInfoTable.ProjAbbr + ".DescriptionOfMapUnits." + SysInfoTable.GetNextIdValue("DescriptionOfMapUnits");
            newDescriptionOfMapUnit.MapUnit = MapUnit;
            newDescriptionOfMapUnit.Name = Name;
            newDescriptionOfMapUnit.FullName = FullName;
            newDescriptionOfMapUnit.Label = Label;
            newDescriptionOfMapUnit.Age = Age;
            newDescriptionOfMapUnit.Description = Description;
            newDescriptionOfMapUnit.HierarchyKey = HierarchyKey;
            newDescriptionOfMapUnit.ParagraphStyle = ParagraphStyle;
            newDescriptionOfMapUnit.AreaFillRGB = AreaFillRGB;
            newDescriptionOfMapUnit.AreaFillPatternDescription = AreaFillPatternDescription;
            newDescriptionOfMapUnit.DescriptionSourceID = DescriptionSourceID;
            newDescriptionOfMapUnit.GeneralLithology = GeneralLithology;
            newDescriptionOfMapUnit.GeneralLithologyConfidence = GeneralLithologyConfidence;
            newDescriptionOfMapUnit.RequiresUpdate = false;

            m_DescriptionOfMapUnitsDictionary.Add(newDescriptionOfMapUnit.DescriptionOfMapUnits_ID, newDescriptionOfMapUnit);
            return newDescriptionOfMapUnit.DescriptionOfMapUnits_ID;
        }

        public void UpdateDescriptionOfMapUnit(DescriptionOfMapUnit theDescriptionOfMapUnit)
        {
            try { m_DescriptionOfMapUnitsDictionary.Remove(theDescriptionOfMapUnit.DescriptionOfMapUnits_ID); }
            catch { }

            theDescriptionOfMapUnit.RequiresUpdate = true;
            m_DescriptionOfMapUnitsDictionary.Add(theDescriptionOfMapUnit.DescriptionOfMapUnits_ID, theDescriptionOfMapUnit);
        }

        public void SaveDescriptionOfMapUnits()
        {
            int idFld = m_DescriptionOfMapUnitsTable.FindField("DescriptionOfMapUnits_ID");
            int unitFld = m_DescriptionOfMapUnitsTable.FindField("MapUnit");
            int nameFld = m_DescriptionOfMapUnitsTable.FindField("Name");
            int flNameFld = m_DescriptionOfMapUnitsTable.FindField("FullName");
            int lblFld = m_DescriptionOfMapUnitsTable.FindField("Label");
            int ageFld = m_DescriptionOfMapUnitsTable.FindField("Age");
            int descFld = m_DescriptionOfMapUnitsTable.FindField("Description");
            int hierFld = m_DescriptionOfMapUnitsTable.FindField("HierarchyKey");
            int styleFld = m_DescriptionOfMapUnitsTable.FindField("ParagraphStyle");
            int rgbFld = m_DescriptionOfMapUnitsTable.FindField("AreaFillRGB");
            int patFld = m_DescriptionOfMapUnitsTable.FindField("AreaFillPatternDescription");
            int dsFld = m_DescriptionOfMapUnitsTable.FindField("DescriptionSourceID");
            int glFld = m_DescriptionOfMapUnitsTable.FindField("GeneralLithology");
            int glConfFld = m_DescriptionOfMapUnitsTable.FindField("GeneralLithologyConfidence");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "DescriptionOfMapUnits_ID = '";
                ICursor insertCursor = m_DescriptionOfMapUnitsTable.Insert(true);

                foreach (KeyValuePair<string, DescriptionOfMapUnit> aDictionaryEntry in m_DescriptionOfMapUnitsDictionary)
                {
                    DescriptionOfMapUnit thisDescriptionOfMapUnit = (DescriptionOfMapUnit)aDictionaryEntry.Value;
                    switch (thisDescriptionOfMapUnit.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisDescriptionOfMapUnit.DescriptionOfMapUnits_ID + "' OR DescriptionOfMapUnits_ID = '";
                            break;

                        case false:
                            IRowBuffer theRowBuffer = m_DescriptionOfMapUnitsTable.CreateRowBuffer();
                            theRowBuffer.set_Value(idFld, thisDescriptionOfMapUnit.DescriptionOfMapUnits_ID);
                            theRowBuffer.set_Value(unitFld, thisDescriptionOfMapUnit.MapUnit);
                            theRowBuffer.set_Value(nameFld, thisDescriptionOfMapUnit.Name);
                            theRowBuffer.set_Value(flNameFld, thisDescriptionOfMapUnit.FullName);
                            theRowBuffer.set_Value(lblFld, thisDescriptionOfMapUnit.Label);
                            theRowBuffer.set_Value(ageFld, thisDescriptionOfMapUnit.Age);
                            theRowBuffer.set_Value(descFld, thisDescriptionOfMapUnit.Description);
                            theRowBuffer.set_Value(hierFld, thisDescriptionOfMapUnit.HierarchyKey);
                            theRowBuffer.set_Value(styleFld, thisDescriptionOfMapUnit.ParagraphStyle);
                            theRowBuffer.set_Value(rgbFld, thisDescriptionOfMapUnit.AreaFillRGB);
                            theRowBuffer.set_Value(patFld, thisDescriptionOfMapUnit.AreaFillPatternDescription);
                            theRowBuffer.set_Value(dsFld, thisDescriptionOfMapUnit.DescriptionSourceID);
                            theRowBuffer.set_Value(glFld, thisDescriptionOfMapUnit.GeneralLithology);
                            theRowBuffer.set_Value(glConfFld, thisDescriptionOfMapUnit.GeneralLithologyConfidence);

                            insertCursor.InsertRow(theRowBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert DescriptionOfMapUnits");

                if (updateWhereClause == "DescriptionOfMapUnits_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 32);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                ICursor updateCursor = m_DescriptionOfMapUnitsTable.Update(QF, false);
                IRow theRow = updateCursor.NextRow();

                while (theRow != null)
                {
                    string theID = theRow.get_Value(idFld).ToString();

                    DescriptionOfMapUnit thisDescriptionOfMapUnit = m_DescriptionOfMapUnitsDictionary[theID];
                    theRow.set_Value(unitFld, thisDescriptionOfMapUnit.MapUnit);
                    theRow.set_Value(nameFld, thisDescriptionOfMapUnit.Name);
                    theRow.set_Value(flNameFld, thisDescriptionOfMapUnit.FullName);
                    theRow.set_Value(lblFld, thisDescriptionOfMapUnit.Label);
                    theRow.set_Value(ageFld, thisDescriptionOfMapUnit.Age);
                    theRow.set_Value(descFld, thisDescriptionOfMapUnit.Description);
                    theRow.set_Value(hierFld, thisDescriptionOfMapUnit.HierarchyKey);
                    theRow.set_Value(styleFld, thisDescriptionOfMapUnit.ParagraphStyle);
                    theRow.set_Value(rgbFld, thisDescriptionOfMapUnit.AreaFillRGB);
                    theRow.set_Value(patFld, thisDescriptionOfMapUnit.AreaFillPatternDescription);
                    theRow.set_Value(dsFld, thisDescriptionOfMapUnit.DescriptionSourceID);
                    theRow.set_Value(glFld, thisDescriptionOfMapUnit.GeneralLithology);
                    theRow.set_Value(glConfFld, thisDescriptionOfMapUnit.GeneralLithologyConfidence);

                    updateCursor.UpdateRow(theRow);

                    theRow = updateCursor.NextRow();
                }

                theEditor.StopOperation("Update DescriptionOfMapUnits");
            }
            catch { theEditor.StopOperation("DescriptionOfMapUnits Management Failure"); }
        }

        public void DeleteDescriptionOfMapUnits(DescriptionOfMapUnit theDescriptionOfMapUnit)
        {
            try { m_DescriptionOfMapUnitsDictionary.Remove(theDescriptionOfMapUnit.DescriptionOfMapUnits_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "DescriptionOfMapUnits_ID = '" + theDescriptionOfMapUnit.DescriptionOfMapUnits_ID + "'";

                m_DescriptionOfMapUnitsTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete DescriptionOfMapUnits");
            }
            catch (Exception e) { theEditor.StopOperation("DescriptionOfMapUnits Management Failure"); }
        }
    }
}

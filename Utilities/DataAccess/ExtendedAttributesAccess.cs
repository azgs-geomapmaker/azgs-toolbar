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
    class ExtendedAttributesAccess
    {
        ITable m_ExtendedAttributesTable;
        IWorkspace m_theWorkspace;

        public ExtendedAttributesAccess(IWorkspace theWorkspace)
        {
            m_ExtendedAttributesTable = commonFunctions.OpenTable(theWorkspace, "ExtendedAttributes");
            m_theWorkspace = theWorkspace;
        }

        public struct ExtendedAttributes
        {
            public string ExtendedAttributes_ID;
            public string OwnerTable;
            public string OwnerID;
            public string Property;
            public string PropertyValue;
            public string ValueLinkID;
            public string Qualifier;
            public string DataSourceID;
            public string Notes;
            public bool RequiresUpdate;
        }

        private Dictionary<string, ExtendedAttributes> m_ExtendedAttributesDictionary = new Dictionary<string, ExtendedAttributes>();
        public Dictionary<string, ExtendedAttributes> ExtendedAttributesDictionary
        {
            get { return m_ExtendedAttributesDictionary; }
            set { m_ExtendedAttributesDictionary = value; }
        }

        public void ClearExtendedAttributes()
        {
            m_ExtendedAttributesDictionary.Clear();
        }

        public void AddExtendedAttributes(string SqlWhereClause = null)
        {
            int idFld = m_ExtendedAttributesTable.FindField("ExtendedAttributes_ID");
            int ownerTableFld = m_ExtendedAttributesTable.FindField("OwnerTable");
            int ownerFld = m_ExtendedAttributesTable.FindField("OwnerID");
            int propertyFld = m_ExtendedAttributesTable.FindField("Property");
            int propertyValueFld = m_ExtendedAttributesTable.FindField("PropertyValue");
            int valueLinkFld = m_ExtendedAttributesTable.FindField("ValueLinkID");
            int qualifierFld = m_ExtendedAttributesTable.FindField("Qualifier");
            int dataSrcFld = m_ExtendedAttributesTable.FindField("DataSourceID");
            int notesFld = m_ExtendedAttributesTable.FindField("Notes");

            ICursor theCursor;

            if (SqlWhereClause == null) { theCursor = m_ExtendedAttributesTable.Search(null, false); }
            else
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = SqlWhereClause;
                theCursor = m_ExtendedAttributesTable.Search(QF, false);
            }

            IRow theRow = theCursor.NextRow();

            while (theRow != null)
            {
                ExtendedAttributes anExtendedAttributes = new ExtendedAttributes();
                anExtendedAttributes.ExtendedAttributes_ID = theRow.get_Value(idFld).ToString();
                anExtendedAttributes.OwnerTable = theRow.get_Value(ownerTableFld).ToString();
                anExtendedAttributes.OwnerID = theRow.get_Value(ownerFld).ToString();
                anExtendedAttributes.Property = theRow.get_Value(propertyFld).ToString();
                anExtendedAttributes.PropertyValue = theRow.get_Value(propertyValueFld).ToString();
                anExtendedAttributes.ValueLinkID = theRow.get_Value(valueLinkFld).ToString();
                anExtendedAttributes.Qualifier = theRow.get_Value(qualifierFld).ToString();
                anExtendedAttributes.DataSourceID = theRow.get_Value(dataSrcFld).ToString();
                anExtendedAttributes.Notes = theRow.get_Value(notesFld).ToString();
                anExtendedAttributes.RequiresUpdate = true;

                m_ExtendedAttributesDictionary.Add(anExtendedAttributes.ExtendedAttributes_ID, anExtendedAttributes);

                theRow = theCursor.NextRow();
            }
        }

        public string NewExtendedAttributes(string OwnerID, string OwnerTable, string Property, string PropertyValue,
            string ValueLinkID, string Qualifier, string DataSourceID, string Notes)
        {
            ExtendedAttributes newExtendedAttributes = new ExtendedAttributes();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newExtendedAttributes.ExtendedAttributes_ID = SysInfoTable.ProjAbbr + ".ExtendedAttributes." + SysInfoTable.GetNextIdValue("ExtendedAttributes");

            newExtendedAttributes.OwnerID = OwnerID;
            newExtendedAttributes.OwnerTable = OwnerTable;
            newExtendedAttributes.Property = Property;
            newExtendedAttributes.PropertyValue = PropertyValue;
            newExtendedAttributes.ValueLinkID = ValueLinkID;
            newExtendedAttributes.Qualifier = Qualifier;
            newExtendedAttributes.DataSourceID = DataSourceID;
            newExtendedAttributes.Notes = Notes;

            m_ExtendedAttributesDictionary.Add(newExtendedAttributes.ExtendedAttributes_ID, newExtendedAttributes);

            return newExtendedAttributes.ExtendedAttributes_ID;
        }

        public void UpdateExtendedAttributes(ExtendedAttributes theExtendedAttributes)
        {
            try { m_ExtendedAttributesDictionary.Remove(theExtendedAttributes.ExtendedAttributes_ID); }
            catch { }

            theExtendedAttributes.RequiresUpdate = true;
            m_ExtendedAttributesDictionary.Add(theExtendedAttributes.ExtendedAttributes_ID, theExtendedAttributes);
        }

        public void SaveExtendedAttributes()
        {
            int idFld = m_ExtendedAttributesTable.FindField("ExtendedAttributes_ID");
            int ownerTableFld = m_ExtendedAttributesTable.FindField("OwnerTable");
            int ownerFld = m_ExtendedAttributesTable.FindField("OwnerID");
            int propertyFld = m_ExtendedAttributesTable.FindField("Property");
            int propertyValueFld = m_ExtendedAttributesTable.FindField("PropertyValue");
            int valueLinkFld = m_ExtendedAttributesTable.FindField("ValueLinkID");
            int qualifierFld = m_ExtendedAttributesTable.FindField("Qualifier");
            int dataSrcFld = m_ExtendedAttributesTable.FindField("DataSourceID");
            int notesFld = m_ExtendedAttributesTable.FindField("Notes");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();
            
            try
            {
                string updateWhereClause = "ExtendedAttributes_ID = '";
                ICursor insertCursor = m_ExtendedAttributesTable.Insert(true);

                foreach (KeyValuePair<string, ExtendedAttributes> aDictionaryEntry in m_ExtendedAttributesDictionary)
                {
                    ExtendedAttributes thisExtendedAttributes = (ExtendedAttributes)aDictionaryEntry.Value;
                    switch (thisExtendedAttributes.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisExtendedAttributes.ExtendedAttributes_ID + "' OR ExtendedAttributes_ID = '";
                            break;

                        case false:
                            IRowBuffer theRowBuffer = m_ExtendedAttributesTable.CreateRowBuffer();
                            theRowBuffer.set_Value(idFld, thisExtendedAttributes.ExtendedAttributes_ID);
                            theRowBuffer.set_Value(ownerFld, thisExtendedAttributes.OwnerID);
                            theRowBuffer.set_Value(propertyFld, thisExtendedAttributes.Property);
                            theRowBuffer.set_Value(propertyValueFld, thisExtendedAttributes.PropertyValue);
                            theRowBuffer.set_Value(valueLinkFld, thisExtendedAttributes.ValueLinkID);
                            theRowBuffer.set_Value(qualifierFld, thisExtendedAttributes.Qualifier);
                            theRowBuffer.set_Value(dataSrcFld, thisExtendedAttributes.DataSourceID);
                            theRowBuffer.set_Value(notesFld, thisExtendedAttributes.Notes);

                            insertCursor.InsertRow(theRowBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert ExtendedAttributes");

                if (updateWhereClause == "ExtendedAttributes_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 29);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                ICursor updateCursor = m_ExtendedAttributesTable.Update(QF, false);
                IRow theRow = updateCursor.NextRow();

                while (theRow != null)
                {
                    string theID = theRow.get_Value(idFld).ToString();

                    ExtendedAttributes thisExtendedAttributes = m_ExtendedAttributesDictionary[theID];
                    theRow.set_Value(ownerFld, thisExtendedAttributes.OwnerID);
                    theRow.set_Value(propertyFld, thisExtendedAttributes.Property);
                    theRow.set_Value(propertyValueFld, thisExtendedAttributes.PropertyValue);
                    theRow.set_Value(valueLinkFld, thisExtendedAttributes.ValueLinkID);
                    theRow.set_Value(qualifierFld, thisExtendedAttributes.Qualifier);
                    theRow.set_Value(dataSrcFld, thisExtendedAttributes.DataSourceID);
                    theRow.set_Value(notesFld, thisExtendedAttributes.Notes);

                    updateCursor.UpdateRow(theRow);

                    theRow = updateCursor.NextRow();
                }

                theEditor.StopOperation("Update ExtendedAttributes");
            }
            catch { theEditor.StopOperation("ExtendedAttributes Management Failure"); }
         
        }

        public void DeleteExtendedAttributes(ExtendedAttributes theExtendedAttributes)
        {
            try { m_ExtendedAttributesDictionary.Remove(theExtendedAttributes.ExtendedAttributes_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace);  }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "ExtendedAttributes_ID = '" + theExtendedAttributes.ExtendedAttributes_ID + "'";

                m_ExtendedAttributesTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete ExtendedAttributes");
            }
            catch (Exception e) { theEditor.StopOperation("ExtendedAttributes Management Failure");  }
        }
    }
}

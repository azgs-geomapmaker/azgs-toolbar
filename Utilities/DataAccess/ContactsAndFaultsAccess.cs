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
    class ContactsAndFaultsAccess
    {
        IFeatureClass m_ContactsAndFaultsFC;
        IWorkspace m_theWorkspace;

        public ContactsAndFaultsAccess(IWorkspace theWorkspace)
        {
            m_ContactsAndFaultsFC = commonFunctions.OpenFeatureClass(theWorkspace, "ContactsAndFaults");
            m_theWorkspace = theWorkspace;
        }

        public struct ContactsAndFault
        {
            public string ContactsAndFaults_ID;
            public string Type;
            public int IsConcealed;
            public double LocationConfidenceMeters;
            public string ExistenceConfidence;
            public string IdentityConfidence;
            public string RuleID;
            public string Label;
            public string Notes;
            public string DataSourceID;
            public IPolyline Shape;
            public bool RequiresUpdate;
        }

        private Dictionary<string, ContactsAndFault> m_ContactsAndFaultsDictionary = new Dictionary<string, ContactsAndFault>();
        public Dictionary<string, ContactsAndFault> ContactsAndFaultsDictionary
        {
            get { return m_ContactsAndFaultsDictionary; }
        }

        public void ClearContactsAndFaults()
        {
            m_ContactsAndFaultsDictionary.Clear();
        }

        public void AddContactsAndFaults(string SqlWhereClause)
        {
            int idFld = m_ContactsAndFaultsFC.FindField("ContactsAndFaults_ID");
            int typeFld = m_ContactsAndFaultsFC.FindField("Type");
            int concFld = m_ContactsAndFaultsFC.FindField("IsConcealed");
            int locConfFld = m_ContactsAndFaultsFC.FindField("LocationConfidenceMeters");
            int exConfFld = m_ContactsAndFaultsFC.FindField("ExistenceConfidence");
            int idConfFld = m_ContactsAndFaultsFC.FindField("IdentityConfidence");
            int lblFld = m_ContactsAndFaultsFC.FindField("Label");
            int notesFld = m_ContactsAndFaultsFC.FindField("Notes");
            int dsFld = m_ContactsAndFaultsFC.FindField("DataSourceID");
            int symFld = m_ContactsAndFaultsFC.FindField("RuleID");

            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = SqlWhereClause;

            IFeatureCursor theCursor = m_ContactsAndFaultsFC.Search(QF, false);
            IFeature theFeature = theCursor.NextFeature();

            while (theFeature != null)
            {
                ContactsAndFault anContactsAndFault = new ContactsAndFault();
                anContactsAndFault.ContactsAndFaults_ID = theFeature.get_Value(idFld).ToString();
                anContactsAndFault.Type = theFeature.get_Value(typeFld).ToString();
                bool result = int.TryParse(theFeature.get_Value(concFld).ToString(), out anContactsAndFault.IsConcealed);
                anContactsAndFault.LocationConfidenceMeters = double.Parse(theFeature.get_Value(locConfFld).ToString());
                anContactsAndFault.ExistenceConfidence = theFeature.get_Value(exConfFld).ToString();
                anContactsAndFault.IdentityConfidence = theFeature.get_Value(idConfFld).ToString();
                anContactsAndFault.Label = theFeature.get_Value(lblFld).ToString();
                anContactsAndFault.Notes = theFeature.get_Value(notesFld).ToString();
                anContactsAndFault.DataSourceID = theFeature.get_Value(dsFld).ToString();
                anContactsAndFault.RuleID = theFeature.get_Value(symFld).ToString();
                anContactsAndFault.Shape = (IPolyline)theFeature.Shape;
                anContactsAndFault.RequiresUpdate = true;

                m_ContactsAndFaultsDictionary.Add(anContactsAndFault.ContactsAndFaults_ID, anContactsAndFault);

                theFeature = theCursor.NextFeature();
            }
        }

        public string NewContactsAndFault(string Type, int IsConcealed, double LocationConfidenceMeters, string ExistenceConfidence, string IdentityConfidence,
            string Label, string Notes, string DataSourceID, string RuleID, IPolyline Shape)
        {
            ContactsAndFault newContactsAndFault = new ContactsAndFault();

            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newContactsAndFault.ContactsAndFaults_ID = SysInfoTable.ProjAbbr + ".ContactsAndFaults." + SysInfoTable.GetNextIdValue("ContactsAndFaults");
            newContactsAndFault.Type = Type;
            newContactsAndFault.IsConcealed = IsConcealed;
            newContactsAndFault.LocationConfidenceMeters = LocationConfidenceMeters;
            newContactsAndFault.ExistenceConfidence = ExistenceConfidence;
            newContactsAndFault.IdentityConfidence = IdentityConfidence;
            newContactsAndFault.Label = Label;
            newContactsAndFault.Notes = Notes;
            newContactsAndFault.DataSourceID = DataSourceID;
            newContactsAndFault.RuleID = RuleID;
            newContactsAndFault.Shape = Shape;
            newContactsAndFault.RequiresUpdate = false;

            m_ContactsAndFaultsDictionary.Add(newContactsAndFault.ContactsAndFaults_ID, newContactsAndFault);
            return newContactsAndFault.ContactsAndFaults_ID;
        }

        public void UpdateContactsAndFault(ContactsAndFault theContactsAndFault)
        {
            try { m_ContactsAndFaultsDictionary.Remove(theContactsAndFault.ContactsAndFaults_ID); }
            catch { }

            theContactsAndFault.RequiresUpdate = true;
            m_ContactsAndFaultsDictionary.Add(theContactsAndFault.ContactsAndFaults_ID, theContactsAndFault);
        }

        public void SaveContactsAndFaults()
        {
            int idFld = m_ContactsAndFaultsFC.FindField("ContactsAndFaults_ID");
            int typeFld = m_ContactsAndFaultsFC.FindField("Type");
            int concFld = m_ContactsAndFaultsFC.FindField("IsConcealed");
            int locConfFld = m_ContactsAndFaultsFC.FindField("LocationConfidenceMeters");
            int exConfFld = m_ContactsAndFaultsFC.FindField("ExistenceConfidence");
            int idConfFld = m_ContactsAndFaultsFC.FindField("IdentityConfidence");
            int lblFld = m_ContactsAndFaultsFC.FindField("Label");
            int notesFld = m_ContactsAndFaultsFC.FindField("Notes");
            int dsFld = m_ContactsAndFaultsFC.FindField("DataSourceID");
            int symFld = m_ContactsAndFaultsFC.FindField("RuleID");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "ContactsAndFaults_ID = '";
                IFeatureCursor insertCursor = m_ContactsAndFaultsFC.Insert(true);

                foreach (KeyValuePair<string, ContactsAndFault> aDictionaryEntry in m_ContactsAndFaultsDictionary)
                {
                    ContactsAndFault thisContactsAndFault = (ContactsAndFault)aDictionaryEntry.Value;
                    switch (thisContactsAndFault.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisContactsAndFault.ContactsAndFaults_ID + "' OR ContactsAndFaults_ID = '";
                            break;

                        case false:
                            IFeatureBuffer theFeatureBuffer = m_ContactsAndFaultsFC.CreateFeatureBuffer();
                            theFeatureBuffer.set_Value(idFld, thisContactsAndFault.ContactsAndFaults_ID);
                            theFeatureBuffer.set_Value(typeFld, thisContactsAndFault.Type);
                            theFeatureBuffer.set_Value(concFld, thisContactsAndFault.IsConcealed);
                            theFeatureBuffer.set_Value(locConfFld, thisContactsAndFault.LocationConfidenceMeters);
                            theFeatureBuffer.set_Value(exConfFld, thisContactsAndFault.ExistenceConfidence);
                            theFeatureBuffer.set_Value(idConfFld, thisContactsAndFault.IdentityConfidence);
                            theFeatureBuffer.set_Value(lblFld, thisContactsAndFault.Label);
                            theFeatureBuffer.set_Value(notesFld, thisContactsAndFault.Notes);
                            theFeatureBuffer.set_Value(dsFld, thisContactsAndFault.DataSourceID);
                            theFeatureBuffer.set_Value(symFld, thisContactsAndFault.RuleID);
                            theFeatureBuffer.Shape = thisContactsAndFault.Shape;

                            insertCursor.InsertFeature(theFeatureBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert ContactsAndFaults");

                if (updateWhereClause == "ContactsAndFaults_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 28);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                IFeatureCursor updateCursor = m_ContactsAndFaultsFC.Update(QF, false);
                IFeature theFeature = updateCursor.NextFeature();

                while (theFeature != null)
                {
                    string theID = theFeature.get_Value(idFld).ToString();

                    ContactsAndFault thisContactsAndFault = m_ContactsAndFaultsDictionary[theID];
                    theFeature.set_Value(typeFld, thisContactsAndFault.Type);
                    theFeature.set_Value(concFld, thisContactsAndFault.IsConcealed);
                    theFeature.set_Value(locConfFld, thisContactsAndFault.LocationConfidenceMeters);
                    theFeature.set_Value(exConfFld, thisContactsAndFault.ExistenceConfidence);
                    theFeature.set_Value(idConfFld, thisContactsAndFault.IdentityConfidence);
                    theFeature.set_Value(lblFld, thisContactsAndFault.Label);
                    theFeature.set_Value(notesFld, thisContactsAndFault.Notes);
                    theFeature.set_Value(dsFld, thisContactsAndFault.DataSourceID);
                    theFeature.set_Value(symFld, thisContactsAndFault.RuleID);
                    theFeature.Shape = thisContactsAndFault.Shape;
                    updateCursor.UpdateFeature(theFeature);

                    theFeature = updateCursor.NextFeature();
                }

                theEditor.StopOperation("Update ContactsAndFaults");
            }
            catch { theEditor.StopOperation("ContactsAndFaults Management Failure"); }
        }

        public void DeleteContactsAndFaults(ContactsAndFault theContactsAndFault)
        {
            try { m_ContactsAndFaultsDictionary.Remove(theContactsAndFault.ContactsAndFaults_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "ContactsAndFaults_ID = '" + theContactsAndFault.ContactsAndFaults_ID + "'";

                ITable ContactsAndFaultsTable = m_ContactsAndFaultsFC as ITable;
                ContactsAndFaultsTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete ContactsAndFaults");
            }
            catch (Exception e) { theEditor.StopOperation("ContactsAndFaults Management Failure"); }
        }
    }
}

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
    class GeologicEventsAccess
    {
        ITable m_GeologicEventsTable;
        IWorkspace m_theWorkspace;

        public GeologicEventsAccess(IWorkspace theWorkspace)
        {
            m_GeologicEventsTable = commonFunctions.OpenTable(theWorkspace, "GeologicEvents");
            m_theWorkspace = theWorkspace;
        }

        public struct GeologicEvents
        {
            public string GeologicEvents_ID;
            public string Event;
            public string AgeDisplay;
            public string AgeYoungerTerm;
            public string AgeOlderTerm;
            public string TimeScale;
            public string AgeYoungerValue;
            public string AgeOlderValue;
            public string DataSourceID;
            public string Notes;
            public bool RequiresUpdate;
        }

        private Dictionary<string, GeologicEvents> m_GeologicEventsDictionary = new Dictionary<string, GeologicEvents>();
        public Dictionary<string, GeologicEvents> GeologicEventsDictionary
        {
            get { return m_GeologicEventsDictionary; }
            set { m_GeologicEventsDictionary = value; }
        }

        public void ClearDescriptionOfMapUnits()
        {
            m_GeologicEventsDictionary.Clear();
        }

        public void AddGeologicEvents(string SqlWhereClause = null)
        {
            int idFld = m_GeologicEventsTable.FindField("GeologicEvents_ID");
            int eventFld = m_GeologicEventsTable.FindField("Event");
            int ageDisplayFld = m_GeologicEventsTable.FindField("AgeDisplay");
            int ageYoungerTermFld = m_GeologicEventsTable.FindField("AgeYoungerTerm");
            int ageOlderTermFld = m_GeologicEventsTable.FindField("AgeOlderTerm");
            int timeScaleFld = m_GeologicEventsTable.FindField("TimeScale");
            int ageYoungerValueFld = m_GeologicEventsTable.FindField("AgeYoungerValue");
            int ageOlderValueFld = m_GeologicEventsTable.FindField("AgeOlderValue");
            int dataSrcFld = m_GeologicEventsTable.FindField("DataSourceID");
            int notesFld = m_GeologicEventsTable.FindField("Notes");

            ICursor theCursor;

            if (SqlWhereClause == null) { theCursor = m_GeologicEventsTable.Search(null, false); }
            else
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = SqlWhereClause;
                theCursor = m_GeologicEventsTable.Search(QF, false);
            }

            IRow theRow = theCursor.NextRow();

            while (theRow != null)
            {
                GeologicEvents anGeologicEvents = new GeologicEvents();
                anGeologicEvents.GeologicEvents_ID = theRow.get_Value(idFld).ToString();
                anGeologicEvents.Event = theRow.get_Value(eventFld).ToString();
                anGeologicEvents.AgeDisplay = theRow.get_Value(ageDisplayFld).ToString();
                anGeologicEvents.AgeYoungerTerm = theRow.get_Value(ageYoungerTermFld).ToString();
                anGeologicEvents.AgeOlderTerm = theRow.get_Value(ageOlderTermFld).ToString();
                anGeologicEvents.TimeScale = theRow.get_Value(timeScaleFld).ToString();
                anGeologicEvents.AgeYoungerValue = theRow.get_Value(ageYoungerValueFld).ToString();
                anGeologicEvents.AgeOlderValue = theRow.get_Value(ageOlderValueFld).ToString();
                anGeologicEvents.DataSourceID = theRow.get_Value(dataSrcFld).ToString();
                anGeologicEvents.Notes = theRow.get_Value(notesFld).ToString();
                anGeologicEvents.RequiresUpdate = true;

                m_GeologicEventsDictionary.Add(anGeologicEvents.GeologicEvents_ID, anGeologicEvents);

                theRow = theCursor.NextRow();
            }
        }

        public string NewGeologicEvents(string Event, string AgeDisplay, string AgeYoungerTerm, string AgeOlderTerm, string TimeScale,
            string AgeYoungerValue, string AgeOlderValue, string DataSourceID, string Notes)
        {
            GeologicEvents newGeologicEvents = new GeologicEvents();
            
            sysInfo SysInfoTable = new sysInfo(m_theWorkspace);
            newGeologicEvents.GeologicEvents_ID = SysInfoTable.ProjAbbr + ".GeologicEvents." + SysInfoTable.GetNextIdValue("GeologicEvents");

            newGeologicEvents.Event = Event;
            newGeologicEvents.AgeDisplay = AgeDisplay;
            newGeologicEvents.AgeYoungerTerm = AgeYoungerTerm;
            newGeologicEvents.AgeOlderTerm= AgeOlderTerm;
            newGeologicEvents.TimeScale = TimeScale;
            newGeologicEvents.AgeYoungerValue = AgeYoungerValue;
            newGeologicEvents.AgeOlderValue = AgeOlderValue;
            newGeologicEvents.DataSourceID = DataSourceID;
            newGeologicEvents.Notes = Notes;
            newGeologicEvents.RequiresUpdate = false;

            m_GeologicEventsDictionary.Add(newGeologicEvents.GeologicEvents_ID, newGeologicEvents);
            return newGeologicEvents.GeologicEvents_ID;
        }

        public void UpdateGeologicEvents(GeologicEvents theGeologicEvents)
        {
            try { m_GeologicEventsDictionary.Remove(theGeologicEvents.GeologicEvents_ID); }
            catch { }

            theGeologicEvents.RequiresUpdate = true;
            m_GeologicEventsDictionary.Add(theGeologicEvents.GeologicEvents_ID, theGeologicEvents);
        }

        public void SaveGeologicEvents()
        {
            int idFld = m_GeologicEventsTable.FindField("GeologicEvents_ID");
            int eventFld = m_GeologicEventsTable.FindField("Event");
            int ageDisplayFld = m_GeologicEventsTable.FindField("AgeDisplay");
            int ageYoungerTermFld = m_GeologicEventsTable.FindField("AgeYoungerTerm");
            int ageOlderTermFld = m_GeologicEventsTable.FindField("AgeOlderTerm");
            int timeScaleFld = m_GeologicEventsTable.FindField("TimeScale");
            int ageYoungerValueFld = m_GeologicEventsTable.FindField("AgeYoungerValue");
            int ageOlderValueFld = m_GeologicEventsTable.FindField("AgeOlderValue");
            int dataSrcFld = m_GeologicEventsTable.FindField("DataSourceID");
            int notesFld = m_GeologicEventsTable.FindField("Notes");

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace); }
            theEditor.StartOperation();

            try
            {
                string updateWhereClause = "GeologicEvents_ID = '";
                ICursor insertCursor = m_GeologicEventsTable.Insert(true);

                foreach(KeyValuePair<string, GeologicEvents> aDictionaryEntry in m_GeologicEventsDictionary)
                {
                    GeologicEvents thisGeologicEvents = (GeologicEvents)aDictionaryEntry.Value;
                    switch (thisGeologicEvents.RequiresUpdate)
                    {
                        case true:
                            updateWhereClause += thisGeologicEvents.GeologicEvents_ID + "' OR GeologicEvents_ID = '";
                            break;

                        case false:
                            IRowBuffer theRowBuffer = m_GeologicEventsTable.CreateRowBuffer();
                            theRowBuffer.set_Value(idFld, thisGeologicEvents.GeologicEvents_ID);
                            theRowBuffer.set_Value(eventFld, thisGeologicEvents.Event);
                            theRowBuffer.set_Value(ageDisplayFld, thisGeologicEvents.AgeDisplay);
                            theRowBuffer.set_Value(ageYoungerTermFld, thisGeologicEvents.AgeYoungerTerm);
                            theRowBuffer.set_Value(ageOlderTermFld, thisGeologicEvents.AgeOlderTerm);
                            theRowBuffer.set_Value(timeScaleFld, thisGeologicEvents.TimeScale);
                            theRowBuffer.set_Value(ageYoungerValueFld, thisGeologicEvents.AgeYoungerValue);
                            theRowBuffer.set_Value(ageOlderValueFld, thisGeologicEvents.AgeOlderValue);
                            theRowBuffer.set_Value(dataSrcFld, thisGeologicEvents.DataSourceID);
                            theRowBuffer.set_Value(notesFld, thisGeologicEvents.Notes);

                            insertCursor.InsertRow(theRowBuffer);
                            break;
                    }
                }

                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
                theEditor.StopOperation("Insert GeologicEvents");

                if (updateWhereClause == "GeologicEvents_ID = '") { return; }

                theEditor.StartOperation();
                updateWhereClause = updateWhereClause.Remove(updateWhereClause.Length - 25);

                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = updateWhereClause;

                ICursor updateCursor = m_GeologicEventsTable.Update(QF, false);
                IRow theRow = updateCursor.NextRow();

                while (theRow != null)
                {
                    string theID = theRow.get_Value(idFld).ToString();

                    GeologicEvents thisGeologicEvents = m_GeologicEventsDictionary[theID];
                    theRow.set_Value(eventFld, thisGeologicEvents.Event);
                    theRow.set_Value(ageDisplayFld, thisGeologicEvents.AgeDisplay);
                    theRow.set_Value(ageYoungerTermFld, thisGeologicEvents.AgeYoungerTerm);
                    theRow.set_Value(ageOlderTermFld, thisGeologicEvents.AgeOlderTerm);
                    theRow.set_Value(timeScaleFld, thisGeologicEvents.TimeScale);
                    theRow.set_Value(ageYoungerValueFld, thisGeologicEvents.AgeYoungerValue);
                    theRow.set_Value(ageOlderValueFld, thisGeologicEvents.AgeOlderValue);
                    theRow.set_Value(dataSrcFld, thisGeologicEvents.DataSourceID);
                    theRow.set_Value(notesFld, thisGeologicEvents.Notes);

                    updateCursor.UpdateRow(theRow);

                    theRow = updateCursor.NextRow();
                }

                theEditor.StopOperation("Update GeologicEvents");
            }
            catch { theEditor.StopOperation("GeologicEvents Management Failure");  }
        }

        public void DeleteGeologicEvents(GeologicEvents theGeologicEvents)
        {
            try { m_GeologicEventsDictionary.Remove(theGeologicEvents.GeologicEvents_ID); }
            catch { }

            IEditor theEditor = ArcMap.Editor;
            if (theEditor.EditState == esriEditState.esriStateNotEditing) { theEditor.StartEditing(m_theWorkspace);  }
            theEditor.StartOperation();

            try
            {
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "GeologicEvents_ID = '" + theGeologicEvents.GeologicEvents_ID + "'";

                m_GeologicEventsTable.DeleteSearchedRows(QF);

                theEditor.StopOperation("Delete GeologicEvents");
            }
            catch (Exception e) { theEditor.StopOperation("GeologicEvents Management Failure");  }
        }
    }
}

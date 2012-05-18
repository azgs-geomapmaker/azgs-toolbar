using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ncgmpToolbar.Utilities;

namespace ncgmpToolbar.Utilities.DataAccess
{
    
    public class sysInfo
    {
        private ITable m_SysInfo;

        public sysInfo(IWorkspace theWorkspace)
        {
            m_SysInfo = commonFunctions.OpenTable(theWorkspace, "SysInfo");
        }

        public string ProjName
        {
            get
            {
                // Create a query to find the Project Name from the SysInfo table
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "Sub = 'Project' AND Pred = 'Name'";

                // Perform the query
                ICursor readCursor = m_SysInfo.Search(QF, false);
                IRow readRow = readCursor.NextRow();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(readCursor);

                // Return the value, if it exists
                if (readRow == null) { return null; }
                return (string)readRow.get_Value(m_SysInfo.FindField("Obj"));

                
            }
            set
            {
                // Create a query to find out if the Project Name is already defined in the SysInfo table
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "Sub = 'Project' AND Pred = 'Name'";

                // Perform the query
                ICursor readCursor = m_SysInfo.Search(QF, false);
                IRow readRow = readCursor.NextRow();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(readCursor);

                // If there is a row returned, update it, otherwise create a new row
                if (readRow == null)
                {
                    // No record, make a new row
                    IRowBuffer newRow = m_SysInfo.CreateRowBuffer();
                    newRow.set_Value(m_SysInfo.FindField("Sub"), "Project");
                    newRow.set_Value(m_SysInfo.FindField("Pred"), "Name");
                    newRow.set_Value(m_SysInfo.FindField("Obj"), value);

                    // Write the new record
                    ICursor writeCursor = m_SysInfo.Insert(true);
                    writeCursor.InsertRow(newRow);
                    writeCursor.Flush();

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                else
                {
                    // Found a record, update it
                    ICursor updateCursor = m_SysInfo.Update(QF, false);
                    IRow updateRow = updateCursor.NextRow();
                    updateRow.set_Value(m_SysInfo.FindField("Obj"), value);
                    // I think I've found that flushing update cursors is usually a bad idea
                    //updateCursor.Flush();

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(updateCursor);
                }
            }
        }

        public string ProjAbbr
        {
            get
            {
                // Create a query to find the Project's abbreviation.
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "Sub = 'Project' AND Pred = 'Abbreviation'";

                // Perform the query
                ICursor readCursor = m_SysInfo.Search(QF, false);
                IRow readRow = readCursor.NextRow();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(readCursor);

                // Return the value, if it exists
                if (readRow == null) { return null; }
                return (string)readRow.get_Value(m_SysInfo.FindField("Obj"));
            }
            set
            {
                // Create a query to find the Project abbreviation, if it exists
                IQueryFilter QF = new QueryFilterClass();
                QF.WhereClause = "Sub = 'Project' AND Pred = 'Abbreviation'";

                // Perform the query
                ICursor readCursor = m_SysInfo.Search(QF, false);
                IRow readRow = readCursor.NextRow();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(readCursor);

                // If there is a row returned, update it, otherwise create a new row
                if (readRow == null)
                {
                    // There is no pre-existing record -- create a new row and assign the new values
                    IRowBuffer newRow = m_SysInfo.CreateRowBuffer();
                    newRow.set_Value(m_SysInfo.FindField("Sub"), "Project");
                    newRow.set_Value(m_SysInfo.FindField("Pred"), "Abbreviation");
                    newRow.set_Value(m_SysInfo.FindField("Obj"), value);

                    // Write the new row
                    ICursor writeCursor = m_SysInfo.Insert(true);
                    writeCursor.InsertRow(newRow);
                    writeCursor.Flush();

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);
                }
                else
                {
                    // There is an existing record, update it
                    ICursor updateCursor = m_SysInfo.Update(QF, false);
                    IRow updateRow = updateCursor.NextRow();
                    updateRow.set_Value(m_SysInfo.FindField("Obj"), value);
                    //updateCursor.Flush();

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(updateCursor);
                }
            }
        }

        public int GetNextIdValue(string ClassName)
        {
            // ----------------------------------------------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------
            // By the way, it's important to remember that if the SysInfo table gets versioned, table identifiers are going to get botched.
            //  Do not version the SysInfo table.
            //  ArcObjects is happy to manipulate it outside an edit session
            // ----------------------------------------------------------------------------------------------------------------------------
            // ----------------------------------------------------------------------------------------------------------------------------

            // Create a query to find the existing Max ID
            IQueryFilter QF = new QueryFilterClass();
            QF.WhereClause = "Sub = '" + ClassName + "' AND Pred = 'MaxID'";

            // Perform the query
            ICursor readCursor = m_SysInfo.Search(QF, false);
            IRow readRow = readCursor.NextRow();

            // If a record exists, update it to the new value and return that new value, otherwise, new row
            if (readRow == null)
            {
                // No record exists - create a new row and assign #1
                IRowBuffer newRow = m_SysInfo.CreateRowBuffer();
                newRow.set_Value(m_SysInfo.FindField("Sub"), ClassName);
                newRow.set_Value(m_SysInfo.FindField("Pred"), "MaxID");
                newRow.set_Value(m_SysInfo.FindField("Obj"), 1);

                // Write the row
                ICursor writeCursor = m_SysInfo.Insert(true);
                writeCursor.InsertRow(newRow);
                writeCursor.Flush();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(writeCursor);

                // Return the ID
                return 1;
            }
            else
            {
                // A record exists - return the next value and increment the record
                string currentMax = (string)readRow.get_Value(m_SysInfo.FindField("Obj"));
                int newMax = int.Parse(currentMax) + 1;

                // Update the record
                ICursor updateCursor = m_SysInfo.Update(QF, false);
                IRow updateRow = updateCursor.NextRow();
                updateRow.set_Value(m_SysInfo.FindField("Obj"), newMax);
                updateCursor.UpdateRow(updateRow);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(updateCursor);
                //Flush failed when editing an SDE workspace where the SysInfo table was not versioned.
                //It didn't die without the flush in a File Geodatabase, but there was some kind of a hang that slowed ArcMap way down while debugging
                //  may have been unrelated
                //updateCursor.Flush();

                // Return the ID
                return newMax;
            }

        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ncgmpToolbar.Utilities;
using ncgmpToolbar.Utilities.DataAccess;

namespace ncgmpToolbar.Forms
{
    public partial class sourceLegendItemSelection : Form
    {
        private bool m_canceled = false;
        public bool Canceled
        {
            get { return m_canceled; }
        }

        public List<string> idsToCopy = new List<string>();
        private IWorkspace m_theWorkspace;
        
        public sourceLegendItemSelection(IWorkspace theWorkspace)
        {
            InitializeComponent();

            sysInfo sourceSysInfo = new sysInfo(theWorkspace);
            this.grpSourceLegend.Text = sourceSysInfo.ProjName;

            m_theWorkspace = theWorkspace;

            PopulateListbox();
        }

        private void PopulateListbox()
        {
            // Get the Dmu sorted by hierarchy
            var sortedDmu = GetDmuSortedByHierarchy();

            // Clear the Listbox
            lstSourceLegend.Items.Clear();

            // Loop through the records, add to the listbox
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> anEntry in sortedDmu)
            {
                if (anEntry.Value.ParagraphStyle.Contains("Heading") == true)
                {
                    lstSourceLegend.Items.Add(anEntry.Value.Name);
                }
                else
                {
                    lstSourceLegend.Items.Add(anEntry.Value.MapUnit + " - " + anEntry.Value.Name);
                }                
            }
        }

        private IOrderedEnumerable<KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit>> GetDmuSortedByHierarchy()
        {
            // Get All DescriptionOfMapUnits.
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);
            DmuAccess.AddDescriptionOfMapUnits();

            // Sort using Linq syntax
            var sortedDmuEntries = (
                from entry in DmuAccess.DescriptionOfMapUnitsDictionary
                orderby ((DescriptionOfMapUnitsAccess.DescriptionOfMapUnit)entry.Value).HierarchyKey ascending
                select entry);

            return sortedDmuEntries;
        }

        private Dictionary<string, string> GetDmuIdFromNameLookup()
        {
            // First get all DMU entries
            DescriptionOfMapUnitsAccess DmuAccess = new DescriptionOfMapUnitsAccess(m_theWorkspace);
            DmuAccess.AddDescriptionOfMapUnits();

            // Pass these records into a new Dictionary correlating ID to HierarchyKey
            Dictionary<string, string> idDmuDictionary = new Dictionary<string, string>();
            foreach (KeyValuePair<string, DescriptionOfMapUnitsAccess.DescriptionOfMapUnit> aDictionaryEntry in DmuAccess.DescriptionOfMapUnitsDictionary)
            {
                DescriptionOfMapUnitsAccess.DescriptionOfMapUnit thisDmuEntry = (DescriptionOfMapUnitsAccess.DescriptionOfMapUnit)aDictionaryEntry.Value;
                if (thisDmuEntry.ParagraphStyle.Contains("Heading") == true) { idDmuDictionary.Add(thisDmuEntry.Name, thisDmuEntry.DescriptionOfMapUnits_ID); }
                else { idDmuDictionary.Add(thisDmuEntry.MapUnit + " - " + thisDmuEntry.Name, thisDmuEntry.DescriptionOfMapUnits_ID); }
                
            }

            return idDmuDictionary;

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            idsToCopy = null;
            m_canceled = true;
            this.Hide();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            // Get a dictionary to look up IDs based on the names in the listbox
            Dictionary<string, string> idValueLookup = GetDmuIdFromNameLookup();
                       
            foreach (string selectedItem in lstSourceLegend.SelectedItems) { idsToCopy.Add(idValueLookup[selectedItem].ToString()); }
            this.Hide();
        }

        
    }
}

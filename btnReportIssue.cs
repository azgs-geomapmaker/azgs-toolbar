using System;
using System.Windows.Forms;

namespace ncgmpToolbar
{
    public class btnReportIssue : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public btnReportIssue()
        {
        }

        protected override void OnClick()
        {
            try {
                string targetURL = "https://github.com/ncgmp09/azgs-toolbar/issues";
                System.Diagnostics.Process.Start(targetURL);
            }
            catch {
            }
        }
    }
}

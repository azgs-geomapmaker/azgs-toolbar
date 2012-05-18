using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ncgmpToolbar.Forms;
using ncgmpToolbar.Utilities;
using ncgmpToolbar.Utilities.DataAccess;

namespace ncgmpToolbar
{
    public class tlDigitizeOrientationData : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        // These variables track the user's click locations and which step they are on
        private int numberOfClicks = 0;
        private double measurementLocationX;
        private double measurementLocationY;
        private double firstEndX;
        private double firstEndY;
        private double secondEndX;
        private double secondEndY;

        public tlDigitizeOrientationData()
        {
            System.Windows.Forms.Cursor locCursor = new System.Windows.Forms.Cursor(GetType(), "Cursors.StationLocCursor.cur");
            Cursor = locCursor;
        }

        protected override void OnUpdate()
        {
            // Should only be enabled if we are editing a valid NCGMP database
            bool pleaseEnabled = ArcMap.Editor.EditState == esriEditState.esriStateEditing;
            if (pleaseEnabled == true) { Enabled = ncgmpEditorExtension.g_EditWorkspaceIsValid; } else { Enabled = false; }
        }

        protected override void  OnMouseUp(MouseEventArgs arg)
        {
            // Did they click the left mouse button?
            if (arg.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Get some application environment variables set
                IMxApplication mxApp = (IMxApplication)ArcMap.Application;

                // Get the clicked location in map units
                IPoint clickedPoint = mxApp.Display.DisplayTransformation.ToMapPoint(arg.Location.X, arg.Location.Y);

                // If this is the first click on the map...
                switch (numberOfClicks)
                {
                    case 0:
                        // This is the first click, giving the location of the measurement.                       
                        measurementLocationX = clickedPoint.X;
                        measurementLocationY = clickedPoint.Y;

                        // Increment the click counter
                        numberOfClicks = 1;

                        // Change the cursor
                        System.Windows.Forms.Cursor firstCursor = new System.Windows.Forms.Cursor(GetType(), "Cursors.firstPointCursor.cur");
                        Cursor = firstCursor;

                        break;

                    case 1:
                        // This is the second click, giving the first end of the strike line, according to right-hand rule.
                        firstEndX = clickedPoint.X;
                        firstEndY = clickedPoint.Y;

                        // Increment the click counter
                        numberOfClicks = 2;

                        // Change the cursor
                        System.Windows.Forms.Cursor secondCursor = new System.Windows.Forms.Cursor(GetType(), "Cursors.SecondPointCursor.cur");
                        Cursor = secondCursor;

                        break;

                    case 2:
                        // This is the third click, giving the second end of the strike line, according to right-hand rule.
                        secondEndX = clickedPoint.X;
                        secondEndY = clickedPoint.Y;

                        // Increment the click counter
                        numberOfClicks = 3;

                        // Change the cursor
                        //base.m_cursor = Properties.Resources.GetDipCursor;

                        // Calculate the Strike from the two collected points
                        int theStrike = GetStrike(firstEndX, firstEndY, secondEndX, secondEndY);

                        // Get the dip input
                        int theDip;
                        string inputValue = "";
                        
                    getDipValue:
                        if (genericForms.InputBox("Digitize Structure Points", "Please enter the dip value", ref inputValue) == DialogResult.OK)
                        {
                            // Make sure they entered a valid dip value
                            int dipValue;
                            bool result = int.TryParse(inputValue, out dipValue);

                            if (result == true)
                            {
                                // inputValue is an integer
                                if (dipValue >= -90)
                                {
                                    if (dipValue <= 90)
                                    {
                                        // inputValue is in the appropriate range
                                        theDip = int.Parse(inputValue);
                                    }
                                    else { goto getDipValue; }
                                }
                                else { goto getDipValue; }
                            }
                            else { goto getDipValue; }
                        }
                        else
                        {
                            // In this case, the action was canceled during the dip input. Reset the counter, cursor, and exit the routine.
                            numberOfClicks = 0;
                            //base.m_cursor = Properties.Resources.firstPointCursor;
                            return;
                        }

                        // Now we need to create the feature. First we'll find the current FeatureTemplate
                        IEditor3 templateEditor = (IEditor3)ArcMap.Editor;
                        IEditTemplate theCurrentTemplate = templateEditor.CurrentTemplate;

                        // If they didn't select a template, they'll need to
                        if (theCurrentTemplate == null)
                        {
                            MessageBox.Show("Please select a feature template for the type of measurement that you're digitizing", "NCGMP Tools");
                            numberOfClicks = 0;
                            System.Windows.Forms.Cursor locCursor = new System.Windows.Forms.Cursor(GetType(), "Cursors.StationLocCursor.cur");
                            Cursor = locCursor;
                            return;
                        }

                        // Make sure that the template puts features into the OrientationDataPoints FeatureClass
                        IFeatureClass templateFC = ((IFeatureLayer)theCurrentTemplate.Layer).FeatureClass;
                        if (templateFC.Equals(commonFunctions.OpenFeatureClass(ArcMap.Editor.EditWorkspace, "OrientationDataPoints")))
                        {                           
                            try
                            {
                                // Create a dummy feature and read in values from the feature template
                                IFeature tempFeature = templateFC.CreateFeature();
                                theCurrentTemplate.SetDefaultValues(tempFeature);

                                // Grab values from the dummy feature - the code is actually cleaner than grabbing the values from the FeatureTemplate
                                string StationID = "";
                                string Type = tempFeature.get_Value(templateFC.FindField("Type")).ToString();
                                string IDConf = tempFeature.get_Value(templateFC.FindField("IdentityConfidence")).ToString();
                                string Label = tempFeature.get_Value(templateFC.FindField("Label")).ToString();
                                string Notes = tempFeature.get_Value(templateFC.FindField("Notes")).ToString();

                                int Plot;
                                bool result = int.TryParse(tempFeature.get_Value(templateFC.FindField("PlotAtScale")).ToString(), out Plot);

                                double OrientConf;
                                result = double.TryParse(tempFeature.get_Value(templateFC.FindField("OrientationConfidenceDegrees")).ToString(), out OrientConf);
                                
                                int Symbol;
                                result = int.TryParse(tempFeature.get_Value(templateFC.FindField("Symbol")).ToString(), out Symbol);                                
         
                                string DataSourceID = commonFunctions.GetCurrentDataSourceID();

                                int SymbolRot = 360 - theStrike;                                                               

                                IPoint Shape = new PointClass();
                                Shape.X = measurementLocationX;
                                Shape.Y = measurementLocationY;

                                // Remove the temporary Feature
                                tempFeature.Delete();

                                // Create the new feature
                                OrientationDataPointsAccess OdpAccess = new OrientationDataPointsAccess(ArcMap.Editor.EditWorkspace);
                                OdpAccess.NewOrientationDataPoint(StationID, Type, IDConf, Label, Plot, (double)theStrike, (double)theDip, OrientConf, Notes, DataSourceID, SymbolRot, Symbol, Shape);
                                OdpAccess.SaveOrientationDataPoints();

                                // Refresh the Active View
                                ArcMap.Document.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, theCurrentTemplate.Layer, null);
                            }
                            catch
                            {
                                MessageBox.Show("Create feature didn't work.", "NCGMP Tools");
                                numberOfClicks = 0;
                                System.Windows.Forms.Cursor locCursor = new System.Windows.Forms.Cursor(GetType(), "Cursors.StationLocCursor.cur");
                                Cursor = locCursor;
                                return;
                            }
                        }
                        else
                        {
                            // User needs to select a relevant feature template
                            MessageBox.Show("Please select a valid feature template to digitize structural measurements.", "NCGMP Tools");
                        }

                        // Finished successfully, reset the counter and cursor
                        numberOfClicks = 0;
                        System.Windows.Forms.Cursor theLocCursor = new System.Windows.Forms.Cursor(GetType(), "Cursors.StationLocCursor.cur");
                        Cursor = theLocCursor;
                        break;

                    default:
                        // Don't do anything here, preventing any more input before the dip is entered and the counter resets
                        break;

                }
            }
        }

        protected override bool OnDeactivate()
        {
            // Reset the Cursor and Counter
            numberOfClicks = 0;
            System.Windows.Forms.Cursor locCursor = new System.Windows.Forms.Cursor(GetType(), "Cursors.StationLocCursor.cur");
            Cursor = locCursor;
            return base.OnDeactivate();
        }

        private int GetStrike(double x1, double y1, double x2, double y2)
        {
            double xdiff = x2 - x1;
            double ydiff = y2 - y1;
            double angle = 450 - (Math.Atan2(ydiff, xdiff) * 180 / Math.PI);

            if (Math.Round(angle, 2) >= 360)
            {
                angle = angle - 360;
            }
            else if (Math.Round(angle, 2) < 0)
            {
                angle = angle + 360;
            }

            return (int)Math.Round(angle, 0);
        }
    }

}

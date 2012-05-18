using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ncgmpToolbar.Utilities.DataAccess;
using ncgmpToolbar.Utilities;


namespace ncgmpToolbar
{
    public class btnTheTestingButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public btnTheTestingButton()
        {
        }

        protected override void OnClick()
        {
            System.Windows.Forms.MessageBox.Show("You're not supposed to do that.");

            
        }

        protected override void OnUpdate()
        {
            // Should only be enabled if we are editing a valid NCGMP database
            //bool pleaseEnabled = ArcMap.Editor.EditState == esriEditState.esriStateEditing;
            //if (pleaseEnabled == true) { Enabled = ncgmpEditorExtension.g_EditWorkspaceIsValid; } else { Enabled = false; }
            Enabled = true;
        }

        private void SetSymbolRotationOverrides(IFeatureClass theFeatureClass)
        {
            // Get RepresentationClass for another method
            IRepresentationClass theRepClass = GetRepClass(theFeatureClass);

            // Grab the enumeration of Representation Rules
            IRepresentationRules theRepRules = theRepClass.RepresentationRules;
            
            // Setup a couple variables and begin iterating through the enumeration
            int ruleId; IRepresentationRule aRepRule;
            theRepRules.Reset();
            theRepRules.Next(out ruleId, out aRepRule);
            while (aRepRule != null)
            {
                // For one particular Rule, set an angle override
                if (ruleId == 339)
                {
                    try
                    {
                        IFieldOverrides fieldOverrides = aRepRule as IFieldOverrides;
                        //IFieldOverride anOverride = fieldOverrides.Next();
                        //while (anOverride != null)
                        //{
                        //    Debug.WriteLine("Graphic Attribute ID: " + anOverride.GraphicAttributeID);
                        //    IGraphicAttributes graphicAttr = anOverride.GraphicAttributes;
                        //    for (int i = 0; i < graphicAttr.GraphicAttributeCount; i++)
                        //    {
                        //        Debug.WriteLine("ID = " + graphicAttr.ID[i]);
                        //        Debug.WriteLine("\tName = " + graphicAttr.Name[i]);
                        //        Debug.WriteLine("\tType = " + graphicAttr.Type[i].Type.ToString());
                        //        Debug.WriteLine("\tValue = " + graphicAttr.Value[i].ToString());
                        //    }
                        //}
                        IGraphicAttributes rotationEffect = new GeometricEffectRotateClass();
                        Debug.WriteLine("Atrribute Count: " + rotationEffect.GraphicAttributeCount);
                        Debug.WriteLine("Attribute Name: " + rotationEffect.get_Name(0));
                        Debug.WriteLine("Attribute Value (pre): " + rotationEffect.get_Value(0));
                        //rotationEffect.set_Value(0, 0);
                        //Debug.WriteLine("Attribute Value (post): " + rotationEffect.get_Value(0));
                        fieldOverrides.Add(rotationEffect, 0, "Override");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.Write(ex.StackTrace);
                       
                    }                   
                }
                theRepRules.Next(out ruleId, out aRepRule);
            }

        }

        private IRepresentationWorkspaceExtension GetRepWSExtFromFeatureClass(IFeatureClass theFeatureClass) 
        {
            IDataset theDS = theFeatureClass as IDataset;
            IWorkspace theWorkspace = theDS.Workspace;
            IWorkspaceExtensionManager theExtManager = theWorkspace as IWorkspaceExtensionManager;

            UID theUID = new UIDClass();
            theUID.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            return theExtManager.FindExtension(theUID) as IRepresentationWorkspaceExtension;
        }

        private IRepresentationClass GetRepClass(IFeatureClass theFeatureClass)
        {
            // Get the RepresentationWorkspaceExtention
            IRepresentationWorkspaceExtension repWsExt = GetRepWSExtFromFeatureClass(theFeatureClass);

            // Get the Representation Names
            IEnumDatasetName repClassNames = repWsExt.get_FeatureClassRepresentationNames(theFeatureClass);

            // Hopefully there's only one... this is just a test after all
            repClassNames.Reset();
            return repWsExt.OpenRepresentationClass(repClassNames.Next().Name);

        }
    }
}


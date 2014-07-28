using System;
using ncgmpToolbar.Utilities;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace ncgmpToolbar
{
    public class tlDrawStratCorDiagram : ESRI.ArcGIS.Desktop.AddIns.Tool
    {
        public tlDrawStratCorDiagram()
        {
        }

        protected override void OnUpdate()
        {
            // Should only be enabled if we are editing a valid NCGMP database in Layout View
            if ((ArcMap.Document.ActiveView is IPageLayout) && (ArcMap.Editor.EditState == esriEditState.esriStateEditing))
            {
                Enabled = ncgmpEditorExtension.g_EditWorkspaceIsValid;
                //System.Windows.Forms.MessageBox.Show(ncgmpEditorExtension.g_EditWorkspaceIsValid.ToString());
            }
            else
            {
                Enabled = false;
            }
        }

        #region Capture Legend Dimensions
        INewEnvelopeFeedback m_Feedback;
        Boolean m_isMouseDown;

        private IPoint getCursorLocation(MouseEventArgs arg)
        {
            IActiveView ActiveView = ArcMap.Document.ActiveView;
            return ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(arg.X, arg.Y) as IPoint;
        }

        protected override void OnMouseDown(ESRI.ArcGIS.Desktop.AddIns.Tool.MouseEventArgs arg)
        {
            IPoint Point = getCursorLocation(arg);
            if (m_Feedback == null)
            {
                m_Feedback = new NewEnvelopeFeedbackClass();
                m_Feedback.Display = ArcMap.Document.ActiveView.ScreenDisplay;
                m_Feedback.Start(Point);
            }
            m_isMouseDown = true;

            base.OnMouseDown(arg);
        }

        protected override void OnMouseMove(MouseEventArgs arg)
        {
            if (m_isMouseDown == false) { return; }

            IPoint Point = getCursorLocation(arg);
            if (m_Feedback != null)
            {
                m_Feedback.MoveTo(Point);
            }

            base.OnMouseMove(arg);
        }

        protected override void OnMouseUp(MouseEventArgs arg)
        {
            IPoint Point = getCursorLocation(arg);
            if (m_Feedback != null)
            {
                IEnvelope Env = m_Feedback.Stop();
                m_Feedback = null;
                drawMapUnits.DrawStratCorDiagram(Env);
            }
            base.OnMouseUp(arg);
        }
        #endregion
    }
}
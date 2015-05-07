//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ncgmpToolbar {
    using ESRI.ArcGIS.Framework;
    using ESRI.ArcGIS.ArcMapUI;
    using ESRI.ArcGIS.Editor;
    using ESRI.ArcGIS.esriSystem;
    using System;
    using System.Collections.Generic;
    using ESRI.ArcGIS.Desktop.AddIns;
    
    
    /// <summary>
    /// A class for looking up declarative information in the associated configuration xml file (.esriaddinx).
    /// </summary>
    internal static class ThisAddIn {
        
        internal static string Name {
            get {
                return "ncgmpToolbar";
            }
        }
        
        internal static string AddInID {
            get {
                return "{39418d12-5d34-4602-9979-2a45d931026d}";
            }
        }
        
        internal static string Company {
            get {
                return "Arizona Geological Survey";
            }
        }
        
        internal static string Version {
            get {
                return "1.8.2";
            }
        }
        
        internal static string Description {
            get {
                return "These set of tools ease the generation of geologic map data in the NCGMP format.";
            }
        }
        
        internal static string Author {
            get {
                return "Ryan Clark, Genhan Chan, Janel Day, Jessica Good, Laura Bookman";
            }
        }
        
        internal static string Date {
            get {
                return "5/7/2015";
            }
        }
        
        internal static ESRI.ArcGIS.esriSystem.UID ToUID(this System.String id) {
            ESRI.ArcGIS.esriSystem.UID uid = new ESRI.ArcGIS.esriSystem.UIDClass();
            uid.Value = id;
            return uid;
        }
        
        /// <summary>
        /// A class for looking up Add-in id strings declared in the associated configuration xml file (.esriaddinx).
        /// </summary>
        internal class IDs {
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_cmbDataSource', the id declared for Add-in ComboBox class 'cmbDataSource'
            /// </summary>
            internal static string cmbDataSource {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_cmbDataSource";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_btnOpenNcgmpDatabase', the id declared for Add-in Button class 'btnOpenNcgmpDatabase'
            /// </summary>
            internal static string btnOpenNcgmpDatabase {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_btnOpenNcgmpDatabase";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_btnAddEditDataSource', the id declared for Add-in Button class 'btnAddEditDataSource'
            /// </summary>
            internal static string btnAddEditDataSource {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_btnAddEditDataSource";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_tlDigitizeOrientationData', the id declared for Add-in Tool class 'tlDigitizeOrientationData'
            /// </summary>
            internal static string tlDigitizeOrientationData {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_tlDigitizeOrientationData";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_btnShowMapUnitLegendEditor', the id declared for Add-in Button class 'btnShowMapUnitLegendEditor'
            /// </summary>
            internal static string btnShowMapUnitLegendEditor {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_btnShowMapUnitLegendEditor";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_btnReportIssue', the id declared for Add-in Button class 'btnReportIssue'
            /// </summary>
            internal static string btnReportIssue {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_btnReportIssue";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_btnTheTestingButton', the id declared for Add-in Button class 'btnTheTestingButton'
            /// </summary>
            internal static string btnTheTestingButton {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_btnTheTestingButton";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_btnDatabaseMaintenance', the id declared for Add-in Button class 'btnDatabaseMaintenance'
            /// </summary>
            internal static string btnDatabaseMaintenance {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_btnDatabaseMaintenance";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_btnNewNcgmpDatabase', the id declared for Add-in Button class 'btnNewNcgmpDatabase'
            /// </summary>
            internal static string btnNewNcgmpDatabase {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_btnNewNcgmpDatabase";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_btnApplySelectedDefaults', the id declared for Add-in Button class 'btnApplySelectedDefaults'
            /// </summary>
            internal static string btnApplySelectedDefaults {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_btnApplySelectedDefaults";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_tlDrawPolygonLegend', the id declared for Add-in Tool class 'tlDrawPolygonLegend'
            /// </summary>
            internal static string tlDrawPolygonLegend {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_tlDrawPolygonLegend";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_tlDrawStratCorDiagram', the id declared for Add-in Tool class 'tlDrawStratCorDiagram'
            /// </summary>
            internal static string tlDrawStratCorDiagram {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_tlDrawStratCorDiagram";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_dwnMapUnitLegendEditor', the id declared for Add-in DockableWindow class 'dwnMapUnitLegendEditor+AddinImpl'
            /// </summary>
            internal static string dwnMapUnitLegendEditor {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_dwnMapUnitLegendEditor";
                }
            }
            
            /// <summary>
            /// Returns 'Arizona_Geological_Survey_ncgmpToolbar_ncgmpEditorExtension', the id declared for Add-in Extension class 'ncgmpEditorExtension'
            /// </summary>
            internal static string ncgmpEditorExtension {
                get {
                    return "Arizona_Geological_Survey_ncgmpToolbar_ncgmpEditorExtension";
                }
            }
        }
    }
    
internal static class ArcMap
{
  private static IApplication s_app = null;
  private static IDocumentEvents_Event s_docEvent;

  public static IApplication Application
  {
    get
    {
      if (s_app == null)
      {
        s_app = Internal.AddInStartupObject.GetHook<IMxApplication>() as IApplication;
        if (s_app == null)
        {
          IEditor editorHost = Internal.AddInStartupObject.GetHook<IEditor>();
          if (editorHost != null)
            s_app = editorHost.Parent;
        }
      }
      return s_app;
    }
  }

  public static IMxDocument Document
  {
    get
    {
      if (Application != null)
        return Application.Document as IMxDocument;

      return null;
    }
  }
  public static IMxApplication ThisApplication
  {
    get { return Application as IMxApplication; }
  }
  public static IDockableWindowManager DockableWindowManager
  {
    get { return Application as IDockableWindowManager; }
  }
  public static IDocumentEvents_Event Events
  {
    get
    {
      s_docEvent = Document as IDocumentEvents_Event;
      return s_docEvent;
    }
  }
  public static IEditor Editor
  {
    get
    {
      UID editorUID = new UID();
      editorUID.Value = "esriEditor.Editor";
      return Application.FindExtensionByCLSID(editorUID) as IEditor;
    }
  }
}

namespace Internal
{
  [StartupObjectAttribute()]
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
  [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
  public sealed partial class AddInStartupObject : AddInEntryPoint
  {
    private static AddInStartupObject _sAddInHostManager;
    private List<object> m_addinHooks = null;

    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    public AddInStartupObject()
    {
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override bool Initialize(object hook)
    {
      bool createSingleton = _sAddInHostManager == null;
      if (createSingleton)
      {
        _sAddInHostManager = this;
        m_addinHooks = new List<object>();
        m_addinHooks.Add(hook);
      }
      else if (!_sAddInHostManager.m_addinHooks.Contains(hook))
        _sAddInHostManager.m_addinHooks.Add(hook);

      return createSingleton;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    protected override void Shutdown()
    {
      _sAddInHostManager = null;
      m_addinHooks = null;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Never)]
    internal static T GetHook<T>() where T : class
    {
      if (_sAddInHostManager != null)
      {
        foreach (object o in _sAddInHostManager.m_addinHooks)
        {
          if (o is T)
            return o as T;
        }
      }

      return null;
    }

    // Expose this instance of Add-in class externally
    public static AddInStartupObject GetThis()
    {
      return _sAddInHostManager;
    }
  }
}
}

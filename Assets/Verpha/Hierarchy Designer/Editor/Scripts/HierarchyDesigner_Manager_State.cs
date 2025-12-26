#if UNITY_EDITOR
using UnityEditor;

namespace Verpha.HierarchyDesigner
{
    internal class HierarchyDesigner_Manager_State : ScriptableSingleton<HierarchyDesigner_Manager_State>
    {
        #region Properties
        public bool isLeftPanelCollapsed = false;
        public bool utilitiesFoldout = false;
        public bool configurationsFoldout = false;
        public HierarchyDesigner_Window_Main.CurrentWindow currentWindow = HierarchyDesigner_Window_Main.CurrentWindow.Home;
        public bool hasInitializedUpdateBoard = false;
        public string sessionUpdateBoardContent = null;
        public bool hasInitializedPatchNotes = false;
        public string sessionPatchNotesContent = null;
        #endregion
    }
}
#endif
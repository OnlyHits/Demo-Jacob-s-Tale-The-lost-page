#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Comic
{
    [CustomEditor(typeof(PanelOutlineController))]
    public class PanelOutlineControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Generate Outline"))
            {
                ((PanelOutlineController)target).GenerateOutline();
            }
        }
    }
}
#endif

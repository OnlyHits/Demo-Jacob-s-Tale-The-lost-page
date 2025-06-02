using System.IO;
using DG.DOTweenEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Comic
{
    [CustomEditor(typeof(Panel))]
    public class PanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Panel panel = (Panel)target;

            RefreshPanelBuilder(panel);
        }

        private void RefreshPanelBuilder(Panel panel)
        {
            if (GUILayout.Button("Refresh 3D Builder"))
            {
                var sorting_group = panel.GetComponent<SortingGroup>();
                sorting_group.sortingLayerName = "Panel";
                sorting_group.sortAtRoot = true;
                sorting_group.sortingOrder = 0;

                panel.Editor_Build();

                EditorUtility.SetDirty(panel);
            }
        }
    }
}

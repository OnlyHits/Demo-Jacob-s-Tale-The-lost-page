#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Comic
{
    [CustomEditor(typeof(Page))]
    public class PageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Page page = (Page)target;

            SpawnPanel(page);
            RefreshPanelList(page);
            AddConfiguration(page);
        }

        private void SpawnPanel(Page page)
        {
            if (GUILayout.Button("Spawn panel"))
            {
                page.InstantiatePanel();
                EditorUtility.SetDirty(page);
            }
        }

        private void RefreshPanelList(Page page)
        {
            if (GUILayout.Button("Refresh panel list"))
            {
                page.RefreshList();
                EditorUtility.SetDirty(page);
            }

        }

        private void AddConfiguration(Page page)
        {
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Page Configuration"))
            {
                page.AddConfiguration();
                EditorUtility.SetDirty(page);
            }
        }
    }
}
#endif

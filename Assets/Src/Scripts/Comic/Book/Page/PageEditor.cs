#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;

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
            AddConfiguration(page);

            RefreshPage(page);
        }

        private void RefreshPage(Page page)
        {
            if (GUILayout.Button("Refresh Page"))
            {
                page.RefreshList();

                foreach (var panel in page.GetNavigables())
                {
                    panel.Editor_Build();
                }
                EditorUtility.SetDirty(page);
            }
        }

        private void SpawnPanel(Page page)
        {
            if (GUILayout.Button("Spawn panel"))
            {
                page.InstantiatePanel();
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

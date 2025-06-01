using DG.DOTweenEditor;
using UnityEditor;
using UnityEngine;

namespace Comic
{
    [CustomEditor(typeof(Panel))]
    public class PanelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Panel panel = (Panel)target;

            RefreshBackground(panel);
            RefreshPanelBuilder(panel);
        }

        private void RefreshPanelBuilder(Panel panel)
        {
            if (GUILayout.Button("Refresh 3D Builder"))
            {
                panel.GetPanel3DBuilder().Build(panel.GetPanelVisual().PanelReference().bounds);
                EditorUtility.SetDirty(panel);
            }
        }

        private void RefreshBackground(Panel panel)
        {
            if (GUILayout.Button("Refresh Background"))
            {
                if (panel.GetBackgroundEditor() == null)
                {
                    Debug.LogWarning($"Decor parent transform not set on [{panel.gameObject.name}]");
                    return;
                }

                var missing = new System.Collections.Generic.List<string>();

                if (panel.GetBackgroundEditor().m_wall == null) missing.Add("wall");
                if (panel.GetBackgroundEditor().m_floor == null) missing.Add("floor");
                if (panel.GetBackgroundEditor().m_ceiling == null) missing.Add("ceiling");

                if (missing.Count > 0)
                {
                    Debug.LogWarning($"Missing decor children ({string.Join(", ", missing)}) on [{panel.gameObject.name}]");
                    return;
                }

                panel.GetBackgroundEditor().UpdateElements();
                EditorUtility.SetDirty(panel);
            }
        }
    }
}

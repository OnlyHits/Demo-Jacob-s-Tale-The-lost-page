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

            Panel decor = (Panel)target;

            if (GUILayout.Button("Refresh Background"))
            {
                if (decor.GetBackgroundEditor() == null)
                {
                    Debug.LogWarning($"Decor parent transform not set on [{decor.gameObject.name}]");
                    return;
                }

                var missing = new System.Collections.Generic.List<string>();

                if (decor.GetBackgroundEditor().m_wall == null) missing.Add("wall");
                if (decor.GetBackgroundEditor().m_floor == null) missing.Add("floor");
                if (decor.GetBackgroundEditor().m_ceiling == null) missing.Add("ceiling");

                if (missing.Count > 0)
                {
                    Debug.LogWarning($"Missing decor children ({string.Join(", ", missing)}) on [{decor.gameObject.name}]");
                    return;
                }

                decor.GetBackgroundEditor().UpdateElements();
                EditorUtility.SetDirty(decor);
            }
        }
    }
}

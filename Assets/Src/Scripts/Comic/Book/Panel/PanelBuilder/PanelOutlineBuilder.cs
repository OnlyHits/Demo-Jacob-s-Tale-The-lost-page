using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Comic
{
    public class PanelOutlineBuilder : BaseBehaviour
    {
        public enum PanelOutlinePartType
        {
            Panel_Outline_Left,
            Panel_Outline_Right,
            Panel_Outline_Top,
            Panel_Outline_Bottom,
            Panel_Outline_None,
        }

        [SerializeField, ReadOnly] private List<PanelOutlinePart> m_currentOutlines;
        [SerializeField] private Transform m_outlineContainer;

        private readonly float      m_delta = .6f;
        private float               m_timer = 0f;

        [Header("Prefabs (Top/Bot - Left/Right)")]
        [SerializeField] private GameObject m_leftRightPrefab;
        [SerializeField] private GameObject m_topBotPrefab;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            if (m_timer > m_delta)
            {
                m_timer = 0f;
                foreach (var outline in m_currentOutlines)
                    outline.IncrementOutlineSprite();
            }
            else
            {
                m_timer += Time.deltaTime;
            }
        }
        public override void LateInit(params object[] parameters)
        {
        }
        public override void Init(params object[] parameters)
        {
            foreach (var outline in m_currentOutlines)
            {
                outline.Init();
            }
        }
        #endregion

        public void Focus()
        {
            foreach (var outline in m_currentOutlines)
                outline.Focus();
        }
        public void Unfocus()
        {
            foreach (var outline in m_currentOutlines)
                outline.Unfocus();
        }

        #region Editor
#if UNITY_EDITOR
        private bool Editor_CheckRootValidity()
        {
            if (m_outlineContainer.childCount != 4)
                return false;

            HashSet<PanelOutlinePartType> found_types = new();

            for (int i = m_outlineContainer.childCount - 1; i >= 0; i--)
            {

                if (!m_outlineContainer.GetChild(i).TryGetComponent<PanelOutlinePart>(out var part))
                    return false;

                var type = part.Type;

                if (type ==  PanelOutlinePartType.Panel_Outline_None || found_types.Contains(type))
                    return false;

                found_types.Add(type);
            }

            return found_types.Count == 4;
        }

        private void Editor_ClearRoot()
        {
            for (int i = m_outlineContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = m_outlineContainer.GetChild(i);
                GameObject.DestroyImmediate(child.gameObject);
            }
        }
        private void Editor_InstantiateParts()
        {
            foreach (PanelOutlinePartType part in System.Enum.GetValues(typeof(PanelOutlinePartType)))
            {
                if (part == PanelOutlinePartType.Panel_Outline_None)
                    continue;

                bool horizontal = (part == PanelOutlinePartType.Panel_Outline_Top) || (part == PanelOutlinePartType.Panel_Outline_Bottom);

                GameObject partObj = (GameObject)PrefabUtility.InstantiatePrefab(horizontal ? m_topBotPrefab : m_leftRightPrefab, m_outlineContainer);

                PanelOutlinePart builderPart = partObj.GetComponent<PanelOutlinePart>();

                builderPart.Type = part;
            }
        }

        public void Editor_Build(Bounds bounds)
        {
            if (m_outlineContainer == null || !AllPrefabsValid())
                return;

            transform.localPosition = Vector3.zero;
            m_outlineContainer.localPosition = Vector3.zero;

            bool is_valid = Editor_CheckRootValidity();

            if (!is_valid)
            {
                Editor_ClearRoot();
                Editor_InstantiateParts();
            }

            m_currentOutlines ??= new();

            m_currentOutlines.Clear();

            foreach (Transform child in m_outlineContainer)
            {
                if (child.TryGetComponent<PanelOutlinePart>(out var part))
                    m_currentOutlines.Add(part);
            }

            foreach (var part in m_currentOutlines)
            {
                part.Init();
                part.Editor_Build(bounds, transform.lossyScale);
            }

            EditorUtility.SetDirty(this);
        }
#endif
        #endregion Editor

        private bool AllPrefabsValid()
        {
            return m_topBotPrefab && m_leftRightPrefab;
        }
    }
}
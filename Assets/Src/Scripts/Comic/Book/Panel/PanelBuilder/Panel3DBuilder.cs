using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using static Comic.Comic;
using System;
using UnityEditor.Build.Reporting;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Comic
{

#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class Panel3DBuilder : BaseBehaviour
    {
        public enum PanelPart3D
        {
            Panel_LeftWall,
            Panel_RightWall,
            Panel_FrontWall,
            Panel_BackWall,
            Panel_Ceil,
            Panel_Ground,
            Panel_None
        }

        [Tooltip("Use this root if you want to transform 3D panel. His position is dynamically modified to be at the center of the 3D panel")]
        [SerializeField] private Transform m_rootTransform;
        [SerializeField] private float m_defaultDepth = 2f;

        // use at runtime, in edit mode it wont be fullfiled
        [NonSerialized] private Dictionary<PanelPart3D, Panel3DPart> m_parts;

#if UNITY_EDITOR
        [SerializeField, ReadOnly] private List<Panel3DPart> m_editorParts;
#endif

        // tmp
        [SerializeField] private Material m_material;
        [SerializeField] private Sprite m_sprite;

        private float m_oldDepth;
        private Bounds m_oldBounds;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            m_parts = new Dictionary<PanelPart3D, Panel3DPart>();

            for (int i = m_rootTransform.childCount - 1; i >= 0; i--)
            {
                var part = m_rootTransform.GetChild(i).GetComponent<Panel3DPart>();

                if (part == null || part.Type == PanelPart3D.Panel_None)
                    continue;

                if (!m_parts.ContainsKey(part.Type))
                {
                    m_parts.Add(part.Type, part);
                }
            }

            foreach (var part in m_parts.Values)
            {
                part.Init(m_material);
            }

            //m_oldDepth = Editor_GetDepth();
            //m_oldBounds = m_parts[PanelPart3D.Panel_FrontWall].GetBounds();
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        private bool Editor_CheckRootValidity()
        {
            if (m_rootTransform.childCount != 6)
                return false;

            HashSet<PanelPart3D> found_types = new();

            for (int i = m_rootTransform.childCount - 1; i >= 0; i--)
            {

                if (!m_rootTransform.GetChild(i).TryGetComponent<Panel3DPart>(out var part))
                    return false;

                var type = part.Type;

                if (type == PanelPart3D.Panel_None || found_types.Contains(type))
                    return false;

                found_types.Add(type);
            }

            return found_types.Count == 6;
        }

        private void Editor_ClearRoot()
        {
            for (int i = m_rootTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = m_rootTransform.GetChild(i);
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        private void Editor_InstantiateParts()
        {
            foreach (PanelPart3D part in System.Enum.GetValues(typeof(PanelPart3D)))
            {
                if (part == PanelPart3D.Panel_None)
                    continue;

                GameObject partObj = new(part.ToString());
                partObj.transform.SetParent(m_rootTransform, false);

                Panel3DPart builderPart = partObj.AddComponent<Panel3DPart>();

                builderPart.Type = part;
            }
        }

        private Panel3DPart Editor_GetPanel3DPart(PanelPart3D type)
        {
            foreach (var part in m_editorParts)
            {
                Debug.Log(part.Type);

                if (part.Type == type)
                    return part;
            }
            return null;
        }

        private void Editor_SetDefaultValues()
        {
            var back = Editor_GetPanel3DPart(PanelPart3D.Panel_BackWall);
            var front = Editor_GetPanel3DPart(PanelPart3D.Panel_FrontWall);

            front.transform.localPosition = Vector3.zero;

            var pos = back.transform.localPosition;
            pos.z = m_defaultDepth;
            back.transform.localPosition = pos;
        }

        // do not manually call
        public void Editor_Build(Bounds bounds, PanelVisualData datas)
        {
            if (m_rootTransform == null)
                return;

            transform.localPosition = Vector3.zero;

            bool is_valid = Editor_CheckRootValidity();

            if (!is_valid)
            {
                Editor_ClearRoot();
                Editor_InstantiateParts();
            }

            m_editorParts ??= new();

            m_editorParts.Clear();

            foreach (Transform child in m_rootTransform)
            {
                if (child.TryGetComponent<Panel3DPart>(out var part))
                    m_editorParts.Add(part);
            }

            if (!is_valid)
            {
                Editor_SetDefaultValues();
            }

            RefreshBuild(bounds, transform.lossyScale, datas);
        }

        private void RefreshBuild(Bounds bounds, Vector3 lossy_scale, PanelVisualData datas)
        {
            foreach (var part in m_editorParts)
                part.Init(m_sprite, m_material);

            var depth = Editor_GetDepth();

            Editor_GetPanel3DPart(PanelPart3D.Panel_FrontWall).Editor_Build(bounds, depth, datas.WallSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_BackWall).Editor_Build(bounds, depth, datas.WallSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_LeftWall).Editor_Build(bounds, depth, datas.WallSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_RightWall).Editor_Build(bounds, depth, datas.WallSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_Ceil).Editor_Build(bounds, depth, datas.CeilSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_Ground).Editor_Build(bounds, depth, datas.GroundSprite, lossy_scale);

            var pos = m_rootTransform.localPosition;
            pos.z = depth * lossy_scale.z * .5f;

            m_rootTransform.localPosition = pos;
        }

        private float Editor_GetDepth()
        {
            var front = Editor_GetPanel3DPart(PanelPart3D.Panel_FrontWall);
            var back = Editor_GetPanel3DPart(PanelPart3D.Panel_BackWall);

            if (front == null || back == null)
            {
                Debug.LogWarning("Front or Back panel is null.");
                return 0f;
            }

            return Mathf.Abs(back.transform.position.z - front.transform.position.z);
        }
#endif
        #endregion Editor
    }
}
using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Comic
{
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

        [SerializeField] private Color nearColor = Color.white;
        [SerializeField] private Color farColor = Color.black;

        private Panel m_manager = null; // ref to panel

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
            if (parameters.Length < 1 || parameters[0] is not Panel)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }

            m_manager = (Panel)parameters[0];
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

            InitParts();
            SetupMaterials();

            //m_oldDepth = Editor_GetDepth();
            //m_oldBounds = m_parts[PanelPart3D.Panel_FrontWall].GetBounds();
        }
        #endregion

        #region Material
        private void InitParts()
        {
            m_parts[PanelPart3D.Panel_LeftWall].Init(m_manager.GetVisualData().DepthMaterial);
            m_parts[PanelPart3D.Panel_RightWall].Init(m_manager.GetVisualData().DepthMaterial);
            m_parts[PanelPart3D.Panel_Ceil].Init(m_manager.GetVisualData().DepthMaterial);
            m_parts[PanelPart3D.Panel_Ground].Init(m_manager.GetVisualData().DepthMaterial);
            m_parts[PanelPart3D.Panel_FrontWall].Init(m_manager.GetVisualData().DepthMaterial);
            m_parts[PanelPart3D.Panel_BackWall].Init(m_manager.GetVisualData().BackWallMaterial);
        }
        private void SetupMaterials()
        {
            float depth = GetDepth();

            SetupDepthMaterial(m_parts[PanelPart3D.Panel_LeftWall].GetSpriteRenderer(), depth);
            SetupDepthMaterial(m_parts[PanelPart3D.Panel_RightWall].GetSpriteRenderer(), depth);
            SetupDepthMaterial(m_parts[PanelPart3D.Panel_Ground].GetSpriteRenderer(), depth);
            SetupDepthMaterial(m_parts[PanelPart3D.Panel_Ceil].GetSpriteRenderer(), depth);
            SetupDepthMaterial(m_parts[PanelPart3D.Panel_FrontWall].GetSpriteRenderer(), depth);
            SetupBackWallMaterial(m_parts[PanelPart3D.Panel_BackWall].GetSpriteRenderer());
        }
        private void SetupDepthMaterial(SpriteRenderer spr, float depth)
        {
            var mpb = new MaterialPropertyBlock();

            spr.GetPropertyBlock(mpb);

            mpb.SetFloat("_GradientSize", depth);
            mpb.SetColor("_ColorA", nearColor);
            mpb.SetColor("_ColorB", farColor);

            spr.SetPropertyBlock(mpb);
        }
        private void SetupBackWallMaterial(SpriteRenderer spr)
        {
            var bounds = spr.sprite.bounds;
            var mpb = new MaterialPropertyBlock();

            spr.GetPropertyBlock(mpb);
            mpb.SetVector("_MinPosOS", bounds.min);
            mpb.SetVector("_MaxPosOS", bounds.max);
            spr.SetPropertyBlock(mpb);
        }
        #endregion Material

        #region Utils
        private float GetDepth()
        {
            var front = m_parts[PanelPart3D.Panel_FrontWall];
            var back = m_parts[PanelPart3D.Panel_BackWall];

            if (front == null || back == null)
            {
                Debug.LogWarning("Front or Back panel is null.");
                return 0f;
            }

            return Mathf.Abs(back.transform.position.z - front.transform.position.z);
        }
        #endregion Utils

        #region Editor
#if UNITY_EDITOR
        private void Editor_InitParts(PanelVisualData datas)
        {
            Editor_GetPanel3DPart(PanelPart3D.Panel_LeftWall).Init(datas.DepthMaterial);
            Editor_GetPanel3DPart(PanelPart3D.Panel_RightWall).Init(datas.DepthMaterial);
            Editor_GetPanel3DPart(PanelPart3D.Panel_Ceil).Init(datas.DepthMaterial);
            Editor_GetPanel3DPart(PanelPart3D.Panel_Ground).Init(datas.DepthMaterial);
            Editor_GetPanel3DPart(PanelPart3D.Panel_FrontWall).Init(datas.DepthMaterial);
            Editor_GetPanel3DPart(PanelPart3D.Panel_BackWall).Init(datas.BackWallMaterial);
        }
        private void Editor_SetupMaterials()
        {
            float depth = Editor_GetDepth();

            SetupDepthMaterial(Editor_GetPanel3DPart(PanelPart3D.Panel_LeftWall).GetSpriteRenderer(), depth);
            SetupDepthMaterial(Editor_GetPanel3DPart(PanelPart3D.Panel_RightWall).GetSpriteRenderer(), depth);
            SetupDepthMaterial(Editor_GetPanel3DPart(PanelPart3D.Panel_Ceil).GetSpriteRenderer(), depth);
            SetupDepthMaterial(Editor_GetPanel3DPart(PanelPart3D.Panel_Ground).GetSpriteRenderer(), depth);
            SetupDepthMaterial(Editor_GetPanel3DPart(PanelPart3D.Panel_FrontWall).GetSpriteRenderer(), depth);
            SetupBackWallMaterial(Editor_GetPanel3DPart(PanelPart3D.Panel_BackWall).GetSpriteRenderer());
        }
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

        public Panel3DPart Editor_GetPanel3DPart(PanelPart3D type)
        {
            foreach (var part in m_editorParts)
            {
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

            Editor_RefreshBuild(bounds, transform.lossyScale, datas);
            Editor_SetupMaterials();

            Editor_GetPanel3DPart(PanelPart3D.Panel_FrontWall).GetSpriteRenderer().enabled = false;

            EditorUtility.SetDirty(this);
        }

        private void Editor_RefreshBuild(Bounds bounds, Vector3 lossy_scale, PanelVisualData datas)
        {
            Editor_InitParts(datas);

            var depth = Editor_GetDepth();

            Editor_GetPanel3DPart(PanelPart3D.Panel_FrontWall).Editor_Build(bounds, depth, datas.WallSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_BackWall).Editor_Build(bounds, depth, datas.WallSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_LeftWall).Editor_Build(bounds, depth, datas.WallSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_RightWall).Editor_Build(bounds, depth, datas.WallSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_Ceil).Editor_Build(bounds, depth, datas.CeilSprite, lossy_scale);
            Editor_GetPanel3DPart(PanelPart3D.Panel_Ground).Editor_Build(bounds, depth, datas.GroundSprite, lossy_scale);

            var pos = m_rootTransform.localPosition;
            depth /= lossy_scale.z;
            pos.z = depth * .5f;

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
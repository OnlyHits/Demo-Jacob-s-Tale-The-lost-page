using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using static Comic.Comic;
using System;
using static Comic.PanelOutlineBuilder;
using UnityEditor;
using System.Security.Cryptography;
using TMPro;
using static Comic.Panel3DBuilder;

namespace Comic
{
    public class Panel2DBuilder : BaseBehaviour
    {
        [SerializeField] private SpriteRenderer             m_hideSprite;
        [SerializeField, ReadOnly] private SpriteRenderer   m_ground;
        [SerializeField, ReadOnly] private SpriteRenderer   m_ceil;
        [SerializeField] private Transform                  m_container;
        [SerializeField] private float                      m_ceilSize = 1f;
        [SerializeField] private float                      m_groundSize = 1f;

        public SpriteRenderer GetHideSprite() => m_hideSprite;
        public void LockPosition() => transform.localPosition = Vector3.zero;

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
            if (m_ceil == null || m_ground == null)
            {
                Debug.LogWarning("2D Builder : ceil or ground not instantiated");
                return;
            }
        }
        #endregion

        #region 2D Bounds management
        public Bounds GetGroundToCeilBounds(Bounds global_bounds)
        {
            Bounds floor_bounds = m_ground.bounds;
            Bounds ceil_bounds = m_ceil.bounds;

            float minX = global_bounds.min.x;
            float maxX = global_bounds.max.x;

            float minY = floor_bounds.max.y;
            float maxY = ceil_bounds.min.y;

            if (maxY < minY)
                maxY = minY;

            Vector3 center = new(
                (minX + maxX) * .5f,
                (minY + maxY) * .5f,
                global_bounds.center.z);

            Vector3 size = new(
                maxX - minX,
                maxY - minY,
                global_bounds.size.z // maintain Z size
            );

            return new Bounds(center, size);
        }

        public Bounds GetInnerPanelBounds(Bounds global_bounds)
        {
            Bounds floorBounds = m_ground.bounds;

            float minX = global_bounds.min.x;
            float maxX = global_bounds.max.x;

            float maxY = global_bounds.max.y;
            float minY = floorBounds.max.y;

            if (minY > maxY)
                minY = maxY;

            Vector3 center = new Vector3(
                (minX + maxX) / 2f,
                (minY + maxY) / 2f,
                0f
            );

            Vector3 size = new Vector3(
                maxX - minX,
                maxY - minY,
                0f
            );

            return new Bounds(center, size);
        }
        #endregion 2D Bounds management

        #region Editor
#if UNITY_EDITOR
        private bool Editor_CheckRootValidity()
        {
            if (m_container.childCount != 2 || m_ceil == null || m_ground == null)
                return false;

            for (int i = m_container.childCount - 1; i >= 0; i--)
            {
                if (!m_container.GetChild(i).TryGetComponent<SpriteRenderer>(out var part))
                    return false;
            }

            return true;
        }

        private void Editor_ClearRoot()
        {
            for (int i = m_container.childCount - 1; i >= 0; i--)
            {
                Transform child = m_container.GetChild(i);
                GameObject.DestroyImmediate(child.gameObject);
            }

            m_ceil = null;
            m_ground = null;
        }
        private void Editor_InstantiateParts(PanelVisualData datas)
        {
            GameObject ceil = new GameObject("2D-Ceil");
            ceil.transform.SetParent(m_container, false);
            var ceil_sr = ceil.AddComponent<SpriteRenderer>();

            ceil_sr.drawMode = SpriteDrawMode.Tiled;
            ceil_sr.tileMode = SpriteTileMode.Continuous;
            ceil_sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            ceil_sr.sprite = datas.CeilSprite;

            m_ceil = ceil_sr;

            GameObject ground = new GameObject("2D-Ground");
            ground.transform.SetParent(m_container, false);
            ground.AddComponent<HalfHeightCollider>();
            var ground_sr = ground.GetComponent<SpriteRenderer>();

            ground_sr.drawMode = SpriteDrawMode.Tiled;
            ground_sr.tileMode = SpriteTileMode.Continuous;
            ground_sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

            ground_sr.sprite = datas.GroundSprite;

            m_ground = ground_sr;
        }
        private void RefreshBuild(Bounds bounds, Vector3 lossy_scale)
        {
            Vector2 n_size = bounds.size / new Vector2(lossy_scale.x, lossy_scale.y);

            m_ceil.size = new Vector2(n_size.x, m_ceilSize);
            m_ceil.transform.localPosition = new Vector3(0f, n_size.y * .5f, 0f);

            m_ground.size = new Vector2(n_size.x, m_groundSize);
            m_ground.transform.localPosition = new Vector3(0f, -n_size.y * .5f, 0f);

//            m_ground.gameObject.GetComponent<HalfHeightCollider>().Setup();
        }

        public void Editor_Build(Bounds bounds, PanelVisualData datas)
        {
            if (m_container == null)
                return;

            transform.localPosition = Vector3.zero;
            m_container.localPosition = Vector3.zero;

            bool is_valid = Editor_CheckRootValidity();

            if (!is_valid)
            {
                Editor_ClearRoot();
                Editor_InstantiateParts(datas);
            }

            RefreshBuild(bounds, transform.lossyScale);
            EditorUtility.SetDirty(this);
        }
#endif
        #endregion Editor
    }
}
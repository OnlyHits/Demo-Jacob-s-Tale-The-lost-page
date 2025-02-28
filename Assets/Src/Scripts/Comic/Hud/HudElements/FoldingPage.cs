using CustomArchitecture;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace Comic
{
    using NUnit.Framework.Internal;
    using TMPro;
    using UnityEngine;
    using static PageHole;

#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class FoldingPage : BaseBehaviour
    {
        private RectTransform   m_rectTransform;
        private Mesh            m_mesh;
        private Vector3[]       m_originalVertex;
        private Vector3[]       m_modifiedVertex;
        [SerializeField] private bool       m_isInit = false;
        [SerializeField, Range(0, 1)] private float m_foldAmount;
        [SerializeField] private float m_bendStrength;
        [SerializeField] private float m_foldDepth;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            //#if UNITY_EDITOR
            if (!m_isInit)
            {
                Init();
                UpdateMesh();
                m_originalVertex = m_mesh.vertices;
                m_modifiedVertex = new Vector3[m_originalVertex.Length];
            }

            //            float foldAmount = Mathf.PingPong(Time.time, 1); // Oscillates between 0 and 1

            FoldVertex(m_foldAmount);
            //#endif
        }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            m_rectTransform = GetComponent<RectTransform>();
            m_mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = m_mesh;
            m_isInit = true;
        }
        #endregion

        private void FoldVertex(float foldAmount)
        {
            m_originalVertex.CopyTo(m_modifiedVertex, 0);

            float foldStrength = Mathf.Lerp(0, 1, foldAmount);

            for (int i = 0; i < m_modifiedVertex.Length; i++)
            {
                Vector3 v = m_originalVertex[i];

                if (v.x > 0)
                {
                    float foldFactor = Mathf.Sin(v.y * Mathf.PI) * foldStrength; // Curve effect
                    v.x -= foldFactor * m_bendStrength;  // Move inward
                    v.z -= foldFactor * m_foldDepth;  // Add slight depth
                }

                m_modifiedVertex[i] = v;
            }

            m_mesh.vertices = m_modifiedVertex;
            m_mesh.RecalculateNormals();
            m_mesh.RecalculateBounds();
        }

        void UpdateMesh()
        {
            Vector2 size = m_rectTransform.rect.size;

            float width = size.x;
            float height = size.y;

            Vector3[] vertices = new Vector3[]
            {
            new Vector3(-width / 2, -height / 2, 0),
            new Vector3(width / 2, -height / 2, 0),
            new Vector3(-width / 2, height / 2, 0),
            new Vector3(width / 2, height / 2, 0)
            };

            int[] triangles = new int[]
            {
            0, 2, 1,
            2, 3, 1
            };

            Vector2[] uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            m_mesh.Clear();
            m_mesh.vertices = vertices;
            m_mesh.triangles = triangles;
            m_mesh.uv = uv;
            m_mesh.RecalculateNormals();
        }
    }
}
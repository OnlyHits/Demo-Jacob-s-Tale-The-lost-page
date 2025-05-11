using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Comic
{
    [ExecuteAlways]
    public class PanelOutlineController : BaseBehaviour
    {
        [SerializeField] private SpriteRenderer m_targetPanel;

        public Transform m_outlineContainer;
        [ReadOnly, SerializeField] private List<PanelOutline> m_currentOutlines;
        private float m_delta = .3f;
        private float m_timer = 0f;

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
            if (m_targetPanel == null || !AllPrefabsValid())
            {
                Debug.LogWarning("Missing target panel or side prefabs.");
                return;
            }

            ClearOutlines();
            InstantiateOutlines();

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

        private void InstantiateOutlines()
        {
            Bounds bounds = m_targetPanel.bounds;
            Vector2 min = bounds.min;
            Vector2 max = bounds.max;
            Vector2 center = bounds.center;

            float width = max.x - min.x;
            float height = max.y - min.y;

            GameObject top = InstantiatePiece(m_topBotPrefab, new Vector2(center.x, max.y));
            SetTiledSize(top, new Vector2(m_targetPanel.transform.localScale.x, top.GetComponent<SpriteRenderer>().size.y), true);

            GameObject bottom = InstantiatePiece(m_topBotPrefab, new Vector2(center.x, min.y));
            SetTiledSize(bottom, new Vector2(m_targetPanel.transform.localScale.x, bottom.GetComponent<SpriteRenderer>().size.y), true);

            GameObject left = InstantiatePiece(m_leftRightPrefab, new Vector2(min.x, center.y));
            SetTiledSize(left, new Vector2(left.GetComponent<SpriteRenderer>().size.x, m_targetPanel.transform.localScale.y), false);

            GameObject right = InstantiatePiece(m_leftRightPrefab, new Vector2(max.x, center.y));
            SetTiledSize(right, new Vector2(right.GetComponent<SpriteRenderer>().size.x, m_targetPanel.transform.localScale.y), false);
        }

        private void ClearOutlines()
        {
            foreach (Transform child in m_outlineContainer)
            {
                Destroy(child.gameObject);
            }
        }

        [ContextMenu("Generate Outline")]
        public void GenerateOutline()
        {
            if (m_targetPanel == null || !AllPrefabsValid())
            {
                Debug.LogWarning("Missing target panel or side prefabs.");
                return;
            }

            foreach (Transform child in m_outlineContainer)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child.gameObject);
                else
                    Destroy(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }

            InstantiateOutlines();

            foreach (var outline in m_currentOutlines)
            {
                outline.Init();
            }
        }

        private GameObject InstantiatePiece(GameObject prefab, Vector2 pos)
        {
#if UNITY_EDITOR
            GameObject obj = Application.isPlaying
                ? Instantiate(prefab, m_outlineContainer)
                : (GameObject)PrefabUtility.InstantiatePrefab(prefab, m_outlineContainer);
#else
            GameObject obj = Instantiate(prefab, m_outlineContainer);
#endif
            obj.transform.position = pos;
            m_currentOutlines.Add(obj.GetComponent<PanelOutline>());
            return obj;
        }

        private void SetTiledSize(GameObject obj, Vector2 worldSize, bool horizontal)
        {
            var sr = obj.GetComponent<SpriteRenderer>();
            var sprite = sr.sprite;
            // is already setup in prefab
//            sr.drawMode = SpriteDrawMode.Tiled;

            Vector3 scale = obj.transform.lossyScale;
            sr.size = new Vector2(horizontal ? worldSize.x : 1f / sprite.GetPPUPerPixelSize().x, horizontal ? 1f / sprite.GetPPUPerPixelSize().y : worldSize.y);
        }

        private bool AllPrefabsValid()
        {
            return m_topBotPrefab && m_leftRightPrefab;
        }
    }
}
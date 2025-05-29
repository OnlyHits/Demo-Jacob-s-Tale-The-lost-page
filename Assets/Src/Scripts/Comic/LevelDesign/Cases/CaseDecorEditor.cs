//#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using static UnityEngine.Rendering.DebugUI;

namespace Comic
{
    public struct CaseDecorProvider
    {
        public CaseDecorProvider(CaseEditor.DecorType dt, Sprite sB = null, Sprite sF = null, Sprite sC = null)
        {
            decorType = dt;
            sBackground = sB;
            sFloor = sF;
            sCeiling = sC;
        }

        public CaseEditor.DecorType decorType;
        public Sprite sBackground;
        public Sprite sFloor;
        public Sprite sCeiling;
    }
    public class CaseDecorEditor : MonoBehaviour
    {
        public PanelVisual      m_visualReference;
        public Transform        m_decorTransform;

        [SerializeField, ReadOnly] private CaseEditor.DecorType m_decorType;

        [Space]
        public SpriteRenderer m_wall;
        public SpriteRenderer m_floor;
        public SpriteRenderer m_ceiling;



        public void Setup(CaseDecorProvider provider)
        {
            m_decorType = provider.decorType;

            m_wall.sprite = provider.sBackground;
            m_floor.sprite = provider.sFloor;
            m_ceiling.sprite = provider.sCeiling;

            switch (m_decorType)
            {
                case CaseEditor.DecorType.Room:
                    m_wall.gameObject.SetActive(true);
                    m_floor.gameObject.SetActive(true);
                    m_ceiling.gameObject.SetActive(true);
                    break;
                case CaseEditor.DecorType.Outside:
                    m_wall.gameObject.SetActive(true);
                    m_floor.gameObject.SetActive(true);
                    m_ceiling.gameObject.SetActive(false);
                    break;
            }
        }

        [Button("Force Refresh")]
        public void Refresh()
        {
            if (m_decorTransform == null)
            {
                Debug.LogWarning("Decor parent transform not set on [" + gameObject.name + "]");
                return;
            }

            if (m_wall == null || m_floor == null || m_ceiling == null)
            {
                Debug.LogWarning("Decor childrens (wall, floor, etc) not set on [" + gameObject.name + "]");
                return;
            }

            UpdateElements();
        }

        private void PositionAt(SpriteRenderer parent, SpriteRenderer child, Vector2 anchor)
        {
            Vector3 parentMin = parent.bounds.min;
            Vector3 parentSize = parent.bounds.size;

            Vector3 worldAnchor = parentMin + new Vector3(anchor.x * parentSize.x, anchor.y * parentSize.y, 1.0f);

            child.transform.position = worldAnchor;
        }

        private void UpdateFloor(Vector2 worldSize)
        {
            m_floor.size = new Vector2(worldSize.x, m_floor.size.y);
            PositionAt(m_visualReference.PanelReference(), m_floor, new Vector2(0.5f, 0.0f));
        }

        private void UpdateCeil(Vector2 worldSize)
        {
            m_ceiling.size = new Vector2(worldSize.x, m_ceiling.size.y);
            PositionAt(m_visualReference.PanelReference(), m_ceiling, new Vector2(0.5f, 1.0f));
        }

        private void UpdateWall(Vector2 worldSize)
        {
            m_wall.transform.localPosition = Vector3.zero;
            m_wall.size = worldSize;
            PositionAt(m_visualReference.PanelReference(), m_wall, new Vector2(0.5f, 0.5f));
        }

        private void UpdateElements()
        {
            m_decorTransform.position = m_visualReference.transform.position;

            var worldSize = (Vector2)m_visualReference.transform.localScale;

            UpdateWall(worldSize);
            UpdateCeil(worldSize);
            UpdateFloor(worldSize);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                m_floor.GetComponent<HalfHeightCollider>().Setup();
#endif
        }
    }
}
//#endif

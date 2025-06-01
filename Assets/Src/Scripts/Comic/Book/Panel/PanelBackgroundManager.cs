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
    public enum BackgroundType
    {
        Room,
        Outside
    }

    public struct PanelBackgroundProvider
    {
        public PanelBackgroundProvider(BackgroundType dt, Sprite sB = null, Sprite sF = null, Sprite sC = null)
        {
            decorType = dt;
            sBackground = sB;
            sFloor = sF;
            sCeiling = sC;
        }

        public BackgroundType decorType;
        public Sprite sBackground;
        public Sprite sFloor;
        public Sprite sCeiling;
    }
    public class PanelBackgroundManager : MonoBehaviour
    {
        public PanelVisual      m_visualReference;
        public Transform        m_decorTransform;

        [SerializeField, ReadOnly] private BackgroundType m_decorType;

        [Space]
        public SpriteRenderer m_wall;
        public SpriteRenderer m_floor;
        public SpriteRenderer m_ceiling;

        public void Setup(PanelBackgroundProvider provider)
        {
            m_decorType = provider.decorType;

            m_wall.sprite = provider.sBackground;
            m_floor.sprite = provider.sFloor;
            m_ceiling.sprite = provider.sCeiling;

            switch (m_decorType)
            {
                case BackgroundType.Room:
                    m_wall.gameObject.SetActive(true);
                    m_floor.gameObject.SetActive(true);
                    m_ceiling.gameObject.SetActive(true);
                    break;
                case BackgroundType.Outside:
                    m_wall.gameObject.SetActive(true);
                    m_floor.gameObject.SetActive(true);
                    m_ceiling.gameObject.SetActive(false);
                    break;
            }
        }

        private void PositionAt(SpriteRenderer child, Bounds bounds, Vector2 anchor)
        {
            Vector3 parentMin = bounds.min;
            Vector3 parentSize = bounds.size;

            Vector3 worldAnchor = parentMin + new Vector3(anchor.x * parentSize.x, anchor.y * parentSize.y, 0.0f);

            child.transform.position = worldAnchor;

            Vector3 local_pos = child.transform.localPosition;
            local_pos.z = 0f;
            child.transform.localPosition = local_pos;
        }

        private void UpdateFloor(Vector2 worldSize)
        {
            m_floor.size = new Vector2(worldSize.x, m_floor.size.y);
            PositionAt(m_floor, m_visualReference.PanelReference().bounds, new Vector2(0.5f, 0.0f));
        }

        private void UpdateCeil(Vector2 worldSize)
        {
            m_ceiling.size = new Vector2(worldSize.x, m_ceiling.size.y);
            PositionAt(m_ceiling, m_visualReference.PanelReference().bounds, new Vector2(0.5f, 1.0f));
        }

        private void UpdateWall(Vector2 worldSize)
        {
            m_wall.transform.localPosition = Vector3.zero;
            m_wall.size = worldSize;
            PositionAt(m_wall, m_visualReference.PanelReference().bounds, new Vector2(0.5f, 0.5f));
        }

        public void UpdateElements()
        {
            m_decorTransform.position = m_visualReference.transform.position;

            var worldSize = (Vector2)m_visualReference.transform.localScale;

            UpdateWall(worldSize);
            UpdateCeil(worldSize);
            UpdateFloor(worldSize);

            m_floor.GetComponent<HalfHeightCollider>().Setup();
        }
    }
}
//#endif

using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;
using static Comic.PanelOutlineBuilder;
using static Comic.Panel3DBuilder;

namespace Comic
{
    public class PanelOutlinePart : BaseBehaviour
    {
        [SerializeField] private List<Sprite> m_outlineSprite = null;

        [SerializeField, ReadOnly] private PanelOutlinePartType        m_type = PanelOutlinePartType.Panel_Outline_None;
        [SerializeField, ReadOnly] private SpriteRenderer              m_outlineSr = null;
        [SerializeField, ReadOnly] private int                         m_index = 0;

        public PanelOutlinePartType Type { get { return m_type; } set { m_type = value; } }

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
        }
        public override void Init(params object[] parameters)
        {
            m_index = 0;
            m_outlineSr = GetComponent<SpriteRenderer>();

            if (m_outlineSr != null && m_outlineSprite.Count > m_index)
                m_outlineSr.sprite = m_outlineSprite[m_index];
        }
        #endregion

        public void Focus()
        {
            m_outlineSr.color = Color.black;
        }

        public void Unfocus()
        {
            m_outlineSr.color = Color.black;
        }

        public void IncrementOutlineSprite()
        {
            m_index = m_index + 1 >= m_outlineSprite.Count ? 0 : m_index + 1;
            m_outlineSr.sprite = m_outlineSprite[m_index];
        }

        #region Editor
#if UNITY_EDITOR
        public void Editor_Build(Bounds front_bounds, Vector3 lossy_scale)
        {
            Vector2 n_size = front_bounds.size / new Vector2(lossy_scale.x, lossy_scale.y);

            var sprite = m_outlineSr.sprite;

            Vector3 pos = Vector3.zero;
            Vector2 size = Vector2.zero;

            switch (m_type)
            {
                case PanelOutlinePartType.Panel_Outline_Left:
                    pos = new Vector3(-n_size.x * .5f, 0f, 0f);
                    size = new Vector2(1f / sprite.GetPPUPerPixelSize().x, n_size.y);
                    break;
                case PanelOutlinePartType.Panel_Outline_Right:
                    pos = new Vector3(n_size.x * .5f, 0f, 0f);
                    size = new Vector2(1f / sprite.GetPPUPerPixelSize().x, n_size.y);
                    break;
                case PanelOutlinePartType.Panel_Outline_Top:
                    pos = new Vector3(0f, n_size.y * .5f, 0f);
                    size = new Vector2(n_size.x, 1f / sprite.GetPPUPerPixelSize().y);
                    break;
                case PanelOutlinePartType.Panel_Outline_Bottom:
                    pos = new Vector3(0f, -n_size.y * .5f, 0f);
                    size = new Vector2(n_size.x, 1f / sprite.GetPPUPerPixelSize().y);
                    break;
                default:
                    return;
            }

            transform.localPosition = pos;
            m_outlineSr.size = size;
        }
#endif
        #endregion Editor
    }
}

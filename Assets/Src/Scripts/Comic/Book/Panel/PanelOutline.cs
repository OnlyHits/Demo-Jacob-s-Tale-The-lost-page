using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Comic
{
    public class PanelOutline : BaseBehaviour
    {
        private SpriteRenderer m_outlineSr = null;
        [SerializeField] private List<Sprite> m_outlineSprite = null;
        private int m_index = 0;

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
            m_outlineSr = GetComponent<SpriteRenderer>();
            m_outlineSr.sprite = m_outlineSprite[m_index];
        }
        #endregion

        public void Focus()
        {
            m_outlineSr.color = Color.red;
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
    }
}
using System;
using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using static Comic.Comic;
using static PageHole;

namespace Comic
{
    public class CaseVisual : BaseBehaviour
    {
        [SerializeField, ReadOnly] private List<SpriteRenderer> m_sprites = new List<SpriteRenderer>();
        [SerializeField, ReadOnly] private SpriteMask m_caseMask;

        private void Awake()
        {
            var sprites = GetComponentsInChildren<SpriteRenderer>(true);
            m_sprites.AddRange(sprites);
            m_caseMask = GetComponentInChildren<SpriteMask>(true);
        }

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
        { }
        #endregion


        public void PushFront()
        {
            m_caseMask.frontSortingLayerID = frontLayerId;
            m_caseMask.backSortingLayerID = frontLayerId;
            foreach (SpriteRenderer sprite in m_sprites)
            {
                sprite.sortingLayerName = frontLayerName;
            }
        }

        public void PushBack()
        {
            m_caseMask.frontSortingLayerID = backLayerId;
            m_caseMask.backSortingLayerID = backLayerId;
            foreach (SpriteRenderer sprite in m_sprites)
            {
                sprite.sortingLayerName = backLayerName;
            }
        }

        public void ResetDefault()
        {
            m_caseMask.frontSortingLayerID = defaultLayerId;
            m_caseMask.backSortingLayerID = defaultLayerId;
            foreach (SpriteRenderer sprite in m_sprites)
            {
                sprite.sortingLayerName = defaultLayerName;
            }
        }

        public void AddOrderInLayer(int addValue)
        {
            foreach (SpriteRenderer sprite in m_sprites)
            {
                sprite.sortingOrder += addValue;
            }
        }

        public void SubOrderInLayer(int subValue)
        {
            foreach (SpriteRenderer sprite in m_sprites)
            {
                sprite.sortingOrder -= subValue;
            }
        }
    }
}

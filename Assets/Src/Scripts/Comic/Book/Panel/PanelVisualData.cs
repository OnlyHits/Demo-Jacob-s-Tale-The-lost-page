using UnityEngine;

namespace Comic
{
    public class PanelVisualData : MonoBehaviour
    {
        [SerializeField] private SpriteMask     m_mask;
        [SerializeField] private Sprite         m_groundSprite;
        [SerializeField] private Sprite         m_ceilSprite;
        [SerializeField] private Sprite         m_wallSprite;

        public SpriteMask Ref { get { return m_mask; } protected set { } }
        public Bounds GetGlobalBounds() { return m_mask.bounds; }
        public Sprite CeilSprite { get { return m_ceilSprite; } protected set { } }
        public Sprite GroundSprite { get { return m_groundSprite; } protected set { } }
        public Sprite WallSprite { get { return m_wallSprite; } protected set { } }
    }
}
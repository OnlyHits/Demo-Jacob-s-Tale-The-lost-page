using UnityEngine;
using CustomArchitecture;
using static Comic.Panel3DBuilder.PanelPart3D;
using UnityEditor;

namespace Comic
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Panel3DPart : BaseBehaviour
    {
        [SerializeField, ReadOnly] private Panel3DBuilder.PanelPart3D  m_type = Panel_None;
        [SerializeField, ReadOnly] private SpriteRenderer              m_sprite = null;
        [SerializeField, ReadOnly] private float                       m_localDepth; // not relativ to parent
        public Panel3DBuilder.PanelPart3D Type { get { return m_type; } set { m_type = value; } }
        public Bounds GetBounds() => m_sprite.bounds;
        public SpriteRenderer GetSpriteRenderer() => m_sprite;
        public float GetLocalDepth() => m_localDepth;

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
            m_sprite = GetComponent<SpriteRenderer>();

            m_sprite.drawMode = SpriteDrawMode.Tiled;
            m_sprite.tileMode = SpriteTileMode.Continuous;

            if (parameters.Length < 1 && parameters[0] is not Material)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }

            m_sprite.material = (Material)parameters[0];
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        public void Editor_Build(Bounds front_bounds, float depth, Sprite sprite, Vector3 lossy_scale)
        {
            if (m_sprite == null)
            {
                Debug.LogWarning("Sprite is null");
                return;
            }

            m_sprite.sprite = sprite;

            Vector2 n_size = front_bounds.size / new Vector2(lossy_scale.x, lossy_scale.y);

            depth /= lossy_scale.z;
            Quaternion rotation = Quaternion.identity;
            Vector3 pos = Vector3.zero;
            Vector2 size = Vector2.zero;

            switch (m_type)
            {
                case Panel3DBuilder.PanelPart3D.Panel_FrontWall:
                    pos = new Vector3(0f, 0f, -depth * .5f);
                    size = new Vector2(n_size.x, n_size.y);
                    m_sprite.color = new Color(0, 0, 0, 0);
                    break;
                case Panel3DBuilder.PanelPart3D.Panel_BackWall:
                    pos = new Vector3(0f, 0f, depth * .5f);
                    size = new Vector2(n_size.x, n_size.y);
                    break;
                case Panel3DBuilder.PanelPart3D.Panel_LeftWall:
                    pos = new Vector3(-n_size.x * .5f, 0f, 0f);
                    size = new Vector2(depth, n_size.y);
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    m_sprite.flipX = true;
                    break;
                case Panel3DBuilder.PanelPart3D.Panel_RightWall:
                    pos = new Vector3(n_size.x * .5f, 0f, 0f);
                    size = new Vector2(depth, n_size.y);
                    rotation = Quaternion.Euler(0f, -90f, 0f);
                    m_sprite.flipX = true;
                    break;
                case Panel3DBuilder.PanelPart3D.Panel_Ceil:
                    pos = new Vector3(0f, n_size.y * .5f, 0f);
                    size = new Vector2(n_size.x, depth);
                    rotation = Quaternion.Euler(-90f, 0f, 0f);
                    break;
                case Panel3DBuilder.PanelPart3D.Panel_Ground:
                    pos = new Vector3(0f, -n_size.y * .5f, 0f);
                    size = new Vector2(n_size.x, depth);
                    rotation = Quaternion.Euler(90f, 0f, 0f);
                    break;
                default:
                    return;
            }

            transform.localPosition = pos;
            transform.rotation = rotation;
            m_sprite.size = size;
            m_sprite.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            m_localDepth = depth * .5f + pos.z;

            EditorUtility.SetDirty(this);
        }
#endif
        #endregion Editor
    }
}
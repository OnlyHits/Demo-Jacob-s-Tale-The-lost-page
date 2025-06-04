using CustomArchitecture;
using UnityEngine;
using static Comic.Panel3DBuilder;

namespace Comic
{
    public enum PropsType
    {
        Props_Lamp,
        Props_Painting,
        Props_Candle,
        Props_NothingSpecial
    }
    public abstract class AProps : BaseBehaviour
    {
        [SerializeField] protected SpriteRenderer m_spriteRenderer;
        [SerializeField] protected Transform m_root;
        [SerializeField] protected PropsType m_type;

        public PropsType GetPropsType() => m_type;

        public override void Pause(bool pause = true)
        {
            base.Pause(pause);

            PauseBehaviour(pause);
        }

        public override void Init(params object[] parameters)
        {
            if (m_spriteRenderer != null)
                m_spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        public abstract void StartBehaviour();
        public abstract void StopBehaviour();
        public abstract void PauseBehaviour(bool pause);
#if UNITY_EDITOR
        public abstract void Editor_Build(Panel3DBuilder builder);
#endif
    }
}

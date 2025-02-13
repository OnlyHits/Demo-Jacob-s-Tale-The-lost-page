using CustomArchitecture;
using UnityEngine;
using DG.Tweening;
using System;

namespace Comic
{
    public enum PropsType
    {
        Props_Lamp,
        Props_Painting
    }

    public abstract class AProps : BaseBehaviour
    {
        [SerializeField] protected PropsType m_type;
        public PropsType GetPropsType() => m_type;

        public override void Pause(bool pause = true)
        {
            base.Pause(pause);

            PauseBehaviour(pause);
        }

        public abstract void StartBehaviour();
        public abstract void StopBehaviour();
        public abstract void PauseBehaviour(bool pause);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using CustomArchitecture;
using UnityEngine.Rendering;
using static PageHole;

namespace Comic
{
    public class GameCameraRegister : ACameraRegister
    {
        public SpriteRenderer m_screenshotSpr;

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
    }
}

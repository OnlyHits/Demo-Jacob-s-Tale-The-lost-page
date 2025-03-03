using CustomArchitecture;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using static PageHole;

namespace Comic
{
    public class BookCoverView : AView
    {
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

        public override void ActiveGraphic(bool active)
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>(true);

            foreach (Image image in images)
            {
                image.enabled = active;
            }
        }

        public override void Pause(bool pause)
        {
            base.Pause();
        }
    }
}

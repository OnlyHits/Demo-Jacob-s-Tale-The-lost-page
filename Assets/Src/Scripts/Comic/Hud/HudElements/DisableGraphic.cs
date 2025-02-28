//using CustomArchitecture;
//using System.Collections.Generic;
//using System.Collections;
//using UnityEngine;
//using System;
//using Sirenix.OdinInspector;
//using UnityEngine.UI;
//using TMPro;
//using static PageHole;

//namespace Comic
//{
//    public class DisableGraphic : BaseBehaviour
//    {
//        #region BaseBehaviour
//        protected override void OnFixedUpdate()
//        { }
//        protected override void OnLateUpdate()
//        { }
//        protected override void OnUpdate()
//        { }
//        public override void LateInit(params object[] parameters)
//        { }
//        public override void Init(params object[] parameters)
//        { }
//        #endregion

//        public void Unactiv(bool active, Page _1, Page _2)
//        {
//            if (!active)
//                ActiveGraphic(true);
//        }

//        public void Middle(bool active, Page _1, Page _2)
//        {
//            if (active)
//                ActiveGraphic(false);
//            else
//                ActiveGraphic(true);
//        }

//        public void After(bool active, Page _1, Page _2)
//        {
//            ActiveGraphic(false);
//        }

//        public void ActiveGraphic(bool active)
//        {
//            Image[] images = gameObject.GetComponentsInChildren<Image>(true);
//            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>(true);

//            foreach (Image image in images)
//            {
//                image.enabled = active;
//            }

//            foreach (TMP_Text text in texts)
//            {
//                text.enabled = active;
//            }
//        }
//    }
//}

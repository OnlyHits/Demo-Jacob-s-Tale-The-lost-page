using TMPro;
using UnityEngine.UI;

namespace Comic
{
    public class ProductionView : AView
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
            TMP_Text[] images = gameObject.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text image in images)
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

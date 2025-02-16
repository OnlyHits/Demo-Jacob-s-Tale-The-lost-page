using CustomArchitecture;

namespace Comic
{
    public abstract class AView : BaseBehaviour
    {
        public virtual void Show(bool instant = false) => gameObject.SetActive(true);
        public virtual void Hide(bool partialy = false, bool instant = false) => gameObject.SetActive(false);

        public virtual void ShowPartial()
        {
            Show(instant: true);
            Hide(partialy: true, instant: true);
        }

        public abstract void Init();
        public abstract void ActiveGraphic(bool active);
    }
}
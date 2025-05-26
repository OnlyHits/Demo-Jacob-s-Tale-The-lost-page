using CustomArchitecture;
using UnityEngine;

namespace Comic
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FootStepVfx : VFXSingleClip
    {
        private readonly Vector2 m_scaleRange = new Vector2(.2f, 1.5f);

        #region Pool element
        public override void OnAllocate(params object[] parameter)
        {
            base.OnAllocate(parameter);

            if (parameter.Length < 1 || parameter[0] is not Vector2)
            {
                Debug.Log("Wrong parameter");
                return;
            }

            if (parameter.Length < 2 || parameter[1] is not bool)
            {
                Debug.Log("Wrong parameter");
                return;
            }

            if (parameter.Length < 3 || parameter[2] is not float)
            {
                Debug.Log("Wrong parameter");
                return;
            }

            if (parameter.Length < 4 || parameter[3] is not bool)
            {
                Debug.Log("Wrong parameter");
                return;
            }

            float scale = 1f * (float)parameter[2]; // ;)
            Vector3 pos = new Vector3(((Vector2)parameter[0]).x, ((Vector2)parameter[0]).y, Random.Range(0, 10));

            transform.localScale = (bool)parameter[3] ? Vector3.one : new Vector3(scale, scale, 1f);
            transform.position = pos;
            GetComponent<SpriteRenderer>().flipX = (bool)parameter[1];
        }
        #endregion Pool element
    }
}
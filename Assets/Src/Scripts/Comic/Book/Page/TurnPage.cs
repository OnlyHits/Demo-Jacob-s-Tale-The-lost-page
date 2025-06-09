using System.Data.Common;
using CustomArchitecture;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UI;

namespace Comic
{
    //[RequireComponent(typeof(SubdividedPlane))]
    public class TurnPage : APoolElement
    {
        [SerializeField] private SubdividedPlane    m_plane;
        private Vector2                             m_size = Vector2.zero;

        //public bool IsFirstHalf() => m_isFirstHalf;

        #region BaseBehaviour
        public override void Init(params object[] parameters)
        {
            base.Init(parameters);
            if (parameters.Length < 1 || parameters[0] is not Bounds)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }

            m_size = ((Bounds)parameters[0]).size;
            m_plane.Init(parameters);
        }
        #endregion BaseBehaviour

        #region APoolElement override
        public override void OnAllocate(params object[] parameter)
        {
            if (parameter.Length < 1 || parameter[0] is not Texture2D)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }
            if (parameter.Length < 2 || parameter[1] is not Texture2D)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }

            var mpb = new MaterialPropertyBlock();

            m_plane.GetComponent<MeshRenderer>().GetPropertyBlock(mpb);

            mpb.SetTexture("_FrontTex", (Texture2D)parameter[0]);
            mpb.SetTexture("_BackTex", (Texture2D)parameter[1]);

            m_plane.GetComponent<MeshRenderer>().SetPropertyBlock(mpb);

            Compute = true;
        }

        public override void OnDeallocate()
        { }
        #endregion

        #region Turn page error sequence
        private Sequence GetRotationSequenceError(bool is_next, float error_angle, float error_ratio, Ease in_out_ease, float duration)
        {
            SetPlanePosition(is_next);

            float error_rotate_duration = (duration * .5f) * error_ratio;

            float from_rotation = is_next ? 90f : 270f;
            float to_rotation = is_next ? 270f : 90f;

            //RectTransform rect = m_pageImage.GetComponent<RectTransform>();
            //rect.eulerAngles = Vector3.zero;

            Sequence rotate_sequence = DOTween.Sequence();

            rotate_sequence.Append(m_plane.transform.DOLocalRotateQuaternion(Quaternion.Euler(0, from_rotation, 0), duration * 0.5f)
                .SetEase(in_out_ease));
                //.OnComplete(() =>
                //{
                //    //m_isFirstHalf = false;
                //    //Vector3 rotation = rect.eulerAngles;
                //    //rotation.y = to_rotation;
                //    //rect.eulerAngles = rotation;

                //    //SetupPivot(!is_next);

                //    //SetupSprite(is_next ? m_backSprite : m_frontSprite);
                //    //m_manager.RefreshRenderingSortOrder();
                //}));

            //rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, to_rotation + (error_angle * (is_next ? 1f : -1f)), 0), error_rotate_duration)
            //    .SetEase(out_ease));
            //rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, to_rotation, 0), error_rotate_duration)
            //    .SetEase(in_ease)
            //    .OnComplete(() =>
            //    {
            //        m_isFirstHalf = true;
            //        Vector3 rotation = rect.eulerAngles;
            //        rotation.y = from_rotation;
            //        rect.eulerAngles = rotation;

            //        SetupPivot(is_next);

            //        SetupSprite(is_next ? m_frontSprite : m_backSprite);
            //        m_manager.RefreshRenderingSortOrder();
            //    }));

            //rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 0, 0), duration * 0.5f)
            //    .SetEase(out_ease));

            //rotate_sequence.OnComplete(() => Compute = false);

            return rotate_sequence;
        }

        public Sequence PlayPageErrorSequence(bool is_next, float error_angle, Ease in_out_ease, float duration)
        {
            //OnStartAnimation(is_next);

            float max_error_angle = 90f;

            float error_ratio = error_angle / max_error_angle;

            Sequence rotate_sequence = GetRotationSequenceError(is_next, error_angle, error_ratio, in_out_ease, duration);

            return rotate_sequence;
        }

        #endregion

        #region Turn page sequence
        public Sequence PlayRotationSequence(bool is_next, Ease in_out_ease, float duration)
        {
            SetPlanePosition(is_next);
            SetFold(is_next);

            Vector3 base_rot = Vector3.zero;
            Vector3 to_rot = Vector3.zero;

            base_rot.y = is_next ? 0f : 0f;
            to_rot.y = is_next ? 180f : -180f;

            Sequence rotate_sequence = DOTween.Sequence();

            transform.localRotation = Quaternion.Euler(base_rot);

            rotate_sequence.Append(transform.DOLocalRotate(to_rot, duration, RotateMode.FastBeyond360)
                .SetEase(in_out_ease));

            float fold_progress = 0f;
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            MeshRenderer renderer = m_plane.GetComponent<MeshRenderer>(); 
            
            rotate_sequence.Join(DOTween.To(() => fold_progress, x => {
                fold_progress = x;
                renderer.GetPropertyBlock(mpb);
                mpb.SetFloat("_FoldProgress", fold_progress);
                renderer.SetPropertyBlock(mpb);
            }, 1f, duration).SetEase(in_out_ease));

            rotate_sequence.OnComplete(() => Compute = false);

            return rotate_sequence;
        }
        #endregion

        #region Class utility
        private void SetFold(bool is_next)
        {
            var mpb = new MaterialPropertyBlock();

            m_plane.GetComponent<MeshRenderer>().GetPropertyBlock(mpb);

            mpb.SetFloat("_FoldProgress", 0f);
            mpb.SetFloat("_FoldRightSide", is_next ? 0f : 1f);

            m_plane.GetComponent<MeshRenderer>().SetPropertyBlock(mpb);
        }

        private void SetPlanePosition(bool is_next)
        {
            var pos = Vector3.zero;
            pos.x = is_next ? m_size.x * .5f : -m_size.x * .5f;
            m_plane.transform.localPosition = pos;
        }
        #endregion
    }
}
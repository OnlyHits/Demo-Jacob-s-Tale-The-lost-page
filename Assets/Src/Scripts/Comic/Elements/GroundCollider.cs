using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Comic
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class HalfHeightCollider : MonoBehaviour
    {
        public void Setup()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            BoxCollider2D collider = GetComponent<BoxCollider2D>();

            if (sr == null || collider == null)
                return;

            Bounds bounds = sr.bounds;
            float halfHeight = bounds.size.y / transform.lossyScale.y / 2f;

            collider.size = new Vector2(bounds.size.x / transform.lossyScale.x, halfHeight);
            collider.offset = new Vector2(0f, halfHeight / 2f);

            //EditorUtility.SetDirty(collider);
        }
    }
}

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Comic
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class SpriteRendererCollider2D : MonoBehaviour
    {
        public void Refresh()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            BoxCollider2D collider = GetComponent<BoxCollider2D>();

            if (sr == null || collider == null)
                return;

            Bounds bounds = sr.bounds;
            float width = bounds.size.x / transform.lossyScale.x;
            float height = bounds.size.y / transform.lossyScale.y;

            collider.size = new Vector2(width, height);
        }
    }
}

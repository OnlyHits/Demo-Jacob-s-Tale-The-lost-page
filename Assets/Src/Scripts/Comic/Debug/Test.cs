using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class Test : MonoBehaviour
{
    public BoxCollider2D boxCollider;      // Assign in Inspector
    public List<SpriteRenderer> sprites;   // List of all SpriteRenderers

    void Update()
    {
        if (boxCollider == null || sprites == null || sprites.Count == 0) return;

        Vector2 colliderPosition = boxCollider.transform.position;
        Bounds colliderBounds = boxCollider.bounds;

        SpriteRenderer closestSprite = sprites
            .OrderBy(sprite => DistanceToBounds(sprite.bounds, colliderBounds))
            .FirstOrDefault();

        if (closestSprite == null) return;

        Bounds spriteBounds = closestSprite.bounds;

        // Calculate new position, clamping inside the closest sprite's bounds
        Vector2 newPosition = colliderPosition;
        newPosition.x = Mathf.Clamp(newPosition.x, spriteBounds.min.x + colliderBounds.extents.x, spriteBounds.max.x - colliderBounds.extents.x);
        newPosition.y = Mathf.Clamp(newPosition.y, spriteBounds.min.y + colliderBounds.extents.y, spriteBounds.max.y - colliderBounds.extents.y);

        // Apply new position
        boxCollider.transform.position = newPosition;
    }

    private float DistanceToBounds(Bounds spriteBounds, Bounds colliderBounds)
    {
        // Calculate the closest point on the sprite bounds to the collider bounds
        float dx = Mathf.Max(0, spriteBounds.min.x - colliderBounds.max.x, colliderBounds.min.x - spriteBounds.max.x);
        float dy = Mathf.Max(0, spriteBounds.min.y - colliderBounds.max.y, colliderBounds.min.y - spriteBounds.max.y);

        return dx * dx + dy * dy; // Use squared distance for efficiency
    }
}

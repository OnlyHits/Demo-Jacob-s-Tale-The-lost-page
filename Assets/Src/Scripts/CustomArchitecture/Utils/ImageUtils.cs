using UnityEngine;
using UnityEngine.UI;

namespace CustomArchitecture
{
    public static class ImageUtils
    {
        // TODO : make it compatible with canvas overlay mode
        public static void MatchSpriteBounds(Image image, Canvas canvas, Camera baseCamera, Bounds copyBounds)
        {
            if (image == null || canvas == null)
            {
                Debug.LogError("MatchSpriteBounds: Image or Canvas is null!");
                return;
            }

            if (baseCamera == null)
                baseCamera = Camera.main;

            Vector3 minScreen = baseCamera.WorldToScreenPoint(copyBounds.min);
            Vector3 maxScreen = baseCamera.WorldToScreenPoint(copyBounds.max);

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            RectTransform rectTransform = image.GetComponent<RectTransform>();

            if (canvasRect == null || rectTransform == null)
            {
                Debug.LogError("MatchSpriteBounds: Missing RectTransform on canvas or image.");
                return;
            }

            Vector2 minLocal, maxLocal;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, minScreen, canvas.worldCamera, out minLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, maxScreen, canvas.worldCamera, out maxLocal);

            rectTransform.sizeDelta = new Vector2(Mathf.Abs(maxLocal.x - minLocal.x), Mathf.Abs(maxLocal.y - minLocal.y));
            //rectTransform.anchoredPosition = (minLocal + maxLocal) * 0.5f; // center the image
        }
    }
}

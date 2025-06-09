using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace CustomArchitecture
{
    public interface ScreenshotBounded
    {
        public Bounds GetBounds();
    }

    public class SpriteRendererScreenshotAdapter : ScreenshotBounded
    {
        private readonly SpriteRenderer sprite_renderer;

        public SpriteRendererScreenshotAdapter(SpriteRenderer spriteRenderer)
        {
            this.sprite_renderer = spriteRenderer;
        }

        public Bounds GetBounds()
        {
            return sprite_renderer.bounds;
        }
    }

    public class ScreenshotData<T> where T : ScreenshotBounded
    {
        private readonly T          target;
        private readonly Camera     camera;
        private readonly Enum       type;

        private Bounds      screen_bounds;
        private Texture2D   screen_texture = null;

        public Bounds Bounds() => screen_bounds;
        public Texture2D Texture() => screen_texture;
        public Enum Type() => type;

        public ScreenshotData(T target, Camera camera, Enum type)
        {
            this.camera = camera;
            this.target = target;
            screen_bounds = target.GetBounds();
            RefreshTexture();
            this.type = type;
        }

        public void RecreateTexture(int width, int height)
        {
            if (screen_texture != null)
                UnityEngine.Object.Destroy(screen_texture);

            screen_texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
            screen_texture.filterMode = FilterMode.Point;
        }

        public void TryUpdateTexture()
        {
            if (screen_bounds != target.GetBounds())
            {
                screen_bounds = target.GetBounds();
                RefreshTexture();
            }
        }

        private void RefreshTexture()
        {
            if (screen_texture != null)
            {
                UnityEngine.Object.Destroy(screen_texture);
                screen_texture = null;
            }

            Vector3 screenBottomLeft = camera.WorldToScreenPoint(screen_bounds.min);
            Vector3 screenTopRight = camera.WorldToScreenPoint(screen_bounds.max);

            int rectWidth = Mathf.RoundToInt(screenTopRight.x - screenBottomLeft.x);
            int rectHeight = Mathf.RoundToInt(screenTopRight.y - screenBottomLeft.y);

            screen_texture = new Texture2D(rectWidth, rectHeight, TextureFormat.ARGB32, false);
            screen_texture.filterMode = FilterMode.Point;
        }
    }

    public class Screenshoter<T> : BaseBehaviour where T : Enum
    {
        protected Camera m_baseCamera;

        [SerializeField] protected RenderTexture                    m_screenshotRenderTexture;
        protected Dictionary<T, ScreenshotData<ScreenshotBounded>>  m_screenshotDatas = new();

//        protected Action<bool, Sprite> m_onScreenshotSprite;
        protected Action<T, Texture2D> m_onScreenshotDone;

        public void SubscribeToScreenshot(Action<T, Texture2D> function) { m_onScreenshotDone -= function; m_onScreenshotDone += function; }
        public Camera GetCameraBase() => m_baseCamera;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        {
            foreach (var data in m_screenshotDatas)
            {
                data.Value.TryUpdateTexture();
            }
        }
        protected override void OnUpdate()
        {
            // c caca
            if (m_screenshotRenderTexture.width != Screen.width || m_screenshotRenderTexture.height != Screen.height)
            {
                m_screenshotRenderTexture.Release();

                m_screenshotRenderTexture.width = Screen.width;
                m_screenshotRenderTexture.height = Screen.height;

                m_screenshotRenderTexture.Create();
            }
        }
        public override void LateInit(params object[] parameters)
        { }

        public override void Init(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not Camera)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }

            m_baseCamera = (Camera)parameters[0];
        }
        #endregion

        #region Register screenshot
        public void RegisterScreenData(T type, ScreenshotData<ScreenshotBounded> data)
        {
            if (!m_screenshotDatas.ContainsKey(type))
                m_screenshotDatas.Add(type, data);
            else
                Debug.LogWarning("Screenshot data already registered");
        }
        #endregion Register screenshot

        #region Screenshot
        public IEnumerator TakeScreenshot(T type)
        {
            if (!m_screenshotDatas.ContainsKey(type))
                yield break;

            m_baseCamera.targetTexture = m_screenshotRenderTexture;

            yield return new WaitForEndOfFrame();

            RenderTexture.active = m_screenshotRenderTexture;

            m_baseCamera.Render();

            CaptureScreenshot(m_screenshotDatas[type]);

            RenderTexture.active = null;
            m_baseCamera.targetTexture = null;
        }

        private void SaveTextureAsPNG(Texture2D texture, string fileName)
        {
            if (texture == null)
            {
                Debug.LogError("Texture is null, cannot save.");
                return;
            }

            byte[] bytes = texture.EncodeToPNG();
            string path = Path.Combine(Application.streamingAssetsPath, fileName);
            File.WriteAllBytes(path, bytes);
        }

        protected void CaptureScreenshot(ScreenshotData<ScreenshotBounded> data)
        {
            Bounds spriteBounds = data.Bounds();

            Vector3 screenBottomLeft = m_baseCamera.WorldToScreenPoint(spriteBounds.min);
            Vector3 screenTopRight = m_baseCamera.WorldToScreenPoint(spriteBounds.max);

            int rectWidth = Mathf.RoundToInt(screenTopRight.x - screenBottomLeft.x);
            int rectHeight = Mathf.RoundToInt(screenTopRight.y - screenBottomLeft.y);
            //float rectX = screenBottomLeft.x;
            //float rectY = screenBottomLeft.y;

            //Rect captureRect = new Rect(rectX, rectY, rectWidth, rectHeight);
            float rectX = screenBottomLeft.x;
            float rectY = screenBottomLeft.y;
            float flippedY = m_screenshotRenderTexture.height - (rectY + rectHeight);

            Rect captureRect = new Rect(rectX, flippedY, rectWidth, rectHeight);

            data.RecreateTexture(rectWidth, rectHeight);

            data.Texture().ReadPixels(captureRect, 0, 0);
            data.Texture().Apply();

//            SaveTextureAsPNG(data.Texture(), "Tests/front.png");

            m_onScreenshotDone?.Invoke((T)data.Type(), data.Texture());
        }
        #endregion
    }
}

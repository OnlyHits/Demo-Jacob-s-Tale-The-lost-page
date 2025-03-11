using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering.Universal;
using System;
using CustomArchitecture;

namespace Comic
{
    public enum URP_OverlayCameraType
    {
        Camera_Game,
        Camera_Hud,
        Camera_Hud_TurnPage,
    }

    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(UniversalAdditionalCameraData))]
    public class URP_CameraManager : BaseBehaviour
    {
        private Dictionary<URP_OverlayCameraType, ACameraRegister> m_overlayCameras;
        private Camera m_baseCamera;

        [SerializeField] private RenderTexture  m_screenshotRenderTexture;

        private Action<bool, Sprite> m_onScreenshotSprite;

        public void SubscribeToScreenshot(Action<bool, Sprite> function) { m_onScreenshotSprite -= function; m_onScreenshotSprite += function; }
        public Bounds GetScreenshotBounds() => m_leftScreenshot.bounds;
        public bool IsCameraRegister(URP_OverlayCameraType type) => m_overlayCameras != null && m_overlayCameras.ContainsKey(type);
        public Camera GetCameraBase() => m_baseCamera;

        private Texture2D               m_screenLeft;
        private Texture2D               m_screenRight;
        private Texture2D               m_coverLeft;
        private Texture2D               m_coverRight;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
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
        {
            if (parameters.Length != 2
                || parameters[0] is not SpriteRenderer
                || parameters[1] is not SpriteRenderer)
            {
                Debug.LogWarning("Bad parameters");
                return;
            }

            InitCover((SpriteRenderer)parameters[0], false);
            InitCover((SpriteRenderer)parameters[1], true);
        }
        public override void Init(params object[] parameters)
        {
            m_overlayCameras = new();
            m_baseCamera = GetComponent<Camera>();

            InitScreenTexture(true);
            InitScreenTexture(false);
        }
        #endregion

        public void ClearCameraStack()
        {
            UniversalAdditionalCameraData baseCameraData = m_baseCamera.GetComponent<UniversalAdditionalCameraData>();
            if (baseCameraData == null)
            {
                Debug.LogError("Base Camera is missing UniversalAdditionalCameraData component!");
                return;
            }

            baseCameraData.cameraStack.Clear();
        }

        public void InitCover(SpriteRenderer sprite_renderer, bool left)
        {
            Vector3 screenBottomLeft = m_baseCamera.WorldToScreenPoint(sprite_renderer.bounds.min);
            Vector3 screenTopRight = m_baseCamera.WorldToScreenPoint(sprite_renderer.bounds.max);

            int rectWidth = Mathf.RoundToInt(screenTopRight.x - screenBottomLeft.x);
            int rectHeight = Mathf.RoundToInt(screenTopRight.y - screenBottomLeft.y);

            if (left)
            {
                m_coverLeftScreenshot = sprite_renderer;
                m_coverLeft = new Texture2D(rectWidth, rectHeight, TextureFormat.ARGB32, false);
                m_coverLeft.filterMode = FilterMode.Point;
            }
            else
            {
                m_coverRightScreenshot = sprite_renderer;
                m_coverRight = new Texture2D(rectWidth, rectHeight, TextureFormat.ARGB32, false);
                m_coverRight.filterMode = FilterMode.Point;
            }
        }

        public void InitScreenTexture(bool left)
        {
            if (left)
            {
                Vector3 screenBottomLeft = m_baseCamera.WorldToScreenPoint(m_leftScreenshot.bounds.min);
                Vector3 screenTopRight = m_baseCamera.WorldToScreenPoint(m_leftScreenshot.bounds.max);

                int rectWidth = Mathf.RoundToInt(screenTopRight.x - screenBottomLeft.x);
                int rectHeight = Mathf.RoundToInt(screenTopRight.y - screenBottomLeft.y);

                m_screenLeft = new Texture2D(rectWidth, rectHeight, TextureFormat.ARGB32, false);
                m_screenLeft.filterMode = FilterMode.Point;
            }
            else
            {
                Vector3 screenBottomLeft = Camera.main.WorldToScreenPoint(m_rightScreenshot.bounds.min);
                Vector3 screenTopRight = Camera.main.WorldToScreenPoint(m_rightScreenshot.bounds.max);

                int rectWidth = Mathf.RoundToInt(screenTopRight.x - screenBottomLeft.x);
                int rectHeight = Mathf.RoundToInt(screenTopRight.y - screenBottomLeft.y);

                m_screenRight = new Texture2D(rectWidth, rectHeight, TextureFormat.ARGB32, false);
                m_screenRight.filterMode = FilterMode.Point;
            }
        }

        public void RegisterCameras(GameCameraRegister game_camera)
        {
            if (game_camera != null)
            {
                if (m_overlayCameras.ContainsKey(URP_OverlayCameraType.Camera_Game))
                {
                    Debug.Log("Game cameras are already registered");
                    return;
                }
                else
                {
                    m_overlayCameras.Add(URP_OverlayCameraType.Camera_Game, game_camera);
                }
            }
            else
            {
                Debug.LogWarning("Game camera data is null");
                return;
            }

            UniversalAdditionalCameraData base_camera_data = m_baseCamera.GetComponent<UniversalAdditionalCameraData>();

            if (base_camera_data == null)
            {
                Debug.LogError("Base Camera is missing UniversalAdditionalCameraData component!");
                return;
            }

            foreach (var camera in m_overlayCameras[URP_OverlayCameraType.Camera_Game].GetCameras())
            {
                UniversalAdditionalCameraData camera_data = camera.GetComponent<UniversalAdditionalCameraData>();
                if (camera_data != null)
                {
                    camera_data.renderType = CameraRenderType.Overlay;
                    base_camera_data.cameraStack.Add(camera);
                }
            }
        }

        public void RegisterCameras(HudCameraRegister hud_camera)
        {
            if (hud_camera != null)
            {
                if (m_overlayCameras.ContainsKey(URP_OverlayCameraType.Camera_Hud))
                {
                    Debug.Log("Hud cameras are already registered");
                    return;
                }
                else
                {
                    m_overlayCameras.Add(URP_OverlayCameraType.Camera_Hud, hud_camera);
                }
            }
            else
            {
                Debug.LogWarning("Hud camera data is null");
            }

            UniversalAdditionalCameraData base_camera_data = m_baseCamera.GetComponent<UniversalAdditionalCameraData>();

            if (base_camera_data == null)
            {
                Debug.LogError("Base Camera is missing UniversalAdditionalCameraData component!");
                return;
            }

            foreach (var camera in m_overlayCameras[URP_OverlayCameraType.Camera_Hud].GetCameras())
            {
                UniversalAdditionalCameraData camera_data = camera.GetComponent<UniversalAdditionalCameraData>();
                if (camera_data != null)
                {
                    camera_data.renderType = CameraRenderType.Overlay;
                    base_camera_data.cameraStack.Add(camera);
                }
            }
        }

        private List<bool> UnactiveAllCameras(Camera ignore_camera)
        {
            List<bool> register_actives = new();

            foreach (var registered_camera in m_overlayCameras)
            {
                foreach (var camera in registered_camera.Value.GetCameras())
                {
                    register_actives.Add(camera.gameObject.activeSelf);

                    camera.gameObject.SetActive(camera == ignore_camera);
                }
            }

            return register_actives;
        }

        private void RestoreActiveCameras(List<bool> register_actives)
        {
            int i = 0;

            foreach (var registered_camera in m_overlayCameras)
            {
                foreach (var camera in registered_camera.Value.GetCameras())
                {
                    camera.gameObject.SetActive(register_actives[i]);
                    ++i;
                }
            }
        }

        //public void RenderOnScreenshot()
        //{
        //    m_baseCamera.targetTexture = m_screenshotRenderTexture;
        //}

        //public void RestoreDefaultRender()
        //{
        //    RenderTexture.active = null;
        //    m_baseCamera.targetTexture = null;
        //}

        public IEnumerator TakeScreenshot(bool screenshot_cover)
        {
            m_baseCamera.targetTexture = m_screenshotRenderTexture;

            yield return new WaitForEndOfFrame();

            RenderTexture.active = m_screenshotRenderTexture;

            m_baseCamera.Render();

            if (screenshot_cover)
                yield return StartCoroutine(TakeCoverScreenshot());
            else
                yield return StartCoroutine(TakePageScreenshot());

            RenderTexture.active = null;
            m_baseCamera.targetTexture = null;
        }

        private IEnumerator TakeCoverScreenshot()
        {
            CaptureScreenshot(m_coverRight, m_coverRightScreenshot, true);
            CaptureScreenshot(m_coverLeft, m_coverLeftScreenshot, false);
            yield return null;
        }

        private IEnumerator TakePageScreenshot()
        {
            CaptureScreenshot(m_screenRight, m_rightScreenshot, true);
            CaptureScreenshot(m_screenLeft, m_leftScreenshot, false);
            yield return null;
        }

        #region Screenshot

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

        private void CaptureScreenshot(Texture2D texture, SpriteRenderer render_area, bool front)
        {
            Bounds spriteBounds = render_area.bounds;

            Vector3 screenBottomLeft = m_baseCamera.WorldToScreenPoint(spriteBounds.min);
            Vector3 screenTopRight = m_baseCamera.WorldToScreenPoint(spriteBounds.max);

            int rectWidth = Mathf.RoundToInt(screenTopRight.x - screenBottomLeft.x);
            int rectHeight = Mathf.RoundToInt(screenTopRight.y - screenBottomLeft.y);
            float rectX = screenBottomLeft.x;
            float rectY = screenBottomLeft.y;

            Rect captureRect = new Rect(rectX, rectY, texture.width, texture.height);

            texture.ReadPixels(captureRect, 0, 0);
            texture.Apply();

//            SaveTextureAsPNG(texture, front ? "Tests/front.png" : "Tests/back.png");

            m_onScreenshotSprite?.Invoke(front, texture.ConvertTextureToSprite());
        }

        #endregion

        #region ScreenshotUtils

        public Vector3? GetScreenshotMin(SpriteRenderer sprite)
        {
            if (sprite == null)
            {
                Debug.LogError("Missing required components for cropping!");
                return null;
            }

            Bounds sprite_bounds = sprite.bounds;

            return m_baseCamera.WorldToScreenPoint(new Vector3(sprite_bounds.min.x, sprite_bounds.min.y, sprite_bounds.center.z));
        }

        public Vector3? GetScreenshotMax(SpriteRenderer sprite)
        {
            if (sprite == null)
            {
                Debug.LogError("Missing required components for cropping!");
                return null;
            }

            Bounds sprite_bounds = sprite.bounds;

            return m_baseCamera.WorldToScreenPoint(new Vector3(sprite_bounds.max.x, sprite_bounds.max.y, sprite_bounds.center.z));
        }

        #endregion
    }
}

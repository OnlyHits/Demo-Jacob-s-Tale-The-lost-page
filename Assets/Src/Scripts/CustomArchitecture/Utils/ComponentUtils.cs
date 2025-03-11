using UnityEngine;
using CustomArchitecture;

namespace CustomArchitecture
{
    public class ComponentUtils
    {
        // Function get or create a BaseBehaviour and not MonoBehaviour to avoid inconsistency
        public static bool GetOrCreateComponent<U>(GameObject parent_object, out U component) where U : BaseBehaviour
        {
            component = parent_object.GetComponent<U>();

            if (component == null)
            {
                component = parent_object.AddComponent<U>();
            }

            return component != null;
        }

        // Find a BaseBehaviour and not MonoBehaviour to avoid inconsistency
        public static T FindObjectAcrossScenes<T>() where T : BaseBehaviour
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                foreach (GameObject rootObject in scene.GetRootGameObjects())
                {
                    T component = rootObject.GetComponentInChildren<T>(true);
                    if (component != null)
                        return component;
                }
            }
            return null;
        }
    }
}
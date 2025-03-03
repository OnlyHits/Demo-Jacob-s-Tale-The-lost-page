using UnityEngine;
using CustomArchitecture;

namespace CustomArchitecture
{
    public class ComponentUtils : MonoBehaviour
    {
        public static bool GetOrCreateComponent<U>(GameObject parent_object, out U component) where U : BaseBehaviour
        {
            component = parent_object.GetComponent<U>();

            if (component == null)
            {
                component = parent_object.AddComponent<U>();
            }

            return component != null;
        }
    }
}
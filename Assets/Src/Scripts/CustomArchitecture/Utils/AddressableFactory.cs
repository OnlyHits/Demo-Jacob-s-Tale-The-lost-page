using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CustomArchitecture
{
    public static class AddressableFactory
    {
        /// <summary>
        /// Coroutine-compatible method that loads and instantiates an addressable prefab at a given position and rotation.
        /// </summary>
        public static IEnumerator CreateAsync(string address, Vector3 position, Quaternion rotation, System.Action<GameObject> onComplete)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject obj = Object.Instantiate(handle.Result, position, rotation);
                onComplete?.Invoke(obj);
            }
            else
            {
                Debug.LogError($"AddressableFactory: Failed to load addressable at address '{address}'");
                onComplete?.Invoke(null);
            }
        }

        /// <summary>
        /// Coroutine-compatible method that loads and instantiates an addressable prefab as a child of a specified parent Transform.
        /// </summary>
        public static IEnumerator CreateAsync(string address, Transform parent, System.Action<GameObject> onComplete)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject obj = Object.Instantiate(handle.Result, parent);
                onComplete?.Invoke(obj);
            }
            else
            {
                Debug.LogError($"AddressableFactory: Failed to load addressable at address '{address}'");
                onComplete?.Invoke(null);
            }
        }
    }
}
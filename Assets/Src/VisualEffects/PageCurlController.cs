using UnityEngine;

public class PageCurlController : MonoBehaviour
{
    public Material pageCurlMaterial;
    public float curlIntensity = 0.5f;
    public float curlAngle = 1.0f;
    public float rotation = 0.0f;

    void Update()
    {
        // Get mesh bounds and calculate the left-center pivot
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null && mf.mesh != null)
        {
            Bounds bounds = mf.mesh.bounds;
            Vector3 leftCenterPivot = new Vector3(bounds.min.x, (bounds.min.y + bounds.max.y) / 2, 0);
            pageCurlMaterial.SetVector("_Pivot", leftCenterPivot);
        }

        // Set shader parameters
        pageCurlMaterial.SetFloat("_CurlIntensity", curlIntensity);
        pageCurlMaterial.SetFloat("_CurlAngle", curlAngle);
        pageCurlMaterial.SetFloat("_Rotation", rotation);
    }
}

using System.Security.Cryptography;
using Comic;
using CustomArchitecture;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.HableCurve;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SubdividedPlane : BaseBehaviour
{
    [SerializeField] private int m_xSegments = 20;
    [SerializeField] private int m_ySegments = 20;

    #region BaseBehaviour
    protected override void OnFixedUpdate()
    { }
    protected override void OnLateUpdate()
    { }
    protected override void OnUpdate()
    { }
    public override void LateInit(params object[] parameters)
    { }
    public override void Init(params object[] parameters)
    {
        if (parameters.Length < 1 || parameters[0] is not Bounds)
        {
            Debug.LogWarning("Wrong parameters");
            return;
        }

        MeshFilter mf = GetComponent<MeshFilter>();
        
        mf.mesh = GenerateMesh((Bounds)parameters[0]);
    }
    #endregion

    public Mesh GenerateMesh(Bounds bounds)
    {
        int vertCountX = m_xSegments + 1;
        int vertCountY = m_ySegments + 1;

        Vector3[] vertices = new Vector3[vertCountX * vertCountY];
        Vector2[] uv = new Vector2[vertices.Length];
        Vector2[] uv2 = new Vector2[vertices.Length];
        int[] triangles = new int[m_xSegments * m_ySegments * 6];

        float width = bounds.size.x;
        float height = bounds.size.y;

        for (int y = 0; y < vertCountY; y++)
        {
            for (int x = 0; x < vertCountX; x++)
            {
                int i = x + y * vertCountX;

                float xNorm = (float)x / m_xSegments;
                float yNorm = (float)y / m_ySegments;

                // Position centered at (0,0), fit bounds
                float xPos = xNorm - 0.5f;
                float yPos = yNorm - 0.5f;

                vertices[i] = new Vector3(xPos * width, yPos * height, 0);
                uv[i] = new Vector2(xNorm, yNorm);
                uv2[i] = new Vector2(xNorm, 0); // store normalized X in uv2.x for shader
            }
        }

        int ti = 0;
        for (int y = 0; y < m_ySegments; y++)
        {
            for (int x = 0; x < m_xSegments; x++)
            {
                int i = x + y * vertCountX;

                triangles[ti++] = i;
                triangles[ti++] = i + vertCountX;
                triangles[ti++] = i + 1;

                triangles[ti++] = i + 1;
                triangles[ti++] = i + vertCountX;
                triangles[ti++] = i + vertCountX + 1;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "SubdividedPlane";
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.uv2 = uv2;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}

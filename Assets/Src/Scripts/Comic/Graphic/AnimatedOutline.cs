using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AnimatedOutline : MonoBehaviour
{
    public Texture2D[] outlineFrames;
    public float frameRate = 6f;

    private SpriteRenderer sr;
    private MaterialPropertyBlock block;
    private int currentFrame;
    private float timer;

    private static readonly int OutlineTex = Shader.PropertyToID("_OutlineTex");

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        block = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (outlineFrames == null || outlineFrames.Length == 0)
            return;

        timer += Time.deltaTime;
        if (timer >= 1f / frameRate)
        {
            currentFrame = (currentFrame + 1) % outlineFrames.Length;
            timer = 0f;
        }

        sr.GetPropertyBlock(block);
        block.SetTexture(OutlineTex, outlineFrames[currentFrame]);
        sr.SetPropertyBlock(block);
    }
}

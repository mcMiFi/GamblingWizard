using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public sealed class LightningChainVfx : MonoBehaviour
{
    private const int PointCount = 8;

    private LineRenderer line;
    private Transform target;
    private Vector3 startPosition;
    private float duration;
    private float elapsed;
    private Material lineMaterial;

    public static void Spawn(Vector3 start, Transform end, Color color, float duration)
    {
        if (end == null)
            return;

        GameObject bolt = new GameObject("Lightning Chain");
        LightningChainVfx effect = bolt.AddComponent<LightningChainVfx>();
        effect.Initialize(start, end, color, duration);
    }

    private void Initialize(Vector3 start, Transform end, Color color, float effectDuration)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
        {
            Debug.LogError("Lightning VFX requires an unlit or sprite shader.", this);
            Destroy(gameObject);
            return;
        }

        startPosition = start;
        target = end;
        duration = Mathf.Max(0.01f, effectDuration);

        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = PointCount;
        line.startWidth = 0.075f;
        line.endWidth = 0.025f;
        line.startColor = color;
        line.endColor = new Color(color.r, color.g, color.b, 0f);
        line.numCapVertices = 2;

        lineMaterial = new Material(shader);
        if (lineMaterial.HasProperty("_BaseColor"))
            lineMaterial.SetColor("_BaseColor", Color.white);
        if (lineMaterial.HasProperty("_Color"))
            lineMaterial.SetColor("_Color", Color.white);
        line.material = lineMaterial;

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        while (elapsed < duration && target != null)
        {
            float progress = Mathf.Clamp01(elapsed / duration);
            UpdateBolt(Mathf.Lerp(0.16f, 0.025f, progress));
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private void UpdateBolt(float jitter)
    {
        Vector3 endPosition = target.position;
        for (int i = 0; i < PointCount; i++)
        {
            float t = i / (float)(PointCount - 1);
            Vector3 point = Vector3.Lerp(startPosition, endPosition, t);
            if (i > 0 && i < PointCount - 1)
                point += Random.insideUnitSphere * jitter;

            line.SetPosition(i, point);
        }
    }

    private void OnDestroy()
    {
        if (lineMaterial != null)
            Destroy(lineMaterial);
    }
}

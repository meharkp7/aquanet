using UnityEngine;
using System.Collections;

public class EffectsEngine : MonoBehaviour
{
    public NodeSystem nodeSystem;

    public void Leak(int index)
    {
        StartCoroutine(LeakEffect(nodeSystem.GetPosition(index)));
    }

    IEnumerator LeakEffect(Vector3 pos)
    {
        while (true)
        {
            GameObject drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            drop.transform.position = pos;
            drop.transform.localScale = Vector3.one * 0.1f;

            var mat = drop.GetComponent<Renderer>().material;
            mat.color = Color.cyan;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.cyan * 2f);

            float t = 0;

            while (t < 1)
            {
                drop.transform.position += Vector3.down * Time.deltaTime * 2f;
                t += Time.deltaTime;
                yield return null;
            }

            Destroy(drop);
        }
    }

    public void ShakeAll()
    {
        StartCoroutine(ShakeEffect());
    }

    IEnumerator ShakeEffect()
    {
        float t = 0;

        while (t < 1.5f)
        {
            foreach (var node in nodeSystem.nodes)
            {
                node.obj.transform.position += Random.insideUnitSphere * 0.05f;
            }

            t += Time.deltaTime;
            yield return null;
        }
    }
}
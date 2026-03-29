using UnityEngine;
using System.Collections;

public class SensorEngine : MonoBehaviour
{
    public NodeSystem nodeSystem;

    public void Scan()
    {
        StartCoroutine(ScanEffect());
    }

    IEnumerator ScanEffect()
    {
        for (int i = 0; i < nodeSystem.nodes.Count; i++)
        {
            var node = nodeSystem.nodes[i].obj;

            var mat = node.GetComponent<Renderer>().material;
            mat.color = Color.yellow;
            mat.SetColor("_EmissionColor", Color.yellow * 2f);

            yield return new WaitForSeconds(0.15f);
        }

        nodeSystem.UpdatePressureVisuals();
    }
}
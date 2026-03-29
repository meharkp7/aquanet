using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowEngine : MonoBehaviour
{
    public NodeSystem nodeSystem;
    public PipeSystem pipeSystem;

    public float baseSpeed = 1.2f;
    public bool leakActive = true;
    public int leakNodeIndex = 0;

    List<Coroutine> activeFlows = new List<Coroutine>();

    // =====================================================
    // START FLOW ON PATH
    // =====================================================
    public void StartFlow(List<int> path)
    {
        StopAllFlows();

        Coroutine c = StartCoroutine(FlowRoutine(path));
        activeFlows.Add(c);
    }

    // =====================================================
    // STOP ALL FLOWS
    // =====================================================
    public void StopAllFlows()
    {
        foreach (var c in activeFlows)
        {
            if (c != null)
                StopCoroutine(c);
        }

        activeFlows.Clear();
    }

    // =====================================================
    // MAIN FLOW LOOP
    // =====================================================
    IEnumerator FlowRoutine(List<int> path)
    {
        while (true)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                int a = path[i];
                int b = path[i + 1];

                // 🚨 BLOCK FLOW IF LEAK PRESENT
                if (leakActive && a == leakNodeIndex)
                    continue;

                float pressure = nodeSystem.nodes[a].pressure;

                float speed = baseSpeed + (pressure / 100f) * 2f;

                StartCoroutine(FlowParticle(
                    nodeSystem.GetPosition(a),
                    nodeSystem.GetPosition(b),
                    speed
                ));
            }

            yield return new WaitForSeconds(0.08f);
        }
    }

    // =====================================================
    // SINGLE WATER PARTICLE
    // =====================================================
    IEnumerator FlowParticle(Vector3 start, Vector3 end, float speed)
    {
        GameObject drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        drop.transform.localScale = Vector3.one * 0.12f;

        var mat = drop.GetComponent<Renderer>().material;
        mat.color = Color.cyan;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.cyan * 2.5f);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;

            // Smooth curved motion (better than straight line)
            Vector3 mid = (start + end) / 2 + Vector3.up * 0.3f;
            Vector3 p1 = Vector3.Lerp(start, mid, t);
            Vector3 p2 = Vector3.Lerp(mid, end, t);

            drop.transform.position = Vector3.Lerp(p1, p2, t);

            yield return null;
        }

        Destroy(drop);
    }

    // =====================================================
    // FLOW BURST (FOR SUCCESS MOMENT)
    // =====================================================
    public void BurstFlow(List<int> path)
    {
        for (int i = 0; i < 10; i++)
        {
            foreach (var segment in path)
            {
                if (segment < path.Count - 1)
                {
                    StartCoroutine(FlowParticle(
                        nodeSystem.GetPosition(segment),
                        nodeSystem.GetPosition(segment + 1),
                        baseSpeed * 2f
                    ));
                }
            }
        }
    }

    // =====================================================
    // FLOW DIRECTION VISUAL (OPTIONAL ADVANCED)
    // =====================================================
    public void HighlightFlowDirection(List<int> path)
    {
        foreach (var p in pipeSystem.pipes)
        {
            var mat = p.obj.GetComponent<Renderer>().material;
            mat.color = Color.gray;
        }

        for (int i = 0; i < path.Count - 1; i++)
        {
            int a = path[i];
            int b = path[i + 1];

            foreach (var pipe in pipeSystem.pipes)
            {
                if ((pipe.from == a && pipe.to == b) ||
                    (pipe.from == b && pipe.to == a))
                {
                    var mat = pipe.obj.GetComponent<Renderer>().material;

                    mat.color = Color.cyan;
                    mat.SetColor("_EmissionColor", Color.cyan * 2f);
                }
            }
        }
    }
}
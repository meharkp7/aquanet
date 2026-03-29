using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathVisualization : MonoBehaviour
{
    public PipeSystem pipeSystem;
    public NodeSystem nodeSystem;
    public GraphModel graph;

    public float stepDelay = 1.5f;

    // =====================================================
    // MAIN AI VISUALIZATION
    // =====================================================
    public void VisualizeDecision(int start, int end, int attempts = 3)
    {
        StartCoroutine(DecisionRoutine(start, end, attempts));
    }

    IEnumerator DecisionRoutine(int start, int end, int attempts)
    {
        List<List<int>> triedPaths = new List<List<int>>();

        for (int i = 0; i < attempts; i++)
        {
            List<int> path = GenerateRandomPath(start, end);

            triedPaths.Add(path);

            // 🧠 SHOW THINKING PATH
            yield return HighlightPathSequential(path, Color.yellow);

            // ❌ REJECT PATH
            yield return MarkPath(path, Color.red);

            yield return new WaitForSeconds(0.5f);
        }

        // ✅ FINAL PATH (BEST)
        List<int> finalPath = graph.FindPath(start, end);

        yield return HighlightPathSequential(finalPath, Color.cyan);

        pipeSystem.HighlightPath(finalPath);

        // pulse nodes on final path
        StartCoroutine(PulseFinalNodes(finalPath));
    }

    // =====================================================
    // RANDOM PATH GENERATOR (SIMULATES AI ATTEMPTS)
    // =====================================================
    List<int> GenerateRandomPath(int start, int end)
    {
        List<int> path = new List<int>();
        HashSet<int> visited = new HashSet<int>();

        int current = start;
        path.Add(current);

        int maxSteps = 10;

        while (current != end && maxSteps-- > 0)
        {
            var neighbors = graph.nodes[current].neighbors;

            int next = neighbors[Random.Range(0, neighbors.Count)];

            if (!visited.Contains(next))
            {
                path.Add(next);
                visited.Add(next);
                current = next;
            }
        }

        return path;
    }

    // =====================================================
    // STEP-BY-STEP PATH HIGHLIGHT
    // =====================================================
    IEnumerator HighlightPathSequential(List<int> path, Color color)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            int a = path[i];
            int b = path[i + 1];

            HighlightEdge(a, b, color);

            HighlightNode(a, color);

            yield return new WaitForSeconds(0.2f);
        }

        HighlightNode(path[path.Count - 1], color);

        yield return new WaitForSeconds(stepDelay);
    }

    // =====================================================
    // MARK FULL PATH
    // =====================================================
    IEnumerator MarkPath(List<int> path, Color color)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            HighlightEdge(path[i], path[i + 1], color);
        }

        foreach (var n in path)
        {
            HighlightNode(n, color);
        }

        yield return new WaitForSeconds(1f);
    }

    // =====================================================
    // HIGHLIGHT EDGE
    // =====================================================
    void HighlightEdge(int a, int b, Color color)
    {
        foreach (var pipe in pipeSystem.pipes)
        {
            if ((pipe.from == a && pipe.to == b) ||
                (pipe.from == b && pipe.to == a))
            {
                var mat = pipe.obj.GetComponent<Renderer>().material;
                mat.color = color;
                mat.SetColor("_EmissionColor", color * 2f);
            }
        }
    }

    // =====================================================
    // HIGHLIGHT NODE
    // =====================================================
    void HighlightNode(int index, Color color)
    {
        var node = nodeSystem.nodes[index].obj;
        var mat = node.GetComponent<Renderer>().material;

        mat.color = color;
        mat.SetColor("_EmissionColor", color * 2f);
    }

    // =====================================================
    // FINAL PATH PULSE (SUCCESS FEEL)
    // =====================================================
    IEnumerator PulseFinalNodes(List<int> path)
    {
        float duration = 2f;
        float t = 0;

        while (t < duration)
        {
            foreach (var index in path)
            {
                var node = nodeSystem.nodes[index].obj;

                float scale = 1f + Mathf.Sin(Time.time * 8f) * 0.2f;
                node.transform.localScale = Vector3.one * scale;
            }

            t += Time.deltaTime;
            yield return null;
        }

        // reset scale
        foreach (var index in path)
        {
            nodeSystem.nodes[index].obj.transform.localScale = Vector3.one * 0.6f;
        }
    }
}
using UnityEngine;
using System.Collections.Generic;

public class PipeSystem : MonoBehaviour
{
    public GraphModel graph;
    public NodeSystem nodeSystem;

    public class PipeData
    {
        public int from;
        public int to;
        public GameObject obj;
    }

    public List<PipeData> pipes = new List<PipeData>();

    // =====================================================
    // INIT
    // =====================================================
    void Start()
    {
        BuildFromGraph();
    }

    // =====================================================
    // BUILD PIPES FROM GRAPH CONNECTIONS
    // =====================================================
    public void BuildFromGraph()
    {
        pipes.Clear();

        HashSet<string> created = new HashSet<string>();

        foreach (var node in graph.nodes)
        {
            foreach (var neighbor in node.neighbors)
            {
                string key = node.id < neighbor 
                    ? node.id + "-" + neighbor 
                    : neighbor + "-" + node.id;

                if (created.Contains(key)) continue;

                CreatePipe(node.id, neighbor);

                created.Add(key);
            }
        }
    }

    // =====================================================
    // CREATE SINGLE PIPE
    // =====================================================
    void CreatePipe(int a, int b)
    {
        Vector3 start = nodeSystem.GetPosition(a);
        Vector3 end = nodeSystem.GetPosition(b);

        GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        pipe.transform.position = (start + end) / 2;
        pipe.transform.up = (end - start).normalized;

        float dist = Vector3.Distance(start, end);
        pipe.transform.localScale = new Vector3(0.12f, dist / 2f, 0.12f);

        var mat = pipe.GetComponent<Renderer>().material;
        mat.color = Color.gray;

        pipes.Add(new PipeData
        {
            from = a,
            to = b,
            obj = pipe
        });
    }

    // =====================================================
    // RESET ALL PIPES
    // =====================================================
    public void ResetPipes()
    {
        foreach (var p in pipes)
        {
            var mat = p.obj.GetComponent<Renderer>().material;
            mat.color = Color.gray;
        }
    }

    // =====================================================
    // HIGHLIGHT PATH (AI DECISION)
    // =====================================================
    public void HighlightPath(List<int> path)
    {
        ResetPipes();

        for (int i = 0; i < path.Count - 1; i++)
        {
            int a = path[i];
            int b = path[i + 1];

            foreach (var p in pipes)
            {
                if ((p.from == a && p.to == b) || (p.from == b && p.to == a))
                {
                    var mat = p.obj.GetComponent<Renderer>().material;

                    mat.color = Color.green;
                    mat.SetColor("_EmissionColor", Color.green * 2f);
                }
            }
        }
    }

    // =====================================================
    // MARK FAILED PATH
    // =====================================================
    public void MarkFailedPath(List<int> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            int a = path[i];
            int b = path[i + 1];

            foreach (var p in pipes)
            {
                if ((p.from == a && p.to == b) || (p.from == b && p.to == a))
                {
                    var mat = p.obj.GetComponent<Renderer>().material;

                    mat.color = Color.red;
                    mat.SetColor("_EmissionColor", Color.red * 2f);
                }
            }
        }
    }

    // =====================================================
    // GLOW ALL PIPES (SENSOR RESPONSE)
    // =====================================================
    public void PulseAllPipes(Color color)
    {
        foreach (var p in pipes)
        {
            var mat = p.obj.GetComponent<Renderer>().material;

            mat.color = color;
            mat.SetColor("_EmissionColor", color * 1.5f);
        }
    }
}
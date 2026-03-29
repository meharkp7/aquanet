using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeSystem : MonoBehaviour
{
    public GraphModel graph;

    public class NodeData
    {
        public int id;
        public GameObject obj;
        public float pressure;
        public bool isFaulty;
    }

    public List<NodeData> nodes = new List<NodeData>();

    // =====================================================
    // INIT
    // =====================================================
    void Start()
    {
        BuildFromGraph();
    }

    // =====================================================
    // BUILD NODES FROM GRAPH
    // =====================================================
    public void BuildFromGraph()
    {
        nodes.Clear();

        foreach (var gNode in graph.nodes)
        {
            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            node.transform.position = gNode.position;
            node.transform.localScale = Vector3.one * 0.6f;
            node.name = "Node_" + gNode.id;

            var mat = node.GetComponent<Renderer>().material;
            mat.EnableKeyword("_EMISSION");

            NodeData data = new NodeData()
            {
                id = gNode.id,
                obj = node,
                pressure = Random.Range(40f, 90f),
                isFaulty = false
            };

            nodes.Add(data);
        }

        UpdatePressureVisuals();
    }

    // =====================================================
    // PRESSURE VISUALIZATION
    // =====================================================
    public void UpdatePressureVisuals()
    {
        foreach (var n in nodes)
        {
            Color c = Color.Lerp(Color.blue, Color.red, n.pressure / 100f);

            var mat = n.obj.GetComponent<Renderer>().material;
            mat.color = c;
            mat.SetColor("_EmissionColor", c * 1.5f);
        }
    }

    // =====================================================
    // PRESSURE PROPAGATION (REALISTIC)
    // =====================================================
    public void PropagatePressure(int sourceIndex, float dropAmount)
    {
        StartCoroutine(PressureSpread(sourceIndex, dropAmount));
    }

    IEnumerator PressureSpread(int source, float drop)
    {
        Queue<int> queue = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();

        queue.Enqueue(source);

        while (queue.Count > 0)
        {
            int current = queue.Dequeue();

            if (visited.Contains(current)) continue;
            visited.Add(current);

            nodes[current].pressure -= drop;
            nodes[current].pressure = Mathf.Clamp(nodes[current].pressure, 0, 100);

            UpdateSingleNodeVisual(current);

            foreach (var neighbor in graph.nodes[current].neighbors)
            {
                queue.Enqueue(neighbor);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    void UpdateSingleNodeVisual(int i)
    {
        var n = nodes[i];

        Color c = Color.Lerp(Color.blue, Color.red, n.pressure / 100f);

        var mat = n.obj.GetComponent<Renderer>().material;
        mat.color = c;
        mat.SetColor("_EmissionColor", c * 1.5f);
    }

    // =====================================================
    // SENSOR SCAN WAVE (CRAZY EFFECT)
    // =====================================================
    public void TriggerSensorScan()
    {
        StartCoroutine(SensorWave());
    }

    IEnumerator SensorWave()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            StartCoroutine(PulseNode(nodes[i].obj, Color.yellow));
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator PulseNode(GameObject node, Color color)
    {
        float t = 0;

        while (t < 1f)
        {
            float scale = 1f + Mathf.Sin(Time.time * 10f) * 0.2f;

            node.transform.localScale = Vector3.one * scale;
            node.GetComponent<Renderer>().material.color = color;

            t += Time.deltaTime;
            yield return null;
        }

        node.transform.localScale = Vector3.one * 0.6f;
    }

    // =====================================================
    // LEAK / FAULT
    // =====================================================
    public void MarkLeak(int index)
    {
        nodes[index].isFaulty = true;

        var mat = nodes[index].obj.GetComponent<Renderer>().material;
        mat.color = Color.red;
        mat.SetColor("_EmissionColor", Color.red * 2f);
    }

    // =====================================================
    // GET NODE POSITION
    // =====================================================
    public Vector3 GetPosition(int index)
    {
        return nodes[index].obj.transform.position;
    }
}
using UnityEngine;
using System.Collections.Generic;

public class GraphModel : MonoBehaviour
{
    // ================================
    // 🧠 DATA STRUCTURES
    // ================================
    public class Node
    {
        public int id;
        public Vector3 position;
        public List<int> neighbors = new List<int>();
    }

    public List<Node> nodes = new List<Node>();

    public int nodeCount = 8;

    // ================================
    // 🚀 INIT
    // ================================
    void Awake()
    {
        GenerateGraph(nodeCount);
    }

    // ================================
    // 🌐 GRAPH GENERATION
    // ================================
    public void GenerateGraph(int count)
    {
        nodes.Clear();

        // 1️⃣ Create nodes
        for (int i = 0; i < count; i++)
        {
            Node n = new Node();
            n.id = i;

            // spread in grid + randomness
            float x = (i % 3) * 3f;
            float z = (i / 3) * 3f + Random.Range(-1f, 1f);

            n.position = new Vector3(x, 0.3f, z);

            nodes.Add(n);
        }

        // 2️⃣ Create connections (branching)
        for (int i = 0; i < nodes.Count; i++)
        {
            // connect to next node
            if (i < nodes.Count - 1)
                AddEdge(i, i + 1);

            // random branching
            int randomIndex = Random.Range(0, nodes.Count);
            if (randomIndex != i)
                AddEdge(i, randomIndex);
        }

        // 3️⃣ Ensure connectivity (important)
        EnsureConnected();
    }

    // ================================
    // 🔗 ADD EDGE (UNDIRECTED)
    // ================================
    void AddEdge(int a, int b)
    {
        if (!nodes[a].neighbors.Contains(b))
            nodes[a].neighbors.Add(b);

        if (!nodes[b].neighbors.Contains(a))
            nodes[b].neighbors.Add(a);
    }

    // ================================
    // 🔒 ENSURE GRAPH CONNECTED
    // ================================
    void EnsureConnected()
    {
        HashSet<int> visited = new HashSet<int>();
        DFS(0, visited);

        for (int i = 0; i < nodes.Count; i++)
        {
            if (!visited.Contains(i))
            {
                AddEdge(i, 0); // connect to root
            }
        }
    }

    void DFS(int node, HashSet<int> visited)
    {
        visited.Add(node);

        foreach (var neighbor in nodes[node].neighbors)
        {
            if (!visited.Contains(neighbor))
                DFS(neighbor, visited);
        }
    }

    // ================================
    // 🧭 PATHFINDING (BFS)
    // ================================
    public List<int> FindPath(int start, int end)
    {
        Queue<List<int>> queue = new Queue<List<int>>();
        HashSet<int> visited = new HashSet<int>();

        queue.Enqueue(new List<int> { start });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            int last = path[path.Count - 1];

            if (last == end)
                return path;

            if (!visited.Contains(last))
            {
                visited.Add(last);

                foreach (var neighbor in nodes[last].neighbors)
                {
                    var newPath = new List<int>(path);
                    newPath.Add(neighbor);
                    queue.Enqueue(newPath);
                }
            }
        }

        return null;
    }

    // ================================
    // 🎯 DEBUG VISUALIZATION
    // ================================
    void OnDrawGizmos()
    {
        if (nodes == null) return;

        Gizmos.color = Color.white;

        foreach (var node in nodes)
        {
            Gizmos.DrawSphere(node.position, 0.2f);

            foreach (var neighbor in node.neighbors)
            {
                if (neighbor < nodes.Count)
                {
                    Gizmos.DrawLine(node.position, nodes[neighbor].position);
                }
            }
        }
    }
}
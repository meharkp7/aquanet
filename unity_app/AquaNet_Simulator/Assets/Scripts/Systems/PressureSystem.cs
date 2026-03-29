using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PressureSystem : MonoBehaviour
{
    public NodeSystem nodeSystem;
    public GraphModel graph;

    public float propagationSpeed = 0.15f;
    public float damping = 0.98f; // stabilization
    public float leakDrop = 30f;

    bool isRunning = false;

    // =====================================================
    // START SYSTEM
    // =====================================================
    public void StartSimulation()
    {
        if (!isRunning)
        {
            isRunning = true;
            StartCoroutine(PressureLoop());
        }
    }

    // =====================================================
    // MAIN LOOP
    // =====================================================
    IEnumerator PressureLoop()
    {
        while (isRunning)
        {
            PropagatePressure();
            yield return new WaitForSeconds(propagationSpeed);
        }
    }

    // =====================================================
    // PRESSURE PROPAGATION
    // =====================================================
    void PropagatePressure()
    {
        float[] newPressures = new float[nodeSystem.nodes.Count];

        for (int i = 0; i < nodeSystem.nodes.Count; i++)
        {
            var node = nodeSystem.nodes[i];

            float total = node.pressure;
            int count = 1;

            foreach (var neighbor in graph.nodes[i].neighbors)
            {
                total += nodeSystem.nodes[neighbor].pressure;
                count++;
            }

            float avg = total / count;

            // smooth transition
            newPressures[i] = Mathf.Lerp(node.pressure, avg, 0.5f);
        }

        // apply damping (stabilization)
        for (int i = 0; i < newPressures.Length; i++)
        {
            nodeSystem.nodes[i].pressure = newPressures[i] * damping;
        }

        nodeSystem.UpdatePressureVisuals();
    }

    // =====================================================
    // APPLY LEAK (PRESSURE DROP)
    // =====================================================
    public void ApplyLeak(int index)
    {
        StartCoroutine(LeakEffect(index));
    }

    IEnumerator LeakEffect(int index)
    {
        while (true)
        {
            nodeSystem.nodes[index].pressure -= leakDrop * Time.deltaTime;
            nodeSystem.nodes[index].pressure = Mathf.Clamp(nodeSystem.nodes[index].pressure, 0, 100);

            yield return null;
        }
    }

    // =====================================================
    // PRESSURE SURGE (FOR TEST / EVENTS)
    // =====================================================
    public void ApplySurge(int index, float amount)
    {
        nodeSystem.nodes[index].pressure += amount;
        nodeSystem.nodes[index].pressure = Mathf.Clamp(nodeSystem.nodes[index].pressure, 0, 100);
    }

    // =====================================================
    // GLOBAL DISTURBANCE (ADVANCED)
    // =====================================================
    public void GlobalDisturbance(float intensity)
    {
        foreach (var node in nodeSystem.nodes)
        {
            node.pressure += Random.Range(-intensity, intensity);
            node.pressure = Mathf.Clamp(node.pressure, 0, 100);
        }
    }

    // =====================================================
    // RESET SYSTEM
    // =====================================================
    public void ResetSystem()
    {
        foreach (var node in nodeSystem.nodes)
        {
            node.pressure = Random.Range(40f, 80f);
        }

        nodeSystem.UpdatePressureVisuals();
    }
}
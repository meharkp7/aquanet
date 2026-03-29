using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    // ================================
    // SYSTEM REFERENCES
    // ================================
    public GraphModel graph;
    public NodeSystem nodeSystem;
    public PipeSystem pipeSystem;
    public FlowEngine flowEngine;
    public PressureSystem pressureSystem;
    public SensorEngine sensorEngine;
    public EffectsEngine effectsEngine;
    public PathVisualization pathVisualization;

    public APIClient apiClient;
    public TimelineUI timeline;
    public HUDController hud;

    int startNode = 0;
    int endNode = 6;

    // ================================
    // ENTRY POINT
    // ================================
    void Start()
    {
        StartCoroutine(ForceStart());
    }

    // ================================
    // FORCE INITIALIZATION
    // ================================
    IEnumerator ForceStart()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("🚀 Starting AQUANET...");

        InitializeSystem();

        Debug.Log("📊 Graph nodes: " + graph.nodes.Count);

        if (apiClient != null)
        {
            apiClient.OnResponse += HandleAPIResponse;
        }
        else
        {
            Debug.Log("⚠️ No API — running demo mode");
            StartCoroutine(RunSimulationLoop());
        }
    }

    // ================================
    // SYSTEM INIT (ORDER FIXED)
    // ================================
    void InitializeSystem()
    {
        if (graph == null || nodeSystem == null || pipeSystem == null)
        {
            Debug.LogError("❌ Missing references in GameController");
            return;
        }

        graph.GenerateGraph(graph.nodeCount);

        nodeSystem.BuildFromGraph();

        pipeSystem.BuildFromGraph();

        pressureSystem.StartSimulation();

        Debug.Log("💀 SYSTEM INITIALIZED");
    }

    // ================================
    // DEMO LOOP (NO BACKEND)
    // ================================
    IEnumerator RunSimulationLoop()
    {
        yield return new WaitForSeconds(1f);

        timeline?.Log("📡 Sensor Scan");
        hud?.UpdateHUD(0, 0, "Scanning");

        sensorEngine.Scan();
        nodeSystem.TriggerSensorScan();

        yield return new WaitForSeconds(2f);

        // 🚨 Leak
        timeline?.Log("🚨 Leak Detected");

        nodeSystem.MarkLeak(startNode);
        pressureSystem.ApplyLeak(startNode);
        effectsEngine.Leak(startNode);

        flowEngine.leakActive = true;
        flowEngine.leakNodeIndex = startNode;

        yield return new WaitForSeconds(2f);

        // 🧠 AI Thinking
        timeline?.Log("🧠 AI Evaluating");

        pathVisualization.VisualizeDecision(startNode, endNode, 3);

        yield return new WaitForSeconds(6f);

        // ✅ Final path
        List<int> finalPath = graph.FindPath(startNode, endNode);

        pipeSystem.HighlightPath(finalPath);

        yield return new WaitForSeconds(1f);

        // 🌊 Flow
        timeline?.Log("🌊 Flow Rerouted");

        flowEngine.leakActive = false;
        flowEngine.StartFlow(finalPath);

        hud?.UpdateHUD(25f, 2, "Stable");

        yield return new WaitForSeconds(2f);

        timeline?.Log("⚙️ System Stable");
    }

    // ================================
    // BACKEND RESPONSE HANDLER
    // ================================
    void HandleAPIResponse(string json)
    {
        APIResponse res = JsonUtility.FromJson<APIResponse>(json);

        List<int> path = DataMapper.MapPath(res);

        StopAllCoroutines();
        StartCoroutine(RunLiveSystem(path, res));
    }

    // ================================
    // LIVE SYSTEM (WITH AI)
    // ================================
    IEnumerator RunLiveSystem(List<int> finalPath, APIResponse res)
    {
        timeline?.Log("📡 Sensor Scan");
        hud?.UpdateHUD(res.risk_score, res.retries, "Scanning");

        sensorEngine.Scan();
        nodeSystem.TriggerSensorScan();

        yield return new WaitForSeconds(2f);

        // 🚨 Leak
        timeline?.Log("🚨 Leak Detected");

        nodeSystem.MarkLeak(startNode);
        pressureSystem.ApplyLeak(startNode);
        effectsEngine.Leak(startNode);

        flowEngine.leakActive = true;

        yield return new WaitForSeconds(2f);

        // 🤖 AI Thinking
        timeline?.Log("🧠 AI Thinking");
        hud?.UpdateHUD(res.risk_score, res.retries, "Thinking");

        for (int i = 0; i < res.retries; i++)
        {
            timeline?.Log("🔁 Retry " + (i + 1));

            pathVisualization.VisualizeDecision(startNode, endNode, 1);

            yield return new WaitForSeconds(2f);

            pipeSystem.MarkFailedPath(finalPath);
        }

        // ✅ Final
        timeline?.Log("✅ Optimal Path");

        pipeSystem.HighlightPath(finalPath);

        yield return new WaitForSeconds(1f);

        // 🌊 Flow
        timeline?.Log("🌊 Flow Rerouted");

        flowEngine.leakActive = false;
        flowEngine.StartFlow(finalPath);

        hud?.UpdateHUD(res.risk_score, res.retries, "Stable");

        yield return new WaitForSeconds(2f);

        timeline?.Log("⚙️ System Stable");
    }
}
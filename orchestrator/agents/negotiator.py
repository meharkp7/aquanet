from typing import List, Dict, Any
import math

from orchestrator.memory.long_term import get_similar_cases
from orchestrator.rag_engine.retriever import get_infra_context
from groq import Groq
import os

api = os.getenv("GROQ_API_KEY")

client = Groq(api_key=api)

# =========================================================
# ⚙️ WEIGHT CONFIGURATION (TUNABLE SYSTEM BEHAVIOR)
# =========================================================
WEIGHTS = {
    "capacity": 0.35,
    "pressure": 0.20,
    "distance": 0.15,
    "priority": 0.15,
    "stability": 0.10,
    "historical_bias": 0.05
}


# =========================================================
# 🧠 NORMALIZATION UTILITIES
# =========================================================
def normalize(value, max_val=100):
    return min(value / max_val, 1)


# =========================================================
# 📊 DEMAND ESTIMATION ENGINE
# =========================================================
def estimate_demand(flow_rate: Dict[str, Any], infra: Dict[str, Any]) -> float:
    base_flow = flow_rate.get("flow", 100)

    # Surge factor based on critical zone
    surge_multiplier = 1.3 if infra.get("critical_zone") else 1.1

    return base_flow * surge_multiplier


# =========================================================
# 🧠 NODE SCORING FUNCTION (MULTI-OBJECTIVE)
# =========================================================
def score_node(node: Dict, priority_zones: List[str], historical_nodes: List[str]) -> float:

    if node.get("status") == "faulty":
        return -999  # hard reject

    capacity_score = normalize(node.get("available_capacity", 0))

    pressure = node.get("pressure", 0)
    pressure_score = 1 if 45 <= pressure <= 75 else 0.3

    distance_score = 1 / (1 + node.get("distance", 1))

    priority_score = 1 if node.get("zone") in priority_zones else 0.4

    stability_score = 1 if node.get("pressure_variance", 10) < 10 else 0.3

    historical_score = 1 if node.get("id") in historical_nodes else 0.5

    score = (
        WEIGHTS["capacity"] * capacity_score +
        WEIGHTS["pressure"] * pressure_score +
        WEIGHTS["distance"] * distance_score +
        WEIGHTS["priority"] * priority_score +
        WEIGHTS["stability"] * stability_score +
        WEIGHTS["historical_bias"] * historical_score
    )

    return score


# =========================================================
# 🔁 FALLBACK STRATEGY ENGINE
# =========================================================
def fallback_strategy(scored_nodes: List[Dict]) -> List[Dict]:
    # Choose closest + stable nodes
    return sorted(scored_nodes, key=lambda x: x["distance"])[:2]


# =========================================================
# 🤖 LLM REASONING (EXPLAINABILITY + META ANALYSIS)
# =========================================================
def llm_reasoning(context: dict) -> str:
    try:
        prompt = f"""
You are an advanced AI managing a smart city water infrastructure system.

A leak has occurred and the system must reroute water efficiently and safely.

--- SYSTEM CONTEXT ---
{context}

--- YOUR TASK ---
Explain the routing decision using engineering reasoning.

Consider:
1. Why the selected nodes are optimal
2. How pressure stability was ensured
3. How load balancing was achieved
4. How distance impacts efficiency
5. Any risks (like pressure drops or instability)

--- OUTPUT STYLE ---
- Be concise but technical
- Sound like an infrastructure engineer
- Avoid generic explanations
"""

        response = client.chat.completions.create(
            model="llama-3.1-8b-instant",
            messages=[{"role": "user", "content": prompt}]
        )

        return response.choices[0].message.content

    except Exception as e:
        return f"""
Fallback reasoning:
Routing selected based on:
- Highest available capacity
- Stable pressure nodes
- Minimum distance routing
- Priority zones (critical infrastructure)

Error: {str(e)}
"""

# =========================================================
# 🚀 MAIN NEGOTIATION FUNCTION
# =========================================================
def negotiate_flow(
    leak_location: str,
    neighbors: List[Dict],
    priority_zones: List[str],
    flow_rate: Dict
) -> Dict:

    if not neighbors:
        return {"status": "failed", "reason": "No neighbors available"}

    # -------------------------
    # 🧠 INFRA CONTEXT (RAG)
    # -------------------------
    infra = get_infra_context(leak_location)

    if infra.get("critical_zone"):
        priority_zones.append(leak_location)

    # -------------------------
    # 🧠 MEMORY CONTEXT
    # -------------------------
    history = get_similar_cases(leak_location)
    historical_nodes = []

    for case in history:
        historical_nodes.extend(case.get("nodes_used", []))

    # -------------------------
    # 📊 DEMAND ESTIMATION
    # -------------------------
    demand = estimate_demand(flow_rate, infra)

    # -------------------------
    # 🧠 NODE SCORING
    # -------------------------
    scored_nodes = []

    for node in neighbors:
        score = score_node(node, priority_zones, historical_nodes)

        scored_nodes.append({
            "node_id": node["id"],
            "score": score,
            "capacity": node["available_capacity"],
            "distance": node["distance"],
            "zone": node["zone"]
        })

    scored_nodes.sort(key=lambda x: x["score"], reverse=True)

    # -------------------------
    # ⚙️ NODE SELECTION
    # -------------------------
    selected_nodes = []
    total_capacity = 0

    for node in scored_nodes:
        if total_capacity >= demand:
            break

        selected_nodes.append(node)
        total_capacity += node["capacity"]

    # -------------------------
    # 🔁 FALLBACK
    # -------------------------
    if total_capacity < demand:
        selected_nodes = fallback_strategy(scored_nodes)

    reroute_path = [leak_location] + [n["node_id"] for n in selected_nodes]

    # -------------------------
    # 🤖 LLM CONTEXT
    # -------------------------
    context = {
        "leak_location": leak_location,
        "demand": demand,
        "selected_nodes": selected_nodes,
        "infra": infra
    }

    reasoning = llm_reasoning(context)

    # -------------------------
    # 📦 FINAL OUTPUT
    # -------------------------
    return {
        "status": "success",
        "selected_nodes": [n["node_id"] for n in selected_nodes],
        "total_capacity": total_capacity,
        "demand": demand,
        "reroute_path": reroute_path,
        "llm_reasoning": reasoning,
        "scored_nodes": scored_nodes[:5]  # top candidates
    }
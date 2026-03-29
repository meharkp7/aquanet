import json
import os
from typing import List, Dict, Any
from datetime import datetime


# =========================================================
# ⚙️ STORAGE CONFIG
# =========================================================
MEMORY_FILE = "orchestrator/memory/memory_store.json"
MAX_MEMORY_SIZE = 1000


# =========================================================
# 📂 LOAD MEMORY
# =========================================================
def load_memory() -> List[Dict[str, Any]]:
    if not os.path.exists(MEMORY_FILE):
        return []

    try:
        with open(MEMORY_FILE, "r") as f:
            return json.load(f)
    except:
        return []


# =========================================================
# 💾 SAVE MEMORY
# =========================================================
def save_memory(memory: List[Dict[str, Any]]):

    # Limit size (prevent explosion)
    if len(memory) > MAX_MEMORY_SIZE:
        memory = memory[-MAX_MEMORY_SIZE:]

    with open(MEMORY_FILE, "w") as f:
        json.dump(memory, f, indent=2)


# =========================================================
# 🧠 STORE INCIDENT (CORE FUNCTION)
# =========================================================
def store_incident(state: Dict[str, Any]):

    memory = load_memory()

    incident = {
        "timestamp": str(datetime.now()),
        "location": state.get("leak_location"),
        "risk_score": state.get("risk_score"),
        "nodes_used": state.get("valves_operated", []),
        "demand": state.get("reroute_plan", {}).get("demand"),
        "capacity": state.get("reroute_plan", {}).get("total_capacity"),
        "success": state.get("repair_status") == "valves_adjusted",
        "retry_count": state.get("retry_count", 0),
        "simulation": state.get("simulation", {})
    }

    memory.append(incident)

    save_memory(memory)


# =========================================================
# 🔍 RETRIEVE SIMILAR CASES
# =========================================================
def get_similar_cases(location: str) -> List[Dict[str, Any]]:

    memory = load_memory()

    similar = [
        m for m in memory
        if m.get("location") == location
    ]

    # Sort by most recent
    similar.sort(key=lambda x: x["timestamp"], reverse=True)

    return similar[:10]


# =========================================================
# 📊 SUCCESS PATTERN EXTRACTION
# =========================================================
def get_successful_nodes(location: str) -> List[str]:

    cases = get_similar_cases(location)

    node_frequency = {}

    for case in cases:
        if case.get("success"):
            for node in case.get("nodes_used", []):
                node_frequency[node] = node_frequency.get(node, 0) + 1

    # Sort by frequency
    sorted_nodes = sorted(
        node_frequency.items(),
        key=lambda x: x[1],
        reverse=True
    )

    return [node for node, _ in sorted_nodes]


# =========================================================
# ⚠️ FAILURE ANALYSIS
# =========================================================
def get_failure_patterns(location: str) -> Dict[str, Any]:

    cases = get_similar_cases(location)

    failures = [c for c in cases if not c.get("success")]

    if not failures:
        return {}

    avg_risk = sum(c["risk_score"] for c in failures) / len(failures)

    high_retry_cases = [c for c in failures if c.get("retry_count", 0) > 2]

    return {
        "failure_count": len(failures),
        "avg_risk": avg_risk,
        "high_retry_cases": len(high_retry_cases)
    }
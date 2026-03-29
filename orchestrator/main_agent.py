from typing import Dict, Any
import pprint

from agents.planner import plan_step
from graph.graph import app

from tools.sensor_tools import get_pressure_data, get_flow_rate
from tools.network_tools import get_neighbors

from memory.long_term import load_memory


# =========================================================
# 🧠 INITIAL STATE BUILDER
# =========================================================
def build_initial_state() -> Dict[str, Any]:

    state = {
        # 🔴 Detection Layer
        "leak_detected": False,
        "leak_location": "Zone-12",

        "pressure_data": get_pressure_data(),
        "flow_rate": get_flow_rate(),

        # 🌐 Network Layer
        "neighbor_states": get_neighbors(),

        # 🧠 Decision Layer
        "reroute_plan": {},
        "priority_zones": ["hospital"],

        # 🛡️ Validation Layer
        "validated": False,
        "risk_score": 0.0,
        "validation_details": {},
        "simulation": {},

        # ⚙️ Execution Layer
        "repair_status": "",
        "valves_operated": [],

        # 🔁 Adaptive System
        "retry_count": 0,

        # 🧠 Memory
        "past_incidents": load_memory(),

        # 📊 Monitoring
        "system_alerts": [],
        "system_health": 100,

        # 🧾 Meta
        "system_time": "12:00"
    }

    return state


# =========================================================
# 🧠 EXECUTION PIPELINE
# =========================================================
def run_system():

    print("\n🚀 ===== AQUANET SYSTEM START =====")

    # -------------------------
    # STEP 1: INITIALIZE
    # -------------------------
    state = build_initial_state()

    print("\n📡 Sensor Input:")
    pprint.pprint(state["pressure_data"])

    print("\n🌐 Network Snapshot:")
    pprint.pprint(state["neighbor_states"])

    # -------------------------
    # STEP 2: LLM PLANNER
    # -------------------------
    decision = plan_step(state)

    print("\n🧠 Planner Decision:", decision["thought"])

    if decision["action"] != "run_pipeline":
        print("\n✅ No action required")
        return state

    # -------------------------
    # STEP 3: RUN GRAPH
    # -------------------------
    print("\n⚙️ Executing mitigation pipeline...")

    final_state = app.invoke(state)

    # -------------------------
    # STEP 4: FINAL OUTPUT
    # -------------------------
    print("\n🏁 ===== FINAL SYSTEM STATE =====")

    pprint.pprint({
        "leak_detected": final_state["leak_detected"],
        "location": final_state["leak_location"],
        "selected_nodes": final_state["reroute_plan"].get("selected_nodes"),
        "risk_score": final_state["risk_score"],
        "retry_count": final_state["retry_count"],
        "repair_status": final_state["repair_status"],
        "valves_operated": final_state["valves_operated"],
        "system_health": final_state.get("system_health"),
        "alerts": final_state.get("system_alerts"),
        "simulation": final_state.get("simulation"),
        "llm_reasoning": final_state["reroute_plan"].get("llm_reasoning")
    })

    print("\n🧠 Reasoning:\n")
    print(final_state["reroute_plan"].get("llm_reasoning"))

    print("\n🚀 ===== SYSTEM END =====")

    return final_state


# =========================================================
# 🚀 ENTRY POINT
# =========================================================
if __name__ == "__main__":
    run_system()
from langgraph.graph import StateGraph, END

from orchestrator.graph.state import AquaState

from orchestrator.agents.negotiator import negotiate_flow
from orchestrator.agents.validator import validate_plan
from orchestrator.agents.repair_dispatch import dispatch_repair
from orchestrator.agents.supervisor import supervise

from orchestrator.memory.long_term import store_incident

MAX_RETRIES = 3


# =========================================================
# 🔴 DETECT NODE
# =========================================================
def detect_node(state: AquaState):

    print("\n🔍 [DETECT] Checking system...")

    if state["pressure_data"].get("anomaly", False):
        state["leak_detected"] = True
        print(f"🚨 Leak detected at {state['leak_location']}")
    else:
        print("✅ System stable")

    return state


# =========================================================
# 🔵 NEGOTIATE NODE
# =========================================================
def negotiate_node(state: AquaState):

    print("\n🤝 [NEGOTIATE] Optimizing flow...")

    state["reroute_plan"] = negotiate_flow(
        leak_location=state["leak_location"],
        neighbors=state["neighbor_states"],
        priority_zones=state["priority_zones"],
        flow_rate=state["flow_rate"]
    )

    print("📊 Selected Nodes:", state["reroute_plan"]["selected_nodes"])
    print("🧠 Reasoning:", state["reroute_plan"]["llm_reasoning"])

    return state


# =========================================================
# 🟡 VALIDATE NODE
# =========================================================
def validate_node(state: AquaState):

    print("\n🛡️ [VALIDATE] Running simulation...")

    result = validate_plan(state["reroute_plan"])

    state["validated"] = result["is_safe"]
    state["risk_score"] = result["risk_score"]
    state["validation_details"] = result.get("details", {})
    state["simulation"] = result.get("simulation", {})

    print(f"📉 Risk Score: {state['risk_score']}")
    print("🧪 Simulation:", state["simulation"])

    if not state["validated"]:
        print("❌ Rejected:", result.get("reason"))

    return state


# =========================================================
# 🟢 ACTION NODE
# =========================================================
def act_node(state: AquaState):

    print("\n⚙️ [EXECUTE] Applying mitigation...")

    result = dispatch_repair(
        leak_location=state["leak_location"],
        plan=state["reroute_plan"]
    )

    state["repair_status"] = result["status"]
    state["valves_operated"] = result["valves"]

    print("🔧 Valves Operated:", state["valves_operated"])
    print("✅ Status:", state["repair_status"])

    # 📚 STORE MEMORY
    store_incident(state)

    # 🧠 SUPERVISOR CHECK
    state = supervise(state)

    return state


# =========================================================
# 🔁 ADAPTIVE ROUTER (CORE INTELLIGENCE)
# =========================================================
def route_after_validation(state: AquaState):

    retries = state.get("retry_count", 0)

    # 🟢 No leak
    if not state["leak_detected"]:
        return END

    # 🟢 Valid plan → execute
    if state["validated"]:
        return "act"

    # 🔁 Adaptive retry
    if retries < MAX_RETRIES:

        print(f"\n🔁 [RETRY] Attempt {retries + 1}")

        state["retry_count"] = retries + 1

        # -------------------------
        # 🧠 ADAPTIVE STRATEGY
        # -------------------------

        risk = state["risk_score"]

        # Reduce load if too risky
        if risk > 70:
            state["flow_rate"]["flow"] *= 0.8

        # Remove unstable nodes
        if risk > 50:
            state["neighbor_states"] = sorted(
                state["neighbor_states"],
                key=lambda x: x["pressure_variance"]
            )

        # Force diversification
        if risk > 60:
            state["neighbor_states"] = state["neighbor_states"][1:]

        print("⚙️ Adapted flow + topology")

        return "negotiate"

    # 🔴 FAIL SAFE
    print("\n🚫 Max retries reached → Escalation")

    state["repair_status"] = "failed"
    state["system_alerts"].append("Manual intervention required")

    return END


# =========================================================
# ⚙️ GRAPH CONSTRUCTION
# =========================================================
graph = StateGraph(AquaState)

graph.add_node("detect", detect_node)
graph.add_node("negotiate", negotiate_node)
graph.add_node("validate", validate_node)
graph.add_node("act", act_node)

graph.set_entry_point("detect")

graph.add_edge("detect", "negotiate")
graph.add_edge("negotiate", "validate")

graph.add_conditional_edges("validate", route_after_validation)

graph.add_edge("act", END)

app = graph.compile()
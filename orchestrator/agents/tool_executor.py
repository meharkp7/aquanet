from tools.network_tools import get_neighbors
from agents.validator import validate_plan
from tools.actuator_tools import operate_valves


def execute_tool(action: str, action_input: dict, state: dict):
    if action == "get_neighbors":
        state["neighbor_states"] = get_neighbors()

    elif action == "validate_plan":
        result = validate_plan(state["reroute_plan"])
        state["validated"] = result["is_safe"]
        state["risk_score"] = result["risk_score"]

    elif action == "operate_valves":
        result = operate_valves(state["reroute_plan"].get("selected_nodes", []))
        state["repair_status"] = result["status"]

    return state
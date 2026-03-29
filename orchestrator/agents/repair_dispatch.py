from orchestrator.tools.actuator_tools import operate_valves

def dispatch_repair(leak_location: str, plan: dict):
    result = operate_valves(plan.get("selected_nodes", []))

    return {
        "status": result["status"],
        "valves": result["valves"]
    }
def plan_step(state: dict):
    if state["pressure_data"].get("anomaly"):
        return {"thought": "Leak detected → run pipeline", "action": "run_pipeline"}

    return {"thought": "System stable", "action": "none"}
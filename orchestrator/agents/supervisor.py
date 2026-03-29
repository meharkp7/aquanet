from typing import Dict, Any, List


# =========================================================
# ⚙️ THRESHOLDS
# =========================================================
HIGH_RISK_THRESHOLD = 80
CRITICAL_RISK_THRESHOLD = 95
MAX_RETRIES = 3


# =========================================================
# 🧠 SUPERVISOR ENGINE
# =========================================================
def supervise(state: Dict[str, Any]) -> Dict[str, Any]:

    alerts: List[str] = []

    risk = state.get("risk_score", 0)
    retries = state.get("retry_count", 0)
    valves = state.get("valves_operated", [])
    validated = state.get("validated", False)
    simulation = state.get("simulation", {})

    # -------------------------
    # 🔴 CRITICAL RISK CHECK
    # -------------------------
    if risk >= CRITICAL_RISK_THRESHOLD:
        alerts.append("🚨 CRITICAL: System instability extremely high")

    elif risk >= HIGH_RISK_THRESHOLD:
        alerts.append("⚠️ WARNING: High risk mitigation")

    # -------------------------
    # 🔁 RETRY ANOMALY
    # -------------------------
    if retries >= MAX_RETRIES:
        alerts.append("⚠️ Excessive retries → possible system inefficiency")

    # -------------------------
    # ⚙️ EXECUTION FAILURE
    # -------------------------
    if validated and not valves:
        alerts.append("⚠️ Plan validated but no valves executed")

    # -------------------------
    # ❌ VALIDATION FAILURE
    # -------------------------
    if not validated:
        alerts.append("❌ No safe plan found")

    # -------------------------
    # 🧪 SIMULATION CHECKS
    # -------------------------
    pressure_drop = simulation.get("pressure_drop", 0)

    if pressure_drop > 20:
        alerts.append("⚠️ High pressure drop detected")

    efficiency = simulation.get("efficiency", 100)

    if efficiency < 50:
        alerts.append("⚠️ Low system efficiency")

    # -------------------------
    # 📊 SYSTEM HEALTH SCORE
    # -------------------------
    health_score = compute_health_score(state)

    # -------------------------
    # 📦 UPDATE STATE
    # -------------------------
    state["system_alerts"] = alerts
    state["system_health"] = health_score

    # -------------------------
    # 🔥 ESCALATION LOGIC
    # -------------------------
    if health_score < 40:
        state["system_alerts"].append("🚨 ESCALATION: Manual intervention required")

    return state


# =========================================================
# 📊 HEALTH SCORE ENGINE
# =========================================================
def compute_health_score(state: Dict[str, Any]) -> float:

    risk = state.get("risk_score", 0)
    retries = state.get("retry_count", 0)
    simulation = state.get("simulation", {})

    efficiency = simulation.get("efficiency", 100)

    # Weighted scoring
    health = 100

    health -= risk * 0.5
    health -= retries * 10
    health -= (100 - efficiency) * 0.3

    return max(0, round(health, 2))
from typing import Dict, Any, Tuple
import math


# =========================================================
# ⚙️ CONSTANTS (REALISTIC SYSTEM LIMITS)
# =========================================================
SAFE_PRESSURE_MIN = 40
SAFE_PRESSURE_MAX = 80

WATER_HAMMER_THRESHOLD = 120
CRITICAL_RISK_THRESHOLD = 85
MAX_SAFE_RISK = 70


# =========================================================
# 🌊 WATER HAMMER DETECTION (PRESSURE SHOCK)
# =========================================================
def detect_water_hammer(plan: Dict[str, Any]) -> Dict[str, Any]:

    nodes = plan.get("selected_nodes", [])
    total_capacity = plan.get("total_capacity", 0)
    demand = plan.get("demand", 100)

    # sudden surge scenario
    surge_ratio = total_capacity / max(demand, 1)

    if len(nodes) <= 1 and surge_ratio > 1.2:
        return {
            "risk": True,
            "severity": "high",
            "reason": "Single node overload → pressure spike"
        }

    if total_capacity > WATER_HAMMER_THRESHOLD:
        return {
            "risk": True,
            "severity": "medium",
            "reason": "High flow surge detected"
        }

    return {"risk": False}


# =========================================================
# ⚖️ LOAD DISTRIBUTION ANALYSIS
# =========================================================
def load_imbalance(plan: Dict[str, Any]) -> float:

    nodes = plan.get("selected_nodes", [])
    total_capacity = plan.get("total_capacity", 0)

    if not nodes:
        return 100

    avg_load = total_capacity / len(nodes)

    # variance simulation
    imbalance_score = min(avg_load * 1.5, 100)

    return imbalance_score


# =========================================================
# 📉 PRESSURE STABILITY MODEL
# =========================================================
def pressure_instability(plan: Dict[str, Any]) -> float:

    path = plan.get("reroute_path", [])
    path_length = len(path)

    instability = path_length * 5

    if path_length > 5:
        instability += 20

    return instability


# =========================================================
# 🔁 FLOW TURBULENCE MODEL
# =========================================================
def turbulence_score(plan: Dict[str, Any]) -> float:

    nodes = plan.get("selected_nodes", [])

    if len(nodes) <= 1:
        return 40

    return max(5, 25 - len(nodes) * 4)


# =========================================================
# 🧪 DIGITAL TWIN SIMULATION
# =========================================================
def simulate_flow(plan: Dict[str, Any]) -> Dict[str, Any]:

    path_length = len(plan.get("reroute_path", []))
    total_capacity = plan.get("total_capacity", 0)

    pressure_drop = path_length * 2.5
    delay = path_length * 0.7

    efficiency = max(0, 100 - pressure_drop - delay)

    return {
        "pressure_drop": pressure_drop,
        "delay": delay,
        "efficiency": efficiency
    }


# =========================================================
# 🧠 COMPOSITE RISK ENGINE
# =========================================================
def compute_risk(plan: Dict[str, Any]) -> Tuple[float, Dict[str, Any]]:

    hammer = detect_water_hammer(plan)
    imbalance = load_imbalance(plan)
    pressure = pressure_instability(plan)
    turbulence = turbulence_score(plan)

    total_risk = (
        0.35 * imbalance +
        0.30 * pressure +
        0.20 * turbulence +
        (50 if hammer.get("risk") else 0)
    )

    breakdown = {
        "water_hammer": hammer,
        "load_imbalance": imbalance,
        "pressure_instability": pressure,
        "turbulence": turbulence
    }

    return total_risk, breakdown


# =========================================================
# 🚀 FINAL VALIDATION ENGINE
# =========================================================
def validate_plan(plan: Dict[str, Any]) -> Dict[str, Any]:

    if plan.get("status") != "success":
        return {
            "is_safe": False,
            "risk_score": 100,
            "reason": "Invalid negotiation output"
        }

    total_risk, breakdown = compute_risk(plan)
    simulation = simulate_flow(plan)

    # -------------------------
    # 🔴 HARD FAILURE CONDITIONS
    # -------------------------
    if breakdown["water_hammer"].get("risk"):
        return {
            "is_safe": False,
            "risk_score": total_risk,
            "reason": breakdown["water_hammer"]["reason"],
            "details": breakdown,
            "simulation": simulation
        }

    if total_risk > CRITICAL_RISK_THRESHOLD:
        return {
            "is_safe": False,
            "risk_score": total_risk,
            "reason": "Critical instability detected",
            "details": breakdown,
            "simulation": simulation
        }

    if total_risk > MAX_SAFE_RISK:
        return {
            "is_safe": False,
            "risk_score": total_risk,
            "reason": "Risk exceeds safe threshold",
            "details": breakdown,
            "simulation": simulation
        }

    # -------------------------
    # 🟢 SAFE EXECUTION
    # -------------------------
    return {
        "is_safe": True,
        "risk_score": total_risk,
        "details": breakdown,
        "simulation": simulation
    }
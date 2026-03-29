from typing import Dict, Any


# =========================================================
# 🗺️ MOCK INFRA DATABASE (CAN BE REPLACED WITH REAL RAG)
# =========================================================
INFRA_DB = {
    "Zone-12": {
        "pipe_type": "steel",
        "diameter": 250,
        "max_pressure": 85,
        "critical_zone": True,
        "priority": "hospital",
        "age": 12,
        "failure_rate": 0.2
    },
    "Zone-9": {
        "pipe_type": "PVC",
        "diameter": 150,
        "max_pressure": 70,
        "critical_zone": False,
        "priority": "residential",
        "age": 20,
        "failure_rate": 0.5
    },
    "Zone-5": {
        "pipe_type": "cast_iron",
        "diameter": 200,
        "max_pressure": 75,
        "critical_zone": False,
        "priority": "industrial",
        "age": 30,
        "failure_rate": 0.7
    }
}


# =========================================================
# 🔍 RETRIEVE INFRASTRUCTURE CONTEXT
# =========================================================
def get_infra_context(location: str) -> Dict[str, Any]:

    # Default fallback (unknown region)
    default = {
        "pipe_type": "unknown",
        "diameter": 180,
        "max_pressure": 75,
        "critical_zone": False,
        "priority": "residential",
        "age": 15,
        "failure_rate": 0.4
    }

    return INFRA_DB.get(location, default)


# =========================================================
# ⚙️ DERIVED RISK FACTORS FROM INFRA
# =========================================================
def compute_infra_risk(infra: Dict[str, Any]) -> float:

    risk = 0

    # Older pipes → higher risk
    age = infra.get("age", 10)
    risk += min(age * 1.5, 30)

    # High failure rate → higher risk
    failure_rate = infra.get("failure_rate", 0.3)
    risk += failure_rate * 50

    # Weak materials
    if infra.get("pipe_type") in ["cast_iron"]:
        risk += 20

    return risk


# =========================================================
# 🎯 PRIORITY ZONE LOGIC
# =========================================================
def get_priority_boost(infra: Dict[str, Any]) -> float:

    if infra.get("critical_zone"):
        return 1.5

    if infra.get("priority") == "hospital":
        return 2.0

    if infra.get("priority") == "industrial":
        return 1.2

    return 1.0


# =========================================================
# 🧠 FULL CONTEXT PACKAGER
# =========================================================
def get_full_context(location: str) -> Dict[str, Any]:

    infra = get_infra_context(location)

    return {
        "infra": infra,
        "infra_risk": compute_infra_risk(infra),
        "priority_boost": get_priority_boost(infra)
    }
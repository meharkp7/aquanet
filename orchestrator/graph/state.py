from typing import TypedDict, List, Dict, Optional, Any


class AquaState(TypedDict):
    """
    CENTRAL SHARED STATE ACROSS THE ENTIRE SYSTEM

    This acts as:
    - Memory bus
    - Communication layer between agents
    - Source of truth for decision making
    """

    leak_detected: bool
    leak_location: Optional[str]

    pressure_data: Dict[str, Any]   # {pressure, anomaly, variance}
    flow_rate: Dict[str, Any]       # {flow, trend}

    neighbor_states: List[Dict[str, Any]]
    """
    Each neighbor node contains:
    {
        id: str
        available_capacity: float
        pressure: float
        distance: float
        zone: str
        status: str (active/faulty)
        pressure_variance: float
    }
    """

    reroute_plan: Dict[str, Any]
    """
    {
        status: success/failed
        selected_nodes: List[str]
        total_capacity: float
        demand: float
        reroute_path: List[str]
        llm_reasoning: str
    }
    """

    priority_zones: List[str]
    validated: bool
    risk_score: float
    validation_details: Dict[str, Any]
    simulation: Dict[str, Any]
    repair_status: str
    valves_operated: List[str]
    retry_count: int
    past_incidents: List[Dict[str, Any]]
    system_alerts: List[str]
    system_time: str
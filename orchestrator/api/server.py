from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from orchestrator.graph.graph import app as graph_app
from orchestrator.tools.sensor_tools import get_pressure_data, get_flow_rate
from orchestrator.tools.network_tools import get_neighbors

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/run")
def run_system():

    state = {
        "leak_detected": False,
        "leak_location": "Zone-12",

        "pressure_data": get_pressure_data(),
        "flow_rate": get_flow_rate(),

        "neighbor_states": get_neighbors(),

        "reroute_plan": {},
        "priority_zones": ["hospital"],

        "validated": False,
        "risk_score": 0.0,

        "repair_status": "",
        "valves_operated": [],

        "retry_count": 0,
        "past_incidents": [],
        "system_time": "12:00"
    }

    result = graph_app.invoke(state)

    nodes = [
        {
            "id": n["id"],
            "pressure": n.get("pressure", 50)
        }
        for n in result.get("neighbor_states", [])
    ]

    edges = []
    for i in range(len(nodes) - 1):
        edges.append({
            "source": nodes[i]["id"],
            "target": nodes[i + 1]["id"]
        })

    return {
        "risk_score": result.get("risk_score", 0),
        "retries": result.get("retry_count", 0),
        "leak_node": nodes[0]["id"] if nodes else "Node-0",

        "nodes": nodes,
        "edges": edges,

        "reasoning": result.get("reasoning", "Fallback reasoning")
    }
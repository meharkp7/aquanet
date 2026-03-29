import random

def get_neighbors():
    return [
        {
            "id": f"Node-{i}",
            "available_capacity": random.randint(20, 80),
            "pressure": random.randint(30, 100),
            "distance": random.uniform(1, 10),
            "zone": random.choice(["hospital", "residential"]),
            "status": random.choice(["active", "active", "faulty"]),
            "pressure_variance": random.uniform(1, 15)
        }
        for i in range(5)
    ]
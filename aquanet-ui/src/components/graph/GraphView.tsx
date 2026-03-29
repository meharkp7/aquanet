"use client";

import ReactFlow, { Background } from "reactflow";
import "reactflow/dist/style.css";
import { motion } from "framer-motion";

const getColor = (p: number) => {
  if (p > 80) return "#ff3b3b";
  if (p > 50) return "#f59e0b";
  return "#00d4ff";
};

export default function GraphView({ data }: { data: any }) {

  // 🛑 SAFETY CHECK (CRITICAL)
  if (!data || !data.nodes || !data.edges) {
    return (
      <div className="h-full flex items-center justify-center text-gray-500">
        Waiting for backend...
      </div>
    );
  }

  const leak = data.leak_node;

  const nodes = data.nodes.map((n: any, i: number) => ({
    id: n.id,
    position: {
      x: 150 + i * 250,
      y: i % 2 === 0 ? 200 : 100,
    },
    data: {
      label: (
        <motion.div
          animate={{
            scale: leak === n.id ? [1, 1.6, 1] : [1, 1.2, 1],
          }}
          transition={{ repeat: Infinity, duration: 2 }}
          className="w-12 h-12 rounded-full flex items-center justify-center text-white text-xs font-bold"
          style={{
            background:
              leak === n.id ? "#ff0000" : getColor(n.pressure),
            boxShadow: `0 0 20px ${getColor(n.pressure)}`,
          }}
        >
          {n.id}
        </motion.div>
      ),
    },
  }));

  const edges = data.edges.map((e: any, i: number) => ({
    id: "e" + i,
    source: e.source,
    target: e.target,
    animated: true,
  }));

  return (
    <div className="h-full">
      <ReactFlow nodes={nodes} edges={edges} fitView>
        <Background />
      </ReactFlow>
    </div>
  );
}
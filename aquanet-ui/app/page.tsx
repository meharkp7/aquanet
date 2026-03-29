"use client";

import { useEffect } from "react";
import { fetchSystem } from "../src/lib/api";
import { useSystemStore } from "../src/store/useSystemStore";

import GraphView from "../src/components/graph/GraphView";
import ThreeScene from "../src/components/three/ThreeScene";
import GlassCard from "../src/components/ui/GlassCard";

export default function Page() {
  const { data, setData } = useSystemStore();

  useEffect(() => {
    const interval = setInterval(async () => {
      const res = await fetchSystem();
      setData(res);
    }, 1500);

    return () => clearInterval(interval);
  }, []);

  return (
    <div className="h-screen bg-black text-white flex flex-col">

      {/* TOP BAR */}
      <div className="flex gap-4 p-4">
        <GlassCard>Risk: {(data?.risk_score * 100).toFixed(1)}%</GlassCard>
        <GlassCard>Retries: {data?.retries ?? "--"}</GlassCard>
        <GlassCard>Status: {data ? "ACTIVE" : "IDLE"}</GlassCard>
      </div>

      {/* MAIN */}
      <div className="flex flex-1 gap-4 p-4">

        {/* LEFT */}
        <div className="w-1/4">
          <GlassCard>Timeline</GlassCard>
        </div>

        {/* CENTER */}
        <div className="flex-1">
          <GraphView data={data} />
        </div>

        {/* RIGHT (3D) */}
        <div className="w-1/3">
          <ThreeScene />
        </div>
      </div>

      {/* AI PANEL */}
      <div className="p-4">
        <GlassCard>AI Reasoning Engine Active</GlassCard>
      </div>

    </div>
  );
}
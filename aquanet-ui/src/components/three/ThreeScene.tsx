"use client";

import { Canvas } from "@react-three/fiber";
import { OrbitControls } from "@react-three/drei";
import { useFrame } from "@react-three/fiber";
import { useRef } from "react";

function Node({ position, pressure }: any) {
  const ref = useRef<any>(null);

  useFrame(() => {
    ref.current.scale.x = 1 + Math.sin(Date.now() * 0.002) * 0.2;
    ref.current.scale.y = ref.current.scale.x;
    ref.current.scale.z = ref.current.scale.x;
  });

  const color =
    pressure > 80 ? "red" : pressure > 50 ? "orange" : "cyan";

  return (
    <mesh ref={ref} position={position}>
      <sphereGeometry args={[0.3, 32, 32]} />
      <meshStandardMaterial color={color} emissive={color} />
    </mesh>
  );
}

export default function ThreeScene() {
  const nodes = [
    { pos: [0, 0, 0], pressure: 30 },
    { pos: [2, 1, 0], pressure: 80 },
    { pos: [4, 0, 0], pressure: 60 },
  ];

  return (
    <Canvas camera={{ position: [5, 5, 10] }}>
      <ambientLight />
      <pointLight position={[10, 10, 10]} />

      {nodes.map((n, i) => (
        <Node key={i} position={n.pos} pressure={n.pressure} />
      ))}

      <OrbitControls />
    </Canvas>
  );
}
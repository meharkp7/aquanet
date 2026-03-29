import { create } from "zustand";

export const useSystemStore = create((set) => ({
  data: null,
  setData: (data) => set({ data }),
}));
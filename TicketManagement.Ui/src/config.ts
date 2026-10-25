// Set VITE_API_URL at build time to point the UI at a different API (see the UI Dockerfile).
export const API_URL: string =
  import.meta.env.VITE_API_URL ?? "http://localhost:5186";

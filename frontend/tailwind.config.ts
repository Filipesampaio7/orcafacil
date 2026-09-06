import type { Config } from "tailwindcss";

const config: Config = {
  darkMode: "class",
  content: ["./app/**/*.{ts,tsx}", "./components/**/*.{ts,tsx}"],
  theme: {
    extend: {
      colors: {
        bg: "hsl(var(--bg))",
        surface: "hsl(var(--surface))",
        "surface-2": "hsl(var(--surface-2))",
        border: "hsl(var(--border))",
        text: "hsl(var(--text))",
        "text-muted": "hsl(var(--text-muted))",
        accent: "hsl(var(--accent))",
        "accent-soft": "hsl(var(--accent-soft))",
      },
      fontFamily: {
        display: ["var(--font-display)", "sans-serif"],
        body: ["var(--font-body)", "sans-serif"],
      },
      maxWidth: { content: "1320px" },
    },
  },
  plugins: [],
};

export default config;

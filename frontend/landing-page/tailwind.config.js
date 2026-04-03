import { addDynamicIconSelectors } from '@iconify/tailwind';

/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Outfit', 'sans-serif'],
        bebas: ['"Bebas Neue"', 'cursive'],
        mono: ['"JetBrains Mono"', 'monospace'],
      },
      colors: {
        slate: { 800: '#1e293b', 900: '#0f172a' },
        emerald: { 400: '#34d399', 500: '#10b981', 600: '#059669' },
        blue: { 
          900: '#1e3a8a',
          950: '#0f172a',
          600: '#2563eb'
        },
        brand: {
          dark: '#0f172a',
          header: '#080e1a',
          surface: '#0d1b2e',
          primary: '#2563eb',
          secondary: '#dc2626',
          accent: '#60a5fa',
          success: '#4ade80',
          muted: '#64748b'
        }
      },
      animation: {
        'marquee': 'marquee 30s linear infinite',
      },
      keyframes: {
        marquee: {
          '0%': { transform: 'translateX(0)' },
          '100%': { transform: 'translateX(-50%)' },
        }
      }
    },
  },
  plugins: [
    addDynamicIconSelectors(),
  ],
}
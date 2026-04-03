/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}"
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Outfit', 'sans-serif'],
        bebas: ['"Bebas Neue"', 'cursive'],
        mono: ['"JetBrains Mono"', 'monospace'],
      },
      colors: {
        pos: {
          surface: '#1A2235',
          border: '#2A3548',
          accent: '#3B82F6',
        },
        brand: {
          dark: '#0f172a',
          header: '#080e1a',
          surface: '#0d1b2e',
          primary: '#2563eb',
          secondary: '#1e3a8a',
          accent: '#60a5fa',
          success: '#4ade80',
          muted: '#64748b'
        }
      },
      animation: {
        'marquee': 'marquee 25s linear infinite',
        'fade-in-up': 'fadeInUp 0.5s ease-out forwards',
        'counter': 'counter 1.5s ease-out forwards',
      },
      keyframes: {
        marquee: {
          '0%': { transform: 'translateX(0%)' },
          '100%': { transform: 'translateX(-100%)' },
        },
        fadeInUp: {
          '0%': { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        counter: {
          'from': { opacity: '0', transform: 'translateY(10px)' },
          'to': { opacity: '1', transform: 'translateY(0)' },
        }
      }
    },
  },
  plugins: [
    require('daisyui'),
    require('@iconify/tailwind').addDynamicIconSelectors(),
    require('tailwindcss-animate'),
  ],
  daisyui: {
    themes: [
      {
        tucolmadolight: {
          "primary": "#2563eb",
          "secondary": "#dc2626",
          "accent": "#78350f",
          "neutral": "#e2e8f0",
          "base-100": "#f8fafc",
          "base-200": "#e2e8f0",
          "base-300": "#cbd5e1",
          "base-content": "#0f172a",
          "info": "#0284c7",
          "success": "#16a34a",
          "warning": "#ca8a04",
          "error": "#dc2626",
        }
      },
      {
        tucolmadodark: {
          "primary": "#2563eb",
          "secondary": "#dc2626",
          "accent": "#78350f",
          "neutral": "#1e293b",
          "base-100": "#0f172a",
          "base-200": "#1e293b",
          "base-300": "#334155",
          "base-content": "#f8fafc",
          "info": "#0284c7",
          "success": "#166534",
          "warning": "#d97706",
          "error": "#dc2626",
        }
      }
    ],
  }
}


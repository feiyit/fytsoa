/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#409EFF',
          50: '#ecf5ff',
          100: '#d9ecff',
          200: '#c0ddff',
          300: '#a6cfff',
          400: '#7eb5ff',
          500: '#409EFF',
          600: '#3a8ee6',
          700: '#337ecc',
          800: '#2b6cb0',
          900: '#1d4f80',
        },
        slate: {
          750: "#36363a", 
        },
      },
    },
  },
  plugins: [],
}


/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{js,jsx,ts,tsx,html}'],
  corePlugins: {
    // ReactUnity is not a browser: preflight's browser-specific rules (e.g. `:-moz-*`) are dead
    // weight and log warnings for pseudo-classes ReactUnity does not know.
    preflight: false,
  },
  theme: {
    extend: {},
  },
  plugins: [],
};

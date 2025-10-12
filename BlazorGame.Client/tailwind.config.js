/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./**/*.{razor,html,cshtml}"],
  theme: {
    extend: {
      colors: {
        medieval: {
          primary: "#2C3E50",
          secondary: "#8B4513",
          accent: "#C19A6B",
          light: "#F5DEB3",
          dark: "#1a202c",
        },
      },
      fontFamily: {
        medieval: ["Cinzel", "serif"],
        text: ["Merriweather", "serif"],
      },
    },
  },
  plugins: [],
};

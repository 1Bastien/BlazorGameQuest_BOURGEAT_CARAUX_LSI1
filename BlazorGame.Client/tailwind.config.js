/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.{razor,html,cshtml}",
    "./Components/**/*.{razor,html,cshtml}",
    "./Layout/**/*.{razor,html,cshtml}",
    "./App.razor",
    "./wwwroot/index.html",
  ],
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

import { createTheme } from "@mui/material";

// Темна тема: mode:"dark" автоматично інвертує MUI-стандартні фони і текст.
// ThemeProvider в App.jsx підхоплює цю тему коли isDark === true
export const darkTheme = createTheme({
    palette: {
        mode: "dark",
        primary: {
            light: "#f5f5f5",
            main: "#9e9e9e",   // сірий AppBar у темному режимі
            dark: "#616161",
        },
        secondary: {
            main: "#CDDC39"    // lime — замінює coral в темному режимі
        },
    },
});
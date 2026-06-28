import { createTheme } from "@mui/material";

// Світла тема: червоний primary + coral secondary.
// ThemeProvider в App.jsx підхоплює цю тему за замовчування (isDark === false)
export const lightTheme = createTheme({
    palette: {
        primary: {
            light: "#ffcdd2",
            main: "#f44336",   // червоний — колір AppBar i кнопок
            dark: "#d32f2f",
        },
        secondary: {
            main: "#FF7F50"    // coral — для кнопки + в ManufacturesListPage
        }
    },
});

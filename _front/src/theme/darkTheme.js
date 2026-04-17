import { createTheme } from "@mui/material";

export const darkTheme = createTheme({
    palette: {
        mode: "dark",
        primary: {
            light: "#f5f5f5",
            main: "#9e9e9e",
            dark: "#616161",
        },
        secondary: {
            main: "#CDDC39"
        },
    },
});
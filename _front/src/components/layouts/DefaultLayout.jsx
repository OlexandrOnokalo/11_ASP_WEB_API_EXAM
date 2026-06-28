import { Box, Container } from "@mui/material";
import Footer from "../footer/Footer";
import Navbar from "../navbar/Navbar";
import { Outlet } from "react-router-dom";

// Отримую isDark/setIsDark з App.jsx — стан теми живе там, бо ThemeProvider теж там
const DefaultLayout = ({isDark, setIsDark}) => {
    return (
        <>
            <Navbar isDark={isDark} setIsDark={setIsDark}/>
            <Box
                sx={{
                    minHeight: "100vh",
                    // Фон міняється разом із темою — плавний перехід 0.2s
                    background: isDark
                        ? "radial-gradient(circle at 30% 25%, #0d2a57 0%, #061736 35%, #041126 100%)"
                        : "#f3f4f6",
                    transition: "background 0.2s ease",
                }}
            >
                <Container sx={{ minHeight: "100vh", pt: 2, pb: 3 }}>
                    {/* Сюди React Router підставляє активну дочірню сторінку */}
                    <Outlet />
                </Container>
            </Box>
            <Footer />
        </>
    );
};

export default DefaultLayout;

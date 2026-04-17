import "./App.css";
import CarListPage from "./pages/carsPage/CarListPage";
import ManufacturesListPage from "./pages/manufacturesPage/ManufacturesListPage";
import ManufacturesCreateForm from "./pages/manufacturesPage/ManufacturesCreateForm";
import ManufacturesUpdateForm from "./pages/manufacturesPage/ManufacturesUpdateForm";
import { Routes, Route } from "react-router-dom";
import CarCreateForm from "./pages/carsPage/CarCreateForm";
import NotFoundPage from "./pages/notFoundPage/NotFoundPage";
import MainPage from "./pages/mainPage/MainPage";
import DefaultLayout from "./components/layouts/DefaultLayout";
import CarUpdateForm from "./pages/carsPage/CarUpdateForm";
import CarDetailsPage from "./pages/carsPage/CarDetailsPage";
import LoginPage from "./pages/auth/loginPage/LoginPage";
import { useEffect, useState } from "react";
import { useAuth } from "./context/AuthContext";
import { ThemeProvider } from "@mui/material";
import { lightTheme } from "./theme/lightTheme";
import { darkTheme } from "./theme/darkTheme";
import RegisterPage from "./pages/auth/registerPage/RegisterPage";

function App() {
    const { isAuth, login, user } = useAuth();

    useEffect(() => {
        const authData = localStorage.getItem("auth");
        if (authData) {
            login();
        }
    }, []);

    const [isDark, setIsDark] = useState(false);

    return (
        <>
            <ThemeProvider theme={isDark ? darkTheme : lightTheme}>
                <Routes>
                    <Route
                        path="/"
                        element={
                            <DefaultLayout
                                setIsDark={setIsDark}
                                isDark={isDark}
                            />
                        }
                    >
                        <Route index element={<MainPage />} />

                        <Route path="cars">
                            <Route index element={<CarListPage />} />
                            <Route path=":id" element={<CarDetailsPage />} />
                            {isAuth && user.role === "admin" && (
                                <>
                                    <Route
                                        path="create"
                                        element={<CarCreateForm />}
                                    />
                                    <Route
                                        path="update/:id"
                                        element={<CarUpdateForm />}
                                    />
                                </>
                            )}
                        </Route>

                        <Route path="Manufactures">
                            <Route index element={<ManufacturesListPage />} />
                            {isAuth && user.role === "admin" && (
                                <>
                                    <Route
                                        path="create"
                                        element={<ManufacturesCreateForm />}
                                    />
                                    <Route
                                        path="update/:id"
                                        element={<ManufacturesUpdateForm />}
                                    />
                                </>
                            )}
                        </Route>

                        {!isAuth && (
                            <>
                                <Route path="login" element={<LoginPage />} />
                                <Route
                                    path="register"
                                    element={<RegisterPage />}
                                />
                            </>
                        )}

                        <Route path="*" element={<NotFoundPage />} />
                    </Route>
                </Routes>
            </ThemeProvider>
        </>
    );
}

export default App;

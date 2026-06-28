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
import { useState } from "react";
import { useAuth } from "./context/AuthContext";
import { ThemeProvider } from "@mui/material";
import { lightTheme } from "./theme/lightTheme";
import { darkTheme } from "./theme/darkTheme";
import RegisterPage from "./pages/auth/registerPage/RegisterPage";

function App() {
    const { isAuth, isAdmin, isHydrated } = useAuth();

    // isDark тут а не в Navbar, бо ThemeProvider теж тут — він обгортає всі маршрути
    const [isDark, setIsDark] = useState(false);

    // Чекаю поки AuthContext прочитає localStorage — інакше Admin-маршрути
    // не зареєструються при першому рендері і прямий перехід дасть 404
    if (!isHydrated) {
        return null;
    }

    return (
        <>
            <ThemeProvider theme={isDark ? darkTheme : lightTheme}>
                <Routes>
                    {/* DefaultLayout — спільна обгортка (Navbar+Footer) для всіх сторінок */}
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
                            {/* Guard: якщо умова false — маршрут взагалі не реєструється.
                                При спробі зайти — відпрацює * і покаже NotFoundPage */}
                            {isAuth && isAdmin && (
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
                            {/* Той самий guard що і для cars */}
                            {isAuth && isAdmin && (
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

                        {/* Ховаю login/register якщо вже авторизований — щоб не було
                            безглуздого повернення на форму входу будучи в системі */}
                        {!isAuth && (
                            <>
                                <Route path="login" element={<LoginPage />} />
                                <Route
                                    path="register"
                                    element={<RegisterPage />}
                                />
                            </>
                        )}

                        {/* Catch-all — будь-який невідомий або закритий маршрут */}
                        <Route path="*" element={<NotFoundPage />} />
                    </Route>
                </Routes>
            </ThemeProvider>
        </>
    );
}

export default App;

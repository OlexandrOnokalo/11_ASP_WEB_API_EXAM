import { createContext, useContext, useEffect, useMemo, useState } from "react";
// Тут використовую naked axios (не api-інстанс), щоб логін/реєстрація
// не потрапили в interceptor і не спровокували нескінченний цикл
import axios from "axios";
import { setUnauthorizedHandler } from "../api";
import {
    clearAuthSession,
    getAuthUser,
    hasAuthSession,
    setAuthSession,
} from "../services/authStorage";

export const AuthContext = createContext(null);

// Зручний хук — щоб у компонентах писати useAuth() замість useContext(AuthContext)
export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({children}) => {
    const baseApiUrl = import.meta.env.VITE_BASE_API_URL || "https://localhost:7178/api/";
    const [isAuth, setIsAuth] = useState(false);
    const [user, setUser] = useState(null);
    // isHydrated — захист від FOUC: App.jsx рендерить null поки цей прапор false,
    // щоб Admin-маршрути не мигнули як "не знайдені" до читання localStorage
    const [isHydrated, setIsHydrated] = useState(false);

    // Відновлюю сесію з localStorage при першому монтуванні.
    // [] — спрацьовує один раз, більше не потрібно
    useEffect(() => {
        if (hasAuthSession()) {
            const savedUser = getAuthUser();
            setUser(savedUser);
            setIsAuth(Boolean(savedUser));
        }
        // Завжди виставляю true — навіть якщо сесії немає,
        // щоб App.jsx розблокував рендер
        setIsHydrated(true);
    }, []);

    // Реєструю callback в api.js щоб він міг викликати logout при провалі рефрешу.
    // api.js не знає про React — тому зв'язок через цей handler
    useEffect(() => {
        setUnauthorizedHandler(() => {
            clearAuthSession();
            setIsAuth(false);
            setUser(null);
            // window.location замість navigate, бо цей код викликається з api.js
            // поза React-деревом і хук useNavigate там недоступний
            window.location.href = "/login";
        });
    }, []);

    // Внутрішній метод — тільки оновлює стан після того як сесія вже збережена
    function loginWithSession(authUser) {
        setIsAuth(true);
        setUser(authUser);
    }

    function logout() {
        clearAuthSession();
        setIsAuth(false);
        setUser(null);
    }

    async function loginRequest(credentials) {
        try {
            // Іду напряму axios щоб не спіймати власний 401-interceptor
            const response = await axios.post(`${baseApiUrl}auth/login`, credentials);
            const payload = response?.data?.data;

            // Перевіряю що сервер дав усе потрібне — якщо щось відсутнє,
            // краще повернути failure ніж зберегти половину і потім ламатися
            if (!payload?.tokens?.accessToken || !payload?.tokens?.refreshToken || !payload?.user) {
                return { success: false, message: "Некоректна відповідь сервера" };
            }

            setAuthSession({
                accessToken: payload.tokens.accessToken,
                refreshToken: payload.tokens.refreshToken,
                expiresAtUtc: payload.tokens.expiresAtUtc,
                user: payload.user,
            });

            loginWithSession(payload.user);
            return { success: true, data: payload };
        } catch (error) {
            // Беру повідомлення від бекенду якщо є, інакше — generic fallback
            return {
                success: false,
                message: error?.response?.data?.message || "Не вдалося виконати вхід",
            };
        }
    }

    async function registerRequest(registerData) {
        try {
            await axios.post(`${baseApiUrl}auth/register`, registerData);
            // Після реєстрації не логінюсь автоматично — юзер сам іде на /login
            return { success: true };
        } catch (error) {
            return {
                success: false,
                message: error?.response?.data?.message || "Не вдалося виконати реєстрацію",
            };
        }
    }

    // useMemo бо не хочу перераховувати при кожному рендері —
    // перераховую тільки коли змінився user
    const isAdmin = useMemo(() => {
        return Boolean(user?.roles?.includes("admin"));
    }, [user]);

    // Виставляю в value все що можуть потребувати компоненти через useAuth()
    return (
        <AuthContext.Provider value={{ isAuth, isHydrated, isAdmin, loginRequest, registerRequest, logout, user }}>
            {children}
        </AuthContext.Provider>
    )
}
import { createContext, useContext, useEffect, useMemo, useState } from "react";
import axios from "axios";
import { setUnauthorizedHandler } from "../api";
import {
    clearAuthSession,
    getAuthUser,
    hasAuthSession,
    setAuthSession,
} from "../services/authStorage";

export const AuthContext = createContext(null);


export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({children}) => {
    const baseApiUrl = import.meta.env.VITE_BASE_API_URL || "https://localhost:7178/api/";
    const [isAuth, setIsAuth] = useState(false);
    const [user, setUser] = useState(null);
    const [isHydrated, setIsHydrated] = useState(false);

    useEffect(() => {
        if (hasAuthSession()) {
            const savedUser = getAuthUser();
            setUser(savedUser);
            setIsAuth(Boolean(savedUser));
        }
        setIsHydrated(true);
    }, []);

    useEffect(() => {
        setUnauthorizedHandler(() => {
            clearAuthSession();
            setIsAuth(false);
            setUser(null);
            window.location.href = "/login";
        });
    }, []);

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
            const response = await axios.post(`${baseApiUrl}auth/login`, credentials);
            const payload = response?.data?.data;

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
            return {
                success: false,
                message: error?.response?.data?.message || "Не вдалося виконати вхід",
            };
        }
    }

    async function registerRequest(registerData) {
        try {
            await axios.post(`${baseApiUrl}auth/register`, registerData);
            return { success: true };
        } catch (error) {
            return {
                success: false,
                message: error?.response?.data?.message || "Не вдалося виконати реєстрацію",
            };
        }
    }

    const isAdmin = useMemo(() => {
        return Boolean(user?.roles?.includes("admin"));
    }, [user]);

    return (
        <AuthContext.Provider value={{ isAuth, isHydrated, isAdmin, loginRequest, registerRequest, logout, user }}>
            {children}
        </AuthContext.Provider>
    )
}
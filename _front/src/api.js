import axios from "axios";
import {
    clearAuthSession,
    getAccessToken,
    getRefreshToken,
    setAuthSession,
} from "./services/authStorage";

const baseURL = import.meta.env.VITE_BASE_API_URL || "https://localhost:7178/api/";

export const api = axios.create({
    baseURL,
});

let onUnauthorized = null;
let refreshPromise = null;

export function setUnauthorizedHandler(handler) {
    onUnauthorized = handler;
}

api.interceptors.request.use((config) => {
    const token = getAccessToken();
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

async function refreshTokens() {
    const refreshToken = getRefreshToken();
    if (!refreshToken) {
        throw new Error("No refresh token");
    }

    const response = await axios.post(`${baseURL}auth/refresh`, { refreshToken });
    const payload = response?.data?.data;

    if (!payload?.accessToken || !payload?.refreshToken) {
        throw new Error("Invalid refresh response");
    }

    setAuthSession({
        accessToken: payload.accessToken,
        refreshToken: payload.refreshToken,
        expiresAtUtc: payload.expiresAtUtc,
    });

    return payload.accessToken;
}

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const original = error.config;
        const status = error?.response?.status;

        if (status !== 401 || original?._retry) {
            return Promise.reject(error);
        }

        original._retry = true;

        try {
            if (!refreshPromise) {
                refreshPromise = refreshTokens();
            }

            const newToken = await refreshPromise;
            refreshPromise = null;

            original.headers.Authorization = `Bearer ${newToken}`;
            return api(original);
        } catch (refreshError) {
            refreshPromise = null;
            clearAuthSession();
            if (onUnauthorized) onUnauthorized();
            return Promise.reject(refreshError);
        }
    }
);

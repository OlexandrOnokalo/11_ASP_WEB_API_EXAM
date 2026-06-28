import axios from "axios";
import {
    clearAuthSession,
    getAccessToken,
    getRefreshToken,
    setAuthSession,
} from "./services/authStorage";

// Беру baseURL з env-змінної, щоб легко переключати між дев і прод.
// Fallback на localhost — тільки для локальної розробки
const baseURL = import.meta.env.VITE_BASE_API_URL || "https://localhost:7178/api/";

// Один axios-інстанс на весь проєкт — всі запити ідуть через нього,
// щоб interceptors спрацьовували автоматично
export const api = axios.create({
    baseURL,
});

// Цей handler реєструє AuthContext при монтуванні.
// Зроблено через callback щоб api.js не залежав від React напряму
let onUnauthorized = null;
// Singleton для refresh — щоб при кількох одночасних 401
// на сервер йшов лише один запит оновлення токена
let refreshPromise = null;

// AuthContext викликає це при монтуванні, передаючи свій logout+redirect
export function setUnauthorizedHandler(handler) {
    onUnauthorized = handler;
}

// Перед кожним запитом автоматично чіпляю Bearer-токен якщо він є.
// Компонентам не треба думати про авторизаційний header — все тут
api.interceptors.request.use((config) => {
    const token = getAccessToken();
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

// Іду напряму через axios (не через api), щоб не потрапити
// в нескінченний цикл interceptors при рефреші
async function refreshTokens() {
    const refreshToken = getRefreshToken();
    if (!refreshToken) {
        // Нема чим рефрешити — одразу кидаю помилку, щоб вилетіти в catch нижче
        throw new Error("No refresh token");
    }

    const response = await axios.post(`${baseURL}auth/refresh`, { refreshToken });
    const payload = response?.data?.data;

    if (!payload?.accessToken || !payload?.refreshToken) {
        // Сервер відповів, але без токенів — щось пішло не так на бекенді
        throw new Error("Invalid refresh response");
    }

    // Зберігаю нову пару токенів в localStorage, щоб наступний запит вже
    // використав свіжий access token
    setAuthSession({
        accessToken: payload.accessToken,
        refreshToken: payload.refreshToken,
        expiresAtUtc: payload.expiresAtUtc,
    });

    return payload.accessToken;
}

api.interceptors.response.use(
    // Успішна відповідь — просто пропускаю далі без змін
    (response) => response,
    async (error) => {
        const original = error.config;
        const status = error?.response?.status;

        // Якщо не 401 або вже пробував рефреш — не лізу, кидаю помилку далі
        if (status !== 401 || original?._retry) {
            return Promise.reject(error);
        }

        // Ставлю прапор щоб не потрапити в петлю якщо рефреш теж поверне 401
        original._retry = true;

        try {
            // Ключова деталь: якщо одночасно прийшло кілька 401,
            // всі вони чекають на той самий Promise — один рефреш-запит
            if (!refreshPromise) {
                refreshPromise = refreshTokens();
            }

            const newToken = await refreshPromise;
            // Скидаю singleton після завершення щоб наступний цикл міг запустити новий
            refreshPromise = null;

            // Повторюю оригінальний запит вже з новим токеном
            original.headers.Authorization = `Bearer ${newToken}`;
            return api(original);
        } catch (refreshError) {
            refreshPromise = null;
            // Рефреш провалився — токени протухли остаточно, чищу сесію
            // і викликаю logout+redirect через зареєстрований handler
            clearAuthSession();
            if (onUnauthorized) onUnauthorized();
            return Promise.reject(refreshError);
        }
    }
);

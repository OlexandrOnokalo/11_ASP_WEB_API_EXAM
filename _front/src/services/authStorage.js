// Константи ключів — щоб не писати рядки вручну в кожній функції
// і не словити typo який мовчки не знайде нічого в localStorage
const ACCESS_TOKEN_KEY = "accessToken";
const REFRESH_TOKEN_KEY = "refreshToken";
const EXPIRES_AT_KEY = "expiresAtUtc";
const USER_KEY = "authUser";

// Читається в request interceptor api.js перед кожним запитом
export function getAccessToken() {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
}

// Читається в refreshTokens() коли треба оновити пару токенів
export function getRefreshToken() {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function getAuthUser() {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;

    try {
        return JSON.parse(raw);
    } catch {
        // Якщо в localStorage якийсь сміття замість JSON — не крашуся,
        // просто повертаю null і далі сесія вважатиметься відсутньою
        return null;
    }
}

// Зберігаю умовно — якщо якесь поле не прийшло (наприклад при рефреші
// user не повертається), існуючі записи не затираю
export function setAuthSession({ accessToken, refreshToken, expiresAtUtc, user }) {
    if (accessToken) localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    if (refreshToken) localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    if (expiresAtUtc) localStorage.setItem(EXPIRES_AT_KEY, expiresAtUtc);
    if (user) localStorage.setItem(USER_KEY, JSON.stringify(user));
}

// Викликається в двох місцях: logout() і при провалі рефрешу в api.js.
// Видаляю всі чотири ключі разом — не можна залишити половину
export function clearAuthSession() {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(EXPIRES_AT_KEY);
    localStorage.removeItem(USER_KEY);
}

// Перевіряю повноту сесії — потрібні всі три складові разом.
// Якщо хоч одного немає — вважаю сесію відсутньою і не гідратую стан
export function hasAuthSession() {
    return Boolean(getAccessToken() && getRefreshToken() && getAuthUser());
}

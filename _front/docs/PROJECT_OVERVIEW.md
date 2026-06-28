# PROJECT OVERVIEW — Car Catalog Frontend

> **Мова документації:** Українська  
> **Версія проєкту:** 0.0.0 (exam build)  
> **Дата:** 2026-06-28  
> **Бекенд URL:** `[API_URL]`

---

## Зміст

1. [Огляд проєкту](#1-огляд-проєкту)
2. [Карта проєкту по файлах](#2-карта-проєкту-по-файлах)
3. [Ключові потоки (Step-by-Step)](#3-ключові-потоки-step-by-step)
4. [Чому це зроблено саме так](#4-чому-це-зроблено-саме-так)
5. [Навчальні компроміси і що я б зробив у продакшн](#5-навчальні-компроміси-і-що-я-б-зробив-у-продакшн)
6. [Глосарій термінів](#6-глосарій-термінів)
7. [Ймовірні питання на інтерв'ю](#7-ймовірні-питання-на-інтервю)

---

## 1. Огляд проєкту

### Що це і для кого

**Car Catalog** — це SPA (Single Page Application) для перегляду, пошуку та управління каталогом автомобілів і їх виробників. Додаток орієнтований на два типи користувачів:

- **Звичайний відвідувач** — переглядає список автомобілів, фільтрує за різними параметрами, переходить на сторінку деталей.
- **Адміністратор** (`role: "admin"`) — має повний CRUD-доступ: створює, редагує та видаляє автомобілі й виробників.

### Архітектурний патерн

Проєкт реалізований за **Feature-based (Page-centric) архітектурою**: кожна функціональна область (автомобілі, виробники, авторизація) живе у власній піддиректорії у `src/pages/`. Спільні елементи (Navbar, Footer, Layout) розміщені в `src/components/`, а кросплатформні сервіси та утиліти — у `src/services/`.

**Чому це правильний вибір для цього додатку:**  
Додаток має чітко розмежовані доменні сутності (автомобілі, виробники, авторизація) без значного перетину логіки. Feature-based структура дозволяє знаходити всі файли, пов'язані з конкретною функцією, в одному місці. Для навчального exam-проєкту середнього розміру це оптимальний баланс між організованістю і простотою.

### Tech Stack

| Бібліотека | Версія | Технічне обґрунтування вибору |
|---|---|---|
| **React** | 18.2.0 | Компонентна модель, хуки, мінімальний overhead. Стандарт де-факто для SPA. |
| **React DOM** | 18.2.0 | Рендерер для браузерного середовища, пара до React. |
| **react-router-dom** | 6.17.0 | Декларативний routing з підтримкою nested routes (потрібні для layout-обгортки) та URL-параметрів. V6 має кращий API порівняно з V5. |
| **@reduxjs/toolkit** | 1.9.5 | Офіційний спосіб роботи з Redux. `configureStore` усуває boilerplate налаштування. |
| **react-redux** | 8.1.1 | Прив'язка Redux-store до React-компонентів через хуки `useSelector` / `useDispatch`. |
| **@mui/material** | 5.14.0 | Готова дизайн-система з підтримкою тем, responsive grid, доступності. Прискорює UI-розробку без написання кастомного CSS. |
| **@mui/icons-material** | 5.14.0 | Іконки у форматі React-компонентів, стилізовані через MUI-систему. |
| **@emotion/react** | 11.11.0 | CSS-in-JS движок, потрібен для роботи MUI. |
| **@emotion/styled** | 11.11.0 | `styled()` API для компонентів, потрібний для кастомних стилів через MUI. |
| **axios** | 1.4.0 | Обраний замість `fetch` через підтримку interceptors — критично для автоматичного оновлення JWT-токена. |
| **formik** | 2.4.0 | Управління станом форм, обробка `touched`/`errors`, `handleChange` без написання боєрплейту. |
| **yup** | 1.2.0 | Schema-based валідація, безшовно інтегрується з formik через `validationSchema`. |
| **prop-types** | 15.8.1 | Runtime-перевірка типів пропсів у development-середовищі (без TypeScript). |
| **vite** | 7.3.1 | Збірник. Миттєвий холодний старт, HMR, ES-модулі нативно. Значно швидший за webpack. |

### Структура папок (ASCII)

```
_front/
├── index.html                  # Точка входу HTML (SPA-шаблон)
├── vite.config.js              # Конфіг збірника Vite
├── eslint.config.js            # Конфіг лінтера
├── package.json                # Залежності та npm-скрипти
│
├── public/                     # Статичні файли (не обробляються Vite)
│
└── src/
    ├── main.jsx                # Точка входу React: Provider, BrowserRouter, AuthProvider
    ├── App.jsx                 # Кореневий компонент: маршрутизація + guards
    ├── api.js                  # Axios-інстанс з interceptors (auth + refresh)
    ├── App.css / index.css     # Глобальні стилі
    │
    ├── assets/                 # Статичні ресурси (зображення, svg)
    │
    ├── components/             # Спільні UI-компоненти (не прив'язані до feature)
    │   ├── layouts/
    │   │   └── DefaultLayout.jsx   # Layout-обгортка: Navbar + Outlet + Footer
    │   ├── navbar/
    │   │   └── Navbar.jsx          # Верхня навігаційна панель
    │   └── footer/
    │       └── Footer.jsx          # Footer з копірайтом
    │
    ├── context/
    │   └── AuthContext.jsx     # Глобальний стан авторизації (React Context)
    │
    ├── pages/                  # Feature-based сторінки
    │   ├── mainPage/
    │   │   └── MainPage.jsx        # Головна сторінка з навігаційними кнопками
    │   │
    │   ├── auth/               # Все що стосується автентифікації
    │   │   ├── loginPage/
    │   │   │   └── LoginPage.jsx       # Форма входу (uncontrolled через useRef)
    │   │   ├── registerPage/
    │   │   │   └── RegisterPage.jsx    # Форма реєстрації (formik + yup)
    │   │   └── components/
    │   │       ├── ForgotPassword.jsx  # Модальне вікно "забули пароль"
    │   │       └── CustomIcons.jsx     # SVG-іконки (Google, FaceCar)
    │   │
    │   ├── carsPage/           # Весь CRUD для автомобілів
    │   │   ├── CarListPage.jsx     # Список + фільтрація + пошук
    │   │   ├── CarCard.jsx         # Картка автомобіля (preview)
    │   │   ├── CarDetailsPage.jsx  # Детальна сторінка автомобіля
    │   │   ├── CarCreateForm.jsx   # Форма створення (formik + multipart)
    │   │   └── CarUpdateForm.jsx   # Форма редагування (formik + enableReinitialize)
    │   │
    │   ├── manufacturesPage/   # Весь CRUD для виробників
    │   │   ├── ManufacturesListPage.jsx    # Список виробників (grid)
    │   │   ├── ManufacturesCard.jsx        # Картка виробника
    │   │   ├── ManufacturesCreateForm.jsx  # Форма створення
    │   │   └── ManufacturesUpdateForm.jsx  # Форма редагування (з Redux store)
    │   │
    │   └── notFoundPage/
    │       └── NotFoundPage.jsx    # Сторінка 404
    │
    ├── services/               # Утиліти (не React, чисті функції)
    │   ├── authStorage.js      # CRUD-операції з localStorage для токенів
    │   ├── imageUrl.js         # Перетворення відносних URL зображень
    │   └── responseParsers.js  # Парсинг обгортки API-відповідей
    │
    ├── store/                  # Redux-стан
    │   ├── store.js            # configureStore
    │   └── reducers/
    │       ├── rootReducer.js          # combineReducers
    │       ├── carReducer/
    │       │   └── carReducer.js       # Редюсер для списку автомобілів
    │       └── manufactureReducer/
    │           └── manufactureReducer.js # Редюсер для списку виробників
    │
    └── theme/
        ├── lightTheme.js       # MUI-тема: червона + coral
        └── darkTheme.js        # MUI-тема: сіра + lime
```

---

## 2. Карта проєкту по файлах

### `src/main.jsx` — Точка входу

**Призначення:** Ініціалізація React-дерева. Монтує кореневий компонент у DOM-елемент `#root`.

**Що робить:**
- Обгортає всю аплікацію у `<Provider store={store}>` — надає Redux-store всім компонентам
- Обгортає у `<BrowserRouter>` — активує History API routing з прапорами сумісності v7
- Обгортає у `<AuthProvider>` — надає контекст авторизації

**З чим взаємодіє:** `store/store.js`, `context/AuthContext.jsx`, `App.jsx`

---

### `src/App.jsx` — Кореневий маршрутизатор

**Призначення:** Центр маршрутизації та управління темою.

**Що тримає:**
- `isDark: boolean` — стан теми (light/dark), передається в DefaultLayout і ThemeProvider
- Споживає `{ isAuth, isAdmin, isHydrated }` з `AuthContext`

**Що рендерить:**
- `<ThemeProvider>` — обгортка MUI-теми
- `<Routes>` з повною деревом маршрутів
- Захищені маршрути реалізовані **умовним рендерингом**: `{isAuth && isAdmin && <Route .../>}`. Якщо умова false — маршрут просто не реєструється в дереві.
- `isHydrated` — гард від flash: поки не завантажений стан із localStorage, компонент повертає `null`

**Маршрутна схема:**
```
/                   → MainPage
/cars               → CarListPage
/cars/:id           → CarDetailsPage
/cars/create        → CarCreateForm        (тільки isAuth && isAdmin)
/cars/update/:id    → CarUpdateForm        (тільки isAuth && isAdmin)
/Manufactures       → ManufacturesListPage
/Manufactures/create → ManufacturesCreateForm (тільки isAuth && isAdmin)
/Manufactures/update/:id → ManufacturesUpdateForm (тільки isAuth && isAdmin)
/login              → LoginPage            (тільки !isAuth)
/register           → RegisterPage         (тільки !isAuth)
*                   → NotFoundPage
```

**З чим взаємодіє:** `AuthContext`, `DefaultLayout`, усі Page-компоненти, `lightTheme`/`darkTheme`

---

### `src/api.js` — HTTP-клієнт

**Призначення:** Налаштований axios-інстанс з повним циклом JWT-авторизації.

**Що робить:**
- Створює `axios.create({ baseURL: [API_URL] })` — єдиний інстанс для всіх API-запитів
- **Request interceptor:** автоматично додає `Authorization: Bearer <token>` до кожного запиту, якщо токен є в localStorage
- **Response interceptor:** при отриманні 401 — автоматично оновлює токен через `POST [API_URL]auth/refresh`, після чого повторює оригінальний запит
- `refreshPromise` — singleton-патерн: якщо одночасно приходять кілька 401, лише один запит на refresh іде на сервер; всі інші чекають на один і той же Promise
- `_retry: true` — прапор запобігає нескінченній петлі, якщо refresh теж повертає 401
- `setUnauthorizedHandler(handler)` — дозволяє AuthContext зареєструвати callback на примусовий logout

**Критичний секурний момент:** при провалі refresh — `clearAuthSession()` очищує всі токени з localStorage і редиректить на `/login`

**З чим взаємодіє:** `services/authStorage.js`, використовується у всіх Page-компонентах

---

### `src/context/AuthContext.jsx` — Глобальний стан авторизації

**Призначення:** React Context для стану поточного користувача, методів входу/виходу/реєстрації.

**Що тримає:**
- `isAuth: boolean` — чи є активна сесія
- `user: object | null` — об'єкт користувача `{ roles, email, ... }` з localStorage
- `isHydrated: boolean` — чи завершено читання з localStorage при монтуванні

**Обчислюваний стан:**
- `isAdmin: boolean` — `useMemo(() => user?.roles?.includes("admin"), [user])` — реактивно перераховується при зміні `user`

**Методи:**
- `loginRequest(credentials)` — HTTP POST до `[API_URL]auth/login`, зберігає сесію, оновлює стан
- `registerRequest(registerData)` — HTTP POST до `[API_URL]auth/register`
- `logout()` — очищує localStorage і скидає стан
- `loginWithSession(authUser)` — внутрішній метод встановлення стану після успішного логіну

**Важлива деталь:** `useEffect` на монтуванні реєструє `setUnauthorizedHandler` в `api.js` — тим самим `api.js` "знає", що робити при остаточному провалі refresh: викликати `logout()` і редиректити.

**Хук:** `export const useAuth = () => useContext(AuthContext)` — зручний споживач

---

### `src/services/authStorage.js` — Сховище токенів

**Призначення:** Ізольований CRUD-інтерфейс до localStorage для токенів авторизації.

**Ключі:** `accessToken`, `refreshToken`, `expiresAtUtc`, `authUser`

**Функції:**
- `getAccessToken()` / `getRefreshToken()` — читання токенів
- `getAuthUser()` — читання та JSON-парсинг об'єкту користувача (з захистом try/catch від JSON-помилок)
- `setAuthSession({ accessToken, refreshToken, expiresAtUtc, user })` — запис сесії
- `clearAuthSession()` — видалення всіх чотирьох ключів
- `hasAuthSession()` — перевірка наявності повної сесії (всі три складові: accessToken + refreshToken + user)

---

### `src/services/responseParsers.js` — Парсер відповідей API

**Призначення:** Абстракція від обгортки API-відповідей.

**Проблема яку вирішує:** Бекенд повертає дані в конверті `{ data: { data: { items: [...] } } }`. Без абстракції кожен компонент мав би писати `response?.data?.data?.items`.

**Функції:**
```js
getResponseData(response)  // → response?.data?.data ?? response?.data
getItems(response)         // → data.items якщо є, або data якщо масив
getEntity(response)        // → getResponseData(response) — для одиночних об'єктів
```

---

### `src/services/imageUrl.js` — Утиліта зображень

**Призначення:** Нормалізація URL зображень з бекенду.

**Логіка `toImageSrc(image)`:**
1. Якщо `image` відсутнє — повертає вбудований SVG-плейсхолдер (без HTTP-запиту)
2. Якщо вже абсолютний URL (`http://...`) — повертає як є
3. Якщо відносний шлях (`/uploads/...`) — додає origin бекенду (`[API_URL]` → витягується `origin`)

---

### `src/store/store.js` — Redux Store

**Призначення:** Налаштування центрального сховища стану.

Використовує `configureStore` з `@reduxjs/toolkit` з `rootReducer`. В dev-режимі автоматично підключає Redux DevTools.

---

### `src/store/reducers/rootReducer.js` — Кореневий редюсер

`combineReducers({ car: carReducer, manufacture: manufactureReducer })` — об'єднує два слайси стану.

---

### `src/store/reducers/carReducer/carReducer.js`

**Стан:** `{ cars: [], isLoaded: false }`

**Actions (рядкові типи):**
- `"loadcars"` → замінює весь масив, встановлює `isLoaded: true`
- `"deletecar"` → фільтрує масив за `id`
- `"updatecar"` → замінює весь масив (після PUT)
- `"createcar"` → додає один елемент до масиву

**Примітка:** Використовуються рядкові константи замість `createSlice` — ручний підхід без генерації action creators.

---

### `src/store/reducers/manufactureReducer/manufactureReducer.js`

**Стан:** `{ Manufactures: [], isLoaded: false }`

Аналогічна структура до `carReducer`, але для виробників. Actions: `"loadManufactures"`, `"deletemanufacture"`, `"updatemanufacture"`, `"createmanufacture"`.

**Особливість:** `ManufacturesListPage` перевіряє `isLoaded` і не робить повторний запит — кешування у Redux.

---

### `src/components/layouts/DefaultLayout.jsx`

**Призначення:** Layout-компонент, спільна обгортка для всіх сторінок.

**Що рендерить:** `<Navbar>` → `<Box>` з градієнтним фоном → `<Container>` → `<Outlet>` (сюди React Router підставляє дочірню сторінку) → `<Footer>`

**Пропси:** `isDark: boolean`, `setIsDark: function` — передаються в Navbar для перемикача теми

---

### `src/components/navbar/Navbar.jsx`

**Призначення:** Верхня навігаційна панель.

**Що тримає:** `anchorElNav`, `anchorElUser` — стан відкриття MUI Menu (responsive мобільне меню)

**Що відображає:**
- Логотип "Cars" з іконкою DirectionsCar
- Навігаційні посилання (Cars, Manufactures)
- Кнопка перемикача теми (іконки LightMode/DarkMode)
- Avatar-меню для авторизованого користувача з кнопкою "Вийти"
- Споживає `{ isAuth, logout }` з `AuthContext`

---

### `src/components/footer/Footer.jsx`

Простий MUI footer із назвою "Car Catalog" та динамічним роком через `new Date().getFullYear()`.

---

### `src/pages/mainPage/MainPage.jsx`

Проста сторінка з двома кнопками: "Автомобілі" (`/cars`) та "Виробники" (`/Manufactures`).

---

### `src/pages/carsPage/CarListPage.jsx`

**Призначення:** Головна сторінка каталогу — список автомобілів з фільтрацією.

**Стан:**
- `loading: boolean` — локальний індикатор завантаження
- `filters: object` — активні фільтри (page, page_size, name, manufactureId, year, color, volume, minValue, maxValue)
- `searchInputs: object` — значення UI-полів введення (відокремлені від `filters` щоб не тригерити fetch при кожному keystroke)
- `manufactures: array` — список виробників для select-фільтра

**З Redux:** `useSelector(state => state.car)` — читає `{ cars, isLoaded }`; `useDispatch` — диспетчеризує `"loadcars"`

**Логіка фільтрації:**
1. `useEffect` на `location.search` — синхронізує `filters` з URL-параметрами (дозволяє share-посилань)
2. `useEffect` на `filters` — запускає fetch при зміні фільтрів
3. Різні API-ендпоінти залежно від типу фільтра:
   - `minValue` або `maxValue` → `GET cars/by-price?minValue=...&maxValue=...`
   - Текстові фільтри → `GET cars?property=<prop>&value=<val>`
   - Без фільтрів → `GET cars?page=1&page_size=100`

**З чим взаємодіє:** `api.js`, `AuthContext` (isAdmin), Redux store, `CarCard`, `responseParsers`

---

### `src/pages/carsPage/CarCard.jsx`

**Призначення:** Карткове представлення одного автомобіля.

**Пропси:** `car: object`, `onDelete?: function`, `canManage: boolean`

**Що рендерить:** MUI Card з зображенням, назвою, характеристиками, кнопками "Деталі" / "Редагувати" (якщо `canManage`) / "Видалити" (якщо `canManage`)

**Логіка видалення:** Якщо передано `onDelete` — делегує батьківському компоненту. Інакше — `DELETE cars/:id` + dispatch `"deletecar"` для оновлення Redux store без повторного fetch.

---

### `src/pages/carsPage/CarDetailsPage.jsx`

**Призначення:** Детальна сторінка одного автомобіля.

**Стан:** `car: object | null`, `loading: boolean`

**Що робить:** `GET cars/:id` при монтуванні (з `useParams`), рендерить всі поля авто + набір кнопок-фільтрів.

**Кнопки-фільтри** (cross-selling навігація): "Всі від виробника", "Всі за роком", "Всі за кольором", "Всі за об'ємом", "Схожі по ціні" — кожна формує URL `/cars?param=value` і навігує на CarListPage.

---

### `src/pages/carsPage/CarCreateForm.jsx`

**Призначення:** Форма створення нового автомобіля.

**Formik schema (yup):** name (string, required), year (number, required), volume (number, required), price (number, required), color (string, required).

**Відправка:** `multipart/form-data` через FormData API. Після успішного POST — refetch всього списку та `navigate('/cars')`.

**Примітка:** Відправляє і `description`, і `desciption` (з помилкою у написанні) для сумісності з бекендом, де є typo в назві поля.

---

### `src/pages/carsPage/CarUpdateForm.jsx`

**Призначення:** Форма редагування існуючого автомобіля.

**Стан:** `initial: object | null` — завантажені з API поточні дані авто; `manufactures: array`

**Ключова опція:** `formik: { enableReinitialize: true }` — дозволяє Formik перезаписати `initialValues` коли `initial` завантажиться async (без цього форма залишається порожньою).

**Відправка:** `PUT cars/:id` з FormData, потім refetch списку і `navigate('/cars/:id')`.

---

### `src/pages/manufacturesPage/ManufacturesListPage.jsx`

**Призначення:** Grid-список всіх виробників.

**З Redux:** `useSelector(state => state.manufacture)` — якщо `isLoaded === true`, не робить нового запиту (простий cache-check).

**Для адміна:** рендерить кнопку `+` (AddCircleIcon) в кінці grid для переходу на `/Manufactures/create`.

---

### `src/pages/manufacturesPage/ManufacturesCard.jsx`

MUI Card з назвою виробника, кнопками "Редагувати" і "Видалити" (лише для `canManage`), та посиланням "Переглянути авто" → `/cars?manufactureId=<id>`.

---

### `src/pages/manufacturesPage/ManufacturesCreateForm.jsx`

Форма з одним полем `name`. Formik + yup (string, required). Після успішного POST — dispatch `"createmanufacture"` і `navigate('/Manufactures')`.

---

### `src/pages/manufacturesPage/ManufacturesUpdateForm.jsx`

**Особливість:** Не робить окремого GET-запиту для завантаження даних. Замість цього — `useSelector(state => state.manufacture)` + `Manufactures.find(a => a.id == id)`. Тобто передбачає, що список вже є в Redux store від попереднього відвідування `ManufacturesListPage`.

---

### `src/pages/auth/loginPage/LoginPage.jsx`

**Форма входу.** Використовує **uncontrolled inputs** через `useRef` замість formik — `emailRef.current.value`, `passwordRef.current.value`. Виклик `loginRequest(credentials)` з `AuthContext`. При успіху — `navigate('/')`.

Містить кнопку "Forgot password?" → відкриває `ForgotPassword` Modal.

---

### `src/pages/auth/registerPage/RegisterPage.jsx`

**Форма реєстрації.** Використовує formik + yup. Schema включає поля: email, userName, password, confirmPassword (yup `ref` для порівняння), firstName, lastName (опціонально). Виклик `registerRequest` з `AuthContext`. При успіху → `navigate('/login')`.

---

### `src/pages/auth/components/ForgotPassword.jsx`

Модальний Dialog "Reset password" — UI-заглушка (onSubmit лише закриває модал, реальний запит не надсилається).

---

### `src/pages/auth/components/CustomIcons.jsx`

SVG-іконки у вигляді React-компонентів (обгорнуті у MUI `SvgIcon`): `SitemarkIcon` (логотип), `FacecarIcon`, `GoogleIcon` — використовуються на сторінках авторизації як декоративні елементи.

---

### `src/theme/lightTheme.js` / `src/theme/darkTheme.js`

MUI `createTheme()` об'єкти.
- **Light:** primary — червоний (#f44336), secondary — coral (#FF7F50)
- **Dark:** mode "dark", primary — сірий (#9e9e9e), secondary — lime (#CDDC39)

Перемикання відбувається в `App.jsx` через `<ThemeProvider theme={isDark ? darkTheme : lightTheme}>`.

---

## 3. Ключові потоки (Step-by-Step)

### Потік 1: Авторизація та збереження токена

**Сценарій:** Користувач вводить email/пароль і натискає "Sign in"

1. **`LoginPage.jsx`** → `handleSubmit(event)`: зчитує значення через `emailRef.current.value` і `passwordRef.current.value`, формує об'єкт `{ email, password }`
2. **`LoginPage.jsx`** → викликає `loginRequest(credentials)` з `AuthContext`
3. **`AuthContext.jsx`** → `loginRequest()`: `axios.post('[API_URL]auth/login', credentials)` (прямий axios, не через `api` інстанс — щоб уникнути циклу interceptors)
4. **`AuthContext.jsx`** → `setAuthSession({ accessToken, refreshToken, expiresAtUtc, user })`: записує всі 4 ключі в localStorage через `authStorage.js`
5. **`AuthContext.jsx`** → `loginWithSession(payload.user)`: встановлює `isAuth = true`, `user = payload.user`
6. **`LoginPage.jsx`** → отримує `{ success: true }`, викликає `navigate('/')`
7. **`App.jsx`** → React re-render: `isAuth === true`, `isAdmin = user.roles.includes('admin')`. Якщо admin — в дерево маршрутів додаються `/cars/create`, `/cars/update/:id` тощо

---

### Потік 2: Автоматичне оновлення JWT (silent refresh)

**Сценарій:** Access token протермінований, але refresh token ще дійсний

1. Компонент (напр. `CarListPage.jsx`) викликає `api.get('cars', ...)`
2. **`api.js`** → request interceptor: `config.headers.Authorization = 'Bearer <expired_token>'`
3. Бекенд повертає **401 Unauthorized**
4. **`api.js`** → response interceptor: `status === 401 && !original._retry` → входить у гілку оновлення
5. `original._retry = true` — запобіжник від петлі
6. `refreshPromise = refreshTokens()` → `POST '[API_URL]auth/refresh' { refreshToken }`
7. Бекенд повертає нові `{ accessToken, refreshToken, expiresAtUtc }`
8. **`api.js`** → `setAuthSession(...)` — оновлює токени в localStorage
9. `original.headers.Authorization = 'Bearer <new_token>'` → повторює оригінальний запит
10. Якщо refresh теж провалився → `clearAuthSession()` + `onUnauthorized()` → logout + редирект на `/login`

---

### Потік 3: CRUD — Перегляд списку автомобілів (Read)

**Сценарій:** Користувач переходить на `/cars`

1. **React Router** → рендерить `CarListPage.jsx`
2. **`CarListPage.jsx`** → перший `useEffect` на `location.search`: URL пустий → `filters = { page: 1, page_size: 100 }`
3. **`CarListPage.jsx`** → другий `useEffect` на `filters`: `api.get('cars', { params: { page: 1, page_size: 100 } })`
4. **`api.js`** → request interceptor додає `Authorization: Bearer <token>` (якщо є)
5. Бекенд повертає `{ data: { data: { items: [...] } } }`
6. **`responseParsers.js`** → `getItems(res)` розпаковує → `items: [...]`
7. `dispatch({ type: 'loadcars', payload: items })` → **`carReducer.js`**: `{ cars: items, isLoaded: true }`
8. **`CarListPage.jsx`** → `useSelector` отримує оновлені `cars`, `setLoading(false)`, рендерить `{cars.map(car => <CarCard car={car} />)}`
9. **`CarCard.jsx`** → кожна картка рендерить зображення через `toImageSrc(car.image)` з `imageUrl.js`

---

### Потік 4: Створення автомобіля (Create — Admin only)

**Сценарій:** Адмін заходить на `/cars/create`, заповнює форму, натискає "Зберегти"

1. **`App.jsx`** → `isAuth && isAdmin` → маршрут `/cars/create` існує в дереві, рендерить `CarCreateForm.jsx`
2. **`CarCreateForm.jsx`** → `useEffect` завантажує список виробників: `api.get('manufactures', ...)` → populates `<select>`
3. Користувач заповнює поля, вибирає файл зображення
4. Formik `onSubmit(values)`: формує `FormData`, додає всі поля включно з `values.image` (якщо `instanceof File`)
5. `api.post('cars', formData)` — multipart/form-data запит
6. Після успіху: `api.get('cars', ...)` → `dispatch({ type: 'loadcars', payload })` — оновлює Redux store
7. `navigate('/cars')` — повертає на список

---

### Потік 5: Захист маршрутів (Route Guards)

**Сценарій:** Неавторизований користувач намагається перейти на `/cars/create`

1. **`App.jsx`** → при рендері перевіряє `isAuth && isAdmin`
2. `isAuth === false` → умова не виконується → `<Route path="create" .../>` **не додається** до дерева маршрутів
3. React Router не знаходить маршрут `/cars/create`
4. Відпрацьовує `<Route path="*" element={<NotFoundPage />}/>` — рендерить сторінку 404
5. Якщо `isAuth === true`, але `isAdmin === false` — та сама поведінка (маршрут не в дереві)

**Додатковий захист на рівні API:** Бекенд незалежно перевіряє роль, тому навіть маніпуляція URL не дасть результату — POST/PUT/DELETE повернуть 403.

---

## 4. Чому це зроблено саме так

### Рішення 1: Axios замість Fetch

**а) ПРОБЛЕМА:** Додаток потребує автоматичного оновлення JWT-токена при 401. З нативним `fetch` це потребує ручного написання wrapper-функції навколо кожного виклику або складного middleware.

**б) РІШЕННЯ:** `axios.create()` в `src/api.js` з response interceptor, що перехоплює 401 і виконує refresh прозоро для компонентів.

**в) ЧОМУ ПРАВИЛЬНО:** Axios interceptors є стандартним індустріальним підходом для обробки токенів. Компоненти не знають про логіку авторизації — це дотримання принципу Single Responsibility.

**г) АЛЬТЕРНАТИВА:** React Query (`@tanstack/react-query`) з middleware або нативний `fetch` з ручним wrapper.

**д) ПОРІВНЯННЯ:** Axios — менше коду у компонентах, централізована обробка помилок. React Query — додає кешування та стан завантаження, але збільшує bundle і складність для навчального проєкту.

---

### Рішення 2: React Context для авторизації + Redux для даних

**а) ПРОБЛЕМА:** Додаток має два типи глобального стану: стан авторизації (користувач, ролі) і дані каталогу (масиви авто/виробників). Змішувати їх в одному рішенні збільшує складність.

**б) РІШЕННЯ:** `AuthContext.jsx` тримає auth-стан і методи входу/виходу. Redux store (`store.js`) тримає `cars` і `Manufactures` — кешовані дані каталогу.

**в) ЧОМУ ПРАВИЛЬНО:** Auth-стан часто оновлюється через side-effects (logout handler, hydration) — Context+useState для цього природний вибір. Списки каталогу мають предсказувані CRUD-операції — Redux з чистими reducer-функціями ідеальний.

**г) АЛЬТЕРНАТИВА:** Тільки Redux для всього, включно з `isAuth`, `user`.

**д) ПОРІВНЯННЯ:** Гібридний підхід — менше залежності від Redux для auth; Redux DevTools прозоро показує мутації каталогу. Тільки Redux — більш послідовно, але auth-логіка в redux ускладнює обробку side-effects.

---

### Рішення 3: Захист маршрутів умовним рендерингом у App.jsx

**а) ПРОБЛЕМА:** Потрібно обмежити доступ до CRUD-маршрутів тільки для адміністраторів.

**б) РІШЕННЯ:** `App.jsx` умовно реєструє `<Route>` компоненти: `{isAuth && isAdmin && <Route path="create" .../>}`. Якщо умова `false` — маршрут відсутній у дереві.

**в) ЧОМУ ПРАВИЛЬНО:** Підхід є валідним в React Router v6: відсутній маршрут гарантовано недоступний. Простий і читабельний.

**г) АЛЬТЕРНАТИВА:** Окремий `PrivateRoute` / `ProtectedRoute` компонент-обгортка, який рендерить `<Navigate to="/login" replace/>` при невиконанні умови.

**д) ПОРІВНЯННЯ:** Поточний підхід — простіший, але маршрут падає на 404 (не редирект на `/login`). `PrivateRoute` — більш UX-коректний (явний redirect), стандартніша практика в командних проєктах.

---

### Рішення 4: `isHydrated` гард від flash of unauthenticated content

**а) ПРОБЛЕМА:** При першому рендері, до того як `useEffect` прочитає localStorage, стан `isAuth === false` і `isAdmin === false`. Це призводить до того, що Admin-маршрути на мить не реєструються, і якщо адмін прямо відкриє `/cars/create` — URL "не знайдений".

**б) РІШЕННЯ:** `AuthContext.jsx` тримає `isHydrated: boolean`, встановлює `true` після читання localStorage. `App.jsx` повертає `null` поки `!isHydrated`.

**в) ЧОМУ ПРАВИЛЬНО:** Запобігає FOUC (Flash Of Unauthenticated Content) і race condition між гідрацією стану і першим рендером маршрутів.

**г) АЛЬТЕРНАТИВА:** Skeleton/Spinner-компонент замість `null`.

**д) ПОРІВНЯННЯ:** `return null` — без UX-затримки для користувача (зазвичай < 5мс), але повна порожнеча. Spinner — кращий UX при повільних пристроях.

---

### Рішення 5: `refreshPromise` singleton для concurrent refresh

**а) ПРОБЛЕМА:** Якщо компонент робить 3 паралельних API-запити і всі три отримують 401 — без захисту відбудеться 3 запити на refresh одночасно. Бекенд може інвалідувати refresh token після першого використання (rotation), і решта 2 запити провалюються.

**б) РІШЕННЯ:** `api.js`: `let refreshPromise = null`. Перший 401 встановлює `refreshPromise = refreshTokens()`. Наступні 401 перевіряють `if (!refreshPromise)` і отримують той самий Promise. Після завершення `refreshPromise = null`.

**в) ЧОМУ ПРАВИЛЬНО:** Класичний патерн "promise deduplication". Гарантує рівно один refresh-запит незалежно від кількості одночасних 401.

**г) АЛЬТЕРНАТИВА:** Queue-based підхід: зберігати чергу провалених запитів і відтворити після refresh.

**д) ПОРІВНЯННЯ:** Singleton Promise — простий, 5 рядків коду. Queue — гнучкіший (можна обробляти помилки окремо для кожного запиту), але складніший.

---

### Рішення 6: `responseParsers.js` як абстракція конверту API

**а) ПРОБЛЕМА:** REST API повертає дані в обгортці `{ status, message, data: { items: [...] } }`. Кожен компонент без абстракції дублює `response?.data?.data?.items ?? []`.

**б) РІШЕННЯ:** `src/services/responseParsers.js` з трьома функціями `getResponseData`, `getItems`, `getEntity`.

**в) ЧОМУ ПРАВИЛЬНО:** DRY-принцип. При зміні структури відповіді API — правка в одному файлі.

**г) АЛЬТЕРНАТИВА:** Axios `transformResponse` або Response Interceptor для трансформації до відправки у компонент.

**д) ПОРІВНЯННЯ:** Окремі функції-парсери — явно видно що відбувається. Interceptor — прозоро для компонентів, але важче налагоджувати.

---

### Рішення 7: Formik + Yup для форм

**а) ПРОБЛЕМА:** Форми з валідацією (CarCreate, CarUpdate, Register) потребують відстеження touched-стану, error-повідомлень, стану завантаження — велика кількість `useState` без бібліотеки.

**б) РІШЕННЯ:** `formik` керує станом форми, `yup` описує схему валідації (type, required, min, ref для порівняння паролів).

**в) ЧОМУ ПРАВИЛЬНО:** Formik + Yup — де-факто стандарт у React-екосистемі для form management без зовнішніх залежностей від store.

**г) АЛЬТЕРНАТИВА:** React Hook Form (більш performant, менш re-renders).

**д) ПОРІВНЯННЯ:** Formik — більш звичний API, більше документації для початківців. RHF — менше re-renders (використовує uncontrolled inputs нативно), але менш інтуїтивний.

---

## 5. Навчальні компроміси і що я б зробив у продакшн

1. **Ручні рядкові action-типи в редюсерах.** Я знаю, що `"loadcars"`, `"deletecar"` як рядки є спрощенням — при опечатці код мовчки не працює. В продакшн-проєкті я б використав `createSlice` з RTK, тому що він автоматично генерує action creators та типи, які TypeScript може перевірити.

2. **Відсутність ProtectedRoute компонента.** Я знаю, що умовний рендеринг `{isAuth && isAdmin && <Route/>}` є спрощенням — при прямій навігації відпрацьовує 404 замість редиректу. В продакшн-проєкті я б використав `<ProtectedRoute requiredRole="admin">` з `<Navigate to="/login" replace/>`, тому що це дає коректний UX і явну семантику.

3. **Зберігання токенів у localStorage.** Я знаю, що localStorage вразливий до XSS-атак. В продакшн-проєкті я б використав httpOnly cookies для refresh token (бекенд-конфігурація) і тримав access token лише в пам'яті (змінна модуля), тому що httpOnly cookies недоступні через JavaScript навіть при XSS.

4. **Відсутність TypeScript.** Я знаю, що `prop-types` є runtime-перевіркою і не замінює статичний аналіз. В продакшн-проєкті я б використав TypeScript з interfaces для всіх API-відповідей і пропсів компонентів, тому що це усуває цілий клас runtime-помилок.

5. **`window.confirm()` для підтвердження видалення.** Я знаю, що нативний `confirm` блокує event loop і не кастомізується стилізацією. В продакшн-проєкті я б використав MUI `Dialog` з кнопками підтвердження/скасування, тому що він неблокуючий, стилізований під дизайн-систему і підтримує доступність (aria).

6. **Відсутність обробки помилок у UI.** Я знаю, що `console.error(e)` у catch-блоках є спрощенням — користувач не бачить помилок. В продакшн-проєкті я б використав toast-нотифікації (наприклад notistack) і Error Boundary компоненти, тому що це критично для UX та debuggability.

7. **Hardcoded `page_size: 100`.** Я знаю, що завантаження 100 записів без пагінації — спрощення. В продакшн-проєкті я б реалізував повноцінну пагінацію або infinite scroll через `IntersectionObserver`, тому що при великих каталогах це суттєво впливає на performance та UX.

8. **`ForgotPassword` — UI-заглушка.** Я знаю, що форма відновлення паролю не надсилає реального запиту. В продакшн-проєкті я б реалізував `POST [API_URL]auth/forgot-password`, тому що відновлення паролю є критичною функцією безпеки.

9. **Typo в полі API `desciption`.** Я знаю, що відправка обох `description` і `desciption` є обхідним рішенням для бекенд-помилки. В продакшн-проєкті я б зафіксував баг в бекенді і використовував лише правильне ім'я поля після фіксу, тому що дублювання полів ускладнює підтримку.

10. **Відсутність тестів.** Я знаю, що проєкт не має жодного unit або integration тесту. В продакшн-проєкті я б написав тести для `authStorage.js`, `responseParsers.js`, `carReducer.js` (unit), і RTL-тести для ключових компонентів як `CarCard`, тому що це єдиний надійний спосіб запобігти регресіям.

---

## 6. Глосарій термінів

| Термін | Пояснення |
|---|---|
| **SPA** | Single Page Application — веб-застосунок, що завантажується одноразово, подальша навігація без перезавантаження сторінки |
| **JWT** | JSON Web Token — стандарт токенів авторизації. Складається з header.payload.signature, де payload містить claims (claims — твердження про користувача: id, roles тощо) |
| **Access Token** | Короткоживучий JWT для авторизації запитів. Передається в `Authorization: Bearer` header |
| **Refresh Token** | Довгоживучий токен для отримання нового access token після його протермінування |
| **Silent Refresh** | Автоматичне оновлення access token у фоні без участі користувача |
| **Interceptor** | Middleware-функція axios, що виконується до/після кожного HTTP-запиту/відповіді |
| **CRUD** | Create, Read, Update, Delete — базові операції з даними |
| **Redux** | Бібліотека управління глобальним станом на основі патерну Flux |
| **Reducer** | Чиста функція `(state, action) => newState` — обробляє дії і повертає новий стан |
| **Action** | Об'єкт `{ type: string, payload?: any }` — опис події що сталась |
| **Dispatch** | Виклик `store.dispatch(action)` — спосіб надіслати дію в store |
| **Selector** | Функція що витягує дані з Redux store (`useSelector(state => state.car.cars)`) |
| **Context API** | Вбудований у React механізм передачі даних по дереву компонентів без prop drilling |
| **Hydration** | В контексті цього проєкту — відновлення стану із localStorage при першому рендері |
| **Formik** | Бібліотека управління станом форм: touched, errors, values, handleSubmit |
| **Yup** | Бібліотека декларативної schema-валідації, інтегрується з Formik |
| **multipart/form-data** | Формат HTTP-запиту для передачі файлів разом з текстовими полями |
| **FormData** | Browser API для побудови multipart/form-data тіла запиту |
| **MUI** | Material UI — бібліотека React-компонентів, що реалізує Material Design |
| **ThemeProvider** | MUI-компонент що надає тему (кольори, типографіку) всім дочірнім компонентам |
| **Nested Routes** | React Router v6 — маршрути всередині маршрутів; `<Outlet>` — місце де рендериться дочірній маршрут |
| **Outlet** | React Router компонент — placeholder де рендерується активний дочірній маршрут |
| **useParams** | React Router хук для читання dynamic segments URL (`/cars/:id` → `{ id }`) |
| **useNavigate** | React Router хук для програмної навігації (`navigate('/cars')`) |
| **useLocation** | React Router хук для доступу до поточного URL, включаючи `search` (query string) |
| **Feature-based architecture** | Структура проєкту де файли організовані по функціональних областях (features/domains), а не по технічному типу |
| **BrowserRouter** | React Router компонент що використовує HTML5 History API для "чистих" URL без `#` |
| **enableReinitialize** | Formik-опція яка дозволяє скинути форму при зміні `initialValues` |
| **HOC** | Higher-Order Component — функція що приймає компонент і повертає новий з додатковою логікою |
| **prop-types** | Runtime-перевірка типів пропсів у development-середовищі |
| **ESM** | ES Modules — стандарт JavaScript-модулів (`import`/`export`) |
| **HMR** | Hot Module Replacement — Vite-функція заміни модулів без перезавантаження сторінки |
| **combineReducers** | RTK/Redux функція що об'єднує декілька reducer-функцій в один root reducer |
| **XSS** | Cross-Site Scripting — атака через впровадження шкідливого JavaScript |
| **FOUC** | Flash Of Unauthenticated Content — миттєве відображення неавторизованого UI до завантаження стану |
| **Promise deduplication** | Патерн де множинні виклики одночасної операції зводяться до одного Promise |

---

## 7. Ймовірні питання на інтерв'ю

### 1. Як реалізована авторизація? Де зберігаються токени і чому?

Я реалізував JWT-авторизацію з парою access/refresh токенів. Обидва токени зберігаються в localStorage через `src/services/authStorage.js`. Я розумію, що localStorage вразливий до XSS, і в продакшн-проєкті я б перейшов на httpOnly cookies для refresh token — але для навчального exam-проєкту localStorage спрощує реалізацію і дозволяє зосередитись на flow.

**Докази:** `authStorage.js` містить `getAccessToken()`, `setAuthSession()`, `clearAuthSession()`. `AuthContext.jsx` виконує `hasAuthSession()` при монтуванні для гідрації стану.

---

### 2. Як працює автоматичне оновлення токена (silent refresh)?

Я реалізував silent refresh через Axios response interceptor в `src/api.js`. При отриманні 401 — interceptor автоматично виконує `POST [API_URL]auth/refresh`, отримує нові токени, і повторює оригінальний запит з оновленим токеном. Я встановив прапор `_retry: true` щоб уникнути нескінченного циклу. Також додав `refreshPromise` singleton щоб при одночасних 401 на сервер йшов лише один refresh-запит.

**Докази:** `api.js`, рядки з `refreshPromise`, `original._retry`, `refreshTokens()`.

---

### 3. Що таке `isHydrated` і навіщо він потрібен?

Я додав `isHydrated` в `AuthContext.jsx` щоб вирішити проблему FOUC. При першому рендері React, до того як `useEffect` прочитав localStorage, `isAuth === false`. Це означало б, що Admin-маршрути не зареєстровані, і прямий перехід на `/cars/create` дав би 404. `isHydrated` вирішує це: `App.jsx` повертає `null` поки стан не відновлено з localStorage, після чого рендерить повне дерево маршрутів.

**Докази:** `AuthContext.jsx` — `setIsHydrated(true)` після `useEffect`. `App.jsx` — `if (!isHydrated) return null`.

---

### 4. Чому захист маршрутів реалізований прямо в App.jsx, а не через окремий ProtectedRoute?

Я вибрав умовний рендеринг `{isAuth && isAdmin && <Route .../>}` в `App.jsx` як простіше рішення для exam-проєкту. Це технічно коректно в React Router v6: відсутній маршрут = недоступний маршрут. Я усвідомлюю, що мінус цього підходу — при прямій навігації неавторизованого користувача на `/cars/create` відпрацьовує 404 замість редиректу на `/login`. В продакшн-проєкті я б реалізував `<ProtectedRoute>` компонент з `<Navigate to="/login" replace/>` — це стандартна практика і кращий UX.

**Докази:** `App.jsx` — `{isAuth && isAdmin && (<> <Route path="create" .../> </>)}`.

---

### 5. Навіщо в проєкті одночасно React Context і Redux? Чи не надлишково?

Ні, не надлишково — це свідоме рішення. Я використав Context для стану авторизації (`isAuth`, `user`, методи входу/виходу) і Redux для даних каталогу (`cars`, `Manufactures`). Auth-стан природньо живе в Context тому що він тісно пов'язаний з side-effects (hydration, unauthorized handler). Redux підходить для каталогу бо списки мають предсказувані CRUD-операції і Redux DevTools дозволяє відстежувати кожну мутацію. Змішування auth в Redux або каталогу в Context збільшило б складність без переваг.

**Докази:** `context/AuthContext.jsx` + `store/reducers/carReducer/carReducer.js`.

---

### 6. Як реалізована фільтрація автомобілів? Що особливого у цьому підході?

Я реалізував фільтрацію на основі URL query parameters. Стан `filters` синхронізується з `location.search` через `useEffect` в `CarListPage.jsx`. Це означає, що URL `/cars?color=red&year=2020` є "джерелом правди" для фільтрів — користувач може поділитися посиланням і отримати той самий результат. Крім того, різні типи фільтрів ведуть на різні API-ендпоінти: цінові фільтри → `GET cars/by-price`, текстові → `GET cars?property=...&value=...`.

**Докази:** `CarListPage.jsx` — `useEffect` на `[location.search]`, логіка вибору ендпоінту в другому `useEffect`.

---

### 7. Як `CarDetailsPage.jsx` реалізує cross-sell навігацію?

На сторінці деталей автомобіля я додав набір кнопок-фільтрів: "Всі від виробника", "Всі за роком", "Всі за кольором", "Всі за об'ємом", "Схожі по ціні". Кожна кнопка — це `<Link>` що формує URL з query params, наприклад `/cars?manufactureId=3` або `/cars?minValue=15000&maxValue=17000`. При переході `CarListPage.jsx` читає `location.search` і автоматично застосовує потрібні фільтри. Це чисте рішення без props drilling або додаткового стану.

**Докази:** `CarDetailsPage.jsx` — масив `<Link to="/cars?...">` кнопок.

---

### 8. Навіщо `enableReinitialize: true` у Formik в CarUpdateForm?

Я зіткнувся з проблемою: форма ініціалізується з порожніми `initialValues`, але дані автомобіля `initial` завантажуються асинхронно через `api.get()`. Без `enableReinitialize` Formik ігнорує зміни `initialValues` після першого рендеру — форма залишається порожньою. З `enableReinitialize: true` Formik перезаписує значення форми коли `initial` завантажиться — і поля показують актуальні дані авто.

**Докази:** `CarUpdateForm.jsx` — `formik = useFormik({ enableReinitialize: true, initialValues: initial ? { ... } : { ... } })`.

---

### 9. Як `ManufacturesUpdateForm` отримує дані для редагування — чому він не робить GET-запит?

Я вибрав підхід де `ManufacturesUpdateForm.jsx` читає дані з Redux store через `useSelector(state => state.manufacture)` і знаходить виробника за `id`. Це можливо тому що `ManufacturesListPage.jsx` завжди завантажує весь список при переходах і кешує його в Redux. Якщо користувач переходить на редагування зі списку — дані вже є в store. Я усвідомлюю слабкість цього підходу: при прямому переході на `/Manufactures/update/:id` без попереднього відвідування списку — store порожній і форма не заповниться.

**Докази:** `ManufacturesUpdateForm.jsx` — `useSelector(state => state.manufacture)` + `Manufactures.find(a => a.id == id)`.

---

### 10. Що таке `responseParsers.js` і як він допомагає?

Я виніс логіку розпакування API-відповідей в окремий утилітний модуль. Бекенд повертає дані в конверті `{ status, message, data: { data: { items: [...] } } }` або `{ data: { data: <entity> } }`. `getItems(response)` витягує масив, `getEntity(response)` витягує одиночний об'єкт. Це DRY-рішення: при зміні структури відповіді — правка в одному файлі `src/services/responseParsers.js` замість оновлення кожного компонента. Усі компоненти (CarListPage, ManufacturesListPage, CarCreateForm тощо) використовують ці функції.

**Докази:** `responseParsers.js` — три функції. Вживання: `getItems(res)` в `CarListPage.jsx`, `ManufacturesListPage.jsx`; `getEntity(r)` в `CarUpdateForm.jsx`, `CarDetailsPage.jsx`.

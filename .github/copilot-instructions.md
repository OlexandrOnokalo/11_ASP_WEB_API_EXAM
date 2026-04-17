# Copilot Instructions

## General Guidelines
- Користувач просить відповідати українською мовою.
- Користувач просить не вставляти звичайний текст у блоки коду; код-блоки використовувати лише для самого коду.

## API Specifications
- Для проєкту Cars фронтенд очікує маршрути `/api/manufactures` (саме так).
- Формат списків: `data.items`.
- Обов’язковий `PUT /api/cars/{id}` (id у URL) для оновлення авто; варіант `PUT /api/cars` не потрібен.
- Вкладений `car.manufacture`.
- CORS-політика називається `allowAll` для `http://localhost:5173`.
- Query-поля: `page_size`, `property`, `value`, `minValue`, `maxValue`.
- Стартовий seed для manufactures/cars.
- У проєкті Cars потрібно орієнтуватися на контракт фронтенду; логіку `ServiceResponse` з `PD411_*` не застосовувати 1-в-1.
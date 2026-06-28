// Бекенд загортає дані в конверт { data: { data: ... } }.
// Тут розпаковую його один раз, щоб у компонентах не писати цей ланцюжок щоразу.
// Fallback на response?.data на випадок якщо сервер відповів без подвійної обгортки
export function getResponseData(response) {
    return response?.data?.data ?? response?.data ?? null;
}

// Для списків: бекенд може повернути або { items: [...] } або просто масив напряму —
// обидва варіанти обробляю тут, щоб компоненти завжди отримували чистий масив
export function getItems(response) {
    const data = getResponseData(response);
    if (Array.isArray(data)) return data;
    return data?.items ?? [];
}

// Для одиночних сутностей — просто розпаковую конверт без додаткової логіки
export function getEntity(response) {
    return getResponseData(response);
}

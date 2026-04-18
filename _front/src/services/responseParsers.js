export function getResponseData(response) {
    return response?.data?.data ?? response?.data ?? null;
}

export function getItems(response) {
    const data = getResponseData(response);
    if (Array.isArray(data)) return data;
    return data?.items ?? [];
}

export function getEntity(response) {
    return getResponseData(response);
}

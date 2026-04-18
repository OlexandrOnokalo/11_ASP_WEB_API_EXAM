const baseApi = import.meta.env.VITE_BASE_API_URL || "https://localhost:7178/api/";
const apiOrigin = new URL(baseApi).origin;

const placeholderSvg = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='800' height='500'%3E%3Crect width='100%25' height='100%25' fill='%23eceff1'/%3E%3Ctext x='50%25' y='50%25' dominant-baseline='middle' text-anchor='middle' fill='%23607080' font-size='32'%3ENo image%3C/text%3E%3C/svg%3E";

export function toImageSrc(image) {
    if (!image) return placeholderSvg;
    if (String(image).startsWith("http")) return image;
    if (String(image).startsWith("/")) return `${apiOrigin}${image}`;
    return `${apiOrigin}/${String(image).replace(/^\/+/, "")}`;
}

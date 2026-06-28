// Знаю що рядкові типи замість createSlice — навчальний компроміс.
// При опечатці в типі action мовчки спрацює default і нічого не станеться.
// В продакшн використав би createSlice де типи генеруються автоматично
const initState = {
    cars: [],
    // isLoaded — простий кеш-прапор: якщо true, повторний fetch не робиться
    isLoaded: false,
};

export const carReducer = (state = initState, action) => {
    switch (action.type) {
        case "loadcars":
            // Замінюю весь масив і вмикаю кеш
            return { ...state, isLoaded: true, cars: action.payload };
        case "deletecar":
            // payload — id, фільтрую локально щоб не робити зайвий GET після DELETE
            // != (не !==) бо id може прийти як рядок або число залежно від бекенду
            return {
                ...state,
                cars: state.cars.filter((b) => b.id != action.payload),
            };
        case "updatecar":
            // payload — вже оновлений масив з GET після PUT
            return {
                ...state,
                cars: action.payload,
            };
        case "createcar":
            // payload — один новий об'єкт, додаю в кінець без повторного fetch
            return {
                ...state,
                cars: [...state.cars, action.payload],
            };
        default:
            return state;
    }
};

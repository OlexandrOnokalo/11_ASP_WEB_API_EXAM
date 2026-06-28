// Та сама структура що й carReducer — рядкові типи замість createSlice
const initState = {
    Manufactures: [],
    // isLoaded — ManufacturesListPage перевіряє цей прапор і не робить
    // повторний запит якщо список вже є в store
    isLoaded: false,
};

export const manufactureReducer = (state = initState, action) => {
    switch (action.type) {
        case "loadManufactures":
            return { ...state, isLoaded: true, Manufactures: action.payload };
        case "deletemanufacture":
            // Видаляю локально щоб не робити GET після DELETE
            // != бо id може бути рядком або числом
            return {
                ...state,
                Manufactures: state.Manufactures.filter((a) => a.id != action.payload),
            };
        case "updatemanufacture":
            // payload — повний оновлений масив
            return {
                ...state,
                Manufactures: action.payload,
            };
        case "createmanufacture":
            // payload — один новий виробник, додаю в кінець
            return {
                ...state,
                Manufactures: [...state.Manufactures, action.payload],
            };
        default:
            return state;
    }
};
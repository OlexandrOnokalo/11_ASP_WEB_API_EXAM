const initState = {
    Manufactures: [],
    isLoaded: false,
};

export const manufactureReducer = (state = initState, action) => {
    switch (action.type) {
        case "loadManufactures":
            return { ...state, isLoaded: true, Manufactures: action.payload };
        case "deletemanufacture":
            return {
                ...state,
                Manufactures: state.Manufactures.filter((a) => a.id != action.payload),
            };
        case "updatemanufacture":
            return {
                ...state,
                Manufactures: action.payload,
            };
        case "createmanufacture":
            return {
                ...state,
                Manufactures: [...state.Manufactures, action.payload],
            };
        default:
            return state;
    }
};
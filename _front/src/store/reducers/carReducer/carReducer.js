const initState = {
    cars: [],
    isLoaded: false,
};

export const carReducer = (state = initState, action) => {
    switch (action.type) {
        case "loadcars":
            return { ...state, isLoaded: true, cars: action.payload };
        case "deletecar":
            return {
                ...state,
                cars: state.cars.filter((b) => b.id != action.payload),
            };
        case "updatecar":
            return {
                ...state,
                cars: action.payload,
            };
        case "createcar":
            return {
                ...state,
                cars: [...state.cars, action.payload],
            };
        default:
            return state;
    }
};

import { combineReducers } from "@reduxjs/toolkit"
import { carReducer } from "./carReducer/carReducer"
import { manufactureReducer } from "./manufactureReducer/manufactureReducer"

// Ключі тут визначають форму store: state.car і state.manufacture
// Коли додаватиму новий slice — просто додаю сюди рядок
export const rootReducer = combineReducers({
    car: carReducer,
    manufacture: manufactureReducer
})
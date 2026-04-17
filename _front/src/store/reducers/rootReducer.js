import { combineReducers } from "@reduxjs/toolkit"
import { carReducer } from "./carReducer/carReducer"
import { manufactureReducer } from "./manufactureReducer/manufactureReducer"

export const rootReducer = combineReducers({
    car: carReducer,
    manufacture: manufactureReducer
})